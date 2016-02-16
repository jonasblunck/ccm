using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;

namespace CCMEngine
{
  public class CCCParser : IFunctionStream
  {
    private LookAheadLangParser parser;
    private bool suppressSignature;
    private List<string> contexts = new List<string>();

    public CCCParser(LookAheadLangParser parser, bool suppressSignature)
    {
      this.parser = parser;
      this.suppressSignature = suppressSignature;
    }

    private string GetFullyQualifiedFunctionName(string functionName)
    {
      StringBuilder context = new StringBuilder();

      if (this.contexts.Count > 0)
      {
        foreach(string part in this.contexts)
        {
          context.Append(part);
          context.Append("::");
        }
      }

      context.Append(functionName);

      return context.ToString();
    }

    public bool NextIsFunction()
    {
      return NextIsFunction(0);
    }

    private StringBuilder TrimAwayUnnecessarySpacesInFunctionName(StringBuilder functionName)
    {
      char[] tokens = new char[] { '(', ')', '<', '>', ',' };
      StringBuilder newName = functionName;

      foreach (char tok in tokens)
      {
        newName = newName.Replace(string.Format(" {0}", tok), tok.ToString());
        newName = newName.Replace(string.Format("{0} ", tok), tok.ToString());
      }

      return newName;
    }

    private void OnFunction()
    {
      StringBuilder functionName = new StringBuilder();

      // check for possible cpp-style (class::function) 
      while (this.parser.PeekNextKeyword(1).Equals("::"))
      {
        functionName.Append(this.parser.NextKeyword());
        functionName.Append(this.parser.NextKeyword());
      }

      if (this.parser.PeekNextKeyword().Equals("operator"))
      {
        functionName.Append(this.parser.NextKeyword());
        functionName.Append(" ");
      }

      functionName.Append(this.parser.NextKeyword());

      while (!this.parser.PeekNextKeyword().Equals(")"))
      {
        functionName.Append(this.parser.NextKeyword());
        functionName.Append(" ");
      }

      functionName.Append(this.parser.NextKeyword());
      functionName = TrimAwayUnnecessarySpacesInFunctionName(functionName);

      while (!this.parser.PeekNextKeyword().Equals("{"))
        this.parser.NextKeyword();

      string name = functionName.ToString();

      if (this.suppressSignature && name.Contains("("))
      {
        name = name.Split('(')[0];
      }

      throw new CCCParserSuccessException(GetFullyQualifiedFunctionName(name), this.parser.StreamOffset);
    }

    public bool NextIsClass()
    {
      if (this.parser.PeekNextKeyword().Equals("class"))
      {
        // is next class keyword a "{" or a ";"?
        int maxLookAHead = 20;

        for (int i = 0; i < maxLookAHead; ++i)
        {
          if (this.parser.PeekNextKeyword(i).Equals(";"))
            return false;
          else if (this.parser.PeekNextKeyword(i).Equals("{"))
            return true;
        }
      }

      return false;
    }

    private bool NextIsCStyleParameterDeclaration(int lookAheadOffset)
    {
      /*
        void
        error(message,a1,a2,a3,a4,a5,a6,a7)
                char *message;
                char *a1,*a2,*a3,*a4,*a5,*a6,*a7;
        {
          fprintf(stderr,message,a1,a2,a3,a4,a5,a6,a7);
        }
      */

      try
      {
        if (this.parser.PeekNextKeyword(lookAheadOffset).Equals(")"))
        {
          int offset = lookAheadOffset + 1; // consume right paranthesis

          while (!this.parser.PeekNextKeyword(offset).Equals("{"))
          {
            List<string> tokens = new List<string>();
            tokens.Add(this.parser.PeekNextKeyword(offset));
            tokens.Add(this.parser.PeekNextKeyword(offset + 1));
            tokens.Add(this.parser.PeekNextKeyword(offset + 2));

            if (tokens.Contains("*") || tokens.Contains(";") || tokens.Contains(",") ||
                tokens.Contains("[") || tokens.Contains("]"))
            {
              offset++;
              continue;
            }

            return false;
          }

          return true;
        }
      }
      catch (EndOfStreamException)
      {
      }

      return false;
    }

    private bool NextIsFunction(int lookAHeadOffset)
    {
      try
      {
        // MACRO(X) Foo(x) {
        // MACRO(X) Foo(x) throws(...) {

        // operator == (GUID& r1, GUID& r2) {}
        // operator >> (const C&) {}
        // operator -> () {}
        // Foo() {
        // Foo() const {
        // Foo() throws(X) {
        // A::A() : Base(), m_x(i) {
        // template <typename X, typename Y> int FOO(X x, Y y) {
        // Foo<Z>(string s) {}
        // Class::Function() {}

        // check for possible cpp-style (class::function) 
        if (this.parser.PeekNextKeyword(lookAHeadOffset + 1).Equals("::"))
          return NextIsFunction(lookAHeadOffset + 2);

        // operator = (xx)
        if (this.parser.PeekNextKeyword(lookAHeadOffset).Equals("operator"))
        {
          if (this.parser.PeekNextKeyword(lookAHeadOffset + 1).Equals("(") && this.parser.PeekNextKeyword(lookAHeadOffset + 2).Equals(")"))
            lookAHeadOffset += 2; // this is the void operator

          // skip forward to left parenthesis
          while (!this.parser.PeekNextKeyword(lookAHeadOffset + 1).Equals("("))
            ++lookAHeadOffset;

          return NextIsFunction(lookAHeadOffset);
        }

        if (this.parser.PeekNextKeyword(lookAHeadOffset + 1).Equals("<"))
        {
          while (!this.parser.PeekNextKeyword(lookAHeadOffset).Equals(">"))
            lookAHeadOffset++;
        }

        if (this.parser.PeekNextKeyword(lookAHeadOffset + 1).Equals("("))
        {
          int offset = lookAHeadOffset + 2; 

          while (!this.parser.PeekNextKeyword(offset).Equals(")"))
          {
            while (this.parser.PeekNextKeyword(offset).Equals("("))
              while (!this.parser.PeekNextKeyword(offset).Equals(")")) // local ( ) pair inside arguments
                ++offset;

            ++offset;
          }

          if (NextIsCStyleParameterDeclaration(offset))
            return true;

          if (this.parser.PeekNextKeyword(offset + 1).Equals("const"))
              offset++;

          if (this.parser.PeekNextKeyword(offset + 1).StartsWith("throw") ||
              this.parser.PeekNextKeyword(offset + 1).Equals(":"))

          {
            while (!this.parser.PeekNextKeyword(offset + 1).Equals("{"))
              offset++;
          }

          if (!this.parser.PeekNextKeyword(offset + 1).Equals("{"))
              return false;

          return true;
        }
      }
      catch (EndOfStreamException)
      {
      }

      return false;
    }

    private void Filter(int lookAHeadIndex)
    {
      try
      {
        if (this.parser.PeekNextKeyword().Equals("="))
        {
          while (!this.parser.PeekNextKeyword().Equals(";"))
            this.parser.NextKeyword();

          this.parser.NextKeyword();
        }
      }
      catch (EndOfStreamException)
      {
      }
    }

    public void ConsumeBlock(string startToken, string endToken)
    {
      if (this.parser.PeekNextKeyword().Equals(startToken))
      {
        this.parser.NextKeyword();

        while(!this.parser.PeekNextKeyword().Equals(endToken))
        {
          if (this.parser.PeekNextKeyword().Equals(startToken))
          {
             ConsumeBlock(startToken, endToken);
          }

          this.parser.NextKeyword();
        }

       this.parser.NextKeyword();
      }
    }

    public void ConsumeTemplate()
    {
      if (this.parser.PeekNextKeyword().Equals("template"))
      {
        this.parser.NextKeyword();
        ConsumeBlock("<", ">");
      }

    }

    public void AdvanceToNextFunction()
    {
      while (true)
      {
        Filter(0);

        if (this.parser.PeekNextKeyword().Equals("template"))
        {
          ConsumeTemplate();
        }
        else if (NextIsFunction())
        {
          OnFunction();
        }
        else if (NextIsClass())
        {
          this.parser.NextKeyword(); // consume "class"
          StringBuilder sb = new StringBuilder();
          sb.Append(this.parser.NextKeyword());

          while (!this.parser.PeekNextKeyword().Equals("{") &&
                 !this.parser.PeekNextKeyword().EndsWith(":") &&
                 !this.parser.PeekNextKeyword().Equals("where"))
            sb.Append(this.parser.NextKeyword());

          while (!this.parser.PeekNextKeyword().Equals("{"))
            this.parser.NextKeyword();

          this.contexts.Add(sb.ToString());
        }
        else if (this.parser.PeekNextKeyword(1).Equals("{") &&
                (this.parser.PeekNextKeyword(2).Equals("get") || this.parser.PeekNextKeyword(2).Equals("set")) &&
                (this.parser.PeekNextKeyword(3).Equals("{"))
          )
        {
          this.contexts.Add(this.parser.NextKeyword());
          this.parser.NextKeyword(); // consume "{"
        }
        else if ((this.parser.PeekNextKeyword().Equals("get") ||
                  this.parser.PeekNextKeyword().Equals("set")) &&
                 (this.parser.PeekNextKeyword(1).Equals("{")))
        {
          throw new CCCParserSuccessException(GetFullyQualifiedFunctionName(this.parser.NextKeyword()), this.parser.StreamOffset);
        }
        else if (this.parser.PeekNextKeyword().Equals("}"))
        {
          this.parser.NextKeyword();

          // remove last context, we have reached end of scope
          if (this.contexts.Count > 0)
            this.contexts.RemoveAt(this.contexts.Count - 1);
        }
        else if (this.parser.PeekNextKeyword().Equals("["))
        {
          while (!this.parser.PeekNextKeyword().Equals("]"))
            this.parser.NextKeyword();

          this.parser.NextKeyword();
        }
        else
        {
          this.parser.NextKeyword();
        }

      }
    }


    public bool NextIsLocalFunction()
    {
      return false;
    }
  }
}

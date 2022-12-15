using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CCMEngine
{
  public class JSParser : IFunctionStream
  {
    private LookAheadLangParser parser;
    private List<string> branchKeywords = new List<string>() { "if", "while", "for", "switch", "case", "foreach", "do" };

    private List<string> context = new List<string>();

    public JSParser(LookAheadLangParser parser)
    {
      this.parser = parser;
    }

    public bool NextIsLocalFunction()
    {
      return NextIsFunction();
    }

    private bool IsFunctionDefinition(int nameOffset)
    {
      string name = this.parser.PeekNextKeyword(nameOffset);

      // could validate name for illegal characters..

      if (this.parser.PeekNextKeyword(nameOffset + 1).Equals("("))
      {
        int offset = nameOffset;
        while (!this.parser.PeekNextKeyword(offset++).Equals(")"))
          ;

        // check for TypeScript return type
        if (this.parser.PeekNextKeyword(offset).Equals(":"))
        {
          offset += 2;
        }

        if (this.parser.PeekNextKeyword(offset).Equals("{"))
          return true;

      }

      return false;
    }

    private bool NextIsMemberFunction()
    {
      // check for member function -- NAME : function(params) {} 
      if (this.parser.PeekNextKeyword(1).Equals(":") && this.parser.PeekNextKeyword(2).Equals("function") && IsFunctionDefinition(2))
        return true;

      return false;
    }

    private bool NextIsFunctionAssignment()
    {
      // X.Y = function(args) { ... }
      if (this.parser.PeekNextKeyword(1).Equals("=") && this.parser.PeekNextKeyword(2).Equals("function") && IsFunctionDefinition(2))
        return true;

      return false;
    }

    private bool NextIsAnonymousFunction()
    {
      // function() {}
      if (IsFunctionDefinition(0) && !this.branchKeywords.Contains(this.parser.PeekNextKeyword()))
        return true;

      return false;
    }

    private bool NextIsDualNamedFunction()
    {
      // AFunction: function BFunction() {}
      if (this.parser.PeekNextKeyword(1).Equals(":") && this.parser.PeekNextKeyword(2).Equals("function") && IsFunctionDefinition(3))
        return true;

      return false;
    }

    public bool NextIsFunction()
    {
      if (this.parser.PeekNextKeyword().Equals("function") && IsFunctionDefinition(1))
        return true;

      if (NextIsMemberFunction())
        return true;

      if (NextIsFunctionAssignment())
        return true;

      if (NextIsAnonymousFunction())
        return true;

      if (NextIsDualNamedFunction())
        return true;

      return false;
    }

    private void AppendUntilFunctionDeclarationEnds(StringBuilder name)
    {
      while (!this.parser.PeekNextKeyword().Equals(")"))
        name.Append(this.parser.NextKeyword());

      name.Append(this.parser.NextKeyword());

      // move stream forward to {
      while (!this.parser.PeekNextKeyword().Equals("{"))
        this.parser.NextKeyword();
    }

    private StringBuilder CreateBuilderForFunctionName()
    {
      StringBuilder name = new StringBuilder();

      this.context.ForEach(c =>
        {
          name.Append(c);
          name.Append("::");
        }
      );

      return name;
    }

    public string GetFunctionName()
    {
      if (!NextIsFunction())
        throw new JSParserException("Call AdvanceToNextFunction() instead of GetFunctionName().");

      if (this.parser.PeekNextKeyword().Equals("function") && IsFunctionDefinition(1))
      {
        this.parser.NextKeyword();

        StringBuilder name = CreateBuilderForFunctionName();
        AppendUntilFunctionDeclarationEnds(name);

        return name.ToString();
      }
      else if (NextIsMemberFunction() || NextIsFunctionAssignment())
      {
        StringBuilder name = CreateBuilderForFunctionName();

        name.Append(this.parser.NextKeyword());
        this.parser.NextKeyword(); // :
        this.parser.NextKeyword(); // function

        AppendUntilFunctionDeclarationEnds(name);

        return name.ToString();
      }
      else if (NextIsDualNamedFunction())
      {
        StringBuilder name = CreateBuilderForFunctionName();

        this.parser.NextKeyword(); // extra name
        this.parser.NextKeyword(); // :
        this.parser.NextKeyword(); // function
        name.Append(this.parser.NextKeyword());

        AppendUntilFunctionDeclarationEnds(name);

        return name.ToString();
      }
      else if (NextIsAnonymousFunction())
      {
        StringBuilder name = CreateBuilderForFunctionName();
        name.Append(this.parser.NextKeyword()); // function

        AppendUntilFunctionDeclarationEnds(name);

        return name.ToString();
      }

      return "Internal parser error.";
    }

    public void AdvanceToNextFunction()
    {
      while (true)
      {
        if (this.parser.PeekNextKeyword().Equals("class") || this.parser.PeekNextKeyword().Equals("module"))
        {
          this.parser.NextKeyword();
          this.context.Add(this.parser.NextKeyword());

          while (!this.parser.PeekNextKeyword().Equals("{"))
            this.parser.NextKeyword();

          this.parser.NextKeyword(); // consume "{"
        }
        else if (this.parser.PeekNextKeyword().Equals("}"))
        {
          this.parser.NextKeyword(); // consume "}"

          if (this.context.Count > 0)
            this.context.RemoveAt(this.context.Count - 1);
        }
        else if (NextIsInterface())
        {
          ConsumeInterface();
        }
        else
        {
          if (NextIsFunction())
          {
            throw new CCCParserSuccessException(GetFunctionName(), this.parser.StreamOffset);
          }

          this.parser.NextKeyword();
        }
      }
    }

    public bool NextIsInterface()
    {
      return this.parser.PeekNextKeyword().Equals("interface");
    }

    public void ConsumeInterface()
    {
      if (!NextIsInterface())
        throw new JSParserException("Next in stream is not an interface and cannot be consumed.");

      while (!this.parser.PeekNextKeyword().Equals("{"))
        this.parser.NextKeyword();

      this.parser.NextKeyword(); // consume {
      int blockLevel = 1;

      while (blockLevel > 0)
      {
        if (this.parser.PeekNextKeyword().Equals("{"))
        {
          blockLevel++;
          this.parser.NextKeyword();
        }
        else if (this.parser.PeekNextKeyword().Equals("}"))
        {
          blockLevel--;
          this.parser.NextKeyword();
        }
        else
        {
          this.parser.NextKeyword();
        }
      }
    }
  }
}

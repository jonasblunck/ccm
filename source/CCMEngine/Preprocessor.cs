using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace CCMEngine
{
  public class PreprocessorException : Exception
  {
  }

  public class Preprocessor
  {
    StringBuilder sb = new StringBuilder();
    LookAheadLangParser parser = null;
    static string[] tokens = new string[] { "#", " ", "\r", "\n", "(", ")", "/", "*", "\t" };

    public static StreamReader GetTextStream(string text)
    {
      MemoryStream ms = new MemoryStream(ASCIIEncoding.ASCII.GetBytes(text));

      StreamReader sr = new StreamReader(ms);

      return sr;
    }

    public Preprocessor(StreamReader inputStream)
    {
      LookAheadTokenParser tokenParser = new LookAheadTokenParser(inputStream, Preprocessor.tokens, false);

      this.parser = new LookAheadLangParser(tokenParser);
    }

    public bool EvaluateExpression()
    {
      int value = 0;

      while (this.parser.PeekNextKeyword().Equals(" "))
        ConumseNextKeyWordSpaceOutput();

      if (int.TryParse(this.parser.PeekNextKeyword(), out value))
      {
        return value > 0;
      }

      return false; // let's just assume it evaluates to false
    }

    public static bool NextIsEndif(LookAheadLangParser parser)
    {
      if (parser.PeekNextKeyword().Equals("#"))
      {
        if (parser.PeekNextKeyword(1).Equals("endif") ||
            parser.PeekNextKeyword(1).Equals("end"))
        {
          return true;
        }
      }

      return false;
    }

    public static bool NextIsElse(LookAheadLangParser parser)
    {
      if (parser.PeekNextKeyword().Equals("#"))
      {
        string directive = Preprocessor.GetDirective(parser);

        if (directive.Equals("else"))
          return true;
      }

      return false;
    }

    public void ConsumeIfDef(LookAheadLangParser parser)
    {
      Debug.Assert(Preprocessor.NextIsIfdef(parser));

      while (!(parser.PeekNextKeyword().Equals("if") ||
          parser.PeekNextKeyword().Equals("ifdef") ||
          parser.PeekNextKeyword().Equals("def")))
      {
        ConumseNextKeyWordSpaceOutput();
      }

      ConumseNextKeyWordSpaceOutput();
    }

    public void ConsumeIfndef(LookAheadLangParser parser)
    {
      Debug.Assert(Preprocessor.NextIsIfndef(parser));

      while (true)
      {
        if (parser.PeekNextKeyword().Equals("ifndef"))
        {
          break;
        }

        ConumseNextKeyWordSpaceOutput();
      }

      ConumseNextKeyWordSpaceOutput();
    }

    private static string GetDirective(LookAheadLangParser parser)
    {
      if (parser.PeekNextKeyword().Equals("#"))
      {
        int offset = 1;

        while (parser.PeekNextKeyword(offset).Equals(" "))
          offset++;

        return parser.PeekNextKeyword(offset);
      }

      throw new UnknownStructureException("Internal pre processor error.");
    }

    public static bool NextIsIfndef(LookAheadLangParser parser)
    {
      if (parser.PeekNextKeyword().Equals("#"))
      {
        string directive = Preprocessor.GetDirective(parser);

        if (directive.Equals("ifndef"))
          return true;
      }

      return false;
    }

    public static bool NextIsIfdef(LookAheadLangParser parser)
    {
      if (parser.PeekNextKeyword().Equals("#"))
      {
        string directive = Preprocessor.GetDirective(parser);

        if (directive.Equals("if") ||
            directive.Equals("ifdef"))
        {
          return true;
        }
      }

      return false;
    }

    private static bool NextIsCommentBlock(LookAheadLangParser parser)
    {
      if (parser.PeekNextKeyword(0).Equals("/") && (
          parser.PeekNextKeyword(1).Equals("/") || parser.PeekNextKeyword(1).Equals("*")))
        return true;
   
      return false;
    }

    private void ConsumeDirectiveMultiLine()
    {
      // move to next line but keep an eye out for multi line defines
      string line = MoveToNextLineSpaceOutput().Trim();

      while (line.EndsWith("\\"))
        line = MoveToNextLineSpaceOutput().Trim();
    }

    private string MoveToNextLineSpaceOutput()
    {
      string line = this.parser.MoveToNextLine();

      foreach (char c in line)
      {
          if (c != '\n')
              this.sb.Append(" ");
          else
              this.sb.Append(c);
      }

      return line;
    }

    private void ConumseNextKeyWordSpaceOutput()
    {
      string keyWord = this.parser.NextKeyword();

      foreach (char c in keyWord)
        this.sb.Append(" ");
    }

    private void ConsumeComments()
    {
      if (this.parser.PeekNextKeyword(1).Equals("/"))
      {
        MoveToNextLineSpaceOutput();
      }
      else if (this.parser.PeekNextKeyword(1).Equals("*"))
      {
        string comment = this.parser.ConsumeBlockComment();
        for (int i = 0; i < comment.Length; ++i)
          this.sb.Append(" ");
      }
      else
      {
        // likely bad comment; I'd better progress stream to avoid infinite looping
        ConumseNextKeyWordSpaceOutput();
      }
    }

    public StreamReader Process()
    {
      try
      {
        Stack<bool> blockInclusion = new Stack<bool>();

        blockInclusion.Push(true); // start by including next block of text

        do
        {
          if (Preprocessor.NextIsIfdef(this.parser))
          {
            ConsumeIfDef(this.parser);

            // if we are currently excluding a block, i.e nested #if's, just push 0 onto the inclusion
            if (!blockInclusion.Peek())
              blockInclusion.Push(false);
            else
              blockInclusion.Push(EvaluateExpression());

            MoveToNextLineSpaceOutput();
          }
          else if (Preprocessor.NextIsElse(this.parser) && blockInclusion.Count > 1)
          {
            // just reverse the inclusion book-keeping

            bool blockShouldInclude = blockInclusion.Pop();

            // check if parent inclusing says no
            if (!blockInclusion.Peek())
              blockInclusion.Push(false);
            else
              blockInclusion.Push(!blockShouldInclude);

            MoveToNextLineSpaceOutput();
          }
          else if (Preprocessor.NextIsEndif(this.parser) && blockInclusion.Count > 1)
          {
            blockInclusion.Pop();
            MoveToNextLineSpaceOutput();
          }
          else if (Preprocessor.NextIsIfndef(this.parser))
          {
            ConsumeIfndef(this.parser);

            if (!blockInclusion.Peek())
              blockInclusion.Push(false);
            else
              blockInclusion.Push(!EvaluateExpression());

            MoveToNextLineSpaceOutput();
          }
          else if (Preprocessor.NextIsCommentBlock(this.parser))
          {
            ConsumeComments();
          }
          else if (this.parser.PeekNextKeyword().Equals("#"))
          {
            ConsumeDirectiveMultiLine();
          }
          else
          {
            if (blockInclusion.Peek())
              this.sb.Append(this.parser.NextKeyword());
            else
              ConumseNextKeyWordSpaceOutput(); // move the stream ahead without including its data...
          }
        }
        while (true);
      }
      catch (EndOfStreamException)
      {
      }
      catch (Exception)
      {
        throw new PreprocessorException();
      }

      return Preprocessor.GetTextStream(this.sb.ToString());
    }
  }
}

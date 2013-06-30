using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CCMEngine
{
  public class LookAheadTokenParserBase
  {
    LookAheadParser parser = null;
    LookAheadCommentPreprocessor preprocessor = null;
    List<string> tokens = null;
    List<char> buffer = new List<char>();

    public LookAheadTokenParserBase(StreamReader stream, string[] tokens)
    {
      this.parser = new LookAheadParser(stream);
      this.tokens = new List<string>(tokens);
      this.preprocessor = new LookAheadCommentPreprocessor(this.parser);
    }

    private bool StopOnNextToken()
    {
      if (this.parser.EndOfStream || NextIsToken() || this.preprocessor.NextIsSingleLineComment())
        return true;

      return false;
    }

    private bool NextIsToken()
    {
      try
      {
        foreach (string tok in this.tokens)
        {
          bool isToken = true;

          for (int i = 0; i < tok.Length; ++i)
          {
            if (tok[i] != this.parser.Peek(i))
            {
              isToken = false;
              break;
            }
          }

          if (isToken)
            return true;
        }
      }
      catch (EndOfStreamException)
      {
      }

      return false;
    }

    private void FillNextToken()
    {
      this.preprocessor.Preprocess(); // removes block of comments

      if (this.parser.EndOfStream)
        throw new EndOfStreamException();

      do
      {
        this.buffer.Add(this.parser.Read());

        if (this.tokens.Contains(new string(this.buffer.ToArray())))
          break;  // we have parsed an entire token!
      }
      while (!StopOnNextToken());
    }
   
    private void FillBufferIfNeeded()
    {
      if (this.buffer.Count == 0)
      {
        FillNextToken();
      }
    }

    public string PeekNextToken()
    {
      FillBufferIfNeeded();

      return new string(this.buffer.ToArray());
    }

    public string NextToken()
    {
      FillBufferIfNeeded();

      string nextToken = new string(this.buffer.ToArray());

      this.buffer.Clear();

      return nextToken;
    }
  }
}

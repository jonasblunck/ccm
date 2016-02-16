using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CCMEngine
{
  public class LookAheadTokenParser
  {
    PeekableStream parser = null;
    List<string> tokens = null;
    List<char> buffer = new List<char>();
    bool consumeWhiteSpace = true;

    public LookAheadTokenParser(StreamReader stream, string[] tokens, bool consumeWhiteSpace)
    {
      this.parser = new PeekableStream(stream);
      this.tokens = new List<string>(tokens);
      this.consumeWhiteSpace = consumeWhiteSpace;

      AddTokens(new string[] { " ", "\r", "\n", "\"", "\'" });
    }

    private void AddTokens(string[] items)
    {
      foreach (string tok in items)
      {
        if (!this.tokens.Contains(tok))
          this.tokens.Add(tok);
      }
    }

    private bool NextIsSpace()
    {
      return char.IsWhiteSpace(this.parser.Peek(0));
    }

    private bool NextIsToken()
    {
      try
      {
        if (NextIsQuotedText())
          return true;

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
      if (this.consumeWhiteSpace)
      {
        while (NextIsSpace()){
          this.parser.Read();
        }
      }

      do
      {
        this.buffer.Add(this.parser.Read());

        if (this.tokens.Contains(new string(this.buffer.ToArray())))
          break;  // we have parsed an entire token!
      }
      while (!NextIsToken());
    }

    private void FillQuotedText()
    {
      bool containsEscapeChars = true; // c# style comments, i.e @"C:\Program files" does not have escape chars

      if (this.parser.Peek(0).Equals('@'))
      {
        this.buffer.Add(this.parser.Read()); // just consume the c# style beginning of comment
        containsEscapeChars = false;
      }

      char delimiter = this.parser.Peek(0);

      do
      {
        if (containsEscapeChars && this.parser.Peek(0).Equals('\\'))
        {
          this.buffer.Add(this.parser.Read());

          if (this.parser.Peek(0).Equals('"') || this.parser.Peek(0).Equals('\'') || this.parser.Peek(0).Equals('\\'))
            this.buffer.Add(this.parser.Read());
        }
        else
        {
          this.buffer.Add(this.parser.Read());
        }
      }
      while (!this.parser.Peek(0).Equals(delimiter));

      this.buffer.Add(this.parser.Read());
    }

    public bool NextIsQuotedText()
    {
      char nextChar = this.parser.Peek(0);

      if (nextChar.Equals('"') || nextChar.Equals('\''))
      {
        return true;
      }

      if (!nextChar.Equals('@')){
        return false;
      }

      char nextNextChar = this.parser.Peek(1);

      if (nextNextChar.Equals('"') || nextNextChar.Equals('\''))
      {
        return true;
      }

      return false;
    }

    private void FillBuffer()
    {
      try
      {
        while (NextIsSpace() && this.consumeWhiteSpace){
          this.parser.Read();
        }

        if (NextIsQuotedText()){
          FillQuotedText();
        }
        else {
          FillNextToken();
        }
      }
      catch (EndOfStreamException)
      {
        if (this.buffer.Count == 0)
          throw; // the stream is empty and we have collected nothing
      }

    }

    public char NextChar()
    {
      if (this.buffer.Count > 0)
      {
        char c = this.buffer[0];
        this.buffer.RemoveAt(0);

        return c;
      }

      return this.parser.Read();
    }


    private void FillBufferIfNeeded()
    {
      if (this.buffer.Count == 0)
      {
        FillBuffer();
      }
    }

    public string MoveToNextLine()
    {
      StringBuilder sb = new StringBuilder();
      sb.Append(this.buffer.ToArray());
      this.buffer.Clear();

      try
      {
        while(true)
        {
            char c = this.parser.Peek(0);
            if ('\r' == c || '\n' == c)
            {
                 break;
            }
            sb.Append(this.parser.Read());
        }

        while(true)
        {
            char c = this.parser.Peek(0);
            if ('\r' != c && '\n' != c)
            {
                 break;
            }
            sb.Append(this.parser.Read());
        }
      }
      catch (EndOfStreamException)
      {
      }

      return sb.ToString().TrimEnd(new char[] { '\r', '\n' });
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
    
  public int StreamOffset
  {
    get { return this.parser.StreamOffset; }
  }
  }
}

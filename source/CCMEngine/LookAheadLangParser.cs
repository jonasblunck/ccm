using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace CCMEngine
{
    public class LookAheadLangParser
    {
        LookAheadTokenParser parser = null;
        List<string> tokens = new List<string>();

        public LookAheadLangParser(StreamReader stream, string[] tokens)
        {
            this.parser = new LookAheadTokenParser(stream, tokens, true);
        }

        public LookAheadLangParser(LookAheadTokenParser tokenParser)
        {
            this.parser = tokenParser;
        }

        private void ExpandLookAHeadTokens(int requestedSize)
        {
            if (requestedSize > tokens.Count)
            {
                for (int i = tokens.Count; i < requestedSize; ++i)
                    this.tokens.Add(this.parser.NextToken());
            }
        }

        private string PopFront()
        {
            string front = this.tokens[0];
            this.tokens.RemoveAt(0);

            return front;
        }

        public string NextKeyword()
        {
            ExpandLookAHeadTokens(1);

            return PopFront();
        }

        public string PeekNextKeyword(int offset)
        {
            ExpandLookAHeadTokens(offset + 1);

            return this.tokens[offset];
        }

        private char NextChar()
        {
            if (this.tokens.Count > 0)
            {
                string tok = tokens[0];
                char c = tok[0];

                if (tok.Length == 1)
                    tokens.RemoveAt(0);
                else
                    tokens[0] = tok.Substring(1);

                return c;
            }

            return this.parser.NextChar();
        }

        public string ConsumeBlockComment()
        {
            StringBuilder comment = new StringBuilder();

            if (!NextChar().Equals('/'))
                throw new UnknownStructureException("Expected beginning of comment.");

            if (!NextChar().Equals('*'))
                throw new UnknownStructureException("Expected beginning of comment.");

            comment.Append("/*");

            while (true)
            {
                char c = NextChar();
                comment.Append(c);

                while (c.Equals('*'))
                {
                    c = NextChar();
                    comment.Append(c);

                    if (c.Equals('/'))
                        return comment.ToString();
                }
            }
        }

        public string PeekNextKeyword()
        {
            return PeekNextKeyword(0);
        }

        public string MoveToNextLine()
        {
            StringBuilder sb = new StringBuilder();

            foreach (string tok in this.tokens)
                sb.Append(tok);

            this.tokens.Clear();
            sb.Append(this.parser.MoveToNextLine());

            return sb.ToString();
        }

        public int StreamOffset
        {
            get { return this.parser.StreamOffset; }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CCMEngine
{
    public class PSParser : IFunctionStream
    {
        private LookAheadLangParser parser;

        public PSParser(LookAheadLangParser parser)
        {
            this.parser = parser;
        }

        public bool NextIsLocalFunction()
        {
            return false;  // must look into later
        }

        public bool NextIsFunction()
        {
            return false;
        }

        public void AdvanceToNextFunction()
        {
            while (true)
            {
                this.parser.NextKeyword(); // consumed and move forward
            }
        }
    }
}

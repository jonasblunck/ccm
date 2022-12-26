using System;
using CCMEngine;
using System.IO;

namespace CCMEngine
{
	public class LookAheadLangParserFactory
	{
            public static LookAheadLangParser CreateJavascriptParser(StreamReader stream)
            {
                return new LookAheadLangParser(stream,
                  new string[] { "{", "}", ":", ";", "(", ")", "&&", "||", "\"", "'", "\t" });
            }

            public static LookAheadLangParser CreateCppParser(StreamReader stream)
            {
                return new LookAheadLangParser(stream,
                  new string[] { "{", "}", ";", "(", ")", "//", "/*", "::", "&&", "||", "\"", "'", "[", "]", "#", " ", "\r", "\n", ">", "<", "->", "*", "\t", "," });
            }

            public static LookAheadLangParser CreatePowerShellParser(StreamReader stream)
            {
                return new LookAheadLangParser(stream,
                  new string[] { "{", "}", "(", ")", "\t" });
            }

            public static LookAheadLangParser CreateLangParser(StreamReader stream, string fileName)
            {
                if (fileName.ToLower().EndsWith(".js") || fileName.ToLower().EndsWith(".ts"))
                    return LookAheadLangParserFactory.CreateJavascriptParser(stream);
                else if (fileName.ToLower().EndsWith(".ps1") || fileName.ToLower().EndsWith(".psm1"))
                    return LookAheadLangParserFactory.CreatePowerShellParser(stream);
                else
                    return LookAheadLangParserFactory.CreateCppParser(stream);
            }
	}
}


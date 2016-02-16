using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace CCMEngine
{
  public interface ICCMNotify
  {
    void OnMetric(ccMetric metric, object context);
  }

    /// <summary>
    /// Class to encapsulate access to the char buffer
    /// </summary>
    public class CharBuffer
    {
        /// <summary>
        /// keep the character buffer private to control access
        /// </summary>
        char[] buffer = null;

        /// <summary>
        /// a list of offsets to newlines
        /// </summary>
        List<int> newLineOffsetList;

        public CharBuffer(long size)
        {
            buffer = new char[size];
            newLineOffsetList = new List<int>();
        }

        public void ReadEntireStream(StreamReader filestream)
        {
            filestream.Read(buffer, 0, buffer.Length);
        }

        public MemoryStream GetBytesMemoryStream()
        {
            return new MemoryStream(Encoding.Default.GetBytes(buffer));
        }

        public int GetLineNumber(int offset)
        {
          int lineNumber = 1;
          int startingLineNumber = 0;

          // simplify the loop counter
          int maxLine = Math.Min(offset, buffer.Length);

          if (newLineOffsetList.Count > 0)
          {
            if (maxLine < newLineOffsetList[newLineOffsetList.Count - 1])
            {
                // we have already called GetLineNumber on this file with an offset
                // less than where we are currently looking, simply search through all the newlines
                // to find the last line
                for (int newLineNumber=0;newLineNumber<newLineOffsetList.Count; newLineNumber++)
                {
                    if (offset < newLineOffsetList[newLineNumber])
                    {
                        return newLineNumber;
                    }
                }
            }
            else {
                // File has been partially read before fast forward to last seen '\n'
                // and use that to seed the loop at somepoint into the buffer
                startingLineNumber = newLineOffsetList[newLineOffsetList.Count - 1]+1;
                lineNumber = newLineOffsetList.Count+1;
            }
          }

          

          // I need to search for the newlines
          for (int i = startingLineNumber; i < maxLine ; ++i)
          {
            if (buffer[i].Equals('\n'))
            {
                ++lineNumber;

                // when I see a newline remember the offset to the CR
                // lineNumber will then be the index
                newLineOffsetList.Add(i);
            }
          }

          return lineNumber;
        }
  }

  public class FileAnalyzer
  {
    ICCMNotify callback = null;
    LookAheadLangParser parser = null;
    object context = null;
    bool suppressMethodSignatures = false;
    string filename;
    CharBuffer buffer;
    
    ParserSwitchBehavior switchBehavior;

    public FileAnalyzer(StreamReader filestream, ICCMNotify callback, object context, bool suppressMethodSignatures, string filename,
      ParserSwitchBehavior switchBehavior = ParserSwitchBehavior.TraditionalInclude)
    {
      this.buffer = new CharBuffer(filestream.BaseStream.Length);
      this.buffer.ReadEntireStream(filestream);

      var processStream = new StreamReader(buffer.GetBytesMemoryStream());

      // run through preprocessor before setting up parser...
      Preprocessor preprocessor = new Preprocessor(processStream);
      StreamReader stream = preprocessor.Process();

      // this construct should be fixed and support OCP
      if (filename.ToLower().EndsWith(".js") || filename.ToLower().EndsWith(".ts"))
        this.parser = LookAheadLangParser.CreateJavascriptParser(stream);
      else
        this.parser = LookAheadLangParser.CreateCppParser(stream);

      this.callback = callback;
      this.context = context;
      this.suppressMethodSignatures = suppressMethodSignatures;
      this.filename = filename;
      this.switchBehavior = switchBehavior;
    }

    private int GetLineNumber(int offset)
    {
        return buffer.GetLineNumber(offset);
    }

    private void OnLocalFunction(IFunctionStream functionStream)
    {
      try
      {
        functionStream.AdvanceToNextFunction();
      }
      catch (CCCParserSuccessException info)
      {
        OnFunction(info.Function, info.StreamOffset, functionStream);
      }
    }

    private void OnFunction(string unit, int streamOffset, IFunctionStream funcStream)
    {
      BlockAnalyzer analyzer = new BlockAnalyzer(this.parser, funcStream, this.OnLocalFunction, this.switchBehavior);
      int ccm = 1 + analyzer.ConsumeBlockCalculateAdditionalComplexity();

      var metric = new ccMetric(this.filename, unit, ccm);
      metric.StartLineNumber = GetLineNumber(streamOffset);
      metric.EndLineNumber = GetLineNumber(this.parser.StreamOffset);

      this.callback.OnMetric(metric, this.context);
    }

    public static IFunctionStream CreateFunctionStream(LookAheadLangParser parser, string filename, bool suppressMethodSignatures)
    {
      if (filename.ToLower().EndsWith(".js") || filename.ToLower().EndsWith(".ts"))
        return new JSParser(parser);

      return new CCCParser(parser, suppressMethodSignatures);
    }

    public void Analyze()
    {
      try
      {
        IFunctionStream functionFinder = CreateFunctionStream(this.parser, this.filename, this.suppressMethodSignatures);

        while(true)
        {
          try
          {
            functionFinder.AdvanceToNextFunction(); // will throw CCCParserSuccessException when it finds a function
          }
          catch(CCCParserSuccessException info)
          {
            OnFunction(info.Function, info.StreamOffset, functionFinder);
          }
        }
      }
      catch (EndOfStreamException)
      {
        // we are done!
      }

      
    }

  }
}

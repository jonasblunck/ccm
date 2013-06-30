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

  public class FileAnalyzer
  {
    ICCMNotify callback = null;
    LookAheadLangParser parser = null;
    object context = null;
    bool suppressMethodSignatures = false;
    string filename;
    char[] buffer = null;

    public FileAnalyzer(StreamReader filestream, ICCMNotify callback, object context, bool suppressMethodSignatures, string filename)
    {
      this.buffer = new char[filestream.BaseStream.Length];
      filestream.Read(this.buffer, 0, this.buffer.Length);

      var processStream = new StreamReader(new MemoryStream(Encoding.Default.GetBytes(this.buffer)));

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
    }

    private int GetLineNumber(int offset)
    {
      int lineNumber = 1;

      for (int i = 0; i < offset && i < this.buffer.Length ; ++i)
      {
        if (this.buffer[i].Equals('\n'))
          ++lineNumber;
      }

      return lineNumber;
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
      BlockAnalyzer analyzer = new BlockAnalyzer(this.parser, funcStream, this.OnLocalFunction);
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

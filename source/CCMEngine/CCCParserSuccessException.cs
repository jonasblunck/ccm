using System;
using System.Collections.Generic;
using System.Text;

namespace CCMEngine
{
  public class CCCParserSuccessException : Exception
  {
    private string function;
    private int streamOffset;

    public CCCParserSuccessException(string function, int streamOffset)
    {
      this.function = function;
      this.streamOffset = streamOffset;      
    }

    public string Function
    {
      get
      {
        return this.function;
      }
    }
    
    public int StreamOffset
    {
      get
      {
        return this.streamOffset;
      }
    }
        
  }
}

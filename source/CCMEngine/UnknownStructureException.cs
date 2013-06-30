using System;
using System.Collections.Generic;
using System.Text;

namespace CCMEngine
{
  public class UnknownStructureException : Exception
  {
    private string message;

    public UnknownStructureException(string message)
    {
      this.message = message;
    }

    public override string Message 
    {
      get
      {
        return this.message;
      }
    }
  }
}

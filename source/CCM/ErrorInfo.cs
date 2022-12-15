using System;
using System.Collections.Generic;
using System.Text;

namespace CCM
{
  public class ErrorInfo
  {
    public ErrorInfo(string file, string message)
    {
      this.File = file;
      this.Message = message;
    }

    public string File { get; private set; }
    public string Message { get; private set; }
  }
}

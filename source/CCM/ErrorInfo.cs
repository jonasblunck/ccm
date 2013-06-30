using System;
using System.Collections.Generic;
using System.Text;

namespace CCM
{
  public class ErrorInfo
  {
    private string file;
    private string message;

    public ErrorInfo(string file, string message)
    {
      this.file = file;
      this.message = message;
    }

    public string File
    {
      get
      {
        return this.file;
      }
    }

    public string Message
    {
      get
      {
        return this.message;
      }
    }
  }
}

using System;
using System.Text;
using System.IO;

namespace CCMTests
{
  public class TestUtil
  {
    public static StreamReader GetTextStream(string text)
    {
      MemoryStream ms = new MemoryStream(ASCIIEncoding.ASCII.GetBytes(text));

      StreamReader sr = new StreamReader(ms);

      return sr;
    }

    
  }
}

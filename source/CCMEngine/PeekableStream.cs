using System;
using System.Collections.Generic;
using System.IO;

namespace CCMEngine
{
  public class PeekableStream
  {
    StreamReader stream = null;
    List<char> localStreamBuffer = new List<char>();
	private int streamOffset = 0;

    public PeekableStream(StreamReader reader)
    {
      this.stream = reader;
    }

    public char Peek(int offset)
    {
        int end = offset + 1;

        if (this.localStreamBuffer.Count < end) {
            while ((this.localStreamBuffer.Count < end ) && 
                   !this.stream.EndOfStream) 
            {
                this.localStreamBuffer.Add((char)this.stream.Read());
            }
        }

        if (end > this.localStreamBuffer.Count)
        {
            throw new EndOfStreamException();
        }

        return this.localStreamBuffer[offset];
    }

    public char Read()
    {
	  char ch;
			
      if (this.localStreamBuffer.Count > 0) {
        ch = this.localStreamBuffer[0];
        this.localStreamBuffer.RemoveAt(0);
      }
      else if (this.stream.EndOfStream) {
        throw new EndOfStreamException();
      }
      else {
        ch =  (char)this.stream.Read();
      }
			
	  this.streamOffset++;
	  return ch;
    }
	
  	public int StreamOffset
  	{
  		get { return this.streamOffset; }
  	}

  }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CCMEngine
{
  public interface IFunctionStream
  {
    void AdvanceToNextFunction();
    bool NextIsLocalFunction();
  }
}

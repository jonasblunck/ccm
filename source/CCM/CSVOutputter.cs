using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CCMEngine;

namespace CCM
{
  class CSVOutputter : CCMOutputter
  {
    public override void Output(List<ccMetric> metrics, List<ErrorInfo> errors, bool verbose)
    {
      if (metrics.Count() > 0)
      {
        Console.WriteLine("Method name,Complexity,Category,Filename,Start line,End line,SLoC");

        metrics.ForEach(m =>
          {
            Console.WriteLine("\"{0}\",{1},{2},{3},{4},{5},{6}",
              m.Unit, m.CCM, m.Classification, m.Filename, m.StartLineNumber, m.EndLineNumber,
              (m.EndLineNumber - m.StartLineNumber));
          }
        );
      }

      if (verbose)
        foreach (ErrorInfo error in errors)
          Console.WriteLine("Error in file '{0}' : {1}", error.File, error.Message);
    }

  }
}

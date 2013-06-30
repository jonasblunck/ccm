using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CCMEngine;

namespace CCM
{
  class TabbedOutputter : CCMOutputter
  {
    public override void Output(List<ccMetric> metrics, List<ErrorInfo> errors, bool verbose)
    {
      if (metrics.Count() > 0)
      {
        Console.WriteLine("Method name\tComplexity\tCategory\tFilename\tStart line\tEnd line\tSLoC");

        metrics.ForEach(m =>
          {
            Console.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}",
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

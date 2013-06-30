using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using CCMEngine;

namespace CCM
{
  class ConsoleOutputter : CCMOutputter
  {
    public override void Output(List<ccMetric> metrics, List<ErrorInfo> errors, bool verbose)
    {
      foreach (ccMetric metric in metrics)
        Console.WriteLine("{0} : {1} - {2} ({3}@line {4})", 
          metric.Unit, metric.CCM, metric.Classification, metric.Filename, metric.StartLineNumber);

      if (verbose)
        foreach (ErrorInfo error in errors)
          Console.WriteLine("Error in file '{0}' : {1}", error.File, error.Message);
    }
  }
}

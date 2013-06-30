using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using CCMEngine;

namespace CCM
{
  class XmlOutputter : CCMOutputter
  {
    private static string XmlAdjust(string text)
    {
      return System.Security.SecurityElement.Escape(text);
    }

    public override void Output(List<ccMetric> metrics, List<ErrorInfo> errors, bool verbose)
    {
      Console.WriteLine("<ccm>");

      foreach (ccMetric metric in metrics)
      {
        Console.WriteLine("  <metric>");
        Console.WriteLine("    <complexity>{0}</complexity>", metric.CCM);
        Console.WriteLine("    <unit>{0}</unit>", XmlOutputter.XmlAdjust(metric.Unit));
        Console.WriteLine("    <classification>{0}</classification>", metric.Classification);
        Console.WriteLine("    <file>{0}</file>", metric.Filename);
        Console.WriteLine("    <startLineNumber>{0}</startLineNumber>", metric.StartLineNumber);
        Console.WriteLine("    <endLineNumber>{0}</endLineNumber>", metric.EndLineNumber);
        Console.WriteLine("    <SLOC>{0}</SLOC>", (metric.EndLineNumber - metric.StartLineNumber).ToString());
        Console.WriteLine("  </metric>");
      }

      if (verbose && (errors.Count > 0))
      {
        Console.WriteLine("    <errors>");

        foreach (ErrorInfo error in errors)
        {
          Console.WriteLine("      <error>");
          Console.WriteLine("        <file>{0}</file>", error.File);
          Console.WriteLine("        <message>{0}</message>", error.Message);
          Console.WriteLine("      </error>");
        }
        Console.WriteLine("    </errors>");
      }

      Console.WriteLine("</ccm>");

    }
  }
}

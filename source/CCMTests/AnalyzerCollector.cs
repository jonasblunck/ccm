using System;
using System.Collections.Generic;
using System.Text;
using CCMEngine;

namespace CCMTests
{
  public class AnalyzerCollector : ICCMNotify
  {
    private Dictionary<string, int> ccms = new Dictionary<string, int>();
    private Dictionary<string, ccMetric> metrics = new Dictionary<string, ccMetric>();

    public string GetUnitname(int index)
    {
      Dictionary<string, int>.Enumerator enumerator = this.ccms.GetEnumerator();

      if (enumerator.MoveNext())
      {
        for (int i = 0; i < index; ++i)
          enumerator.MoveNext();

        return enumerator.Current.Key;
      }

      return "";
    }

    public Dictionary<string, int> Collection
    {
      get
      {
        return this.ccms;
      }
    }

    public Dictionary<string, ccMetric> Metrics
    {
      get
      {
        return this.metrics;
      }
    }

    public void OnMetric(ccMetric metric, object context)
    {
      if (!this.ccms.ContainsKey(metric.Unit))
        this.ccms.Add(metric.Unit, metric.CCM);

      this.metrics.Add(metric.Unit, metric);
    }
  }
}

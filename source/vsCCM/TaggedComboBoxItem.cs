using System;
using System.Collections.Generic;
using System.Text;

namespace vsCCM
{
  public class TaggedComboBoxItem
  {
    private string text;
    private int tag;

    public TaggedComboBoxItem(string text, int tag)
    {
      this.text = text;
      this.tag = tag;
    }

    public int Tag
    {
      get
      {
        return this.tag;
      }
    }

    public override string ToString()
    {
      return this.text;
    }
  }
}

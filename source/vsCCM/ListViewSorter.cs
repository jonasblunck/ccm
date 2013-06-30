using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Windows.Forms;

namespace vsCCM
{
  class ListViewSorter : IComparer
  {
    public int Column { get; set; }
    public ListViewSorter(int colIndex)
    {
      Column = colIndex;
    }

    private int Compare(string a, string b)
    {
      int i, j;
      if (int.TryParse(a, out i) && int.TryParse(b, out j))
      {
        if (i >= j)
          return -1;

        return 1;
      }

      double d, e;
      if (double.TryParse(a, out d) && double.TryParse(b, out e))
      {
        if (d >= e)
          return -1;

        return 1;
      }

      float f, g;
      if (float.TryParse(a, out f) && float.TryParse(b,out g))
      {
        if (f >= g)
          return -1;

        return 1;
      }

      return String.Compare(a, b);
    }

    public int Compare(object a, object b)
    {
      ListViewItem itemA = a as ListViewItem;
      ListViewItem itemB = b as ListViewItem;
      if (itemA == null || itemB == null)
        return 0;

      return Compare(itemA.SubItems[Column].Text, itemB.SubItems[Column].Text);
    }
  }
}

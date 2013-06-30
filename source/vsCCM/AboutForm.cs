using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;

namespace vsCCM
{
  public partial class AboutForm : Form
  {
    public AboutForm()
    {
      InitializeComponent();
    }

    private void AboutForm_Load(object sender, EventArgs e)
    {
      this.labelVersion.Text =
        string.Format("v{0}", Assembly.GetExecutingAssembly().GetName().Version.ToString());

      string link = "http://www.blunck.se/ccm.html";
      this.linkLabel.Links.Add(0, link.Length, link);
      this.linkLabel.Click += new EventHandler(linkLabel_Click);
    }

    void linkLabel_Click(object sender, EventArgs e)
    {
      System.Diagnostics.Process.Start("http://www.blunck.se/ccm.html");
    }
  }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace vsCCM
{
  public partial class CoverageFileLocationForm : Form
  {
    private string browsePath;
    public CoverageFileLocationForm(string file)
    {
      InitializeComponent();

      this.labelMessage.Text = string.Format("Unable to find the file:\r\n'{0}'.", file);
    }

    private void CoverageFileLocationForm_Load(object sender, EventArgs e)
    {
    }

    private void buttonAbort_Click(object sender, EventArgs e)
    {
      this.DialogResult = DialogResult.Abort; 
      this.Close();
    }

    private void buttonIgnore_Click(object sender, EventArgs e)
    {
      this.DialogResult = DialogResult.Ignore;
      this.Close();
    }

    private void buttonBrowse_Click(object sender, EventArgs e)
    {
      FolderBrowserDialog browse = new FolderBrowserDialog();

      if (DialogResult.OK == browse.ShowDialog())
      {
        this.browsePath = browse.SelectedPath;
        this.DialogResult = DialogResult.OK;
        this.Close();
      }
    }

    public string SelectedPath
    {
      get
      {
        return this.browsePath;
      }
    }
  }
}

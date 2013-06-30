using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Threading;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace vsCCM
{
  public partial class WaitForm : Form
  {
    private int waitBeforeShow = 1;
    private ManualResetEvent completedEvent = null;
    private System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();

    public WaitForm(ManualResetEvent completedEvent, string message)
    {
      this.completedEvent = completedEvent;
      InitializeComponent();
      this.labelMessage.Text = message;

    }

    private void buttonCancel_Click(object sender, EventArgs e)
    {
      this.DialogResult = DialogResult.Cancel;
      this.Close(); // wait for this one..
    }

    public void Initialize()
    {
      this.timer.Interval = (this.waitBeforeShow * 1000);
      this.timer.Tick += new EventHandler(timer_Tick);
      this.timer.Start();
      this.ShowDialog();
    }

    void timer_Tick(object sender, EventArgs e)
    {
      bool completed = this.completedEvent.WaitOne(0);

      if (completed && this.Visible)
      {
        this.timer.Stop();
        this.Close();
      }
    }

    public string Message
    {
      set
      {
        this.labelMessage.Text = value;
      }
    }
  }
}

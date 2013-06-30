namespace ccmUI
{
  partial class ccmUI
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.ccmControl1 = new vsCCM.ccmControl();
      this.SuspendLayout();
      // 
      // ccmControl1
      // 
      this.ccmControl1.AllowDrop = true;
      this.ccmControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.ccmControl1.Location = new System.Drawing.Point(2, 4);
      this.ccmControl1.Name = "ccmControl1";
      this.ccmControl1.Size = new System.Drawing.Size(860, 294);
      this.ccmControl1.TabIndex = 0;
      // 
      // ccmUI
      // 
      this.AllowDrop = true;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(874, 307);
      this.Controls.Add(this.ccmControl1);
      this.Name = "ccmUI";
      this.Text = "ccm";
      this.ResumeLayout(false);

    }

    #endregion

    private vsCCM.ccmControl ccmControl1;
  }
}


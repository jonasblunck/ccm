namespace vsCCM
{
  partial class AboutForm
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
      this.linkLabel = new System.Windows.Forms.LinkLabel();
      this.labelName = new System.Windows.Forms.Label();
      this.labelVersion = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // linkLabel
      // 
      this.linkLabel.AutoSize = true;
      this.linkLabel.Location = new System.Drawing.Point(100, 83);
      this.linkLabel.Name = "linkLabel";
      this.linkLabel.Size = new System.Drawing.Size(158, 13);
      this.linkLabel.TabIndex = 0;
      this.linkLabel.TabStop = true;
      this.linkLabel.Text = "http://www.blunck.se/ccm.html";
      // 
      // labelName
      // 
      this.labelName.AutoSize = true;
      this.labelName.Location = new System.Drawing.Point(82, 25);
      this.labelName.Name = "labelName";
      this.labelName.Size = new System.Drawing.Size(194, 13);
      this.labelName.TabIndex = 1;
      this.labelName.Text = "ccm tool (cyclomatic complexity metrics)";
      // 
      // labelVersion
      // 
      this.labelVersion.AutoSize = true;
      this.labelVersion.Location = new System.Drawing.Point(161, 54);
      this.labelVersion.Name = "labelVersion";
      this.labelVersion.Size = new System.Drawing.Size(46, 13);
      this.labelVersion.TabIndex = 2;
      this.labelVersion.Text = "v0.0.0.0";
      // 
      // AboutForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(359, 130);
      this.Controls.Add(this.labelVersion);
      this.Controls.Add(this.labelName);
      this.Controls.Add(this.linkLabel);
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "AboutForm";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "About";
      this.Load += new System.EventHandler(this.AboutForm_Load);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.LinkLabel linkLabel;
    private System.Windows.Forms.Label labelName;
    private System.Windows.Forms.Label labelVersion;
  }
}
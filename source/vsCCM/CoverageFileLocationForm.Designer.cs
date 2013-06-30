namespace vsCCM
{
  partial class CoverageFileLocationForm
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
      this.buttonAbort = new System.Windows.Forms.Button();
      this.buttonIgnore = new System.Windows.Forms.Button();
      this.buttonBrowse = new System.Windows.Forms.Button();
      this.labelMessage = new System.Windows.Forms.Label();
      this.label1 = new System.Windows.Forms.Label();
      this.panel1 = new System.Windows.Forms.Panel();
      this.panel1.SuspendLayout();
      this.SuspendLayout();
      // 
      // buttonAbort
      // 
      this.buttonAbort.Location = new System.Drawing.Point(64, 12);
      this.buttonAbort.Name = "buttonAbort";
      this.buttonAbort.Size = new System.Drawing.Size(92, 34);
      this.buttonAbort.TabIndex = 0;
      this.buttonAbort.Text = "Abort";
      this.buttonAbort.UseVisualStyleBackColor = true;
      this.buttonAbort.Click += new System.EventHandler(this.buttonAbort_Click);
      // 
      // buttonIgnore
      // 
      this.buttonIgnore.Location = new System.Drawing.Point(173, 12);
      this.buttonIgnore.Name = "buttonIgnore";
      this.buttonIgnore.Size = new System.Drawing.Size(92, 34);
      this.buttonIgnore.TabIndex = 1;
      this.buttonIgnore.Text = "Ignore";
      this.buttonIgnore.UseVisualStyleBackColor = true;
      this.buttonIgnore.Click += new System.EventHandler(this.buttonIgnore_Click);
      // 
      // buttonBrowse
      // 
      this.buttonBrowse.Location = new System.Drawing.Point(283, 12);
      this.buttonBrowse.Name = "buttonBrowse";
      this.buttonBrowse.Size = new System.Drawing.Size(92, 34);
      this.buttonBrowse.TabIndex = 2;
      this.buttonBrowse.Text = "Browse...";
      this.buttonBrowse.UseVisualStyleBackColor = true;
      this.buttonBrowse.Click += new System.EventHandler(this.buttonBrowse_Click);
      // 
      // labelMessage
      // 
      this.labelMessage.AutoSize = true;
      this.labelMessage.Location = new System.Drawing.Point(8, 11);
      this.labelMessage.Name = "labelMessage";
      this.labelMessage.Size = new System.Drawing.Size(35, 13);
      this.labelMessage.TabIndex = 3;
      this.labelMessage.Text = "label1";
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(12, 75);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(397, 52);
      this.label1.TabIndex = 4;
      this.label1.Text = "If you choose to Browse for a folder, that folder will be scanned\r\nrecursively fo" +
          "r files during analysis. \r\n\r\nChoose Ignore to exclude this file from analysis an" +
          "d choose Abort to stop analysis...";
      // 
      // panel1
      // 
      this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.panel1.BackColor = System.Drawing.SystemColors.ControlLight;
      this.panel1.Controls.Add(this.buttonBrowse);
      this.panel1.Controls.Add(this.buttonIgnore);
      this.panel1.Controls.Add(this.buttonAbort);
      this.panel1.Location = new System.Drawing.Point(2, 142);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(438, 55);
      this.panel1.TabIndex = 5;
      // 
      // CoverageFileLocationForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.SystemColors.ButtonHighlight;
      this.ClientSize = new System.Drawing.Size(442, 197);
      this.ControlBox = false;
      this.Controls.Add(this.panel1);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.labelMessage);
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "CoverageFileLocationForm";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "File not found";
      this.TopMost = true;
      this.Load += new System.EventHandler(this.CoverageFileLocationForm_Load);
      this.panel1.ResumeLayout(false);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button buttonAbort;
    private System.Windows.Forms.Button buttonIgnore;
    private System.Windows.Forms.Button buttonBrowse;
    private System.Windows.Forms.Label labelMessage;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Panel panel1;
  }
}
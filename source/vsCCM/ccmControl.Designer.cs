namespace vsCCM
{
  partial class ccmControl
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

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ccmControl));
      this.listView = new System.Windows.Forms.ListView();
      this.Category = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.Unit = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.Complexity = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.Coverage = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.Weight = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.LOC = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.File = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.labelInfo = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // listView
      // 
      this.listView.AllowColumnReorder = true;
      this.listView.AllowDrop = true;
      this.listView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.Category,
            this.Unit,
            this.Complexity,
            this.Coverage,
            this.Weight,
            this.LOC,
            this.File});
      this.listView.Dock = System.Windows.Forms.DockStyle.Fill;
      this.listView.FullRowSelect = true;
      this.listView.GridLines = true;
      this.listView.Location = new System.Drawing.Point(0, 0);
      this.listView.Name = "listView";
      this.listView.Size = new System.Drawing.Size(842, 286);
      this.listView.TabIndex = 1;
      this.listView.UseCompatibleStateImageBehavior = false;
      this.listView.View = System.Windows.Forms.View.Details;
      this.listView.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listView_ColumnClick);
      this.listView.DragDrop += new System.Windows.Forms.DragEventHandler(this.listView_DragDrop);
      this.listView.DragEnter += new System.Windows.Forms.DragEventHandler(this.listView_DragEnter);
      this.listView.DoubleClick += new System.EventHandler(this.listView_DoubleClick);
      // 
      // Category
      // 
      this.Category.Text = "Category";
      this.Category.Width = 100;
      // 
      // Unit
      // 
      this.Unit.Text = "Unit";
      this.Unit.Width = 200;
      // 
      // Complexity
      // 
      this.Complexity.Text = "Complexity";
      this.Complexity.Width = 100;
      // 
      // Coverage
      // 
      this.Coverage.Text = "Coverage";
      // 
      // Weight
      // 
      this.Weight.Text = "Weight";
      // 
      // LOC
      // 
      this.LOC.Text = "SLOC";
      this.LOC.Width = 58;
      // 
      // File
      // 
      this.File.Text = "File";
      this.File.Width = 300;
      // 
      // labelInfo
      // 
      this.labelInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.labelInfo.AutoSize = true;
      this.labelInfo.BackColor = System.Drawing.SystemColors.Window;
      this.labelInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.labelInfo.Location = new System.Drawing.Point(86, 57);
      this.labelInfo.Name = "labelInfo";
      this.labelInfo.Size = new System.Drawing.Size(521, 80);
      this.labelInfo.TabIndex = 2;
      this.labelInfo.Text = resources.GetString("labelInfo.Text");
      // 
      // ccmControl
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.labelInfo);
      this.Controls.Add(this.listView);
      this.Name = "ccmControl";
      this.Size = new System.Drawing.Size(842, 286);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.ListView listView;
    private System.Windows.Forms.ColumnHeader Unit;
    private System.Windows.Forms.ColumnHeader Complexity;
    private System.Windows.Forms.ColumnHeader File;
    private System.Windows.Forms.ColumnHeader Category;
    private System.Windows.Forms.ColumnHeader Coverage;
    private System.Windows.Forms.ColumnHeader Weight;
    private System.Windows.Forms.ColumnHeader LOC;
    private System.Windows.Forms.Label labelInfo;

  }
}

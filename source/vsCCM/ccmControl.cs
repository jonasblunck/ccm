using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.IO;
using EnvDTE80;
using EnvDTE;
using CCMEngine;
using System.Threading;
using System.Collections;
using System.Xml;
using System.Reflection;

namespace vsCCM
{
  public partial class ccmControl : UserControl
  {
    private delegate void Function();
    private DTE2 _applicationObject;
    SortedListener listener = null;
    private object syncLock = new object();
    private Semaphore sempahore = null;
    private int numWorkingThreads = Environment.ProcessorCount + 1;
    private string[] droppedFiles = null;

    public ccmControl()
    {
      InitializeComponent();

      this.listView.ContextMenu = new ContextMenu();
      this.listView.ContextMenu.MenuItems.Add(
         new MenuItem("Analyze Current...", new MenuItem[] 
            {
              new MenuItem("Solution", new EventHandler(this.analyzeSolution_Click)),
              new MenuItem("Project", new EventHandler(this.analyzeProject_Click)),
              new MenuItem("File", new EventHandler(this.analyzeFile_Click)),
            })
            { Visible = false, Name = "Analyze" }
          )
        ;

      this.listView.ContextMenu.MenuItems.Add(
        new MenuItem("Show...", new MenuItem[] 
            {
              new MenuItem("10 metrics", new EventHandler(SelectNumMetricsToShow_Click)),
              new MenuItem("30 metrics", new EventHandler(SelectNumMetricsToShow_Click)) { Checked = true } ,
              new MenuItem("50 metrics", new EventHandler(SelectNumMetricsToShow_Click)),
              new MenuItem("100 metrics", new EventHandler(SelectNumMetricsToShow_Click)),
              new MenuItem("200 metrics", new EventHandler(SelectNumMetricsToShow_Click)),
            }) { Name = "Show", }
           );

      this.listView.ContextMenu.MenuItems.Add("Copy to Clipboard", new EventHandler(this.OnCopyToClipboard));
      this.listView.ContextMenu.MenuItems.Add("About", this.About_Click);

#if !CCM_INCLUDE_COVERAGE_FEATURE
//      this.toolLoadCoverage.Visible = false; // replace
      this.listView.Columns[3].Width = 0;
      this.listView.Columns[4].Width = 0;
#endif
      this.labelInfo.ContextMenu = this.listView.ContextMenu;
    }

    void About_Click(object sender, EventArgs e)
    {
      AboutForm about = new AboutForm();
      about.ShowDialog();
    }

    void OnCopyToClipboard(object sender, EventArgs e)
    {
      if (this.listView.Items.Count > 0)
      {
        StringBuilder resultText = new StringBuilder();
        resultText.Append("Method,Cyclomatic complexity,sloc,File\r\n");

        foreach (ListViewItem item in this.listView.Items)
        {
                  // unit, ccm, coverage, sloc, file
          resultText.Append(item.SubItems[1].Text + ",");
          resultText.Append(item.SubItems[2].Text + ",");
          resultText.Append(item.SubItems[5].Text + ",");
          resultText.Append(item.SubItems[6].Text + "@line" + item.Tag + "\r\n");
        }

        Clipboard.SetText(resultText.ToString());
      }

    }

    void SelectNumMetricsToShow_Click(object sender, EventArgs e)
    {
      MenuItem selectedItem = sender as MenuItem;

      if (null != selectedItem)
      {
        foreach (MenuItem i in selectedItem.Parent.MenuItems)
        {
          if (i.Text == selectedItem.Text)
            i.Checked = true;
          else
            i.Checked = false;
        }
      }
    }

    public void Initialize(DTE2 applicationObject)
    {
      this._applicationObject = applicationObject;
      this.listView.ContextMenu.MenuItems["Analyze"].Visible = true;
    }

    private void AnalyzeFileInBackgroundThread(object filename)
    {
      try
      {
        string file = (string)filename;

        using (System.IO.StreamReader reader = new System.IO.StreamReader(file))
        {
          FileAnalyzer analyzer = new FileAnalyzer(reader, this.listener, null, true, file);
          analyzer.Analyze();
        }
      }
      catch (Exception)
      {
      }
      finally
      {
        this.sempahore.Release();
      }
    }

    private void AnalyzeFile(string file)
    {
      this.sempahore.WaitOne();

      ThreadPool.QueueUserWorkItem(new WaitCallback(this.AnalyzeFileInBackgroundThread), file);
    }

    private bool IsCandidateForComplexityAnalysis(ProjectItem item, ConfigurationFile config)
    {
      return IsCandidateForComplexityAnalysis(item.Name, config);
    }

    private void AnalyzeCurrentFile()
    {
      string fullPath = String.Empty;

      if (null != this._applicationObject)
        fullPath = string.Format("{0}\\{1}", this._applicationObject.ActiveDocument.Path, this._applicationObject.ActiveDocument.Name);
      else
      {
        // select file
      }

      if (IsCandidateForComplexityAnalysis(fullPath, ccmControl.GetConfigurationFile()))
        AnalyzeFile(fullPath);
    }

    private void UpdateUI(List<ccMetric> metrics)
    {
      foreach (ccMetric metric in metrics)
      {
        ListViewItem lvi = new ListViewItem(ccMetric.GetClassification(metric.CCM));
        lvi.SubItems.Add(metric.Unit);
        lvi.SubItems.Add(metric.CCM.ToString());

        if (metric.Custom is float)
        {
          lvi.SubItems.Add(((float)metric.Custom).ToString("00.00"));
          lvi.SubItems.Add(((float)metric.Custom).ToString("00.00"));
        }
        else
        {
          lvi.SubItems.Add(""); // we have no coverage data..
          lvi.SubItems.Add(""); // we have no coverage data..
        }

        lvi.SubItems.Add((metric.EndLineNumber - metric.StartLineNumber).ToString()); // this should be LOC
        lvi.SubItems.Add(metric.Filename);
        lvi.Tag = metric.StartLineNumber;

        this.listView.Items.Add(lvi);
      }

      if (this.listView.Items.Count > 0)
        this.labelInfo.Visible = false;
      else
        this.labelInfo.Visible = true;
    }

    private void AnalyzeSolution()
    {
      ConfigurationFile configFile = ccmControl.GetConfigurationFile();

      foreach (string file in GetAllSolutionFiles())
      {
        if (IsCandidateForComplexityAnalysis(file, configFile))
          AnalyzeFile(file);
      }
    }

    private void AnalyzeCurrentProject()
    {
      ConfigurationFile configFile = ccmControl.GetConfigurationFile();
      Array projs = (Array)this._applicationObject.DTE.ActiveSolutionProjects;

      List<string> files = new List<string>();
      foreach (object p in projs)
      {
        Project project = (Project)p;

        foreach (ProjectItem item in project.ProjectItems)
          GetAllProjectFiles(item, files);
      }

      foreach (string file in files)
      {
        if (IsCandidateForComplexityAnalysis(file, configFile))
          AnalyzeFile(file);
      }
    }

    private int GetNumMetrics()
    {
      Menu showMenu = this.listView.ContextMenu.MenuItems["Show"];

      if (null != showMenu)
      {
        foreach (MenuItem item in showMenu.MenuItems)
        {
          if (item.Checked)
          {
            int numMetrics = int.Parse(item.Text.Split(' ')[0]);
            return numMetrics;
          }
        }
      }

      return 1;
    }

    private void RunFunctionAndUpdateUI(Function function)
    {
      try
      {
        lock (this.syncLock)
        {
          this.listener = new SortedListener(GetNumMetrics(), new List<string>(), 0);

          //
          // setup semaphore for multi processing
          //
          this.sempahore = new Semaphore(this.numWorkingThreads, this.numWorkingThreads);

          // 
          // clear old items 
          //
          this.listView.Items.Clear();
          this.listener.Clear();

          function();

          //  
          // wait for completion of possible background workers
          for (int i = 0; i < this.numWorkingThreads; ++i)
            this.sempahore.WaitOne();

          UpdateUI(this.listener.Metrics);

          //
          // cleanup
          this.sempahore.Close();
          this.sempahore = null;
        }
      }
      catch (Exception)
      {
      }
    }


    public void analyzeSolution_Click(object sender, EventArgs e)
    {
      RunFunctionAndUpdateUI(new Function(this.AnalyzeSolution));
    }

    public void analyzeFile_Click(object sender, EventArgs e)
    {
      RunFunctionAndUpdateUI(new Function(this.AnalyzeCurrentFile));
    }

    public void analyzeProject_Click(object sender, EventArgs e)
    {
      RunFunctionAndUpdateUI(new Function(this.AnalyzeCurrentProject));
    }

    private void listView_DoubleClick(object sender, EventArgs e)
    {
      if (this.listView.SelectedItems.Count > 0 && null != this._applicationObject)
      {
        ListViewItem item = this.listView.SelectedItems[0];
        string file = item.SubItems[6].Text;

        int lineNumber = (int)item.Tag;
        Window win =
          this._applicationObject.DTE.OpenFile(EnvDTE.Constants.vsViewKindCode, file);

        win.SetFocus();

        DTE dte = this._applicationObject.DTE;
        TextSelection selection = (TextSelection)dte.ActiveDocument.Selection;
        selection.GotoLine(lineNumber, true);
      }
    }

    private void listView_ColumnClick(object sender, ColumnClickEventArgs e)
    {
      var sorter = new ListViewSorter(e.Column);
      this.listView.ListViewItemSorter = sorter;

      this.listView.Sort();
    }

    private void AnalyzeFolder(string folder, ConfigurationFile configFile)
    {
      string[] files = System.IO.Directory.GetFiles(folder);

      foreach (string path in files)
      {
        if (IsCandidateForComplexityAnalysis(path, configFile))
          AnalyzeFile(path);
      }

      string[] folders = System.IO.Directory.GetDirectories(folder);

      foreach (string f in folders)
        AnalyzeFolder(f, configFile);
    }

    private static ConfigurationFile GetConfigurationFile()
    {
      ConfigurationFile config = null;
      FileInfo configFileInfo = new FileInfo(
        Path.Combine(
          new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName,
            "ccm.config"));

      if (configFileInfo.Exists)
      {
        XmlDocument configDoc = new XmlDocument();
        configDoc.Load(configFileInfo.FullName);

        config = new ConfigurationFile(configDoc);
      }

      return config;
    }

    private void AnalyzeDroppedFiles()
    {
      ConfigurationFile config = ccmControl.GetConfigurationFile();

      foreach (string path in this.droppedFiles)
      {
        if (IsCandidateForComplexityAnalysis(path, config))
          AnalyzeFile(path);
        else if (System.IO.Directory.Exists(path))
          AnalyzeFolder(path, config);
      }
    }

    private void listView_DragDrop(object sender, DragEventArgs e)
    {
      if (e.Data.GetDataPresent(DataFormats.FileDrop))
      {
        this.droppedFiles = (string[])e.Data.GetData(DataFormats.FileDrop, false);

        RunFunctionAndUpdateUI(new Function(this.AnalyzeDroppedFiles));
      }

    }

    private void listView_DragEnter(object sender, DragEventArgs e)
    {
      e.Effect = DragDropEffects.Link;
    }


  }
}

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
using System.Reflection;
using System.Xml;

namespace vsCCM
{
  public partial class ccmControl : UserControl
  {
    private static bool IsCandidateForComplexityAnalysis(string fileName, ConfigurationFile config)
    {
      FileInfo file = new FileInfo(fileName);

      if (!file.Exists || string.IsNullOrEmpty(file.Extension))
        return false;

      string lower = fileName.ToLower();
      string extension = lower.Substring(lower.LastIndexOf('.'));

      if (null != config)
        return config.SupportedExtensions.Contains(extension);
     
      if (CCMEngine.ConfigurationFile.GetDefaultSupportedFileExtensions().Contains(extension))
        return true;

      return false;
    }

    private List<string> GetAllSolutionFiles()
    {
      List<string> files = new List<string>();

      foreach (Project project in this._applicationObject.Solution.Projects)
      {
        foreach (ProjectItem item in project.ProjectItems)
          GetAllProjectFiles(item, files);
      }

      return files;
    }

    private void GetAllProjectFiles(ProjectItem projItem, List<string> files)
    {
      if (null != projItem.ProjectItems)
      {
        foreach (ProjectItem subItem in projItem.ProjectItems)
          GetAllProjectFiles(subItem, files);
      }

      if (null != projItem.SubProject && null != projItem.SubProject.ProjectItems)
      {
        foreach (ProjectItem subItem in projItem.SubProject.ProjectItems)
          GetAllProjectFiles(subItem, files);
      }

      if (projItem.Properties != null && projItem.Properties.Item("FullPath") != null)
      {
        files.Add(projItem.Properties.Item("FullPath").Value.ToString());
      }
    }
  }

}

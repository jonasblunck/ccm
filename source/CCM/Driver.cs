using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CCMEngine;
using System.Threading;
using System.Xml;

namespace CCM
{
  internal struct AnalyzeThreadParameters
  {
    public StreamReader stream;
    public string filename;
  }

  public class Driver 
  {
    private List<ErrorInfo> errors = new List<ErrorInfo>();
    private ConfigurationFile configFile = null;
    private object mutex = new object();
    private Semaphore threadSemaphore;
    private int threadCount;
    private SortedListener listener = null;

    public Driver()
    {
      // added for testability purposes, should not be instansiated outside of test framework
      this.configFile = new ConfigurationFile(new XmlDocument());
      Init();
    }

    public Driver(ConfigurationFile configFile) 
    {
      this.configFile = configFile;
      Init();
    }

    private void Init()
    {
      this.threadCount = Environment.ProcessorCount;
      this.threadSemaphore = new Semaphore(this.threadCount, this.threadCount);
      this.listener = new SortedListener(this.configFile.NumMetrics, this.configFile.ExcludeFunctions, this.configFile.Threshold);
    }

    private void AnalyzeFilestream(object context)
    {
      AnalyzeThreadParameters parameters = (AnalyzeThreadParameters)context;

      try
      {
        FileAnalyzer analyzer =
          new FileAnalyzer(parameters.stream, this.listener, null, this.configFile.SuppressMethodSignatures, 
            parameters.filename, this.configFile.SwitchStatementBehavior);

        analyzer.Analyze();
        parameters.stream.Close(); // free up the stream.
      }
      catch (UnknownStructureException error)
      {
        lock (this.mutex)
        {
          this.errors.Add(new ErrorInfo(parameters.filename, error.Message));
        }
      }
      catch (PreprocessorException)
      {
        lock (this.mutex)
        {
          this.errors.Add(new ErrorInfo(parameters.filename, "Error running pre-processor. Only basic support for #ifdefs."));
        }
      }
      catch (Exception)
      {
        lock (this.mutex)
        {
          this.errors.Add(new ErrorInfo(parameters.filename, "Unknown error parsing file."));
        }
      }

      this.threadSemaphore.Release();
    }

    public bool IsValidFile(string filename)
    {
      if (this.configFile != null)
      {
        foreach (string name in this.configFile.ExcludeFiles)
        {
          if (filename.ToLower().Contains(name.ToLower()))
            return false;
        }
      }

      // check if file should be excluded
      int extDelim = filename.LastIndexOf('.');

      if (-1 != extDelim)
      {
        string extension = filename.Substring(extDelim);

        if (this.configFile.SupportedExtensions.Contains(extension.ToLower()))
          return true;
      }

      return false;
    }

    public bool PathShouldBeExcluded(string path)
    {
      if (this.configFile != null)
      {
        string[] pathFolderNames = path
          .Split(new char[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);

        foreach (string pathToExclude in this.configFile.ExcludeFolders)
        {
          if (path.ToLower().StartsWith(pathToExclude.ToLower()))
            return true;

          if (pathFolderNames.Contains(pathToExclude, StringComparer.InvariantCultureIgnoreCase))
            return true;
        }
      }

      return false;
    }

    public void StartAnalyze(StreamReader fileStream, string fileName)
    {
      AnalyzeThreadParameters parameters = new AnalyzeThreadParameters();
      parameters.filename = fileName;
      parameters.stream = fileStream;

      // wait until a thread is available and then queue the work 
      this.threadSemaphore.WaitOne();

      ThreadPool.QueueUserWorkItem(new WaitCallback(AnalyzeFilestream), parameters);
    }

    private void HandleDirectory(string basePath, string path)
    {
      if (Directory.Exists(path) && !PathShouldBeExcluded(path))
      {
        string[] files = Directory.GetFiles(path);

        foreach (string fileName in files)
        {
          if (IsValidFile(fileName))
          {
            StartAnalyze(
                new StreamReader(fileName),
                fileName);
          }
        }
      }

      if (this.configFile.RecursiveAnalyze)
      {
        string[] directories = Directory.GetDirectories(path);

        foreach (string directory in directories)
          HandleDirectory(basePath, directory);
      }
    }

    public List<ccMetric> Metrics
    {
      get
      {
        return this.listener.Metrics;
      }
    }

    public List<ErrorInfo> Errors
    {
      get
      {
        return this.errors;
      }
    }

    public void Drive()
    {
      foreach (string dir in this.configFile.AnalyzeFolders)
        HandleDirectory(dir, dir);

      WaitForWorkThreadsToFinish();
    }

    public void WaitForWorkThreadsToFinish()
    {
      // wait for all threads to exit
      for (int i = 0; i < this.threadCount; ++i)
        this.threadSemaphore.WaitOne();
    }
  }

}
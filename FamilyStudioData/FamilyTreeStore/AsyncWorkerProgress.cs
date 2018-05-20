﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using FamilyStudioData.FamilyTreeStore;

namespace FamilyStudioData.FamilyTreeStore
{
  public delegate void WorkProgressHandler(int progressPercent, string text = null);

  public interface AsyncWorkerProgressInterface : IDisposable
  {
    void DoWork(object sender, DoWorkEventArgs e);
    void ProgressChanged(object sender, ProgressChangedEventArgs e);
    void Completed(object sender, RunWorkerCompletedEventArgs e);
  }
  public interface AsyncWorkerThreadInterface : IDisposable
  {
    void DoWork(object sender, DoWorkEventArgs e);
    //void Completed(object sender, RunWorkerCompletedEventArgs e);
  }

  public interface ProgressReporterInterface
  {
    void ReportProgress(double progressPercent, string progressText = null);
    void Completed(string completedText = null);
  }
  public class AsyncWorkerProgress : ProgressReporterInterface
  {
    private DateTime startTime;
    private double currentProgress;
    private string currentProgressText;
    private TraceSource trace;

    private WorkProgressHandler progressHandlerFcn;

    public AsyncWorkerProgress(WorkProgressHandler progressHandler)
    {
      trace = new TraceSource("FamilyFormProgress", SourceLevels.Warning);
      progressHandlerFcn = progressHandler;
      startTime = DateTime.Now;
      currentProgress = 0.0;
      currentProgressText = "";
    }

    public void ReportProgress(double progressPercent, string progressText = null)
    {
      TimeSpan deltaTime;
      DateTime estimatedEndTime;
      string endTimeString = "";

      if (progressText != null)
      {
        currentProgressText = progressText;
      }
      if (progressPercent < currentProgress)
      {
        trace.TraceInformation("FamilyFormProgress::ReportProgress(" + progressPercent + " < " + currentProgress + ") =>" + DateTime.Now + " restart!");
        startTime = DateTime.Now;
      }
      deltaTime = DateTime.Now - startTime;
      currentProgress = progressPercent;
      if ((progressPercent > 0.02) && (startTime != DateTime.Now))
      {
        estimatedEndTime = DateTime.Now.AddSeconds((100.0 - progressPercent) * deltaTime.TotalSeconds / progressPercent);
        trace.TraceInformation("FamilyFormProgress::ReportProgress(" + progressPercent + ")" + DateTime.Now + ", elapsed:" + deltaTime.TotalSeconds + ",estimated time in seconds:" + deltaTime.TotalSeconds * 100.0 / progressPercent + ",end:" + estimatedEndTime);
        endTimeString = " Estimated done at " + estimatedEndTime;
      }
      if (progressHandlerFcn != null)
      {
        progressHandlerFcn((int)progressPercent, currentProgressText + endTimeString);
      }
    }

    public void Completed(string completedText = null)
    {
      string text = "";

      if (completedText != null)
      {
        text = completedText;
      }
      trace.TraceInformation("FamilyFormProgress::Completed(" + text + ")" + DateTime.Now);

      if (progressHandlerFcn != null)
      {
        progressHandlerFcn(-1, completedText);
      }
    }

    public override string ToString()
    {
      TimeSpan delta = DateTime.Now.Subtract(startTime);
      return delta.ToString(@"hh\:mm\:ss") + " " + currentProgress.ToString("F2") + "%";
    }
  }

}

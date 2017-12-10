using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using FamilyStudioData.FamilyTreeStore;

namespace FamilyStudioFormsGui.WindowsGui.Forms
{
  

  public delegate void WorkProgressHandler(int progressPercent, string text = null);

  interface AsyncWorkerProgress : IDisposable
  {
    void DoWork(object sender, DoWorkEventArgs e);
    void ProgressChanged(object sender, ProgressChangedEventArgs e);
    void Completed(object sender, RunWorkerCompletedEventArgs e);
  }
  interface AsyncWorkerThread : IDisposable
  {
    void DoWork(object sender, DoWorkEventArgs e);
    //void Completed(object sender, RunWorkerCompletedEventArgs e);
  }

  /*public interface ProgressReporter
  {
    void ReportProgress(int progressPercent, string progressText = null);
    void Completed(string completedText = null);
  }*/
}

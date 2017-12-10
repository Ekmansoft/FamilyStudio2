﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;

namespace FamilyStudioData.FamilyTreeStore
{
  public interface ProgressReporter
  {
    void ReportProgress(double progressPercent, string progressText = null);
    void Completed(string completedText = null);
  }
  public class FileProgressReporter
  {
    //int counter, maxValue, updateValue, subCounter;
    int lastUpdateValue, currentValue, maxValue, updateValue;

    public FileProgressReporter(int maxValueIn)
    {
      maxValue = maxValueIn;
      lastUpdateValue = 0;
      currentValue = 0;
      updateValue = maxValueIn / 500;
      if (updateValue < 1)
      {
        updateValue = 1;
      }
      //subCounter = 0;
    }

    public bool Update(int current)
    {
      //counter++;
      //subCounter++;

      currentValue = current;
      if ((currentValue - lastUpdateValue) >= updateValue)
      {
        lastUpdateValue = currentValue;
        return true;
      }
      return false;
    }
    public double GetPercent()
    {
      return 100.0 * currentValue / maxValue;
    }
  }
}

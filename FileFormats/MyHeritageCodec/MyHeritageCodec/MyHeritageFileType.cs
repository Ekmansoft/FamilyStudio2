using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.ComponentModel;
using FamilyStudioData.FamilyData;
//using FamilyStudioData.FamilyFileFormat;
using FamilyStudioData.FamilyTreeStore;

namespace FamilyStudioData.FileFormats.MyHeritageCodec
{
  public class MyHeritageFileType : FamilyFileTypeBaseClass
  {
    private static TraceSource trace = new TraceSource("MyHeritageFileType", SourceLevels.Warning);
    private bool printFlag;

    public MyHeritageFileType()
    {
      printFlag = false;
    }

    public override bool IsKnownFileType(String fileName)
    {

      if (fileName.ToLower().Contains("myheritage.com"))
      {
        return true;
      }
      return false;
    }

    public override FamilyTreeStoreBaseClass CreateFamilyTreeStore(String fileName, CompletedCallback callback)
    {
      FamilyTreeStoreMyHeritage webStore = new FamilyTreeStoreMyHeritage();

      if (printFlag)
      {
        trace.TraceInformation("MyHeritageFileType::CreateFamilyTreeStore( " + fileName + ")");
      }

      webStore.SetFile(fileName);
      callback(true);
      return (FamilyTreeStoreBaseClass)webStore;
    }

    public override bool OpenFile(String fileName, ref FamilyTreeStoreBaseClass inFamilyTree, CompletedCallback callback)
    {
      FamilyTreeStoreMyHeritage myHeritageStore = (FamilyTreeStoreMyHeritage)inFamilyTree;

      if (printFlag)
      {
        trace.TraceInformation("MyHeritageFileType::OpenFile( " + fileName + ")");
      }
      myHeritageStore.SetFile(fileName);
      callback(true);
      return true;
    }
    public override bool SetProgressTarget(BackgroundWorker inBackgroundWorker)
    {
      if (printFlag)
      {
        trace.TraceInformation("MyHeritageFileType::SetProgressTarget ");
      }
      //backgroundWorker = inBackgroundWorker;
      return false;
    }
    public override string GetFileTypeWebName()
    {
      return "MyHeritage.com";
    }

  }
}

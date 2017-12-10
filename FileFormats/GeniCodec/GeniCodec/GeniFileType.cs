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

namespace FamilyStudioData.FileFormats.GeniCodec
{
  public class GeniFileType : FamilyFileTypeBaseClass
  {
    private static TraceSource trace = new TraceSource("FamilyTreeStoreGeni2", SourceLevels.Warning);
    //private bool printFlag;

    public GeniFileType()
    {
    }

    public override bool IsKnownFileType(String fileName)
    {

      if (fileName.ToLower().Contains("geni.com"))
      {
        return true;
      }
      return false;
    }

    public override FamilyTreeStoreBaseClass CreateFamilyTreeStore(String fileName, CompletedCallback callback)
    {
      FamilyTreeStoreGeni2 GeniStore = new FamilyTreeStoreGeni2(callback);

      trace.TraceInformation("GeniFileType::CreateFamilyTreeStore( " + fileName + ")");

      GeniStore.SetFile(fileName);
      return (FamilyTreeStoreBaseClass)GeniStore;
    }

    public override bool OpenFile(String fileName, ref FamilyTreeStoreBaseClass inFamilyTree, CompletedCallback callback)
    {
      FamilyTreeStoreGeni2 GeniStore = (FamilyTreeStoreGeni2)inFamilyTree;

      trace.TraceInformation("GeniFileType::OpenFile( " + fileName + ")");
      GeniStore.SetFile(fileName);
      if (!GeniStore.CallbackArmed())
      {
        callback(true);
      }
      return true;
    }
    public override bool SetProgressTarget(BackgroundWorker inBackgroundWorker)
    {
      trace.TraceInformation("GeniFileType::SetProgressTarget ");
      //backgroundWorker = inBackgroundWorker;
      return false;
    }
    public override string GetFileTypeWebName()
    {
      return "Geni.com";
    }
    

  }
}

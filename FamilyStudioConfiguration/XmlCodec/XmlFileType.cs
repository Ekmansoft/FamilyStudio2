using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.ComponentModel;
using FamilyStudioData.FamilyData;
//using FamilyStudioData.FamilyFileFormat;
using FamilyStudioData.FamilyTreeStore;

namespace FamilyStudioData.FileFormats.XmlCodec
{
  public class XmlFileType : FamilyFileTypeBaseClass
  {
    private TraceSource trace;

    public XmlFileType()
    {
      trace = new TraceSource("XmlFileType", SourceLevels.Warning);
    }
    public override bool IsKnownFileType(String fileName)
    {

      if (fileName.ToLower().Contains(".xml"))
      {
        return true;
      }
      return false;
    }

    public override FamilyTreeStoreBaseClass CreateFamilyTreeStore(String fileName, CompletedCallback callback)
    {
      FamilyTreeStoreRam ramStore = new FamilyTreeStoreRam();

      trace.TraceInformation("XmlFileType::CreateFamilyTreeStore( " + fileName + ")" + DateTime.Now);
      ramStore.SetSourceFileName(fileName);
      //FileStream fileStream = new FileStream(fileName, FileMode.CreateNew);

      //ramStore.SetFile(fileName);
      callback(true);
      return (FamilyTreeStoreBaseClass)ramStore;
    }

    public override bool OpenFile(String fileName, ref FamilyTreeStoreBaseClass inFamilyTree, CompletedCallback callback)
    {
      //FamilyTreeStoreRam ramStore = (FamilyTreeStoreRam)inFamilyTree;

      trace.TraceInformation("XmlFileType::OpenFile( " + fileName + ") start " + DateTime.Now);

      FileStream fileStream = new FileStream(fileName, FileMode.Open);
      if (fileStream != null)
      {
        //DataContractSerializer serializer = new DataContractSerializer(typeof(FamilyTreeStoreRam));
        DataContractSerializer serializer = new DataContractSerializer(inFamilyTree.GetType());
        try
        {
          inFamilyTree = (FamilyTreeStoreBaseClass)serializer.ReadObject(fileStream);
        }
        catch (SerializationException e)
        {
          trace.TraceInformation("Exceptions:" + e.ToString());
          inFamilyTree = null;
        }
        catch (ArgumentNullException e)
        {
          trace.TraceInformation("Exceptions:" + e.ToString());
          inFamilyTree = null;
        }

        fileStream.Close();
      }

      trace.TraceInformation("XmlFileType::OpenFile( " + fileName + ") done " + DateTime.Now);
      //anarkivStore.SetFile(fileName);
      callback(true);
      return true;
    }
    public override bool SetProgressTarget(BackgroundWorker inBackgroundWorker)
    {
      trace.TraceInformation("SetProgressTarget 2");
      //backgroundWorker = inBackgroundWorker;
      return false;
    }
    public override string GetFileTypeFilter(FamilyFileTypeOperation operation)
    {
      if (operation == FamilyFileTypeOperation.Open)
      {
        return "GeneaLite|*.xml";
      }
      return null;
    }

  }
}

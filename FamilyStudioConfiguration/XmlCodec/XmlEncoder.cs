using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.IO;
using FamilyStudioData.FamilyData;
//using FamilyStudioData.FamilyFileFormat;
using FamilyStudioData.FamilyTreeStore;
//using FamilyStudioData.FileFormats.GeniCodec;

namespace FamilyStudioData.FileFormats.XmlCodec
{
  public class XmlEncoder : FamilyTreeStore.FamilyFileEncoder
  {
    private ProgressReporterInterface workerProgressTarget;
    private TraceSource trace;

    public XmlEncoder()
    {
      trace = new TraceSource("XmlEncoder", SourceLevels.Warning);
    }


    public void SetProgressTarget(ProgressReporterInterface progressTarget)
    {
      workerProgressTarget = progressTarget;
    }


    public void StoreFile(FamilyTreeStoreBaseClass familyTree, string filename, FamilyFileTypeOperation operation, int variant = 0)
    {
      trace.TraceInformation("XmlEncoder::StoreFile() start " + DateTime.Now);

      FileStream fileStream = new FileStream(filename, FileMode.Create);
      DataContractSerializer serializer = new DataContractSerializer(familyTree.GetType());

      serializer.WriteObject(fileStream, familyTree);

      fileStream.Close();

      trace.TraceInformation("XmlEncoder::StoreFile() done " + DateTime.Now);
    }
    public string GetFileTypeFilter(FamilyFileTypeOperation operation, int variant = 0)
    {
      if (operation == FamilyFileTypeOperation.Save)
      {
        return "GeneaLite|*.xml";
      }
      return null;
    }
    public bool IsKnownFileType(string filename)
    {
      if (filename.ToLower().IndexOf(".xml") >= 0)
      {
        return true;
      }
      return false;
    }
    public IDictionary<int, string> GetOperationVariantList(FamilyFileTypeOperation operation)
    {
      return null;
    }
  }


}

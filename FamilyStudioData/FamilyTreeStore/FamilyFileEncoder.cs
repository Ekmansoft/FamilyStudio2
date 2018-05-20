using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
//using FamilyStudioData.FamilyData;
//using FamilyStudioData.FamilyTreeStore;


namespace FamilyStudioData.FamilyTreeStore
{
  public interface FamilyFileEncoder
  {
    void StoreFile(FamilyTreeStoreBaseClass familyTree, string filename, FamilyFileTypeOperation operation, int variant = 0);

    void SetProgressTarget(ProgressReporterInterface progressTarget);

    string GetFileTypeFilter(FamilyFileTypeOperation operation, int variant = 0);

    bool IsKnownFileType(string filename);

    IDictionary<int, string> GetOperationVariantList(FamilyFileTypeOperation operation);
  }
}

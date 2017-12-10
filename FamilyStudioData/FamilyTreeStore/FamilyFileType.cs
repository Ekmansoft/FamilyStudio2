using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using System.ComponentModel;
using FamilyStudioData.FamilyData;
//using FamilyStudioData.FamilyTreeStore;

namespace FamilyStudioData.FamilyTreeStore
{
  public enum FamilyFileTypeOperation
  {
    Open,
    Save,
    Import,
    Export
  };

  public delegate void CompletedCallback(Boolean result);
  public interface FamilyFileType
  {
    bool IsKnownFileType(String fileName);

    FamilyTreeStoreBaseClass CreateFamilyTreeStore(String fileName, CompletedCallback callback);

    bool OpenFile(String fileName, ref FamilyTreeStoreBaseClass inFamilyTree, CompletedCallback callback);

    bool SetProgressTarget(BackgroundWorker inBackgroundWorker);

    string GetFileTypeFilter(FamilyFileTypeOperation operation);
  }

  public abstract class FamilyFileTypeBaseClass : FamilyFileType
  {
    public abstract bool IsKnownFileType(String fileName);

    public abstract FamilyTreeStoreBaseClass CreateFamilyTreeStore(String fileName, CompletedCallback callback);

    public abstract bool OpenFile(String fileName, ref FamilyTreeStoreBaseClass inFamilyTree, CompletedCallback callback);

    public abstract bool SetProgressTarget(BackgroundWorker inBackgroundWorker);

    public virtual string GetFileTypeFilter(FamilyFileTypeOperation operation)
    {
      return null;
    }
    public virtual string GetFileTypeWebName()
    {
      return null;
    }
    public virtual bool GetFileTypeCreatesStorage()
    {
      return false;
    }
  }
}

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

namespace FamilyStudioData.FileFormats.AnarkivCodec
{
  public class AnarkivFileType : FamilyFileTypeBaseClass
  {
    private TraceSource trace;

    class XrefMapperClass
    {
      //public string oldXref;
      public string newXref;
      public int noOfReferences;
      public int noOfDefinitions;

      public XrefMapperClass(string newXref, bool defined = false)
      {
        this.newXref = newXref;
        this.noOfReferences = 0;
        this.noOfDefinitions = 0;
      }
      /*public XrefMapperClass()
      {
        this.newXref = null;
        noOfReferences = 1;
      }*/

      public void CheckAndSetDefined(bool defined)
      {
        if (defined)
        {
          noOfDefinitions++;
        }
        else
        {
          noOfReferences++;
        }
      }
    }

    class AnarkivMappers
    {
      private FamilyTreeStoreBaseClass familyTree;
      private IDictionary<string, XrefMapperClass> individualXrefMapper;
      private IDictionary<string, XrefMapperClass> familyXrefMapper;
      private IDictionary<string, XrefMapperClass> multimediaXrefMapper;
      private IDictionary<string, XrefMapperClass> noteXrefMapper;
      private IDictionary<string, XrefMapperClass> repositoryXrefMapper;
      private IDictionary<string, XrefMapperClass> sourceXrefMapper;
      private IDictionary<string, XrefMapperClass> submissionXrefMapper;
      private IDictionary<string, XrefMapperClass> submitterXrefMapper;

      public AnarkivMappers(FamilyTreeStoreBaseClass familyTree)
      {
        this.familyTree = familyTree;
        individualXrefMapper = new Dictionary<string, XrefMapperClass>();
        familyXrefMapper = new Dictionary<string, XrefMapperClass>();
        multimediaXrefMapper = new Dictionary<string, XrefMapperClass>();
        noteXrefMapper = new Dictionary<string, XrefMapperClass>();
        repositoryXrefMapper = new Dictionary<string, XrefMapperClass>();
        sourceXrefMapper = new Dictionary<string, XrefMapperClass>();
        submissionXrefMapper = new Dictionary<string, XrefMapperClass>();
        submitterXrefMapper = new Dictionary<string, XrefMapperClass>();
      }


      private string GetXRef(ref IDictionary<string, XrefMapperClass> mapper, XrefType type, string fileXref, bool defined)
      {
        if (mapper.ContainsKey(fileXref))
        {
          mapper[fileXref].CheckAndSetDefined(defined);
          return mapper[fileXref].newXref;
        }
        //resultStr = "I" + Guid.NewGuid().ToString();
        string newXref = familyTree.CreateNewXref(type);
        mapper.Add(fileXref, new XrefMapperClass(newXref, defined));
        mapper[fileXref].CheckAndSetDefined(defined);
        return newXref;
      }

      public IDictionary<string, XrefMapperClass> GetMapper(XrefType type)
      {
        switch (type)
        {
          case XrefType.Individual:
            return individualXrefMapper;

          case XrefType.Family:
            return familyXrefMapper;

          case XrefType.Multimedia:
            return multimediaXrefMapper;

          case XrefType.Note:
            return noteXrefMapper;

          case XrefType.Source:
            return sourceXrefMapper;

          case XrefType.Repository:
            return repositoryXrefMapper;

          case XrefType.Submission:
            return submissionXrefMapper;

          case XrefType.Submitter:
            return submitterXrefMapper;

          default:
            //DebugStringAdd("Unknown xref tag type:" + type);
            return null;
        }
      }


      public string GetLocalXRef(XrefType type, string fileXref, bool defined = false)
      {
        switch (type)
        {
          case XrefType.Individual:
            return GetXRef(ref individualXrefMapper, type, fileXref, defined);

          case XrefType.Family:
            return GetXRef(ref familyXrefMapper, type, fileXref, defined);

          case XrefType.Multimedia:
            return GetXRef(ref multimediaXrefMapper, type, fileXref, defined);

          case XrefType.Note:
            return GetXRef(ref noteXrefMapper, type, fileXref, defined);

          case XrefType.Source:
            return GetXRef(ref sourceXrefMapper, type, fileXref, defined);

          case XrefType.Repository:
            return GetXRef(ref repositoryXrefMapper, type, fileXref, defined);

          case XrefType.Submission:
            return GetXRef(ref submissionXrefMapper, type, fileXref, defined);

          case XrefType.Submitter:
            return GetXRef(ref submitterXrefMapper, type, fileXref, defined);

          default:
            //DebugStringAdd("Unknown xref tag type:" + type);
            return "";
        }
      }

    }

    public AnarkivFileType()
    {
      trace = new TraceSource("AnarkivFileType", SourceLevels.Warning);
    }
    public override bool IsKnownFileType(String fileName)
    {

      if (fileName.ToLower().Contains(".ddb"))
      {
        return true;
      }
      return false;
    }

    public override FamilyTreeStoreBaseClass CreateFamilyTreeStore(String fileName, CompletedCallback callback)
    {
      FamilyTreeStoreAnarkiv anarkivStore = new FamilyTreeStoreAnarkiv();

      trace.TraceInformation("AnarkivFileType::CreateFamilyTreeStore( " + fileName + ")");

      anarkivStore.SetFile(fileName);
      callback(true);
      return (FamilyTreeStoreBaseClass)anarkivStore;
    }

    private void ReadFile(ref FamilyTreeStoreBaseClass inFamilyTree, FamilyTreeStoreAnarkiv anarkivStore)
    {
      //IndividualClass person = anarkivStore.GetIndividual();

      IEnumerator<IndividualClass> people = anarkivStore.SearchPerson();

      AnarkivMappers mappers = new AnarkivMappers(inFamilyTree);
      IDictionary<string, XrefMapperClass> individualMapper = mappers.GetMapper(XrefType.Individual);
      IDictionary<string, XrefMapperClass> familyMapper = mappers.GetMapper(XrefType.Family);

      int counter = 0;
      while (people.MoveNext())
      {
        IndividualClass person = people.Current;

        trace.TraceInformation("Person[" + counter++ + "]:" + person.GetPersonalName().ToString());

        IndividualXrefClass xref = new IndividualXrefClass(inFamilyTree.CreateNewXref(XrefType.Individual));

        individualMapper.Add(person.GetXrefName(), new XrefMapperClass(xref.GetXrefName(), true));
        person.SetXrefName(xref.GetXrefName());

        if (person.GetFamilyChildList() != null)
        {
          IList<FamilyXrefClass> newChildFamilies = new List<FamilyXrefClass>();
          foreach (FamilyXrefClass childFamily in person.GetFamilyChildList())
          {
            FamilyXrefClass newFamily = new FamilyXrefClass(mappers.GetLocalXRef(XrefType.Family, childFamily.GetXrefName()));
            newChildFamilies.Add(newFamily);
          }
          person.SetFamilyChildList(newChildFamilies);
        }

        if (person.GetFamilySpouseList() != null)
        {
          IList<FamilyXrefClass> newSpouseFamilies = new List<FamilyXrefClass>();
          foreach (FamilyXrefClass spouseFamily in person.GetFamilySpouseList())
          {
            FamilyXrefClass newFamily = new FamilyXrefClass(mappers.GetLocalXRef(XrefType.Family, spouseFamily.GetXrefName()));
            newSpouseFamilies.Add(newFamily);
          }
          person.SetFamilySpouseList(newSpouseFamilies);
        }


        inFamilyTree.AddIndividual(person);
      }

      IEnumerator<FamilyClass> familyEnumerator = anarkivStore.SearchFamily();
      counter = 0;
      while(familyEnumerator.MoveNext())
      {
        FamilyClass family = familyEnumerator.Current;
        FamilyClass newFamily = new FamilyClass();

        trace.TraceInformation("Family[" + counter++ + "]:" + family.GetXrefName());
        newFamily.SetXrefName(mappers.GetLocalXRef(XrefType.Family, family.GetXrefName(), true));

        trace.TraceInformation("Family xref " + family.GetXrefName() + " ==> " + newFamily.GetXrefName());

        if (family.GetParentList() != null)
        {
          //IList<IndividualXrefClass> newParentList = new List<IndividualXrefClass>();
          foreach (IndividualXrefClass parent in family.GetParentList())
          {
            IndividualXrefClass newParent = new IndividualXrefClass(mappers.GetLocalXRef(XrefType.Individual, parent.GetXrefName()));
            //newParentList.Add(newParent);
            newFamily.AddRelation(newParent, FamilyClass.RelationType.Parent);
            trace.TraceInformation(" add parent  " + parent.GetXrefName() + " => " + newParent.GetXrefName());
          }
        }
        if (family.GetChildList() != null)
        {
          //IList<IndividualXrefClass> newChildList = new List<IndividualXrefClass>();
          foreach (IndividualXrefClass child in family.GetChildList())
          {
            IndividualXrefClass newChild = new IndividualXrefClass(mappers.GetLocalXRef(XrefType.Individual, child.GetXrefName()));
            //newChildList.Add(newChild);
            newFamily.AddRelation(newChild, FamilyClass.RelationType.Child);
            trace.TraceInformation(" add child  " + child.GetXrefName() + " => " + newChild.GetXrefName());
          }
        }
        //family.

        inFamilyTree.AddFamily(newFamily);
      }


    }

    public override bool OpenFile(String fileName, ref FamilyTreeStoreBaseClass inFamilyTree, CompletedCallback callback)
    {
      FamilyTreeStoreAnarkiv anarkivStore;

      if (inFamilyTree.GetType() == typeof(FamilyTreeStoreAnarkiv))
      {
        anarkivStore = (FamilyTreeStoreAnarkiv)inFamilyTree;
      }
      else
      {
        anarkivStore = (FamilyTreeStoreAnarkiv)CreateFamilyTreeStore(fileName, callback);
        ReadFile(ref inFamilyTree, anarkivStore);
      }

      trace.TraceInformation("AnarkivFileType::OpenFile( " + fileName + ")");
      anarkivStore.SetFile(fileName);
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
      if (operation == FamilyFileTypeOperation.Import)
      {
        return "Anarkiv|*.ddb";
      }
      return null;
    }

  }
}

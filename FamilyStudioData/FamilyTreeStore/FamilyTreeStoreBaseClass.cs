using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
//using System.ComponentModel;
using FamilyStudioData.FamilyData;
//using FamilyStudioData.FamilyFileFormat;

namespace FamilyStudioData.FamilyTreeStore
{
  public class FamilyTreeContentClass
  {
    public int families;
    public int submitters;
    public int individuals;
    public int notes;
    public int sources;
    public int repositories;
    public int submissions;
    public int multimediaObjects;
    public int percent;
  }

  public class ValidationData
  {
    public int familyNo;
    public int submitterNo;
    public int individualNo;
    public int noteNo;
  };

  public enum FamilyTreeCharacterSet
  {
    Unknown,
    Utf8,
    Unicode,
    Ascii,
    Ansel
  }
  public enum SelectIndex
  {
    NoIndex = 0x7fffffff
  }

  public enum PersonDetail
  {
    PersonDetail_Name     = 0x0001,
    PersonDetail_Events   = 0x0002,
    PersonDetail_Sex      = 0x0004,
    PersonDetail_Children = 0x0008,
    PersonDetail_Parents  = 0x0010,

    PersonDetail_All = 0xFFFF
  }

  public enum PersonUpdateType
  {
    Name         = 0x0001,
    Events       = 0x0002,
    SpouseFamily = 0x0004,
    ChildFamily  = 0x0008,
  }
  public interface FamilyTreeStoreBaseClass
  {

    // Family interface
    void AddFamily(FamilyClass tempFamily);
    FamilyClass GetFamily(String xrefName);
    IEnumerator<FamilyClass> SearchFamily(String familyXrefName = null, ProgressReporter progressReporter = null);

    // Person interface
    bool AddIndividual(IndividualClass tempIndividual);
    bool UpdateIndividual(IndividualClass tempIndividual, PersonUpdateType type);
    IndividualClass GetIndividual(String xrefName = null, uint index = (uint)SelectIndex.NoIndex, PersonDetail detailLevel = PersonDetail.PersonDetail_All);
    IEnumerator<IndividualClass> SearchPerson(String individualName = null, ProgressReporter progressReporter = null);
    void SetHomeIndividual(String xrefName);
    string GetHomeIndividual();

    // Multimedia object interface
    void AddMultimediaObject(MultimediaObjectClass tempMultimediaObject);
    IEnumerator<MultimediaObjectClass> SearchMultimediaObject(String mmoString = null, ProgressReporter progressReporter = null);

    // Note interface
    void AddNote(NoteClass tempNote);
    NoteClass GetNote(String xrefName);
    IEnumerator<NoteClass> SearchNote(String noteString = null, ProgressReporter progressReporter = null);

    // Repository interface
    void AddRepository(RepositoryClass tempRepository);
    IEnumerator<RepositoryClass> SearchRepository(String repositoryString = null, ProgressReporter progressReporter = null);

    // Source interface (Move to import?)
    void AddSource(SourceClass tempSource);
    IEnumerator<SourceClass> SearchSource(String sourceString = null, ProgressReporter progressReporter = null);

    // Submission interface (Move to import?)
    void AddSubmission(SubmissionClass tempSubmission);
    IEnumerator<SubmissionClass> SearchSubmission(String submissionString = null, ProgressReporter progressReporter = null);

    // Submitter interface (Move to import?)
    void AddSubmitter(SubmitterClass tempSubmitter);
    void SetSubmitterXref(SubmitterXrefClass tempSubmitterXref);
    IEnumerator<SubmitterClass> SearchSubmitter(String submitterName = null, ProgressReporter progressReporter = null);

    string CreateNewXref(XrefType type);

    // Source information (Move to import?)
    void SetSourceFileType(String type);
    void SetSourceFileTypeVersion(String version);
    void SetSourceFileTypeFormat(String format);
    void SetSourceFileName(String filename);
    string GetSourceFileName();

    void SetSourceName(String source);

    void SetCharacterSet(FamilyTreeCharacterSet charSet);

    void SetDate(FamilyDateTimeClass inDate);

    // Print functions
    void Print();
    void PrintShort();

    String GetShortTreeInfo();

    FamilyTreeContentClass GetContents();

    // Validation functions
    bool ValidateFamilies();

    bool ValidateIndividuals();

    bool ValidateTree();



  }
}

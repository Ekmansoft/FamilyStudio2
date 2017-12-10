using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using FamilyStudioData.FamilyData;

namespace FamilyStudioData.FamilyData
{
  [DataContract]
  public class PlaceStructureClass
  {
    [DataMember]
    private IList<NoteClass> noteList;
    [DataMember]
    private IList<NoteXrefClass> noteXrefList;
    [DataMember]
    private IList<SourceDescriptionClass> sourceList;
    [DataMember]
    private IList<SourceXrefClass> sourceXrefList;
    [DataMember]
    private String placeValue;
    [DataMember]
    private String placeHierarchy;

    public PlaceStructureClass(string place = null)
    {
      placeValue = "";
      noteList = new List<NoteClass>();
      //sourceList = new List<SourceDescriptionClass>();
      noteXrefList = new List<NoteXrefClass>();
      sourceXrefList = new List<SourceXrefClass>();
      if(place != null)
      {
        placeValue = place;
      }
    }
    public void SetPlace(string place)
    {
      this.placeValue = place;
    }
    public string GetPlace()
    {
      return this.placeValue;
    }
    public void SetPlaceHierarchy(string placeHierarchy)
    {
      this.placeHierarchy = placeHierarchy;
    }
    public string GetPlaceHierarchy()
    {
      return this.placeHierarchy;
    }
    public void AddNote(NoteClass note)
    {
      noteList.Add(note);
    }
    public void AddNoteXref(NoteXrefClass note)
    {
      noteXrefList.Add(note);
    }
    public void AddSource(SourceDescriptionClass source)
    {
      if (sourceList == null)
      {
        sourceList = new List<SourceDescriptionClass>();
      }
      sourceList.Add(source);
    }
    public void AddSourceXref(SourceXrefClass source)
    {
      sourceXrefList.Add(source);
    }
    public IList<SourceDescriptionClass> GetSourceList()
    {
      return sourceList;
    }
    public IList<SourceXrefClass> GetSourceXrefList()
    {
      return sourceXrefList;
    }
    public IList<NoteClass> GetNoteList()
    {
      return noteList;
    }
    public IList<NoteXrefClass> GetNoteXrefList()
    {
      return noteXrefList;
    }
    public override string ToString()
    {
      string tString = this.placeHierarchy + " " + this.placeValue;

      foreach (NoteClass note in noteList)
      {
        tString += " " + note.ToString();
      }
      return tString;
    }
  }
}

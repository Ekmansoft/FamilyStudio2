using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using FamilyStudioData.FamilyTreeStore;

namespace FamilyStudioData.FamilyData
{
  [DataContract]
  public class FamilyDateTimeClass
  {
    private static TraceSource trace = new TraceSource("FamilyDateTimeClass", SourceLevels.Warning);
    public enum FamilyDateType
    {
      Unknown,
      //Exact,
      //Approximate,

      Year,
      YearMonth,
      YearMonthDay,
      YearMonthDayHour,
      YearMonthDayHourMinute,
      YearMonthDayHourMinteSecond,

      DateString
    }
    public enum FamilyTimeType
    {
      Unknown,
      Hour,
      HourMinute,
      HourMinuteSecond
    }

    [DataMember]
    private int year, month, day;
    [DataMember]
    private int hour, minute, second;
    //private DateTime endDate;
    [DataMember]
    private bool approximate;
    [DataMember]
    private FamilyDateType dateType;
    [DataMember]
    private FamilyTimeType timeType;
    [DataMember]
    private String dateStr;

    public FamilyDateTimeClass()
    {
      dateType = FamilyDateType.Unknown;
      timeType = FamilyTimeType.Unknown;
      approximate = false;
    }
    public FamilyDateTimeClass(int inYear, int inMonth, int inDay)
    {
      dateType = FamilyDateType.YearMonthDay;
      year = inYear;
      month = inMonth;
      day = inDay;
      if (month < 1)
      {
        dateType = FamilyDateType.Year;
      }
      else if (day < 1)
      {
        dateType = FamilyDateType.YearMonth;
      }
      approximate = false;
    }
    public FamilyDateTimeClass(String inDateStr)
    {
      dateType = FamilyDateType.DateString;
      approximate = true;
      dateStr = inDateStr;
    }

    public FamilyDateTimeClass(int inYear, String monthStr, int inDay)
    {
      dateType = FamilyDateType.YearMonthDay;
      year = inYear;

      switch (monthStr)
      {
        case "JAN":
          month = 1;
          break;
        case "FEB":
          month = 2;
          break;
        case "MAR":
          month = 3;
          break;
        case "APR":
          month = 4;
          break;
        case "MAY":
          month = 5;
          break;
        case "JUN":
          month = 6;
          break;
        case "JUL":
          month = 7;
          break;
        case "AUG":
          month = 8;
          break;
        case "SEP":
          month = 9;
          break;
        case "OCT":
          month = 10;
          break;
        case "NOV":
          month = 11;
          break;
        case "DEC":
          month = 12;
          break;
        default:
          trace.TraceInformation("Warning: Unknown month[" + monthStr + "]");
          month = 0;
          break;
      }
      day = inDay;
      approximate = false;
      if (month == 0)
      {
        if (day != 0)
        {
          trace.TraceInformation("Warning: strange date:[" + monthStr + "]");
        }
        dateType = FamilyDateTimeClass.FamilyDateType.Year;
      }

    }
    private string GetMonthStr(int month)
    {
      switch(month)
      {
        case 1:
          return "JAN";
        case 2:
          return "FEB";
        case 3:
          return "MAR";
        case 4:
          return "APR";
        case 5:
          return "MAY";
        case 6:
          return "JUN";
        case 7:
          return "JUL";
        case 8:
          return "AUG";
        case 9:
          return "SEP";
        case 10:
          return "OCT";
        case 11:
          return "NOV";
        case 12:
          return "DEC";
        default:
          return "errmonth:" + month;
      }
    }

    public void SetApproximate(bool inApproximate)
    {
      approximate = inApproximate;
    }
    public bool GetApproximate()
    {
      return approximate;
    }
    public void SetDateType(FamilyDateType inType)
    {
      dateType = inType;
    }
    public FamilyDateType GetDateType()
    {
      return dateType;
    }
    public FamilyTimeType GetTimeType()
    {
      return timeType;
    }
    public bool ValidDate()
    {
      return dateType != FamilyDateType.Unknown;
    }
    public override String ToString()
    {
      string approxStr = "";
      string monthStr = "";
      string dayStr = "";

      if (month < 10)
      {
        monthStr = "0";
      }
      monthStr += month.ToString();

      if (day < 10)
      {
        dayStr = "0";
      }
      dayStr += day.ToString();

      if (approximate)
      {
        approxStr = "circa ";
      }
      switch (dateType)
      {
        case FamilyDateType.YearMonthDay:
          return approxStr + year.ToString("D4") + "-" + monthStr + "-" + dayStr;

        case FamilyDateType.YearMonth:
          return approxStr + year.ToString("D4") + "-" + monthStr;

        case FamilyDateType.Year:
          return approxStr + year.ToString("D4");

        case FamilyDateType.DateString:
          return approxStr + dateStr;

        case FamilyDateType.Unknown:
          return "";

        default:
          return "error-date:" + dateType;
      }
    }

    public String ToGedcomDateString()
    {
      string approxStr = "";

      if (approximate)
      {
        approxStr = "ABT ";
      }
      switch (dateType)
      {
        case FamilyDateType.YearMonthDay:
          return approxStr + day.ToString("D2") + " " + GetMonthStr(month) + " " + year.ToString();

        case FamilyDateType.YearMonth:
          return approxStr + GetMonthStr(month) + " " + year.ToString();

        case FamilyDateType.Year:
          return approxStr + year.ToString("D4");

        case FamilyDateType.DateString:
          return dateStr;

        case FamilyDateType.Unknown:
          return "";

        default:
          return "error-date:" + dateType;
      }
    }

    public String ToGedcomTimeString()
    {
      switch (timeType)
      {
        case FamilyTimeType.HourMinuteSecond:
          return hour.ToString("D2") + ":" + minute.ToString("D2") + ":" + second.ToString("D2");

        case FamilyTimeType.HourMinute:
          return hour.ToString("D2") + ":" + minute.ToString("D2");

        case FamilyTimeType.Hour:
          return hour.ToString("D2") + ":00";

        case FamilyTimeType.Unknown:
          return "";

        default:
          return "error-time:" + timeType;
      }
    }

    public DateTime ToDateTime()
    {
      FamilyDateType type = dateType;
      if(year <= 0)
      {
        trace.TraceInformation("Warning bad year!" + year);
        type = FamilyDateType.Unknown;
      }
      try
      {

        switch (type)
        {
          case FamilyDateType.YearMonthDay:
            return new DateTime(year, month, day);

          case FamilyDateType.YearMonth:
            return new DateTime(year, month, 1);

          case FamilyDateType.Year:
            return new DateTime(year, 1, 1);

          case FamilyDateType.DateString:
          case FamilyDateType.Unknown:
          default:
            return new DateTime();
        }
      }
      catch (ArgumentOutOfRangeException e)
      {
        trace.TraceInformation("Date type" + type + " " + year + "-" + month + "-" + day );
        trace.TraceInformation(e.ToString());
        return new DateTime();
      }

    }

    public void SetTime(int inHour, int inMinute, int inSecond)
    {
      if((inHour >= 0) && (inHour <= 24))
      {
        hour = inHour;
        if ((inMinute >= 0) && (inMinute <= 59))
        {
          minute = inMinute;
          if ((inSecond >= 0) && (inSecond <= 59))
          {
            second = inSecond;
            timeType = FamilyTimeType.HourMinuteSecond;
          }
          else
          {
            timeType = FamilyTimeType.HourMinute;
          }
        }
        else
        {
          timeType = FamilyTimeType.Hour;
        }
      }
      
    }


  }
  [DataContract]
  public class FamilyClass
  {
    private static TraceSource trace = new TraceSource("FamilyClass", SourceLevels.Warning);
    public enum RelationType
    {
      Child,
      Parent,
      Submitter
    };
    public enum FamilySpecialRecordIdType
    {
      PermanentRecordFileNumber,
      AutomatedRecordId,
      AncestralFileNumber,
      UserReferenceNumber
    }
    [DataMember]
    private String xrefName;
    [DataMember]
    private int numberOfChildren;
    [DataMember]
    private IDictionary<FamilySpecialRecordIdType, string> specialRecordList;


    public FamilyClass()
    {
      xrefName = "";
      numberOfChildren = 0;
    }

    public void AddRelation(IndividualXrefClass person, RelationType relation)
    {
      switch (relation)
      {
        case RelationType.Parent:
          if (parentList == null)
          {
            parentList = new List<IndividualXrefClass>();
          }
          parentList.Add(person);
          break;

        case RelationType.Child:
          if (childList == null)
          {
            childList = new List<IndividualXrefClass>();
          }
          childList.Add(person);
          break;

        default:
          break;
      }
    }

    public void AddSubmitter(SubmitterXrefClass person)
    {
      if (submitterList == null)
      {
        submitterList = new List<SubmitterXrefClass>();
      }
      submitterList.Add(person);
    }

    public void AddEvent(IndividualEventClass.EventType eventType, FamilyDateTimeClass date)
    {
      IndividualEventClass tempEvent = new IndividualEventClass();

      tempEvent.SetEventType(eventType);
      tempEvent.SetDate(date);

      if (familyEventList == null)
      {
        familyEventList = new List<IndividualEventClass>();
      }
      familyEventList.Add(tempEvent);
    }
    public void AddEvent(IndividualEventClass tempEvent)
    {
      if (familyEventList == null)
      {
        familyEventList = new List<IndividualEventClass>();
      }
      familyEventList.Add(tempEvent);
    }
    public IList<IndividualEventClass> GetEventList(IndividualEventClass.EventType requestedEventTypes = IndividualEventClass.EventType.AllEvents)
    {
      return familyEventList;
    }
    public IndividualEventClass GetEvent(IndividualEventClass.EventType requestedEventType)
    {
      if (familyEventList != null)
      {
        foreach (IndividualEventClass ev in familyEventList)
        {
          if (ev.GetEventType() == requestedEventType)
          {
            return ev;
          }
        }
      }
      return null;
    }

    public void SetXrefName(String name)
    {
      //trace.TraceInformation(this,"SetXrefName:" + name);
      xrefName = name;
    }
    public String GetXrefName()
    {
      return xrefName;
    }
    public void SetNumberOfChildren(int number)
    {
      //trace.TraceInformation(this,"SetNumberOfChildren:" + number);
      numberOfChildren = number;
    }
    public int GetNumberOfChildren()
    {
      return numberOfChildren;
    }

    public void AddNoteXref(NoteXrefClass noteXref)
    {
      //trace.TraceInformation(this,"SetXrefName:" + name);
      if (noteXrefList == null)
      {
        noteXrefList = new List<NoteXrefClass>();
      }
      noteXrefList.Add(noteXref);
    }
    public IList<NoteXrefClass> GetNoteXrefList(string noteFilter = null)
    {
      return noteXrefList;
    }

    public void AddNote(NoteClass note)
    {
      //trace.TraceInformation(this,"SetXrefName:" + name);
      if (noteList == null)
      {
        noteList = new List<NoteClass>();
      }
      noteList.Add(note);
    }
    public IList<NoteClass> GetNoteList(string noteFilter = null)
    {
      return noteList;
    }
    public IList<NoteXrefClass> GetNoteXrefList()
    {
      return noteXrefList;
    }
    public void AddSource(SourceDescriptionClass source)
    {
      if (sourceList == null)
      {
        sourceList = new List<SourceDescriptionClass>();
      }
      sourceList.Add(source);
    }
    public IList<SourceDescriptionClass> GetSourceList(string sourceFilter = null)
    {
      return sourceList;
    }
    public void AddSourceXref(SourceXrefClass sourceXref)
    {
      if (sourceXrefList == null)
      {
        sourceXrefList = new List<SourceXrefClass>();
      }
      sourceXrefList.Add(sourceXref);
    }
    public IList<SourceXrefClass> GetSourceXrefList()
    {
      return sourceXrefList;
    }

    public void AddMultimediaLink(MultimediaLinkClass multimediaLink)
    {
      //trace.TraceInformation(this,"IndividualClass.SetDate(" + eventType + "," + date + ")");
      if (multimediaLinkList == null)
      {
        multimediaLinkList = new List<MultimediaLinkClass>();
      }
      multimediaLinkList.Add(multimediaLink);
    }
    public IList<MultimediaLinkClass> GetMultimediaLinkList()
    {
      //trace.TraceInformation("IndividualClass.AddMultimediaLink(" + multimediaLink + ")");
      if (multimediaLinkList != null)
      {
        return multimediaLinkList;
      }
      return new List<MultimediaLinkClass>();
    }

    public void SetSpecialRecordId(FamilySpecialRecordIdType type, String recordId)
    {
      //trace.TraceInformation(this,"SetXrefName:" + name);
      //automatedRecordId = recordId;
      if (specialRecordList == null)
      {
        specialRecordList = new Dictionary<FamilySpecialRecordIdType, string>();
      }
      specialRecordList.Add(type, recordId);
    }

    public void Print()
    {
      trace.TraceInformation("Family:" + xrefName + "-start");
      //trace.TraceInformation("Family:" + xrefName + " f:" + fatherList.Count + " m:" + motherList.Count + " c:" + childList.Count + " s:" + submitterList.Count);
      if (parentList != null)
      {
        trace.TraceInformation("\n p:" + parentList.Count + ":");
        foreach (IndividualXrefClass indi in parentList)
        {
          indi.Print();
        }

      }
      else
      {
        trace.TraceInformation("\n p:0");
      }
      if (childList != null)
      {
        trace.TraceInformation("\n c:" + childList.Count + ":");
        foreach (IndividualXrefClass indi in childList)
        {
          indi.Print();
        }
      }
      else
      {
        trace.TraceInformation("\n c:0");
      }
      if (submitterList != null)
      {
        trace.TraceInformation("\n s:" + submitterList.Count + ":");
        foreach (SubmitterXrefClass indi in submitterList)
        {
          indi.Print();
        }
      }
      else
      {
        trace.TraceInformation("\n s:0");
      }
      trace.TraceInformation("\nFamily:" + xrefName + "-end");
    }

    public bool Validate(FamilyTreeStoreBaseClass familyTree, ref ValidationData validationData)
    {
      return true;
    }

    public override int GetHashCode()
    {
      if (hashCodeValid)
      {
        return hashCode;
      }
      hashCode = 0;

      for (int i = 0; i < xrefName.Length; i++)
      {
        hashCode += (int)((xrefName[i] - '0') * (int)Math.Pow(10, (xrefName.Length - i - 1)));
      }
      hashCodeValid = true;
      return hashCode;
    }

    public IList<IndividualXrefClass> GetParentList()
    {
      return parentList;
    }
    public IList<IndividualXrefClass> GetChildList()
    {
      return childList;
    }


    [DataMember]
    private IList<IndividualXrefClass> parentList;
    [DataMember]
    private IList<IndividualXrefClass> childList;
    [DataMember]
    private IList<SubmitterXrefClass> submitterList;
    [DataMember]
    private IList<IndividualEventClass> familyEventList;
    [DataMember]
    private IList<NoteClass> noteList;
    [DataMember]
    private IList<NoteXrefClass> noteXrefList;
    [DataMember]
    private IList<SourceDescriptionClass> sourceList;
    [DataMember]
    private IList<SourceXrefClass> sourceXrefList;
    [DataMember]
    private IList<MultimediaLinkClass> multimediaLinkList;
    [DataMember]
    private int hashCode;
    [DataMember]
    private bool hashCodeValid;


  }

  [DataContract]
  public class FamilyXrefClass : BaseXrefClass
  {
    public enum PedigreeType
    {
      Unknown,
      Adopted,
      Birth,
      Foster,
      Sealing
    };
    [DataMember]
    public IList<NoteClass> noteList;
    [DataMember]
    public IList<NoteXrefClass> noteXrefList;
    [DataMember]
    private PedigreeType pedigreeType;


    public FamilyXrefClass(String name) : base(XrefType.Family, name)
    {
    }

    public void SetPedigreeType(PedigreeType pedigree)
    {
      pedigreeType = pedigree;
    }
    public void AddNoteXref(NoteXrefClass noteXref)
    {
      if (noteXrefList == null)
      {
        noteXrefList = new List<NoteXrefClass>();
      }
      noteXrefList.Add(noteXref);
    }

    public void AddNote(NoteClass note)
    {
      if (noteList == null)
      {
        noteList = new List<NoteClass>();
      }
      noteList.Add(note);
    }
  }
}

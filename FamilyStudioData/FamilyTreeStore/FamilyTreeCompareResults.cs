using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using FamilyStudioData.FamilyData;

namespace FamilyStudioData.FamilyTreeStore
{
  class FamilyTreeCompareResults
  {
  }

  [DataContract]
  public class DuplicateTreeItems
  {
    [DataMember]
    public string item1;
    [DataMember]
    public string item2;

    public DuplicateTreeItems(string i1, string i2)
    {
      item1 = i1;
      item2 = i2;
    }
  }

  [DataContract]
  public class SavedMatches
  {
    [DataMember]
    public string database1, database2;
    [DataMember]
    public IList<DuplicateTreeItems> itemList;

    public SavedMatches()
    {
      itemList = new List<DuplicateTreeItems>();
    }
  }

  public delegate void ReportCompareResult(FamilyTreeStoreBaseClass familyTree1, string person1, FamilyTreeStoreBaseClass familyTree2, string person2);

  public class CompareTreeClass
  {
    private static TraceSource trace = new TraceSource("CompareTrees", SourceLevels.Warning);

    static string NormalizeName(string name)
    {
      return name.ToLower().Replace("w", "v").Replace("  ", " ").Replace("*", "").Replace("å", "a").Replace("ä", "a").Replace("ö", "o");
    }

    static bool IsNamesEqual(string name1, string name2)
    {
      name1 = NormalizeName(name1);
      name2 = NormalizeName(name2);

      return name1 == name2;
    }

    enum DateMatch
    {
      Unknown,
      Bad,
      Ok,
      Good
    }
    static DateMatch MatchDates(FamilyDateTimeClass date1, FamilyDateTimeClass date2)
    {
      if ((date1 != null) && (date2 != null))
      {
        if (date1.ValidDate() && date2.ValidDate())
        {
          if ((date1.GetDateType() >= FamilyDateTimeClass.FamilyDateType.YearMonthDay) &&
              (date2.GetDateType() >= FamilyDateTimeClass.FamilyDateType.YearMonthDay))
          {
            DateTime dt1 = date1.ToDateTime();
            DateTime dt2 = date2.ToDateTime();

            TimeSpan diff = dt1 - dt2;

            if (Math.Abs(diff.Days) <= 10)
            {
              return DateMatch.Good;
            }
            if (Math.Abs(diff.Days) <= 300)
            {
              return DateMatch.Ok;
            }
            return DateMatch.Bad;
          }
          if ((date1.GetDateType() >= FamilyDateTimeClass.FamilyDateType.YearMonth) &&
              (date2.GetDateType() >= FamilyDateTimeClass.FamilyDateType.YearMonth))
          {
            DateTime dt1 = date1.ToDateTime();
            DateTime dt2 = date2.ToDateTime();

            if ((dt1.Year == dt2.Year) && (dt1.Month == dt2.Month))
            {
              return DateMatch.Good;
            }
            if (Math.Abs(dt1.Year - dt2.Year) <= 1)
            {
              return DateMatch.Ok;
            }
            return DateMatch.Bad;
          }
          if ((date1.GetDateType() >= FamilyDateTimeClass.FamilyDateType.Year) &&
              (date2.GetDateType() >= FamilyDateTimeClass.FamilyDateType.Year))
          {
            DateTime dt1 = date1.ToDateTime();
            DateTime dt2 = date2.ToDateTime();

            if (dt1.Year == dt2.Year)
            {
              return DateMatch.Good;
            }
            if (Math.Abs(dt1.Year - dt2.Year) <= 2)
            {
              return DateMatch.Ok;
            }
          }
          return DateMatch.Bad;
        }
      }
      return DateMatch.Unknown;
    }

    public static bool ComparePerson(IndividualClass person1, IndividualClass person2)
    {
      if (IsNamesEqual(person1.GetName(), person2.GetName()))
      {
        IndividualEventClass birth1 = person1.GetEvent(IndividualEventClass.EventType.Birth);
        IndividualEventClass birth2 = person2.GetEvent(IndividualEventClass.EventType.Birth);
        IndividualEventClass death1 = person1.GetEvent(IndividualEventClass.EventType.Death);
        IndividualEventClass death2 = person2.GetEvent(IndividualEventClass.EventType.Death);

        DateMatch birthMatch = DateMatch.Unknown, deathMatch = DateMatch.Unknown;

        if ((birth1 != null) && (birth2 != null))
        {
          birthMatch = MatchDates(birth1.GetDate(), birth2.GetDate());
        }
        if ((death1 != null) && (death2 != null))
        {
          deathMatch = MatchDates(death1.GetDate(), death2.GetDate());
        }
        if ((birthMatch == DateMatch.Unknown) && (deathMatch == DateMatch.Unknown))
        {
          return false;
        }
        if ((birthMatch == DateMatch.Bad) || (deathMatch == DateMatch.Bad))
        {
          return false;
        }
        return (birthMatch == DateMatch.Good) || (deathMatch == DateMatch.Good);
      }
      return false;
    }

    public static void SearchDuplicates(IndividualClass person1, FamilyTreeStoreBaseClass familyTree1, FamilyTreeStoreBaseClass familyTree2, ReportCompareResult reportDuplicate, ProgressReporterInterface reporter = null)
    {
      IndividualEventClass birth = person1.GetEvent(IndividualEventClass.EventType.Birth);
      IndividualEventClass death = person1.GetEvent(IndividualEventClass.EventType.Death);

      if (reporter != null)
      {
        trace.TraceInformation(reporter.ToString());
      }
      if (((birth != null) && (birth.GetDate() != null) && (birth.GetDate().ValidDate())) ||
          ((death != null) && (death.GetDate() != null) && (death.GetDate().ValidDate())))
      {
        string fullName = person1.GetName().Replace("*", "");
        IEnumerator<IndividualClass> iterator2 = familyTree2.SearchPerson(fullName);
        int cnt2 = 0;

        if (iterator2 != null)
        {
          int cnt3 = 0;
          do
          {
            IndividualClass person2 = iterator2.Current;

            if (person2 != null)
            {
              cnt3++;
              //trace.TraceInformation(reporter.ToString() + "   2:" + person2.GetName());
              if ((familyTree1 != familyTree2) || (person1.GetXrefName() != person2.GetXrefName()))
              {
                if (ComparePerson(person1, person2))
                {
                  reportDuplicate(familyTree1, person1.GetXrefName(), familyTree2, person2.GetXrefName());
                }
                cnt2++;
              }
            }
          } while (iterator2.MoveNext());

          iterator2.Dispose();
          trace.TraceInformation(" " + fullName + " matched with " + cnt2 + "," + cnt3);
        }

        if (cnt2 == 0) // No matches found for full name
        {
          if ((person1.GetPersonalName().GetName(PersonalNameClass.PartialNameType.BirthSurname).Length > 0) &&
              (person1.GetPersonalName().GetName(PersonalNameClass.PartialNameType.Surname).Length > 0) &&
              !person1.GetPersonalName().GetName(PersonalNameClass.PartialNameType.Surname).Equals(person1.GetPersonalName().GetName(PersonalNameClass.PartialNameType.BirthSurname)))
          {
            String strippedName = person1.GetName().Replace("*", "");

            if (strippedName.Contains(person1.GetPersonalName().GetName(PersonalNameClass.PartialNameType.Surname)))
            {
              String maidenName = strippedName.Replace(person1.GetPersonalName().GetName(PersonalNameClass.PartialNameType.Surname), "").Replace("  ", " ");
              IEnumerator<IndividualClass> iterator3 = familyTree2.SearchPerson(maidenName);
              //trace.TraceInformation(" Searching Maiden name " + maidenName);

              if (iterator3 != null)
              {
                int cnt3 = 0;
                do
                {
                  IndividualClass person2 = iterator3.Current;

                  if (person2 != null)
                  {
                    if ((familyTree1 != familyTree2) || (person1.GetXrefName() != person2.GetXrefName()))
                    {
                      cnt3++;
                      if (ComparePerson(person1, person2))
                      {
                        reportDuplicate(familyTree1, person1.GetXrefName(), familyTree2, person2.GetXrefName());
                      }
                    }
                  }
                } while (iterator3.MoveNext());
                iterator3.Dispose();
                trace.TraceInformation(" Maiden name " + maidenName + " mathched with " + cnt3);
              }
            }
            if (strippedName.Contains(person1.GetPersonalName().GetName(PersonalNameClass.PartialNameType.BirthSurname)))
            {
              String marriedName = strippedName.Replace(person1.GetPersonalName().GetName(PersonalNameClass.PartialNameType.BirthSurname), "").Replace("  ", " ");
              IEnumerator<IndividualClass> iterator3 = familyTree2.SearchPerson(marriedName);

              //trace.TraceInformation(" Searching Married name " + marriedName);
              if (iterator3 != null)
              {
                int cnt3 = 0;
                do
                {
                  //IndividualClass person1 = iterator1.Current;
                  IndividualClass person2 = iterator3.Current;

                  if (person2 != null)
                  {
                    //trace.TraceInformation(reporter.ToString() + "   2:" + person2.GetName());
                    if ((familyTree1 != familyTree2) || (person1.GetXrefName() != person2.GetXrefName()))
                    {
                      cnt3++;
                      if (ComparePerson(person1, person2))
                      {
                        reportDuplicate(familyTree1, person1.GetXrefName(), familyTree2, person2.GetXrefName());
                      }
                    }
                  }
                } while (iterator3.MoveNext());
                iterator3.Dispose();
                trace.TraceInformation(" Married name "+ marriedName + " matched to " + cnt3);
              }
            }
          }
        }
      }
      else
      {
        trace.TraceInformation("No valid birth or death date for " + person1.GetName().ToString() + " skip duplicate search");
      }
    }

    public static void CompareTrees(FamilyTreeStoreBaseClass familyTree1, FamilyTreeStoreBaseClass familyTree2, ReportCompareResult reportDuplicate, ProgressReporterInterface reporter = null)
    {
      IEnumerator<IndividualClass> iterator1;
      int cnt1 = 0;
      iterator1 = familyTree1.SearchPerson(null, reporter);

      trace.TraceInformation("CompareTrees() started");

      if (iterator1 != null)
      {
        do
        {
          IndividualClass person1 = iterator1.Current;

          cnt1++;
          if (person1 != null)
          {
            trace.TraceInformation(" 1:" + person1.GetName());
            SearchDuplicates(person1, familyTree1, familyTree2, reportDuplicate, reporter);
          }
        } while (iterator1.MoveNext());
        iterator1.Dispose();
      }
      else
      {
        trace.TraceInformation("iter=null");
      }
      trace.TraceInformation("CompareTrees() done");
    }
  }
}

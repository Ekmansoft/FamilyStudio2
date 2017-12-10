using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using FamilyStudioData.FamilyData;
using FamilyStudioData.FamilyTreeStore;
//using FamilyStudioData.FamilyTreeStore;
//using System.Threading.Tasks;

namespace FamilyStudioData.FamilyTreeStore
{
  [DataContract]
  public class SanityProperty
  {
    [DataMember]
    public bool active;
    [DataMember]
    public int value;
  }

  [DataContract]
  public class SanityCheckLimits
  {
    [DataMember]
    public SanityProperty parentLimitMin;
    [DataMember]
    public SanityProperty motherLimitMax;
    [DataMember]
    public SanityProperty fatherLimitMax;
    [DataMember]
    public SanityProperty eventLimitMin;
    [DataMember]
    public SanityProperty eventLimitMax;
    [DataMember]
    public SanityProperty noOfChildrenMin;
    [DataMember]
    public SanityProperty noOfChildrenMax;
    [DataMember]
    public SanityProperty daysBetweenChildren;
    [DataMember]
    public SanityProperty twins;
    [DataMember]
    public SanityProperty inexactBirthDeath;
    [DataMember]
    public SanityProperty unknownBirthDeath;
    [DataMember]
    public SanityProperty parentsMissing;
    [DataMember]
    public SanityProperty parentsProblem;
    [DataMember]
    public SanityProperty generationlimited;

    public SanityCheckLimits()
    {
      parentLimitMin = new SanityProperty();
      parentLimitMin.value = 15;
      parentLimitMin.active = true;

      motherLimitMax = new SanityProperty();
      motherLimitMax.value = 48;
      motherLimitMax.active = true;

      fatherLimitMax = new SanityProperty();
      fatherLimitMax.active = true;
      fatherLimitMax.value = 65;

      eventLimitMin = new SanityProperty();
      eventLimitMin.active = false;
      eventLimitMin.value = 0;

      eventLimitMax = new SanityProperty();
      eventLimitMax.active = true;
      eventLimitMax.value = 105;

      noOfChildrenMin = new SanityProperty();
      noOfChildrenMin.active = true;
      noOfChildrenMin.value = 1;

      noOfChildrenMax = new SanityProperty();
      noOfChildrenMax.value = 15;
      noOfChildrenMax.active = false;

      daysBetweenChildren = new SanityProperty();
      daysBetweenChildren.value = 250;
      daysBetweenChildren.active = true;

      twins = new SanityProperty();
      twins.active = false;

      inexactBirthDeath = new SanityProperty();
      inexactBirthDeath.active = false;

      unknownBirthDeath = new SanityProperty();
      unknownBirthDeath.active = true;

      parentsMissing = new SanityProperty();
      parentsMissing.active = true;

      parentsProblem = new SanityProperty();
      parentsProblem.active = true;

      generationlimited = new SanityProperty();
      generationlimited.active = false;

      
    }
  }

  [DataContract]
  public class Relation
  {
    public enum Type
    {
      Person,
      Woman,
      Man,
      Mother,
      Father,
      Parent,
      Daughter,
      Son,
      Child,
      Spouse
    }

    [DataMember]
    public Type type;
    [DataMember]
    public string personXref;

    public Relation(Type type, string personXref)
    {
      this.type = type;
      this.personXref = personXref;
    }

    public override string ToString()
    {
      return this.type + ":" + this.personXref;
    }
    public string ToString(FamilyTreeStoreBaseClass familyTree = null, bool showRelation = true)
    {
      if (familyTree != null)
      {
        IndividualClass person = familyTree.GetIndividual(personXref);

        if(person != null)
        {
          //string str = this.type + ":" + personXref + "= " + person.GetName().ToString() + " (";
          string str = "";

          if (showRelation)
          {
            str += this.type + ":";
          }
          str += person.GetName().ToString() + " (";
          {
            IndividualEventClass ev = person.GetEvent(IndividualEventClass.EventType.Birth);
            if (ev != null)
            {
              FamilyDateTimeClass evDate = ev.GetDate();

              if (evDate != null)
              {
                str += evDate.ToString();
              }

            }
          }
          str += " - ";
          {
            IndividualEventClass ev = person.GetEvent(IndividualEventClass.EventType.Death);
            if (ev != null)
            {
              FamilyDateTimeClass evDate = ev.GetDate();

              if (evDate != null)
              {
                str += evDate.ToString();
              }

            }
          }
          str += ")";


          return str;
        }
      }
      return this.type + ":" + this.personXref + " (no tree data)";
    }
    static public Relation.Type GetChildRelation(IndividualClass person)
    {
      switch (person.GetSex())
      {
        case IndividualClass.IndividualSexType.Female:
          return Relation.Type.Daughter;
        case IndividualClass.IndividualSexType.Male:
          return Relation.Type.Son;
        default:
          return Relation.Type.Child;

      }
    }
    static public Relation.Type GetParentRelation(IndividualClass person)
    {
      switch (person.GetSex())
      {
        case IndividualClass.IndividualSexType.Female:
          return Relation.Type.Mother;
        case IndividualClass.IndividualSexType.Male:
          return Relation.Type.Father;
        default:
          return Relation.Type.Parent;

      }
    }
    static public Relation.Type GetSex(IndividualClass person)
    {
      switch (person.GetSex())
      {
        case IndividualClass.IndividualSexType.Female:
          return Relation.Type.Woman;
        case IndividualClass.IndividualSexType.Male:
          return Relation.Type.Man;
        default:
          return Relation.Type.Person;
      }
    }

  }

  [CollectionDataContract]
  public class RelationStack : List<Relation>
  {
    public string ToString(FamilyTreeStoreBaseClass familyTree = null)
    {
      string str = CalculateRelation(familyTree) + "\r\n";


      foreach (Relation relation in this)
      {
        str += "  " + relation.ToString(familyTree) + "\r\n";
      }
      return str;
    }

    public RelationStack Duplicate()
    {
      RelationStack stack = new RelationStack();

      foreach(Relation rel in this)
      {
        stack.Add(rel);
      }
      return stack;
    }
    public int Generations()
    {
      int gen = 0;
      foreach (Relation relation in this)
      {
        switch(relation.type)
        {
          case Relation.Type.Father:
          case Relation.Type.Mother:
          case Relation.Type.Parent:
            gen++;
            break;

          case Relation.Type.Son:
          case Relation.Type.Daughter:
          case Relation.Type.Child:
            gen--;
            break;
        }
      }
      return gen;
    }
    public string GetLast()
    {
      if (this.Count > 0)
      {
        return this[this.Count - 1].personXref;
      }
      return "";
    }
    public string GetFirst()
    {
      if (this.Count > 0)
      {
        return this[0].personXref;
      }
      return "";
    }
    public void RemoveLast()
    {
      if (this.Count > 0)
      {
        this.RemoveAt(this.Count - 1);
      }
    }
    public string CalculateRelation(FamilyTreeStoreBaseClass familyTree = null)
    {
      int rootIndex = 0;

      for( int i = 0; i < this.Count; i++)
      {
        switch(this[i].type)
        {
          case Relation.Type.Father:
          case Relation.Type.Mother:
          case Relation.Type.Parent:
            {
              rootIndex = i;              
            }
            break;
        }
      }

      int minGenerations = Math.Min(rootIndex, this.Count - rootIndex - 1);
      int maxGenerations = Math.Max(rootIndex, this.Count - rootIndex - 1);
      int diffGenerations = maxGenerations - minGenerations;
      bool directAncestor = false;
      string str = "";

      switch (minGenerations)
      {
        case 0:
          if (minGenerations == maxGenerations)
          {
            str = "the same person";
          }
          else
          {
            //str = "Direct ancestor";
            directAncestor = true;
          }
          break;
        case 1:
          if (diffGenerations == 0)
          {
            str = "sibling";
          }
          else
          {
            if (rootIndex == 1)
            {
              switch (this[Count - 1].type)
              {
                case Relation.Type.Woman:
                case Relation.Type.Mother:
                case Relation.Type.Daughter:
                  str = "niece";
                  break;
                case Relation.Type.Man:
                case Relation.Type.Father:
                case Relation.Type.Son:
                case Relation.Type.Parent:
                  str = "nephew";
                  break;
                default:
                  str = "error:";
                  break;
              }

            }
            else if (rootIndex == (Count - 2))
            {
              switch (this[0].type)
              {
                case Relation.Type.Mother:
                case Relation.Type.Woman:
                case Relation.Type.Daughter:
                  str = "aunt";
                  break;
                case Relation.Type.Man:
                case Relation.Type.Father:
                case Relation.Type.Son:
                case Relation.Type.Parent:
                  str = "uncle";
                  break;
                default:
                  str = "error:";
                  break;
              }

            }
            else
            {
              str = "error:";
            }
            diffGenerations--;

          }
          break;
        case 2:
          str = "cousin";
          break;
        case 3:
          str = "second cousin";
          break;
        case 4:
          str = "third cousin";
          break;
        case 5:
          str = "fourth cousin";
          break;
        default:
          str = (minGenerations - 1) + "-th cousin";
          break;
      }
      if (diffGenerations != 0)
      {
        switch (diffGenerations)
        {
          case 1:
            str += " once removed";
            break;
          case 2:
            str += " twice removed";
            break;
          default:
            str += " " + diffGenerations + " generations removed";
            break;
        }
      }
      if (familyTree != null)
      {
        if (!directAncestor)
        {
          str += " with common ancestor: " + this[rootIndex].ToString(familyTree, false);
        }
        else
        {
          str += " direct descendant from " + this[rootIndex].ToString(familyTree, false);
        }
      }
      return str;
    }

  }

  [DataContract]
  public class RelationStackList
  {
    [DataMember]
    public string sourceTree;
    [DataMember]
    public List<RelationStack> relations;
    [DataMember]
    public DateTime time;

    public RelationStackList()
    {
      relations = new List<RelationStack>();
    }

    public string ToString(FamilyTreeStoreBaseClass familyTree = null)
    {
      string str = "";

      if(sourceTree != null)
      {
        str += sourceTree;
      }
      str += "\r\n";
      if (time != null)
      {
        str += time.ToString();
      }
      str += "\r\n";

      foreach(RelationStack rel in relations)
      {
        str += rel.ToString(familyTree);
      }

      return str;
    }
  }


  [DataContract]
  public class AncestorLineInfo
  {
    private static TraceSource trace = new TraceSource("AncestorLineInfo", SourceLevels.Warning);

    [DataMember]
    public int depth;
    [DataMember]
    public string rootAncestor;
    [DataMember]
    public string details;
    [DataMember]
    public RelationStack relationPath;

    public AncestorLineInfo(string xref, RelationStack relationStack, int depth, string detailString)
    {
      this.depth = depth;
      this.rootAncestor = xref;
      this.details = detailString;
      this.relationPath = relationStack.Duplicate();

      // Depth no longer the same as number of generations
      /*if (depth != relationStack.Generations())
      {
        trace.TraceEvent(TraceEventType.Error, 0, "Error: Generation depth mismatch: " + depth + " " + relationStack.Count + " = " + relationStack.Generations());
      }*/
      if(relationStack.GetLast() != xref)
      {
        trace.TraceEvent(TraceEventType.Error, 0, "Error: Last person mismatch: " + relationStack.GetLast() + "!=" + xref + "; " + relationStack.Generations());
        trace.TraceEvent(TraceEventType.Error, 0, relationStack.ToString());
      }
    }
  }

  [DataContract]
  class CompletenessList
  {
    [DataMember]
    public string baseFileName;
    [DataMember]
    public IList<AncestorLineInfo> limitList;


    public CompletenessList(string filename)
    {
      baseFileName = filename;
      limitList = new List<AncestorLineInfo>();
    }

  }

  [DataContract]
  public class HandledItem
  {
    private static TraceSource trace = new TraceSource("Sanity:HandledItem", SourceLevels.Warning);
    [DataMember]
    public string xref;
    [DataMember]
    public int number;
    [DataMember]
    public IList<RelationStack> relationStackList;

    public HandledItem(string xref = null, RelationStack relationStack = null)
    {
      this.xref = xref;
      number = 1;
      this.relationStackList = new List<RelationStack>();
      this.relationStackList.Add(relationStack);
    }
    public void Add(RelationStack relationStack)
    {
      trace.TraceInformation("Add to existing root person:" + number);
      //bool duplicateExists = false;
      foreach(RelationStack stack in relationStackList)
      {
        bool duplicate = false;
        if (stack.Count == relationStack.Count)
        {
          duplicate = true;
          for (int i = 0; (i < stack.Count) && duplicate; i++)
          {
            if (stack[i].personXref != relationStack[i].personXref)
            {
              duplicate = false;
            }
          }
        }
        if(duplicate)
        {
          //duplicateExists = true;
          //trace.TraceEvent(TraceEventType.Error, 0, "Error: Add to existing root person:Duplication!");
          //trace.TraceEvent(TraceEventType.Error, 0, stack.ToString());
          //trace.TraceEvent(TraceEventType.Error, 0, relationStack.ToString());
          // Just ignore duplicates for now
          return;
        }
      }
      this.relationStackList.Add(relationStack);
      foreach (RelationStack stack in relationStackList)
      {
        trace.TraceInformation(stack.ToString());
      }
      number++;
    }
  }

  [DataContract]
  public class AncestorStatistics
  {
    private static TraceSource trace = new TraceSource("Sanity:AncestorStatistics", SourceLevels.Warning);
    [DataMember]
    private IList<AncestorLineInfo> ancestorList;
    private FamilyTreeStoreBaseClass familyTree;
    [DataMember]
    private int people, duplicatePeople;
    [DataMember]
    private int families, duplicateFamilies;
    [DataMember]
    private IList<string> analysedFamilies;
    [DataMember]
    private IList<string> analysedPeople;
    [DataMember]
    private IList<HandledItem> analysedFamiliesNo;
    [DataMember]
    private IList<HandledItem> analysedPeopleNo;
    [DataMember]
    private int descendantGenerationNo;
    //private SearchMode mode;
    //private double progress;
    private DateTime startTime;
    /*private TimeSpan oldestParent;
    private TimeSpan youngestParent;
    private TimeSpan youngestAtEvent;
    private TimeSpan oldestAtEvent;*/
    //private int maxNoOfChildren;
    //private int maxNoOfParents;
    [DataMember]
    private SanityCheckLimits limits;
    [DataMember]
    private int ancestorGenerationNo;
    private ProgressReporter progressReporter;
    private double latestPercent;

    public const int AllGenerations = 1000;

    public AncestorStatistics(FamilyTreeStoreBaseClass familyTree, SanityCheckLimits limits, int ancestorGenerations = AllGenerations, int descendantGenerations = 0, ProgressReporter progressReporter = null)
    {
      this.familyTree = familyTree;
      this.descendantGenerationNo = descendantGenerations;
      //this.mode = mode;
      ancestorGenerationNo = ancestorGenerations;

      analysedFamilies = new List<string>();
      analysedPeople = new List<string>();
      analysedFamiliesNo = new List<HandledItem>();
      analysedPeopleNo = new List<HandledItem>();
      this.progressReporter = progressReporter;

      this.limits = limits;
      latestPercent = 0.0;

      ancestorList = new List<AncestorLineInfo>();

      people = 0;
      families = 0;
      duplicatePeople = 0;
      duplicateFamilies = 0;
      //progress = 0.0;
      startTime = DateTime.Now;
      //maxNoOfChildren = 0;
      //maxNoOfParents = 0;
      /*youngestParent = TimeSpan.FromDays(100000);
      youngestAtEvent= TimeSpan.FromDays(100000);
      oldestParent = TimeSpan.FromDays(0);
      oldestAtEvent = TimeSpan.FromDays(0);*/


    }

    /*public SearchMode GetMode()
    {
      return mode;
    }*/

    public int GetAncestorGenerationNo()
    {
      return ancestorGenerationNo;
    }
    public int GetDescendantGenerationNo()
    {
      return descendantGenerationNo;
    }

    public FamilyTreeStoreBaseClass GetFamilyTree()
    {
      return familyTree;
    }
    public void SetFamilyTree(FamilyTreeStoreBaseClass tree)
    {
      familyTree = tree;
    }

    private int ToYears(TimeSpan difference)
    {
      return (int)(difference.Days / 365.25);
    }
    private int ToMonths(TimeSpan difference)
    {
      return (int)(difference.Days / (365.25 / 12));
    }

    bool IsInList(string person)
    {

      foreach (AncestorLineInfo info in ancestorList)
      {
        if (info.rootAncestor == person)
        {
          return true;
        }
      }
      return false;
    }

    public void AddToList(string rootAncestor, RelationStack relationStack, int depth, string description = null)
    {
      trace.TraceInformation("AddToList(" + rootAncestor + "," + depth + "," + description + "," + relationStack.Count + ")");
      trace.TraceInformation(relationStack.ToString(familyTree));

      foreach(AncestorLineInfo info in ancestorList)
      {
        if(info.rootAncestor == rootAncestor)
        {
          if(info.details.IndexOf(description) < 0)
          {
            info.details += "; " + description;
          }
          return;
        }
      }
      /*if(ancestorList.Count > 0)
      {
        if(ancestorList[ancestorList.Count - 1].rootAncestor == rootAncestor)
        {
          ancestorList[ancestorList.Count - 1].details += "; " + description;
          return;
        }
      }*/
      ancestorList.Add(new AncestorLineInfo(rootAncestor, relationStack, depth, description));
    }

    private void SanityCheckIndividual(IndividualClass person, RelationStack relationStack, int depth)
    {
      IList<IndividualEventClass> evList = person.GetEventList();
      IndividualEventClass birth = person.GetEvent(IndividualEventClass.EventType.Birth);
      IndividualEventClass death = person.GetEvent(IndividualEventClass.EventType.Death);

      if ((birth != null) && (birth.GetDate().GetDateType() != FamilyDateTimeClass.FamilyDateType.Unknown))
      {
        foreach (IndividualEventClass ev in evList)
        {
          if ((ev.GetDate().GetDateType() != FamilyDateTimeClass.FamilyDateType.Unknown) &&
              (ev.GetDate().GetDateType() != FamilyDateTimeClass.FamilyDateType.DateString) &&
              (ev.GetEventType() != IndividualEventClass.EventType.Birth) &&
              (ev.GetEventType() != IndividualEventClass.EventType.RecordUpdate))
          {
            if (limits.eventLimitMin.active &&
                (ToYears(ev.GetDate().ToDateTime() - birth.GetDate().ToDateTime()) < limits.eventLimitMin.value) &&
                (ev.GetEventType() != IndividualEventClass.EventType.Baptism))
            {
              //youngestAtEvent = ev.GetDate().ToDateTime() - birth.GetDate().ToDateTime();
              trace.TraceInformation(" Young person at event " + person.GetXrefName() + ":" + person.GetName() + ", b:" + birth.GetDate() + " " + person.GetEvent(IndividualEventClass.EventType.Birth) + " : " + ev.GetEventType() + " " + ev.GetDate());
              AddToList(person.GetXrefName(), relationStack, depth, "Young at event, born " + birth.GetDate() + " event" + ev.GetEventType() + " " + ev.GetDate());
            }
            if (limits.eventLimitMax.active && ToYears(ev.GetDate().ToDateTime() - birth.GetDate().ToDateTime()) > limits.eventLimitMax.value)
            {
              //oldestAtEvent = ev.GetDate().ToDateTime() - birth.GetDate().ToDateTime();
              trace.TraceInformation(" Old person at event " + person.GetXrefName() + ":" + person.GetName() + ", b:" + person.GetEvent(IndividualEventClass.EventType.Birth) + " : " + ev.GetEventType() + ev.GetDate());
              AddToList(person.GetXrefName(), relationStack, depth, "Old at event, born " + birth.GetDate() + " event" + ev.GetEventType() + " " + ev.GetDate());
            }
          }

        }
        string birthDate = null;
        string deathDate = null;

        if (birth != null)
        {
          switch (birth.GetDate().GetDateType())
          {
            case FamilyDateTimeClass.FamilyDateType.Unknown:
              if (limits.unknownBirthDeath.active)
              {
                birthDate = "unknown";
              }
              break;
            case FamilyDateTimeClass.FamilyDateType.Year:
            case FamilyDateTimeClass.FamilyDateType.YearMonth:
            case FamilyDateTimeClass.FamilyDateType.DateString:
              if (limits.inexactBirthDeath.active)
              {
                birthDate = "inexact";
              }
              break;
            default:
              break;
          }
        }
        else
        {
          if (limits.unknownBirthDeath.active)
          {
            birthDate = "unknown";
          }
        }

        if (birth == null || (DateTime.Now > birth.GetDate().ToDateTime().AddYears(100)))
        {
          if (death != null)
          {
            switch (death.GetDate().GetDateType())
            {
              case FamilyDateTimeClass.FamilyDateType.Unknown:
                if (limits.unknownBirthDeath.active)
                {
                  deathDate = "unknown";
                }
                break;
              case FamilyDateTimeClass.FamilyDateType.Year:
              case FamilyDateTimeClass.FamilyDateType.YearMonth:
              case FamilyDateTimeClass.FamilyDateType.DateString:
                if (limits.inexactBirthDeath.active)
                {
                  deathDate = "inexact";
                }
                break;
              default:
                break;
            }
          }
          else if (!person.GetIsAlive())
          {
            if (limits.unknownBirthDeath.active)
            {
              deathDate = "unknown";
            }
          }
        }

        if (birthDate != null)
        {
          if (deathDate != null)
          {
            AddToList(person.GetXrefName(), relationStack, depth, birthDate + "  birth date and " + deathDate + " death date");
          }
          else
          {
            AddToList(person.GetXrefName(), relationStack, depth, birthDate + "  birth date");
          }
        }
        else
        {
          if (deathDate != null)
          {
            AddToList(person.GetXrefName(), relationStack, depth, deathDate + "  death date");
          }
        }

      }
    }

    public bool AnalysePerson(string xref, RelationStack relationStack)
    {
      if (analysedPeople.Contains(xref))
      {
        trace.TraceInformation("  analyse " + xref + " done");
        duplicatePeople++;
        analysedPeopleNo[analysedPeople.IndexOf(xref)].Add(relationStack);

        if (descendantGenerationNo == 0)
        {
          return false;
        }
        //return true; // With the current implementation we will check people twice, but we need to do so to be able to check both ancestors and descendants.
        return false;
      }
      this.people++;
      analysedPeople.Add(xref);
      analysedPeopleNo.Add(new HandledItem(xref, relationStack));
      trace.TraceInformation("  analysed " + people + " people");
      return true;
    }
    public class ParentInfo
    {
      public DateTime birth;
      public DateTime death;
      public IndividualClass parent;

      public ParentInfo()
      {
        birth = DateTime.MinValue;
        death = DateTime.MinValue;
        parent = null;
      }
    }

    public void SanityCheckFamily(FamilyClass family, RelationStack relationStack, int depth)
    {
      //DateTime oldestParentBirth = DateTime.MaxValue;
      //DateTime youngestParentBirth = DateTime.MinValue;
      /*DateTime motherBirth = DateTime.MinValue;
      DateTime motherDeath = DateTime.MinValue;
      DateTime fatherBirth = DateTime.MinValue;
      IndividualClass mother = null;
      IndividualClass father = null;*/
      ParentInfo mother = new ParentInfo();
      ParentInfo father = new ParentInfo();

      IList<IndividualXrefClass> parentList = family.GetParentList();
      if (parentList != null)
      {
        foreach (IndividualXrefClass parentXref in parentList)
        {
          IndividualClass parent = familyTree.GetIndividual(parentXref.GetXrefName());
          if (parent != null)
          {
            IndividualEventClass birth = parent.GetEvent(IndividualEventClass.EventType.Birth);
            IndividualEventClass death = parent.GetEvent(IndividualEventClass.EventType.Death);

            if ((birth != null) && birth.GetDate().ValidDate())
            {
              if (parent.GetSex() == IndividualClass.IndividualSexType.Female)
              {
                if (mother.birth != DateTime.MinValue)
                {
                  RelationStack stack = relationStack.Duplicate();
                  stack.Add(new Relation(Relation.Type.Mother, parent.GetXrefName()));
                  AddToList(parent.GetXrefName(), stack, depth + 1, "More than one mother in family " + parent.GetName() + ",b:" + birth.GetDate());
                }
                mother.birth = birth.GetDate().ToDateTime();
                mother.parent = parent;
                if ((death != null) && death.GetDate().ValidDate())
                {
                  mother.death = death.GetDate().ToDateTime();
                }
              }
              else if (parent.GetSex() == IndividualClass.IndividualSexType.Male)
              {
                if (father.birth != DateTime.MinValue)
                {
                  RelationStack stack = relationStack.Duplicate();
                  stack.Add(new Relation(Relation.Type.Father, parent.GetXrefName()));
                  AddToList(parent.GetXrefName(), stack, depth + 1, "More than one father in family " + parent.GetName() + ",b:" + birth.GetDate());
                }
                father.birth = birth.GetDate().ToDateTime();
                father.parent = parent;
                if ((death != null) && death.GetDate().ValidDate())
                {
                  father.death = death.GetDate().ToDateTime();
                }
              }
            }

          }
        }
        /*if (maxNoOfParents < parentList.Count)
        {
          maxNoOfParents = parentList.Count;
        }*/
        IList<IndividualXrefClass> childList = family.GetChildList();
        if (childList != null)
        {
          if (mother.parent != null)
          {
            if (limits.noOfChildrenMax.active && (limits.noOfChildrenMax.value < childList.Count))
            {
              //maxNoOfChildren = childList.Count;
              RelationStack stack = relationStack.Duplicate();
              if (stack.GetLast() != mother.parent.GetXrefName())
              {
                stack.Add(new Relation(Relation.Type.Mother, mother.parent.GetXrefName()));
              }
              AddToList(mother.parent.GetXrefName(), stack, depth + 1, "Mother with many children " + childList.Count);

            }
            if (limits.noOfChildrenMin.active && (limits.noOfChildrenMin.value >= childList.Count))
            {
              //maxNoOfChildren = childList.Count;
              RelationStack stack = relationStack.Duplicate();
              if (stack.GetLast() != mother.parent.GetXrefName())
              {
                stack.Add(new Relation(Relation.Type.Mother, mother.parent.GetXrefName()));
              }
              AddToList(mother.parent.GetXrefName(), stack, depth + 1, "Mother with few children " + childList.Count);

            }
          }
          if (mother.birth != DateTime.MinValue)
          {
            IList<DateTime> birthDateList = new List<DateTime>();
            foreach (IndividualXrefClass childXref in childList)
            {
              IndividualClass child = familyTree.GetIndividual(childXref.GetXrefName());

              if (child != null)
              {
                IndividualEventClass birth = child.GetEvent(IndividualEventClass.EventType.Birth);

                if (birth != null)
                {
                  if (birth.GetDate().ValidDate())
                  {
                    // Only compare those where we know at least birth month for close births...
                    if (birth.GetDate().GetDateType() != FamilyDateTimeClass.FamilyDateType.Year)
                    {
                      birthDateList.Add(birth.GetDate().ToDateTime());
                    }
                    if ((mother.death != DateTime.MinValue) && (ToYears(mother.death - birth.GetDate().ToDateTime()) < 0))
                    {
                      //youngestParent = age;
                      trace.TraceInformation(" Mother dead when child " + child.GetXrefName() + ":" + child.GetName() + birth.GetDate() + " was born at " + ToYears(birth.GetDate().ToDateTime() - mother.birth) + " years");
                      RelationStack stack = relationStack.Duplicate();
                      stack.Add(new Relation(Relation.Type.Mother, mother.parent.GetXrefName()));
                      stack.Add(new Relation(Relation.Type.Child, child.GetXrefName()));
                      AddToList(child.GetXrefName(), stack, depth, "Mother died " + ToYears(birth.GetDate().ToDateTime() - mother.death) + " years before birth.");
                    }
                    else if (limits.motherLimitMax.active && (mother.birth != DateTime.MinValue) && (ToYears(birth.GetDate().ToDateTime() - mother.birth) > limits.motherLimitMax.value))
                    {
                      //oldestParent = age;
                      trace.TraceInformation(" Old mother to child " + child.GetXrefName() + ":" + child.GetName() + " born " + birth.GetDate() + " mother born " + mother.birth.ToShortDateString());
                      //sanity.OldParent = true;
                      RelationStack stack = relationStack.Duplicate();
                      stack.Add(new Relation(Relation.Type.Mother, mother.parent.GetXrefName()));
                      stack.Add(new Relation(Relation.Type.Child, child.GetXrefName()));
                      AddToList(child.GetXrefName(), stack, depth, "Old mother: " + ToYears(birth.GetDate().ToDateTime() - mother.birth) + " years at birth");

                    }
                    if (limits.parentLimitMin.active && (mother.birth != DateTime.MinValue) && (ToYears(birth.GetDate().ToDateTime() - mother.birth) < 0))
                    {
                      //youngestParent = age;
                      trace.TraceInformation(" Mother younger than child " + child.GetXrefName() + ":" + child.GetName() + birth.GetDate() + " at " + ToYears(birth.GetDate().ToDateTime() - mother.birth) + " years");
                      RelationStack stack = relationStack.Duplicate();
                      stack.Add(new Relation(Relation.Type.Mother, mother.parent.GetXrefName()));
                      stack.Add(new Relation(Relation.Type.Child, child.GetXrefName()));
                      AddToList(child.GetXrefName(), stack, depth, "Child born " + ToYears(mother.birth - birth.GetDate().ToDateTime()) + " years before mother");
                    }
                    else if (limits.parentLimitMin.active && (mother.birth != DateTime.MinValue) && (ToYears(birth.GetDate().ToDateTime() - mother.birth) < limits.parentLimitMin.value))
                    {
                      //youngestParent = age;
                      trace.TraceInformation(" Young mother to child " + child.GetXrefName() + ":" + child.GetName() + birth.GetDate() + " at " + ToYears(birth.GetDate().ToDateTime() - mother.birth) + " years");
                      RelationStack stack = relationStack.Duplicate();
                      stack.Add(new Relation(Relation.Type.Mother, mother.parent.GetXrefName()));
                      stack.Add(new Relation(Relation.Type.Child, child.GetXrefName()));
                      AddToList(child.GetXrefName(), stack, depth, "Young mother: " + ToYears(birth.GetDate().ToDateTime() - mother.birth) + " years at birth");
                    }
                    if ((father.death != DateTime.MinValue) && (ToMonths(father.death - birth.GetDate().ToDateTime()) < -8))
                    {
                      //youngestParent = age;
                      trace.TraceInformation(" Father dead when child " + child.GetXrefName() + ":" + child.GetName() + birth.GetDate() + " was born at " + ToMonths(birth.GetDate().ToDateTime() - father.death) + " months");
                      RelationStack stack = relationStack.Duplicate();
                      stack.Add(new Relation(Relation.Type.Mother, mother.parent.GetXrefName()));
                      stack.Add(new Relation(Relation.Type.Child, child.GetXrefName()));
                      //AddToList(child.GetXrefName(), stack, depth, "Father died at " + father.parent.GetEvent(IndividualEventClass.EventType.Death).ToString(false));
                      AddToList(child.GetXrefName(), stack, depth, "Father died " + ToMonths(birth.GetDate().ToDateTime() - father.death) + " months before birth.");
                    }
                    else if (limits.fatherLimitMax.active && (father.birth != DateTime.MinValue) && (ToYears(birth.GetDate().ToDateTime() - father.birth) > limits.fatherLimitMax.value))
                    {
                      //oldestParent = age;
                      trace.TraceInformation(" Old father to child " + child.GetXrefName() + ":" + child.GetName() + birth.GetDate() + " = " + father.birth.ToShortDateString() + " parent birth:");
                      //sanity.OldParent = true;
                      RelationStack stack = relationStack.Duplicate();
                      stack.Add(new Relation(Relation.Type.Mother, mother.parent.GetXrefName()));
                      stack.Add(new Relation(Relation.Type.Child, child.GetXrefName()));
                      AddToList(child.GetXrefName(), stack, depth, "Old father: " + ToYears(birth.GetDate().ToDateTime() - father.birth) + " years at birth.");

                    }
                    if (limits.parentLimitMin.active && (father.birth != DateTime.MinValue) && (ToYears(birth.GetDate().ToDateTime() - father.birth) < 0))
                    {
                      //youngestParent = age;
                      trace.TraceInformation("Father younger than child " + child.GetXrefName() + ":" + child.GetName() + birth.GetDate() + " at " + ToYears(birth.GetDate().ToDateTime() - father.birth) + " years");
                      RelationStack stack = relationStack.Duplicate();
                      stack.Add(new Relation(Relation.Type.Mother, mother.parent.GetXrefName()));
                      stack.Add(new Relation(Relation.Type.Child, child.GetXrefName()));
                      AddToList(child.GetXrefName(), stack, depth, "Child born " + ToYears(father.birth - birth.GetDate().ToDateTime()) + " years before father");
                    }
                    else if (limits.parentLimitMin.active && (father.birth != DateTime.MinValue) && (ToYears(birth.GetDate().ToDateTime() - father.birth) < limits.parentLimitMin.value))
                    {
                      //youngestParent = age;
                      trace.TraceInformation(" Young father to child " + child.GetXrefName() + ":" + child.GetName() + birth.GetDate() + " at " + ToYears(birth.GetDate().ToDateTime() - father.birth) + " years");
                      RelationStack stack = relationStack.Duplicate();
                      stack.Add(new Relation(Relation.Type.Mother, mother.parent.GetXrefName()));
                      stack.Add(new Relation(Relation.Type.Child, child.GetXrefName()));
                      AddToList(child.GetXrefName(), stack, depth, "Young father: " + ToYears(birth.GetDate().ToDateTime() - father.birth) + " years at birth");
                    }
                  }
                }
              }
            }
            //birthDateList.OrderBy((d1, d2) => DateTime.Compare(d1, d2));
            //ArrayList sorted = new ArrayList(birthDateList);
            ArrayList.Adapter((IList)birthDateList).Sort();

            DateTime lastBirth = DateTime.MinValue;
            foreach (DateTime birth in birthDateList)
            {
              if (lastBirth != DateTime.MinValue)
              {
                if (birth.Subtract(lastBirth).Days == 0)
                {
                  if (limits.twins.active)
                  {
                    RelationStack stack = relationStack.Duplicate();
                    stack.Add(new Relation(Relation.Type.Mother, mother.parent.GetXrefName()));
                    //stack.RemoveLast();
                    AddToList(mother.parent.GetXrefName(), stack, depth + 1, "Twins born at " + birth);
                  }
                }
                else if (limits.daysBetweenChildren.active && (birth.Subtract(lastBirth).Days < limits.daysBetweenChildren.value))
                {
                  RelationStack stack = relationStack.Duplicate();
                  stack.Add(new Relation(Relation.Type.Mother, mother.parent.GetXrefName()));
                  //stack.RemoveLast();
                  
                  AddToList(mother.parent.GetXrefName(), stack, depth + 1, "Close children found: Only " + birth.Subtract(lastBirth).Days + " days in between");
                }

              }
              lastBirth = birth;
            }
            //sorted.Sort();
          }
        }
      }
    }
    public bool AnalyseFamily(string xref)
    {
      if (analysedFamilies.Contains(xref))
      {
        trace.TraceInformation("  analyse " + xref + " done");
        duplicateFamilies++;
        analysedFamiliesNo[analysedFamilies.IndexOf(xref)].number++;

        if (descendantGenerationNo == 0)
        {
          return false;
        }
        return true;// With the current implementation we will check families twice, but we need to do so to be able to check both ancestors and descendants.
      }
      this.families++;
      analysedFamilies.Add(xref);
      analysedFamiliesNo.Add(new HandledItem(xref));
      trace.TraceInformation("  analysed " + families + " families");
      return true;
    }



    private int CountParents(IList<FamilyXrefClass> parentFamilyList)
    {
      int parentNo = 0;

      foreach (FamilyXrefClass xref in parentFamilyList)
      {
        FamilyClass family = familyTree.GetFamily(xref.GetXrefName());
        if (family != null)
        {
          IList<IndividualXrefClass> parentXrefList = family.GetParentList();
          if (parentXrefList != null)
          {
            parentNo += parentXrefList.Count;
          }
          else
          {
            trace.TraceEvent(TraceEventType.Information, 0, "Error: Family list " + xref.ToString() + " empty!");
          }
        }
        else
        {
          trace.TraceInformation("Error: Family " + xref.ToString() + " not found!");
        }

      }
      return parentNo;

    }

    /*public bool CheckIfRootPerson(IndividualClass person)
    {
      IList<FamilyXrefClass> parentFamilyList = person.GetFamilyChildList();

      if ((parentFamilyList == null) || (parentFamilyList.Count == 0))
      {
        return true;
      }
      if (CountParents(parentFamilyList) < 1)
      {
        return true;
      }
      return false;
    }*/

    public int NumberOfParents(IndividualClass person)
    {
      IList<FamilyXrefClass> parentFamilyList = person.GetFamilyChildList();

      if ((parentFamilyList == null) || (parentFamilyList.Count == 0))
      {
        return 0;
      }
      return CountParents(parentFamilyList);
    }



    public void AnalyseAncestors(IndividualClass person, int depth = 0, double progress = 0.0, RelationStack relationStack = null, Relation.Type relation = Relation.Type.Person)
    {
      trace.TraceInformation("AnalyseAncestors(" + person.GetName() + ")");

      if (relationStack == null)
      {
        relationStack = new RelationStack();
        relationStack.Add(new Relation(Relation.GetSex(person), person.GetXrefName()));
      }
      else
      {
        relationStack.Add(new Relation(relation, person.GetXrefName()));
      }

      if (AnalysePerson(person.GetXrefName(), relationStack))
      {

        SanityCheckIndividual(person, relationStack, depth);

        int noOfParents = NumberOfParents(person);
        if (noOfParents < 2)
        {
          if (limits.parentsMissing.active)
          {
            if (noOfParents == 0)
            {
              AddToList(person.GetXrefName().ToString(), relationStack, depth, "No parents");
            }
            else
            {
              AddToList(person.GetXrefName().ToString(), relationStack, depth, "Only one parent");
            }
          }
        }
        if (limits.parentsProblem.active)
        {
          if (noOfParents > 2)
          {
            AddToList(person.GetXrefName().ToString(), relationStack, depth, noOfParents + " parents");
          }
          IList<FamilyXrefClass> childFamilies = person.GetFamilyChildList();
          if(childFamilies != null)
          {
            if(childFamilies.Count > 1)
            {
              AddToList(person.GetXrefName().ToString(), relationStack, depth, "Child in " + childFamilies.Count + " families");
            }
          }
        }
        if (depth < ancestorGenerationNo)
        {
          IList<FamilyXrefClass> parentFamilyList = person.GetFamilyChildList();

          if (parentFamilyList != null)
          {
            int familyNo = 1;
            foreach (FamilyXrefClass familyXref in parentFamilyList)
            {
              if (AnalyseFamily(familyXref.GetXrefName()))
              {
                FamilyClass family = familyTree.GetFamily(familyXref.GetXrefName());

                if (family != null)
                {
                  IList<IndividualXrefClass> parentXrefList = family.GetParentList();

                  SanityCheckFamily(family, relationStack, depth);

                  if (parentXrefList != null)
                  {
                    int parentNo = 0;
                    foreach (IndividualXrefClass parentXref in parentXrefList)
                    {
                      IndividualClass parent = familyTree.GetIndividual(parentXref.GetXrefName());
                      if (parent != null)
                      {
                        double levelAddition = Math.Pow(2, -depth);
                        double progressPercent;

                        trace.TraceInformation("progress = " + progress.ToString() + " gen:" + depth + " => progress: " + progress + " + levelAdd:" + levelAddition + " * (famNo:" + familyNo + " / famCnt:" + parentFamilyList.Count + ") * (parNo:" + parentNo + " / parCnt:" + parentXrefList.Count + ") = " + (double)(progress + levelAddition * ((double)familyNo / parentFamilyList.Count) * ((double)parentNo / parentXrefList.Count)) + " " + parentXref.GetXrefName() + "=" + parent.GetName().ToString());
                        progressPercent = progress + levelAddition * ((double)familyNo / parentFamilyList.Count) * ((double)parentNo / parentXrefList.Count);
                        if (latestPercent > progressPercent)
                        {
                          trace.TraceData(TraceEventType.Warning, 0, "Progress = backwards!!! " + progressPercent.ToString("P2") + "<" + latestPercent.ToString("P2"));
                        }
                        latestPercent = progressPercent;
                        trace.TraceInformation("Progress = " + progressPercent.ToString("P2"));
                        if (progressReporter != null)
                        {
                          progressReporter.ReportProgress(progressPercent * 100.0, "Analyzing: " + this.people + " people and " + families + " families. Found " + ancestorList.Count + " problems...");
                        }
                        RelationStack stack2 = relationStack.Duplicate();

                        AnalyseAncestors(parent, depth + 1, progress + levelAddition * ((double)familyNo / parentFamilyList.Count) * ((double)parentNo / parentXrefList.Count), stack2, Relation.GetParentRelation(parent));
                      }
                      else
                      {
                        trace.TraceEvent(TraceEventType.Error, 0, "Error person " + parentXref.GetXrefName() + " not found in database!");
                      }
                      parentNo++;
                    }
                  }
                  else
                  {
                    //trace.TraceEvent(TraceEventType.Information, 0, "family.GetParentList() " + familyXref.ToString() + " is null!");
                  }
                }
                else
                {
                  trace.TraceEvent(TraceEventType.Error, 0, "Error family " + familyXref.ToString() + " not found in database!");
                }

              }
              familyNo++;
            }

          }
        }
        else
        {
          if(limits.generationlimited.active)
          {
            AddToList(person.GetXrefName().ToString(), relationStack, depth, "Max depth, " + NumberOfParents(person) + " parents");
          }
        }
      }
      AnalyseDescendants(person, descendantGenerationNo - 1, depth, progress, relationStack, Relation.Type.Person);

    }

    public void AnalyseDescendants(IndividualClass person, int descendantDepth, int depth, double progress, RelationStack relationStack, Relation.Type relation)
    {
      trace.TraceInformation("AnalyseDescendants(" + person.GetName() + ")");
      if (relation != Relation.Type.Person)
      {
        relationStack.Add(new Relation(relation, person.GetXrefName()));
      }

      if (descendantDepth > 0)
      {
        //if (AnalysePerson(person.GetXrefName(), relationStack))
        {
          if (AnalysePerson(person.GetXrefName(), relationStack))
          {
            SanityCheckIndividual(person, relationStack, depth);
          }

          IList<FamilyXrefClass> spouseList = person.GetFamilySpouseList();

          if (spouseList != null)
          {
            //int familyNo = 1;
            foreach (FamilyXrefClass familyXref in spouseList)
            {
              trace.TraceInformation(" Descendant:person (" + person.GetName() + ") family " + familyXref.ToString());
              if (AnalyseFamily(familyXref.GetXrefName()))
              {
                FamilyClass family = familyTree.GetFamily(familyXref.GetXrefName());

                if (family != null)
                {
                  IList<IndividualXrefClass> childXrefList = family.GetChildList();

                  SanityCheckFamily(family, relationStack, depth);

                  if (descendantDepth > 0)
                  {
                    if (childXrefList != null)
                    {
                      int parentNo = 0;
                      foreach (IndividualXrefClass childXref in childXrefList)
                      {
                        IndividualClass child = familyTree.GetIndividual(childXref.GetXrefName());
                        if (child != null)
                        {
                          trace.TraceInformation(" Descendant:person (" + person.GetName() + ") child " + child.GetName());

                          trace.TraceInformation("Progress = " + progress.ToString("P2"));
                          if (progressReporter != null)
                          {
                            progressReporter.ReportProgress(latestPercent * 100.0, "Analyzing: " + this.people + " people and " + families + " families. Found " + ancestorList.Count + " problems...");
                          }
                          RelationStack stack2 = relationStack.Duplicate();

                          AnalyseDescendants(child, descendantDepth - 1, depth + 1, progress, stack2, Relation.GetChildRelation(child));
                        }
                        else
                        {
                          trace.TraceEvent(TraceEventType.Error, 0, "Error person " + childXref.GetXrefName() + " not found in database!");
                        }
                        parentNo++;
                      }
                    }
                  }
                  else
                  {
                    //trace.TraceEvent(TraceEventType.Information, 0, "family.GetChildList() " + familyXref.ToString() + " is null!");
                  }
                }
                else
                {
                  trace.TraceEvent(TraceEventType.Error, 0, "Error family " + familyXref.ToString() + " not found in database!");
                }

              }
              else
              {
                trace.TraceInformation(" person (" + person.GetName() + ") analysefamily = false");
              }
              //familyNo++;
            }


          }
          else
          {
            trace.TraceInformation(" person (" + person.GetName() + ") spouse family = null");
          }
        }
      }

    }

    public void Print()
    {
      //ancestorList.OrderBy<int, depth>();

      trace.TraceInformation("Analysis started at " + startTime + " done at " + DateTime.Now);
      trace.TraceInformation("Ancestor overview:");
      trace.TraceInformation("  analysed " + people + " people   " + duplicatePeople + " more than once");
      trace.TraceInformation("  analysed " + families + " families " + duplicateFamilies + " more than once");
      trace.TraceInformation("  roots    " + ancestorList.Count);
      //trace.TraceInformation("  max children: " + maxNoOfChildren+ " parents " + maxNoOfParents);
      familyTree.Print();

      {
        IEnumerable<AncestorLineInfo> query = ancestorList.OrderBy(ancestor => ancestor.depth);

        trace.TraceInformation("Roots:");
        foreach (AncestorLineInfo root in query)
        {
          IndividualClass person = familyTree.GetIndividual(root.rootAncestor);
          trace.TraceInformation("  " + root.depth + " generations: " + person.GetName() + " " + person.GetDate(IndividualEventClass.EventType.Birth) + " - " + person.GetDate(IndividualEventClass.EventType.Death));
        }
      }
      {
        IEnumerable<HandledItem> query = analysedPeopleNo.OrderByDescending(ancestor => ancestor.number);
        int i = 0;

        trace.TraceInformation("Multiply Referenced:");
        foreach (HandledItem item in query)
        {
          IndividualClass person = familyTree.GetIndividual(item.xref);
          if (item.number > 1)
          {
            trace.TraceInformation("  Referenced " + item.number + " times: " + person.GetName() + " " + person.GetDate(IndividualEventClass.EventType.Birth) + " - " + person.GetDate(IndividualEventClass.EventType.Death));
          }
          i++;
        }
        trace.TraceInformation("Items:" + i);
      }
    }

    private string Linefeed()
    {
      return "\r\n";
    }

    override public string ToString()
    {
      string str = "";
      //ancestorList.OrderBy<int, depth>();

      str += "Analysis started at " + startTime + " done at " + DateTime.Now + Linefeed();
      str += "Ancestor overview:" + Linefeed();
      str += "  analysed " + people + " people   " + duplicatePeople + " more than once" + Linefeed();
      str += "  analysed " + families + " families " + duplicateFamilies + " more than once" + Linefeed();
      str += "  roots    " + ancestorList.Count + Linefeed();
      //trace.TraceInformation("  max children: " + maxNoOfChildren+ " parents " + maxNoOfParents);
      //familyTree.Print();

      {
        IEnumerable<AncestorLineInfo> query = ancestorList.OrderBy(ancestor => ancestor.depth);

        str += "Roots:" + Linefeed();
        foreach (AncestorLineInfo root in query)
        {
          IndividualClass person = familyTree.GetIndividual(root.rootAncestor);
          //str += root.depth + " generations: " + person.GetName() + " " + person.GetDate(IndividualEventClass.EventType.Birth) + " - " + person.GetDate(IndividualEventClass.EventType.Death) + Linefeed();
          str += root.details + Linefeed();
          str += root.relationPath.ToString(familyTree) + Linefeed();
        }
      }
      {
        IEnumerable<HandledItem> query = analysedPeopleNo.OrderByDescending(ancestor => ancestor.number);

        str += "Multiply Referenced: " + query.Count<HandledItem>() + " items" + Linefeed();
        foreach (HandledItem item in query)
        {
          IndividualClass person = familyTree.GetIndividual(item.xref);
          if (item.number > 1)
          {
            str += "Referenced " + item.number + " times: " + person.GetName() + " " + person.GetDate(IndividualEventClass.EventType.Birth) + " - " + person.GetDate(IndividualEventClass.EventType.Death) + Linefeed();
            //str += root.relationPath.ToString(familyTree) + Linefeed();
            foreach (RelationStack stack in item.relationStackList)
            {
              if (stack != null)
              {
                str += " " + stack.ToString(familyTree) + Linefeed();
              }
            }
          }
        }
      }
      return str;
    }

    public IList<AncestorLineInfo> GetAncestorList()
    {
      return ancestorList;

    }

    public IList<HandledItem> GetAnalysedFamiliesNo()
    {
      return analysedFamiliesNo;
    }
    public IList<HandledItem> GetAnalysedPeopleNo()
    {
      return analysedPeopleNo;
    }


  }

  public class CheckRelation
  {
    private ProgressReporter progressReporter;
    private double latestPercent;
    private int totalGenerations; 
    private static TraceSource trace = new TraceSource("CheckRelation", SourceLevels.Warning);
    private bool IsSubsetOf(RelationStack smallStack, RelationStack bigStack)
    {
      foreach(Relation sRel in smallStack)
      {
        bool subset = false;
        foreach (Relation bRel in bigStack)
        {
          if(sRel.personXref == bRel.personXref)
          {
            subset = true;
          }
        }
        if(!subset)
        {
          return false;
        }
      }
      return true;

    }

    private string CalculateRelation(RelationStack person1, RelationStack person2)
    {
      string str = "";

      int minGenerations = Math.Min(person1.Count, person2.Count);
      int maxGenerations = Math.Max(person1.Count, person2.Count);
      int diffGenerations = maxGenerations - minGenerations;

      switch(minGenerations)
      {
        case 1:
          str = "the same person";
          break;
        case 2:
          str = "sibling";
          break;
        case 3:
          str = "cousin";
          break;
        case 4:
          str = "second cousin";
          break;
        case 5:
          str = "third cousin";
          break;
        case 6:
          str = "fourth cousin";
          break;
        default:
          str = (minGenerations - 2) + "-th cousin";
          break;
      }
      if(diffGenerations != 0)
      {
        switch(diffGenerations)
        {
          case 1: 
            str += " once removed";
            break;
          case 2: 
            str += " twice removed";
            break;
          default: 
            str += " " + diffGenerations + " times removed";
            break;
        }
      }

      return str;
    }

    public Relation.Type InvertRelation(Relation.Type rel)
    {
      //Relation.Type newRel = Relation.Type.Person;
      switch (rel)
      {
        case Relation.Type.Parent:
        case Relation.Type.Person:
          return Relation.Type.Child;
        case Relation.Type.Father:
        case Relation.Type.Man:
          return Relation.Type.Son;
        case Relation.Type.Mother:
        case Relation.Type.Woman:
          return Relation.Type.Daughter;
        default:
          trace.TraceInformation("sex inversion problem!" + rel);
          return Relation.Type.Person;
      }

    }
    public Relation.Type GetSex(Relation.Type rel)
    {
      //Relation.Type newRel = Relation.Type.Person;
      switch (rel)
      {
        case Relation.Type.Parent:
        case Relation.Type.Child:
        case Relation.Type.Person:
          return Relation.Type.Person;
        case Relation.Type.Father:
        case Relation.Type.Son:
        case Relation.Type.Man:
          return Relation.Type.Man;
        case Relation.Type.Mother:
        case Relation.Type.Daughter:
        case Relation.Type.Woman:
          return Relation.Type.Woman;
        default:
          trace.TraceInformation("sex problem!" + rel);
          return Relation.Type.Person;
      }

    }

    public CheckRelation(FamilyTreeStoreBaseClass familyTree, string xrefPerson1, string xrefPerson2, int noOfGenerations, ref RelationStackList relationList, ProgressReporter progress)
    {
      if (familyTree != null)
      {
        IndividualClass person1 = familyTree.GetIndividual(xrefPerson1);
        IndividualClass person2 = familyTree.GetIndividual(xrefPerson2);

        relationList.sourceTree = familyTree.GetSourceFileName();
        relationList.time = DateTime.Now;

        progressReporter = progress;

        if ((person1 != null) && (person2 != null))
        {
          IList<RelationStack> person1verified = new List<RelationStack>();
          IList<RelationStack> person2verified = new List<RelationStack>();
          IDictionary<string,RelationStack> person1Ancestors = new Dictionary<string,RelationStack>();
          IDictionary<string,RelationStack> person2Ancestors = new Dictionary<string,RelationStack>();

          LoadAncestors(familyTree, person1, ref person1Ancestors, noOfGenerations, Relation.GetSex(person1), null, 0.0, progress, " Loading ancestors to " + person1.GetName().ToString() + " (1/2) ");
          LoadAncestors(familyTree, person2, ref person2Ancestors, noOfGenerations, Relation.GetSex(person2), null, 0.0, progress, " Loading ancestors to " + person2.GetName().ToString() + " (2/2) ");

          IEnumerator<KeyValuePair<string,RelationStack>> person1enum = person1Ancestors.GetEnumerator();

          while(person1enum.MoveNext())
          {
            IEnumerator<KeyValuePair<string, RelationStack>> person2enum = person2Ancestors.GetEnumerator();

            while (person2enum.MoveNext())
            {
              if(person1enum.Current.Key == person2enum.Current.Key)
              {
                bool duplicate = false;
                trace.TraceInformation("Found match!" + person1verified.Count);


                for(int i = 0; i < person1verified.Count; i++)
                {
                  RelationStack stack1 = person1verified[i];
                  RelationStack stack2 = person2verified[i];

                  if(IsSubsetOf(stack1, person1enum.Current.Value) && IsSubsetOf(stack2, person2enum.Current.Value))
                  {
                    duplicate = true;
                    trace.TraceInformation("Don't add this. Duplicate of number " + i);
                    trace.TraceInformation(person1enum.Current.Value.ToString(familyTree));
                    trace.TraceInformation(person2enum.Current.Value.ToString(familyTree));
                  }
                }
                if(!duplicate)
                {
                  trace.TraceInformation("Unique match, added!");
                  person1verified.Add(person1enum.Current.Value);
                  person2verified.Add(person2enum.Current.Value);

                  trace.TraceInformation(person1enum.Current.Value.ToString(familyTree));
                  trace.TraceInformation(person2enum.Current.Value.ToString(familyTree));
                }

              }
            }
          }
          trace.TraceInformation("Done searching! Found " + person1verified.Count + " matches!");
          for (int i = 0; i < person1verified.Count; i++)
          {
            trace.TraceInformation("Final match " + (i + 1) + " out of " + person1verified.Count + " distance " + person1verified[i].Count + " + " + person2verified[i].Count + " = " + (person1verified[i].Count + person2verified[i].Count) + " steps or " + CalculateRelation(person1verified[i], person2verified[i]));
            //trace.TraceInformation(person1verified[i].ToString(familyTree));
            //trace.TraceInformation(person2verified[i].ToString(familyTree));

            if((person1verified[i].Count >= 1) && (person2verified[i].Count >= 1))
            {
              if (person1verified[i][person1verified[i].Count - 1].personXref == person2verified[i][person2verified[i].Count - 1].personXref)
              {
                RelationStack printStack = person1verified[i].Duplicate();

                if (person2verified[i].Count > 1)
                {
                  // Turn one of the relation stacks upside down to get kinship...
                  for (int j = (person2verified[i].Count - 2); j >= 0; j--)
                  {
                    printStack.Add(new Relation(InvertRelation(person2verified[i][j].type), person2verified[i][j].personXref));
                  }
                }
                trace.TraceInformation(printStack.ToString(familyTree));
                trace.TraceInformation(printStack.CalculateRelation(familyTree));
                if (relationList != null)
                {
                  relationList.relations.Add(printStack.Duplicate());
                  trace.TraceInformation("Add:" + printStack.CalculateRelation(familyTree));
                }
              }
              else
              {
                trace.TraceEvent(TraceEventType.Error, 0, "error: " + person1verified[i][person1verified[i].Count - 1].personXref + "!=" + person2verified[i][person2verified[i].Count - 1].personXref);
                trace.TraceEvent(TraceEventType.Error, 0, person1verified[i].ToString(familyTree));
                trace.TraceEvent(TraceEventType.Error, 0, person2verified[i].ToString(familyTree));
              }
            }
            else
            {
              trace.TraceEvent(TraceEventType.Error, 0, "error: " + person1verified[i].Count + " or " + person2verified[i].Count + " < 2");
              trace.TraceEvent(TraceEventType.Error, 0, person1verified[i].ToString(familyTree));
              trace.TraceEvent(TraceEventType.Error, 0, person2verified[i].ToString(familyTree));
            }
          }
        }
      }
    }

    private Relation.Type GetParentRelation(IndividualClass person)
    {
      switch (person.GetSex())
      {
        case IndividualClass.IndividualSexType.Female:
          return Relation.Type.Mother;
        case IndividualClass.IndividualSexType.Male:
          return Relation.Type.Father;
        default:
          return Relation.Type.Parent;

      }
    }


    private void LoadAncestors(FamilyTreeStoreBaseClass familyTree, IndividualClass person, ref IDictionary<string, RelationStack> ancestors, int generations, Relation.Type relation, RelationStack relationStack, double startProgress, ProgressReporter progressReporter, string progressDescription)
    {
      double progress = startProgress;
      trace.TraceInformation("LoadAncestors(" + person.GetName() + "," + generations + "," + ancestors.Count + ")");

      if (relationStack == null)
      {
        relationStack = new RelationStack();
        relationStack.Add(new Relation(Relation.GetSex(person), person.GetXrefName()));
        totalGenerations = generations;
      }
      else
      {
        relationStack.Add(new Relation(relation, person.GetXrefName()));
      }

      if (!ancestors.ContainsKey(person.GetXrefName()))
      {
        ancestors.Add(person.GetXrefName(), relationStack.Duplicate());
      }
      else
      {
        trace.TraceInformation("Add new relation to person!");
      }

      if (generations > 0)
      {
        IList<FamilyXrefClass> parentFamilyList = person.GetFamilyChildList();
        int depth = totalGenerations - generations;

        if (parentFamilyList != null)
        {
          int familyNo = 1;
          foreach (FamilyXrefClass familyXref in parentFamilyList)
          {
            FamilyClass family = familyTree.GetFamily(familyXref.GetXrefName());

            if (family != null)
            {
              IList<IndividualXrefClass> parentXrefList = family.GetParentList();

              if (parentXrefList != null)
              {
                int parentNo = 0;
                foreach (IndividualXrefClass parentXref in parentXrefList)
                {
                  IndividualClass parent = familyTree.GetIndividual(parentXref.GetXrefName());
                  if (parent != null)
                  {
                    RelationStack stack2 = relationStack.Duplicate();

                    double levelAddition = Math.Pow(2, -depth);

                    trace.TraceInformation("progress-pre = " + progress.ToString() + " depth:" + depth + " => progress: " + progress + " + levelAdd:" + levelAddition + " * (famNo:" + familyNo + "  / famCount:" + parentFamilyList.Count + ") * (parentNo:" + parentNo + " / parentCount:" + parentXrefList.Count + ") = " + (double)(progress + levelAddition * ((double)familyNo / parentFamilyList.Count) * ((double)parentNo / parentXrefList.Count)) + " " + parentXref.GetXrefName() + "=" + parent.GetName().ToString());
                    double progressPercent = progress + levelAddition * ((double)familyNo / parentFamilyList.Count) * ((double)parentNo / parentXrefList.Count);
                    if (latestPercent > progressPercent)
                    {
                      trace.TraceData(TraceEventType.Warning, 0, "Progress = backwards!!! " + progressPercent.ToString("P2") + "<" + latestPercent.ToString("P2"));
                    }
                    latestPercent = progressPercent;
                    trace.TraceInformation("Progress-post = " + progressPercent.ToString("P2"));
                    if (progressReporter != null)
                    {
                      progressReporter.ReportProgress(progressPercent * 100.0, progressDescription + ancestors.Count);
                    }
                    LoadAncestors(familyTree, parent, ref ancestors, generations - 1, GetParentRelation(parent), stack2, progressPercent, progressReporter, progressDescription);
                  }
                  else
                  {
                    trace.TraceEvent(TraceEventType.Error, 0, "Error person " + parentXref.GetXrefName() + " not found in database!");
                  }
                  parentNo++;
                }
              }
              else
              {
                //trace.TraceEvent(TraceEventType.Information, 0, "family.GetParentList() " + familyXref.ToString() + " is null!");
              }
            }
            else
            {
              trace.TraceEvent(TraceEventType.Error, 0, "Error family " + familyXref.ToString() + " not found in database!");
            }
            familyNo++;
          }
        }
      }
    }
  }
}


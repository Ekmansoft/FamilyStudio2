using System;
using System.Collections.Generic;
using System.Collections;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.ComponentModel;
//using System.Object;
using FamilyStudioData.FamilyData;
using FamilyStudioData.FamilyTreeStore;
//using FamilyStudioData.FamilyFileFormat;

namespace FamilyStudioData.FileFormats.AnarkivCodec
{
  public class FamilyTreeStoreAnarkiv : FamilyTreeStoreBaseClass, IDisposable
  {
    private static TraceSource trace = new TraceSource("FamilyTreeStoreAnarkiv", SourceLevels.Warning);
    private class AnarkivFamilyRelations : Object
    {
      public IList<int> children;
      public IList<int> parents;
      public int familyId;

      public AnarkivFamilyRelations()
      {
        children = new List<int>();
        parents = new List<int>();
      }
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }
    protected virtual void Dispose(bool managed)
    {
      if(managed)
      {
        foreach (DatabaseInstance inst in adapterMapper.Values)
        {
          if (inst.connection != null)
          {
            inst.connection.Dispose();
          }
          if (inst.adapter != null)
          {
            inst.adapter.Dispose();
          }
        }
        adapterMapper.Clear();
      }
    }

    private class AnarkivPersonSex : Object
    {
      public int personXref;
      public IndividualClass.IndividualSexType sex;

      public AnarkivPersonSex()
      {
        personXref = -1;
        sex = IndividualClass.IndividualSexType.Unknown;
      }

    }

    struct DatabaseInstance
    {
      public OleDbConnection connection;
      public OleDbDataAdapter adapter;
      public int threadId;
      public int connectionUsers;
    }
    private String m_fileName;
    private string sourceFileName;
    private IDictionary<int, AnarkivFamilyRelations> familyMapper;
    private IDictionary<int, AnarkivPersonSex> personSexMapper;
    private IDictionary<int, DatabaseInstance> adapterMapper;

    private void CacheFamilies()
    {
      int familyNo = 0, parentNo = 0, childNo = 0;
      trace.TraceInformation("FamilyStoreAnarkiv::CacheFamilies()-start:" + DateTime.Now);

      if (familyMapper != null)
      {
        trace.TraceInformation("FamilyStoreAnarkiv::CacheFamilies()-already done:");
        return;
      }

      familyMapper = new Dictionary<int, AnarkivFamilyRelations>();


      OleDbConnection connection = GetDbConnection();

      OleDbDataAdapter adapter = GetDbAdapter();

      String sqlString;
      //sqlString = "SELECT f.Personnr AS Parent,f.Familjenr,b.Personnr AS Child FROM Familjer AS f LEFT OUTER JOIN Barn AS b ON b.Familjenr = f.Familjenr ORDER BY f.Familjenr,f.Personnr,b.Personnr";
      sqlString = "SELECT Personnr,Familjenr FROM Familjer";

      trace.TraceInformation("CacheFamilies sql:[" + sqlString + "]");// + dataset.Tables.Count + "," + dataset.Tables[0].Rows[0].ToString() + "," + dataset.Tables[0].DataSet.DataSetName);
      adapter.SelectCommand = new OleDbCommand(sqlString, connection);

      DataTable familyTable = new DataTable();
      //familyTable.Clear();
      adapter.Fill(familyTable);//dataset);

      if (familyTable.Rows.Count > 0)
      {

        trace.TraceInformation("familyTable.parents:" + familyTable.ToString() + ", columns.count=" + familyTable.Columns.Count + ", rows.count=" + familyTable.Rows.Count);

        foreach (DataRow familyRow in familyTable.Rows)
        {
          //trace.TraceInformation("Row[" + count + ":" + familyRow[familyTable.Columns["Familjenr"]].ToString() + " child:" + familyRow[familyTable.Columns["Child"]] + " parent:" + familyRow[familyTable.Columns["Parent"]].ToString());

          //trace.TraceInformation("Row[" + count + "]:" + familyRow + " list.count:" + familyMapper.Count);

          //IEnumerator families = familyMapper.GetEnumerator();
          int familyId = (int)familyRow[familyTable.Columns["Familjenr"]];

          AnarkivFamilyRelations family;
          //families 
          if (familyMapper.ContainsKey(familyId))
          {
            family = (AnarkivFamilyRelations)familyMapper[familyId];
          }
          else
          {
            family = new AnarkivFamilyRelations();
            family.familyId = familyId;
            familyNo++;
            //familyMapper[familyId] = family;
          }

          family.parents.Add((int)familyRow[familyTable.Columns["Personnr"]]);
          familyMapper[familyId] = family;
          parentNo++;

        }
      }
      else
      {
        trace.TraceInformation("familyTable: null");
      }


      sqlString = "SELECT Personnr,Familjenr FROM Barn";

      trace.TraceInformation("CacheFamilies sql:[" + sqlString + "]");// + dataset.Tables.Count + "," + dataset.Tables[0].Rows[0].ToString() + "," + dataset.Tables[0].DataSet.DataSetName);
      adapter.SelectCommand = new OleDbCommand(sqlString, connection);

      familyTable.Clear();
      adapter.Fill(familyTable);//dataset);

      if (familyTable.Rows.Count > 0)
      {
        trace.TraceInformation("familyTable.children:" + familyTable.ToString() + ", columns.count=" + familyTable.Columns.Count + ", rows.count=" + familyTable.Rows.Count);

        foreach (DataRow familyRow in familyTable.Rows)
        {
          //trace.TraceInformation("Row[" + count + ":" + familyRow[familyTable.Columns["Familjenr"]].ToString() + " child:" + familyRow[familyTable.Columns["Child"]] + " parent:" + familyRow[familyTable.Columns["Parent"]].ToString());

          //trace.TraceInformation("Row[" + count + "]:" + familyRow + " list.count:" + familyMapper.Count);

          //IEnumerator families = familyMapper.GetEnumerator();
          int familyId = (int)familyRow[familyTable.Columns["Familjenr"]];

          AnarkivFamilyRelations family;
          //families 
          if (familyMapper.ContainsKey(familyId))
          {
            family = (AnarkivFamilyRelations)familyMapper[familyId];
          }
          else
          {
            family = new AnarkivFamilyRelations();
            familyNo++;
          }
          family.children.Add((int)familyRow[familyTable.Columns["Personnr"]]);
          familyMapper[familyId] = family;
          childNo++;

        }
      }
      else
      {
        trace.TraceInformation("familyTable: null");
      }


      CloseDbConnection();
      trace.TraceInformation("FamilyStoreAnarkiv::CacheFamilies()-end:" + familyNo + "," + parentNo + "," + childNo + ":" + DateTime.Now);

    }

    private void CachePersonSexTable()
    {
      int personNo = 0, femaleNo = 0, maleNo = 0;
      trace.TraceInformation("FamilyStoreAnarkiv::CachePersonSexTable()-start:" + DateTime.Now);

      if (personSexMapper != null)
      {
        trace.TraceInformation("FamilyStoreAnarkiv::CachePersonSexTable()-already done:");
        return;
      }

      personSexMapper = new Dictionary<int, AnarkivPersonSex>();

      OleDbConnection connection = GetDbConnection();

      OleDbDataAdapter adapter = GetDbAdapter();

      String sqlString;
      //sqlString = "SELECT f.Personnr AS Parent,f.Familjenr,b.Personnr AS Child FROM Familjer AS f LEFT OUTER JOIN Barn AS b ON b.Familjenr = f.Familjenr ORDER BY f.Familjenr,f.Personnr,b.Personnr";
      sqlString = "SELECT Personnr,Kön FROM Personer";

      trace.TraceInformation("CachePersonSexTable sql:[" + sqlString + "]");// + dataset.Tables.Count + "," + dataset.Tables[0].Rows[0].ToString() + "," + dataset.Tables[0].DataSet.DataSetName);
      adapter.SelectCommand = new OleDbCommand(sqlString, connection);

      DataTable personTable = new DataTable();
      adapter.Fill(personTable);//dataset);

      if (personTable.Rows.Count > 0)
      {
        foreach (DataRow personRow in personTable.Rows)
        {
          AnarkivPersonSex individualSex = new AnarkivPersonSex();
          int personId = Convert.ToInt32(personRow[personTable.Columns["Personnr"]].ToString());
          personNo++;
          individualSex.personXref = personId;

          //trace.TraceInformation("Sex:" + personRow[personTable.Columns["Kön"]]);
          if (personRow[personTable.Columns["Kön"]] != null)
          {

            switch (personRow[personTable.Columns["Kön"]].ToString())
            {
              case "M":
                individualSex.sex = IndividualClass.IndividualSexType.Male;
                maleNo++;
                break;
              case "Q":
                individualSex.sex = IndividualClass.IndividualSexType.Female;
                femaleNo++;
                break;
              default:
                break;
            }
          }
          //personSexMapper.Add(personId, individualSex);
          personSexMapper[personId] = individualSex;
        }
        CloseDbConnection();
      }
      trace.TraceInformation("CachePersonSexTable sql:[" + personTable.Rows.Count + "," + personNo + "," + maleNo + "," + femaleNo + "]");
    }


    private class AddressClassOrder
    {
      public IndividualEventClass address;
      public int orderNo;

      public AddressClassOrder(int order)
      {
        address = new IndividualEventClass(IndividualEventClass.EventType.Residence);
        orderNo = order;
      }
    }

    private AddressClassOrder GetAddressOrder(ref IList<AddressClassOrder> list, int order)
    {
      if (list == null)
      {
        return null;
      }
      foreach (AddressClassOrder address in list)
      {
        if (address.orderNo == order)
        {
          //list.Remove(address);
          return address;
        }
      }
      return null;
    }

    private class NoteOrder
    {
      public string note;
      public int noteIndex;

      public NoteOrder(int order)
      {
        note = "";
        noteIndex = order;
      }
    }
    private NoteOrder GetNoteOrder(ref IList<NoteOrder> list, int order)
    {
      if (list == null)
      {
        return null;
      }
      foreach (NoteOrder note in list)
      {
        if (note.noteIndex == order)
        {
          return note;
        }
      }
      return null;
    }

    class DataBasePostInfo
    {
      public string personId;
      public IList<string> familyChildId;
      public IList<string> familySpouseId;

      public DataBasePostInfo()
      {
        personId = "";
        familyChildId = new List<string>();
        familySpouseId = new List<string>();
      }
      public bool IsFamilyAdded(bool spouse, string familyId)
      {
        IList<string> searchList;

        if (spouse)
        {
          searchList = familySpouseId;
        }
        else
        {
          searchList = familyChildId;
        }
        foreach(string s in searchList)
        {
          if (s == familyId)
          {
            return true;
          }
        }
        return false;


      }
    };


    public FamilyTreeStoreAnarkiv()
    {
      //connectionUsers = 0;

      trace.TraceInformation("FamilyTreeStoreAnarkiv");

      familyMapper = null;
      adapterMapper = new Dictionary<int, DatabaseInstance>();
    }

    private IndividualClass TransformRecordToIndividual(DataRow personRow)
    {
      //if (personTable.Rows.Count > 0)
      {
        IndividualClass individual = new IndividualClass();
        PersonalNameClass name = new PersonalNameClass();

        /*trace.TraceInformation("Table:" + personTable.ToString() + ":OK-" + personRow.Columns.Count + "," + personTable.Rows.Count);

        foreach (DataRow personRow in personTable.Rows)
        {
          trace.TraceInformation("Row:" + personRow.ToString());

          if (personRow[personRow.Columns["Förnamn"]] != null)
          {
            name.SetName(PersonalNameClass.PartialNameType.GivenName, personRow[personTable.Columns["Förnamn"]].ToString());
          }
          if (personRow[personTable.Columns["Efternamn"]] != null)
          {
            name.SetName(PersonalNameClass.PartialNameType.GivenName, personRow[personTable.Columns["Efternamn"]].ToString());
          }
          if (personRow[personTable.Columns["Datum"]] != null)
          {
            individual.AddEvent(IndividualEventClass.EventType.Birth, ParseAnarkivDate(personRow[personTable.Columns["Datum"]].ToString()));
          }
          if (personRow[personTable.Columns["DopDatum"]] != null)
          {
            individual.AddEvent(IndividualEventClass.EventType.Baptism, ParseAnarkivDate(personRow[personTable.Columns["DopDatum"]].ToString()));
          }
          if (personRow[personTable.Columns["DödDatum"]] != null)
          {
            individual.AddEvent(IndividualEventClass.EventType.Death, ParseAnarkivDate(personRow[personTable.Columns["DödDatum"]].ToString()));
          }
          if (personRow[personTable.Columns["Begravning"]] != null)
          {
            individual.AddEvent(IndividualEventClass.EventType.Burial, ParseAnarkivDate(personRow[personTable.Columns["Begravning"]].ToString()));
          }
          if (personRow[personTable.Columns["Personnr"]] != null)
          {
            individual.SetXrefName(personRow[personTable.Columns["Personnr"]].ToString());
          }
          if (personRow[personTable.Columns["Kön"]] != null)
          {
            switch (personRow[personTable.Columns["Kön"]].ToString())
            {
              case "M":
                individual.SetSex(IndividualClass.IndividualSexType.Male);
                break;
              case "Q":
                individual.SetSex(IndividualClass.IndividualSexType.Female);
                break;
              default:
                //individual.SetSex(IndividualClass.IndividualSexType.Male);
                break;
            }

    individual.SetPersonalName(name);

            sqlString = "SELECT * FROM Familjer WHERE Personnr = " + individual.GetXrefName();
            trace.TraceInformation("sql:[" + sqlString + "]");// + dataset.Tables.Count + "," + dataset.Tables[0].personRows[0].ToString() + "," + dataset.Tables[0].DataSet.DataSetName);
            adapter.SelectCommand = new OleDbCommand(sqlString, connection);

            DataTable familyTable = new DataTable();

            adapter.Fill(familyTable);//dataset);

            if (familyTable.Rows.Count > 0)
            {
              foreach (DataRow familyRow in familyTable.Rows)
              {
                foreach (DataColumn familjerColumn in familyTable.Columns)
                {
                  trace.TraceInformation("  " + familjerColumn.ToString() + ":" + familyRow[familjerColumn]);
                }
                if (familyRow[familyTable.Columns["Familjenr"]] != null)
                {
                  individual.AddRelation(new FamilyXrefClass(familyRow[familyTable.Columns["Familjenr"]].ToString()), IndividualClass.RelationType.Spouse);
                }
              }

            }


            individual.Print();

            return individual;
            //}
          }
        }*/
      }
      return null;
    }

    public void AddFamily(FamilyClass tempFamily)
    {
    }

    public FamilyClass GetFamily_ddb(String xrefName)
    {
      OleDbConnection connection;
      String sqlString;
      FamilyClass family = new FamilyClass();

      trace.TraceInformation("FamilyTreeStoreAnarkiv::GetFamily()-start" + xrefName + "," + DateTime.Now);
      connection = GetDbConnection();


      sqlString = "SELECT Personnr FROM Familjer WHERE Familjenr = " + xrefName;

      OleDbDataAdapter adapter = GetDbAdapter();
      trace.TraceInformation("sql:[" + sqlString + "]");// + dataset.Tables.Count + "," + dataset.Tables[0].personRows[0].ToString() + "," + dataset.Tables[0].DataSet.DataSetName);
      adapter.SelectCommand = new OleDbCommand(sqlString, connection);
      //OleDbDataAdapter adapter = new OleDbDataAdapter(sqlString, connection);

      DataTable familyTable = new DataTable();
      adapter.Fill(familyTable);//dataset);

      trace.TraceInformation("GetFamily:familyTable.Rows.Count=" + familyTable.Rows.Count);// + dataset.Tables.Count + "," + dataset.Tables[0].Rows[0].ToString() + "," + dataset.Tables[0].DataSet.DataSetName);

      if (familyTable.Rows.Count > 0)
      {
        foreach (DataRow familyRow in familyTable.Rows)
        {
          //trace.TraceInformation("  familj " + familyRow.ToString() + ":" + row[column]);
          if (familyRow[familyTable.Columns["Personnr"]] != null)
          {
            family.AddRelation(new IndividualXrefClass(familyRow[familyTable.Columns["Personnr"]].ToString()), FamilyClass.RelationType.Parent);
          }
          if (trace.Switch.Level.HasFlag(SourceLevels.Information))
          {
            trace.TraceInformation(familyRow.ToString());
            foreach (DataColumn column in familyTable.Columns)
            {
              trace.TraceInformation("  " + column.ToString() + ":" + familyRow[column]);
            }
          }
        }
      }

      sqlString = "SELECT Personnr FROM Barn WHERE Familjenr = " + xrefName;
      trace.TraceInformation("sql:[" + sqlString + "]");// + dataset.Tables.Count + "," + dataset.Tables[0].personRows[0].ToString() + "," + dataset.Tables[0].DataSet.DataSetName);

      adapter.SelectCommand = new OleDbCommand(sqlString, connection);

      DataTable childrenTable = new DataTable();
      adapter.Fill(childrenTable);//dataset);
      //connection.Close();
      trace.TraceInformation("GetFamily-barn:childrenTable.Rows.Count=" + childrenTable.Rows.Count);// + dataset.Tables.Count + "," + dataset.Tables[0].Rows[0].ToString() + "," + dataset.Tables[0].DataSet.DataSetName);

      if (childrenTable.Rows.Count > 0)
      {
        foreach (DataRow barnRow in childrenTable.Rows)
        {
          if (barnRow[childrenTable.Columns["Personnr"]] != null)
          {
            family.AddRelation(new IndividualXrefClass(barnRow[childrenTable.Columns["Personnr"]].ToString()), FamilyClass.RelationType.Child);
          }
          if (trace.Switch.Level.HasFlag(SourceLevels.Information))
          {
            trace.TraceInformation(barnRow.ToString());
            foreach (DataColumn column in childrenTable.Columns)
            {
              trace.TraceInformation("  " + column.ToString() + ":" + barnRow[column]);
            }
          }
        }
      }

      CloseDbConnection();

      //foundRows = anarkivDataset.Personer.Select("Personnr = 12");
      trace.TraceInformation("FamilyTreeStoreAnarkiv::GetFamily({0})-end" + xrefName + "," + DateTime.Now);

      return family;
    }

    public FamilyClass GetFamily(String xrefName)
    {
      //OleDbConnection connection;
      //String sqlString;

      CacheFamilies();

      int familyId = Convert.ToInt32(xrefName);

      trace.TraceInformation("FamilyTreeStoreAnarkiv::GetFamily_ram()-start" + xrefName + " == " + familyId + " @time:" + DateTime.Now + "," + familyMapper.Count);


      if (familyMapper.ContainsKey(familyId))
      {
        AnarkivFamilyRelations anarkivFamily = (AnarkivFamilyRelations)familyMapper[familyId];

        FamilyClass family = new FamilyClass();

        family.SetXrefName(anarkivFamily.familyId.ToString());

        foreach (int childId in anarkivFamily.children)
        {
          family.AddRelation(new IndividualXrefClass(childId.ToString()), FamilyClass.RelationType.Child);
          //childNo++;
        }
        foreach (int parentId in anarkivFamily.parents)
        {
          family.AddRelation(new IndividualXrefClass(parentId.ToString()), FamilyClass.RelationType.Parent);
          //parentNo++;
        }
        trace.TraceInformation("FamilyTreeStoreAnarkiv::GetFamily_ram()-done" + xrefName + "," + DateTime.Now);
        return family;
      }
      trace.TraceEvent(TraceEventType.Error, 0, "FamilyTreeStoreAnarkiv::GetFamily_ram()-error" + xrefName + "," + DateTime.Now);
      return null;
    }

    public bool AddIndividual(IndividualClass tempIndividual)
    {
      return false;
    }
    public bool UpdateIndividual(IndividualClass tempIndividual, PersonUpdateType updateType)
    {
      return false;
    }

    private FamilyDateTimeClass ParseAnarkivDate(string dateString)
    {
      int year = 0, month = 0, day = 0;
      int cnt;
      bool dateValid = true;
      FamilyDateTimeClass date;

      for (cnt = 0; cnt < dateString.Length; cnt++)
      {
        char digit = dateString[cnt];

        if ((digit >= '0') && (digit <= '9'))
        {
          int number = (int)digit - (int)'0';

          if (cnt < 4)
          {
            year = year * 10 + number;
          }
          else if ((cnt > 4) && (cnt < 7))
          {
            month = month * 10 + number;
          }
          else if (cnt > 7)
          {
            day = day * 10 + number;
          }
        }
        else if ((cnt == 4) || (cnt == 7))
        {
          if (digit != '-')
          {
            dateValid = false;
          }
        }
      }
      if (dateValid && (month <= 12) && (day <= 32))
      {
        date = new FamilyDateTimeClass(year, month, day);

        if (dateString.IndexOf("circa") >= 0)
        {
          date.SetApproximate(true);
        }
        if (year != 0)
        {
          if (month != 0)
          {
            if (day != 0)
            {
              date.SetDateType(FamilyDateTimeClass.FamilyDateType.YearMonthDay);
            }
            else
            {
              date.SetDateType(FamilyDateTimeClass.FamilyDateType.YearMonth);
            }
          }
          else
          {
            date.SetDateType(FamilyDateTimeClass.FamilyDateType.Year);
          }
        }
        else
        {
          date.SetDateType(FamilyDateTimeClass.FamilyDateType.Unknown);
        }
        //trace.TraceInformation("ParseDate(" + dateString + ")=" + year + "-" + month + "-" + day + " = " + date);// + dataset.Tables.Count + "," + dataset.Tables[0].Rows[0].ToString() + "," + dataset.Tables[0].DataSet.DataSetName);
        return date;
      }
      date = new FamilyDateTimeClass(dateString);
      date.SetDateType(FamilyDateTimeClass.FamilyDateType.DateString);
      //trace.TraceInformation("ParseDate(" + dateString + ")=" + year + "-" + month + "-" + day + " = " + date);// + dataset.Tables.Count + "," + dataset.Tables[0].Rows[0].ToString() + "," + dataset.Tables[0].DataSet.DataSetName);
      return date;
    }

    public IndividualClass GetIndividual(String xrefName = null, uint index = (uint)SelectIndex.NoIndex, PersonDetail detailLevel = PersonDetail.PersonDetail_All)
    {
      //AnarkivDataSet dataset = new AnarkivDataSet();
      //DataSet dataset = new DataSet();
      //DataTable personTable = new DataTable();
      String sqlString;
      OleDbConnection connection;

      //trace.TraceInformation("FamilyStoreAnarkiv::GetIndividual(" + xrefName + ")-start:" + DateTime.Now);
      connection = GetDbConnection();
      //connection.Open();
      //connection = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=C:\\anarkiv-db.ddb;");//User ID=\"Admin;NewValue=Bad\"");

      //OleDbDataAdapter adapter = new OleDbDataAdapter();
      OleDbDataAdapter adapter = GetDbAdapter();

      if (detailLevel == PersonDetail.PersonDetail_Sex)
      {

        if (personSexMapper == null)
        {
          CachePersonSexTable();
        }

        if (personSexMapper != null)
        {
          int personId = Convert.ToInt32(xrefName);
          if (personSexMapper.ContainsKey(personId))
          {
            IndividualClass individual = new IndividualClass();

            AnarkivPersonSex personSex = (AnarkivPersonSex)personSexMapper[personId];

            individual.SetXrefName(xrefName);
            individual.SetSex(personSex.sex);
              
            return individual;
          }
          return null;

        }
        sqlString = "SELECT Personnr,Kön FROM Personer WHERE Personnr = " + xrefName;
        adapter.SelectCommand = new OleDbCommand(sqlString, connection);
        DataTable personTable2 = new DataTable();
        adapter.Fill(personTable2);//dataset);

        //trace.TraceInformation("Sex-list:" + personTable.Rows.Count);
        if (personTable2.Rows.Count > 0)
        {
          IndividualClass individual = new IndividualClass();

          foreach (DataRow personRow in personTable2.Rows)
          {
            //trace.TraceInformation("Sex:" + personRow[personTable.Columns["Kön"]]);
            if (personRow[personTable2.Columns["Kön"]] != null)
            {
              switch (personRow[personTable2.Columns["Kön"]].ToString())
              {
                case "M":
                  individual.SetSex(IndividualClass.IndividualSexType.Male);
                  break;
                case "Q":
                  individual.SetSex(IndividualClass.IndividualSexType.Female);
                  break;
                default:
                  individual.SetSex(IndividualClass.IndividualSexType.Unknown);
                  break;
              }
            }
          }
          CloseDbConnection();
          //connection.Close();
          //trace.TraceInformation("FamilyStoreAnarkiv::GetIndividual(" + xrefName + ")-end:" + DateTime.Now);
          return individual;
        }
        return null;
      }
      else
      {
        sqlString = "SELECT Personnr,Förnamn,Efternamn,Datum,Dopdatum,Döddatum,Begravning,Kön FROM Personer";
      }
      if (xrefName != null)
      {
        sqlString += " WHERE Personnr = " + xrefName;
      }
      trace.TraceInformation("sql:[" + sqlString + "]");// + dataset.Tables.Count + "," + dataset.Tables[0].Rows[0].ToString() + "," + dataset.Tables[0].DataSet.DataSetName);
      adapter.SelectCommand = new OleDbCommand(sqlString, connection);

      DataTable personTable = new DataTable();
      bool failed = false;
      do
      {
        failed = false; 
        try
        {
          adapter.Fill(personTable);//dataset);
        }
        catch(Exception e)
        {
          trace.TraceEvent(TraceEventType.Error, 0, "error filling data" + e);
          failed = true;
          //Thread.wait
        }
      } while (failed);

      //connection.Close();

      //trace.TraceInformation("GetIndividual:");// + dataset.Tables.Count + "," + dataset.Tables[0].Rows[0].ToString() + "," + dataset.Tables[0].DataSet.DataSetName);

      //trace.TraceInformation("dataset.Tables.Count=" + dataset.Tables.Count);

      //trace.TraceInformation("PersonTable.Rows.Count=" + personTable.Rows.Count);

      if (personTable.Rows.Count > 0)
      {
        IndividualClass individual = new IndividualClass();
        PersonalNameClass name = new PersonalNameClass();
        uint counter = 0;

        //trace.TraceInformation("Table:" + personTable.ToString() + ", columns.count=" + personTable.Columns.Count + ", rows.count=" + personTable.Rows.Count);

        foreach (DataRow personRow in personTable.Rows)
        {
          if(index != (uint) SelectIndex.NoIndex)
          {
            if (++counter <= index)
            {
              continue;
            }
          }

          //trace.TraceInformation("Row:" + personRow.ToString());

          if (personRow[personTable.Columns["Förnamn"]] != null)
          {
            //trace.TraceInformation("SetName.given:" + personRow[personTable.Columns["Förnamn"]]);
            name.SetName(PersonalNameClass.PartialNameType.GivenName, personRow[personTable.Columns["Förnamn"]].ToString());
          }
          if (personRow[personTable.Columns["Efternamn"]] != null)
          {
            name.SetName(PersonalNameClass.PartialNameType.Surname, personRow[personTable.Columns["Efternamn"]].ToString());
          }
          if (personRow[personTable.Columns["Datum"]] != null)
          {

            FamilyDateTimeClass birth = ParseAnarkivDate(personRow[personTable.Columns["Datum"]].ToString());

            trace.TraceInformation("Birthdate:" + personRow[personTable.Columns["Datum"]] + " => " + birth);
 
            individual.AddEvent(IndividualEventClass.EventType.Birth, ParseAnarkivDate(personRow[personTable.Columns["Datum"]].ToString()));
          }
          if (personRow[personTable.Columns["DopDatum"]] != null)
          {
            individual.AddEvent(IndividualEventClass.EventType.Baptism, ParseAnarkivDate(personRow[personTable.Columns["DopDatum"]].ToString()));
          }
          if (personRow[personTable.Columns["DödDatum"]] != null)
          {
            individual.AddEvent(IndividualEventClass.EventType.Death, ParseAnarkivDate(personRow[personTable.Columns["DödDatum"]].ToString()));
          }
          if(personRow[personTable.Columns["Begravning"]] != null)
          {
            individual.AddEvent(IndividualEventClass.EventType.Burial, ParseAnarkivDate(personRow[personTable.Columns["Begravning"]].ToString()));
          }
          if (personRow[personTable.Columns["Personnr"]] != null)
          {
            individual.SetXrefName(personRow[personTable.Columns["Personnr"]].ToString());
          }
          if (personRow[personTable.Columns["Kön"]] != null)
          {
            switch (personRow[personTable.Columns["Kön"]].ToString())
            {
              case "M":
                individual.SetSex(IndividualClass.IndividualSexType.Male);
                break;
              case "Q":
                individual.SetSex(IndividualClass.IndividualSexType.Female);
                break;
              default:
                individual.SetSex(IndividualClass.IndividualSexType.Unknown);
                break;
            }
          }


            /*foreach (DataColumn column in personTable.Columns)
            {
              trace.TraceInformation("  " + column.ToString() + ":" + personRow[column]);

              if (column.ToString() == "Förnamn")
              {
                name.SetName(PersonalNameClass.PartialNameType.GivenName, personRow[column].ToString());
              }
              if (column.ToString() == "Efternamn")
              {
                name.SetName(PersonalNameClass.PartialNameType.GivenName, personRow[column].ToString());
              }
              if (column.ToString() == "Personnr")
              {
                individual.SetXrefName(personRow[column].ToString());
              }

            }*/
            individual.SetPersonalName(name);

            if((detailLevel & PersonDetail.PersonDetail_Parents) != 0)
            {
              sqlString = "SELECT Familjenr FROM Familjer WHERE Personnr = " + individual.GetXrefName();
              trace.TraceInformation("sql:[" + sqlString + "]");// + dataset.Tables.Count + "," + dataset.Tables[0].personRows[0].ToString() + "," + dataset.Tables[0].DataSet.DataSetName);
              adapter.SelectCommand = new OleDbCommand(sqlString, connection);

              DataTable familyTable = new DataTable();

              adapter.Fill(familyTable);//dataset);

              if (familyTable.Rows.Count > 0)
              {
                foreach (DataRow familyRow in familyTable.Rows)
                {
                  if (trace.Switch.Level.HasFlag(SourceLevels.Information))
                  {
                    foreach (DataColumn familjerColumn in familyTable.Columns)
                    {
                      trace.TraceInformation("  " + familjerColumn.ToString() + ":" + familyRow[familjerColumn]);
                    }
                  }
                  if (familyRow[familyTable.Columns["Familjenr"]] != null)
                  {
                    individual.AddRelation(new FamilyXrefClass(familyRow[familyTable.Columns["Familjenr"]].ToString()), IndividualClass.RelationType.Spouse);
                  }
                }

              }
            }

            if ((detailLevel & PersonDetail.PersonDetail_Children) != 0)
            {
              sqlString = "SELECT Familjenr FROM Barn WHERE Personnr = " + individual.GetXrefName();
              trace.TraceInformation("sql:[" + sqlString + "]");// + dataset.Tables.Count + "," + dataset.Tables[0].personRows[0].ToString() + "," + dataset.Tables[0].DataSet.DataSetName);
              adapter.SelectCommand = new OleDbCommand(sqlString, connection);

              //DataTable childrenTable = new DataTable();

              DataTable childrenTable = new DataTable();

              adapter.Fill(childrenTable);//dataset);

              if (childrenTable.Rows.Count > 0)
              {
                foreach (DataRow familyRow in childrenTable.Rows)
                {
                  if (trace.Switch.Level.HasFlag(SourceLevels.Information))
                  {
                    foreach (DataColumn familjerColumn in childrenTable.Columns)
                    {
                      trace.TraceInformation("  " + familjerColumn.ToString() + ":" + familyRow[familjerColumn]);
                    }
                  }

                  if (familyRow[childrenTable.Columns["Familjenr"]] != null)
                  {
                    individual.AddRelation(new FamilyXrefClass(familyRow[childrenTable.Columns["Familjenr"]].ToString()), IndividualClass.RelationType.Child);
                  }
                }

              }
            }


            if (trace.Switch.Level.HasFlag(SourceLevels.Information))
            {
              individual.Print();
            }

            CloseDbConnection();
            //connection.Close();
            //trace.TraceInformation("FamilyStoreAnarkiv::GetIndividual(" + xrefName + ")-end:" + DateTime.Now);
            return individual;
            //}
          

        }

        foreach (DataRow row in personTable.Rows)
        {
          trace.TraceInformation(row.ToString());
          foreach (DataColumn column in personTable.Columns)
          {
            trace.TraceInformation("  " + column.ToString() + ":" + row[column]);
          }
        }
      }
      CloseDbConnection();
      //connection.Close();
      trace.TraceInformation("FamilyStoreAnarkiv::GetIndividual(" + xrefName + ")-end-2");// + dataset.Tables.Count + "," + dataset.Tables[0].Rows[0].ToString() + "," + dataset.Tables[0].DataSet.DataSetName);
      return null;
    }

    public IEnumerator<IndividualClass> GetEnumerator()
    {
      trace.TraceInformation("GetEnumerator(): null");
      return null;
    }

    private void MakeEvent(ref IndividualEventClass ev, IndividualEventClass.EventType evType)
    {
      if(ev == null)
      {
        ev = new IndividualEventClass(evType);
      }
    }


    public IEnumerator<IndividualClass> SearchPerson(String individualName = null, ProgressReporterInterface progressReporter = null)
    {
      string searchString = "";
      IList<AddressClassOrder> addressHemmanList = null;
      IList<NoteOrder> noteList = null;

      if(individualName != null)
      {
        searchString = individualName.ToUpper();
      }
      trace.TraceInformation("FamilyStoreAnarkiv::SearchPerson(" + searchString + ")-start:" + DateTime.Now);

      String sqlString;
      OleDbConnection connection;

      connection = GetDbConnection();

      OleDbDataAdapter adapter = GetDbAdapter();

      string hemmanPersonnrName = null;
      string hemmanInsert = "";

      adapter.SelectCommand = new OleDbCommand("SELECT [2:Personnr] FROM 1:Hemman", connection);

      try
      {
        Object result = adapter.SelectCommand.ExecuteScalar();
        hemmanPersonnrName = "2:Personnr";
      }
      catch (Exception e)
      {
        trace.TraceData(TraceEventType.Error, 0, "OleDb query ask 1:");
        trace.TraceData(TraceEventType.Error, 0, "OleDb error:" + e.ToString());
      }

      adapter.SelectCommand = new OleDbCommand("SELECT [Personnr] FROM 1:Hemman", connection);

      try
      {
        Object result = adapter.SelectCommand.ExecuteScalar();
        hemmanPersonnrName = "Personnr";
      }
      catch (Exception e)
      {
        trace.TraceData(TraceEventType.Error, 0, "OleDb query ask 2:");
        trace.TraceData(TraceEventType.Error, 0, "OleDb error:" + e.ToString());
      }
      if(hemmanPersonnrName != null)
      {
        hemmanInsert = " LEFT OUTER JOIN 1:Hemman AS h ON p.Personnr = h.[" + hemmanPersonnrName + "]";
      }


      // The 1:Hemman table has a row called Personnr that may also be called 2:Personnr (in the database for Tony's ancestors)
      //sqlString = "SELECT p.Personnr,Förnamn,Efternamn,Datum,Källa,Församling,Adress,Dopdatum,Döddatum,DödFörsamling,DödAdress,DödKälla,DödKommentar,Begravning,Kön,f.Familjenr,f.Personnr,b.Familjenr,b.Personnr,f.Familjenr AS ParentFam,b.Familjenr AS BarnFam FROM (((((Personer AS p) LEFT OUTER JOIN Familjer AS f ON p.Personnr = f.Personnr) LEFT OUTER JOIN Barn AS b ON p.Personnr = b.Personnr) LEFT OUTER JOIN Löptext AS l ON p.Personnr = l.Personnr)  LEFT OUTER JOIN 1:Hemman AS h ON p.Personnr = h.[2:Personnr]) LEFT OUTER JOIN Notis AS n ON p.Personnr = n.Personnr";
      sqlString = "SELECT p.Personnr,Förnamn,Efternamn,Datum,Källa,Församling,Adress,Dopdatum,Döddatum,DödFörsamling,DödAdress,DödKälla,DödKommentar,Begravning,Kön,f.Familjenr,f.Personnr,b.Familjenr,b.Personnr,f.Familjenr AS ParentFam,b.Familjenr AS BarnFam FROM (((((Personer AS p) LEFT OUTER JOIN Familjer AS f ON p.Personnr = f.Personnr) LEFT OUTER JOIN Barn AS b ON p.Personnr = b.Personnr) LEFT OUTER JOIN Löptext AS l ON p.Personnr = l.Personnr) "+hemmanInsert+") LEFT OUTER JOIN Notis AS n ON p.Personnr = n.Personnr";

      if (searchString.Length > 0)
      {
        sqlString += " WHERE p.Efternamn LIKE '%" + searchString + "%' OR p.Förnamn LIKE '%" + searchString + "%'";
      }
      sqlString += " ORDER BY p.Personnr";


      
      trace.TraceInformation("sql:[" + sqlString + "]");// + dataset.Tables.Count + "," + dataset.Tables[0].Rows[0].ToString() + "," + dataset.Tables[0].DataSet.DataSetName);
      adapter.SelectCommand = new OleDbCommand(sqlString, connection);

      DataTable personTable = new DataTable();

      try {
        adapter.Fill(personTable);//dataset);
      }
      catch (Exception e)
      {
        trace.TraceData(TraceEventType.Error, 0, "OleDb query:" + sqlString);
        trace.TraceData(TraceEventType.Error, 0, "OleDb error:" + e.ToString());
      }


      //connection.Close();

      //trace.TraceInformation("GetIndividual:");// + dataset.Tables.Count + "," + dataset.Tables[0].Rows[0].ToString() + "," + dataset.Tables[0].DataSet.DataSetName);

      //trace.TraceInformation("dataset.Tables.Count=" + dataset.Tables.Count);

      //trace.TraceInformation("PersonTable.Rows.Count=" + personTable.Rows.Count);

      if (personTable.Rows.Count > 0)
      {
        int count = 0;
        //String prevPerson = "";
        IndividualClass individual = null;
        PersonalNameClass name = null;
        ProgressReporterClass progress = new ProgressReporterClass(personTable.Rows.Count);

        name = new PersonalNameClass();
        //trace.TraceInformation("Row:" + personRow.ToString());
        IndividualEventClass birthEvent = null;
        IndividualEventClass deathEvent = null;

        DataBasePostInfo postInfo = new DataBasePostInfo();

        trace.TraceInformation("Table:" + personTable.ToString() + ", columns.count=" + personTable.Columns.Count + ", rows.count=" + personTable.Rows.Count);

        foreach (DataRow personRow in personTable.Rows)
        {
          count++;

          /*if (count > 300)
          {
            //trace.TraceInformation("Decoding breaks at " + count + "/" + personTable.Rows.Count + " Name: " + personRow[personTable.Columns["Förnamn"]] + " " + personRow[personTable.Columns["Efternamn"]]);
            continue;
          }*/
          if (trace.Switch.Level.HasFlag(SourceLevels.Information))
          {
            if (count % 1000 == 0)
            {
              trace.TraceInformation("Decoding no " + count + "/" + personTable.Rows.Count + " Name: " + personRow[personTable.Columns["Förnamn"]] + " " + personRow[personTable.Columns["Efternamn"]]);
              //continue;
            }
          }
          if (progressReporter != null)
          {
            if (progress.Update(count))
            {
              progressReporter.ReportProgress(progress.GetPercent());
            }
          }


          if (postInfo.personId != personRow[personTable.Columns["p.Personnr"]].ToString())
          {

            if (individual != null)
            {
              if (addressHemmanList != null)
              {
                foreach (AddressClassOrder addressEvent in addressHemmanList)
                {
                  individual.AddEvent(addressEvent.address);
                }
                addressHemmanList = null;
              }
              if (noteList != null)
              {
                foreach (NoteOrder note in noteList)
                {
                  individual.AddNote(new NoteClass(note.note));
                }
                noteList = null;
              }

              if (trace.Switch.Level.HasFlag(SourceLevels.Information))
              {
                individual.Print();
                trace.TraceInformation("SearchPerson(" + searchString + ")-end-2");
              }
              yield return (IndividualClass)individual;

              individual = new IndividualClass();
              name = new PersonalNameClass();
              //trace.TraceInformation("Row:" + personRow.ToString());
              birthEvent = null;
              deathEvent = null;
              postInfo = new DataBasePostInfo();

            }
            else
            {
              // First iteration..
              individual = new IndividualClass();
            }


            
            // Name...
            if ((personRow[personTable.Columns["Förnamn"]] != null) && !personRow.IsNull("Förnamn") && ((string)(personRow[personTable.Columns["Förnamn"]]) != ""))
            {
              //trace.TraceInformation("SetName.given:" + personRow[personTable.Columns["Förnamn"]]);
              name.SetName(PersonalNameClass.PartialNameType.GivenName, personRow[personTable.Columns["Förnamn"]].ToString());              
            }
            if ((personRow[personTable.Columns["Efternamn"]] != null) && !personRow.IsNull("Efternamn") && ((string)(personRow[personTable.Columns["Efternamn"]]) != ""))
            {
              name.SetName(PersonalNameClass.PartialNameType.Surname, personRow[personTable.Columns["Efternamn"]].ToString());
            }

            // Birth...
            if ((personRow[personTable.Columns["Datum"]] != null) && !personRow.IsNull("Datum") && ((string)(personRow[personTable.Columns["Datum"]]) != ""))
            {
              //individual.AddEvent(IndividualEventClass.EventType.Birth, ParseAnarkivDate(personRow[personTable.Columns["Datum"]].ToString()));
              MakeEvent(ref birthEvent, IndividualEventClass.EventType.Birth);
              birthEvent.SetDate(ParseAnarkivDate(personRow[personTable.Columns["Datum"]].ToString()));
            }
            if ((personRow[personTable.Columns["Adress"]] != null) && !personRow.IsNull("Adress") && ((string)(personRow[personTable.Columns["Adress"]]) != ""))
            {
              //individual.AddEvent(IndividualEventClass.EventType.Birth, ParseAnarkivDate(personRow[personTable.Columns["Datum"]].ToString()));
              MakeEvent(ref birthEvent, IndividualEventClass.EventType.Birth);
              birthEvent.AddAddressPart(AddressPartClass.AddressPartType.StreetAddress, personRow[personTable.Columns["Adress"]].ToString());
            }
            if ((personRow[personTable.Columns["Församling"]] != null) && !personRow.IsNull("Församling") && ((string)(personRow[personTable.Columns["Församling"]]) != ""))
            {
              //individual.AddEvent(IndividualEventClass.EventType.Birth, ParseAnarkivDate(personRow[personTable.Columns["Datum"]].ToString()));
              MakeEvent(ref birthEvent, IndividualEventClass.EventType.Birth);
              birthEvent.AddAddressPart(AddressPartClass.AddressPartType.City, personRow[personTable.Columns["Församling"]].ToString());
            }
            if ((personRow[personTable.Columns["Källa"]] != null) && !personRow.IsNull("Källa") && ((string)(personRow[personTable.Columns["Källa"]]) != ""))
            {
              MakeEvent(ref birthEvent, IndividualEventClass.EventType.Birth);
              birthEvent.AddSource(new SourceDescriptionClass(personRow[personTable.Columns["Källa"]].ToString()));
            }

            // Christening...
            if ((personRow[personTable.Columns["DopDatum"]] != null) && !personRow.IsNull("DopDatum") && ((string)(personRow[personTable.Columns["DopDatum"]]) != ""))
            {
              individual.AddEvent(IndividualEventClass.EventType.Baptism, ParseAnarkivDate(personRow[personTable.Columns["DopDatum"]].ToString()));
            }

            // Death...
            if ((personRow[personTable.Columns["DödDatum"]] != null) && !personRow.IsNull("DödDatum") && ((string)(personRow[personTable.Columns["DödDatum"]]) != ""))
            {
              MakeEvent(ref deathEvent, IndividualEventClass.EventType.Death);
              deathEvent.SetDate(ParseAnarkivDate(personRow[personTable.Columns["DödDatum"]].ToString()));
              //IndividualEventClass ev = new IndividualEventClass(IndividualEventClass.EventType.Death, ParseAnarkivDate(personRow[personTable.Columns["DödDatum"]].ToString()));
            }
            if ((personRow[personTable.Columns["DödAdress"]] != null) && !personRow.IsNull("DödAdress") && ((string)(personRow[personTable.Columns["DödAdress"]]) != ""))
            {
              MakeEvent(ref deathEvent, IndividualEventClass.EventType.Death);
              deathEvent.AddAddressPart(AddressPartClass.AddressPartType.StreetAddress, personRow[personTable.Columns["DödAdress"]].ToString());
            }
            if ((personRow[personTable.Columns["DödFörsamling"]] != null) && !personRow.IsNull("DödFörsamling") && ((string)(personRow[personTable.Columns["DödFörsamling"]]) != ""))
            {
              MakeEvent(ref deathEvent, IndividualEventClass.EventType.Death);
              deathEvent.AddAddressPart(AddressPartClass.AddressPartType.City, personRow[personTable.Columns["DödFörsamling"]].ToString());
            }
            if ((personRow[personTable.Columns["DödKälla"]] != null) && !personRow.IsNull("DödKälla") && ((string)(personRow[personTable.Columns["DödKälla"]]) != ""))
            {
              MakeEvent(ref deathEvent, IndividualEventClass.EventType.Death);
              deathEvent.AddSource(new SourceDescriptionClass(personRow[personTable.Columns["DödKälla"]].ToString()));
            }
            if ((personRow[personTable.Columns["DödKommentar"]] != null) && !personRow.IsNull("DödKommentar") && ((string)(personRow[personTable.Columns["DödKommentar"]]) != ""))
            {
              MakeEvent(ref deathEvent, IndividualEventClass.EventType.Death);
              deathEvent.AddNote(new NoteClass(personRow[personTable.Columns["DödKommentar"]].ToString()));
            }

            // Funeral...
            if ((personRow[personTable.Columns["Begravning"]] != null) && !personRow.IsNull("Begravning") && ((string)(personRow[personTable.Columns["Begravning"]]) != ""))
            {
              individual.AddEvent(IndividualEventClass.EventType.Burial, ParseAnarkivDate(personRow[personTable.Columns["Begravning"]].ToString()));
            }
            if (personRow[personTable.Columns["p.Personnr"]] != null)
            {
              individual.SetXrefName(personRow[personTable.Columns["p.Personnr"]].ToString());
              postInfo.personId = personRow[personTable.Columns["p.Personnr"]].ToString();
            }
            if (personTable.Columns["Löptext"] != null)
            {
              if ((personRow[personTable.Columns["Löptext"]] != null) && !personRow.IsNull("Löptext") && ((string)(personRow[personTable.Columns["Löptext"]]) != ""))
              {
                individual.AddNote(new NoteClass(personRow[personTable.Columns["Löptext"]].ToString().Normalize()));
              }
            }

            individual.SetPersonalName(name);

            if (personTable.Columns["Kön"] != null)
            {
              if (personRow[personTable.Columns["Kön"]] != null)
              {
                switch (personRow[personTable.Columns["Kön"]].ToString())
                {
                  case "M":
                    individual.SetSex(IndividualClass.IndividualSexType.Male);
                    break;
                  case "Q":
                    individual.SetSex(IndividualClass.IndividualSexType.Female);
                    break;
                  default:
                    //individual.SetSex(IndividualClass.IndividualSexType.Male);
                    break;
                }
              }
            }

            if(birthEvent != null)
            {
              individual.AddEvent(birthEvent);
              birthEvent = null;
            }
            if(deathEvent != null)
            {
              individual.AddEvent(deathEvent);
              deathEvent = null;
            }





            /*sqlString = "SELECT Familjenr FROM Familjer WHERE Personnr = " + individual.GetXrefName();
            trace.TraceInformation("sql:[" + sqlString + "]");// + dataset.Tables.Count + "," + dataset.Tables[0].personRows[0].ToString() + "," + dataset.Tables[0].DataSet.DataSetName);
            adapter.SelectCommand = new OleDbCommand(sqlString, connection);

            DataTable familyTable = new DataTable();

            adapter.Fill(familyTable);//dataset);

            if (familyTable.Rows.Count > 0)
            {
              foreach (DataRow familyRow in familyTable.Rows)
              {
                if (trace.Switch.Level.HasFlag(SourceLevels.Information))
                {
                  foreach (DataColumn familjerColumn in familyTable.Columns)
                  {
                    trace.TraceInformation("  " + familjerColumn.ToString() + ":" + familyRow[familjerColumn]);
                  }
                }
                if (familyRow[familyTable.Columns["Familjenr"]] != null)
                {
                  individual.AddRelation(new FamilyXrefClass(familyRow[familyTable.Columns["Familjenr"]].ToString()), IndividualClass.RelationType.Spouse);
                }
              }

            }*/


            /*if (trace.Switch.Level.HasFlag(SourceLevels.Information))
            {
              individual.Print();
            }

            yield return (IndividualClass)individual;*/
            //return individual;
            //}
          } // if Person != oldPerson

          if (individual.GetSex() != IndividualClass.IndividualSexType.Unknown)
          {
            if (personTable.Columns["ParentFam"] != null)
            {
              if ((personRow[personTable.Columns["ParentFam"]] != null) && (personRow[personTable.Columns["ParentFam"]].ToString() != "") && (!postInfo.IsFamilyAdded(true, personRow[personTable.Columns["ParentFam"]].ToString())))
              {
                individual.AddRelation(new FamilyXrefClass(personRow[personTable.Columns["ParentFam"]].ToString()), IndividualClass.RelationType.Spouse);
                //prevPerson = personRow[personTable.Columns["Personnr"]].ToString();
                postInfo.familySpouseId.Add(personRow[personTable.Columns["ParentFam"]].ToString());
              }
            }
            if (personTable.Columns["BarnFam"] != null)
            {
              if ((personRow[personTable.Columns["BarnFam"]] != null) && (personRow[personTable.Columns["BarnFam"]].ToString() != "") && (!postInfo.IsFamilyAdded(false, personRow[personTable.Columns["BarnFam"]].ToString())))
              {
                individual.AddRelation(new FamilyXrefClass(personRow[personTable.Columns["BarnFam"]].ToString()), IndividualClass.RelationType.Child);
                //prevPerson = personRow[personTable.Columns["Personnr"]].ToString();
                postInfo.familyChildId.Add(personRow[personTable.Columns["BarnFam"]].ToString());
              }
            }
          }
          else // unknown sex.. (???)
          {
            if (personTable.Columns["ParentFam"] != null)
            {
              if ((personRow[personTable.Columns["ParentFam"]] != null) && (personRow[personTable.Columns["ParentFam"]].ToString() != "") && (!postInfo.IsFamilyAdded(true, personRow[personTable.Columns["ParentFam"]].ToString())))
              {
                individual.AddRelation(new FamilyXrefClass(personRow[personTable.Columns["ParentFam"]].ToString()), IndividualClass.RelationType.Spouse);
                //prevPerson = personRow[personTable.Columns["Personnr"]].ToString();
                postInfo.familySpouseId.Add(personRow[personTable.Columns["ParentFam"]].ToString());
              }
            }
            if (personTable.Columns["BarnFam"] != null)
            {
              if ((personRow[personTable.Columns["BarnFam"]] != null) && (personRow[personTable.Columns["BarnFam"]].ToString() != "") && (!postInfo.IsFamilyAdded(false, personRow[personTable.Columns["BarnFam"]].ToString())))
              {
                individual.AddRelation(new FamilyXrefClass(personRow[personTable.Columns["BarnFam"]].ToString()), IndividualClass.RelationType.Child);
                //prevPerson = personRow[personTable.Columns["Personnr"]].ToString();
                postInfo.familyChildId.Add(personRow[personTable.Columns["BarnFam"]].ToString());
              }
            }
          }

          if (personTable.Columns["HemmanOrdningsnr"] != null)
          {
            // Hemman...
            if ((personRow[personTable.Columns["HemmanOrdningsnr"]] != null) && !personRow.IsNull("HemmanOrdningsnr") && (personRow[personTable.Columns["HemmanOrdningsnr"]].ToString() != ""))
            {
              AddressClassOrder address = null;
              int ordningsNr;

              ordningsNr = Convert.ToInt32(personRow[personTable.Columns["HemmanOrdningsnr"]].ToString());

              // Only add new adress if it's not already added..
              if ((address = GetAddressOrder(ref addressHemmanList, ordningsNr)) == null)
              {
                address = new AddressClassOrder(ordningsNr);

                if ((personRow[personTable.Columns["Hemman"]] != null) && !personRow.IsNull("Hemman") && ((string)(personRow[personTable.Columns["Hemman"]]) != ""))
                {
                  address.address.AddAddressPart(new AddressPartClass(AddressPartClass.AddressPartType.Line1, personRow[personTable.Columns["Hemman"]].ToString().Normalize()));
                }
                if ((personRow[personTable.Columns["HemmanMantal"]] != null) && !personRow.IsNull("HemmanMantal") && ((string)(personRow[personTable.Columns["HemmanMantal"]]) != ""))
                {
                  address.address.AddNote(new NoteClass(personRow[personTable.Columns["HemmanMantal"]].ToString().Normalize()));
                }
                if ((personRow[personTable.Columns["HemmanPeriod"]] != null) && !personRow.IsNull("HemmanPeriod") && ((string)(personRow[personTable.Columns["HemmanPeriod"]]) != ""))
                {
                  address.address.SetDate(new FamilyDateTimeClass(personRow[personTable.Columns["HemmanPeriod"]].ToString().Normalize()));
                }
                if (addressHemmanList == null)
                {
                  addressHemmanList = new List<AddressClassOrder>();
                }
                addressHemmanList.Add(address);
              }
            }
          }

          if (personTable.Columns["NotisOrdning"] != null)
          {
            if (!personRow.IsNull("NotisOrdning") && (personRow[personTable.Columns["NotisOrdning"]] != null) && (personRow[personTable.Columns["NotisOrdning"]].ToString() != ""))
            {
              NoteOrder note = null;
              int ordningsNr;

              ordningsNr = Convert.ToInt32(personRow[personTable.Columns["NotisOrdning"]].ToString());

              // Only add new adress if it's not already added..
              if ((note = GetNoteOrder(ref noteList, ordningsNr)) == null)
              {
                note = new NoteOrder(ordningsNr);

                if ((personRow[personTable.Columns["Notis"]] != null) && !personRow.IsNull("Notis") && ((string)(personRow[personTable.Columns["Notis"]]) != ""))
                {
                  note.note = personRow[personTable.Columns["Notis"]].ToString().Normalize();
                }

                if (noteList == null)
                {
                  noteList = new List<NoteOrder>();
                }
                noteList.Add(note);
              }
            }
          }



        }
        if (individual != null)
        {
          if (addressHemmanList != null)
          {
            foreach (AddressClassOrder addressEvent in addressHemmanList)
            {
              individual.AddEvent(addressEvent.address);
            }
            addressHemmanList = null;
          }
          if (noteList != null)
          {
            foreach (NoteOrder note in noteList)
            {
              individual.AddNote(new NoteClass(note.note));
            }
            noteList = null;
          }

          if (trace.Switch.Level.HasFlag(SourceLevels.Information))
          {
            individual.Print();
            trace.TraceInformation("SearchPerson(" + searchString + ")-end-3");
          }
          yield return (IndividualClass)individual;


        }


        /*foreach (DataRow row in personTable.Rows)
        {
          trace.TraceInformation(row.ToString());
          foreach (DataColumn column in personTable.Columns)
          {
            trace.TraceInformation("  " + column.ToString() + ":" + row[column]);
          }
        }*/
      }
      //return null;
      CloseDbConnection();
      //connection.Close();
      trace.TraceInformation("FamilyStoreAnarkiv::SearchPerson(" + searchString + ")-end:" + DateTime.Now);
    }

    private void CheckMarriage(ref IndividualEventClass marriage)
    {
      if(marriage == null)
      {
        marriage = new IndividualEventClass(IndividualEventClass.EventType.FamMarriage);
      }
    }
    public void SetHomeIndividual(String xrefName)
    {

    }
    public string GetHomeIndividual()
    {
      return null;
    }

    public IEnumerator<FamilyClass> SearchFamily(String familyXrefName = null, ProgressReporterInterface progressReporter = null)
    {
      trace.TraceInformation("SearchFamily_ddb(" + familyXrefName + ")-start:" + DateTime.Now);

      OleDbConnection connection = GetDbConnection();
      OleDbDataAdapter adapter = GetDbAdapter();

      //sqlString = "SELECT p.Personnr,Förnamn,Efternamn,Datum,Källa,Församling,Adress,Dopdatum,Döddatum,DödFörsamling,DödAdress,DödKälla,DödKommentar,Begravning,Kön,f.Familjenr 
      //             AS ParentFam,b.Familjenr AS BarnFam,l.Löptext AS Löptext,h.Ordningsnr AS HemmanOrdningsnr,h.Hemman AS Hemman,h.Mantal AS HemmanMantal,h.Period AS HemmanPeriod,n.Notis AS Notis,n.Ordning AS NotisOrdning FROM 
      //             (((((Personer AS p) LEFT OUTER JOIN Familjer AS f ON p.Personnr = f.Personnr) 
      //                LEFT OUTER JOIN Barn AS b ON p.Personnr = b.Personnr) 
      //                LEFT OUTER JOIN Löptext AS l ON p.Personnr = l.Personnr) 
      //                LEFT OUTER JOIN 1:Hemman AS h ON p.Personnr = h.Personnr) 
      //                LEFT OUTER JOIN Notis AS n ON p.Personnr = n.Personnr"; 

      String sqlString;
      //sqlString = "SELECT f.Personnr AS Parent,f.Familjenr,b.Personnr AS Child FROM ((Familjer AS f) LEFT OUTER JOIN Barn AS b ON b.Familjenr = f.Familjenr) ORDER BY f.Familjenr,f.Personnr,b.Personnr";
      //sqlString = "SELECT f.Personnr AS Parent,f.Familjenr,
      //                    b.Personnr AS Child,
      //                    v.Datum AS Datum,v.Församling AS Församling,v.Kommentar AS Kommentar,v.Källa AS Källa 
      //                    FROM 
      //                    (((Familjer AS f) 
      //                      LEFT OUTER JOIN Barn AS b ON b.Familjenr = f.Familjenr) 
      //                      LEFT OUTER JOIN Vigslar AS v ON v.Familjenr = f.Familenr) 
      //                      ORDER BY f.Familjenr,f.Personnr,b.Personnr";


      sqlString = "SELECT f.Personnr AS Parent,f.Familjenr,b.Personnr AS Child,v.Datum AS Datum,v.Församling AS Församling,v.Kommentar AS Kommentar,v.Källa AS Källa FROM (((Familjer AS f) LEFT OUTER JOIN Barn AS b ON b.Familjenr = f.Familjenr) LEFT OUTER JOIN Vigslar AS v ON v.Familjenr = f.Familjenr) ORDER BY f.Familjenr,f.Personnr,b.Personnr";
      //sqlString = "SELECT f.Personnr AS Parent,f.Familjenr,b.Personnr AS Child FROM (((Familjer AS f) LEFT OUTER JOIN Barn AS b ON b.Familjenr = f.Familjenr) LEFT OUTER JOIN Vigslar AS v ON v.Familjenr = f.Familenr) ORDER BY f.Familjenr,f.Personnr,b.Personnr";
      if ((familyXrefName != null) && (familyXrefName != ""))
      {
        sqlString += " WHERE Familjenr = " + familyXrefName;
      }
      trace.TraceInformation("SearchFamily:sql:[" + sqlString + "]");// + dataset.Tables.Count + "," + dataset.Tables[0].Rows[0].ToString() + "," + dataset.Tables[0].DataSet.DataSetName);
      adapter.SelectCommand = new OleDbCommand(sqlString, connection);

      DataTable familyTable = new DataTable();
      adapter.Fill(familyTable);//dataset);

      if (familyTable.Rows.Count > 0)
      {
        int count = 0;
        FamilyClass family = null;
        string familyXref = "";
        string parentXref = "";
        //string childXref = "";
        string firstChildXref = "";
        string previousPlace = "";
        string previousComment = "";
        string previousSource = "";
        bool addChildren = true;
        ProgressReporterClass progress = new ProgressReporterClass(familyTable.Rows.Count);
        IndividualEventClass marriage = null;

        trace.TraceInformation("familyTable:" + familyTable.ToString() + ", columns.count=" + familyTable.Columns.Count + ", rows.count=" + familyTable.Rows.Count);

        foreach (DataRow familyRow in familyTable.Rows)
        {
          //PersonalNameClass name = new PersonalNameClass();
          //trace.TraceInformation("Row[" + count + ":" + familyRow[familyTable.Columns["Familjenr"]].ToString() + " child:" + familyRow[familyTable.Columns["Child"]] + " parent:" + familyRow[familyTable.Columns["Parent"]].ToString());

          count++;
          if (trace.Switch.Level.HasFlag(SourceLevels.Information))
          {
            if (count % 1000 == 0)
            {
              trace.TraceInformation("SearchFamily-ddb: .Family: " + familyXref + "==" + familyRow[familyTable.Columns["Familjenr"]].ToString() + ", (" + count + "/" + familyTable.Rows.Count + "), " + DateTime.Now);
            }
          }
          if (progressReporter != null)
          {
            if (progress.Update(count))
            {
              progressReporter.ReportProgress(progress.GetPercent());
            }
          }

          if ((family == null) || (familyRow[familyTable.Columns["Familjenr"]].ToString() != family.GetXrefName()))
          {
            if (family != null)
            {
              if (marriage != null)
              {
                family.AddEvent(marriage);
                marriage = null;
              }
              //trace.TraceInformation("ana.Family return:" + family.GetXrefName());
              yield return family;
              family = null;
              //childXref = "";
              firstChildXref = "";
              parentXref = "";
              addChildren = true;
              previousPlace = "";
              previousComment = "";
              previousSource = "";
            }
            family = new FamilyClass();

            if (familyRow[familyTable.Columns["Familjenr"]] != null)
            {
              //trace.TraceInformation("ana.family:" + familyRow[familyTable.Columns["Familjenr"]].ToString());
              family.SetXrefName(familyRow[familyTable.Columns["Familjenr"]].ToString());
              familyXref = familyRow[familyTable.Columns["Familjenr"]].ToString();
            }
          }
          else
          {
          }
          if ((family != null) && !familyRow.IsNull("Parent"))
          {
            if (parentXref != familyRow[familyTable.Columns["Parent"]].ToString())
            {

              //trace.TraceInformation("ana.family:"+ family.GetXrefName() + "parent:" + familyRow[familyTable.Columns["Parent"]].ToString());
              family.AddRelation(new IndividualXrefClass(familyRow[familyTable.Columns["Parent"]].ToString()), FamilyClass.RelationType.Parent);
              parentXref = familyRow[familyTable.Columns["Parent"]].ToString();
            }
          }
          if ((family != null) && !familyRow.IsNull("Child"))
          {
            if (firstChildXref == "")
            {
              firstChildXref = familyRow[familyTable.Columns["Child"]].ToString();
            }
            else if (firstChildXref == familyRow[familyTable.Columns["Child"]].ToString())
            {
              // We have added all children using one parent..no need to add them again..
              addChildren = false;
            }
            if (addChildren)
            {
              //trace.TraceInformation("ana.family:" + family.GetXrefName() + "child:" + familyRow[familyTable.Columns["Child"]].ToString());
              family.AddRelation(new IndividualXrefClass(familyRow[familyTable.Columns["Child"]].ToString()), FamilyClass.RelationType.Child);
            }
          }
          if ((family != null) && !familyRow.IsNull("Datum") && (familyRow[familyTable.Columns["Datum"]].ToString() != ""))
          {
            CheckMarriage(ref marriage);
            marriage.SetDate(ParseAnarkivDate(familyRow[familyTable.Columns["Datum"]].ToString()));
          }
          if ((family != null) && !familyRow.IsNull("Församling"))
          {
            if (previousPlace != familyRow[familyTable.Columns["Församling"]].ToString())
            {
              CheckMarriage(ref marriage);
              marriage.AddAddressPart(AddressPartClass.AddressPartType.City, familyRow[familyTable.Columns["Församling"]].ToString());
              previousPlace = familyRow[familyTable.Columns["Församling"]].ToString();
            }
          }
          if ((family != null) && !familyRow.IsNull("Kommentar") && (familyRow[familyTable.Columns["Kommentar"]].ToString() != ""))
          {
            if (previousComment != familyRow[familyTable.Columns["Kommentar"]].ToString())
            {
              CheckMarriage(ref marriage);
              marriage.AddNote(new NoteClass(familyRow[familyTable.Columns["Kommentar"]].ToString()));
              previousComment = familyRow[familyTable.Columns["Kommentar"]].ToString();
            }
          }
          if ((family != null) && !familyRow.IsNull("Källa"))
          {
            if (previousSource != familyRow[familyTable.Columns["Källa"]].ToString())
            {
              CheckMarriage(ref marriage);
              marriage.AddSource(new SourceDescriptionClass(familyRow[familyTable.Columns["Källa"]].ToString()));
              previousSource = familyRow[familyTable.Columns["Källa"]].ToString();
            }
          }
        }
        if (family != null)
        {
          if (marriage != null)
          {
            family.AddEvent(marriage);
            marriage = null;
          }
          trace.TraceInformation("FamilyStoreAnarkiv::SearchFamily(" + familyXrefName + ")-end-1:" + DateTime.Now);
          yield return family;
          family = null;
        }

      }
      CloseDbConnection();
      //connection.Close();
      //return null;
      trace.TraceInformation("FamilyStoreAnarkiv::SearchFamily(" + familyXrefName + ")-end-2:" + DateTime.Now);
    }

    public IEnumerator<FamilyClass> SearchFamily_ram(String familyXrefName = null, ProgressReporterInterface progressReporter = null)
    {
      trace.TraceInformation("SearchFamily-ram(" + familyXrefName + ")-start:" + DateTime.Now);
      //AnarkivDataSet dataset = new AnarkivDataSet();

      int count = 0;
      //int childNo = 0;
      //int parentNo = 0;
      CacheFamilies();

      IEnumerator<KeyValuePair<int,AnarkivFamilyRelations>> anarkivFamily = familyMapper.GetEnumerator();

      ProgressReporterClass progress = new ProgressReporterClass(familyMapper.Count);

      while(anarkivFamily.MoveNext())
      {
        FamilyClass family = new FamilyClass();
        AnarkivFamilyRelations thisFamily = (AnarkivFamilyRelations)anarkivFamily.Current.Value;

        count++;
        if (trace.Switch.Level.HasFlag(SourceLevels.Information))
        {
          if (count % 1000 == 0)
          {
            trace.TraceInformation("SearchFamily-ramcache: .Family: (" + count + "/" + familyMapper.Count + "), " + DateTime.Now);
          }
        }
        if (progressReporter != null)
        {
          if (progress.Update(count))
          {
            progressReporter.ReportProgress(progress.GetPercent());
          }
        }

        family.SetXrefName(thisFamily.familyId.ToString());

        foreach (int childId in thisFamily.children)
        {
          family.AddRelation(new IndividualXrefClass(childId.ToString()), FamilyClass.RelationType.Child);
          //childNo++;
        }
        foreach (int parentId in thisFamily.parents)
        {
          family.AddRelation(new IndividualXrefClass(parentId.ToString()), FamilyClass.RelationType.Parent);
          //parentNo++;
        }
        yield return family;
      }
      //connection.Close();
      //return null;
      trace.TraceInformation("FamilyStoreAnarkiv::SearchFamily-ram(" + familyXrefName + ")-end-2:" + DateTime.Now);
    }

    public void AddMultimediaObject(MultimediaObjectClass tempMultimediaObject)
    {
    }

    public IEnumerator<MultimediaObjectClass> SearchMultimediaObject(String mmoString = null, ProgressReporterInterface progressReporter = null)
    {
      return null;
    }

    public void AddNote(NoteClass tempNote)
    {
    }

    public NoteClass GetNote(String xrefName)
    {
      return null;
    }
    public IEnumerator<NoteClass> SearchNote(String noteString = null, ProgressReporterInterface progressReporter = null)
    {
      return null;
    }

    public void AddRepository(RepositoryClass tempRepository)
    {
    }

    public IEnumerator<RepositoryClass> SearchRepository(String repositoryString = null, ProgressReporterInterface progressReporter = null)
    {
      return null;
    }

    public void AddSource(SourceClass tempSource)
    {
    }

    public IEnumerator<SourceClass> SearchSource(String sourceString = null, ProgressReporterInterface progressReporter = null)
    {
      return null;
    }

    public void AddSubmission(SubmissionClass tempSubmission)
    {
    }
    public IEnumerator<SubmissionClass> SearchSubmission(String submissionString = null, ProgressReporterInterface progressReporter = null)
    {
      return null;
    }
    public void AddSubmitter(SubmitterClass tempSubmitter)
    {
    }

    public void SetSubmitterXref(SubmitterXrefClass tempSubmitterXref)
    {
    }
    public IEnumerator<SubmitterClass> SearchSubmitter(String noteString = null, ProgressReporterInterface progressReporter = null)
    {
      return null;
    }


    public string CreateNewXref(XrefType type)
    {
      

      return "";
    }

    public void SetSourceFileType(String type)
    {
    }
    public void SetSourceFileTypeVersion(String version)
    {
    }
    public void SetSourceFileTypeFormat(String format)
    {
    }
    public void SetSourceFileName(String file)
    {
      sourceFileName = file;
    }
    public string GetSourceFileName()
    {
      return sourceFileName;
    }

    public void SetSourceName(String source)
    {
    }

    public void SetCharacterSet(FamilyTreeCharacterSet charSet)
    {
    }

    public void SetDate(FamilyDateTimeClass inDate)
    {
    }

    public void Print()
    {
      FamilyTreeContentClass contents = GetContents();

      trace.TraceInformation("Tree Overview:");
      trace.TraceInformation(" Families:     " + contents.families);
      trace.TraceInformation(" Individuals:  " + contents.individuals);
      trace.TraceInformation(" Notes:        " + contents.notes);
      trace.TraceInformation(" Sources:      " + contents.sources);
      trace.TraceInformation(" Submitters:   " + contents.submitters);
      trace.TraceInformation(" Repositories: " + contents.repositories);
      trace.TraceInformation(" Submissions:  " + contents.submissions);
      trace.TraceInformation(" Multimedia:   " + contents.multimediaObjects);
    }
    public void PrintShort()
    {
    }

    public String GetShortTreeInfo()
    {
      FamilyTreeContentClass contents = GetContents();
      return "I:" + contents.individuals + " F:" + contents.families + " N:" + contents.notes;
    }

    public bool ValidateFamilies()
    {
      return false;
    }

    public bool ValidateIndividuals()
    {
      return false;
    }

    public bool ValidateTree()
    {
      return false;
    }

    public FamilyTreeContentClass GetContents()
    {
      FamilyTreeContentClass contents = new FamilyTreeContentClass();
      String sqlString;

      OleDbDataAdapter adapter = GetDbAdapter();

      sqlString = "SELECT COUNT(Personnr) FROM Personer";

      OleDbConnection connection;
      OleDbCommand command;
      connection = GetDbConnection();

      trace.TraceInformation("sql:[" + sqlString + "]");// + dataset.Tables.Count + "," + dataset.Tables[0].Rows[0].ToString() + "," + dataset.Tables[0].DataSet.DataSetName);
      command = new OleDbCommand(sqlString, connection);

      contents.individuals = (int)command.ExecuteScalar();

      command = null;

      // COUNT(DISTINCT not supported in MS Access...
      sqlString = "SELECT COUNT(Familjenr) FROM Familjer";

      trace.TraceInformation("sql:[" + sqlString + "]");// + dataset.Tables.Count + "," + dataset.Tables[0].personRows[0].ToString() + "," + dataset.Tables[0].DataSet.DataSetName);

      command = new OleDbCommand(sqlString, connection);

      contents.families = (int)command.ExecuteScalar();

      command = null;

      // COUNT(DISTINCT not supported in MS Access...
      sqlString = "SELECT COUNT(Personnr) FROM Löptext";

      trace.TraceInformation("sql:[" + sqlString + "]");// + dataset.Tables.Count + "," + dataset.Tables[0].personRows[0].ToString() + "," + dataset.Tables[0].DataSet.DataSetName);

      command = new OleDbCommand(sqlString, connection);

      contents.notes = (int)command.ExecuteScalar();
      CloseDbConnection();
      //connection.Close();
      trace.TraceInformation("families:" + contents.families + " indi:" + contents.individuals + " notes:" + contents.notes);// + dataset.Tables.Count + "," + dataset.Tables[0].personRows[0].ToString() + "," + dataset.Tables[0].DataSet.DataSetName);


      //contents.families = familyList.Count;
      contents.repositories = 0;
      contents.sources = 0;
      contents.submissions = 0;
      contents.submitters = 0;
      contents.multimediaObjects = 0;

      return contents;
    }

    public void SetFile(String fileName)
    {
      m_fileName = fileName;
    }

    // Private

    private OleDbConnection GetDbConnection()
    {
      DatabaseInstance dbi = GetDbInstance();
      trace.TraceInformation("GetDbConnection({0}):{1} {2}", dbi.connectionUsers, DateTime.Now, Thread.CurrentThread.ManagedThreadId);
      if (dbi.connectionUsers++ == 0)
      {
        trace.TraceInformation("GetDbConnection({0}):{1} {2} {3}", dbi.connectionUsers, DateTime.Now, Thread.CurrentThread.ManagedThreadId, dbi.connection.State);
        // We could add try/catch here, trying both OLEDB 4.0 and 12.0 database connection strings, if we want to be kind...
        if (dbi.connection.State == ConnectionState.Closed)
        {
          try
          {
            dbi.connection.Open();
          }
          catch(Exception e)
          {
            trace.TraceData(TraceEventType.Error, 0, "Error opening MS Access database:" + e);
            trace.TraceData(TraceEventType.Error, 0, "Error opening MS Access database:" + dbi.connection.ConnectionString);
            trace.TraceData(TraceEventType.Error, 0, "Perhaps the OLEDB driver for Access is not installed on the machine?");
          }
        }
        else
        {
          trace.TraceData(TraceEventType.Warning, 0, "GetDbConnection() users == 0, but open({0}):{1} {2} {3}", dbi.connectionUsers, DateTime.Now, Thread.CurrentThread.ManagedThreadId);
        }
      }
      UpdateDbInstance(dbi);
      return dbi.connection;
    }
    private void UpdateDbInstance(DatabaseInstance  dbi)
    {
      if (adapterMapper.ContainsKey(Thread.CurrentThread.ManagedThreadId))
      {
        adapterMapper[Thread.CurrentThread.ManagedThreadId] = dbi;
      }
      else
      {
        trace.TraceData(TraceEventType.Warning, 0, "UpdateDbInstance() thread not found" + dbi.connectionUsers + " " + DateTime.Now + " " + Thread.CurrentThread.ManagedThreadId);
      }
    }
    private DatabaseInstance GetDbInstance()
    {
      if (adapterMapper.ContainsKey(Thread.CurrentThread.ManagedThreadId))
      {
        return adapterMapper[Thread.CurrentThread.ManagedThreadId];
      }
      else
      {
        DatabaseInstance dbi = new DatabaseInstance();
        dbi.connection = new OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0; Data Source=" + m_fileName);//User ID=\"Admin;NewValue=Bad\"");
        dbi.adapter = new OleDbDataAdapter();
        dbi.threadId = Thread.CurrentThread.ManagedThreadId;
        dbi.connectionUsers = 0;
        adapterMapper.Add(Thread.CurrentThread.ManagedThreadId, dbi);
        return dbi;
      }
    }
    private void CloseDbConnection()
    {
      DatabaseInstance dbi = GetDbInstance();
      trace.TraceInformation("CloseDbConnection({0}):{1}", dbi.connectionUsers, DateTime.Now);

      if (dbi.connectionUsers > 0)
      {
        dbi.connectionUsers--;
        if (dbi.connectionUsers == 0)
        {
          dbi.connection.Close();
        }
      }
      else
      {
        trace.TraceEvent(TraceEventType.Error, 0, "error closing un-open connection!" + Thread.CurrentThread.ManagedThreadId);
      }
      UpdateDbInstance(dbi);
    
    }
    private OleDbDataAdapter GetDbAdapter()
    {
      DatabaseInstance dbi = GetDbInstance();
      return dbi.adapter;
    }
  }
}

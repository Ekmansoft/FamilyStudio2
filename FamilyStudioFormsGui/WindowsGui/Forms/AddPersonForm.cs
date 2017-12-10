using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FamilyStudioData.FamilyData;

namespace FamilyStudioFormsGui.WindowsGui.Forms
{
  public partial class AddPersonForm : Form
  {
    private static TraceSource trace = new TraceSource("AddPersonForm", SourceLevels.Warning);
    private CustomClass propertyList = new CustomClass();
    private IndividualClass selectedPerson;
    private System.EventHandler eventHandler;

    private enum PersonPropertySex
    {
      Unknown,
      Male,
      Female
    }

    public AddPersonForm()
    {
      InitializeComponent();

      CustomProperty nameCat = new CustomProperty("Name");
      CustomProperty personCat = new CustomProperty("Person");
      CustomProperty eventsCat = new CustomProperty("Events");

      propertyList.Add(new CustomProperty(ref nameCat, "First Name", "Sven", typeof(string), false, true));
      propertyList.Add(new CustomProperty(ref nameCat, "Middle Name", "Sune", typeof(string), false, true));
      propertyList.Add(new CustomProperty(ref nameCat, "Last Name", "Ericsson", typeof(string), false, true));
      propertyList.Add(new CustomProperty(ref nameCat, "Birth Surname", "Sven", typeof(string), false, true));
      propertyList.Add(new CustomProperty(ref nameCat, "Full Name", "Sven Ericsson", typeof(string), false, true));
      propertyList.Add(new CustomProperty(ref personCat, "Sex", PersonPropertySex.Unknown, typeof(PersonPropertySex), false, true));
      propertyList.Add(new CustomProperty(ref eventsCat, "Birth Date", DateTime.MinValue, typeof(DateTime), false, true));
      propertyList.Add(new CustomProperty(ref eventsCat, "Death Date", DateTime.MinValue, typeof(DateTime), false, true));
      //propertyList.Add(new CustomProperty("Custom", "", typeof(StatesList), false, true)); //<-- doesn't work

      selectedPerson = null;
      trace.TraceInformation("AddPersonForm");

    }

    public void SetEventHandler(System.EventHandler evHnd)
    {
      this.eventHandler = evHnd;
    }

    public bool GetPerson(ref IndividualClass person)
    {
      PersonalNameClass name = person.GetPersonalName();

      name.SetName(PersonalNameClass.PartialNameType.GivenName, propertyList[0].Value.ToString());
      name.SetName(PersonalNameClass.PartialNameType.MiddleName, propertyList[1].Value.ToString());
      name.SetName(PersonalNameClass.PartialNameType.Surname, propertyList[2].Value.ToString());
      name.SetName(PersonalNameClass.PartialNameType.BirthSurname, propertyList[3].Value.ToString());
      //name.SetName(PersonalNameClass.PartialNameType.GivenName, propertyList[4].Value.ToString());

      person.SetPersonalName(name);

      return true;
    }

    public void SetSelectedPerson(IndividualClass person)
    {
      trace.TraceInformation("SetSelectedPerson");
      if (person != null)
      {
        selectedPerson = person;

        if (propertyList != null)
        {
          propertyList[0].Value = person.GetPersonalName().GetName(PersonalNameClass.PartialNameType.GivenName);
          propertyList[1].Value = person.GetPersonalName().GetName(PersonalNameClass.PartialNameType.MiddleName);
          propertyList[2].Value = person.GetPersonalName().GetName(PersonalNameClass.PartialNameType.Surname);
          propertyList[3].Value = person.GetPersonalName().GetName(PersonalNameClass.PartialNameType.BirthSurname);
          propertyList[4].Value = person.GetPersonalName().GetName(PersonalNameClass.PartialNameType.NameString);

          switch(person.GetSex())
          {
            case IndividualClass.IndividualSexType.Female:
              propertyList[5].Value = PersonPropertySex.Female;
              break;
            case IndividualClass.IndividualSexType.Male:
              propertyList[5].Value = PersonPropertySex.Male;
              break;
            case IndividualClass.IndividualSexType.Unknown:
              propertyList[5].Value = PersonPropertySex.Unknown;
              break;
          }

          IList<IndividualEventClass> eventList = person.GetEventList();

          if (eventList != null)
          {
            foreach (IndividualEventClass ev in eventList)
            {
              switch (ev.GetEventType())
              {
                case IndividualEventClass.EventType.Birth:
                  propertyList[6].Value = ev.GetDate().ToDateTime();
                  break;
                case IndividualEventClass.EventType.Death:
                  propertyList[7].Value = ev.GetDate().ToDateTime();
                  break;
              }
            }
          }
          if (propertyList[6].Value == null)
          {
            propertyList[6].Value = DateTime.MinValue;
          }
          if (propertyList[7].Value == null)
          {
            propertyList[7].Value = DateTime.MinValue;
          }

        }
        else
        {
          trace.TraceEvent(TraceEventType.Error, 0, "error no proplist");
        }
      }
      else
      {
        trace.TraceEvent(TraceEventType.Error, 0, "error no selected person");
      }
    }

    private void AddPersonForm_Load(object sender, EventArgs e)
    {
      propertyGrid1.SelectedObject = propertyList;
      trace.TraceInformation("AddPersonForm_Load");

    }

    private void label1_Click(object sender, EventArgs e)
    {
      trace.TraceInformation("label1_Click");

    }

    private void radioButton1_CheckedChanged(object sender, EventArgs e)
    {
      trace.TraceInformation("AddPersonForm_Load");

    }

    private void childRadioButton_CheckedChanged(object sender, EventArgs e)
    {
      trace.TraceInformation("childRadioButton_CheckedChanged");

    }

    private void propertyGrid1_CausesValidationChanged(object sender, EventArgs e)
    {
      trace.TraceInformation("propertyGrid1_CausesValidationChanged");

    }

    private void propertyGrid1_Click(object sender, EventArgs e)
    {
      trace.TraceInformation("propertyGrid1_Click");

    }

    private void propertyGrid1_SelectedGridItemChanged(object sender, SelectedGridItemChangedEventArgs e)
    {
      trace.TraceInformation("propertyGrid1_SelectedGridItemChanged");

    }

    private void propertyGrid1_Validating(object sender, CancelEventArgs e)
    {
      trace.TraceInformation("propertyGrid1_Validating");

    }

    private void propertyGrid1_MouseClick(object sender, MouseEventArgs e)
    {
      trace.TraceInformation("propertyGrid1_MouseClick");

    }

    private void propertyGrid1_Enter(object sender, EventArgs e)
    {
      trace.TraceInformation("propertyGrid1_Enter");

    }

    private void propertyGrid1_Validated(object sender, EventArgs e)
    {
      trace.TraceInformation("propertyGrid1_Validated");

    }

    private void propertyGrid1_SelectedObjectsChanged(object sender, EventArgs e)
    {
      trace.TraceInformation("propertyGrid1_SelectedObjectsChanged");

    }

    private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
    {
      trace.TraceInformation("propertyGrid1_SelectedObjectsChanged");

    }

    private void saveButton_OnClick(object sender, EventArgs e)
    {
      trace.TraceInformation("saveButton_OnClick");

      this.eventHandler(this, new EventArgs());

    }

  }
}

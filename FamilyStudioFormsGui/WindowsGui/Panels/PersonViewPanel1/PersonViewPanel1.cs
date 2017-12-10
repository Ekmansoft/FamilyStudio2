using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FamilyStudioFormsGui.WindowsGui.Controls;
using FamilyStudioData.FamilyData;
using FamilyStudioData.FamilyTreeStore;
using System.Collections;
using FamilyStudioFormsGui.WindowsGui.Forms;

namespace FamilyStudioFormsGui.WindowsGui.Panels.PersonViewPanel1
{
  class PersonViewPanel1 : TreeViewPanelBaseClass
  {
    private FamilyTreeStoreBaseClass familyTree;
    private IndividualClass selectedIndividual;
    //private FamilyClass selectedFamily;
    private IList<Control> controlList;
    private FamilyForm2 parentForm;
    private PropertyGrid propertyGrid1;
    private CustomClass propertyList = new CustomClass();
    private Button saveButton;
    const int SexIndex = 6;
    const int BirthIndex = 7;
    const int DeathIndex = 9;
    private TraceSource trace;

    private enum PersonPropertySex
    {
      Unknown,
      Male,
      Female
    }

    public PersonViewPanel1()
    {
      trace = new TraceSource("PersonViewPanel1", SourceLevels.Warning);
      controlList = new List<Control>();

      this.Dock = DockStyle.Fill;
      parentForm = null;
      propertyGrid1 = new PropertyGrid();

      CustomProperty nameCat = new CustomProperty("Name");
      CustomProperty personCat = new CustomProperty("Person");
      //CustomProperty eventsCat = new CustomProperty("Events");
      CustomProperty birthCat = new CustomProperty("Birth");
      CustomProperty deathCat = new CustomProperty("Death");

      propertyList.Add(new CustomProperty(ref nameCat, "First Name", "", typeof(string), false, true));
      propertyList.Add(new CustomProperty(ref nameCat, "Middle Name", "", typeof(string), false, true));
      propertyList.Add(new CustomProperty(ref nameCat, "Last Name", "", typeof(string), false, true));
      propertyList.Add(new CustomProperty(ref nameCat, "Birth Surname", "", typeof(string), false, true));
      propertyList.Add(new CustomProperty(ref nameCat, "Full Name", "", typeof(string), false, true));
      propertyList.Add(new CustomProperty(ref nameCat, "Public Name", "", typeof(string), false, true));

      propertyList.Add(new CustomProperty(ref personCat, "Sex", PersonPropertySex.Unknown, typeof(PersonPropertySex), false, true));
      propertyList.Add(new CustomProperty(ref birthCat, "Date", DateTime.MinValue, typeof(DateTime), false, true));
      propertyList.Add(new CustomProperty(ref birthCat, "Place", "", typeof(string), false, true));
      propertyList.Add(new CustomProperty(ref deathCat, "Date", DateTime.MinValue, typeof(DateTime), false, true));
      propertyList.Add(new CustomProperty(ref deathCat, "Place", "", typeof(string), false, true));
      //propertyList.Add(new CustomProperty("Custom", "", typeof(StatesList), false, true)); //<-- doesn't work

      propertyGrid1.Location = new System.Drawing.Point(0, 0);
      propertyGrid1.Size = new System.Drawing.Size(this.Width, this.Height - 40);
      propertyGrid1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right;
      //propertyGrid1.Dock = DockStyle.Fill;
      propertyGrid1.SelectedObject = propertyList;


      this.Controls.Add(propertyGrid1);

      saveButton = new Button();

      saveButton.Text = "Save";
      saveButton.Top = this.Height - saveButton.Height;
      saveButton.MouseClick += saveButton_MouseClick;
      saveButton.Visible = true;
      saveButton.Enabled = true;
      saveButton.AutoSize = true;
      saveButton.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
      this.Controls.Add(saveButton);

      this.VisibleChanged += PersonViewPanel1_VisibleChanged;

      trace.TraceInformation("PersonViewPanel1::PersonViewPanel1()");
      trace.TraceInformation("  size" + this.Width + "x" + this.Height);
      trace.TraceInformation("  size" + propertyGrid1.Width + "x" + propertyGrid1.Height);

    }

    public override void PasteFromClipboard(object clipboard)
    {
      trace.TraceEvent(TraceEventType.Information, 0, "PasteFromClipboard:" + clipboard);
    }

    void saveButton_MouseClick(object sender, MouseEventArgs e)
    {
      trace.TraceInformation("PersonViewPanel1::save()");

      if(selectedIndividual != null)
      {
        SetPersonProperties(ref selectedIndividual);
        parentForm.SetSelectedIndividual(selectedIndividual.GetXrefName());
      }
      
    }
    public override void SetParentForm(FamilyForm2 parentForm)
    {
      this.parentForm = parentForm;

      trace.TraceInformation("PersonViewPanel1::SetParentForm()");
      parentForm.SelectedPersonChanged += OnSelectedPersonChangedEvent;

    }



    public override void SetFamilyTree(FamilyTreeStoreBaseClass inFamilyTree)
    {
      trace.TraceInformation("PersonViewPanel1::SetFamilyTree()");

      familyTree = inFamilyTree;
    }

    public void PersonViewPanel1_VisibleChanged(object sender, EventArgs e)
    {
      ShowSelectedPerson();
    }

    private void GetPersonProperties(IndividualClass person)
    {
      if (propertyList != null)
      {
        propertyList[0].Value = person.GetPersonalName().GetName(PersonalNameClass.PartialNameType.GivenName);
        propertyList[1].Value = person.GetPersonalName().GetName(PersonalNameClass.PartialNameType.MiddleName);
        propertyList[2].Value = person.GetPersonalName().GetName(PersonalNameClass.PartialNameType.Surname);
        propertyList[3].Value = person.GetPersonalName().GetName(PersonalNameClass.PartialNameType.BirthSurname);
        propertyList[4].Value = person.GetPersonalName().GetName(PersonalNameClass.PartialNameType.NameString);
        propertyList[5].Value = person.GetPersonalName().GetName(PersonalNameClass.PartialNameType.PublicName);

        switch (person.GetSex())
        {
          case IndividualClass.IndividualSexType.Female:
            propertyList[SexIndex].Value = PersonPropertySex.Female;
            break;
          case IndividualClass.IndividualSexType.Male:
            propertyList[SexIndex].Value = PersonPropertySex.Male;
            break;
          case IndividualClass.IndividualSexType.Unknown:
            propertyList[SexIndex].Value = PersonPropertySex.Unknown;
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
                propertyList[BirthIndex].Value = ev.GetDate().ToDateTime();
                propertyList[BirthIndex + 1].Value = ev.GetAddress();
                trace.TraceInformation("Birth: " + ev.GetAddress() + " " + ev.GetPlace() + " " + ev.ToString());
                break;
              case IndividualEventClass.EventType.Death:
                propertyList[DeathIndex].Value = ev.GetDate().ToDateTime();
                propertyList[DeathIndex + 1].Value = ev.GetAddress();
                trace.TraceInformation("Death: " + ev.GetAddress() + " " + ev.GetPlace() + " " + ev.ToString());
                break;
            }
          }
        }
        if (propertyList[BirthIndex].Value == null)
        {
          propertyList[BirthIndex].Value = DateTime.MinValue;
        }
        if (propertyList[DeathIndex].Value == null)
        {
          propertyList[DeathIndex].Value = DateTime.MinValue;
        }
      }
      else
      {
        trace.TraceEvent(TraceEventType.Error, 0, "error no proplist");
      }

    }

    private void SetPersonProperties(ref IndividualClass person)
    {
      if (propertyList != null)
      {
        person.GetPersonalName().SetName(PersonalNameClass.PartialNameType.GivenName, propertyList[0].Value.ToString());
        person.GetPersonalName().SetName(PersonalNameClass.PartialNameType.MiddleName, propertyList[1].Value.ToString());
        person.GetPersonalName().SetName(PersonalNameClass.PartialNameType.Surname, propertyList[2].Value.ToString());
        person.GetPersonalName().SetName(PersonalNameClass.PartialNameType.BirthSurname, propertyList[3].Value.ToString());
        person.GetPersonalName().SetName(PersonalNameClass.PartialNameType.NameString, propertyList[4].Value.ToString());
        person.GetPersonalName().SetName(PersonalNameClass.PartialNameType.PublicName, propertyList[5].Value.ToString());

        switch ((PersonPropertySex)propertyList[SexIndex].Value)
        {
          case PersonPropertySex.Female:
            person.SetSex(IndividualClass.IndividualSexType.Female);
            break;
          case PersonPropertySex.Male:
            person.SetSex(IndividualClass.IndividualSexType.Male);
            break;
          case PersonPropertySex.Unknown:
            person.SetSex(IndividualClass.IndividualSexType.Unknown);
            break;
        }

        IList<IndividualEventClass> eventList = person.GetEventList();

        familyTree.UpdateIndividual(person, PersonUpdateType.ChildFamily|PersonUpdateType.Events|PersonUpdateType.Name|PersonUpdateType.SpouseFamily);

      }
      else
      {
        trace.TraceEvent(TraceEventType.Error, 0, "error no proplist");
      }

    }

    private void ShowSelectedPerson()
    {
      trace.TraceInformation("PersonViewPanel1::ShowSelectedPerson()");

      foreach (CustomProperty property in propertyList)
      {
        property.Value = null;
      }
      if ((familyTree == null) || !this.Visible)
      {
        return;
      }

      if (selectedIndividual != null)
      {
        IndividualClass person = selectedIndividual;

        GetPersonProperties(person);
      }
      else
      {
        trace.TraceEvent(TraceEventType.Error,0, "error no selected person");
      }
      propertyGrid1.Refresh();
    }
    /*public void SetSelectedIndividual(String xrefName)
    {
      trace.TraceInformation("PersonViewPanel1::SetSelectedIndividual(" + xrefName + ")");
      if (familyTree != null)
      {
        selectedIndividual = (IndividualClass)familyTree.GetIndividual(xrefName);

        ShowSelectedPerson();
      }
    }*/

    public override string GetTitle()
    {
      return "PersonView1";
    }

    public override void OnSelectedPersonChangedEvent(object sender, PersonChangeEvent e)
    {
      if (familyTree != null)
      {
        selectedIndividual = e.selectedPerson;
        ShowSelectedPerson();
      }
    }




  }
}

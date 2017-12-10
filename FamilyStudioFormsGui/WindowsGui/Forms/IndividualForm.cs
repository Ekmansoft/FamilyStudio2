using System;
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
    public partial class IndividualForm : Form
    {
      private static TraceSource trace = new TraceSource("IndividualForm", SourceLevels.Warning);
      private IndividualClass tempIndividual;
      private bool result = false;
      //System.Windows.Forms.Control.ControlCollection textBoxList;
      //private IndividualClass currentIndividual;
      const String controlNamePrefix = "Family.Individual.name.value.";

      public IndividualForm()
      {
        //int counter = 0;
        int lastHeight = 0;
        const int lineHeight = 24;
        const int columnWidth = 100;

        InitializeComponent();
        //preNameTextBox = new TextBox();
        //surNameTextBox = new TextBox();

        //controlNamePrefix = new String(40);

        //controlNamePrefix = "Family.Individual.name.value.";

         PersonalNameClass.PartialNameType nameType;

         for (nameType = PersonalNameClass.PartialNameType.NameString; nameType <= PersonalNameClass.PartialNameType.BirthSurname; nameType++)
         {
           Label tempTitle;
           TextBox tempTextBox;
           //int height;

           tempTitle = new Label();

           tempTitle.Visible = true;

           tempTitle.SetBounds(0, lastHeight, columnWidth, lineHeight);

           tempTitle.AccessibleName = "Family.Individual.name.title." + nameType;
           tempTitle.Text = "" + nameType;
           this.nameControlPanel.Controls.Add(tempTitle);

           tempTextBox = new TextBox();

           tempTextBox.Visible = true;

           tempTextBox.SetBounds(columnWidth, lastHeight, columnWidth, lineHeight);

           tempTextBox.AccessibleName = controlNamePrefix + nameType;

           this.nameControlPanel.Controls.Add(tempTextBox);
           trace.TraceInformation("Add control flow " + tempTextBox.AccessibleName + " at " + (int)nameType);

           lastHeight += lineHeight;

           //textBoxList.Add(tempTextBox);

         }
    


      }

      public IndividualClass NewIndividual()
      {
        tempIndividual = new IndividualClass();

        //currentIndividual = tempIndividual;

        this.ShowDialog();

        if (result)
        {
          return tempIndividual;
        }
        else
        {
          return null;
        }

      }

      private void SaveButtonClick(object sender, EventArgs e)
      {
        PersonalNameClass personalName = new PersonalNameClass();

        personalName = tempIndividual.GetPersonalName();

        foreach(Control tempControl in this.nameControlPanel.Controls)
        {
          if (tempControl is TextBox)
          {
            String controlName = tempControl.AccessibleName;

            trace.TraceInformation("control name = " + controlName);

            /*if (controlName.Contains(controlNamePrefix))
            {
              String subString = controlName.Substring(controlNamePrefix.Length);
              trace.TraceInformation("control substr = " + subString);

              personalName.SetName(subString, tempControl.Text);

            }*/
          }

        }

        tempIndividual.SetPersonalName(personalName);
        result = true;
        this.Close();
      }

    }
}

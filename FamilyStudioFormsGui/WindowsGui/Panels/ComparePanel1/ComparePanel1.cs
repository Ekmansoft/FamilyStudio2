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
//using FamilyStudioData.FamilyFileFormat;

namespace FamilyStudioFormsGui.WindowsGui.Panels.ComparePanel1
{
  class ComparePanel1 : TreeViewPanelBaseClass
  {
    private static TraceSource trace = new TraceSource("ComparePanel1", SourceLevels.Warning);
    private FamilyTreeStoreBaseClass familyTree;
    private IList<Control> controlList;
    private FamilyForm2 parentForm;
    private ComboBox searchTextBox;
    private Button startButton;
    private ListView resultList;

    public override void PasteFromClipboard(object clipboard)
    {
      trace.TraceEvent(TraceEventType.Information, 0, "PasteFromClipboard:" + clipboard);
    }


    public ComparePanel1()
    {
      controlList = new List<Control>();

      this.Dock = DockStyle.Fill;
      parentForm = null;
      searchTextBox = new ComboBox();
      searchTextBox.Name = "SearchText";
      searchTextBox.Left = 0;
      //searchTextBox.Location = new System.Drawing.Point(0, 0);
      //searchTextBox.Size = new System.Drawing.Size(213, 20);
      //searchTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
      searchTextBox.TabIndex = 0;

      searchTextBox.KeyPress += searchTextBox_KeyPress;



      this.Controls.Add(searchTextBox);

      startButton = new Button();

      startButton.Text = "Search";

      startButton.Name = "Parent";
      //startButton.Location = new System.Drawing.Point(211, 0);
      //startButton.Size = new System.Drawing.Size(74, 23);

      startButton.Left = searchTextBox.Right;
      //startButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));

      this.startButton.MouseClick += startButton_MouseClick;
      
      startButton.TabIndex = 1;

      this.Controls.Add(startButton);


      resultList = new ListView();

      resultList.Top = startButton.Bottom;
      //resultList.Width = 400;
      //resultList.Height = 400;

      resultList.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right;
      resultList.Left = 3;
      //resultList.Margin.Right = 3;
      resultList.Location = new System.Drawing.Point(3, 20);
      resultList.Size = new System.Drawing.Size(200, 80);
      //resultList.Left = 3;

      //resultList.MultiColumn = true;
      //resultList.ScrollAlwaysVisible = true;
      //resultList.MouseClick += resultList_MouseClick;
      resultList.SelectedIndexChanged += resultList_SelectedIndexChanged;
      resultList.Columns.Add("Name1", 120, HorizontalAlignment.Left);
      //resultList.Columns.Add("Gen",      40, HorizontalAlignment.Left);
      resultList.Columns.Add("Birth.1", 80, HorizontalAlignment.Left);
      resultList.Columns.Add("Death.1", 80, HorizontalAlignment.Left);
      resultList.Columns.Add("Name.2", 120, HorizontalAlignment.Left);
      //resultList.Columns.Add("Gen",      40, HorizontalAlignment.Left);
      resultList.Columns.Add("Birth.2", 80, HorizontalAlignment.Left);
      resultList.Columns.Add("Death.2", 80, HorizontalAlignment.Left);
      //resultList.Columns.Add("Details", 250, HorizontalAlignment.Left);
      resultList.View = View.Details;
      resultList.AllowColumnReorder = true;
      resultList.FullRowSelect = true;
      //resultList.Columns.;
      //resultList.DisplayMember = "LongName";
      //resultList.ValueMember = "ShortName";

      //resultList.Items.AddRange()

      this.Controls.Add(resultList);
      //this.AcceptButton = this.startButton;


      trace.TraceInformation("ComparePanel1::ComparePanel1()");

    }

    void searchTextBox_KeyPress(object sender, KeyPressEventArgs e)
    {
      trace.TraceInformation("ComparePanel1::searchTextBox_KeyPress()" + e.KeyChar.ToString()  + " " + DateTime.Now);
      if (e.KeyChar == '\r')
      {
        StartSearch();


      }
    }

    void resultList_SelectedIndexChanged(object sender, EventArgs e)
    {
      trace.TraceInformation("ComparePanel1::resultList_SelectedIndexChanged()" +sender.ToString() + e.ToString());
      if (resultList.SelectedIndices.Count > 0)
      {
        trace.TraceInformation("ComparePanel1::resultList_SelectedIndexChanged()" + sender.ToString() + e.ToString() + " " + resultList.SelectedIndices.Count);

        foreach (ListViewItem item in resultList.SelectedItems)
        {
          //item.
          trace.TraceInformation(" selected: " + familyTree.GetIndividual(item.Tag.ToString()));

          if (parentForm != null)
          {
            parentForm.SetSelectedIndividual(item.Tag.ToString());
          }
        }

      }
    }

    void StartSearch()
    {
      trace.TraceInformation("ComparePanel1::StartSearch()" + DateTime.Now);
      if (familyTree == null)
      {
        return;
      }

      //parentForm.
      //startButton.Enabled = false;
      //dateButton.Enabled = false;
      resultList.Items.Clear();

      trace.TraceInformation("search:" + searchTextBox.Text + " " + DateTime.Now);

      if (searchTextBox.Text.Length > 0)
      {
        IEnumerator<IndividualClass> iterator;
        searchTextBox.Items.Add(searchTextBox.Text);

        iterator = familyTree.SearchPerson(searchTextBox.Text);

        if (iterator != null)
        {
          while (iterator.MoveNext())
          {
            IndividualClass person = (IndividualClass)iterator.Current;

            if (person != null)
            {
              ListViewItem item = new ListViewItem(person.GetName());
              item.SubItems.AddRange(new string[] { person.GetDate(IndividualEventClass.EventType.Birth).ToString(), person.GetDate(IndividualEventClass.EventType.Death).ToString() });
              item.Tag = person.GetXrefName();

              resultList.Items.Add(item);

            }
          }
        }
      }
      trace.TraceInformation(" Database: " + familyTree.GetSourceFileName() + "  " + DateTime.Now);
    }





    void startButton_MouseClick(object sender, MouseEventArgs e)
    {
      trace.TraceInformation("ComparePanel1::startButton_MouseClick()" + DateTime.Now);

      StartSearch();
    }

    public override void SetParentForm(FamilyForm2 parentForm)
    {
      this.parentForm = parentForm;

      trace.TraceInformation("ComparePanel1::SetParentForm()");
      parentForm.SelectedPersonChanged += OnSelectedPersonChangedEvent;

    }



    public override void SetFamilyTree(FamilyTreeStoreBaseClass inFamilyTree)
    {
      trace.TraceInformation("ComparePanel1::SetFamilyTree():" + DateTime.Now);

      familyTree = inFamilyTree;

    }

    private void ShowSelectedPerson()
    {
      trace.TraceInformation("ComparePanel1::ShowSelectedPerson()");
    }
    public void SetSelectedIndividual(String xrefName)
    {
      trace.TraceInformation("ComparePanel1::SetSelectedIndividual(" + xrefName + ")");
      if (familyTree != null)
      {

      }
    }

    public override string GetTitle()
    {
      return "Compare";
    }


    public override void OnSelectedPersonChangedEvent(object sender, PersonChangeEvent e)
    {
      if (familyTree != null)
      {
      }

    }






  }


}

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

namespace FamilyStudioFormsGui.WindowsGui.Panels.SearchPanel1
{
  class SearchPanel1 : TreeViewPanelBaseClass
  {
    private FamilyTreeStoreBaseClass familyTree;
    private IList<Control> controlList;
    private FamilyForm2 parentForm;
    private ComboBox searchTextBox;
    private Button startButton;
    private ListView resultList;
    private TraceSource trace;
    private AsyncWorkerProgress progressReporter;
    private TreeWorker treeWorker;

    public SearchPanel1()
    {
      trace = new TraceSource("SearchPanel1", SourceLevels.Warning);
      controlList = new List<Control>();

      this.Dock = DockStyle.Fill;
      parentForm = null;
      searchTextBox = new ComboBox();
      searchTextBox.Name = "SearchText";
      searchTextBox.Left = 6;
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
      resultList.Columns.Add("Name",    120, HorizontalAlignment.Left);
      //resultList.Columns.Add("Gen",      40, HorizontalAlignment.Left);
      resultList.Columns.Add("Birth", 80, HorizontalAlignment.Left);
      resultList.Columns.Add("Birthplace", 40, HorizontalAlignment.Left);
      resultList.Columns.Add("Death", 80, HorizontalAlignment.Left);
      resultList.Columns.Add("Deathplace", 40, HorizontalAlignment.Left);
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


      trace.TraceInformation("SearchPanel1::SearchPanel1()");

    }

    void searchTextBox_KeyPress(object sender, KeyPressEventArgs e)
    {
      trace.TraceInformation("SearchPanel1::searchTextBox_KeyPress()" + e.KeyChar.ToString()  + " " + DateTime.Now);
      if (e.KeyChar == '\r')
      {
        StartSearch();


      }
    }

    public override void PasteFromClipboard(object clipboard)
    {
      trace.TraceEvent(TraceEventType.Information, 0, "PasteFromClipboard:" + clipboard);
    }

    void resultList_SelectedIndexChanged(object sender, EventArgs e)
    {
      trace.TraceInformation("SearchPanel1::resultList_SelectedIndexChanged()" +sender.ToString() + e.ToString());
      if (resultList.SelectedIndices.Count > 0)
      {
        trace.TraceInformation("SearchPanel1::resultList_SelectedIndexChanged()" + sender.ToString() + e.ToString() + " " + resultList.SelectedIndices.Count);

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


    void AddPersonToListView(IndividualClass person)
    {
      string birthAddress = "";
      string deathAddress = "";
      IndividualEventClass birthEv = person.GetEvent(IndividualEventClass.EventType.Birth);
      if (birthEv != null)
      {
        AddressClass address = birthEv.GetAddress();
        if (address != null)
        {
          birthAddress = address.ToString();
        }
      }
      IndividualEventClass deathEv = person.GetEvent(IndividualEventClass.EventType.Death);
      if (deathEv != null)
      {
        AddressClass address = deathEv.GetAddress();
        if (address != null)
        {
          deathAddress = address.ToString();
        }
      }

      ListViewItem item = new ListViewItem(person.GetName());
      item.SubItems.AddRange(new string[] { person.GetDate(IndividualEventClass.EventType.Birth).ToString(), birthAddress, person.GetDate(IndividualEventClass.EventType.Death).ToString(), deathAddress});
      item.Tag = person.GetXrefName();

      resultList.Items.Add(item);
    }


    void AddToSearchResults(IndividualClass person)
    {
      if (resultList.InvokeRequired)
      {
        Invoke(new Action(() => AddPersonToListView(person)));
      }
      else
      {
        AddPersonToListView(person);
      }
    }

    void SearchTree(FamilyTreeStoreBaseClass familyTree, String searchString)
    {
      IEnumerator<IndividualClass> iterator;


      iterator = familyTree.SearchPerson(searchString, progressReporter);

      if (iterator != null)
      {
        while (iterator.MoveNext())
        {
          IndividualClass person = (IndividualClass)iterator.Current;

          if (person != null)
          {
            AddToSearchResults(person);
          }
        }
      }
    }

    public void SearchProgress(int progressPercent, string text = null)
    {
      trace.TraceInformation("SearchPanel1::SearchProgress(" + progressPercent + ")");
      parentForm.TextCallback(progressPercent, text);

      if (progressPercent < 0)
      {
        startButton.Text = "Search";
        startButton.Enabled = true;
      }
    }

    void StartSearch()
    {
      trace.TraceInformation("SearchPanel1::StartSearch()" + DateTime.Now);
      if (familyTree == null)
      {
        return;
      }
      //startButton.Enabled = false;
      //dateButton.Enabled = false;

      trace.TraceInformation("search:" + searchTextBox.Text + " " + DateTime.Now);

      if (searchTextBox.Text.Length > 0)
      {
        resultList.Items.Clear();
        if (searchTextBox.FindStringExact(searchTextBox.Text) == -1)
        {
          searchTextBox.Items.Add(searchTextBox.Text);
        }
        startButton.Text = "Searching...";
        startButton.Enabled = false;

        searchTextBox.Items.Add(searchTextBox.Text);

        progressReporter = new AsyncWorkerProgress(SearchProgress);

        //stats = new AncestorStatistics(familyTree, limits, ancestorGenerations, descendantGenerationNo, progressReporter, AncestorUpdate);
        //trace.TraceInformation("selected:" + searchTextBox.Text + " " + DateTime.Now);


        treeWorker = new TreeWorker(this, progressReporter, searchTextBox.Text, ref familyTree);


        startButton.Text = "Searching...";
        startButton.Enabled = false;
      }
      trace.TraceInformation(" Database: " + familyTree.GetSourceFileName() + "  " + DateTime.Now);
    }





    void startButton_MouseClick(object sender, MouseEventArgs e)
    {
      trace.TraceInformation("SearchPanel1::startButton_MouseClick()" + DateTime.Now);

      StartSearch();
    }

    public override void SetParentForm(FamilyForm2 parentForm)
    {
      this.parentForm = parentForm;

      trace.TraceInformation("SearchPanel1::SetParentForm()");
      parentForm.SelectedPersonChanged += OnSelectedPersonChangedEvent;

    }



    public override void SetFamilyTree(FamilyTreeStoreBaseClass inFamilyTree)
    {
      trace.TraceInformation("SearchPanel1::SetFamilyTree():" + DateTime.Now);

      familyTree = inFamilyTree;

    }

    private void ShowSelectedPerson()
    {
      trace.TraceInformation("SearchPanel1::ShowSelectedPerson()");
    }
    /*public void SetSelectedIndividual(String xrefName)
    {
      trace.TraceInformation("SearchPanel1::SetSelectedIndividual(" + xrefName + ")");
      if (familyTree != null)
      {
      }
    }*/

    public override string GetTitle()
    {
      return "Search";
    }


    public override void OnSelectedPersonChangedEvent(object sender, PersonChangeEvent e)
    {
      if (familyTree != null)
      {
      }

    }

    private class TreeWorker : AsyncWorkerProgressInterface
    {
      private BackgroundWorker backgroundWorker;
      private DateTime startTime;
      FamilyTreeStoreBaseClass familyTree;
      ProgressReporterInterface progressReporter;
      string searchString;
      private TraceSource trace;
      private SearchPanel1 parentWindow;

      public TreeWorker(
        SearchPanel1 sender,
        ProgressReporterInterface progress,
        string searchString,
        ref FamilyTreeStoreBaseClass familyTree)
      {
        trace = new TraceSource("TreeWorker", SourceLevels.Warning);
        parentWindow = sender;

        progressReporter = progress;
        this.familyTree = familyTree;
        this.searchString = searchString;

        backgroundWorker = new BackgroundWorker();

        backgroundWorker.WorkerReportsProgress = true;
        backgroundWorker.DoWork += new DoWorkEventHandler(DoWork);
        backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(Completed);
        backgroundWorker.ProgressChanged += new ProgressChangedEventHandler(ProgressChanged);

        backgroundWorker.RunWorkerAsync(searchString);

      }

      public void DoWork(object sender, DoWorkEventArgs e)
      {

        // This method will run on a thread other than the UI thread.
        // Be sure not to manipulate any Windows Forms controls created
        // on the UI thread from this method.
        startTime = DateTime.Now;

        parentWindow.SearchTree(familyTree, searchString);

        trace.TraceInformation("TreeWorker::DoWork(" + ")" + DateTime.Now);
      }

      public void ProgressChanged(object sender, ProgressChangedEventArgs e)
      {
        trace.TraceInformation("TreeWorker::ProgressChanged(" + e.ProgressPercentage + ")" + DateTime.Now);

        progressReporter.ReportProgress(e.ProgressPercentage, familyTree.GetShortTreeInfo());
      }
      public void Completed(object sender, RunWorkerCompletedEventArgs e)
      {
        trace.TraceInformation("TreeWorker::Completed()" + DateTime.Now);
        trace.TraceInformation("  Start time:" + startTime + " end time: " + DateTime.Now);

        progressReporter.Completed(familyTree.GetShortTreeInfo());
      }

      public void Dispose()
      {
        backgroundWorker.DoWork -= new DoWorkEventHandler(DoWork);
        backgroundWorker.RunWorkerCompleted -= new RunWorkerCompletedEventHandler(Completed);
        backgroundWorker.ProgressChanged -= new ProgressChangedEventHandler(ProgressChanged);
        backgroundWorker.Dispose();
      }
    }



  }

}

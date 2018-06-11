using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Windows.Forms;
using FamilyStudioFormsGui.WindowsGui.Controls;
using FamilyStudioData.FamilyData;
using FamilyStudioData.FamilyTreeStore;
//using FamilyStudioData.FamilyTreeStore;
using FamilyStudioFormsGui.WindowsGui.Forms;

namespace FamilyStudioFormsGui.WindowsGui.Panels.CompletenessViewPanel1
{
  public delegate void SettingsUpdateHandler(SanityCheckLimits newLimits);

  class CompletenessViewPanel1 : TreeViewPanelBaseClass
  {
    private FamilyTreeStoreBaseClass familyTree;
    private IndividualClass selectedIndividual;
    private IList<Control> controlList;
    private FamilyForm2 parentForm;
    //private PropertyGrid propertyGrid1;
    //private CustomClass propertyList = new CustomClass();
    private Button startButton;
    private Button settingsButton;
    private Button stopButton;
    private DomainUpDown descendantGenerationNoCtrl;
    private DomainUpDown ancestorGenerationNoCtrl;
    private ListView resultList;
    //private ArrayList personList;
    private AsyncWorkerProgress progressReporter;
    private CompletenessTreeWorker analyseTreeWorker;
    private AncestorStatistics stats;
    //private int descendantGenerationNo;
    private FamilyUtility utility;
    private TraceSource trace;
    private SanitySettingsForm settings;
    private SanityCheckLimits limits;

    public override void PasteFromClipboard(object clipboard)
    {
      trace.TraceEvent(TraceEventType.Information, 0, "PasteFromClipboard:" + clipboard);
    }

    SanityCheckLimits GetSanitySettings(string filename)
    {
      SanityCheckLimits limits = new SanityCheckLimits();
      FileStream readSettings;
      try
      {
        readSettings = new FileStream(filename, FileMode.Open);
      }
      catch (FileNotFoundException e)
      {
        trace.TraceInformation("FileNotFoundException:" + e.ToString());
        readSettings = null;
      }

      if (readSettings != null)
      {
        bool delete = false;
        DataContractSerializer serializer = new DataContractSerializer(typeof(SanityCheckLimits));

        try
        {
          limits = (SanityCheckLimits)serializer.ReadObject(readSettings);
        }
        catch (SerializationException e)
        {
          trace.TraceInformation("SerializationException:" + e.ToString());
          delete = true;
        }
        readSettings.Close();

        if (limits.parentsProblem == null)
        {
          limits.parentsProblem = new SanityProperty();
          limits.parentsProblem.active = true;
        }
        if (limits.generationlimited == null)
        {
          limits.generationlimited = new SanityProperty();
          limits.generationlimited.active = true;
        }
        if (limits.missingWeddingDate == null)
        {
          limits.missingWeddingDate = new SanityProperty();
          limits.missingWeddingDate.active = true;
        }
        if (limits.marriageProblem == null)
        {
          limits.marriageProblem = new SanityProperty();
          limits.marriageProblem.active = true;
        }
        if (limits.missingPartner == null)
        {
          limits.missingPartner = new SanityProperty();
          limits.missingPartner.active = true;
          limits.missingPartner.value = 115;
        }
        if (limits.duplicateCheck == null)
        {
          limits.duplicateCheck = new SanityProperty();
          limits.duplicateCheck.active = false;
        }
        limits.CreateArray();

        if (delete)
        {
          File.Delete(filename);
        }
      }
      else
      {
        FileStream storeSettings = new FileStream(filename, FileMode.CreateNew);

        DataContractSerializer serializer = new DataContractSerializer(typeof(SanityCheckLimits));

        serializer.WriteObject(storeSettings, (SanityCheckLimits)limits);
        storeSettings.Close();
      }
      return limits;
    }

    void SetSanitySettings(string filename, SanityCheckLimits limits)
    {
      FileStream storeSettings = new FileStream(filename, FileMode.Create);

      DataContractSerializer serializer = new DataContractSerializer(typeof(SanityCheckLimits));

      serializer.WriteObject(storeSettings, (SanityCheckLimits)limits);
      storeSettings.Close();
    }

    void AddItemToListView(AncestorLineInfo ancestor, SanityCheckLimits limits)
    {
      IndividualClass person = familyTree.GetIndividual(ancestor.rootAncestor);
      if (person != null)
      {
        trace.TraceInformation("  " + ancestor.depth + " generations: " + person.GetName() + " " + person.GetDate(IndividualEventClass.EventType.Birth) + " - " + person.GetDate(IndividualEventClass.EventType.Death));

        ListViewItem oldItem = resultList.FindItemWithText(person.GetName());

        if (oldItem != null)
        {
          if (oldItem.Tag.ToString() == ancestor.rootAncestor)
          {
            resultList.Items.Remove(oldItem);
          }
        }
        string detailString = ancestor.GetDetailString(limits);

        if (detailString.Length > 0)
        {
          ListViewItem item = new ListViewItem(person.GetName());
          item.SubItems.AddRange(new string[] { ancestor.depth.ToString(), ancestor.relationPath.GetDistance(), person.GetDate(IndividualEventClass.EventType.Birth).ToString(), person.GetDate(IndividualEventClass.EventType.Death).ToString(), detailString });
          item.ToolTipText = ancestor.relationPath.ToString(familyTree);
          item.Tag = person.GetXrefName();

          resultList.Items.Add(item);
        }
        //list.Items.
      }
      else
      {
        trace.TraceEvent(TraceEventType.Error, 0, " Error could not fetch " + ancestor.rootAncestor + " from tree " + ancestor.depth + " generations " + ancestor.GetDetailString(limits));
      }
    }

    public void AddToListView(ref ListView list, AncestorStatistics stats)
    {
      bool disableCounter = true;
      {
        resultList.Items.Clear();
        if (stats != null)
        {
          IEnumerable<AncestorLineInfo> query = stats.GetAncestorList().OrderBy(ancestor => ancestor.depth);

          //SanityCheckLimits limits = GetSanitySettings(utility.GetCurrentDirectory() + "\\SanitySettings.fssan");
          foreach (AncestorLineInfo root in query)
          {
            AddItemToListView(root, limits);
          }
        }
      }

      if(!disableCounter)
      {
        IEnumerable<HandledItem> query = stats.GetAnalysedPeopleNo().OrderByDescending(ancestor => ancestor.number);

        foreach (HandledItem item in query)
        {
          if (item.number > 1)
          {
            IndividualClass person = familyTree.GetIndividual(item.xref);
            if (person != null)
            {
              trace.TraceInformation("  Referenced " + item.number + " times: " + person.GetName() + " " + person.GetDate(IndividualEventClass.EventType.Birth) + " - " + person.GetDate(IndividualEventClass.EventType.Death) + " " +item.relationStackList.Count);
              //list.Add(new ListedPerson("  Multiply Referenced " + item.number + " times: " + person.GetName() + " " + person.GetDate(IndividualEventClass.EventType.Birth) + " - " + person.GetDate(IndividualEventClass.EventType.Death), person.GetXrefName()));
              ListViewItem lvItem = new ListViewItem(person.GetName());
              lvItem.SubItems.AddRange(new string[] { "X:" + item.number, person.GetDate(IndividualEventClass.EventType.Birth).ToString(), person.GetDate(IndividualEventClass.EventType.Death).ToString(), "referenced " + item.number + " times"});
              lvItem.Tag = person.GetXrefName();
              lvItem.ToolTipText = "";  
              foreach(RelationStack stack in item.relationStackList)
              {
                if(stack != null)
                {
                  lvItem.ToolTipText += stack.ToString(familyTree);
                  lvItem.ToolTipText += "\n";
                  trace.TraceInformation(stack.ToString(familyTree));
                }
              }


              list.Items.Add(lvItem);
            }
            else
            {
              trace.TraceInformation("  Person == null:" + item);
            }
          }
        }
      }
      /*{
        foreach (HandledItem item in analysedFamiliesNo)
        {
          if (item.number > 1)
          {
            trace.TraceInformation("Duplicate family " + item.number + " " + item.xref);
          }
        }
      }*/


    }


    public CompletenessViewPanel1()
    {
      trace = new TraceSource("PersonViewPanel1", SourceLevels.Warning);
      controlList = new List<Control>();

      this.Dock = DockStyle.Fill;
      parentForm = null;
      //descendantGenerationNo = 0;

      settingsButton = new Button();

      settingsButton.Text = "Settings";

      settingsButton.Left = 0;

      this.settingsButton.MouseClick += settingsButton_MouseClick;

      this.Controls.Add(settingsButton);

      startButton = new Button();

      startButton.Text = "Analyse";

      startButton.Name = "Parent";

      startButton.Left = settingsButton.Right;

      this.startButton.MouseClick += startButton_MouseClick;

      this.Controls.Add(startButton);

      stopButton = new Button();

      stopButton.Left = startButton.Right;

      stopButton.Text = "Stop";

      this.stopButton.MouseClick += stopButton_MouseClick;

      stopButton.Enabled = false;
      //stopButton.

      this.Controls.Add(stopButton);

      ancestorGenerationNoCtrl = new DomainUpDown();

      ancestorGenerationNoCtrl.Left = stopButton.Right;

      ancestorGenerationNoCtrl.Items.Add("All g. ancestors");
      for (int i = 20; i >= 5; i--)
      {
        ancestorGenerationNoCtrl.Items.Add(i.ToString() + " g. ancestors");
      }
      ancestorGenerationNoCtrl.SelectedItem = "5 g. ancestors";
      ancestorGenerationNoCtrl.Width = 100;
      //ancestorGenerationNoCtrl.AutoSize = true;

      this.Controls.Add(ancestorGenerationNoCtrl);

      descendantGenerationNoCtrl = new DomainUpDown();
      descendantGenerationNoCtrl.Text = "Descendants";
      descendantGenerationNoCtrl.Left = ancestorGenerationNoCtrl.Right;
      descendantGenerationNoCtrl.Enabled = true;
      descendantGenerationNoCtrl.AutoSize = true;
      descendantGenerationNoCtrl.Items.Add("All g. descendants");
      for (int i = 10; i >= 0; i--)
      {
        descendantGenerationNoCtrl.Items.Add(i.ToString() + " g. descendants");
      }
      descendantGenerationNoCtrl.SelectedItem = "0 g. descendants";
      descendantGenerationNoCtrl.Width = 100;
      this.Controls.Add(descendantGenerationNoCtrl);

      stopButton.Enabled = false;
      //stopButton.

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
      resultList.Columns.Add("Name", 120, HorizontalAlignment.Left);
      resultList.Columns.Add("Distance", 40, HorizontalAlignment.Right);
      resultList.Columns.Add("Relation", 40, HorizontalAlignment.Left);
      resultList.Columns.Add("Birth", 80, HorizontalAlignment.Left);
      resultList.Columns.Add("Death", 80, HorizontalAlignment.Left);
      resultList.Columns.Add("Details", 250, HorizontalAlignment.Left);
      resultList.View = View.Details;
      resultList.AllowColumnReorder = true;
      resultList.FullRowSelect = true;
      resultList.ShowItemToolTips = true;

      resultList.MouseUp += ResultList_MouseUp;

      /*resultList.ContextMenuStrip = new ContextMenuStrip();
      ToolStripItem openItem = new ToolStripMenuItem();
      openItem.Text = "Open...";
      openItem.MouseUp += ContextMenuStrip_SelectOpen;
      resultList.ContextMenuStrip.Items.Add(openItem);

      ToolStripItem saveItem = new ToolStripMenuItem();
      saveItem.Text = "Save...";
      saveItem.MouseUp += ContextMenuStrip_SelectSave;
      resultList.ContextMenuStrip.Items.Add(saveItem);

      ToolStripItem exportItem = new ToolStripMenuItem();
      exportItem.Text = "Export to Text...";
      exportItem.MouseUp += ContextMenuStrip_SelectExport;
      resultList.ContextMenuStrip.Items.Add(exportItem);*/


      this.Controls.Add(resultList);

      utility = new FamilyUtility();

      limits = GetSanitySettings(utility.GetCurrentDirectory() + "\\SanitySettings.fssan");

      trace.TraceInformation("CompletenessViewPanel1::CompletenessViewPanel1()");

    }

    void ResultList_Open_Click(object sender, EventArgs e)
    {
      ReadListFromFile();
    }
    void ResultList_Save_Click(object sender, EventArgs e)
    {
      SaveFileDialog fileDlg = new SaveFileDialog();
      fileDlg.Filter = "Stats List|*.fss";
      fileDlg.InitialDirectory = utility.GetCurrentDirectory();

      if (fileDlg.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
      {
        SaveListToFile(fileDlg.FileName);
      }
    }

    void ResultList_ExportText_Click(object sender, EventArgs e)
    {
      SaveFileDialog fileDlg = new SaveFileDialog();
      fileDlg.Filter = "Text file|*.txt";
      fileDlg.InitialDirectory = utility.GetCurrentDirectory();

      if (fileDlg.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
      {
        ExportListText(fileDlg.FileName);
      }
    }

    void ResultList_ExportHtml_Click(object sender, EventArgs e)
    {
      SaveFileDialog fileDlg = new SaveFileDialog();
      fileDlg.Filter = "Html file|*.html";
      fileDlg.InitialDirectory = utility.GetCurrentDirectory();

      if (fileDlg.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
      {
        ExportListHtml(fileDlg.FileName);
      }
    }

    void ResultList_Url_Click(object sender, EventArgs e)
    {
      if (sender.GetType() == typeof(MenuItem))
      {
        MenuItem clickedItem = (MenuItem)sender;
        //parent.AddRelative(AsyncTreePanel1.RelativeType.Child);
        Process.Start(clickedItem.Text);
      }
    }


    private void ResultList_MouseUp(object sender, MouseEventArgs e)
    {
      if (e.Button == System.Windows.Forms.MouseButtons.Right)
      {
        ContextMenu menu = new ContextMenu();

        menu.MenuItems.Add(new MenuItem("Open...", ResultList_Open_Click));
        menu.MenuItems.Add(new MenuItem("Save...", ResultList_Save_Click));
        menu.MenuItems.Add(new MenuItem("Export text...", ResultList_ExportText_Click));
        menu.MenuItems.Add(new MenuItem("Export HTML...", ResultList_ExportHtml_Click));

        if (parentForm != null)
        {
          string selectedPerson = parentForm.GetSelectedIndividual();
          IndividualClass individual = familyTree.GetIndividual(selectedPerson);
          if (individual != null)
          {
            IList<string> urlList = individual.GetUrlList();
            if (urlList != null)
            {
              foreach (string url in urlList)
              {
                menu.MenuItems.Add(new MenuItem(url, ResultList_Url_Click));
              }
            }
          }
          trace.TraceData(TraceEventType.Warning, 0, "selectedperson = " + selectedPerson);

          if (stats != null)
          {
            AncestorLineInfo selected = stats.GetAncestor(selectedPerson);

            if (selected != null)
            {
              trace.TraceData(TraceEventType.Warning, 0, "selected = " + selected + " dups=" + selected.duplicate.Count);

              foreach (string duplicate in selected.duplicate)
              {
                menu.MenuItems.Add(new MenuItem(duplicate, ResultList_Url_Click));
              }
            }
            else
            {
              trace.TraceData(TraceEventType.Warning, 0, "selected = null");
            }
          }
        }
        menu.Show(this, e.Location, LeftRightAlignment.Right);
      }
    }

    void ContextMenuStrip_SelectOpen(object sender, MouseEventArgs e)
    {
      ReadListFromFile();
    }
    void ContextMenuStrip_SelectSave(object sender, MouseEventArgs e)
    {
      SaveFileDialog fileDlg = new SaveFileDialog();
      fileDlg.Filter = "Stats List|*.fss";
      fileDlg.InitialDirectory = utility.GetCurrentDirectory();

      if (fileDlg.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
      {
        SaveListToFile(fileDlg.FileName);
      }
    }

    void ContextMenuStrip_SelectExport(object sender, MouseEventArgs e)
    {
      SaveFileDialog fileDlg = new SaveFileDialog();
      fileDlg.Filter = "Text file|*.txt";
      fileDlg.InitialDirectory = utility.GetCurrentDirectory();

      if (fileDlg.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
      {
        ExportListText(fileDlg.FileName);
      }
    }

    void ContextMenuStrip_SelectHtmlExport(object sender, MouseEventArgs e)
    {
      SaveFileDialog fileDlg = new SaveFileDialog();
      fileDlg.Filter = "HTML file|*.html";
      fileDlg.InitialDirectory = utility.GetCurrentDirectory();

      if (fileDlg.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
      {
        ExportListHtml(fileDlg.FileName);
      }
    }

    void ReadListFromFile()
    {
      OpenFileDialog fileDlg = new OpenFileDialog();
      fileDlg.Filter = "Stats List|*.fss";
      fileDlg.InitialDirectory = utility.GetCurrentDirectory();

      if (fileDlg.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
      {
        DataContractSerializer serializer = new DataContractSerializer(typeof(AncestorStatistics));

        FileStream readFile = new FileStream(fileDlg.FileName, FileMode.Open);
        AncestorStatistics localStats;
        localStats = (AncestorStatistics)serializer.ReadObject(readFile);

        stats = localStats;
        stats.SetFamilyTree(familyTree);
        readFile.Close();

        AddToListView(ref resultList, stats);
        stopButton.Enabled = false;
        startButton.Enabled = true;
        ancestorGenerationNoCtrl.Enabled = true;

      }
    }


    private void SaveListToFile(string filename)
    {
      try
      {
        FileStream saveList = new FileStream(filename, FileMode.Create);

        DataContractSerializer serializer = new DataContractSerializer(typeof(AncestorStatistics));

        serializer.WriteObject(saveList, stats);
        saveList.Close();
      }
      catch (System.NotSupportedException e)
      {
        trace.TraceEvent(TraceEventType.Error, 0, "Error saving file: " + filename);
        trace.TraceEvent(TraceEventType.Error, 0, e.ToString());
      }
    }

    private void ExportListText(string filename)
    {
      try
      {
        StreamWriter exportFile = new StreamWriter(filename);

        exportFile.Write(stats.ToString());
        exportFile.Close();
      }
      catch (System.NotSupportedException e)
      {
        trace.TraceInformation("Error saving file: " + filename);
        trace.TraceInformation(e.ToString());
      }

    }

    private void ExportListHtml(string filename)
    {
      try
      {
        StreamWriter exportFile = new StreamWriter(filename);

        exportFile.Write(stats.ToHtml());
        exportFile.Close();
      }
      catch (System.NotSupportedException e)
      {
        trace.TraceInformation("Error saving file: " + filename);
        trace.TraceInformation(e.ToString());
      }

    }


    void resultList_SelectedIndexChanged(object sender, EventArgs e)
    {
      trace.TraceInformation("CompletenessViewPanel1::resultList_SelectedIndexChanged()" + sender.ToString() + e.ToString());
      if (resultList.SelectedIndices.Count > 0)
      {
        trace.TraceInformation("CompletenessViewPanel1::resultList_SelectedIndexChanged()" + sender.ToString() + e.ToString() + " " + resultList.SelectedIndices.Count);

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

    /*void resultList_MouseClick(object sender, MouseEventArgs e)
    {
      trace.TraceInformation("CompletenessViewPanel1::resultList_MouseClick()" +sender.ToString() );
    }*/


    public void CompletenessProgress(int progressPercent, string text = null)
    {
      trace.TraceInformation("CompletenessViewPanel1::CompletenessProgress(" + progressPercent + ")");
      parentForm.TextCallback(progressPercent, text);

      if (progressPercent < 0)
      {
        AddToListView(ref resultList, stats);

        SaveListToFile(utility.GetCurrentDirectory() + "\\" + FamilyUtility.MakeFilename("treeanalysis_" + familyTree.GetSourceFileName() + "_" + DateTime.Now.ToString() + "_" + stats.GetAncestorGenerationNo() + "_ancestGen_" + stats.GetDescendantGenerationNo() + "_descGen_.fss"));
        ancestorGenerationNoCtrl.Enabled = true;
        stopButton.Enabled = false;
        startButton.Enabled = true;
        descendantGenerationNoCtrl.Enabled = true;

      }
    }

    private Int32 GetSelectedInt(DomainUpDown intListControl)
    {
      string numberStr = intListControl.SelectedItem.ToString().Substring(0, intListControl.SelectedItem.ToString().IndexOf(' '));

      if (intListControl.SelectedItem.ToString().Substring(0, 3) != "All")
      {
        if (numberStr.Length > 0)
        {
          return Convert.ToInt32(numberStr);
        }
      }
      return AncestorStatistics.AllGenerations;
    }

    void SettingsUpdateHandlerFcn(SanityCheckLimits newLimits)
    {
      if(newLimits != null)
      {
        SetSanitySettings(utility.GetCurrentDirectory() + "\\SanitySettings.fssan", newLimits);
        limits = newLimits;
        AddToListView(ref resultList, stats);
      }
      settings = null;
    }

    void settingsButton_MouseClick(object sender, MouseEventArgs e)
    {
      trace.TraceInformation("CompletenessViewPanel1::settingsButton_MouseClick():" + DateTime.Now);
      if (stopButton.Visible)
      {
        if (settings == null)
        {
          settings = new SanitySettingsForm();

          limits = GetSanitySettings(utility.GetCurrentDirectory() + "\\SanitySettings.fssan");
          settings.Update(limits, SettingsUpdateHandlerFcn);
        }
      }
    }

    void AncestorUpdate(AncestorLineInfo ancestor)
    {
      if (resultList.InvokeRequired)
      {
        Invoke(new Action(() => AddItemToListView(ancestor, limits)));
      }
      else
      {
        AddItemToListView(ancestor, limits);
      }
    }

    void startButton_MouseClick(object sender, MouseEventArgs e)
    {
      trace.TraceInformation("CompletenessViewPanel1::startButton_MouseClick()" + DateTime.Now);
      if ((selectedIndividual != null) && (startButton.Visible))
      {
        //SearchMode runMode = SearchMode.ParentMissing;

        stopButton.Enabled = true;
        startButton.Enabled = false;

        descendantGenerationNoCtrl.Enabled = false;
        ancestorGenerationNoCtrl.Enabled = false;

        //dateButton.Enabled = false;
        resultList.Items.Clear();


        int ancestorGenerations = GetSelectedInt(ancestorGenerationNoCtrl);
        int descendantGenerationNo = GetSelectedInt(descendantGenerationNoCtrl);

        //limits = GetSanitySettings(utility.GetCurrentDirectory() + "\\SanitySettings.fssan");

        progressReporter = new AsyncWorkerProgress(CompletenessProgress);

        stats = new AncestorStatistics(familyTree, limits, ancestorGenerations, descendantGenerationNo, progressReporter, AncestorUpdate);
        trace.TraceInformation("selected:" + selectedIndividual.GetName() + " " + DateTime.Now);


        analyseTreeWorker = new CompletenessTreeWorker(this, progressReporter, selectedIndividual.GetXrefName(), ref stats);

        //AnalyseParents(selectedIndividual, ref stats, 1);

        //dateButton.Enabled = true;

        trace.TraceInformation(" Database: " + familyTree.GetSourceFileName() + " person " + selectedIndividual.GetName() + " " + DateTime.Now);


      }
      else
      {
        MessageBox.Show("Error: No person selected!");
      }
    }

    void stopButton_MouseClick(object sender, MouseEventArgs e)
    {
      trace.TraceInformation("CompletenessViewPanel1::stopButton_MouseClick():" + DateTime.Now);
      if (stopButton.Visible)
      {
        MessageBox.Show("Stop/Abort unfortunately not yet supported!");

        //startButton.Visible = true;
        //descendantGenerationNoCtrl.Enabled = true;

      }
    }
    public override void SetParentForm(FamilyForm2 parentForm)
    {
      this.parentForm = parentForm;

      trace.TraceInformation("CompletenessViewPanel1::SetParentForm()");
      parentForm.SelectedPersonChanged += OnSelectedPersonChangedEvent;

    }



    public override void SetFamilyTree(FamilyTreeStoreBaseClass inFamilyTree)
    {
      trace.TraceInformation("CompletenessViewPanel1::SetFamilyTree():" + DateTime.Now);

      familyTree = inFamilyTree;

    }

    public void SetSelectedIndividual(String xrefName)
    {
      trace.TraceInformation("CompletenessViewPanel1::SetSelectedIndividual(" + xrefName + ")");
      if (familyTree != null)
      {
        selectedIndividual = (IndividualClass)familyTree.GetIndividual(xrefName);
      }
    }

    public override string GetTitle()
    {
      return "Completeness";
    }


    public override void OnSelectedPersonChangedEvent(object sender, PersonChangeEvent e)
    {
      if (familyTree != null)
      {
        selectedIndividual = e.selectedPerson;
        /*if (selectedIndividual != null)
        {
          ShowSelectedPerson();
        }*/
      }

    }


    private class CompletenessTreeWorker : AsyncWorkerProgressInterface
    {
      private BackgroundWorker backgroundWorker;
      private DateTime startTime;
      AncestorStatistics stats;
      ProgressReporterInterface progressReporter;
      string startPersonXref;
      private TraceSource trace;

      public CompletenessTreeWorker(
        object sender,
        ProgressReporterInterface progress,
        string startIndividualXref,
        ref AncestorStatistics stats)
      {
        trace = new TraceSource("CompletenessTreeWorker", SourceLevels.Warning);

        //familyTree = stats.familyTree;

        progressReporter = progress;
        this.stats = stats;
        startPersonXref = startIndividualXref;



        backgroundWorker = new BackgroundWorker();

        backgroundWorker.WorkerReportsProgress = true;
        backgroundWorker.DoWork += new DoWorkEventHandler(DoWork);
        backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(Completed);
        backgroundWorker.ProgressChanged += new ProgressChangedEventHandler(ProgressChanged);

        backgroundWorker.RunWorkerAsync(startIndividualXref);

      }

      public void DoWork(object sender, DoWorkEventArgs e)
      {

        // This method will run on a thread other than the UI thread.
        // Be sure not to manipulate any Windows Forms controls created
        // on the UI thread from this method.
        startTime = DateTime.Now;
        //workerFileName = (String)e.Argument;
        IndividualClass startperson = stats.GetFamilyTree().GetIndividual(startPersonXref);
        
        if (startperson != null)
        {
          stats.AnalyseTree(startperson);
        }

        trace.TraceInformation("AnalyseTreeWorker::DoWork(" + ")" + DateTime.Now);
      }

      public void ProgressChanged(object sender, ProgressChangedEventArgs e)
      {
        trace.TraceInformation("CompletenessTreeWorker::ProgressChanged(" + e.ProgressPercentage + ")" + DateTime.Now);

        if (stats.GetFamilyTree() != null)
        {
          progressReporter.ReportProgress(e.ProgressPercentage, stats.GetFamilyTree().GetShortTreeInfo());
        }
        else
        {
          progressReporter.ReportProgress(e.ProgressPercentage);
        }
      }
      public void Completed(object sender, RunWorkerCompletedEventArgs e)
      {
        trace.TraceInformation("AnalyseTreeWorker::Completed()" + DateTime.Now);
        trace.TraceInformation("  Start time:" + startTime + " end time: " + DateTime.Now);

        progressReporter.Completed(stats.GetFamilyTree().GetShortTreeInfo());
        stats.Print();
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

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

namespace FamilyStudioFormsGui.WindowsGui.Panels.RelationFinderPanel
{
  class RelationFinderPanel : TreeViewPanelBaseClass
  {
    private static TraceSource trace = new TraceSource("RelationFinderPanel", SourceLevels.Warning);
    private FamilyTreeStoreBaseClass familyTree;
    private IndividualClass selectedIndividual;
    //private FamilyClass selectedFamily;
    private IList<Control> controlList;
    private FamilyForm2 parentForm;
    //private PropertyGrid propertyGrid1;
    //private CustomClass propertyList = new CustomClass();
    private Button startButton;//, dateButton;
    private Button stopButton;
    //private Button relationButton;
    //private CheckBox descendants;
    private DomainUpDown resultNoCtrl;
    private TreeView resultList;
    //private ArrayList personList;
    //private FamilyFormProgress progressReporter;
    //private AnalyseTreeWorker analyseTreeWorker;
    //private AncestorStatistics stats;
    private RelationStackList relationList;
    private FamilyUtility utility;
    private RelationTreeWorker relWorker;

    public RelationFinderPanel()
    {
      controlList = new List<Control>();

      this.Dock = DockStyle.Fill;
      parentForm = null;
      //checkDescendants = false;

      startButton = new Button();

      startButton.Text = "Analyse";

      startButton.Name = "Parent";

      startButton.Left = 0;

      this.startButton.MouseClick += startButton_MouseClick;

      this.Controls.Add(startButton);

      stopButton = new Button();

      stopButton.Left = startButton.Right;

      stopButton.Text = "Stop";

      stopButton.Enabled = false;
      //stopButton.

      resultNoCtrl = new DomainUpDown();

      resultNoCtrl.Left = stopButton.Right;

      resultNoCtrl.Items.Add("All");
      for (int i = 20; i >= 5; i--)
      {
        resultNoCtrl.Items.Add(i.ToString());
      }
      resultNoCtrl.SelectedItem = "5";
      resultNoCtrl.Width = 50;
      //resultNoCtrl.AutoSize = true;

      this.Controls.Add(resultNoCtrl);

      this.stopButton.MouseClick += stopButton_MouseClick;

      this.Controls.Add(stopButton);

      resultList = new TreeView();

      resultList.Top = startButton.Bottom;
      //resultList.Width = 400;
      //resultList.Height = 400;

      resultList.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right;
      resultList.Left = 3;
      //resultList.Margin.Right = 3;
      resultList.Location = new System.Drawing.Point(3, 20);
      resultList.Size = new System.Drawing.Size(200, 80);
      //resultList.Left = 3;

      resultList.NodeMouseClick += resultList_NodeMouseClick;

      resultList.ContextMenuStrip = new ContextMenuStrip();
      ToolStripItem openItem = new ToolStripMenuItem();
      openItem.Text = "Open...";
      openItem.MouseUp += ContextMenuStrip_SelectOpen;
      ToolStripItem saveItem = new ToolStripMenuItem();
      saveItem.Text = "Save...";
      saveItem.MouseUp += ContextMenuStrip_SelectSave;
      ToolStripItem exportItem = new ToolStripMenuItem();
      exportItem.Text = "Export...";
      exportItem.MouseUp += ContextMenuStrip_SelectExport;

      //matchListView1.ContextMenuStrip.Items.Add("Open");
      resultList.ContextMenuStrip.Items.Add(openItem);
      resultList.ContextMenuStrip.Items.Add(saveItem);
      resultList.ContextMenuStrip.Items.Add(exportItem);

      utility = new FamilyUtility();

      this.Controls.Add(resultList);

      relWorker = null;

      trace.TraceInformation("RelationFinderPanel::RelationFinderPanel()");
      ResetGui();

    }

    private void ResetGui()
    {
      stopButton.Enabled = false;
      startButton.Enabled = true;
      resultNoCtrl.Enabled = true;
    }

    public override void PasteFromClipboard(object clipboard)
    {
      trace.TraceEvent(TraceEventType.Information, 0, "PasteFromClipboard:" + clipboard);
    }

    void resultList_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
    {
      if (parentForm != null)
      {
        //IndividualClass person = familyTree.GetIndividual(e.Node.Tag.ToString());
        if (e.Node.Tag != null)
        {
          parentForm.SetSelectedIndividual(e.Node.Tag.ToString());
        }
      }
    }

    void ContextMenuStrip_SelectOpen(object sender, MouseEventArgs e)
    {
      ReadListFromFile();
    }
    void ContextMenuStrip_SelectSave(object sender, MouseEventArgs e)
    {
      if (relationList != null)
      {
        SaveListConents(relationList);
      }
      else
      {
        MessageBox.Show("Error", "No list to save");
      }
    }

    void ContextMenuStrip_SelectExport(object sender, MouseEventArgs e)
    {
      if (relationList != null)
      {
        ExportListContents(relationList);
      }
      else
      {
        MessageBox.Show("Error", "No list to save");
      }
    }

    void ReadListFromFile()
    {
      OpenFileDialog fileDlg = new OpenFileDialog();
      fileDlg.InitialDirectory = utility.GetCurrentDirectory();
      fileDlg.Filter = "Stats List|*.fsrel";

      if (fileDlg.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
      {
        DataContractSerializer serializer = new DataContractSerializer(typeof(RelationStackList));

        FileStream readFile = new FileStream(fileDlg.FileName, FileMode.Open);
        RelationStackList localRelation;
        localRelation = (RelationStackList)serializer.ReadObject(readFile);

        //stats = localStats;
        //stats.SetFamilyTree(familyTree);
        readFile.Close();

        if (localRelation.sourceTree != null)
        {
          if(localRelation.sourceTree != familyTree.GetSourceFileName())
          {
            MessageBox.Show("Warning: The relation list does not seem to be from the current family tree: " + localRelation.sourceTree + " " + familyTree.GetSourceFileName());
          }
        }

        ShowRelations(localRelation);

        ResetGui();

      }
    }


    private void SaveListConents(RelationStackList relationList)
    {
      SaveFileDialog fileDlg = new SaveFileDialog();
      fileDlg.Filter = "Relation List|*.fsrel";
      fileDlg.InitialDirectory = utility.GetCurrentDirectory();

      if (fileDlg.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
      {
        SaveListToFile(fileDlg.FileName, relationList);
      }
    }

    private void ExportListContents(RelationStackList relationList)
    {
      SaveFileDialog fileDlg = new SaveFileDialog();
      fileDlg.Filter = "Relation List|*.txt";
      fileDlg.InitialDirectory = utility.GetCurrentDirectory();

      if (fileDlg.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
      {
        ExportListToFile(fileDlg.FileName, relationList);
      }
    }

    private void SaveListToFile(string filename, RelationStackList relationList)
    {
      FileStream saveList = new FileStream(filename, FileMode.Create);

      DataContractSerializer serializer = new DataContractSerializer(typeof(RelationStackList));

      serializer.WriteObject(saveList, relationList);
      saveList.Close();
    }

    private void ExportListToFile(string filename, RelationStackList relationList)
    {
      StreamWriter exportFile = new StreamWriter(filename);

      exportFile.Write(relationList.ToString(familyTree));

      exportFile.Close();
    }


    void resultList_SelectedIndexChanged(object sender, EventArgs e)
    {
      trace.TraceInformation("RelationFinderPanel::resultList_SelectedIndexChanged()" + sender.ToString() + e.ToString());
    }

    /*public void AnalyseProgress(int progressPercent, string text = null)
    {
      trace.TraceInformation("RelationFinderPanel::AnalyseProgress(" + progressPercent + ")");
      if (progressPercent < 0)
      {
        //AddToListView(ref resultList, stats);

        if (relWorker != null)
        {
          RelationStackList relations = relWorker.GetRelationStack();
          if (relations != null)
          {
            this.relationList = relations;
            ShowRelations(this.relationList);

            SaveListToFile("relations_" + familyTree.GetSourceFileName() + "_" + DateTime.Now.ToString().Replace("-", "").Replace(":", "").Replace(" ", "_") + "_" + resultNoCtrl.SelectedItem.ToString() + "_gen_" + ".fsrel", relationList);
          }
          relWorker = null;
        }
        ResetGui();

      }
    }*/

    private void ShowRelations(RelationStackList relations)
    {
      resultList.Nodes.Clear();
      if (relationList != null)
      {
        foreach (RelationStack rel in relations.relations)
        {
          TreeNode treeRel = new TreeNode(rel.CalculateRelation(familyTree));

          foreach (Relation relative in rel)
          {
            TreeNode person = new TreeNode(relative.ToString(familyTree));

            person.Tag = relative.personXref;
            treeRel.Nodes.Add(person);
          }
          treeRel.Expand();

          resultList.Nodes.Add(treeRel);
        }
      }

    }

    void startButton_MouseClick(object sender, MouseEventArgs e)
    {
      trace.TraceInformation("RelationFinderPanel::startButton_MouseClick()" + DateTime.Now);
      if (familyTree.GetHomeIndividual() == null)
      {
        MessageBox.Show("Error: No root / base person selected!");
        return;
      }
      if (selectedIndividual != null)
      {
        int noOfGenerations = AncestorStatistics.AllGenerations;
        RelationStackList relationList = new RelationStackList();

        if (resultNoCtrl.SelectedItem.ToString() != "All")
        {
          noOfGenerations = Convert.ToInt32(resultNoCtrl.SelectedItem.ToString());
        }

        FamilyFormProgress progress = new FamilyFormProgress(RelationProgress);
        //CheckRelation relation = new CheckRelation(familyTree, selectedIndividual.GetXrefName(), familyTree.GetHomeIndividual(), noOfGenerations, ref relationList);
        this.relWorker = new RelationTreeWorker(this,
          progress,
          selectedIndividual.GetXrefName(),
          familyTree.GetHomeIndividual(),
          noOfGenerations,
          familyTree);

        //ShowRelations(relationList.relations);
        //this.relationList = relationList;
      }
      else
      {
        MessageBox.Show("Error: No person selected!");
      }
    }
    public void RelationProgress(int progressPercent, string text = null)
    {
      trace.TraceInformation("RelationFinderViewPanel::RelationProgress(" + progressPercent + ")");
      parentForm.TextCallback(progressPercent, text);

      if (progressPercent < 0)
      {
        //AddToListView(ref resultList, stats);        if (relWorker != null)
        RelationStackList relations = relWorker.GetRelationStack();
        if (relations != null)
        {
          this.relationList = relations;
          ShowRelations(this.relationList);

          SaveListToFile("relations_" + familyTree.GetSourceFileName() + "_" + DateTime.Now.ToString().Replace("-", "").Replace(":", "").Replace(" ", "_") + "_" + resultNoCtrl.SelectedItem.ToString() + "_gen_" + ".fsrel", relationList);
        }
        relWorker = null;


        //SaveListToFile(utility.GetCurrentDirectory() + "\\relations_" + familyTree.GetSourceFileName() + "_" + DateTime.Now.ToString().Replace("-", "").Replace(":", "").Replace(" ", "_") + "_" + resultNoCtrl.SelectedItem.ToString() + "_gen_" + (checkDescendants ? "_desc_" : "") + ".fss");
        resultNoCtrl.Enabled = true;
        stopButton.Enabled = false;
        startButton.Enabled = true;
        //descendants.Enabled = true;

      }
    }

    void stopButton_MouseClick(object sender, MouseEventArgs e)
    {
      trace.TraceInformation("RelationFinderPanel::stopButton_MouseClick():" + DateTime.Now);
      if (stopButton.Visible)
      {

        relationList = null;
        ResetGui();

      }
    }
    public override void SetParentForm(FamilyForm2 parentForm)
    {
      this.parentForm = parentForm;

      trace.TraceInformation("RelationFinderPanel::SetParentForm()");
      parentForm.SelectedPersonChanged += OnSelectedPersonChangedEvent;

    }



    public override void SetFamilyTree(FamilyTreeStoreBaseClass inFamilyTree)
    {
      trace.TraceInformation("RelationFinderPanel::SetFamilyTree():" + DateTime.Now);

      familyTree = inFamilyTree;

    }

    public void SetSelectedIndividual(String xrefName)
    {
      trace.TraceInformation("RelationFinderPanel::SetSelectedIndividual(" + xrefName + ")");
      if (familyTree != null)
      {
        selectedIndividual = (IndividualClass)familyTree.GetIndividual(xrefName);

      }
    }

    public override string GetTitle()
    {
      return "Relation";
    }


    public override void OnSelectedPersonChangedEvent(object sender, PersonChangeEvent e)
    {
      if (familyTree != null)
      {
        selectedIndividual = e.selectedPerson;
      }

    }


    /*private class AnalyseTreeWorker : AsyncWorkerProgress
    {
      private bool printDecode;
      private BackgroundWorker backgroundWorker;
      private DateTime startTime;
      //private FamilyTreeStoreBaseClass familyTree;
      //string workerFileName;
      AncestorStatistics stats;
      ProgressReporter progressReporter;
      string startPersonXref;

      public AnalyseTreeWorker(
        object sender,
        ProgressReporter progress,
        string startIndividualXref,
        ref AncestorStatistics stats)
      {
        printDecode = false;

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
          stats.AnalyseAncestors(startperson, 0, 0.0);
        }

        if (printDecode)
        {
          trace.TraceInformation("AnalyseTreeWorker::DoWork(" + ")" + DateTime.Now);
        }
      }

      public void ProgressChanged(object sender, ProgressChangedEventArgs e)
      {
        if (printDecode)
        {
          trace.TraceInformation("AnalyseTreeWorker::ProgressChanged(" + e.ProgressPercentage + ")" + DateTime.Now);
        }

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
        if (printDecode)
        {
          trace.TraceInformation("AnalyseTreeWorker::Completed()" + DateTime.Now);
          trace.TraceInformation("  Start time:" + startTime + " end time: " + DateTime.Now);
        }

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
    }*/




  }

  class RelationTreeWorker : AsyncWorkerProgress
  {
    private BackgroundWorker backgroundWorker;
    private DateTime startTime;
    //private FamilyTreeStoreBaseClass familyTree;
    //string workerFileName;
    //AncestorStatistics stats;
    private CheckRelation relation;
    ProgressReporter progressReporter;
    string startPerson1Xref;
    string startPerson2Xref;
    private TraceSource trace;
    private FamilyTreeStoreBaseClass familyTree;
    private int noOfGenerations;
    private RelationStackList relationList;

    public RelationStackList GetRelationStack()
    {
      return relationList;
    }

    public RelationTreeWorker(
      object sender,
      ProgressReporter progress,
      string startPerson1Xref,
      string startPerson2Xref,
      int noOfGenerations,
      FamilyTreeStoreBaseClass familyTree)
    {
      trace = new TraceSource("RelationTreeWorker", SourceLevels.Information);

      //familyTree = stats.familyTree;

      progressReporter = progress;
      //this.stats = stats;
      this.familyTree = familyTree;
      this.startPerson1Xref = startPerson1Xref;
      this.startPerson2Xref = startPerson2Xref;
      this.noOfGenerations = noOfGenerations;
      this.relationList = new RelationStackList();



      backgroundWorker = new BackgroundWorker();

      backgroundWorker.WorkerReportsProgress = true;
      backgroundWorker.DoWork += new DoWorkEventHandler(DoWork);
      backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(Completed);
      backgroundWorker.ProgressChanged += new ProgressChangedEventHandler(ProgressChanged);

      backgroundWorker.RunWorkerAsync();

    }
    public void DoWork(object sender, DoWorkEventArgs e)
    {

      // This method will run on a thread other than the UI thread.
      // Be sure not to manipulate any Windows Forms controls created
      // on the UI thread from this method.
      startTime = DateTime.Now;
      //workerFileName = (String)e.Argument;
      //IndividualClass startperson = stats.GetFamilyTree().GetIndividual(startPersonXref);

      trace.TraceInformation("RelationTreeWorker ::DoWork()-done" + DateTime.Now);
      if ((startPerson1Xref != null) && (startPerson2Xref != null))
      {
        //CheckRelation relation = new CheckRelation(familyTree, selectedIndividual.GetXrefName(), familyTree.GetHomeIndividual(), noOfGenerations, ref relationList);
        this.relation = new CheckRelation(familyTree, startPerson1Xref, startPerson2Xref, noOfGenerations, ref relationList, progressReporter);
        //stats.AnalyseAncestors(startperson, 0, 0.0);
      }

      trace.TraceInformation("RelationTreeWorker ::DoWork()-done" + DateTime.Now);
    }

    public void ProgressChanged(object sender, ProgressChangedEventArgs e)
    {
      trace.TraceInformation("RelationTreeWorker::ProgressChanged(" + e.ProgressPercentage + ")" + DateTime.Now);

      /*if (stats.GetFamilyTree() != null)
      {
        progressReporter.ReportProgress(e.ProgressPercentage, stats.GetFamilyTree().GetShortTreeInfo());
      }
      else
      {
        progressReporter.ReportProgress(e.ProgressPercentage);
      }*/
    }
    public void Completed(object sender, RunWorkerCompletedEventArgs e)
    {
      trace.TraceInformation("AnalyseTreeWorker::Completed()" + DateTime.Now);
      trace.TraceInformation("  Start time:" + startTime + " end time: " + DateTime.Now);

      progressReporter.Completed();
      //stats.Print();
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

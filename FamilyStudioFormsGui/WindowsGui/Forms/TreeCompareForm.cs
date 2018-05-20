using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Windows.Forms;
using FamilyStudioData.FamilyTreeStore;
using FamilyStudioData.FamilyData;

namespace FamilyStudioFormsGui.WindowsGui.Forms
{
  public partial class TreeCompareForm : Form
  {
    private static TraceSource trace = new TraceSource("TreeCompareForm", SourceLevels.Warning);
    private MDIFamilyParent parent;
    private IList<FamilyForm2> formList;
    //private ListView matchLis
    private FamilyForm2 selectedForm1;
    private FamilyForm2 selectedForm2;
    private String individual1;
    private String individual2;
    private FamilyUtility utility;
    private CompareTreeWorker compareWorker;
    private FamilyTreeStoreBaseClass familyTree1;
    private FamilyTreeStoreBaseClass familyTree2;

    public TreeCompareForm(IList<Form> mdiChildren, MDIFamilyParent parent)
    {
      this.parent = parent;

      InitializeComponent();
      formList = new List<FamilyForm2>();

      matchListView1.Columns.Add("Name (tree 1)", 120, HorizontalAlignment.Left);
      matchListView1.Columns.Add("Birth", 80, HorizontalAlignment.Left);
      matchListView1.Columns.Add("Death", 80, HorizontalAlignment.Left);
      matchListView1.Columns.Add("Quality", 80, HorizontalAlignment.Left);
      matchListView1.Columns.Add("Name (tree 2)", 120, HorizontalAlignment.Left);
      matchListView1.Columns.Add("Birth", 80, HorizontalAlignment.Left);
      matchListView1.Columns.Add("Death", 80, HorizontalAlignment.Left);
      matchListView1.Columns.Add("Quality", 80, HorizontalAlignment.Left);
      matchListView1.Columns.Add("Difference", 80, HorizontalAlignment.Left);

      matchListView1.SelectedIndexChanged += matchListView1_SelectedIndexChanged;

      matchListView1.ContextMenuStrip = new ContextMenuStrip();
      ToolStripItem openItem = new ToolStripMenuItem();
      openItem.Text = "Open...";
      openItem.MouseUp += ContextMenuStrip_SelectOpen;
      ToolStripItem saveItem = new ToolStripMenuItem();
      saveItem.Text = "Save...";
      saveItem.MouseUp += ContextMenuStrip_SelectSave;
      ToolStripItem exportItemText = new ToolStripMenuItem();
      exportItemText.Text = "Export text...";
      exportItemText.MouseUp += ContextMenuStrip_SelectExportText;

      ToolStripItem exportItemHtml = new ToolStripMenuItem();
      exportItemHtml.Text = "Export HTML...";
      exportItemHtml.MouseUp += ContextMenuStrip_SelectExportHtml;

      ToolStripItem showPersons = new ToolStripMenuItem();
      showPersons.Text = "Show doublets...";
      showPersons.MouseUp += ContextMenuStrip_SelectPersons;


      //matchListView1.ContextMenuStrip.Items.Add("Open");
      matchListView1.ContextMenuStrip.Items.Add(openItem);
      matchListView1.ContextMenuStrip.Items.Add(saveItem);
      matchListView1.ContextMenuStrip.Items.Add(exportItemText);
      matchListView1.ContextMenuStrip.Items.Add(exportItemHtml);
      matchListView1.ContextMenuStrip.Items.Add(showPersons);

      //matchListView1.ContextMenuStrip.MouseClick += ContextMenuStrip_MouseClick;
      //matchListView1.ContextMenuStrip.Items.Add(saveItem);

      utility = new FamilyUtility();

      foreach (Form form in mdiChildren)
      {
        if (form.GetType() == typeof(FamilyForm2))
        {
          string windowName = form.Text;

          if (windowName.LastIndexOf('\\') >= 0)
          {
            windowName = windowName.Substring(windowName.LastIndexOf('\\') + 1);
          }
          listBox1.Items.Add(windowName);
          listBox2.Items.Add(windowName);
          formList.Add((FamilyForm2)form);
        }
      }
    }

    private void ContextMenuStrip_SelectPersons(object sender, MouseEventArgs e)
    {
      if (selectedForm1 != null)
      {
        String personXref = individual1;
        IndividualClass person = selectedForm1.GetTree().GetIndividual(personXref);

        if (person != null)
        {
          IList<string> urlList = person.GetUrlList();
          if (urlList != null)
          {
            foreach (string url in urlList)
            {
              Process.Start(url);
            }
          }
        }
      }
      if (selectedForm2 != null)
      {
        String personXref = individual2;
        IndividualClass person = selectedForm2.GetTree().GetIndividual(personXref);

        if (person != null)
        {
          IList<string> urlList = person.GetUrlList();
          if (urlList != null)
          {
            foreach (string url in urlList)
            {
              Process.Start(url);
            }
          }
        }
      }
    }

    void ContextMenuStrip_SelectOpen(object sender, MouseEventArgs e)
    {
      ReadListFromFile();
    }
    void ContextMenuStrip_SelectSave(object sender, MouseEventArgs e)
    {
      SaveListConents();
    }
    void ContextMenuStrip_SelectExportText(object sender, MouseEventArgs e)
    {
      ExportListConents(false);
    }
    void ContextMenuStrip_SelectExportHtml(object sender, MouseEventArgs e)
    {
      ExportListConents(true);
    }



    void ReadListFromFile()
    {
      OpenFileDialog fileDlg = new OpenFileDialog();
      fileDlg.InitialDirectory = utility.GetCurrentDirectory();
      fileDlg.Filter = "Compare List|*.fsc";

      if (fileDlg.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
      {
        DataContractSerializer serializer = new DataContractSerializer(typeof(SavedMatches));

        FileStream readFile = new FileStream(fileDlg.FileName, FileMode.Open);
        SavedMatches matches;
        matches = (SavedMatches)serializer.ReadObject(readFile);

        readFile.Close();

        bool found1 = false;
        bool found2 = false;
        familyTree1 = null;
        familyTree2 = null;

        int i = 0;
        foreach (FamilyForm2 form in formList)
        {
          if (matches.database1 == form.Text)
          {
            found1 = true;
            selectedForm1 = form;
            familyTree1 = form.GetTree();
            listBox1.SelectedIndex = i;

          }
          if (matches.database2 == form.Text)
          {
            found2 = true;
            selectedForm2 = form;
            familyTree2 = form.GetTree();
            listBox2.SelectedIndex = i;
          }
          i++;
        }

        if (found1 && found2)
        {
          matchListView1.Items.Clear();

          AsyncWorkerProgress reporter = new AsyncWorkerProgress(WorkProgress);

          compareWorker = new CompareTreeWorker(this, matches, reporter, familyTree1, familyTree2, ReportCompareResultFunction);

        }
      }
    }


    void SaveListConents()
    {
      SaveFileDialog fileDlg = new SaveFileDialog();
      fileDlg.Filter = "Compare List|*.fsc";
      fileDlg.InitialDirectory = utility.GetCurrentDirectory();

      if (fileDlg.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
      {
        FileStream saveList = new FileStream(fileDlg.FileName, FileMode.Create);

        SavedMatches savedMatches = new SavedMatches();
        DataContractSerializer serializer = new DataContractSerializer(typeof(SavedMatches));

        savedMatches.database1 = selectedForm1.Text;
        savedMatches.database2 = selectedForm2.Text;
        foreach (ListViewItem item in matchListView1.Items)
        {
          DuplicateTreeItems matchingPersons = (DuplicateTreeItems)item.Tag;

          savedMatches.itemList.Add(matchingPersons);
        }

        serializer.WriteObject(saveList, savedMatches);
        saveList.Close();
      }
    }

    string UrlsToString(IndividualClass person)
    {
      IList<string> urlList = person.GetUrlList();
      StringBuilder builder = new StringBuilder();
      if (urlList != null)
      {
        bool first = true;
        foreach (string url in urlList)
        {
          if (!first)
          {
            builder.Append(" ");
          }
          builder.Append(url);
          first = false;
        }
      }
      return builder.ToString();
    }

    string GetEventDateString(IndividualClass person, IndividualEventClass.EventType evType)
    {
      if (person != null)
      {
        IndividualEventClass ev = person.GetEvent(IndividualEventClass.EventType.Birth);

        if (ev != null)
        {
          FamilyDateTimeClass date = ev.GetDate();

          if (date != null)
          {
            return date.ToString();
          }
        }
      }
      return "";
    }

    void FormatPerson(IndividualClass person, bool html, StreamWriter exportFile)
    {
      if (!html)
      {
        exportFile.Write(UrlsToString(person));
        exportFile.Write("\t");
        exportFile.Write(person.GetName());
        exportFile.Write("\t");
        exportFile.Write(GetEventDateString(person, IndividualEventClass.EventType.Birth));
        exportFile.Write("\t");
        exportFile.Write(GetEventDateString(person, IndividualEventClass.EventType.Death));
        exportFile.Write("\t");
      }
      else
      {
        exportFile.Write("\n<td><a href=\"");
        exportFile.Write(UrlsToString(person));
        exportFile.Write("\">");
        exportFile.Write(person.GetName());
        exportFile.Write(" (");
        exportFile.Write(GetEventDateString(person, IndividualEventClass.EventType.Birth));
        exportFile.Write(" - ");
        exportFile.Write(GetEventDateString(person, IndividualEventClass.EventType.Death));
        exportFile.Write(")</a></td>\n");
      }    
    }

    void ExportListConents(bool html)
    {
      SaveFileDialog fileDlg = new SaveFileDialog();
      if (html)
      {
        fileDlg.Filter = "Compare List|*.html";
      }
      else
      {
        fileDlg.Filter = "Compare List|*.txt";
      }
      fileDlg.InitialDirectory = utility.GetCurrentDirectory();

      if (fileDlg.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
      {
        //FileStream saveList = new FileStream(fileDlg.FileName, FileMode.Create);
        StreamWriter exportFile = new StreamWriter(fileDlg.FileName);

        bool found1 = false;
        bool found2 = false;
        FamilyTreeStoreBaseClass familyTree1 = null;
        FamilyTreeStoreBaseClass familyTree2 = null;

        SavedMatches matches = new SavedMatches();
        matches.database1 = selectedForm1.Text;
        matches.database2 = selectedForm2.Text;
        int i = 0;
        foreach (FamilyForm2 form in formList)
        {
          if (matches.database1 == form.Text)
          {
            found1 = true;
            selectedForm1 = form;
            familyTree1 = form.GetTree();
            listBox1.SelectedIndex = i;

          }
          if (matches.database2 == form.Text)
          {
            found2 = true;
            selectedForm2 = form;
            familyTree2 = form.GetTree();
            listBox2.SelectedIndex = i;
          }
          i++;
        }

        if (found1 && found2)
        {
          if (html)
          {
            exportFile.WriteLine("<!DOCTYPE html><html><head><title>List of possible duplicate people</title></head><body><table><tr><th>Name1 (Birth - Death)</th><th>Name2 (Birth - Death)</th></tr>\n");
          }
          else
          {
            exportFile.WriteLine("Url\tName\tBirth\tDeath\tUrl\tName\tBirth\tDeath");
          }

          foreach (ListViewItem item in matchListView1.Items)
          {
            DuplicateTreeItems matchingPersons = (DuplicateTreeItems)item.Tag;
            IndividualClass person1 = null, person2 = null;

            person1 = familyTree1.GetIndividual(matchingPersons.item1);
            person2 = familyTree2.GetIndividual(matchingPersons.item2);
            if ((person1 != null) && (person2 != null))
            {
              if(html)
              {
                exportFile.WriteLine("\n<tr>\n");
              }
              FormatPerson(person1, html, exportFile);
              FormatPerson(person2, html, exportFile);
              if (html)
              {
                exportFile.WriteLine("\n</tr>\n");
              }
            }
          }
        }

        if (html)
        {
          exportFile.WriteLine("\n</table></body></html>");
        }
        exportFile.Close();
      }
    }

    void matchListView1_SelectedIndexChanged(object sender, EventArgs e)
    {
      foreach (ListViewItem item in matchListView1.SelectedItems)
      {
        DuplicateTreeItems items = (DuplicateTreeItems)item.Tag;
        if (selectedForm1 != null)
        {
          selectedForm1.SetSelectedIndividual(items.item1);
          individual1 = items.item1;
        }
        if (selectedForm2 != null)
        {
          selectedForm2.SetSelectedIndividual(items.item2);
          individual2 = items.item2;
        }
      }
    }


    public void WorkProgressDo(int progressPercent, string text = null)
    {
      if (progressPercent < 0)
      {
        progressBar1.Hide();
        if(compareWorker != null)
        {
          compareWorker = null;
        }

      }
      else
      {
        if (!progressBar1.Visible)
        {
          progressBar1.Show();
        }
        progressBar1.Value = progressPercent;
        //TextCallback(progressPercent, text);
        trace.TraceInformation("progress:" + progressPercent + "%");
      }

    }

    public void WorkProgress(int progressPercent, string text = null)
    {
      if(progressBar1.InvokeRequired)
      {
        Invoke(new Action(() => WorkProgressDo(progressPercent, text)));
      }
      else
      {
        WorkProgressDo(progressPercent, text);
      }
    }

    void AddToListbox(ListViewItem lvItem)
    {
      matchListView1.Items.Add(lvItem);
    }
    void MarkCompareFinished()
    {
      button1.Text = "Compare";
    }

    delegate void ReportCompareResult(ListViewItem lvItem);

    void ReportCompareResultFunction(ListViewItem lvItem)
    {
      if(matchListView1.InvokeRequired)
      {
        //SetTextCallback callback = new SetTextCallback(TextCallback);
        if (lvItem != null)
        {
          Invoke(new Action(() => AddToListbox(lvItem)));
        } 
        else
        {
          Invoke(new Action(() => MarkCompareFinished()));
        }
      }
      else
      {
        if (lvItem != null)
        {
          AddToListbox(lvItem);
        }
        else
        {
          MarkCompareFinished();
        }
      }
    }
  

    private void button1_Click(object sender, EventArgs e)
    {
      if(compareWorker != null)
      {
        trace.TraceInformation("Warning, compare already running...");
        return;
      }
      if (listBox1.SelectedIndex < 0)
      {
        MessageBox.Show("No tree selected as first", "Error", MessageBoxButtons.OK);
        return;
      }
      if (listBox2.SelectedIndex < 0)
      {
        MessageBox.Show("No tree selected as second", "Error", MessageBoxButtons.OK);
        return;
      }

      string name1 = listBox1.Items[listBox1.SelectedIndex].ToString();
      string name2 = listBox2.Items[listBox2.SelectedIndex].ToString();

      FamilyTreeStoreBaseClass familyTree1 = null;
      FamilyTreeStoreBaseClass familyTree2 = null;

      foreach (FamilyForm2 form in formList)
      {
        if (form.Text.IndexOf(name1) >= 0)
        {
          familyTree1 = form.GetTree();
          selectedForm1 = form;
        }
        if (form.Text.IndexOf(name2) >= 0)
        {
          familyTree2 = form.GetTree();
          selectedForm2 = form;
        }
      }



      if ((familyTree1 != null) && (familyTree2 != null))
      {
        matchListView1.Items.Clear();

        AsyncWorkerProgress reporter = new AsyncWorkerProgress(WorkProgress);

        compareWorker = new CompareTreeWorker(this, null, reporter, familyTree1, familyTree2, ReportCompareResultFunction);

        button1.Text = "Stop";

      }
    }


    private class CompareTreeWorker : AsyncWorkerProgressInterface
    {
      private BackgroundWorker backgroundWorker;
      private DateTime startTime;
      AsyncWorkerProgress progressReporter;
      private TraceSource trace;
      private ReportCompareResult resultReporterFunction;
      public SavedMatches matches;

      private class WorkerInterface
      {
        public FamilyTreeStoreBaseClass familyTree1;
        public FamilyTreeStoreBaseClass familyTree2;
      }

      public CompareTreeWorker(
        object sender,
        SavedMatches matches,
        AsyncWorkerProgress progress,
        FamilyTreeStoreBaseClass familyTree1,
        FamilyTreeStoreBaseClass familyTree2,
        ReportCompareResult resultReporter)
      {
        trace = new TraceSource("CompareTreeWorker", SourceLevels.All);

        resultReporterFunction = resultReporter;

        progressReporter = progress;
        this.matches = matches;

        backgroundWorker = new BackgroundWorker();

        backgroundWorker.WorkerReportsProgress = true;
        if (matches == null)
        {
          backgroundWorker.DoWork += new DoWorkEventHandler(DoWork);
        }
        else
        {
          backgroundWorker.DoWork += new DoWorkEventHandler(DoWorkLoadFile);
        }

        backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(Completed);
        backgroundWorker.ProgressChanged += new ProgressChangedEventHandler(ProgressChanged);

        WorkerInterface workerInterface = new WorkerInterface();
        workerInterface.familyTree1 = familyTree1;
        workerInterface.familyTree2 = familyTree2;
        backgroundWorker.RunWorkerAsync(workerInterface);

      }

      string GetShortFacts(FamilyStatusClass.IndividualStatus status)
      {
        string statusStr = "";

        statusStr += ConvertCorrectnessToString("birth date", status.birthCorrectness);
        statusStr += "; " + ConvertCorrectnessToString("death date", status.deathCorrectness);
        statusStr += "; " + status.noOfParents + " parents";
        statusStr += "; " + status.noOfChildren + " children";

        return statusStr;
      }

      private string GetPersonString(IndividualClass person, string str)
      {
        return person.GetName() + ",b:" + person.GetDate(IndividualEventClass.EventType.Birth).ToString() + ",d:" + person.GetDate(IndividualEventClass.EventType.Death).ToString() + ",f:" + str;
      }

      ListViewItem CreateListItem(FamilyTreeStoreBaseClass familyTree1, IndividualClass person1, FamilyTreeStoreBaseClass familyTree2, IndividualClass person2)
      {
        ListViewItem item = new ListViewItem(person1.GetName());
        FamilyStatusClass.IndividualStatus status1 = FamilyStatusClass.CheckCorrectness(familyTree1, person1);
        FamilyStatusClass.IndividualStatus status2 = FamilyStatusClass.CheckCorrectness(familyTree2, person2);
        string str1 = GetShortFacts(status1);
        string str2 = GetShortFacts(status2);
        item.SubItems.AddRange(new string[] { person1.GetDate(IndividualEventClass.EventType.Birth).ToString(), person1.GetDate(IndividualEventClass.EventType.Death).ToString(), str1, person2.GetName(), person2.GetDate(IndividualEventClass.EventType.Birth).ToString(), person2.GetDate(IndividualEventClass.EventType.Death).ToString(), str2 });

        trace.TraceInformation("match1:" + GetPersonString(person1, str1));
        trace.TraceInformation("match2:" + GetPersonString(person2, str2));

        item.UseItemStyleForSubItems = false;
        if (!person1.GetDate(IndividualEventClass.EventType.Birth).ToString().Equals(person2.GetDate(IndividualEventClass.EventType.Birth).ToString()))
        {
          //string checkChar = "Good birth";
          int idx1 = 1;
          int idx2 = 5;
          if (status1.birthCorrectness == FamilyStatusClass.EventCorrectness.Perfect && status2.birthCorrectness != FamilyStatusClass.EventCorrectness.Perfect)
          {
            item.SubItems[idx1].BackColor = Color.LightGreen;
            item.SubItems[idx2].BackColor = Color.LightSalmon;
          }
          else if (status1.birthCorrectness != FamilyStatusClass.EventCorrectness.Perfect && status2.birthCorrectness == FamilyStatusClass.EventCorrectness.Perfect)
          {
            item.SubItems[idx1].BackColor = Color.LightSalmon;
            item.SubItems[idx2].BackColor = Color.LightGreen;
          }
          else
          {
            item.SubItems[idx1].BackColor = Color.Yellow;
            item.SubItems[idx2].BackColor = Color.Yellow;
          }
        }
        if (!person1.GetDate(IndividualEventClass.EventType.Death).ToString().Equals(person2.GetDate(IndividualEventClass.EventType.Death).ToString()))
        {
          int idx1 = 2;
          int idx2 = 6;
          if (status1.deathCorrectness == FamilyStatusClass.EventCorrectness.Perfect && status2.deathCorrectness != FamilyStatusClass.EventCorrectness.Perfect)
          {
            item.SubItems[idx1].BackColor = Color.LightGreen;
            item.SubItems[idx2].BackColor = Color.LightSalmon;
          }
          else if (status1.deathCorrectness != FamilyStatusClass.EventCorrectness.Perfect && status2.deathCorrectness == FamilyStatusClass.EventCorrectness.Perfect)
          {
            item.SubItems[idx1].BackColor = Color.LightSalmon;
            item.SubItems[idx2].BackColor = Color.LightGreen;
          }
          else
          {
            item.SubItems[idx1].BackColor = Color.Yellow;
            item.SubItems[idx2].BackColor = Color.Yellow;
          }
        }
        if (!str1.Equals(str2))
        {
          item.SubItems[3].BackColor = Color.Yellow;
          item.SubItems[7].BackColor = Color.Yellow;
          //item.GetSubItemAt(2, 0).BackColor = Color.Blue;
          //item.GetSubItemAt(5, 0).BackColor = Color.Brown;
        }


        //item.Tag = person1.GetXrefName();
        item.Tag = new DuplicateTreeItems(person1.GetXrefName(), person2.GetXrefName());

        //matchListView1.Items.Add(item);
        return item;
      }


      string ConvertCorrectnessToString(string s, FamilyStatusClass.EventCorrectness correctness)
      {
        switch (correctness)
        {
          case FamilyStatusClass.EventCorrectness.Perfect:
            return "Good " + s;

          case FamilyStatusClass.EventCorrectness.Semi:
            return "Inexact " + s;
        }
        return "No " + s;
      }

      private void ReportMatchingProfiles(FamilyTreeStoreBaseClass familyTree1, string person1, FamilyTreeStoreBaseClass familyTree2, string person2)
      {
        IndividualClass person1full = familyTree1.GetIndividual(person1);
        IndividualClass person2full = familyTree2.GetIndividual(person2);
        resultReporterFunction(CreateListItem(familyTree1, person1full, familyTree2, person2full));

      }


      private void DoCompare(FamilyTreeStoreBaseClass familyTree1, FamilyTreeStoreBaseClass familyTree2, AsyncWorkerProgress reporter)
      {

        CompareTreeClass.CompareTrees(familyTree1, familyTree2, ReportMatchingProfiles, reporter);
      }

      public void DoWork(object sender, DoWorkEventArgs e)
      {

        // This method will run on a thread other than the UI thread.
        // Be sure not to manipulate any Windows Forms controls created
        // on the UI thread from this method.
        startTime = DateTime.Now;

        WorkerInterface workerInput = (WorkerInterface)e.Argument;

        DoCompare(workerInput.familyTree1, workerInput.familyTree2, progressReporter);

        trace.TraceInformation("CompareTreeWorker::DoWork(" + ")" + DateTime.Now);
      }
      public void DoWorkLoadFile(object sender, DoWorkEventArgs e)
      {

        // This method will run on a thread other than the UI thread.
        // Be sure not to manipulate any Windows Forms controls created
        // on the UI thread from this method.
        startTime = DateTime.Now;

        WorkerInterface workerInput = (WorkerInterface)e.Argument;

        foreach (DuplicateTreeItems itemPair in matches.itemList)
        {
          IndividualClass person1 = null, person2 = null;

          person1 = workerInput.familyTree1.GetIndividual(itemPair.item1);
          person2 = workerInput.familyTree2.GetIndividual(itemPair.item2);

          if ((person1 != null) && (person2 != null))
          {
            resultReporterFunction(CreateListItem(workerInput.familyTree1, person1, workerInput.familyTree2, person2));
          }
        }
        trace.TraceInformation("CompareTreeWorker::DoWork(" + ")" + DateTime.Now);
      }

      public void ProgressChanged(object sender, ProgressChangedEventArgs e)
      {
        trace.TraceInformation("CompareTreeWorker::ProgressChanged(" + e.ProgressPercentage + ")" + DateTime.Now);

        progressReporter.ReportProgress(e.ProgressPercentage);
      }
      public void Completed(object sender, RunWorkerCompletedEventArgs e)
      {
        trace.TraceInformation("CompareTreeWorker::Completed()" + DateTime.Now);
        trace.TraceInformation("  Start time:" + startTime + " end time: " + DateTime.Now);

        progressReporter.Completed();
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

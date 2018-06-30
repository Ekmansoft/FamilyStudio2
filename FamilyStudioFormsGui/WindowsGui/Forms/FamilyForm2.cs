using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
//using System.Reflection;
//using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Windows;
//using System.Windows.Controls;
//using System.Windows.Shapes;
using FamilyStudioData.FamilyData;
//using FamilyStudio1.GedcomDecoder;
//using System.Windows.Forms.Integration;
//using FamilyStudioFormsGui.WindowsGui.Controls;
using FamilyStudioFormsGui.WindowsGui.Forms;
//using FamilyStudioData.FamilyFileFormat;
using FamilyStudioData.FamilyTreeStore;
using FamilyStudioFormsGui.WindowsGui.Panels;
using FamilyStudioFormsGui.WindowsGui.Panels.AsyncTreePanel1;
using FamilyStudioFormsGui.WindowsGui.Panels.TreeViewPanel4;
using FamilyStudioFormsGui.WindowsGui.Panels.TreeViewPanel5;
using FamilyStudioFormsGui.WindowsGui.Panels.TreeViewPanel2;
using FamilyStudioFormsGui.WindowsGui.Panels.TreeViewPanel1;
using FamilyStudioFormsGui.WindowsGui.Panels.PersonViewPanel1;
using FamilyStudioFormsGui.WindowsGui.Panels.CompletenessViewPanel1;
using FamilyStudioFormsGui.WindowsGui.Panels.RelationFinderPanel;
using FamilyStudioFormsGui.WindowsGui.Panels.SearchPanel1;
//using FamilyStudioFormsGui.WindowsGui.Panels.ComparePanel1;
using FamilyStudioFormsGui.WindowsGui.Panels.NotePanel;
using FamilyStudioData.FamilyFileFormat;
using System.Drawing.Imaging;
using System.Net;
//using System.Collections;

namespace FamilyStudioFormsGui.WindowsGui.Forms
{
  public partial class FamilyForm2 : Form
  {
    private IList<TreeViewPanelBaseClass> panelList;
    private String filename;
    private FamilyTreeStoreBaseClass familyTree;
    private IndividualClass selectedIndividual;
    private IList<string> searchResultXrefList;
    //private IList<Type> typeList;
    private AddPersonForm m_addPersonForm;
    private System.Windows.Forms.Timer updateTimer;

    public IList<IndividualFilterClass> filterList;

    private ReadFileWorker  readFileWorker;
    private WriteFileWorker writeFileWorker;
    private AsyncWorkerProgress progressReporter;
    private int nextTreeTabToAdd;
    private int nextPersonTabToAdd;
    private TraceSource trace;

    private void AddSearchPanel(TreeViewPanelBaseClass panel)
    {
      trace.TraceInformation("Creating search panel:" + panel.GetTitle() + "," + panel.ToString());
      //treeViewPanelList.Add(panel);
      panelList.Add(panel);
      this.splitContainer1.Panel1.Controls.Add(panel);
    }
    private void AddTreePanel(TreeViewPanelBaseClass panel)
    {
      if (tabControl1.TabPages.Count <= nextTreeTabToAdd)
      {
        tabControl1.TabPages.Add("");
      }
      trace.TraceInformation("Creating tree panel:" + panel.GetTitle() + "," + panel.ToString());
      TabPage currentPage = tabControl1.TabPages[nextTreeTabToAdd];
      currentPage.Controls.Add(panel);
      currentPage.Text = panel.GetTitle();
      panelList.Add(panel);

      nextTreeTabToAdd++;
    }

    private void AddPersonPanel(TreeViewPanelBaseClass panel)
    {
      if (tabControl2.TabPages.Count < (nextPersonTabToAdd + 1))
      {
        tabControl2.TabPages.Add("");
      }
      trace.TraceInformation("Creating person panel:" + panel.GetTitle() + "," + panel.ToString());
      TabPage currentPage = tabControl2.TabPages[nextPersonTabToAdd];
      currentPage.Controls.Add(panel);
      currentPage.Text = panel.GetTitle();
      panelList.Add(panel);

      nextPersonTabToAdd++;
    }

    private void ConnectPanelsToTree(FamilyTreeStoreBaseClass tree)
    {
      foreach (TreeViewPanelBaseClass panel in panelList)
      {
        panel.SetFamilyTree(tree);
      }
      if (tree.GetIndividual() != null)
      {
        foreach (TreeViewPanelBaseClass panel in panelList)
        {
          panel.OnSelectedPersonChangedEvent(this, new PersonChangeEvent(tree.GetIndividual()));
          //panel.SetSelectedIndividual(tree.GetIndividual().GetXrefName());
        }
      }
      else
      {
        trace.TraceEvent(TraceEventType.Error, 0, "  Error: GetIndividual(first) == null! " + DateTime.Now);
      }


    }
    private void CompletedCallback(bool result)
    {
      trace.TraceInformation("FamilyForm2.CompletedCallback(" + result + ")" + DateTime.Now);
      //return true;
      if (familyTree != null)
      {
        string homePerson = familyTree.GetHomeIndividual();

        if (homePerson != null)
        {
          SetSelectedIndividual(homePerson);
        }
        if (InvokeRequired)
        {
          Invoke(new Action(() => ConnectPanelsToTree(familyTree)));
        }
        else
        {
          ConnectPanelsToTree(familyTree);
        }
      }
    }

    public FamilyForm2(bool createTree = false)
    {
      trace = new TraceSource("FamilyForm2", SourceLevels.Warning);
      nextTreeTabToAdd = 0;
      nextPersonTabToAdd = 0;

      InitializeComponent();

      filterList = new List<IndividualFilterClass>();

      panelList = new List<TreeViewPanelBaseClass>();

      AddSearchPanel(new SearchPanel1());

      AddTreePanel(new AsyncTreePanel1());
      AddTreePanel(new TreeViewPanel2());
      AddTreePanel(new TreeViewPanel4());
      AddTreePanel(new TreeViewPanel5());
      AddTreePanel(new TreeViewPanel1());
      AddTreePanel(new ImageViewPanel1());

      AddPersonPanel(new PersonViewPanel1());
      AddPersonPanel(new SearchPanel1());
      AddPersonPanel(new CompletenessViewPanel1());
      AddPersonPanel(new RelationFinderPanel());
      //AddPersonPanel(new ComparePanel1());
      AddPersonPanel(new NotePanel());

      searchResultXrefList = new List<string>();

      foreach (TreeViewPanelBaseClass panel in panelList)
      {
        panel.SetParentForm(this);
      }
      m_addPersonForm = new AddPersonForm();

      m_addPersonForm.SetEventHandler(new System.EventHandler(AddPersonEvent));
      m_addPersonForm.Visible = false;

      if (createTree)
      {
        FamilyFileTypeCollection codec = new FamilyFileTypeCollection();
        //familyTree = new FamilyTreeStoreRam();
        //FamilyFileEncoderCollection codec = new FamilyFileEncoderCollection();

        familyTree = codec.CreateFamilyTreeStore("", CompletedCallback);

        //ConnectPanelsToTree(familyTree);
      }
      updateTimer = new System.Windows.Forms.Timer();
      updateTimer.Tick += new EventHandler(TimerEventProcessor);
      updateTimer.Interval = 1000 * 10; // 10 seconds
      updateTimer.Enabled = true;
    }

    // This is the method to run when the timer is raised.
    private void TimerEventProcessor(Object myObject, EventArgs myEventArgs)
    {
      //updateTimer.Stop();

      //updateTimer.Enabled = true;
      if (!this.statusStrip1.InvokeRequired)
      {
        if (familyTree != null)
        {
          trace.TraceData(TraceEventType.Warning, 0, "Update timer expired:" + familyTree.GetShortTreeInfo() + toolStripStatusLabel1.Text + toolStripStatusLabel1.Visible);
          if (!toolStripStatusLabel1.Visible)
          {
            toolStripStatusLabel1.Text = familyTree.GetShortTreeInfo();
          }
        }
      } 
      else
      {
        trace.TraceData(TraceEventType.Warning, 0, "invoke req ");
      }
    }


    private string ExtractFilename(string filename)
    {
      int i = filename.Length - 1;
      int startPos = -1;

      while ((i > 0) && (startPos < 0))
      {
        if ((filename[i] == '/') || (filename[i] == '\\'))
        {
          startPos = i + 1;
        }
        i--;
      }
      if (startPos < 0)
      {
        startPos = 0;
      }
      return filename.Substring(startPos);

    }

    public delegate void PersonChangeHandler(object sender, PersonChangeEvent e);

    delegate void SetTextCallback(int percent, string text);

    public void TextCallback(int progressPercent, string text = null)
    {      
      if (this.statusStrip1.InvokeRequired)
      {
        SetTextCallback callback = new SetTextCallback(TextCallback);
        this.Invoke(callback, new object[] { progressPercent, text });
      }
      else
      {
        if (progressPercent >= 0)
        {
          if (!toolStripProgressBar1.Visible)
          {
            toolStripProgressBar1.Visible = true;
          }
          toolStripProgressBar1.Value = progressPercent;
          if (text != null)
          {
            toolStripStatusLabel1.Text = text;
          }
        }
        else
        {
          toolStripProgressBar1.Visible = false;
          toolStripStatusLabel1.Text = familyTree.GetShortTreeInfo();
        }
      }
    }

    public string GetSelectedIndividual()
    {
      if (selectedIndividual != null)
      {
        return selectedIndividual.GetXrefName();
      }
      return null;
    }

    public event PersonChangeHandler SelectedPersonChanged;

    public void CopyToClipboard(ref object clipboard)
    {
      if(selectedIndividual != null)
      {
        IndividualClass clipbboardIndividual = (IndividualClass)selectedIndividual.Clone();

        clipbboardIndividual.SetFamilyChildList(null);
        clipbboardIndividual.SetFamilySpouseList(null);

        clipboard = (object)clipbboardIndividual;
      }
    }
    public void PasteFromClipboard(object clipboard)
    {
      if(clipboard != null)
      {
        foreach (TabPage panelTab in tabControl1.TabPages)
        {
          Debug.WriteLine("TabPage:" + panelTab + " " + panelTab.Focused + " " + panelTab.CanFocus);
          foreach (Control tabControl in panelTab.Controls)
          {
            //Debug.WriteLine("Control:" + tabControl);
            if (tabControl.GetType().IsSubclassOf(typeof(TreeViewPanelBaseClass)))
            {
              Debug.WriteLine("TreeViewBase:" + tabControl + " " + tabControl.Focused + " " + tabControl.CanFocus);
              TreeViewPanelBaseClass panel = (TreeViewPanelBaseClass)tabControl;
              if (panel.CanFocus)
              {
                panel.PasteFromClipboard(clipboard);
              }
            }
          }
        }
      }

    }

    public void FileLoadProgress(int progressPercent, string text = null)
    {
      trace.TraceInformation("FamilyForm2::FileLoadProgress(" + progressPercent + ")");

      if (progressPercent < 0)
      {
        familyTree = readFileWorker.GetFamilyTree();
        if (familyTree != null)
        {
          if (trace.Switch.Level.HasFlag(SourceLevels.Information))
          {
            familyTree.Print();
          }

          toolStripProgressBar1.Visible = false;
          toolStripStatusLabel1.Text = familyTree.GetShortTreeInfo();//"I:" + familyTree.individualList.Count + " F:" + familyTree.familyList.Count + " N:" + familyTree.noteList.Count;

          //PopulatePersonList();

          ConnectPanelsToTree(familyTree);

        }
        else
        {
          trace.TraceEvent(TraceEventType.Error, 0, "  Error: tree == null! " + DateTime.Now);
        }
        progressReporter = null;
        readFileWorker = null;
      }
      else
      {
        TextCallback(progressPercent, text);
      }

    }

    public void FileSaveProgress(int progressPercent, string text = null)
    {
      trace.TraceInformation("FamilyForm2::FileSaveProgress(" + progressPercent + ")");

      if (progressPercent < 0)
      {
        toolStripProgressBar1.Visible = false;
        if (familyTree != null)
        {
          toolStripStatusLabel1.Text = familyTree.GetShortTreeInfo();//"I:" + familyTree.individualList.Count + " F:" + familyTree.familyList.Count + " N:" + familyTree.noteList.Count;

          trace.TraceInformation("FamilyForm2::FileSaveCompleted()-3");
        }
      }
      else
      {
        TextCallback(progressPercent, text);
      }

    }


    public void SetSelectedIndividual(String xrefName)
    {
      trace.TraceInformation("FamilyForm2::SetSelectedIndividual(" + xrefName + ")");
      selectedIndividual = (IndividualClass)familyTree.GetIndividual(xrefName);

      if (SelectedPersonChanged != null)
      {
        SelectedPersonChanged(this, new PersonChangeEvent(selectedIndividual));
      }

      if (familyTree != null)
      {
        toolStripStatusLabel1.Text = familyTree.GetShortTreeInfo();// "I:" + familyTree.individualList.Count + " F:" + familyTree.familyList.Count + " N:" + familyTree.noteList.Count;
      }
    }

    public void OpenFile(String FileName)
    {
      trace.TraceInformation("FamilyForm2::OpenFile(" + FileName + ")" + DateTime.Now);

      Text = ExtractFilename(FileName);

      filename = FileName;
      this.Text = filename;

      ImportFile(FileName);
    }
    public void OpenWeb(String FileName)
    {
      trace.TraceInformation("FamilyForm2::OpenWeb(" + FileName + ")" + DateTime.Now);

      progressReporter = new AsyncWorkerProgress(FileLoadProgress);

      readFileWorker = new ReadFileWorker(this, progressReporter, FileName, ref familyTree, CompletedCallback);
    }

    public void ImportFile(String FileName)
    {
      trace.TraceInformation("FamilyForm2::ImportFile(" + FileName + ")" + DateTime.Now);

      progressReporter = new AsyncWorkerProgress(FileLoadProgress);

      readFileWorker = new ReadFileWorker(this, progressReporter, FileName, ref familyTree, CompletedCallback);
    }


    public void ValidateTree()
    {
      trace.TraceInformation("ValidateTree()" + DateTime.Now);

      familyTree.ValidateTree();
      familyTree.ValidateFamilies();
      familyTree.ValidateIndividuals();
      trace.TraceInformation("ValidateTree() end time: " + DateTime.Now);

    }



    public string GetFileTypeFilter(FamilyFileTypeOperation operation)
    {
      if ((operation == FamilyFileTypeOperation.Open) || (operation == FamilyFileTypeOperation.Import))
      {
        FamilyFileTypeCollection codec = new FamilyFileTypeCollection();

        string filter;

        filter = codec.GetFileTypeFilter(operation);

        codec = null;

        return filter;
      }
      else if ((operation == FamilyFileTypeOperation.Save) || (operation == FamilyFileTypeOperation.Export))
      {
        FamilyFileEncoderCollection codec = new FamilyFileEncoderCollection();

        string filter;

        filter = codec.GetFileTypeFilter(operation);

        codec = null;

        return filter;
      }
      return null;
    }
    public string GetWebTypeList()
    {
      FamilyFileTypeCollection codec = new FamilyFileTypeCollection();

      string filter;

      filter = codec.GetWebTypeList();

      codec = null;

      return filter;
    }

    public void SaveFile(string filename, FamilyFileTypeOperation operation, int filterIndex)
    {
      trace.TraceInformation("FamilyForm2::SaveFile:" + filename + " idx:" + filterIndex);

      progressReporter = new AsyncWorkerProgress(FileSaveProgress);

      writeFileWorker = new WriteFileWorker(this, progressReporter, filename, operation, filterIndex, ref familyTree);

      if (operation == FamilyFileTypeOperation.Save)
      {
        this.Text = filename;
      }

    }


    public void AddPersonEvent(object sender, EventArgs e)
    {
      if (m_addPersonForm != null)
      {
        selectedIndividual = new IndividualClass();

        m_addPersonForm.GetPerson(ref selectedIndividual);

        //m_addPersonForm.Close();

        m_addPersonForm.Visible = false;

        selectedIndividual.SetXrefName(familyTree.CreateNewXref(XrefType.Individual));
        familyTree.AddIndividual(selectedIndividual);
        if (SelectedPersonChanged != null)
        {
          SelectedPersonChanged(this, new PersonChangeEvent(selectedIndividual));
        }

      }

    }

    public void AddPerson()
    {
      trace.TraceInformation("AddPerson");


      m_addPersonForm.Visible = true;

    }

    public void SetHomePerson()
    {
      if (familyTree != null)
      {
        if (selectedIndividual != null)
        {
          familyTree.SetHomeIndividual(selectedIndividual.GetXrefName());
        }
      }

    }

    public bool SaveImageLocal(String filename, String imageUrl, ImageFormat format)
    {
      WebClient client = new WebClient();
      Stream stream = null;
      //MultimediaLinkClass newLink = null;

      try
      {
        stream = client.OpenRead(imageUrl);
      }
      catch (WebException e)
      {
        trace.TraceData(TraceEventType.Warning, 0, "could not fetch " + imageUrl + " " + e.ToString());
        return false;
      }
      trace.TraceData(TraceEventType.Information, 0, "fetched " + imageUrl);
      if (stream != null)
      {
        Bitmap bitmap = null;
        try
        {
          bitmap = new Bitmap(stream);
        }
        catch (Exception e)
        {
          trace.TraceData(TraceEventType.Warning, 0, "stream error " + imageUrl + " " + e.ToString());
          return false;
        }

        if (bitmap != null)
        {
          bitmap.Save(filename, format);

          trace.TraceData(TraceEventType.Information, 0, "saved as " + filename);
          //newLink = new MultimediaLinkClass(link.GetFormat(), filename);
          //newLink.SetLink(filename);
        }

        stream.Flush();
        stream.Close();
      }
      client.Dispose();
      return true;
    }

    String GetLastPart(string url)
    {
      int fileNameStart = url.LastIndexOf('/');
      int filenameEnd = 0;


      if (fileNameStart < 0)
      {
        fileNameStart = url.LastIndexOf('\\');
      }

      if (fileNameStart < 0)
      {
        return url;
      }
      filenameEnd = url.Length - fileNameStart;

      String result = url.Substring(fileNameStart + 1, filenameEnd - 1);

      if (result.LastIndexOf('.') >= 0)
      {
        return result.Substring(0, result.LastIndexOf('.'));
      }

      return result;
    }

    public bool DownloadImages2(FamilyTreeStoreBaseClass tree)
    {

      IEnumerator<IndividualClass> individualIterator = tree.SearchPerson();
      List<IndividualClass> individualList = new List<IndividualClass>();
      FamilyUtility utility = new FamilyUtility();

      String dirname = utility.GetCurrentDirectory() + "\\" + FamilyUtility.MakeFilename("ImageDownload_" + tree.GetSourceFileName() + "_" + DateTime.Now.ToString());
      Directory.CreateDirectory(dirname);

      List<String> endings = new List<string>();

      endings.Add("_large.jpg");
      endings.Add("_medium.jpg");
      endings.Add("_t.jpg");
      endings.Add("_t2.jpg");

      while (individualIterator.MoveNext())
      {
        individualList.Add(individualIterator.Current);
      }
      foreach (IndividualClass person in individualList)
      {
        IList<MultimediaLinkClass> links = person.GetMultimediaLinkList();

        IList<MultimediaLinkClass> newLinks = new List<MultimediaLinkClass>();
        IList<String> dlLinks = new List<String>();
        IList<String> dlLinksBase = new List<String>();
        IList<String> dlLinksTrue = new List<String>();

        foreach (MultimediaLinkClass link in links)
        {
          Boolean webimage = (link.GetLink().ToLower().IndexOf("http:") >= 0) || (link.GetLink().ToLower().IndexOf("https:") >= 0);

          if (webimage)
          {
            dlLinks.Add(link.GetLink());
          }
        }

        foreach (String link in dlLinks)
        {
          String basename = link;

          if (link.ToLower().IndexOf(".jpg") >= 0)
          {
            foreach (String ending in endings)
            {
              if (link.ToLower().IndexOf(ending) >= 0)
              {
                basename = link.Substring(0, link.ToLower().IndexOf(ending));

                if (dlLinksBase.IndexOf(basename) < 0)
                {
                  dlLinksBase.Add(basename);
                }
              }
            }
          }
        }
        foreach (String link in dlLinksBase)
        {
          int bestIndex = -1;
          int index = 0;
          while ((bestIndex < 0) && (index < endings.Count))
          {
            String newLink = link + endings[index];
            bestIndex = dlLinks.IndexOf(newLink);
            dlLinksTrue.Add(newLink);
            index++;
          }
        }

        foreach (String link in dlLinksTrue)
        {
          String filename = dirname + "\\" + FamilyUtility.MakeFilename(person.GetXrefName() + " " + person.GetName() + " " + GetLastPart(link));

          if (filename.Length > 255)
          {
            filename = filename.Substring(0, 255);
          }
          filename = filename + ".jpg";
          //trace.TraceInformation(filename);

          if (SaveImageLocal(filename, link, ImageFormat.Jpeg))
          {
            newLinks.Add(new MultimediaLinkClass("image/jpeg", filename));
          }
        }
        foreach (MultimediaLinkClass link in newLinks)
        {
          person.AddMultimediaLink(link);
        }
      }

      return true;
    }

    public void DownloadImages()
    {
      if (familyTree != null)
      {
        //familyTree.DownloadImages();
        DownloadImages2(familyTree);
      }
    }

    public FamilyTreeStoreBaseClass GetTree()
    {
      return familyTree;
    }

    private void OnSelectIndexChanged(object sender, EventArgs e)
    {
      if (trace.Switch.Level.HasFlag(SourceLevels.Information))
      {
        trace.TraceInformation("OnSelectIndexChanged(" + tabControl1.Focused + ")" + e);

        foreach (TreeViewPanelBaseClass panel in panelList)
        {
          trace.TraceInformation("OnSelectIndexChanged(" + panel.Name + "," + panel.Focused + "," + panel.CanFocus + "," + panel.CanSelect + "," + panel.ClientSize.Height + "," + panel.DisplayRectangle.Height + ")");
        }
      }

    }

  }
  public class PersonChangeEvent : EventArgs
  {
    public IndividualClass selectedPerson;
    public PersonChangeEvent(IndividualClass selectedPerson)
    {
      this.selectedPerson = selectedPerson;
    }
  }



  public class ReadFileWorker : AsyncWorkerProgressInterface
  {
    private BackgroundWorker backgroundWorker;
    private DateTime startTime;
    private FamilyTreeStoreBaseClass familyTree;
    string workerFileName;
    ProgressReporterInterface progressReporter;
    private TraceSource trace;
    private FamilyFileTypeCollection codec;
    private CompletedCallback completedCallback;

    public ReadFileWorker(
      object sender, 
      ProgressReporterInterface progress, 
      string filename, 
      ref FamilyTreeStoreBaseClass tree,
      CompletedCallback callback)
    {
      trace = new TraceSource("ReadFileWorker", SourceLevels.Warning);

      familyTree = tree;

      progressReporter = progress;

      backgroundWorker = new BackgroundWorker();

      backgroundWorker.WorkerReportsProgress = true;
      backgroundWorker.DoWork += new DoWorkEventHandler(DoWork);
      backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(Completed);
      backgroundWorker.ProgressChanged += new ProgressChangedEventHandler(ProgressChanged);
      completedCallback = callback;

      codec = new FamilyFileTypeCollection();

      if (codec.IsKnownFileType(filename))
      {
        trace.TraceInformation("ReadFileWorker.DoWork(" + filename + "): OK!" + DateTime.Now);

        if (familyTree == null)
        {
          familyTree = codec.CreateFamilyTreeStore(filename, completedCallback);
        }
        codec.SetProgressTarget(backgroundWorker);

        familyTree.SetSourceFileName(filename);
      }
      else
      {
        trace.TraceInformation("ReadFileWorker(" + workerFileName + "): Filetype unknown!" + DateTime.Now);
      }
      workerFileName = filename;
      backgroundWorker.RunWorkerAsync(filename);

    }
    public void DoWork(object sender, DoWorkEventArgs e)
    {

      // This method will run on a thread other than the UI thread.
      // Be sure not to manipulate any Windows Forms controls created
      // on the UI thread from this method.
      startTime = DateTime.Now;

      codec.OpenFile((string)(e.Argument), ref familyTree, this.completedCallback);

      trace.TraceInformation("ReadFileWorker::DoWork(" + workerFileName + ")" + DateTime.Now);
    }

    public FamilyTreeStoreBaseClass GetFamilyTree()
    {
      return familyTree;
    }
    public void ProgressChanged(object sender, ProgressChangedEventArgs e)
    {
      trace.TraceInformation("ReadFileWorker::ProgressChanged(" + e.ProgressPercentage + ")" + DateTime.Now);

      if (familyTree != null)
      {
        progressReporter.ReportProgress(e.ProgressPercentage, familyTree.GetShortTreeInfo());
      }
      else
      {
        progressReporter.ReportProgress(e.ProgressPercentage);
      }
    }
    public void Completed(object sender, RunWorkerCompletedEventArgs e)
    {
      trace.TraceInformation("ReadFileWorker::Completed()" + DateTime.Now);
      trace.TraceInformation("  Start time:" + startTime + " end time: " + DateTime.Now);

      if (familyTree != null)
      {
        progressReporter.Completed(familyTree.GetShortTreeInfo());
      }
      else
      {
        progressReporter.Completed();
      }
    }
    protected virtual void Dispose(bool managed)
    {
      if (managed)
      {
        backgroundWorker.DoWork -= new DoWorkEventHandler(DoWork);
        backgroundWorker.RunWorkerCompleted -= new RunWorkerCompletedEventHandler(Completed);
        backgroundWorker.ProgressChanged -= new ProgressChangedEventHandler(ProgressChanged);
        backgroundWorker.Dispose();
      }
    }
    public void Dispose()
    {
      Dispose(true);
    }
  }


  public class WriteFileWorker : AsyncWorkerProgressInterface
  {
    private BackgroundWorker backgroundWorker;
    private DateTime startTime;
    private FamilyTreeStoreBaseClass familyTree;
    private string workerFileName;
    private ProgressReporterInterface progressReporter;
    private string progressString;
    private TraceSource trace;
    private FamilyFileTypeOperation operation;
    private int filterIndex;

    public WriteFileWorker(
      object sender,
      ProgressReporterInterface progress,
      string filename,
      FamilyFileTypeOperation operation,
      int filterIndex,
      ref FamilyTreeStoreBaseClass tree)
    {
      trace = new TraceSource("WriteFileWorker", SourceLevels.Warning);
      trace.TraceInformation("WriteFileWorker(" + filename + ")" + DateTime.Now);
      progressString = "Exporting...";

      familyTree = tree;
      this.operation = operation;
      this.filterIndex = filterIndex;

      progressReporter = progress;

      backgroundWorker = new BackgroundWorker();

      backgroundWorker.WorkerReportsProgress = true;
      backgroundWorker.DoWork += new DoWorkEventHandler(DoWork);
      backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(Completed);
      backgroundWorker.ProgressChanged += new ProgressChangedEventHandler(ProgressChanged);

      backgroundWorker.RunWorkerAsync(filename);

    }

    public void SetProgressString(string str)
    {
      progressString = str;
    }

    public void DoWork(object sender, DoWorkEventArgs e)
    {

      // This method will run on a thread other than the UI thread.
      // Be sure not to manipulate any Windows Forms controls created
      // on the UI thread from this method.
      startTime = DateTime.Now;
      workerFileName = (String)e.Argument;

      trace.TraceInformation("WriteFileWorker::DoWork(" + workerFileName + ")" + DateTime.Now);
      FamilyFileEncoderCollection encoder = new FamilyFileEncoderCollection();

      encoder.SetProgressTarget(progressReporter);

      encoder.StoreFile(familyTree, workerFileName, operation, filterIndex);

    }

    public void ProgressChanged(object sender, ProgressChangedEventArgs e)
    {
      trace.TraceInformation("WriteFileWorker::ProgressChanged(" + e.ProgressPercentage + ")" + DateTime.Now);

      if (familyTree != null)
      {
        progressReporter.ReportProgress(e.ProgressPercentage, progressString);
      }
      else
      {
        progressReporter.ReportProgress(e.ProgressPercentage);
      }
    }
    public void Completed(object sender, RunWorkerCompletedEventArgs e)
    {
      trace.TraceInformation("WriteFileWorker::Completed()" + DateTime.Now);
      trace.TraceInformation("  Start time:" + startTime + " end time: " + DateTime.Now);

      if (familyTree != null)
      {
        progressReporter.Completed(familyTree.GetShortTreeInfo());
      }
      else
      {
        progressReporter.Completed();
      }
    }
    protected virtual void Dispose(bool managed)
    {
      if (managed)
      {
        backgroundWorker.DoWork -= new DoWorkEventHandler(DoWork);
        backgroundWorker.RunWorkerCompleted -= new RunWorkerCompletedEventHandler(Completed);
        backgroundWorker.ProgressChanged -= new ProgressChangedEventHandler(ProgressChanged);
        backgroundWorker.Dispose();
      }
    }
    public void Dispose()
    {
      Dispose(true);
    }
  }

}

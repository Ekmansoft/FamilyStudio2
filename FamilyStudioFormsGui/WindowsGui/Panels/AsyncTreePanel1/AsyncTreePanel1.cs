using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;      // Stream
using System.Drawing; // Image
using System.Threading;
using System.Windows.Forms;
using FamilyStudioFormsGui.WindowsGui.Controls;
using FamilyStudioFormsGui.WindowsGui.Forms;
using FamilyStudioData.FamilyData;
using FamilyStudioData.FamilyTreeStore;
using System.Collections;

namespace FamilyStudioFormsGui.WindowsGui.Panels.AsyncTreePanel1
{
  class AsyncTreePanel1 : TreeViewPanelBaseClass
  {
    private static TraceSource trace = new TraceSource("AsyncTreePanel1", SourceLevels.Warning);
    private FamilyTreeStoreBaseClass familyTree;
    private IndividualClass selectedIndividual;
    private string prevSelectedIndividual;
    private IDictionary<string, IndividualButton> personControlList;
    private IDictionary<string, FamilyButton> familyControlList;
    private IDictionary<string, FindPersonThread> personThreadList;
    private IDictionary<string, FindFamilyThread> familyThreadList;
    private FamilyForm2 parentForm;
    private TreeViewLayout layout;
    private TreeViewLayout prevLayout;
    private float scaleFactorVertical, scaleFactorHorizontal;
    private Panel mainPanel;
    private ComboBox generationNoCtrl;
    private Button generationNoChange;
    private System.Windows.Forms.Timer updateDelay;
    public delegate void HandleNewIndividual(string xref, IndividualClass person);
    public delegate void HandleNewFamily(string xref, FamilyClass family);
    private Font font;
    private HandleNewIndividual handleNewIndividual;
    private HandleNewFamily handleNewFamily;


    public override void PasteFromClipboard(object clipboard)
    {
      trace.TraceEvent(TraceEventType.Information, 0, "PasteFromClipboard:" + clipboard);
    }

    private void UpdateIndividualSync(string xref, IndividualClass person)
    {
      if(xref != person.GetXrefName())
      {
        trace.TraceData(TraceEventType.Error, 0, "Requested person doesn't match response!" + xref + "!=" + person.GetXrefName());
      }
      if (personThreadList.ContainsKey(xref))
      {
        personThreadList.Remove(xref);
      }
      else
      {
        trace.TraceData(TraceEventType.Error, 0, "Individual thread not added!" + xref);
      }
      if (personControlList.ContainsKey(xref))
      {
        if (person != null)
        {
          IndividualButton personButton = personControlList[xref];
          UpdateIndividual(person, layout, ref personButton);
        }
      }
      else
      {
        trace.TraceData(TraceEventType.Error, 0, "Individual button not added!" + xref);
      }
    }
    public void UpdateFamilySync(string xref, FamilyClass family)
    {
      if (xref != family.GetXrefName())
      {
        trace.TraceData(TraceEventType.Error, 0, "Requested family doesn't match response!" + xref + "!=" + family.GetXrefName());
      }
      if (familyThreadList.ContainsKey(xref))
      {
        familyThreadList.Remove(xref);
      }
      else
      {
        trace.TraceData(TraceEventType.Error, 0, "Family thread not added!" + xref);
      }
      if (familyControlList.ContainsKey(xref))
      {
        FamilyButton familyButton = familyControlList[xref];
        if (family != null)
        {
          UpdateFamily(family, layout, ref familyButton, null);
        }
      }
      else
      {
        trace.TraceData(TraceEventType.Error, 0, "Family button not added!" + xref);
      }
    }
    public void HandleNewIndividual_Function(string xref, IndividualClass person)
    {
      if(InvokeRequired)
      {
        Invoke(new Action(() => UpdateIndividualSync(xref, person)));
      }
      else
      {
        UpdateIndividualSync(xref, person);
      }
    }

    public void HandleNewFamily_Function(string xref, FamilyClass family)
    {
      if (InvokeRequired)
      {
        Invoke(new Action(() => UpdateFamilySync(xref, family)));
      }
      else
      {
        UpdateFamilySync(xref, family);
      }
    }

    class FindPersonThread : AsyncWorkerThreadInterface
    {
      static TraceSource trace = new TraceSource("FindPersonThread", SourceLevels.Warning);
      private BackgroundWorker backgroundWorker;
      private FamilyTreeStoreBaseClass familyTree;
      private HandleNewIndividual personCallback;
      private string personXref;


      public FindPersonThread(FamilyTreeStoreBaseClass familyTree, string personXref, HandleNewIndividual individualCallback)
      {
        this.familyTree = familyTree;
        backgroundWorker = new BackgroundWorker();

        this.personCallback = individualCallback;
        backgroundWorker.DoWork += new DoWorkEventHandler(DoWork);
        //backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(Completed);
        backgroundWorker.RunWorkerAsync(personXref);
        this.personXref = personXref;
        trace.TraceInformation("FindPersonThread(" + personXref + ")-started");

      }
      public void DoWork(object sender, DoWorkEventArgs e)
      {
        string tXref = (string)e.Argument;
        if (familyTree != null)
        {
          trace.TraceData(TraceEventType.Information, 0, "Ask for person :" + tXref + " thread:" + Thread.CurrentThread.ManagedThreadId);
          IndividualClass person = familyTree.GetIndividual(tXref);

          if (person != null)
          {
            trace.TraceInformation("FindPersonThread(" + personXref + ")-done-ok");
            if (person.GetXrefName() != tXref)
            {
              trace.TraceData(TraceEventType.Error, 0, "Wrong person found:" + person.GetXrefName() + "!=" + tXref + " thread:" + Thread.CurrentThread.ManagedThreadId);
              return;
            }
            personCallback(tXref, person);
            this.Dispose();
            return;
          }
        }
        trace.TraceInformation("FindPersonThread(" + personXref + ")-failed");
        personCallback(tXref, null);
        this.Dispose();
      }

      public void Dispose()
      {
        backgroundWorker.DoWork -= new DoWorkEventHandler(DoWork);
        //backgroundWorker.RunWorkerCompleted -= new RunWorkerCompletedEventHandler(Completed);
        backgroundWorker.Dispose();
      }
    
    }
    class FindFamilyThread : AsyncWorkerThreadInterface
    {
      static TraceSource trace = new TraceSource("FindFamilyThread", SourceLevels.Warning);
      private BackgroundWorker backgroundWorker;
      private FamilyTreeStoreBaseClass familyTree;
      private HandleNewFamily familyCallback;
      private string familyXref;


      public FindFamilyThread(FamilyTreeStoreBaseClass familyTree, string familyXref, HandleNewFamily familyCallback)
      {
        this.familyTree = familyTree;
        backgroundWorker = new BackgroundWorker();

        this.familyCallback = familyCallback;
        backgroundWorker.DoWork += new DoWorkEventHandler(DoWork);
        backgroundWorker.RunWorkerAsync(familyXref);
        this.familyXref = familyXref;
        trace.TraceInformation("FindFamilyThread(" + familyXref + ")-start");
      }
      public void DoWork(object sender, DoWorkEventArgs e)
      {
        string tXref = (string)e.Argument;
        if (familyTree != null)
        {
          FamilyClass family = familyTree.GetFamily(tXref);

          if (family != null)
          {
            trace.TraceInformation("FindFamilyThread(" + familyXref + ")-done ok");
            familyCallback(tXref, family);
            this.Dispose();
            return;
          }
        }
        trace.TraceInformation("FindFamilyThread(" + familyXref + ")-failed");
        familyCallback(tXref, null);
        this.Dispose();
      }

      public void Dispose()
      {
        backgroundWorker.DoWork -= new DoWorkEventHandler(DoWork);
        //backgroundWorker.RunWorkerCompleted -= new RunWorkerCompletedEventHandler(Completed);
        backgroundWorker.Dispose();
      }
    
    }

    public enum RelativeType
    {
      Unrelated,
      Parent,
      Child
    };

    private void SetControlSize(Control control, int size, Font font)
    {
      //Font font = new System.Drawing.Font("Segue UI", 9F);
      if (scaleFactorVertical == 0)
      {
        scaleFactorVertical = (float)font.Height / (float)font.Size;
        scaleFactorHorizontal = 1.75F;
        Debug.WriteLine("scaleFactors=" + scaleFactorHorizontal + "x" + scaleFactorVertical);
      }
      control.Font = font;
      control.Width = (int)(size * scaleFactorVertical);
      control.Height = (int)(font.Height * scaleFactorHorizontal);
    }

    public AsyncTreePanel1()
    {
      scaleFactorVertical = (float)(0.0);
      personControlList = new Dictionary<string, IndividualButton>();
      familyControlList = new Dictionary<string, FamilyButton>();
      personThreadList = new Dictionary<string, FindPersonThread>();
      familyThreadList = new Dictionary<string, FindFamilyThread>();
    
      generationNoCtrl = new ComboBox();

      layout = new TreeViewLayout();
      for (int i = 9; i >= 2; i--)
      {
        generationNoCtrl.Items.Add(i.ToString());
      }
      //generationNoCtrl.Font = new System.Drawing.Font("Segue UI", 9F);
      //scaleFactorVertical = (float)generationNoCtrl.Font.Height / (float)generationNoCtrl.Font.Size;
      generationNoCtrl.SelectedItem = layout.GetGenerations().ToString();
      //generationNoCtrl.Width = (int)(40 * scaleFactorVertical);
      //generationNoCtrl.Height = (int)(generationNoCtrl.Font.Height * 1.5);

      font = new System.Drawing.Font("Segue UI", 9F);

      SetControlSize(generationNoCtrl, 25, font);

      //generationNoCtrl.SelectedItemChanged += GenerationNoCtrl_SelectedItemChanged;

      this.Controls.Add(generationNoCtrl);

      generationNoChange = new Button();
      generationNoChange.Text = "Go";
      generationNoChange.Left = generationNoCtrl.Right;
      //generationNoChange.Font = new System.Drawing.Font("Segue UI", 9F);
      //Debug.WriteLine("fontsize " + generationNoChange.Font.Height + " sz " + generationNoChange.Font.Size + " sz-pt " + generationNoChange.Font.SizeInPoints); 
      //generationNoChange.Width = (int)(30 * scaleFactorVertical);
      generationNoChange.MouseClick += generationNoChange_MouseClick;
      //generationNoChange.Height = (int)(generationNoChange.Font.Height * 1.5);
      SetControlSize(generationNoChange, 20, font);
      this.Controls.Add(generationNoChange);

      /*Button check = new Button();
      check.Text = "Check";
      check.Left = generationNoChange.Right;
      //check.Width = (int)(50 * scaleFactorVertical);
      check.MouseClick += check_MouseClick;
      //check.Font = new System.Drawing.Font("Segue UI", 9F);
      //check.Height = (int)(check.Font.Height * 1.5);
      SetControlSize(check, 40, font);
      this.Controls.Add(check);
      */
      /*Button force = new Button();
      force.Text = "Force";
      force.Left = check.Right;
      //force.Width = (int)(50 * scaleFactorVertical);
      force.MouseClick += force_MouseClick;
      //force.Font = new System.Drawing.Font("Segue UI", 9F);
      //force.Height = (int)(force.Font.Height * 1.5);
      SetControlSize(force, 40, font);
      this.Controls.Add(force);
      */
      mainPanel = new Panel();
      //mainPanel.VerticalScroll.Visible = true;
      //mainPanel.HorizontalScroll.Visible = true;
      mainPanel.AutoScroll = true;
      mainPanel.Dock = DockStyle.Fill;
      mainPanel.Top = generationNoCtrl.Bottom;
      //SetStyle(ControlStyles.FixedHeight | ControlStyles.FixedWidth, true);
      //ScrollBars = 
      //AutoScroll = true;
      mainPanel.Font = font;// new System.Drawing.Font("Segue UI", 9F);
      this.Controls.Add(mainPanel);

      parentForm = null;
      
      trace.TraceInformation("AsyncTreePanel1::AsyncTreePanel1()");



      handleNewIndividual = new HandleNewIndividual(HandleNewIndividual_Function);
      handleNewFamily = new HandleNewFamily(HandleNewFamily_Function);

      //this.VScroll = true;
      //this.HScroll = true;
      //SetStyle(ControlStyles.FixedHeight | ControlStyles.FixedWidth, true);
      //ScrollBars = 
      //AutoScroll = true;
      this.Dock = DockStyle.Fill;

      updateDelay = null;

      this.VisibleChanged += TreeViewPanel_VisibleChanged;

    }

    /*void check_MouseClick(object sender, MouseEventArgs e)
    {
      if (selectedIndividual != null)
      {
        CheckLayout(selectedIndividual.GetXrefName(), false);
      }
    }*/
    /*void force_MouseClick(object sender, MouseEventArgs e)
    {
      if (selectedIndividual != null)
      {
        CheckLayout(selectedIndividual.GetXrefName(), true);
      }
    }*/
    void generationNoChange_MouseClick(object sender, MouseEventArgs e)
    {
      StartRefreshTimer();
    }
    void GenerationNoCtrl_SelectedItemChanged(object sender, EventArgs e)
    {
      //layout.SetGenerations(Convert.ToInt32(generationNoCtrl.SelectedItem));
      //ShowActiveFamily();
      StartRefreshTimer();
    }

    private void StartRefreshTimer(bool force = false)
    {
      if(force)
      {
        if (updateDelay != null)
        {
          updateDelay.Stop();
          updateDelay = null;
        }
      }
      if (updateDelay == null)
      {
        updateDelay = new System.Windows.Forms.Timer();
        updateDelay.Tick += updateDelay_Tick;
        updateDelay.Interval = 300;
        updateDelay.Start();
      }
      else
      {
        trace.TraceData(TraceEventType.Information, 0, "updateDelay timer already started!");
      }
    }
    void updateDelay_Tick(object sender, EventArgs e)
    {
      if (updateDelay != null)
      {
        updateDelay.Stop();
        updateDelay = null;
      }
      else
      {
        trace.TraceData(TraceEventType.Warning, 0, "updateDelay timer missing!");
      }
      ShowActiveFamily();
    }

    void TreeViewPanel_VisibleChanged(object sender, EventArgs e)
    {
      trace.TraceData(TraceEventType.Information, 0, "TreeViewPanel_VisibleChanged()!" + e.ToString());
      StartRefreshTimer();
      //ShowActiveFamily();
    }
    public override void SetParentForm(FamilyForm2 parentForm)
    {
      this.parentForm = parentForm;

      parentForm.SelectedPersonChanged += OnSelectedPersonChangedEvent;
    }

    public FamilyTreeStoreBaseClass GetFamilyTree()
    {
      return familyTree;
    }



    private void ClearButtonLayout()
    {
      foreach (IndividualButton button in personControlList.Values)
      {
        button.ClearLayout();
      }
      foreach (FamilyButton button in familyControlList.Values)
      {
        button.ClearLayout();
      }
    }

    public override void SetFamilyTree(FamilyTreeStoreBaseClass inFamilyTree)
    {
      trace.TraceInformation("SetFamilyTree()");

      familyTree = inFamilyTree;

    }

    private void UpdateFamilyList(IList<FamilyXrefClass> familyList, IndividualButton personButton)
    {
      if (familyList != null)
      {
        foreach (FamilyXrefClass familyXref in familyList)
        {
          if (!familyControlList.ContainsKey(familyXref.GetXrefName()))
          {
            FamilyButton familyButton = new FamilyButton(familyXref.GetXrefName());
            familyButton.SetParent(this);
            familyButton.SetLayout(layout, personButton);
            familyControlList.Add(familyXref.GetXrefName(), familyButton);
            mainPanel.Controls.Add(familyButton);

            if (!familyThreadList.ContainsKey(familyXref.GetXrefName()))
            {
              FindFamilyThread thread = new FindFamilyThread(familyTree, familyXref.GetXrefName(), new HandleNewFamily(HandleNewFamily_Function));
              familyThreadList.Add(familyXref.GetXrefName(), thread);
            }
          }
          else
          {
            FamilyButton familyButton = familyControlList[familyXref.GetXrefName()];

            if (familyButton.family != null)
            {
              UpdateFamily(familyButton.family, layout, ref familyButton, personButton);
            }
            else
            {
              if (familyButton.SetLayout(layout, personButton))
              {
                if (!familyThreadList.ContainsKey(familyXref.GetXrefName()))
                {
                  FindFamilyThread thread = new FindFamilyThread(familyTree, familyXref.GetXrefName(), new HandleNewFamily(HandleNewFamily_Function));
                  familyThreadList.Add(familyXref.GetXrefName(), thread);
                }
              }
            }
          }
        }
      }
    }

    private void UpdateIndividualList(IList<IndividualXrefClass> personList, FamilyButton familyButton)
    {
      if (personList != null)
      {
        foreach (IndividualXrefClass personXref in personList)
        {
          if (!personControlList.ContainsKey(personXref.GetXrefName()))
          {
            IndividualButton personButton = new IndividualButton(personXref.GetXrefName());
            personButton.SetParent(this);
            personButton.SetLayout(layout, familyButton);
            personControlList.Add(personXref.GetXrefName(), personButton);
            mainPanel.Controls.Add(personButton);
            if (!personThreadList.ContainsKey(personXref.GetXrefName()))
            {
              FindPersonThread thread = new FindPersonThread(familyTree, personXref.GetXrefName(), new HandleNewIndividual(HandleNewIndividual_Function));
              personThreadList.Add(personXref.GetXrefName(), thread);
            }
          }
          else
          {
            IndividualButton personButton = personControlList[personXref.GetXrefName()];

            if (personButton.individual != null)
            {
              UpdateIndividual(personButton.individual, layout, ref personButton, familyButton);
            }
            else
            {
              if (personButton.SetLayout(layout, familyButton))
              {
                if (!personThreadList.ContainsKey(personXref.GetXrefName()))
                {
                  FindPersonThread thread = new FindPersonThread(familyTree, personXref.GetXrefName(), new HandleNewIndividual(HandleNewIndividual_Function));
                  personThreadList.Add(personXref.GetXrefName(), thread);
                }
              }
            }
          }
        }
      }
    }
    private void UpdateIndividual(IndividualClass person, TreeViewLayout layout, ref IndividualButton personButton, FamilyButton parentButton = null)
    {
      trace.TraceInformation("UpdateIndividual(" + person.GetXrefName() + "),gen:" + personButton.bLayout.generation + ",name:" + person.GetPersonalName().GetName());
      if (personButton.individual == null)
      {
        personButton.individual = person;
        personButton.Refresh();
      }
      if (!personButton.Visible)
      {
        if(!personButton.SetLayout(layout, parentButton))
        {
          trace.TraceInformation("UpdateIndividual(" + person.GetXrefName() + "),gen:" + personButton.bLayout.generation + ")-aborted,hidden");
          return;
        }
      }

      if (layout.VisibleButton(personButton.bLayout))
      {
        if (layout.SearchChildren())
        {
          UpdateFamilyList(person.GetFamilySpouseList(), personButton);
        }
        if (layout.SearchParents())
        {
          UpdateFamilyList(person.GetFamilyChildList(), personButton);
        }
        trace.TraceInformation("UpdateIndividual(" + person.GetXrefName() + ", " + personButton.bLayout.generation + ")-done");
      }
      else
      {
        trace.TraceInformation("UpdateIndividual(" + person.GetXrefName() + ", " + personButton.bLayout.generation + ") not visible generation => stop!");
      }
    }

    private void UpdateFamily(FamilyClass family, TreeViewLayout layout, ref FamilyButton familyButton, IndividualButton parentButton = null)
    {
      trace.TraceInformation("UpdateFamily(" + family.GetXrefName() + "):" + familyButton.bLayout.generation);
      if (familyButton.family == null)
      {
        familyButton.family = family;
        familyButton.Refresh();
      }
      if (!familyButton.Visible)
      {
        if(!familyButton.SetLayout(layout, parentButton))
        {
          trace.TraceInformation("UpdateFamily(" + family.GetXrefName() + ")-aborted");
          return;
        }
        //familyButton.Refresh();
      }

      if (layout.VisibleButton(familyButton.bLayout))
      {
        if (layout.SearchChildren())
        {
          UpdateIndividualList(family.GetChildList(), familyButton);
        }
        if (layout.SearchParents())
        {
          UpdateIndividualList(family.GetParentList(), familyButton);
        }
        trace.TraceInformation("UpdateFamily(" + family.GetXrefName() + ")-done");
      }
      else
      {
        trace.TraceInformation("UpdateFamily(" + family.GetXrefName() + "," + familyButton.bLayout.generation + ") not visible generation => stop!");
      }

    }
    public void ClickPerson(IndividualClass person)
    {
      trace.TraceInformation("AsyncTreePanel1::SetSelectedIndividual(" + person + ")");
      if (familyTree != null)
      {
        selectedIndividual = person;

        parentForm.SetSelectedIndividual(person.GetXrefName());
      }
    }

    public override string GetTitle()
    {
      return "AncestorView1";
    }


    public override void OnSelectedPersonChangedEvent(object sender, PersonChangeEvent e)
    {
      if (familyTree != null)
      {
        selectedIndividual = e.selectedPerson;
        trace.TraceInformation("OnSelectedPersonChangedEvent(" + selectedIndividual.GetXrefName() + ")");
        StartRefreshTimer(true);
        //ShowActiveFamily();
      }
      else
      {
        trace.TraceInformation("OnSelectedPersonChangedEvent(null)");
      }

    }

    /*protected override void OnLayout(LayoutEventArgs e)
    {
      trace.TraceInformation("OnLayout()");
      StartRefreshTimer();
      base.OnLayout(e);
    }*/

    private void CheckFamilyButton(string xref, int gen, bool force)
    {
      if (familyControlList.ContainsKey(xref))
      {
        FamilyButton familyButton = familyControlList[xref];

        familyButton.CheckPosition(force);

        if (familyButton.family != null)
        {
          FamilyClass family = familyButton.family;

          IList<IndividualXrefClass> parentList = family.GetParentList();

          if (parentList != null)
          {
            foreach (IndividualXrefClass parentXref in parentList)
            {
              CheckPersonButton(parentXref.GetXrefName(), gen + 1, force);
            }
          }

        }
        else
        {
          trace.TraceData(TraceEventType.Warning, 0, "Not yet cached family:" + gen + " " + xref);
        }
      }
      else
      {
        trace.TraceData(TraceEventType.Warning, 0, "Not buttoned family:" + gen + " " + xref);
      }
    }
    private void CheckPersonButton(string xref, int gen, bool force)
    {
      if (personControlList.ContainsKey(xref))
      {
        IndividualButton personButton = personControlList[xref];

        personButton.CheckPosition(force);

        if (personButton.individual != null)
        {
          IndividualClass person = personButton.individual;

          IList<FamilyXrefClass> parentList = person.GetFamilyChildList();

          if (parentList != null)
          {
            foreach (FamilyXrefClass parentXref in parentList)
            {
              CheckFamilyButton(parentXref.GetXrefName(), gen + 1, force);
            }
          }
        }
        else
        {
          trace.TraceData(TraceEventType.Warning, 0, "Not yet cached person:" + gen + " " + xref);
        }
      }
      else
      {
        trace.TraceData(TraceEventType.Warning, 0, "Not buttoned person:" + gen + " " + xref);
      }
    }
    private void CheckLayout(string xref, bool force)
    {
      CheckPersonButton(xref, 0, force);
    }

    private void ShowActiveFamily()
    {
      bool clear = false;
      trace.TraceInformation("ShowActiveFamily (start)");

      if (!this.CanFocus)
      {
        trace.TraceInformation("ShowActiveFamily (end:hidden->shown=>refresh)");
        return;
      }

      if (generationNoCtrl.SelectedItem == null)
      {
        trace.TraceInformation("ShowActiveFamily (selecteditem == null)");
        return;
      }
      if (generationNoCtrl.SelectedItem.ToString() != layout.GetGenerations().ToString())
      {
        layout.SetGenerations(Convert.ToInt32(generationNoCtrl.SelectedItem));
      }

      if (!layout.IsEqual(prevLayout))
      {
        //lastSize.Height = this.Height;
        //lastSize.Width = this.Width;
        //layout.Init(lastSize);
        //ClearButtonLayout();
        clear = true;
        prevLayout = layout.Clone();
      }

      if (selectedIndividual != null)
      {
        if ((prevSelectedIndividual == null) || (prevSelectedIndividual != selectedIndividual.GetXrefName()))
        {
          //ClearButtonLayout();
          clear = true;
          //prevLayout = layout;
          prevSelectedIndividual = selectedIndividual.GetXrefName();
        }

        if(clear)
        {
          ClearButtonLayout();
          trace.TraceData(TraceEventType.Warning, 0, " scrollv:" + mainPanel.VerticalScroll.Value + " " + mainPanel.VerticalScroll.Maximum + " " + mainPanel.VerticalScroll.Minimum + " scrollh:" + mainPanel.HorizontalScroll.Value + " " + mainPanel.HorizontalScroll.Maximum + " " + mainPanel.HorizontalScroll.Minimum);
          mainPanel.Width += 100;
          mainPanel.Height += 100;
          mainPanel.VerticalScroll.Value = mainPanel.VerticalScroll.Maximum;
          mainPanel.HorizontalScroll.Value = mainPanel.HorizontalScroll.Maximum;
          trace.TraceData(TraceEventType.Warning, 0, " scrollv:" + mainPanel.VerticalScroll.Value + " " + mainPanel.VerticalScroll.Maximum + " " + mainPanel.VerticalScroll.Minimum + " scrollh:" + mainPanel.HorizontalScroll.Value + " " + mainPanel.HorizontalScroll.Maximum + " " + mainPanel.HorizontalScroll.Minimum);
        }
        if (!personControlList.ContainsKey(selectedIndividual.GetXrefName()))
        {
          IndividualButton selectedButton = new IndividualButton(selectedIndividual.GetXrefName(), selectedIndividual);
          selectedButton.SetParent(this);

          personControlList.Add(selectedIndividual.GetXrefName(), selectedButton);
          mainPanel.Controls.Add(selectedButton);
        }
        IndividualButton personControl = personControlList[selectedIndividual.GetXrefName()];
        UpdateIndividual(selectedIndividual, layout, ref personControl);
        CheckLayout(selectedIndividual.GetXrefName(), false);
      }
      else
      {
        if (prevSelectedIndividual != null)
        {
          ClearButtonLayout();
          prevSelectedIndividual = null;
        }
      }
      //Refresh();

      trace.TraceInformation("ShowActiveFamily (end-done) ");
    }
  }
}

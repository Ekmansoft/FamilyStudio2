using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using FamilyStudioFormsGui.WindowsGui.Controls;
using FamilyStudioFormsGui.WindowsGui.Forms;
using FamilyStudioData.FamilyData;
using FamilyStudioData.FamilyTreeStore;
using System.Collections;

namespace FamilyStudioFormsGui.WindowsGui.Panels.TreeViewPanel5
{

  class IndividualButton : Button
  {
    public IndividualClass individual;
    private TreeViewPanel5 parent;
    private ToolTip details;
    private static TraceSource trace = new TraceSource("IndividualButton5", SourceLevels.Warning);

    public IndividualButton(TreeViewPanel5 parent, IndividualClass individual, Point size, Font font)
    {
      this.parent = parent;
      this.individual = individual;

      this.Text = individual.GetName();

      this.Font = font;
      if (size.Y > 15)
      {
        this.Text += "\n" + individual.GetDate(IndividualEventClass.EventType.Birth).ToString() + " - " + individual.GetDate(IndividualEventClass.EventType.Death).ToString();
      }
      TextAlign = ContentAlignment.MiddleLeft;
      AutoEllipsis = true;
      details = new ToolTip();
      //details.IsBalloon = true;
      FlatStyle = FlatStyle.Flat;
      //AutoSize = true;
      Anchor = AnchorStyles.Left | AnchorStyles.Top;
      Click += new System.EventHandler(Clicked);
      //BackColor = Color.Beige;
      //Margin = new Padding(-3,-3,-3,-3);

      //details.AutomaticDelay = 10000;
      details.AutoPopDelay = 600000;
      //string toolTip = ;
      details.SetToolTip(this, CreateToolString());
      //details.ToolTipTitle = individual.GetName();
      //details.ToolTipIcon = ToolTipIcon.Info;

      AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
      AutoSize = false;
      Height = size.Y;
      Width = size.X;

    }

    private string CreateToolString()
    {
      IndividualEventClass ev;
      FamilyDateTimeClass date;
      AddressClass address;
      string str = "";

      str = individual.GetName() + "\n";

      ev = individual.GetEvent(IndividualEventClass.EventType.Birth);

      if (ev != null)
      {
        str += "Born ";

        date = ev.GetDate();

        if (date.GetDateType() != FamilyDateTimeClass.FamilyDateType.Unknown)
        {
          str += date.ToString();
        }
        address = ev.GetAddress();
        if (address != null)
        {
          str += " in " + address.ToString();
        }
        else
        {
          PlaceStructureClass place = ev.GetPlace();

          if (place != null)
          {
            str += " in " + place.ToString();
          }
        }
      }
      str += "\n";// Environment.NewLine;
      ev = individual.GetEvent(IndividualEventClass.EventType.Death);

      if (ev != null)
      {
        str += "Died ";

        date = ev.GetDate();

        if (date.GetDateType() != FamilyDateTimeClass.FamilyDateType.Unknown)
        {
          str += date.ToString();
        }
        address = ev.GetAddress();
        if (address != null)
        {
          str += " in " + address.ToString();
        }
        else
        {
          PlaceStructureClass place = ev.GetPlace();

          if (place != null)
          {
            str += " in " + place.ToString();
          }
        }
      }
      {
        IList<NoteClass> noteList = individual.GetNoteList();

        if (noteList != null)
        {
          foreach (NoteClass note in noteList)
          {
            if (note.note != null)
            {
              str += "\n";//Environment.NewLine;
              str += note.note.Replace("\r\n", "\n").Replace("\n\n", "\n");
              //trace.TraceInformation("ShowNote:" + note.note);
            }
          }
        }
      }

      //str = str.Replace("\r\n", "\n");
      //str = str.Replace("\n\r", "\n");
      return str;
    }

    public void SetParent(TreeViewPanel5 inParent)
    {
      parent = inParent;
    }

    public void Clicked(object sender, EventArgs e)
    {
      trace.TraceInformation("person clicked:");

      if (individual != null)
      {
        if (trace.Switch.Level.HasFlag(SourceLevels.Information))
        {
          individual.Print();
        }
        if (parent != null)
        {
          parent.ClickPerson(individual);
        }
      }
      else
      {
        trace.TraceInformation("error: Clicked person = null!");
      }

    }

  }
  class TreeViewPanel5 : TreeViewPanelBaseClass
  {
    private static TraceSource trace = new TraceSource("TreeViewPanel5", SourceLevels.Warning);
    private FamilyTreeStoreBaseClass familyTree;
    private IndividualClass selectedIndividual;
    //private FamilyClass selectedFamily;
    private IList<IndividualButton> controlList;
    private FamilyForm2 parentForm;
    private int maxGenerations;
    private int controlHeight;
    private int controlWidth;
    //private int controlWidthSmall;
    private int controlMargin;
    private DomainUpDown generationNoCtrl;
    private Font boxFont;
    //private ScrollableControl vScroll;
    //private ScrollableControl hScroll;
    //private ScrollBar vScrollBar;
    //private ScrollBar hScrollBar;


    public TreeViewPanel5()
    {
      controlList = new List<IndividualButton>();

      this.Dock = DockStyle.Fill;
      parentForm = null;

      maxGenerations = 3;
      UpdateControlSizes();

      generationNoCtrl = new DomainUpDown();

      //generationNoCtrl.Left = 0;

      for (int i = 9; i >= 2; i--)
      {
        generationNoCtrl.Items.Add(i.ToString());
      }
      generationNoCtrl.SelectedItem = "3";
      generationNoCtrl.Width = 40;

      this.Controls.Add(generationNoCtrl);

      generationNoCtrl.SelectedItemChanged += generationNoCtrl_SelectedItemChanged;

      /*vScrollBar = new VScrollBar();
      vScrollBar.Dock = DockStyle.Right;
      hScrollBar = new HScrollBar();
      hScrollBar.Dock = DockStyle.Bottom;*/

      //vScroll = new ScrollableControl();
      //hScroll = new ScrollableControl();
      //this.SetBounds
      this.VScroll = true;
      this.HScroll = true;
      //SetStyle(ControlStyles.FixedHeight | ControlStyles.FixedWidth, true);
      //ScrollBars = 
      AutoScroll = true;

      this.VisibleChanged += TreeViewPanel5_VisibleChanged;

      trace.TraceInformation("TreeViewPanel5::TreeViewPanel5()");

    }
    public override void PasteFromClipboard(object clipboard)
    {
      trace.TraceEvent(TraceEventType.Information, 0, "PasteFromClipboard:" + clipboard);
    }

    void TreeViewPanel5_VisibleChanged(object sender, EventArgs e)
    {
      ShowActiveFamily();
    }
    void UpdateControlSizes()
    {
      if (maxGenerations < 5)
      {
        controlHeight = 40;
        controlWidth = 150;
        //controlWidthSmall = 25;
        boxFont = new Font(FontFamily.GenericSansSerif, 8);
        controlMargin = 5;
      }
      else
      {
        controlHeight = 20;
        controlWidth = 100;
        //controlWidthSmall = 25;
        boxFont = new Font(FontFamily.GenericSansSerif, 6);
        controlMargin = 2;
      }
    }

    void generationNoCtrl_SelectedItemChanged(object sender, EventArgs e)
    {
      ShowActiveFamily();      
    }
    public override void SetParentForm(FamilyForm2 parentForm)
    {
      this.parentForm = parentForm;

      parentForm.SelectedPersonChanged += OnSelectedPersonChangedEvent;

    }



    public override void SetFamilyTree(FamilyTreeStoreBaseClass inFamilyTree)
    {
      trace.TraceInformation("TreeViewPanel5::SetFamilyTree()");

      familyTree = inFamilyTree;

    }
    public void ClickPerson(IndividualClass person)
    {
      trace.TraceInformation("TreeViewPanel5::SetSelectedIndividual(" + person + ")");
      if (familyTree != null)
      {
        selectedIndividual = person;

        ShowActiveFamily();
      }
    }

    public override string GetTitle()
    {
      return "AncestorView5";
    }


    public override void OnSelectedPersonChangedEvent(object sender, PersonChangeEvent e)
    {
      if (familyTree != null)
      {
        selectedIndividual = e.selectedPerson;
        ShowActiveFamily();
      }

    }

    private void AddPerson(IndividualClass person, Point position, int generation)
    {
      Point size = new Point(controlWidth, controlHeight);
      IndividualButton individual = new IndividualButton(this, person, size, boxFont);

      individual.Top = position.Y;
      individual.Left = position.X;


      this.controlList.Add(individual);
      this.Controls.Add(individual);

      /*if (individual.Width > controlWidth)
      {
        individual.Width = controlWidth;
      }*/

      if (generation < maxGenerations)
      {

        IList<FamilyXrefClass> parentFamilies = individual.individual.GetFamilyChildList();

        if (parentFamilies != null)
        {
          foreach (FamilyXrefClass familyXref in parentFamilies)
          {
            FamilyClass family = familyTree.GetFamily(familyXref.GetXrefName());

            if (family != null)
            {
              IList<IndividualXrefClass> parentXrefList = family.GetParentList();

              if (parentXrefList != null)
              {
                Point nextPosition = position;
                int margin = controlMargin;
                int controlOffset = (controlHeight + margin) * (int) Math.Pow((double)2, (double)(maxGenerations - generation - 1));

                //if(generation == 0)
                {
                  //margin += controlHeight / 2;
                }

                if (controlOffset > controlHeight)
                {
                  nextPosition.X += controlWidth + margin;
                }
                /*else
                {
                  nextPosition.X += controlWidthSmall + margin;
                }*/
                nextPosition.Y -= controlOffset / 2; 

                foreach (IndividualXrefClass parentXref in parentXrefList)
                {
                  IndividualClass parent = familyTree.GetIndividual(parentXref.GetXrefName());

                  if (parent != null)
                  {
                    AddPerson(parent, nextPosition, generation + 1);
                  }
                  nextPosition.Y += controlOffset;
                }

              }
            }
          }
        }
      }
    }


    private void ShowActiveFamily()
    {
      trace.TraceInformation("TreeViewPanel5::ShowActiveFamily (start) " + this.CanFocus);

      while(this.controlList.Count > 0)
      {
        IndividualButton button = controlList[0];
        this.Controls.Remove(button);
        this.controlList.RemoveAt(0);
      }

      //SetBounds(0, 0, maxGenerations * controlWidth, (int)Math.Pow((double)2, (double)(maxGenerations)), BoundsSpecified.All);
      maxGenerations = Convert.ToInt32(generationNoCtrl.SelectedItem);

      //SetScrollState();

      if ((familyTree == null) || !this.Visible)
      {
        return;
      }


      UpdateControlSizes();

      if (selectedIndividual != null)
      {
        Point pos = new Point(0, (controlHeight + controlMargin) * (int)Math.Pow((double)2, (double)(maxGenerations)) / 2);
        AddPerson(selectedIndividual, pos, 0);
        //IndividualButton individual = new IndividualButton(selectedIndividual);

        //this.controlList.Add(individual);
        //this.Controls.Add(individual);
      }


      trace.TraceInformation("TreeViewPanel5::ShowActiveFamily (end) ");
    }

    /*public void SetParentForm(FamilyForm2 parentForm)
    {
      this.GetTopLevel.parentForm = parentForm;
    }*/


  }
}

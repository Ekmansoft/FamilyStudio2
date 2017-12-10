using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FamilyStudioFormsGui.WindowsGui.Controls;
using FamilyStudioFormsGui.WindowsGui.Forms;
using FamilyStudioData.FamilyData;
using FamilyStudioData.FamilyTreeStore;
using System.Collections;

namespace FamilyStudioFormsGui.WindowsGui.Panels.TreeViewPanel1
{
  class TreeViewPanel1 : TreeViewPanelBaseClass
  {
    private static TraceSource trace = new TraceSource("TreeViewPanel", SourceLevels.Warning);
    private FamilyTreeStoreBaseClass familyTree;
    private IndividualClass selectedIndividual;
    private FamilyLayout layout;
    bool layoutDone;
    private FamilyForm2 parentForm;

    public TreeViewPanel1()
    {
      layout = new FamilyLayout();
      layoutDone = false;

      this.Height = 0;
      this.Width = 0;
      layoutDone = false;
      parentForm = null;
    }

    public override void PasteFromClipboard(object clipboard)
    {
      trace.TraceEvent(TraceEventType.Information, 0, "PasteFromClipboard:" + clipboard);
    }

    public override void SetFamilyTree(FamilyTreeStoreBaseClass inFamilyTree)
    {
      trace.TraceInformation("TreeViewPanel1::SetFamilyTree()");

      familyTree = inFamilyTree;

      layoutDone = false;
    }
    /*public void SetSelectedIndividual(String xrefName)
    {
      trace.TraceInformation("TreeViewPanel1::SetSelectedIndividual(" + xrefName + ")");
      if (familyTree != null)
      {
        selectedIndividual = (IndividualClass)familyTree.GetIndividual(xrefName);
        //ShowActiveFamily();
      }

      layoutDone = false;

    }*/

    public override void SetParentForm(FamilyForm2 parentForm)
    {
      this.parentForm = parentForm;

      parentForm.SelectedPersonChanged += OnSelectedPersonChangedEvent;

    }

    public override string GetTitle()
    {
      return "FamilyView1";
    }

    public override void OnSelectedPersonChangedEvent(object sender, PersonChangeEvent e)
    {
      if (familyTree != null)
      {
        selectedIndividual = e.selectedPerson;
        ShowActiveFamily();
      }

    }



    private void ShowActiveFamily()
    {
      trace.TraceInformation("TreeViewPanel1::ShowActiveFamily(start) " + this.CanFocus);
      trace.TraceInformation("  Controls.count = " + this.Controls.Count);
      //this.Hide();
      foreach (Control ctrl in this.Controls)
      {
        trace.TraceInformation("  ctrl Top:" + ctrl.Top + ", Left:" + ctrl.Left + ", Height:" + ctrl.Height + ", Width:" + ctrl.Width + ", Visible:" + ctrl.Visible + ", Text:" + ctrl.Text);
        if (ctrl.GetType() == typeof(IndividualControl3))
        {
          IndividualControl3 ctrl3 = (IndividualControl3)ctrl;

          ctrl3.Hide();
          trace.TraceInformation("  remove ctrl Top:" + ctrl.Top + ", Left:" + ctrl.Left + ", Height:" + ctrl.Height + ", Width:" + ctrl.Width + ", Visible:" + ctrl.Visible + ", Text:" + ctrl.Text);
          ctrl3.SetIndividual(null);

          this.Controls.Remove(ctrl3);
          ctrl3.Dispose();
        }
        else
        {
          trace.TraceInformation("  dont remove ctrl Top:" + ctrl.Top + ", Left:" + ctrl.Left + ", Height:" + ctrl.Height + ", Width:" + ctrl.Width + ", Visible:" + ctrl.Visible + ", ToStr:" + ctrl.ToString() + ", Text:" + ctrl.Text + ", GetType:" + ctrl.GetType());
        }
      }
      trace.TraceInformation("  Controls.count step 2 = " + this.Controls.Count);

      this.Left = 0;
      this.Top = 0;
      this.Width = 0;
      this.Height = 0;
      /*foreach (Control ctrl in this.Controls)
      {
        if (ctrl.GetType() == typeof(IndividualControl3))
        {
          IndividualControl3 ctrl3 = (IndividualControl3)ctrl;

          trace.TraceInformation("  remove-2 ctrl Top:" + ctrl.Top + ", Left:" + ctrl.Left + ", Height:" + ctrl.Height + ", Width:" + ctrl.Width + ", Visible:" + ctrl.Visible + ", ToStr:" + ctrl.ToString() + ", Text:" + ctrl.Text + ", GetType:" + ctrl.GetType());
          ctrl3.SetIndividual(null);

          this.Controls.Remove(ctrl3);
          ctrl3.Dispose();
        }
        else
        {
          //trace.TraceInformation("  dont remove ctrl Top:" + ctrl.Top + ", Left:" + ctrl.Left + ", Height:" + ctrl.Height + ", Width:" + ctrl.Width + ", Visible:" + ctrl.Visible + ", ToStr:" + ctrl.ToString() + ", Text:" + ctrl.Text + ", GetType:" + ctrl.GetType());
        }
      }
      trace.TraceInformation("Controls.count.3 = " + this.Controls.Count);
      foreach (Control ctrl in this.Controls)
      {
        if (ctrl.GetType() == typeof(IndividualControl3))
        {
          IndividualControl3 ctrl3 = (IndividualControl3)ctrl;

          trace.TraceInformation("  remove-3 ctrl Top:" + ctrl.Top + ", Left:" + ctrl.Left + ", Height:" + ctrl.Height + ", Width:" + ctrl.Width + ", Visible:" + ctrl.Visible + ", ToStr:" + ctrl.ToString() + ", Text:" + ctrl.Text + ", GetType:" + ctrl.GetType());
          ctrl3.SetIndividual(null);

          this.Controls.Remove(ctrl3);
          ctrl3.Dispose();
        }
        else
        {
          //trace.TraceInformation("  dont remove ctrl Top:" + ctrl.Top + ", Left:" + ctrl.Left + ", Height:" + ctrl.Height + ", Width:" + ctrl.Width + ", Visible:" + ctrl.Visible + ", ToStr:" + ctrl.ToString() + ", Text:" + ctrl.Text + ", GetType:" + ctrl.GetType());
        }
      }*/
      layout.Reset();
      //trace.TraceInformation("  Controls.count step 3 = " + this.Controls.Count);

      /*trace.TraceInformation("Number of undisposed controls... " + this.Controls.Count);
      {
        foreach (Control ctrl in this.Controls)
        {
          if (ctrl.GetType() == typeof(IndividualControl3))
          {
            trace.TraceInformation("ctrls-left!:" + ctrl.Text);
          }
        }
      }
      trace.TraceInformation("Controls.count.5 = " + this.Controls.Count);*/
      if(selectedIndividual != null)
      {
        IndividualControl3 indiCtrl2 = new IndividualControl3(selectedIndividual);

        indiCtrl2.SetPosition(0, layout.AddIndividual(0));
        indiCtrl2.SetSelected(true);
        indiCtrl2.Anchor = AnchorStyles.Left | AnchorStyles.Top;
        int generation = 0, position = 0;
        indiCtrl2.GetPosition(ref generation, ref position);

        trace.TraceInformation(" selected:" + selectedIndividual.GetXrefName() + " indiCtrl3 Top:" + indiCtrl2.Top + ", Left:" + indiCtrl2.Left + ", Height:" + indiCtrl2.Height + ", Width:" + indiCtrl2.Width + ", Visible:" + indiCtrl2.Visible + ", Text:" + indiCtrl2.Text);

        indiCtrl2.Left = 10 + (position * (indiCtrl2.Width + 10));
        indiCtrl2.Top = 100 + (generation * (indiCtrl2.Height + 10));


        this.Controls.Add(indiCtrl2);
        //trace.TraceInformation("Add!!!" + this.Controls.Count);
      }

      trace.TraceInformation("Selected->Parents: Controls.count.6 = " + this.Controls.Count);
      if (selectedIndividual != null)
      {
        IList<FamilyXrefClass> familyList;
        familyList = selectedIndividual.GetFamilyChildList();

        if (familyList != null)
        {
          trace.TraceInformation(" sel {1} is Child in {0} families", familyList.Count, selectedIndividual.GetXrefName());
          foreach (FamilyXrefClass familyXref in familyList)
          {
            FamilyClass family;
            IList<IndividualXrefClass> individualList;
            //familyXref.Print();

            family = familyTree.GetFamily(familyXref.GetXrefName());
            if (family != null)
            {
              if (trace.Switch.Level.HasFlag(SourceLevels.Information))
              {
                family.Print();
              }
              //generation = 1;
              //position = 0;

              individualList = family.GetParentList();
              if (individualList != null)
              {
                //family.Print();
                //trace.TraceInformation(" sel->child->parentList: " + individualList.Count);
                //trace.TraceInformation(" family {0} has {1} parents: ", family.GetXrefName(), individualList.Count);

                foreach (IndividualXrefClass individualXref in individualList)
                {
                  //individualXref.Print();
                  //if (individualXref.GetXrefName() != selectedIndividual.GetXrefName())
                  {
                    IndividualClass individual;

                    individual = familyTree.GetIndividual(individualXref.GetXrefName());

                    if (individual != null)
                    {
                      //individual.Print();

                      IndividualControl3 indiCtrl2b = new IndividualControl3(individual);
                      indiCtrl2b.SetPosition(-1, layout.AddIndividual(-1));
                      indiCtrl2b.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                      int generation = 0, position = 0;
                      indiCtrl2b.GetPosition(ref generation, ref position);

                      trace.TraceInformation(" add sel->child->parent:" + individualXref.GetXrefName() + " ctrl Top:" + indiCtrl2b.Top + ", Left:" + indiCtrl2b.Left + ", Height:" + indiCtrl2b.Height + ", Width:" + indiCtrl2b.Width + ", Visible:" + indiCtrl2b.Visible + ", xref:" + individualXref.GetXrefName() + ":Name:" + individual.GetName());


                      indiCtrl2b.Left = 10 + (position * (indiCtrl2b.Width + 10));
                      indiCtrl2b.Top = 100 + (generation * (indiCtrl2b.Height + 10));

                      this.Controls.Add(indiCtrl2b);
                    }
                    else
                    {
                      trace.TraceInformation(" add sel->child->parent:" + individualXref.GetXrefName() + " invalid!");

                    }
                    //trace.TraceInformation("Add!!!" + this.Controls.Count);
                  }
                  /*else
                  {
                    trace.TraceInformation("...ignored same as selected!...");
                  }*/

                }
              }
              else
              {
                //family.Print();
                trace.TraceInformation("family {0} has no parents: " + family.GetXrefName());
              }
            }
            else
            {
              trace.TraceEvent(TraceEventType.Error, 0, "Error: sel->child->parentList family " + familyXref.GetXrefName() + " was not found! ");
            }
          }
        }
        else
        {
          //trace.TraceInformation("sel->ChildList: null");
        }

        trace.TraceInformation("Selected + Spouses: Controls.count = " + this.Controls.Count);
        familyList = selectedIndividual.GetFamilySpouseList();

        if (familyList != null)
        {
          trace.TraceInformation(" selected {0} is spouse in {1} families: ", selectedIndividual.GetXrefName(), familyList.Count);
          foreach (FamilyXrefClass familyXref in familyList)
          {
            FamilyClass family;
            IList<IndividualXrefClass> individualList;

            family = familyTree.GetFamily(familyXref.GetXrefName());

            if (family != null)
            {
              individualList = family.GetParentList();

              if (individualList != null)
              {
                //trace.TraceInformation(" sel->spouse->parentList: " + individualList.Count);
                foreach (IndividualXrefClass individualXref in individualList)
                {
                  //trace.TraceInformation(" checking spouse->parent " + individualXref.GetXrefName());

                  if (individualXref.GetXrefName() != selectedIndividual.GetXrefName())
                  {
                    IndividualClass individual;
                    individual = familyTree.GetIndividual(individualXref.GetXrefName());

                    if (individual != null)
                    {
                      //individual.Print();


                      IndividualControl3 indiCtrl2b = new IndividualControl3(individual);
                      indiCtrl2b.SetPosition(0, layout.AddIndividual(0));
                      indiCtrl2b.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                      int generation = 0, position = 0;
                      indiCtrl2b.GetPosition(ref generation, ref position);

                      trace.TraceInformation(" add sel->spouse->parent:" + individualXref.GetXrefName() + " ctrl Top:" + indiCtrl2b.Top + ", Left:" + indiCtrl2b.Left + ", Height:" + indiCtrl2b.Height + ", Width:" + indiCtrl2b.Width + ", Visible:" + indiCtrl2b.Visible + ", Text:" + indiCtrl2b.Text);


                      indiCtrl2b.Left = 10 + (position * (indiCtrl2b.Width + 10));
                      indiCtrl2b.Top = 100 + (generation * (indiCtrl2b.Height + 10));

                      this.Controls.Add(indiCtrl2b);
                      //trace.TraceInformation("Add!!!" + this.Controls.Count);
                    }
                    else
                    {
                      trace.TraceInformation(" add sel->spouse->parent:" + individualXref.GetXrefName() + " invalid!");

                    }
                  }
                  else
                  {
                    trace.TraceInformation(" sel->spouse->parent:" + individualXref.GetXrefName() + "...ignored same as selected!...");
                  }

                }
              }
              else
              {
                trace.TraceInformation("  sel->Spouse->Parent: (null)");
              }

              trace.TraceInformation(" Controls.count = " + this.Controls.Count);


              /* Spouse's children not shown (except above)*/
              individualList = family.GetChildList();
              if (individualList != null)
              {
                //trace.TraceInformation(" sel->spouse->ChildList: " + familyList.Count);
                //family.Print();
                //trace.TraceInformation("sel->Spouse->Child: " + individualList.Count);

                foreach (IndividualXrefClass individualXref in individualList)
                {
                  //trace.TraceInformation(" checking spouse->child " + individualXref.GetXrefName());
                  //individualXref.Print();
                  if (individualXref.GetXrefName() != selectedIndividual.GetXrefName())
                  {
                    IndividualClass individual;
                    individual = familyTree.GetIndividual(individualXref.GetXrefName());
                    if (individual != null)
                    {
                      //individual.Print();

                      IndividualControl3 indiCtrl2b = new IndividualControl3(individual);
                      indiCtrl2b.SetPosition(1, layout.AddIndividual(1));
                      indiCtrl2b.Anchor = AnchorStyles.Left | AnchorStyles.Top;

                      int generation = 0, position = 0;
                      indiCtrl2b.GetPosition(ref generation, ref position);

                      trace.TraceInformation(" add sel->spouse->child:" + individualXref.GetXrefName() + " ctrl Top:" + indiCtrl2b.Top + ", Left:" + indiCtrl2b.Left + ", Height:" + indiCtrl2b.Height + ", Width:" + indiCtrl2b.Width + ", Visible:" + indiCtrl2b.Visible + ", Text:" + indiCtrl2b.Text);


                      indiCtrl2b.Left = 10 + (position * (indiCtrl2b.Width + 10));
                      indiCtrl2b.Top = 100 + (generation * (indiCtrl2b.Height + 10));


                      this.Controls.Add(indiCtrl2b);
                      //trace.TraceInformation("Add!!!" + this.Controls.Count);
                    }
                    else
                    {
                      trace.TraceInformation(" add sel->spouse->child:" + individualXref.GetXrefName() + " invalid!");
                    }

                  }
                  else
                  {
                    trace.TraceInformation("  sel->spouse->child:" + individualXref.GetXrefName() + " ...ignored same as selected!...");
                  }

                }
              }
              else
              {
                trace.TraceInformation(" sel->Spouse->Child: (null)");
              }
            }
            else
            {
              trace.TraceInformation("  sel->Spouse->Parent: family" + familyXref.GetXrefName() + " not found!");
            }

          }
        }
        else
        {
          trace.TraceInformation(" GetFamilySpouseList: null");
        }
        //layout.Print();
        if (trace.Switch.Level.HasFlag(SourceLevels.Information))
        {
          trace.TraceInformation("Selected->Children: Controls.count = " + this.Controls.Count);

          trace.TraceInformation("ShowActiveFamily (end)");
        }

        //FamilyForm2_Layout();
        //layoutDone = false;


        //this.Height = 400;
        //this.Width = 400;
        //this.SetAutoScrollMargin(400, 400);

        //this.Controls.Add(hScrollBar);

        //this.Show();
      }
    }

    protected override void OnEnter(EventArgs e)
    {

      if (!layoutDone && (selectedIndividual != null))
      {
        ShowActiveFamily();
        layoutDone = true;
      }
      base.OnEnter(e);
    }

    protected override void OnLayout(LayoutEventArgs levent)
    {
      //int x = 0, y = 0, maxY = 0;
      //int center_x = this.Width / 2;
      //int center_y = this.Height / 2;
      //trace.TraceInformation("FamilyForm2_Layout");
      if (!layoutDone && (selectedIndividual != null))
      {
        ShowActiveFamily();
        layoutDone = true;

      }

      base.OnLayout(levent);

/*      if (layoutDone)
      {
        //return;
      }

      trace.TraceInformation("OnLayout(" + layoutDone + "," + levent.AffectedComponent + "," + levent.AffectedControl + "," + levent.AffectedProperty + "," + levent.AffectedComponent + ") Top:" + this.Top + ", Left:" + this.Left + ", Height:" + this.Height + ", Width:" + this.Width);
      if (!layoutDone)
      {
        //ShowActiveFamily();
        //layoutDone = true;
      }*/
      //foreach (Control ctrl in this.Controls)
      {
        Control ctrl = levent.AffectedControl;
        //if (ctrl.GetType() == typeof(IndividualControl3))
        {
          //int generation = 0, position = 0;
          //IndividualControl3 ctrl3 = (IndividualControl3)ctrl;

          //ctrl3.GetPosition(ref generation, ref position);
          //trace.TraceInformation(" my ctrl Top:" + ctrl.Top + ", Left:" + ctrl.Left + ", Height:" + ctrl.Height + ", Width:" + ctrl.Width + ", Visible:" + ctrl.Visible + ", Text:" + ctrl.Text);

          //ctrl.Left = 10 + (position * (ctrl.Width + 10));
          //ctrl.Top = 100 + (generation * (ctrl.Height + 10));

          if (ctrl.Top + ctrl.Height > this.Height)
          {
            this.Height = ctrl.Top + ctrl.Height;
          }
          if (ctrl.Left + ctrl.Width > this.Width)
          {
            this.Width = ctrl.Left + ctrl.Width;
          }
          
        }
        //trace.TraceInformation("  ctrl Top:" + ctrl.Top + ", Left:" + ctrl.Left + ", Height:" + ctrl.Height + ", Width:" + ctrl.Width + ", Visible:" + ctrl.Visible + ", ToStr:" + ctrl.ToString() + ", Text:" + ctrl.Text + ", GetType:" + ctrl.GetType());
      }
    }


    public class FamilyLayout
    {
      //private int minGeneration;
      //private int maxGeneration;
      //private IDictionary maxPosition;
      private ArrayList maxPosition;
      private const int maxGenerations = 6;

      public FamilyLayout()
      {
        //minGeneration = -3;
        //maxPosition = 3;
        //maxPosition = new Dictionary<int, int>();
        maxPosition = new ArrayList(2 * maxGenerations);
        Reset();
        trace.TraceInformation("FamilyLayout()");
      }
      public void Reset()
      {
        //maxPosition = new Dictionary<int, int>();
        for (int i = 0; i < 2 * maxGenerations; i++)
        {
          maxPosition.Insert(i, 0);
        }
        trace.TraceInformation("Reset()");
      }

      public int AddIndividual(int generation)
      {
        int position;// = (int)maxPosition[generation + 10];

        /*if (maxPosition[generation+10] != null)
        {
          trace.TraceInformation("AddIndividual-not-null");
          position = (int)maxPosition[generation + 10];
          trace.TraceInformation(position);
        }*/
        position = (int)maxPosition[generation + maxGenerations];
        maxPosition[generation + maxGenerations] = ++position;
        //trace.TraceInformation("AddIndividual(" + generation + "," + (position - 1) + ")");
        //Print();
        return position - 1;
      }
      public int GetIndividualNo(int generation)
      {
        int position;

        /*if (maxPosition[generation+10] != null)
        {
          position = (int)maxPosition[generation+10];
        }*/
        position = (int)maxPosition[generation + maxGenerations];

        //trace.TraceInformation("GetIndividualNo(" + generation + ")=" + position);

        return position;
      }

      public void Print()
      {
        for (int i = 0; i < 2 * maxGenerations; i++)
        {
          trace.TraceInformation("[" + maxPosition[i] + "]");
        }
      }

    }
  }
}

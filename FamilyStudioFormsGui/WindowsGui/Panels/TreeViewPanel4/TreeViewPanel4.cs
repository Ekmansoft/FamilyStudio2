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

namespace FamilyStudioFormsGui.WindowsGui.Panels.TreeViewPanel4
{
  class FamilyButton : Button
  {
    //public FamilyClass family;

    private IList<IndividualButton> parents;
    private IList<IndividualButton> children;
    private Point IndividualBoxSize = new Point(5, 20);

    public FamilyButton()
    {
      parents = new List<IndividualButton>();
      children = new List<IndividualButton>();
    }

    public void AddParentButton(IndividualButton button)
    {
      parents.Add(button);
    }
    public void AddChildButton(IndividualButton button)
    {
      children.Add(button);
    }

    public IList<IndividualButton> GetParentList()
    {
      return parents;
    }
    public IList<IndividualButton> GetChildList()
    {
      return children;
    }

    public Point GetParentBoxSize()
    {
      Point size = new Point(IndividualBoxSize.X, IndividualBoxSize.Y);

      foreach (IndividualButton parent in parents)
      {
        Point parentBoxSize = parent.GetAncestorBoxSize();

        size.X += parentBoxSize.X;

        if (parentBoxSize.Y > size.Y)
        {
          size.Y = parentBoxSize.Y;
        }
      }
      return size;
    }
    public Point GetChildBoxSize()
    {
      Point size = new Point(IndividualBoxSize.X, IndividualBoxSize.Y);

      foreach (IndividualButton child in children)
      {
        Point childBoxSize = child.GetSuccessorBoxSize();

        size.X += childBoxSize.X;

        if (childBoxSize.Y > size.Y)
        {
          size.Y = childBoxSize.Y;
        }
      }
      return size;
    }
  }

  class IndividualButton : Button
  {
    private static TraceSource trace = new TraceSource("IndividualButton", SourceLevels.Warning);
    public IndividualClass individual;
    private TreeViewPanel4 parent;
    private IList<FamilyButton> childFamily;
    private IList<FamilyButton> ownFamily;

    private Point FamilyBoxSize = new Point(5, 20);

    public IndividualButton()
    {
      childFamily = new List<FamilyButton>();
      ownFamily = new List<FamilyButton>();
    }

    public void SetParent(TreeViewPanel4 inParent)
    {
      parent = inParent;
    }

    public void AddChildFamilyButton(FamilyButton button)
    {
      childFamily.Add(button);
    }
    public void AddOwnFamilyButton(FamilyButton button)
    {
      ownFamily.Add(button);
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
        trace.TraceEvent(TraceEventType.Error, 0, "error: Clicked person = null!");
      }

    }

    public IList<FamilyButton> GetChildFamilyList()
    {
      return childFamily;
    }
    public IList<FamilyButton> GetOwnFamilyList()
    {
      return ownFamily;
    }


    public Point GetAncestorBoxSize()
    {
      Point size = new Point(FamilyBoxSize.X, FamilyBoxSize.Y);

      foreach (FamilyButton family in ownFamily)
      {
        Point familySize = family.GetParentBoxSize();

        size.X += familySize.X;

        if (familySize.Y > size.Y)
        {
          size.Y = familySize.Y;
        }
      }
      return size;
    }
    public Point GetSuccessorBoxSize()
    {
      Point size = new Point(FamilyBoxSize.X, FamilyBoxSize.Y);

      foreach (FamilyButton family in childFamily)
      {
        Point familySize = family.GetChildBoxSize();

        size.X += familySize.X;

        if (familySize.Y > size.Y)
        {
          size.Y = familySize.Y;
        }
      }
      return size;
    }
  }

  class LayoutControl : Object
  {
    private int predecessorGenerations;
    private int successorGenerations;
    private IList<Point> predecessorGenerationPosition;
    private IList<Point> successorGenerationPosition;

    public LayoutControl(int predecessors, int successors)
    {
      predecessorGenerations = predecessors;
      successorGenerations = successors;

      predecessorGenerationPosition = new List<Point>();
      successorGenerationPosition = new List<Point>();

      for (int i = 0; i < predecessors; i++)
      {
        predecessorGenerationPosition[i] = new Point(0, 0);
      }
      for (int i = 0; i < successors; i++)
      {
        successorGenerationPosition[i] = new Point(0, 0);
      }

    }
  }


  class TreeViewPanel4 : TreeViewPanelBaseClass
  {
    private static TraceSource trace = new TraceSource("TreeViewPanel4", SourceLevels.Warning);
    private FamilyTreeStoreBaseClass familyTree;
    private IndividualClass selectedIndividual;
    private FamilyClass selectedFamily;
    private IList<Control> controlList;
    private FamilyForm2 parentForm;


    public TreeViewPanel4()
    {
      controlList = new List<Control>();

      this.Dock = DockStyle.Fill;
      parentForm = null;

      trace.TraceInformation("TreeViewPanel4::TreeViewPanel4()");

    }
    public override void SetParentForm(FamilyForm2 parentForm)
    {
      this.parentForm = parentForm;

      parentForm.SelectedPersonChanged += OnSelectedPersonChangedEvent;

    }
    public override void PasteFromClipboard(object clipboard)
    {

    }

    public void ClickPerson(IndividualClass person)
    {
      selectedIndividual = person;
      if (parentForm != null)
      {
        parentForm.SetSelectedIndividual(person.GetXrefName());
      }
    }



    public override void SetFamilyTree(FamilyTreeStoreBaseClass inFamilyTree)
    {
      trace.TraceInformation("TreeViewPanel4::SetFamilyTree()");

      familyTree = inFamilyTree;

    }
    /*public void SetSelectedIndividual(String xrefName)
    {
      trace.TraceInformation("TreeViewPanel4::SetSelectedIndividual(" + xrefName + ")");
      if (familyTree != null)
      {
        selectedIndividual = (IndividualClass)familyTree.GetIndividual(xrefName);

        ShowActiveFamily();
      }
    }*/

    public override string GetTitle()
    {
      return "FamilyView4";
    }


    public override void OnSelectedPersonChangedEvent(object sender, PersonChangeEvent e)
    {
      if (familyTree != null)
      {
        selectedIndividual = e.selectedPerson;
        ShowActiveFamily();
      }

    }


    private Image GetImage(String url)
    {
      // Create a request for the URL. 		
      WebRequest request = WebRequest.Create(url);
      // If required by the server, set the credentials.
      request.Credentials = CredentialCache.DefaultCredentials;
      // Get the response.
      HttpWebResponse response = (HttpWebResponse)request.GetResponse();
      // Display the status.
      trace.TraceInformation(response.StatusDescription);
      // Get the stream containing content returned by the server.
      Stream dataStream = response.GetResponseStream();
      // Open the stream using a StreamReader for easy access.
      //StreamReader reader = new StreamReader(dataStream);
      // Read the content.
      //string responseFromServer = reader.ReadToEnd();
      // Display the content.
      //trace.TraceInformation(responseFromServer);
      // Cleanup the streams and the response.

      Image image = Image.FromStream(dataStream);

      //reader.Close();
      dataStream.Close();
      response.Close();

      return image;
    }

    private void ShowActiveFamily()
    {
      trace.TraceInformation("TreeViewPanel4::ShowActiveFamily (start) " + this.CanFocus);

      while(controlList.Count > 0)
      {
        Control ctrl = controlList[0];

        this.Controls.Remove(ctrl);
        ctrl.Dispose();

        controlList.RemoveAt(0);

        //ctrl.
      }
      selectedFamily = null;

      if (selectedIndividual != null)
      {
        //int pos = 0;
        System.Drawing.Point position = new Point(0, 0);

        int ctrlHeight = 0;

        {

          IList<FamilyXrefClass> children = selectedIndividual.GetFamilyChildList();
          //trace.TraceInformation("GetFamilyChildList");

          if (children != null)
          {
            //trace.TraceInformation("Children.count = " + children.Count);
            foreach (FamilyXrefClass childXref in children)
            {
              FamilyClass childFamily = new FamilyClass();
              childFamily = familyTree.GetFamily(childXref.GetXrefName());
              if (childFamily != null)
              {
                trace.TraceInformation(" parentFamily:" + childFamily.GetXrefName());

                if (childFamily != null)
                {
                  if (childFamily.GetParentList() != null)
                  {
                    foreach (IndividualXrefClass parentXref in childFamily.GetParentList())
                    {
                      IndividualClass parent = new IndividualClass();

                      parent = familyTree.GetIndividual(parentXref.GetXrefName());

                      if (parent != null)
                      {
                        IndividualButton ctrl2 = new IndividualButton();
                        //int position.Y = 0;

                        if (ctrlHeight == 0)
                        {
                          Label label = new Label();

                          label.Top = position.Y;
                          label.Left = position.X;
                          label.Text = "Parents:";

                          this.Controls.Add(label);
                          controlList.Add(label);

                          position.Y += label.Height;
                        }

                        ctrl2.AutoSize = true;
                        ctrl2.Left = position.X;
                        ctrl2.Top = position.Y;
                        //ctrl.Height = 100;
                        //ctrl.Width = 400;
                        ctrl2.Text = parent.GetName() + "\n" + parent.GetDate(IndividualEventClass.EventType.Birth).ToString() + " - " + parent.GetDate(IndividualEventClass.EventType.Death).ToString();
                        trace.TraceInformation(" parent: AddControl:" + parent.GetName() + " X:" + position.X + " Y:" + position.Y + " Top:" + ctrl2.Top + " height:" + ctrl2.Height);
                        ctrl2.FlatStyle = FlatStyle.Flat;
                        ctrl2.individual = parent;
                        ctrl2.SetParent(this);

                        ctrl2.Click += new System.EventHandler(ctrl2.Clicked);

                        //ctrl2.Anchor = AnchorStyles.Left | AnchorStyles.Top;

                        //ctrl.Height = 40;
                        //ctrl.Width = 40;
                        //ctrl.Show();

                        controlList.Add(ctrl2);

                        this.Controls.Add(ctrl2);

                        position.X += ctrl2.Width;

                        ctrlHeight = ctrl2.Height;

                      }
                      else
                      {
                        trace.TraceEvent(TraceEventType.Error, 0, "Error not a vaild person xref:" + parentXref.GetXrefName());
                      }
                    }
                  }
                }
              }
              else
              {
                trace.TraceEvent(TraceEventType.Error, 0, "Error not a vaild person xref:" + childXref.GetXrefName());
              }
            }
          }
        }

        if (ctrlHeight != 0)
        {
          position.X = 0;
          position.Y += ctrlHeight;
          position.Y += 20;

          ctrlHeight = 0;
        }

        {
          {
            Label label = new Label();

            label.Top = position.Y;
            label.Left = position.X;
            label.Text = "Selected:";

            this.Controls.Add(label);
            controlList.Add(label);

            position.Y += label.Height;
          }
          IndividualButton ctrl = new IndividualButton();

          ctrl.AutoSize = true;
          ctrl.Left = position.X;
          ctrl.Top = position.Y;
          //ctrl.Height = 100;
          //ctrl.Width = 400;
          ctrl.Text = selectedIndividual.GetName() + "\n" + selectedIndividual.GetDate(IndividualEventClass.EventType.Birth).ToString() + " - " + selectedIndividual.GetDate(IndividualEventClass.EventType.Death).ToString();

          ctrl.Anchor = AnchorStyles.Left | AnchorStyles.Top;
          ctrl.FlatStyle = FlatStyle.Flat;
          ctrl.Click += new System.EventHandler(ctrl.Clicked);
          ctrl.BackColor = Color.Beige;

          ctrl.individual = selectedIndividual;
          ctrl.SetParent(this);

          trace.TraceInformation(" selected: AddControl:" + selectedIndividual.GetName() + " X:" + position.X + " Y:" + position.Y + " Top:" + ctrl.Top + " Height:" + ctrl.Height);
          controlList.Add(ctrl);

          this.Controls.Add(ctrl);

          position.X += ctrl.Width;

          ctrlHeight = ctrl.Height;
        }

        /*if (ctrlHeight != 0)
        {
          position.Y += 20;
          ctrlHeight = 0;
        }*/

        {

        IList<FamilyXrefClass> spouseList = selectedIndividual.GetFamilySpouseList();

        //trace.TraceInformation("GetFamilySpouseList()");
        if (spouseList != null)
        {
          //trace.TraceInformation("spouses.count = " + spouseList.Count);
          foreach (FamilyXrefClass spouseFamilyXref in spouseList)
          {
            FamilyClass spouseFamily = new FamilyClass();
            spouseFamily = familyTree.GetFamily(spouseFamilyXref.GetXrefName());

            //trace.TraceInformation("spouses.count s2=" + spouseFamilyXref.GetXrefName());
            if (spouseFamily != null)
            {
              trace.TraceInformation(" spouseFamily:" + spouseFamily.GetXrefName());
              //trace.TraceInformation("spouses.count s3 = " + spouseFamily);
              if (selectedFamily == null)
              {
                selectedFamily = spouseFamily;
              }
              //trace.TraceInformation("spouses.count s4 = ");
              if (spouseFamily.GetParentList() != null)
              {
                foreach (IndividualXrefClass spouseXref in spouseFamily.GetParentList())
                {
                  //trace.TraceInformation("spouses.count s5 = ");
                  if (spouseXref.GetXrefName() != selectedIndividual.GetXrefName())
                  {
                    IndividualClass spouse = new IndividualClass();

                    spouse = familyTree.GetIndividual(spouseXref.GetXrefName());

                    if (spouse != null)
                    {
                      IndividualButton ctrl2 = new IndividualButton();
                      //int position.Y = 0;

                      ctrl2.AutoSize = true;
                      ctrl2.Left = position.X;
                      ctrl2.Top = position.Y;

                      ctrl2.Text = spouse.GetName() + "\r" + spouse.GetDate(IndividualEventClass.EventType.Birth).ToString() + " - " + spouse.GetDate(IndividualEventClass.EventType.Death).ToString();

                      ctrl2.SetParent(this);


                      ctrl2.FlatStyle = FlatStyle.Flat;
                      ctrl2.individual = spouse;
                      ctrl2.Click += new System.EventHandler(ctrl2.Clicked);

                      controlList.Add(ctrl2);

                      this.Controls.Add(ctrl2);
                      //ctrl2.PerformLayout();
                      position.X += ctrl2.Width;
                      ctrlHeight = ctrl2.Height;
                      trace.TraceInformation(" spouse: AddControl:" + spouse.GetName() + " X:" + position.X + " Y:" + position.Y + " Top:" + ctrl2.Top + " Height:" + ctrl2.Height);

                    }
                  }
                }
              }
            }
            }
          }
        }
        if (ctrlHeight != 0)
        {
          position.X = 0;
          position.Y += ctrlHeight;
          position.Y += 20;
          ctrlHeight = 0;
        }
        if (selectedFamily != null)
        {
          IList<IndividualXrefClass> childXrefList = selectedFamily.GetChildList();

          trace.TraceInformation(" childFamily:" + selectedFamily.GetXrefName());
          if (childXrefList != null)
          {
            foreach (IndividualXrefClass childXref in childXrefList)
            {
              IndividualClass child = new IndividualClass();

              child = familyTree.GetIndividual(childXref.GetXrefName());

              if (child != null)
              {
                if (ctrlHeight == 0)
                {
                  Label label = new Label();

                  label.Top = position.Y;
                  label.Left = position.X;
                  label.Text = "Children:";

                  this.Controls.Add(label);
                  controlList.Add(label);

                  position.Y += label.Height;
                }
                IndividualButton ctrl2 = new IndividualButton();
                //int position.Y = 0;

                ctrl2.AutoSize = true;
                ctrl2.Left = position.X;
                ctrl2.Top = position.Y;
                ctrl2.FlatStyle = FlatStyle.Flat;
                ctrl2.Click += new System.EventHandler(ctrl2.Clicked);
                //ctrl.Height = 100;
                //ctrl.Width = 400;
                ctrl2.Text = child.GetName() + "\n" + child.GetDate(IndividualEventClass.EventType.Birth).ToString() + " - " + child.GetDate(IndividualEventClass.EventType.Death).ToString();
                ctrl2.individual = child;
                ctrl2.SetParent(this);

                //ctrl2.Anchor = AnchorStyles.Left | AnchorStyles.Top;

                //ctrl.Height = 40;
                //ctrl.Width = 40;
                //ctrl.Show();

                controlList.Add(ctrl2);

                this.Controls.Add(ctrl2);

                trace.TraceInformation(" child: AddControl:" + child.GetName() + " X:" + position.X + " Y:" + position.Y + " Top:" + ctrl2.Top + " Height:" + ctrl2.Height);

                position.Y += ctrl2.Height;
                ctrlHeight += ctrl2.Height;
              }
            }
          }
        }




      }
      this.Top = 0;
      this.Left = 0;

      this.Width = 600;
      this.Height = 600;


      //this.Show();

      trace.TraceInformation("TreeViewPanel4::ShowActiveFamily (end) ");
    }

    /*public void SetParentForm(FamilyForm2 parentForm)
    {
      this.GetTopLevel.parentForm = parentForm;
    }*/


  }
}

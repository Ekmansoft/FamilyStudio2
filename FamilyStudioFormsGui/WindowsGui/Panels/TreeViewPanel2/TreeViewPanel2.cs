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

namespace FamilyStudioFormsGui.WindowsGui.Panels.TreeViewPanel2
{
  class IndividualButton : Button
  {
    public IndividualClass individual;
    private TreeViewPanel2 parent;
    private ToolTip details;

    public IndividualButton(IndividualClass individual, bool selected = false)
    {
      this.individual = individual;

      this.Text = individual.GetName() + "\n" + individual.GetDate(IndividualEventClass.EventType.Birth).ToString() + " - " + individual.GetDate(IndividualEventClass.EventType.Death).ToString();
      details = new ToolTip();
      //details.IsBalloon = true;
      FlatStyle = FlatStyle.Flat;
      AutoSize = true;
      Anchor = AnchorStyles.Left | AnchorStyles.Top;
      Click += new System.EventHandler(Clicked);
      this.MouseUp += IndividualButton_MouseUp;
      //BackColor = Color.Beige;

      //details.AutomaticDelay = 10000;
      details.AutoPopDelay = 600000;
      //string toolTip = ;
      details.SetToolTip(this, CreateToolString());
      //details.ToolTipTitle = individual.GetName();
      //details.ToolTipIcon = ToolTipIcon.Info;
      IndividualClass.IndividualSexType sex = individual.GetSex();

      if (!selected)
      {
        if (sex == IndividualClass.IndividualSexType.Female)
        {
          this.BackColor = Color.LightPink;
        }
        else if (sex == IndividualClass.IndividualSexType.Male)
        {
          this.BackColor = Color.LightBlue;
        }
        else if (sex == IndividualClass.IndividualSexType.Unknown)
        {
          this.BackColor = Color.LightGray;
        }
      }

    }

    void IndividualButton_MouseUp(object sender, MouseEventArgs e)
    {
      if (e.Button == System.Windows.Forms.MouseButtons.Right)
      {
        ContextMenu menu = new ContextMenu();

        menu.MenuItems.Add(new MenuItem("Add parent", AddParent_Click));
        menu.MenuItems.Add(new MenuItem("Add child", AddChild_Click));
        IList<string> urlList = individual.GetUrlList();
        if (urlList != null)
        {
          foreach (string url in urlList)
          {
            menu.MenuItems.Add(new MenuItem(url, Url_Click));
          }
        }
        menu.Show(this, new Point(0, 0));
      }      
    }

    void AddParent_Click(object sender, EventArgs e)
    {
      parent.AddRelative(TreeViewPanel2.RelativeType.Parent);
    }
    void AddChild_Click(object sender, EventArgs e)
    {
      parent.AddRelative(TreeViewPanel2.RelativeType.Child);
    }
    void Url_Click(object sender, EventArgs e)
    {
      if (sender.GetType() == typeof(MenuItem))
      {
        MenuItem clickedItem = (MenuItem)sender;
        //parent.AddRelative(AsyncTreePanel1.RelativeType.Child);
        Process.Start(clickedItem.Text);
      }
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

          if(place != null)
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

    public void SetParent(TreeViewPanel2 inParent)
    {
      parent = inParent;
    }

    public void Clicked(object sender, EventArgs e)
    {
      if (individual != null)
      {
        if (parent != null)
        {
          parent.ClickSelectedIndividual(individual);
        }
      }

    }

  }


  class TreeViewPanel2 : TreeViewPanelBaseClass
  {
    private static TraceSource trace = new TraceSource("TreeViewPanel2", SourceLevels.Warning);
    private FamilyTreeStoreBaseClass familyTree;
    private IndividualClass selectedIndividual;
    private IndividualClass pastedIndividual;
    //private FamilyClass selectedFamily;
    private IList<Control> controlList;
    private FamilyForm2 parentForm;
    //private MessageBox details;

    public enum RelativeType
    {
      Unrelated,
      Parent,
      Child
    };


    public TreeViewPanel2()
    {
      controlList = new List<Control>();

      this.Dock = DockStyle.Fill;
      parentForm = null;
      //details = new MessageBox();

      trace.TraceInformation("TreeViewPanel2::TreeViewPanel2()");

    }
    public override void SetParentForm(FamilyForm2 parentForm)
    {
      this.parentForm = parentForm;

      parentForm.SelectedPersonChanged += OnSelectedPersonChangedEvent;

    }

    public override void PasteFromClipboard(object clipboard)
    {
      trace.TraceEvent(TraceEventType.Warning, 0, "PasteFromClipboard:" + clipboard);

      if ((clipboard != null) && (clipboard.GetType() == typeof(IndividualClass)))
      {
        pastedIndividual = (IndividualClass) clipboard;
        ContextMenu menu = new ContextMenu();

        if (selectedIndividual != null)
        {
          menu.MenuItems.Add(new MenuItem("Paste as parent", PasteAsParent_Click));
          menu.MenuItems.Add(new MenuItem("Paste as child", PasteAsChild_Click));
          menu.Show(this, new Point(0, 0));
        }
      }
    }

    void PasteAsParent_Click(object sender, EventArgs e)
    {
      AddRelative(TreeViewPanel2.RelativeType.Parent, pastedIndividual);
      pastedIndividual = null;
    }
    void PasteAsChild_Click(object sender, EventArgs e)
    {
      AddRelative(TreeViewPanel2.RelativeType.Child, pastedIndividual);
      pastedIndividual = null;
    }

    public override void SetFamilyTree(FamilyTreeStoreBaseClass inFamilyTree)
    {
      trace.TraceInformation("TreeViewPanel2::SetFamilyTree()");

      familyTree = inFamilyTree;

    }
    /*public void SetSelectedIndividual(String xrefName)
    {
      trace.TraceInformation("TreeViewPanel2::SetSelectedIndividual(" + xrefName + ")");
      if (familyTree != null)
      {
        selectedIndividual = (IndividualClass)familyTree.GetIndividual(xrefName);

        ShowActiveFamily();
      }
    }*/
    public void ClickSelectedIndividual(IndividualClass individual)
    {
      trace.TraceInformation("TreeViewPanel2::ClickSelectedIndividual(" + individual.GetPersonalName().GetName() + ")");
      if (familyTree != null)
      {
        selectedIndividual = individual;

        ShowActiveFamily();

        if (parentForm != null)
        {
          parentForm.SetSelectedIndividual(individual.GetXrefName());
        }
      }
    }

    public  void AddRelative(RelativeType relation, IndividualClass person = null)
    {
      trace.TraceInformation("TreeViewPanel2::AddRelative " + relation);
      if (familyTree != null)
      {
        if (selectedIndividual != null)
        {
          IndividualClass newPerson;
          if (person != null)
          {
            newPerson = person;
          }
          else
          {
            newPerson = new IndividualClass();
          }
          newPerson.SetXrefName(familyTree.CreateNewXref(XrefType.Individual));

          if (relation == RelativeType.Parent)
          {
            FamilyXrefClass parentFamilyXref = null;
            FamilyClass parentFamily = null;
            IList<FamilyXrefClass> parents = selectedIndividual.GetFamilyChildList();
            if (parents != null)
            {
              if (parents.Count > 0)
              {
                // ToDo: Full support for multiple families..
                parentFamilyXref = parents[0];
                parentFamily = familyTree.GetFamily(parentFamilyXref.GetXrefName());
              }
            }
            if (parentFamilyXref == null)
            {
              parentFamilyXref = new FamilyXrefClass(familyTree.CreateNewXref(XrefType.Family));
              //parentFamily.SetXrefName();              
              parentFamily = new FamilyClass();
              parentFamily.SetXrefName(parentFamilyXref.GetXrefName());
              parentFamily.AddRelation(new IndividualXrefClass(selectedIndividual.GetXrefName()), FamilyClass.RelationType.Child);
              selectedIndividual.AddRelation(parentFamilyXref, IndividualClass.RelationType.Child);
              familyTree.UpdateIndividual(selectedIndividual, PersonUpdateType.ChildFamily);
            }
            parentFamily.AddRelation(new IndividualXrefClass(newPerson.GetXrefName()), FamilyClass.RelationType.Parent);
            newPerson.AddRelation(parentFamilyXref, IndividualClass.RelationType.Spouse);
            familyTree.AddFamily(parentFamily);

            familyTree.AddIndividual(newPerson);
          }
          else if (relation == RelativeType.Child)
          {
            FamilyXrefClass childFamilyXref = null;
            FamilyClass childFamily = null;
            IList<FamilyXrefClass> children = selectedIndividual.GetFamilySpouseList();
            if (children != null)
            {
              if (children.Count > 0)
              {
                // ToDo: Full support for multiple families..
                childFamilyXref = children[0];
                childFamily = familyTree.GetFamily(childFamilyXref.GetXrefName());
              }
            }
            if (childFamilyXref == null)
            {
              childFamilyXref = new FamilyXrefClass(familyTree.CreateNewXref(XrefType.Family));
              //parentFamily.SetXrefName();              
              childFamily = new FamilyClass();
              childFamily.SetXrefName(childFamilyXref.GetXrefName());
              childFamily.AddRelation(new IndividualXrefClass(selectedIndividual.GetXrefName()), FamilyClass.RelationType.Parent);
              selectedIndividual.AddRelation(childFamilyXref, IndividualClass.RelationType.Spouse);
              familyTree.UpdateIndividual(selectedIndividual, PersonUpdateType.SpouseFamily);
            }
            childFamily.AddRelation(new IndividualXrefClass(newPerson.GetXrefName()), FamilyClass.RelationType.Child);
            newPerson.AddRelation(childFamilyXref, IndividualClass.RelationType.Child);
            familyTree.AddFamily(childFamily);

            familyTree.AddIndividual(newPerson);
          }
          else // if (relation == RelativeType.Unrelated)
          {
            familyTree.AddIndividual(newPerson);
          }
          //familyTree.AddIndividual(newPerson);

        }
        ShowActiveFamily();

      }


    }

    public override string GetTitle()
    {
      return "FamilyView2";
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
      trace.TraceInformation("TreeViewPanel2::ShowActiveFamily (start) " + this.CanFocus);

      while(controlList.Count > 0)
      {
        Control ctrl = controlList[0];

        this.Controls.Remove(ctrl);
        ctrl.Dispose();

        controlList.RemoveAt(0);
      }

      if (selectedIndividual != null)
      {
        Point position = new Point(0, 0);

        int ctrlHeight = 0;
        IDictionary<string,Point> familyPosition = new Dictionary<string,Point>();

        {

          IList<FamilyXrefClass> children = selectedIndividual.GetFamilyChildList();

          if (children != null)
          {
            trace.TraceInformation(" selected->parentFamilies.count = " + children.Count);
            foreach (FamilyXrefClass childXref in children)
            {
              FamilyClass childFamily = new FamilyClass();
              childFamily = familyTree.GetFamily(childXref.GetXrefName());

              if (childFamily != null)
              {
                trace.TraceInformation(" selected->parentFamily:" + childFamily.GetXrefName());
                if (childFamily.GetParentList() != null)
                {
                  foreach (IndividualXrefClass parentXref in childFamily.GetParentList())
                  {
                    IndividualClass parent = new IndividualClass();

                    parent = familyTree.GetIndividual(parentXref.GetXrefName());

                    if (parent != null)
                    {
                      IndividualButton ctrl2 = new IndividualButton(parent);

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

                      //ctrl2.AutoSize = true;
                      ctrl2.Left = position.X;
                      ctrl2.Top = position.Y;
                      //ctrl2.Text = parent.GetName() + "\n" + parent.GetDate(IndividualEventClass.EventType.Birth).ToString() + " - " + parent.GetDate(IndividualEventClass.EventType.Death).ToString();
                      trace.TraceInformation(" parent: AddControl:" + parent.GetName() + " X:" + position.X + " Y:" + position.Y + " Top:" + ctrl2.Top + " height:" + ctrl2.Height);
                      //ctrl2.FlatStyle = FlatStyle.Flat;
                      //ctrl2.individual = parent;
                      ctrl2.SetParent(this);

                      //ctrl2.Click += new System.EventHandler(ctrl2.Clicked);
                      //ctrl2.MouseEnter += MouseEntered;
                      //ctrl2.MouseLeave += MouseLeft;

                      controlList.Add(ctrl2);

                      this.Controls.Add(ctrl2);

                      position.X += ctrl2.Width + 10;

                      ctrlHeight = ctrl2.Height;

                    }
                    else
                    {
                      trace.TraceInformation("Error not a vaild person xref:" + parentXref.GetXrefName());
                    }
                  }
                }
              }
              else
              {
                trace.TraceInformation("Error not a vaild person xref:" + childXref.GetXrefName());

              }
            }
          }
          else
          {
            trace.TraceInformation("selected->Children null ");
          }
        }

        if (ctrlHeight != 0)
        {
          position.X = 0;
          position.Y += ctrlHeight;
          //position.Y += 20;

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
          IndividualButton ctrl = new IndividualButton(selectedIndividual, true);

          //ctrl.AutoSize = true;
          ctrl.Left = position.X;
          ctrl.Top = position.Y;

          //ctrl.Text = selectedIndividual.GetName() + "\n" + selectedIndividual.GetDate(IndividualEventClass.EventType.Birth).ToString() + " - " + selectedIndividual.GetDate(IndividualEventClass.EventType.Death).ToString();

          //ctrl.Anchor = AnchorStyles.Left | AnchorStyles.Top;
          //ctrl.FlatStyle = FlatStyle.Flat;
          //ctrl.Click += new System.EventHandler(ctrl.Clicked);
          //ctrl.MouseEnter += new System.EventHandler(MouseEntered);
          //ctrl.MouseLeave += new System.EventHandler(MouseLeft);
          //ctrl.BackColor = Color.Beige;

          //ctrl.individual = selectedIndividual;
          ctrl.SetParent(this);

          trace.TraceInformation(" selected: AddControl:" + selectedIndividual.GetName() + " X:" + position.X + " Y:" + position.Y + " Top:" + ctrl.Top + " Height:" + ctrl.Height);
          controlList.Add(ctrl);

          this.Controls.Add(ctrl);

          position.X += ctrl.Width + 10;

          ctrlHeight = ctrl.Height;
        }

        {

          IList<FamilyXrefClass> spouseList = selectedIndividual.GetFamilySpouseList();

          if (spouseList != null)
          {
            trace.TraceInformation(" selected->spouseFamily->count:" + spouseList.Count);
            foreach (FamilyXrefClass spouseFamilyXref in spouseList)
            {
              FamilyClass spouseFamily = new FamilyClass();
              spouseFamily = familyTree.GetFamily(spouseFamilyXref.GetXrefName());

              if (spouseFamily != null)
              {
                trace.TraceInformation(" selected->spouseFamily:" + spouseFamily.GetXrefName());
                if (!familyPosition.ContainsKey(spouseFamily.GetXrefName()))
                {
                  Point famPos = new Point(position.X - 20, position.Y + ctrlHeight);
                  familyPosition.Add(spouseFamilyXref.GetXrefName(), famPos);
                  trace.TraceInformation(" selected->spouseFamily Add:" + famPos.X + "," + famPos.Y);
                }
                IList<IndividualXrefClass> spouseParentList = spouseFamily.GetParentList();

                if (spouseParentList != null)
                {
                  foreach (IndividualXrefClass spouseXref in spouseParentList)
                  {
                    if (spouseXref.GetXrefName() != selectedIndividual.GetXrefName())
                    {
                      IndividualClass spouse = new IndividualClass();

                      spouse = familyTree.GetIndividual(spouseXref.GetXrefName());

                      if (spouse != null)
                      {


                        IndividualButton ctrl2 = new IndividualButton(spouse);
                        //int position.Y = 0;

                        //ctrl2.AutoSize = true;
                        ctrl2.Left = position.X;
                        ctrl2.Top = position.Y;

                        //ctrl2.Text = spouse.GetName() + "\r" + spouse.GetDate(IndividualEventClass.EventType.Birth).ToString() + " - " + spouse.GetDate(IndividualEventClass.EventType.Death).ToString();

                        ctrl2.SetParent(this);

                        //ctrl2.FlatStyle = FlatStyle.Flat;
                        //ctrl2.individual = spouse;
                        //ctrl2.Click += new System.EventHandler(ctrl2.Clicked);
                        //ctrl2.MouseEnter += MouseEntered;
                        //ctrl2.MouseLeave += MouseLeft;

                        controlList.Add(ctrl2);

                        this.Controls.Add(ctrl2);

                        position.X += ctrl2.Width + 10;
                        ctrlHeight = ctrl2.Height;
                        trace.TraceInformation(" spouse: AddControl:" + spouse.GetName() + " X:" + position.X + " Y:" + position.Y + " Top:" + ctrl2.Top + " Height:" + ctrl2.Height);

                      }
                      else
                      {
                        trace.TraceEvent(TraceEventType.Error, 0, "Error not a vaild person xref:" + spouseXref.GetXrefName());

                      }
                    }
                  }
                }
              }
              else
              {
                trace.TraceEvent(TraceEventType.Error, 0, "Error not a vaild family xref:" + spouseFamilyXref.GetXrefName());

              }
            }
            if (spouseList != null)
            {
              foreach (FamilyXrefClass family in spouseList)
              {
                FamilyClass familyObject = familyTree.GetFamily(family.GetXrefName());

                if (familyObject != null)
                {
                  IList<IndividualXrefClass> childXrefList = familyTree.GetFamily(family.GetXrefName()).GetChildList();

                  if (childXrefList != null)
                  {
                    trace.TraceInformation(" selectedFamily->childFamily:" + family.GetXrefName() + " children.count:" + childXrefList.Count);
                    {
                      Label label = new Label();

                      Point childPosition = familyPosition[family.GetXrefName()];

                      label.Top = childPosition.Y;
                      label.Left = childPosition.X;
                      label.Text = "Children:" + childXrefList.Count;
                      label.AutoSize = true;

                      this.Controls.Add(label);
                      controlList.Add(label);

                      childPosition.Y += label.Height;
                      familyPosition[family.GetXrefName()] = childPosition;
                    }
                    foreach (IndividualXrefClass childXref in childXrefList)
                    {
                      IndividualClass child = new IndividualClass();

                      child = familyTree.GetIndividual(childXref.GetXrefName());

                      if (child != null)
                      {
                        IndividualButton ctrl2 = new IndividualButton(child);

                        ctrl2.AutoSize = true;
                        Point childPosition = familyPosition[family.GetXrefName()];
                        ctrl2.Left = childPosition.X;
                        ctrl2.Top = childPosition.Y;
                        ctrl2.FlatStyle = FlatStyle.Flat;
                        ctrl2.Click += new System.EventHandler(ctrl2.Clicked);
                        //ctrl2.MouseEnter += MouseEntered;
                        //ctrl2.MouseLeave += MouseLeft;

                        //ctrl2.Text = child.GetName() + "\n" + child.GetDate(IndividualEventClass.EventType.Birth).ToString() + " - " + child.GetDate(IndividualEventClass.EventType.Death).ToString();
                        ctrl2.individual = child;
                        ctrl2.SetParent(this);

                        controlList.Add(ctrl2);

                        this.Controls.Add(ctrl2);

                        trace.TraceInformation(" child: AddControl:" + child.GetName() + " X:" + position.X + " Y:" + position.Y + " Top:" + ctrl2.Top + " Height:" + ctrl2.Height);

                        childPosition.Y += ctrl2.Height;
                        familyPosition[family.GetXrefName()] = childPosition;
                        ctrlHeight += ctrl2.Height;
                      }
                    }
                  }
                  else
                  {
                    trace.TraceInformation(" selectedFamily->childFamily:" + family.GetXrefName() + " children null");
                  }

                }
                else
                {
                  trace.TraceInformation(" selectedFamily:" + family.GetXrefName() + " null");
                }

              }
            }
          }
          else
          {
            trace.TraceInformation(" selected->spouseFamily null");
          }
        }




      }
      this.Top = 0;
      this.Left = 0;

      this.Width = 600;
      this.Height = 600;


      //this.Show();
      trace.TraceInformation("TreeViewPanel2::ShowActiveFamily (end) ");
    }

    public void MouseEntered_not(object sender, EventArgs e)
    {
      trace.TraceInformation("person Enter");
      /*details.Text = "person test2";// individual.GetName();
      details.AutoSize = true;
      details.BorderStyle = BorderStyle.Fixed3D;
      details.Top = this.Top;
      details.Left = this.Right;
      //details.
      details.Show();*/
      //this.Container.Add(details);
      //throw new NotImplementedException();
    }
    public void MouseLeft_not(object sender, EventArgs e)
    {
      trace.TraceInformation("person Leave");
      //throw new NotImplementedException();
      //details.Hide();
    }


    /*public void SetParentForm(FamilyForm2 parentForm)
    {
      this.GetTopLevel.parentForm = parentForm;
    }*/


  }
}

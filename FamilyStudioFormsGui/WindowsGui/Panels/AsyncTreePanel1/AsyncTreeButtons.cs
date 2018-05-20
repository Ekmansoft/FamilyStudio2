using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
using System.Windows.Forms;
using FamilyStudioData.FamilyData;

namespace FamilyStudioFormsGui.WindowsGui.Panels.AsyncTreePanel1
{
  class IndividualButton : Button
  {
    static TraceSource trace = new TraceSource("AsyncIndividualButton", SourceLevels.Warning);
    public IndividualClass individual;
    private AsyncTreePanel1 parent;
    private ToolTip details;
    private FamilyButton parentButton;
    public ButtonLayout bLayout;
    private string xref;

    public IndividualButton(string xref, IndividualClass individual = null)
    {
      Visible = false;
      this.xref = xref;
      this.individual = individual;
      bLayout = new ButtonLayout();

      FlatStyle = FlatStyle.Flat;
      Anchor = AnchorStyles.Left | AnchorStyles.Top;
      Click += new System.EventHandler(Clicked);
      this.MouseUp += IndividualButton_MouseUp;

      details = new ToolTip();
      UpdateData();
      ClearLayout();
    }
    public bool CheckPosition(bool force = false)
    {
      if (!Visible)
      {
        trace.TraceData(TraceEventType.Information, 0, "Not visible person:" + xref + " act:" + Location + " bLay" + bLayout.position);
        return false;
      }
      Point reqPos = bLayout.position;
      Point actPos = Location;
      if ((this.Parent != null) && (this.Parent.GetType() == typeof(Panel)))
      {
        Panel parentPanel = (Panel)this.Parent;
        if (parentPanel.AutoScroll)
        {
          reqPos.Offset(parentPanel.AutoScrollPosition);
        }
      }
      if ((reqPos.X != actPos.X) || (reqPos.X != actPos.X) || force)
      {
        trace.TraceData(TraceEventType.Warning, 0, "Has moved:" + xref + " req:" + reqPos + "act:" + Location + "bLay" + bLayout.position);
        if ((this.Parent != null) && (this.Parent.GetType() == typeof(Panel)))
        {
          Panel parentPanel = (Panel)this.Parent;
          if (parentPanel.AutoScroll)
          {
            trace.TraceData(TraceEventType.Warning, 0, "autoscroll" + parentPanel.AutoScrollPosition);
          }
        }
        //SetPosition(bLayout.position);
        //trace.TraceData(TraceEventType.Warning, 0, "Upd:" + xref + " req:" + reqPos + "act:" + Location + "bLay" + bLayout.position);
        return false;
      }
      if (parent != null)
      {

      }
      return true;
    }
    void UpdateData()
    {
      string newText;
      if (individual != null)
      {
        newText = individual.GetName() + "\n" + individual.GetDate(IndividualEventClass.EventType.Birth).ToString() + " - " + individual.GetDate(IndividualEventClass.EventType.Death).ToString();
        IndividualClass.IndividualSexType sex = individual.GetSex();

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
      else
      {
        newText = xref + " ...";
      }

      this.Text = newText;
      details.SetToolTip(this, CreateToolString());

    }
    void SetPosition(Point newPosition)
    {
      Location = newPosition;
      /*if ((this.Parent != null) && (this.Parent.GetType() == typeof(Panel)))
      {
        Panel parentPanel = (Panel)this.Parent;
        if (parentPanel.AutoScroll)
        {
          Location.Offset(parentPanel.AutoScrollPosition);
        }
      }*/
    }

    public bool SetLayout(TreeViewLayout layout, FamilyButton parentButton)
    {
      if (Visible)
      {
        return true;
      }

      if (this.parentButton == null)
      {
        this.parentButton = parentButton;
      }
      ButtonLayout parentLayout = null;
      if (parentButton != null)
      {
        parentLayout = parentButton.bLayout;
      }

      if (!layout.AddChildPersonButton(parentLayout, bLayout))
      {
        trace.TraceData(TraceEventType.Error, 0, "warning: AddChildPersonButton failed!");
        return false;
      }
      SetPosition(bLayout.position);
      Size = bLayout.size;
      trace.TraceInformation("SetLayout()-person:" + xref + " " + bLayout.generation + " " + bLayout.position + " " + bLayout.size);
      this.Visible = true;
      return true;
    }
    public void ClearLayout()
    {
      if (this.Visible)
      {
        this.Visible = false;
        this.parentButton = null;
        this.bLayout.childNo = 0;
        this.Refresh();
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
        if(urlList != null)
        {
          foreach(string url in urlList)
          {
            menu.MenuItems.Add(new MenuItem(url, Url_Click));
          }
        }
        menu.Show(this, new Point(0, 0));
      }
    }

    void AddParent_Click(object sender, EventArgs e)
    {
      //parent.AddRelative(AsyncTreePanel1.RelativeType.Parent);
    }
    void AddChild_Click(object sender, EventArgs e)
    {
      //parent.AddRelative(AsyncTreePanel1.RelativeType.Child);
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

      if (individual == null)
      {
        return "Updating..." + xref;
      }
      str = individual.GetName() + " (" + xref + ")\n";

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
        IList<FamilyXrefClass> childList = individual.GetFamilyChildList();
        int childFamilies = 0;

        if (childList != null)
        {
          childFamilies = childList.Count;
        }
        IList<FamilyXrefClass> spouseList = individual.GetFamilySpouseList();
        int spouseFamilies = 0;

        if (spouseList != null)
        {
          spouseFamilies = spouseList.Count;
        }
        str += "\nChild in " + childFamilies + " families and spouse in " + spouseFamilies + " families";
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

    public override void Refresh()
    {
      if (CheckPosition())
      {
        UpdateData();
      }
      base.Refresh();
    }

    public void SetParent(AsyncTreePanel1 inParent)
    {
      parent = inParent;
    }

    public void Clicked(object sender, EventArgs e)
    {
      trace.TraceInformation("Clicked:" + xref);
      if (individual != null)
      {
        if (parent != null)
        {
          parent.ClickPerson(individual);
        }
      }

    }

  }
  class FamilyButton : Button
  {
    static TraceSource trace = new TraceSource("AsyncFamilyButton", SourceLevels.Warning);
    public FamilyClass family;
    private AsyncTreePanel1 parent;
    public ButtonLayout bLayout;
    private IndividualButton parentButton;
    private string xref;
    private ToolTip details;

    public FamilyButton(string xref, FamilyClass family = null)
    {
      Visible = false;
      this.family = family;
      this.xref = xref;
      bLayout = new ButtonLayout();

      FlatStyle = FlatStyle.Flat;
      Anchor = AnchorStyles.Left | AnchorStyles.Top;
      Click += new System.EventHandler(Clicked);
      details = new ToolTip();
      UpdateData();

      ClearLayout();
    }

    public void ClearLayout()
    {
      if (this.Visible)
      {
        this.Visible = false;
        this.parentButton = null;
        this.bLayout.childNo = 0;
        this.Refresh();
      }
    }

    public bool SetLayout(TreeViewLayout layout, IndividualButton parentButton)
    {
      if (Visible)
      {
        return true;
      }

      if (this.parentButton == null)
      {
        this.parentButton = parentButton;
      }
      if (this.parentButton != null)
      {
        ButtonLayout parentButtonLayout = this.parentButton.bLayout;
        if (!layout.AddChildFamilyButton(parentButtonLayout, bLayout))
        {
          trace.TraceInformation("SetLayout()-family" + xref + " " + bLayout.generation + " abort, max gen ");
          return false;
        }

        SetPosition(bLayout.position);
        Size = bLayout.size;
        trace.TraceInformation("SetLayout()-family:" + xref + " " + bLayout.generation + " " + bLayout.position + " " + bLayout.size);
        this.Visible = true;
        return true;
      }
      return false;
    }
    void SetPosition(Point newPosition)
    {
      Location = newPosition;
      /*if ((this.Parent != null) && (this.Parent.GetType() == typeof(Panel)))
      {
        Panel parentPanel = (Panel)this.Parent;
        if (parentPanel.AutoScroll)
        {
          Location.Offset(parentPanel.AutoScrollPosition);
        }
      }*/
    }
    public bool CheckPosition(bool force = false)
    {
      if (!Visible)
      {
        trace.TraceData(TraceEventType.Information, 0, "Not visible family:" + xref + " act:" + Location + " bLay" + bLayout.position);
        return false;
      }
      Point reqPos = bLayout.position;
      Point actPos = Location;
      if ((this.Parent != null) && (this.Parent.GetType() == typeof(Panel)))
      {
        Panel parentPanel = (Panel)this.Parent;
        if (parentPanel.AutoScroll)
        {
          reqPos.Offset(parentPanel.AutoScrollPosition);
        }
      }
      if ((reqPos.X != actPos.X) || (reqPos.X != actPos.X) || force)
      {
        trace.TraceData(TraceEventType.Warning, 0, "Has moved:" + xref + " req:" + reqPos + "act:" + Location + "bLay" + bLayout.position);
        if ((this.Parent != null) && (this.Parent.GetType() == typeof(Panel)))
        {
          Panel parentPanel = (Panel)this.Parent;
          if (parentPanel.AutoScroll)
          {
            trace.TraceData(TraceEventType.Warning, 0, "autoscroll" + parentPanel.AutoScrollPosition);
          }
        }
        //SetPosition(bLayout.position);
        //trace.TraceData(TraceEventType.Warning, 0, "Upd:" + xref + " req:" + reqPos + "act:" + Location + "bLay" + bLayout.position);
        return false;
      }
      return true;
    }

    void UpdateData()
    {
      details.SetToolTip(this, CreateToolString());
    }

    private string CreateToolString()
    {
      string str = xref + "\n";
      if (family != null)
      {
        IndividualEventClass ev = family.GetEvent(IndividualEventClass.EventType.FamMarriage);

        if (ev != null)
        {
          str += "Married ";

          FamilyDateTimeClass date = ev.GetDate();

          if ((date != null) && (date.GetDateType() != FamilyDateTimeClass.FamilyDateType.Unknown))
          {
            str += date.ToString();
          }
          AddressClass address = ev.GetAddress();
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
          str += "\n";
        }
        IList<IndividualXrefClass> childList = family.GetChildList();
        int children = 0;

        if (childList != null)
        {
          children = childList.Count;
        }
        IList<IndividualXrefClass> parentList = family.GetParentList();
        int parents = 0;

        if (parentList != null)
        {
          parents = parentList.Count;
        }
        str += parents + " parents and " + children + " children";
      }

      return str;
    }
    public override void Refresh()
    {
      if (CheckPosition())
      {
        UpdateData();
      }
      base.Refresh();
    }

    public void SetParent(AsyncTreePanel1 inParent)
    {
      parent = inParent;
    }

    public void Clicked(object sender, EventArgs e)
    {
      trace.TraceInformation("Clicked:" + xref);
    }

  }

}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing; // Image
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using System.Windows.Forms;

namespace FamilyStudioFormsGui.WindowsGui.Panels.AsyncTreePanel1
{
  class ButtonLayout
  {
    public Point position;
    public Size size;
    public int childNo;
    public int generation;
  }
  class TreeViewLayout
  {
    static TraceSource trace = new TraceSource("TreeviewLayout", SourceLevels.Warning);
    public enum ViewType
    {
      Ancestors,
      Descendants
    };

    private Point personOffset;
    private Size personSize;
    private Point familyOffset;
    private Size familySize;
    private Point startPersonPosition;
    private ViewType type;
    private int maxGenerations;

    public TreeViewLayout()
    {
      //zoom = 1.0;
      personOffset.X = 10;
      personOffset.Y = 0;
      personSize.Width = 100;
      personSize.Height = 40;
      familySize.Width = 10;
      familySize.Height = 10;
      familyOffset.X = personSize.Width;
      familyOffset.Y = personSize.Height / 2 - familySize.Height / 2;
      startPersonPosition.X = 10;
      startPersonPosition.Y = 0;
      type = ViewType.Ancestors;
      maxGenerations = 3;
    }
    public bool IsEqual(TreeViewLayout layout)
    {
      if(layout == null)
      {
        return false;
      }
      return (this.type == layout.type) && (this.GetGenerations() == layout.GetGenerations());
    }
    public void SetGenerations(int gen)
    {
      maxGenerations = gen;
    }
    public int GetGenerations()
    {
      return maxGenerations;
    }
    public bool SearchChildren()
    {
      switch (type)
      {
        case ViewType.Ancestors:
          return false;
        case ViewType.Descendants:
          return true;
      }
      return false;
    }
    public TreeViewLayout Clone()
    {
      return (TreeViewLayout)this.MemberwiseClone();
    }
    public bool SearchParents()
    {
      switch (type)
      {
        case ViewType.Ancestors:
          return true;
        case ViewType.Descendants:
          return false;
      }
      return false;
    }
    public bool VisibleButton(ButtonLayout bLayout)
    {
      return (bLayout.generation < this.maxGenerations);
    }
    public bool AddChildFamilyButton(ButtonLayout parentButtonLayout, ButtonLayout bLayout)
    {
      Point retPoint = new Point();
      int genDiff = this.maxGenerations - parentButtonLayout.generation;

      Point parentPosition = parentButtonLayout.position;
      retPoint.Offset(parentPosition);
      retPoint.Offset(familyOffset);
      parentButtonLayout.childNo++;
      bLayout.generation = parentButtonLayout.generation;
      bLayout.size = this.familySize;
      bLayout.position = retPoint;
      trace.TraceInformation("AddChildFamilyButton()g:" + bLayout.generation + " " + parentButtonLayout.childNo + " " + parentPosition + " " + retPoint);
      return true;
    }
    public bool AddChildPersonButton(ButtonLayout parentButtonLayout, ButtonLayout bLayout)
    {
      Point retPoint = new Point();
      if (parentButtonLayout != null)
      {
        int genOffset = (this.maxGenerations - parentButtonLayout.generation);

        if (genOffset < 0)
        {
          return false;
        }
        double yOffset = Math.Pow(2, genOffset - 2) * (2 * parentButtonLayout.childNo - 1) * this.personSize.Height;

        Point parentPosition = parentButtonLayout.position;
        retPoint.Offset(parentPosition);
        retPoint.Offset(personOffset);
        retPoint.Y += (int)yOffset - this.familyOffset.Y;
        bLayout.generation = parentButtonLayout.generation + 1;
        trace.TraceInformation("AddChildPersonButton()g:" + bLayout.generation + " ch:" + parentButtonLayout.childNo + " pos:" + parentPosition + " yOffset:" + yOffset + "=>" + retPoint);
        parentButtonLayout.childNo++;
      }
      else
      {
        double startY = Math.Pow(2, this.maxGenerations - 1) * this.personSize.Height;
        retPoint.Offset(startPersonPosition);
        retPoint.Y += (int)startY;
        trace.TraceInformation("AddChildPersonButton()root:" + bLayout.generation + "=>" + retPoint);
        bLayout.generation = 0;
      }
      bLayout.size = this.personSize;
      bLayout.position = retPoint;
      return true;
    }
  }
}

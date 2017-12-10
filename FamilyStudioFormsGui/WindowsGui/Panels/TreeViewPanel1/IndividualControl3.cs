using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FamilyStudioData.FamilyData;
using FamilyStudioFormsGui.WindowsGui.Forms;

namespace FamilyStudioFormsGui.WindowsGui.Controls
{
  public partial class IndividualControl3 : UserControl
  {
    private static TraceSource trace = new TraceSource("IndividualControl3", SourceLevels.Warning);
    private IndividualClass m_Individual;
    private int m_Generation;
    private int m_Position;
    private bool m_Selected;

    public IndividualControl3()
    {
      //InitializeComponent();
      //trace.TraceInformation("indictrl3: -");
    }
    public IndividualControl3(IndividualClass individual)
    {
      InitializeComponent();
      m_Individual = individual;
      if (m_Individual != null)
      {
        this.Text = m_Individual.GetName();
      }
      else
      {
        this.Text = "-";
      }

      //trace.TraceInformation("indictrl3: " + Text);
      label1.Text = Text;
      m_Selected = false;
    }
    public void SetIndividual(IndividualClass individual)
    {
      m_Individual = individual;

      if (m_Individual != null)
      {
        Text = m_Individual.GetName();
      }
      else
      {
        Text = "";
        this.Width = 10;
        this.Height = 10;
      }
      //trace.TraceInformation("indictrl3: set " + Text);
      label1.Text = Text;
    }
    public void SetPosition(int generation, int position)
    {
      m_Generation = generation;
      m_Position = position;
    }

    public void GetPosition(ref int generation, ref int position)
    {
      generation = m_Generation;
      position = m_Position;

    }

    public void SetSelected(bool selected)
    {
      m_Selected = selected;
      label1.ForeColor = Color.Black;
      this.BackColor = Color.White;
    }

    private void IndividualControl3_MouseLeftButtonDown(object sender, MouseEventArgs e)
    {
      if (trace.Switch.Level.HasFlag(SourceLevels.Information))
      {
        if (m_Individual != null)
        {
          trace.TraceInformation("IndividualControl3_MouseLeftButtonDown():" + m_Individual.GetName());
        }
        else
        {
          trace.TraceInformation("IndividualControl3_MouseLeftButtonDown():null");
        }
        trace.TraceInformation("IndividualControl3_MouseLeftButtonDown():" + this.ToString() + " " + this.Height);
        trace.TraceInformation("IndividualControl3_MouseLeftButtonDown():this.Parent.ToString:" + this.Parent.ToString());
      }

      if (this.Parent.GetType() == typeof(FamilyForm2))
      {
        FamilyForm2 fForm = (FamilyForm2)this.Parent;
        fForm.SetSelectedIndividual(m_Individual.GetXrefName());
        
      }

    }

    public void Print()
    {
      trace.TraceInformation("indictrl::Print: " + Text);
    }

    public String GetId()
    {
      if (m_Individual == null)
      {
        return "-";
      }
      return m_Individual.GetName() + ":" + m_Individual.GetXrefName(); 
    }

    private void IndividualControl3_MouseDown(object sender, MouseEventArgs e)
    {
      if (trace.Switch.Level.HasFlag(SourceLevels.Information))
      {
        if (m_Individual != null)
        {
          trace.TraceInformation("IndividualControl3_MouseDown():" + m_Individual.GetName());
        }
        else
        {
          trace.TraceInformation("IndividualControl3_MouseDown():null");
        }
        trace.TraceInformation("IndividualControl3_MouseDown():" + this.ToString() + " " + this.Height);
        trace.TraceInformation("IndividualControl3_MouseDown():this.Parent.ToString:" + this.Parent.ToString());
      }
      if (this.Parent.GetType() == typeof(FamilyForm2))
      {
        FamilyForm2 fForm = (FamilyForm2)this.Parent;
        fForm.SetSelectedIndividual(m_Individual.GetXrefName());

      }

    }

    private void IndividualControl3_Paint(object sender, PaintEventArgs e)
    {
      trace.TraceInformation("IndividualControl3_Paint():" + m_Individual.GetName() + " Pos:{" + this.Left + "," + this.Top + "," + this.Right + "," + this.Bottom + "}");
    }

    private void label1_Click(object sender, EventArgs e)
    {
      trace.TraceInformation("label1_Click():" + this.ToString() + " " + this.Height);
      if (this.Parent.GetType() == typeof(FamilyForm2))
      {
        FamilyForm2 fForm = (FamilyForm2)this.Parent;
        fForm.SetSelectedIndividual(m_Individual.GetXrefName());

      }

    }
  }
}

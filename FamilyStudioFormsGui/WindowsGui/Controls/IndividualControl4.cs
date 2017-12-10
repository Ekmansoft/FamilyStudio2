using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FamilyStudioData.FamilyData;

namespace FamilyStudioFormsGui.WindowsGui.Controls
{
  class IndividualControl4 : Button
  {
    private static TraceSource trace = new TraceSource("IndividualControl4", SourceLevels.Warning);
    IndividualClass m_Individual;
    //Button m_Button;

    public IndividualControl4()
    {
      //m_Button = new Button();
    }
    public IndividualControl4(IndividualClass individual)
    {
      //::InitializeComponent();
      m_Individual = individual;
      Text = m_Individual.GetName();

      trace.TraceInformation("indictrl: " + Text);
    }

    public void SetIndividual(IndividualClass individual)
    {
      m_Individual = individual;
      Text = m_Individual.GetName();
      trace.TraceInformation("indictrl4: set " + Text);
      this.Width = 100;
      this.Height = 100;
    }

    /*private void IndividualControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
      if (m_Individual != null)
      {
        trace.TraceInformation("IndividualControl_MouseLeftButtonDown():" + m_Individual.GetName());
      }
      else
      {
        trace.TraceInformation("IndividualControl_MouseLeftButtonDown():null");
      }
      trace.TraceInformation("IndividualControl_MouseLeftButtonDown():" + this.ToString() + " " + this.ActualHeight);

    }*/

    public void Print()
    {
      trace.TraceInformation("indictrl::Print: " + Text);
    }
  }
}

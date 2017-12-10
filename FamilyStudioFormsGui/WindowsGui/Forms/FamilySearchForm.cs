using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FamilyStudioData.FamilyTreeStore;
using FamilyStudioData.FamilyData;

namespace FamilyStudioFormsGui.WindowsGui.Forms
{
  public partial class FamilySearchForm : Form
  {
    private static TraceSource trace = new TraceSource("FamilySearchForm", SourceLevels.Warning);
    private FamilyTreeStoreBaseClass m_familyTree;
    public FamilySearchForm()
    {
      InitializeComponent();
    }

    private void button1_Click(object sender, EventArgs e)
    {
      String searchString = nameSearchTextBox.Text.ToUpper();
      //m_familyTree.SearchPerson("ekman");

      searchResultListBox.Items.Clear();

      if (m_familyTree != null)
      {
        IEnumerator<IndividualClass> iterator;
        iterator = m_familyTree.SearchPerson(searchString);

        trace.TraceInformation("dialog.ok");

        if (iterator != null)
        {
          while (iterator.MoveNext())
          {
            IndividualClass indi = (IndividualClass)iterator.Current;

            //trace.TraceInformation("iter:[" + indi.GetName() + "]");

            searchResultListBox.Items.Add(indi.GetName());
            //indi.Print();
          }
        }
        else
        {
          trace.TraceInformation("iter=null");
        }
        trace.TraceInformation("done");
      }
      
      //this.Close();

    }

    public void SetFamilyStore(FamilyTreeStoreBaseClass familyTree)
    {
      m_familyTree = familyTree;
    }



    public String GetName()
    {
      return "ekman";
    }

  }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FamilyStudioData.FamilyData;

namespace FamilyStudioFormsGui.WindowsGui.Forms
{
  public partial class FlagsForm : Form
  {
    private IList<IndividualFilterClass> filterList;
    private FamilyForm2 parentForm;
    public FlagsForm(FamilyForm2 parentForm)
    {
      InitializeComponent();
      this.parentForm = parentForm;
      this.filterList = parentForm.filterList;
      RedrawFilterList();
      listView1.Columns.Add("Title");
    }

    private void RedrawFilterList()
    {
      listView1.Items.Clear();
      foreach (IndividualFilterClass filter in filterList)
      {
        listView1.Items.Add(filter.commentTextString);
      }
      listView1.Show();
    }
    private void RemoveButton_Click(object sender, EventArgs e)
    {
      if(listView1.SelectedIndices.Count == 1)
      {
        filterList.RemoveAt(listView1.SelectedIndices[0]);
        RedrawFilterList();
      }
    }

    Boolean AddToFilterList(string filterString)
    {
      foreach(IndividualFilterClass filter in filterList)
      {
        if(filter.commentTextString.Equals(filterString))
        {
          return false;
        }
      }
      IndividualFilterClass newFilter = new IndividualFilterClass();
      newFilter.commentTextString = filterString;
      //ListViewItem item = new ListViewItem(filterString);
      //item.SubItems.AddRange(new string[] { filterString });
      //item.Tag = filterString;
      filterList.Add(newFilter);
      //listView1.Items.Add(newFilter.commentTextString);
      RedrawFilterList();

      return true;
    }

    private void AddButton_Click(object sender, EventArgs e)
    {
      if(filterTextBox.Text.Length > 0)
      {
        if(!AddToFilterList(filterTextBox.Text))
        {
          MessageBox.Show("Error: FIlter already in list!");
        }
      }


    }
    /*public IList<IndividualFilterClass> GetFilterList()
    {
      return filterList;

    }*/

    private void OkButton_Click(object sender, EventArgs e)
    {
      parentForm.filterList = filterList;
      parentForm.SetSelectedIndividual(parentForm.GetSelectedIndividual());
      this.Close();
    }
  }
}

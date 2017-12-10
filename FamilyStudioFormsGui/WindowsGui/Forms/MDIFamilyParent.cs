using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using FamilyStudioData.FamilyTreeStore;
//using FamilyStudioFormsGui.WindowsGui;
using FamilyStudioFormsGui.WindowsGui.Forms;
using System.Deployment.Application;

//using FamilyStudioFormsGui.WindowsGui.Forms;

namespace FamilyStudioFormsGui.WindowsGui.Forms
{
  public partial class MDIFamilyParent : Form
  {
    private static TraceSource trace = new TraceSource("MDIFamilyParent", SourceLevels.Warning);
    private int childFormNumber = 1;
    private IndividualForm individualForm;
    private FamilyForm2 childForm;
    private FamilyUtility utility;
    private object clipboard;
    private string newFamilyTreeStringName = "New Family Tree ";


    public MDIFamilyParent()
    {
      InitializeComponent();

      individualForm = new IndividualForm();

      FamilyForm2 childForm = new FamilyForm2(false);

      string webList = childForm.GetWebTypeList();

      utility = new FamilyUtility();

      if (webList.Length > 0)
      {
        toolStripMenuItemOpenWeb.Visible = true;
        int strPosStart = webList.IndexOf('{');
        int strLength = webList.IndexOf('}') - 1;

        while ((strPosStart >= 0) && (strPosStart < webList.Length) && (strLength > 0))
        {
          string webName = webList.Substring(strPosStart + 1, strLength);

          trace.TraceInformation("add sub menu:" + webName);

          ToolStripMenuItem newWebItem = new ToolStripMenuItem();

          newWebItem.Name = webName;
          newWebItem.Text = webName;
          //newWebItem.MouseUp += newWebItem_MouseUp;

          newWebItem.Click += newWebItem_Click;

          toolStripMenuItemOpenWeb.DropDownItems.Add(newWebItem);

          if (webList.Substring(strPosStart + strLength).IndexOf('{') >= 0)
          {
            strPosStart = strPosStart + strLength + webList.Substring(strPosStart + strLength).IndexOf('{');
            if ((strPosStart >= 0) && (strPosStart < webList.Length))
            {
              strLength = webList.Substring(strPosStart).IndexOf('}') - 1;
            }
          }
          else
          {
            strPosStart = -1;
          }
        }
      }
      else
      {
        toolStripMenuItemOpenWeb.Visible = false;
      }

    }

    void newWebItem_Click(object sender, EventArgs e)
    {
      trace.TraceInformation("select sub menu:" + sender.ToString());

      childForm = new FamilyForm2();

      /*FamilyForm2*/
      childForm.MdiParent = this;
      childForm.Text = "Family Tree:" + sender.ToString();
      childForm.Show();

      //trace.TraceInformation("strip: " + childForm.statusStrip1.Text + " " + childForm.statusStrip1.ToString() + " " + childForm.statusStrip1.Visible);

      childForm.OpenWeb(sender.ToString());
    }

    private void ShowNewForm(object sender, EventArgs e)
    {
      FamilyForm2 childForm = new FamilyForm2(true);
      childForm.MdiParent = this;
      childForm.Text = newFamilyTreeStringName + childFormNumber++;
      childForm.Show();

    }

    private void OpenFile(object sender, EventArgs e)
    {
      OpenFileDialog openFileDialog = new OpenFileDialog();
      openFileDialog.InitialDirectory = utility.GetCurrentDirectory();
      //openFileDialog.Filter = "Gedcom Files (*.ged)|*.ged|All Files (*.*)|*.*";
      childForm = new FamilyForm2();
      openFileDialog.Filter = childForm.GetFileTypeFilter(FamilyFileTypeOperation.Open);
      
      if (openFileDialog.ShowDialog(this) == DialogResult.OK)
      {
        string FileName = openFileDialog.FileName;

        /*FamilyForm2*/
        childForm.MdiParent = this;
        childForm.Show();

        //trace.TraceInformation("strip: " + childForm.statusStrip1.Text + " " + childForm.statusStrip1.ToString() + " " + childForm.statusStrip1.Visible);

        childForm.OpenFile(FileName);
      }
    }

    private void SaveAsToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (this.ActiveMdiChild != null)
      {
        SaveFileDialog saveFileDialog = new SaveFileDialog();
        saveFileDialog.InitialDirectory = utility.GetCurrentDirectory();
        //saveFileDialog.Filter = "GEDCOM Files (*.ged)|*.ged|All Files (*.*)|*.*";

        childForm = (FamilyForm2)this.ActiveMdiChild;
        saveFileDialog.Filter = childForm.GetFileTypeFilter(FamilyFileTypeOperation.Save);
        if (saveFileDialog.ShowDialog(this) == DialogResult.OK)
        {
          string FileName = saveFileDialog.FileName;
          int index = saveFileDialog.FilterIndex - 1; // convert to zerobased

          ((FamilyForm2)this.ActiveMdiChild).SaveFile(FileName, FamilyFileTypeOperation.Save, index);
        }
      }
    }



    private void ExitToolsStripMenuItem_Click(object sender, EventArgs e)
    {
      this.Close();
    }

    private void CutToolStripMenuItem_Click(object sender, EventArgs e)
    {
    }

    private void CopyToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (this.ActiveMdiChild != null)
      {
        ((FamilyForm2)this.ActiveMdiChild).CopyToClipboard(ref clipboard);
      }
    }

    private void PasteToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (this.ActiveMdiChild != null)
      {
        ((FamilyForm2)this.ActiveMdiChild).PasteFromClipboard(clipboard);
      }
    }

    private void ToolBarToolStripMenuItem_Click(object sender, EventArgs e)
    {
      toolStrip.Visible = toolBarToolStripMenuItem.Checked;
    }

    private void StatusBarToolStripMenuItem_Click(object sender, EventArgs e)
    {
      statusStrip.Visible = statusBarToolStripMenuItem.Checked;
    }

    private void CascadeToolStripMenuItem_Click(object sender, EventArgs e)
    {
      LayoutMdi(MdiLayout.Cascade);
    }

    private void TileVerticalToolStripMenuItem_Click(object sender, EventArgs e)
    {
      LayoutMdi(MdiLayout.TileVertical);
    }

    private void TileHorizontalToolStripMenuItem_Click(object sender, EventArgs e)
    {
      LayoutMdi(MdiLayout.TileHorizontal);
    }

    private void ArrangeIconsToolStripMenuItem_Click(object sender, EventArgs e)
    {
      LayoutMdi(MdiLayout.ArrangeIcons);
    }

    private void CloseAllToolStripMenuItem_Click(object sender, EventArgs e)
    {
      foreach (Form childForm in MdiChildren)
      {
        childForm.Close();
      }
    }

    private void ValidateTree(object sender, EventArgs e)
    {
      if (this.ActiveMdiChild.ActiveControl != null)
      {
        ((FamilyForm2)this.ActiveMdiChild).ValidateTree();
      }

    }
    
    private void AddPerson_Clicked(object sender, EventArgs e)
    {
      trace.TraceInformation("AddPerson_Clicked");

      if (this.ActiveMdiChild != null)
      {
        ((FamilyForm2)this.ActiveMdiChild).AddPerson();
      }

    }

    private void MdiChildActivatedChanged(object sender, EventArgs e)
    {
      if (trace.Switch.Level.HasFlag(SourceLevels.Information))
      {
        if (this.ActiveMdiChild != null)
        {
          trace.TraceInformation("MdiChildActivatedChanged:" + this.ActiveMdiChild.Text);
        }
        else
        {
          trace.TraceInformation("MdiChildActivatedChanged:null");
        }
      }

    }

    private void importToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (this.ActiveMdiChild != null)
      {
        OpenFileDialog openFileDialog = new OpenFileDialog();
        openFileDialog.InitialDirectory = utility.GetCurrentDirectory();
        //openFileDialog.Filter = "Gedcom Files (*.ged)|*.ged|All Files (*.*)|*.*";
        childForm = (FamilyForm2)this.ActiveMdiChild;
        openFileDialog.Filter = childForm.GetFileTypeFilter(FamilyFileTypeOperation.Import);

        if (openFileDialog.ShowDialog(this) == DialogResult.OK)
        {
          string FileName = openFileDialog.FileName;

          /*FamilyForm2*/
          childForm.MdiParent = this;
          childForm.Show();

          //trace.TraceInformation("strip: " + childForm.statusStrip1.Text + " " + childForm.statusStrip1.ToString() + " " + childForm.statusStrip1.Visible);

          childForm.ImportFile(FileName);
          if (childForm.Text.IndexOf(newFamilyTreeStringName) >= 0)
          {
            int filenameStart = FileName.LastIndexOf('\\');
            if ((filenameStart >= 0) && (filenameStart < (FileName.Length - 1)))
            {
              FileName = FileName.Substring(FileName.LastIndexOf('\\') + 1);
            }
            childForm.Text += "; " + FileName;
          }
        }
      }
    }

    private void toolStripMenuItemExport_Click(object sender, System.EventArgs e)
    {
      if (this.ActiveMdiChild != null)
      {
        SaveFileDialog saveFileDialog = new SaveFileDialog();
        saveFileDialog.InitialDirectory = utility.GetCurrentDirectory();
        //saveFileDialog.Filter = "GEDCOM Files (*.ged)|*.ged|All Files (*.*)|*.*";

        childForm = (FamilyForm2)this.ActiveMdiChild;
        saveFileDialog.Filter = childForm.GetFileTypeFilter(FamilyFileTypeOperation.Export);
        if (saveFileDialog.ShowDialog(this) == DialogResult.OK)
        {
          string FileName = saveFileDialog.FileName;

          ((FamilyForm2)this.ActiveMdiChild).SaveFile(FileName, FamilyFileTypeOperation.Export, saveFileDialog.FilterIndex - 1);
        }
      }
    }


    private void CompareTreesMenuItem_Click(object sender, EventArgs e)
    {
      TreeCompareForm compareForm = new TreeCompareForm(MdiChildren, this);
      compareForm.MdiParent = this;
      compareForm.Show();
      

    }

    private void SetHomePerson_Clicked(object sender, EventArgs e)
    {
      if (this.ActiveMdiChild.ActiveControl != null)
      {
        ((FamilyForm2)this.ActiveMdiChild).SetHomePerson();
      }

    }

    private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
    {
      //MessageBox.Show("Copyright endian.net / Kenneth Ekman 2014", "FamilyStudio 0.9");

      //System.Reflection.Assembly assembly1 = System.Reflection.Assembly.GetExecutingAssembly();

      trace.TraceData(TraceEventType.Error, 0, "GetExecutingAssembly:" + System.Reflection.Assembly.GetExecutingAssembly());
      trace.TraceData(TraceEventType.Error, 0, "GetCallingAssembly:" + System.Reflection.Assembly.GetCallingAssembly());
      trace.TraceData(TraceEventType.Error, 0, "GetEntryAssembly:" + System.Reflection.Assembly.GetEntryAssembly());

      string version = "1.0.0.dev";
      try
      {
        ApplicationDeployment deply = ApplicationDeployment.CurrentDeployment;
        version = deply.CurrentVersion.ToString();
      }
      catch (InvalidDeploymentException exc)
      {
        trace.TraceData(TraceEventType.Error, 0, exc.ToString());
        version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() + "-dev";
      }

      MessageBox.Show("Copyright endian.net / Kenneth Ekman 2014-2016", "FamilyStudio " + version);

    }

    private void flagsToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (this.ActiveMdiChild != null)
      {
        FamilyForm2 activeForm = (FamilyForm2)this.ActiveMdiChild;

        FlagsForm flagsForm = new FlagsForm(activeForm);

        //flagsForm.filterList = this.ActiveMdiChild.filterList;

        flagsForm.Show();

        //activeForm.filterList = flagsForm.GetFilterList();

        // Force redraw of controls..
        //activeForm.SetSelectedIndividual(activeForm.GetSelectedIndividual());


      }
    }
  }
}

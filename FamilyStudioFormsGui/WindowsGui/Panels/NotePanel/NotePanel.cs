using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using FamilyStudioFormsGui.WindowsGui.Forms;
using FamilyStudioFormsGui.WindowsGui.Controls;
using FamilyStudioData.FamilyData;
using FamilyStudioData.FamilyTreeStore;
using System.Collections;

namespace FamilyStudioFormsGui.WindowsGui.Panels.NotePanel
{
  class NotePanel : TreeViewPanelBaseClass
  {
    private static TraceSource trace = new TraceSource("NotePanel", SourceLevels.Warning);
    private FamilyTreeStoreBaseClass familyTree;
    private IndividualClass selectedIndividual;
    //private FamilyClass selectedFamily;
    private IList<Control> controlList;
    private FamilyForm2 parentForm;
    private TextBox textBox;


    public NotePanel()
    {
      controlList = new List<Control>();

      this.Dock = DockStyle.Fill;
      parentForm = null;

      textBox = new TextBox();

      textBox.Left = 0;
      textBox.Top = 0;
      //textBox.Right = 200;
      textBox.Size = new System.Drawing.Size(200, 80);
      textBox.Multiline = true;
      textBox.Dock = DockStyle.Fill;
      textBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right;

      this.Controls.Add(textBox);
      trace.TraceInformation("NotePanel::NotePanel()");

    }
    public override void SetParentForm(FamilyForm2 parentForm)
    {
      this.parentForm = parentForm;

      parentForm.SelectedPersonChanged += OnSelectedPersonChangedEvent;

    }

    public override void PasteFromClipboard(object clipboard)
    {
      trace.TraceEvent(TraceEventType.Information, 0, "PasteFromClipboard:" + clipboard);
    }


    public override void SetFamilyTree(FamilyTreeStoreBaseClass inFamilyTree)
    {
      trace.TraceInformation("NotePanel::SetFamilyTree()");

      familyTree = inFamilyTree;

    }
    /*public void SetSelectedIndividual(String xrefName)
    {
      trace.TraceInformation("NotePanel::SetSelectedIndividual(" + xrefName + ")");
    }*/

    public override string GetTitle()
    {
      return "NoteView";
    }


    public override void OnSelectedPersonChangedEvent(object sender, PersonChangeEvent e)
    {
      if (familyTree != null)
      {
        selectedIndividual = e.selectedPerson;
        ShowNote();
      }

    }


    private void ShowNote()
    {
      trace.TraceInformation("NotePanel::ShowNote(start) " + this.CanFocus);
      textBox.Text = "";
      if (selectedIndividual != null)
      {
        IList<NoteClass> noteList = selectedIndividual.GetNoteList();

        if (noteList != null)
        {
          foreach (NoteClass note in noteList)
          {
            if (textBox.Text != "")
            {
              textBox.Text += "\r\n\r\n";
            }
            textBox.Text += note.note;
            trace.TraceInformation("ShowNote:" + note.note);
          }
        }
      }

      trace.TraceInformation("NotePanel::ShowNote(end) ");
    }
  }
}

using System;
using System.Collections.Generic;
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

namespace FamilyStudioFormsGui.WindowsGui.Panels
{
  abstract class TreeViewPanelBaseClass : Panel
  {
    public abstract void SetFamilyTree(FamilyTreeStoreBaseClass inFamilyTree);
    //public abstract void SetSelectedIndividual(String xrefName);
    public abstract void SetParentForm(FamilyForm2 parentForm);
    public abstract string GetTitle();
    public abstract void OnSelectedPersonChangedEvent(object sender, PersonChangeEvent e);
    public abstract void PasteFromClipboard(object clipboard);

  }
}

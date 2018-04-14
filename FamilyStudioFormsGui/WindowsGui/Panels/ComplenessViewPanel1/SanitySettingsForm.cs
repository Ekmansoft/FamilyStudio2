using FamilyStudioData.FamilyTreeStore;
using System;
//using System.Collections.Generic;
using System.ComponentModel;
//using System.Data;
//using System.Drawing;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
using System.Windows.Forms;


namespace FamilyStudioFormsGui.WindowsGui.Panels.CompletenessViewPanel1
{
  public partial class SanitySettingsForm : Form
  {
    //private System.EventHandler eventHandler;
    private SanityCheckLimits limits;
    private SettingsUpdateHandler updateHandler;

    public SanitySettingsForm()
    {
      InitializeComponent();
    }

    public void Update(SanityCheckLimits limits, SettingsUpdateHandler handler)
    {
      this.limits = limits;
      this.updateHandler = handler;

      this.minParentsCheckbox.Checked = limits.parentLimitMin.active;
      this.minParentsNumericUpDown.Value = limits.parentLimitMin.value;

      this.maxMothersCheckBox.Checked = limits.motherLimitMax.active;
      this.maxMothersNumericUpDown.Value = limits.motherLimitMax.value;

      this.maxFathersCheckBox.Checked = limits.fatherLimitMax.active;
      this.maxFathersNumericUpDown.Value = limits.fatherLimitMax.value;

      this.minEventsCheckBox.Checked = limits.eventLimitMin.active;
      this.minEventsNumericUpDown.Value = limits.eventLimitMin.value;

      this.maxEventsCheckBox.Checked = limits.eventLimitMax.active;
      this.maxEventsNumericUpDown.Value = limits.eventLimitMax.value;

      this.minChildrenCheckBox.Checked = limits.noOfChildrenMin.active;
      this.minChildrenNumericUpDown.Value = limits.noOfChildrenMin.value;

      this.maxChildrenCheckBox.Checked = limits.noOfChildrenMax.active;
      this.maxChildrenNumericUpDown.Value = limits.noOfChildrenMax.value;

      this.closeChildrenCheckBox.Checked = limits.daysBetweenChildren.active;
      this.closeChildrenNumericUpDown.Value = limits.daysBetweenChildren.value;

      this.twinsCheckBox.Checked = limits.twins.active;

      this.inexactBirthDateCheckBox.Checked = limits.inexactBirthDeath.active;

      this.unknownBirthDateCheckBox.Checked = limits.unknownBirthDeath.active;

      this.missingParentsCheckBox.Checked = limits.parentsMissing.active;

      this.parentProblemsCheckBox.Checked = limits.parentsProblem.active;

      this.missingWeddingDateCheckBox.Checked = limits.missingWeddingDate.active;

      this.missingPartnerCheckBox.Checked = limits.missingPartner.active;

      this.Show();

    }

    private void ClickOkHandler(object sender, EventArgs e)
    {
      limits.parentLimitMin.active = this.minParentsCheckbox.Checked;
      limits.parentLimitMin.value = (Int32)this.minParentsNumericUpDown.Value;

      limits.motherLimitMax.active = this.maxMothersCheckBox.Checked;
      limits.motherLimitMax.value = (int)this.maxMothersNumericUpDown.Value;

      limits.fatherLimitMax.active = this.maxFathersCheckBox.Checked;
      limits.fatherLimitMax.value = (int)this.maxFathersNumericUpDown.Value;

      limits.eventLimitMin.active = this.minEventsCheckBox.Checked;
      limits.eventLimitMin.value = (int)this.minEventsNumericUpDown.Value;

      limits.eventLimitMax.active = this.maxEventsCheckBox.Checked;
      limits.eventLimitMax.value = (int)this.maxEventsNumericUpDown.Value;

      limits.noOfChildrenMin.active = this.minChildrenCheckBox.Checked;
      limits.noOfChildrenMin.value = (int)this.minChildrenNumericUpDown.Value;

      limits.noOfChildrenMax.active = this.maxChildrenCheckBox.Checked;
      limits.noOfChildrenMax.value = (int)this.maxChildrenNumericUpDown.Value;

      limits.daysBetweenChildren.active = this.closeChildrenCheckBox.Checked;
      limits.daysBetweenChildren.value = (int)this.closeChildrenNumericUpDown.Value;

      limits.twins.active = this.twinsCheckBox.Checked;

      limits.inexactBirthDeath.active = this.inexactBirthDateCheckBox.Checked;

      limits.unknownBirthDeath.active = this.unknownBirthDateCheckBox.Checked;

      limits.parentsMissing.active = this.missingParentsCheckBox.Checked;

      limits.parentsProblem.active = this.parentProblemsCheckBox.Checked;

      limits.missingWeddingDate.active = this.missingWeddingDateCheckBox.Checked;

      limits.missingPartner.active = this.missingPartnerCheckBox.Checked;

      updateHandler(limits);

      this.Close();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
      base.OnClosing(e);
      updateHandler(null);
    }
  }
}

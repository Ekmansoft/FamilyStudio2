namespace FamilyStudioFormsGui.WindowsGui.Forms
{
  partial class AddPersonForm
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.propertyGrid1 = new System.Windows.Forms.PropertyGrid();
      this.label1 = new System.Windows.Forms.Label();
      this.parentRadioButton = new System.Windows.Forms.RadioButton();
      this.childRadioButton = new System.Windows.Forms.RadioButton();
      this.unrelatedRadioButton = new System.Windows.Forms.RadioButton();
      this.saveButton = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // propertyGrid1
      // 
      this.propertyGrid1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.propertyGrid1.Location = new System.Drawing.Point(113, 3);
      this.propertyGrid1.Name = "propertyGrid1";
      this.propertyGrid1.PropertySort = System.Windows.Forms.PropertySort.Categorized;
      this.propertyGrid1.Size = new System.Drawing.Size(485, 307);
      this.propertyGrid1.TabIndex = 0;
      this.propertyGrid1.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.propertyGrid1_PropertyValueChanged);
      this.propertyGrid1.SelectedGridItemChanged += new System.Windows.Forms.SelectedGridItemChangedEventHandler(this.propertyGrid1_SelectedGridItemChanged);
      this.propertyGrid1.SelectedObjectsChanged += new System.EventHandler(this.propertyGrid1_SelectedObjectsChanged);
      this.propertyGrid1.CausesValidationChanged += new System.EventHandler(this.propertyGrid1_CausesValidationChanged);
      this.propertyGrid1.Click += new System.EventHandler(this.propertyGrid1_Click);
      this.propertyGrid1.Enter += new System.EventHandler(this.propertyGrid1_Enter);
      this.propertyGrid1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.propertyGrid1_MouseClick);
      this.propertyGrid1.Validating += new System.ComponentModel.CancelEventHandler(this.propertyGrid1_Validating);
      this.propertyGrid1.Validated += new System.EventHandler(this.propertyGrid1_Validated);
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(13, 13);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(62, 13);
      this.label1.TabIndex = 1;
      this.label1.Text = "Add Person";
      this.label1.Click += new System.EventHandler(this.label1_Click);
      // 
      // parentRadioButton
      // 
      this.parentRadioButton.AutoSize = true;
      this.parentRadioButton.Checked = true;
      this.parentRadioButton.Location = new System.Drawing.Point(16, 44);
      this.parentRadioButton.Name = "parentRadioButton";
      this.parentRadioButton.Size = new System.Drawing.Size(56, 17);
      this.parentRadioButton.TabIndex = 2;
      this.parentRadioButton.TabStop = true;
      this.parentRadioButton.Text = "Parent";
      this.parentRadioButton.UseVisualStyleBackColor = true;
      this.parentRadioButton.CheckedChanged += new System.EventHandler(this.radioButton1_CheckedChanged);
      // 
      // childRadioButton
      // 
      this.childRadioButton.AutoSize = true;
      this.childRadioButton.Location = new System.Drawing.Point(16, 68);
      this.childRadioButton.Name = "childRadioButton";
      this.childRadioButton.Size = new System.Drawing.Size(48, 17);
      this.childRadioButton.TabIndex = 3;
      this.childRadioButton.Text = "Child";
      this.childRadioButton.UseVisualStyleBackColor = true;
      this.childRadioButton.CheckedChanged += new System.EventHandler(this.childRadioButton_CheckedChanged);
      // 
      // unrelatedRadioButton
      // 
      this.unrelatedRadioButton.AutoSize = true;
      this.unrelatedRadioButton.Location = new System.Drawing.Point(16, 91);
      this.unrelatedRadioButton.Name = "unrelatedRadioButton";
      this.unrelatedRadioButton.Size = new System.Drawing.Size(71, 17);
      this.unrelatedRadioButton.TabIndex = 4;
      this.unrelatedRadioButton.Text = "Unrelated";
      this.unrelatedRadioButton.UseVisualStyleBackColor = true;
      // 
      // saveButton
      // 
      this.saveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.saveButton.Location = new System.Drawing.Point(13, 275);
      this.saveButton.Name = "saveButton";
      this.saveButton.Size = new System.Drawing.Size(75, 23);
      this.saveButton.TabIndex = 5;
      this.saveButton.Text = "Save";
      this.saveButton.UseVisualStyleBackColor = true;
      this.saveButton.Click += new System.EventHandler(this.saveButton_OnClick);
      // 
      // AddPersonForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(598, 310);
      this.Controls.Add(this.saveButton);
      this.Controls.Add(this.unrelatedRadioButton);
      this.Controls.Add(this.childRadioButton);
      this.Controls.Add(this.parentRadioButton);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.propertyGrid1);
      this.Name = "AddPersonForm";
      this.Text = "AddPersonForm";
      this.Load += new System.EventHandler(this.AddPersonForm_Load);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.PropertyGrid propertyGrid1;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.RadioButton parentRadioButton;
    private System.Windows.Forms.RadioButton childRadioButton;
    private System.Windows.Forms.RadioButton unrelatedRadioButton;
    private System.Windows.Forms.Button saveButton;
  }
}
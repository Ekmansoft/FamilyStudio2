namespace FamilyStudioFormsGui.WindowsGui.Panels.CompletenessViewPanel1
{
  partial class SanitySettingsForm
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
      this.minParentsNumericUpDown = new System.Windows.Forms.NumericUpDown();
      this.maxMothersNumericUpDown = new System.Windows.Forms.NumericUpDown();
      this.minParentsCheckbox = new System.Windows.Forms.CheckBox();
      this.maxMothersCheckBox = new System.Windows.Forms.CheckBox();
      this.maxFathersCheckBox = new System.Windows.Forms.CheckBox();
      this.maxFathersNumericUpDown = new System.Windows.Forms.NumericUpDown();
      this.minEventsCheckBox = new System.Windows.Forms.CheckBox();
      this.minEventsNumericUpDown = new System.Windows.Forms.NumericUpDown();
      this.maxEventsCheckBox = new System.Windows.Forms.CheckBox();
      this.maxEventsNumericUpDown = new System.Windows.Forms.NumericUpDown();
      this.minChildrenCheckBox = new System.Windows.Forms.CheckBox();
      this.minChildrenNumericUpDown = new System.Windows.Forms.NumericUpDown();
      this.maxChildrenCheckBox = new System.Windows.Forms.CheckBox();
      this.maxChildrenNumericUpDown = new System.Windows.Forms.NumericUpDown();
      this.closeChildrenCheckBox = new System.Windows.Forms.CheckBox();
      this.closeChildrenNumericUpDown = new System.Windows.Forms.NumericUpDown();
      this.twinsCheckBox = new System.Windows.Forms.CheckBox();
      this.inexactBirthDateCheckBox = new System.Windows.Forms.CheckBox();
      this.unknownBirthDateCheckBox = new System.Windows.Forms.CheckBox();
      this.missingParentsCheckBox = new System.Windows.Forms.CheckBox();
      this.parentProblemsCheckBox = new System.Windows.Forms.CheckBox();
      this.okButton = new System.Windows.Forms.Button();
      ((System.ComponentModel.ISupportInitialize)(this.minParentsNumericUpDown)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.maxMothersNumericUpDown)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.maxFathersNumericUpDown)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.minEventsNumericUpDown)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.maxEventsNumericUpDown)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.minChildrenNumericUpDown)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.maxChildrenNumericUpDown)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.closeChildrenNumericUpDown)).BeginInit();
      this.SuspendLayout();
      // 
      // minParentsNumericUpDown
      // 
      this.minParentsNumericUpDown.Location = new System.Drawing.Point(202, 12);
      this.minParentsNumericUpDown.Name = "minParentsNumericUpDown";
      this.minParentsNumericUpDown.Size = new System.Drawing.Size(120, 26);
      this.minParentsNumericUpDown.TabIndex = 0;
      // 
      // maxMothersNumericUpDown
      // 
      this.maxMothersNumericUpDown.Location = new System.Drawing.Point(202, 40);
      this.maxMothersNumericUpDown.Name = "maxMothersNumericUpDown";
      this.maxMothersNumericUpDown.Size = new System.Drawing.Size(120, 26);
      this.maxMothersNumericUpDown.TabIndex = 2;
      // 
      // minParentsCheckbox
      // 
      this.minParentsCheckbox.AutoSize = true;
      this.minParentsCheckbox.Location = new System.Drawing.Point(16, 12);
      this.minParentsCheckbox.Name = "minParentsCheckbox";
      this.minParentsCheckbox.Size = new System.Drawing.Size(141, 24);
      this.minParentsCheckbox.TabIndex = 4;
      this.minParentsCheckbox.Text = "Min parent age";
      this.minParentsCheckbox.UseVisualStyleBackColor = true;
      // 
      // maxMothersCheckBox
      // 
      this.maxMothersCheckBox.AutoSize = true;
      this.maxMothersCheckBox.Location = new System.Drawing.Point(16, 42);
      this.maxMothersCheckBox.Name = "maxMothersCheckBox";
      this.maxMothersCheckBox.Size = new System.Drawing.Size(160, 24);
      this.maxMothersCheckBox.TabIndex = 5;
      this.maxMothersCheckBox.Text = "Max mother\'s age";
      this.maxMothersCheckBox.UseVisualStyleBackColor = true;
      // 
      // maxFathersCheckBox
      // 
      this.maxFathersCheckBox.AutoSize = true;
      this.maxFathersCheckBox.Location = new System.Drawing.Point(16, 72);
      this.maxFathersCheckBox.Name = "maxFathersCheckBox";
      this.maxFathersCheckBox.Size = new System.Drawing.Size(152, 24);
      this.maxFathersCheckBox.TabIndex = 7;
      this.maxFathersCheckBox.Text = "Max father\'s age";
      this.maxFathersCheckBox.UseVisualStyleBackColor = true;
      // 
      // maxFathersNumericUpDown
      // 
      this.maxFathersNumericUpDown.Location = new System.Drawing.Point(202, 70);
      this.maxFathersNumericUpDown.Name = "maxFathersNumericUpDown";
      this.maxFathersNumericUpDown.Size = new System.Drawing.Size(120, 26);
      this.maxFathersNumericUpDown.TabIndex = 6;
      // 
      // minEventsCheckBox
      // 
      this.minEventsCheckBox.AutoSize = true;
      this.minEventsCheckBox.Location = new System.Drawing.Point(16, 102);
      this.minEventsCheckBox.Name = "minEventsCheckBox";
      this.minEventsCheckBox.Size = new System.Drawing.Size(152, 24);
      this.minEventsCheckBox.TabIndex = 9;
      this.minEventsCheckBox.Text = "Min age at event";
      this.minEventsCheckBox.UseVisualStyleBackColor = true;
      // 
      // minEventsNumericUpDown
      // 
      this.minEventsNumericUpDown.Location = new System.Drawing.Point(202, 100);
      this.minEventsNumericUpDown.Maximum = new decimal(new int[] {
            200,
            0,
            0,
            0});
      this.minEventsNumericUpDown.Name = "minEventsNumericUpDown";
      this.minEventsNumericUpDown.Size = new System.Drawing.Size(120, 26);
      this.minEventsNumericUpDown.TabIndex = 8;
      // 
      // maxEventsCheckBox
      // 
      this.maxEventsCheckBox.AutoSize = true;
      this.maxEventsCheckBox.Location = new System.Drawing.Point(16, 132);
      this.maxEventsCheckBox.Name = "maxEventsCheckBox";
      this.maxEventsCheckBox.Size = new System.Drawing.Size(156, 24);
      this.maxEventsCheckBox.TabIndex = 11;
      this.maxEventsCheckBox.Text = "Max age at event";
      this.maxEventsCheckBox.UseVisualStyleBackColor = true;
      // 
      // maxEventsNumericUpDown
      // 
      this.maxEventsNumericUpDown.Location = new System.Drawing.Point(202, 130);
      this.maxEventsNumericUpDown.Maximum = new decimal(new int[] {
            500,
            0,
            0,
            0});
      this.maxEventsNumericUpDown.Name = "maxEventsNumericUpDown";
      this.maxEventsNumericUpDown.Size = new System.Drawing.Size(120, 26);
      this.maxEventsNumericUpDown.TabIndex = 10;
      // 
      // minChildrenCheckBox
      // 
      this.minChildrenCheckBox.AutoSize = true;
      this.minChildrenCheckBox.Location = new System.Drawing.Point(16, 162);
      this.minChildrenCheckBox.Name = "minChildrenCheckBox";
      this.minChildrenCheckBox.Size = new System.Drawing.Size(119, 24);
      this.minChildrenCheckBox.TabIndex = 13;
      this.minChildrenCheckBox.Text = "Min children";
      this.minChildrenCheckBox.UseVisualStyleBackColor = true;
      // 
      // minChildrenNumericUpDown
      // 
      this.minChildrenNumericUpDown.Location = new System.Drawing.Point(202, 160);
      this.minChildrenNumericUpDown.Name = "minChildrenNumericUpDown";
      this.minChildrenNumericUpDown.Size = new System.Drawing.Size(120, 26);
      this.minChildrenNumericUpDown.TabIndex = 12;
      // 
      // maxChildrenCheckBox
      // 
      this.maxChildrenCheckBox.AutoSize = true;
      this.maxChildrenCheckBox.Location = new System.Drawing.Point(16, 192);
      this.maxChildrenCheckBox.Name = "maxChildrenCheckBox";
      this.maxChildrenCheckBox.Size = new System.Drawing.Size(123, 24);
      this.maxChildrenCheckBox.TabIndex = 15;
      this.maxChildrenCheckBox.Text = "Max children";
      this.maxChildrenCheckBox.UseVisualStyleBackColor = true;
      // 
      // maxChildrenNumericUpDown
      // 
      this.maxChildrenNumericUpDown.Location = new System.Drawing.Point(202, 190);
      this.maxChildrenNumericUpDown.Name = "maxChildrenNumericUpDown";
      this.maxChildrenNumericUpDown.Size = new System.Drawing.Size(120, 26);
      this.maxChildrenNumericUpDown.TabIndex = 14;
      // 
      // closeChildrenCheckBox
      // 
      this.closeChildrenCheckBox.AutoSize = true;
      this.closeChildrenCheckBox.Location = new System.Drawing.Point(16, 222);
      this.closeChildrenCheckBox.Name = "closeChildrenCheckBox";
      this.closeChildrenCheckBox.Size = new System.Drawing.Size(181, 24);
      this.closeChildrenCheckBox.TabIndex = 17;
      this.closeChildrenCheckBox.Text = "Close children (days)";
      this.closeChildrenCheckBox.UseVisualStyleBackColor = true;
      // 
      // closeChildrenNumericUpDown
      // 
      this.closeChildrenNumericUpDown.Location = new System.Drawing.Point(202, 220);
      this.closeChildrenNumericUpDown.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
      this.closeChildrenNumericUpDown.Name = "closeChildrenNumericUpDown";
      this.closeChildrenNumericUpDown.Size = new System.Drawing.Size(120, 26);
      this.closeChildrenNumericUpDown.TabIndex = 16;
      // 
      // twinsCheckBox
      // 
      this.twinsCheckBox.AutoSize = true;
      this.twinsCheckBox.Location = new System.Drawing.Point(16, 252);
      this.twinsCheckBox.Name = "twinsCheckBox";
      this.twinsCheckBox.Size = new System.Drawing.Size(75, 24);
      this.twinsCheckBox.TabIndex = 19;
      this.twinsCheckBox.Text = "Twins";
      this.twinsCheckBox.UseVisualStyleBackColor = true;
      // 
      // inexactBirthDateCheckBox
      // 
      this.inexactBirthDateCheckBox.AutoSize = true;
      this.inexactBirthDateCheckBox.Location = new System.Drawing.Point(16, 282);
      this.inexactBirthDateCheckBox.Name = "inexactBirthDateCheckBox";
      this.inexactBirthDateCheckBox.Size = new System.Drawing.Size(158, 24);
      this.inexactBirthDateCheckBox.TabIndex = 21;
      this.inexactBirthDateCheckBox.Text = "Inexact birth date";
      this.inexactBirthDateCheckBox.UseVisualStyleBackColor = true;
      // 
      // unknownBirthDateCheckBox
      // 
      this.unknownBirthDateCheckBox.AutoSize = true;
      this.unknownBirthDateCheckBox.Location = new System.Drawing.Point(16, 312);
      this.unknownBirthDateCheckBox.Name = "unknownBirthDateCheckBox";
      this.unknownBirthDateCheckBox.Size = new System.Drawing.Size(173, 24);
      this.unknownBirthDateCheckBox.TabIndex = 22;
      this.unknownBirthDateCheckBox.Text = "Unknown birth date";
      this.unknownBirthDateCheckBox.UseVisualStyleBackColor = true;
      // 
      // missingParentsCheckBox
      // 
      this.missingParentsCheckBox.AutoSize = true;
      this.missingParentsCheckBox.Location = new System.Drawing.Point(16, 342);
      this.missingParentsCheckBox.Name = "missingParentsCheckBox";
      this.missingParentsCheckBox.Size = new System.Drawing.Size(146, 24);
      this.missingParentsCheckBox.TabIndex = 23;
      this.missingParentsCheckBox.Text = "Missing parents";
      this.missingParentsCheckBox.UseVisualStyleBackColor = true;
      // 
      // parentProblemsCheckBox
      // 
      this.parentProblemsCheckBox.AutoSize = true;
      this.parentProblemsCheckBox.Location = new System.Drawing.Point(16, 372);
      this.parentProblemsCheckBox.Name = "parentProblemsCheckBox";
      this.parentProblemsCheckBox.Size = new System.Drawing.Size(151, 24);
      this.parentProblemsCheckBox.TabIndex = 24;
      this.parentProblemsCheckBox.Text = "Parent problems";
      this.parentProblemsCheckBox.UseVisualStyleBackColor = true;
      // 
      // okButton
      // 
      this.okButton.Location = new System.Drawing.Point(224, 403);
      this.okButton.Name = "okButton";
      this.okButton.Size = new System.Drawing.Size(98, 39);
      this.okButton.TabIndex = 25;
      this.okButton.Text = "OK";
      this.okButton.UseVisualStyleBackColor = true;
      this.okButton.Click += new System.EventHandler(this.ClickOkHandler);
      // 
      // SanitySettingsForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(340, 456);
      this.Controls.Add(this.okButton);
      this.Controls.Add(this.parentProblemsCheckBox);
      this.Controls.Add(this.missingParentsCheckBox);
      this.Controls.Add(this.unknownBirthDateCheckBox);
      this.Controls.Add(this.inexactBirthDateCheckBox);
      this.Controls.Add(this.twinsCheckBox);
      this.Controls.Add(this.closeChildrenCheckBox);
      this.Controls.Add(this.closeChildrenNumericUpDown);
      this.Controls.Add(this.maxChildrenCheckBox);
      this.Controls.Add(this.maxChildrenNumericUpDown);
      this.Controls.Add(this.minChildrenCheckBox);
      this.Controls.Add(this.minChildrenNumericUpDown);
      this.Controls.Add(this.maxEventsCheckBox);
      this.Controls.Add(this.maxEventsNumericUpDown);
      this.Controls.Add(this.minEventsCheckBox);
      this.Controls.Add(this.minEventsNumericUpDown);
      this.Controls.Add(this.maxFathersCheckBox);
      this.Controls.Add(this.maxFathersNumericUpDown);
      this.Controls.Add(this.maxMothersCheckBox);
      this.Controls.Add(this.minParentsCheckbox);
      this.Controls.Add(this.maxMothersNumericUpDown);
      this.Controls.Add(this.minParentsNumericUpDown);
      this.Name = "SanitySettingsForm";
      this.Text = "SanitySettingsForm";
      ((System.ComponentModel.ISupportInitialize)(this.minParentsNumericUpDown)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.maxMothersNumericUpDown)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.maxFathersNumericUpDown)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.minEventsNumericUpDown)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.maxEventsNumericUpDown)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.minChildrenNumericUpDown)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.maxChildrenNumericUpDown)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.closeChildrenNumericUpDown)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.NumericUpDown minParentsNumericUpDown;
    private System.Windows.Forms.NumericUpDown maxMothersNumericUpDown;
    private System.Windows.Forms.CheckBox minParentsCheckbox;
    private System.Windows.Forms.CheckBox maxMothersCheckBox;
    private System.Windows.Forms.CheckBox maxFathersCheckBox;
    private System.Windows.Forms.NumericUpDown maxFathersNumericUpDown;
    private System.Windows.Forms.CheckBox minEventsCheckBox;
    private System.Windows.Forms.NumericUpDown minEventsNumericUpDown;
    private System.Windows.Forms.CheckBox maxEventsCheckBox;
    private System.Windows.Forms.NumericUpDown maxEventsNumericUpDown;
    private System.Windows.Forms.CheckBox minChildrenCheckBox;
    private System.Windows.Forms.NumericUpDown minChildrenNumericUpDown;
    private System.Windows.Forms.CheckBox maxChildrenCheckBox;
    private System.Windows.Forms.NumericUpDown maxChildrenNumericUpDown;
    private System.Windows.Forms.CheckBox closeChildrenCheckBox;
    private System.Windows.Forms.NumericUpDown closeChildrenNumericUpDown;
    private System.Windows.Forms.CheckBox twinsCheckBox;
    private System.Windows.Forms.CheckBox inexactBirthDateCheckBox;
    private System.Windows.Forms.CheckBox unknownBirthDateCheckBox;
    private System.Windows.Forms.CheckBox missingParentsCheckBox;
    private System.Windows.Forms.CheckBox parentProblemsCheckBox;
    private System.Windows.Forms.Button okButton;
  }
}
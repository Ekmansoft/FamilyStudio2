namespace FamilyStudioFormsGui.WindowsGui.Forms
{
    partial class IndividualForm
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
      this.listBox1 = new System.Windows.Forms.ListBox();
      this.preNameTextBox = new System.Windows.Forms.TextBox();
      this.surNameTextBox = new System.Windows.Forms.TextBox();
      this.birthDateDateTimePicker = new System.Windows.Forms.DateTimePicker();
      this.deathDateDateTimePicker = new System.Windows.Forms.DateTimePicker();
      this.saveButton = new System.Windows.Forms.Button();
      this.nameControlPanel = new System.Windows.Forms.Panel();
      this.SuspendLayout();
      // 
      // listBox1
      // 
      this.listBox1.FormattingEnabled = true;
      this.listBox1.Location = new System.Drawing.Point(15, 20);
      this.listBox1.Name = "listBox1";
      this.listBox1.Size = new System.Drawing.Size(302, 719);
      this.listBox1.TabIndex = 0;
      // 
      // preNameTextBox
      // 
      this.preNameTextBox.Location = new System.Drawing.Point(509, 29);
      this.preNameTextBox.Name = "preNameTextBox";
      this.preNameTextBox.Size = new System.Drawing.Size(100, 20);
      this.preNameTextBox.TabIndex = 1;
      // 
      // surNameTextBox
      // 
      this.surNameTextBox.Location = new System.Drawing.Point(511, 60);
      this.surNameTextBox.Name = "surNameTextBox";
      this.surNameTextBox.Size = new System.Drawing.Size(100, 20);
      this.surNameTextBox.TabIndex = 2;
      // 
      // birthDateDateTimePicker
      // 
      this.birthDateDateTimePicker.Location = new System.Drawing.Point(512, 104);
      this.birthDateDateTimePicker.Name = "birthDateDateTimePicker";
      this.birthDateDateTimePicker.Size = new System.Drawing.Size(200, 20);
      this.birthDateDateTimePicker.TabIndex = 3;
      // 
      // deathDateDateTimePicker
      // 
      this.deathDateDateTimePicker.Location = new System.Drawing.Point(512, 146);
      this.deathDateDateTimePicker.Name = "deathDateDateTimePicker";
      this.deathDateDateTimePicker.Size = new System.Drawing.Size(200, 20);
      this.deathDateDateTimePicker.TabIndex = 4;
      // 
      // saveButton
      // 
      this.saveButton.Location = new System.Drawing.Point(509, 235);
      this.saveButton.Name = "saveButton";
      this.saveButton.Size = new System.Drawing.Size(75, 23);
      this.saveButton.TabIndex = 5;
      this.saveButton.Text = "Save";
      this.saveButton.UseVisualStyleBackColor = true;
      this.saveButton.Click += new System.EventHandler(this.SaveButtonClick);
      // 
      // nameControlPanel
      // 
      this.nameControlPanel.Location = new System.Drawing.Point(756, 20);
      this.nameControlPanel.Name = "nameControlPanel";
      this.nameControlPanel.Size = new System.Drawing.Size(321, 390);
      this.nameControlPanel.TabIndex = 7;
      // 
      // IndividualForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(1216, 758);
      this.Controls.Add(this.nameControlPanel);
      this.Controls.Add(this.saveButton);
      this.Controls.Add(this.deathDateDateTimePicker);
      this.Controls.Add(this.birthDateDateTimePicker);
      this.Controls.Add(this.surNameTextBox);
      this.Controls.Add(this.preNameTextBox);
      this.Controls.Add(this.listBox1);
      this.Name = "IndividualForm";
      this.Text = "Form1";
      this.ResumeLayout(false);
      this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.TextBox preNameTextBox;
        private System.Windows.Forms.TextBox surNameTextBox;
        private System.Windows.Forms.DateTimePicker birthDateDateTimePicker;
        private System.Windows.Forms.DateTimePicker deathDateDateTimePicker;
        private System.Windows.Forms.Button saveButton;
        private System.Windows.Forms.Panel nameControlPanel;
    }
}


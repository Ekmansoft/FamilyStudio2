namespace FamilyStudioFormsGui.WindowsGui.Forms
{
  partial class FlagsForm
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
      this.listView1 = new System.Windows.Forms.ListView();
      this.OkButton = new System.Windows.Forms.Button();
      this.AddButton = new System.Windows.Forms.Button();
      this.RemoveButton = new System.Windows.Forms.Button();
      this.filterTextBox = new System.Windows.Forms.TextBox();
      this.SuspendLayout();
      // 
      // listView1
      // 
      this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
      this.listView1.Location = new System.Drawing.Point(0, 0);
      this.listView1.Name = "listView1";
      this.listView1.Size = new System.Drawing.Size(187, 262);
      this.listView1.TabIndex = 0;
      this.listView1.UseCompatibleStateImageBehavior = false;
      // 
      // OkButton
      // 
      this.OkButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.OkButton.Location = new System.Drawing.Point(430, 12);
      this.OkButton.Name = "OkButton";
      this.OkButton.Size = new System.Drawing.Size(75, 23);
      this.OkButton.TabIndex = 1;
      this.OkButton.Text = "OK";
      this.OkButton.UseVisualStyleBackColor = true;
      this.OkButton.Click += new System.EventHandler(this.OkButton_Click);
      // 
      // AddButton
      // 
      this.AddButton.Location = new System.Drawing.Point(193, 12);
      this.AddButton.Name = "AddButton";
      this.AddButton.Size = new System.Drawing.Size(75, 23);
      this.AddButton.TabIndex = 2;
      this.AddButton.Text = "Add";
      this.AddButton.UseVisualStyleBackColor = true;
      this.AddButton.Click += new System.EventHandler(this.AddButton_Click);
      // 
      // RemoveButton
      // 
      this.RemoveButton.Location = new System.Drawing.Point(193, 41);
      this.RemoveButton.Name = "RemoveButton";
      this.RemoveButton.Size = new System.Drawing.Size(75, 23);
      this.RemoveButton.TabIndex = 3;
      this.RemoveButton.Text = "Remove";
      this.RemoveButton.UseVisualStyleBackColor = true;
      this.RemoveButton.Click += new System.EventHandler(this.RemoveButton_Click);
      // 
      // filterTextBox
      // 
      this.filterTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.filterTextBox.Location = new System.Drawing.Point(274, 14);
      this.filterTextBox.Name = "filterTextBox";
      this.filterTextBox.Size = new System.Drawing.Size(150, 20);
      this.filterTextBox.TabIndex = 4;
      // 
      // FlagsForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(517, 261);
      this.Controls.Add(this.filterTextBox);
      this.Controls.Add(this.RemoveButton);
      this.Controls.Add(this.AddButton);
      this.Controls.Add(this.OkButton);
      this.Controls.Add(this.listView1);
      this.Name = "FlagsForm";
      this.Text = "FlagsForm";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.ListView listView1;
    private System.Windows.Forms.Button OkButton;
    private System.Windows.Forms.Button AddButton;
    private System.Windows.Forms.Button RemoveButton;
    private System.Windows.Forms.TextBox filterTextBox;
  }
}
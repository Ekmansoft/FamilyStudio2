namespace FamilyStudioFormsGui.WindowsGui.Forms
{
  partial class FamilySearchForm
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
      this.nameSearchTextBox = new System.Windows.Forms.TextBox();
      this.searchButton = new System.Windows.Forms.Button();
      this.searchResultListBox = new System.Windows.Forms.ListBox();
      this.SuspendLayout();
      // 
      // nameSearchTextBox
      // 
      this.nameSearchTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.nameSearchTextBox.Location = new System.Drawing.Point(0, 0);
      this.nameSearchTextBox.Name = "nameSearchTextBox";
      this.nameSearchTextBox.Size = new System.Drawing.Size(213, 20);
      this.nameSearchTextBox.TabIndex = 0;
      // 
      // searchButton
      // 
      this.searchButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.searchButton.Location = new System.Drawing.Point(211, 0);
      this.searchButton.Name = "searchButton";
      this.searchButton.Size = new System.Drawing.Size(74, 23);
      this.searchButton.TabIndex = 1;
      this.searchButton.Text = "Search";
      this.searchButton.UseVisualStyleBackColor = true;
      this.searchButton.Click += new System.EventHandler(this.button1_Click);
      // 
      // searchResultListBox
      // 
      this.searchResultListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.searchResultListBox.FormattingEnabled = true;
      this.searchResultListBox.Location = new System.Drawing.Point(0, 27);
      this.searchResultListBox.Name = "searchResultListBox";
      this.searchResultListBox.Size = new System.Drawing.Size(285, 238);
      this.searchResultListBox.TabIndex = 2;
      // 
      // FamilySearchForm
      // 
      this.AcceptButton = this.searchButton;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(284, 262);
      this.Controls.Add(this.searchResultListBox);
      this.Controls.Add(this.searchButton);
      this.Controls.Add(this.nameSearchTextBox);
      this.Name = "FamilySearchForm";
      this.Text = "Search";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.TextBox nameSearchTextBox;
    private System.Windows.Forms.Button searchButton;
    private System.Windows.Forms.ListBox searchResultListBox;
  }
}
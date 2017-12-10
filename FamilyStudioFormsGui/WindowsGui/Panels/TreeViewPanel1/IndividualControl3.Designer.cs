namespace FamilyStudioFormsGui.WindowsGui.Controls
{
  partial class IndividualControl3
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

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.label1 = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // label1
      // 
      this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.label1.AutoSize = true;
      this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label1.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
      this.label1.Location = new System.Drawing.Point(0, 0);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(35, 13);
      this.label1.TabIndex = 0;
      this.label1.Text = "label1";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.label1.Click += new System.EventHandler(this.label1_Click);
      // 
      // IndividualControl3
      // 
      this.AccessibleDescription = "Grutt";
      this.AccessibleName = "Fisker";
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.SystemColors.GrayText;
      this.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.Controls.Add(this.label1);
      this.Name = "IndividualControl3";
      this.Size = new System.Drawing.Size(126, 68);
      this.Tag = "Text";
      this.Paint += new System.Windows.Forms.PaintEventHandler(this.IndividualControl3_Paint);
      this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.IndividualControl3_MouseDown);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label label1;
  }
}

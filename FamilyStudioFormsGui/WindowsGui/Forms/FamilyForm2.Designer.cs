namespace FamilyStudioFormsGui.WindowsGui.Forms
{
  partial class FamilyForm2
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
      if (disposing)
      {
        if (components != null)
        {
          components.Dispose();
        }
        if (readFileWorker != null)
        {
          readFileWorker.Dispose();
        }
        if (writeFileWorker != null)
        {
          writeFileWorker.Dispose();
        }
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
      this.statusStrip1 = new System.Windows.Forms.StatusStrip();
      this.toolStripProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
      this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
      this.splitContainer1 = new System.Windows.Forms.SplitContainer();
      this.splitContainer2 = new System.Windows.Forms.SplitContainer();
      this.tabControl1 = new System.Windows.Forms.TabControl();
      this.tabPage1 = new System.Windows.Forms.TabPage();
      this.tabPage2 = new System.Windows.Forms.TabPage();
      this.tabPage3 = new System.Windows.Forms.TabPage();
      this.tabPage4 = new System.Windows.Forms.TabPage();
      this.tabControl2 = new System.Windows.Forms.TabControl();
      this.tabPage5 = new System.Windows.Forms.TabPage();
      this.splitter1 = new System.Windows.Forms.Splitter();
      this.splitter2 = new System.Windows.Forms.Splitter();
      this.statusStrip1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
      this.splitContainer1.Panel2.SuspendLayout();
      this.splitContainer1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
      this.splitContainer2.Panel1.SuspendLayout();
      this.splitContainer2.Panel2.SuspendLayout();
      this.splitContainer2.SuspendLayout();
      this.tabControl1.SuspendLayout();
      this.tabControl2.SuspendLayout();
      this.SuspendLayout();
      // 
      // statusStrip1
      // 
      this.statusStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
      this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripProgressBar1,
            this.toolStripStatusLabel1});
      this.statusStrip1.Location = new System.Drawing.Point(0, 845);
      this.statusStrip1.Name = "statusStrip1";
      this.statusStrip1.Padding = new System.Windows.Forms.Padding(2, 0, 30, 0);
      this.statusStrip1.Size = new System.Drawing.Size(1614, 38);
      this.statusStrip1.TabIndex = 0;
      this.statusStrip1.Text = "statusStrip1";
      // 
      // toolStripProgressBar1
      // 
      this.toolStripProgressBar1.Name = "toolStripProgressBar1";
      // 
      // toolStripStatusLabel1
      // 
      this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
      this.toolStripStatusLabel1.Size = new System.Drawing.Size(151, 33);
      this.toolStripStatusLabel1.Text = "toolStripStatusLabel1";
      // 
      // splitContainer1
      // 
      this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
      this.splitContainer1.Location = new System.Drawing.Point(0, 0);
      this.splitContainer1.Margin = new System.Windows.Forms.Padding(7, 8, 7, 8);
      this.splitContainer1.Name = "splitContainer1";
      this.splitContainer1.Panel1MinSize = 0;
      // 
      // splitContainer1.Panel2
      // 
      this.splitContainer1.Panel2.AutoScroll = true;
      this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
      this.splitContainer1.Panel2.Controls.Add(this.splitter1);
      this.splitContainer1.Panel2MinSize = 75;
      this.splitContainer1.Size = new System.Drawing.Size(1614, 845);
      this.splitContainer1.SplitterDistance = 257;
      this.splitContainer1.SplitterWidth = 9;
      this.splitContainer1.TabIndex = 3;
      // 
      // splitContainer2
      // 
      this.splitContainer2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.splitContainer2.Location = new System.Drawing.Point(7, 0);
      this.splitContainer2.Margin = new System.Windows.Forms.Padding(7, 8, 7, 8);
      this.splitContainer2.Name = "splitContainer2";
      // 
      // splitContainer2.Panel1
      // 
      this.splitContainer2.Panel1.Controls.Add(this.tabControl1);
      // 
      // splitContainer2.Panel2
      // 
      this.splitContainer2.Panel2.Controls.Add(this.tabControl2);
      this.splitContainer2.Size = new System.Drawing.Size(1341, 845);
      this.splitContainer2.SplitterDistance = 633;
      this.splitContainer2.SplitterWidth = 9;
      this.splitContainer2.TabIndex = 2;
      // 
      // tabControl1
      // 
      this.tabControl1.Controls.Add(this.tabPage1);
      this.tabControl1.Controls.Add(this.tabPage2);
      this.tabControl1.Controls.Add(this.tabPage3);
      this.tabControl1.Controls.Add(this.tabPage4);
      this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tabControl1.Font = new System.Drawing.Font("Segoe UI", 9F);
      this.tabControl1.Location = new System.Drawing.Point(0, 0);
      this.tabControl1.Margin = new System.Windows.Forms.Padding(7, 8, 7, 8);
      this.tabControl1.Name = "tabControl1";
      this.tabControl1.SelectedIndex = 0;
      this.tabControl1.Size = new System.Drawing.Size(629, 841);
      this.tabControl1.TabIndex = 1;
      // 
      // tabPage1
      // 
      this.tabPage1.AutoScroll = true;
      this.tabPage1.Location = new System.Drawing.Point(4, 29);
      this.tabPage1.Margin = new System.Windows.Forms.Padding(7, 8, 7, 8);
      this.tabPage1.Name = "tabPage1";
      this.tabPage1.Padding = new System.Windows.Forms.Padding(7, 8, 7, 8);
      this.tabPage1.Size = new System.Drawing.Size(621, 808);
      this.tabPage1.TabIndex = 0;
      this.tabPage1.Text = "Tree-1";
      this.tabPage1.UseVisualStyleBackColor = true;
      // 
      // tabPage2
      // 
      this.tabPage2.AutoScroll = true;
      this.tabPage2.Location = new System.Drawing.Point(4, 29);
      this.tabPage2.Margin = new System.Windows.Forms.Padding(7, 8, 7, 8);
      this.tabPage2.Name = "tabPage2";
      this.tabPage2.Padding = new System.Windows.Forms.Padding(7, 8, 7, 8);
      this.tabPage2.Size = new System.Drawing.Size(619, 804);
      this.tabPage2.TabIndex = 1;
      this.tabPage2.Text = "Tree-2";
      this.tabPage2.UseVisualStyleBackColor = true;
      // 
      // tabPage3
      // 
      this.tabPage3.AutoScroll = true;
      this.tabPage3.Location = new System.Drawing.Point(4, 29);
      this.tabPage3.Margin = new System.Windows.Forms.Padding(7, 8, 7, 8);
      this.tabPage3.Name = "tabPage3";
      this.tabPage3.Padding = new System.Windows.Forms.Padding(7, 8, 7, 8);
      this.tabPage3.Size = new System.Drawing.Size(619, 804);
      this.tabPage3.TabIndex = 2;
      this.tabPage3.Text = "Tree-3";
      this.tabPage3.UseVisualStyleBackColor = true;
      // 
      // tabPage4
      // 
      this.tabPage4.Location = new System.Drawing.Point(4, 29);
      this.tabPage4.Margin = new System.Windows.Forms.Padding(7, 8, 7, 8);
      this.tabPage4.Name = "tabPage4";
      this.tabPage4.Padding = new System.Windows.Forms.Padding(7, 8, 7, 8);
      this.tabPage4.Size = new System.Drawing.Size(619, 804);
      this.tabPage4.TabIndex = 3;
      this.tabPage4.Text = "Images";
      this.tabPage4.UseVisualStyleBackColor = true;
      // 
      // tabControl2
      // 
      this.tabControl2.Controls.Add(this.tabPage5);
      this.tabControl2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tabControl2.Font = new System.Drawing.Font("Segoe UI", 9F);
      this.tabControl2.Location = new System.Drawing.Point(0, 0);
      this.tabControl2.Margin = new System.Windows.Forms.Padding(7, 8, 7, 8);
      this.tabControl2.Name = "tabControl2";
      this.tabControl2.SelectedIndex = 0;
      this.tabControl2.Size = new System.Drawing.Size(695, 841);
      this.tabControl2.TabIndex = 0;
      // 
      // tabPage5
      // 
      this.tabPage5.Location = new System.Drawing.Point(4, 29);
      this.tabPage5.Margin = new System.Windows.Forms.Padding(7, 8, 7, 8);
      this.tabPage5.Name = "tabPage5";
      this.tabPage5.Padding = new System.Windows.Forms.Padding(7, 8, 7, 8);
      this.tabPage5.Size = new System.Drawing.Size(687, 808);
      this.tabPage5.TabIndex = 0;
      this.tabPage5.Text = "tabPage5";
      this.tabPage5.UseVisualStyleBackColor = true;
      // 
      // splitter1
      // 
      this.splitter1.Location = new System.Drawing.Point(0, 0);
      this.splitter1.Margin = new System.Windows.Forms.Padding(7, 8, 7, 8);
      this.splitter1.Name = "splitter1";
      this.splitter1.Size = new System.Drawing.Size(6, 845);
      this.splitter1.TabIndex = 1;
      this.splitter1.TabStop = false;
      // 
      // splitter2
      // 
      this.splitter2.Location = new System.Drawing.Point(0, 0);
      this.splitter2.Margin = new System.Windows.Forms.Padding(7, 8, 7, 8);
      this.splitter2.Name = "splitter2";
      this.splitter2.Size = new System.Drawing.Size(6, 845);
      this.splitter2.TabIndex = 4;
      this.splitter2.TabStop = false;
      // 
      // FamilyForm2
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.AutoScroll = true;
      this.AutoScrollMinSize = new System.Drawing.Size(6, 6);
      this.ClientSize = new System.Drawing.Size(1614, 883);
      this.Controls.Add(this.splitter2);
      this.Controls.Add(this.splitContainer1);
      this.Controls.Add(this.statusStrip1);
      this.Font = new System.Drawing.Font("Segoe UI", 9F);
      this.Margin = new System.Windows.Forms.Padding(7, 8, 7, 8);
      this.Name = "FamilyForm2";
      this.Text = "Family Tree";
      this.statusStrip1.ResumeLayout(false);
      this.statusStrip1.PerformLayout();
      this.splitContainer1.Panel2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
      this.splitContainer1.ResumeLayout(false);
      this.splitContainer2.Panel1.ResumeLayout(false);
      this.splitContainer2.Panel2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
      this.splitContainer2.ResumeLayout(false);
      this.tabControl1.ResumeLayout(false);
      this.tabControl2.ResumeLayout(false);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    public System.Windows.Forms.StatusStrip statusStrip1;
    public System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
    private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar1;
    private System.Windows.Forms.SplitContainer splitContainer1;
    private System.Windows.Forms.SplitContainer splitContainer2;
    private System.Windows.Forms.TabControl tabControl1;
    private System.Windows.Forms.TabPage tabPage1;
    private System.Windows.Forms.TabPage tabPage2;
    private System.Windows.Forms.TabPage tabPage3;
    private System.Windows.Forms.TabPage tabPage4;
    private System.Windows.Forms.Splitter splitter1;
    private System.Windows.Forms.Splitter splitter2;
    private System.Windows.Forms.TabControl tabControl2;
    private System.Windows.Forms.TabPage tabPage5;
  }
}
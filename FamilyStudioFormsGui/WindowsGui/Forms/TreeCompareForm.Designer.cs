namespace FamilyStudioFormsGui.WindowsGui.Forms
{
  partial class TreeCompareForm
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
        if (compareWorker != null)
        {
          compareWorker.Dispose();
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
      this.listBox1 = new System.Windows.Forms.ComboBox();
      this.listBox2 = new System.Windows.Forms.ComboBox();
      this.button1 = new System.Windows.Forms.Button();
      this.matchListView1 = new System.Windows.Forms.ListView();
      this.splitter1 = new System.Windows.Forms.Splitter();
      this.statusStrip1 = new System.Windows.Forms.StatusStrip();
      this.progressBar1 = new System.Windows.Forms.ProgressBar();
      this.SuspendLayout();
      // 
      // listBox1
      // 
      this.listBox1.FormattingEnabled = true;
      this.listBox1.Location = new System.Drawing.Point(0, 0);
      this.listBox1.Name = "listBox1";
      this.listBox1.Size = new System.Drawing.Size(229, 28);
      this.listBox1.TabIndex = 3;
      // 
      // listBox2
      // 
      this.listBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.listBox2.FormattingEnabled = true;
      this.listBox2.Location = new System.Drawing.Point(235, 0);
      this.listBox2.Name = "listBox2";
      this.listBox2.Size = new System.Drawing.Size(426, 28);
      this.listBox2.TabIndex = 4;
      // 
      // button1
      // 
      this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.button1.Location = new System.Drawing.Point(668, 0);
      this.button1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(138, 28);
      this.button1.TabIndex = 5;
      this.button1.Text = "Compare";
      this.button1.UseVisualStyleBackColor = true;
      this.button1.Click += new System.EventHandler(this.button1_Click);
      // 
      // matchListView1
      // 
      this.matchListView1.AllowColumnReorder = true;
      this.matchListView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.matchListView1.FullRowSelect = true;
      this.matchListView1.GridLines = true;
      this.matchListView1.HideSelection = false;
      this.matchListView1.Location = new System.Drawing.Point(0, 29);
      this.matchListView1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
      this.matchListView1.MultiSelect = false;
      this.matchListView1.Name = "matchListView1";
      this.matchListView1.Size = new System.Drawing.Size(804, 749);
      this.matchListView1.TabIndex = 6;
      this.matchListView1.UseCompatibleStateImageBehavior = false;
      this.matchListView1.View = System.Windows.Forms.View.Details;
      // 
      // splitter1
      // 
      this.splitter1.Cursor = System.Windows.Forms.Cursors.HSplit;
      this.splitter1.Dock = System.Windows.Forms.DockStyle.Top;
      this.splitter1.Location = new System.Drawing.Point(0, 0);
      this.splitter1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
      this.splitter1.Name = "splitter1";
      this.splitter1.Size = new System.Drawing.Size(806, 28);
      this.splitter1.TabIndex = 7;
      this.splitter1.TabStop = false;
      // 
      // statusStrip1
      // 
      this.statusStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
      this.statusStrip1.Location = new System.Drawing.Point(0, 790);
      this.statusStrip1.Name = "statusStrip1";
      this.statusStrip1.Padding = new System.Windows.Forms.Padding(2, 0, 21, 0);
      this.statusStrip1.Size = new System.Drawing.Size(806, 22);
      this.statusStrip1.TabIndex = 8;
      this.statusStrip1.Text = "statusStrip1";
      // 
      // progressBar1
      // 
      this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.progressBar1.Location = new System.Drawing.Point(0, 780);
      this.progressBar1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
      this.progressBar1.Name = "progressBar1";
      this.progressBar1.Size = new System.Drawing.Size(150, 33);
      this.progressBar1.TabIndex = 9;
      // 
      // TreeCompareForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(806, 812);
      this.Controls.Add(this.listBox2);
      this.Controls.Add(this.listBox1);
      this.Controls.Add(this.progressBar1);
      this.Controls.Add(this.statusStrip1);
      this.Controls.Add(this.button1);
      this.Controls.Add(this.splitter1);
      this.Controls.Add(this.matchListView1);
      this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
      this.Name = "TreeCompareForm";
      this.Text = "Compare";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.ComboBox listBox1;
    private System.Windows.Forms.ComboBox listBox2;
    private System.Windows.Forms.Button button1;
    private System.Windows.Forms.ListView matchListView1;
    private System.Windows.Forms.Splitter splitter1;
    private System.Windows.Forms.StatusStrip statusStrip1;
    private System.Windows.Forms.ProgressBar progressBar1;
  }
}
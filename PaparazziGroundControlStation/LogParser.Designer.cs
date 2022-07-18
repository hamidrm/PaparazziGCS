namespace PaparazziGroundControlStation
{
    partial class LogParser
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LogParser));
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.toolStripProgressBar = new System.Windows.Forms.ToolStripProgressBar();
            this.toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.zedGraphControl = new ZedGraph.ZedGraphControl();
            this.MessagesList = new System.Windows.Forms.TreeView();
            this.splitContainerLogAnalizer = new System.Windows.Forms.SplitContainer();
            this.statusStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerLogAnalizer)).BeginInit();
            this.splitContainerLogAnalizer.Panel1.SuspendLayout();
            this.splitContainerLogAnalizer.Panel2.SuspendLayout();
            this.splitContainerLogAnalizer.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip
            // 
            this.statusStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Visible;
            this.statusStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripProgressBar,
            this.toolStripStatusLabel});
            this.statusStrip.Location = new System.Drawing.Point(0, 408);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(776, 24);
            this.statusStrip.TabIndex = 2;
            this.statusStrip.Text = "statusStrip1";
            // 
            // toolStripProgressBar
            // 
            this.toolStripProgressBar.Name = "toolStripProgressBar";
            this.toolStripProgressBar.Size = new System.Drawing.Size(100, 18);
            // 
            // toolStripStatusLabel
            // 
            this.toolStripStatusLabel.Name = "toolStripStatusLabel";
            this.toolStripStatusLabel.Size = new System.Drawing.Size(0, 19);
            // 
            // zedGraphControl
            // 
            this.zedGraphControl.BackColor = System.Drawing.SystemColors.Control;
            this.zedGraphControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.zedGraphControl.IsAntiAlias = true;
            this.zedGraphControl.Location = new System.Drawing.Point(0, 0);
            this.zedGraphControl.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.zedGraphControl.Name = "zedGraphControl";
            this.zedGraphControl.ScrollGrace = 0D;
            this.zedGraphControl.ScrollMaxX = 0D;
            this.zedGraphControl.ScrollMaxY = 0D;
            this.zedGraphControl.ScrollMaxY2 = 0D;
            this.zedGraphControl.ScrollMinX = 0D;
            this.zedGraphControl.ScrollMinY = 0D;
            this.zedGraphControl.ScrollMinY2 = 0D;
            this.zedGraphControl.Size = new System.Drawing.Size(500, 408);
            this.zedGraphControl.TabIndex = 1;
            // 
            // MessagesList
            // 
            this.MessagesList.CheckBoxes = true;
            this.MessagesList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MessagesList.Location = new System.Drawing.Point(0, 0);
            this.MessagesList.Name = "MessagesList";
            this.MessagesList.Size = new System.Drawing.Size(272, 408);
            this.MessagesList.TabIndex = 3;
            this.MessagesList.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.MessagesList_AfterCheck);
            // 
            // splitContainerLogAnalizer
            // 
            this.splitContainerLogAnalizer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerLogAnalizer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainerLogAnalizer.Location = new System.Drawing.Point(0, 0);
            this.splitContainerLogAnalizer.Name = "splitContainerLogAnalizer";
            // 
            // splitContainerLogAnalizer.Panel1
            // 
            this.splitContainerLogAnalizer.Panel1.Controls.Add(this.zedGraphControl);
            // 
            // splitContainerLogAnalizer.Panel2
            // 
            this.splitContainerLogAnalizer.Panel2.Controls.Add(this.MessagesList);
            this.splitContainerLogAnalizer.Size = new System.Drawing.Size(776, 408);
            this.splitContainerLogAnalizer.SplitterDistance = 500;
            this.splitContainerLogAnalizer.TabIndex = 4;
            // 
            // LogParser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(776, 432);
            this.Controls.Add(this.splitContainerLogAnalizer);
            this.Controls.Add(this.statusStrip);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "LogParser";
            this.Text = "LogParser";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.LogParser_FormClosing);
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.splitContainerLogAnalizer.Panel1.ResumeLayout(false);
            this.splitContainerLogAnalizer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerLogAnalizer)).EndInit();
            this.splitContainerLogAnalizer.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ZedGraph.ZedGraphControl zedGraphControl;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
        private System.Windows.Forms.TreeView MessagesList;
        private System.Windows.Forms.SplitContainer splitContainerLogAnalizer;
    }
}
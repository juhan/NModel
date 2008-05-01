namespace NModel.Visualization
{
    partial class ModelProgramGraphView
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters", MessageId = "System.Windows.Forms.Control.set_Text(System.String)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters", MessageId = "System.Windows.Forms.ToolStripItem.set_Text(System.String)")]
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.exploreContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.expandToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.showAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.hideOutgoingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.exploreToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.hideReachableToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.selectNextToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectPreviousToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showStateGraphToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exploreContextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // exploreContextMenuStrip
            // 
            this.exploreContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.expandToolStripMenuItem,
            this.toolStripSeparator1,
            this.showAllToolStripMenuItem,
            this.hideOutgoingToolStripMenuItem,
            this.toolStripSeparator2,
            this.exploreToolStripMenuItem,
            this.hideReachableToolStripMenuItem,
            this.toolStripSeparator3,
            this.selectNextToolStripMenuItem,
            this.selectPreviousToolStripMenuItem,
            this.showStateGraphToolStripMenuItem});
            this.exploreContextMenuStrip.Name = "exploreContextMenuStrip";
            this.exploreContextMenuStrip.ShowCheckMargin = true;
            this.exploreContextMenuStrip.ShowImageMargin = false;
            this.exploreContextMenuStrip.Size = new System.Drawing.Size(211, 198);
            this.exploreContextMenuStrip.Text = "View Transitions";
            // 
            // expandToolStripMenuItem
            // 
            this.expandToolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.expandToolStripMenuItem.Name = "expandToolStripMenuItem";
            this.expandToolStripMenuItem.Size = new System.Drawing.Size(210, 22);
            this.expandToolStripMenuItem.Text = "Outgoing Transitions";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(207, 6);
            // 
            // showAllToolStripMenuItem
            // 
            this.showAllToolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.showAllToolStripMenuItem.Name = "showAllToolStripMenuItem";
            this.showAllToolStripMenuItem.ShortcutKeyDisplayString = "Enter";
            this.showAllToolStripMenuItem.Size = new System.Drawing.Size(210, 22);
            this.showAllToolStripMenuItem.Text = "Show Outgoing";
            this.showAllToolStripMenuItem.ToolTipText = "Show all outgoing transitions";
            this.showAllToolStripMenuItem.Click += new System.EventHandler(this.showAllToolStripMenuItem_Click);
            // 
            // hideOutgoingToolStripMenuItem
            // 
            this.hideOutgoingToolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.hideOutgoingToolStripMenuItem.Name = "hideOutgoingToolStripMenuItem";
            this.hideOutgoingToolStripMenuItem.ShortcutKeyDisplayString = "Backspace";
            this.hideOutgoingToolStripMenuItem.Size = new System.Drawing.Size(210, 22);
            this.hideOutgoingToolStripMenuItem.Text = "Hide Outgoing";
            this.hideOutgoingToolStripMenuItem.ToolTipText = "Hide all outgoing transitions";
            this.hideOutgoingToolStripMenuItem.Click += new System.EventHandler(this.hideOutgoingToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(207, 6);
            // 
            // exploreToolStripMenuItem
            // 
            this.exploreToolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.exploreToolStripMenuItem.Name = "exploreToolStripMenuItem";
            this.exploreToolStripMenuItem.ShortcutKeyDisplayString = "Insert";
            this.exploreToolStripMenuItem.Size = new System.Drawing.Size(210, 22);
            this.exploreToolStripMenuItem.Text = "Show Reachable";
            this.exploreToolStripMenuItem.ToolTipText = "Show all reachable transitions";
            this.exploreToolStripMenuItem.Click += new System.EventHandler(this.exploreToolStripMenuItem_Click);
            // 
            // hideReachableToolStripMenuItem
            // 
            this.hideReachableToolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.hideReachableToolStripMenuItem.Name = "hideReachableToolStripMenuItem";
            this.hideReachableToolStripMenuItem.ShortcutKeyDisplayString = "Delete";
            this.hideReachableToolStripMenuItem.Size = new System.Drawing.Size(210, 22);
            this.hideReachableToolStripMenuItem.Text = "Hide Reachable";
            this.hideReachableToolStripMenuItem.ToolTipText = "Hide all reachable transitions";
            this.hideReachableToolStripMenuItem.Click += new System.EventHandler(this.hideReachableToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(207, 6);
            // 
            // selectNextToolStripMenuItem
            // 
            this.selectNextToolStripMenuItem.Name = "selectNextToolStripMenuItem";
            this.selectNextToolStripMenuItem.ShortcutKeyDisplayString = "n";
            this.selectNextToolStripMenuItem.Size = new System.Drawing.Size(210, 22);
            this.selectNextToolStripMenuItem.Text = "Select Next";
            this.selectNextToolStripMenuItem.ToolTipText = "Select next node";
            this.selectNextToolStripMenuItem.Click += new System.EventHandler(this.selectNextToolStripMenuItem_Click);
            // 
            // selectPreviousToolStripMenuItem
            // 
            this.selectPreviousToolStripMenuItem.Name = "selectPreviousToolStripMenuItem";
            this.selectPreviousToolStripMenuItem.ShortcutKeyDisplayString = "p";
            this.selectPreviousToolStripMenuItem.Size = new System.Drawing.Size(210, 22);
            this.selectPreviousToolStripMenuItem.Text = "Select Previous";
            this.selectPreviousToolStripMenuItem.ToolTipText = "Select previous node";
            this.selectPreviousToolStripMenuItem.Click += new System.EventHandler(this.selectPreviousToolStripMenuItem_Click);
            // 
            // showStateGraphToolStripMenuItem
            // 
            this.showStateGraphToolStripMenuItem.Name = "showStateGraphToolStripMenuItem";
            this.showStateGraphToolStripMenuItem.Size = new System.Drawing.Size(210, 22);
            this.showStateGraphToolStripMenuItem.Text = "Show State Graph";
            this.showStateGraphToolStripMenuItem.Click += new System.EventHandler(this.showStateGraphToolStripMenuItem_Click);
            // 
            // ModelProgramGraphView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.Name = "ModelProgramGraphView";
            this.exploreContextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ContextMenuStrip exploreContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem expandToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exploreToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem hideReachableToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem hideOutgoingToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem selectNextToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectPreviousToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showStateGraphToolStripMenuItem;
    }
}

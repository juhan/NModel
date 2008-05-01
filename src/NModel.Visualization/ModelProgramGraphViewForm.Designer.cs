//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
namespace NModel.Visualization
{
    partial class ModelProgramGraphViewForm
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
            this.panelForTestMode = new System.Windows.Forms.Panel();
            this.labelIsCorrectView = new System.Windows.Forms.Label();
            this.buttonNo = new System.Windows.Forms.Button();
            this.buttonYes = new System.Windows.Forms.Button();
            this.menuStripMPV = new System.Windows.Forms.MenuStrip();
            this.toolStripMenuItemFile = new System.Windows.Forms.ToolStripMenuItem();
            this.saveSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.modelProgramGraphView1 = new NModel.Visualization.ModelProgramGraphView();
            this.panelForTestMode.SuspendLayout();
            this.menuStripMPV.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelForTestMode
            // 
            this.panelForTestMode.Controls.Add(this.labelIsCorrectView);
            this.panelForTestMode.Controls.Add(this.buttonNo);
            this.panelForTestMode.Controls.Add(this.buttonYes);
            this.panelForTestMode.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelForTestMode.Location = new System.Drawing.Point(0, 377);
            this.panelForTestMode.Name = "panelForTestMode";
            this.panelForTestMode.Size = new System.Drawing.Size(638, 31);
            this.panelForTestMode.TabIndex = 1;
            this.panelForTestMode.Visible = false;
            // 
            // labelIsCorrectView
            // 
            this.labelIsCorrectView.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.labelIsCorrectView.AutoSize = true;
            this.labelIsCorrectView.Location = new System.Drawing.Point(339, 10);
            this.labelIsCorrectView.Name = "labelIsCorrectView";
            this.labelIsCorrectView.Size = new System.Drawing.Size(125, 13);
            this.labelIsCorrectView.TabIndex = 2;
            this.labelIsCorrectView.Text = "Is the view as expected?";
            // 
            // buttonNo
            // 
            this.buttonNo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonNo.DialogResult = System.Windows.Forms.DialogResult.No;
            this.buttonNo.Location = new System.Drawing.Point(551, 5);
            this.buttonNo.Name = "buttonNo";
            this.buttonNo.Size = new System.Drawing.Size(75, 23);
            this.buttonNo.TabIndex = 1;
            this.buttonNo.Text = "No";
            this.buttonNo.UseVisualStyleBackColor = true;
            // 
            // buttonYes
            // 
            this.buttonYes.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonYes.DialogResult = System.Windows.Forms.DialogResult.Yes;
            this.buttonYes.Location = new System.Drawing.Point(470, 5);
            this.buttonYes.Name = "buttonYes";
            this.buttonYes.Size = new System.Drawing.Size(75, 23);
            this.buttonYes.TabIndex = 0;
            this.buttonYes.Text = "Yes";
            this.buttonYes.UseVisualStyleBackColor = true;
            // 
            // menuStripMPV
            // 
            this.menuStripMPV.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemFile});
            this.menuStripMPV.Location = new System.Drawing.Point(0, 0);
            this.menuStripMPV.Name = "menuStripMPV";
            this.menuStripMPV.Size = new System.Drawing.Size(638, 24);
            this.menuStripMPV.TabIndex = 4;
            this.menuStripMPV.Text = "File";
            // 
            // toolStripMenuItemFile
            // 
            this.toolStripMenuItemFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveSettingsToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.toolStripMenuItemFile.Name = "toolStripMenuItemFile";
            this.toolStripMenuItemFile.Size = new System.Drawing.Size(35, 20);
            this.toolStripMenuItemFile.Text = "File";
            // 
            // saveSettingsToolStripMenuItem
            // 
            this.saveSettingsToolStripMenuItem.Name = "saveSettingsToolStripMenuItem";
            this.saveSettingsToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
            this.saveSettingsToolStripMenuItem.Text = "Save Settings ...";
            this.saveSettingsToolStripMenuItem.Click += new System.EventHandler(this.saveSettingsToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click_1);
            // 
            // modelProgramGraphView1
            // 
            this.modelProgramGraphView1.CustomStateLabelProvider = null;
            this.modelProgramGraphView1.CustomStateTooltipProvider = null;
            this.modelProgramGraphView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.modelProgramGraphView1.InitialTransitions = -1;
            this.modelProgramGraphView1.Location = new System.Drawing.Point(0, 24);
            this.modelProgramGraphView1.Name = "modelProgramGraphView1";
            this.modelProgramGraphView1.Size = new System.Drawing.Size(638, 353);
            this.modelProgramGraphView1.TabIndex = 0;
            // 
            // ModelProgramGraphViewForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(638, 408);
            this.Controls.Add(this.modelProgramGraphView1);
            this.Controls.Add(this.panelForTestMode);
            this.Controls.Add(this.menuStripMPV);
            this.Name = "ModelProgramGraphViewForm";
            this.Text = "ModelProgramGraphViewForm";
            this.panelForTestMode.ResumeLayout(false);
            this.panelForTestMode.PerformLayout();
            this.menuStripMPV.ResumeLayout(false);
            this.menuStripMPV.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ModelProgramGraphView modelProgramGraphView1;
        private System.Windows.Forms.Panel panelForTestMode;
        private System.Windows.Forms.Button buttonYes;
        private System.Windows.Forms.Label labelIsCorrectView;
        private System.Windows.Forms.Button buttonNo;
        private System.Windows.Forms.MenuStrip menuStripMPV;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemFile;
        private System.Windows.Forms.ToolStripMenuItem saveSettingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
    }
}
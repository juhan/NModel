//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
namespace NModel.Visualization
{
    /// <summary>
    /// Provides a component to create a viewer for model program sta5te
    /// </summary>
    partial class StateViewer
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components;

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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters", MessageId = "System.Windows.Forms.ToolStripItem.set_Text(System.String)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters", MessageId = "System.Windows.Forms.Control.set_Text(System.String)")]
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.propertyGrid1 = new System.Windows.Forms.PropertyGrid();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.helpVisibleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.labelSelectedState = new System.Windows.Forms.Label();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // propertyGrid1
            // 
            this.propertyGrid1.ContextMenuStrip = this.contextMenuStrip1;
            this.propertyGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGrid1.Location = new System.Drawing.Point(0, 13);
            this.propertyGrid1.Name = "propertyGrid1";
            this.propertyGrid1.Size = new System.Drawing.Size(264, 198);
            this.propertyGrid1.TabIndex = 2;
            this.propertyGrid1.ToolbarVisible = false;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.helpVisibleToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(190, 26);
            // 
            // helpVisibleToolStripMenuItem
            // 
            this.helpVisibleToolStripMenuItem.Checked = true;
            this.helpVisibleToolStripMenuItem.CheckOnClick = true;
            this.helpVisibleToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.helpVisibleToolStripMenuItem.Name = "helpVisibleToolStripMenuItem";
            this.helpVisibleToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            this.helpVisibleToolStripMenuItem.Text = "Full Term View Visible";
            this.helpVisibleToolStripMenuItem.Click += new System.EventHandler(this.helpVisibleToolStripMenuItem_Click);
            // 
            // labelSelectedState
            // 
            this.labelSelectedState.AutoSize = true;
            this.labelSelectedState.BackColor = System.Drawing.Color.Transparent;
            this.labelSelectedState.Dock = System.Windows.Forms.DockStyle.Top;
            this.labelSelectedState.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSelectedState.Location = new System.Drawing.Point(0, 0);
            this.labelSelectedState.Name = "labelSelectedState";
            this.labelSelectedState.Size = new System.Drawing.Size(91, 13);
            this.labelSelectedState.TabIndex = 3;
            this.labelSelectedState.Text = "Selected State";
            this.labelSelectedState.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // StateViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.InactiveBorder;
            this.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.Controls.Add(this.propertyGrid1);
            this.Controls.Add(this.labelSelectedState);
            this.Name = "StateViewer";
            this.Size = new System.Drawing.Size(264, 211);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PropertyGrid propertyGrid1;
        private System.Windows.Forms.Label labelSelectedState;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem helpVisibleToolStripMenuItem;
    }
}

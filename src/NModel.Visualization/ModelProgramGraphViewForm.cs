//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
//using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace NModel.Visualization
{
    /// <summary>
    /// Used for creating a form that contains the ModelProgramGraphView component.
    /// </summary>
    public partial class ModelProgramGraphViewForm : Form
    {
        /// <summary>
        /// Creates a form containing the ModelProgramGraphView component.
        /// With an optional panel if testMode is true 
        /// that contains Yes/No buttons that close the form 
        /// and return either DialogResult.Yes or DialogResult.No
        /// </summary>
        /// <param name="title">used as the title of the form if title != null</param>
        /// <param name="testMode">if true shows a testpanel</param>
        public ModelProgramGraphViewForm(string title, bool testMode)
        {
            InitializeComponent();
            if (title != null)
                this.Text = title;
            if (testMode)
                this.panelForTestMode.Visible = true;  
        }

        /// <summary>
        /// Creates a form containing the ModelProgramGraphView component.
        /// </summary>
        /// <param name="title">used as the title of the form if title != null</param>
        public ModelProgramGraphViewForm(string title)
        {
            InitializeComponent();
            if (title != null)
                this.Text = title;
        }

        /// <summary>
        /// Gets the ModelProgramGraphView component.
        /// </summary>
        public ModelProgramGraphView View
        {
            get
            {
                return this.modelProgramGraphView1;
            }
        }

        //private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    this.Close();
        //}

        /// <summary>
        /// Called when Save Settings menu item is selected
        /// </summary>
        public event EventHandler OnSaveSettings;

        private void saveSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (OnSaveSettings != null)
                OnSaveSettings(this, e);
        }

        private void exitToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
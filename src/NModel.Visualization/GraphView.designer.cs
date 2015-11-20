//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
namespace NModel.Visualization
{
    partial class GraphView
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        //^ [Microsoft.Contracts.Owned]
        private System.ComponentModel.IContainer/*?*/ components = null;

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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters")]
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
            System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
            System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GraphView));
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this.projectionButton = new System.Windows.Forms.ToolStripDropDownButton();
            this.backButton = new System.Windows.Forms.ToolStripButton();
            this.forwardButton = new System.Windows.Forms.ToolStripButton();
            this.zoomInButton = new System.Windows.Forms.ToolStripButton();
            this.saveToolStripSplitButton = new System.Windows.Forms.ToolStripSplitButton();
            this.saveAsImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsDotToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsFSMToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.zoomOutButton = new System.Windows.Forms.ToolStripButton();
            this.handButton = new System.Windows.Forms.ToolStripButton();
            this.printButton = new System.Windows.Forms.ToolStripButton();
            this.transitionsButton = new System.Windows.Forms.ToolStripButton();
            this.loopsButton = new System.Windows.Forms.ToolStripButton();
            this.actionLabelsButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.combineActionsButton = new System.Windows.Forms.ToolStripButton();
            this.layoutButton = new System.Windows.Forms.ToolStripDropDownButton();
            this.topToBottomToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.leftToRightToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rightToLeftToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bottomToTopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.stateValuesButton = new System.Windows.Forms.ToolStripButton();
            this.propertiesButton = new System.Windows.Forms.ToolStripButton();
            this.progressBar = new System.Windows.Forms.ToolStripProgressBar();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.buttonTimer = new System.Windows.Forms.Timer(this.components);
            this.selectedItemToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.propertyGrid = new System.Windows.Forms.PropertyGrid();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.restoreDefaultsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sheHideHelpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.graphWorker = new System.ComponentModel.BackgroundWorker();
            this.panel1 = new System.Windows.Forms.Panel();
            this.splitterHorizontal = new System.Windows.Forms.Splitter();
            this.imageListProjections = new System.Windows.Forms.ImageList(this.components);
            this.selectNodeContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.selectNextNodeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectPreviousNodeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.panel2 = new System.Windows.Forms.Panel();
            this.viewer = new Microsoft.Msagl.GraphViewerGdi.GViewer();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.stateViewer1 = new NModel.Visualization.StateViewer();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStrip.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.selectNodeContextMenuStrip.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStrip
            // 
            this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.projectionButton,
            this.backButton,
            this.forwardButton,
            toolStripSeparator1,
            this.zoomInButton,
            this.saveToolStripSplitButton,
            this.zoomOutButton,
            this.handButton,
            toolStripSeparator2,
            this.printButton,
            toolStripSeparator3,
            this.transitionsButton,
            this.loopsButton,
            this.actionLabelsButton,
            this.toolStripSeparator4,
            this.combineActionsButton,
            this.layoutButton,
            this.toolStripSeparator5,
            this.stateValuesButton,
            this.propertiesButton,
            this.progressBar,
            this.toolStripSeparator6});
            this.toolStrip.Location = new System.Drawing.Point(0, 0);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.Size = new System.Drawing.Size(719, 25);
            this.toolStrip.TabIndex = 1;
            this.toolStrip.Text = "Tool Strip";
            this.toolStrip.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.toolStrip_ItemClicked);
            // 
            // projectionButton
            // 
            this.projectionButton.AutoSize = false;
            this.projectionButton.AutoToolTip = false;
            this.projectionButton.Image = ((System.Drawing.Image)(resources.GetObject("projectionButton.Image")));
            this.projectionButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.projectionButton.ImageTransparentColor = System.Drawing.Color.Transparent;
            this.projectionButton.Name = "projectionButton";
            this.projectionButton.Size = new System.Drawing.Size(98, 22);
            this.projectionButton.Text = "     State Macine";
            this.projectionButton.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.projectionButton.TextImageRelation = System.Windows.Forms.TextImageRelation.Overlay;
            // 
            // backButton
            // 
            this.backButton.Enabled = false;
            this.backButton.Image = ((System.Drawing.Image)(resources.GetObject("backButton.Image")));
            this.backButton.Name = "backButton";
            this.backButton.Size = new System.Drawing.Size(56, 22);
            this.backButton.Text = "Undo";
            this.backButton.Visible = false;
            this.backButton.Click += new System.EventHandler(this.backButton_Click);
            // 
            // forwardButton
            // 
            this.forwardButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.forwardButton.Enabled = false;
            this.forwardButton.Image = ((System.Drawing.Image)(resources.GetObject("forwardButton.Image")));
            this.forwardButton.Name = "forwardButton";
            this.forwardButton.Size = new System.Drawing.Size(23, 22);
            this.forwardButton.Text = "Redo";
            this.forwardButton.Visible = false;
            this.forwardButton.Click += new System.EventHandler(this.forwardButton_Click);
            // 
            // zoomInButton
            // 
            this.zoomInButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.zoomInButton.Image = ((System.Drawing.Image)(resources.GetObject("zoomInButton.Image")));
            this.zoomInButton.Name = "zoomInButton";
            this.zoomInButton.Size = new System.Drawing.Size(23, 22);
            this.zoomInButton.Text = "Zoom In";
            this.zoomInButton.Click += new System.EventHandler(this.zoomInButton_Click);
            // 
            // saveToolStripSplitButton
            // 
            this.saveToolStripSplitButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.saveToolStripSplitButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveAsImageToolStripMenuItem,
            this.saveAsDotToolStripMenuItem,
            this.saveAsFSMToolStripMenuItem});
            this.saveToolStripSplitButton.Image = ((System.Drawing.Image)(resources.GetObject("saveToolStripSplitButton.Image")));
            this.saveToolStripSplitButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.saveToolStripSplitButton.Name = "saveToolStripSplitButton";
            this.saveToolStripSplitButton.Size = new System.Drawing.Size(32, 22);
            this.saveToolStripSplitButton.Tag = "Image";
            this.saveToolStripSplitButton.Text = "Save as image ...";
            this.saveToolStripSplitButton.ButtonClick += new System.EventHandler(this.saveToolStripSplitButton_ButtonClick);
            // 
            // saveAsImageToolStripMenuItem
            // 
            this.saveAsImageToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("saveAsImageToolStripMenuItem.Image")));
            this.saveAsImageToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.White;
            this.saveAsImageToolStripMenuItem.Name = "saveAsImageToolStripMenuItem";
            this.saveAsImageToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this.saveAsImageToolStripMenuItem.Text = "Save as Image ...";
            this.saveAsImageToolStripMenuItem.ToolTipText = "Save as Image ...";
            this.saveAsImageToolStripMenuItem.Click += new System.EventHandler(this.saveAsImageToolStripMenuItem_Click);
            // 
            // saveAsDotToolStripMenuItem
            // 
            this.saveAsDotToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("saveAsDotToolStripMenuItem.Image")));
            this.saveAsDotToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.White;
            this.saveAsDotToolStripMenuItem.Name = "saveAsDotToolStripMenuItem";
            this.saveAsDotToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this.saveAsDotToolStripMenuItem.Text = "Save as Dot ...";
            this.saveAsDotToolStripMenuItem.ToolTipText = "Save as Dot ...";
            this.saveAsDotToolStripMenuItem.Click += new System.EventHandler(this.saveAsDotToolStripMenuItem_Click);
            // 
            // saveAsFSMToolStripMenuItem
            // 
            this.saveAsFSMToolStripMenuItem.Name = "saveAsFSMToolStripMenuItem";
            this.saveAsFSMToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this.saveAsFSMToolStripMenuItem.Text = "Save as FSM ...";
            this.saveAsFSMToolStripMenuItem.Click += new System.EventHandler(this.saveAsFSMToolStripMenuItem_Click);
            // 
            // zoomOutButton
            // 
            this.zoomOutButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.zoomOutButton.Image = ((System.Drawing.Image)(resources.GetObject("zoomOutButton.Image")));
            this.zoomOutButton.Name = "zoomOutButton";
            this.zoomOutButton.Size = new System.Drawing.Size(23, 22);
            this.zoomOutButton.Text = "Zoom Out";
            this.zoomOutButton.Click += new System.EventHandler(this.zoomOutButton_Click);
            // 
            // handButton
            // 
            this.handButton.CheckOnClick = true;
            this.handButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.handButton.Image = ((System.Drawing.Image)(resources.GetObject("handButton.Image")));
            this.handButton.Name = "handButton";
            this.handButton.Size = new System.Drawing.Size(23, 22);
            this.handButton.Text = "Pan";
            this.handButton.Click += new System.EventHandler(this.handButton_Click);
            // 
            // printButton
            // 
            this.printButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.printButton.Image = ((System.Drawing.Image)(resources.GetObject("printButton.Image")));
            this.printButton.Name = "printButton";
            this.printButton.Size = new System.Drawing.Size(23, 22);
            this.printButton.Text = "&Print";
            this.printButton.Click += new System.EventHandler(this.printButton_Click);
            // 
            // transitionsButton
            // 
            this.transitionsButton.Checked = true;
            this.transitionsButton.CheckOnClick = true;
            this.transitionsButton.CheckState = System.Windows.Forms.CheckState.Checked;
            this.transitionsButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.transitionsButton.Image = ((System.Drawing.Image)(resources.GetObject("transitionsButton.Image")));
            this.transitionsButton.ImageTransparentColor = System.Drawing.Color.Transparent;
            this.transitionsButton.Name = "transitionsButton";
            this.transitionsButton.Size = new System.Drawing.Size(23, 22);
            this.transitionsButton.ToolTipText = "Merge labels";
            this.transitionsButton.Click += new System.EventHandler(this.transitionsButton_Click);
            // 
            // loopsButton
            // 
            this.loopsButton.Checked = true;
            this.loopsButton.CheckOnClick = true;
            this.loopsButton.CheckState = System.Windows.Forms.CheckState.Checked;
            this.loopsButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.loopsButton.Image = ((System.Drawing.Image)(resources.GetObject("loopsButton.Image")));
            this.loopsButton.Name = "loopsButton";
            this.loopsButton.Size = new System.Drawing.Size(23, 22);
            this.loopsButton.Text = "Show Self-Loops";
            this.loopsButton.Click += new System.EventHandler(this.loopsButton_Click);
            // 
            // actionLabelsButton
            // 
            this.actionLabelsButton.Checked = true;
            this.actionLabelsButton.CheckState = System.Windows.Forms.CheckState.Checked;
            this.actionLabelsButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.actionLabelsButton.Image = ((System.Drawing.Image)(resources.GetObject("actionLabelsButton.Image")));
            this.actionLabelsButton.Name = "actionLabelsButton";
            this.actionLabelsButton.Size = new System.Drawing.Size(23, 22);
            this.actionLabelsButton.Text = "Transition Labels";
            this.actionLabelsButton.Click += new System.EventHandler(this.actionLabelsButton_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(6, 25);
            // 
            // combineActionsButton
            // 
            this.combineActionsButton.CheckOnClick = true;
            this.combineActionsButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.combineActionsButton.Image = ((System.Drawing.Image)(resources.GetObject("combineActionsButton.Image")));
            this.combineActionsButton.Name = "combineActionsButton";
            this.combineActionsButton.Size = new System.Drawing.Size(23, 22);
            this.combineActionsButton.Text = "Combine Start and Finish Actions";
            this.combineActionsButton.Click += new System.EventHandler(this.combineActionsButton_Click);
            // 
            // layoutButton
            // 
            this.layoutButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.layoutButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.topToBottomToolStripMenuItem,
            this.leftToRightToolStripMenuItem,
            this.rightToLeftToolStripMenuItem,
            this.bottomToTopToolStripMenuItem});
            this.layoutButton.Image = ((System.Drawing.Image)(resources.GetObject("layoutButton.Image")));
            this.layoutButton.Name = "layoutButton";
            this.layoutButton.Size = new System.Drawing.Size(29, 22);
            this.layoutButton.Text = "Layout Direction";
            // 
            // topToBottomToolStripMenuItem
            // 
            this.topToBottomToolStripMenuItem.Checked = true;
            this.topToBottomToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.topToBottomToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("topToBottomToolStripMenuItem.Image")));
            this.topToBottomToolStripMenuItem.Name = "topToBottomToolStripMenuItem";
            this.topToBottomToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.topToBottomToolStripMenuItem.Text = "Top to Bottom";
            this.topToBottomToolStripMenuItem.Click += new System.EventHandler(this.topToBottomToolStripMenuItem_Click);
            // 
            // leftToRightToolStripMenuItem
            // 
            this.leftToRightToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("leftToRightToolStripMenuItem.Image")));
            this.leftToRightToolStripMenuItem.Name = "leftToRightToolStripMenuItem";
            this.leftToRightToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.leftToRightToolStripMenuItem.Text = "Left to Right";
            this.leftToRightToolStripMenuItem.Click += new System.EventHandler(this.leftToRightToolStripMenuItem_Click);
            // 
            // rightToLeftToolStripMenuItem
            // 
            this.rightToLeftToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("rightToLeftToolStripMenuItem.Image")));
            this.rightToLeftToolStripMenuItem.Name = "rightToLeftToolStripMenuItem";
            this.rightToLeftToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.rightToLeftToolStripMenuItem.Text = "Right to Left";
            this.rightToLeftToolStripMenuItem.Click += new System.EventHandler(this.rightToLeftToolStripMenuItem_Click);
            // 
            // bottomToTopToolStripMenuItem
            // 
            this.bottomToTopToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("bottomToTopToolStripMenuItem.Image")));
            this.bottomToTopToolStripMenuItem.Name = "bottomToTopToolStripMenuItem";
            this.bottomToTopToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.bottomToTopToolStripMenuItem.Text = "Bottom to Top";
            this.bottomToTopToolStripMenuItem.Click += new System.EventHandler(this.bottomToTopToolStripMenuItem_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(6, 25);
            // 
            // stateValuesButton
            // 
            this.stateValuesButton.CheckOnClick = true;
            this.stateValuesButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.stateValuesButton.Image = ((System.Drawing.Image)(resources.GetObject("stateValuesButton.Image")));
            this.stateValuesButton.ImageTransparentColor = System.Drawing.Color.White;
            this.stateValuesButton.Name = "stateValuesButton";
            this.stateValuesButton.Size = new System.Drawing.Size(23, 22);
            this.stateValuesButton.Text = "State Viewer";
            this.stateValuesButton.Click += new System.EventHandler(this.stateValuesButton_Click);
            // 
            // propertiesButton
            // 
            this.propertiesButton.CheckOnClick = true;
            this.propertiesButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.propertiesButton.Image = ((System.Drawing.Image)(resources.GetObject("propertiesButton.Image")));
            this.propertiesButton.Name = "propertiesButton";
            this.propertiesButton.Size = new System.Drawing.Size(23, 22);
            this.propertiesButton.Text = "Advanced Properties";
            this.propertiesButton.Click += new System.EventHandler(this.propertiesButton_Click);
            // 
            // progressBar
            // 
            this.progressBar.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.progressBar.AutoSize = false;
            this.progressBar.Name = "progressBar";
            this.progressBar.Overflow = System.Windows.Forms.ToolStripItemOverflow.Never;
            this.progressBar.Size = new System.Drawing.Size(100, 15);
            this.progressBar.ToolTipText = "Graph Progress";
            this.progressBar.Visible = false;
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(6, 25);
            // 
            // buttonTimer
            // 
            this.buttonTimer.Enabled = true;
            this.buttonTimer.Tick += new System.EventHandler(this.buttonTimer_Tick);
            // 
            // selectedItemToolTip
            // 
            this.selectedItemToolTip.AutoPopDelay = 32767;
            this.selectedItemToolTip.InitialDelay = 1;
            this.selectedItemToolTip.ReshowDelay = 1;
            // 
            // propertyGrid
            // 
            this.propertyGrid.ContextMenuStrip = this.contextMenuStrip1;
            this.propertyGrid.Dock = System.Windows.Forms.DockStyle.Top;
            this.propertyGrid.Location = new System.Drawing.Point(0, 0);
            this.propertyGrid.Name = "propertyGrid";
            this.propertyGrid.SelectedObject = this;
            this.propertyGrid.Size = new System.Drawing.Size(229, 253);
            this.propertyGrid.TabIndex = 2;
            this.propertyGrid.ToolbarVisible = false;
            this.propertyGrid.Visible = false;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.restoreDefaultsToolStripMenuItem,
            this.sheHideHelpToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.ShowImageMargin = false;
            this.contextMenuStrip1.Size = new System.Drawing.Size(137, 48);
            // 
            // restoreDefaultsToolStripMenuItem
            // 
            this.restoreDefaultsToolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.restoreDefaultsToolStripMenuItem.Name = "restoreDefaultsToolStripMenuItem";
            this.restoreDefaultsToolStripMenuItem.Size = new System.Drawing.Size(136, 22);
            this.restoreDefaultsToolStripMenuItem.Text = "RestoreDefaults";
            this.restoreDefaultsToolStripMenuItem.Click += new System.EventHandler(this.restoreDefaultsToolStripMenuItem_Click);
            // 
            // sheHideHelpToolStripMenuItem
            // 
            this.sheHideHelpToolStripMenuItem.Name = "sheHideHelpToolStripMenuItem";
            this.sheHideHelpToolStripMenuItem.Size = new System.Drawing.Size(136, 22);
            this.sheHideHelpToolStripMenuItem.Text = "Show/Hide Help";
            this.sheHideHelpToolStripMenuItem.Click += new System.EventHandler(this.sheHideHelpToolStripMenuItem_Click);
            // 
            // graphWorker
            // 
            this.graphWorker.WorkerReportsProgress = true;
            this.graphWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.graphWorker_DoWork);
            this.graphWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.graphWorker_RunWorkerCompleted);
            this.graphWorker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.graphWorker_ProgressChanged);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.stateViewer1);
            this.panel1.Controls.Add(this.splitterHorizontal);
            this.panel1.Controls.Add(this.propertyGrid);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel1.Location = new System.Drawing.Point(490, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(229, 420);
            this.panel1.TabIndex = 5;
            this.panel1.Visible = false;
            // 
            // splitterHorizontal
            // 
            this.splitterHorizontal.Dock = System.Windows.Forms.DockStyle.Top;
            this.splitterHorizontal.Location = new System.Drawing.Point(0, 253);
            this.splitterHorizontal.Name = "splitterHorizontal";
            this.splitterHorizontal.Size = new System.Drawing.Size(229, 5);
            this.splitterHorizontal.TabIndex = 5;
            this.splitterHorizontal.TabStop = false;
            this.splitterHorizontal.Visible = false;
            // 
            // imageListProjections
            // 
            this.imageListProjections.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListProjections.ImageStream")));
            this.imageListProjections.TransparentColor = System.Drawing.Color.White;
            this.imageListProjections.Images.SetKeyName(0, "root");
            this.imageListProjections.Images.SetKeyName(1, "L");
            this.imageListProjections.Images.SetKeyName(2, "R");
            this.imageListProjections.Images.SetKeyName(3, "LL");
            this.imageListProjections.Images.SetKeyName(4, "LR");
            this.imageListProjections.Images.SetKeyName(5, "RL");
            this.imageListProjections.Images.SetKeyName(6, "RR");
            // 
            // selectNodeContextMenuStrip
            // 
            this.selectNodeContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.selectNextNodeToolStripMenuItem,
            this.selectPreviousNodeToolStripMenuItem});
            this.selectNodeContextMenuStrip.Name = "selectNodeContextMenuStrip";
            this.selectNodeContextMenuStrip.ShowImageMargin = false;
            this.selectNodeContextMenuStrip.ShowItemToolTips = false;
            this.selectNodeContextMenuStrip.Size = new System.Drawing.Size(143, 48);
            this.selectNodeContextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.selectNodeContextMenuStrip_Opening);
            // 
            // selectNextNodeToolStripMenuItem
            // 
            this.selectNextNodeToolStripMenuItem.AutoToolTip = true;
            this.selectNextNodeToolStripMenuItem.Name = "selectNextNodeToolStripMenuItem";
            this.selectNextNodeToolStripMenuItem.ShortcutKeyDisplayString = "n";
            this.selectNextNodeToolStripMenuItem.Size = new System.Drawing.Size(142, 22);
            this.selectNextNodeToolStripMenuItem.Text = "Select Next";
            this.selectNextNodeToolStripMenuItem.ToolTipText = "Select next node";
            this.selectNextNodeToolStripMenuItem.Click += new System.EventHandler(this.selectNextNodeToolStripMenuItem_Click);
            // 
            // selectPreviousNodeToolStripMenuItem
            // 
            this.selectPreviousNodeToolStripMenuItem.AutoToolTip = true;
            this.selectPreviousNodeToolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.selectPreviousNodeToolStripMenuItem.Name = "selectPreviousNodeToolStripMenuItem";
            this.selectPreviousNodeToolStripMenuItem.ShortcutKeyDisplayString = "p";
            this.selectPreviousNodeToolStripMenuItem.Size = new System.Drawing.Size(142, 22);
            this.selectPreviousNodeToolStripMenuItem.Text = "Select Previous";
            this.selectPreviousNodeToolStripMenuItem.ToolTipText = "Select previous node";
            this.selectPreviousNodeToolStripMenuItem.Click += new System.EventHandler(this.selectPreviousNodeToolStripMenuItem_Click);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.viewer);
            this.panel2.Controls.Add(this.splitter1);
            this.panel2.Controls.Add(this.panel1);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 25);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(719, 420);
            this.panel2.TabIndex = 6;
            // 
            // viewer
            // 
            this.viewer.AsyncLayout = false;
            this.viewer.AutoScroll = true;
            this.viewer.BackwardEnabled = false;
            this.viewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.viewer.ForwardEnabled = false;
            this.viewer.Graph = null;
            this.viewer.Location = new System.Drawing.Point(0, 0);
            this.viewer.MouseHitDistance = 0.05;
            this.viewer.Name = "viewer";
            this.viewer.NavigationVisible = true;
            this.viewer.PanButtonPressed = false;
            this.viewer.SaveButtonVisible = true;
            this.viewer.Size = new System.Drawing.Size(485, 420);
            this.viewer.TabIndex = 7;
            this.viewer.ZoomF = 1;
            this.viewer.ZoomFraction = 0.5;
            this.viewer.ZoomWindowThreshold = 0.05;
            this.viewer.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.viewer_MouseWheel);
            this.viewer.Load += new System.EventHandler(this.viewer_Load);
            this.viewer.ObjectUnderMouseCursorChanged += new System.EventHandler<Microsoft.Msagl.Drawing.ObjectUnderMouseCursorChangedEventArgs>(this.viewer_SelectionChanged);
            // 
            // splitter1
            // 
            this.splitter1.Dock = System.Windows.Forms.DockStyle.Right;
            this.splitter1.Location = new System.Drawing.Point(485, 0);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(5, 420);
            this.splitter1.TabIndex = 6;
            this.splitter1.TabStop = false;
            this.splitter1.Visible = false;
            // 
            // stateViewer1
            // 
            this.stateViewer1.BackColor = System.Drawing.SystemColors.InactiveBorder;
            this.stateViewer1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.stateViewer1.DefaultModelName = "Fsm";
            this.stateViewer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.stateViewer1.Location = new System.Drawing.Point(0, 258);
            this.stateViewer1.Margin = new System.Windows.Forms.Padding(4);
            this.stateViewer1.Name = "stateViewer1";
            this.stateViewer1.Size = new System.Drawing.Size(229, 162);
            this.stateViewer1.TabIndex = 4;
            this.stateViewer1.Title = "Selected State";
            this.stateViewer1.TitleVisible = true;
            this.stateViewer1.Visible = false;
            // 
            // GraphView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.toolStrip);
            this.Name = "GraphView";
            this.Size = new System.Drawing.Size(719, 445);
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            this.contextMenuStrip1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.selectNodeContextMenuStrip.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripButton zoomInButton;
        private System.Windows.Forms.ToolStripButton zoomOutButton;
        private System.Windows.Forms.ToolStripButton handButton;
        private System.Windows.Forms.ToolStripButton backButton;
        private System.Windows.Forms.ToolStripButton forwardButton;
        private System.Windows.Forms.ToolStripButton printButton;
        private System.Windows.Forms.Timer buttonTimer;
        private System.Windows.Forms.ToolStripButton loopsButton;
        private System.Windows.Forms.ToolStripDropDownButton layoutButton;
        private System.Windows.Forms.ToolStripMenuItem topToBottomToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem leftToRightToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rightToLeftToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem bottomToTopToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton actionLabelsButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolTip selectedItemToolTip;
        private System.Windows.Forms.PropertyGrid propertyGrid;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripButton propertiesButton;
        private System.Windows.Forms.ToolStripButton stateValuesButton;
        private System.ComponentModel.BackgroundWorker graphWorker;
        private System.Windows.Forms.ToolStripProgressBar progressBar;
        private System.Windows.Forms.ToolStripButton combineActionsButton;
        private StateViewer stateViewer1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Splitter splitterHorizontal;
        private System.Windows.Forms.ToolStripButton transitionsButton;
        private System.Windows.Forms.ToolStripDropDownButton projectionButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ImageList imageListProjections;
        private System.Windows.Forms.ToolStripSplitButton saveToolStripSplitButton;
        private System.Windows.Forms.ToolStripMenuItem saveAsImageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAsDotToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectNextNodeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectPreviousNodeToolStripMenuItem;
        internal System.Windows.Forms.ContextMenuStrip selectNodeContextMenuStrip;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem restoreDefaultsToolStripMenuItem;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.ToolStripMenuItem sheHideHelpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAsFSMToolStripMenuItem;
        internal Microsoft.Msagl.GraphViewerGdi.GViewer viewer;
    }
}

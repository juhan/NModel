//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;
using System.Text;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;
using System.Collections.Generic;

using NModel.Algorithms;
using NModel;
using NModel.Terms;
using Transition = NModel.Triple<NModel.Terms.Term, NModel.Terms.CompoundTerm, NModel.Terms.Term>;
using Node = NModel.Terms.Term;
using NModel.Execution;
using GraphLayout = Microsoft.Msagl;


namespace NModel.Visualization
{
    /// <summary>
    /// Delegate that maps a finite automaton state to a model program state
    /// </summary>
    /// <param name="state">given finite automaton state</param>
    /// <returns>corresponding model program state</returns>
    public delegate IState StateProvider(Term state);

    /// <summary>
    /// Displays a finite state machine graph.
    /// </summary>
    public partial class GraphView : UserControl
    {
        /// <summary>
        /// Stores whether the graph has changed since the last layout.
        /// </summary>
        private bool graphChanged;

        /// <summary>
        /// Maps AGL nodes to FSM nodes.
        /// </summary>
        internal Dictionary<GraphLayout.Drawing.Node, Node> nodes = new Dictionary<GraphLayout.Drawing.Node, Node>();

        /// <summary>
        /// Maps AGL edges to multilabeled transitions.
        /// </summary>
        private Dictionary<GraphLayout.Drawing.Edge, MultiLabeledTransition> transitions = new Dictionary<GraphLayout.Drawing.Edge, MultiLabeledTransition>();

        /// <summary>
        /// Current context regarding which finite automaton is being viewed.
        /// Is a reduct of the top level machine.
        /// </summary>
        internal FAContext finiteAutomatonContext;

        /// <summary>
        /// Info regarding the top level FA that was set.
        /// </summary>
        FAInfo faInfo;

        SaveFileDialog saveAsDotDialog;
        SaveFileDialog saveAsFSMDialog;

        /// <summary>
        /// Creates a new graph giew
        /// </summary>
        public GraphView()
        {
            InitializeComponent();

            // Set the property grid to only show RuntimeBrowsable properties.
            propertyGrid.BrowsableAttributes = new AttributeCollection(RuntimeBrowsableAttribute.Yes);

            // Remove the viewer's default toolbar and set the whole background white.
            viewer.ToolBarIsVisible = false;
            viewer.OutsideAreaBrush = Brushes.White;

            //hide MDE specific buttons:
            combineActionsButton.Visible = true;
            propertiesButton.Visible = true;
            toolStripSeparator4.Visible = true;
            toolStripSeparator5.Visible = true;

            this.stateViewer1.Title = "No state is selected";

            //set the click_handler to select a node 
            this.viewer.MouseClick += new MouseEventHandler(GraphView_MouseClick);

            //set the keup event to select nodes in the viewer
            //this.viewer.KeyUp += new KeyEventHandler(viewer_KeyUp);
            this.viewer.KeyPress += new KeyPressEventHandler(viewer_KeyPress);

            //this.viewer.GotFocus += new EventHandler(viewer_GotFocus);
            //this.viewer.LostFocus += new EventHandler(viewer_LostFocus);
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.InitialDirectory = System.Environment.CurrentDirectory;
            dialog.OverwritePrompt = true;
            dialog.Title = "Save as dot file";
            dialog.Filter = "dot files (*.dot)|*.dot|All files (*.*)|*.*";
            dialog.FilterIndex = 0;
            dialog.RestoreDirectory = false;
            this.saveAsDotDialog = dialog;

            SaveFileDialog dialog2 = new SaveFileDialog();
            dialog2.InitialDirectory = System.Environment.CurrentDirectory;
            dialog2.OverwritePrompt = true;
            dialog2.Title = "Save as FSM";
            dialog2.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            dialog2.FilterIndex = 0;
            dialog2.RestoreDirectory = false;
            this.saveAsFSMDialog = dialog2;
        }

        //void viewer_LostFocus(object sender, EventArgs e)
        //{
        //    this.BackColor = SystemColors.InactiveBorder;
        //    this.label1.ForeColor = SystemColors.InactiveCaptionText;
        //}

        //void viewer_GotFocus(object sender, EventArgs e)
        //{
        //    this.BackColor = SystemColors.ActiveBorder;
        //    this.label1.ForeColor = SystemColors.ActiveCaptionText;
        //}

        void viewer_KeyPress(object sender, KeyPressEventArgs e)
        {
            switch (e.KeyChar)
            {
                //case '6':
                //    SelectRightSibling();
                //    return;
                case '8':  //up arrow on numpad
                case 'u':
                    SelectParent();
                    return;
                case 'd': //down arrow on numpad
                case '2':
                    SelectChild();
                    return;
                //case '4':
                //    SelectLeftSibling();
                //    return;
                case '6':  //right on numpad
                case 'n':
                    SelectNextNode();
                    return;
                case '4': //left on numpad
                case 'p':
                    SelectPreviousNode();
                    return;
                default:
                    //MessageBox.Show(e.KeyCode.ToString());
                    return;
            }
        }

        internal void SelectNextNode()
        {
            if (this.selectedNode == null)
            {
                this.selectedNode = this.finiteAutomatonContext.fa.InitialState;
            }
            else
            {
                Node nextNode = FindNextNode(this.selectedNode);
                if (nextNode.Equals(this.selectedNode))
                {
                    //select the minimum state
                    nextNode = this.finiteAutomatonContext.fa.States.Minimum();
                }
                this.UnSelectPreviousSelection();
                this.selectedNode = nextNode;
            }
            this.ReselectNode();
        }

        internal void SelectPreviousNode()
        {
            if (this.selectedNode == null)
            {
                this.selectedNode = this.finiteAutomatonContext.fa.InitialState;
            }
            else
            {
                Node nextNode = FindPreviousNode(this.selectedNode);
                if (nextNode.Equals(this.selectedNode))
                {
                    //select the maximum state
                    nextNode = this.finiteAutomatonContext.fa.States.Maximum();
                }
                this.UnSelectPreviousSelection();
                this.selectedNode = nextNode;
            }
            this.ReselectNode();
        }

        private Term FindNextNode(Term node)
        {
            Set<Node> nextNodes = this.finiteAutomatonContext.fa.States.Select(delegate(Node n) { return n.CompareTo(node) > 0; });
            if (nextNodes.IsEmpty) return node;
            Node nextNode = nextNodes.Minimum();
            if (this.hiddenMealyNodes.Contains(nextNode))
            {
                Set<Node> secondNextNodes = this.finiteAutomatonContext.fa.States.Select(delegate(Node n) { return n.CompareTo(nextNode) > 0; });
                if (secondNextNodes.IsEmpty) return node;
                return secondNextNodes.Minimum();
            }
            return nextNode;
        }

        private Term FindPreviousNode(Term node)
        {
            Set<Node> prevnodes = this.finiteAutomatonContext.fa.States.Select(delegate(Node n) { return n.CompareTo(node) < 0; });
            if (prevnodes.IsEmpty) return node;
            Node prevNode = prevnodes.Maximum();
            if (this.hiddenMealyNodes.Contains(prevNode))
            {
                Set<Node> secondPrevNodes = this.finiteAutomatonContext.fa.States.Select(delegate(Node n) { return n.CompareTo(prevNode) < 0; });
                if (secondPrevNodes.IsEmpty) return node;
                return secondPrevNodes.Maximum();
            }
            return prevNode;
        }

        private void SelectChild()
        {
            if (this.selectedNode == null)
            {
                this.selectedNode = this.finiteAutomatonContext.fa.InitialState;
            }
            else
            {
                Node nextNode = FindSmallestChild(this.selectedNode);
                this.UnSelectPreviousSelection();
                this.selectedNode = nextNode;
            }
            this.ReselectNode();
        }

        private Node FindSmallestChild(Node node)
        {
            Set<Transition> exitingTransitions =
                this.finiteAutomatonContext.fa.Transitions.Select(delegate(Transition t) { return t.First.Equals(node) && t.Third.CompareTo(node) > 0; });
            if (exitingTransitions.IsEmpty)
                return node;
            else
            {
                Set<Node> children =
                    exitingTransitions.Convert<Node>(delegate(Transition t) { return t.Third; });
                Node child = children.Minimum();
                if (this.hiddenMealyNodes.Contains(child))
                {
                    return FindSmallestChild(child);
                }
                return child;
            }
        }

        private void SelectParent()
        {
            if (this.selectedNode == null)
            {
                this.selectedNode = this.finiteAutomatonContext.fa.InitialState;
            }
            else
            {
                Node nextNode = FindParent(this.selectedNode);
                this.UnSelectPreviousSelection();
                this.selectedNode = nextNode;
            }
            this.ReselectNode();
        }

        private Node FindParent(Node node)
        {
            Set<Transition> enteringTransitions =
                this.finiteAutomatonContext.fa.Transitions.Select(delegate(Transition t) { return t.Third.Equals(node) && t.First.CompareTo(node) < 0; });
            if (enteringTransitions.IsEmpty)
                return node;
            else
            {
                Set<Node> fromNodes =
                    enteringTransitions.Convert<Node>(delegate(Transition t) { return t.First; });
                Node fromNode = fromNodes.Minimum();
                if (this.hiddenMealyNodes.Contains(fromNode))
                {
                    return FindParent(fromNode);
                }
                return fromNode;
            }
        }

        GraphLayout.Drawing.Node selectedAglNode;
        object selectedNodeOriginalColor;
        internal Node selectedNode;
        

        void GraphView_MouseClick(object sender, MouseEventArgs e)
        {
            if (viewer.SelectedObject is GraphLayout.Drawing.Node)
            {
                if (selectedAglNode != viewer.SelectedObject)
                {
                    //UnSelectPreviousSelection
                    if (selectedAglNode != null)
                    {
                        SetNodeSelectionColor(selectedAglNode, (GraphLayout.Drawing.Color)selectedNodeOriginalColor);
                    }
                    selectedAglNode = (GraphLayout.Drawing.Node)viewer.SelectedObject;
                    selectedNode = this.nodes[selectedAglNode];
                    RememberSelectedNodeOriginalColor(selectedAglNode);
                    SetNodeSelectionColor(
                    selectedAglNode,ToAglColor(this.selectionColor));
                    this.viewer.Invalidate();
                    UpdateStateViewer();
                }
                if (e.Button == MouseButtons.Right)
                {
                    this.selectNodeContextMenuStrip.Show(this.viewer,e.Location);
                }
            }
            else
            {
                UnSelectPreviousSelection();
            }
            this.viewer.Focus();
        }

        private void UnSelectPreviousSelection()
        {
            if (selectedAglNode != null)
            {
                SetNodeSelectionColor(selectedAglNode,(GraphLayout.Drawing.Color)selectedNodeOriginalColor);
                selectedAglNode = null;
                selectedNode = null;
                this.viewer.Invalidate();
                this.stateViewer1.SetState(null);
                this.stateViewer1.Title = "No state is selected";
            }
        }

        //using the background color does not seem to work in this version of glee
        //use the font color of nodes instead,
        //node label highlighting during hovering has therefore been turned off
        private void SetNodeSelectionColor(GraphLayout.Drawing.Node n, GraphLayout.Drawing.Color c)
        {
            //n.Attr.FillColor = c;
            n.Label.FontColor = c;
        }
        private void RememberSelectedNodeOriginalColor(GraphLayout.Drawing.Node n)
        {
            //selectedNodeOriginalColor = n.Attr.FillColor;
            selectedNodeOriginalColor = n.Label.FontColor;
        }

        /// <summary>
        /// View the given state machine
        /// </summary>
        /// <param name="fa">given finite automaton</param>
        /// <param name="stateProvider">optional model program state provider</param>
        public void SetStateMachine(FSM fa, StateProvider stateProvider)
        {
            SetStateMachine(fa, stateProvider, null, Set<Transition>.EmptySet);
        }

        ModelProgram mp;

        /// <summary>
        /// Used internally by the derived viewer that may also set the modelprogram
        /// </summary>
        internal void SetStateMachine(FSM fa, StateProvider stateProvider, ModelProgram mp1, Set<Transition> groupingTransitions)
        {
            // Clear the node and transition tables and the cache
            this.mp = mp1;
            nodes.Clear();
            transitions.Clear();
            //reductCache.Clear();
            this.faInfo = new FAInfo(this.stateViewer1.DefaultModelName, fa, stateProvider, mp1);
            this.finiteAutomatonContext = new FAContext(fa, stateProvider, faInfo.ReductNames.Last, mp1);
            this.finiteAutomatonContext.groupingTransitions = groupingTransitions;
            //this.reductCache[this.faInfo.ReductNames.Last] = this.finiteAutomatonContext;
            this.projectionButton.Text = "     " + faInfo.ReductNames.Last.name.ToString();
            this.projectionButton.ToolTipText = faInfo.ReductNames.Last.name.ToString();
            if (faInfo.IsProduct)
            {
                this.projectionButton.Enabled = true;
                EnableProjection();
            }
            else
            {
                this.projectionButton.Enabled = false;
            }
            graphChanged = true;
            PerformLayout();
        }

        /// <summary>
        /// Enable projection of the original graph if its states are pairstates
        /// </summary>
        private void EnableProjection()
        {
            this.projectionButton.DropDownItems.Clear();
            for (int k = 0; k < this.faInfo.ReductNames.Count; k++)
            {
                ReductName rn = this.faInfo.ReductNames[k];
                ToolStripMenuItem item = new ToolStripMenuItem(rn.name.ToString(),
                    ChooseProjectionImage(rn.treePosition),
                    new EventHandler(this.ProjectionItem_Click));
                item.CheckOnClick = false;
                this.projectionButton.DropDownItems.Add(item);
                item.Tag = rn;
                //mark the last item checked as it is being displayed by default
                if (k == this.faInfo.ReductNames.Count - 1)
                    item.Checked = true;
            }
        }

        /// <summary>
        /// Choose an appropriate image to go with the projected item
        /// </summary>
        /// <param name="treePos"></param>
        /// <returns></returns>
        Image ChooseProjectionImage(Sequence<FSMBuilder.Branch> treePos)
        {
            if (treePos.IsEmpty)
                return imageListProjections.Images["root"];
            else if (treePos.Head == FSMBuilder.Branch.Left)
            {
                if (treePos.Tail.IsEmpty)
                {
                    return imageListProjections.Images["L"];
                }
                else
                {
                    if (treePos.Tail.Head == FSMBuilder.Branch.Left)
                    {
                        return imageListProjections.Images["LL"];
                    }
                    else
                    {
                        return imageListProjections.Images["LR"];
                    }
                }
            }
            else
            {
                if (treePos.Tail.IsEmpty)
                {
                    return imageListProjections.Images["R"];
                }
                else
                {
                    if (treePos.Tail.Head == FSMBuilder.Branch.Left)
                    {
                        return imageListProjections.Images["RL"];
                    }
                    else
                    {
                        return imageListProjections.Images["RR"];
                    }
                }
            }
        }

        private void ProjectionItem_Click(object o, EventArgs e)
        {
            ToolStripMenuItem i = (ToolStripMenuItem)o;
            //if the item is already checked, the projection is current 
            //and nothing changes
            if (!i.Checked)
            {
                i.Checked = true;
                //uncheck the other items
                foreach (ToolStripMenuItem j in this.projectionButton.DropDownItems)
                {
                    if (i != j) j.Checked = false; 
                }
                ShowReduct((ReductName)i.Tag);
                this.projectionButton.Text = "     " + ((ReductName)i.Tag).name.ToString();
                this.projectionButton.ToolTipText = ((ReductName)i.Tag).name.ToString();
                this.projectionButton.Image = ChooseProjectionImage(((ReductName)i.Tag).treePosition);
            }
        }

        //Dictionary<ReductName, FAContext> reductCache = new Dictionary<ReductName, FAContext>();

        private void ShowReduct(ReductName reductName)
        {
            FAContext faContext1;
            //if (!reductCache.TryGetValue(reductName, out faContext1))
            //{
                Dictionary<Term, IState> reductStateMap;
                Dictionary<Term, IState> stateMap = new Dictionary<Term, IState>();
                foreach (Term t in faInfo.fa.States)
                {
                    stateMap[t] = faInfo.stateProvider(t);
                }
                FSM fa = FSMBuilder.ProjectFromProduct(faInfo.fa, ProjectedActionSymbols(reductName.treePosition), reductName.treePosition, stateMap, out reductStateMap);
                faContext1 = new FAContext(fa, delegate(Term n) { return reductStateMap[n]; }, reductName, ProjectedModelProgram(reductName.treePosition));//+++
                //reductCache[reductName] = faContext1;
            //}
            finiteAutomatonContext = faContext1;
            graphChanged = true;
            this.selectedAglNode = null; //forget previous selected glee node
            this.selectedNode = null;     //forget previous selected corresponding node
            PerformLayout();
        }

        Set<Symbol> ProjectedActionSymbols(Sequence<FSMBuilder.Branch> position)
        {
            if (this.mp == null) 
                return faInfo.fa.Vocabulary;
            return ProjectedModelProgram(position).ActionSymbols();
        }

        ModelProgram ProjectedModelProgram(Sequence<FSMBuilder.Branch> position)
        {
            if (this.mp == null) 
                return null;
            ModelProgram mpProj = this.mp;
            for (int i = 0; i < position.Count; i++)
            {
                if (position[i] == FSMBuilder.Branch.Left)
                    mpProj = ((ProductModelProgram)mpProj).M1;
                else
                    mpProj = ((ProductModelProgram)mpProj).M2;
            }
            return mpProj;
        }

        CustomLabelProvider customStateLabelProvider;

        /// <summary>
        /// Delegate that maps a finite automata state to a string that is 
        /// used to label it.
        /// </summary>
        public delegate string CustomLabelProvider(IState state);

        /// <summary>
        /// Function that maps a finite auomaton state to a string label used 
        /// instead of the default label that is the state id.
        /// </summary>
        public CustomLabelProvider CustomStateLabelProvider
        {
            get { return customStateLabelProvider; }
            set { customStateLabelProvider = value; }
        }

        CustomLabelProvider customStateTooltipProvider;

        /// <summary>
        /// Function that maps a finite auomaton state to a string label used as tooltip
        /// instead of the default that is the state id.
        /// </summary>
        public CustomLabelProvider CustomStateTooltipProvider
        {
            get { return customStateTooltipProvider; }
            set { customStateTooltipProvider = value; }
        }

        private void UpdateStateViewer()
        {
            Term node = this.nodes[selectedAglNode];
            if (finiteAutomatonContext.stateProvider != null)
            {
                this.stateViewer1.Title = "Variables in state " + node.ToString();
                IState state = finiteAutomatonContext.stateProvider(node);
                this.stateViewer1.SetState(state, finiteAutomatonContext.name);
            }
            else
            {
                this.stateViewer1.Title = "Variables in state " + node.ToString() + " are undefined";
            }
        }

        /// <summary>
        /// Draws the graph when the form is loaded.
        /// </summary>
        /// <param name="e">Unused event arguments.</param>
        protected override void OnLoad(EventArgs/*?*/ e)
        {
            base.OnLoad(e);
        }

        /// <summary>
        /// Refreshes the control and recreates the graph if needed.
        /// </summary>
        /// <param name="levent">Unused event arguments.</param>
        protected override void OnLayout(LayoutEventArgs levent)
        {
            // If the graph has changed,
            if (graphChanged)
            {
                // Always refresh the property grid since this is quick.
                propertyGrid.Refresh();

                //always unselect any possibly selected or hovered node because the 
                //selection will not be valid after redrawing the graph
                this.stateViewer1.SetState(null);
                this.selectedAglNode = null;
                this.previouslyHoveredObject = null;
                this.stateViewer1.Title = "No state is selected";


                // Reset the status since we're going to recreate it now.
                graphChanged = false;

                // If we have a state machine.
                if (finiteAutomatonContext.fa != null)
                {
                    // The background thread actually lays out the graph.
                    if (!graphWorker.IsBusy)
                    {
                        // Start using the wait cursor and disable the controls.
                        Application.UseWaitCursor = true;
                        Enabled = false;
                        progressBar.Visible = true;

                        // Begin the calculation.
                        graphWorker.RunWorkerAsync();
                    }
                    else
                    {
                        // DrawGraph was called in the middle of a layout operation somehow.
                        // We can't cancel graph layout, so for now we will just ignore it.
                    }
                }
                else
                {
                    // Clear the display.
                    viewer.Graph = null;
                }
            }

            base.OnLayout(levent);
        }

        Set<Node> hiddenMealyNodes = Set<Node>.EmptySet;

        // The set is used for generating dot output.
        Dictionary<MultiLabeledTransition,int> dashedEdges = new Dictionary<MultiLabeledTransition,int>();

        /// <summary>
        /// Performs the work of constructing and laying out the graph in a background thread using MsAGL.
        /// </summary>
        /// <param name="sender">BackgroundWorker.</param>
        /// <param name="e">worker event args</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private void graphWorker_DoWork(object/*?*/ sender, DoWorkEventArgs/*?*/ e)
        //^ requires sender is BackgroundWorker;
        {
            BackgroundWorker worker = (BackgroundWorker)sender;
            
            // Combine the inputs and outputs if desired.
            // FiniteStateMachine fsm = combineActions ? machine.CombineInputsAndOutputs() : machine;

            // Clear the node and transition tables.
            nodes.Clear();
            transitions.Clear();
            dashedEdges.Clear();

            // Create a new Agl graph.
            Microsoft.Msagl.Drawing.Graph graph = new Microsoft.Msagl.Drawing.Graph("Finite Automaton");

            // Set the layout direction.
            graph.Attr.LayerDirection = LayerDirection();

            // Report the total number of states and transitions.
            int progress = 0;
            worker.ReportProgress(progress, finiteAutomatonContext.fa.States.Count + Math.Min(maxTransitions, finiteAutomatonContext.fa.Transitions.Count));

            //for quick lookup create maps from nodes to all exiting transitions and entering transitions
            Dictionary<Node, List<Transition>> exitingTransitions = new Dictionary<Node, List<Transition>>();
            Dictionary<Node, List<Transition>> enteringTransitions = new Dictionary<Node, List<Transition>>();
            foreach (Node n in finiteAutomatonContext.fa.States)
            {
                exitingTransitions[n] = new List<Transition>();
                enteringTransitions[n] = new List<Transition>();
            }
            foreach (Transition t in finiteAutomatonContext.fa.Transitions)
            {
                exitingTransitions[t.First].Add(t);
                enteringTransitions[t.Third].Add(t);
            }
            //foreach (Transition t in finiteAutomatonContext.dashedTransitions)
            //{
            //    exitingTransitions[t.First].Add(t);
            //    enteringTransitions[t.Third].Add(t);
            //}
            //In Mealy view hidden nodes have one incoming Start action and at
            //least one outgoing Finish action
            //with the same action symbol name
            hiddenMealyNodes = Set<Node>.EmptySet;
            if (combineActions)
            {
                foreach (Node n in finiteAutomatonContext.fa.States)
                {
                    if (enteringTransitions[n].Count == 1 && IsStartAction(enteringTransitions[n][0].Second))
                    {
                        string actionname = ((CompoundTerm)enteringTransitions[n][0].Second).Symbol.Name.Replace("_Start","");
                        if (exitingTransitions[n].Count >= 1
                            && AllTransitionsAreFinish(actionname, exitingTransitions[n]))
                        {
                            hiddenMealyNodes = hiddenMealyNodes.Add(n);
                        }
                    }
                }
            }


            #region Add the initial state first.
            GraphLayout.Drawing.Node initialNode = graph.AddNode(finiteAutomatonContext.fa.InitialState.ToString());
            if (this.nodeLabelsVisible)
            {
                if (this.finiteAutomatonContext.stateProvider != null && this.customStateLabelProvider != null)
                {
                    initialNode.LabelText= this.customStateLabelProvider(this.finiteAutomatonContext.stateProvider(this.finiteAutomatonContext.fa.InitialState));
                }
            }
            else
            {
                initialNode.LabelText = "";
            }
            //initial progress, one node is handled
            worker.ReportProgress(++progress);
            initialNode.Attr.FillColor = new GraphLayout.Drawing.Color(initialStateColor.R, initialStateColor.G, initialStateColor.B);
            initialNode.Attr.Shape = MapToAglShape(this.stateShape);
            #endregion

            #region Add the transitions by walking the graph depth first

            Stack<Node> stack = new Stack<Node>();
            Set<Node> visited = Set<Node>.EmptySet;
            //Notice the invariant: nodes on the stack can not be hidden nodes
            //base case: initial state cannot be hidden because 
            //it must have at least one exiting start action or atomic action
            stack.Push(finiteAutomatonContext.fa.InitialState);
            visited = visited.Add(finiteAutomatonContext.fa.InitialState);
            while (stack.Count > 0 && graph.EdgeCount < maxTransitions)
            {
                //invariant: nodes on the stack are not hidden nodes
                Node current = stack.Pop();
                GraphLayout.Drawing.Node fromNode = graph.AddNode(current.ToString());

                nodes[fromNode] = current;

                //record the transitions to be added as 
                //dictionary of MultiLabeledTransitions from current
                //state indexed by end state
                Dictionary<Node, MultiLabeledTransition> transitionsToBeAdded = 
                    new Dictionary<Node, MultiLabeledTransition>();

                #region collect transitions to be added into transitionsToBeAdded
                foreach (Transition t in exitingTransitions[current])
                {
                    //do not add dead states if requested
                    if (this.livenessCheckIsOn &&
                        !this.deadstatesVisible &&
                        this.finiteAutomatonContext.deadNodes.Contains(t.Third))
                        continue;

                    worker.ReportProgress(++progress); //one transition is handled
                    if (hiddenMealyNodes.Contains(t.Third))
                    {
                        //construct Mealy view of all the exiting transitions from t.Third
                        foreach (Transition tf in exitingTransitions[t.Third])
                        {
                            worker.ReportProgress(++progress);
                            //string mealyLabel = (this.actionLabelsVisible ? ConstructMealyLabel(t.Second, tf.Second) : "");
                            //add target node to the stack, if not visited already
                            if (!visited.Contains(tf.Third))
                            {
                                visited = visited.Add(tf.Third);
                                if (exitingTransitions[tf.Third].Count > 0)
                                {
                                    stack.Push(tf.Third); //notice that invariant is preserved
                                }
                                //one more node is handled
                                worker.ReportProgress(++progress);
                            }
                            // add edge
                            GraphLayout.Drawing.Node toNode = graph.AddNode(tf.Third.ToString());
                            toNode.Attr.Shape = MapToAglShape(this.stateShape);
                            if (this.nodeLabelsVisible)
                            {
                                if (this.finiteAutomatonContext.stateProvider != null && this.customStateLabelProvider != null)
                                {
                                    toNode.LabelText= this.customStateLabelProvider(this.finiteAutomatonContext.stateProvider(tf.Third));
                                }
                            }
                            else
                                toNode.LabelText = "";
                            nodes[toNode] = tf.Third;
                            if (loopsVisible || fromNode.Attr.Id != toNode.Attr.Id)
                            {
                                MultiLabeledTransition trans;
                                if (transitionsToBeAdded.TryGetValue(tf.Third, out trans))
                                {
                                    trans.AddMealyLabel(t.Second, tf.Second);
                                }
                                else
                                {
                                    trans = MultiLabeledTransition.Create(t.First, t.Second, tf.Second, tf.Third);
                                    transitionsToBeAdded[tf.Third] = trans;
                                }
                                //GraphLayout.Drawing.Edge edge = graph.AddEdge(fromNode.Attr.Id, mealyLabel, toNode.Attr.Id);
                                //transitions[edge] = CombinedTransition.Create(t, tf);
                            }
                        }
                    }
                    else
                    {
                        //string edgeLabel = (this.actionLabelsVisible ? ConstructEdgeLabel(t.Second) : "");
                        //add target node to the stack, if not visited already
                        if (!visited.Contains(t.Third))
                        {
                            visited = visited.Add(t.Third);
                            if (exitingTransitions[t.Third].Count > 0)
                            {
                                stack.Push(t.Third); //notice that invariant is trivially preserved
                            }
                            //one more node is handled
                            worker.ReportProgress(++progress);
                        }
                        // add edge
                        GraphLayout.Drawing.Node toNode = graph.AddNode(t.Third.ToString());
                        toNode.Attr.Shape = MapToAglShape(this.stateShape);
                        if (this.nodeLabelsVisible)
                        {
                            if (this.finiteAutomatonContext.stateProvider != null && this.customStateLabelProvider != null)
                            {
                                toNode.LabelText = this.customStateLabelProvider(this.finiteAutomatonContext.stateProvider(t.Third));
                            }
                        }
                        else
                            toNode.LabelText = "";
                        nodes[toNode] = t.Third;
                        if (loopsVisible || !fromNode.Attr.Id.Equals(toNode.Attr.Id))
                        {
                            MultiLabeledTransition trans;
                            if (transitionsToBeAdded.TryGetValue(t.Third, out trans))
                            {
                                trans.AddLabel(t.Second);
                            }
                            else
                            {
                                trans = MultiLabeledTransition.Create(t.First, t.Second, t.Third);
                                transitionsToBeAdded[t.Third] = trans;
                            }
                            //GraphLayout.Drawing.Edge edge = graph.AddEdge(fromNode.Attr.Id, edgeLabel, toNode.Attr.Id);
                            //transitions[edge] = CombinedTransition.Create(t);
                        }
                        //one more edge is added
                        worker.ReportProgress(++progress);
                    }
                }
                #endregion

                //add all transitions from the current state to glee
                foreach (MultiLabeledTransition trans in transitionsToBeAdded.Values)
                {
                    //don't add transitions that lead to dead states if dead state are turned off
                    //if (this.livenessCheckIsOn &&
                    //    !this.deadstatesVisible &&
                    //    this.finiteAutomatonContext.deadNodes.Contains(trans.endState))
                    //    continue;

                    GraphLayout.Drawing.Node toNode = graph.AddNode(trans.endState.ToString());
                    if (mergeLabels)
                    {
                        string lab = (this.transitionLabels != TransitionLabel.None ? trans.CombinedLabel(this.transitionLabels == TransitionLabel.ActionSymbol) : "");
                        GraphLayout.Drawing.Edge edge = graph.AddEdge(fromNode.Attr.Id, lab, toNode.Attr.Id);
                        transitions[edge] = trans;
                    }
                    else
                    {
                        foreach (MultiLabeledTransition tr in trans.CreateOnePerLabel())
                        {
                            string lab = (this.transitionLabels != TransitionLabel.None ? tr.CombinedLabel(this.transitionLabels == TransitionLabel.ActionSymbol) : "");
                            GraphLayout.Drawing.Edge edge = graph.AddEdge(fromNode.Attr.Id, lab, toNode.Attr.Id);
                            transitions[edge] = tr;
                        }
                    }
                }

            }

            #region Draw dashed arrows specified in the set finiteAutomatonContext.dashedTransitions

            Dictionary<Transition, MultiLabeledTransition> mlDashedTransitions = new Dictionary<Transition, MultiLabeledTransition>();
            foreach (Transition trans in finiteAutomatonContext.groupingTransitions)
                mlDashedTransitions.Add(trans, MultiLabeledTransition.Create(trans.First, trans.Second, trans.Third));
            //add all dashed transitions from the current facontext to GLEE
            foreach (MultiLabeledTransition trans in mlDashedTransitions.Values)
            {
                //don't add transitions that lead to dead states if dead state are turned off
                //if (this.livenessCheckIsOn &&
                //    !this.deadstatesVisible &&
                //    this.finiteAutomatonContext.deadNodes.Contains(trans.endState))
                //    continue;

                GraphLayout.Drawing.Node toNd = graph.AddNode(trans.endState.ToString());
                GraphLayout.Drawing.Node fromNd = graph.AddNode(trans.startState.ToString());
                //if (mergeLabels)
                //{
                //    string lab = (this.transitionLabels != TransitionLabel.None ? trans.CombinedLabel(this.transitionLabels == TransitionLabel.ActionSymbol) : "");
                //    GraphLayout.Drawing.IEdge edge = graph.AddEdge(fromNode.NodeAttribute.Id, lab, toNode.NodeAttribute.Id);
                //    transitions[edge] = trans;
                //}
                //else
                //{
                //    foreach (MultiLabeledTransition tr in trans.CreateOnePerLabel())
                //    {
                string lab = (this.transitionLabels != TransitionLabel.None ? trans.CombinedLabel(this.transitionLabels == TransitionLabel.ActionSymbol) : "");
                GraphLayout.Drawing.Edge edge = graph.AddEdge(fromNd.Attr.Id, lab, toNd.Attr.Id);
                GraphLayout.Drawing.AttributeBase eattr = edge.Attr as GraphLayout.Drawing.AttributeBase;
                eattr.AddStyle(GraphLayout.Drawing.Style.Dashed);
                transitions[edge] = trans;
                dashedEdges.Add(trans,0);
                //    }
                //}
            }

            # endregion

            if (stack.Count == 0)
            {
                //add unreachable edges and nodes up to the given max count
                if (visited.Count + hiddenMealyNodes.Count < finiteAutomatonContext.fa.States.Count)
                {
                    foreach (Term n in finiteAutomatonContext.fa.States.Difference(visited.Union(hiddenMealyNodes)))
                    {
                        //do not consider dead state if they are not visible
                        if (this.livenessCheckIsOn &&
                            !this.deadstatesVisible &&
                            this.finiteAutomatonContext.deadNodes.Contains(n))
                            continue;

                        //record the transitions to be added as 
                        //dictionary of MultiLabeledTransitions from current
                        //state indexed by end state
                        Dictionary<Node, MultiLabeledTransition> transitionsToBeAdded = 
                             new Dictionary<Node, MultiLabeledTransition>();


                        GraphLayout.Drawing.Node n1 = graph.AddNode(n.ToString());
                        n1.Attr.Color = ToAglColor(Color.Gray);
                        n1.Label.FontColor = ToAglColor(Color.Gray);
                        n1.Attr.Shape = MapToAglShape(this.StateShape);
                        nodes[n1] = n;
                        if (!this.nodeLabelsVisible)
                        {
                            n1.LabelText = "";
                        }

                        foreach (Triple<Term, CompoundTerm, Term> t in exitingTransitions[n])
                        {
                            MultiLabeledTransition t1;
                            if (transitionsToBeAdded.TryGetValue(t.Third, out t1))
                            {
                                t1.AddLabel(t.Second);
                            }
                            else
                            {
                                t1 = MultiLabeledTransition.Create(n, t.Second, t.Third);
                                transitionsToBeAdded[t.Third] = t1;
                            }
                        }


                        //add all transitions from the current state to glee
                        foreach (MultiLabeledTransition t1 in transitionsToBeAdded.Values)
                        {
                            GraphLayout.Drawing.Node n2 = graph.AddNode(t1.endState.ToString());
                            nodes[n2] = t1.endState;
                            if (mergeLabels)
                            {
                                string lab = (this.transitionLabels != TransitionLabel.None ? t1.CombinedLabel(this.transitionLabels == TransitionLabel.ActionSymbol) : "");
                                GraphLayout.Drawing.Edge e1 = graph.AddEdge(n1.Attr.Id, lab, n2.Attr.Id);
                                e1.Label.FontColor = ToAglColor(Color.Gray);
                                e1.Attr.Color = ToAglColor(Color.Gray);
                                transitions[e1] = t1;
                            }
                            else
                            {
                                foreach (MultiLabeledTransition t2 in t1.CreateOnePerLabel())
                                {
                                    string lab = (this.transitionLabels != TransitionLabel.None ? t2.CombinedLabel(this.transitionLabels == TransitionLabel.ActionSymbol) : "");
                                    GraphLayout.Drawing.Edge e1 = graph.AddEdge(n1.Attr.Id, lab, n2.Attr.Id);
                                    e1.Label.FontColor = ToAglColor(Color.Gray);
                                    e1.Attr.Color = ToAglColor(Color.Gray);
                                    transitions[e1] = t2;
                                }
                            }
                        }
                        if (graph.EdgeCount >= maxTransitions)
                            break;
                    }
                }
            }
            #endregion

            #region Format the nodes.
            // Mark dead states
            if (this.livenessCheckIsOn)
            {
                Set<Node> deadNodes = finiteAutomatonContext.deadNodes.Intersect(visited);
                foreach (Node deadNode in deadNodes)
                {
                    GraphLayout.Drawing.Node deadAglNode = graph.AddNode(deadNode.ToString());
                    deadAglNode.Attr.FillColor =
                       new GraphLayout.Drawing.Color(deadStateColor.R, deadStateColor.G, deadStateColor.B);
                }
            }

            //while (stack.Count > 0)
            //{
            //    Node truncatedNode = stack.Pop();
            //    GraphLayout.Drawing.Node truncatedAglNode = graph.AddNode(truncatedNode.ToString());
            //    truncatedAglNode.Attr.Fillcolor =
            //        new GraphLayout.Drawing.Color(truncatedStateColor.R, truncatedStateColor.G, truncatedStateColor.B);
            //}

            // Mark accepting states with a thicker line
            // TBD: waiting for Lev to provide double-line feature
            if (this.acceptingStatesMarked)
            {
                foreach (Node accNode in finiteAutomatonContext.fa.AcceptingStates.Intersect(visited))
                {
                    GraphLayout.Drawing.Node acceptingAglNode = graph.AddNode(accNode.ToString());
                    acceptingAglNode.Attr.Shape = MapToAglShape(stateShape);
                    acceptingAglNode.Attr.LineWidth = 4;
                }
            }

            // Mark error states
            if (this.safetyCheckIsOn)
            {
                if (finiteAutomatonContext.stateProvider != null && finiteAutomatonContext.mp != null)
                {
                    this.finiteAutomatonContext.unsafeNodes = Set<Term>.EmptySet;
                    foreach (Node visitedNode in finiteAutomatonContext.fa.States.Intersect(visited))
                    {
                        IState istate = finiteAutomatonContext.stateProvider(visitedNode);
                        if (!finiteAutomatonContext.mp.SatisfiesStateInvariant(istate))
                        {
                            this.finiteAutomatonContext.unsafeNodes = 
                                this.finiteAutomatonContext.unsafeNodes.Add(visitedNode);
                            GraphLayout.Drawing.Node visitedAglNode = graph.AddNode(visitedNode.ToString());
                            visitedAglNode.Attr.Shape = MapToAglShape(stateShape);
                            visitedAglNode.Attr.FillColor = ToAglColor(this.unsafeStateColor);
                        }
                    }
                }
            }
            #endregion

            // Start the marquee mode of the progress bar.
            worker.ReportProgress(-1);

            //insert a delay to avoid a crash in this version of GLEE
            System.Threading.Thread.Sleep(200);

            // Return the calculated layout as the result.
            e.Result = viewer.CalculateLayout(graph);
        }

        private Microsoft.Msagl.Drawing.LayerDirection LayerDirection()
        {
            return direction == GraphDirection.TopToBottom ? Microsoft.Msagl.Drawing.LayerDirection.TB :
                            direction == GraphDirection.LeftToRight ? Microsoft.Msagl.Drawing.LayerDirection.LR :
                            direction == GraphDirection.RightToLeft ? Microsoft.Msagl.Drawing.LayerDirection.RL :
                            direction == GraphDirection.BottomToTop ? Microsoft.Msagl.Drawing.LayerDirection.BT :
                            Microsoft.Msagl.Drawing.LayerDirection.None;
        }

        static GraphLayout.Drawing.Shape MapToAglShape(StateShape shape)
        {
            switch (shape)
            {
                case StateShape.Box :
                    return GraphLayout.Drawing.Shape.Box;
                case StateShape.Circle:
                    return GraphLayout.Drawing.Shape.Circle;
                case StateShape.Diamond:
                    return GraphLayout.Drawing.Shape.Diamond;
                //case StateShape.DoubleCircle:
                //    return GraphLayout.Drawing.Shape.DoubleCircle;
                case StateShape.Octagon:
                    return GraphLayout.Drawing.Shape.Octagon;
                case StateShape.Plaintext:
                    return GraphLayout.Drawing.Shape.Plaintext;
                //case StateShape.Point:
                //    return GraphLayout.Drawing.Shape.Point;
                default :
                    return GraphLayout.Drawing.Shape.Ellipse;
            }
        }

        internal static string ConstructEdgeLabel(Term term)
        {
            if (term == null) return "";
            CompoundTerm action = term as CompoundTerm;
            if (action == null) return term.ToString();
            if (IsStartAction(action))
                return GetActionLabel(action, null);
            if (IsFinishAction(action))
                return GetActionLabel(null, action);
            return term.ToString();
        }

        internal static string ConstructMealyLabel(Term term, Term term2)
        {
            return GetActionLabel((CompoundTerm)term, (CompoundTerm)term2);
        }

        //internal static string GetActionName(Term action)
        //{
        //    return ((CompoundTerm)action).FunctionSymbol1.ShortName;
        //}

        static bool IsStartAction(Term action)
        {
            CompoundTerm a = action as CompoundTerm;
            if (a == null) return false;
            // return Symbol.Kind(a.FunctionSymbol1) == ActionKind.Start;
            return a.Symbol.Name.EndsWith("_Start");
        }

        static bool IsFinishAction(Term action)
        {
            CompoundTerm a = action as CompoundTerm;
            if (a == null) return false;
            // return Symbol.Kind(a.FunctionSymbol1) == ActionKind.Finish ;
            return a.Symbol.Name.EndsWith("_Finish");
        }

        static bool IsFinishAction(Term action, string name)
        {
            CompoundTerm a = action as CompoundTerm;
            if (a==null) return false;
            return IsFinishAction(action) && a.Symbol.Name == name + "_Finish";
        }

        static bool AllTransitionsAreFinish(string actionName, List<Transition> transitions)
        {
            foreach (Transition t in transitions)
                if (!IsFinishAction(t.Second, actionName))
                    return false;
            return true;
        }

        /// <summary>
        /// Sets the calculated layout of the graphworker.
        /// </summary>
        /// <param name="sender">sender of args</param>
        /// <param name="e">Event args containing the calculated layout as the result.</param>
        private void graphWorker_RunWorkerCompleted(object/*?*/ sender, RunWorkerCompletedEventArgs/*?*/ e)
        //^ requires e != null;
        {
            // Draw the layout on the UI thread.
            viewer.SetCalculatedLayout(e.Result);

            // Reset the cursor and enable the controls.
            Application.UseWaitCursor = false;
            Enabled = true;
            progressBar.Visible = false;

            //reselect previously selected node if applicable
            if (this.selectedNode != null)
            {
                ReselectNode();
            }

            this.viewer.Focus();
        }

        void ReselectNode()
        {
            //find the corresponding new AGL node, if it exists
            this.selectedAglNode = this.FindAglNode(selectedNode);
            if (this.selectedAglNode != null)
            {
                RememberSelectedNodeOriginalColor(selectedAglNode);
                SetNodeSelectionColor(
                selectedAglNode,ToAglColor(this.selectionColor));
                this.viewer.Invalidate();
                UpdateStateViewer();
            }
            else //forget the previous selection that is invalid
            {
                this.selectedNode = null;
            }
        }

        GraphLayout.Drawing.Node FindAglNode(Node node)
        {
            foreach (KeyValuePair<GraphLayout.Drawing.Node,Node> entry in this.nodes)
            {
                if (entry.Value.Equals(node)) return (GraphLayout.Drawing.Node)entry.Key;
            }
            return null;
        }

        /// <summary>
        /// Formats an action for display in the view.
        /// </summary>
        /// <param name="start">Optional start action with input arguments.</param>
        /// <param name="finish">Optional finish action with output arguments and return value.</param>
        /// <returns>A nicely formatted string representing the action(s).</returns>
        static string GetActionLabel(Terms.CompoundTerm/*?*/ start, Terms.CompoundTerm/*?*/ finish)
        {
            if (start != null && finish != null && finish.Arguments.Count > 0)
                return string.Format((IFormatProvider)null,
                    "{0}({1}) / {2}",
                    start.Symbol.Name.Replace("_Start",""),
                    GetArgumentLabel(start.Arguments),
                    GetArgumentLabel(finish.Arguments));

            if (start != null)
                return string.Format((IFormatProvider)null, 
                    "{0}({1})",
                    start.Symbol.Name,
                    GetArgumentLabel(start.Arguments));

            if (finish != null)
                return string.Format((IFormatProvider)null, 
                    "{0}({1})",
                    finish.Symbol.Name,
                    GetArgumentLabel(finish.Arguments));

            return "";
        }

        /// <summary>
        /// Gets a comma-separated list of arguments as a string for use in an edge.
        /// </summary>
        /// <param name="arguments">Argument list.</param>
        /// <returns>String of the arguments.</returns>
        static string GetArgumentLabel(Sequence<Terms.Term> arguments)
        {
            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < arguments.Count; i++)
            {
                if (i > 0) builder.Append(", ");
                builder.Append(arguments[i].ToString());
            }

            return builder.ToString();
        }

        #region User events 
        private void zoomInButton_Click(object/*?*/ sender, EventArgs/*?*/ e)
        {
            viewer.ZoomInPressed();
        }

        private void zoomOutButton_Click(object/*?*/ sender, EventArgs/*?*/ e)
        {
            viewer.ZoomOutPressed();
        }

        private void handButton_Click(object/*?*/ sender, EventArgs/*?*/ e)
        {
            viewer.PanButtonPressed = handButton.Checked;
        }

        private void backButton_Click(object/*?*/ sender, EventArgs/*?*/ e)
        {
            viewer.BackwardButtonPressed();
        }

        private void forwardButton_Click(object/*?*/ sender, EventArgs/*?*/ e)
        {
            viewer.ForwardButtonPressed();
        }

        private void printButton_Click(object/*?*/ sender, EventArgs/*?*/ e)
        {
            viewer.PrintButtonPressed();
        }

        private void buttonTimer_Tick(object/*?*/ sender, EventArgs/*?*/ e)
        {
            backButton.Enabled = viewer.BackwardEnabled;
            forwardButton.Enabled = viewer.ForwardEnabled;
        }

        private void combineActionsButton_Click(object/*?*/ sender, EventArgs/*?*/ e)
        {
            CombineActions = combineActionsButton.Checked;
        }

        private void forwardStateButton_Click(object/*?*/ sender, EventArgs/*?*/ e)
        {
            //ForwardInitialState = forwardStateButton.Checked;
        }

        private void loopsButton_Click(object/*?*/ sender, EventArgs/*?*/ e)
        {
            LoopsVisible = loopsButton.Checked;
        }

        private void actionLabelsButton_Click(object/*?*/ sender, EventArgs/*?*/ e)
        {
            TransitionLabels = (actionLabelsButton.CheckState == CheckState.Checked ? TransitionLabel.ActionSymbol : (actionLabelsButton.CheckState == CheckState.Indeterminate ? TransitionLabel.None : TransitionLabel.Action));
        }

        private void stateValuesButton_Click(object/*?*/ sender, EventArgs/*?*/ e)
        {
            UpdateVisibility();
        }

        private void enumerationsButton_Click(object/*?*/ sender, EventArgs/*?*/ e)
        {
            //NonEnumerationsVisible = enumerationsButton.Checked;
        }

        private void topToBottomToolStripMenuItem_Click(object/*?*/ sender, EventArgs/*?*/ e)
        {
            Direction = GraphDirection.TopToBottom;
        }

        private void leftToRightToolStripMenuItem_Click(object/*?*/ sender, EventArgs/*?*/ e)
        {
            Direction = GraphDirection.LeftToRight;
        }

        private void rightToLeftToolStripMenuItem_Click(object/*?*/ sender, EventArgs/*?*/ e)
        {
            Direction = GraphDirection.RightToLeft;
        }

        private void bottomToTopToolStripMenuItem_Click(object/*?*/ sender, EventArgs/*?*/ e)
        {
            Direction = GraphDirection.BottomToTop;
        }

        private void propertiesButton_Click(object/*?*/ sender, EventArgs/*?*/ e)
        {
            UpdateVisibility();
        }

        private void UpdateVisibility()
        {
            if (stateValuesButton.Checked && propertiesButton.Checked)
            {
                this.panel1.Visible = true;
                this.splitter1.Visible = true;
                this.splitterHorizontal.Visible = true;
                this.propertyGrid.Visible = true;
                this.stateViewer1.Visible = true;
                this.stateViewer1.Dock = DockStyle.Fill;
                this.propertyGrid.Dock = DockStyle.Top;
            }
            else if (stateValuesButton.Checked)
            {
                this.panel1.Visible = true;
                this.splitter1.Visible = true;
                this.splitterHorizontal.Visible = false;
                this.propertyGrid.Visible = false;
                this.stateViewer1.Visible = true;
                this.stateViewer1.Dock = DockStyle.Fill;
            }
            else if (propertiesButton.Checked)
            {
                this.panel1.Visible = true;
                this.splitter1.Visible = true;
                this.splitterHorizontal.Visible = false;
                this.propertyGrid.Visible = true;
                this.stateViewer1.Visible = false;
                this.propertyGrid.Dock = DockStyle.Fill;
            }
            else
            {
                this.panel1.Visible = false;
                this.splitter1.Visible = false;
            }
        }

        static GraphLayout.Drawing.Color ToAglColor(Color c)
        {
            return new GraphLayout.Drawing.Color(c.R, c.G, c.B);
        }

        // So we can write our own alternative to ToAglColor
        struct DotColor
        {
            byte R, G, B; 
            // must declare both public 
            public DotColor(byte r, byte g, byte b) { R = r; B = b; G = g; }
            public override string ToString()
            {
                // return "white"; // stub
                return String.Format("\"#{0:x2}{1:x2}{2:x2}\"", R, G, B);
            }
        }

        static DotColor ToDotColor(Color c)
        {
            return new DotColor(c.R, c.G, c.B);
        }

        //GraphLayout.Drawing.Color black = new GraphLayout.Drawing.Color(Color.Black.R, Color.Black.G, Color.Black.B);
        object previouslyHoveredObject;
        GraphLayout.Drawing.Color previouslyHoveredObjectColor = new GraphLayout.Drawing.Color(Color.Black.R, Color.Black.G, Color.Black.B);
        GraphLayout.Drawing.Color previouslyHoveredObjectFontColor = new GraphLayout.Drawing.Color(Color.Black.R, Color.Black.G, Color.Black.B);

        void ResetPreviouslyHoveredObject(object newSelection)
        {
            if (previouslyHoveredObject != null && previouslyHoveredObject != newSelection)
            {
                if (previouslyHoveredObject is GraphLayout.Drawing.Node)
                {
                    ((GraphLayout.Drawing.Node)previouslyHoveredObject).Attr.Color = previouslyHoveredObjectColor;
                    //((GraphLayout.Drawing.Node)previouslyHoveredObject).Attr.Fontcolor = previouslyHoveredObjectFontColor;
                }
                else if (previouslyHoveredObject is GraphLayout.Drawing.Edge)
                {
                    ((GraphLayout.Drawing.Edge)previouslyHoveredObject).Attr.Color = previouslyHoveredObjectColor;
                    ((GraphLayout.Drawing.Edge)previouslyHoveredObject).Label.FontColor = previouslyHoveredObjectFontColor;
                }
            }
            GraphLayout.Drawing.Node newSelectionNode = newSelection as GraphLayout.Drawing.Node;
            GraphLayout.Drawing.Edge newSelectionEdge = newSelection as GraphLayout.Drawing.Edge;
            if (newSelectionNode != null)
            {
                previouslyHoveredObjectColor = newSelectionNode.Attr.Color;
                //previouslyHoveredObjectFontColor = newSelectionNode.Attr.Fontcolor;
            }
            else if (newSelectionEdge != null)
            {
                previouslyHoveredObjectColor = newSelectionEdge.Attr.Color;
                previouslyHoveredObjectFontColor = newSelectionEdge.Label.FontColor;
            }
            this.previouslyHoveredObject = newSelection;
        }

        private void viewer_SelectionChanged(object/*?*/ sender, EventArgs/*?*/ e)
        {
            ResetPreviouslyHoveredObject(viewer.SelectedObject);
            GraphLayout.Drawing.Node node = viewer.SelectedObject as GraphLayout.Drawing.Node;
            if (node != null)
            {
                GraphLayout.Drawing.Color c = ToAglColor(this.hoverColor);
                node.Attr.Color = c;
                //node.Attr.Fontcolor = c;
                if (this.finiteAutomatonContext.stateProvider != null && this.customStateTooltipProvider != null)
                {
                    selectedItemToolTip.SetToolTip(viewer.DrawingPanel,
                        this.customStateTooltipProvider(this.finiteAutomatonContext.stateProvider(nodes[node])));
                }
                else
                {
                    if (this.finiteAutomatonContext.stateProvider != null)
                    {
                        selectedItemToolTip.SetToolTip(viewer.DrawingPanel,
                        DefaultNodeTooltip(this.finiteAutomatonContext.stateProvider(nodes[node])));
                    }
                    else
                    {
                        selectedItemToolTip.SetToolTip(viewer.DrawingPanel, node.Attr.Id);
                    }
                }
                this.viewer.Invalidate();
                return;
            }

            GraphLayout.Drawing.Edge edge = viewer.SelectedObject as GraphLayout.Drawing.Edge;
            if (edge != null)
            {
                GraphLayout.Drawing.Color c = ToAglColor(this.hoverColor);
                edge.Attr.Color = c;
                edge.Label.FontColor = c;
                MultiLabeledTransition transition = transitions[edge];
                selectedItemToolTip.SetToolTip(viewer.DrawingPanel, transition.CombinedLabel(false));
                this.viewer.Invalidate();
                return;
            }
            this.viewer.Invalidate();
            selectedItemToolTip.SetToolTip(viewer.DrawingPanel, null);
        }

        /// <summary>
        /// Display the values of state variables
        /// </summary>
        private string DefaultNodeTooltip(IState iState)
        {


            IPairState pairState = iState as IPairState;

            if (pairState != null)
            {
                string firstState = DefaultNodeTooltip(pairState.First);
                string secState = DefaultNodeTooltip(pairState.Second);

                return firstState + "\n" + secState;
            }

            IExtendedState iestate = iState as IExtendedState;
            if (iestate == null)
                return iState.ControlMode.ToCompactString();

            StringBuilder sb = new StringBuilder();
            bool rest = false;
            for (int i = 0; i < iestate.LocationValuesCount; i++)
            {
                string varName = iestate.GetLocationName(i);
                string varVal = iestate.GetLocationValue(i).ToCompactString();

                if (rest) sb.Append("\n");

                sb.Append(varName);
                sb.Append(" = ");
                sb.Append(varVal);

                rest = true;
            }

            return sb.ToString();
        }

        private void viewer_MouseWheel(object/*?*/ sender, MouseEventArgs/*?*/ e)
        //^ requires e != null;
        {
            int zooms = e.Delta / 120;
            for (int i = 0; i < zooms; i++)
                viewer.ZoomInPressed();
            for (int i = 0; i > zooms; i--)
                viewer.ZoomOutPressed();
        }

        private void graphWorker_ProgressChanged(object/*?*/ sender, ProgressChangedEventArgs/*?*/ e)
        //^ requires e != null;
        {
            // A user state means update the maximum.
            if (e.UserState is int)
                progressBar.Maximum = (int)e.UserState;

            if (e.ProgressPercentage >= 0)
            {
                // Update the progress bar.
                progressBar.Style = ProgressBarStyle.Blocks;
                progressBar.Value = e.ProgressPercentage % (progressBar.Maximum + 1);
            }
            else
            {
                // A negative percentage means further progress is unknown.
                progressBar.Style = ProgressBarStyle.Marquee;
            }
        }
        #endregion

        //represents transitions represented by a single glee edge
        //is referenced from DotWriter (actually it should be in the Utilities.Graph namespace)
        internal class MultiLabeledTransition
        {
            internal Node startState;
            internal Node endState;
            SortedList<Pair<Term,Term>,object> labels;

            MultiLabeledTransition(Node startState, Pair<Term, Term> label, Node endState)
            {
                this.startState = startState;
                this.labels = new SortedList<Pair<Term, Term>,object>();
                this.labels.Add(label,null);
                this.endState = endState;
            }

            internal IEnumerable<MultiLabeledTransition> CreateOnePerLabel()
            {
                if (labels.Count == 1)
                {
                    yield return this;
                }
                else
                {
                    foreach (Pair<Term, Term> lab in this.labels.Keys)
                    {
                        yield return new MultiLabeledTransition(this.startState, lab, this.endState);
                    }
                }
            }

            static internal MultiLabeledTransition Create(Node startState, Term label1, Term label2, Node endState)
            {
                return new MultiLabeledTransition(startState, new Pair<Term, Term>(label1, label2), endState);
            }

            static internal MultiLabeledTransition Create(Node startState, Term label1, Node endState)
            {
                return new MultiLabeledTransition(startState, new Pair<Term, Term>(label1, null), endState);
            }

            internal void AddMealyLabel(Term label1, Term label2)
            {
                labels.Add(new Pair<Term,Term>(label1,label2),null);
            }
            internal void AddLabel(Term label1)
            {
                labels.Add(new Pair<Term,Term>(label1,null),null);
            }

            internal string CombinedLabel(bool nameOnly)
            {
                if (nameOnly)
                {
                    StringBuilder nb = new StringBuilder();
                    Set<string> actionNames = Set<string>.EmptySet;
                    bool rest = false;
                    foreach (Pair<Term, Term> label in labels.Keys)
                    {
                        string nextLabel;
                        if (label.Second == null) //Classical view
                            nextLabel = ((CompoundTerm)label.First).Symbol.FullName;
                        else //Mealy view: omit the _Start part and _Finish parts
                            nextLabel = ((CompoundTerm)label.First).Symbol.Name.Replace("_Start","").Replace("_Finish","");
                        if (actionNames.Contains(nextLabel)) //omit if already included
                            continue;
                        actionNames = actionNames.Add(nextLabel);
                        if (rest) nb.Append("\n");
                        nb.Append(nextLabel);
                        rest = true;
                    }
                    return nb.ToString();
                }
                else
                {
                    StringBuilder sb = new StringBuilder();
                    bool rest = false;
                    foreach (Pair<Term, Term> label in labels.Keys)
                    {
                        if (rest) sb.Append("\n");
                        if (label.Second == null)
                            sb.Append(GraphView.ConstructEdgeLabel(label.First));
                        else
                            sb.Append(GraphView.ConstructMealyLabel(label.First, label.Second));
                        rest = true;
                    }
                    return sb.ToString();
                }
            }

        }


        //represents the current FA context that is being viewed
        internal class FAContext
        {
            internal FAName name
            {
                get { return this.reductName.name; }
            }
            internal ReductName reductName;
            internal FSM fa;
            internal StateProvider stateProvider;
            internal ModelProgram mp;
            internal Set<Node> unsafeNodes;
            internal Set<Node> deadNodes;
            internal Set<Transition> groupingTransitions;
            internal FAContext(FSM fa, StateProvider stateProvider, ReductName reductName, ModelProgram mp)
            {
                this.fa = fa;
                this.stateProvider = stateProvider;
                this.reductName = reductName;
                this.mp = mp;
                this.deadNodes = FsmTraversals.GetDeadStates(fa);
                this.unsafeNodes = Set<Node>.EmptySet;
                this.groupingTransitions = Set<Transition>.EmptySet;
            }
        }
        /// <summary>
        /// Represents info about the original (top-level) FA
        /// </summary>
        private class FAInfo
        {
            string defaultModelName;
            internal FSM fa;
            internal StateProvider stateProvider;
            Dictionary<string, int> nameDisambiguator;
            ModelProgram mp;

            internal FAInfo(string defaultModelName,
                FSM fa, StateProvider stateProvider, ModelProgram mp)
            {
                this.fa = fa;
                this.stateProvider = stateProvider;
                this.defaultModelName = defaultModelName;
                this.nameDisambiguator = new Dictionary<string, int>();
                this.mp = mp;
            }

            internal bool IsProduct
            {
                get 
                {
                    return mp != null && mp is ProductModelProgram;
                    //return stateProvider != null && stateProvider(fa.InitialState) is IPairState;
                }
            }

            Sequence<ReductName> reductNames;

            //the last one in the sequence is the root itself
            internal Sequence<ReductName> ReductNames
            {
                get
                {
                    nameDisambiguator.Clear();

                    if (reductNames != null) return reductNames;

                    if (mp != null)
                    {
                        reductNames = GetSubFANames(Sequence<FSMBuilder.Branch>.EmptySequence, this.mp);
                    }
                    else
                    {
                        reductNames = new Sequence<ReductName>(new ReductName(Sequence<FSMBuilder.Branch>.EmptySequence, new FAName(this.defaultModelName)));
                    }
                    return reductNames;
                }
            }

            //do postorder traversal
            private Sequence<ReductName> GetSubFANames(Sequence<FSMBuilder.Branch> treePosition, ModelProgram mp1)
            {
                ProductModelProgram pmp = mp1 as ProductModelProgram;
                if (pmp != null)
                {
                    Sequence<ReductName> leftChildren =
                        GetSubFANames(treePosition.AddLast(FSMBuilder.Branch.Left),
                                      pmp.M1);
                    Sequence<ReductName> rightChildren =
                        GetSubFANames(treePosition.AddLast(FSMBuilder.Branch.Right),
                                      pmp.M2);
                    ReductName name = new ReductName(treePosition,
                        new FAName(leftChildren.Last.name,rightChildren.Last.name));
                    return leftChildren.Concatentate(rightChildren).AddLast(name);
                }
                else
                {
                    return new Sequence<ReductName>(new ReductName(treePosition, GetModelName(mp1)));
                }
            }

            FAName GetModelName(ModelProgram mp1)
            {
                IName nameProvider = mp1 as IName;
                if (nameProvider != null)
                    return new FAName(Disambiguate(nameProvider.Name));
                return new FAName(Disambiguate(this.defaultModelName));
            }

            string Disambiguate(string name)
            {
                if (nameDisambiguator.ContainsKey(name))
                {
                    string newname = name + "_" + nameDisambiguator[name];
                    nameDisambiguator[name] = nameDisambiguator[name] + 1;
                    return newname;
                }
                else
                {
                    nameDisambiguator.Add(name, 1);
                    return name;
                }
            }
        }

        /// <summary>
        /// Represents the name of a reduct FA
        /// </summary>
        internal class ReductName : CompoundValue
        {
            readonly internal Sequence<FSMBuilder.Branch> treePosition;
            readonly internal FAName name;
            
            internal ReductName(Sequence<FSMBuilder.Branch> treePosition, FAName name)
            {
                this.treePosition = treePosition;
                this.name = name;
            }

            public override IEnumerable<IComparable> FieldValues()
            {
                yield return this.treePosition;
                yield return this.name;
            }
        }

        private void viewer_Load(object sender, EventArgs e)
        {

        }

        private void transitionsButton_Click(object sender, EventArgs e)
        {
            MergeLabels = transitionsButton.Checked;
        }

        private void toolStrip_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        /// <summary>
        /// Produce dot output of the graph
        /// </summary>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        string ToDot()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("digraph ");
            AppendLabel(sb, finiteAutomatonContext.name);
            sb.Append(" {\n\n  rankdir=");
            sb.Append(this.direction == GraphDirection.TopToBottom ? "TB" : "LR");

            sb.Append(";\n\n  //Initial state\n  node ");
            AppendInitialStateAttributes(sb);
            sb.Append("\n  ");
            AppendLabel(sb, finiteAutomatonContext.fa.InitialState);
            if (this.customStateLabelProvider != null && this.finiteAutomatonContext.stateProvider != null)
            {
                sb.Append(" [label = ");
                AppendLabel(sb, this.customStateLabelProvider(this.finiteAutomatonContext.stateProvider(this.finiteAutomatonContext.fa.InitialState)));
                sb.Append("]");
            }
            sb.Append("\n\n");

            sb.Append("  //Accepting states\n  node ");
            AppendAcceptingStateAttributes(sb);
            foreach (Node node in this.nodes.Values)
            {
                if (this.finiteAutomatonContext.fa.AcceptingStates.Contains(node) && 
                    !node.Equals(finiteAutomatonContext.fa.InitialState) &&
                    !(this.safetyCheckIsOn && this.finiteAutomatonContext.unsafeNodes.Contains(node)))
                {
                    AddNodeLabel(sb, node);
                }
            }
            sb.Append("\n\n");

            if (this.livenessCheckIsOn)
            {
                sb.Append("  //Dead states\n  node ");
                AppendDeadStateAttributes(sb);
                foreach (Node node in this.nodes.Values)
                {
                    if (this.finiteAutomatonContext.deadNodes.Contains(node) && !node.Equals(finiteAutomatonContext.fa.InitialState))
                    {
                        AddNodeLabel(sb, node);
                    }
                }
            }
            sb.Append("\n\n");

            if (this.safetyCheckIsOn)
            {
                sb.Append("  //Unsafe states\n  node ");
                AppendUnsafeStateAttributes(sb);
                foreach (Node node in this.nodes.Values)
                {
                    if (this.finiteAutomatonContext.unsafeNodes.Contains(node) 
                        && !node.Equals(finiteAutomatonContext.fa.InitialState))
                    {
                        AddNodeLabel(sb, node, this.finiteAutomatonContext.fa.AcceptingStates.Contains(node));
                    }
                }
            }
            sb.Append("\n\n");

            sb.Append("  //Safe live nonaccepting states\n  node ");
            AppendNonAcceptingStateAttributes(sb);
            foreach (Node node in this.nodes.Values)
            {
                if (!this.finiteAutomatonContext.fa.AcceptingStates.Contains(node) && 
                    !node.Equals(finiteAutomatonContext.fa.InitialState) && 
                    !(this.livenessCheckIsOn && this.finiteAutomatonContext.deadNodes.Contains(node)) &&
                    !(this.safetyCheckIsOn && this.finiteAutomatonContext.unsafeNodes.Contains(node)) )
                {
                    AddNodeLabel(sb, node);
                }
            }
            sb.Append("\n\n");

            sb.Append("  //Transitions");
            foreach (MultiLabeledTransition t in this.transitions.Values)
            {
                sb.Append("\n  ");
                AppendLabel(sb, t.startState);
                sb.Append(" -> ");
                AppendLabel(sb, t.endState);
                String style="";
                if (dashedEdges.ContainsKey(t)) {
                     style="style=dashed";
                }
                if (this.transitionLabels != TransitionLabel.None || style.Length>0)
                {
                    sb.Append(" [ ");
                    bool comma = false;
                    if (this.transitionLabels != TransitionLabel.None)
                    {
                        sb.Append("label = ");
                        AppendLabel(sb, t.CombinedLabel(this.transitionLabels == TransitionLabel.ActionSymbol));
                        comma = true;
                    }
                    if (style.Length > 0)
                    {
                        if (comma)
                            sb.Append(", ");
                        sb.Append(style);
                    }
                    sb.Append(" ];");
                }
            }
            sb.Append("\n}\n");
            return sb.ToString();
        }

        private void AddNodeLabel(StringBuilder sb, Node node)
        {
            sb.Append("\n  ");
            AppendLabel(sb, node);
            if (this.customStateLabelProvider != null && this.finiteAutomatonContext.stateProvider != null)
            {
                sb.Append(" [label = ");
                AppendLabel(sb, this.customStateLabelProvider(this.finiteAutomatonContext.stateProvider(node)));
                sb.Append("]");
            }
        }

        private void AddNodeLabel(StringBuilder sb, Node node, bool accepting)
        {
            sb.Append("\n  ");
            AppendLabel(sb, node);
            if (this.customStateLabelProvider != null && this.finiteAutomatonContext.stateProvider != null)
            {
                sb.Append(" [label = ");
                AppendLabel(sb, this.customStateLabelProvider(this.finiteAutomatonContext.stateProvider(node)));
                if (accepting)
                    sb.Append(", peripheries = 2");
                sb.Append("]");
            }
            else if (accepting)
                sb.Append(" [peripheries = 2]");

        }

        void AppendAcceptingStateAttributes(StringBuilder sb)
        {
            sb.Append("[style = filled, shape = ");
            sb.Append(ToDotShape(this.stateShape));
            sb.Append(", peripheries = 2, fillcolor = white]");
        }

        void AppendDeadStateAttributes(StringBuilder sb)
        {
            sb.Append("[style = filled, shape = ");
            sb.Append(ToDotShape(this.stateShape));
            sb.Append(", peripheries = 1, fillcolor = ");
            sb.Append(ToDotColor(this.deadStateColor).ToString());
            sb.Append("]");
        }

        void AppendUnsafeStateAttributes(StringBuilder sb)
        {
            sb.Append("[style = filled, shape = ");
            sb.Append(ToDotShape(this.stateShape));
            sb.Append(", peripheries = 1, fillcolor = ");
            sb.Append(ToDotColor(this.unsafeStateColor).ToString());
            sb.Append("]");
        }

        void AppendNonAcceptingStateAttributes(StringBuilder sb)
        {
            sb.Append("[style = filled, shape = ");
            sb.Append(ToDotShape(this.stateShape));
            sb.Append(", peripheries = 1, fillcolor = white]");
        }

        void AppendInitialStateAttributes(StringBuilder sb)
        {
            sb.Append("[style = filled, shape = ");
            sb.Append(ToDotShape(this.stateShape));
            sb.Append(", peripheries = ");
            if (this.finiteAutomatonContext.fa.AcceptingStates.Contains(this.finiteAutomatonContext.fa.InitialState))
                sb.Append("2");
            else
                sb.Append("1");
            sb.Append(", fillcolor = ");
            sb.Append(ToDotColor(this.initialStateColor).ToString());
            sb.Append("]");
        }

        static void AppendLabel(StringBuilder sb, IComparable l)
        {
            sb.Append("\"");
            sb.Append(l.ToString().Replace("\"", "\\\"").Replace("\n", "\\n"));
            sb.Append("\"");
        }

        static string ToDotShape(StateShape shape)
        {
            switch (shape)
            {
                case StateShape.Box:
                    return "box";
                case StateShape.Circle:
                    return "circle";
                case StateShape.Diamond:
                    return "diamond";
                //case StateShape.DoubleCircle:
                //    return "doublecircle";
                case StateShape.Ellipse:
                    return "ellipse";
                case StateShape.Octagon:
                    return "octagon";
                default:
                    return "plaintext";
                //default:
                //    return "point";

            }
        }

        private void saveAsImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.saveToolStripSplitButton.Tag = "Image";
            this.saveToolStripSplitButton.Image = this.saveAsImageToolStripMenuItem.Image;
            this.saveToolStripSplitButton.ToolTipText = "Save as Image";
            SaveAsImage();
        }

        private void SaveAsImage()
        {
            this.viewer.SaveButtonPressed();
        }

        private void saveAsDotToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.saveToolStripSplitButton.Tag = "Dot";
            this.saveToolStripSplitButton.Image = this.saveAsDotToolStripMenuItem.Image;
            this.saveToolStripSplitButton.ToolTipText = "Save as Dot";
            SaveAsDot();
        }

        private void SaveAsDot()
        {
            DialogResult res = this.saveAsDotDialog.ShowDialog();
            if (res == DialogResult.OK && !String.IsNullOrEmpty(this.saveAsDotDialog.FileName))
            {
                System.IO.StreamWriter sw = new System.IO.StreamWriter(this.saveAsDotDialog.FileName);
                string dot = ToDot();
                sw.Write(dot);
                sw.Close();
            }
        }

        private void SaveAsFSM()
        {
            DialogResult res = this.saveAsFSMDialog.ShowDialog();
            if (res == DialogResult.OK && !String.IsNullOrEmpty(this.saveAsFSMDialog.FileName))
            {
                System.IO.StreamWriter sw = new System.IO.StreamWriter(this.saveAsFSMDialog.FileName);
                string fsmAsString = this.finiteAutomatonContext.fa.ToString();
                sw.Write(fsmAsString);
                sw.Close();
            }
        }

        private void saveToolStripSplitButton_ButtonClick(object sender, EventArgs e)
        {
            if (saveToolStripSplitButton.Tag.Equals("Image"))
            {
                this.viewer.SaveButtonPressed();
            }
            else
            {
                SaveAsDot();
            }
        }

        private void selectNodeContextMenuStrip_Opening(object sender, CancelEventArgs e)
        {

        }

        private void selectNextNodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.SelectNextNode();
        }

        private void selectPreviousNodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.SelectPreviousNode();
        }

        private void restoreDefaultsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RestoreDefaults();
        }

        private void sheHideHelpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.propertyGrid.HelpVisible = !this.propertyGrid.HelpVisible;
        }

        private void saveAsFSMToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.SaveAsFSM();
        }
    }

    internal class FAName : CompoundValue
    {
        readonly string name;
        readonly internal FAName left;
        readonly internal FAName right;

        public override IEnumerable<IComparable> FieldValues()
        {
            yield return name;
            yield return left;
            yield return right;
        }
        
        internal FAName(string name)
        {
            this.name = name;
            //this.left = null;
            //this.right = null;
        }
        internal FAName(FAName left, FAName right)
        {
            //this.name = null;
            this.left = left;
            this.right = right;
        }

        public override string ToString()
        {
            if (name != null) return name;
            else
            {
                return "(" + left.ToString() + " x " + right.ToString() + ")";
            }
        }
    }
}

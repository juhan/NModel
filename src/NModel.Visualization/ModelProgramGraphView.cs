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

using NModel.Algorithms;
using NModel;
using NModel.Terms;
using Transition = NModel.Triple<NModel.Terms.Term, NModel.Terms.CompoundTerm, NModel.Terms.Term>;
using Node = NModel.Terms.Term;
using NModel.Execution;
using NModel.Internals;
using GraphLayout = Microsoft.Glee;


namespace NModel.Visualization
{
    /// <summary>
    /// Supports incremental browsing of the state space of a model program
    /// </summary>
    public partial class ModelProgramGraphView : NModel.Visualization.GraphView
    {
        /// <summary>
        /// Create an instance of the model program graph viewer.
        /// </summary>
        public ModelProgramGraphView()
        {
            InitializeComponent();

            this.viewer.MouseClick += new MouseEventHandler(viewer_RightClick);

            this.viewer.KeyDown += new KeyEventHandler(viewer_KeyDown);
        }

        #region InitialTransitions
        /// <summary>
        /// Number of transitions that are explored initially up to maxTransitions. Negative value implies no bound. Default is -1.
        /// </summary>
        internal int initialTransitions = -1;
        /// <summary>
        /// Number of transitions that are explored initially up to MaxTransitions. Negative value implies no bound. Default is -1.
        /// </summary>
        public int InitialTransitions
        {
            get
            {
                return initialTransitions;
            }
            set
            {
                initialTransitions = value;
            }
        }
        #endregion

        void viewer_KeyDown(object sender, KeyEventArgs e)
        {
            //check that some node is selected and that the viewer is not showing 
            //a projection but the whole model that is being explored
            if (this.selectedNode != null && this.finiteAutomatonContext.reductName.treePosition.IsEmpty)
            {
                switch (e.KeyCode)
                {
                    case Keys.Add:     //show all transitions from the selected node
                    case Keys.Enter: 
                        this.exploredTransitions.ShowOutgoing(this.selectedNode);
                        this.InitializeViewer();
                        return;
                    case Keys.Subtract: //hide all transitions from the selected node
                    case Keys.Back:
                        this.exploredTransitions.HideOutgoing(this.selectedNode);
                        this.InitializeViewer();
                        return;
                    case Keys.NumPad0://do the same thing with num lock on
                    case Keys.Insert: //show all transitions from the selected node recursively
                        this.exploredTransitions.ShowReachable(this.selectedNode);
                        this.InitializeViewer();
                        return;
                    case Keys.Decimal://do the same thing with num lock on
                    case Keys.Delete: //hide all transitions from the selected node recursively
                        this.exploredTransitions.HideReachable(this.selectedNode);
                        this.InitializeViewer();
                        return;
                    default:
                        return;
                }
            }
        }

        void viewer_RightClick(object sender, MouseEventArgs e)//+++
        {
            //notice that the context menu is possible only in the root view 
            //i.e. when reduct of the fa in the current context has the empty tree position
            if (e.Button == MouseButtons.Right && this.finiteAutomatonContext.reductName.treePosition.IsEmpty)
            {
                if (viewer.SelectedObject is GraphLayout.Drawing.Node && this.exploredTransitions != null)
                {
                    GraphLayout.Drawing.Node currentGleeNode =
                        viewer.SelectedObject as GraphLayout.Drawing.Node;

                    //populate the context menu with actions that can be explored
                    Node currentNode = this.nodes[currentGleeNode];
                    Map<Symbol,Sequence<CompoundTerm>> enabledActions = Map<Symbol,Sequence<CompoundTerm>>.EmptyMap;
                    foreach (Symbol actionSymbol in this.exploredTransitions.GetEnabledActionSymbols(currentNode))
                    {
                        Sequence<CompoundTerm> actions = new Sequence<CompoundTerm>(this.exploredTransitions.GetEnabledActions(currentNode, actionSymbol));
                        if (actions.Count > 0)
                            enabledActions = enabledActions.Add(actionSymbol,actions);
                    }

                    bool someSymbolIsVisible = false;
                    bool allSymbolsAreFullyVisible = true;
                    //populate and show the expand context menu, one per action symbol
                    List<ToolStripMenuItem> expandItems = new List<ToolStripMenuItem>(enabledActions.Count);
                    foreach (Pair<Symbol, Sequence<CompoundTerm>> entry in enabledActions)
                    {
                        ToolStripMenuItem expandItem =
                           new ToolStripMenuItem(entry.First.FullName);
                        //expandItem.Tag = new Pair<Node, Sequence<CompoundTerm>>(currentNode, entry.Second);
                        //expandItem.Click += new EventHandler(actionSymbol_Click);
                        expandItem.DisplayStyle = ToolStripItemDisplayStyle.Text;
                        //expandItem.CheckOnClick = false;
                        ToolStripMenuItem[] subitems = new ToolStripMenuItem[entry.Second.Count];
                        bool someActionIsVisible = false;
                        bool allActionsAreVisible = true;
                        for (int i = 0; i < subitems.Length; i++)
                        {
                            subitems[i] = new ToolStripMenuItem(entry.Second[i].ToCompactString());
                            subitems[i].Tag = new Pair<Node, CompoundTerm>(currentNode, entry.Second[i]);
                            subitems[i].Click += new EventHandler(action_Click);
                            subitems[i].DisplayStyle = ToolStripItemDisplayStyle.Text;
                            subitems[i].CheckOnClick = false;
                            if (this.exploredTransitions.IsActionVisible(currentNode, entry.Second[i]))
                            {
                                subitems[i].Checked = true;
                                subitems[i].ToolTipText = "Uncheck to hide";
                                someActionIsVisible = true;
                                someSymbolIsVisible = true;
                            }
                            else
                            {
                                subitems[i].Checked = false;
                                subitems[i].ToolTipText = "Check to show";
                                allActionsAreVisible = false;
                                allSymbolsAreFullyVisible = false;
                            }
                        }

                        if (allActionsAreVisible)
                            expandItem.CheckState = CheckState.Checked;
                        else if (someActionIsVisible)
                            expandItem.CheckState = CheckState.Indeterminate;
                        else
                            expandItem.CheckState = CheckState.Unchecked;
                        //if (expandItem.CheckState == CheckState.Checked)
                        //    expandItem.ToolTipText = "Uncheck to hide all " + entry.First.FullName + " transitions from selected state";
                        //else
                        //    expandItem.ToolTipText = "Check to show all " + entry.First.FullName + " transitions from selected state";
                        
                        //add a menu item to hide all actions

                        #region showAll - menu item to show all actions
                        ToolStripMenuItem showAll = new ToolStripMenuItem("Show All");
                        showAll.Tag = new Pair<Node,Sequence<CompoundTerm>>(currentNode,entry.Second);
                        showAll.DisplayStyle = ToolStripItemDisplayStyle.Text;
                        showAll.CheckOnClick = false;
                        showAll.Checked = false;
                        showAll.Click +=new EventHandler(showAll_Click);
                        #endregion

                        #region hideAll - menu item to hide all actions
                        ToolStripMenuItem hideAll = new ToolStripMenuItem("Hide All");
                        hideAll.Tag = new Pair<Node,Symbol>(currentNode,entry.First);
                        hideAll.DisplayStyle = ToolStripItemDisplayStyle.Text;
                        hideAll.CheckOnClick = false;
                        hideAll.Checked = false;
                        hideAll.Click +=new EventHandler(hideAll_Click);
                        #endregion



                        expandItem.DropDownItems.AddRange(subitems);
                        expandItem.DropDownItems.Add(new ToolStripSeparator());
                        expandItem.DropDownItems.Add(showAll);
                        expandItem.DropDownItems.Add(hideAll);
                        expandItems.Add(expandItem);
                    }
                    this.expandToolStripMenuItem.DropDownItems.Clear();
                    this.expandToolStripMenuItem.DropDownItems.AddRange(expandItems.ToArray());

                    if (allSymbolsAreFullyVisible)
                        this.expandToolStripMenuItem.CheckState = CheckState.Checked;
                    else if (someSymbolIsVisible)
                        this.expandToolStripMenuItem.CheckState = CheckState.Indeterminate;
                    else
                        this.expandToolStripMenuItem.CheckState = CheckState.Unchecked;

                    this.exploreContextMenuStrip.Show(this.viewer, e.Location);
                }
            }
        }

        //void actionSymbol_Click(object sender, EventArgs e)
        //{
        //    ToolStripMenuItem item = (ToolStripMenuItem)sender;
        //    Pair<Node, Sequence<CompoundTerm>> node_actions = (Pair<Node, Sequence<CompoundTerm>>)item.Tag;
        //    if (item.CheckState == CheckState.Checked)
        //        //hide all transitions with the given action symbol from the given node
        //        this.exploredTransitions.HideAll(node_actions.First, node_actions.Second[0].FunctionSymbol1);
        //    else
        //        //show all transitions with the given action symbol from the given node
        //        foreach (CompoundTerm action in node_actions.Second)
        //            this.exploredTransitions.ShowTransition(node_actions.First, action);
        //    this.exploreContextMenuStrip.Hide();
        //    this.InitializeViewer();
        //}

        void hideAll_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = (ToolStripMenuItem)sender;
            Pair<Node, Symbol> node_symbol = (Pair<Node, Symbol>)item.Tag;
            this.exploredTransitions.HideAll(node_symbol.First, node_symbol.Second);
            this.InitializeViewer();
        }

        void showAll_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = (ToolStripMenuItem)sender;
            Pair<Node, Sequence<CompoundTerm>> node_actions = (Pair<Node, Sequence<CompoundTerm>>)item.Tag;
            foreach (CompoundTerm action in node_actions.Second)
                this.exploredTransitions.ShowTransition(node_actions.First, action);
            this.InitializeViewer();
        }

        void action_Click(object sender, EventArgs e)
        {
            //if the transition is not checked, show it otherwise hide it
            ToolStripMenuItem item = (ToolStripMenuItem)sender;
            Pair<Node, CompoundTerm> node_action = (Pair<Node, CompoundTerm>)item.Tag;
            if (!item.Checked)
                this.exploredTransitions.ShowTransition(node_action.First, node_action.Second);
            else
                this.exploredTransitions.HideTransition(node_action.First, node_action.Second);
            this.InitializeViewer();
        }
 
        /// <summary>
        /// Explored part of the model program
        /// </summary>
        ExploredTransitions exploredTransitions;

        // There is a declaration of
        // ModelProgram mp;
        // in the GraphView class, but these can not be merged
        // as they are used for different purposes.
        // mp is used for projections while this
        // is used for the whole model program.
        ModelProgram modelProgram;

        /// <summary>
        /// ResetModelProgram is a helper member that calls SetModelProgram.
        /// Called from GraphView.ExcludeIsomorphicStates
        /// </summary>
        protected override void ResetModelProgram() {
            this.SetModelProgram(this.modelProgram);
        }

        /// <summary>
        /// Sets the initial state of the view 
        /// </summary>
        /// <param name="modelProgram">given model program to be viewed</param>
        public void SetModelProgram(ModelProgram modelProgram1)
        {
            this.modelProgram = modelProgram1;
            this.exploredTransitions = new ExploredTransitions(modelProgram1, this.initialTransitions, this.maxTransitions);
            this.exploredTransitions.excludeIsomorphicStates = this.ExcludeIsomorphicStates;
            this.exploredTransitions.collapseExcludedIsomorphicStates = this.collapseExcludedIsomorphicStates;
            this.exploredTransitions.ShowReachable(this.exploredTransitions.initialNode);
            this.InitializeViewer();
        }

        void InitializeViewer()
        {
            this.SetStateMachine(this.exploredTransitions.GetFA(), this.exploredTransitions.GetModelState, this.modelProgram,this.exploredTransitions.groupingTransitions);
        }

        private void showAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.selectedNode != null)
            {
                this.exploredTransitions.ShowOutgoing(this.selectedNode);
                this.InitializeViewer();
            }
        }

        private void exploreToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.selectedNode != null)
            {
                this.exploredTransitions.ShowReachable(this.selectedNode);
                this.InitializeViewer();
            }
        }

        private void hideOutgoingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.selectedNode != null)
            {
                this.exploredTransitions.HideOutgoing(this.selectedNode);
                this.InitializeViewer();
            }

        }

        private void hideReachableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.selectedNode != null)
            {
                this.exploredTransitions.HideReachable(this.selectedNode);
                this.InitializeViewer();
            }
        }

        private void selectNextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.SelectNextNode();
        }

        private void selectPreviousToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.SelectPreviousNode();
        }

        internal override void RegenerateFiniteAutomaton()
        {
            //check that the viewer is not showing 
            //a projection but the whole model that is being explored
            if (this.finiteAutomatonContext != null && this.finiteAutomatonContext.reductName.treePosition.IsEmpty)
            {
               this.exploredTransitions.maxTransitions = this.MaxTransitions;
               this.exploredTransitions.ShowReachable(this.finiteAutomatonContext.fa.InitialState);
               this.InitializeViewer();

            }
        }

        private void showStateGraphToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.selectedNode != null)
            {
                IState state = this.exploredTransitions.stateMap[this.selectedNode];
                StateContainer<IState> sc = new StateContainer<IState>(this.modelProgram, state);
                // FSM fsm = FSM.Create("T(o1(),label2(),o2())", "T(o2(),label2(),o3())").Accept("o2()");
                FSM fsm = FSM.Create(sc.ExtractFSM(state));
                FsmModelProgram mp = new FsmModelProgram(fsm, this.selectedNode.ToString());
                Visualization.ModelProgramGraphViewForm form = new ModelProgramGraphViewForm("State graph viewer", true);
                form.View.StateShape = StateShape.Box;
                //form.View.SetModelProgram(mp);
                form.View.SetStateMachine(fsm,null);
                form.View.transitionLabels = TransitionLabel.ActionSymbol;
                form.Show();
            }
        }
    } 

    /// <summary>
    /// Represents the explored part of the model program
    /// </summary>
    internal class ExploredTransitions
    {
        internal Node initialNode;
        Set<Node> nodes;
        Set<Node> acceptingNodes;
        //Set<Node> errorNodes;
        Set<Transition> transitions;
        internal Set<Transition> groupingTransitions;
        ModelProgram modelProgram;
        internal Dictionary<Node, IState> stateMap;
        Dictionary<IState, Node> nodeMap;
        Dictionary<Node, Dictionary<CompoundTerm,Node>> actionsExploredFromNode;
        Set<Transition> hiddenTransitions;
        internal int maxTransitions;
        internal bool excludeIsomorphicStates;
        internal bool collapseExcludedIsomorphicStates;
        int initTransitions;

        internal ExploredTransitions(ModelProgram modelProgram, int initTransitions, int maxTransitions)
        {
            this.modelProgram = modelProgram;
            this.transitions = Set<Transition>.EmptySet;
            this.groupingTransitions = Set<Transition>.EmptySet;
            Node initNode = new Literal(0);
            this.initialNode = initNode;
            this.nodes = new Set<Node>(initNode);
            this.acceptingNodes = (modelProgram.IsAccepting(modelProgram.InitialState) ?
                new Set<Node>(initNode) :
                Set<Node>.EmptySet);
            //this.errorNodes = (!modelProgram.SatisfiesStateInvariant(modelProgram.InitialState) ?
            //    new Set<Node>(initNode) :
            //    Set<Node>.EmptySet);
            Dictionary<Node, IState> initialStateMap =
                new Dictionary<Node, IState>();
            initialStateMap[initNode] = modelProgram.InitialState;
            this.stateMap = initialStateMap;
            actionsExploredFromNode = new Dictionary<Node, Dictionary<CompoundTerm,Node>>();
            Dictionary<IState, Node> initialNodeMap =
                new Dictionary<IState, Node>();
            initialNodeMap[modelProgram.InitialState] = initNode;
            this.nodeMap = initialNodeMap;
            this.hiddenTransitions = Set<Transition>.EmptySet;
            this.maxTransitions = maxTransitions;
            this.initTransitions = initTransitions;
        }

        internal IState GetModelState(Node state)
        {
            return stateMap[state];
        }

        internal FSM GetFA()
        {
            //Remove isolated nodes from the view that are not the initial node
            Set<Node> visibleSourceNodes = this.transitions.Convert<Node>(delegate(Transition t) { return t.First; });
            Set<Node> visibleTargetNodes = this.transitions.Convert<Node>(delegate(Transition t) { return t.Third; });
            Set<Node> visibleNodes = visibleSourceNodes.Union(visibleTargetNodes).Add(this.initialNode);
            return new FSM(this.initialNode, visibleNodes, this.transitions, this.acceptingNodes.Intersect(visibleNodes));
        }

        internal Set<Symbol> GetEnabledActionSymbols(Node node)
        {
            IState istate = stateMap[node];
            return modelProgram.PotentiallyEnabledActionSymbols(istate);
        }

        internal IEnumerable<CompoundTerm> GetEnabledActions(Node node, Symbol actionSymbol)
        {
            IState istate = stateMap[node];
            return modelProgram.GetActions(istate, actionSymbol);
        }


        /// <summary>
        /// Returns true if the action is a label of a visible transition from the given node
        /// </summary>
        internal bool IsActionVisible(Node node, CompoundTerm action)
        {
            return this.transitions.Exists(delegate(Transition t) { return t.First.Equals(node) && t.Second.Equals(action); });
        }

        /// <summary>
        /// Show the transition with the given action from the given state
        /// </summary>
        internal void ShowTransition(Node sourceNode, CompoundTerm action)
        {
            Node targetNode;
            if (TryGetTargetNode(sourceNode, action, out targetNode))
            {
                Transition t = new Triple<Term, CompoundTerm, Term>(sourceNode, action, targetNode);
                this.transitions = this.transitions.Add(t);
                this.hiddenTransitions = this.hiddenTransitions.Remove(t);
            }
        }


        // This method has side effects on this.stateMap, this.nodeMap, this.nodes, this.acceptingNodes, this.actionsExploredFromNode
        private bool TryGetTargetNode(Node sourceNode, CompoundTerm action, out Node targetNode)
        {
            IState targetState;
            if (this.actionsExploredFromNode.ContainsKey(sourceNode))
            {
                if (!this.actionsExploredFromNode[sourceNode].TryGetValue(action, out targetNode))
                {
                    //this action has not been explored yet from the given node
                    TransitionProperties transitionProperties;
                    targetState = this.modelProgram.GetTargetState(stateMap[sourceNode], action, Set<string>.EmptySet, out transitionProperties);
                    if (!this.nodeMap.TryGetValue(targetState, out targetNode))
                    {
                        targetNode = new Literal(this.nodes.Count);
                        this.stateMap[targetNode] = targetState;
                        this.nodeMap[targetState] = targetNode;
                        this.nodes = this.nodes.Add(targetNode);
                        if (this.modelProgram.IsAccepting(targetState))
                            this.acceptingNodes = this.acceptingNodes.Add(targetNode);
                        //if (!this.modelProgram.SatisfiesStateInvariant(targetState))
                        //    this.errorNodes = this.errorNodes.Add(targetNode);
                    }
                }
                else
                {
                    targetState = this.stateMap[targetNode];
                }
            }
            else //the state has not yet been explored at all
            {
                TransitionProperties transitionProperties;
                targetState = this.modelProgram.GetTargetState(stateMap[sourceNode], action, Set<string>.EmptySet, out transitionProperties);
                if (!this.nodeMap.TryGetValue(targetState, out targetNode))
                {
                    targetNode = new Literal(this.nodes.Count);
                    this.stateMap[targetNode] = targetState;
                    this.nodeMap[targetState] = targetNode;
                    this.nodes = this.nodes.Add(targetNode);
                    if (this.modelProgram.IsAccepting(targetState))
                        this.acceptingNodes = this.acceptingNodes.Add(targetNode);
                    //if (!this.modelProgram.SatisfiesStateInvariant(targetState))
                    //    this.errorNodes = this.errorNodes.Add(targetNode);
                }
                Dictionary<CompoundTerm, Node> actionsFromState = new Dictionary<CompoundTerm, Node>();
                actionsFromState[action] = targetNode;
                this.actionsExploredFromNode[sourceNode] = actionsFromState;
            }
            return this.modelProgram.SatisfiesStateFilter(targetState);
        }
        
        /// <summary>
        /// Show all transitions from the given node
        /// </summary>
        internal void ShowOutgoing(Node node)
        {
            foreach (Symbol aSymbol in this.modelProgram.PotentiallyEnabledActionSymbols(stateMap[node]))
            {
                foreach (CompoundTerm a in this.modelProgram.GetActions(stateMap[node], aSymbol))
                {
                    this.ShowTransition(node, a);
                }
            }
        }

        bool firstExploration = true;
        /// <summary>
        /// Show all transitions from the given node and from the nodes that are reached from 
        /// the given node etc., until the maximum nr of transitions is reached
        /// </summary>
        internal void ShowReachable(Node node)
        {

            int transCnt = (firstExploration ? (initTransitions < 0 ? maxTransitions : initTransitions) : maxTransitions);
            firstExploration = false;

            if (excludeIsomorphicStates)
            {
                Set<IState> frontier = new Set<IState>(stateMap[node]);
                StateContainer<IState> visited = new StateContainer<IState>(this.modelProgram, stateMap[node]);
                while (!frontier.IsEmpty && this.transitions.Count < transCnt)
                {
                    IState sourceIState = frontier.Choose(0);
                    Node sourceNode = nodeMap[sourceIState];
                    frontier = frontier.Remove(sourceIState);
                    foreach (Symbol aSymbol in this.modelProgram.PotentiallyEnabledActionSymbols(sourceIState))
                    {
                        foreach (CompoundTerm action in this.modelProgram.GetActions(sourceIState, aSymbol))
                        {
                            Node targetNode;
                            if (TryGetTargetNode(sourceNode, action, out targetNode))
                            {
                                IState targetIState = stateMap[targetNode];
                                IState isomorphicState;
                                Transition t;
                                if (!visited.HasIsomorphic(targetIState, out isomorphicState))
                                {
                                    frontier = frontier.Add(targetIState);
                                    //visited = visited.Add(targetIState);
                                    visited.Add(targetIState);
                                    t = new Triple<Term, CompoundTerm, Term>(sourceNode, action, targetNode);
                                }
                                else
                                {
                                    if (collapseExcludedIsomorphicStates)
                                        t = new Triple<Term, CompoundTerm, Term>(sourceNode, action, nodeMap[isomorphicState]);
                                    else
                                    {
                                        Term isoNode = nodeMap[isomorphicState];
                                        t = new Triple<Term, CompoundTerm, Term>(sourceNode, action, targetNode);
                                        if (!targetNode.Equals(sourceNode) && !targetNode.Equals(isoNode))
                                            groupingTransitions = groupingTransitions.Add(new Triple<Term, CompoundTerm, Term>(targetNode, new CompoundTerm(new Symbol("IsomorphicTo"), new Sequence<Term>()), isoNode));
                                    }
                                }
                                this.transitions = this.transitions.Add(t);
                                this.hiddenTransitions = this.hiddenTransitions.Remove(t);
                            }
                        }
                    }
                }
                //Console.WriteLine(dashedTransitions.ToString());
                //Console.WriteLine(visited.ToString());
            }
            else
            {
                Set<Node> frontier = new Set<Node>(node);
                Set<Node> visited = frontier;

                while (!frontier.IsEmpty && this.transitions.Count < transCnt)
                {
                    Node sourceNode = frontier.Choose(0);
                    frontier = frontier.Remove(sourceNode);
                    foreach (Symbol aSymbol in this.modelProgram.PotentiallyEnabledActionSymbols(stateMap[sourceNode]))
                    {
                        foreach (CompoundTerm action in this.modelProgram.GetActions(stateMap[sourceNode], aSymbol))
                        {
                            Node targetNode;
                            if (TryGetTargetNode(sourceNode, action, out targetNode))
                            {
                                if (!visited.Contains(targetNode))
                                {
                                    frontier = frontier.Add(targetNode);
                                    visited = visited.Add(targetNode);
                                }
                                Transition t = new Triple<Term, CompoundTerm, Term>(sourceNode, action, targetNode);
                                this.transitions = this.transitions.Add(t);
                                this.hiddenTransitions = this.hiddenTransitions.Remove(t);
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Hide all previously shown transitions from the given node
        /// </summary>
        /// <param name="node">given node</param>
        internal void HideOutgoing(Node node)
        {
            this.hiddenTransitions = this.hiddenTransitions.Union(this.transitions.Select(delegate(Transition t) { return t.First.Equals(node); }));
            this.transitions = this.transitions.Difference(this.hiddenTransitions);
        }

        /// <summary>
        /// Hide the transition with the given action from the given node
        /// </summary>
        internal void HideTransition(Node node, CompoundTerm action)
        {
            this.hiddenTransitions = this.hiddenTransitions.Union(this.transitions.Select(delegate(Transition t) { return t.First.Equals(node) && t.Second.Equals(action); }));
            this.transitions = this.transitions.Difference(this.hiddenTransitions);
        }

        /// <summary>
        /// Hide all previously shown transitions with te given action symbol from the given node
        /// </summary>
        internal void HideAll(Node node, Symbol actionSymbol)
        {
            this.hiddenTransitions = this.hiddenTransitions.Union(this.transitions.Select(delegate(Transition t) { return t.First.Equals(node) && ((CompoundTerm)t.Second).Symbol.Equals(actionSymbol); }));
            this.transitions = this.transitions.Difference(this.hiddenTransitions);
        }

        /// <summary>
        /// Hide all previously shown transitions from the given node
        /// and those that can be reached recursively
        /// </summary>
        internal void HideReachable(Node node)
        {
            Set<Transition> alltransitions = this.hiddenTransitions.Union(this.transitions);
            Set<Node> frontier = new Set<Term>(node);
            Set<Node> visited = frontier;
            Set<Transition> tobehidden = Set<Transition>.EmptySet;
            while (!frontier.IsEmpty)
            {
                Node n = frontier.Choose(0);
                frontier = frontier.Remove(n);
                Set<Transition> toBeHiddenTransitions =
                   alltransitions.Select(delegate(Transition t) { return t.First.Equals(n); });
                Set<Node> targetStates =
                    toBeHiddenTransitions.Convert<Node>(delegate(Transition t) { return t.Third; });
                frontier = frontier.Union(targetStates.Difference(visited));
                visited = visited.Union(targetStates);
                tobehidden = tobehidden.Union(toBeHiddenTransitions);
            }
            this.hiddenTransitions = this.hiddenTransitions.Union(tobehidden);
            this.transitions = this.transitions.Difference(tobehidden);
        }
    }
}


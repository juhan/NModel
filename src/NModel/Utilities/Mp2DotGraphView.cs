using System;
using System.Text;
// using System.Drawing; // keep or remove?
using System.ComponentModel;
// using System.Windows.Forms;  // Does not need Windows.Forms 
using System.Collections.Generic;

using NModel.Algorithms;
using NModel;
using NModel.Terms;
using Transition = NModel.Triple<NModel.Terms.Term, NModel.Terms.CompoundTerm, NModel.Terms.Term>;
using Node = NModel.Terms.Term;
using NModel.Execution;
using NModel.Internals;
// using GraphLayout = Microsoft.Glee;  // Does not need Glee

// This file copied from NModel.Visualization GraphView.cs and renamed here in NModel.Utilities.Graph
// then edited to remove dependency on Windows.Forms and Glee, 
// and to remove all code not needed for mp2dot 
// They payload here is GraphView.ToDot, which writes dot output.

// namespace NModel.Visualization
namespace NModel.Utilities.Graph
{
    // declaration removed, already present in DotWriter.cs
    //prepublic delegate IState StateProvider(Term state);

    /// <summary>Use this Color type for Dot instead of System.Drawing.Color</summary>
    // Rather convoluted here so we can use same (or similar) caller code
    public struct Color
    {
        /// <summary>color name string</summary>
        public string color; // "Red", "Yellow", "Gray" etc.
        /// <summary>Constructor, parameter is color name string</summary>
        public Color(string c) { color = c; }
        /// <summary>Similar to constructor, make Color from color name string</summary>
        public static Color FromName(string c) { return new Color(c); }
    }

    // ToDotColor is static method in GraphView

    /// <summary>
    /// Used for creating an object, NOT a Windows form, that contains the GraphView instance
    ///  This class plays similar role in mp2dot and NModel.Utilities.Graph 
    ///  that class ModelProgramGraphViewForm plays in mpv and NModel.Visualization
    /// </summary>
    public class DummyGraphViewForm
    {
        /// <summary>
        /// GraphView instance in DummyGraphViewForm
        /// </summary>
        public GraphView View;

        /// <summary>
        /// Creates an object, not a Windows form, containing the GraphView instance.
        /// </summary>
        /// <param name="title">used as the title of the form if title != null</param>
        public DummyGraphViewForm(string title)
        {
            // title arg is just so it resembles NModel.Visualization ModelProgramGraphView, not used
            this.View = new GraphView();
        }
    }

    // Copied from NModel.Visualization ModelProgramGraphView.cs

    /// <summary>
    /// Represents the explored part of the model program
    /// </summary>
    internal class ExploredTransitions
    {
        internal Node initialNode;
        Set<Node> nodes;
        internal Set<Node> acceptingNodes;
        //Set<Node> errorNodes;
        Set<Transition> transitions;
        internal Set<Transition> groupingTransitions;
        ModelProgram modelProgram;
        internal Dictionary<Node, IState> stateMap;
        Dictionary<IState, Node> nodeMap;
        Dictionary<Node, Dictionary<CompoundTerm, Node>> actionsExploredFromNode;
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
            actionsExploredFromNode = new Dictionary<Node, Dictionary<CompoundTerm, Node>>();
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

    /// <summary>
    /// Displays a finite state machine graph.
    /// </summary>
    public partial class GraphView
    {
        // Code copied frm GraphView.Settings.cs in NModel.Visualization follows
        // NOW this code has been moved toMp2DotGraphView.Settings.cs
        // ... moved ...

        // Code copied or modified from ModelProgramGraphView.cs in NModel.Visualization follows
        // ModelProgramGraphView class inherits GraphView class, adds these additional members
        // Here we include them all in GraphView, no separate Mp2DotGraphView class.

        /// <summary>
        /// Explored part of the model program
        /// </summary>
        internal ExploredTransitions exploredTransitions;

        // There is a declaration of
        // ModelProgram mp;
        // in the GraphView class, but these can not be merged
        // as they are used for different purposes.
        // mp is used for projections while this
        // is used for the whole model program.
        ModelProgram modelProgram;

        /// <summary>
        /// Sets the initial state of the view 
        /// </summary>
        /// <param name="modelProgram1">given model program to be viewed</param>
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
            this.SetStateMachine(this.exploredTransitions.GetFA(), this.exploredTransitions.GetModelState, this.modelProgram, this.exploredTransitions.groupingTransitions);
        }
        
        // Code copied or modified from Graphview.cs in NModel.Visualization follows

        // Color name strings originally chosen for .NET work with Dot also, fortunately
        static string ToDotColor(Color c) { return c.color; }

        // removed several members here

        /// <summary>
        /// Current context regarding which finite automaton is being viewed.
        /// Is a reduct of the top level machine.
        /// </summary>
        internal FAContext finiteAutomatonContext;

        /// <summary>
        /// Info regarding the top level FA that was set.
        /// </summary>
        FAInfo faInfo;

        /// <summary>
        /// Creates a new graph giew
        /// </summary>
        public GraphView()
        {
            // removed code here
        }

        // removed several members here

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
            // nodes.Clear(); // not needed for mp2dot, I believe
            // transitions.Clear(); // ditto
            //reductCache.Clear();
            //this.faInfo = new FAInfo(this.stateViewer1.DefaultModelName, fa, stateProvider, mp1);
            this.faInfo = new FAInfo("Fsm", fa, stateProvider, mp1); // defined in StateView.cs
            this.finiteAutomatonContext = new FAContext(fa, stateProvider, faInfo.ReductNames.Last, mp1);
            this.finiteAutomatonContext.groupingTransitions = groupingTransitions;
            //this.reductCache[this.faInfo.ReductNames.Last] = this.finiteAutomatonContext;

            // removed code here
        }

        // add this
        internal void WriteExplorationStatistics()
        {
            Console.WriteLine("{0,5} states", States); //finiteAutomatonContext.fa.States.Count);
            Console.WriteLine("{0,5} transitions", Transitions); //finiteAutomatonContext.fa.Transitions.Count);
            Console.WriteLine("{0,5} accepting states", AcceptingStates); //exploredTransitions.acceptingNodes.Count);
            Console.WriteLine("{0,5} dead states", DeadStates); //finiteAutomatonContext.deadNodes.Count);
            Console.WriteLine("{0,5} unsafe states", UnsafeStates); //finiteAutomatonContext.unsafeNodes.Count);
        }

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

        CustomLabelProvider customStateLabelProvider;

        // used for combineActions
        Set<Node> hiddenMealyNodes = Set<Node>.EmptySet;

        // The set is used for generating dot output.
        Dictionary<MultiLabeledTransition,int> dashedEdges = new Dictionary<MultiLabeledTransition,int>();

        // removed members here, including GetActionLabel, GetArgumentLabel, apparently used only for GLEE

        // #region User events 
        // removed members here
        
        //represents transitions represented by a single glee edge
        //is referenced from DotWriter (actually it should be in the Utilities.Graph namespace)
        // internal class MultiLabeledTransition
        // ....
        // Removed! This was copied from NModel.Visualization.GraphView in GraphView.cs
        // Use NModel.Utilities.Graph.MultilabeledTransition in DotWriter.cs instead
        
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


            // This was a block of code in GraphView.ToDot, move here because it assigns unsafeNodes
            internal void CollectUnsafeNodes()
            {
                foreach (Node node in fa.States)
                {
                    IState istate = stateProvider(node);
                    if (!mp.SatisfiesStateInvariant(istate))
                    {
                        unsafeNodes = unsafeNodes.Add(node);
                    }
                }
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

        // members removed

        /// <summary>
        /// Produce dot output of the graph
        /// </summary>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        internal string ToDot()
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
            foreach (Node node in this.finiteAutomatonContext.fa.States)
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
                foreach (Node node in this.finiteAutomatonContext.fa.States)
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

                // Code here assigned unsafeNodes, now moved to FAContext.CollectUnsafeNodes

                foreach (Node node in this.finiteAutomatonContext.fa.States)
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
            foreach (Node node in this.finiteAutomatonContext.fa.States)
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
            foreach (Transition ta in this.finiteAutomatonContext.fa.Transitions)
            {
                MultiLabeledTransition t = MultiLabeledTransition.Create(ta.First, ta.Second, ta.Third);
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

        // members removed
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

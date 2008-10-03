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
            if (a == null) return false;
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
        /// Dictionary of nodes to be drawn by dot, after preprocessing by graphWorker.
        /// Based on nodes in NModel.Visualization GraphView
        /// Key of dictionary here is just incrementing integers
        /// NO THAT DIDN'T WORK - got duplicate values.  
        /// So let's use *same* item for both key and value
        /// Use Dictionary not Set because MultiLabeledTransition (below) is apparently not IComparable
        /// </summary>
        // internal Dictionary<GraphLayout.Drawing.Node, Node> nodes = new Dictionary<GraphLayout.Drawing.Node, Node>();
        // internal Set<Node> nodes = new Set<Node>();  
        // internal Dictionary<int, Node> nodes = new Dictionary<int, Node>();
        internal Dictionary<Node, Node> nodes = new Dictionary<Node, Node>();

        /// <summary>
        /// Dictionary of transitions to be drawn by dot, after preprocessing by graphWorker.
        /// Based transitions in NModel.Visualization GraphView
        /// Key of dictionary here is just incrementing integers
        /// Use Dictionary not Set because MultiLabeledTransition (below) is apparently not IComparable
        /// </summary>
        // private Dictionary<GraphLayout.Drawing.Edge, MultiLabeledTransition> transitions = new Dictionary<GraphLayout.Drawing.Edge, MultiLabeledTransition>(); 
        // private Set<MultiLabeledTransition> transitions = new Set<MultiLabeledTransition>();
        private Dictionary<int, MultiLabeledTransition> transitions = new Dictionary<int, MultiLabeledTransition>();

        /// <summary>
        /// Pre-process graph data before calling ToDot
        /// Reads data in finiteAutomatonContext, writes the two sets nodes and transitions
        /// Based on NModel.Visualization GraphView graphWorker_DoWork
        /// Comment out all uses of GLEE, events, and threads
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        // private void graphWorker_DoWork(object/*?*/ sender, DoWorkEventArgs/*?*/ e)
        internal void graphWorker()
        //^ requires sender is BackgroundWorker;
        {
            // BackgroundWorker worker = (BackgroundWorker)sender;

            // Combine the inputs and outputs if desired.
            // FiniteStateMachine fsm = combineActions ? machine.CombineInputsAndOutputs() : machine;

            int nkey = 0; // integer key into nodes dictionary
            int tkey = 0; // integer key into transitions dictionary

            // Clear the node and transition tables.
            nodes.Clear();
            // nodes.EmptySet;
            transitions.Clear();
            // transitions.EmptySet;
            dashedEdges.Clear();

            // Create a new GLEE graph.
            // Microsoft.Glee.Drawing.Graph graph = new Microsoft.Glee.Drawing.Graph("Finite Automaton");

            // Set the layout direction.
            // graph.GraphAttr.LayerDirection = LayerDirection();

            // Report the total number of states and transitions.
            // int progress = 0;
            // worker.ReportProgress(progress, finiteAutomatonContext.fa.States.Count + Math.Min(maxTransitions, finiteAutomatonContext.fa.Transitions.Count));

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
                        string actionname = ((CompoundTerm)enteringTransitions[n][0].Second).Symbol.Name.Replace("_Start", "");
                        if (exitingTransitions[n].Count >= 1
                            && AllTransitionsAreFinish(actionname, exitingTransitions[n]))
                        {
                            hiddenMealyNodes = hiddenMealyNodes.Add(n);
                        }
                    }
                }
            }

            #region Add the initial state first.
            // GraphLayout.Drawing.Node initialNode = graph.AddNode(finiteAutomatonContext.fa.InitialState.ToString());
            if (this.nodeLabelsVisible)
            {
                if (this.finiteAutomatonContext.stateProvider != null && this.customStateLabelProvider != null)
                {
                    // NB no non-Glee code in this block
                    // initialNode.Attr.Label = this.customStateLabelProvider(this.finiteAutomatonContext.stateProvider(this.finiteAutomatonContext.fa.InitialState));
                }
            }
            else
            {
                // NB no non-Glee code in this block
                // initialNode.Attr.Label = "";
            }
            // NB no non-Glee code in this block
            //initial progress, one node is handled
            // worker.ReportProgress(++progress);
            // initialNode.Attr.Fillcolor = new GraphLayout.Drawing.Color(initialStateColor.R, initialStateColor.G, initialStateColor.B);
            // initialNode.Attr.Shape = MapToGleeShape(this.stateShape);
            #endregion

            #region Add the transitions by walking the graph depth first

            Stack<Node> stack = new Stack<Node>();
            Set<Node> visited = Set<Node>.EmptySet;
            //Notice the invariant: nodes on the stack can not be hidden nodes
            //base case: initial state cannot be hidden because 
            //it must have at least one exiting start action or atomic action
            stack.Push(finiteAutomatonContext.fa.InitialState);
            visited = visited.Add(finiteAutomatonContext.fa.InitialState);
            // while (stack.Count > 0 && graph.EdgeCount < maxTransitions)
            while (stack.Count > 0 && transitions.Count < maxTransitions)
            {
                //invariant: nodes on the stack are not hidden nodes
                Node current = stack.Pop();
                // GraphLayout.Drawing.Node fromNode = graph.AddNode(current.ToString());

                // nodes[fromNode] = current;
                // nodes[nkey++] = current;
                nodes[current] = current;
                // Console.WriteLine("graphWorker, 1 (line 833): nodes.Count {0}", nodes.Count);
                // nodes = nodes.Add(current);

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

                    // worker.ReportProgress(++progress); //one transition is handled
                    if (hiddenMealyNodes.Contains(t.Third))
                    {
                        //construct Mealy view of all the exiting transitions from t.Third
                        foreach (Transition tf in exitingTransitions[t.Third])
                        {
                            // worker.ReportProgress(++progress);
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
                                // worker.ReportProgress(++progress);
                            }
                            // add edge
                            // GraphLayout.Drawing.Node toNode = graph.AddNode(tf.Third.ToString());
                            // toNode.Attr.Shape = MapToGleeShape(this.stateShape);
                            if (this.nodeLabelsVisible)
                            {
                                if (this.finiteAutomatonContext.stateProvider != null && this.customStateLabelProvider != null)
                                {
                                    // NB no non-Glee code in this block
                                    // toNode.Attr.Label = this.customStateLabelProvider(this.finiteAutomatonContext.stateProvider(tf.Third));
                                }
                            }
                            // else
                                // NB no non-Glee code in this block
                                // toNode.Attr.Label = "";
                            // nodes[toNode] = tf.Third;
                            // nodes[nkey++] = tf.Third;
                            nodes[tf.Third] = tf.Third;
                            // Console.WriteLine("graphWorker, 2 (line 885): nodes.Count {0}", nodes.Count);
                            // nodes = nodes.Add(tf.Third);
                            // if (loopsVisible || fromNode.Attr.Id != toNode.Attr.Id)
                            if (loopsVisible || !current.Equals(tf.Third))
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
                            //worker.ReportProgress(++progress);
                        }
                        // add edge
                        // GraphLayout.Drawing.Node toNode = graph.AddNode(t.Third.ToString());
                        // toNode.Attr.Shape = MapToGleeShape(this.stateShape);
                        if (this.nodeLabelsVisible)
                        {
                            if (this.finiteAutomatonContext.stateProvider != null && this.customStateLabelProvider != null)
                            {
                                // NB no non-Glee code in this block
                                // toNode.Attr.Label = this.customStateLabelProvider(this.finiteAutomatonContext.stateProvider(t.Third));
                            }
                        }
                        // else
                            // NB no non-Glee code in this block
                            // toNode.Attr.Label = "";
                        // nodes[toNode] = t.Third;
                        // nodes[nkey++] = t.Third;
                        nodes[t.Third] = t.Third;
                        // Console.WriteLine("graphWorker, 3 (line 934): nodes.Count {0}", nodes.Count);
                        // nodes = nodes.Add(t.Third);
                        // if (loopsVisible || !fromNode.Attr.Id.Equals(toNode.Attr.Id))
                        if (loopsVisible || !current.Equals(t.Third))
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
                        //worker.ReportProgress(++progress);
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

                    // GraphLayout.Drawing.Node toNode = graph.AddNode(trans.endState.ToString());
                    if (mergeLabels)
                    {
                        string lab = (this.transitionLabels != TransitionLabel.None ? trans.CombinedLabel(this.transitionLabels == TransitionLabel.ActionSymbol) : "");
                        // GraphLayout.Drawing.Edge edge = graph.AddEdge(fromNode.Attr.Id, lab, toNode.Attr.Id);
                        // transitions[edge] = trans;
                        transitions[tkey++] = trans;
                        // transitions = transitions.Add(trans);
                    }
                    else
                    {
                        foreach (MultiLabeledTransition tr in trans.CreateOnePerLabel())
                        {
                            string lab = (this.transitionLabels != TransitionLabel.None ? tr.CombinedLabel(this.transitionLabels == TransitionLabel.ActionSymbol) : "");
                            // GraphLayout.Drawing.Edge edge = graph.AddEdge(fromNode.Attr.Id, lab, toNode.Attr.Id);
                            // transitions[edge] = tr;
                            transitions[tkey++] = tr;
                            // transitions = transitions.Add(tr);
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

                // GraphLayout.Drawing.Node toNd = graph.AddNode(trans.endState.ToString());
                // GraphLayout.Drawing.Node fromNd = graph.AddNode(trans.startState.ToString());
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
                // GraphLayout.Drawing.Edge edge = graph.AddEdge(fromNd.Attr.Id, lab, toNd.Attr.Id);
                // GraphLayout.Drawing.BaseAttr eattr = edge.EdgeAttr as GraphLayout.Drawing.BaseAttr;
                // eattr.AddStyle(GraphLayout.Drawing.Style.Dashed);
                // transitions[edge] = trans;
                transitions[tkey++] = trans;
                // transitions = transitions.Add(trans);
                dashedEdges.Add(trans, 0);
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


                        // GraphLayout.Drawing.Node n1 = graph.AddNode(n.ToString());
                        // n1.Attr.Color = ToGleeColor(Color.Gray);
                        // n1.Attr.Fontcolor = ToGleeColor(Color.Gray);
                        // n1.Attr.Shape = MapToGleeShape(this.StateShape);
                        // nodes[n1] = n;
                        // nodes[nkey++] = n;
                        nodes[n] = n;
                        // Console.WriteLine("graphWorker, 4 (line 1056): nodes.Count {0}", nodes.Count);
                        // nodes = nodes.Add(n);
                        if (!this.nodeLabelsVisible)
                        {
                            // NB no non-Glee code in this block
                            // n1.Attr.Label = "";
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
                            // GraphLayout.Drawing.Node n2 = graph.AddNode(t1.endState.ToString());
                            // nodes[n2] = t1.endState;
                            // nodes[nkey++] = t1.endState;
                            nodes[t1.endState] = t1.endState;
                            // Console.WriteLine("graphWorker, 5 (line 1084): nodes.Count {0}", nodes.Count);
                            // nodes = nodes.Add(t1.endState);
                            if (mergeLabels)                                              
                            {
                                string lab = (this.transitionLabels != TransitionLabel.None ? t1.CombinedLabel(this.transitionLabels == TransitionLabel.ActionSymbol) : "");
                                // GraphLayout.Drawing.Edge e1 = graph.AddEdge(n1.Attr.Id, lab, n2.Attr.Id);
                                // e1.EdgeAttr.Fontcolor = ToGleeColor(Color.Gray);
                                // e1.EdgeAttr.Color = ToGleeColor(Color.Gray);
                                // transitions[e1] = t1;
                                transitions[tkey++] = t1;
                                // transitions = transitions.Add(t1);
                            }
                            else
                            {
                                foreach (MultiLabeledTransition t2 in t1.CreateOnePerLabel())
                                {
                                    // string lab = (this.transitionLabels != TransitionLabel.None ? t2.CombinedLabel(this.transitionLabels == TransitionLabel.ActionSymbol) : "");
                                    // GraphLayout.Drawing.Edge e1 = graph.AddEdge(n1.Attr.Id, lab, n2.Attr.Id);
                                    // e1.EdgeAttr.Fontcolor = ToGleeColor(Color.Gray);
                                    // e1.EdgeAttr.Color = ToGleeColor(Color.Gray);
                                    // transitions[e1] = t2;
                                    transitions[tkey++] = t2;
                                    // transitions = transitions.Add(t2);
                                }
                            }
                        }
                        // if (graph.EdgeCount >= maxTransitions)
                        if (transitions.Count >= maxTransitions)
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
                    // NB no non-Glee code in this block
                    // GraphLayout.Drawing.Node deadGleeNode = graph.AddNode(deadNode.ToString());
                    //deadGleeNode.Attr.Fillcolor =
                    // new GraphLayout.Drawing.Color(deadStateColor.R, deadStateColor.G, deadStateColor.B);
                }
            }

            //while (stack.Count > 0)
            //{
            //    Node truncatedNode = stack.Pop();
            //    GraphLayout.Drawing.Node truncatedGleeNode = graph.AddNode(truncatedNode.ToString());
            //    truncatedGleeNode.Attr.Fillcolor =
            //        new GraphLayout.Drawing.Color(truncatedStateColor.R, truncatedStateColor.G, truncatedStateColor.B);
            //}

            // Mark accepting states with a thicker line
            // TBD: waiting for Lev to provide double-line feature
            if (this.acceptingStatesMarked)
            {
                foreach (Node accNode in finiteAutomatonContext.fa.AcceptingStates.Intersect(visited))
                {
                    // NB no non-Glee code in this block
                    // GraphLayout.Drawing.Node acceptingGleeNode = graph.AddNode(accNode.ToString());
                    // acceptingGleeNode.Attr.Shape = MapToGleeShape(stateShape);
                    // acceptingGleeNode.Attr.LineWidth = 4;
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
                            // NB update finiteAutomatonContext here to indicate unsafe nodes
                            this.finiteAutomatonContext.unsafeNodes =
                                this.finiteAutomatonContext.unsafeNodes.Add(visitedNode);
                            // GraphLayout.Drawing.Node visitedGleeNode = graph.AddNode(visitedNode.ToString());
                            // visitedGleeNode.Attr.Shape = MapToGleeShape(stateShape);
                            // visitedGleeNode.Attr.Fillcolor = ToGleeColor(this.unsafeStateColor);
                        }
                    }
                }
            }
            #endregion

            // Start the marquee mode of the progress bar.
            // worker.ReportProgress(-1);

            //insert a delay to avoid a crash in this version of GLEE
            // System.Threading.Thread.Sleep(200);

            // Return the calculated layout as the result.
            // e.Result = viewer.CalculateLayout(graph);
        }

        /// COPIED FROM NModel.Visualization  GraphView ToDot
        /// contains: foreach (Node node in this.nodes.Values) ...
        /// <summary>
        /// Produce dot output of the graph
        /// </summary>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        internal string ToDot()
        {
            // Console.WriteLine("ToDot: this.nodes.Count {0}, this.transitions.Count {1}", this.nodes.Count, this.transitions.Count);
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
                    !(this.safetyCheckIsOn && this.finiteAutomatonContext.unsafeNodes.Contains(node)))
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
                String style = "";
                if (dashedEdges.ContainsKey(t))
                {
                    style = "style=dashed";
                }
                if (this.transitionLabels != TransitionLabel.None || style.Length > 0)
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

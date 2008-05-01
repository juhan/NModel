//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using NModel.Terms;
using NModel.Execution;
using Transition = NModel.Triple<NModel.Terms.Term, NModel.Terms.CompoundTerm, NModel.Terms.Term>;

namespace NModel.Utilities.Graph
{
    // These settings make it possible to write Dot files independeltly of the 
    // Visualization package. 
    /// <summary>
    /// Describes the direction of graph layout.
    /// </summary>
    public enum GraphDirection
    {
        /// <summary>
        /// Layout direction is from top to bottom
        /// </summary>
        TopToBottom,
        /// <summary>
        /// Layout direction is from left to right
        /// </summary>
        LeftToRight,
        /// <summary>
        /// Layout direction is from right to left
        /// </summary>
        RightToLeft,
        /// <summary>
        /// Layout direction is from bottom to top
        /// </summary>
        BottomToTop
    } 

    /// <summary>
    /// Possible shapes of states
    /// </summary>
    public enum StateShape
    {
        /// <summary>
        /// Rectangular box
        /// </summary>
        Box,
        /// <summary>
        /// Round circle
        /// </summary>
        Circle,
        /// <summary>
        /// Diamond
        /// </summary>
        Diamond,
        /// <summary>
        /// Ellipse
        /// </summary>
        Ellipse,
        /// <summary>
        /// Octagon
        /// </summary>
        Octagon,
        /// <summary>
        /// Plain text with no surrounding border
        /// </summary>
        Plaintext
    }

    /// <summary>
    /// Determines what is shown as a transition label.
    /// </summary>
    public enum TransitionLabel
    {
        /// <summary>
        /// Label is omitted.
        /// </summary>
        None,
        /// <summary>
        /// Action symbol is shown.
        /// </summary>
        ActionSymbol,
        /// <summary>
        /// Full action is shown.
        /// </summary>
        Action
    }

    /// <summary>
    /// Delegate that maps an IState to a string that is used to label it.
    /// </summary>
    public delegate string CustomLabelProvider(IState state);

    /// <summary>
    /// Delegate that maps a term to a model program state
    /// </summary>
    /// <param name="state">given term</param>
    /// <returns>corresponding model program state</returns>
    public delegate IState StateProvider(Term state);

    /// <summary>
    /// Represents transitions that may have multiple labels
    /// </summary>
    public class MultiLabeledTransition
    {
        internal Term startState;
        internal Term endState;
        SortedList<Pair<Term, Term>, object> labels;

        /// <summary>
        /// Start state of the transition (readonly)
        /// </summary>
        public Term StartState
        {
            get { return startState; }
            //set { startState = value; }
        }

        /// <summary>
        /// End state of the transition (readonly)
        /// </summary>
        public Term EndState
        {
            get { return endState; }
            //set { endState = value; }
        }

        MultiLabeledTransition(Term startState, Pair<Term, Term> label, Term endState)
        {
            this.startState = startState;
            this.labels = new SortedList<Pair<Term, Term>, object>();
            this.labels.Add(label, null);
            this.endState = endState;
        }

        /// <summary>
        /// Enumerate all the labels in this multilabeled transition as 
        /// multilabeled transition with a single label
        /// </summary>
        public IEnumerable<MultiLabeledTransition> CreateOnePerLabel()
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

        /// <summary>
        /// Create a multilabeled transition with a compound label, if labelOut is non-null, the label is 
        /// considered as a Mealy label with an input part labelIn and an output part labelOut
        /// </summary>
        /// <param name="startState">given start state</param>
        /// <param name="labelIn">input part of the label</param>
        /// <param name="labelOut">output part of the label</param>
        /// <param name="endState">given end state</param>
        static public MultiLabeledTransition Create(Term startState, Term labelIn, Term labelOut, Term endState)
        {
            return new MultiLabeledTransition(startState, new Pair<Term, Term>(labelIn, labelOut), endState);
        }

        /// <summary>
        /// Create a multilabeled transition with the given input label. 
        /// Same as MultiLabeledTransition.Create(startState, labelIn, null, endState)
        /// </summary>
        /// <param name="startState">given start state</param>
        /// <param name="labelIn">given label</param>
        /// <param name="endState">given end state</param>
        static public MultiLabeledTransition Create(Term startState, Term labelIn, Term endState)
        {
            return new MultiLabeledTransition(startState, new Pair<Term, Term>(labelIn, null), endState);
        }

        /// <summary>
        /// Add another compound Mealy label to the multilabeled transition
        /// </summary>
        /// <param name="labelIn">input part of the label</param>
        /// <param name="labelOut">output part of the label</param>
        public void AddMealyLabel(Term labelIn, Term labelOut)
        {
            labels.Add(new Pair<Term, Term>(labelIn, labelOut), null);
        }

        /// <summary>
        /// Add another label to the multilabeled transition
        /// </summary>
        /// <param name="labelIn">the label to add</param>
        public void AddLabel(Term labelIn)
        {
            labels.Add(new Pair<Term, Term>(labelIn, null), null);
        }

        /// <summary>
        /// Create a combined string representation of all the labels with
        /// '\n' separating the different strings of the individual (compound) labels
        /// </summary>
        /// <param name="nameOnly">if true uses only the action names and ignores parameters</param>
        public string CombinedLabel(bool nameOnly)
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
                        nextLabel = ((CompoundTerm)label.First).Symbol.Name.Replace("_Start", "").Replace("_Finish", "");
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
                        sb.Append(ConstructEdgeLabel(label.First));
                    else
                        sb.Append(ConstructMealyLabel(label.First, label.Second));
                    rest = true;
                }
                return sb.ToString();
            }
        }

        static string ConstructEdgeLabel(Term term)
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

        static bool IsStartAction(Term action)
        {
            CompoundTerm a = action as CompoundTerm;
            if (a == null) return false;
            return a.Name.EndsWith("_Start");
        }

        static bool IsFinishAction(Term action)
        {
            CompoundTerm a = action as CompoundTerm;
            if (a == null) return false;
            return a.Name.EndsWith("_Finish");
        }

        static string ConstructMealyLabel(Term term, Term term2)
        {
            return GetActionLabel((CompoundTerm)term, (CompoundTerm)term2);
        }

        /// <summary>
        /// Formats an action for display in the view.
        /// </summary>
        /// <param name="start">Optional start action with input arguments.</param>
        /// <param name="finish">Optional finish action with output arguments and return value.</param>
        /// <returns>A nicely formatted string representing the action(s).</returns>
        static string GetActionLabel(Terms.CompoundTerm start, Terms.CompoundTerm finish)
        {
            if (start != null && finish != null && finish.Arguments.Count > 0)
                return string.Format((IFormatProvider)null,
                    "{0}({1}) / {2}",
                    start.Symbol.Name.Replace("_Start", ""),
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
        /// Gets a comma-separated list of arguments as a string for use in an action label.
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
    }

    /// <summary>
    /// A class that is used as a parameter for generating Graphviz dot representation of an FSM.
    /// </summary>
    public class GraphParams
    {
        // There are some variables which are never assigned to. They are here because this class 
        // can later be used for separating the visualization code from the NModel part.
        internal String name = "";
        internal GraphDirection direction = GraphDirection.TopToBottom;
        internal FSM fa;
        //this should not be referenced from here.
        internal CustomLabelProvider customStateLabelProvider;
        //this should not be referenced from here.
        internal StateProvider faStateProvider; // this.finiteAutomatonContext.stateProvider
        internal Term node;
        internal bool livenessCheckIsOn = false;
        internal bool safetyCheckIsOn = false;
        internal Set<Term> unsafeNodes=Set<Term>.EmptySet;
        internal Set<Term> deadNodes = Set<Term>.EmptySet;
        //<NModel.Visualization.GraphView.MultiLabeledTransition>
        internal IEnumerable<MultiLabeledTransition> transitionValues;
        private Dictionary<MultiLabeledTransition, object> multilabeledTransitions = new Dictionary<MultiLabeledTransition, object>();
        internal StateShape stateShape = StateShape.Ellipse;
        internal string deadStateColor = "black";
        internal string unsafeStateColor = "black";
        internal string initialStateColor = "grey";
        internal TransitionLabel transitionLabels = TransitionLabel.ActionSymbol;

        internal GraphParams() {
            customStateLabelProvider = null;
            faStateProvider = null;
            node = null;
        }

        internal Term Node {get {return node;}}
        
        /// <summary>
        /// The constructor that initializes the parameters using a name and a FSM.
        /// </summary>
        /// <param name="name">Name of the FSM.</param>
        /// <param name="fa"></param>
        public GraphParams(string name, FSM fa )
        {
            this.name=name;
            this.fa=fa;
            ParseTransitions();
        
        }

        internal void ParseTransitions()
        {
            foreach (Transition t in this.fa.Transitions)
            {
                multilabeledTransitions.Add(MultiLabeledTransition.Create(t.First,t.Second,t.Third), default(object));
            
            }
            //(IEnumerable<NModel.Visualization.GraphView.MultiLabeledTransition>)
            this.transitionValues =  (multilabeledTransitions.Keys);
        }
    }

    internal sealed class FSM2DotCmdLineParams
    {
        [Argument(ArgumentType.AtMostOnce, ShortName = "n", HelpText = "The name of the FSM")]
        public string fsmName="";

        [Argument(ArgumentType.Required, ShortName = "fsm", HelpText = "The name of the file containing the FSM")]
        public string fsmFileName="";

        [Argument(ArgumentType.AtMostOnce, ShortName = "dot", HelpText = "The name of the file where Dot goes")]
        public string dotFileName;
    
    }


    /// <summary>
    /// Class for generating Graphviz dot fromat from finite state machines.
    /// </summary>
    public static class DotWriter
    {

        /// <summary>
        /// A method that is used by the command line interface.
        /// </summary>
        /// <param name="args"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public static void RunWithCommandLineArguments(string[] args)
        {
            FSM2DotCmdLineParams cmd = new FSM2DotCmdLineParams();
            try
            {
                if (!Parser.ParseArgumentsWithUsage(args, cmd))
                    return;
                StreamReader fsm = new StreamReader(cmd.fsmFileName);
                StringBuilder dot = FSM2Dot(cmd.fsmName, fsm.ReadToEnd());
                fsm.Close();
                if (cmd.dotFileName == null)
                    cmd.dotFileName = cmd.fsmFileName + ".dot";
                StreamWriter dotStream = new StreamWriter(cmd.dotFileName);
                dotStream.Write(dot.ToString());
                dotStream.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Invocation of 'FSM2Dot' failed: " + e.ToString());
            }

        }

        internal static StringBuilder FSM2Dot(string name, string fsm)
        {
            FSM fa = FSM.FromTerm(CompoundTerm.Parse(fsm));
            GraphParams gp = new GraphParams(name, fa);
            return DotWriter.ToDot(gp);
        }

        
        /// <summary>
        /// Produce dot output of the graph
        /// </summary>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        public static StringBuilder ToDot(GraphParams gp)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("digraph ");
            //            AppendLabel(sb, finiteAutomatonContext.name);
            AppendLabel(sb, gp.name);
            sb.Append(" {\n\n  rankdir=");
            //            sb.Append(this.direction == GraphDirection.TopToBottom ? "TB" : "LR");
            sb.Append(gp.direction == GraphDirection.TopToBottom ? "TB" : "LR");

            sb.Append(";\n\n  //Initial state\n  node ");
            AppendInitialStateAttributes(gp,sb);
            sb.Append("\n  ");
            //            AppendLabel(sb, finiteAutomatonContext.fa.InitialState);
            AppendLabel(sb, gp.fa.InitialState);
            //            if (this.customStateLabelProvider != null && this.finiteAutomatonContext.stateProvider != null)
            if (gp.customStateLabelProvider != null && gp.faStateProvider != null)
            {
                sb.Append(" [label = ");
                AppendLabel(sb, gp.customStateLabelProvider(gp.faStateProvider(gp.fa.InitialState)));
                sb.Append("]");
            }
            sb.Append("\n\n");

            sb.Append("  //Accepting states\n  node ");
            AppendAcceptingStateAttributes(gp,sb);
            //When we load an FSM from file, then how do we know this?
            foreach (Term node in gp.fa.States)
            {
                if (gp.fa.AcceptingStates.Contains(node) &&
                    !node.Equals(gp.fa.InitialState) &&
                    !(gp.safetyCheckIsOn && gp.unsafeNodes.Contains(node)))
                {
                    AddNodeLabel(gp, sb, node);
                }
            }
            sb.Append("\n\n");

            if (gp.livenessCheckIsOn)
            {
                sb.Append("  //Dead states\n  node ");
                AppendDeadStateAttributes(gp,sb);
                foreach (Term node in gp.fa.States)
                {
                    if (gp.deadNodes.Contains(node) && !node.Equals(gp.fa.InitialState))
                    {
                        AddNodeLabel(gp, sb, node);
                    }
                }
            }
            sb.Append("\n\n");

            if (gp.safetyCheckIsOn)
            {
                sb.Append("  //Unsafe states\n  node ");
                AppendUnsafeStateAttributes(gp,sb);
                foreach (Term node in gp.fa.States)
                {
                    if (gp.unsafeNodes.Contains(node)
                        && !node.Equals(gp.fa.InitialState))
                    {
                        AddNodeLabel(gp,sb, node, gp.fa.AcceptingStates.Contains(node));
                    }
                }
            }
            sb.Append("\n\n");

            sb.Append("  //Safe live nonaccepting states\n  node ");
            AppendNonAcceptingStateAttributes(gp,sb);
            foreach (Term node in gp.fa.States)
            {
                if (!gp.fa.AcceptingStates.Contains(node) &&
                    !node.Equals(gp.fa.InitialState) &&
                    !(gp.livenessCheckIsOn && gp.deadNodes.Contains(node)) &&
                    !(gp.safetyCheckIsOn && gp.unsafeNodes.Contains(node)))
                {
                    AddNodeLabel(gp, sb, node);
                }
            }
            sb.Append("\n\n");

            sb.Append("  //Transitions");
            foreach (MultiLabeledTransition t in gp.transitionValues)
            {
                sb.Append("\n  ");
                AppendLabel(sb, t.startState);
                sb.Append(" -> ");
                AppendLabel(sb, t.endState);
                if (gp.transitionLabels != TransitionLabel.None)
                {
                    sb.Append(" [ label = ");
                    AppendLabel(sb, t.CombinedLabel(gp.transitionLabels == TransitionLabel.ActionSymbol));
                    sb.Append(" ];");
                }
            }
            sb.Append("\n}\n");
            return sb;
        }

        private static void AddNodeLabel(GraphParams gp, StringBuilder sb, Term node)
        {
            sb.Append("\n  ");
            AppendLabel(sb, node);
            if (gp.customStateLabelProvider != null && gp.faStateProvider != null)
            {
                sb.Append(" [label = ");
                AppendLabel(sb, gp.customStateLabelProvider(gp.faStateProvider(node)));
                sb.Append("]");
            }
        }

        private static void AddNodeLabel(GraphParams gp, StringBuilder sb, Term node, bool accepting)
        {
            sb.Append("\n  ");
            AppendLabel(sb, node);
            if (gp.customStateLabelProvider != null && gp.faStateProvider != null)
            {
                sb.Append(" [label = ");
                AppendLabel(sb, gp.customStateLabelProvider(gp.faStateProvider(node)));
                if (accepting)
                    sb.Append(", peripheries = 2");
                sb.Append("]");
            }
            else if (accepting)
                sb.Append(" [peripheries = 2]");

        }

        private static void AppendAcceptingStateAttributes(GraphParams gp, StringBuilder sb)
        {
            sb.Append("[style = filled, shape = ");
            sb.Append(ToDotShape(gp.stateShape));
            sb.Append(", peripheries = 2, fillcolor = white]");
        }

        private static void AppendDeadStateAttributes(GraphParams gp, StringBuilder sb)
        {
            sb.Append("[style = filled, shape = ");
            sb.Append(ToDotShape(gp.stateShape));
            sb.Append(", peripheries = 1, fillcolor = ");
            sb.Append(gp.deadStateColor);
            sb.Append("]");
        }

        private static void AppendUnsafeStateAttributes(GraphParams gp, StringBuilder sb)
        {
            sb.Append("[style = filled, shape = ");
            sb.Append(ToDotShape(gp.stateShape));
            sb.Append(", peripheries = 1, fillcolor = ");
            sb.Append(gp.unsafeStateColor);
            sb.Append("]");
        }

        private static void AppendNonAcceptingStateAttributes(GraphParams gp, StringBuilder sb)
        {
            sb.Append("[style = filled, shape = ");
            sb.Append(ToDotShape(gp.stateShape));
            sb.Append(", peripheries = 1, fillcolor = white]");
        }

        private static void AppendInitialStateAttributes(GraphParams gp, StringBuilder sb)
        {
            sb.Append("[style = filled, shape = ");
            sb.Append(ToDotShape(gp.stateShape));
            sb.Append(", peripheries = ");
            if (gp.fa.AcceptingStates.Contains(gp.fa.InitialState))
                sb.Append("2");
            else
                sb.Append("1");
            sb.Append(", fillcolor = ");
            sb.Append(gp.initialStateColor);
            sb.Append("]");
        }

        static void AppendLabel(StringBuilder sb, IComparable l)
        {
            sb.Append("\"");
            sb.Append(l.ToString().Replace("\"", "\\\"").Replace("\n", "\\n"));
            sb.Append("\"");
        }

        /// <summary>
        /// Map a state shape to corresponding dot node shape
        /// </summary>
        public static string ToDotShape(StateShape shape)
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

    }
}

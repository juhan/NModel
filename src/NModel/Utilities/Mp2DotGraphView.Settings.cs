using System.Text;
using System.Collections.Generic;

using NModel.Algorithms;
using NModel;
using NModel.Terms;
using NModel.Execution;

namespace NModel.Utilities.Graph
{
    // This file is based in GraphView.Settings.cs,
    // but with set methods in properties simplified.


    /// <summary>
    /// Determines what is shown as a node label.
    /// </summary>
    public enum NodeLabel
    {
        /// <summary>
        /// Label is omitted.
        /// </summary>
        None,
        /// <summary>
        /// Abstract Id is shown.
        /// </summary>
        Id,
        /// <summary>
        /// Control mode is shown.
        /// </summary>
        ControlMode,
        /// <summary>
        /// All state variable values are shown.
        /// </summary>
        StateVariables
    }

    //settings that can be configured by the user
    partial class GraphView
    {

        #region TransitionLabels
        /// <summary>
        /// Determines what is shown as a transition label.
        /// </summary>
        internal TransitionLabel transitionLabels = TransitionLabel.Action;
        /// <summary>
        /// Determines what is shown as a transition label.
        /// </summary>
        /// DELETE following attributes used in GraphView.Settings.cs, intended to support GUI
        //[RuntimeBrowsable]
        //[Category("Appearance (states)")]
        //[DefaultValue(typeof(System.Drawing.Color), "LightGray")]
        //[Description("Background color of the initial state. Default is LightGray.")]
        public TransitionLabel TransitionLabels
        {
            get
            {
                return transitionLabels;
            }
            set
            {
                transitionLabels = value;
            }
        }
        #endregion

        #region NodeLabelsVisible
        /// <summary>
        /// Visibility of node labels. Default is true.
        /// </summary>
        internal bool nodeLabelsVisible = true;
        /// <summary>
        /// Visibility of node labels. Default is true.
        /// </summary>
        public bool NodeLabelsVisible
        {
            get
            {
                return nodeLabelsVisible;
            }
            set
            {
                    nodeLabelsVisible = value;
            }
        }
        #endregion

        // struct Color is now defined in Mp2DotGraphView.cs
        #region InitialStateColor
        /// <summary>
        /// Background color of the initial state. Default is LightGray.
        /// </summary>
        internal Color initialStateColor = new Color("LightGray");
        /// <summary>
        /// Background color of the initial state. Default is LightGray.
        /// </summary>
        public Color InitialStateColor
        {
            get
            {
                return initialStateColor;
            }
            set
            {
                    initialStateColor = value;
            }
        }
        #endregion

        // mp2dot is not interactive but hoverColor is an mpv option so we must include it here
        #region HoverColor
        /// <summary>
        /// Line and action label color to use when edges or nodes are hovered over. Default is Lime.
        /// </summary>
        internal Color hoverColor = new Color("Lime");
        /// <summary>
        /// Line and action label color to use when edges or nodes are hovered over. Default is Lime.
        /// </summary>
        public Color HoverColor
        {
            get
            {
                return hoverColor;
            }
            set
            {
                    hoverColor = value;
            }
        }
        #endregion

        // mp2dot is not interactive but selectionColor is an mpv option so we must include it here
        #region SelectionColor
        /// <summary>
        /// Background color to use when a node is selected. Default is Blue.
        /// </summary>
        internal Color selectionColor = new Color("Blue");
        /// <summary>
        /// Background color to use when a node is selected. Default is Blue.
        /// </summary>
        public Color SelectionColor
        {
            get
            {
                return selectionColor;
            }
            set
            {
                    selectionColor = value;
            }
        }
        #endregion

        #region LivenessCheckIsOn
        /// <summary>
        /// Mark states from which no accepting state is reachable in the current view. Default is false.
        /// </summary>
        internal bool livenessCheckIsOn = false;
        /// <summary>
        /// Mark states from which no accepting state is reachable in the current view. Default is false.
        /// </summary>
        public bool LivenessCheckIsOn
        {
            get
            {
                return this.livenessCheckIsOn;
            }
            set
            {
                    this.livenessCheckIsOn = value;
            }
        }
        #endregion

        #region DeadStateColor
        /// <summary>
        /// The background color of dead states.
        /// Dead states are states from which no accepting state is reachable.
        /// Default is Yellow.
        /// </summary>
        Color deadStateColor = new Color("Yellow");
        /// <summary>
        /// The background color of dead states.
        /// Dead states are states from which no accepting state is reachable.
        /// Default is Yellow.
        /// </summary>
        public Color DeadStateColor
        {
            get
            {
                return deadStateColor;
            }
            set
            {
                    deadStateColor = value;
            }
        }
        #endregion

        #region SafetyCheckIsOn
        /// <summary>
        /// Whether to mark states that violate a state invariant. Default is false.
        /// </summary>
        internal bool safetyCheckIsOn = false;
        /// <summary>
        /// Whether to mark states that violate a state invariant. Default is false.
        /// </summary>
        public bool SafetyCheckIsOn
        {
            get
            {
                return this.safetyCheckIsOn;
            }
            set
            {
                    this.safetyCheckIsOn = value;
            }
        }
        #endregion

        #region UnsafeStateColor
        /// <summary>
        /// Background color of states that violate a safety condition (state invariant). Default is Red.
        /// </summary>
        internal Color unsafeStateColor = new Color("Red");
        /// <summary>
        /// Background color of states that violate a safety condition (state invariant). Default is Red.
        /// </summary>
        public Color UnsafeStateColor
        {
            get
            {
                return unsafeStateColor;
            }
            set
            {
                    unsafeStateColor = value;
            }
        }
        #endregion

        #region MaxTransitions
        /// <summary>
        /// Maximum number of transitions to draw in the graph. Default is 100.
        /// </summary>
        internal int maxTransitions = 100;
        /// <summary>
        /// Maximum number of transitions to draw in the graph. Default is 100.
        /// </summary>
        public int MaxTransitions
        {
            get
            {
                return maxTransitions;
            }
            set
            {
                    maxTransitions = value;
            }
        }
        #endregion

        #region LoopsVisible
        /// <summary>
        /// Visibility of transitions whose start and end states are the same. Default is true.
        /// </summary>
        internal bool loopsVisible = true;
        /// <summary>
        /// Visibility of transitions whose start and end states are the same. Default is true.
        /// </summary>
        public bool LoopsVisible
        {
            get
            {
                return loopsVisible;
            }
            set
            {
                    loopsVisible = value;
            }
        }
        #endregion

        #region DeadstatesVisible
        /// <summary>
        /// Visibility of of dead states. Default is true.
        /// </summary>
        internal bool deadstatesVisible = true;
        /// <summary>
        /// Visibility of of dead states. Default is true.
        /// </summary>
        public bool DeadstatesVisible
        {
          get
          {
              return deadstatesVisible;
          }
          set
          {
                deadstatesVisible = value;
          }
        }
        #endregion

        #region SimpleGraph
        /// <summary>
        /// Multiple transitions between same start and end states are shown as one transition with a merged label. Default is true.
        /// </summary>
        internal bool mergeLabels = true;
        /// <summary>
        /// Multiple transitions between same start and end states are shown as one transition with a merged label. Default is true.
        /// </summary>
        public bool MergeLabels
        {
            get
            {
                return mergeLabels;
            }
            set
            {
                    mergeLabels = value;
            }
        }
        #endregion

        #region AcceptingStatesMarked
        /// <summary>
        /// Mark accepting states with a bold outline. Default is true.
        /// </summary>
        internal bool acceptingStatesMarked = true;
        /// <summary>
        /// Mark accepting states with a bold outline. Default is true.
        /// </summary>
        public bool AcceptingStatesMarked
        {
            get
            {
                return this.acceptingStatesMarked;
            }
            set
            {
                    this.acceptingStatesMarked = value;
            }
        }
        #endregion

        #region StateShape
        /// <summary>
        /// State shape. Default is Ellipse.
        /// </summary>
        internal StateShape stateShape = StateShape.Ellipse;
        /// <summary>
        /// State shape. Default is Ellipse.
        /// </summary>
        public StateShape StateShape
        {
            get
            {
                return stateShape;
            }
            set
            {
                    stateShape = value;
            }
        }
        #endregion

        #region Direction
        /// <summary>
        /// Direction of graph layout. Default is TopToBottom.
        /// </summary>
        internal GraphDirection direction = GraphDirection.TopToBottom;
        /// <summary>
        /// Direction of graph layout. Default is TopToBottom.
        /// </summary>
        public GraphDirection Direction
        {
            get
            {
                return direction;
            }
            set
            {
                    direction = value;
            }
        }
        #endregion

        #region CombineActions
        /// <summary>
        /// Whether to view start actions and finish actions as single transitions. Default is false.
        /// </summary>
        internal bool combineActions = false;
        /// <summary>
        /// Whether to view start actions and finish actions as single transitions. Default is false.
        /// </summary>
        /// <remarks>
        /// The default value is <c>false</c>, meaning that adjacent start/finish actions are not collapsed into a single edge.
        /// </remarks>
        public bool CombineActions
        {
            get
            {
                return combineActions;
            }
            set
            {
                    combineActions = value;
            }
        }
        #endregion

        #region exploration statistics (readonly info)
        /// <summary>
        /// Shows the number of explored transitions
        /// </summary>
        public int Transitions
        {
            get
            {
                if (this.finiteAutomatonContext != null)
                    return this.finiteAutomatonContext.fa.Transitions.Count;
                else
                    return 0;
            }
        }

        /// <summary>
        /// Shows the number of explored states
        /// </summary>
        public int States
        {
            get
            {
                if (this.finiteAutomatonContext != null)
                    return this.finiteAutomatonContext.fa.States.Count;
                else
                    return 0;
            }
        }

        /// <summary>
        /// Shows the number of accepting states
        /// </summary>
        public int AcceptingStates
        {
            get
            {
                if (this.finiteAutomatonContext != null)
                    return this.finiteAutomatonContext.fa.AcceptingStates.Count;
                else
                    return 0;
            }
        }

        /// <summary>
        /// Shows the number of dead states
        /// </summary>
        public int DeadStates
        {
            get
            {
                if (this.finiteAutomatonContext != null && 
                    this.finiteAutomatonContext.deadNodes != null)
                    return this.finiteAutomatonContext.deadNodes.Count;
                else
                    return 0;
            }
        }

        /// <summary>
        /// Shows the number of unsafe states
        /// </summary>
        public int UnsafeStates
        {
            get
            {
                if (this.finiteAutomatonContext != null)
                    return this.finiteAutomatonContext.unsafeNodes.Count;
                else
                    return 0;
            }
        }

        #endregion

        #region ExcludeIsomorphicStates
        /// <summary>
        /// Whether to discard isomorphic states when traversing the state space. Default is false.
        /// </summary>
        internal bool excludeIsomorphicStates = false;

        /// <summary>
        /// Whether to discard isomorphic states when traversing the state space. Default is false.
        /// </summary>
        /// <remarks>
        /// The default value is <c>false</c>, meaning that no isomorphism checks are performed.
        /// </remarks>
        public bool ExcludeIsomorphicStates
        {
            get
            {
                return excludeIsomorphicStates;
            }
            set
            {
                    excludeIsomorphicStates = value;
            }
        }

        internal bool collapseExcludedIsomorphicStates = false;

        /// <summary>
        /// Whether to group isomorphic states together. Default is false.
        /// </summary>
        /// <remarks>
        /// The default value is <c>false</c>, meaning that no isomorphic states are not grouped. Has meaning only when <c>ExcludeIsomorphicStates</c> is <c>true</c>;
        /// </remarks>
        public bool CollapseExcludedIsomorphicStates
        {
            get
            {
                return collapseExcludedIsomorphicStates;
            }
            set
            {
                        collapseExcludedIsomorphicStates = value;
            }
        }
        #endregion

        // InitialTransitions copied from ModelProgramGraphView.cs not GraphView.Settings.cs
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

        private void RestoreDefaults()
        {
            transitionLabels = TransitionLabel.Action;
            nodeLabelsVisible = true;
            initialStateColor = new Color("LightGray");
            hoverColor = new Color("Lime");
            selectionColor = new Color("Blue");
            livenessCheckIsOn = false;
            deadStateColor = new Color("Yellow");
            safetyCheckIsOn = false;
            unsafeStateColor = new Color("Red");
            maxTransitions = 100;
            loopsVisible = true;
            mergeLabels = true;
            acceptingStatesMarked = true;
            stateShape = StateShape.Ellipse;
            direction = GraphDirection.TopToBottom;
            combineActions = false;
            excludeIsomorphicStates = false;
            collapseExcludedIsomorphicStates = false;
        }
    }
}

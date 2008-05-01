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
using NModel.Execution;

namespace NModel.Visualization
{
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
        [RuntimeBrowsable]
        [Category("Appearance (transitions)")]
        [DefaultValue(TransitionLabel.Action)]
        [Description("Determines what is shown as a transition label. Default is Action.")]
        public TransitionLabel TransitionLabels
        {
            get
            {
                return transitionLabels;
            }
            set
            {
                if (transitionLabels != value)
                {
                    transitionLabels = value;
                    actionLabelsButton.CheckState = (value == TransitionLabel.Action ? CheckState.Checked : (value == TransitionLabel.ActionSymbol ? CheckState.Indeterminate : CheckState.Unchecked));
                    if (this.finiteAutomatonContext != null)
                    {
                        graphChanged = true;
                        PerformLayout();
                    }
                }
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
        [RuntimeBrowsable]
        [Category("Appearance (states)")]
        [DefaultValue(true)]
        [Description("Visibility of node labels. Default is true.")]
        public bool NodeLabelsVisible
        {
            get
            {
                return nodeLabelsVisible;
            }
            set
            {
                if (nodeLabelsVisible != value)
                {
                    nodeLabelsVisible = value;
                    if (this.finiteAutomatonContext != null)
                    {
                        graphChanged = true;
                        PerformLayout();
                    }
                }
            }
        }
        #endregion

        #region InitialStateColor
        /// <summary>
        /// Background color of the initial state. Default is LightGray.
        /// </summary>
        internal Color initialStateColor = Color.LightGray;
        /// <summary>
        /// Background color of the initial state. Default is LightGray.
        /// </summary>
        [RuntimeBrowsable]
        [Category("Appearance (states)")]
        [DefaultValue(typeof(System.Drawing.Color), "LightGray")]
        [Description("Background color of the initial state. Default is LightGray.")]
        public System.Drawing.Color InitialStateColor
        {
            get
            {
                return initialStateColor;
            }
            set
            {
                if (initialStateColor != value)
                {
                    initialStateColor = value;
                    if (this.finiteAutomatonContext != null)
                    {
                        graphChanged = true;
                        PerformLayout();
                    }
                }
            }
        }
        #endregion

        #region HoverColor
        /// <summary>
        /// Line and action label color to use when edges or nodes are hovered over. Default is Lime.
        /// </summary>
        internal Color hoverColor = Color.Lime;
        /// <summary>
        /// Line and action label color to use when edges or nodes are hovered over. Default is Lime.
        /// </summary>
        [RuntimeBrowsable]
        [Category("Appearance (graph)")]
        [DefaultValue(typeof(System.Drawing.Color), "Lime")]
        [Description("Line and action label color to use when edges or nodes are hovered over. Default is Lime.")]
        public System.Drawing.Color HoverColor
        {
            get
            {
                return hoverColor;
            }
            set
            {
                if (hoverColor != value)
                {
                    hoverColor = value;
                }
            }
        }
        #endregion

        #region SelectionColor
        /// <summary>
        /// Background color to use when a node is selected. Default is Blue.
        /// </summary>
        internal Color selectionColor = System.Drawing.Color.Blue;
        /// <summary>
        /// Background color to use when a node is selected. Default is Blue.
        /// </summary>
        [RuntimeBrowsable]
        [Category("Appearance (graph)")]
        [DefaultValue(typeof(System.Drawing.Color), "Blue")]
        [Description("Background color to use when a node is selected. Default is Blue.")]
        public System.Drawing.Color SelectionColor
        {
            get
            {
                return selectionColor;
            }
            set
            {
                if (selectionColor != value)
                {
                    selectionColor = value;
                }
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
        [RuntimeBrowsable]
        [Category("Analysis")]
        [DefaultValue(false)]
        [Description("Mark states from which no accepting state is reachable in the current view. Default is false.")]
        public bool LivenessCheckIsOn
        {
            get
            {
                return this.livenessCheckIsOn;
            }
            set
            {
                if (this.livenessCheckIsOn != value)
                {
                    this.livenessCheckIsOn = value;
                    if (this.finiteAutomatonContext != null)
                    {
                        this.graphChanged = true;
                        PerformLayout();
                    }
                }
            }
        }
        #endregion

        #region DeadStateColor
        /// <summary>
        /// The background color of dead states.
        /// Dead states are states from which no accepting state is reachable.
        /// Default is Yellow.
        /// </summary>
        Color deadStateColor = Color.Yellow;
        /// <summary>
        /// The background color of dead states.
        /// Dead states are states from which no accepting state is reachable.
        /// Default is Yellow.
        /// </summary>
        [RuntimeBrowsable]
        [Category("Analysis")]
        [DefaultValue(typeof(System.Drawing.Color), "Yellow")]
        [Description("The background color of dead states. Dead states are states from which no accepting state is reachable. Default is Yellow.")]
        public System.Drawing.Color DeadStateColor
        {
            get
            {
                return deadStateColor;
            }
            set
            {
                if (deadStateColor != value)
                {
                    deadStateColor = value;
                    if (this.finiteAutomatonContext != null)
                    {
                        graphChanged = true;
                        PerformLayout();
                    }
                }
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
        [RuntimeBrowsable]
        [Category("Analysis")]
        [DefaultValue(false)]
        [Description("Mark states that violate a safety condition (state invariant). Default is false.")]
        public bool SafetyCheckIsOn
        {
            get
            {
                return this.safetyCheckIsOn;
            }
            set
            {
                if (this.safetyCheckIsOn != value)
                {
                    this.safetyCheckIsOn = value;
                    if (this.finiteAutomatonContext != null)
                    {
                        this.graphChanged = true;
                        PerformLayout();
                    }
                }
            }
        }
        #endregion

        #region UnsafeStateColor
        /// <summary>
        /// Background color of states that violate a safety condition (state invariant). Default is Red.
        /// </summary>
        internal Color unsafeStateColor = Color.Red;
        /// <summary>
        /// Background color of states that violate a safety condition (state invariant). Default is Red.
        /// </summary>
        [RuntimeBrowsable]
        [Category("Analysis")]
        [DefaultValue(typeof(System.Drawing.Color), "Red")]
        [Description("Background color of states that violate a safety condition (state invariant). Default is Red.")]
        public System.Drawing.Color UnsafeStateColor
        {
            get
            {
                return unsafeStateColor;
            }
            set
            {
                if (unsafeStateColor != value)
                {
                    unsafeStateColor = value;
                    if (this.finiteAutomatonContext != null)
                    {
                        graphChanged = true;
                        PerformLayout();
                    }
                }
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
        [RuntimeBrowsable]
        [Category("Exploration limits")]
        [DefaultValue(100)]
        [Description("Maximum number of transitions to draw in the graph. Default is 100.")]
        public int MaxTransitions
        {
            get
            {
                return maxTransitions;
            }
            set
            {
                if (maxTransitions != value)
                {
                    maxTransitions = value;
                    if (this.finiteAutomatonContext != null)
                    {
                        RegenerateFiniteAutomaton();
                        //graphChanged = true;
                        //PerformLayout();
                    }
                }
            }
        }

        /// <summary>
        /// Override in a derived class to regenerate the finite automaton
        /// </summary>
        internal virtual void RegenerateFiniteAutomaton()
        {
            graphChanged = true;
            PerformLayout();
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
        [RuntimeBrowsable]
        [Category("Appearance (transitions)")]
        [DefaultValue(true)]
        [Description("Visibility of transitions whose start and end states are the same. Default is true.")]
        public bool LoopsVisible
        {
            get
            {
                return loopsVisible;
            }
            set
            {
                if (loopsVisible != value)
                {
                    loopsVisible = value;
                    loopsButton.Checked = value;
                    if (this.finiteAutomatonContext != null)
                    {
                        graphChanged = true;
                        PerformLayout();
                    }
                }
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
        [RuntimeBrowsable]
        [Category("Analysis")]
        [DefaultValue(true)]
        [Description("Visibility of of dead states. This setting has no effect when liveness check is off. Default is true.")]
        public bool DeadstatesVisible
        {
          get
          {
              return deadstatesVisible;
          }
          set
          {
              if (deadstatesVisible != value)
            {
                deadstatesVisible = value;
              if (this.finiteAutomatonContext != null)
              {
                graphChanged = true;
                PerformLayout();
              }
            }
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
        [RuntimeBrowsable]
        [Category("Appearance (transitions)")]
        [DefaultValue(true)]
        [Description("Multiple transitions between same start and end states are shown as one transition with a merged label. Default is true.")]
        public bool MergeLabels
        {
            get
            {
                return mergeLabels;
            }
            set
            {
                if (mergeLabels != value)
                {
                    mergeLabels = value;
                    transitionsButton.Checked = value;
                    if (this.finiteAutomatonContext != null)
                    {
                        graphChanged = true;
                        PerformLayout();
                    }
                }
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
        [RuntimeBrowsable]
        [Category("Appearance (states)")]
        [DefaultValue(true)]
        [Description("Mark accepting states with a bold outline. Default is true.")]
        public bool AcceptingStatesMarked
        {
            get
            {
                return this.acceptingStatesMarked;
            }
            set
            {
                if (this.acceptingStatesMarked != value)
                {
                    this.acceptingStatesMarked = value;
                    if (this.finiteAutomatonContext != null)
                    {
                        this.graphChanged = true;
                        PerformLayout();
                    }
                }
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
        [RuntimeBrowsable]
        [Category("Appearance (states)")]
        [DefaultValue(StateShape.Ellipse)]
        [Description("State shape. Default is Ellipse.")]
        public StateShape StateShape
        {
            get
            {
                return stateShape;
            }
            set
            {
                if (stateShape != value)
                {
                    stateShape = value;
                    if (this.finiteAutomatonContext != null)
                    {
                        graphChanged = true;
                        PerformLayout();
                    }
                }
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
        [RuntimeBrowsable]
        [Category("Appearance (graph)")]
        [DefaultValue(GraphDirection.TopToBottom)]
        [Description("Direction of graph layout. Default is TopToBottom.")]
        public GraphDirection Direction
        {
            get
            {
                return direction;
            }
            set
            {
                if (direction != value)
                {
                    direction = value;

                    topToBottomToolStripMenuItem.Checked = value == GraphDirection.TopToBottom;
                    leftToRightToolStripMenuItem.Checked = value == GraphDirection.LeftToRight;
                    rightToLeftToolStripMenuItem.Checked = value == GraphDirection.RightToLeft;
                    bottomToTopToolStripMenuItem.Checked = value == GraphDirection.BottomToTop;

                    layoutButton.Image =
                        value == GraphDirection.TopToBottom ? topToBottomToolStripMenuItem.Image :
                        value == GraphDirection.LeftToRight ? leftToRightToolStripMenuItem.Image :
                        value == GraphDirection.RightToLeft ? rightToLeftToolStripMenuItem.Image :
                        value == GraphDirection.BottomToTop ? bottomToTopToolStripMenuItem.Image : null;
                    if (this.finiteAutomatonContext != null)
                    {
                        graphChanged = true;
                        PerformLayout();
                    }
                }
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
        [RuntimeBrowsable]
        [Category("Appearance (transitions)")]
        [DefaultValue(false)]
        [Description("Whether to view start actions and finish actions as single transitions.\nThe default value is false, meaning that adjacent start/finish actions are not collapsed into a single edge.")]
        public bool CombineActions
        {
            get
            {
                return combineActions;
            }
            set
            {
                if (combineActions != value)
                {
                    combineActions = value;
                    combineActionsButton.Checked = value;
                    if (this.finiteAutomatonContext != null)
                    {
                        graphChanged = true;
                        PerformLayout();
                    }
                }
            }
        }
        #endregion

        #region exploration statistics (readonly info)

        /// <summary>
        /// Shows the number of explored transitions
        /// </summary>
        [RuntimeBrowsable]
        [Category("Exploration statistics")]
        [Description("Shows the number of explored transitions")]
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
        [RuntimeBrowsable]
        [Category("Exploration statistics")]
        [Description("Shows the number of explored states")]
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
        [RuntimeBrowsable]
        [Category("Exploration statistics")]
        [Description("Shows the number of accepting states")]
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
        [RuntimeBrowsable]
        [Category("Exploration statistics")]
        [Description("Shows the number of dead states")]
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
        [RuntimeBrowsable]
        [Category("Exploration statistics")]
        [Description("Shows the number of unsafe states")]
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
        /// This method needs to be overriden by the instantiation of the ModelProgramGraphView class.
        /// It is provided to make ExcludeIsomorphicStates() and RegenerateFiniteAutomaton()  neater.
        /// </summary>
        virtual protected void ResetModelProgram() {  }

        /// <summary>
        /// Whether to discard isomorphic states when traversing the state space. Default is false.
        /// </summary>
        /// <remarks>
        /// The default value is <c>false</c>, meaning that no isomorphism checks are performed.
        /// </remarks>
        // This should actually restart the search, thus should it be RuntimeBrowsable?
        [RuntimeBrowsable]
        [Category("Analysis")]
        [DefaultValue(false)]
        [Description("Whether to discard isomorphic states when traversing the state space.\nThe default value is false, meaning that no isomorphism checks are performed and all states are explored until the max transition count is exhausted.")]
        public bool ExcludeIsomorphicStates
        {
            get
            {
                return excludeIsomorphicStates;
            }
            set
            {
                if (excludeIsomorphicStates != value)
                {
                    excludeIsomorphicStates = value;
                    if (this.finiteAutomatonContext != null)
                    {
                        this.ResetModelProgram();

                    }
                }
            }
        }

        internal bool collapseExcludedIsomorphicStates = false;

        /// <summary>
        /// Whether to group isomorphic states together. Default is false.
        /// </summary>
        /// <remarks>
        /// The default value is <c>false</c>, meaning that no isomorphic states are not grouped. Has meaning only when <c>ExcludeIsomorphicStates</c> is <c>true</c>;
        /// </remarks>
        [RuntimeBrowsable]
        [Category("Analysis")]
        [DefaultValue(false)]
        [Description("Whether to group isomorphic states together. Depends on ExcludeIsomorphicStates.\nThe default value is false, meaning that all concrete states will be displayed but exploration is continued only from one of the isomorphic states.")]
        public bool CollapseExcludedIsomorphicStates
        {
            get
            {
                return collapseExcludedIsomorphicStates;
            }
            set
            {
                if (excludeIsomorphicStates){
                    if (collapseExcludedIsomorphicStates != value)
                    {
                        collapseExcludedIsomorphicStates = value;
                        if (this.finiteAutomatonContext != null)
                        {
                             //if (this.finiteAutomatonContext != null)
                             //{
                             //   graphChanged = true;
                             //   PerformLayout();
                             //}
                           this.ResetModelProgram();

                        }
                    }
                } else
                {
                    if (collapseExcludedIsomorphicStates != value)
                    {
                        collapseExcludedIsomorphicStates = value;
                    }
                    //throw new Exception("Isomorphic states can be excluded only when ExcludeIsomorphicStates is activated.");
                }
            }
        }


        #endregion

        #region StateViewVisible

        /// <summary>
        /// Whether the State View is visible. Default is false.
        /// </summary>
        [Category("Misc")]
        [DefaultValue(false)]
        [Description("Whether the State View is visible")]
        public bool StateViewVisible
        {
            get
            {
                return stateValuesButton.Checked;
            }
            set
            {
                stateValuesButton.Checked = value;
                UpdateVisibility();
            }
        }

        #endregion


        private void RestoreDefaults()
        {
            transitionLabels = TransitionLabel.Action;
            nodeLabelsVisible = true;
            initialStateColor = Color.LightGray;
            hoverColor = Color.Lime;
            selectionColor = System.Drawing.Color.Blue;
            livenessCheckIsOn = false;
            deadStateColor = Color.Yellow;
            safetyCheckIsOn = false;
            unsafeStateColor = Color.Red;
            maxTransitions = 100;
            loopsVisible = true;
            mergeLabels = true;
            acceptingStatesMarked = true;
            stateShape = StateShape.Ellipse;
            direction = GraphDirection.TopToBottom;
            combineActions = false;
            excludeIsomorphicStates = false;
            collapseExcludedIsomorphicStates = false;
            if (this.finiteAutomatonContext != null)
            {
                graphChanged = true;
                PerformLayout();
            }
        }
    }
}

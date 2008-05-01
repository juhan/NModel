//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
//using System.Data;
using System.Text;
using System.Windows.Forms;
using NModel.Terms;
using NModel.Execution;

namespace NModel.Visualization
{

    public partial class StateViewer : UserControl
    {
        IState rootState;

        /// <summary>
        /// The default model name to be used when name is not provided
        /// </summary>
        string defaultModelName = "Fsm";

        /// <summary>
        /// The default model name to be used when name is not provided
        /// </summary>
        public string DefaultModelName
        {
            get { return defaultModelName; }
            set { defaultModelName = value; }
        }
        /// <summary>
        /// Sets the state to be viewed
        /// </summary>
        public void SetState(IState state)
        {
            if (state != rootState)
            {
                if (state != null)
                {
                    rootState = state;
                    StateVariablesViewer viewer = new StateVariablesViewer(state,this.defaultModelName);
                    viewer.Initialize();
                    this.propertyGrid1.SelectedObject = viewer;
                }
                else
                {
                    rootState = null;
                    this.propertyGrid1.SelectedObject = null;
                }
            }
        }

        /// <summary>
        /// Sets the state to be viewed
        /// </summary>
        internal void SetState(IState state, FAName name)
        {
            if (state != rootState)
            {
                if (state != null)
                {
                    rootState = state;
                    StateVariablesViewer viewer = new StateVariablesViewer(state, name);
                    viewer.Initialize();
                    this.propertyGrid1.SelectedObject = viewer;
                }
                else
                {
                    rootState = null;
                    this.propertyGrid1.SelectedObject = null;
                }
            }
        }

        /// <summary>
        /// Visibility of the title
        /// </summary>
        public bool TitleVisible
        {
            get
            {
                return this.labelSelectedState.Visible;
            }
            set
            {
                this.labelSelectedState.Visible = value;
            }
        }

        /// <summary>
        /// The title of the state viewer
        /// </summary>
        public string Title
        {
            get
            {
                return this.labelSelectedState.Text;
            }

            set
            {
                this.labelSelectedState.Text = value;
            }
        }

        private class StateVariablesViewer : DefaultCustomPropertyConverter
        {
            string defaultModelName;
            IState rootState;
            List<IState> leafStates;
            List<string> modelNames;
            Dictionary<TermViewer,int> termViewers; //maps each viewer to the correct state id
            Dictionary<string, int> nameDisambiguator;
            internal string composedModelName;

            internal StateVariablesViewer(IState rootState, string defaultModelName)
            {
                this.defaultModelName = defaultModelName;
                this.rootState = rootState;
                this.leafStates = new List<IState>();
                this.modelNames = new List<string>();
                this.termViewers = new Dictionary<TermViewer, int>();
                this.nameDisambiguator = new Dictionary<string, int>();
            }

            FAName stateName;
            internal StateVariablesViewer(IState rootState, FAName stateName)
            {
                this.stateName = stateName;
                this.rootState = rootState;
                this.leafStates = new List<IState>();
                this.modelNames = new List<string>();
                this.termViewers = new Dictionary<TermViewer, int>();
                this.nameDisambiguator = new Dictionary<string, int>();
            }

            internal void Initialize()
            {
                if (this.stateName == null)
                {
                    StringBuilder sb = new StringBuilder();
                    PreorderLeaves(this.rootState, sb);
                    this.composedModelName = sb.ToString();
                    CreateVariableWiewers();
                }
                else
                {
                    //state names are known
                    PreorderLeaves(this.rootState, stateName);
                    this.composedModelName = stateName.ToString();
                    CreateVariableWiewers();
                }
            }

            #region possibly unknown names
            /// <summary>
            /// Create a post-order traversal of the leaves and
            /// create the composed name that reflects the structure of the tree,
            /// disambiguate multiple occurrences of the same model name
            /// </summary>
            void PreorderLeaves(IState stateNode, StringBuilder sb)
            {
                IPairState pstate = stateNode as IPairState;
                if (pstate != null)
                {
                    sb.Append("(");
                    PreorderLeaves(pstate.First, sb);
                    sb.Append(" x ");
                    PreorderLeaves(pstate.Second, sb);
                    sb.Append(")");
                }
                else
                {
                    string modelName = GetModelName(stateNode);
                    leafStates.Add(stateNode);
                    modelNames.Add(modelName);
                    sb.Append(modelName);
                }
            }

            string GetModelName(IState state)
            {
                string name = this.defaultModelName;
                IExtendedState estate = state as IExtendedState;
                if (estate != null)
                    name = estate.ModelName;
                return Disambiguate(name);
            }

            string Disambiguate(string name)
            {
                if (nameDisambiguator.ContainsKey(name))
                {
                    string newname = name + "_" + nameDisambiguator[name];
                    nameDisambiguator[name] = nameDisambiguator[name] + 1;
                    return newname;
                }
                else{
                    nameDisambiguator.Add(name,1);
                    return name;
                }
            }
            #endregion

            #region known names
            void PreorderLeaves(IState stateNode, FAName sn)
            {
                if (sn != null)
                {
                    IPairState pstate = stateNode as IPairState;
                    if (pstate != null)
                    {
                        PreorderLeaves(pstate.First, sn.left);
                        PreorderLeaves(pstate.Second, sn.right);
                    }
                    else
                    {
                        string modelName = sn.ToString();
                        leafStates.Add(stateNode);
                        modelNames.Add(modelName);
                    }
                }
            }
            #endregion

            void CreateVariableWiewers()
            {
                //view also the control mode of the root state if the product has more than one machine in it
                //the id of the composed state is -1
                //if (leafStates.Count > 1)
                //    termViewers.Add(TermViewer.Create("[Control Mode]", rootState.ControlMode), -1);

                for (int stateId = 0; stateId < this.leafStates.Count; stateId++)
                {
                    IState leafState = this.leafStates[stateId]; 
                    //string modelName = this.modelNames[stateId];

                    //each state has a control mode, create a viewer for it if the controlMode is
                    //not an empty sequence
                    if (!leafState.ControlMode.ToCompactString().Equals("Sequence()"))
                        termViewers.Add(TermViewer.Create("[Control Mode]", leafState.ControlMode), stateId);

                    IExtendedState estate = leafState as IExtendedState;
                    if (estate != null)
                    {
                        #region add the domain map viewer, skip this if there are no domains
                        if (estate.DomainMap.Count > 0)
                        {
                            //StringBuilder sb = new StringBuilder();
                            Sequence<Term> args = Sequence<Term>.EmptySequence;
                            foreach (Pair<Symbol, int> si in estate.DomainMap)
                            {
                                args = args.AddLast(new Literal(si.First.FullName));
                                args = args.AddLast(new Literal(si.Second));
                            }
                            Symbol symb = Symbol.Parse("Map<String, Integer>");
                            Term domMap = new CompoundTerm(symb, args);

                            termViewers.Add(TermViewer.Create("[Domain Map]", domMap), stateId);
                        }
                        #endregion

                        //add a viewer for each state variable
                        for (int j = 0; j < estate.LocationValuesCount; j++)
                        {
                            termViewers.Add(TermViewer.Create(estate.GetLocationName(j),
                                               estate.GetLocationValue(j)), stateId);
                        }
                    }
                }
            }


            #region override methods of DefaultCustomPropertyConverter for customized viewing
            public override System.Collections.ICollection GetKeys()
            {
                return termViewers.Keys;
            }

            public override object ValueOf(object key)
            {
                ElementViewer ev = key as ElementViewer;
                return (ev != null ? ev.t.ToCompactString() : key);
            }
            public override string DisplayNameOf(object key)
            {
                return ((TermViewer)key).keyName;
            }

            //Provides the model name as a category in the property grid
            public override string CategoryOf(object key)
            {
                int stateId = termViewers[(TermViewer)key];
                if (stateId < 0)
                    return this.composedModelName;
                else
                    return modelNames[stateId];
            }
            public override string DescriptionOf(object key)
            {
                return ((TermViewer)key).t.ToString();
            }
            #endregion
        }

        /// <summary>
        /// Creates an instance of the state viewer component
        /// </summary>
        public StateViewer()
        {
            InitializeComponent();

            this.GotFocus += new EventHandler(StateViewer_GotFocus);
            this.LostFocus += new EventHandler(StateViewer_LostFocus);
        }

        void StateViewer_LostFocus(object sender, EventArgs e)
        {
            this.BackColor = SystemColors.InactiveBorder;
        }

        void StateViewer_GotFocus(object sender, EventArgs e)
        {
            this.BackColor = SystemColors.ActiveBorder;
        }

        private void helpVisibleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.propertyGrid1.HelpVisible = ((ToolStripMenuItem)sender).Checked;
        }
    }
}

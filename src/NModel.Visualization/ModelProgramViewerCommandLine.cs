//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using NModel.Visualization;
using NModel.Execution;
using NModel.Utilities;
using NModel.Terms;
using NModel.Algorithms;
using System.Reflection;


namespace NModel.Visualization
{
    /// <summary>
    /// Represents a commandline utility that starts up ModelProgramViewer and displays
    /// a product composition of provided model programs.
    /// </summary>
    public static class CommandLineViewer
    {

        /// <summary>
        /// Provides programmatic access to the ModelProgramViewer commandline utility 'mpv.exe'.
        /// </summary>
        /// <param name="args">command line arguments: model program(s), optional settings for the viewer</param>
        /// <remarks>The settings are displayed when 'mpv.exe /?' is executed from the command line without arguments.</remarks>
        public static void RunWithCommandLineArguments(params string[] args)
        {
            ProgramSettings settings = new ProgramSettings();
            if (!Parser.ParseArgumentsWithUsage(args, settings))
            {
                return;
            }

            #region load the libraries
            List<Assembly> libs = new List<Assembly>();
            try
            {

                if (settings.reference != null)
                {
                    foreach (string l in settings.reference)
                    {
                        libs.Add(System.Reflection.Assembly.LoadFrom(l));
                    }
                }
            }
            catch (Exception e)
            {
                throw new ModelProgramUserException(e.Message);
            }
            #endregion

            #region create a model program for each model using the factory method and compose into product
            string mpMethodName;
            string mpClassName;
            ModelProgram mp = null;
            if (settings.model != null && settings.model.Length > 0)
            {
                if (libs.Count == 0)
                {
                    throw new ModelProgramUserException("No reference was provided to load models from.");
                }
                ReflectionHelper.SplitFullMethodName(settings.model[0], out mpClassName, out mpMethodName);
                Type mpType = ReflectionHelper.FindType(libs, mpClassName);
                MethodInfo mpMethod = ReflectionHelper.FindMethod(mpType, mpMethodName, Type.EmptyTypes, typeof(ModelProgram));
                try
                {
                    mp = (ModelProgram)mpMethod.Invoke(null, null);
                }
                catch (Exception e)
                {
                    throw new ModelProgramUserException("Invocation of '" + settings.model[0] + "' failed: " + e.ToString());
                }
                for (int i = 1; i < settings.model.Length; i++)
                {
                    ReflectionHelper.SplitFullMethodName(settings.model[i], out mpClassName, out mpMethodName);
                    mpType = ReflectionHelper.FindType(libs, mpClassName);
                    mpMethod = ReflectionHelper.FindMethod(mpType, mpMethodName, Type.EmptyTypes, typeof(ModelProgram));
                    ModelProgram mp2 = null;
                    try
                    {
                        mp2 = (ModelProgram)mpMethod.Invoke(null, null);
                    }
                    catch (Exception e)
                    {
                        throw new ModelProgramUserException("Invocation of '" + settings.model[i] + "' failed: " + e.ToString());
                    }
                    mp = new ProductModelProgram(mp, mp2);
                }
            }
            #endregion

            #region create a model program from given namespace and feature names
            if (settings.mp != null && settings.mp.Length > 0)
            {
                if (libs.Count == 0)
                {
                    throw new ModelProgramUserException("No reference was provided to load models from.");
                }
                //parse the model program name and the feature names for each entry
                foreach (string mps in settings.mp)
                {
                    //the first element is the model program, the remaining ones are 
                    //feature names
                    string[] mpsSplit = mps.Split(new string[] { "[", "]", "," },
                        StringSplitOptions.RemoveEmptyEntries);
                    if (mpsSplit.Length == 0)
                    {
                        throw new ModelProgramUserException("Invalid model program specifier '" + mps + "'.");
                    }
                    string mpName = mpsSplit[0];
                    Assembly mpAssembly = ReflectionHelper.FindAssembly(libs, mpName);
                    Set<string> mpFeatures = new Set<string>(mpsSplit).Remove(mpName);
                    ModelProgram mp1 = new LibraryModelProgram(mpAssembly, mpName, mpFeatures);
                    mp = (mp == null ? mp1 : new ProductModelProgram(mp, mp1));
                }
            }

            #endregion

            #region load the test cases if any
            Sequence<Sequence<CompoundTerm>> testcases = Sequence<Sequence<CompoundTerm>>.EmptySequence;
            if (!String.IsNullOrEmpty(settings.testSuite))
            {
                try
                {
                    System.IO.StreamReader testSuiteReader =
                        new System.IO.StreamReader(settings.testSuite);
                    string testSuiteAsString = testSuiteReader.ReadToEnd();
                    testSuiteReader.Close();
                    CompoundTerm testSuite = CompoundTerm.Parse(testSuiteAsString);
                    foreach (CompoundTerm testCaseTerm in testSuite.Arguments)
                    {
                        Sequence<CompoundTerm> testCase =
                            testCaseTerm.Arguments.Convert<CompoundTerm>(delegate (Term t) { return (CompoundTerm)t; });
                        testcases = testcases.AddLast(testCase);
                    }
                }
                catch (Exception e)
                {
                    throw new ModelProgramUserException("Cannot create test suite: " + e.Message);
                }
            }
            #endregion

            #region load the fsms if any
            Dictionary<string, FSM> fsms = new Dictionary<string, FSM>();
            if (settings.fsm != null && settings.fsm.Length > 0)
            {
                try
                {
                    foreach (string fsmFile in settings.fsm)
                    {
                        System.IO.StreamReader fsmReader = new System.IO.StreamReader(fsmFile);
                        string fsmAsString = fsmReader.ReadToEnd();
                        fsmReader.Close();
                        fsms[fsmFile] = FSM.FromTerm(CompoundTerm.Parse(fsmAsString));
                    }
                }
                catch (Exception e)
                {
                    throw new ModelProgramUserException("Cannot create fsm: " + e.Message);
                }
            }
            #endregion

            if (mp == null && testcases.IsEmpty && fsms.Count == 0)
            {
                throw new ModelProgramUserException("No model, fsm, or test suite was given.");
            }

            if (!testcases.IsEmpty)
            {
                FSM fa = FsmTraversals.GenerateTestSequenceAutomaton(
                    settings.startTestAction, testcases, GetActionSymbols(testcases));
                ModelProgram famp = new FsmModelProgram(fa, settings.testSuite);
                if (mp == null)
                    mp = famp;
                else
                    mp = new ProductModelProgram(mp, famp);
            }

            if (fsms.Count > 0)
            {
                foreach (string fsmName in fsms.Keys)
                {
                    ModelProgram fsmmp = new FsmModelProgram(fsms[fsmName], fsmName);
                    if (mp == null)
                        mp = fsmmp;
                    else
                        mp = new ProductModelProgram(mp, fsmmp);
                }
            }

            ModelProgramGraphViewForm form = new ModelProgramGraphViewForm("Model Program Viewer");
            //configure the settings of the viewer
            form.View.AcceptingStatesMarked = settings.acceptingStatesMarked;
            form.View.TransitionLabels = settings.transitionLabels;
            form.View.CombineActions = settings.combineActions;
            form.View.Direction = settings.direction;
            form.View.UnsafeStateColor = Color.FromName(settings.unsafeStateColor);
            form.View.HoverColor = Color.FromName(settings.hoverColor);
            form.View.InitialStateColor = Color.FromName(settings.initialStateColor);
            form.View.LoopsVisible = settings.loopsVisible;
            form.View.MaxTransitions = settings.maxTransitions;
            form.View.NodeLabelsVisible = settings.nodeLabelsVisible;
            form.View.SelectionColor = Color.FromName(settings.selectionColor);
            form.View.MergeLabels = settings.mergeLabels;
            form.View.StateShape = settings.stateShape;
            form.View.DeadStateColor = Color.FromName(settings.deadStateColor);
            form.View.InitialTransitions = settings.initialTransitions;
            form.View.LivenessCheckIsOn = settings.livenessCheckIsOn;
            form.View.ExcludeIsomorphicStates = settings.excludeIsomorphicStates;
            form.View.SafetyCheckIsOn = settings.safetyCheckIsOn;
            form.View.DeadstatesVisible = settings.deadStatesVisible;
            form.View.StateViewVisible = settings.stateViewVisible;

            //show the view of the product of all the model programs
            form.View.SetModelProgram(mp);

            form.OnSaveSettings += new EventHandler(settings.SaveSettings);
            form.ShowDialog();
        }

        private static Set<Symbol> GetActionSymbols(Sequence<Sequence<CompoundTerm>> testcases)
        {
            Set<Symbol> symbs = Set<Symbol>.EmptySet;
            foreach (Sequence<CompoundTerm> testcase in testcases)
                foreach (CompoundTerm action in testcase)
                    symbs = symbs.Add(action.Symbol);
            return symbs;
        }
    }

    internal sealed class ProgramSettings
    {
        public ProgramSettings()
        {
            model = null;
            mp = null;
            reference = null;
            combineActions = false;
            livenessCheckIsOn = false;
            safetyCheckIsOn = false;
            testSuite = "";
            fsm = null;
            startTestAction = "Test";
            excludeIsomorphicStates = false;
            collapseExcludedIsomorphicStates = false;
            stateViewVisible = false;
        }

        public void SaveSettings(object sender, EventArgs e)
        {
            System.Windows.Forms.SaveFileDialog dialog = new System.Windows.Forms.SaveFileDialog();
            dialog.InitialDirectory = System.Environment.CurrentDirectory;
            dialog.OverwritePrompt = true;
            dialog.Title = "Save Settings";
            dialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            dialog.FilterIndex = 0;
            dialog.RestoreDirectory = false;
            System.Windows.Forms.DialogResult res = dialog.ShowDialog();

            if (res == System.Windows.Forms.DialogResult.OK && !String.IsNullOrEmpty(dialog.FileName))
            {
                System.IO.StreamWriter sw = new System.IO.StreamWriter(dialog.FileName);
                sw.Write(this.GetString(((ModelProgramGraphViewForm)sender).View));
                sw.Close();
            }
        }

        string GetString(ModelProgramGraphView v)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("# --- Assemblies ---\n");
            if (reference != null)
                foreach (string r in reference)
                {
                    sb.Append("/r:");
                    sb.Append(r);
                    sb.Append("\n");
                }
            sb.Append("\n# --- Models ---\n");
            if (model != null)
                foreach (string m in model)
                {
                    sb.Append(m);
                    sb.Append("\n");
                }
            if (fsm != null)
                foreach (string f in fsm)
                {
                    sb.Append("/fsm:");
                    sb.Append(f);
                    sb.Append("\n");
                }
            if (!String.IsNullOrEmpty(testSuite))
            {
                sb.Append("/testSuite:");
                sb.Append(testSuite);
                sb.Append("\n");
                sb.Append("/startTestAction:");
                sb.Append(startTestAction);
                sb.Append("\n");
            }
            sb.Append("\n# --- Analysis ---\n");
            sb.Append("/livenessCheckIsOn");
            sb.Append((v.LivenessCheckIsOn ? "" : "-"));
            sb.Append("\n/safetyCheckIsOn");
            sb.Append((v.SafetyCheckIsOn ? "" : "-"));
            sb.Append("\n/deadStatesVisible");
            sb.Append((v.DeadstatesVisible ? "" : "-"));
            sb.Append("\n/deadStateColor:");
            sb.Append(v.DeadStateColor.Name);
            sb.Append("\n/unsafeStateColor:");
            sb.Append(v.UnsafeStateColor.Name);

            sb.Append("\n\n# --- Exploration limits ---\n");
            sb.Append("/initialTransitions:");
            sb.Append(initialTransitions);
            sb.Append("\n/maxTransitions:");
            sb.Append(v.MaxTransitions);

            sb.Append("\n\n# --- States ---\n");
            sb.Append("/nodeLabelsVisible");
            sb.Append((v.NodeLabelsVisible ? "" : "-"));
            sb.Append("\n/acceptingStatesMarked");
            sb.Append((v.AcceptingStatesMarked ? "" : "-"));
            sb.Append("\n/stateShape:");
            sb.Append(v.StateShape.ToString());
            sb.Append("\n/initialStateColor:");
            sb.Append(v.InitialStateColor.Name);

            sb.Append("\n\n# --- Transitions ---\n");
            sb.Append("/transitionLabels:");
            sb.Append(v.TransitionLabels.ToString());
            sb.Append("\n/loopsVisible");
            sb.Append(v.LoopsVisible ? "" : "-");
            sb.Append("\n/combineActions");
            sb.Append(v.CombineActions ? "" : "-");
            sb.Append("\n/mergeLabels");
            sb.Append(v.MergeLabels ? "" : "-");


            sb.Append("\n\n# --- Graph ---\n");
            sb.Append("/hoverColor:");
            sb.Append(v.HoverColor.Name);
            sb.Append("\n/selectionColor:");
            sb.Append(v.SelectionColor.Name);
            sb.Append("\n/direction:");
            sb.Append(v.Direction.ToString());
            sb.Append("\n");

            return sb.ToString();
        }

        [DefaultArgument(ArgumentType.MultipleUnique, HelpText = "Fully qualified names of factory methods returning an object that implements ModelProgram. Multiple model programs are composed into a product.")]
        public string[] model;

        [Argument(ArgumentType.MultipleUnique, ShortName = "", HelpText = "Model programs given on the form M or M[F1,...,Fn] where M is a model program name (namespace) and each Fi is a feature in M. Multiple model programs are composed into a product. No factory method is needed if this option is used.")]
        public string[] mp;

        [Argument(ArgumentType.MultipleUnique, ShortName = "r", HelpText = "Referenced assemblies.")]
        public string[] reference;

        [Argument(ArgumentType.LastOccurenceWins, DefaultValue = -1, ShortName = "", HelpText = "Number of transitions that are explored initially up to maxTransitions. Negative value implies no bound.")]
        public int initialTransitions = -1;

        [Argument(ArgumentType.LastOccurenceWins, DefaultValue = TransitionLabel.Action, ShortName = "", HelpText = "Determines what is shown as a transition label.")]
        public TransitionLabel transitionLabels = TransitionLabel.Action;

        [Argument(ArgumentType.LastOccurenceWins, DefaultValue = true, ShortName = "", HelpText = "Visibility of node labels.")]
        public bool nodeLabelsVisible = true;

        [Argument(ArgumentType.LastOccurenceWins, DefaultValue = "LightGray", ShortName = "", HelpText = "Background color of the initial state.")]
        public string initialStateColor = "LightGray";

        [Argument(ArgumentType.LastOccurenceWins, DefaultValue = "Lime", ShortName = "", HelpText = "Line and action label color to use when edges or nodes are hovered over.")]
        public string hoverColor = "Lime";

        [Argument(ArgumentType.LastOccurenceWins, DefaultValue = "Blue", ShortName = "", HelpText = "Background color to use when a node is selected.")]
        public string selectionColor = "Blue";

        [Argument(ArgumentType.LastOccurenceWins, DefaultValue = "Yellow", ShortName = "", HelpText = "Background color of dead states. Dead states are states from which no accepting state is reachable.")]
        public string deadStateColor = "Yellow";

        [Argument(ArgumentType.LastOccurenceWins, DefaultValue = true, ShortName = "", HelpText = "Visibility of dead states.")]
        public bool deadStatesVisible = true;

        [Argument(ArgumentType.LastOccurenceWins, DefaultValue = "Red", ShortName = "", HelpText = "Background color of states that violate a safety condition (state invariant).")]
        public string unsafeStateColor = "Red";

        [Argument(ArgumentType.LastOccurenceWins, DefaultValue = 100, ShortName = "", HelpText = "Maximum number of transitions to draw in the graph.")]
        public int maxTransitions = 100;

        [Argument(ArgumentType.LastOccurenceWins, DefaultValue = true, ShortName = "", HelpText = "Visibility of transitions whose start and end states are the same.")]
        public bool loopsVisible = true;

        [Argument(ArgumentType.LastOccurenceWins, DefaultValue = true, ShortName = "", HelpText = "Multiple transitions between same start and end states are shown as one transition with a merged label.")]
        public bool mergeLabels = true;

        [Argument(ArgumentType.LastOccurenceWins, DefaultValue = true, ShortName = "", HelpText = "Mark accepting states with a bold outline.")]
        public bool acceptingStatesMarked = true;

        [Argument(ArgumentType.LastOccurenceWins, DefaultValue = StateShape.Ellipse, ShortName = "", HelpText = "State shape.")]
        public StateShape stateShape = StateShape.Ellipse;

        [Argument(ArgumentType.LastOccurenceWins, DefaultValue = GraphDirection.TopToBottom, ShortName = "", HelpText = "Direction of graph layout.")]
        public GraphDirection direction = GraphDirection.TopToBottom;

        [Argument(ArgumentType.LastOccurenceWins, DefaultValue = false, ShortName = "", HelpText = "Whether to view matching start and finish actions by a single label.")]
        public bool combineActions;

        [Argument(ArgumentType.LastOccurenceWins, DefaultValue = false, ShortName = "", HelpText = "Mark states from which no accepting state is reachable in the current view.")]
        internal bool livenessCheckIsOn;

        [Argument(ArgumentType.LastOccurenceWins, DefaultValue = false, ShortName = "", HelpText = "Mark states that violate a safety condition (state invariant).")]
        internal bool safetyCheckIsOn;

        [Argument(ArgumentType.LastOccurenceWins, ShortName = "", HelpText = "File name of a file containing a sequence of actions sequences (test cases) to be viewed.")]
        public string testSuite;

        [Argument(ArgumentType.AtMostOnce, DefaultValue = false, ShortName = "", HelpText = "Exclude isomorphic states from state space (symmetry reduction)")]
        internal bool excludeIsomorphicStates;

        [Argument(ArgumentType.AtMostOnce, DefaultValue = false, ShortName = "", HelpText = "Group excluded isomorphic states together (symmetry reduction)")]
        internal bool collapseExcludedIsomorphicStates;

        [Argument(ArgumentType.MultipleUnique, ShortName = "", HelpText = "File name of a file containing the term representation fsm.ToTerm() of an fsm (object of type FSM). Multiple fsms are composed into a product.")]
        public string[] fsm;

        [Argument(ArgumentType.LastOccurenceWins, ShortName = "", DefaultValue = "Test", HelpText = "Name of start action of a test case. This value is used only if a testSuite is provided.")]
        public string startTestAction;

        [Argument(ArgumentType.AtMostOnce, DefaultValue = false, ShortName = "", HelpText = "Whether the State View is visible.")]
        public bool stateViewVisible;
    }

    public static class Interactive {

        /// <summary>
        /// Provides programmatic access to the ModelProgramViewer commandline utility 'mpv.exe'.
        /// </summary>
        /// <param name="args">command line arguments: model program(s), optional settings for the viewer</param>
        /// <remarks>The settings are displayed when 'mpv.exe /?' is executed from the command line without arguments.</remarks>
        public static void Run(ModelProgram mp)
        {

            ProgramSettings settings = new ProgramSettings();

            //ModelProgram mp = (ModelProgram)lmp;

            ModelProgramGraphViewForm form = new ModelProgramGraphViewForm("Model Program Viewer");
            //configure the settings of the viewer
            form.View.AcceptingStatesMarked = settings.acceptingStatesMarked;
            form.View.TransitionLabels = settings.transitionLabels;
            form.View.CombineActions = settings.combineActions;
            form.View.Direction = settings.direction;
            form.View.UnsafeStateColor = Color.FromName(settings.unsafeStateColor);
            form.View.HoverColor = Color.FromName(settings.hoverColor);
            form.View.InitialStateColor = Color.FromName(settings.initialStateColor);
            form.View.LoopsVisible = settings.loopsVisible;
            form.View.MaxTransitions = settings.maxTransitions;
            form.View.NodeLabelsVisible = settings.nodeLabelsVisible;
            form.View.SelectionColor = Color.FromName(settings.selectionColor);
            form.View.MergeLabels = settings.mergeLabels;
            form.View.StateShape = settings.stateShape;
            form.View.DeadStateColor = Color.FromName(settings.deadStateColor);
            form.View.InitialTransitions = settings.initialTransitions;
            form.View.LivenessCheckIsOn = settings.livenessCheckIsOn;
            form.View.ExcludeIsomorphicStates = settings.excludeIsomorphicStates;
            form.View.SafetyCheckIsOn = settings.safetyCheckIsOn;
            form.View.DeadstatesVisible = settings.deadStatesVisible;
            form.View.StateViewVisible = settings.stateViewVisible;

            //show the view of the product of all the model programs
            form.View.SetModelProgram(mp);

            form.OnSaveSettings += new EventHandler(settings.SaveSettings);
            form.ShowDialog();
        }

        private static Set<Symbol> GetActionSymbols(Sequence<Sequence<CompoundTerm>> testcases)
        {
            Set<Symbol> symbs = Set<Symbol>.EmptySet;
            foreach (Sequence<CompoundTerm> testcase in testcases)
                foreach (CompoundTerm action in testcase)
                    symbs = symbs.Add(action.Symbol);
            return symbs;
        }
    }

}


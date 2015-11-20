//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using NModel.Terms;
using NModel;
using NModel.Execution;
using Action = NModel.Terms.Action;

namespace NModel.Conformance
{
    //This part includes all the publicly configurable settings of the conformance tester

    /// <summary>
    /// Delegate for passing information about completed test runs.
    /// </summary>
    /// <param name="testResult">test result containig the verdict and the action trace</param>
    /// <returns>testing stops if false is returned, testing continues otherwise</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
    public delegate bool TestResultDelegate(TestResult testResult);

    /// <summary>
    /// Delegate for returning the amount of time within which the given tester action 
    /// call to the implementation must return. 
    /// If the action does not return, a conformance failure occurs.
    /// </summary>
    /// <param name="state">given model state</param>
    /// <param name="action">given tester action</param>
    /// <returns>amount of time to wait</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
    public delegate TimeSpan TesterActionTimeoutDelegate(IState state, CompoundTerm action);

    partial class ConformanceTester
    {
        #region TestResultNotifier
        /// <summary>
        /// Test result delegate that is called each time a test run is completed.
        /// </summary>
        TestResultDelegate testResultNotifier;
        /// <summary>
        /// Gets or sets the test result delegate that is called each time a test run is completed.
        /// The default notifier prettyprints the term representation of each test result to the console or 
        /// a logfile (if one is provided).
        /// </summary>
        public TestResultDelegate TestResultNotifier
        {
            get
            {
                return testResultNotifier;
            }
            set
            {
                if (value == null)
                    throw new ConformanceTesterException("TestResultNotifier cannot be set to null.");
                testResultNotifier = value;
            }
        }

        bool DefaultTestResultNotifier(TestResult testResult)
        {

            // Failed actions metrics
            AddFailedActionsWithMessages(testResult);

            using (StreamWriter sw = GetStreamWriter())
            {
                WriteLine(sw, "TestResult(" + testResult.testNr + ", "
                                                + "Verdict(\"" + testResult.verdict + "\"), \""
                                                + testResult.reason + "\",");

                // Requirements metrics
                if (showTestCaseCoveredRequirements == true)
                    AddExecutedRequirementsToTest(testResult, sw);

                WriteLine(sw, "    Trace(");
                for (int i = 0; i < testResult.trace.Count; i++)
                {
                    WriteLine(sw, "        " + testResult.trace[i].ToString() +
                                      (i < testResult.trace.Count - 1 ? "," : ""));

                }
                WriteLine(sw, "    )");
            }
            if (!this.continueOnFailure)
            {
                return testResult.verdict == Verdict.Success;
            }
            else
            {
                return true;
            }
        }
        #endregion

        #region ContinueOnFailure
        bool continueOnFailure = true;
        /// <summary>
        /// If set to false, the default test result notifier returns false when 
        /// a test case fails. Default is true.
        /// This setting has no effect if the <see cref="TestResultNotifier"/> has been set
        /// to a custom notifier.
        /// </summary>
        public bool ContinueOnFailure
        {
            get { return continueOnFailure; }
            set { continueOnFailure = value; }
        }
        #endregion

        #region StepsCnt
        int stepsCnt = 0;
        /// <summary>
        /// The desired number of steps that a single test run should have.
        /// After the number is reached, only cleanup tester actions are used
        /// and the test run continues until an accepting state is reached or 
        /// the number of steps is MaxStepsCnt (whichever occurs first).
        /// </summary>
        /// <remarks>Negative value or 0 implies no bound and a test case is executed 
        /// until either a failure occurs or no more actions are enabled.
        /// Default is 0.</remarks>
        public int StepsCnt
        {
            get
            {
                return stepsCnt;
            }
            set
            {
                stepsCnt = value;
            }
        }
        #endregion

        #region MaxStepsCnt
        int maxStepsCnt = 0;
        /// <summary>
        /// The maximum number of steps that a single test run can have. This value must be 0, which means that there is no bound, or greater than or equal to stepsCnt.
        /// If stepsCnt is 0 then stepsCnt is set to be equal to maxStepsCnt.
        /// </summary>
        public int MaxStepsCnt
        {
            get
            {
                return maxStepsCnt;
            }
            set
            {
                if (value != 0 && value < stepsCnt)
                    throw new ConformanceTesterException("The maximum number of steps in a run cannot be less than the desired number of steps in a run.");
                if (stepsCnt == 0)
                    stepsCnt = value;
                maxStepsCnt = value;
            }
        }
        #endregion

        #region RunsCnt
        int runsCnt = 0;
        /// <summary>
        /// The desired number of test runs. Must be nonnegative. Testing stops when this number has been reached.
        /// Test runs are numbered from 0 to RunsCnt-1.
        /// </summary>
        /// <remarks>0 implies no bound. Default is 0.</remarks>
        public int RunsCnt
        {
            get
            {
                return runsCnt;
            }
            set
            {
                if (value < 0)
                    throw new ConformanceTesterException("Number of test runs cannot be negative.");
                runsCnt = value;
            }
        }
        #endregion

        #region TesterActionTimeout
        /// <summary>
        /// Returns the amount of time within which the given tester action 
        /// call to the implementation must return. 
        /// If the action does not return, a conformance failure occurs.
        /// Default is 100ms.
        /// </summary>
        TesterActionTimeoutDelegate testerActionTimeout;
        /// <summary>
        /// Returns the amount of time within which the given tester action 
        /// call to the implementation must return. 
        /// If the action does not return, a conformance failure occurs.
        /// Default is 100ms.
        /// </summary>
        public TesterActionTimeoutDelegate TesterActionTimeout
        {
            get
            {
                return testerActionTimeout;
            }
            set
            {
                if (value == null)
                    throw new ConformanceTesterException("TesterActionTimeout cannot be set to null");
                testerActionTimeout = value;
            }
        }
        #endregion

        #region ObservableActionSymbols and testerActionSymbols
        /// <summary>
        /// Set of action symbols controlled by the tester.
        /// Default is all action symbols of the model program.
        /// </summary>
        Set<Symbol> testerActionSymbols;
        /// <summary>
        /// Set of action symbols controlled by the implementation.
        /// Default is the empty set.
        /// </summary>
        public Set<Symbol> ObservableActionSymbols
        {
            get
            {
                return this.model.ActionSymbols.Difference(testerActionSymbols);
            }
            set
            {
                if (value == null)
                    throw new ConformanceTesterException("ObservableActionSymbols cannot be set to null");
                Set<Symbol> unknown = value.Difference(this.model.ActionSymbols);
                if (!unknown.IsEmpty)
                    throw new ConformanceTesterException("Unexpected ObservableActionSymbols: " + unknown.ToString());
                testerActionSymbols = this.model.ActionSymbols.Difference(value); ;
            }
        }
        #endregion

        #region CleanupActionSymbols
        /// <summary>
        /// Subset of tester action symbols that are cleanup action symbols.
        /// Default is the empty set.
        /// </summary>
        Set<Symbol> cleanupActionSymbols;
        /// <summary>
        /// Subset of tester action symbols that are cleanup action symbols.
        /// Default is the empty set.
        /// </summary>
        public Set<Symbol> CleanupActionSymbols
        {
            get
            {
                return cleanupActionSymbols;
            }
            set
            {
                if (value == null)
                    throw new ConformanceTesterException("CleanupActionSymbols cannot be set to null");
                Set<Symbol> unknown = value.Difference(this.model.ActionSymbols);
                if (!unknown.IsEmpty)
                    throw new ConformanceTesterException("Unexpected CleanupActionSymbols: " + unknown.ToString());
                cleanupActionSymbols = value;
            }
        }
        #endregion

        #region InternalActionSymbols
        /// <summary>
        /// Subset of tester action symbols that are not shared with the implementation.
        /// Default is the empty set.
        /// </summary>
        Set<Symbol> internalActionSymbols;
        /// <summary>
        /// Subset of tester action symbols that are not shared with the implementation.
        /// Default is the empty set.
        /// </summary>
        public Set<Symbol> InternalActionSymbols
        {
            get
            {
                return internalActionSymbols;
            }
            set
            {
                if (value == null)
                    throw new ConformanceTesterException("InternalActionSymbols cannot be set to null");
                Set<Symbol> unknown = value.Difference(this.model.ActionSymbols);
                if (!unknown.IsEmpty)
                    throw new ConformanceTesterException("Unexpected InternalActionSymbols: " + unknown.ToString());
                internalActionSymbols = value;
            }
        }
        #endregion

        #region Logfile
        /// <summary>
        /// Filename where test results are logged by the default test result notifier. 
        /// The console is used if no logfile is provided.
        /// </summary>
        string logfile;
        /// <summary>
        /// Filename where test results are logged by the default test result notifier. 
        /// The console is used if no logfile is provided.
        /// </summary>
        public string Logfile
        {
            get { return logfile; }
            set { logfile = value; }
        }
        #endregion

        #region OverwriteLog
        /// <summary>
        /// If true the log file is overwritten, otherwise the testresults are appended to the logfile (if provided).
        /// </summary>
        bool overwriteLog = true;
        /// <summary>
        /// If true the log file is overwritten, otherwise the testresults are appended to the logfile (if provided).
        /// </summary>
        public bool OverwriteLog
        {
            get { return overwriteLog; }
            set { overwriteLog = value; }
        }
        #endregion

        #region RandomSeed
        /// <summary>
        /// A number used to calculate the starting value for the pseudo-random number sequence 
        /// that is used by the global choice controller. 
        /// If a negative number is specified, the absolute value is used.
        /// </summary>
        int randomSeed;
        /// <summary>
        /// A number used to calculate the starting value for the pseudo-random number sequence 
        /// that is used by the global choice controller.
        /// If a negative number is specified, the absolute value is used.
        /// Setting this property resets the GlobalChoiceController with a new instance 
        /// of the System.Random class with the given random seed.
        /// </summary>
        public int RandomSeed
        {
            get
            {
                return randomSeed;
            }
            set
            {
                randomSeed = value;
                NModel.Internals.HashAlgorithms.GlobalChoiceController = new Random(value);
            }
        }
        #endregion

        #region WaitAction
        internal Set<Symbol> waitActionSet = new Set<Symbol>(Symbol.Parse("Wait"));
        /// <summary>
        /// A name of an action that is used to wait for observable actions in a 
        /// state where no controllable actions are enabled. A wait action is controllable 
        /// and internal and must take one integer argument that determines the time to 
        /// wait in milliseconds during which an observable action is expected. Default is "Wait".
        /// Only used with IAsyncStepper.
        /// </summary>
        public string WaitAction
        {
            get
            {
                return waitActionSet.Choose().Name;
            }
            set
            {
                waitActionSet = new Set<Symbol>(Symbol.Parse(value));
            }
        }
        #endregion

        #region TimeoutAction
        /// <summary>
        /// A name of an action that happens when a wait action has been executed and no 
        /// obsevable action occurred within the time limit provided in the wait action. 
        /// A timeout action is observable and takes no arguments. Default is "Timeout".
        /// Only used with IAsyncStepper.
        /// </summary>
        internal Action timeoutAction = Action.Create("Timeout");
        /// <summary>
        /// A name of an action that happens when a wait action has been executed and no 
        /// obsevable action occurred within the time limit provided in the wait action. 
        /// A timeout action is observable and takes no arguments. Default is "Timeout".
        /// Only used with IAsyncStepper.
        /// </summary>
        public string TimeoutAction
        {
            get
            {
                return timeoutAction.Name;
            }
            set
            {
                timeoutAction = Action.Create(value);
            }
        }
        #endregion

        bool logFileWasAlreadyOpened;

        StreamWriter GetStreamWriter()
        {
            if (String.IsNullOrEmpty(logfile))
                return null;
            else
            {
                StreamWriter sw;
                if (logFileWasAlreadyOpened)
                    sw = new StreamWriter(logfile, true);
                else
                    sw = new StreamWriter(logfile, !OverwriteLog);
                logFileWasAlreadyOpened = true;
                return sw;
            }
        }

        static void WriteLine(StreamWriter sw, object value)
        {
            if (sw == null)
                Console.WriteLine(value);
            else
                sw.WriteLine(value);
        }
    }
}

//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using NModel;
using NModel.Terms;
using NModel.Algorithms;
using NModel.Execution;

using Action = NModel.Terms.CompoundTerm;
using ActionSymbol = NModel.Terms.Symbol;


namespace NModel.Conformance
{
    /// <summary>
    /// Provides conformance testing functionality. 
    /// </summary>
    public sealed partial class ConformanceTester : IDisposable
    {
        IStrategy model;
        IStepper impl;
        TimedQueue<Action> observations;
        TimedWorker worker;

        static TimeSpan defaultTimeSpan = new TimeSpan(0, 0, 0, 0, 100);
        /// <summary>
        /// Creates an instance of the conformance tester for an IUT stepper.
        /// </summary>
        /// <param name="model">given model stepper</param>
        /// <param name="impl">given implementation stepper</param>
        public ConformanceTester(IStrategy model, IStepper impl)
        {
            this.model = model;
            this.impl = impl;
            //set the callback in the implementation
            //if the implementation implements IAsyncStepper
            IAsyncStepper implWithObs = impl as IAsyncStepper;
            if (implWithObs != null)
            {
                TimedQueue<Action> obs = new TimedQueue<Action>();
                this.observations = obs;
                implWithObs.SetObserver(obs.Enqueue);
            }
            this.testResultNotifier = this.DefaultTestResultNotifier;
            this.testerActionTimeout = delegate(IState state, CompoundTerm action) { return defaultTimeSpan; };
            this.testerActionSymbols = model.ActionSymbols;
            this.cleanupActionSymbols = Set<Symbol>.EmptySet;
            this.internalActionSymbols = Set<Symbol>.EmptySet;
            this.RandomSeed = new Random().Next();
            this.worker = new TimedWorker();
        }

        internal bool IsAsync
        {
            get
            {
                return this.observations != null;
            }
        }

        /// <summary>
        /// Reset the implementation and the model
        /// </summary>
        /// <exception cref="ConformanceTesterException">Is thrown when Reset fails</exception>
        public void Reset()
        {
            try
            {
                impl.Reset();
                model.Reset();
                if (observations != null)
                    observations.Clear();
            }
            catch (Exception e)
            {
                throw new ConformanceTesterException("Reset failed: " + e.Message, e);
            }
        }

        /// <summary>
        /// Call RunTestCase runsCnt times, reset in between runs.
        /// </summary>
        /// <exception cref="ConformanceTesterException">Is thrown when Run does not finish normally</exception>
        public void Run()
        {
            bool reset = false;
            try
            {
                int run = 0;
                while ((runsCnt <= 0) || run < runsCnt)
                {
                    TestResult testResult = RunTestCase(run);

                    // Tests results summary metrics
                    if (testResult.verdict == Verdict.Failure)
                        ++totalFailedTests;

                    // Requirements metrics 
                    totalExecutedRequirements = totalExecutedRequirements.Union(testResult.executedRequirements);

                    if (!this.testResultNotifier(testResult))
                    {
                        Reset();

                        //Metrics
                        AddMetricsToEndOfLog();

                        return;
                    }
                    Reset();
                    run += 1;

                    // Tests results summary metrics
                    totalExecutedTests = run;
                }

                // Metrics 
                AddMetricsToEndOfLog();
            }
            catch (Exception e)
            {
                if (e.Message != "Reset failed.")
                    reset = true;
                if (e is ConformanceTesterException)
                {
                    throw;
                }
                throw new ConformanceTesterException("Run failed. " + e.Message);
            }
            finally
            {
                //dispose of the worker thread pool
                worker.Dispose();
                if (reset)
                    Reset();
                worker = null;
            }
        }


        /// <summary>
        /// Run a single test case.
        /// Returns the result of the test case, containing the action trace
        /// </summary>
        /// <param name="testNr">test case number</param>
        /// <returns>test result</returns>
        internal TestResult RunTestCase(int testNr)
        {
            Sequence<Action> testCase = Sequence<Action>.EmptySequence;
            Action/*?*/ o = null;

            // Requirements metrics
            Bag<Pair<string, string>> executedRequirements = Bag<Pair<string, string>>.EmptyBag;

            while ((this.stepsCnt <= 0) || testCase.Count < stepsCnt || (!model.IsInAcceptingState && (maxStepsCnt <= 0 || testCase.Count < maxStepsCnt)))
            {
                if (o != null || (IsAsync && !observations.IsEmpty))
                {
                    #region there is an implementation action o to be checked
                    if (o == null)
                        o = observations.Dequeue();

                    testCase = testCase.AddLast(o);  //record the action in the testCase

                    string failureReason = "";
                    if (model.IsActionEnabled(o, out failureReason)) //check conformance to the model
                    {
                        model.DoAction(o);
                        o = null;                    //consume the action
                    }
                    else
                        return new TestResult(testNr, Verdict.Failure, failureReason, testCase, executedRequirements); // Requirements metrics: ", executedRequirements"
                    #endregion
                }
                else
                {
                    //use only cleanup actions when in cleanup phase
                    Set<Symbol> actionSymbols = (((stepsCnt <= 0) || testCase.Count < stepsCnt) ? this.testerActionSymbols : this.cleanupActionSymbols);

                    //select a tester action that is enabled in the current model state
                    Action testerAction = model.SelectAction(actionSymbols);

                    //if a tester action could be chosen
                    if (testerAction != null)
                    {
                        #region execute the tester action

                        //get the timespan within which calling impl with testerAction must return
                        TimeSpan t = (!internalActionSymbols.Contains(testerAction.Symbol) ?
                            this.testerActionTimeout(model.CurrentState, testerAction) : new TimeSpan());

                        //do the action in the model
                        model.DoAction(testerAction);

                        // Requirements metrics

                        string actionName = testerAction.Name;
                        foreach (string methodName in LibraryModelProgram.AllModeledRequirements.Keys)
                        {
                            // The methods names don't contain "_Start"
                            // when testerAction.Name == actionName_Start, remove the "_Start"
                            // in order to check it in AllModeledRequirements.Keys 
                            if (actionName.Contains("_Start"))
                                actionName = actionName.Replace("_Start", "");
                            // I use 'Contains' to get all the enabled actions as well
                            if (methodName.Contains(actionName))
                            {
                                foreach (Pair<string, string> req in LibraryModelProgram.AllModeledRequirements[methodName])
                                    executedRequirements = executedRequirements.Add(req);
                            }
                        }

                        //record the action in the testCase
                        testCase = testCase.AddLast(testerAction);

                        //call the implementation if the symbol is shared
                        if (!internalActionSymbols.Contains(testerAction.Symbol))
                        {
                            try
                            {
                                DateTime startAction = DateTime.Now; // Performance metrics

                                o = DoAction(testerAction, t); //if return value is non-null it will be checked next time around

                                // Requirements metrics
                                CalcPerformance(testerAction, startAction);
                            }
                            catch (ConformanceTesterException e)
                            {
                                return new TestResult(testNr, Verdict.Failure, e.Message, testCase, executedRequirements);  //conformance failure // Requirements : ", executedRequirements"
                            }
                        }
                        #endregion
                    }
                    else
                    {
                        //otherwise, try to get an implementation action
                        if (IsAsync)
                        {
                            //get the Wait action from the model
                            Action w = model.SelectAction(waitActionSet);
                            int obsTimeout = (w == null ? 0 : (int)w[0]);
                            if (w != null)
                            {
                                testCase = testCase.AddLast(w);
                                model.DoAction(w);
                            }
                            if (!observations.TryDequeue(new TimeSpan(0, 0, 0, 0, obsTimeout), out o))
                            {
                                //if there are no tester actions and no observables but the 
                                //model is in accepting state, the test succeeds
                                if (model.IsInAcceptingState)
                                    return new TestResult(testNr, Verdict.Success, "", testCase, executedRequirements);// Requirements metrics: ", executedRequirements"
                                else
                                    o = timeoutAction;
                            }
                        }
                        else
                        {
                            //if there are no tester actions and no observables but the 
                            //model is in accepting state, the test succeeds
                            if (model.IsInAcceptingState)
                                return new TestResult(testNr, Verdict.Success, "", testCase, executedRequirements);// Requirements metrics: ", executedRequirements"
                            else
                                return new TestResult(testNr, Verdict.Failure, "Run stopped in a non-accepting state", testCase, executedRequirements);// Requirements metrics: ", executedRequirements"
                        }
                    }
                }
            }
            if (model.IsInAcceptingState)
                return new TestResult(testNr, Verdict.Success, "", testCase, executedRequirements);// Requirements metrics: ", executedRequirements"
            else
                return new TestResult(testNr, Verdict.Failure, "Test run did not finish in accepting state", testCase, executedRequirements);// Requirements metrics: ", executedRequirements"
        }


        #region calling the implementation

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        static void CallTheImplementation(object implAndActionAndRes)
        {
            object[] args = (object[])implAndActionAndRes;
            try
            {
                Action implAction = ((IStepper)args[0]).DoAction((Action)args[1]);
                ((ImplementationResultWrapper)args[2]).implAction = implAction;
            }

            catch (Exception e)
            {
                ((ImplementationResultWrapper)args[2]).exception = e;
            }
        }
        //ParameterizedThreadStart CallTheImplementationThreadStart = new ParameterizedThreadStart(CallTheImplementation);

        Action DoAction(Action action, TimeSpan t)
        {
            ImplementationResultWrapper implRes = new ImplementationResultWrapper();
            //Thread CallTheImplementationThread = new Thread(CallTheImplementationThreadStart);
            //CallTheImplementationThread.Start(new object[] { impl, action, implRes});
            //bool ok = CallTheImplementationThread.Join(t);
            bool ok = worker.StartWork(CallTheImplementation,
                new object[] { impl, action, implRes }, t);
            if (!ok)
            {
                //CallTheImplementationThread.Abort();

                throw new ConformanceTesterException("Action timed out");  //conformance failure
            }
            else
            {
                if (implRes.exception != null)
                    //rethrow the exception thrown by the implementation
                    //include only the message in the exception
                    throw new ConformanceTesterException(MakeQuotedString(implRes.exception.Message)); //conformance failure
                else
                {
                    return implRes.implAction;           //return either null or the following implementation action
                }
            }
        }

        internal static string MakeQuotedString(string s)
        {
            return s.Replace("\"", "\\\"");
        }

        private class ImplementationResultWrapper
        {
            internal Exception exception = null;
            internal CompoundTerm implAction = null;
            internal ImplementationResultWrapper()
            {
            }
        }
        #endregion

        #region IDisposable Members

        /// <summary>
        /// Dispose the conformance tester
        /// </summary>
        public void Dispose()
        {
            if (this.worker != null)
                this.worker.Dispose();
        }

        #endregion
    }

    /// <summary>
    /// Exception that is thrown by the ConformanceTester
    /// </summary>
    [Serializable]
    public sealed class ConformanceTesterException : Exception
    {
        /// <summary>
        /// Create a conformance tester exception with a given message and given inner exception
        /// </summary>
        public ConformanceTesterException(string message, Exception innerException)
            :
            base(message, innerException)
        {
        }

        private ConformanceTesterException(System.Runtime.Serialization.SerializationInfo si, System.Runtime.Serialization.StreamingContext sc)
            : base(si, sc)
        {
        }

        /// <summary>
        /// Create a conformance tester exception with a given message
        /// </summary>
        public ConformanceTesterException(string message)
            :
            base(message)
        {
        }

        /// <summary>
        /// Create a conformance tester exception
        /// </summary>
        public ConformanceTesterException()
            :
            base()
        {
        }
    }


}

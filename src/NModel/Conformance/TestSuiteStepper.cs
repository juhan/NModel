//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using NModel.Execution;
using NModel.Terms;
using NModel.Algorithms;

namespace NModel.Conformance
{
    /// <summary>
    /// Provides a model stepper for a test suite given as a sequence of test cases in form of action sequences.
    /// </summary>
    public class TestSuiteStepper : Strategy
    {
        Sequence<Sequence<CompoundTerm>> testsuite;
        bool currentTestInProgress;
        int testnr;
        Sequence<CompoundTerm> currentTest;
        string startTest;

        /// <summary>
        /// Constructs a model stepper for a given test suite
        /// </summary>
        /// <param name="startTest">name of start action of a test case</param>
        /// <param name="testsuite">given test suite as a nonempty sequence of test cases</param>
        public TestSuiteStepper(string startTest, Sequence<Sequence<CompoundTerm>> testsuite) :
            this(startTest, testsuite, null)
        {
        }


        /// <summary>
        /// Constructs a model stepper for a given test suite and a given model program,
        /// using the product of the test suite and the model program.
        /// </summary>
        /// <param name="startTest">name of start action of a test case</param>
        /// <param name="testsuite">given test suite as a nonempty sequence of test cases</param>
        /// <param name="mp">given model program</param>
        public TestSuiteStepper(string startTest, Sequence<Sequence<CompoundTerm>> testsuite, ModelProgram mp)
            :
            base(CreateModelProgram(startTest, testsuite, mp))
        {
            if (testsuite == null ||
                testsuite.IsEmpty ||
                testsuite.Exists(delegate(Sequence<CompoundTerm> testcase)
                    {
                        return (testcase == null || testcase.IsEmpty);
                    }))
                throw new ConformanceTesterException("Invalid test suite, the test suite must be a nonempty sequence of nonempty action sequences.");
            this.currentTest = testsuite.Head.AddFirst((CompoundTerm)Term.Parse(startTest + "(" + 0 + ")"));
            this.testsuite = testsuite.Tail;
            this.currentTestInProgress = false;
            this.testnr = 1;
            this.startTest = startTest;
        }

        CompoundTerm NextStartTestAction()
        {
            CompoundTerm a = (CompoundTerm)Term.Parse(this.startTest + "(" + this.testnr + ")");
            this.testnr += 1;
            return a;
        }

        static ModelProgram CreateModelProgram(string startTest, Sequence<Sequence<CompoundTerm>> testsuite, ModelProgram mp)
        {

            Set<Symbol> symbs = Set<Symbol>.EmptySet;
            foreach (Sequence<CompoundTerm> testcase in testsuite)
                foreach (CompoundTerm action in testcase)
                    symbs = symbs.Add(action.Symbol);
            FSM fa = FsmTraversals.GenerateTestSequenceAutomaton(startTest, testsuite, symbs);
            FsmModelProgram fsmmp = new FsmModelProgram(fa, "TestSuite");
            if (mp == null)
                return fsmmp;
            else
                return new ProductModelProgram(fsmmp, mp);
        }

        /// <summary>
        /// Update the current state to the target state of the action from the current state.
        /// If the action is the current action in the current test case, consume that action.
        /// </summary>
        /// <param name="action">given action</param>
        public override void DoAction(CompoundTerm action)
        {
            base.DoAction(action);
            if (!this.currentTest.IsEmpty && this.currentTest.Head.Equals(action))
            {
                this.currentTest = this.currentTest.Tail;
                this.currentTestInProgress = true;
            }
        }

        /// <summary>
        /// Select the next action in the current test case.
        /// The action symbol must be among the given set of action symbols.
        /// </summary>
        /// <param name="actionSymbols">given action symbols</param>
        /// <returns>selected action or null if no action could be selected</returns>
        public override CompoundTerm SelectAction(Set<Symbol> actionSymbols)
        {
            if (this.currentTest.IsEmpty ||
                !actionSymbols.Contains(this.currentTest.Head.Symbol))
                return null;
            return this.currentTest.Head;
        }

        /// <summary>
        /// Reset the model stepper to the initial state of the model.
        /// If the current test case is in progress, go to the next test case
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            if (this.currentTestInProgress)
            {
                this.currentTestInProgress = false;
                if (!this.testsuite.IsEmpty)
                {
                    this.currentTest = this.testsuite.Head.AddFirst(NextStartTestAction());
                    this.testsuite = this.testsuite.Tail;
                }
            }
        }

    }
}

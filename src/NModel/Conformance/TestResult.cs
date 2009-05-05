//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using Action = NModel.Terms.CompoundTerm;
using ActionSymbol = NModel.Terms.Symbol;

namespace NModel.Conformance
{
    /// <summary>
    /// Describes the result of a single test case
    /// </summary>
    public class TestResult : CompoundValue
    {
        /// <summary>
        /// Test case number.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        readonly public int testNr;
        /// <summary>
        /// The verdict of the test case. 
        /// If the verdict is Failure, the last action of 
        /// the test case violated conformance
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        readonly public Verdict verdict;

        /// <summary>
        /// Provides the reason for failure
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        readonly public string reason;

        /// <summary>
        /// The sequence of actions in the test case
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        readonly public Sequence<Action> trace;

        // Requirements metrics
        /// <summary>
        /// A Bag that contains all the executed requirements during this test-case run
        /// </summary>
        readonly public Bag<Pair<string, string>> executedRequirements = Bag<Pair<string, string>>.EmptyBag;

        /// <summary>
        /// Constructs a test case result
        /// </summary>
        /// <param name="testNr">test case number</param>
        /// <param name="verdict">verdict of the test case</param>
        /// <param name="trace">actions of the test case</param>
        /// <param name="reason">failure reason</param>
        /// <param name="executedRequirements">executed requirements</param>
        public TestResult(int testNr, Verdict verdict, string reason, Sequence<Action> trace, Bag<Pair<string, string>> executedRequirements) // Requirements metrics: ", Sequence<string> executedRequirements"
        {
            this.testNr = testNr;
            this.verdict = verdict;
            this.trace = trace;
            this.reason = reason;
            this.executedRequirements = executedRequirements;
        }
    }

    /// <summary>
    /// Verdict of a test case
    /// </summary>
    public enum Verdict
    {
        /// <summary>
        /// Successful test case
        /// </summary>
        Success,
        /// <summary>
        /// Failed test case
        /// </summary>
        Failure
    }
}

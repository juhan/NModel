using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using Action = NModel.Terms.CompoundTerm;


namespace NModel.Conformance
{
    partial class ConformanceTester
    {
        public bool showTestCaseCoveredRequirements;
        /// <summary>
        /// List of all the requirements that we want to check against 
        /// the executed requirements to get coverage metrics
        /// Pairs of: id,description
        /// </summary>
        public static List<KeyValuePair<string, string>> AllRequirements =
            new List<KeyValuePair<string, string>>();

        static Bag<Pair<string, string>> FailedActionsWithMessages = Bag<Pair<string, string>>.EmptyBag;

        static Bag<Pair<string, string>> totalExecutedRequirements = Bag<Pair<string, string>>.EmptyBag;

        static Map<string, TimeSpan> TotalExecutionTimePerAction = Map<string, TimeSpan>.EmptyMap;
        static Bag<string> ActionNumberOfExecutions = Bag<string>.EmptyBag;

        static int totalFailedTests = 0;
        static int totalExecutedTests = 0;

        public bool ShowTestCaseCoveredRequirements
        {
            set { showTestCaseCoveredRequirements = value; }
        }

        void AddMetricsToEndOfLog()
        {
            using (StreamWriter sw = GetStreamWriter())
            {
                LogMetrics lm = new LogMetrics(sw);
                writeTestsResultsSummary(lm);
                if(FailedActionsWithMessages.IsEmpty == false)
                    writeFailedActionsSummary(lm);
                if (AllRequirements.Count > 0)
                {
                    writeNotCoveredReqs(lm);
                    writeNotExecutedReqs(lm);
                }
                writeActionsTimeSpent(lm);
                lm.Write();
            }

        }

        private void writeTestsResultsSummary(LogMetrics lm)
        {
            lm.AddLine(TextType.HeaderOpenBlock, "General Summary");
            double passRate = (((double)NModel.Conformance.ConformanceTester.totalExecutedTests -
                (double)NModel.Conformance.ConformanceTester.totalFailedTests) /
               (double)NModel.Conformance.ConformanceTester.totalExecutedTests
               ) * 100.00;
            string percentage = passRate.ToString("#0.0");

            lm.AddLine(TextType.Indent1, NModel.Conformance.ConformanceTester.totalExecutedTests + " tests Executed");
            lm.AddLine(TextType.Indent1, NModel.Conformance.ConformanceTester.totalFailedTests + " tests Failed");
            lm.AddLine(TextType.Indent1, "Pass Rate: " + percentage + "%");
            lm.AddCloseBlock();
        }

        private void writeFailedActionsSummary(LogMetrics lm)
        {
            lm.AddLine(TextType.HeaderOpenBlock, "Failed Actions");
            foreach (Pair<string, string> action in FailedActionsWithMessages.Keys)
            {
                lm.AddLine(TextType.Indent1, "(" + action.First +
                    ", Reason: " + action.Second +
                    "( " + FailedActionsWithMessages.CountItem(action) +
                    (FailedActionsWithMessages.CountItem(action) > 1 ? " times ))" : " time ))"));
            }
            lm.AddCloseBlock();
        }

        private void writeNotCoveredReqs(LogMetrics lm)
        {
            Set<Pair<string, string>> NotCoveredReqs = Set<Pair<string, string>>.EmptySet;
            foreach (KeyValuePair<string, string> req in NModel.Conformance.ConformanceTester.AllRequirements)
                NotCoveredReqs = NotCoveredReqs.Add(new Pair<string, string>(req.Key, req.Value));

            foreach (Pair<string, string> req in NotCoveredReqs)
            {
                if (NModel.Attributes.RequirementAttribute.AllRequirementsInModels.Contains(req))
                    NotCoveredReqs = NotCoveredReqs.Remove(req);
            }

            double coverage = (
                (double)NModel.Attributes.RequirementAttribute.AllRequirementsInModels.Count /
                (double)NModel.Conformance.ConformanceTester.AllRequirements.Count
                ) * 100.00;
            string percentage = coverage.ToString("#0.0");

            lm.AddLine(TextType.Header, percentage + "% requirements coverage by test suite");
            lm.AddLine(TextType.HeaderOpenBlock, "Not covered requirements by test suite");
            if (!NotCoveredReqs.IsEmpty)
            {
                foreach (Pair<string, string> req in NotCoveredReqs)
                    lm.AddLine(TextType.Indent1, req.First + " - " + req.Second);
            }
            lm.AddCloseBlock();
        }

        private void writeNotExecutedReqs(LogMetrics lm)
        {
            Set<Pair<string, string>> NotExecutedReqs = Set<Pair<string, string>>.EmptySet;
            foreach (Pair<string, string> req in NModel.Attributes.RequirementAttribute.AllRequirementsInModels)
                NotExecutedReqs = NotExecutedReqs.Add(req);

            foreach (Pair<string, string> req in NotExecutedReqs)
            {
                if (totalExecutedRequirements.Keys.Contains(req))
                    NotExecutedReqs = NotExecutedReqs.Remove(req);
            }

            double coverage = (
                (double)totalExecutedRequirements.Keys.Count /
                (double)NModel.Attributes.RequirementAttribute.AllRequirementsInModels.Count
                ) * 100.00;
            string percentage = coverage.ToString("#0.0");

            lm.AddLine(TextType.Header, percentage + "% of the covered requirements have been executed");
            lm.AddLine(TextType.HeaderOpenBlock, "Covered requirements that have not been executed");
            if (!NotExecutedReqs.IsEmpty)
            {
                foreach (Pair<string, string> req in NotExecutedReqs)
                    lm.AddLine(TextType.Indent1, req.First + " - " + req.Second);
            }
            lm.AddCloseBlock();
        }

        private void writeActionsTimeSpent(LogMetrics lm)
        {
            if (!ConformanceTester.TotalExecutionTimePerAction.IsEmpty)
            {
                lm.AddLine(TextType.HeaderOpenBlock, "Toatal time spent in each action");
                foreach (string action in ConformanceTester.TotalExecutionTimePerAction.Keys)
                {
                    TimeSpan average = new TimeSpan(0, 0, 0, 0, ((int)ConformanceTester.TotalExecutionTimePerAction[action].TotalMilliseconds / ConformanceTester.ActionNumberOfExecutions.CountItem(action)));
                    lm.AddLine(TextType.Indent1, action +
                        "(Executed " + ConformanceTester.ActionNumberOfExecutions.CountItem(action) +
                        (ConformanceTester.ActionNumberOfExecutions.CountItem(action) > 1 ? " times" : " time") +
                        ", Average execution time: " +
                        average.ToString() +
                        ") : " +
                        ConformanceTester.TotalExecutionTimePerAction[action]);
                }
            }
            lm.AddCloseBlock();
        }


        private void AddFailedActionsWithMessages(TestResult testResult)
        {
            if (testResult.verdict == Verdict.Failure)
                FailedActionsWithMessages = FailedActionsWithMessages.Add(
                    new Pair<string, string>(
                    testResult.trace.Last.Name,
                    testResult.reason));
        }

        /// <summary>
        /// The list of executed requierments by each test case
        /// </summary>
        /// <param name="testResult"></param>
        /// <param name="sw"></param>
        private void AddExecutedRequirementsToTest(TestResult testResult, StreamWriter sw)
        {
            WriteLine(sw, "    Executed Requirements (");
            int c = 0;
            int count;
            foreach (Pair<string, string> req in testResult.executedRequirements.Keys)
            {
                count = testResult.executedRequirements.CountItem(req);
                WriteLine(sw, "        " + req.First + " - " + req.Second + " (" +
                    count + (count > 1 ? " times" : " time") + ")" +
                                  (c++ < testResult.executedRequirements.Keys.Count - 1 ? "," : ""));
            }
            WriteLine(sw, "    ),");
        }

        private void CalcPerformance(Action testerAction, DateTime startAction)
        {
            DateTime endAction = DateTime.Now;
            TimeSpan duration = endAction - startAction;
            if (TotalExecutionTimePerAction.ContainsKey(testerAction.Name))
            {
                duration = duration + TotalExecutionTimePerAction[testerAction.Name];
                TotalExecutionTimePerAction = TotalExecutionTimePerAction.Override
                    (testerAction.Name, duration);
            }
            else
                TotalExecutionTimePerAction = TotalExecutionTimePerAction.Add
                    (testerAction.Name, duration);
            ActionNumberOfExecutions = ActionNumberOfExecutions.Add(testerAction.Name);
        }
    }
}

using System;
using NModel;
using NModel.Attributes;
using NModel.Execution;

namespace RevisionControl
{
    static class Factory
    {
        public static ModelProgram CreateScenario1()
        {
            return new LibraryModelProgram(typeof(Factory).Assembly, "RevisionControl", new Set<string>("Scenario1"));
        }

        public static ModelProgram CreateContract()
        {
            return new LibraryModelProgram(typeof(Factory).Assembly, "RevisionControl", new Set<string>());
        }

        public static ModelProgram CreateFSM1()
        {
            FSM f = FSM.Create("T(0, Checkout(\"bob\"), 1)",
                "T(1, Checkout(\"alice\"), 2)",
                "T(2, Edit(), 2)",
                "T(2, Revert(), 2)",
                "T(2, Commit(\"bob\"), 101)",
                   "T(101, Commit(\"alice\"), 102)").Accept("102");
            return new FsmModelProgram(f, "FSM1");
        }

        public static ModelProgram CreateFSM2()
        {
            FSM f = FSM.Create(
                "T(1, Checkout(\"alice\"), 2)",
                "T(2, Edit(\"alice\", _, Op(\"Add\")), 3)",
                "T(3, Commit(\"alice\"), 4)",
                "T(4, CommitComplete(\"alice\"), 5)",
                "T(5, Checkout(\"bob\"), 99)",
                "T(99, Edit(\"bob\", _, Op(\"Change\")), 6)", 
                "T(6, Edit(\"alice\", _, Op(\"Change\")), 8)",
                "T(8, Commit(\"alice\"), 9)",
                "T(9, CommitComplete(\"alice\"), 11)",
                "T(11, Edit(\"bob\", _, Op(\"Change\")), 11)", 
                "T(11, Revert(), 11)",
                "T(11, Commit(\"bob\"), 11)",
                "T(11, CommitComplete(\"bob\"), 13)").Accept("13");
            return new FsmModelProgram(f, "FSM1");
        }

    }
}

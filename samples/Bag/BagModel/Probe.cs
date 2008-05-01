using System;
using System.Collections.Generic;
using System.Text;
using NModel;
using NModel.Attributes;
using NModel.Execution;

namespace BagModel
{
    [Feature]
    static class Probe
    {
        static Set<Set<string>> E()
        { return new Set<Set<string>>(Contract.content.Keys); }
        [Action]
        static void CheckView([Domain("E")]Set<string> elems) { }
        static bool CheckViewEnabled([Domain("E")]Set<string> elems)
        { return elems.Equals(Contract.content.Keys); }
    }
}

namespace ProbeUsage
{
    public static class Factory
    {
        public static ModelProgram Create()
        {
            FSM fsm = FSM.Create("t(0,Add(),1)", "t(0,Delete(),1)",
                               "t(1,CheckView(),0)").Accept("0");
            fsm = fsm.Expand("Lookup_Start", "Count_Start");
            return new FsmModelProgram(fsm, "ProbeModel");
        }
    }
}
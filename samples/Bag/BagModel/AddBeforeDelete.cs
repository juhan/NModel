using System;
using System.Collections.Generic;
using System.Text;
using NModel;
using NModel.Attributes;
using NModel.Execution;

namespace AddBeforeDelete
{
    public class Factory
    {
        public static ModelProgram Create()
        {
            FSM fsm = FSM.Create("t(0,Add(),0)", "t(0,Delete(),1)",
                                 "t(1,Delete(),1)").Accept("0", "1");
            return new FsmModelProgram(fsm, "AddBeforeDelete");
        }
    }
}


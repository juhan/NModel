using System.Collections.Generic;
using NModel;
using NModel.Execution;
using NModel.Attributes;

namespace SP
{
  public enum Phase { Inactive, Activating, Active }

  [Feature]
  public static class SetupModel
  {
    static Phase phase = Phase.Inactive;

    [Action("ReqSetup(_,_)")]
    static void ReqSetup() { phase = Phase.Activating; }
    static bool ReqSetupEnabled() { return phase == Phase.Inactive; }

    [Action("ResSetup(_,_,s)")]
    static void ResSetup(Status s)
    {
      if (s == Status.Cancelled)
        phase = Phase.Inactive;
      else
        phase = Phase.Active;
    }
    static bool ResSetupEnabled() { return phase == Phase.Activating; }

    [Action("ReqWork(_,_)")]
    [Action("ResWork(_,_,_)")]
    static void ReqOrRes() { }
    static bool ReqOrResEnabled() { return phase == Phase.Active; }

    [AcceptingStateCondition]
    static bool IsAcceptingState() { return phase == Phase.Active; }

    public static FsmModelProgram Make()
    {
      return new FsmModelProgram(FSM.Create(
        "t(Inactive(),ReqSetup(_,_),Activating())",
        "t(Activating(),ResSetup(_,_,Status(\"Cancelled\")),Inactive())",
        "t(Activating(),ResSetup(_,_,Status(\"Completed\")),Active())",
        "t(Active(),ReqWork(_,_),Active())",
        "t(Active(),ResWork(_,_,_),Active())"
        ).Accept("Active()"), "SetupModel");
    }
  }
}

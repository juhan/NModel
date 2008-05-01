using System.Collections.Generic;
using NModel;
using NModel.Execution;
using NModel.Attributes;

namespace SP
{

  public enum COMMAND { Setup, Work }
  [Feature]
  public class Commands
  {
    static Map<int, COMMAND> cmd = Map<int, COMMAND>.EmptyMap;

    [Action("ReqSetup(m,_)")]
    static void ReqSetup(int m) { cmd = cmd.Add(m, COMMAND.Setup); }
    static bool ReqSetupEnabled(int m) { return !cmd.ContainsKey(m); }

    [Action("ReqWork(m,_)")]
    static void ReqWork(int m) { cmd = cmd.Add(m, COMMAND.Work); }
    static bool ReqWorkEnabled(int m) { return !cmd.ContainsKey(m); }

    [Action("ResSetup(m,_,_)")]
    static void ResSetup(int m) { cmd = cmd.RemoveKey(m); }
    static bool ResSetupEnabled(int m) { return cmd.ContainsKey(m) && cmd[m] == COMMAND.Setup; }

    [Action("ResWork(m,_,_)")]
    static void ResWork(int m) { cmd = cmd.RemoveKey(m); }
    static bool ResWorkEnabled(int m) { return cmd.ContainsKey(m) && cmd[m] == COMMAND.Work; }

    [AcceptingStateCondition]
    static bool IsAcceptingState() { return cmd.IsEmpty; }
  }

}

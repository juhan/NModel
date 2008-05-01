using System.Collections.Generic;
using NModel;
using NModel.Execution;
using NModel.Attributes;

namespace SP
{
  public enum Mode { Sent, CancelRequested }

  public enum Status { Cancelled, Completed }

  [Feature]
  class Cancellation
  {
    internal static Map<int, Mode> mode = Map<int, Mode>.EmptyMap;

    [Action("ReqSetup(m,_)")]
    [Action("ReqWork(m,_)")]
    static void Req(int m) { mode = mode.Add(m, Mode.Sent); }
    static bool ReqEnabled(int m) { return !mode.ContainsKey(m); }

    [Action("Cancel(m)")]
    static void Cancel(int m)
    {
      if (mode.ContainsKey(m) && mode[m] == Mode.Sent)
        mode = mode.Override(m, Mode.CancelRequested);
    }

    static Set<int> TMP() { return new Set<int>(0); }
    [Action("ResSetup(m,_,s)")]
    [Action("ResWork(m,_,s)")]
    public static void Res(int m, Status s) { mode = mode.RemoveKey(m); }
    public static bool ResEnabled(int m, Status s)
    {
      return mode.ContainsKey(m) &&
             (s != Status.Cancelled || mode[m] == Mode.CancelRequested);
    }

    [AcceptingStateCondition]
    static bool IsAcceptingState() { return mode.IsEmpty; }

      public static ModelProgram Make()
      {
          return new LibraryModelProgram(typeof(Cancellation).Assembly, "SP",
              new Set<string>("Cancellation"));
      }
  }
}

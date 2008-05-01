using System.Collections.Generic;
using NModel;
using NModel.Execution;
using NModel.Attributes;

namespace SP
{
  [Feature]
  class Credits
  {
    internal static Set<int> window = new Set<int>(0);
    static int maxId = 0;
    internal static Map<int, int> requests = Map<int, int>.EmptyMap;

    [Action("ReqSetup(m,c)")]
    [Action("ReqWork(m,c)")]
    static void Req(int m, int c)
    {
      requests = requests.Add(m, c);
      window = window.Remove(m);
    }

    [Requirement("Section ...: Message IDs must not be repeated")]
    static bool ReqEnabled(int m) { return window.Contains(m); }

    [Requirement("Section ...: Requested credits must be > 0.")]
    static bool ReqEnabled(int m, int c) { return c > 0; }

    [Action("ResSetup(m,c,_)")]
    [Action("ResWork(m,c,_)")]
    public static void Res(int m, int c)
    {
      for (int i = 1; i <= c; i++)
        window = window.Add(maxId + i);
      requests = requests.RemoveKey(m);
      maxId = maxId + c;
    }

    [Requirement("Section ...: Must be a pending credit request")]
    static bool ResEnabled(int m) { return requests.ContainsKey(m); }

    [Requirement("Section ...: Must not grant more credits than requested")]
    static bool ResEnabled(int m, int c)
    {
      return requests[m] >= c;
    }

    [Requirement("Section ...: Client must have enough credits")]
    static bool ResEnabled2(int m, int c)
    {
      return requests.Count > 1 || window.Count > 0 || c > 0;
    }

    [StateInvariant]
    static bool ClientHasEnoughCredits()
    {
      if (requests.Count == 0) return window.Count > 0;
      else return true;
    }

    [AcceptingStateCondition]
    static bool IsAcceptingState() { return requests.IsEmpty; }

    static public ModelProgram Make()
    {
      return new LibraryModelProgram(typeof(Credits).Assembly, "SP",
          new Set<string>("Credits", "MessageParameters"));
    }
  }
}
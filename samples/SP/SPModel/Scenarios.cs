using System.Collections.Generic;
using NModel;
using NModel.Execution;
using NModel.Attributes;

namespace SP
{
  [Feature]
  public static class CreditScenario
  {
    static readonly Set<int> C = new Set<int>(2);
    static Set<int> M()
    {
      if (Credits.window.IsEmpty)
        return Set<int>.EmptySet;
      else
        return new Set<int>(Credits.window.Minimum());
    }
    static int nrOfSentRequests = 0;
    [Action("ReqSetup(m,c)")]
    [Action("ReqWork(m,c)")]
    static void Req([Domain("M")]int m, [Domain("C")]int c) { nrOfSentRequests += 1; }
    static bool ReqEnabled(int m) { return nrOfSentRequests < 3; }

    [Action("ResSetup(m,c,_)")]
    [Action("ResWork(m,c,_)")]
    static void Res(int m, int c) { }
    static bool ResEnabled(int m, int c)
    {
      return Credits.requests.ContainsKey(m) &&
             Credits.requests[m] == c;
    }

    public static ModelProgram Create()
    {
      return new LibraryModelProgram(typeof(SP.Credits).Assembly,
        "SP",new Set<string>("Credits","MessageParameters","CreditScenario"));
    }
  }

  [Feature]
  public static class CancelScenario
  {
    static Set<int> M()
    {
      if (Cancellation.mode.Keys.Count < 2)
        return Set<int>.EmptySet;
      else
        return new Set<int>(Cancellation.mode.Keys.Minimum());
    }

    [Action]
    static void Cancel([Domain("M")]int m) { }
    static bool CancelEnabled(int m)
    {
      return Cancellation.mode[m] == Mode.Sent;
    }

    public static ModelProgram Create()
    {
      return new LibraryModelProgram(typeof(SP.Credits).Assembly,
        "SP",new Set<string>("Cancellation","CancelScenario"));
    }
  }
}

//allow one cancel after a request
namespace CancelScenario2
{
    static class CancelScenario2
    {
        static Set<int> M = Set<int>.EmptySet;
        [Action]
        static void Cancel([Domain("M")]int m)
        {
            M = Set<int>.EmptySet;
        }
        [Action]
        static void ReqWork(int m)
        {
            M = new Set<int>(m);
        }
        [Action]
        static void ReqSetup(int m)
        {
            M = new Set<int>(m);
        }

        public static ModelProgram Create()
        {
            return new LibraryModelProgram(typeof(CancelScenario2).Assembly,
                "CancelScenario2");
        }
    }
}

using System.Collections.Generic;
using NModel;
using NModel.Execution;
using NModel.Attributes;

namespace SP
{
  [Feature]
  public static class ReqParams
  {
    readonly static Set<int> C = new Set<int>(3);
    static Set<int> M() { return Credits.window; }

    [Action("ReqSetup(m,c)")]
    [Action("ReqWork(m,c)")]
    static void Req([Domain("M")]int m, [Domain("C")]int c) { }
  }

  public static class OTFTest
  {
    static public ModelProgram Make()
    {
      return new LibraryModelProgram(typeof(Credits).Assembly, "SP",
        new Set<string>("Credits", "Commands", "SetupModel", "ReqParams"));
    }
  }
}
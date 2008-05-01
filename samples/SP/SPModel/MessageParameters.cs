using System.Collections.Generic;
using NModel;
using NModel.Execution;
using NModel.Attributes;

namespace SP
{
  [Feature]
  class MessageParameters
  {
    [Action("ResSetup(m,c,_)")]
    [Action("ResWork(m,c,_)")]
    static void Res([Domain("M")]int m, [Domain("C")]int c){}
    static Set<int> M() { return Credits.requests.Keys; }
    static Set<int> C() 
    {
      if (Credits.requests.Values.IsEmpty)
        return Set<int>.EmptySet;
      else{
        int maxCredits = Credits.requests.Values.Maximum();
        Set<int> res = Set<int>.EmptySet;
        for (int c = 0; c <= maxCredits; c++) res = res.Add(c);
        return res;
      }
    }

    [Action("ReqSetup(m,_)")]
    [Action("ReqWork(m,_)")]
    static void Req([Domain("ReqM")]int m){}
    static Set<int> ReqM() { return Credits.window; }

    //[Action]
    //static void Cancel([Domain("CancelM")]int m) { }
    //static Set<int> CancelM() { return Credits.requests.Keys; }
  }
}

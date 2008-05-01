using System;
using System.Collections.Generic;
using System.Text;
using NModel;
using NModel.Attributes;
using NModel.Execution;

namespace BagModel
{
    [Feature]
    static class ElementRestriction2
    {
        static Set<string> Elements() { return new Set<string>("a", "b"); }
        static Set<string> Elements2() { return new Set<string>(); }

        [Action]
        static void Add([Domain("Elements")]string element) { }
        static bool AddEnabled()
        {
            return Contract.content.Count < 2;
        }

        [Action]
        static void Count_Start() { }
        static bool Count_StartEnabled() { return false; }

        [Action]
        static void Delete([Domain("Elements")]string element) { }

        [Action]
        static void Lookup_Start([Domain("Elements2")]string element) { }

        [AcceptingStateCondition]
        static bool IsAcceptingState()
        {
            return Contract.content.Count == 0;
        }
    }
}

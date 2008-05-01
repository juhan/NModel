using System;
using System.Collections.Generic;
using System.Text;
using NModel;
using NModel.Attributes;
using NModel.Execution;

namespace BagModel
{
    [Feature]
    static class ElementRestriction
    {
        static Set<string> Elements() { return new Set<string>("", "b"); }

        [Action]
        static void Add([Domain("Elements")]string element) { }
        static bool AddEnabled()
        {
            return Contract.content.Count < 1;
        }

        [Action]
        static void Delete([Domain("Elements")]string element) { }

        [Action]
        static void Lookup_Start([Domain("Elements")]string element) { }

        [AcceptingStateCondition]
        static bool IsAcceptingState()
        {
            return Contract.content.Count == 0;
        }
    }
}

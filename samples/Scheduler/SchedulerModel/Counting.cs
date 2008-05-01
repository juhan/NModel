using System;
using System.Collections.Generic;
using System.Text;
using NModel;
using NModel.Attributes;
using NModel.Execution;
using NModel.Terms;

namespace Counting
{
    static class Contract
    {
        internal static Map<string, int> barCounter = Map<string, int>.EmptyMap;
        static Set<string> barNames { get { return barCounter.Keys; } }
        [Action]
        static void Execute([Domain("barNames")]string name)
        {
            if (barCounter[name] == 1)
                barCounter = barCounter.RemoveKey(name);
            else
                barCounter = barCounter.Override(name, barCounter[name] - 1);
        }
        static bool ExecuteEnabled(string name)
        {
            return barCounter.ContainsKey(name);
        }
    }

    static class Initializer
    {
        [Action]
        static void Init()
        {
            Contract.barCounter = Contract.barCounter.Add("read", 2);
            Contract.barCounter = Contract.barCounter.Add("filter", 1);
            Contract.barCounter = Contract.barCounter.Add("output", 1);
        }
        static bool InitEnabled()
        {
            return Contract.barCounter.IsEmpty;
        }
    }

    public static class Factory
    {
        public static ModelProgram Create()
        {
            return LibraryModelProgram.Create(typeof(Factory));
        }
    }
}

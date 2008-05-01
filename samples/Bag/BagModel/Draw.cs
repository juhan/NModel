using System;
using System.Collections.Generic;
using System.Text;
using NModel;
using NModel.Attributes;
using NModel.Execution;

namespace BagModel
{
    [Feature]
    static class Draw
    {
        static Bag<string> B { get { return Contract.content; } }
        static bool choosing = false;

        [Action]
        static void Draw_Start() { choosing = true; }
        static bool Draw_StartEnabled() { return !choosing && !B.IsEmpty; }

        [Action]
        static void Draw_Finish([Domain("B")]string e)
        { choosing = false; Contract.content = B.Remove(e); }
        static bool Draw_FinishEnabled(string e)
        { return choosing && B.Contains(e); }

        [Action]
        static void Delete(string e) { }
        static bool DeleteEnabled() { return !choosing; }
        [Action]
        static void Add(string e) { }
        static bool AddEnabled() { return !choosing; }
        [Action]
        static int Lookup(string e) { return Contract.content.CountItem(e); }
        static bool LookupEnabled() { return !choosing; }
        [Action]
        static int Count() { return Contract.content.Count; }
        static bool CountEnabled() { return !choosing; }

        public static ModelProgram Make()
        {
            return new LibraryModelProgram(typeof(Contract).Assembly,
              "BagModel", new Set<string>("ElementRestriction2", "Draw"));
        }
    }
}

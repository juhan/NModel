using System;
using System.Collections.Generic;
using System.Text;
using NModel;
using NModel.Attributes;
using NModel.Execution;
using NModel.Terms;

namespace Triggering
{
    public class Trigger : CompoundValue
    {
        [ExcludeFromState] //BUG
        readonly public string name;
        [ExcludeFromState] //BUG
        readonly public Mode mode;
        [ExcludeFromState] //BUG
        readonly public int frequency;
        public Trigger(string name, Mode mode, int frequency)
        {
            this.name = name;
            this.mode = mode;
            this.frequency = frequency;
        }
        public Trigger(string name)
        {
            this.name = name;
            this.mode = Mode.Always;
            this.frequency = 0;
        }
    }
    public enum Mode { Always, Initially, Frequently }

    class Contract
    {
        static Map<string, Set<Trigger>> triggers = Map<string, Set<Trigger>>.EmptyMap;
        static Set<string> readyBars = Set<string>.EmptySet;
        static Map<string, int> occurrences = Map<string, int>.EmptyMap;
        [Action]
        static void Execute([Domain("readyBars")]string b)
        {
            readyBars = readyBars.Remove(b);
            foreach (Trigger t in triggers[b])
                if (t.mode == Mode.Always ||
                    (t.mode == Mode.Initially && occurrences[t.name] == 0) ||
                    (t.mode == Mode.Frequently &&
                     occurrences[b] == t.frequency * (occurrences[t.name] + 1)))
                    readyBars = readyBars.Add(t.name);
            occurrences = occurrences.Override(b, occurrences[b] + 1);
        }
        static bool ExecuteEnabled(string b) { return readyBars.Contains(b); }


        [Action]
        static void Init()
        {
            readyBars = new Set<string>("read");
            triggers = triggers.Add("read", new Set<Trigger>(new Trigger("read"), new Trigger("filter")));
            triggers = triggers.Add("filter", new Set<Trigger>(new Trigger("output", Mode.Frequently, 2)));
            triggers = triggers.Add("output", Set<Trigger>.EmptySet);
            occurrences = new Map<string, int>("read", 0, "filter", 0, "output", 0);
        }
        static bool InitEnabled() { return readyBars.IsEmpty; }
    }

    public static class Factory
    {
        public static ModelProgram Create()
        {
            return LibraryModelProgram.Create(typeof(Factory));
        }
    }
}

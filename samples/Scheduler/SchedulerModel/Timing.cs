using System;
using System.Collections.Generic;
using System.Text;
using NModel;
using NModel.Attributes;
using NModel.Execution;
using NModel.Terms;

namespace Timing
{
    class Contract
    {
        internal static Map<string, Set<string>> triggeredBars = Map<string, Set<string>>.EmptyMap;
        internal static Set<string> readyBars = Set<string>.EmptySet;
        internal static double time = 0.0;
        internal static Map<string, double> offset = Map<string, double>.EmptyMap;
        internal static Map<string, double> duration = Map<string, double>.EmptyMap;
        internal static Map<string, double> deadline = Map<string, double>.EmptyMap;
        internal static Map<string, double> slack = Map<string, double>.EmptyMap;


        [Action]
        static void Init()
        {
            slack = slack.Add("read", 1.0);
            slack = slack.Add("filter", 1);
            slack = slack.Add("output", 0.5);

            duration = duration.Add("read", 1);
            duration = duration.Add("filter", 2);
            duration = duration.Add("output", 0.5);

            offset = offset.Add("read", 5);
            offset = offset.Add("filter", 6);
            offset = offset.Add("output", 3);

            deadline = deadline.Add("read",0);
            deadline = deadline.Add("filter",0);
            deadline = deadline.Add("output",0);

            triggeredBars = triggeredBars.Add("read", new Set<string>("read", "filter"));
            triggeredBars = triggeredBars.Add("filter", new Set<string>("output"));
            triggeredBars = triggeredBars.Add("output", new Set<string>());
           
            readyBars = new Set<string> ("read");
        }
        static bool InitEnabled() { return readyBars.IsEmpty; }

        [Action]
        static void Execute([Domain("readyBars")]string b)
        {
            time = time + duration[b];
            readyBars =  readyBars.Remove(b).Union(triggeredBars[b]);
            foreach (string a in readyBars) deadline = deadline.Override(a, time);
        }
        static bool ExecuteEnabled(string b)
        {
            double latest = deadline[b]+offset[b]-duration[b];
            double earliest = latest - slack[b]; 
            return (earliest <= time && time < latest);
        }
        [Action]
        static void Idle()
        {
            double ff = (readyBars.IsEmpty ? time : double.MaxValue); 
            foreach (string b in readyBars)
                ff = Math.Min(ff, deadline[b] + offset[b] - duration[b] - slack[b]);
            time = ff; //fast-forward to time ff
        }
        static bool IdleEnabled()
        {
            if (readyBars.IsEmpty) return false;
            foreach (string b in readyBars)
                if (time >= deadline[b] + offset[b] - duration[b] - slack[b]) return false;
            return true;
        }
    }
    
    public static class Factory
    {
        public static ModelProgram Create()
        {
            return LibraryModelProgram.Create(typeof(Contract));
        }
    }
}
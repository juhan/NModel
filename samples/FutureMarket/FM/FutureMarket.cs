using System;
using System.Collections.Generic;
using NModel;
using NModel.Attributes;
using NModel.Execution;
//using NModel.Algorithms;

namespace FM
{
    static class Investor
    {
        internal static Boolean i ; // i=false no reservation and i=true made reservation
        [Action]
        static void Invest(bool c)
        {
            i = c;
        }
        static bool InvestEnabled()
        {
            return (!i || (i & Barred.b));
        }
        [Action]
        static void Done()
        {
            i = true;
        }
        static bool DoneEnabled()
        {
            return !Barred.b & i;    
        }
    }
    [Feature]
    static class Month
    {
        internal static bool m;
        [Action("Invest(_)")]
        static void Invest()
        {
            if (!m)
                m = true;         
        }
        static bool InvestEnabled()
        {
            return (!m);
        }
        [Action("Month1(_,_)")]
        static void Month1()
        {
            if (m)
                m = false;         
        }
        static bool Month1Enabled()
        {
            return (!m);
        }
        [Action]
        static void Done()
        {
            if (!m)
                m = false;         
        }
        static bool DoneEnabled()
        {
            return (!m);
        }
    }
    [Feature]
    static class Barred
    {
        internal static bool b;
        [Action]
        static void Invest(bool c)
        {
            if (!b)
                b = c;
            else
                b = false;
        }     

    }
    [Feature]
    static class Value
    {
        internal static int v;
        [Action("Month1(_,_)")]
        static void Month1()
        {
            if (Prob.prob <1)
                v = Math.Min(v+1,Cap.cap);
            else
                v = Math.Min(Math.Max(v-1,0),Cap.cap);
        }
    }
    [Feature]
    static class Prob
    {
        internal static int prob = 5;
        [Action]
        static void Month1(bool x, bool y)
        {
            if (Value.v < 5) 
                if (x || y)
                    prob = Math.Min(prob+1,10);
                else
                    prob = Math.Max(prob-1,0);
            else if (Value.v == 5)
                if (x) 
                    prob = Math.Min(prob+1,10); 
                else
                    prob = Math.Max(prob-1,0);
            else
                if (x & y)
                    prob = Math.Min(prob+1,10);
                else
                    prob = Math.Max(prob-1,0);
        }
    }
    [Feature]
    static class Cap
    {
        internal static int cap = 10;
        [Action("Month1(c,_)")]
        static void Month1(bool c)
        {
            if (c)
                cap = Math.Max(cap-1,0);            
        }        
    }
    public static class Factory
    {
        public static ModelProgram CreateScenario()
        {
            return new LibraryModelProgram(typeof(Investor).Assembly,
                "FM", new Set<string>("Month","Barred","Value","Prob","Cap"));
        }        
    }

}

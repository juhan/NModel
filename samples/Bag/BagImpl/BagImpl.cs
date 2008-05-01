using System;
using System.Collections.Generic;
using NModel.Conformance;
using NModel.Terms;
using NModel;
using Action = NModel.Terms.CompoundTerm;

namespace BagImpl
{
    public class BagImpl
    {
        internal Dictionary<string, int> table = new Dictionary<string, int>();
        int count = 0;

        public int Lookup(string element)
        {
            int c = 0;
            table.TryGetValue(element, out c);
            return c;
        }

        public void Add(string element)
        {
            if (table.ContainsKey(element)) 
                table[element] += 1;
            else
                table[element] = 1;
            count += 1;
        }

        public void Delete(string element)
        {
            if (table.ContainsKey(element))
            {
                table[element] -= 1;
                count -= 1;
                //if (table[element] == 0)
                //    table.Remove(element);
            }
        }

        public int Count
        {
            get
            {
                return count;
            }
        }
    }

    public class Stepper : IStepper
    {

        BagImpl bag = new BagImpl();

        public Action DoAction(Action action)
        {
            switch (action.Name)
            {
                case ("CheckView"):
                    Set<string> modelView = (Set<string>)action[0];
                    Set<string> implView = new Set<string>(bag.table.Keys);
                    if (!modelView.Equals(implView))
                        throw new Exception("Inconsistent views of state: model:" +
                                            modelView + " iut:" + implView);
                    return null;
                case ("Add"):
                    bag.Add((string)action[0]);
                    return null;
                case ("Delete"):
                    bag.Delete((string)action[0]);
                    return null;
                case ("Lookup_Start"):
                    return Action.Create("Lookup_Finish", bag.Lookup((string)action[0]));
                case ("Count_Start"):
                    return Action.Create("Count_Finish", bag.Count);
                default:
                    throw new Exception("Unexpected action " + action);
            }
        }

        public void Reset()
        {
            bag = new BagImpl();
        }

        public static Stepper Create()
        {
            return new Stepper();
        }
    }
}

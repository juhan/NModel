using System;
using System.Collections.Generic;
using System.Text;
using NModel;
using NModel.Attributes;
using NModel.Execution;

namespace BagModel
{
    static class Contract
    {
        internal static Bag<string> content = 
            Bag<string>.EmptyBag;

        [Action]
        static void Add(string element)
        {
            content = content.Add(element);
        }

        [Action]
        static void Delete(string element)
        {
            content = content.Remove(element);
        }

        [Action]
        static int Lookup(string element)
        {
            return content.CountItem(element);
        }

        [Action]
        static int Count()
        {
            return content.Count;
        }
    }
}



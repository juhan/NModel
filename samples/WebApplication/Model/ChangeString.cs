using System;
using NModel.Attributes;
using NModel.Terms;
using NModel.Execution;
using NModel;

namespace WebModel
{
    [Feature("ChangeString")]
    public class ChangeString
    {
        public static Set<string> strings = new Set<string>("aaa", "bb", "ccc", "dd", "e", "ffffff");

        [Action]
        public static void ModifyString_Start([Domain("strings")] string s)
        {
            //TODO! Hint: look into ChangeNumber feature.
        }
        [Action]
        public static void ModifyString_Finish()
        {

        }
    }
}
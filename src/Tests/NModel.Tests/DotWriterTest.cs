using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using NModel;
using NModel.Terms;
using NModel.Utilities.Graph;
using NUnit.Framework;

namespace NModel.Tests
{
    /// <summary>
    /// Summary description for DotWriterTest
    /// </summary>
    [TestFixture]
    public class DotWriterTest
    {
        const string FSMTRIANGLE = "FSM(0, AcceptingStates(21, 0, 33, 37, 31, 1, 19), Transitions(t(0, AssignColor(SideOfTriangle(\"S2\"), Color(\"RED\")), 1), t(19, AssignColor(SideOfTriangle(\"S1\"), Color(\"RED\")), 31), t(21, AssignColor(SideOfTriangle(\"S1\"), Color(\"MAGENTA\")), 33), t(0, AssignColor(SideOfTriangle(\"S3\"), Color(\"RED\")), 1), t(0, AssignColor(SideOfTriangle(\"S1\"), Color(\"GREEN\")), 1), t(19, AssignColor(SideOfTriangle(\"S1\"), Color(\"YELLOW\")), 31), t(0, AssignColor(SideOfTriangle(\"S1\"), Color(\"BLUE\")), 1), t(0, AssignColor(SideOfTriangle(\"S2\"), Color(\"YELLOW\")), 1), t(1, AssignColor(SideOfTriangle(\"S2\"), Color(\"GREEN\")), 21), t(1, AssignColor(SideOfTriangle(\"S1\"), Color(\"GREEN\")), 21), t(1, AssignColor(SideOfTriangle(\"S2\"), Color(\"YELLOW\")), 21), t(0, AssignColor(SideOfTriangle(\"S1\"), Color(\"CYAN\")), 1), t(0, AssignColor(SideOfTriangle(\"S1\"), Color(\"RED\")), 1), t(0, AssignColor(SideOfTriangle(\"S2\"), Color(\"MAGENTA\")), 1), t(0, AssignColor(SideOfTriangle(\"S2\"), Color(\"CYAN\")), 1), t(0, AssignColor(SideOfTriangle(\"S3\"), Color(\"MAGENTA\")), 1), t(19, AssignColor(SideOfTriangle(\"S1\"), Color(\"BLUE\")), 31), t(1, AssignColor(SideOfTriangle(\"S1\"), Color(\"MAGENTA\")), 21), t(0, AssignColor(SideOfTriangle(\"S2\"), Color(\"GREEN\")), 1), t(0, AssignColor(SideOfTriangle(\"S3\"), Color(\"GREEN\")), 1), t(19, AssignColor(SideOfTriangle(\"S1\"), Color(\"CYAN\")), 37), t(21, AssignColor(SideOfTriangle(\"S1\"), Color(\"YELLOW\")), 33), t(0, AssignColor(SideOfTriangle(\"S3\"), Color(\"BLUE\")), 1), t(1, AssignColor(SideOfTriangle(\"S2\"), Color(\"RED\")), 21), t(1, AssignColor(SideOfTriangle(\"S1\"), Color(\"RED\")), 21), t(1, AssignColor(SideOfTriangle(\"S2\"), Color(\"CYAN\")), 19), t(1, AssignColor(SideOfTriangle(\"S1\"), Color(\"YELLOW\")), 21), t(21, AssignColor(SideOfTriangle(\"S1\"), Color(\"RED\")), 33), t(1, AssignColor(SideOfTriangle(\"S1\"), Color(\"CYAN\")), 19), t(21, AssignColor(SideOfTriangle(\"S1\"), Color(\"BLUE\")), 31), t(1, AssignColor(SideOfTriangle(\"S1\"), Color(\"BLUE\")), 21), t(19, AssignColor(SideOfTriangle(\"S1\"), Color(\"GREEN\")), 31), t(1, AssignColor(SideOfTriangle(\"S2\"), Color(\"MAGENTA\")), 21), t(0, AssignColor(SideOfTriangle(\"S3\"), Color(\"YELLOW\")), 1), t(21, AssignColor(SideOfTriangle(\"S1\"), Color(\"GREEN\")), 33), t(0, AssignColor(SideOfTriangle(\"S1\"), Color(\"YELLOW\")), 1), t(0, AssignColor(SideOfTriangle(\"S1\"), Color(\"MAGENTA\")), 1), t(19, AssignColor(SideOfTriangle(\"S1\"), Color(\"MAGENTA\")), 31), t(1, AssignColor(SideOfTriangle(\"S2\"), Color(\"BLUE\")), 21), t(0, AssignColor(SideOfTriangle(\"S2\"), Color(\"BLUE\")), 1), t(21, AssignColor(SideOfTriangle(\"S1\"), Color(\"CYAN\")), 31), t(0, AssignColor(SideOfTriangle(\"S3\"), Color(\"CYAN\")), 1)), Vocabulary(\"AssignColor\"))";

        /// <summary>
        /// A test that parses the FSM given in FSMTRIANGLE and counts the number of 
        /// transitions in the resulting Dot.
        /// </summary>
        [Test]
        public void FSM2Dot1()
        {
            FSM fa=FSM.FromTerm(CompoundTerm.Parse(FSMTRIANGLE));
            GraphParams gp = new GraphParams("Test1",fa);
            StringBuilder sb = DotWriter.ToDot(gp);
            string s=sb.ToString();
            Regex r=new Regex("\\[ label = \"AssignColor\" \\];",RegexOptions.Multiline);
            int count=0;
            count = r.Matches(s).Count;
            Assert.AreEqual(42, count);        
        }
    }
}

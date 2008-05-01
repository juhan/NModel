using System.Collections.Generic;
using NModel;
using NModel.Attributes;
using NModel.Execution;
//using NModel.Algorithms;

namespace FMP
{
    
    class Pr
    {
        readonly static Set<double> P = new Set<double>(0.5,0.75,0.25);
        [Action("Month1(x,_,p)")]
        static void Month1(bool x,[Domain("P")]double p)
        {
                            
        }
        static bool Month1Enabled(bool x, double p)
        {
            return (x && p == 0.25) || (!x && p == 0.75); 
        }
        public static ModelProgram Create()
        {
            return new LibraryModelProgram(typeof(Pr).Assembly, "FMP");
        }
    }

}

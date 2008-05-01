using System;
using System.Collections.Generic;
using System.Text;
using NModel;
using NModel.Attributes;
using NModel.Execution;

namespace BagModel
{
    public static class Factory
    {

        public static ModelProgram CreateContract()
        {
            return new LibraryModelProgram(typeof(Contract).Assembly,
                "BagModel",Set<string>.EmptySet);
        }

        public static ModelProgram CreateScenario()
        {
            return new LibraryModelProgram(typeof(Contract).Assembly,
                "BagModel", new Set<string>("ElementRestriction"));
        }

        public static ModelProgram CreateScenario2()
        {
            return new LibraryModelProgram(typeof(Contract).Assembly,
                "BagModel", new Set<string>("ElementRestriction2"));
        }
    }
}

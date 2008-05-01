using System;
using System.Collections.Generic;
using System.Text;
using NModel;
using NModel.Attributes;
using NModel.Execution;

namespace BagWithProbe
{
    public static class Factory
    {
        public static ModelProgram Create()
        {
            return new LibraryModelProgram(typeof(BagModel.Contract).Assembly,
                "BagModel", new Set<string>("Probe"));
        }
    }
}

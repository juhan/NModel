using System;
using NModel.Attributes;
using NModel.Terms;
using NModel.Execution;
using NModel;

namespace WebModel
{


    public partial class Contract
    {
        #region Contructor functions that compose different features of the model

        public static ModelProgram CreateLoginModel()
        {
            return new
                LibraryModelProgram(typeof(Contract).Assembly, "WebModel",
                new Set<string>("Login"));
        }

        public static ModelProgram CreateIntModel()
        {
            return new
                LibraryModelProgram(typeof(Contract).Assembly, "WebModel",
                new Set<string>("Login", "ChangeInt"));
        }

        public static ModelProgram CreateFullModel()
        {
            return new
                LibraryModelProgram(typeof(Contract).Assembly, "WebModel",
                new Set<string>("Login", "ChangeInt", "ChangeString"));
        }

        #endregion
    }
}
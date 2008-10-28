using System;
using NModel.Attributes;
using NModel.Terms;
using NModel.Execution;
using NModel;

namespace WebModel
{
    public enum ControlMode { Initializing, Running }

    [Abstract]
    public enum User { VinniPuhh, OleBrumm }

    public partial class Contract
    {

        public static ControlMode state = ControlMode.Initializing;

        #region initialisation
        [Action]
        public static void Initialize()
        {
            state = ControlMode.Running;
        }
        public static bool InitializeEnabled() { return state == ControlMode.Initializing; }

        #endregion



        /// <summary>
        /// The set of users who are currently logged in.
        /// </summary>
        public static Set<User> usersLoggedIn = Set<User>.EmptySet;

    }
}

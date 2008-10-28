using NModel.Attributes;
using NModel.Terms;
using NModel.Execution;
using NModel;

namespace WebModel
{
    public enum Password { Correct, Incorrect }
    public enum LoginStatus { Success, Failure }


    [Feature("Login")]
    public static class Login
    {
        #region Logging in

        public static Map<User, LoginStatus> activeLoginRequests = Map<User, LoginStatus>.EmptyMap;

        [Requirement("Send username and password to the server to log in.")]
        [Action]
        public static void Login_Start(User user, Password password)
        {
            if (password == Password.Correct)
                activeLoginRequests = activeLoginRequests.Add(user, LoginStatus.Success);
            else
                activeLoginRequests = activeLoginRequests.Add(user, LoginStatus.Failure);

        }

        public static bool Login_StartEnabled()
        { return Contract.state == ControlMode.Running; }

        public static bool Login_StartEnabled(User user)
        { return !activeLoginRequests.ContainsKey(user) && !Contract.usersLoggedIn.Contains(user); }


        [Requirement("Web server returns login response to browser.")]
        [Action]
        public static void Login_Finish(User user, LoginStatus status)
        {
            if (status == LoginStatus.Success)
            {
                Contract.usersLoggedIn = Contract.usersLoggedIn.Add(user);
            }
            else // if status == LoginStatus.Failure
                if (Contract.usersLoggedIn.Contains(user))
                    Contract.usersLoggedIn = Contract.usersLoggedIn.Remove(user);
            activeLoginRequests = activeLoginRequests.RemoveKey(user);
        }
        public static bool Login_FinishEnabled()
        {
            return Contract.state == ControlMode.Running;
        }


        [Requirement("")]
        public static bool Login_FinishEnabled(User user, LoginStatus status)
        {
            return activeLoginRequests.Keys.Contains(user) && activeLoginRequests[user].Equals(status);
        }

        [Requirement("It should be possible to log out from any page")]
        [Action]
        public static void Logout(User user)
        {
            Contract.usersLoggedIn = Contract.usersLoggedIn.Remove(user);
        }
        public static bool LogoutEnabled()
        {
            return Contract.state == ControlMode.Running;
        }

        public static bool LogoutEnabled(User user)
        {
            return Contract.usersLoggedIn.Contains(user);
        }

        #endregion
    }
}
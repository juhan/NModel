using System;
using NModel.Attributes;
using NModel.Terms;
using NModel.Execution;
using NModel;

namespace WebModel
{
    [Feature("ChangeInt")]
    public class ChangeInt
    {
        public static Map<User,int> userToInt = Map<User,int>.EmptyMap;

        public static Set<int> numbers = new Set<int>(0, 1, 2);

        [Action]
        public static void Initialize()
        {
            foreach (User u in Enum.GetValues(typeof(User)))
            {
                userToInt = userToInt.Add(u, 0);
            }
        }


        [Action]
        public static void UpdateInt(User user, [Domain("numbers")] int number)
        {
            userToInt = userToInt.Override(user,number);
        }
        public static bool UpdateIntEnabled()
        { return Contract.state > ControlMode.Initializing; }
        public static bool UpdateIntEnabled(User user)
        { return Contract.usersLoggedIn.Contains(user); }

        [Action]
        public static void ReadInt(User user,[Domain("numbers")] int number)
        {
            // Reading a number from the page should not change the state
            // thus the body is empty.
        }
        public static bool ReadIntEnabled()
        { return Contract.state > ControlMode.Initializing; }
        public static bool ReadIntEnabled(User user)
        { return Contract.usersLoggedIn.Contains(user); }
        public static bool ReadIntEnabled(User user, int number)
        { return userToInt[user] == number; }
    }
}

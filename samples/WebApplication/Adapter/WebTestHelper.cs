using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Adapter
{
    /// <summary>
    /// This class contains application specific functions to
    /// build requests, decide which page is displayed by the
    /// server, and to get some particular information from the page.
    /// </summary>
    public static class WebTestHelper
    {
        public static string createLoginParams(string userName, string userPass)
        {
            return "username=" + userName + "&" +
                   "password=" + userPass;
        }

        /// <summary>
        /// This is an example of a function which decides upon which page is
        /// displayed based on some regular expression.
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public static bool isLoginPage(string page)
        {
            Match m = Regex.Match(page, "LoginPage");
            return m.Success;
        }

        /// <summary>
        /// This is an example of a function which decides upon which page is
        /// displayed based on some regular expression. In this case we just match
        /// if "Incorrect login name or password" is displayed on the page or not.
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public static bool loginFailed(string page)
        {
            Match m = Regex.Match(page, "Incorrect login name or password");
            return m.Success;
        }

        /// <summary>
        /// A helper function which checks if the given number is displayed on the page.
        /// If not, an exception is thrown with justification which is available in the
        /// current context.
        /// </summary>
        /// <param name="page"></param>
        /// <param name="number"></param>
        internal static void containsInt(string page, int number)
        {
            Match m = Regex.Match(page, "Number: ([0-9]+)");
            if (m.Success)
            {
                if (Int32.Parse(m.Groups[1].Value) != number)
                    throw new
                        Exception("Expected number " + number + ", but page contained number " + m.Groups[1] + ".");
            }
            else
                    throw new Exception("No number at all was found on page: " + page);
        }
    }
}

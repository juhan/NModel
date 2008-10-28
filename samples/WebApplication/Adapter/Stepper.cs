using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using NModel;
using NModel.Conformance;
using NModel.Terms;

using RealUser = System.String; //just for improving readability of the model
using ModelUser = System.String;

// We use enumerations from the webtest namespace
using WebModel;

namespace Adapter
{
    public class Stepper : IStepper
    {
        public Stepper()
        {
            initialize();
        }

        #region Site specific variables that should get their values from a site specific subclass

        public string currentURL;
        public string logOffURL;
        public string wrongPassword;

        public Dictionary<ModelUser, RealUser> modelUserToRealUser;
        public Dictionary<RealUser, string> realUserPassword;

        #endregion

        #region Some private variables for session and logging
        private Dictionary<RealUser,Session> sessionDictionary = new Dictionary<RealUser,Session>();
        #endregion


        public void initialize()
        {
            sessionDictionary = new Dictionary<RealUser,Session>();
        }


        string page = "";

        #region IStepper Members

        CompoundTerm IStepper.DoAction(CompoundTerm action)
        {
            Session session = null;
            Console.WriteLine(action.Name);
            switch (action.Name)
            {
                #region Initialization action.
                case "Initialize":
                    initialize();
                    return null;
                #endregion

                #region Login action
                case "Login_Start":
                    string userName = "";
                    modelUserToRealUser.TryGetValue((string)((CompoundTerm)action[0])[0],out userName); //"OleBrumm" -> "user"
                    string userPass = "";
                    switch ((string)((CompoundTerm)action[1])[0]) //Correct or Incorrect
                    {
                        case "Correct":
                            userPass = realUserPassword[userName];
                            break;
                        case "Incorrect":
                            userPass = wrongPassword;
                            break;
                        default:
                            throw new Exception("The password is neither correct nor incorrect!");
                    }
                    session = getUserSession(userName);
                    page = session.getQuery(currentURL, "");
                    string queryString = WebTestHelper.createLoginParams(userName, userPass);
                    page = session.postQuery(currentURL, currentURL, queryString);
                    LoginStatus status = LoginStatus.Success;
                    if (WebTestHelper.loginFailed(page))
                        status = LoginStatus.Failure;
                    return Action.Create("Login_Finish", action[0], status);

                case "Logout":
                    modelUserToRealUser.TryGetValue((string)((CompoundTerm)action[0])[0], out userName); //"OleBrumm" -> "user"
                    session = getUserSession(userName);
                    session.getQuery(logOffURL, "");
                    resetUserSession(userName);
                    return null;

                #endregion
                #region ChangeNumber
                case "UpdateInt":
                    modelUserToRealUser.TryGetValue((string)((CompoundTerm)action[0])[0], out userName); //"OleBrumm" -> "user"
                    session = getUserSession(userName);
                    session.getQuery(currentURL, "num=" + action[1].ToString());
                    return null;
                case "ReadInt":
                    modelUserToRealUser.TryGetValue((string)((CompoundTerm)action[0])[0], out userName); //"OleBrumm" -> "user"
                    session = getUserSession(userName);
                    page=session.getQuery(currentURL, "");
                    WebTestHelper.containsInt(page, (int)action[1]);
                    return null;


                #endregion



                default: throw new Exception("The action " + action.Name + " is undefined in the system adapter!");

            }
        }

        private Session getUserSession(string userName)
        {
            Session session=null;
            sessionDictionary.TryGetValue(userName, out session);
            if (session == null)
            {
                session = new Session();
                sessionDictionary.Add(userName, session);
            }
            return session;
        }

        private void resetUserSession(string userName)
        {
            sessionDictionary.Remove(userName);
            sessionDictionary.Add(userName, new Session());
        }

        void IStepper.Reset()
        {
            initialize();
        }

        #endregion


    }
}

using System.Collections.Generic;
using NModel.Conformance;
using WebModel;

namespace Adapter
{
    public class Site1 : Stepper
    {

        
        public Site1() {
            modelUserToRealUser = new Dictionary<string, string>();
            modelUserToRealUser.Add("OleBrumm", "user1");
            modelUserToRealUser.Add("VinniPuhh", "user2");
            modelUserToRealUser.Add("WinniePooh", "user3");


            realUserPassword = new Dictionary<string, string>();
            realUserPassword.Add("user1", "123");
            realUserPassword.Add("user2", "234");
            realUserPassword.Add("user3", "345");
        
            currentURL = "http://192.168.32.128/doStuff.php";
            logOffURL = "http://192.168.32.128/logout.php";
            wrongPassword = "valeparool";
        }

        public static IStepper Create()
        {
            Stepper s = new Site1();
            return s;
        }
    }
}

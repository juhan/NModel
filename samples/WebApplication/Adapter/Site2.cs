using System.Collections.Generic;
using NModel.Conformance;
using WebModel;

namespace Adapter
{
    public class Site2 : Stepper
    {


        public Site2()
        {
            modelUserToRealUser = new Dictionary<string, string>();
            modelUserToRealUser.Add("OleBrumm", "user1");
            modelUserToRealUser.Add("VinniPuhh", "user2");
            modelUserToRealUser.Add("WinniePooh", "user3");


            realUserPassword = new Dictionary<string, string>();
            realUserPassword.Add("user1", "123");
            realUserPassword.Add("user2", "234");
            realUserPassword.Add("user3", "345");

            currentURL = "http://www.cc.ioc.ee/~juhan/nmodel/webapplication/php/doStuff.php";
            logOffURL = "http://www.cc.ioc.ee/~juhan/nmodel/webapplication/php/logout.php";
            wrongPassword = "vale";
        }

        public static IStepper Create()
        {
            Stepper s = new Site2();
            return s;
        }
    }
}

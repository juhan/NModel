using System;
using NModel.Conformance;
using Action = NModel.Terms.CompoundTerm;

namespace SPImpl
{
    public class Stepper : IAsyncStepper
    {
        ObserverDelegate Respond;
        Server server = new Server();

        public void SetObserver(ObserverDelegate obs)
        {
            Respond = obs;
            server.ResponseEvent += new ResponseEventDelegate(Responder);
        }

        void Responder(string cmd, int id, int credits, Status status)
        {
            Action response = Action.Create("Res" + cmd, id, credits, status);
            Respond(response);
        }

        public Action DoAction(Action a)
        {
            switch (a.Name)
            {
                case ("ReqSetup"):
                    server.Request("Setup", (int)a[0], (int)a[1]);
                    return null;
                case ("ReqWork"):
                    server.Request("Work", (int)a[0], (int)a[1]);
                    return null;
                case ("Cancel"):
                    server.Request("Cancel",(int)a[0], 0);
                    return null;
                default: throw new Exception("Unknown action: " + a);
            }
        }

        public void Reset()
        { if (server.IsBusy) throw new Exception("Server is busy"); }

        public static IAsyncStepper Make() { return new Stepper(); }
    }

}

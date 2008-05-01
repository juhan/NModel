using System;
using System.Threading;
using System.Collections.Generic;

namespace SPImpl
{
    public enum Status { Cancelled, Completed }

    public delegate void ResponseEventDelegate(string cmd, int id, int credits, Status s);

    public class Server
    {
        Random rnd = new Random();
        public event ResponseEventDelegate ResponseEvent;

        object requestsLock = new object();
        Dictionary<int, WorkItem> requests = new Dictionary<int, WorkItem>();


        public bool IsBusy { get { lock (requestsLock) { return requests.Count > 0; } } }

        public void Request(string cmd, int id, int credits)
        {
            switch (cmd)
            {
                case "Setup":
                case "Work":
                    lock (requestsLock)
                    {
                        Thread req = new Thread(Req);
                        WorkItem w = new WorkItem(req, cmd, credits);
                        requests.Add(id, w);
                        req.Start(new IComparable[] { cmd, id, credits });
                    }
                    break;
                case "Cancel":
                    lock (requestsLock)
                    {
                        if (requests.ContainsKey(id))
                            new Thread(Cancel).Start(id);
                    }
                    break;
                default: throw new Exception("Unknown command: " + cmd);
            }
        }

        void Cancel(object data)
        {
            Thread.Sleep(rnd.Next(80)); //...cancellation may take time 
            int id = (int)data;
            lock (requestsLock)
            {
                if (requests.ContainsKey(id))
                {
                    WorkItem w = requests[id];
                    w.thread.Abort();
                    requests.Remove(id);
                    if (ResponseEvent != null)   //... notify that work has been cancelled
                        ResponseEvent(w.command, id,
                                      ProvideCredits(w.credits), Status.Cancelled);
                }
            }
        }

        //contains a bug: may return 0 credits when at least 1 credit must be returned
        int ProvideCredits(int requestedCredits)
        {
            return rnd.Next(requestedCredits);
        }

        void Req(object data)
        {
            try
            {
                IComparable[] args = (IComparable[])data;
                Thread.Sleep(rnd.Next(100)); //... actually doing the work ...
                if (ResponseEvent != null)   //... notify that work has been completed
                    ResponseEvent((string)args[0], (int)args[1],
                                  ProvideCredits((int)args[2]), Status.Completed);
                lock (requestsLock)
                {
                    if (requests.ContainsKey((int)args[1]))
                        requests.Remove((int)args[1]);
                }
            }
            catch (ThreadAbortException)
            {
            }
        }

        class WorkItem
        {
            public Thread thread;
            public string command;
            public int credits;

            public WorkItem(Thread thread, string command, int credits)
            {
                this.thread = thread;
                this.command = command;
                this.credits = credits;
            }
        }
    }
}


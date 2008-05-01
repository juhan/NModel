//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;
using System.Threading;

namespace NModel.Algorithms
{
    /// <summary>
    /// A singleton threadpool consisting of one worker thread
    /// and a manager thread that aborts the worker when a timeout elapses
    /// </summary>
    internal sealed class TimedWorker : IDisposable
    {
        private WorkItem work; 
        private bool workTimedOut;
        private Thread manager;
        private Thread worker;

        private AutoResetEvent workIsWaiting;
        private AutoResetEvent workerIsStarting;
        private AutoResetEvent workerIsDone;
        private AutoResetEvent workIsDone;

        private TimeSpan timeLimit = new TimeSpan(0,0,0,5,0); 

        public TimedWorker()
        {
            work = new WorkItem();
            workIsWaiting = new AutoResetEvent(false);
            workerIsStarting = new AutoResetEvent(false);
            workerIsDone = new AutoResetEvent(false);
            workIsDone = new AutoResetEvent(false);

            manager = new Thread(new ThreadStart(RunManager));
            worker = new Thread(new ThreadStart(RunWorker));
            manager.Start();
            worker.Start();
        }

        private void RunManager()
        {
            try
            {
                while (true)
                {
                    workerIsStarting.WaitOne(); //wait until the worker is starting
                    //wait until either the worker is done or the timeout happens
                    bool done = workerIsDone.WaitOne(timeLimit, false);
                    if (!done)
                    {
                        //abort the current worker and start a new worker
                        worker.Abort(); //abort the worker
                        worker = new Thread(new ThreadStart(RunWorker));
                        worker.Start(); //start the new worker
                        workTimedOut = true;
                    }
                    workerIsDone.Reset();
                    //indicate that the work is done
                    workIsDone.Set();
                }
            }
            catch (ObjectDisposedException)
            {
            }
            catch (ThreadInterruptedException)
            {
            }
        }

        private void RunWorker()
        {
            try
            {
                while (true)
                {
                    workIsWaiting.WaitOne();
                    WaitCallback callback;
                    object state;
                    ExecutionContext context;

                    callback = work.Callback;
                    state = work.State;
                    context = work.Context;
                    work.Callback = null;
                    work.State = null;
                    work.Context = null;

                    try
                    {
                        workerIsStarting.Set();
                        ExecutionContext.Run(context, new ContextCallback(callback), state);
                        workerIsDone.Set();
                    }
                    catch (ThreadAbortException)
                    {
                        return;
                    }
                }
            }
            catch (ThreadInterruptedException)
            {
            }
            catch (ObjectDisposedException)
            {
            }
        }

        public void Dispose()
        {
            if (worker != null)
                worker.Interrupt();
            if (manager != null)
                manager.Interrupt();
            if (workerIsDone != null)
                workerIsDone.Close();
            if (workIsDone != null)
                workIsDone.Close();
            if (workerIsStarting != null)
                workerIsStarting.Close();
            if (workIsWaiting != null)
                workIsWaiting.Close();
        }

        public bool StartWork(WaitCallback callback, object state, TimeSpan time)
        {
            this.workTimedOut = false;
            this.timeLimit = time;
            this.work.Callback = callback;
            this.work.State = state;
            this.work.Context = ExecutionContext.Capture();
            this.workIsWaiting.Set();
            this.workIsDone.WaitOne();
            return !this.workTimedOut;
        }

        private class WorkItem
        {
            public WaitCallback Callback;
            public object State;
            public ExecutionContext Context;
        }
    }
}

using System;
using System.Collections.Generic;
using NModel;
using NModel.Attributes;
using NModel.Execution;
using BarName = System.String;


//
// Unfinished model of the scheduler itself
//

namespace SchedulerModel
{
    public enum SchedulerState {
        INIT,
        WAITING,        
        EXECUTING,
        COMPLETED,
        EXPIRED
    }
    public enum ProcessState
    {
        INIT,
        MATCH,
        PREWAIT,
        DEFERRED,
        READY,
        RUN,
        REMATCH,
        REWAIT,
        COMPLETE
    }
    public class Bar : CompoundValue
    {
        [ExcludeFromState]
        public readonly string name;
        [ExcludeFromState]
        public readonly double duration; //in mili sec
        [ExcludeFromState]
        public readonly double offset; //ready time for next iteration
        [ExcludeFromState]
        public readonly Set<BarName> triggers; // name of the delagates it can trigger after it is done
        [ExcludeFromState]
        public readonly double earliestStartTime;
        [ExcludeFromState]
        public readonly double deadline; // latestStartTime = deadline - duration
        [ExcludeFromState]
        public readonly int count;
        public Bar(string name,double d, double offset, Set<BarName> cmds, double est, double lst, int count)
        {
            this.name = name;
            this.duration = d;
            this.offset = offset;
            this.triggers = cmds;
            this.earliestStartTime = est;
            this.deadline = lst;
            this.count = count;
        }        
    }
    static class Scheduler
    {
        [ExcludeFromState]
        internal static double inf = 1000000;
        internal static SchedulerState state = SchedulerState.INIT;
        internal static Map<BarName, Bar> schedule = Map<string,Bar>.EmptyMap;
        internal static Map<int, Set<Bar>> readyQueue = Map<int, Set<Bar>>.EmptyMap;
        internal static Set<Bar> bars = Set<Bar>.EmptySet;  
        [Action]
        static void Init ()
        {
            setInitSchedule();            
            readyQueue = readyQueue.Add(0,new Set<Bar>(schedule["start"]));
            bars = schedule.Values;
            //bars = readyQueue[0];
            state = SchedulerState.WAITING;
            
        }
        static bool InitEnabled()
        {
            return (state == SchedulerState.INIT);
        }
       
        static void setInitSchedule()
        {
            Set<BarName> cmd1 = new Set<BarName>();
            cmd1 = cmd1.Add("sample-microphone");
            cmd1 = cmd1.Add("read-commands");
            Bar bar1 = new Bar("start",1000.0, inf, cmd1, 0, inf, 1);
            schedule = schedule.Add("start", bar1);

            Set<BarName> cmd2 = new Set<BarName>();
            cmd2 = cmd2.Add("filter");
            Bar bar2 = new Bar("sample-microphone",1.0, 20.0, cmd2, 0, inf, (int)inf);
            schedule = schedule.Add("sample-microphone", bar2);

            Set<BarName> cmd3 = new Set<BarName>();
            cmd3 = cmd3.Add("fft-start");
            Bar bar3 = new Bar("filter",1.0, inf, cmd3, 0, inf, (int)inf);
            schedule = schedule.Add("filter", bar3);


            Set<BarName> cmd4 = new Set<BarName>();
            cmd4 = cmd4.Add("fft");
            Bar bar4 = new Bar("fft-start",10.0, inf, cmd4, 0, inf, (int)inf);
            schedule = schedule.Add("fft-start", bar4);

            Set<BarName> cmd5 = new Set<BarName>();
            cmd5 = cmd5.Add("fft");
            cmd5 = cmd5.Add("send-bytes");
            Bar bar5 = new Bar("fft",10.0, inf, cmd5, 0, inf, 10);
            schedule = schedule.Add("fft", bar5);

            Set<BarName> cmd6 = new Set<BarName>();
            Bar bar6 = new Bar("send-bytes",0.2, 1, cmd6, 0, inf, 4);
            schedule = schedule.Add("send-bytes", bar6);

            Set<BarName> cmd7 = new Set<BarName>();
            Bar bar7 = new Bar("read-commands",1, 10, cmd7, 0, inf, (int)inf);
            schedule = schedule.Add("read-command", bar7);
            //state = SchedulerState.WAITING;
        }
        /*
        static bool setInitScheduleEnabled()
        {
            return schedule.IsEmpty;
        }*/
        
        [Action]
        static void Match([Domain("bars")]Bar b)        
        {
            Set<Bar> readyBars;
            if (readyQueue.ContainsKey(TimeUpdate.time))
            {
                readyBars = readyQueue[TimeUpdate.time];
                bars = readyBars;
                if (readyBars.Contains(b))
                {                    
                    state = SchedulerState.WAITING;
                }
                else
                    state = SchedulerState.COMPLETED;
            }
            else
                state = SchedulerState.COMPLETED;
        }
        static bool MatchEnabled(
)
        {
            return (state == SchedulerState.WAITING);
        }

        static void UpdateReadyQueue(Bar b, int currentTime)
        {
            readyQueue = readyQueue.RemoveKey(currentTime);
            foreach (Bar b1 in schedule.Values)
            {
                if (b.triggers.Contains(b1.name))
                {
                    if (readyQueue.ContainsKey(currentTime + (int)b.duration)){
                        Set<Bar> bars = readyQueue[currentTime+(int)b.duration];
                        readyQueue = readyQueue.RemoveKey(currentTime+(int)b.duration);
                        readyQueue = readyQueue.Add(currentTime + (int)b.duration,bars.Add(b1));
                    }
                    else
                        readyQueue = readyQueue.Add(currentTime + (int)b.duration, new Set<Bar>(b1));
                }
            }
            if (b.offset < Scheduler.inf)
                if (readyQueue.ContainsKey(currentTime + (int)b.offset))
                {
                    Set<Bar> bars = readyQueue[currentTime + (int)b.offset];
                    readyQueue = readyQueue.RemoveKey(currentTime + (int)b.offset);
                    readyQueue = readyQueue.Add(currentTime + (int)b.offset, bars.Add(b));                    
                }
                else
                    readyQueue = readyQueue.Add(currentTime + (int)b.offset, new Set<Bar>(b));               
        }
        
        [Action]
        static void StartExecute([Domain("bars")]Bar b)
        {
            if (TimeUpdate.time >= b.earliestStartTime &&
                TimeUpdate.time < (b.deadline - b.duration))
            {
                state = SchedulerState.EXECUTING;
                UpdateReadyQueue(b, TimeUpdate.time);
                Process.state = ProcessState.DEFERRED;
            }
            else
            {
                TimeUpdate.tickEnabled = true;
            }
        }
        static bool StartExecuteEnabled()
        {
            return state == SchedulerState.WAITING && Process.state == ProcessState.PREWAIT;
        }
        [Action]
        static void Completed()
        {
            bars = readyQueue[TimeUpdate.time];
            state = SchedulerState.WAITING;
        }
        static bool CompletedEnabled()
        {
            return state == SchedulerState.COMPLETED;
        }
    }
    [Feature]
    static class TimeUpdate
    {
        internal static int time = 0;
        internal static bool tickEnabled = false;
        [Action]
        static void Tick(int i)
        {
            time = time + i;
            tickEnabled = false;
        }
        static bool TickEnabled()
        {
            return tickEnabled;
        }
    }
    [Feature]
    static class Process    
    {        
        internal static ProcessState state = ProcessState.INIT;
        internal static Map<BarName, int> frequncy = new Map<string, int>();
        internal static Set<Bar> currentBar = Set<Bar>.EmptySet;
        
        [Action]
        static void Init()
        {
            state = ProcessState.MATCH;            
        }
        static bool InitEnabled()
        {
            return state == ProcessState.INIT;
        }
        
        [Action]
        static void Match(Bar b)
        {
            Set<Bar> readyBars;
            if (Scheduler.readyQueue.ContainsKey(TimeUpdate.time))
            {
                readyBars = Scheduler.readyQueue[TimeUpdate.time];
                if (readyBars.Contains(b))
                {
                    currentBar = new Set<Bar> (b);                    
                    switch (state)
                    {                        
                        case ProcessState.MATCH:
                            state = ProcessState.PREWAIT;
                            break;
                        case ProcessState.REMATCH:
                            state = ProcessState.REWAIT;
                            break;
                    }
                }
                else
                    state = ProcessState.COMPLETE;
            }
            else state = ProcessState.COMPLETE;
        }
        static bool MatchEnabled()
        {
            return (state == ProcessState.MATCH) || (state == ProcessState.REMATCH);
        }
        
        [Action]
        static void StartExecute([Domain("currentBar")]Bar b)
        {
            
            state = ProcessState.DEFERRED;
           
        }
        static bool StartExecuteEnabled()
        {
            return (state == ProcessState.PREWAIT);
        }
         
        [Action]
        static void GetStack([Domain("currentBar")]Bar b)
        {
            if (Resources.stacks > 0)
                state = ProcessState.READY;
            else
                TimeUpdate.tickEnabled = true;
        }
        static bool GetStackEnabled()
        {
            return state == ProcessState.DEFERRED;
        }

        [Action]
        static void GetProcessor([Domain("currentBar")]Bar b)
        {
            if (Resources.cpus > 0)
                state = ProcessState.RUN;
            else
                TimeUpdate.tickEnabled = true;
        }
        static bool GetProcessorEnabled()
        {
            return state == ProcessState.READY;
        }
        [Action]
        static void Run([Domain("currentBar")]Bar b)
        {
            TimeUpdate.time = TimeUpdate.time + (int)b.duration;           
            Resources.cpus = Resources.cpus + 1;
            Resources.stacks = Resources.stacks + 1;
            state = ProcessState.COMPLETE;
            currentBar = currentBar.Remove(b);
        }
        static bool RunEnabled()
        {
            return state == ProcessState.RUN;
        }
        [Action]
        static void Completed()
        {
            state = ProcessState.MATCH;
        }
        static bool CompletedEnabled()
        {
            return state == ProcessState.COMPLETE;
        }
    }
    [Feature]
    public static class Resources
    {
        internal static int stacks = 1;
        internal static int cpus = 1;
        [Action("GetStack(_)")]
        static void GetStack()
        {
            if (stacks > 0)
                stacks = stacks -1;
        }
        [Action("GetProcessor(_)")]
        static void GetProcessor()
        {
            if (cpus > 0)
                cpus = cpus - 1;
        }
    }
    public static class Factory
    {
        /*
        public static ModelProgram Create()
        {
            return new LibraryModelProgram(typeof(Scheduler).Assembly,
                "SchedulerModel");
        }
        */
        public static ModelProgram CreateExtend()
        {
            return new LibraryModelProgram(typeof(Scheduler).Assembly,
                "SchedulerModel", new Set<string>("Process","TimeUpdate","Resources"));
        }
    }
}

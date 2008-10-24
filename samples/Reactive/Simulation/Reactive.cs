using System; // Console for progress messages, Math for Abs in CheckMessage
using NModel;
using NModel.Attributes;
using NModel.Execution;

namespace Reactive
{
    public static class Controller
    {
	// Types from implementation
	public enum ControlEvent { Timeout, Message, Command, Exit }
	public enum WaitFor { Timeout, Message }
	public enum Sensor { OK, Error }

	// Types added for model
	public enum Phase { WaitForEvent, HandleEvent }

	// Control state from implementation
	public static ControlEvent cevent = ControlEvent.Timeout;
	public static WaitFor waitfor = WaitFor.Timeout; // enable Reset
	public static Sensor sensor  = Sensor.Error;     // enable Reset

	// Control state added for model
	// Alternate event, handler
	public static Phase phase = Phase.WaitForEvent;
	// Model timer, sensor
	public static bool TimeoutScheduled = true;
	public static bool MessageRequested = false;

	// Data state from implementation
	public static string buffer  = OutOfRange;     
	public static double previous = Uninitialized;

	// Constants 
	const string InRange = "99.9";                // for buffer
	const string OutOfRange = "999.9";            // for buffer
	const double Uninitialized = double.MaxValue; // for previous

	// Actions and enabling conditions for Controller, from implementation

	public static bool ResetEnabled()
	{
	    return (cevent == ControlEvent.Timeout 
		    && waitfor == WaitFor.Timeout
		    && sensor == Sensor.Error 
		    && phase == Phase.HandleEvent);
	}

	[Action]
	public static void Reset()
	{
	    Console.WriteLine(" Reset");
	    ResetSensor(); // send reset command to sensor
	    StartTimer();  // wait for message from from sensor
	    waitfor = WaitFor.Message;
	    phase = Phase.WaitForEvent;
	}

	public static bool PollEnabled() 
	{
	    return (cevent == ControlEvent.Timeout 
		    && waitfor == WaitFor.Timeout 
		    && sensor == Sensor.OK 
		    && phase == Phase.HandleEvent);
	}
	
	[Action]
	public static void Poll()
	{
	    Console.WriteLine(" Poll");
	    PollSensor();   // send poll command to sensor
	    StartTimer();   // wait for message from sensor
	    waitfor = WaitFor.Message;
	    phase = Phase.WaitForEvent;
	}

	public static bool CalibrateEnabled()
	{
	    return (cevent == ControlEvent.Command 
		    && waitfor == WaitFor.Timeout
		    && sensor == Sensor.OK 
		    && phase == Phase.HandleEvent);
	}

	[Action]
	public static void Calibrate()
	{
	    Console.WriteLine(" Calibrate '" + buffer + "'");
	    double data = double.Parse(buffer);
	    // compute with data (not shown)
	    phase = Phase.WaitForEvent;
	    //    CalRequested = false;
	}
	public static bool CheckMessageEnabled()
	{
	    return (cevent == ControlEvent.Message 
		    && waitfor == WaitFor.Message
		    && phase == Phase.HandleEvent);
	}

	// Whoa... action may thow exception ... effecton exploration?
	[Action]
	public static void CheckMessage()
	{
	    double tol = 5.0;
	    Console.Write(" CheckMessage '" + buffer + "'");
	    try { 
		double data = double.Parse(buffer);
		if (previous == double.MaxValue) previous = data; // initialize
		Console.Write(", compare to " + previous);
		if (Math.Abs(data - previous) < tol) {
		    previous = data;
		    sensor = Sensor.OK;
		}
		else sensor = Sensor.Error; // retain old previous
		Console.WriteLine(", " + sensor);
	    }
	    catch { 
		sensor = Sensor.Error; 
		Console.WriteLine(", Error");
	    }
	    CancelTimer();  // cancel messageTimeout
	    StartTimer();   // wait for next time to poll
	    waitfor = WaitFor.Timeout;
	    phase = Phase.WaitForEvent;
	}

	public static bool ReportLostMessageEnabled() 
	{
	    return (cevent == ControlEvent.Timeout 
		    && waitfor == WaitFor.Message
		    && sensor == Sensor.OK 
		    && phase == Phase.HandleEvent);
	}

	[Action]
	public static void ReportLostMessage()
	{
	    Console.WriteLine(" ReportLostMessage");
	    // sensor = Sensor.Error;  NOT!  Doesn't change sensor
	    StartTimer();  // wait for next time to poll
	    waitfor = WaitFor.Timeout;
	    phase = Phase.WaitForEvent;
	}

	// NoHandler is enabled when no other handler is enabled
	public static bool NoHandlerEnabled()
	{
	    return (phase == Phase.HandleEvent 
		    && !ResetEnabled()
		    && !PollEnabled() 
		    && !CalibrateEnabled() 
		    && !CheckMessageEnabled() 
		    && !ReportLostMessageEnabled());
	}

	[Action]
	public static void NoHandler()
	{
	    phase = Phase.WaitForEvent;
	}

	// Actions and enabling conditions for environment, added for model

	// This one might be called DelayMessageEvent 
	// It leaves MessageRequested == true, so message will arrive later
	public static bool TimeoutEnabled()
	{
	    return (!MessageRequested 
		    && TimeoutScheduled 
		    && phase == Phase.WaitForEvent);
	}

	[Action]
	public static void Timeout() 
	{
	    Console.WriteLine("?Timeout, {0}, {1}", waitfor, sensor);
	    cevent = ControlEvent.Timeout;
	    TimeoutScheduled = false;
	    phase = Phase.HandleEvent;
	}

	public static bool TimeoutMsgLostEnabled()
	{
	    return (MessageRequested 
		    && TimeoutScheduled 
		    && phase == Phase.WaitForEvent);
	}

	[Action]
	public static void TimeoutMsgLost() 
	{
	    Console.WriteLine("?Timeout (message lost), {0}, {1}", waitfor, sensor);
	    cevent = ControlEvent.Timeout;
	    TimeoutScheduled = false;
	    phase = Phase.HandleEvent;
	    MessageRequested = false;
	}

	public static bool TimeoutMsgLateEnabled()
	{
	    return (MessageRequested 
		    && TimeoutScheduled 
		    && phase == Phase.WaitForEvent);
	}

	[Action]
	public static void TimeoutMsgLate() 
	{
	    Console.WriteLine("?Timeout (message late), {0}, {1}", waitfor, sensor);
	    cevent = ControlEvent.Timeout;
	    TimeoutScheduled = false;
	    phase = Phase.HandleEvent;
	    // MessageRequested remains true, message will arrive later
	}

	// A different action for each message value

	public static bool MessageEnabled()
	{
	    return (MessageRequested 
		    && phase == Phase.WaitForEvent);
	}

	[Action]
	public static void Message([Domain("Messages")] string message)
	{
	    Console.WriteLine("?Message '{0}', {1}, {2}", message, waitfor, sensor);
	    cevent = ControlEvent.Message;
	    buffer = message;
	    MessageRequested = false;
	    phase = Phase.HandleEvent;
	}

	static Set<string> Messages()
	{
	    return new Set<string>(InRange, OutOfRange);
	}

	public static bool CommandEnabled()
	{
	    return (phase == Phase.WaitForEvent);
	}

	[Action]
	public static void Command()
	{
	    Console.WriteLine("?Command, {0}, {1}", waitfor, sensor);
	    cevent = ControlEvent.Command;
	    phase = Phase.HandleEvent;
	    //    CalRequested = true; // for dead state checking
	}

	// Helpers for enabling events

	static void PollSensor() { MessageRequested = true; }   

	static void ResetSensor() { MessageRequested = true; }  

	static void StartTimer() { TimeoutScheduled = true; }

	static void CancelTimer() {  TimeoutScheduled = false; }

	// Helpers for analysis

	// UNSAFE state: calibrate is enabled, BUT buffer is out of range
	public static bool CalibrateOutOfRange() 
	{ 
	    return (CalibrateEnabled()
		    &&  buffer != InRange);
	}

	// Negation of unsafe state, above
	[StateInvariant]
	public static bool CalibrateInRange() 
	{ 
	    return (!CalibrateEnabled()
		    ||  buffer == InRange);
	}

	// Calibrate is enabled, both buffer and previous are in range
	[AcceptingStateCondition]
	public static bool SafeCalibrateEnabled()
	{
	    return (CalibrateEnabled() 
		    && buffer == InRange
		    && previous == double.Parse(InRange));
	}
    }

    public static class Factory
    {
        public static ModelProgram Create()
        {
            return new LibraryModelProgram(typeof(Factory).Assembly, "Reactive");
        }
    }
}

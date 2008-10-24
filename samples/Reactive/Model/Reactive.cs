using System;             // Math for Abs in CheckMessage
using NModel;
using NModel.Attributes;

namespace Reactive
{
    static class Controller
    {
	// Types from implementation
	enum ControlEvent { Timeout, Message, Command, Exit }
	enum WaitFor { Timeout, Message }
	enum Sensor { OK, Error }

	// Types added for model
	enum Phase { WaitForEvent, HandleEvent }

	// Control state from implementation
	static ControlEvent cevent = ControlEvent.Timeout;
	static WaitFor waitfor = WaitFor.Timeout; // enable Reset
	static Sensor sensor  = Sensor.Error;     // enable Reset

	// Control state added for model
	// Alternate event, handler
	static Phase phase = Phase.WaitForEvent;
	// Model timer, sensor
	static bool TimeoutScheduled = true;
	static bool MessageRequested = false;

	// Data state from implementation
	static string buffer  = OutOfRange;     
	static double previous = Uninitialized;

	// Constants 
	const string InRange = "99.9";                // for buffer
	const string OutOfRange = "999.9";            // for buffer
	const double Uninitialized = double.MaxValue; // for previous

	// Actions and enabling conditions for Controller, from implementation

	static bool ResetEnabled()
	{
	    return (cevent == ControlEvent.Timeout 
		    && waitfor == WaitFor.Timeout
		    && sensor == Sensor.Error 
		    && phase == Phase.HandleEvent);
	}

	[Action]
	static void Reset()
	{
	    ResetSensor(); // send reset command to sensor
	    StartTimer();  // wait for message from from sensor
	    waitfor = WaitFor.Message;
	    phase = Phase.WaitForEvent;
	}

	static bool PollEnabled() 
	{
	    return (cevent == ControlEvent.Timeout 
		    && waitfor == WaitFor.Timeout 
		    && sensor == Sensor.OK 
		    && phase == Phase.HandleEvent);
	}
	
	[Action]
	static void Poll()
	{
	    PollSensor();   // send poll command to sensor
	    StartTimer();   // wait for message from sensor
	    waitfor = WaitFor.Message;
	    phase = Phase.WaitForEvent;
	}

	static bool CalibrateEnabled()
	{
	    return (cevent == ControlEvent.Command 
		    && waitfor == WaitFor.Timeout
		    && sensor == Sensor.OK 
		    && phase == Phase.HandleEvent);
	}

	[Action]
	static void Calibrate()
	{
	    double data = double.Parse(buffer);
	    // compute with data (not shown)
	    phase = Phase.WaitForEvent;
	    //    CalRequested = false;
	}
	static bool CheckMessageEnabled()
	{
	    return (cevent == ControlEvent.Message 
		    && waitfor == WaitFor.Message
		    && phase == Phase.HandleEvent);
	}

	// Whoa... action may thow exception ... effecton exploration?
	[Action]
	static void CheckMessage()
	{
	    double tol = 5.0;
	    try { 
		double data = double.Parse(buffer);
		if (previous == double.MaxValue) previous = data; // initialize
		if (Math.Abs(data - previous) < tol) {
		    previous = data;
		    sensor = Sensor.OK;
		}
		else sensor = Sensor.Error; // retain old previous
	    }
	    catch { 
		sensor = Sensor.Error; 
	    }
	    CancelTimer();  // cancel messageTimeout
	    StartTimer();   // wait for next time to poll
	    waitfor = WaitFor.Timeout;
	    phase = Phase.WaitForEvent;
	}

	static bool ReportLostMessageEnabled() 
	{
	    return (cevent == ControlEvent.Timeout 
		    && waitfor == WaitFor.Message
		    && sensor == Sensor.OK 
		    && phase == Phase.HandleEvent);
	}

	[Action]
	static void ReportLostMessage()
	{
	    // sensor = Sensor.Error;  NOT!  Doesn't change sensor
	    StartTimer();  // wait for next time to poll
	    waitfor = WaitFor.Timeout;
	    phase = Phase.WaitForEvent;
	}

	// NoHandler is enabled when no other handler is enabled
	static bool NoHandlerEnabled()
	{
	    return (phase == Phase.HandleEvent 
		    && !ResetEnabled()
		    && !PollEnabled() 
		    && !CalibrateEnabled() 
		    && !CheckMessageEnabled() 
		    && !ReportLostMessageEnabled());
	}

	[Action]
	static void NoHandler()
	{
	    phase = Phase.WaitForEvent;
	}

	// Actions and enabling conditions for environment, added for model

	// This one might be called DelayMessageEvent 
	// It leaves MessageRequested == true, so message will arrive later
	static bool TimeoutEnabled()
	{
	    return (!MessageRequested 
		    && TimeoutScheduled 
		    && phase == Phase.WaitForEvent);
	}

	[Action]
	static void Timeout() 
	{
	    cevent = ControlEvent.Timeout;
	    TimeoutScheduled = false;
	    phase = Phase.HandleEvent;
	}

	static bool TimeoutMsgLostEnabled()
	{
	    return (MessageRequested 
		    && TimeoutScheduled 
		    && phase == Phase.WaitForEvent);
	}

	[Action]
	static void TimeoutMsgLost() 
	{
	    cevent = ControlEvent.Timeout;
	    TimeoutScheduled = false;
	    phase = Phase.HandleEvent;
	    MessageRequested = false;
	}

	static bool TimeoutMsgLateEnabled()
	{
	    return (MessageRequested 
		    && TimeoutScheduled 
		    && phase == Phase.WaitForEvent);
	}

	[Action]
	static void TimeoutMsgLate() 
	{
	    cevent = ControlEvent.Timeout;
	    TimeoutScheduled = false;
	    phase = Phase.HandleEvent;
	    // MessageRequested remains true, message will arrive later
	}

	// A different action for each message value

	static bool MessageEnabled()
	{
	    return (MessageRequested 
		    && phase == Phase.WaitForEvent);
	}

	[Action]
	static void Message([Domain("Messages")] string message)
	{
	    cevent = ControlEvent.Message;
	    buffer = message;
	    MessageRequested = false;
	    phase = Phase.HandleEvent;
	}

	static Set<string> Messages()
	{
	    return new Set<string>(InRange, OutOfRange);
	}

	static bool CommandEnabled()
	{
	    return (phase == Phase.WaitForEvent);
	}

	[Action]
	static void Command()
	{
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
	static bool CalibrateOutOfRange() 
	{ 
	    return (CalibrateEnabled()
		    &&  buffer != InRange);
	}

	// Negation of unsafe state, above
	[StateInvariant]
	static bool CalibrateInRange() 
	{ 
	    return (!CalibrateEnabled()
		    ||  buffer == InRange);
	}

	// Calibrate is enabled, both buffer and previous are in range
	[AcceptingStateCondition]
	static bool SafeCalibrateEnabled()
	{
	    return (CalibrateEnabled() 
		    && buffer == InRange
		    && previous == double.Parse(InRange));
	}
    }
}

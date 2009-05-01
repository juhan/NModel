Payroll samples from Chapter 15, and some variations. 

Build all projects from VS, or run the build command in the solution 
directory: > build

Payroll project is Payroll1 example, p. 250, with static methods 
and explicit field map.  The Domain attributes are on the parameters in 
the static methods.

Payroll2 project is Payroll2 example, p. 251 - 252, with instance methods
and no explicit field map.  The Domain attributes are on the instance
methods.

Payroll3 project is like Payroll2, except the Domain attribute is on the
class; it is applied to all the instance methods.

Payroll4 project is like Payroll2, except it has an additional static method
with a Domain attribute on the parameter.  Commented-out code shows a
Domain attribute on the static method; this is not allowed. 

In all four projects, run mpv /fsm:FSM.txt to display the scenario FSM as in
Fig 15.1, p. 254.  Run mpv @mpv_args.txt to display the scenario FSM
composed with the model program, as in Fig. 15.2, p. 254.
Running mpv @mpv_args_iso.txt should show the graph with isomorphism reduction
as in Fig 15.4, p. 257.

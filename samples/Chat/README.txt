This example illustrates on-the-fly testing of the chat system. 
There is a server and several clients that communicate over sockets. 
The test harness of the chat system implements the IAsyncStepper interface.

In order to run the model program viewer (mpv) or the conformance
tester (ct) you must first compile the model and the implementation
samples from Viusal Studio or by calling:

>build

In this example the building adds the binaries directly to the Chat 
folder in order for the sample to work correctly.

Sample command line arguments for mpv are given in the response file
mpv_args.txt.  You can edit this file or provide additional settings
on the commandline.  In order to see what settings are possible, call

>mpv /?

In order to view or explore the state space of the model, call:

>mpv @mpv_args.txt

Sample command line arguments for ct are given in the response file
ct_args.txt.  You can edit this file or provide additional settings
on the commandline.  In order to see what settings are possible, call:

>ct /?

In order to run the conformance tester with the given model program
and the given implemenation, call:

>ct @ct_args.txt


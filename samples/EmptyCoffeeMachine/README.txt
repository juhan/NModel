In order to run the model program viewer (mpv) or the conformance
tester (ct) you must first compile the model and the implementation
samples by using Visual studio or calling:

Running ct requires that MDPAlgos has been compiled

>build

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

Choose between different strategies by modifying the ct_args.txt file
Test results of ct are saved in a log file specified by the /log option 
or printed to the screen if no log file is given.
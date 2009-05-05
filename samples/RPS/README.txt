Metrics module

Sample: RPS

Build: Open the sln file in visual-studio and run the build few times until all the references are found. 
You might have to supply the NModel.dll path to the harness and model projects if it can't be found by the build.

/run
	ct_online.txt: ct arguments file
	RPS_Reqs.txt: external requirements file	
	
---------------------------------------------------------------

Metrics:

1. General Summary
	1.1 Total executed tests
	1.2 Total failed tests
	1.3 Pass rate
2. Failed Actions summary
3. Requirements coverage by test-suite (compare with a requirements list from a loaded file)
4. Requirements coverage by the IUT, out of the requirements covered by the test suite
5. Total time spent in each action
6. Executed Requirements by each test

---------------------------------------------------------------

Command-line arguments:

Show the requirements that are executed by each test:
/showTestCaseCoveredRequirements[+|-] 
Default value: '-' 
Short form: /tcreq


File containing the requirements for checking execution coverage
/RequirementsFile:<string>                                                       
Short form: /req

---------------------------------------------------------------

Requirements metrics info:

The requirements attributes must only be added to action and guard (enabled) methods.
Example for a requirement attribute:

[Requirement("string - id", "string - description")]

The requirements in the file should be in the form:

action name | req - id | req - description

The 'action name' is to enable sorting the requirements in the external requirements file (only for usability)
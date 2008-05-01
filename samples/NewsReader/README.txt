To run this example, follow these steps:

1) Build the dlls either with VS or run

> build


2) Produce a file Test.txt that contains a test suite 
containing a single test case that provides transition 
coverage of the NewsReaderUI model.

> otg @otg_args.txt 


3) Execute the generated test case against the sample 
implementation stepper

> ct @ct_args.txt


4) To view the model program with mpv run

> mpv @mpv_args.txt

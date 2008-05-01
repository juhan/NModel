Run 

> mpv /fsm:nfa.txt

The sample illustrates how a non-deterministic finite automaton
is determinized and viewed by mpv.

The given nfa accepts all strings of the form (a+b)*a(a+b)(a+b)
i.e. the third letter from the end is 'a'
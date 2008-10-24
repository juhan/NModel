Client/Server implementation from chapter 2, Stepper from section 8.3,
test as in 8.4 (offline tests) and 12.1 (online ).

Build in VS or at Command Prompt

> build

To run the offline tests you must first execute the otg commands in the
Model directory, see the README there.

To execute the offline tests generated from the contract model, as in 8.4

> ct @ct_contract.txt

To execute the offline test generated from the contract model
composed with the test scenario, as in 8.4

> ct @ct_scenario.txt

To execute tests randomly generated on-the-fly from the contract model
program, one at a time, as in 12.1

> ct @ct_contract_random.txt

To execute tests randomly generated on-the-fly from the contract model
program, one at a time, as in 12.1

> ct @ct_scenario_random.txt
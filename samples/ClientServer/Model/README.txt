Client/Server model from section 5.6, visualization and composition 
from chapter 7, offline test generation from chapter 8

In this version, the ClientServer model program namespace does NOT
include a factory method.  The response files here use the /mp switch
but no factory method argument.  The model program DLL is named
ClientServer.dll.

Build in VS or at the command prompt

> build

To display the FSM of the model program as in fig. 7.1

> mpv @mpv_contract.txt

To display the test scenario as in fig. 7.14:

> mpv @mpv_scenario.txt

To display the test scenario composed with the contract model program 
as in fig 7.15:

> mpv @mpv_composition.txt

To create, then display the test suite that covers the entire contract
model FSM, as described in section 8.1 and shown in figure 8.1.

> otg @otg_contract.txt
> mpv @mpv_contract_test.txt

To create, then display the test suite generated from the contract
model program composed with a test scenario, as described in section 8.1 
and shown in figure 8.2.

> otg @otg_scenario.txt
> mpv @mpv_scenario_test.txt

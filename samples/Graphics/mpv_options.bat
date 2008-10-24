rem mpv_options.bat
rem Document a collection of response files that covers all mpv options
rem Use for regression testing mpv, mp2dot

rem uses /r:Reactive.dll /mp:Reactive  (not factory method) 
rem uses non-default /maxTransitions:300 to show the whole graph
rem uses non-default /stateShape:Circle /combineActions+ /safetyCheckIsOn+
rem uses non-default /unsafeStateColor:DarkOrange
mpv @mpv_safety.txt

rem uses default, implicit /safetyCheckIsOn-
mpv @mpv_safety_nocheck.txt

rem uses non-default /maxTransitions:15 to limit graph
mpv @mpv_safety_max15.txt

rem uses non-default /initialTransitions:15 to limit graph
mpv @mpv_safety_init15.txt

rem uses /r:ClientServerFactoryMethod.dll ... ClientServer.Factory.Create, <model>
rem uses non-default /initialStateColor:Green /stateShape:Octagon
rem uses non-default /transitionLabels:ActionSymbol
rem uses non-default /acceptingStatesMarked-
rem uses non-default /hoverColor:Red /selectionColor:DarkOrange
rem uses /fsm
mpv @mpv_composition_options.txt

rem uses @<file>
call mpv_composition_options.bat

rem uses non-default /direction:LeftToRight
mpv @mpv_composition_lr.txt

rem uses non-default /nodeLabelsVisible-
rem uses /fsm
mpv @mpv_hidenodelabels.txt

rem uses default, implicit /livenessCheckIsOn-
mpv @mpv_contract_nolive.txt

rem uses non-default /deadStateColor:Magenta
rem uses non-default /livenessCheckIsOn+
rem uses default, implicit /deadStatesVisible+
mpv @mpv_contract.txt

rem uses non-default /livenessCheckIsOn+
rem uses non-default /deadStatesVisible-
mpv @mpv_contract_options.txt

rem uses default, implicit /loopsVisible+
rem uses /fsm
mpv @mpv_showloops.txt

rem uses non-default /loopsVisible-
rem uses /fsm
mpv @mpv_hideloops.txt 

rem uses default, implicit /mergeLabels+
rem uses /fsm
mpv @mpv_mergelabels.txt

rem uses non-default /mergeLabels-
rem uses /fsm
mpv @mpv_separatelabels.txt

rem uses default, implicit /combineActions-
mpv @mpv_composition_nocombine.txt

rem uses non-default /combineActions+
rem uses /fsm
mpv @mpv_composition.txt

rem uses /testSuite
mpv @mpv_contract_test.txt

rem uses /startTestAction
rem uses /testSuite
mpv @mpv_contract_test_startaction.txt

rem uses /group
mpv @mpv_safety_group.txt

rem uses /help
mpv @mpv_help.txt

rem mp2dot_options.bat
rem Document a collection of response files that covers all mpv options
rem Use for regression testing mpv, mp2dot
rem Uses mp2ps.bat, dotps.bat, dot program

rem uses /r:Reactive.dll /mp:Reactive  (not factory method) 
rem uses non-default /maxTransitions:300 to show the whole graph
rem uses non-default /stateShape:Circle /combineActions+ /safetyCheckIsOn+
rem uses non-default /unsafeStateColor:DarkOrange
rem second argument 'n' (or anything else) suppresses display of generated .ps
call mp2ps mpv_safety n

rem uses default, implicit /safetyCheckIsOn-
call mp2ps mpv_safety_nocheck n

rem uses non-default /maxTransitions:15 to limit graph
call mp2ps mpv_safety_max15 n

rem uses non-default /initialTransitions:15 to limit graph
call mp2ps mpv_safety_init15 n

rem uses /r:ClientServerFactoryMethod.dll ... ClientServer.Factory.Create, <model>
rem uses non-default /initialStateColor:Green /stateShape:Octagon
rem uses non-default /transitionLabels:ActionSymbol
rem uses non-default /acceptingStatesMarked-
rem uses non-default /hoverColor:Red /selectionColor:DarkOrange
rem uses /fsm
call mp2ps mpv_composition_options n

rem Uses mpv not mp2dot, don't call here
rem uses @<file>
rem call mpv_composition_options.bat

rem uses non-default /direction:LeftToRight
call mp2ps mpv_composition_lr n

rem uses non-default /nodeLabelsVisible-
rem uses /fsm
call mp2ps mpv_hidenodelabels n

rem uses default, implicit /livenessCheckIsOn-
call mp2ps mpv_contract_nolive n

rem uses non-default /deadStateColor:Magenta
rem uses non-default /livenessCheckIsOn+
rem uses default, implicit /deadStatesVisible+
call mp2ps mpv_contract n

rem uses non-default /livenessCheckIsOn+
rem uses non-default /deadStatesVisible-
call mp2ps mpv_contract_options n

rem uses default, implicit /loopsVisible+
rem uses /fsm
call mp2ps mpv_showloops n

rem uses non-default /loopsVisible-
rem uses /fsm
call mp2ps mpv_hideloops n

rem uses default, implicit /mergeLabels+
rem uses /fsm
call mp2ps mpv_mergelabels n

rem uses non-default /mergeLabels-
rem uses /fsm
call mp2ps mpv_separatelabels n

rem uses default, implicit /combineActions-
call mp2ps mpv_composition_nocombine n

rem uses non-default /combineActions+
rem uses /fsm
call mp2ps mpv_composition n

rem uses /testSuite
call mp2ps mpv_contract_test n

rem uses /startTestAction
rem uses /testSuite
call mp2ps mpv_contract_test_startaction n

rem uses /group
call mp2ps mpv_safety_group n

rem This generates no dot output, omit
rem uses /help
rem call mp2ps mpv_help n


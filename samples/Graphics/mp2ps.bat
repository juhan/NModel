rem Run mp2dot, generate .ps and display (can optionally suppress display)
rem %1 response file basename without final .txt
rem %2 any nonempty string suppresses display of generated .ps
mp2dot /dot:%1_mp2.dot @%1.txt
dotps %1_mp2 %2

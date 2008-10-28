

@echo Please make sure you have copied NModel binaries to the build directory!

csc /r:build\NModel.dll /t:library /out:build\WebModel.dll Model\*.cs
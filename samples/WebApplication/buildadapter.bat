
@echo Please make sure you have copied NModel binaries to the build directory!

csc /r:build\NModel.dll /r:build\WebModel.dll /t:library /out:build\Adapter.dll Adapter\*.cs
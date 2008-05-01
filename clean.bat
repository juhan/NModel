rmdir /S /Q build\NModel.Tasks\obj
del /Q /A:H build\NModel.Tasks\NModel.Tasks.suo
del /Q build\NModel.chm build\NModel.chw build\NModel.msi 
del /Q build\NModel.Tasks.dll build\NModel.Tasks.pdb
del /Q build\reflection.*
del /Q build\manifest.xml
rmdir /S /Q build\Output
rmdir /S /Q build\Intellisense
rmdir /S /Q Samples\Bag\bin
del /S /Q /F *.dll *.pdb *.exe *.user *.csproj.vspscc *.SCC *.csproj.FileList.txt *.cache TestResult.xml NModel.xml NModel.vssscc
del /S /Q /A:H *.suo
del /S /Q *.csproj.FileListAbsolute.txt
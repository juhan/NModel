@rem build.bat for ClientServer model

csc /t:library ^
  /r:"C:\Program Files\NModel\bin\NModel.dll" ^
  ClientServer.cs

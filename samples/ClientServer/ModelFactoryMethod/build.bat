@rem build.bat for ClientServer model

csc /t:library ^
  /out:ClientServerFactoryMethod.dll ^
  /r:"C:\Program Files\NModel\bin\NModel.dll" ^
  ClientServer.cs

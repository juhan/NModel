@rem build.bat for ClientServer implementation and stepper

csc /t:library /out:Server.dll ^
  Server.cs

csc /t:library /out:Client.dll ^
  Client.cs

csc /t:library /out:Stepper.dll ^
    /r:Server.dll ^
    /r:Client.dll ^
    /r:"C:\Program Files\NModel\bin\NModel.dll" ^
  Stepper.cs

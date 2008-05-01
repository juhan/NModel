ECHO OFF

REM ************ Generate NModel.chm with Sandcastle ****************


REM ********** Set path for .net framework2.0, sandcastle, hhc****************************

set PATH=%windir%\Microsoft.NET\Framework\v2.0.50727;%DXROOT%\ProductionTools;%ProgramFiles%\HTML Help Workshop;%ProgramFiles%\Microsoft Help 2.0 SDK;%PATH%

if exist output rmdir output /s /q
if exist chm rmdir chm /s /q

REM ********** Copy NModel assembly here ****************************
copy "..\bin\NModel.dll" NModel.dll
copy "..\bin\NModel.pdb" NModel.pdb
copy "..\bin\NModel.xml" comments.xml

REM ********** Call MRefBuilder ****************************

MRefBuilder NModel.dll /out:reflection.org /internal-

REM ********** Apply Transforms **************************** 

XslTransform /xsl:"%DXROOT%\ProductionTransforms\ApplyVSDocModel.xsl" reflection.org /xsl:"%DXROOT%\ProductionTransforms\AddFriendlyFilenames.xsl" /out:reflection.xml /arg:IncludeAllMembersTopic=true /arg:IncludeInheritedOverloadTopics=true

XslTransform /xsl:"%DXROOT%\ProductionTransforms\ReflectionToManifest.xsl"  reflection.xml /out:manifest.xml

call "%DXROOT%\Presentation\vs2005\copyOutput.bat"

REM ********** Call BuildAssembler ****************************
BuildAssembler /config:"%DXROOT%\Presentation\vs2005\configuration\sandcastle.config" manifest.xml

REM **************Generate an intermediate Toc file that simulates the Whidbey TOC format.

XslTransform /xsl:"%DXROOT%\ProductionTransforms\createvstoc.xsl" reflection.xml /out:toc.xml 

REM ************ Generate CHM help project ******************************

if not exist chm mkdir chm
if not exist chm\html mkdir chm\html
if not exist chm\icons mkdir chm\icons
if not exist chm\scripts mkdir chm\scripts
if not exist chm\styles mkdir chm\styles
if not exist chm\media mkdir chm\media

xcopy output\icons\* chm\icons\ /y /r
xcopy output\media\* chm\media\ /y /r
xcopy output\scripts\* chm\scripts\ /y /r
xcopy output\styles\* chm\styles\ /y /r

ChmBuilder.exe /project:NModel /html:Output\html /lcid:1033 /toc:Toc.xml /out:Chm

DBCSFix.exe /d:Chm /l:1033 

hhc chm\NModel.hhp

REM ******* move the generated help file to the build directory *******
move /Y chm\NModel.chm NModel.chm

ECHO Deleting temporary files
REM ******* remove temporary files *******

rmdir output /s /q
rmdir chm /s /q
rmdir Intellisense /s /q
del NModel.dll
del NModel.pdb
del toc.xml
del manifest.xml
del reflection.org
del reflection.xml
del comments.xml

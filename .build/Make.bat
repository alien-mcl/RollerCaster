@ECHO OFF
ECHO Cleaning up...
IF EXIST RollerCaster\bin RD /s /q RollerCaster\bin
IF EXIST RollerCaster\obj RD /s /q RollerCaster\obj
IF EXIST RollerCaster.Tests\bin RD /s /q RollerCaster.Tests\bin
IF EXIST RollerCaster.Tests\obj RD /s /q RollerCaster.Tests\obj
IF EXIST NugetBuild RD /s /q NugetBuild
ECHO Setting up a version...
@ECHO OFF
CALL ".build\Version"
ECHO Building .net Framework v4.6.1
@ECHO OFF
msbuild RollerCaster.sln /t:Restore
msbuild RollerCaster.sln /p:Configuration=Release
ECHO Building NETSTANDARD2.0
@ECHO OFF
msbuild RollerCaster.Core.sln /t:Restore
msbuild RollerCaster.Core.sln /p:Configuration=Release
ECHO Building up Nuget packages
MD NugetBuild
MD NugetBuild\lib
MD NugetBuild\lib\net461
MD NugetBuild\lib\netstandard2.0
COPY RollerCaster\bin\Release\RollerCaster.dll NugetBuild\lib\net461
COPY RollerCaster\bin\Release\RollerCaster.xml NugetBuild\lib\net461
COPY RollerCaster\bin\Release\netstandard2.0\RollerCaster.dll NugetBuild\lib\netstandard2.0
COPY ".nuget\RollerCaster.nuspec" NugetBuild
COPY "LICENCE.md" NugetBuild
COPY "README.md" NugetBuild
".build\nuget" pack NugetBuild\RollerCaster.nuspec -version %tag:~1%.%version%.%release% -outputdirectory NugetBuild
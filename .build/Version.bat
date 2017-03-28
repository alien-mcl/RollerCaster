@ECHO OFF
SET "tag=0.5"
SET release=0
SET version=0
SET hash=0
FOR /F "tokens=*" %%a IN ('git tag') DO (
	SET tag=%%a
	SET /A version+=1
)
FOR /F "tokens=1" %%a IN ('git show-ref -s %tag%') DO SET hash=%%a
IF %version% gtr 0 SET /A version-=1
ECHO [assembly: System.Reflection.AssemblyVersion("%tag:~1%.%version%.%release%")] > "%CD%\.build\VersionAssemblyInfo.cs""
ECHO [assembly: System.Reflection.AssemblyFileVersion("%tag:~1%.%version%.%release%")] >> "%CD%\.build\VersionAssemblyInfo.cs""
ECHO [assembly: System.Reflection.AssemblyInformationalVersion("%tag:~1%.%version%.%release%-%hash%")] >> "%CD%\.build\VersionAssemblyInfo.cs""

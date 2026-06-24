:: Batch file assumes parameters: postBuild.bat "$(ConfigurationName)" "$(SolutionDir)" "$(OutDir)"

ECHO ****************************
SET this=Post-build script

SETLOCAL ENABLEEXTENSIONS
IF ERRORLEVEL 1 ECHO %this%: Unable to enable extensions

:: Rename the passed parameters for clarity
SET "buildConfiguration=%~1"
SET "solutionDir=%~2"
SET "outDir=%~3"

:: Our build configuration
ECHO %this%: Build configuration is %buildConfiguration%

IF /I "%buildConfiguration%"=="Release" (
  :: Release builds must run broad validation before packaging.
  SET "testCaseFilter=TestCategory!=SpeechTests"
) ELSE (
  :: Debug builds run fast Credentials and Doc Generation smoke tests only.
  SET "testCaseFilter=TestCategory=Credentials|TestCategory=DocGen"
)

ECHO %this%: Running dotnet tests for "%solutionDir%Tests\Tests.csproj"
ECHO %this%: Test filter is "%testCaseFilter%"
dotnet test "%solutionDir%Tests\Tests.csproj" -c "%buildConfiguration%" --no-build --no-restore --filter "%testCaseFilter%" -p:SolutionDir="%solutionDir%" -p:Platform=x64 --blame-hang --blame-hang-timeout 5m --blame-hang-dump-type mini
EXIT /B %ERRORLEVEL%

ECHO ****************************

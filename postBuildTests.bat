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
  CALL :RunTests "TestCategory!=SpeechTests" "%buildConfiguration%-postBuildTests.trx"
  EXIT /B %ERRORLEVEL%
)

:: Debug builds run fast Credentials and Doc Generation smoke tests only.
CALL :RunTests "TestCategory=Credentials" "%buildConfiguration%-credentials-postBuildTests.trx"
IF ERRORLEVEL 1 EXIT /B %ERRORLEVEL%

CALL :RunTests "TestCategory=DocGen" "%buildConfiguration%-docgen-postBuildTests.trx"
EXIT /B %ERRORLEVEL%

:RunTests
SET "testCaseFilter=%~1"
SET "trxLogFileName=%~2"

ECHO %this%: Running dotnet tests for "%solutionDir%Tests\Tests.csproj"
ECHO %this%: Test filter is "%testCaseFilter%"
SET "testResultsDir=%solutionDir%TestResults\%buildConfiguration%"
ECHO %this%: Test results and blame artifacts will be written to "%testResultsDir%"
powershell -NoProfile -ExecutionPolicy Bypass -Command "& dotnet test '%solutionDir%Tests\Tests.csproj' -c '%buildConfiguration%' --no-build --no-restore --filter '%testCaseFilter%' '-p:SolutionDir=%solutionDir%' -p:Platform=x64 --results-directory '%testResultsDir%' --logger 'console;verbosity=detailed' --logger 'trx;LogFileName=%trxLogFileName%' --blame-hang --blame-hang-timeout 5m --blame-hang-dump-type mini"
EXIT /B %ERRORLEVEL%

ECHO ****************************

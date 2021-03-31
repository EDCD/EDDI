:: Batch file assumes parameters: postBuild.bat "$(ConfigurationName)" "$(SolutionDir)" "$(OutDir)"

ECHO ****************************
SET this=Post-build script

SETLOCAL ENABLEEXTENSIONS
IF ERRORLEVEL 1 ECHO %this%: Unable to enable extensions

:: Rename the passed parameters for clarity
SET "buildConfiguration=%1"
SET "solutionDir=%~2"
SET "outDir=%~3"

:: Our build configuration
ECHO %this%: Build configuration is %buildConfiguration%

:: Find our install directory
SET "vswhere=%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe"
SET "vswhereArgs=-latest -products * -requires Microsoft.VisualStudio.Workload.ManagedDesktop Microsoft.VisualStudio.Workload.Web -requiresAny -property installationPath"
FOR /f "usebackq tokens=*" %%i IN (
   `CALL "%vswhere%" %vswhereArgs%`
 ) DO (
  SET devEnvDir=%%i
)

:: Ref. vstest.console.exe documentation at https://docs.microsoft.com/en-us/visualstudio/test/vstest-console-options?view=vs-2019
:: We need to apply batch file rules for escaping certain characters in our command (using "^"), ref. https://www.robvanderwoude.com/escapechars.php
IF %buildConfiguration%=="Release" (
  :: Run all tests except Speech tests 
  SET "testCaseFilter=^/TestCaseFilter:""TestCategory!=Speech"""
) ELSE (
  :: Run just our Credentials and Doc Generation tests
  SET "testCaseFilter=^/TestCaseFilter:""TestCategory=Credentials""^|""TestCategory=DocGen"""
)

:: Invoke our test adapter in our install directory
SET "testAdapter=%devEnvDir%\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe"
SET "command="%testAdapter%" "%solutionDir%Tests\%outDir%Tests.dll" %testCaseFilter%"
%command%

ECHO ****************************
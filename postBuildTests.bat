:: Batch file assumes parameters: postBuildTests.bat "$(Configuration)" "$(SolutionDir)" "bin\$(Configuration)\"

ECHO ****************************
SET this=Post-build script

SETLOCAL ENABLEEXTENSIONS
IF ERRORLEVEL 1 ECHO %this%: Unable to enable extensions

:: Rename the passed parameters for clarity
SET "buildConfiguration=%1"
SET "solutionDir=%~2"
SET "outDir=%~3"

IF NOT "%solutionDir:~-1%"=="\" SET "solutionDir=%solutionDir%\"
IF "%outDir%"=="" SET "outDir=bin\%buildConfiguration%\"
IF NOT "%outDir:~-1%"=="\" SET "outDir=%outDir%\"

:: Our build configuration
ECHO %this%: Build configuration is %buildConfiguration%

:: If Debug build, skip tests
IF "%buildConfiguration%"=="Debug" (
    ECHO %this%: Skipping post-build tests for Debug configuration
    EXIT /B 0
)

:: Find our install directory
SET "vswhere=%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe"
SET "vswhereArgs=-latest -products * -property installationPath"
FOR /f "usebackq tokens=*" %%i IN (
   `CALL "%vswhere%" %vswhereArgs%`
 ) DO (
  SET devEnvDir=%%i
)
:: Run appropriate tests based on configuration
IF "%buildConfiguration%"=="Release" (
    :: Run all tests except Speech tests 
    SET "testCaseFilter=/TestCaseFilter:""TestCategory!=SpeechTests"""
) ELSE (
    :: Run just our Credentials and Doc Generation tests
    SET "testCaseFilter=/TestCaseFilter:""TestCategory=Credentials""^|""TestCategory=DocGen"""
)

:: Invoke our test adapter in our install directory
SET "testAdapter=%devEnvDir%\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe"
SET "command="%testAdapter%" "%solutionDir%Tests\%outDir%Tests.dll" %testCaseFilter%"
%command%

ECHO ****************************
EXIT /B 0
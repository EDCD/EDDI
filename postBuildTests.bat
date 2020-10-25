:: Batch file assumes parameters: postBuild.bat "$(ConfigurationName)" "$(DevEnvDir)" "$(SolutionDir)$(OutDir)"

ECHO ****************************
SET this=Post-build script

:: Rename the passed parameters for clarity
SET "buildConfiguration=%1"
SET "devEnvDir=%~2"
SET "buildDir=%~3"

ECHO %this%: Build configuration is %buildConfiguration%

:: Ref. vstest.console.exe documentation at https://docs.microsoft.com/en-us/visualstudio/test/vstest-console-options?view=vs-2019
:: We need to apply batch file rules for escaping certain characters in our command (using "^"), ref. https://www.robvanderwoude.com/escapechars.php
IF %buildConfiguration%=="Release" (
  :: Run all tests except Speech tests 
  SET "testCaseFilter=^/TestCaseFilter:""TestCategory!=Speech"""
) ELSE (
  :: Run just our Credentials test
  SET "testCaseFilter=^/TestCaseFilter:""TestCategory=Credentials"""
)

SET "command="%devEnvDir%CommonExtensions\Microsoft\TestWindow\vstest.console.exe" "%buildDir%Tests.dll" %testCaseFilter%"

ECHO %this%: Invoking... %command%
%command%

ECHO ****************************
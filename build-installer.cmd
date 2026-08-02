rem Requires Inno Setup from http://www.jrsoftware.org/isdl.php
setlocal

set "iscc="
for %%I in (ISCC.exe) do set "iscc=%%~$PATH:I"
if not defined iscc if exist "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" set "iscc=C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
if not defined iscc (
    echo ISCC.exe not found on PATH or at "C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
    exit /b 1
)

set "releaseRoot=bin\Release"
set "appOutput=%releaseRoot%\Application"
set "shimOutput=%releaseRoot%\VoiceAttackPluginShim"
set "docGenerator=%releaseRoot%\BuildTools\DocumentationGenerator\DocumentationGenerator.dll"

echo ---- Checking MSBuild release layout ----
if not exist "%appOutput%\Eddi.exe" (
    echo Eddi.exe not found at "%appOutput%\Eddi.exe"
    exit /b 1
)

if not exist "%docGenerator%" (
    echo DocumentationGenerator.dll not found at "%docGenerator%"
    exit /b 1
)

if not exist "%shimOutput%\EddiVoiceAttackAdapter.dll" (
    echo EddiVoiceAttackAdapter.dll not found at "%shimOutput%\EddiVoiceAttackAdapter.dll"
    exit /b 1
)

if not exist "%shimOutput%\EDDI.vap" (
    echo EDDI.vap not found at "%shimOutput%\EDDI.vap"
    exit /b 1
)

if not exist "%shimOutput%\eddi_app_path.txt" (
    echo eddi_app_path.txt not found at "%shimOutput%\eddi_app_path.txt"
    exit /b 1
)

echo ---- Generating installer documentation payload ----
dotnet "%docGenerator%" "%appOutput%"
if not "%ERRORLEVEL%"=="0" exit /b %ERRORLEVEL%

if not exist "%appOutput%\Cottle\Custom keywords.txt" (
    echo Custom keywords.txt not found at "%appOutput%\Cottle\Custom keywords.txt"
    exit /b 1
)

if not exist "%appOutput%\Wiki\Variables.md" (
    echo Variables.md not found at "%appOutput%\Wiki\Variables.md"
    exit /b 1
)

if not exist "%appOutput%\Wiki\VoiceAttack-Integration.md" (
    echo VoiceAttack-Integration.md not found at "%appOutput%\Wiki\VoiceAttack-Integration.md"
    exit /b 1
)

echo ---- Compiling installer ----
"%iscc%" Installer.iss
if not "%ERRORLEVEL%"=="0" exit /b %ERRORLEVEL%
echo ---- Installer compiled ----

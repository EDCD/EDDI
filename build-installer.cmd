rem Requires Inno Setup from http://www.jrsoftware.org/isdl.php
rem Assumes the Inno Setup dir, by default "%ProgramFiles(x86)%\Inno Setup 5" is in your path
echo ---- Compiling installer ----
Compil32 /cc Installer.iss
echo ---- Installer compiled ----

rem Requires 7Zip from http://www.7-zip.org/download.html
rem Assumes the 7Zip dir, by default "C:\Program Files\7-Zip" is in your path
echo ---- zipping PDB files ----
7z a -r bin\Installer\PDBs.zip bin\Release\*.pdb
echo ---- PDB files zipped ----

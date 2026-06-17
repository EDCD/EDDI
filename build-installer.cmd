rem Requires Inno Setup from http://www.jrsoftware.org/isdl.php
echo ---- Compiling installer ----
"C:\Program Files (x86)\Inno Setup 6\ISCC.exe" Installer.iss
echo ---- Installer compiled ----

rem Requires 7Zip from http://www.7-zip.org/download.html
echo ---- zipping PDB files ----
"C:\Program Files\7-Zip\7z.exe" a -r bin\Installer\PDBs.zip bin\Release\*.pdb
echo ---- PDB files zipped ----

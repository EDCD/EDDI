Building this project requires:

* Microsoft Visual Studio (obviously)
* Inno Setup from http://www.jrsoftware.org/isdl.php
  * Get the Unicode variant.
  * Assumes the Inno Setup dir, by default "%ProgramFiles(x86)%\Inno Setup 6" is in your path.

Locally running EDDI:

* Start Visual Studio and choose "Open a project/solution" then select the "EDDI.sln" solution file
* In the top toolbar select the desired build configuration (e.g. "Debug" or "Release")
* In the Solution Explorer, right-click "Eddi" and choose "Set as Startup Project"
* In the "Build" menu choose "Build Solution" (or use the keyboard shortcut Ctrl-Shift-B)
* In the top toolbar press the "Start" button
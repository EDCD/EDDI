# EDDI: The Elite Dangerous Data Interface

Current version: 2.0.10

EDDI is a companion application for Elite: Dangerous, providing responses to events that occur in-game using data from the game as well as various third-party tools.

EDDI responds to a wide range of events, and also provides a VoiceAttack plugin to allow for direct interaction with information made available by EDDI.

## Installing EDDI

EDDI can be installed standalone or as a VoiceAttack plugin.

Download the EDDI installer from [http://www.mcdee.net/elite/EDDI.exe](http://www.mcdee.net/elite/EDDI.exe).  By default it will install in C:\Program Files (x86)\VoiceAttack\Apps\EDDI, which is fine regardless of if you have VoiceAttack or not, but of course you can change it if you wish (although note that if the installation is not in a subdirectory of your VoiceAttack's Apps directory then it will not be able to be used as a VoiceAttack plugin).

Alternatively you can compile EDDI from the sources at [https://github.com/cmdrmcdonald/EliteDangerousDataProvider](https://github.com/cmdrmcdonald/EliteDangerousDataProvider).


### Configuring EDDI

When you start EDDI it will bring up a window with a number of tabs.  Each tab explains its function and how to configure it, so you will be best served to read each tab and set it up according to your liking.

## Upgrading EDDI

If you are upgrading from EDDI 1 EDDI it is recommended that you uninstall your existing version of EDDI and remove your %APPDATA%\EDDI directory prior to upgrading to the new one.  This ensures that there is a clean installation and reduces the chances of problems occurring.

If you are upgrading from another version of EDDI 2 the installer will take care of changes so you just need to run it.

## EDDI Voices

EDDI uses the standard Windows TTS (text-to-speech) voices.  To be eligible for use in EDDI the voice needs to support phonetic speech.  Apart from Windows' default voices there are commercial voices available from IVONA and Cereproc, amongst others, that can be used.  The voice needs to be visible to Windows' TTS system to be made available to EDDI: this usually takes place when you install the voice.  If you cannot see a voice in EDDI then check the Windows TTS settings.

## How EDDI Works

EDDI uses a number of monitors to obtain information about the Elite: Dangerous universe.  When these monitors have something to report they send the information to EDDI in the form of an event.

When EDDI receives n event from a monitor it updates its internal data structures as required, for example if a jump has taken place it will obtain information about the system to which you are jumping.  Once EDDI is up-to-date it will pass on the event to all enabled responders.

Each responder has a specific task.  Built-in responders generate speech, send data to external tools such as EDSM, update VoiceAttack, etc.

Details on the operation of each monitor and responder is available in their tab on the EDDI UI.

# Known Issues

  * EDDI relies on the Elite: Dangerous companion app API for a lot of its information.  Sometimes EDDI loses connection to the API and needs to re-authenticate.  If you think that this is a problem you can re-run the 'EDDI.exe' and if the connection is bad it will ask for re-authentication
  * EDDI is unable to know for sure if you have provided the correct path to the Logs directory.  The only way of knowing this for sure is to jump and see if EDDI tells you about your destination when you make a jump

If you have an issue with EDDI then please report it at https://github.com/cmdrmcdonald/EliteDangerousDataProvider/issues  If you have encountered a problem then please provide the output of the error report (shift-control-alt-e) to aid in fixing the issue.

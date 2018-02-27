# Troubleshooting

This troubleshooting guide is divided into three sections:
- [General Issues](https://github.com/EDCD/EDDI/blob/master/TROUBLESHOOTING.md#general-issues)
- [Specific Issues](https://github.com/EDCD/EDDI/blob/master/TROUBLESHOOTING.md#specific-issues)
- [Issues with VoiceAttack Integration](https://github.com/EDCD/EDDI/blob/master/TROUBLESHOOTING.md#issues-with-voiceattack-integration).

# General Issues

## Ensure that EDDI is running

EDDI can be started with the EDDI.exe program or by starting VoiceAttack with the EDDI plugin.  Note that you should only do one of these, as otherwise you will have to separate copies of EDDI running and will hear speech twice, etc.

## Ensure that you have configured EDDI

EDDI has a number of configuration items.  Although they are not required, parts of EDDI will not work without them.  Start up the EDDI.exe program and read through the instructions presented on the first tab to ensure that you have configured all of your required information.

# Specific Issues

## EDDI doesn't start or crashes on startup

Check whether you are running the latest version of EDDI. On older versions of EDDI (prior to version 2.4.6-b2), EDDI would crash or fail to start if Elite Dangerous had not been run previously and the folder containing the player's journal files could not be located. 

If you are already running the latest version of EDDI and EDDI either fails to run or crashes on startup, you may have a problem with your configuration files. To identify a configuration which has become corrupted and is preventing EDDI from working correctly:

1. Navigate to %APPDATA%/EDDI
2. Cut all of the files and paste them to another location
3. Attempt to start EDDI. EDDI should open with the default configuration.
4. Exit EDDI.
5. Cut and paste one of your configuration files into %APPDATA%/EDDI (overwrite as required)
6. Repeat steps 3 - 5 until EDDI fails to start. Remove the offending file(s) and reconfigure as necessary.

## EDDI is (or is not) giving me suggestions of which commodities to purchase when I dock at a station

To obtain the commodity information EDDI needs access to the companion API.  This requires that the user has configured their "Companion App" tab on the EDDI interface.

EDDI will only provide this information if you are flying in a ship that is configured for trading.  Ensure that your ship is configured for either "Trading" or "Multi-purpose" in the "Shipyard" tab on the EDDI interface.

## EDDI is not using the voice that I configured for it

Windows comes with a small number of text-to-speech voices, most of which can be used for EDDI.  There are a number of methods to obtain additional free voices on the internet but these often result in voices that do not provide the full range of functionality, specifically they do not have the ability to provide phonetic pronunciation.  If EDDI detects that a chosen voice is failing to use phonetic pronunciation it will fall back to using normal speech, or halt the voice entirely depending on the error found.

It is possible (though not typically recommended) to disable phonetic speech if the voice you are using is not compatible with SSML markups.  With phonetic speech disabled, advanced features like vocal pauses will be less accurate and may cease to function entirely. To disable phonetic speech:

  - shut down VoiceAttack if running
  - start up EDDI
  - in the "Text-to-speech" tab check "Disable phonetic speech"
  - shut down EDDI
  - restart EDDI (or VoiceAttack if you are using that)

# Issues with VoiceAttack Integration

## Ensure EDDI is in the correct place

EDDI should be in the "Apps" subdirectory of your VoiceAttack directory.  The directory structure should look like this:

![](images/Directory.jpg)

## Ensure you are running a suitable version of VoiceAttack

EDDI requires at least version 1.5.12 of VoiceAttack.  To check the version of VoiceAttack first select the configuration button on the main VoiceAttack window:

![](images/MainOptions.jpg)

The version number is in the top right of the window.

![](images/OptionsVersion.jpg)

## Ensure that VoiceAttack plugins are enabled

To ensure that plugins are enabled first select the configuration button on the main VoiceAttack window:

![](images/MainOptions.jpg)

Then confirm that plugin support is enabled.

![](images/OptionsPluginSupport.jpg)

## Further questions about integration with VoiceAttack?

Read the wiki entry describing [VoiceAttack Integration](https://github.com/EDCD/EDDI/wiki/VoiceAttack-Integration) with EDDI.

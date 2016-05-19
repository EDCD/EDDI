#Troubleshooting

If EDDI is not working properly then there are a number of items that you should check.

##Ensure EDDI is in the correct place

EDDI should be in the "Apps" subdirectory of your VoiceAttack directory.  The directory structure should look like this:

![](images/Directory.jpg)

##Ensure you are running a suitable version of VoiceAttack

EDDI requires at least version 1.5.8.16 of VoiceAttack.  To check the version of VoiceAttack first select the configuration button on the main VoiceAttack window:

![](images/MainOptions.jpg)

The version number is in the top right of the window.

![](images/OptionsVersion.jpg)

##Ensure that VoiceAttack plugins are enabled

To ensure that plugins are enabled first select the configuration button on the main VoiceAttack window:

![](images/MainOptions.jpg)

Then confirm that plugin support is enabled.

![](images/OptionsPluginSupport.jpg)

##Ensure that the EDDI startup command runs when the profile is loaded

To ensure that the EDDI startup command runs when the profile is loaded first select the profile details button on the main VoiceAttack window:

![](images/MainProfile.jpg)

Then select the profile options button:

![](images/ProfileOptions.jpg)

Then confirm that the "Execute a command each time this profile is loaded" option is checked and that the "((EDDI: startup))" command is selected.

![](images/ProfileOptionsOnLoad.jpg)

If none of these fix your issue then please report it on the issues page.
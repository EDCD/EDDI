EDDI integrates with VoiceAttack in two ways.  Firstly, it generates a large number of variables inside VoiceAttack and keeps them up-to-date.  Secondly, it runs VoiceAttack commands when events occur.

# Using EDDI with VoiceAttack

N.B. EDDI requires VoiceAttack 2 (version 2.0.0+) to function correctly.VoiceAttack version 1.X is no longer supported.

For EDDI to work with VoiceAttack it must be installed as a VoiceAttack plugin.  To do this EDDI should be installed within the `Apps` directory of your VoiceAttack installation; by default VoiceAttack installs in one of two locations: 
- `C:\Program Files\VoiceAttack2\Apps` (for standard licenses)
- `C:\Program Files (x86)\Steam\steamapps\common\VoiceAttack2\Apps` (for Steam licenses)

VoiceAttack must be configured to use plugins.  To do so you must click on the VoiceAttack Options icon (a spanner) in the bottom-right corner of the VoiceAttack main screen, check the 'Enable plugin support' option, and then restart VoiceAttack.

If EDDI is installed in the correct location and plugin support is enabled you should see a message when starting VoiceAttack along the lines of `Plugin EDDI 5.0.0 initialized`.

## EDDI's VoiceAttack Profile

EDDI provides a VoiceAttack profile with some basic commands to get you started.  This is not a control profile in that it does not provide you with the ability to control the ship.  It allows you to display and manipulate EDDI's User Interface with commands like "Configure EDDI" and demonstrates how you can interact with EDDI via the plugin using command phrases such as "please could you repeat that" and questions such as "what use is decoded emission data?"

The profile is available in the EDDI installation directory (normally "C:\Program Files (x86)\Steam\steamapps\common\VoiceAttack2\Apps\EDDI" or "C:\Program Files (x86)\VoiceAttack2\Apps\EDDI") as EDDI.vap. To make the commands in this profile both available and easily updateable, we recommend [importing the profile into VoiceAttack](https://voiceattack.com/VoiceAttackHelp.pdf#page=148) and supplementing your base profile with EDDI profile commands via the ['Include commands from other profiles'](https://voiceattack.com/VoiceAttackHelp.pdf#page=9) profile option.

**_Disclaimer: We recommend that users check the terms and conditions of third party licensing agreements prior to linking or using EDDI's VoiceAttack profile with any licensed third party product. We shall not be held responsible for any third party licensing claims that arise from breaches of third party licensing agreements._**

{{VoiceAttackVariables}}

# Running Commands on EDDI Events

Whenever EDDI sees a particular event occur it will attempt to run a command in VoiceAttack.  The name of the command depends on the event, but follows the form:

    ((EDDI <event>))

with the \<event\> being in lower-case.  For example, if you wanted VoiceAttack to run a command every time you docked you would create a command called `((EDDI docked))` (note the lower-case d at the beginning of docked).

![](images/VoiceAttack-EDDI-Event.jpg)

There are a large number of events available.  Full details of the variables available for each event are available in the individual [event pages](https://github.com/EDCD/EDDI/wiki/Events).  Note that event variables are only valid when the event occurs, and cannot be relied upon to be present or a specific value at any other time.  If you want to use information in an event after the event itself then you should copy the value to another variable.

# EDDI Plugin Functions

EDDI's VoiceAttack plugin allows you to access its features in your own profile.  Details of these functions are laid out below.

![](images/VoiceAttack-PluginView.jpg)

Note: Though the examples in this section show variables being passed as parameters within the plugin interface, it is no longer necessary to do so. Rather, when the plugin is invoked then the plugin will search for variables matching the plugin context and set prior to invoking the plugin.

## Speech functions

### say

This function uses EDDI's voice to render arbitrary speech (outside of the context of a Speech Responder script). It takes one mandatory and two optional variables as parameters.

- 'Script' (text variable) is a mandatory parameter containing the actual speech to render. 
- 'Priority' (integer variable) is an optional parameter defining the priority of the invoked speech (defaults to 3).
- 'Voice' (text variable) is an optional parameter defining the name of the voice you want to use.  Note that when you set this variable it will continue to be used until you unset it, at which point EDDI will use the voice configured in its text-to-speech settings.

For convenience, the value `$=` in the script stands for the phonetic name of your ship while the value `$-` stands for your commander's phonetic name.

To use this function in your own commands set the 'Script' variable and optionally the 'Priority' and 'Voice' variables, then use the 'Execute an external plugin function' command with the plugin context set to 'say'.

A tip for advanced users: It is possible to invoke the Cottle language used in the Speech Responder from the `say` context. To do so, curly brackets used in Cottle functions must be escaped using the `|` (vertical pipe) character. For example, you might use `|{P('{TXT:EDDI body mapped shortname}', 'body')|}` to modify the pronunciation of `{TXT:EDDI body mapped shortname}` by passing it through the Cottle `P()` function.

### speech

This function uses EDDI's voice to read a pre-configured Speech Responder script. It takes one mandatory and two optional variables as parameters.

- 'Script' (text variable) is a mandatory parameter containing the name of the script to invoke. 
- 'Priority' (integer variable) is an optional parameter defining the priority of the invoked speech (defaults to 3).
- 'Voice' (text variable) is an optional parameter defining the name of the voice you want to use.  Note that when you set this variable it will continue to be used until you unset it, at which point EDDI will use the voice configured in its text-to-speech settings.
 
To use this function in your own commands set the 'Script' variable and optionally the 'Priority' and 'Voice' variables, then use the 'Execute an external plugin function' command with the plugin context set to 'speech'.

### transmit

This function uses EDDI's voice to render arbitrary speech (outside of the context of a Speech Responder script) with a radio effect. It takes one mandatory and two optional variables as parameters.

- 'Script' (text variable) is a mandatory parameter containing the actual speech to render.
- 'Priority' (integer variable) is an optional parameter defining the priority of the invoked speech (defaults to 3).
- 'Voice' (text variable) is an optional parameter defining the name of the voice you want to use.  Note that when you set this variable it will continue to be used until you unset it, at which point EDDI will use the voice configured in its text-to-speech settings.
 
For convenience, the value `$=` in the script stands for the phonetic name of your ship while the value `$-` stands for your commander's phonetic name.

To use this function in your own commands set the 'Script' variable and optionally the 'Priority' and 'Voice' variables, then use the 'Execute an external plugin function' command with the plugin context set to 'transmit'.

### shutup

This function stops any active EDDI speech and dequeues any pending speech. There are no parameters.

To use this function in your own commands use the 'Execute an external plugin function' command with the plugin context set to 'shutup'.

### disablespeechresponder

This function tells the speech responder to not talk unless specifically asked for information. There are no parameters. This lasts until either VoiceAttack is restarted or an enablespeechresponder call is made.

To use this function in your own commands use the 'Execute an external plugin function' command with the plugin context set to 'disablespeechresponder'.

### enablespeechresponder

This function tells the speech responder to respond normally to events. There are no parameters.

To use this function in your own commands use the 'Execute an external plugin function' command with the plugin context set to 'enablespeechresponder'.

### setspeechresponderpersonality

This function changes the speech responder's personality. It takes one mandatory variable as a parameter.

- 'Personality' (text variable) is a mandatory parameter containing the name of the personality to invoke.

Note that unlike enablespeechresponder and disablespeechresponder any changes made here are persistent.

To use this function in your own commands set the 'Personality' parameter then use the 'Execute an external plugin function' command with the plugin context set to 'setspeechresponderpersonality'.

### volume

This function changes the text to speech volume. It takes one mandatory variable as a parameter.

- 'Volume' (integer variable) is a mandatory parameter containing the desired volume setting (from 0 - 100).

To use this function in your own commands set the 'Volume' parameter then use the 'Execute an external plugin function' command with the plugin context set to 'volume'.

## Information functions

### coriolis, coriolisbeta, edshipyard, inaracarrier, inaraprofile, inarasystem, or inarastation

Looks up the current ship, the current starsystem, or the current station (as applicable). A web uri is written to '\{TXT: EDDI uri\}' and, unless '\{BOOL:EDDI open uri in browser\}' has been set to false, the uri is opened in the default browser.

### inara

Looks up a named commander on the website Inara.cz. It takes one mandatory variable as a parameter.

- 'Name' (text variable) is a mandatory parameter containing the name of the commander to look up on Inara.cz.

A web uri is written to '\{TXT: EDDI uri\}' and, unless '\{BOOL:EDDI open uri in browser\}' has been set to false, the uri is opened in the default browser.

To use this function in your own commands set the 'Name' parameter then use the 'Execute an external plugin function' command with the plugin context set to 'inara'.

### jumpdetails

This function will provide jump information based on your ship loadout and current fuel level. It takes one mandatory variable as a parameter.

- 'Type variable' (text variable) is a mandatory parameter containing the type of the information to return.

  * `next` range of next jump at current fuel mass and current laden mass
  * `max` maximum jump range at minimum fuel mass and current laden mass
  * `total` total range of multiple jumps from current fuel mass and current laden mass
  * `full` total range of multiple jumps from maximum fuel mass and current laden mass

When this function is used, the following variables will be updated and made available for use in VoiceAttack:

- \{DEC:Ship jump detail distance\}
- \{INT:Ship jump detail jumps\}

To use this function in your own commands set the 'Type variable' parameter then use the 'Execute an external plugin function' command with the plugin context set to 'jumpdetails'.

### route

This function will produce a destination/route. It takes at least one mandatory variable and up to two optional variables as parameters.

- 'Type variable' (text variable) is a mandatory parameter defining the type of command you are sending to the Navigation Monitor. This variable may be used either to plot a new route or to send commands to control a previously plotted route.

  - Route Plotting Types
    * `carrier` Plots a fleet carrier route between systems. Parameters:
      * 'System variable' (mandatory text): Defines the destination system for the fleet carrier.
      * 'System variable 2' (optional text): If set, defines the starting system for the fleet carrier.
      * 'Numeric variable' (optional decimal): If set, defines the used capacity of the fleet carrier.
    * `encoded` Plots a route to the nearest encoded materials trader. Parameters:
      * 'Numeric variable' (optional decimal): If set, overrides the normal maximum distance from arrival to the station in light seconds.
    * `expiring` Plots a route to the system containing your earliest expiring active mission. No additional data required.
    * `facilitator` Plots a route to the nearest 'Legal Facilities' contact. Parameters:
      * 'Numeric variable' (optional decimal): If set, overrides the normal maximum distance from arrival to the station in light seconds.
    * `farthest` Plots a route to the active mission system farthest from your current location. No additional data required.
    * `guardian` Plots a route to the nearest guardian technology broker. Parameters:
      * 'Numeric variable' (optional decimal): If set, overrides the normal maximum distance from arrival to the station in light seconds.
    * `human` Plots a route to the nearest human technology broker. Parameters:
      * 'Numeric variable' (optional decimal): If set, overrides the normal maximum distance from arrival to the station in light seconds.
    * `manufactured` Plots a route to the nearest manufactured materials trader. Parameters:
      * 'Numeric variable' (optional decimal): If set, overrides the normal maximum distance from arrival to the station in light seconds.
    * `most` Plots a route to the system with the most active missions. Parameters:
      * 'System variable' (optional text): If multiple systems have an equal number of active missions, selects the mission system which is nearest the specified system.
    * `neutron` Plots a route to a named star system using neutron stars (pulsars) where available. Parameters:
      * 'System variable' (mandatory text): Defines the destination system for your ship's route.
    * `nearest` Plots a route to the nearest system with missions. No additional data required.
    * `raw` Plots a route to the nearest raw materials trader. Parameters:
      * 'Numeric variable' (optional decimal): If set, overrides the normal maximum distance from arrival to the station in light seconds.
    * `route` Plots the shortest path between active mission destinations in light years. Parameters:
      * 'System variable' (optional text): If set, the resulting route shall begin at the specified star system rather than at the current star system.
    * `scoop` Plots a route to the nearest scoopable star system. Parameters:
      * 'Numeric variable' (optional decimal): If set, overrides the search radius in light years. Maximum value: 100.
    * `source` Plots a route to the nearest recently visited mission 'cargo source'. Parameters:
      * 'System variable' (optional text): If set, the resulting route shall identify cargo source locations near the specified star system rather than near the current star system.

  - Control Types
    * `cancel` Deactivates guidance along the current plotted route.
    * `set` Activates guidance along the current plotted route. Parameters:
      * 'System variable' (optional text): If set, plots a `neutron` route to a specified system then activates guidance.
      * 'Station variable' (optional text): If set, sets the station name in the event output.
    * `update` If guidance is enabled, updates to the next route destination once the current system contains no more active missions. Recalculates the route as required.

To use this function in your own commands set the 'Type variable' parameter and when appropriate the `System variable`, `System variable 2`, 'Numeric variable', and 'Station variable' parameters then use the 'Execute an external plugin function' command with the plugin context set to 'route'. Upon success, a '((EDDI route details))' event is triggered, providing event data as described [in the appropriate wiki page](https://github.com/EDCD/EDDI/wiki/Route-details-event).

![](images/VoiceAttack-PluginView-Route.jpg)

Upon success of the query, a 'Route details' event is triggered with details from the destination and route.

## Utility functions

### setstate

This function pushes a state variable to EDDI's internal session state, allowing it to be shared with other responders. It takes two mandatory variables as parameters.

- 'State variable' (text variable) is a mandatory parameter containing the name of the VoiceAttack variable to store in EDDI.
- The variable to store in EDDI (integer, boolean, decimal, or text variable), as referenced by the 'State variable' parameter.

To use this function in your own commands set the variables described above then use the 'Execute an external plugin function' command with the plugin context set to 'setstate'. This function will read the text variable 'State variable' and store the VoiceAttack variable named in there as a state variable.  

For example, if you wanted to store the VoiceAttack boolean variable "Verbose" as a state variable you would:

    * set the boolean variable "Verbose" to the desired value
    * set the text variable "State variable" to "Verbose"
    * call EDDI with the context set to "setstate"

![](images/VoiceAttack-PluginView-SetState.jpg)

This function only supports integers, booleans, decimals and strings as state values.  The name of the value will be altered if necessary to ensure that it is all lower-case, and that spaces are replace by underscores.  For example, if you attempt to store a state variable "My variable" it will be stored as "my_variable".

State variables are made available in VoiceAttack with the prefix 'EDDI state'.  For example, to access the text variable stored in the last paragraph you would use '\{TXT:EDDI state my_variable\}'.

Variables shall be set to as many variable types as possible, e.g. 'TXT', 'DEC', 'INT', 'SMALL', and 'BOOL' as applicable. Decimal values shall be set to their rounded values in 'INT' and 'SMALL' variable types. Non-zero numeric values and non-empty string values shall be set to 'True' in the BOOL variable type.

To access the same variable from within EDDI's Speech Responder, you would call '\{state.my_variable\}'.

Please note that state is transient, and is purposefully not persisted beyond the running instance of EDDI.  This means that every time you start VoiceAttack the state will be empty.  Also, because EDDI responders run asynchronously and concurrently there is no guarantee that, for example, the speech responder for an event will finish before the VoiceAttack responder for an event starts (or vice versa).

### configuration

This function opens or restores EDDI's UI. There are no parameters.

To use this function in your own commands use the 'Execute an external plugin function' command with the plugin context set to 'configuration'.

### configurationminimize

This function minimizes EDDI's UI. There are no parameters.

To use this function in your own commands use the 'Execute an external plugin function' command with the plugin context set to 'configurationminimize'.

### configurationmaximize

This function maximizes EDDI's UI. There are no parameters.

To use this function in your own commands use the 'Execute an external plugin function' command with the plugin context set to 'configurationmaximize'.

### configurationrestore

This function restores EDDI's UI to a normal window. There are no parameters.

To use this function in your own commands use the 'Execute an external plugin function' command with the plugin context set to 'configurationrestore'.

### configurationclose

This function closes EDDI's UI. There are no parameters.

To use this function in your own commands use the 'Execute an external plugin function' command with the plugin context set to 'configurationclose'.

### system comment

Sets a comment on the current star system on the website EDSM.net. You must have entered your EDSM credentials in EDDI's EDSM responder for this to work. It takes one mandatory variable as a parameter.

- 'EDDI system comment' (text variable) is a mandatory parameter containing the comment to add to the current star system on EDSM.net.

To use this function in your own commands set the 'EDDI system comment' parameter then use the 'Execute an external plugin function' command with the plugin context set to 'system comment'.

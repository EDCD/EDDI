# Using EDDI with VoiceAttack

EDDI interfaces with VoiceAttack in two ways.  Firstly, it generates a large number of variables inside VoiceAttack and keeps them up-to-date.  Secondly, it runs VoiceAttack commands when events occur.  This document explains how to use both of these features.

## Installing EDDI for VoiceAttack

N.B. EDDI requires at least version 1.5.12.22 of VoiceAttack to function correctly.

For EDDI to work with VoiceAttack it must be installed as a VoiceAttack plugin.  To do this EDDI should be installed within the `Apps` directory of your VoiceAttack installation.

VoiceAttack must be configured to use plugins.  To do so you must click on the Settings icon (a spanner) in the top-right corner of the VoiceAttack and check the 'Enable plugin support' option and restart VoiceAttack.

If EDDI is installed in the correct location and plugin support is enabled you should see a message when starting VoiceAttack along the lines of `Plugin EDDI 2.0.0 initialized`.

If you have problems with your installation then your first port of call is the [Troubleshooting guide](https://github.com/cmdrmcdonald/EliteDangerousDataProvider/blob/master/TROUBLESHOOTING.md).  If after following the troubleshooting guide EDDI is still not working them please [report an issue](https://github.com/cmdrmcdonald/EliteDangerousDataProvider/issues).

## Upgrading from EDDI 1.x

If you have version 1.x of EDDI installed then you should remove it entirely prior to installing EDDI.  To do so:

  * If you hav a separate EDDI profile them remove it entirely
  * If you have integrated EDDI in to your own profile then remove all commands with the categories 'EDDI' and 'EDDI debug'
  * Shut down VoiceAttack
  * Remove the EDDI directory from the `Apps` directory of your VoiceAttack installation (default C:\Program Files (x86)\VoiceAttack\Apps)

## EDDI Variables

EDDI makes a large number of values available to augment your existing scripts.  The values are shown below, along with a brief description of what the value holds.

If a value is not available it will be not set rather than empty.

### Commander Variables

  * {TXT:Name}: the name of the commander
  * {TXT:Home system}: the name of the home system of the commander, set from EDDI configuration
  * {TXT:Home system (spoken)}: the name of the home system of the commander, set from EDDI configuration as would be spoken
  * {TXT:Home station}: the name of the home station of the commander in the home system, set from EDDI configuration
  * {INT:Combat rating}: the combat rating of the commander, with 0 being Harmless and 8 being Elite
  * {TXT:Combat rank}: the combat rank of the commander, from Harmless to Elite
  * {INT:Trade rating}: the trade rating of the commander, with 0 being Penniless and 8 being Elite
  * {TXT:Trade rank}: the trade rank of the commander, from Penniless to Elite
  * {INT:Explore rating}: the exploration rating of the commander, with 0 being Aimless and 8 being Elite
  * {TXT:Explore rank}: the exploration rank of the commander, from Aimless to Elite
  * {INT:Empire rating}: the empire rating of the commander, with 0 being None and 14 being King
  * {TXT:Empire rank}: the empire rating of the commander, from None to King
  * {INT:Federation rating}: the federation rating of the commander, with 0 being None and 14 being Admiral
  * {TXT:Federation rank}: the federation rating of the commander, from None to Admiral
  * {DEC:Credits}: the number of credits owned by the commander
  * {TXT:Credits (spoken)}: the number of credits owned by the commander as would be spoken (e.g. "just over 2 million")
  * {DEC:Debt}: the number of credits owed by the commander
  * {TXT:Debt}: the number of credits owed by the commander as would be spoken (e.g. "a little under 100 thousand")
  * {DEC:Insurance}: the percentage insurance excess for the commander (usually 5, 3.75 or 2.5)

### Ship Variables

  * {TXT:Ship model}: the model of the ship (e.g. "Cobra Mk", "Fer-de-Lance")
  * {TXT:Ship model (spoken)}: the model of the ship as would be spoken (e.g. "Cobra Mark 4")
  * {TXT:Ship name}: the name of the ship as set in EDDI configuration
  * {TXT:Ship callsign}: the callsign of the ship as shown in EDDI configuration (e.g. "GEF-1020")
  * {TXT:Ship callsign (spoken)}: the callsign of the ship as shown in EDDI configuration as would be spoken
  * {TXT:Ship role}: the role of the ship as set in EDDI configuration (Multipurpose, Combat, Exploration, Trading, Mining, Smuggling)
  * {TXT:Ship size}: the size of the ship (Small, Medium, or Large)
  * {DEC:Ship value}: the replacement cost of the ship plus modules
  * {TXT:Ship value (spoken)}: the replacement cost of the ship plus modules as would be spoken
  * {INT:Ship cargo capacity}: the maximum cargo capacity of the ship as currently configured
  * {INT:Ship cargo carried}: the cargo currently being carried by the ship
  * {INT:Ship limpets carried}: the number of limpets currently being carried by the ship
  * {DEC:Ship health}: the percentage health of the ship's hull
  * {TXT:Ship bulkheads}: the type of bulkheads fitted to the ship (e.g. "Military Grade Composite")
  * {TXT:Ship bulkheads class}: the class of bulkheads fitted to the ship (e.g. 3)
  * {TXT:Ship bulkheads grade}: the grade of bulkheads fitted to the ship (e.g. "A")
  * {DEC:Ship bulkheads health}: the percentage health of the bulkheads fitted to the ship
  * {DEC:Ship bulkheads cost}: the purchase cost of the bulkheads
  * {DEC:Ship bulkheads value}: the undiscounted cost of the bulkheads
  * {DEC:Ship bulkheads discount}: the percentage discount of the purchased bulkheads against the undiscounted cost
  * {TXT:Ship power plant}: the name of power plant fitted to the ship
  * {TXT:Ship power plant class}: the class of bulkheads fitted to the ship (e.g. 3)
  * {TXT:Ship power plant grade}: the grade of bulkheads fitted to the ship (e.g. "A")
  * {DEC:Ship power plant health}: the percentage health of the power plant fitted to the ship
  * {DEC:Ship power plant cost}: the purchase cost of the power plant
  * {DEC:Ship power plant value}: the undiscounted cost of the power plant
  * {DEC:Ship power plant discount}: the percentage discount of the purchased power plant against the undiscounted cost
  * {TXT:Ship thrusters}: the name of thrusters fitted to the ship
  * {TXT:Ship thrusters class}: the class of thrusters fitted to the ship (e.g. 3)
  * {TXT:Ship thrusters grade}: the grade of thrusters fitted to the ship (e.g. "A")
  * {DEC:Ship thrusters health}: the percentage health of the thrusters fitted to the ship
  * {DEC:Ship thrusters cost}: the purchase cost of the thrusters
  * {DEC:Ship thrusters value}: the undiscounted cost of the thrusters
  * {DEC:Ship thrusters discount}: the percentage discount of the purchased thrusters against the undiscounted cost
  * {TXT:Ship frame shift drive}: the name of frame shift drive fitted to the ship
  * {TXT:Ship frame shift drive class}: the class of frame shift drive fitted to the ship (e.g. 3)
  * {TXT:Ship frame shift drive grade}: the grade of frame shift drive fitted to the ship (e.g. "A")
  * {DEC:Ship frame shift drive health}: the percentage health of the frame shift drive fitted to the ship
  * {DEC:Ship frame shift drive cost}: the purchase cost of the frame shift drive
  * {DEC:Ship frame shift drive value}: the undiscounted cost of the frame shift drive
  * {DEC:Ship frame shift drive discount}: the percentage discount of the purchased frame shift drive against the undiscounted cost
  * {TXT:Ship life support}: the name of life support fitted to the ship (e.g. "6D")
  * {TXT:Ship life support class}: the class of life support fitted to the ship (e.g. 3)
  * {TXT:Ship life support grade}: the grade of life support fitted to the ship (e.g. "A")
  * {DEC:Ship life support health}: the percentage health of the life support fitted to the ship
  * {DEC:Ship life support cost}: the purchase cost of the life support
  * {DEC:Ship life support value}: the undiscounted cost of the life support
  * {DEC:Ship life support discount}: the percentage discount of the purchased life support against the undiscounted cost
  * {TXT:Ship power distributor}: the name of power distributor fitted to the ship
  * {TXT:Ship power distributor class}: the class of power distributor fitted to the ship (e.g. 3)
  * {TXT:Ship power distributor drive grade}: the grade of power distributor fitted to the ship (e.g. "A")
  * {DEC:Ship power distributor health}: the percentage health of the power distributor fitted to the ship
  * {DEC:Ship power distributor cost}: the purchase cost of the power distributor
  * {DEC:Ship power distributor value}: the undiscounted cost of the power distributor
  * {DEC:Ship power distributor discount}: the percentage discount of the purchased power distributor against the undiscounted cost
  * {TXT:Ship sensors}: the name of sensors fitted to the ship
  * {TXT:Ship sensors class}: the class of sensors fitted to the ship (e.g. 3)
  * {TXT:Ship sensors drive grade}: the grade of sensors fitted to the ship (e.g. "A")
  * {DEC:Ship sensors health}: the percentage health of the sensors fitted to the ship
  * {DEC:Ship sensors cost}: the purchase cost of the sensors
  * {DEC:Ship sensors value}: the undiscounted cost of the sensors
  * {DEC:Ship sensors discount}: the percentage discount of the purchased sensors against the undiscounted cost
  * {TXT:Ship fuel tank}: the name of the main fuel tank fitted to the ship
  * {TXT:Ship fuel tank class}: the class of the main fuel tank fitted to the ship (e.g. 3)
  * {TXT:Ship fuel tank drive grade}: the grade of the main fuel tank fitted to the ship (e.g. "A")
  * {DEC:Ship fuel tank cost}: the purchase cost of the the main fuel tank
  * {DEC:Ship fuel tank value}: the undiscounted cost of the the main fuel tank
  * {DEC:Ship fuel tank discount}: the percentage discount of the purchased main fuel tank against the undiscounted cost
  * {DEC:Ship fuel tank capacity}: the capacity of the main fuel tank
  * {DEC:Ship total fuel tank capacity}: the capacity of the main fuel tank plus all additional fuel tanks
  * {BOOL:Ship tiny/small/medium/large/huge hardpoint *n* occupied}: true if there is a module in this slot, otherwise false
  * {TXT:Ship tiny/small/medium/large/huge hardpoint *n* module}: the name of the module in this slot
  * {INT:Ship tiny/small/medium/large/huge hardpoint *n* module class}: the class of the module in this slot
  * {TXT:Ship tiny/small/medium/large/huge hardpoint *n* module grade}: the grade of the module in this slot
  * {DEC:Ship tiny/small/medium/large/huge hardpoint *n* module health}: the percentage health of the module in this slot
  * {DEC:Ship tiny/small/medium/large/huge hardpoint *n* module cost}: the purchase cost of the module in this slot
  * {DEC:Ship tiny/small/medium/large/huge hardpoint *n* module value}: the undiscounted cost of the module in this slot
  * {DEC:Ship tiny/small/medium/large/huge hardpoint *n* module discount}: the percentage discount of the purchased module against the undiscounted cost
  * {INT:Ship Compartment *n* size}: the size of this slot
  * {BOOL:Ship Compartment *n* occupied}: true if there is a module in this slot, otherwise false
  * {TXT:Ship compartment *n* module}: the name of the module in this slot
  * {INT:Ship compartment *n* module class}: the class of the module in this slot
  * {TXT:Ship compartment *n* module grade}: the grade of the module in this slot
  * {DEC:Ship compartment *n* module health}: the percentage health of the module in this slot
  * {DEC:Ship compartment *n* module cost}: the purchase cost of the module in this slot
  * {DEC:Ship compartment *n* module value}: the undiscounted cost of the module in this slot
  * {DEC:Ship compartment *n* module station cost}: the purchase cost of the module at this station

### Current System Variables

  * {TXT:System name}: the name of the system
  * {TXT:System name (spoken)}: the name of the system as would be spoken
  * {DEC:System distance from home}: the number of lights years between this system and the your home system, to two decimal places
  * {INT:System visits}: the number of times the commander has visited the system (whilst the plugin has been active)
  * {DATETIME:System previous visit}: the last time the commander visited the system (empty if this is their first visit)
  * {INT:System minutes since previous visit}: the number of minutes since the commander's last visit to the system
  * {DEC:System population}: the population of the system
  * {TXT:System population}: the population of the system as would be spoken (e.g. "nearly 12 and a half billion")
  * {TXT:System allegiance}: the allegiance of the system ("Federation", "Empire", "Alliance", "Independant" or empty)
  * {TXT:System government}: the government of the system (e.g. "Democracy")
  * {TXT:System faction}: the primary faction of the system (e.g. "The Pilots' Federation")
  * {TXT:System primary economy}: the primary economy of the system (e.g. "Industrial")
  * {TXT:System state}: the overall state of the system (e.g. "Boom")
  * {TXT:System security}: the security level in the system ("High", "Medium", "Low", "None" or empty)
  * {TXT:System power}: the name of the power that controls the system (e.g. "Aisling Duval")
  * {TXT:System power (spoken)}: the name of the power that controls the system as would be spoken (e.g. "Ashling Du-val")
  * {TXT:System power state}: the state of the power in the system (e.g. "Expansion")
  * {TXT:System rank}: the rank of the Commander in the system (e.g. "Duke" if an Empire system, "Admiral" if a Federation system)
  * {DEC:System X} the X co-ordinate of the system
  * {DEC:System Y} the Y co-ordinate of the system
  * {DEC:System Z} the Z co-ordinate of the system
  * {INT:System stations}: the total number of stations, both in orbit and on planets, in the system
  * {INT:System orbital stations}: the number of orbital stations in the system
  * {INT:System starports}: the total number of orbital starports in the system
  * {INT:System outposts}: the total number of orbital outposts in the system
  * {INT:System planetary stations}: the total number of planetary stations (outposts and ports) in the system
  * {INT:System planetary outposts}: the total number of planetary outposts in the system
  * {INT:System planetary ports}: the total number of planetary ports in the system
  * {TXT:System main star stellar class}: the stellar class of the main star of the system (M, G etc)
  * {INT:System main star age}: the age of the main star of the system, in millions of years

### Last System Variables

  * {TXT:Last system name}: the name of the last system
  * {TXT:Last system name (spoken)}: the name of the last system as would be spoken
  * {INT:Last system visits}: the number of times the commander has visited the last system (whilst the plugin has been active)
  * {DATETIME:Last system previous visit}: the last time the commander visited the last system (empty if this is their first visit)
  * {DEC:Last system population}: the population of the last system
  * {TXT:Last system population}: the population of the last system as would be spoken (e.g. "nearly 12 and a half billion")
  * {TXT:Last system allegiance}: the allegiance of the last system ("Federation", "Empire", "Alliance", "Independant" or empty)
  * {TXT:Last system government}: the government of the last system (e.g. "Democracy")
  * {TXT:Last system faction}: the primary faction of the last system (e.g. "The Pilots' Federation")
  * {TXT:Last system primary economy}: the primary economy of the last system (e.g. "Industrial")
  * {TXT:Last system state}: the overall state of the last system (e.g. "Boom")
  * {TXT:Last system security}: the security level in the last system ("High", "Medium", "Low", "None" or empty)
  * {TXT:Last system power}: the name of the power that controls the last system (e.g. "Felicia Winters")
  * {TXT:Last system power (spoken)}: the name of the power that controls the last system as would be spoken
  * {TXT:Last system power state}: the state of the power in the last system (e.g. "Expansion")
  * {TXT:Last system rank}: the rank of the Commander in the last system (e.g. "Duke" if an Empire system, "Admiral" if a Federation system)
  * {DEC:Last system X} the X co-ordinate of the last system
  * {DEC:Last system Y} the Y co-ordinate of the last system
  * {DEC:Last system Z} the Z co-ordinate of the last system

### Current Station Variables

  * {DEC:Ship bulkheads station cost}: the purchase cost of the bulkheads at the station (not set if not for sale at the station)
  * {DEC:Ship bulkheads station discount}: the number of credits discount of the bulkheads over those currently fitted (not set if no additional discount)
  * {TXT:Ship bulkheads station discount (spoken)}: the number of credits discount of the bulkheads over those currently fitted as would be spoken (not set if no additional discount)
  * {DEC:Ship power plant station cost}: the purchase cost of the power plant at the station (not set if not for sale at the station)
  * {DEC:Ship power plant station discount}: the number of credits discount of the power plant over that currently fitted (not set if no additional discount)
  * {TXT:Ship power plant station discount (spoken)}: the number of credits discount of the power plant over thothatse currently fitted as would be spoken (not set if no additional discount)
  * {DEC:Ship thrusters station cost}: the purchase cost of the thrusters at the station (not set if not for sale at the station)
  * {DEC:Ship thrusters station discount}: the number of credits discount of the thrusters over those currently fitted (not set if no additional discount)
  * {TXT:Ship thrusters station discount (spoken)}: the number of credits discount of the thrusters over those currently fitted as would be spoken (not set if no additional discount)
  * {DEC:Ship frame shift drive station cost}: the purchase cost of the frame shift drive at the station (not set if not for sale at the station)
  * {DEC:Ship frame shift drive station discount}: the number of credits discount of the frame shift drive over those currently fitted (not set if no additional discount)
  * {TXT:Ship frame shift drive station discount (spoken)}: the number of credits discount of the frame shift drive over those currently fitted as would be spoken (not set if no additional discount)
  * {DEC:Ship life support station cost}: the purchase cost of the life support at the station (not set if not for sale at the station)
  * {DEC:Ship life support station discount}: the number of credits discount of the life support over that currently fitted (not set if no additional discount)
  * {TXT:Ship life support station discount (spoken)}: the number of credits discount of the life support over those currently fitted as would be spoken (not set if no additional discount)
  * {DEC:Ship power distributor station cost}: the purchase cost of the power distributor at the station (not set if not for sale at the station)
  * {DEC:Ship power distributor station discount}: the number of credits discount of the power distributor over those currently fitted (not set if no additional discount)
  * {TXT:Ship power distributor station discount (spoken)}: the number of credits discount of the power distributor over those currently fitted as would be spoken (not set if no additional discount)
  * {DEC:Ship sensors station cost}: the purchase cost of the sensors at the station (not set if not for sale at the station)
  * {DEC:Ship sensors station discount}: the number of credits discount of the sensors over those currently fitted (not set if no additional discount)
  * {TXT:Ship sensors station discount (spoken)}: the number of credits discount of the sensors over those currently fitted as would be spoken (not set if no additional discount)
  * {DEC:Ship tiny/small/medium/large/huge hardpoint *n* module station cost}: the purchase cost of this module at this station (not set if not for sale at the station)
  * {DEC:Ship tiny/small/medium/large/huge hardpoint *n* module station discount}: the number of credits discount of the module over that currently fitted (not set if no additional discount)
  * {TXT:Ship tiny/small/medium/large/huge hardpoint *n* module station discount (spoken)}: the number of credits discount of the module over that currently fitted as would be spoken (not set if no additional discount)
  * {DEC:Ship compartment *n* module station cost}: the purchase cost of this module at this station (not set if not for sale at the station)
  * {DEC:Ship compartment *n* module station discount}: the number of credits discount of the module over that currently fitted (not set if no additional discount)
  * {TXT:Ship compartment *n* module station discount (spoken)}: the number of credits discount of the module over that currently fitted as would be spoken (not set if no additional discount)
  * {TXT:Last station name}: the name of the last station the commander docked at
  * {TXT:Last station faction}: the name of the controlling faction of the last station
  * {TXT:Last station government}: the name of the government of the last station
  * {TXT:Last station allegiance}: the name of the allegiance of the last station (Federation, Empire, etc.)
  * {TXT:Last station state}: the name of the state of the last station (boom, outbreak, etc.)
  * {DEC:Last station distance from star}: the distance from the primary star to this station, in light seconds
  * {TXT:Last station primary economy}: the primary economy of this station (extraction, prison colony, etc.)
  * {BOOL:Last station has refuel}: true if this station has refuel capability
  * {BOOL:Last station has rearm}: true if this station has rearm capability
  * {BOOL:Last station has repair}: true if this station has repair capability
  * {BOOL:Last station has market}: true if this station has a commodities market
  * {BOOL:Last station has black market}: true if this station has a black market
  * {BOOL:Last station has outfitting}: true if this station has outfitting
  * {BOOL:Last station has shipyard}: true if this station has a shipyard

### Shipyard Variables

  * {INT:Stored ship entries}: the number of ships in storage
  * {TXT:Stored ship *n* model}: the model of the *n*th stored ship
  * {TXT:Stored ship *n* name}: the name of the *n*th stored ship as set in EDDI configuration
  * {TXT:Stored ship *n* callsign}: the callsign of the *n*th stored ship as shown in EDDI configuration (e.g. "GEF-1020")
  * {TXT:Stored ship *n* callsign (spoken)}: the callsign of the *n*th stored ship as shown in EDDI configuration as would be spoken
  * {TXT:{TXT:Stored ship *n* role}: the role of the *n*th stored ship as set in EDDI configuration (Multipurpose, Combat, Trade, Exploration, Smuggling)
  * {TXT:{TXT:Stored ship *n* station}: the station in which the *n*th stored ship resides
  * {TXT:Stored ship *n* system}: the system in which the *n*th stored ship resides
  * {DEC:Stored ship *n* distance}: the number of light years between the current system and that where the *n*th ship resiedes, to two decimal places

### Miscellaneous Variables

  * {TXT:Environment}: the environment the ship is in ("Normal space", "Supercruise" or "Witch space")
  * {TXT:Vehicle}: the vehicle the commander is currently controlling ("Ship", "SRV" or "Fighter")

EDDI also provides a number of pre-built commands to show off some of what it is capable of.  These include:

  * a voice command spoken by the pilot before they launch ("run pre flight checks") that carries out a check of areas such as insurance, repairs /etc./
  * a voice command spoken by the pilot when they wish to see their ship in https://coriolis.edcd.io/ ("Display my ship in coriolis")
  * a voice command spoken by the pilot when they wish to see their current system in https://www.eddb.io/ ("Show this system in EDDB")
  * a voice command spoken by the pilot when they wish to see their current station in https://www.eddb.io/ ("Show this station in EDDB")

To access these commands, as well as to obtain a number of commands that display the values of the variables listed above, import the "EDDI" VoiceAttack from the EDDI directory in to VoiceAttack.  To access these commands edit your own profile and set "Include commands from another profile" to "EDDI".

Note that if you are running in VR, want to display your information on another tablet, or for some other reason do not want the functions listed above to open a web browser directly on your screen you can set the boolean value "EDDI use clipboard" and rather than (for example) showing your ship in Coriolis EDDI will construct a suitable URL and place it on the clipboard.

## Functions

EDDI's VoiceAttack plugin allows you to access its features in your own profile.  Details of these functions are laid out below.

### say

This function uses EDDI's voice to read a script.  The script should be a text variable with the name 'Script'.

If you want to use a different voice to the standard one then you can set the name of the voice you want to use in a text variable with the name 'Voice'.  Note that when you set this variable it will continue to be used until you unset it, at which point EDDI will use the voice configured in its text-to-speech settings.

To use this function in your own commands use the 'Execute an external plugin function' command with the plugin context set to 'say'.

### speech

This function uses EDDI's voice to read a Speech Responder script.  The name of the script should be a text variable with the name 'Script'.

If you want to use a different voice to the standard one then you can set the name of the voice you want to use in a text variable with the name 'Voice'.  Note that when you set this variable it will continue to be used until you unset it, at which point EDDI will use the voice configured in its text-to-speech settings.

To use this function in your own commands use the 'Execute an external plugin function' command with the plugin context set to 'speech'.

### shutup

This function stops any active EDDI speech.

To use this function in your own commands use the 'Execute an external plugin function' command with the plugin context set to 'shutup'.

### disablespeechresponder

This function disables the speech responder until either VoiceAttack is restarted or an enablespeechresponder call is made.

To use this function in your own commands use the 'Execute an external plugin function' command with the plugin context set to 'disablespeechresponder'.

### enablespeechresponder

This function enables the speech responder until either VoiceAttack is restarted or a disablespeechresponder call is made.

To use this function in your own commands use the 'Execute an external plugin function' command with the plugin context set to 'enablespeechresponder'.

### setspeechresponderpersonality

This function changes the speech responder's personality.  The name of the personality should be a text variable with the name 'Personality'.

Note that unlike enablespeechresponder and disablespeechresponder any changes made here are persistent.

To use this function in your own commands use the 'Execute an external plugin function' command with the plugin context set to 'setspeechresponderpersonality'.

### profile

This function obtains the latest information from the Elite servers.  It will give you up-to-date information on your ship (including its loadout and cargo), the station you are docked at, and your credit balance and rankings.

To use this function in your own commands use the 'Execute an external plugin function' command with the plugin context set to 'profile'.

Please note that you should be careful when using this function.  EDDI internally restricts you to calling it once per minute, but even then this should not be used too frequently as it puts a load on Frontier's servers.

Also note that the information returned by this function is not guaranteed to be totally up-to-date, because the remote server can take a few seconds to sync its information with the changes that have taken place.  If you do want to use this command then it is recommended that in your script you do something like say "Obtaining information", waiting for a couple of seconds, then calling the profile command.  This means that if you call this directly after carrying out an operation (for example purchasing cargo) you are more likely to have the full information available to you in the rest of the script.

### setstate

This function pushes a state variable to EDDI's internal session state, allowing it to be shared with other responders.

To use this function in your own commands use the 'Execute an external plugin function' command with the plugin context set to 'setstate'.  This function will read the text variable "State variable" and store the VoiceAttack variable named in there as a state variable.  For example, if you wanted to store the VoiceAttack boolean variable "Verbose" as a state variable you would:

    * set the text variable "State variable" to "Verbose"
    * call EDDI with the context set to "setstate"

This function only supports integers, booleans, decimals and strings as state values.  The name of the value will be altered if necessary to ensure that it is all lower-case, and that spaces are replace by underscores.  For example, if you attempt to store a state variable "My variable" it will be stored as "my_variable".

State variables are made available in VoiceAttack with the prefix 'EDDI state'.  For example, to access the text variable stored in the last paragraph you would use '{TXT:EDDI state my_variable}'.

Please note that state is transient, and is purposefully not persisted beyond the running instance of EDDI.  This means that every time you start VoiceAttack the state will be empty.  Also, because EDDI responders run asynchronously and concurrently there is no guarantee that, for example, the speech responder for an event will finish before the VoiceAttack responder for an event starts (or vice versa).

## Events

Whenever EDDI sees a particular event occur it will attempt to run a script.  The name of the script depends on the event, but follows the form:

    ((EDDI <event>))

with the <event> being in lower-case.  For example, if you wanted VoiceAttack to run a script every time you docked you would create a script called `((EDDI docked))` (note the lower-case d at the beginning of docked).

There are a large number of events available.  Full details of them and the variables that are set for each of them are as below.  Note that event variables are only valid when the event occurs, and cannot be relied upon to be present or a specific value at any other time.  If you want to use information in an event after the event itself then you should copy the value to another variable.

### Body scanned
Triggered when you complete a scan of a planetary body.
To run a command when this event occurs you should create the command with the name ((EDDI body scanned))

Variables set with this event are as follows:

  * {TXT:EDDI body scanned atmosphere} The atmosphere of the body that has been scanned
  * {TXT:EDDI body scanned bodyclass} The class of the body that has been scanned (High metal content body etc)
  * {DEC:EDDI body scanned distancefromarrival} The distance in LS from the main star
  * {DEC:EDDI body scanned eccentricity} 
  * {DEC:EDDI body scanned gravity} The surface gravity of the body that has been scanned, relative to Earth's gravity
  * {BOOL:EDDI body scanned landable} True if the body is landable
  * {TXT:EDDI body scanned name} The name of the body that has been scanned
  * {DEC:EDDI body scanned orbitalinclination} 
  * {DEC:EDDI body scanned orbitalperiod} The number of seconds taken for a full orbit of the main star
  * {DEC:EDDI body scanned periapsis} 
  * {DEC:EDDI body scanned pressure} The surface pressure of the body that has been scanned
  * {DEC:EDDI body scanned rotationperiod} The number of seconds taken for a full rotation
  * {DEC:EDDI body scanned semimajoraxis} 
  * {DEC:EDDI body scanned temperature} The surface temperature of the body that has been scanned
  * {TXT:EDDI body scanned terraformstate} Whether the body can be, is in the process of, or has been terraformed
  * {BOOL:EDDI body scanned tidallylocked} True if the body is tidally locked
  * {TXT:EDDI body scanned volcanism} The volcanism of the body that has been scanned

### Bond awarded
Triggered when you are awarded a combat bond.
To run a command when this event occurs you should create the command with the name ((EDDI bond awarded))

Variables set with this event are as follows:

  * {TXT:EDDI bond awarded awardingfaction} The name of the faction awarding the bond
  * {DEC:EDDI bond awarded reward} The number of credits received
  * {TXT:EDDI bond awarded victimfaction} The name of the faction whose ship you destroyed

### Bounty awarded
Triggered when you are awarded a bounty.
To run a command when this event occurs you should create the command with the name ((EDDI bounty awarded))

Variables set with this event are as follows:

  * {TXT:EDDI bounty awarded faction} The name of the faction whose ship you destroyed
  * {DEC:EDDI bounty awarded reward} The total number of credits obtained for destroying the ship
  * {BOOL:EDDI bounty awarded shared} True if the rewards have been shared with wing-mates
  * {TXT:EDDI bounty awarded target} The name of the pilot you destroyed

### Bounty incurred
Triggered when you incur a bounty.
To run a command when this event occurs you should create the command with the name ((EDDI bounty incurred))

Variables set with this event are as follows:

  * {DEC:EDDI bounty incurred bounty} The number of credits issued as the bounty
  * {TXT:EDDI bounty incurred crimetype} The type of crime committed
  * {TXT:EDDI bounty incurred faction} The name of the faction issuing the bounty
  * {TXT:EDDI bounty incurred victim} The name of the victim of the crime

### Cleared save
Triggered when you clear your save.
To run a command when this event occurs you should create the command with the name ((EDDI cleared save))

Variables set with this event are as follows:

  * {TXT:EDDI cleared save name} The name of the player whose save has been cleared

### Cockpit breached
Triggered when your ship's cockpit is broken.
To run a command when this event occurs you should create the command with the name ((EDDI cockpit breached))

### Combat promotion
Triggered when your combat rank increases.
To run a command when this event occurs you should create the command with the name ((EDDI combat promotion))

Variables set with this event are as follows:

  * {TXT:EDDI combat promotion rating} The commander's new combat rating

### Commander continued
Triggered when you continue an existing game.
To run a command when this event occurs you should create the command with the name ((EDDI commander continued))

Variables set with this event are as follows:

  * {TXT:EDDI commander continued commander} The commander's name
  * {DEC:EDDI commander continued credits} the number of credits the commander has
  * {TXT:EDDI commander continued group} The name of the group (only if mode == Group)
  * {TXT:EDDI commander continued mode} The game mode (Open, Group or Solo)
  * {TXT:EDDI commander continued ship} The commander's ship
  * {INT:EDDI commander continued shipid} The ID of the commander's ship

### Commander progress
Triggered when your progress is reported.
To run a command when this event occurs you should create the command with the name ((EDDI commander progress))

Variables set with this event are as follows:

  * {DEC:EDDI commander progress combat} The percentage progress of the commander's combat rating
  * {DEC:EDDI commander progress cqc} The percentage progress of the commander's CQC rating
  * {DEC:EDDI commander progress empire} The percentage progress of the commander's empire rating
  * {DEC:EDDI commander progress exploration} The percentage progress of the commander's exploration rating
  * {DEC:EDDI commander progress federation} The percentage progress of the commander's federation rating
  * {DEC:EDDI commander progress trade} The percentage progress of the commander's trade rating

### Commander ratings
Triggered when your ratings are reported.
To run a command when this event occurs you should create the command with the name ((EDDI commander ratings))

Variables set with this event are as follows:


### Commander started
Triggered when you start a new game.
To run a command when this event occurs you should create the command with the name ((EDDI commander started))

Variables set with this event are as follows:

  * {TXT:EDDI commander started name} The name of the new commander
  * {TXT:EDDI commander started package} The starting package of the new commander

### Commodity collected
Triggered when you pick up a commodity in your ship or SRV.
To run a command when this event occurs you should create the command with the name ((EDDI commodity collected))

Variables set with this event are as follows:

  * {TXT:EDDI commodity collected commodity} The name of the commodity collected
  * {BOOL:EDDI commodity collected stolen} If the cargo is stolen

### Commodity ejected
Triggered when you eject a commodity from your ship or SRV.
To run a command when this event occurs you should create the command with the name ((EDDI commodity ejected))

Variables set with this event are as follows:

  * {BOOL:EDDI commodity ejected abandoned} If the cargo has been abandoned
  * {INT:EDDI commodity ejected amount} The amount of cargo ejected
  * {TXT:EDDI commodity ejected commodity} The name of the commodity ejected

### Commodity purchased
Triggered when you buy a commodity from the markets.
To run a command when this event occurs you should create the command with the name ((EDDI commodity purchased))

Variables set with this event are as follows:

  * {INT:EDDI commodity purchased amount} The amount of the purchased commodity
  * {TXT:EDDI commodity purchased commodity} The name of the purchased commodity
  * {DEC:EDDI commodity purchased price} The price paid per unit of the purchased commodity

### Commodity refined
Triggered when you refine a commodity from the refinery.
To run a command when this event occurs you should create the command with the name ((EDDI commodity refined))

Variables set with this event are as follows:

  * {TXT:EDDI commodity refined commodity} The name of the commodity refined

### Commodity sold
Triggered when you sell a commodity to the markets.
To run a command when this event occurs you should create the command with the name ((EDDI commodity sold))

Variables set with this event are as follows:

  * {INT:EDDI commodity sold amount} The amount of the commodity sold
  * {BOOL:EDDI commodity sold blackmarket} True if the commodity was sold to a black market
  * {TXT:EDDI commodity sold commodity} The name of the commodity sold
  * {BOOL:EDDI commodity sold illegal} True if the commodity is illegal at the place of sale
  * {DEC:EDDI commodity sold price} The price obtained per unit of the commodity sold
  * {DEC:EDDI commodity sold profit} The number of credits profit per unit of the commodity sold
  * {BOOL:EDDI commodity sold stolen} True if the commodity was stolen

### Controlling fighter
Triggered when you switch control from your ship to your fighter.
To run a command when this event occurs you should create the command with the name ((EDDI controlling fighter))

### Controlling ship
Triggered when you switch control from your fighter to your ship.
To run a command when this event occurs you should create the command with the name ((EDDI controlling ship))

### Crew assigned
Triggered when you assign crew.
To run a command when this event occurs you should create the command with the name ((EDDI crew assigned))

Variables set with this event are as follows:

  * {TXT:EDDI crew assigned name} The name of the crewmember being assigned
  * {TXT:EDDI crew assigned role} The role to which the crewmember is being assigned

### Crew fired
Triggered when you fire crew.
To run a command when this event occurs you should create the command with the name ((EDDI crew fired))

Variables set with this event are as follows:

  * {TXT:EDDI crew fired name} The name of the crewmember being fired

### Crew hired
Triggered when you hire crew.
To run a command when this event occurs you should create the command with the name ((EDDI crew hired))

Variables set with this event are as follows:

  * {TXT:EDDI crew hired combatrating} The combat rating of the crewmember being hired
  * {TXT:EDDI crew hired faction} The faction of the crewmember being hired
  * {TXT:EDDI crew hired name} The name of the crewmember being hired
  * {DEC:EDDI crew hired price} The price of the crewmember being hired

### Died
Triggered when you have died.
To run a command when this event occurs you should create the command with the name ((EDDI died))

Variables set with this event are as follows:


### Docked
Triggered when your ship docks at a station or outpost.
To run a command when this event occurs you should create the command with the name ((EDDI docked))

Variables set with this event are as follows:

  * {TXT:EDDI docked economy} The economy of the station at which the commander has docked
  * {TXT:EDDI docked faction} The faction controlling the station at which the commander has docked
  * {TXT:EDDI docked factionstate} The state of the faction controlling the station at which the commander has docked
  * {TXT:EDDI docked government} The government of the station at which the commander has docked
  * {TXT:EDDI docked model} The model of the station at which the commander has docked (Orbis, Coriolis, etc)
  * {TXT:EDDI docked station} The station at which the commander has docked
  * {TXT:EDDI docked system} The system at which the commander has docked

### Docking cancelled
Triggered when your ship cancels a docking request at a station or outpost.
To run a command when this event occurs you should create the command with the name ((EDDI docking cancelled))

Variables set with this event are as follows:

  * {TXT:EDDI docking cancelled station} The station at which the commander has cancelled docking

### Docking denied
Triggered when your ship is denied docking at a station or outpost.
To run a command when this event occurs you should create the command with the name ((EDDI docking denied))

Variables set with this event are as follows:

  * {TXT:EDDI docking denied reason} The reason why commander has been denied docking (too far, fighter deployed etc)
  * {TXT:EDDI docking denied station} The station at which the commander has been denied docking

### Docking granted
Triggered when your ship is granted docking permission at a station or outpost.
To run a command when this event occurs you should create the command with the name ((EDDI docking granted))

Variables set with this event are as follows:

  * {INT:EDDI docking granted landingpad} The landing apd at which the commander has been granted docking
  * {TXT:EDDI docking granted station} The station at which the commander has been granted docking

### Docking requested
Triggered when your ship requests docking at a station or outpost.
To run a command when this event occurs you should create the command with the name ((EDDI docking requested))

Variables set with this event are as follows:

  * {TXT:EDDI docking requested station} The station at which the commander has requested docking

### Docking timed out
Triggered when your docking request times out.
To run a command when this event occurs you should create the command with the name ((EDDI docking timed out))

Variables set with this event are as follows:

  * {TXT:EDDI docking timed out station} The station at which the docking request has timed out

### Engineer progressed
Triggered when you reach a new rank with an engineer.
To run a command when this event occurs you should create the command with the name ((EDDI engineer progressed))

Variables set with this event are as follows:

  * {TXT:EDDI engineer progressed engineer} The name of the engineer with whom you have progressed
  * {INT:EDDI engineer progressed rank} The rank of your relationship with the engineer

### Entered CQC
Triggered when you enter CQC.
To run a command when this event occurs you should create the command with the name ((EDDI entered cqc))

Variables set with this event are as follows:

  * {TXT:EDDI entered cqc commander} The commander's name

### Entered normal space
Triggered when your ship enters normal space.
To run a command when this event occurs you should create the command with the name ((EDDI entered normal space))

Variables set with this event are as follows:

  * {TXT:EDDI entered normal space body} The nearest body to the commander when entering normal space
  * {TXT:EDDI entered normal space bodytype} The type of the nearest body to the commander when entering normal space
  * {TXT:EDDI entered normal space system} The system at which the commander has entered normal space

### Entered signal source
Triggered when your ship enters a signal source.
To run a command when this event occurs you should create the command with the name ((EDDI entered signal source))

Variables set with this event are as follows:

  * {TXT:EDDI entered signal source source} The type of the signal source
  * {INT:EDDI entered signal source threat} The threat level of the signal source (0-4)

### Entered supercruise
Triggered when your ship enters supercruise.
To run a command when this event occurs you should create the command with the name ((EDDI entered supercruise))

Variables set with this event are as follows:

  * {TXT:EDDI entered supercruise system} The system at which the commander has entered supercruise

### Exploration data purchased
Triggered when you purchase exploration data.
To run a command when this event occurs you should create the command with the name ((EDDI exploration data purchased))

Variables set with this event are as follows:

  * {DEC:EDDI exploration data purchased price} The price of the purchase
  * {TXT:EDDI exploration data purchased system} The system for which the exploration data was purchased

### Exploration data sold
Triggered when you sell exploration data.
To run a command when this event occurs you should create the command with the name ((EDDI exploration data sold))

Variables set with this event are as follows:

  * {DEC:EDDI exploration data sold bonus} The bonus for first discovereds
  * {DEC:EDDI exploration data sold reward} The reward for selling the exploration data

### Exploration promotion
Triggered when your exploration rank increases.
To run a command when this event occurs you should create the command with the name ((EDDI exploration promotion))

Variables set with this event are as follows:

  * {TXT:EDDI exploration promotion rating} The commander's new exploration rating

### Fighter docked
Triggered when you dock a fighter with your ship.
To run a command when this event occurs you should create the command with the name ((EDDI fighter docked))

### Fighter launched
Triggered when you launch a fighter from your ship.
To run a command when this event occurs you should create the command with the name ((EDDI fighter launched))

Variables set with this event are as follows:

  * {TXT:EDDI fighter launched loadout} The fighter's loadout
  * {BOOL:EDDI fighter launched playercontrolled} True if the fighter is controlled by the player

### Fine incurred
Triggered when your incur a fine.
To run a command when this event occurs you should create the command with the name ((EDDI fine incurred))

Variables set with this event are as follows:

  * {TXT:EDDI fine incurred crimetype} The type of crime committed
  * {TXT:EDDI fine incurred faction} The name of the faction issuing the fine
  * {TXT:EDDI fine incurred victim} The name of the victim of the crime

### Fine paid
Triggered when you pay a fine.
To run a command when this event occurs you should create the command with the name ((EDDI fine paid))

Variables set with this event are as follows:

  * {DEC:EDDI fine paid amount} The amount of the fine paid
  * {BOOL:EDDI fine paid legacy} True if the payment is for a legacy fine

### Heat damage
Triggered when your ship is taking damage from excessive heat.
To run a command when this event occurs you should create the command with the name ((EDDI heat damage))

### Heat warning
Triggered when your ship's heat exceeds 100%.
To run a command when this event occurs you should create the command with the name ((EDDI heat warning))

### Hull damaged
Triggered when your hull is damaged to a certain extent.
To run a command when this event occurs you should create the command with the name ((EDDI hull damaged))

Variables set with this event are as follows:

  * {DEC:EDDI hull damaged health} The percentage health of the hull
  * {BOOL:EDDI hull damaged piloted} True if the vehicle receiving damage is piloted by the player
  * {TXT:EDDI hull damaged vehicle} The vehicle that has been damaged (Ship, SRV, Fighter)

### Jumped
Triggered when you complete a jump to another system.
To run a command when this event occurs you should create the command with the name ((EDDI jumped))

Variables set with this event are as follows:

  * {TXT:EDDI jumped allegiance} The allegiance of the system to which the commander has jumped
  * {DEC:EDDI jumped distance} The distance the commander has jumped, in light years
  * {TXT:EDDI jumped economy} The economy of the system to which the commander has jumped
  * {TXT:EDDI jumped faction} The faction controlling the system to which the commander has jumped
  * {TXT:EDDI jumped factionstate} The state of the faction controlling the system to which the commander has jumped
  * {DEC:EDDI jumped fuelremaining} The amount of fuel remaining after this jump
  * {DEC:EDDI jumped fuelused} The amount of fuel used in this jump
  * {TXT:EDDI jumped government} The government of the system to which the commander has jumped
  * {TXT:EDDI jumped security} The security of the system to which the commander has jumped
  * {TXT:EDDI jumped system} The name of the system to which the commander has jumped
  * {DEC:EDDI jumped x} The X co-ordinate of the system to which the commander has jumped
  * {DEC:EDDI jumped y} The Y co-ordinate of the system to which the commander has jumped
  * {DEC:EDDI jumped z} The Z co-ordinate of the system to which the commander has jumped

### Jumping
Triggered when you start a jump to another system.
To run a command when this event occurs you should create the command with the name ((EDDI jumping))

Variables set with this event are as follows:

  * {TXT:EDDI jumping system} The name of the system to which the commander is jumping
  * {DEC:EDDI jumping x} The X co-ordinate of the system to which the commander is jumping
  * {DEC:EDDI jumping y} The Y co-ordinate of the system to which the commander is jumping
  * {DEC:EDDI jumping z} The Z co-ordinate of the system to which the commander is jumping

### Killed
Triggered when you kill another player.
To run a command when this event occurs you should create the command with the name ((EDDI killed))

Variables set with this event are as follows:

  * {TXT:EDDI killed rating} The combat rating of the player killed
  * {TXT:EDDI killed victim} The name of the player killed

### Liftoff
Triggered when your ship lifts off from a planet's surface.
To run a command when this event occurs you should create the command with the name ((EDDI liftoff))

Variables set with this event are as follows:

  * {DEC:EDDI liftoff latitude} The latitude from where the commander has lifted off
  * {DEC:EDDI liftoff longitude} The longitude from where the commander has lifted off

### Limpet purchased
Triggered when you buy limpets from a station.
To run a command when this event occurs you should create the command with the name ((EDDI limpet purchased))

Variables set with this event are as follows:

  * {INT:EDDI limpet purchased amount} The amount of limpets purchased
  * {DEC:EDDI limpet purchased price} The price paid per limpet

### Limpet sold
Triggered when you sell limpets to a station.
To run a command when this event occurs you should create the command with the name ((EDDI limpet sold))

Variables set with this event are as follows:

  * {INT:EDDI limpet sold amount} The amount of limpets sold
  * {DEC:EDDI limpet sold price} The price obtained per limpet

### Location
Triggered when the commander's location is reported, usually when they reload their game..
To run a command when this event occurs you should create the command with the name ((EDDI location))

Variables set with this event are as follows:

  * {TXT:EDDI location allegiance} The allegiance of the system in which the commander resides
  * {TXT:EDDI location body} The nearest body to the commander
  * {TXT:EDDI location bodytype} The type of the nearest body to the commander
  * {BOOL:EDDI location docked} True if the commander is docked
  * {TXT:EDDI location economy} The economy of the system in which the commander resides
  * {TXT:EDDI location faction} The faction controlling the system in which the commander resides
  * {TXT:EDDI location government} The government of the system in which the commander resides
  * {TXT:EDDI location security} The security of the system in which the commander resides
  * {TXT:EDDI location station} The name of the station at which the commander is docked
  * {TXT:EDDI location stationtype} The type of the station at which the commander is docked
  * {TXT:EDDI location system} The name of the system in which the commander resides
  * {DEC:EDDI location x} The X co-ordinate of the system in which the commander resides
  * {DEC:EDDI location y} The Y co-ordinate of the system in which the commander resides
  * {DEC:EDDI location z} The Z co-ordinate of the system in which the commander resides

### Market information updated
Triggered when market information for the currently docked station has been updated.
To run a command when this event occurs you should create the command with the name ((EDDI market information updated))

### Material collected
Triggered when you collect a material.
To run a command when this event occurs you should create the command with the name ((EDDI material collected))

Variables set with this event are as follows:

  * {INT:EDDI material collected amount} The amount of the collected material
  * {TXT:EDDI material collected name} The name of the collected material

### Material discarded
Triggered when you discard a material.
To run a command when this event occurs you should create the command with the name ((EDDI material discarded))

Variables set with this event are as follows:

  * {INT:EDDI material discarded amount} The amount of the discarded material
  * {TXT:EDDI material discarded name} The name of the discarded material

### Material discovered
Triggered when you discover a material.
To run a command when this event occurs you should create the command with the name ((EDDI material discovered))

Variables set with this event are as follows:

  * {TXT:EDDI material discovered name} The name of the discovered material

### Material donated
Triggered when you donate a material.
To run a command when this event occurs you should create the command with the name ((EDDI material donated))

Variables set with this event are as follows:

  * {INT:EDDI material donated amount} The amount of the donated material
  * {TXT:EDDI material donated name} The name of the donated material

### Message received
Triggered when you receive a message.
To run a command when this event occurs you should create the command with the name ((EDDI message received))

Variables set with this event are as follows:

  * {TXT:EDDI message received channel} The channel in which the message came (direct, local, wing)
  * {TXT:EDDI message received from} The name of the pilot who sent the message
  * {TXT:EDDI message received message} The message
  * {BOOL:EDDI message received player} True if the sender is a player

### Message sent
Triggered when you send a message.
To run a command when this event occurs you should create the command with the name ((EDDI message sent))

Variables set with this event are as follows:

  * {TXT:EDDI message sent message} The message
  * {TXT:EDDI message sent to} The name of the player to which the message was sent

### Mission abandoned
Triggered when you abandon a mission.
To run a command when this event occurs you should create the command with the name ((EDDI mission abandoned))

Variables set with this event are as follows:

  * {DEC:EDDI mission abandoned missionid} The ID of the mission
  * {TXT:EDDI mission abandoned name} The name of the mission

### Mission accepted
Triggered when you accept a mission.
To run a command when this event occurs you should create the command with the name ((EDDI mission accepted))

Variables set with this event are as follows:

  * {INT:EDDI mission accepted amount} The amount of the commodity or passengers involved in the mission (if applicable)
  * {TXT:EDDI mission accepted commodity} The commodity involved in the mission (if applicable)
  * {BOOL:EDDI mission accepted communal} True if the mission is a community goal
  * {TXT:EDDI mission accepted destinationstation} The destination station for the mission (if applicable)
  * {TXT:EDDI mission accepted destinationsystem} The destination system for the mission (if applicable)
  * {TXT:EDDI mission accepted faction} The faction issuing the mission
  * {DEC:EDDI mission accepted missionid} The ID of the mission
  * {TXT:EDDI mission accepted name} The name of the mission
  * {BOOL:EDDI mission accepted passengerswanted} True if the passengers are wanted (if applicable)
  * {TXT:EDDI mission accepted passengertype} The type of passengers in the mission (if applicable)
  * {TXT:EDDI mission accepted target} Name of the target of the mission (if applicable)
  * {TXT:EDDI mission accepted targetfaction} Faction of the target of the mission (if applicable)
  * {TXT:EDDI mission accepted targettype} Type of the target of the mission (if applicable)

### Mission completed
Triggered when you complete a mission.
To run a command when this event occurs you should create the command with the name ((EDDI mission completed))

Variables set with this event are as follows:

  * {INT:EDDI mission completed amount} The amount of the commodity involved in the mission (if applicable)
  * {TXT:EDDI mission completed commodity} The commodity involved in the mission (if applicable)
  * {BOOL:EDDI mission completed communal} True if the mission is a community goal
  * {DEC:EDDI mission completed donation} The monetary donation when completing the mission
  * {TXT:EDDI mission completed faction} The faction receiving the mission
  * {DEC:EDDI mission completed missionid} The ID of the mission
  * {TXT:EDDI mission completed name} The name of the mission
  * {DEC:EDDI mission completed reward} The monetary reward for completing the mission

### Modification applied
Triggered when you apply a modification to a module.
To run a command when this event occurs you should create the command with the name ((EDDI modification applied))

Variables set with this event are as follows:

  * {TXT:EDDI modification applied blueprint} The blueprint of the modification being applied
  * {TXT:EDDI modification applied engineer} The name of the engineer applying the modification
  * {INT:EDDI modification applied level} The level of the modification being applied

### Modification crafted
Triggered when you craft a modification to a module.
To run a command when this event occurs you should create the command with the name ((EDDI modification crafted))

Variables set with this event are as follows:

  * {TXT:EDDI modification crafted blueprint} The blueprint being crafted
  * {TXT:EDDI modification crafted engineer} The name of the engineer crafting the modification
  * {INT:EDDI modification crafted level} The level of the blueprint being crafted

### NPC attack commenced
Triggered when an attach on your ship by an NPC is detected.
To run a command when this event occurs you should create the command with the name ((EDDI npc attack commenced))

Variables set with this event are as follows:

  * {TXT:EDDI npc attack commenced by} Who the attack is by (Pirate, Military, Bounty hunter, Cargo hunter, etc)

### NPC cargo scan commenced
Triggered when a cargo scan on your ship by an NPC is detected.
To run a command when this event occurs you should create the command with the name ((EDDI npc cargo scan commenced))

Variables set with this event are as follows:

  * {TXT:EDDI npc cargo scan commenced by} Who the cargo scan is by (Pirate, Military, Bounty hunter, Cargo hunter, etc)

### NPC interdiction commenced
Triggered when an interdiction attempt on your ship by an NPC is detected.
To run a command when this event occurs you should create the command with the name ((EDDI npc interdiction commenced))

Variables set with this event are as follows:

  * {TXT:EDDI npc interdiction commenced by} Who the interdiction is by (Pirate, Military, Bounty hunter, Cargo hunter, etc)

### Power commodity delivered
Triggered when a commander delivers a commodity to a power.
To run a command when this event occurs you should create the command with the name ((EDDI power commodity delivered))

Variables set with this event are as follows:

  * {INT:EDDI power commodity delivered amount} The amount of the commodity the commander is delivering
  * {TXT:EDDI power commodity delivered commodity} The commodity the commander is delivering
  * {TXT:EDDI power commodity delivered power} The name of the power for which the commander is delivering the commodity

### Power commodity fast tracked
Triggered when a commander fast tracks a commodity of a power.
To run a command when this event occurs you should create the command with the name ((EDDI power commodity fast tracked))

Variables set with this event are as follows:

  * {INT:EDDI power commodity fast tracked amount} The number of credits spent fast tracking
  * {TXT:EDDI power commodity fast tracked power} The name of the power for which the commander is fast tracking the commodity

### Power commodity obtained
Triggered when a commander obtains a commodity from a power.
To run a command when this event occurs you should create the command with the name ((EDDI power commodity obtained))

Variables set with this event are as follows:

  * {INT:EDDI power commodity obtained amount} The amount of the commodity the commander is obtaining
  * {TXT:EDDI power commodity obtained commodity} The commodity the commander is obtaining
  * {TXT:EDDI power commodity obtained power} The name of the power for which the commander is obtaining the commodity

### Power defected
Triggered when you defect from one power to another.
To run a command when this event occurs you should create the command with the name ((EDDI power defected))

Variables set with this event are as follows:

  * {TXT:EDDI power defected frompower} The name of the power that the commander has defected from
  * {TXT:EDDI power defected topower} The name of the power that the commander has defected to

### Power joined
Triggered when you join a power.
To run a command when this event occurs you should create the command with the name ((EDDI power joined))

Variables set with this event are as follows:

  * {TXT:EDDI power joined power} The name of the power that the commander has joined

### Power left
Triggered when you leave a power.
To run a command when this event occurs you should create the command with the name ((EDDI power left))

Variables set with this event are as follows:

  * {TXT:EDDI power left power} The name of the power that the commander has left

### Power preparation vote cast
Triggered when a commander votes for system perparation.
To run a command when this event occurs you should create the command with the name ((EDDI power preparation vote cast))

Variables set with this event are as follows:

  * {INT:EDDI power preparation vote cast amount} The number of votes cast for the system
  * {TXT:EDDI power preparation vote cast power} The name of the power for which the commander is voting
  * {TXT:EDDI power preparation vote cast system} The name of the system for which the commander voted (might be missing due to journal bug)

### Power salary claimed
Triggered when a commander claims salary from a power.
To run a command when this event occurs you should create the command with the name ((EDDI power salary claimed))

Variables set with this event are as follows:

  * {INT:EDDI power salary claimed amount} The salary claimed
  * {TXT:EDDI power salary claimed power} The name of the power for which the commander is claiming salary

### Screenshot
Triggered when you take a screenshot.
To run a command when this event occurs you should create the command with the name ((EDDI screenshot))

Variables set with this event are as follows:

  * {TXT:EDDI screenshot body} The name of the nearest body to where the screenshot was taken
  * {TXT:EDDI screenshot filename} The name of the file where the screenshot has been saved
  * {INT:EDDI screenshot height} The height in pixels of the screenshot
  * {TXT:EDDI screenshot system} The name of the system where the screenshot was taken
  * {INT:EDDI screenshot width} The width in pixels of the screenshot

### Self destruct
Triggered when you start the self destruct sequence.
To run a command when this event occurs you should create the command with the name ((EDDI self destruct))

### Shields down
Triggered when your ship's shields go offline.
To run a command when this event occurs you should create the command with the name ((EDDI shields down))

### Shields up
Triggered when your ship's shields come online.
To run a command when this event occurs you should create the command with the name ((EDDI shields up))

### Ship delivered
Triggered when your newly-purchased ship is delivered to you.
To run a command when this event occurs you should create the command with the name ((EDDI ship delivered))

Variables set with this event are as follows:

  * {TXT:EDDI ship delivered ship} The ship that was delivered
  * {INT:EDDI ship delivered shipid} The ID of the ship that was delivered

### Ship interdicted
Triggered when your ship is interdicted by another ship.
To run a command when this event occurs you should create the command with the name ((EDDI ship interdicted))

Variables set with this event are as follows:

  * {TXT:EDDI ship interdicted faction} The faction of the NPC carrying out the interdiction
  * {TXT:EDDI ship interdicted interdictor} The name of the commander or NPC carrying out the interdiction
  * {BOOL:EDDI ship interdicted iscommander} If the player carrying out the interdiction is a commander (as opposed to an NPC)
  * {TXT:EDDI ship interdicted power} The power of the NPC carrying out the interdiction
  * {TXT:EDDI ship interdicted rating} The combat rating of the commander or NPC carrying out the interdiction
  * {BOOL:EDDI ship interdicted submitted} If the commander submitted to the interdiction
  * {BOOL:EDDI ship interdicted succeeded} If the interdiction attempt was successful

### Ship interdiction
Triggered when you interdict another ship.
To run a command when this event occurs you should create the command with the name ((EDDI ship interdiction))

Variables set with this event are as follows:

  * {TXT:EDDI ship interdiction faction} The faction of the commander being interdicted
  * {TXT:EDDI ship interdiction interdictee} The name of the commander being interdicted
  * {BOOL:EDDI ship interdiction iscommander} If the player being interdicted is a commander (as opposed to an NPC)
  * {TXT:EDDI ship interdiction power} The power of the commander being interdicted
  * {TXT:EDDI ship interdiction rating} The combat rating of the commander being interdicted
  * {BOOL:EDDI ship interdiction succeeded} If the interdiction attempt was successful

### Ship purchased
Triggered when you purchase a ship.
To run a command when this event occurs you should create the command with the name ((EDDI ship purchased))

Variables set with this event are as follows:

  * {DEC:EDDI ship purchased price} The price of the ship that was purchased
  * {TXT:EDDI ship purchased ship} The ship that was purchased
  * {TXT:EDDI ship purchased soldname} The name of the ship that was sold as part of the purchase
  * {DEC:EDDI ship purchased soldprice} The credits obtained by selling the ship
  * {TXT:EDDI ship purchased soldship} The ship that was sold as part of the purchase
  * {TXT:EDDI ship purchased storedname} The name of the ship that was stored as part of the purchase
  * {TXT:EDDI ship purchased storedship} The ship that was stored as part of the purchase

### Ship rebooted
Triggered when you run reboot/repair on your ship.
To run a command when this event occurs you should create the command with the name ((EDDI ship rebooted))

Variables set with this event are as follows:


### Ship refuelled
Triggered when you refuel your ship.
To run a command when this event occurs you should create the command with the name ((EDDI ship refuelled))

Variables set with this event are as follows:

  * {DEC:EDDI ship refuelled amount} The amount of fuel obtained
  * {DEC:EDDI ship refuelled price} The price of refuelling (only available if the source is Market)
  * {TXT:EDDI ship refuelled source} The source of the fuel (Market or Scoop)
  * {DEC:EDDI ship refuelled total} The new fuel level (only available if the source is Scoop)

### Ship repaired
Triggered when you repair your ship.
To run a command when this event occurs you should create the command with the name ((EDDI ship repaired))

Variables set with this event are as follows:

  * {TXT:EDDI ship repaired item} The item repaired, if repairing a specific item
  * {DEC:EDDI ship repaired price} The price of refuelling

### Ship restocked
Triggered when you restock your ship's ammunition.
To run a command when this event occurs you should create the command with the name ((EDDI ship restocked))

Variables set with this event are as follows:

  * {DEC:EDDI ship restocked price} The price of restocking

### Ship sold
Triggered when you sell a ship.
To run a command when this event occurs you should create the command with the name ((EDDI ship sold))

Variables set with this event are as follows:

  * {DEC:EDDI ship sold price} The price for which the ship was sold
  * {TXT:EDDI ship sold ship} The ship that was sold
  * {INT:EDDI ship sold shipid} The ID of the ship that was sold

### Ship swapped
Triggered when you swap a ship.
To run a command when this event occurs you should create the command with the name ((EDDI ship swapped))

Variables set with this event are as follows:

  * {TXT:EDDI ship swapped ship} The ship that was swapped
  * {INT:EDDI ship swapped shipid} The ID of the ship that was swapped
  * {TXT:EDDI ship swapped soldship} The ship that was sold as part of the swap
  * {INT:EDDI ship swapped soldshipid} The ID of the ship that was sold as part of the swap
  * {TXT:EDDI ship swapped storedship} The ship that was stored as part of the swap
  * {INT:EDDI ship swapped storedshipid} The ID of the ship that was stored as part of the swap

### Ship transfer initiated
Triggered when you initiate a ship transfer.
To run a command when this event occurs you should create the command with the name ((EDDI ship transfer initiated))

Variables set with this event are as follows:

  * {DEC:EDDI ship transfer initiated distance} The distance that the transferred ship needs to travel, in light years
  * {DEC:EDDI ship transfer initiated price} The price of transferring the ship
  * {TXT:EDDI ship transfer initiated ship} The ship that is being transferred
  * {INT:EDDI ship transfer initiated shipid} The ID of the ship that is being transferred
  * {TXT:EDDI ship transfer initiated system} The system from which the ship is being transferred

### SRV docked
Triggered when you dock an SRV with your ship.
To run a command when this event occurs you should create the command with the name ((EDDI srv docked))

### SRV launched
Triggered when you launch an SRV from your ship.
To run a command when this event occurs you should create the command with the name ((EDDI srv launched))

Variables set with this event are as follows:

  * {TXT:EDDI srv launched loadout} The SRV's loadout
  * {BOOL:EDDI srv launched playercontrolled} True if the SRV is controlled by the player

### Star scanned
Triggered when you complete a scan of a stellar body.
To run a command when this event occurs you should create the command with the name ((EDDI star scanned))

Variables set with this event are as follows:

  * {DEC:EDDI star scanned absolutemagnitude} The absolute magnitude of the star that has been scanned
  * {DEC:EDDI star scanned age} The age of the star that has been scanned, in years (rounded to millions of years)
  * {DEC:EDDI star scanned ageprobability} The probablility of finding a star of this class with this age
  * {TXT:EDDI star scanned chromaticity} The apparent colour of the star that has been scanned
  * {DEC:EDDI star scanned distancefromarrival} The distance in LS from the main star
  * {DEC:EDDI star scanned eccentricity} 
  * {DEC:EDDI star scanned luminosity} The luminosity of the star that has been scanned
  * {DEC:EDDI star scanned massprobability} The probablility of finding a star of this class with this mass
  * {TXT:EDDI star scanned name} The name of the star that has been scanned
  * {DEC:EDDI star scanned orbitalinclination} 
  * {DEC:EDDI star scanned orbitalperiod} The number of seconds taken for a full orbit of the main star
  * {DEC:EDDI star scanned periapsis} 
  * {DEC:EDDI star scanned radius} The radius of the star that has been scanned, in metres
  * {DEC:EDDI star scanned radiusprobability} The probablility of finding a star of this class with this radius
  * {DEC:EDDI star scanned rotationperiod} The number of seconds taken for a full rotation
  * {DEC:EDDI star scanned semimajoraxis} 
  * {DEC:EDDI star scanned solarmass} The mass of the star that has been scanned, relative to Sol's mass
  * {DEC:EDDI star scanned solarradius} The radius of the star that has been scanned, compared to Sol
  * {TXT:EDDI star scanned stellarclass} The stellar class of the star that has been scanned (O, G, etc)
  * {DEC:EDDI star scanned temperature} The temperature of the star that has been scanned
  * {DEC:EDDI star scanned tempprobability} The probablility of finding a star of this class with this temperature

### Station no fire zone entered
Triggered when your ship enters a station's no fire zone.
To run a command when this event occurs you should create the command with the name ((EDDI station no fire zone entered))

Variables set with this event are as follows:

  * {BOOL:EDDI station no fire zone entered weaponsdeployed} True if the ship's weapons are deployed when entering the zone

### Station no fire zone exited
Triggered when your ship exits a station's no fire zone.
To run a command when this event occurs you should create the command with the name ((EDDI station no fire zone exited))

### Synthesised
Triggered when you synthesise something from materials.
To run a command when this event occurs you should create the command with the name ((EDDI synthesised))

Variables set with this event are as follows:

  * {TXT:EDDI synthesised synthesis} The thing that has been synthesised

### Touchdown
Triggered when your ship touches down on a planet's surface.
To run a command when this event occurs you should create the command with the name ((EDDI touchdown))

Variables set with this event are as follows:

  * {DEC:EDDI touchdown latitude} The latitude from where the commander has touched down
  * {DEC:EDDI touchdown longitude} The longitude from where the commander has touched down

### Trade data purchased
Triggered when you purchase trade data.
To run a command when this event occurs you should create the command with the name ((EDDI trade data purchased))

Variables set with this event are as follows:

  * {DEC:EDDI trade data purchased price} The price of the purchase
  * {TXT:EDDI trade data purchased system} The system for which trade data was purchased

### Trade promotion
Triggered when you trade rank increases.
To run a command when this event occurs you should create the command with the name ((EDDI trade promotion))

Variables set with this event are as follows:

  * {TXT:EDDI trade promotion rating} The commander's new trade rating

### Undocked
Triggered when your ship undocks from a station or outpost.
To run a command when this event occurs you should create the command with the name ((EDDI undocked))

Variables set with this event are as follows:

  * {TXT:EDDI undocked station} The station from which the commander has undocked

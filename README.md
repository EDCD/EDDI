# EDDI: The Elite Dangerous Data Interface VoiceAttack plugin

Current version: 0.9.4

EDDI is a VoiceAttack plugin that provides over 150 values related to a commander's status, ship and system to VoiceAttack scripts, creating the basis for providing a rich VoiceAttack experience.  It is not in itself a complete VoiceAttack profile, similar to that provided by HCS and numerous personal contributors, but does give the tools to build new or augment existing profiles.  The available values are as follows:

###Commander values

  * Name (text): the name of the commander
  * Home system (text): the name of the home system of the commander, set from EDDI configuration
  * Home system (spoken) (text): the name of the home system of the commander, set from EDDI configuration as would be spoken
  * Home station (text): the name of the home station of the commander in the home system, set from EDDI configuration
  * Combat rating (int): the combat rating of the commander, with 0 being Harmless and 9 being Elite
  * Combat rank (text): the combat rank of the commander, from Harmless to Elite
  * Trade rating (int): the trade rating of the commander, with 0 being Penniless and 9 being Elite
  * Trade rank (text): the trade rank of the commander, from Penniless to Elite
  * Explore rating (int): the exploration rating of the commander, with 0 being Aimless and 9 being Elite
  * Explore rank (text): the exploration rank of the commander, from Aimless to Elite
  * Empire rating (int): the empire rating of the commander, with 0 being None and 14 being King
  * Empire rank (text): the empire rating of the commander, from None to King
  * Federation rating (int): the federation rating of the commander, with 0 being None and 14 being Admiral
  * Federation rank (text): the federation rating of the commander, from None to Admiral
  * Credits (decimal): the number of credits owned by the commander
  * Credits (text): the number of credits owned by the commander as would be spoken (e.g. "just over 2 million")
  * Debt (decimal): the number of credits owed by the commander
  * Debt (text): the number of credits owed by the commander as would be spoken (e.g. "a little under 100 thousand")

###Ship values

  * Ship model (text): the model of the ship (e.g. "Cobra Mk", "Fer-de-Lance")
  * Ship model (spoken) (text): the model of the ship as would be spoken (e.g. "Cobra Mark 4")
  * Ship name (text): the name of the ship as set in EDDI configuration
  * Ship callsign (text): the callsign of the ship as shown in EDDI configuration (e.g. "GEF-1020")
  * Ship callsign (spoken) (text): the callsign of the ship as shown in EDDI configuration as would be spoken
  * Ship role (text): the role of the ship as set in EDDI configuration (Multipurpose, Combat, Exploration, Trading, Mining, Smuggling)
  * Ship size (text): the size of the ship (Small, Medium, or Large)
  * Ship value (decimal): the replacement cost of the ship plus modules
  * Ship value (text): the replacement cost of the ship plus modules as would be spoken
  * Ship cargo capacity (int): the maximum cargo capacity of the ship as currently configured
  * Ship cargo carried (int): the cargo currently being carried by the ship
  * Ship health (decimal): the percentage health of the ship's hull
  * Ship bulkheads (text): the type of bulkheads fitted to the ship (e.g. "Military Grade Composite")
  * Ship bulkheads health (decimal): the percentage health of the bulkheads fitted to the ship
  * Ship bulkheads cost (decimal): the purchase cost of the bulkheads
  * Ship bulkheads value (decimal): the undiscounted cost of the bulkheads
  * Ship bulkheads discount (decimal): the percentage discount of the purchased bulkheads against the undiscounted cost
  * Ship power plant (text): the model of power plant fitted to the ship (e.g. "6D")
  * Ship power plant health (decimal): the percentage health of the power plant fitted to the ship
  * Ship power plant cost (decimal): the purchase cost of the power plant
  * Ship power plant value (decimal): the undiscounted cost of the power plant
  * Ship power plant discount (decimal): the percentage discount of the purchased power plant against the undiscounted cost
  * Ship thrusters (text): the model of thrusters fitted to the ship (e.g. "6D")
  * Ship thrusters health (decimal): the percentage health of the thrusters fitted to the ship
  * Ship thrusters cost (decimal): the purchase cost of the thrusters
  * Ship thrusters value (decimal): the undiscounted cost of the thrusters
  * Ship thrusters discount (decimal): the percentage discount of the purchased thrusters against the undiscounted cost
  * Ship frame shift drive (text): the model of frame shift drive fitted to the ship (e.g. "6D")
  * Ship frame shift drive health (decimal): the percentage health of the frame shift drive fitted to the ship
  * Ship frame shift drive cost (decimal): the purchase cost of the frame shift drive
  * Ship frame shift drive value (decimal): the undiscounted cost of the frame shift drive
  * Ship frame shift drive discount (decimal): the percentage discount of the purchased frame shift drive against the undiscounted cost
  * Ship life support (text): the model of life support fitted to the ship (e.g. "6D")
  * Ship life support health (decimal): the percentage health of the life support fitted to the ship
  * Ship life support cost (decimal): the purchase cost of the life support
  * Ship life support value (decimal): the undiscounted cost of the life support
  * Ship life support discount (decimal): the percentage discount of the purchased life support against the undiscounted cost
  * Ship power distributor (text): the model of power distributor fitted to the ship (e.g. "6D")
  * Ship power distributor health (decimal): the percentage health of the power distributor fitted to the ship
  * Ship power distributor cost (decimal): the purchase cost of the power distributor
  * Ship power distributor value (decimal): the undiscounted cost of the power distributor
  * Ship power distributor discount (decimal): the percentage discount of the purchased power distributor against the undiscounted cost
  * Ship sensors (text): the model of sensors fitted to the ship (e.g. "6D")
  * Ship sensors health (decimal): the percentage health of the sensors fitted to the ship
  * Ship sensors cost (decimal): the purchase cost of the sensors
  * Ship sensors value (decimal): the undiscounted cost of the sensors
  * Ship sensors discount (decimal): the percentage discount of the purchased sensors against the undiscounted cost
  * Ship fuel tank (text): the model of fuel tank fitted to the ship (e.g. "5C")
  * Ship fuel tank cost (decimal): the purchase cost of the fuel tank
  * Ship fuel tank value (decimal): the undiscounted cost of the fuel tank
  * Ship fuel tank discount (decimal): the percentage discount of the purchased fuel tank against the undiscounted cost
  * Ship fuel tank capacity (decimal): the capacity of the fuel tank
  * Ship tiny/small/medium/large/huge hardpoint *n* occupied (boolean): true if there is a module in this slot, otherwise false
  * Ship tiny/small/medium/large/huge hardpoint *n* module (string): the name of the module in this slot
  * Ship tiny/small/medium/large/huge hardpoint *n* module class (int): the class of the module in this slot
  * Ship tiny/small/medium/large/huge hardpoint *n* module grade(grade): the grade of the module in this slot
  * Ship tiny/small/medium/large/huge hardpoint *n* module health (decimal): the percentage health of the module in this slot
  * Ship tiny/small/medium/large/huge hardpoint *n* module cost (decimal): the purchase cost of the module in this slot
  * Ship tiny/small/medium/large/huge hardpoint *n* module value (decimal): the undiscounted cost of the module in this slot
  * Ship tiny/small/medium/large/huge hardpoint *n* module discount (decimal): the percentage discount of the purchased module against the undiscounted cost
  * Ship Compartment *n* size: the size of this slot
  * Ship Compartment *n* occupied (boolean): true if there is a module in this slot, otherwise false
  * Ship compartment *n* module (string): the name of the module in this slot
  * Ship compartment *n* module class (int): the class of the module in this slot
  * Ship compartment *n* module grade (string): the grade of the module in this slot
  * Ship compartment *n* module health (decimal): the percentage health of the module in this slot
  * Ship compartment *n* module cost (decimal): the purchase cost of the module in this slot
  * Ship compartment *n* module value (decimal): the undiscounted cost of the module in this slot
  * Ship compartment *n* module station cost (decimal): the purchase cost of the module at this station

###Current system values

  * System name (text): the name of the system
  * System name (spoken) (text): the name of the system as would be spoken
  * System distance from home (decimal): the number of lights years between this system and the your home system, to two decimal places
  * System visits (int): the number of times the commander has visited the system (whilst the plugin has been active)
  * System previous visit (datetime): the last time the commander visited the system (empty if this is their first visit)
  * System minutes since previous visit (int): the number of minutes since the commander's last visit to the system
  * System population (decimal): the population of the system
  * System population (text): the population of the system as would be spoken (e.g. "nearly 12 and a half billion")
  * System allegiance (text): the allegiance of the system ("Federation", "Empire", "Alliance", "Independant" or empty)
  * System government (text): the government of the system (e.g. "Democracy")
  * System faction (text): the primary faction of the system (e.g. "The Pilots' Federation")
  * System primary economy (text): the primary economy of the system (e.g. "Industrial")
  * System state (text): the overall state of the system (e.g. "Boom")
  * System security (text): the security level in the system ("High", "Medium", "Low", "None" or empty)
  * System power (text): the name of the power that controls the system (e.g. "Aisling Duval")
  * System power (spoken) (text): the name of the power that controls the system as would be spoken (e.g. "Ashling Du-val")
  * System power state (text): the state of the power in the system (e.g. "Expansion")
  * System rank (text): the rank of the Commander in the system (e.g. "Duke" if an Empire system, "Admiral" if a Federation system)
  * System X (decimal) the EDDB X co-ordinate of the system
  * System Y (decimal) the EDDB X co-ordinate of the system
  * System Z (decimal) the EDDB X co-ordinate of the system
  * System stations (int): the total number of stations (starports and outposts) in the system
  * System starports (int): the total number of starports in the system
  * System outposts (int): the total number of outposts in the system
  * System planetary stations (int): the total number of plaentary stations (outposts and ports) in the system
  * System planetary outposts (int): the total number of planetary outposts in the system
  * System planetary ports (int): the total number of planetary ports in the system

###Last system values

  * Last system name (text): the name of the last system
  * Last system name (spoken) (text): the name of the last system as would be spoken
  * Last system visits (int): the number of times the commander has visited the last system (whilst the plugin has been active)
  * Last system previous visit (datetime): the last time the commander visited the last system (empty if this is their first visit)
  * Last system population (decimal): the population of the last system
  * Last system population (text): the population of the last system as would be spoken (e.g. "nearly 12 and a half billion")
  * Last system allegiance (text): the allegiance of the last system ("Federation", "Empire", "Alliance", "Independant" or empty)
  * Last system government (text): the government of the last system (e.g. "Democracy")
  * Last system faction (text): the primary faction of the last system (e.g. "The Pilots' Federation")
  * Last system primary economy (text): the primary economy of the last system (e.g. "Industrial")
  * Last system state (text): the overall state of the last system (e.g. "Boom")
  * Last system security (text): the security level in the last system ("High", "Medium", "Low", "None" or empty)
  * Last system power (text): the name of the power that controls the last system (e.g. "Felicia Winters")
  * Last system power (spoken) (text): the name of the power that controls the last system as would be spoken
  * Last system power state (text): the state of the power in the last system (e.g. "Expansion")
  * Last system rank (text): the rank of the Commander in the last system (e.g. "Duke" if an Empire system, "Admiral" if a Federation system)
  * Last system X (decimal) the EDDB X co-ordinate of the last system
  * Last system Y (decimal) the EDDB X co-ordinate of the last system
  * Last system Z (decimal) the EDDB X co-ordinate of the last system

###Current station values

  * Ship bulkheads station cost (decimal): the purchase cost of the bulkheads at the station (not set if not for sale at the station)
  * Ship bulkheads station discount (decimal): the number of credits discount of the bulkheads over those currently fitted (not set if no additional discount)
  * Ship bulkheads station discount (spoken) (text): the number of credits discount of the bulkheads over those currently fitted as would be spoken (not set if no additional discount)
  * Ship power plant station cost (decimal): the purchase cost of the power plant at the station (not set if not for sale at the station)
  * Ship power plant station discount (decimal): the number of credits discount of the power plant over that currently fitted (not set if no additional discount)
  * Ship power plant station discount (spoken) (text): the number of credits discount of the power plant over thothatse currently fitted as would be spoken (not set if no additional discount)
  * Ship thrusters station cost (decimal): the purchase cost of the thrusters at the station (not set if not for sale at the station)
  * Ship thrusters station discount (decimal): the number of credits discount of the thrusters over those currently fitted (not set if no additional discount)
  * Ship thrusters station discount (spoken) (text): the number of credits discount of the thrusters over those currently fitted as would be spoken (not set if no additional discount)
  * Ship frame shift drive station cost (decimal): the purchase cost of the frame shift drive at the station (not set if not for sale at the station)
  * Ship frame shift drive station discount (decimal): the number of credits discount of the frame shift drive over those currently fitted (not set if no additional discount)
  * Ship frame shift drive station discount (spoken) (text): the number of credits discount of the frame shift drive over those currently fitted as would be spoken (not set if no additional discount)
  * Ship life support station cost (decimal): the purchase cost of the life support at the station (not set if not for sale at the station)
  * Ship life support station discount (decimal): the number of credits discount of the life support over that currently fitted (not set if no additional discount)
  * Ship life support station discount (spoken) (text): the number of credits discount of the life support over those currently fitted as would be spoken (not set if no additional discount)
  * Ship power distributor station cost (decimal): the purchase cost of the power distributor at the station (not set if not for sale at the station)
  * Ship power distributor station discount (decimal): the number of credits discount of the power distributor over those currently fitted (not set if no additional discount)
  * Ship power distributor station discount (spoken) (text): the number of credits discount of the power distributor over those currently fitted as would be spoken (not set if no additional discount)
  * Ship sensors station cost (decimal): the purchase cost of the sensors at the station (not set if not for sale at the station)
  * Ship sensors station discount (decimal): the number of credits discount of the sensors over those currently fitted (not set if no additional discount)
  * Ship sensors station discount (spoken) (text): the number of credits discount of the sensors over those currently fitted as would be spoken (not set if no additional discount)
  * Ship tiny/small/medium/large/huge hardpoint *n* module station cost (decimal): the purchase cost of this module at this station (not set if not for sale at the station)
  * Ship tiny/small/medium/large/huge hardpoint *n* module station discount (decimal): the number of credits discount of the module over that currently fitted (not set if no additional discount)
  * Ship tiny/small/medium/large/huge hardpoint *n* module station discount (spoken) (text): the number of credits discount of the module over that currently fitted as would be spoken (not set if no additional discount)
  * Ship compartment *n* module station cost (decimal): the purchase cost of this module at this station (not set if not for sale at the station)
  * Ship compartment *n* module station discount (decimal): the number of credits discount of the module over that currently fitted (not set if no additional discount)
  * Ship compartment *n* module station discount (spoken) (text): the number of credits discount of the module over that currently fitted as would be spoken (not set if no additional discount)

###Shipyard values

  * Stored ships (int): the number of ships in storage
  * Stored ship *n* model (text): the model of the *n*th stored ship
  * Stored ship *n* name (text): the name of the *n*th stored ship as set in EDDI configuration
  * Stored ship *n* callsign (text): the callsign of the *n*th stored ship as shown in EDDI configuration (e.g. "GEF-1020")
  * Stored ship *n* callsign (spoken) (text): the callsign of the *n*th stored ship as shown in EDDI configuration as would be spoken
  * Stored ship *n* role (text): the role of the *n*th stored ship as set in EDDI configuration (Multipurpose, Combat, Trade, Exploration, Smuggling)
  * Stored ship *n* station (text): the station in which the *n*th stored ship resides
  * Stored ship *n* system (text): the system in which the *n*th stored ship resides
  * Stored ship *n* distance (decimal): the number of light years between the current system and that where the *n*th ship resiedes, to two decimal places

###Miscellaneous

  * Environment (text): the environment the ship is in (either "Normal space" or "Supercruise")
  * Last jump (decimal): the number of lights years between this system and the last, to two decimal places
  * Last station name (text): the name of the last station the commander docked at

EDDI also provides a number of pre-built commands to show off some of what it is capable of.  These include:

  * system information triggered whenever a pilot jumps to a new system, including role-specific information
  * environment information triggered whenever a pilot moves from supercruise to normal space and /vice versa/
  * a voice command spoken by the pilot whenever they finish docking ("I have docked" or "Docking complete"), including role-specific information
  * a voice command spoken by the pilot whenever they change ship ("Ship handover complete") that provides a run-down of the ship the pilot is now in
  * a voice command spoken by the pilot before they launch ("run pre flight checks") that carries out a check of areas such as insurance, repairs /etc./
  * a voice command spoken by the pilot when they wish to see their ship in http://www.coriolis.io/ ("Display my ship in coriolis")
  * a voice command spoken by the pilot when they wish to check the current discounts on their ship ("Report on ship discounts")
  * voice commands spoken by the pilot when they wish to check the current status of their ship ("Report" and "Quick report")
  * voice commands spoken by the pilot when they wish to check just the damage on their ship ("Damage report")
  * voice commands spoken by the pilot when they wish to carry out checks prior to undocking ("Run pre-flight checks")

##Installing

Download the EDDI ZIP file from http://www.mcdee.net/elite/EDDI.zip or compile it from the sources at https://github.com/cmdrmcdonald/EliteDangerousDataProvider  The files need to be installed in the 'Apps' directory within the VoiceAttack program directory (usually installed at c:\program files (x86)\VoiceAttack).  The resultant directory structure, assuming the standard directories, should be c:\program files (x86)\VoiceAttack\Apps\EDDI.

You must use the latest VoiceAttack beta for EDDI to operate.  At current this is version 1.5.8.14

##Upgrading

If you are upgrading from an earlier version of EDDI it is recommended that you remove the existing EDDI directory from within VoiceAttack's Apps directory before installing the new one.  This ensures that there is a clean installation and reduces the chances of problems occurring.

When upgrading EDDI you should overwrite all of the existing EDDI actions in VoiceAttack except any event handlers you have customised.

Please note that as of 0.8.6 the startup command has changed from '((EDDI: event loop))' to '((EDDI: startup))'.  Please ensure that you have set the correct startup command in your profile.

##Configuring

To configure EDDI run the 'configuration.exe' file in the plugin directory.  This allows you to configure the various connectors that EDDI uses to obtain data.

You will also need to configure verbose net logs for Elite: Dangerous to ensure that you receive system change messages.  To do so you need to find the AppConfig.xml file in your Elite product installation and ensure that the network section of it looks like this:

    <Network
     Port="0"
     upnpenabled="1"
     LogFile="netLog"
     DatestampLog="1"
     VerboseLogging="1">

Once this is complete the final step is to configure VoiceAttack itself.  To do this you need to start VoiceAttack and ensure that 'Enable Plugins' is checked in the settings.  You also need to import the EDDI profile found in the plugin directory.  Finally, you need to edit the profile to set ((EDDI: startup)) to execute when the profile is loaded.

Once all of this is complete you can restart VoiceAttack and press shift-control-alt-v to obtain a full list of the variables provided by EDDI.

##Ship Voice

EDDI provides a ship's voice through use of the 'say' EDDI plugin command.  To use this yourself set a variable ending with the name " script" and pass it in to the say command.

Say translates variables.  Current variables available are:

  * $= this translates to the name of the ship, or "your ship" if the ship has not been named.

##The EDDI Event Handler

Elite writes a number of messages to its activity log, some of which are parsed by EDDI and turned in to events.  The EDDI event loop triggers one of a number of actions depending on the event.  At current the actions are:

  * ((EDDI: event handler for change of system)) triggered whenever the commander changes to a different system.  This triggers at the end of the hyperspace countdown.
  * ((EDDI: event handler for change of environment)) triggered whenever the commander moves between supercruise and normal space.  This triggers as the move occurs.
  * ((EDDI: event handler for ship docked)) triggered whenever the commander issues a "ship docked" command to VoiceAttack.
  * ((EDDI: event handler for ship changed)) triggered whenever the commander issues a "ship handover complete" command to VoiceAttack.

The above actions can be customised as you see fit.  If you customise them then when you upgrade EDDI you should not overwrite the event handlers (although you should overwrite the event loop).

#Using EDDI Elsewhere

Although EDDI is primarily used for the VoiceAttack plugin all of the relevant functions are in separate DLLs and the data access and storage pieces can be used for purposes other than VoiceAttack if desired.

#Issues

If you have an issue with EDDI then please report it at https://github.com/cmdrmcdonald/EliteDangerousDataProvider/issues  If you have encountered a problem then please provide the output of the error report (shift-control-alt-e) to aid in fixing the issue.
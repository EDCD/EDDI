# Using EDDI with VoiceAttack

EDDI interfaces with VoiceAttack in two ways.  Firstly, it generates a large number of variables inside VoiceAttack and keeps them up-to-date.  Secondly, it runs VoiceAttack commands when events occur.  This document explains how to use both of these features.

## Installing EDDI for VoiceAttack

N.B. EDDI requires at least version 1.8.12.22 of VoiceAttack to function correctly.

For EDDI to work with VoiceAttack it must be installed as a VoiceAttack plugin.  To do this EDDI should be installed within the `Apps` directory of your VoiceAttack installation.

VoiceAttack must be configured to use plugins.  To do so you must click on the Settings icon (a spanner) in the top-right corner of the VoiceAttack and check the 'Enable plugin support' option and restart VoiceAttack.

If EDDI is installed in the correct location and plugin support is enabled you should see a message when starting VoiceAttack along the lines of `Plugin EDDI 2.0.0 initialized`.

## EDDI Variables


EDDI makes a large number of values available to augment your existing scripts.  The values are shown below, along with the type of the value listed in brackets and a brief description of what the value holds.

If a value is not available it will be not set rather than empty.

### Commander Variables

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
  * Insurance (decimal): the percentage insurance excess for the commander (usually 5, 3.75 or 2.5)

### Ship Variables

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
  * Ship limpets carried (int): the number of limpets currently being carried by the ship
  * Ship health (decimal): the percentage health of the ship's hull
  * Ship bulkheads (text): the type of bulkheads fitted to the ship (e.g. "Military Grade Composite")
  * Ship bulkheads class (text): the class of bulkheads fitted to the ship (e.g. 3)
  * Ship bulkheads grade (text): the grade of bulkheads fitted to the ship (e.g. "A")
  * Ship bulkheads health (decimal): the percentage health of the bulkheads fitted to the ship
  * Ship bulkheads cost (decimal): the purchase cost of the bulkheads
  * Ship bulkheads value (decimal): the undiscounted cost of the bulkheads
  * Ship bulkheads discount (decimal): the percentage discount of the purchased bulkheads against the undiscounted cost
  * Ship power plant (text): the name of power plant fitted to the ship
  * Ship power plant class (text): the class of bulkheads fitted to the ship (e.g. 3)
  * Ship power plant grade (text): the grade of bulkheads fitted to the ship (e.g. "A")
  * Ship power plant health (decimal): the percentage health of the power plant fitted to the ship
  * Ship power plant cost (decimal): the purchase cost of the power plant
  * Ship power plant value (decimal): the undiscounted cost of the power plant
  * Ship power plant discount (decimal): the percentage discount of the purchased power plant against the undiscounted cost
  * Ship thrusters (text): the name of thrusters fitted to the ship
  * Ship thrusters class (text): the class of thrusters fitted to the ship (e.g. 3)
  * Ship thrusters grade (text): the grade of thrusters fitted to the ship (e.g. "A")
  * Ship thrusters health (decimal): the percentage health of the thrusters fitted to the ship
  * Ship thrusters cost (decimal): the purchase cost of the thrusters
  * Ship thrusters value (decimal): the undiscounted cost of the thrusters
  * Ship thrusters discount (decimal): the percentage discount of the purchased thrusters against the undiscounted cost
  * Ship frame shift drive (text): the name of frame shift drive fitted to the ship
  * Ship frame shift drive class (text): the class of frame shift drive fitted to the ship (e.g. 3)
  * Ship frame shift drive grade (text): the grade of frame shift drive fitted to the ship (e.g. "A")
  * Ship frame shift drive health (decimal): the percentage health of the frame shift drive fitted to the ship
  * Ship frame shift drive cost (decimal): the purchase cost of the frame shift drive
  * Ship frame shift drive value (decimal): the undiscounted cost of the frame shift drive
  * Ship frame shift drive discount (decimal): the percentage discount of the purchased frame shift drive against the undiscounted cost
  * Ship life support (text): the name of life support fitted to the ship (e.g. "6D")
  * Ship life support class (text): the class of life support fitted to the ship (e.g. 3)
  * Ship life support drive grade (text): the grade of life support fitted to the ship (e.g. "A")
  * Ship life support health (decimal): the percentage health of the life support fitted to the ship
  * Ship life support cost (decimal): the purchase cost of the life support
  * Ship life support value (decimal): the undiscounted cost of the life support
  * Ship life support discount (decimal): the percentage discount of the purchased life support against the undiscounted cost
  * Ship power distributor (text): the name of power distributor fitted to the ship
  * Ship power distributor class (text): the class of power distributor fitted to the ship (e.g. 3)
  * Ship power distributor drive grade (text): the grade of power distributor fitted to the ship (e.g. "A")
  * Ship power distributor health (decimal): the percentage health of the power distributor fitted to the ship
  * Ship power distributor cost (decimal): the purchase cost of the power distributor
  * Ship power distributor value (decimal): the undiscounted cost of the power distributor
  * Ship power distributor discount (decimal): the percentage discount of the purchased power distributor against the undiscounted cost
  * Ship sensors (text): the name of sensors fitted to the ship
  * Ship sensors class (text): the class of sensors fitted to the ship (e.g. 3)
  * Ship sensors drive grade (text): the grade of sensors fitted to the ship (e.g. "A")
  * Ship sensors health (decimal): the percentage health of the sensors fitted to the ship
  * Ship sensors cost (decimal): the purchase cost of the sensors
  * Ship sensors value (decimal): the undiscounted cost of the sensors
  * Ship sensors discount (decimal): the percentage discount of the purchased sensors against the undiscounted cost
  * Ship fuel tank (text): the name of fuel tank fitted to the ship
  * Ship fuel tank class (text): the class of fuel tank fitted to the ship (e.g. 3)
  * Ship fuel tank drive grade (text): the grade of fuel tank fitted to the ship (e.g. "A")
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

### Current System Variables

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
  * System stations (int): the total number of stations, both in orbit and on planets, in the system
  * System orbital stations (int): the number of orbital stations in the system
  * System starports (int): the total number of orbital starports in the system
  * System outposts (int): the total number of orbital outposts in the system
  * System planetary stations (int): the total number of planetary stations (outposts and ports) in the system
  * System planetary outposts (int): the total number of planetary outposts in the system
  * System planetary ports (int): the total number of planetary ports in the system

### Last System Variables

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

### Current Station Variables

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
  * Last station name (text): the name of the last station the commander docked at
  * Last station faction (string): the name of the controlling faction of the last station
  * Last station government (string): the name of the government of the last station
  * Last station allegiance (string): the name of the allegiance of the last station (Federation, Empire, etc.)
  * Last station state (string): the name of the state of the last station (boom, outbreak, etc.)
  * Last station distance from star (decimal): the distance from the primary star to this station, in light seconds
  * Last station primary economy (string): the primary economy of this station (extraction, prison colony, etc.)
  * Last station secondary economy (string): the secondary economy of this station (extraction, prison colony, etc.)
  * Last station tertiary economy (string): the tertiary economy of this station (extraction, prison colony, etc.)
  * Last station has refuel (boolean): true if this station has refuel capability
  * Last station has rearm (boolean): true if this station has rearm capability
  * Last station has repair (boolean): true if this station has repair capability
  * Last station has market (boolean): true if this station has a commodities market
  * Last station has black market (boolean): true if this station has a black market
  * Last station has outfitting (boolean): true if this station has outfitting
  * Last station has shipyard (boolean): true if this station has a shipyard

### Shipyard Variables

  * Stored ships (int): the number of ships in storage
  * Stored ship *n* model (text): the model of the *n*th stored ship
  * Stored ship *n* name (text): the name of the *n*th stored ship as set in EDDI configuration
  * Stored ship *n* callsign (text): the callsign of the *n*th stored ship as shown in EDDI configuration (e.g. "GEF-1020")
  * Stored ship *n* callsign (spoken) (text): the callsign of the *n*th stored ship as shown in EDDI configuration as would be spoken
  * Stored ship *n* role (text): the role of the *n*th stored ship as set in EDDI configuration (Multipurpose, Combat, Trade, Exploration, Smuggling)
  * Stored ship *n* station (text): the station in which the *n*th stored ship resides
  * Stored ship *n* system (text): the system in which the *n*th stored ship resides
  * Stored ship *n* distance (decimal): the number of light years between the current system and that where the *n*th ship resiedes, to two decimal places

### Miscellaneous Variables

  * Environment (text): the environment the ship is in (either "Normal space" or "Supercruise")
  * Last jump (decimal): the number of lights years between this system and the last, to two decimal places

EDDI also provides a number of pre-built commands to show off some of what it is capable of.  These include:

  * a voice command spoken by the pilot before they launch ("run pre flight checks") that carries out a check of areas such as insurance, repairs /etc./
  * a voice command spoken by the pilot when they wish to see their ship in https://www.coriolis.io/ ("Display my ship in coriolis")
  * a voice command spoken by the pilot when they wish to see their current system in https://www.eddb.io/ ("Show this system in EDDB")
  * a voice command spoken by the pilot when they wish to see their current station in https://www.eddb.io/ ("Show this station in EDDB")

To access these commands, as well as to obtain a number of commands that display the values of the variables listed above, import the "EDDI" VoiceAttack from the EDDI directory in to VoiceAttack.  To access these commands edit your own profile and set "Include commands from another profile" to "EDDI".

## Events

Whenever EDDI sees a particular event occur it will attempt to run a script.  The name of the script depends on the event, but follows the form:

    ((EDDI <event>))

with the <event> being in lower-case.  For example, if you wanted VoiceAttack to run a script every time you docked you would create a script called `((EDDI docked))` (note the lower-case d at the beginning of docked).

There are a large number of events available.  The full list of them can be found in EDDI's "Speech responder" tab in the UI.

In addition to the events themselves EDDI populates VoiceAttack with additional variables for each event.  For example, for the `Docked` event EDDI populates the following additional variables:

    - `EDDI docked station` The station at which the commander has docked
    - `EDDI docked system` The system at which the commander has docked
    - `EDDI docked allegiance` The allegiance of the station at which the commander has docked
    - `EDDI docked faction` The faction controlling the station at which the commander has docked
    - `EDDI docked factionstate` The state of the faction controlling the station at which the commander has docked
    - `EDDI docked economy` The economy of the station at which the commander has docked
    - `EDDI docked government` The government of the station at which the commander has docked
    - `EDDI docked security` The security of the station at which the commander has docked

So to access the name of the station from within the ((EDDI docked)) script you would use {TXT:EDDI docked station}.  Note that all variables are TXT unless otherwise stated.

Details of the variables for each event can be found in EDDI's "Speech responder" tab in the UI, by clicking on a given event and selecting "Edit".
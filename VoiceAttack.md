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

There are a large number of events available.  Full details of them and the variables that are set for each of them are as follows:

### Body scanned
Triggered when you complete a scan of a planetary body.
To run a command when this event occurs you should create the command with the name ((EDDI body scanned))

Variables set with this events are as follows:

{TXT:EDDI body scanned atmosphere} The atmosphere of the body that has been scanned
{TXT:EDDI body scanned bodyclass} The class of the body that has been scanned (High metal content body etc)
{DEC:EDDI body scanned distancefromarrival} The distance in LS from the main star
{DEC:EDDI body scanned eccentricity} 
{DEC:EDDI body scanned gravity} The surface gravity of the body that has been scanned, relative to Earth's gravity
{BOOL:EDDI body scanned landable} True if the body is landable
{TXT:EDDI body scanned name} The name of the body that has been scanned
{DEC:EDDI body scanned orbitalinclination} 
{DEC:EDDI body scanned orbitalperiod} The number of seconds taken for a full orbit of the main star
{DEC:EDDI body scanned periapsis} 
{DEC:EDDI body scanned pressure} The surface pressure of the body that has been scanned
{DEC:EDDI body scanned rotationperiod} The number of seconds taken for a full rotation
{DEC:EDDI body scanned semimajoraxis} 
{DEC:EDDI body scanned temperature} The surface temperature of the body that has been scanned
{BOOL:EDDI body scanned tidallylocked} True if the body is tidally locked
{TXT:EDDI body scanned volcanism} The volcanism of the body that has been scanned

### Bond awarded
Triggered when you are awarded a combat bond.
To run a command when this event occurs you should create the command with the name ((EDDI bond awarded))

Variables set with this events are as follows:

{TXT:EDDI bond awarded awardingfaction} The name of the faction awarding the bond
{DEC:EDDI bond awarded reward} The number of credits received
{TXT:EDDI bond awarded victimfaction} The name of the faction whose ship you destroyed

### Bounty awarded
Triggered when you are awarded a bounty.
To run a command when this event occurs you should create the command with the name ((EDDI bounty awarded))

Variables set with this events are as follows:

{TXT:EDDI bounty awarded faction} The name of the faction whose ship you destroyed
{DEC:EDDI bounty awarded reward} The total number of credits obtained for destroying the ship
{TXT:EDDI bounty awarded target} The name of the pilot you destroyed

### Bounty incurred
Triggered when you incur a bounty.
To run a command when this event occurs you should create the command with the name ((EDDI bounty incurred))

Variables set with this events are as follows:

{DEC:EDDI bounty incurred bounty} The number of credits issued as the bounty
{TXT:EDDI bounty incurred crimetype} The type of crime committed
{TXT:EDDI bounty incurred faction} The name of the faction issuing the bounty
{TXT:EDDI bounty incurred victim} The name of the victim of the crime

### Cleared save
Triggered when you clear your save.
To run a command when this event occurs you should create the command with the name ((EDDI cleared save))

Variables set with this events are as follows:

{TXT:EDDI cleared save name} The name of the player whose save has been cleared

### Cockpit breached
Triggered when your ship's cockpit is broken.
To run a command when this event occurs you should create the command with the name ((EDDI cockpit breached))

### Combat promotion
Triggered when your combat rank increases.
To run a command when this event occurs you should create the command with the name ((EDDI combat promotion))

Variables set with this events are as follows:

{TXT:EDDI combat promotion rating} The commander's new combat rating

### Commander continued
Triggered when you continue an existing game.
To run a command when this event occurs you should create the command with the name ((EDDI commander continued))

Variables set with this events are as follows:

{TXT:EDDI commander continued commander} The commander's name
{DEC:EDDI commander continued credits} the number of credits the commander has
{TXT:EDDI commander continued group} The name of the group (only if mode == Group)
{TXT:EDDI commander continued ship} The commander's ship

### Commander progress
Triggered when your progress is reported.
To run a command when this event occurs you should create the command with the name ((EDDI commander progress))

Variables set with this events are as follows:

{DEC:EDDI commander progress combat} The percentage progress of the commander's combat rating
{DEC:EDDI commander progress cqc} The percentage progress of the commander's CQC rating
{DEC:EDDI commander progress empire} The percentage progress of the commander's empire rating
{DEC:EDDI commander progress exploration} The percentage progress of the commander's exploration rating
{DEC:EDDI commander progress federation} The percentage progress of the commander's federation rating

### Commander ratings
Triggered when your ratings are reported.
To run a command when this event occurs you should create the command with the name ((EDDI commander ratings))

Variables set with this events are as follows:


### Commander started
Triggered when you start a new game.
To run a command when this event occurs you should create the command with the name ((EDDI commander started))

Variables set with this events are as follows:

{TXT:EDDI commander started name} The name of the new commander
{TXT:EDDI commander started package} The starting package of the new commander

### Commodity collected
Triggered when you pick up a commodity in your ship or SRV.
To run a command when this event occurs you should create the command with the name ((EDDI commodity collected))

Variables set with this events are as follows:

{TXT:EDDI commodity collected commodity} The name of the commodity collected
{BOOL:EDDI commodity collected stolen} If the cargo is stolen

### Commodity ejected
Triggered when you eject a commodity from your ship or SRV.
To run a command when this event occurs you should create the command with the name ((EDDI commodity ejected))

Variables set with this events are as follows:

{BOOL:EDDI commodity ejected abandoned} If the cargo has been abandoned
{INT:EDDI commodity ejected amount} The amount of cargo ejected
{TXT:EDDI commodity ejected commodity} The name of the commodity ejected

### Commodity purchased
Triggered when you buy a commodity from the markets.
To run a command when this event occurs you should create the command with the name ((EDDI commodity purchased))

Variables set with this events are as follows:

{INT:EDDI commodity purchased amount} The amount of the purchased commodity
{TXT:EDDI commodity purchased commodity} The name of the purchased commodity
{DEC:EDDI commodity purchased price} The price paid per unit of the purchased commodity

### Commodity refined
Triggered when you refine a commodity from the refinery.
To run a command when this event occurs you should create the command with the name ((EDDI commodity refined))

Variables set with this events are as follows:

{TXT:EDDI commodity refined commodity} The name of the commodity refined

### Commodity sold
Triggered when you sell a commodity to the markets.
To run a command when this event occurs you should create the command with the name ((EDDI commodity sold))

Variables set with this events are as follows:

{INT:EDDI commodity sold amount} The amount of the commodity sold
{BOOL:EDDI commodity sold blackmarket} True if the commodity was sold to a black market
{TXT:EDDI commodity sold commodity} The name of the commodity sold
{BOOL:EDDI commodity sold illegal} True if the commodity is illegal at the place of sale
{DEC:EDDI commodity sold price} The price obtained per unit of the commodity sold
{DEC:EDDI commodity sold profit} The number of credits profit per unit of the commodity sold
{BOOL:EDDI commodity sold stolen} True if the commodity was stolen

### Controlling fighter
Triggered when you switch control from your ship to your fighter.
To run a command when this event occurs you should create the command with the name ((EDDI controlling fighter))

### Controlling ship
Triggered when you switch control from your fighter to your ship.
To run a command when this event occurs you should create the command with the name ((EDDI controlling ship))

### Crew assigned
Triggered when you assign crew.
To run a command when this event occurs you should create the command with the name ((EDDI crew assigned))

Variables set with this events are as follows:

{TXT:EDDI crew assigned name} The name of the crewmember being assigned
{TXT:EDDI crew assigned role} The role to which the crewmember is being assigned

### Crew fired
Triggered when you fire crew.
To run a command when this event occurs you should create the command with the name ((EDDI crew fired))

Variables set with this events are as follows:

{TXT:EDDI crew fired name} The name of the crewmember being fired

### Crew hired
Triggered when you hire crew.
To run a command when this event occurs you should create the command with the name ((EDDI crew hired))

Variables set with this events are as follows:

{TXT:EDDI crew hired combatrating} The combat rating of the crewmember being hired
{TXT:EDDI crew hired faction} The faction of the crewmember being hired
{TXT:EDDI crew hired name} The name of the crewmember being hired
{DEC:EDDI crew hired price} The price of the crewmember being hired

### Died
Triggered when you have died.
To run a command when this event occurs you should create the command with the name ((EDDI died))

Variables set with this events are as follows:


### Docked
Triggered when your ship docks at a station or outpost.
To run a command when this event occurs you should create the command with the name ((EDDI docked))

Variables set with this events are as follows:

{TXT:EDDI docked allegiance} The allegiance of the station at which the commander has docked
{TXT:EDDI docked economy} The economy of the station at which the commander has docked
{TXT:EDDI docked faction} The faction controlling the station at which the commander has docked
{TXT:EDDI docked factionstate} The state of the faction controlling the station at which the commander has docked
{TXT:EDDI docked government} The government of the station at which the commander has docked
{TXT:EDDI docked security} The security of the station at which the commander has docked
{TXT:EDDI docked station} The station at which the commander has docked
{TXT:EDDI docked system} The system at which the commander has docked

### Docking cancelled
Triggered when your ship cancels a docking request at a station or outpost.
To run a command when this event occurs you should create the command with the name ((EDDI docking cancelled))

Variables set with this events are as follows:

{TXT:EDDI docking cancelled station} The station at which the commander has cancelled docking

### Docking denied
Triggered when your ship is denied docking at a station or outpost.
To run a command when this event occurs you should create the command with the name ((EDDI docking denied))

Variables set with this events are as follows:

{TXT:EDDI docking denied reason} The station at which the commander has been denied docking
{TXT:EDDI docking denied station} The station at which the commander has been denied docking

### Docking granted
Triggered when your ship is granted docking permission at a station or outpost.
To run a command when this event occurs you should create the command with the name ((EDDI docking granted))

Variables set with this events are as follows:

{INT:EDDI docking granted landingpad} The landing apd at which the commander has been granted docking
{TXT:EDDI docking granted station} The station at which the commander has been granted docking

### Docking requested
Triggered when your ship requests docking at a station or outpost.
To run a command when this event occurs you should create the command with the name ((EDDI docking requested))

Variables set with this events are as follows:

{TXT:EDDI docking requested station} The station at which the commander has requested docking

### Docking timed out
Triggered when your docking request times out.
To run a command when this event occurs you should create the command with the name ((EDDI docking timed out))

Variables set with this events are as follows:

{TXT:EDDI docking timed out station} The station at which the docking request has timed out

### Entered normal space
Triggered when your ship enters normal space.
To run a command when this event occurs you should create the command with the name ((EDDI entered normal space))

Variables set with this events are as follows:

{TXT:EDDI entered normal space body} The nearest body to the commander when entering normal space
{TXT:EDDI entered normal space bodytype} The type of the nearest body to the commander when entering normal space
{TXT:EDDI entered normal space system} The system at which the commander has entered normal space

### Entered signal source
Triggered when your ship enters a signal source.
To run a command when this event occurs you should create the command with the name ((EDDI entered signal source))

Variables set with this events are as follows:

{TXT:EDDI entered signal source source} The type of the signal source
{INT:EDDI entered signal source threat} The threat level of the signal source (0-4)

### Entered supercruise
Triggered when your ship enters supercruise.
To run a command when this event occurs you should create the command with the name ((EDDI entered supercruise))

Variables set with this events are as follows:

{TXT:EDDI entered supercruise system} The system at which the commander has entered supercruise

### Exploration data purchased
Triggered when you purchase exploration data.
To run a command when this event occurs you should create the command with the name ((EDDI exploration data purchased))

Variables set with this events are as follows:

{DEC:EDDI exploration data purchased price} The price of the purchase
{TXT:EDDI exploration data purchased system} The system for which the exploration data was purchased

### Exploration data sold
Triggered when you sell exploration data.
To run a command when this event occurs you should create the command with the name ((EDDI exploration data sold))

Variables set with this events are as follows:

{DEC:EDDI exploration data sold bonus} The bonus for first discovereds
{DEC:EDDI exploration data sold reward} The reward for selling the exploration data

### Exploration promotion
Triggered when your exploration rank increases.
To run a command when this event occurs you should create the command with the name ((EDDI exploration promotion))

Variables set with this events are as follows:

{TXT:EDDI exploration promotion rating} The commander's new exploration rating

### Fighter docked
Triggered when you dock a fighter with your ship.
To run a command when this event occurs you should create the command with the name ((EDDI fighter docked))

### Fighter launched
Triggered when you launch a fighter from your ship.
To run a command when this event occurs you should create the command with the name ((EDDI fighter launched))

Variables set with this events are as follows:

{TXT:EDDI fighter launched loadout} The fighter's loadout
{BOOL:EDDI fighter launched playercontrolled} True if the fighter is controlled by the player

### Fine incurred
Triggered when your incur a fine.
To run a command when this event occurs you should create the command with the name ((EDDI fine incurred))

Variables set with this events are as follows:

{TXT:EDDI fine incurred crimetype} The type of crime committed
{TXT:EDDI fine incurred faction} The name of the faction issuing the fine
{TXT:EDDI fine incurred victim} The name of the victim of the crime

### Fine paid
Triggered when you pay a fine.
To run a command when this event occurs you should create the command with the name ((EDDI fine paid))

Variables set with this events are as follows:

{DEC:EDDI fine paid amount} The amount of the fine paid
{BOOL:EDDI fine paid legacy} True if the payment is for a legacy fine

### Heat damage
Triggered when your ship is taking damage from excessive heat.
To run a command when this event occurs you should create the command with the name ((EDDI heat damage))

### Heat warning
Triggered when your ship's heat exceeds 100%.
To run a command when this event occurs you should create the command with the name ((EDDI heat warning))

### Hull damaged
Triggered when your hull is damaged to a certain extent.
To run a command when this event occurs you should create the command with the name ((EDDI hull damaged))

Variables set with this events are as follows:

{DEC:EDDI hull damaged health} The percentage health of the hull

### Jumped
Triggered when you complete a jump to another system.
To run a command when this event occurs you should create the command with the name ((EDDI jumped))

Variables set with this events are as follows:

{TXT:EDDI jumped allegiance} The allegiance of the system to which the commander has jumped
{TXT:EDDI jumped economy} The economy of the system to which the commander has jumped
{TXT:EDDI jumped faction} The faction controlling the system to which the commander has jumped
{TXT:EDDI jumped factionstate} The state of the faction controlling the system to which the commander has jumped
{TXT:EDDI jumped government} The government of the system to which the commander has jumped
{TXT:EDDI jumped security} The security of the system to which the commander has jumped
{TXT:EDDI jumped system} The name of the system to which the commander has jumped
{DEC:EDDI jumped x} The X co-ordinate of the system to which the commander has jumped
{DEC:EDDI jumped y} The Y co-ordinate of the system to which the commander has jumped
{DEC:EDDI jumped z} The Z co-ordinate of the system to which the commander has jumped

### Jumping
Triggered when you start a jump to another system.
To run a command when this event occurs you should create the command with the name ((EDDI jumping))

Variables set with this events are as follows:

{TXT:EDDI jumping system} The name of the system to which the commander is jumping
{DEC:EDDI jumping x} The X co-ordinate of the system to which the commander is jumping
{DEC:EDDI jumping y} The Y co-ordinate of the system to which the commander is jumping
{DEC:EDDI jumping z} The Z co-ordinate of the system to which the commander is jumping

### Killed
Triggered when you kill another player.
To run a command when this event occurs you should create the command with the name ((EDDI killed))

Variables set with this events are as follows:

{TXT:EDDI killed rating} The combat rating of the player killed
{TXT:EDDI killed victim} The name of the player killed

### Liftoff
Triggered when your ship lifts off from a planet's surface.
To run a command when this event occurs you should create the command with the name ((EDDI liftoff))

Variables set with this events are as follows:

{DEC:EDDI liftoff latitude} The latitude from where the commander has lifted off
{DEC:EDDI liftoff longitude} The longitude from where the commander has lifted off

### Limpet purchased
Triggered when you buy limpets from a station.
To run a command when this event occurs you should create the command with the name ((EDDI limpet purchased))

Variables set with this events are as follows:

{INT:EDDI limpet purchased amount} The amount of limpets purchased
{DEC:EDDI limpet purchased price} The price paid per limpet

### Limpet sold
Triggered when you sell limpets to a station.
To run a command when this event occurs you should create the command with the name ((EDDI limpet sold))

Variables set with this events are as follows:

{INT:EDDI limpet sold amount} The amount of limpets sold
{DEC:EDDI limpet sold price} The price obtained per limpet

### Location
Triggered when the commander's location is reported, usually when they reload their game..
To run a command when this event occurs you should create the command with the name ((EDDI location))

Variables set with this events are as follows:

{TXT:EDDI location allegiance} The allegiance of the system in which the commander resides
{TXT:EDDI location body} The nearest body to the commander
{TXT:EDDI location bodytype} The type of the nearest body to the commander
{BOOL:EDDI location docked} True if the commander is docked
{TXT:EDDI location economy} The economy of the system in which the commander resides
{TXT:EDDI location faction} The faction controlling the system in which the commander resides
{TXT:EDDI location factionstate} The state of the faction controlling the system in which the commander resides
{TXT:EDDI location government} The government of the system in which the commander resides
{TXT:EDDI location security} The security of the system in which the commander resides
{TXT:EDDI location system} The name of the system in which the commander resides
{DEC:EDDI location x} The X co-ordinate of the system in which the commander resides
{DEC:EDDI location y} The Y co-ordinate of the system in which the commander resides
{DEC:EDDI location z} The Z co-ordinate of the system in which the commander resides

### Material collected
Triggered when you collect a material.
To run a command when this event occurs you should create the command with the name ((EDDI material collected))

Variables set with this events are as follows:

{INT:EDDI material collected amount} The amount of the collected material
{TXT:EDDI material collected name} The name of the collected material

### Material discarded
Triggered when you discard a material.
To run a command when this event occurs you should create the command with the name ((EDDI material discarded))

Variables set with this events are as follows:

{INT:EDDI material discarded amount} The amount of the discarded material
{TXT:EDDI material discarded name} The name of the discarded material

### Material discovered
Triggered when you discover a material.
To run a command when this event occurs you should create the command with the name ((EDDI material discovered))

Variables set with this events are as follows:

{TXT:EDDI material discovered name} The name of the discovered material

### Message received
Triggered when you receive a message.
To run a command when this event occurs you should create the command with the name ((EDDI message received))

Variables set with this events are as follows:

{TXT:EDDI message received channel} The channel in which the message came (direct, local, wing)
{TXT:EDDI message received from} The name of the pilot who sent the message
{TXT:EDDI message received message} The message
{BOOL:EDDI message received player} True if the sender is a player

### Message sent
Triggered when you send a message.
To run a command when this event occurs you should create the command with the name ((EDDI message sent))

Variables set with this events are as follows:

{TXT:EDDI message sent message} The message
{TXT:EDDI message sent to} The name of the player to which the message was sent

### Mission abandoned
Triggered when you abandon a mission.
To run a command when this event occurs you should create the command with the name ((EDDI mission abandoned))

Variables set with this events are as follows:

{DEC:EDDI mission abandoned missionid} The ID of the mission
{TXT:EDDI mission abandoned name} The name of the mission

### Mission accepted
Triggered when you accept a mission.
To run a command when this event occurs you should create the command with the name ((EDDI mission accepted))

Variables set with this events are as follows:

{BOOL:EDDI mission accepted communal} True if the mission is a community goal
{TXT:EDDI mission accepted faction} The faction issuing the mission
{TXT:EDDI mission accepted name} The name of the mission
{TXT:EDDI mission accepted system} The system in which the mission was obtained

### Mission completed
Triggered when you complete a mission.
To run a command when this event occurs you should create the command with the name ((EDDI mission completed))

Variables set with this events are as follows:

{BOOL:EDDI mission completed communal} True if the mission is a community goal
{DEC:EDDI mission completed donation} The monetary donatin when completing the mission
{TXT:EDDI mission completed name} The name of the mission
{DEC:EDDI mission completed reward} The monetary reward for completing the mission
{TXT:EDDI mission completed system} The system in which the mission was obtained

### Screenshot
Triggered when you take a screenshot.
To run a command when this event occurs you should create the command with the name ((EDDI screenshot))

Variables set with this events are as follows:

{TXT:EDDI screenshot body} The name of the nearest body to where the screenshot was taken
{TXT:EDDI screenshot filename} The name of the file where the screenshot has been saved
{INT:EDDI screenshot height} The height in pixels of the screenshot
{TXT:EDDI screenshot system} The name of the system where the screenshot was taken
{INT:EDDI screenshot width} The width in pixels of the screenshot

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

Variables set with this events are as follows:

{TXT:EDDI ship delivered ship} The ship that was delivered

### Ship interdicted
Triggered when your ship is interdicted by another ship.
To run a command when this event occurs you should create the command with the name ((EDDI ship interdicted))

Variables set with this events are as follows:

{TXT:EDDI ship interdicted faction} The faction of the commander carrying out the interdiction
{TXT:EDDI ship interdicted interdictor} The name of the commander carrying out the interdiction
{BOOL:EDDI ship interdicted iscommander} If the player carrying out the interdiction is a commander (as opposed to an NPC)
{TXT:EDDI ship interdicted power} The power of the commander carrying out the interdiction
{TXT:EDDI ship interdicted rating} The combat rating of the commander carrying out the interdiction
{BOOL:EDDI ship interdicted submitted} If the commander submitted to the interdiction
{BOOL:EDDI ship interdicted succeeded} If the interdiction attempt was successful

### Ship interdiction
Triggered when you interdict another ship.
To run a command when this event occurs you should create the command with the name ((EDDI ship interdiction))

Variables set with this events are as follows:

{TXT:EDDI ship interdiction faction} The faction of the commander being interdicted
{TXT:EDDI ship interdiction interdictee} The name of the commander being interdicted
{BOOL:EDDI ship interdiction iscommander} If the player being interdicted is a commander (as opposed to an NPC)
{TXT:EDDI ship interdiction power} The power of the commander being interdicted
{TXT:EDDI ship interdiction rating} The combat rating of the commander being interdicted
{BOOL:EDDI ship interdiction succeeded} If the interdiction attempt was successful

### Ship purchased
Triggered when you purchase a ship.
To run a command when this event occurs you should create the command with the name ((EDDI ship purchased))

Variables set with this events are as follows:

{DEC:EDDI ship purchased price} The price of the ship that was purchased
{TXT:EDDI ship purchased ship} The ship that was purchased
{TXT:EDDI ship purchased soldname} The name of the ship that was sold as part of the purchase
{TXT:EDDI ship purchased soldship} The ship that was sold as part of the purchase
{TXT:EDDI ship purchased storedname} The name of the ship that was stored as part of the purchase
{TXT:EDDI ship purchased storedship} The ship that was stored as part of the purchase

### Ship rebooted
Triggered when you run reboot/repair on your ship.
To run a command when this event occurs you should create the command with the name ((EDDI ship rebooted))

Variables set with this events are as follows:


### Ship refuelled
Triggered when you refuel your ship.
To run a command when this event occurs you should create the command with the name ((EDDI ship refuelled))

Variables set with this events are as follows:

{DEC:EDDI ship refuelled amount} The amount of fuel supplied
{DEC:EDDI ship refuelled price} The price of refuelling

### Ship repaired
Triggered when you repair your ship.
To run a command when this event occurs you should create the command with the name ((EDDI ship repaired))

Variables set with this events are as follows:

{TXT:EDDI ship repaired item} The item repaired, if repairing a specific item
{DEC:EDDI ship repaired price} The price of refuelling

### Ship restocked
Triggered when you restock your ship's ammunition.
To run a command when this event occurs you should create the command with the name ((EDDI ship restocked))

Variables set with this events are as follows:

{DEC:EDDI ship restocked price} The price of restocking

### Ship sold
Triggered when you sell a ship.
To run a command when this event occurs you should create the command with the name ((EDDI ship sold))

Variables set with this events are as follows:

{DEC:EDDI ship sold price} The price for which the ship was sold
{TXT:EDDI ship sold ship} The ship that was sold

### Ship swapped
Triggered when you swap a ship.
To run a command when this event occurs you should create the command with the name ((EDDI ship swapped))

Variables set with this events are as follows:

{TXT:EDDI ship swapped ship} The ship that was swapped
{TXT:EDDI ship swapped soldship} The ship that was sold as part of the swap
{TXT:EDDI ship swapped storedship} The ship that was stored as part of the swap

### Ship transfer initiated
Triggered when you initiate a ship transfer.
To run a command when this event occurs you should create the command with the name ((EDDI ship transfer initiated))

Variables set with this events are as follows:

{DEC:EDDI ship transfer initiated distance} The distance that the transferred ship needs to travel, in light years
{DEC:EDDI ship transfer initiated price} The price of transferring the ship
{TXT:EDDI ship transfer initiated ship} The ship that is being transferred
{TXT:EDDI ship transfer initiated system} The system from which the ship is being transferred

### SRV docked
Triggered when you dock an SRV with your ship.
To run a command when this event occurs you should create the command with the name ((EDDI srv docked))

### SRV launched
Triggered when you launch an SRV from your ship.
To run a command when this event occurs you should create the command with the name ((EDDI srv launched))

Variables set with this events are as follows:

{TXT:EDDI srv launched loadout} The SRV's loadout

### Star scanned
Triggered when you complete a scan of a stellar body.
To run a command when this event occurs you should create the command with the name ((EDDI star scanned))

Variables set with this events are as follows:

{DEC:EDDI star scanned absolutemagnitude} The absolute magnitude of the star that has been scanned
{DEC:EDDI star scanned age} The age of the star that has been scanned, in years (rounded to millions of years)
{TXT:EDDI star scanned chromaticity} The apparent colour of the star that has been scanned
{DEC:EDDI star scanned distancefromarrival} The distance in LS from the main star
{DEC:EDDI star scanned eccentricity} 
{DEC:EDDI star scanned luminosity} The luminosity of the star that has been scanned
{DEC:EDDI star scanned luminosityprobability} The probablility of finding a star of this class and at least this luminosity
{DEC:EDDI star scanned massprobability} The probablility of finding a star of this class and at least this mass
{TXT:EDDI star scanned name} The name of the star that has been scanned
{DEC:EDDI star scanned orbitalinclination} 
{DEC:EDDI star scanned orbitalperiod} The number of seconds taken for a full orbit of the main star
{DEC:EDDI star scanned periapsis} 
{DEC:EDDI star scanned radius} The radius of the star that has been scanned, in metres
{DEC:EDDI star scanned radiusprobability} The probablility of finding a star of this class and at least this radius
{DEC:EDDI star scanned rotationperiod} The number of seconds taken for a full rotation
{DEC:EDDI star scanned semimajoraxis} 
{DEC:EDDI star scanned solarmass} The mass of the star that has been scanned, relative to Sol's mass
{DEC:EDDI star scanned solarradius} The radius of the star that has been scanned, compared to Sol
{TXT:EDDI star scanned stellarclass} The stellar class of the star that has been scanned (O, G, etc)
{DEC:EDDI star scanned temperature} The temperature of the star that has been scanned

### Touchdown
Triggered when your ship touches down on a planet's surface.
To run a command when this event occurs you should create the command with the name ((EDDI touchdown))

Variables set with this events are as follows:

{DEC:EDDI touchdown latitude} The latitude from where the commander has touched down
{DEC:EDDI touchdown longitude} The longitude from where the commander has touched down

### Trade data purchased
Triggered when you purchase trade data.
To run a command when this event occurs you should create the command with the name ((EDDI trade data purchased))

Variables set with this events are as follows:

{DEC:EDDI trade data purchased price} The price of the purchase
{TXT:EDDI trade data purchased system} The system for which trade data was purchased

### Trade promotion
Triggered when you trade rank increases.
To run a command when this event occurs you should create the command with the name ((EDDI trade promotion))

Variables set with this events are as follows:

{TXT:EDDI trade promotion rating} The commander's new trade rating

### Undocked
Triggered when your ship undocks from a station or outpost.
To run a command when this event occurs you should create the command with the name ((EDDI undocked))

Variables set with this events are as follows:

{TXT:EDDI undocked station} The station from which the commander has undocked


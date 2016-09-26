# Scripting with EDDI

EDDI's speech responder uses Cottle for scripting.  Cottle has a number of great features, including:

    * Ability to set and update variables, including arrays
    * Loops
    * Conditionals
    * Subroutines

Information on how to write Cottle scripts is available at http://r3c.github.io/cottle/#toc-2, and EDDI's default scripts use a lot of the functions available.

## EDDI objects

EDDI provides a number of objects that scripts can access.  The information available can enhance the scripts and make them reactive to your own status.  Details of the objects available are as follows:

### Commander

Commander information is available under the 'cmdr' object.  

Any values might be missing, depending on EDDI's configuration.

  - name the Commander's name
  - phoneticname the pronunciation of the commander's name 
  - combatrating the current combat rating of the commander (this is a Rating object)
  - traderating the current trad rating of the commander (this is a Rating object)
  - explorationrating the exploration combat rating of the commander (this is a Rating object)
  - empirerating the current Empire rating of the commander (this is a Rating object)
  - federationrating the current Federation rating of the commander (this is a Rating object)
  - credits the number of credits the commander owns
  - debt the amount of debt the commander owes

### Ship

Information about your current ship is available under the 'ship' object.

Any values might be missing, depending on EDDI's configuration.

    - manufacturer the manufacturer of the ship (Lakon, Core Dynamics etc)
	- model the model of the ship (Cobra Mk III, Fer-de-Lance etc)
	- size the size of the ship (small/medium/large)
	- value the value of the ship without cargo, in credits
	- cargocapacity the total tonnage cargo capacity
	- cargocarried the current tonnage cargo carried
	- cargo specific details on the cargo being carried
	- callsign the callsign for the ship
	- name the name of the ship
    - role the role of the ship 
	- health the current health of the hull, from 0 to 100
	- bulkheads - details of the ship's bulkheads (this is a Module object)
	- powerplant - details of the ship's powerplant (this is a Module object)
	- thrusters - details of the ship's thrusters (this is a Module object)
	- frameshiftdrive - details of the ship's FSD (this is a Module object)
	- lifesupport - details of the ship's life support (this is a Module object)
	- powerdistributor - details of the ship's power distributor (this is a Module object)
	- sensors - details of the ship's sensors (this is a Module object)
	- fueltank - details of the ship's fuel tank (this is a Module object)
	- hardpoints - the ship's hardpoints (this is an array of HardPoint objects)
	- compartments - the ship's internal compartments (this is an array of Compartment objects)

### Current starsystem

Information about your current starsystem is avaialble under the 'system' object.

Any values might be missing, depending on EDDI's configuration and the information avaialable about the system.

    - name the name of the starsystem
	- population the population of the starsystem
	- allegiance the superpower allegiance of this starsystem (Federation, Empire etc)
	- government the type of government in this starsystem (Democracy, Confederacy etc)
	- faction the dominant faction in this starsystem
	- primaryeconomy the primary economy in this starsystem (High Technology, Agriculture, etc)
	- state the state of the starsystem (Boom, War, etc)
	- security the level of security in the starsystem (Low, Medium, High)
	- power the power who is controlling the starsystem (Edmund Mahon, Zachary Hudson etc)
	- powerstate the state of the system for the power (controlled, contested etc)
    - stations the starsystem's stations (array of Station objects)
	- bodies the starsystem's bodies (array of Body objects)
	- visits the number of visits that the commander has made to this starsystem
	- distancefromhome the distance in LY from the commander's home starsystem
	- comment any comment the commander has made on the starsystem

### Last starsystem

Information about your current starsystem is avaialble under the 'lastsystem' object.

Any values might be missing, depending on EDDI's configuration and the information avaialable about the system.

Values are the same as for the current starsystem.

### Home starsystem

Information about your home starsystem is available under the 'homesystem' object.

Any values might be missing, depending on EDDI's configuration and the information avaialable about the system.

Values are the same as for the current starsystem.

### Event

The event that triggered the speech responder.  Information held in here is event-specific and can be found by clicking on the 'Variables' button when editing the script.

### Rating

A rating, for example a combat rating or empire rating.

  - name the name of the rating, for example 'Harmless'
  - rank the numeric rank of the rating, for example 0

### Module

An internal module.

  - name the name of the module
  - class the numeric class of the module
  - grade the character grade of the module
  - value the base value of the module
  - cost the amount of credits paid for the module
  - enabled if the module is currently enabled
  - priority the current power priority of the module
  - health the current health of the module
  - mount only for weapons, this defines the type of mount (fixed, gimballed, turreted)
  - clipcapacity only for weapons with ammunition, this defines the clip capacity of the loaded weapon
  - hoppercapacity only for weapons with ammunition, this defines the hopper capacity of the loaded weapon

### Hardpoint

A harpoint, which may or may not contain a moudlemodule.

  - size the numeric size of the hardpoint, from 0 (utility) to 4 (huge)
  - module the module in the hardpoint

### Compartment

A compartment, which may or may not contain a moudlemodule.

  - size the numeric size of the compartment, from 1 to 8
  - module the module in the compartment

### Station

### Body

## EDDI Functions

In addition to the basic Cottle features EDDI has a number of features that provide added functionality and specific information for Elite: Dangerous.  Details of these functions are as follows:

### P()

This function will attempt to provide phonetic pronunciation for the supplied text.

P() takes a single argument of the string for which to alter the pronunciation.

Common usage of this is to wrap the names of planets, powers, ships etc., for example:

    You are in the {P(system.name)} system.

### OneOf()

This function will take one of the arguments available to it, picking randomly.

OneOf() takes as many arguments are you want to give it.

Common usage of this is to provide variation to spoken text, for example:

    You have {OneOf("docked", "finished docking", "completed docking procedures")}.

### Occasionally()

This function will take its argument 1/*n*th of the time, the rest of time discarding it.

Occasionally() takes two arguments: n, and the text argument.

Common usage of this is to provide additional text that is said now and again but would become irritating if said all the time, for example:

   Boost engaged.  {Occasionally(7, "Hold on to something.")}

Note that Occasionally() works on random numbers rather than counters, so in the above example the additional text will not show up every 7th time you boost but will show up on average 1/7 of the times that you boost.

### Humanise()

This function will turn its argument into a more human number, for example turning 31245 in to "just over thirty thousand".

Humanise() takes one argument: the number to humanise.

Common usage of this is to provide human-sounding numbers when speacking rather than saying every digit, for example:

   You have {Humanise(cmdr.credits)} credits.

### ShipDetails()

This function will provide full information for a ship given its name.

ShipDetails() takes a single argument of the model of the ship for which you want more information.

Common usage of this is to provide further information about a ship, for example:

    The Vulture is made by {ShipDetails("Vulture").manufacturer}.

### CombatRatingDetails()

This function will provide full information for a combat rating given its name.

CombatRatingDetails() takes a single argument of the combat rating for which you want more information.

Common usage of this is to provide further information about your rating, for example:

    You have been promoted {CombatRatingDetails("Expert").rank} times.

### TradeRatingDetails()

This function will provide full information for a trade rating given its name.

TradeRatingDetails() takes a single argument of the trade rating for which you want more information.

Common usage of this is to provide further information about your rating, for example:

    You have been promoted {TradeRatingDetails("Peddler").rank} times.

### ExplorationRatingDetails()

This function will provide full information for an exploration rating given its name.

ExplorationRatingDetails() takes a single argument of the exploration rating for which you want more information.

Common usage of this is to provide further information about your rating, for example:

    You have been promoted {ExplorationRatingDetails("Surveyor").rank} times.

### EmpireRatingDetails()

This function will provide full information for an empire rating given its name.

EmpireRatingDetails() takes a single argument of the empire rating for which you want more information.

Common usage of this is to provide further information about your rating, for example:

    You have been promoted {EmpireRatingDetails("Lord").rank} times.

### FederationRatingDetails()

This function will provide full information for an federation rating given its name.

FederationRatingDetails() takes a single argument of the Federation rating for which you want more information.

Common usage of this is to provide further information about your rating, for example:

    You have been promoted {FederationRatingDetails("Post Commander").rank} times.

### SystemDetails()

This function will provide full information for a star system given its name.

SystemDetails() takes a single argument of the star system for which you want more information.

Common usage of this is to provide further information about a star system, for example:

    Sol has {len(SystemDetails("Sol").bodies)} bodies.

### StationDetails()

This function will provide full information for a station given its name.

StationDetails() takes two arguments of the station ane the starsystem of the station.

Common usage of this is to provide further information about a station's capabilities, for example:

    Jameson Memorial is {StationDetails("Jameson Memorial", "Shinrarta Dezhra").distancefromstar} light seconds from the main star.

### SuperpowerDetails()

This function will provide full information for a superpower given its name.

SuperpowerDetails() takes a single argument of the superpower for which you want more information.

At current this does not have a lot of use as the superpower object only contains its name, but expect it to be expanded in future.

### StateDetails()

This function will provide full information for a state given its name.

StateDetails() takes a single argument of the state for which you want more information.

At current this does not have a lot of use as the state object only contains its name, but expect it to be expanded in future.

### EconomyDetails()

This function will provide full information for an economy given its name.

EconomyDetails() takes a single argument of the economy for which you want more information.

At current this does not have a lot of use as the economy object only contains its name, but expect it to be expanded in future.

### GovernmentDetails()

This function will provide full information for a government given its name.

GovernmentDetails() takes a single argument of the government for which you want more information.

At current this does not have a lot of use as the government object only contains its name, but expect it to be expanded in future.

### SecurityLevelDetails()

This function will provide full information for a security level given its name.

SecurityLevelDetails() takes a single argument of the security level for which you want more information.

At current this does not have a lot of use as the security level object only contains its name, but expect it to be expanded in future.

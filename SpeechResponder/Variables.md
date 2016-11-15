# EDDI Variables

EDDI provides many variables that can be used to help provide immersive scripts and more interesting.  Most variables are held as objects that are exposed from EDDI.  Access to the property of the object is using a period, for example `cmdr.name` would obtain the `name` value from the `cmdr` object.

EDDI can also have event-specific variables in the `event` object.  These usually come from external sources and so are not part of the standard objects, although they might reference them.

Details of the objects available are as follows:

## Commander

Commander information is available under the `cmdr` object.  

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

## Ship

Information about your current ship is available under the `ship` object.

Any values might be missing, depending on EDDI's configuration.

    - manufacturer the manufacturer of the ship (Lakon, Core Dynamics etc)
    - model the model of the ship (Cobra Mk III, Fer-de-Lance etc)
    - size the size of the ship (small/medium/large)
    - value the value of the ship without cargo, in credits
    - cargocapacity the total tonnage cargo capacity
    - cargocarried the current tonnage cargo carried
    - cargo specific details on the cargo being carried
    - name the name of the ship
    - role the role of the ship 
    - health the current health of the hull, from 0 to 100
    - bulkheads details of the ship's bulkheads (this is a Module object)
    - powerplant details of the ship's powerplant (this is a Module object)
    - thrusters details of the ship's thrusters (this is a Module object)
    - frameshiftdrive details of the ship's FSD (this is a Module object)
    - lifesupport details of the ship's life support (this is a Module object)
    - powerdistributor details of the ship's power distributor (this is a Module object)
    - sensors details of the ship's sensors (this is a Module object)
    - fueltank details of the ship's fuel tank (this is a Module object)
    - hardpoints the ship's hardpoints (this is an array of HardPoint objects)
    - compartments the ship's internal compartments (this is an array of Compartment objects)

## Current starsystem

Information about your current starsystem is avaialble under the `system` object.

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

## Last starsystem

Information about your current starsystem is avaialble under the `lastsystem` object.

Any values might be missing, depending on EDDI's configuration and the information avaialable about the system.

Values are the same as for the current starsystem.

## Home starsystem

Information about your home starsystem is available under the `homesystem` object.

Any values might be missing, depending on EDDI's configuration and the information avaialable about the system.

Values are the same as for the current starsystem.

## Home station

Information about your home starsystem is available under the `homestation` object.

Any values might be missing, depending on EDDI's configuration and the information avaialable about the system.

Values are below in the 'Station' object.

## Event

The event that triggered the speech responder.  Information held in here is event-specific and can be found by clicking on the 'Variables' button when editing the script.

## Rating

A rating, for example a combat rating or empire rating.

    - name the name of the rating, for example 'Harmless'
    - rank the numeric rank of the rating, for example 0

## Module

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

## Hardpoint

A harpoint, which may or may not contain a module.

    - size the numeric size of the hardpoint, from 0 (utility) to 4 (huge)
    - module the module in the hardpoint

## Compartment

A compartment, which may or may not contain a module.

    - size the numeric size of the compartment, from 1 to 8
    - module the module in the compartment

## Material

A material.

    - name the name of the material (e.g. Iron)
    - category the category of the material (Element, Data or Manufactured)
    - rarity the rarity of the material

## Rarity

A rarity rating.

    - name the name of the rarity (e.g. Very common)
    - level the level of the rarity (0 == Very common, 5 == Very rare)

## Station

An orbital or planetary station.

    - name the name of the station
    - systemname the name of the system in which this station resides
    - model the model of the sation (Orbis, Coriolis, etc)
    - government the type of government in this station (Democracy, Confederacy etc)
    - faction the faction that controls this station
    - allegiance the superpower allegiance of the faction that controls this station (Federation, Empire etc)
    - state the state of the station (Boom, War, etc)
    - economies an array of economies at this station
    - distancefromstar the distance from the main star to this station (in light years)
    - hasrefuel true if this station has refuelling
    - hasrearm true if this station has rearming
    - hasrepair true if this station has repairs
    - hasoutfitting true if this station has outfitting
    - hasshipyard true if this station has a shipyard
    - hasmarket true if this station has a market
    - hasblackmarket true if this station has a black market
    - largestpad the largest pad available at this station (None, Small, Medium, Large)
    - commodities the commodities that are bought and sold by this station (array of Commodity objects)
    - outfitting the modules that are available for outfitting at this station (array of Module objects)

## Body

A star or planetary body.

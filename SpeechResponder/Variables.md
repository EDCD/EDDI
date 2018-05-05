# EDDI Variables

EDDI provides many variables that can be used to help provide immersive scripts and more interesting.  Most variables are held as objects that are exposed from EDDI.  Access to the property of the object is using a period, for example `cmdr.name` would obtain the `name` value from the `cmdr` object.

EDDI can also have event-specific variables in the `event` object.  These usually come from external sources and so are not part of the standard objects, although they might reference them.

Details of the objects available are as follows:

## Game state

Information on game state is available at the top level i.e. these values can be accessed directly.

    - `environment` the commander's current environment.  Can be one of "Normal space", "Supercruise" or "Witch space"
    - `vehicle` the vehicle that is under the commander's control.  Can be one of "Ship", "SRV" or "Fighter"

## Commander

Commander information is available under the `cmdr` object.  

Any values might be missing, depending on EDDI's configuration.

    - `name` the commander's name
    - `phoneticname` the pronunciation of the commander's name 
    - `combatrating` the current combat rating of the commander (this is a Rating object)
    - `traderating` the current trade rating of the commander (this is a Rating object)
    - `explorationrating` the exploration combat rating of the commander (this is a Rating object)
    - `empirerating` the current Empire rating of the commander (this is a Rating object)
    - `federationrating` the current Federation rating of the commander (this is a Rating object)
    - `credits` the number of credits the commander owns
    - `debt` the amount of debt the commander owes
    - `gender` the gender of the commander, as selected in EDDI's configuration (string, either 'Male', 'Female' or 'Neither')
    - `friends` the commander's friends observed during the play session (this is a Friend object)

## Ship

Information about your current ship is available under the `ship` object.

Any values might be missing, depending on EDDI's configuration.

    - `manufacturer` the manufacturer of the ship (Lakon, Core Dynamics etc)
    - `model` the model of the ship (Cobra Mk III, Fer-de-Lance etc)
    - `size` the size of the ship (small/medium/large)
    - `value` the value of the ship without cargo, in credits
    - `cargocapacity` the total tonnage cargo capacity
    - `cargocarried` the current tonnage cargo carried
    - `name` the name of the ship
    - `role` the role of the ship 
    - `health` the current health of the hull, from 0 to 100
    - `bulkheads` details of the ship's bulkheads (this is a Module object)
    - `powerplant` details of the ship's powerplant (this is a Module object)
    - `thrusters` details of the ship's thrusters (this is a Module object)
    - `frameshiftdrive` details of the ship's FSD (this is a Module object)
    - `lifesupport` details of the ship's life support (this is a Module object)
    - `powerdistributor` details of the ship's power distributor (this is a Module object)
    - `sensors` details of the ship's sensors (this is a Module object)
    - `fueltank` details of the ship's fuel tank (this is a Module object)
    - `fueltankcapacity` the capacity of the main fuel tank
    - `fueltanktotalcapacity` the capacity of the main fuel tank plus all secondary fuel tanks
    - `hardpoints` the ship's hardpoints (this is an array of HardPoint objects)
    - `compartments` the ship's internal compartments (this is an array of Compartment objects)
    - `launchbays` the ship's internal hangars, containing SRV or Fighter 'vehicles' (this is an array of launchbay objects) 


## Status

Information about your current status is available under the `status` object.

Any values might be missing, depending on EDDI's configuration.

    - `vehicle` the vehicle that is under the commander's control.  Can be one of "Ship", "SRV" or "Fighter"
    - `being_interdicted` a boolean value indicating whether the commander is currently being interdicted
    - `in_danger` a boolean value indicating whether the commander is currently in danger
    - `near_surface` a boolean value indicating whether the commander is near a landable surface (within it's gravity well)
    - `overheating` a boolean value indicating whether the commander's vehicle is overheating
    - `low_fuel` a boolean value indicating whether the commander has less than 25% fuel remaining
    - `fsd_status` the current status of the ship's frame shift drive. Can be one of "ready", "cooldown", "charging", or "masslock"
    - `srv_drive_assist` a boolean value indicating whether SRV drive assist is active
    - `srv_under_ship` a boolean value indicating whether the SRV in within the proximity zone around the ship
    - `srv_turret_deployed` a boolean value indicating whether the SRV's turret has been deployed
    - `srv_handbrake_activated` a boolean value indicating whether the SRV's handbrake has been activated
    - `scooping_fuel` a boolean value indicating whether the ship is currently scooping fuel
    - `silent_running` a boolean value indicating whether silent running is active
    - `cargo_scoop_deployed` a boolean value indicating whether the cargo scoop has been deployed
    - `lights_on` a boolean value indicating whether the vehicle's external lights are active
    - `in_wing` a boolean value indicating whether the commander is currently in a wing
    - `hardpoints_deployed` a boolean value indicating whether hardpoints are currently deployed
    - `flight_assist_off` a boolean value indicating whether flight assistance has been deactivated
    - `supercruise` a boolean value indicating whether the ship is currently in supercruise
    - `shields_up` a boolean value indicating whether the ship's shields are maintaining their integrity
    - `landing_gear_down` a boolean value indicating whether the ship's landing gears have been deployed
    - `landed` a boolean value indicating whether the ship is currently landed (on a surface)
    - `docked` a boolean value indicating whether the ship is currently docked (at a station)
    - `pips_sys` a decimal value indicating the power distributor allocation to systems
    - `pips_eng` a decimal value indicating the power distributor allocation to engines
    - `pips_wea` a decimal value indicating the power distributor allocation to weapons
    - `firegroup` an integer value indicating the ship's currently selected firegroup
    - `gui_focus` the commander's current focus. Can be one of "none", "internal panel" (right panel), "external panel" (left panel), "communications panel" (top panel), "role panel" (bottom panel), "station services", "galaxy map", or "system map"
    - `latitude` a decimal value indicating the ship's current latitude (if near a surface)
    - `longitude` a decimal value indicating the ship's current longitude (if near a surface)
    - `altitude` a decimal value indicating the ship's current altitude (if in flight near a surface)
    - `heading` a decimal value indicating the ship's current heading (if near a surface)

## Current starsystem

Information about your current starsystem is available under the `system` object.

Any values might be missing, depending on EDDI's configuration and the information available about the system.

    - `name` the name of the starsystem
    - `population` the population of the starsystem
    - `allegiance` the superpower allegiance of this starsystem (Federation, Empire etc)
    - `government` the type of government in this starsystem (Democracy, Confederacy etc)
    - `faction` the dominant faction in this starsystem
    - `primaryeconomy` the primary economy in this starsystem (High Technology, Agriculture, etc)
    - `state` the state of the starsystem (Boom, War, etc)
    - `security` the level of security in the starsystem (Low, Medium, High)
    - `power` the power who is controlling the starsystem (Edmund Mahon, Zachary Hudson etc)
    - `powerstate` the state of the system for the power (controlled, contested etc)
    - `x` the X co-ordinate of the starsystem
    - `y` the Y co-ordinate of the starsystem
    - `z` the Z co-ordinate of the starsystem
    - `stations` the starsystem's stations (array of Station objects)
    - `bodies` the starsystem's bodies (array of Body objects)
    - `visits` the number of visits that the commander has made to this starsystem
    - `lastvisit` the time that the commander last visited this starsystem
    - `distancefromhome` the distance in LY from the commander's home starsystem
    - `comment` any comment the commander has made on the starsystem
    - `updatedat` the timestamp at which the system information was last updated

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

Any values might be missing, depending on EDDI's configuration and the information available about the system.

Values are below in the 'Station' object.

## Event

The event that triggered the speech responder.  Information held in here is event-specific and can be found by clicking on the 'Variables' button when editing the script.

## Rating

A rating, for example a combat rating or empire rating.

    - `rank` the numeric rank of the rating, for example 0
    - `name` the name of the rating, for example 'Harmless'
    - `femininename` the feminine name of the rating, for example 'Baroness' if it differs, otherwise the same as `name`

## Module

An internal module.

    - `name` the name of the module
    - `class` the numeric class of the module
    - `grade` the character grade of the module
    - `value` the base value of the module
    - `cost` the amount of credits paid for the module
    - `enabled` if the module is currently enabled
    - `priority` the current power priority of the module
    - `health` the current health of the module
    - `mount` only for weapons, this defines the type of mount (fixed, gimballed, turreted)
    - `clipcapacity` only for weapons with ammunition, this defines the clip capacity of the loaded weapon
    - `hoppercapacity` only for weapons with ammunition, this defines the hopper capacity of the loaded weapon

## Hardpoint

A harpoint, which may or may not contain a module.

    - `size` the numeric size of the hardpoint, from 0 (utility) to 4 (huge)
    - `module` the module in the hardpoint

## Compartment

A compartment, which may or may not contain a module.

    - `size` the numeric size of the compartment, from 1 to 8
    - `module` the module in the compartment

## Vehicle

An SRV or Fighter, within an associated launchbay.

    - `name` the name of the vehicle, for example 'F63 Condor'
    - `loadout` the loadout of the vehicle, for example 'Starter' for SRV or 'Gelid' for Fighter
    - `mount` only for Fighters, this defines the type of mount ('F' = fixed, 'G' = gimballed)
    - `rebuilds` the number of rebuilds remaining for the vehicle

## Launchbay

An SRV or Fighter hangar.

    - `type` the of launchbay, 'SRV' of 'Fighter'
    - `size` the numeric size of the launchbay
    - `vehicles` the vehicles within the launchbay (this is an array of vehicle objects)

## Material

A material.

    - `name` the name of the material (e.g. Iron)
    - `category` the category of the material (Element, Data or Manufactured)
    - `rarity` the rarity of the material
    - `blueprints` the blueprints for which the material is used (this is an array of blueprint objects)

## Blueprint

An engineering blueprint.

    - `name` the name of the blueprint (e.g. 'heavy duty')
    - `modulename` the name of the applicable module (e.g. 'shield booster')
    - `grade` the grade of the engineering blueprint (e.g. 'grade 4')

## Inventory

The inventory of cargo carried within the ship is available under the `inventory` object.

    - `cargo` specific details on the cargo being carried

## Cargo

Details of an individual commodity being carried.

    - `name` name of the cargo (e.g. Tea)
    - `commodity` the object containing commodity details
	- `category` the category of the commodity (e.g. Foods, Machinery, Technology)
    - `stolen` the number of units flagged as stolen
    - `haulage` the number of units associated with a mission
    - `owned` the number of units privately purchased or collected (not stolen or mission related)
    - `total` the total number of units
    - `price` the price of an individual unit

## Commodity

A commodity.

    - `name` the name of the commodity (e.g. Tea)
    - `category` the category of the commodity (e.g. Foods, Machinery, Technology)
    - `rare` the rarity of the commodity (boolean true or false)

Additional properties may become available in station, via station.commodities.[property]  
(see Commodity sale check and Commodity purchase check for sample usages).

    - `buyprice` the price to purchase the commodity from the market
    - `sellprice` the price being offered for the commodity in the market
    - `avgprice` the average price offered for this commodity in a legal market
    - `stock` the amount of the commodity available from the market 
    - `demand` the demand for the commodity from the market
    - `EDDBID` the unique id number assigned to represent the commodity in EDDB

## Friend

A friend.

    - `name` the name of the friend
    - `status` the status of the friend (one of the following: Requested, Declined, Added, Lost, Offline, Online)

## Rarity

A rarity rating.

    - `name` the name of the rarity (e.g. Very common)
    - `level` the level of the rarity (0 == Very common, 5 == Very rare)

## Station

An orbital or planetary station.

    - `name` the name of the station
    - `systemname` the name of the system in which this station resides
    - `model` the model of the sation (Orbis, Coriolis, etc)
    - `government` the type of government in this station (Democracy, Confederacy etc)
    - `faction` the faction that controls this station
    - `allegiance` the superpower allegiance of the faction that controls this station (Federation, Empire etc)
    - `state` the state of the station (Boom, War, etc)
    - `primaryeconomy` the primary economy in this station (High Technology, Agriculture, etc)
    - `distancefromstar` the distance from the main star to this station (in light years)
    - `hasrefuel` true if this station has refuelling
    - `hasrearm` true if this station has rearming
    - `hasrepair` true if this station has repairs
    - `hasoutfitting` true if this station has outfitting
    - `hasshipyard` true if this station has a shipyard
    - `hasmarket` true if this station has a market
    - `hasblackmarket` true if this station has a black market
    - `largestpad` the largest pad available at this station (None, Small, Medium, Large)
    - `commodities` the commodities that are bought and sold by this station (array of Commodity objects)
    - `outfitting` the modules that are available for outfitting at this station (array of Module objects)
    - `updatedat` the timestamp at which the station information was last updated
    - `commoditiesupdatedat` the timestamp at which the station commodities information was last updated

## Body

A star or planet.  Any values might be missing, depending on EDDI's configuration and the information available about the body.

All bodies have the following data:

    - `type` the type of the body (Star or Planet)
    - `systemname` the name of the system in which this body resides
    - `name` the name of the body
    - `distance` the distance from the arrival point in the system, in light seconds
    - `tidallylocked` true if the body is tidally locked to its parent
    - `temperature` the surface temperature of the body
	- rings (when applicable) (list of ring objects)

In addition, stars have the following data:

    - `mainstar` true if this is the main star of the syste
    - `stellarclass` the stellar class of the star (M, G, etc)
    - `solarmass` the solar mass of the star
    - `solarradius` the solar radius of the star
    - `age` the age of the star in millions of years

Planets have the following data:

    - `periapsis` the argument of periapsis of the planet
    - `atmosphere` the atmosphere of the planet
    - `tilt` the axial tilt of the planet
    - `earthmass` the earth masses of the planet
    - `gravity` the gravity of the planet
    - `eccentricity` the orbital eccentricity of the planet
    - `inclination` the orbital inclination of the planet
    - `orbitalperiod` the orbital period of the planet, in days
    - `radius` the radius of the planet, in km
    - `rotationalperiod` the rotational period of the planet, in days
    - `semimajoraxis` the semi-major axis of the planet, in km
    - `pressure` the surface pressure on the planet
    - `terraformstate` the terraforming state of the planet (Not terraformable, Terraformable, Terraforming, Terraformed)
    - `planettype` the type of the planet (Metal-rich body, Earth-like world, etc.)
    - `volcanism` the volcanism of the planet (Volcanism object)
    - `landable` true if the planet can be landed upon
    - `materials` list of materials and their percentage availability on the planet

Rings have the following data:

    - `name` the name of the ring
    - `mass` the mass of the ring
    - `innerradius` the inner radius of the ring
    - `outerradius` the outer radius of the ring
    - `composition` the composition of the ring

## Volcanism

Details of a body's volcanism.

    - `type` the type of volcanism: either "Geysers" or "Magma"
    - `composition` the composition of the volcanism (Iron, Carbon dioxide, Nitrogen etc.)
    - `amount` the amount of volcanism ("Major", "Minor" or nothing)

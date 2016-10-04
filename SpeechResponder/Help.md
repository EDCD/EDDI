# Scripting with EDDI

EDDI's speech responder uses Cottle for scripting.  Cottle has a number of great features, including:

    * Ability to set and update variables, including arrays
    * Loops
    * Conditionals
    * Subroutines

Information on how to write Cottle scripts is available at http://r3c.github.io/cottle/#toc-2, and EDDI's default scripts use a lot of the functions available.

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

### Pause()

This function will pause the speech for a given amount of time.

Pause() takes one argument: the number of milliseconds to pause.

Common usage of this is to allow speech to sync up with in-game sounds, for example to wait for a known response to a phrase before continuing, for example:

    Hello.  {Pause(2000)} Yes.

### Humanise()

This function will turn its argument into a more human number, for example turning 31245 in to "just over thirty thousand".

Humanise() takes one argument: the number to humanise.

Common usage of this is to provide human-sounding numbers when speacking rather than saying every digit, for example:

   You have {Humanise(cmdr.credits)} credits.

### ShipName()

This function will provide the name of your ship.

If you have set up a phonetic name for your ship it will return that, otherwise if you have set up a name for your ship it will return that.

If you have not set up a name for your ship it will just return "your ship".

### ShipCallsign()

This function will provide your ship's callsign in the same way that Elite provides it (i.e. manufacturer followed by first three letters of your commander name).

This will only work if EDDI is connected to the Frontier API.

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

StationDetails() takes two arguments of the station and the starsystem of the station.

Common usage of this is to provide further information about a station's capabilities, for example:

    Jameson Memorial is {StationDetails("Jameson Memorial", "Shinrarta Dezhra").distancefromstar} light seconds from the main star.

### MaterialDetails()

This function will provide full information for a material given its name.

MaterialDetails() takes a single argument of the material for which you want more information.

Common usage of this is to provide further information about a material, for example:

    Iron is a {MaterialDetails("Iron").rarity.name} material.

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

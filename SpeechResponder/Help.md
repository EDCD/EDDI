# Templating with EDDI

EDDI's speech responder uses Cottle for templating.  Cottle has a number of great features, including:

    * Ability to set and update variables, including arrays
    * Loops
    * Conditionals
    * Subroutines

Information on how to write Cottle templates is available at http://r3c.github.io/cottle/#toc-2, and EDDI's default templates use a lot of the functions available.

## State Variables

Cottle does not retain state between templates, but EDDI provides a way of doing this with state variables.  State variables are provided to each Cottle template, and templates can set state variables that will be made available in future templates.

State variables are available for individual templates in the 'state' object.  Note that state variables are not persistent, and the state is empty whenever EDDI restarts.  Also, because EDDI responders run asynchronously and concurrently there is no guarantee that, for example, the speech responder for an event will finish before the VoiceAttack responder for an event starts (or vice versa).

## Context

EDDI uses the idea of context to attempt to keep track of what it is talking about.  This can enhance the experience when used with VoiceAttack by allowing repetition and more detailed information to be provided.

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

### SetState()

This function will set a session state value.  The value will be available as a property of the 'state' object in future templates within the same EDDI session.

SetState takes two arguments: the name of the state value to set, and its value.  The name of the state value will be converted to lower-case and spaces changed to underscores.  The value must be either a boolean, a number, or a string; other values will be ignored.

Common usage of this is to keep track of the cumulative or persistent information within a session, for example:

    {SetState("distance_travelled_today", state.distance_travelled_today + event.distance)}

### Humanise()

This function will turn its argument into a more human number, for example turning 31245 in to "just over thirty thousand".

Humanise() takes one argument: the number to humanise.

Common usage of this is to provide human-sounding numbers when speacking rather than saying every digit, for example:

   You have {Humanise(cmdr.credits)} credits.

### Spacialise()

This function will add spaces between letters in a string & convert to uppercase, in order to allow letters in a string to be pronounced individually.

Spacialise() takes one argument: the number to Spacialise.

Common usage of this is to provide a more human-sounding reading of a string of letters that are not a part of known word:

   Star luminosity class: {Spacialise(event.luminosityclass)}.

### StartsWithVowel()

This function returns true or false depending on whether the first letter in a string is a vowel.

StartsWithVowel() takes one argument: the string that may or may not start with a vowel.

Common usage of this is to select the word that should proceed the string (e.g. **a** Adaptive Encryptors Capture vs **an** Adaptive Encryptors Capture).
   
   {if StartsWithVowel(event.name): an |else: a } {event.name}

### Play()

This function will play an audio file as supplied in the argument.  If this is in the result of a template then all other text is removed; it is not possible for EDDI to both play an audio file and speak in the same response.

Play() takes one argument: the path to the file to play.  This file must be a '.wav' file.  Any backslashes for path separators must be escaped, so '\\' must be written as '\\\\'

Common usage of this is to provide a pre-recorded custom audio file rather than use EDDI's text-to-speech, for example:

    {Play('C:\\Users\\CmdrMcDonald\\Desktop\\Warning.wav')}

### ICAO()

This function will turn its argument into an ICAO spoken value, for example "NCC" becomes "November Charlie Charlie".

ICAO() takes one argument: the value to turn in to ICAO.

Common usage of this is to provide clear callsigns and idents for ships, for example:

   Ship ident is {ICAO(ship.ident)}.

### ShipName()

This function will provide the name of your ship.

If you have set up a phonetic name for your ship it will return that, otherwise if you have set up a name for your ship it will return that.

ShipName() takes an optional ship ID for which to provide the name. If no argument is supplied then it provides the name for your current ship.

If you have not set up a name for your ship it will just return "your ship".

### ShipCallsign()

This function will provide your ship's callsign in the same way that Elite provides it (i.e. manufacturer followed by first three letters of your commander name).

ShipCallsign() takes an optional ship ID for which to provide the callsign. If no argument is supplied then it provides the callsign for your current ship.

This will only work if EDDI is connected to the Frontier API.

### ShipDetails()

This function will provide full information for a ship given its name.

ShipDetails() takes a single argument of the model of the ship for which you want more information.

Common usage of this is to provide further information about a ship, for example:

    The Vulture is made by {ShipDetails("Vulture").manufacturer}.

### SecondsSince()

This function will provide the number of seconds since a given timestamp.

SecondsSince() takes a single argument of a UNIX timestamp.

Common usage of this is to check how long it has been since a given time, for example:

    Station data is {SecondsSince(station.updatedat) / 3600} hours old.

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

This function will provide full information for a station given its name and optional system.

StationDetails() takes a single mandatory argument of the name of the station for which you want more information.  If the station is not in the current system then it can be provided with a second parameter of the name of the system.

Common usage of this is to provide further information about a station, for example:

    {set station to StationDetails("Jameson Memorial", "Shinrarta Dezhra")}
    Jameson Memorial is {station.distancefromstar} light years from the system's main star.

### BodyDetails()

This function will provide full information for a body given its name.

BodyDetails() takes a single mandatory argument of the name of the body for which you want more information.  If the body is not in the current system then it can be provided with a second parameter of the name of the system.

Common usage of this is to provide further information about a body, for example:

    {set body to BodyDetails("Earth", "Sol")}
    Earth is {body.distancefromstar} light years from the system's main star.

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

### Log()

This function will write the supplied text to EDDI's log.

Log() takes a single argument of the string to log.

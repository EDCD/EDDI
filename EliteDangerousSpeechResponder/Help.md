# Scripting with EDDI

EDDI's speech responder uses Cottle for scripting.  Cottle has a number of great features, including:

    * Ability to set and update variables
    * Loops
    * Conditionals
    * Subroutines

Information on how to write Cottle scripts is available at http://r3c.github.io/cottle/, and EDDI's default scripts use a lot of the functions available.

## EDDI Functions

There are a number of functions for specific features with EDDI.

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


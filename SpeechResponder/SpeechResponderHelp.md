# Speech responder

The speech responder reponds to events with scripted speech based on the information in the event. Not all events have scripted responses, and if a script response is empty, its 'Test' and 'View' buttons are disabled. The default personality is not editable. 

Speech generated from the speech responder can optionally be written to a file, either in addition to or in place of voiced speech. 

## Personalities

A collection of scripts is called a "Personality". If you would like to make edits from the default personality, please make a copy and assign a new name to your custom personality.

You may also use personalities shared by the community. To use a shared personality, you will need to save a copy of it to EDDI's configuration files, located at: `%appdata%\EDDI\personalities`.

## Scripts

EDDI's speech responder uses Cottle for scripting. Cottle has a number of great features, including:

* Ability to set and update variables, including arrays
* Loops
* Conditionals
* Subroutines

Information on how to write Cottle scripts is available at http://r3c.github.io/cottle/#toc-2, and EDDI's default scripts use a lot of the functions available. EDDI also uses a number of custom functions, documented in-app when editing a script via the `Help` button. 

Variables made available for scripting are in-app when editing a script via the `Variables` button. Standard variables appear at the top of the page. Additional event-specific variables are appended to the bottom of the page for applicable events and are documented in the wiki at https://github.com/EDCD/EDDI/wiki/Events.

The speech responder contains both event-based and non-event-based scripts.

### Event-based scripts

Event-based scripts can be disabled but cannot be deleted. They can also be assigned to differing priorities, as noted below:
* 1 - high priority, interrupts other speech
* 2 - high priority
* 3 - default priority
* 4 - low priority
* 5 - low priority, interruptible by any higher priority speech

### Non-event-based scripts 

Non-event-based scripts must be invoked, either by using a function call from another script or via the VoiceAttack plugin. Since they must be invoked they can be deleted but can neither be disabled nor assigned to new priorities. If invoked by another script, they will take on the priority of the invoking script. If invoked from VoiceAttack, they will use the default priority unless a new priority is assigned via the VoiceAttack plugin, as documented in https://github.com/EDCD/EDDI/wiki/VoiceAttack-Integration. 


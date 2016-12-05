#2.1.0-b1
  * Fix material name for Cadmium
  * Add ability to set state variables from VoiceAttack.  Details on how to use this are in the VoiceAttack documentation
  * Add option to write speech responder output to a file.  This is an option that can be checked in the speech responder tab, and writes all speech to %APPDATA%\EDDI\speechresponder.out
  * Add module definitions for SRV, fighter and training loadouts
  * Provide new API for EDDI versioning.  This allows EDDI to provide more information about updates to users, and is a precursor for automatic updates
  * Update local system database with details from the journal and companion API.  This ensures that this data is always as up-to-date as possible
  * ModificationCraftedEvent now has details of commodities used in crafting as well as materials
  * Provide update and MOTD information in VoiceAttack window if applicable
  * Update mission accepted and completed events to contain more fields.  Details on the new fields are in the relevant documentation
  * Update location event with details of station name and type where available
  * Add powerplay events.  Details on the new events are in the relevant documentation
  * Ensure that personality names do not contain illegal file or path characters to avoid issues when saving them
  * Send EDDN messages with "Unknown Commander" when commander name is not known
  * Use recursive/dynamic method to populate VoiceAttack variables.  This provides many more VoiceAttack variables than were previously available; details are in the relevent documentation

#2.0.13
  * Fix issue where engineer rank journal message without rank would cause a crash
  * Allow non-string sample events for testing scripts
  * Add sample galnet news event for testing
  * Do not throw spurious errors when shutting down
  * Ensure that stellar class VoiceAttack variable is unset if the information available

#2.0.12
  * Avoid bug in journal where superpower promotions are logged as combat promotions
  * Update shield resistances with booster stats when exporting ship
  * Add body information to speech responder
  * Add system main star stellar class and age to VoiceAttack variables
  * Fix commander progress "trade" rating
  * Disable "distort voice on damage" effect until we can find a better distortion process
  * Add separate exception logging system
  * Fix incorrect name for Type-7 when exporting to Coriolis
  * Send raw ship JSON directly to Coriolis for import rather than use local processing
  * Update 'Jumping' script: add warning if jumping to known white dwarf or neutron star
  * Update 'Body scanned' script: fix typo where 'higher' was written 'higer'

#2.0.11
  * Further fixes for renamed and missing fields in 2.2.02
  * Add "log out" option for companion API tab in UI
  * Provide internal Nullable values in VoiceAttack
  * Update VoiceAttack documentation with new variables
  * Credentials are removed on EDDI uninstall

#2.0.10
  * Update documentation for material and rarity information
  * Update coriolis export for Beluga Liner
  * Help large star system databases by adding an index
  * Add "shared" flag for bounty events
  * Add "source" for ship refuelled
  * Ship refuelled event now triggered when finishing scooping fuel
  * Handle renamed fields for system information in FSD jumps in 2.2.02
  * Add "Modification crafted" event
  * Add "Modification applied" event
  * Add "Engineer progressed" event

#2.0.9
  * Do not update ship configuration when data is not available from the companion API
  * Be a little harsher when shutting down speech threads on close
  * Make home station accessible to scripts
  * Make current station more dynamic to match reality
  * Fix crash when bringing up help windows from VoiceAttack

#2.0.8
  * Update coriolis export with additional properties for improved accuracy
  * Handle "Profile unavailable" response from companion API
  * Provide ship model rather than manufacturer to Coriolis
  * Update star class information with better probability distributions
  * Add stellar age and temperature probabilities
  * Catch bad allegiance data sent from companion API
  * Avoid repetition of docking information when still docked at the same station
  * Fix potential crash when station model is undefined
  * Update default docking and swapout scripts to make them less chatty

#2.0.7
  * Add ship's main and total fuel tank capacities
  * Add capability to upload logs to EDDI server
  * Add ability to access EDDI's confguration UI from VoiceAttack
  * Update VoiceAttack profile with fuel tank variables
  * Provide better translation for VESPER-M4 when speaking that system's name
  * Add information about the VoiceAttack 'profile' command
  * Update good and great percentage values for materials (thanks to Baroness Galaxy)
  * Update coriolis export to include modifications
  * Fix gravity and terraformstate for planets (thanks to Michael Werle)

#2.0.6
  * Update ship information when undocking
  * Retain ship information between relogs
  * Attempt to avoid crashes when configuration files have been corrupted
  * Avoid potential crash when network request times out
  * Added keepalive harness to monitor threads to catch errors and restart when appropriate
  * Strip SSML tags if SSML speech fails, then try again
  * Add material IDs for previously-known materials
  * Attempt to avoid bad voices that could crash EDDI
  * Fall back to standard speech if SSML isn't working
  * Add configuration option to avoid SSML altogether
  * Updated troubleshooting guide
  * Updated VoiceAttack system variables

#2.0.5
  * Added fuel used and fuel remaining to JumpedEvent
  * Handle missing data in body scans
  * Fix incorrect name of key in ShipyardNew
  * Update text on speech responder tab to be more informative about copying personalities
  * Fix oddity in jumping script where EDDI thought that 'None' was a real allegiance

#2.0.4
   * Add FDev IDs for new ships and modules
   * Add phonetic pronunciation for Lakon
   * Fix issue where unknown IDs could cause a crash

#2.0.3
   * Fix issue where jumps are not announced if the companion API is unavailable

#2.0.2
  * Fix crash when companion API does not return correct information
  * Fix crash when starting EDDI after Elite has started
  * Attempt to patch around missing information when companion API does not provide it

#2.0.1
  * Complete rework of product and VoiceAttack integration
  * EDDI no longer dependent on VoiceAttack for spoken responses to events
  * Use Frontier journal in addition to netlog to provide more events
  * Plugin architecture for monitors (that provide events) and responders (that act on events)
  * Introduction of personalities - bundles of responses for the speech responder 
  * Fix issue with text-to-speech engines mispronouncing sector names with two-letter phrases such as DR and NY
#1.4.0
  * Rework network communications to provide better debug information
  * Unify logging to a single file
  * Always use the selected voice's default language when using phonemes
  * Validate spoken ship names for valid IPA
  * Provide more details when obtaining EDSM logs from configuration UI
  * Use local store as primary for star system data and update EDSM asynchronously
  * Silently drop existing phonetic ship names with invalid IPA

#1.3.5
  * Tweaks to increase volume of processed voice
  * Fix issue with last utility slots on large ships being missed
  * Add verbose logging option to configuration UI
  * Add definition for huge burst lasers (fixed, gimballed)
  * Avoid hang when sending to EDSM
  * Avoid hang when sending to EDDP
  * Provide local co-ordinates when systems are not available in EDSM
  * Never write credentials to log file

#1.3.1
  * Add station variables.  Note that these are all prefixed 'Last station' because there is no way of knowing if a ship is currently docked at a station or has already left it from the information provided in the API
      * Last station name (string): the name of the last station
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
  * Add more system translations
  * Fix issue with 0-cargo ships causing profile problems

#1.3.0
  * Fix issue where unknown systems provided an incorrect JSON result
  * Add command to show the current system in EDDB
  * Add command to show the current station in EDDB
  * Add timeout for EDDP queries
  * Fix isue where hardpoints returned out of order resulted in incorrect export URL for Coriolis
  * Log engineer modifications as part of module definition

#1.2.2
  * Fix issue where shipyard would not be populated in some situations
  * Close down HTTP responses sooner, hopefully fixing occasional hangs
  * Populate the email address field in the configuration GUI from stored data if available
  * Ensure that co-ordinates are sent to EDSM in US format regardless of client locale
  * Added 'System orbital stations' value

#1.2.1
  * Added modules from 1.6/2.1
  * Added commodities from 1.6/2.1
  * Support for new modules when exporting to Coriolis
  * Handle situation where companion API is available but not responding with data
  * Added more star system translations
  * Removed requirement for verbose logging

#1.2.0
  * Compatibility with Elite: Dangerous 1.6/2.1
  * Fixed issue where unknown station models would cause EDDI to crash
  * Fixed issue where commander's home system and insurance discount were reset on configuration startup
  * Added relative volume for text-to-speech voice
  * Added support for 1.6/2.1 new-style system change messages
  * Send system co-ordinates to EDSM as part of the travel log if available
  * Added more star system translations

#1.1.0
  * Fixed issue where EDDI would need the user to log in repeatedly to maintain a connection to the Elite servers
  * Added configuration option to set insurance excess percentage
  * Added Insurance (decimal): the percentage insurance excess (usually 5, 3.75 or 2.5)
  * Added Ship limpets carried (int): the number of limpets carried by the ship
  * Added 'generate callsign' plugin context
  * Added configuration of the ship voice
  * Added configuration option to sync EDSM data with local information
  * Added ability to set and recollect notes from EDSM
  * Added ability to carry out trilateration of systems for EDSM
  * Added phonetic pronunciation for ship names
  * Added further Powerplay weapons

#1.0.0
  * Fix minor VoiceAttack script issues

#0.9.4
  * Added 'System distance from home'
  * Provide information about primary faction, number of stations and distance from home with the system report
  * Added 'Tell me about this system' command to trigger the system report
  * Reworked star system name translation routines to provide correct result in more situations

#0.9.3
  * Move to internal speech generation routines for computer voice

#0.9.2
  * Fix issue where sold ships still showed up in the shipyard
  * Add information on modules and hardpoints to damage and outfitting reports
  * Add missing IDs for some EDDB->Coriolis mappings
  * Fix issue where removed ship names are not treated as absent

#0.9.1
  * Added ship variables for hardpoints and internal compartments
    * Ship tiny/small/medium/large/huge hardpoint *n* occupied (boolean): true if there is a module in this slot, otherwise false
    * Ship tiny/small/medium/large/huge hardpoint *n* module (string): the name of the module in this slot
    * Ship tiny/small/medium/large/huge hardpoint *n* module class (int): the class of the module in this slot
    * Ship tiny/small/medium/large/huge hardpoint *n* module grade(grade): the grade of the module in this slot
    * Ship tiny/small/medium/large/huge hardpoint *n* module health (decimal): the percentage health of the module in this slot
    * Ship tiny/small/medium/large/huge hardpoint *n* module cost (decimal): the purchase cost of the module in this slot
    * Ship tiny/small/medium/large/huge hardpoint *n* module value (decimal): the undiscounted cost of the module in this slot
    * Ship tiny/small/medium/large/huge hardpoint *n* module discount (decimal): the percentage discount of the purchased module against the undiscounted cost
    * Ship tiny/small/medium/large/huge hardpoint *n* module discount (spoken) (text): the percentage discount of the purchased module against the undiscounted cost
    * Ship Compartment *n* size: the size of this slot
    * Ship Compartment *n* occupied (boolean): true if there is a module in this slot, otherwise false
    * Ship compartment *n* module (string): the name of the module in this slot
    * Ship compartment *n* module class (int): the class of the module in this slot
    * Ship compartment *n* module grade (string): the grade of the module in this slot
    * Ship compartment *n* module health (decimal): the percentage health of the module in this slot
    * Ship compartment *n* module cost (decimal): the purchase cost of the module in this slot
    * Ship compartment *n* module value (decimal): the undiscounted cost of the module in this slot
    * Ship compartment *n* module discount (decimal): the percentage discount of the purchased module against the undiscounted cost
    * Ship compartment *n* module discount (spoken) (text): the percentage discount of the purchased module against the undiscounted cost
  * Created separate variable debug commands for commander, ship, ship hardpoints, ship compartments, and shipyard
  * Ensure that 'System power' is not set if a system is not controlled by a power

#0.9.0
  * Fix issue where setting a home system caused scripts not to report system information
  * Do not create 'system change' or 'location change' events when in CQC 

#0.8.9
  * Provide starsystem information even if profile is not available

#0.8.8
  * Fix crash if starsystem has no stations

#0.8.7
  * Avoid VoiceAttack bug that causes VoiceAttack to crash

#0.8.6
  * Added more checks for correct system data prior to triggering system change event
  * Added debug value for the event loop
  * Separated the event loop from the startup, to provide better reliability for the event loop [B]Anyone updating will need to change the startup command in their profile from EDDI event loop to EDDI startup[/B]
  * Added 'System minutes since previous visit' value

#0.8.5
  * Added callsigns, names and roles for ships
  * Added home system and station
  * Added more events in the handler.  These are often triggered by voice actions themselves, but have been built in this way to allow for the future when these events become available directly to EDDI
  * Added the name of the last station the commander docked at
  * Added the ship's fuel tank capacity
  * Add ship name and callsign to coriolis export
  * Provided a number of voice-activated commands; see the README for details

#0.8.0
  * Added EDSM integration: provide the ability for EDDI to send data to EDSM, keeping a log of every system you have visited
  * Ensure that "Last system rank" is set appropriately
  * Move from 'Login' to 'Configuration' binary for setting up EDDI

#0.7.3
  * Event loop only returns if there is a new event to handle
  * Avoid problems if VA_Init1() is called multiple times
  * Added decimal variables 'Stored ship *n* distance' for the distance to each stored ship from the current system

#0.7.2
  * Ensure that Environment is set on startup
  * Ensure that cached system data is refreshed correctly

#0.7.1
  * Fix issue where Empire rating was showing incorrectly with Federation names
  * Catch errors when authenticating and obtaining the first profile and display a suitable error message
  * Added text variables "Ship model (spoken)", "System name (spoken)", "System power (spoken)", "Last system name (spoken)" and "Last system power (spoken)" to provide values that fit the text-to-speech engine
  * Renamed string variables "Credits", "Debt", "Ship value", "System population", "Last system population", "Ship <module> station discount" to have the " (spoken)" suffix.  The old names of the variables will be removed in the next major release.

#0.7.0

  * Deprecated int variables "Credits", "Debt", "Ship value", "System population", "Last system population" in favour of decimal values with the same name.  The int variables were in thousands due to int limits and the decimal values are in units.  The int variables will be removed in the next major release
  * Added event loop to monitor the netlog
  * Added decimal variables "Ship * cost" "Ship * value" and "Ship * discount" for each of the ship's standard modules (e.g. "Ship power plant cost", "Ship power plant value" and "Ship power plant discount").  Cost is the number of credits the commander spent purchasing the module, Value is the undiscounted cost of the module and discount is (1 - cost / value)
  * Added decimal varaibles "Ship * station cost" for the cost of the ship's existing modules at the currently docked station.
  * Added text varaibles "Ship * station discount" for the discount possible by purchasing the ship's existing modules at the currently docked station.
  * Added int variable "System visits" that contains the number of times that the commander has visited the current system since the plugin was initialised
  * Added datetime variable "Previous system visit" that contains the date and time of the the commander previously visited this system
  * Added text variable "Environment" that lists the current environment ("Normal space" or "Supercruise")
  * Fixed issue where templates were updated with commander data, resulting in incorrect data after the first refresh

#0.6.1

  * Added int variable "System stations" with the number of stations (of all types) in the system
  * Added int variable "System starports" with the number of starports in the system
  * Added int variable "System outposts" with the number of outposts in the system
  * Added int variable "System planetary stations" with the number of planetary stations (of all types) in the system
  * Added int variable "System planetary outposts" with the number of planetary outposts in the system
  * Added int variable "System planetary ports" with the number of planetary ports in the system
  * Added int variable "Last system stations" with the number of stations (of all types) in the last system
  * Added int variable "Last system starports" with the number of starports in the last system
  * Added int variable "Last system outposts" with the number of outposts in the last system
  * Added int variable "Last system planetary stations" with the number of planetary stations (of all types) in the last system
  * Added int variable "Last system planetary outposts" with the number of planetary outposts in the last system
  * Added int variable "Last system planetary ports" with the number of planetary ports in the last system
  * Added string variable 'Ship size' containing the size of the ship (Small, Medium or Large)

#0.6.0

  * Added decimal variables 'System X' 'System Y', 'System Z', 'Last system X', 'Last system Y', 'Last system Z' using EDDB co-ordinates
  * Added decimal variable 'Just jump' with the distance of the last jump in LY to two decimal places
  * Fixed incorrect trade rank (showed 'Elite' as 'Pioneer')
  * Added int variable 'Stored ships' with the number of ships in storage
  * Added string variables 'Stored ship *n* model', 'Stored ship *n* system' and 'Stored ship *n* location' for each ship in storage

#0.5.0
Initial release

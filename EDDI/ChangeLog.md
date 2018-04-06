# CHANGE LOG

### 3.0.0-rc2
  * Core
    * The EDSM responder has been updated to send data to EDSM per their revised API. 
    * Switched error reporting to [Rollbar](https://rollbar.com/).
  * Speech Responder
    * Added 'Fighter rebuilt' event

### 3.0.0-rc1
  * Core
    * Incorporated new data definitions for 3.0.
  * Installer
    * First installations will now take any custom VoiceAttack installation location into account when proposing a location for EDDI.
    * Upgrade installations will continue to use whatever location was selected in the first installation.
  * Speech Responder
    * Added 'Jet cone damage' event
    * Script changes
      * Added new script 'Jet cone damage'
  * VoiceAttack
    * Added the following new variables
      * `{BOOL:EDDI speaking}` True if EDDI is speaking, false otherwise. Useful for synchronizing speech between EDDI and other sources in VoiceAttack.
    * Fixed a bug whereby `cmdr.title` was not being initialised. 

### 3.0.0-b3
  * Core
    * Fixed a crash upon startup when the EDSM responder was not configured.
    * Fixed a crash upon shipyard refresh when any ships had been sold while EDDI was not running.

### 3.0.0-b2
  * Core
    * Squashed a bug that was preventing EDDI from correctly registering changes to the shipyard.
    * Squashed a bug with the status monitor that was preventing events from being detected in VoiceAttack and was messing up some other variables.
    * EDDI will no longer try to sync data from EDSM while the EDSM responder is disabled, and when syncing EDSM data EDDI will now write to the local SQL database in batches.
    * Squashed a bug that was causing EDDI to request and re-process complete EDSM flight logs on every load. Now it'll only request the new stuff since its last update.
  * Update Server
    * Fixed the outdated TLS protocol usage on EDDI's side whereby the update server began refusing to talk to existing releases of EDDI.
      * In future, EDDI will let you know if it cannot reach its update server for any reason.
      * **Unfortunately, users of all prior versions won't be able to automatically update so please tell your friends that they can manually update to the latest and greatest version.**
  * Speech Responder
    * Added new 'SRV turret deployable' event. The variable `deployable` is a boolean value describing whether the SRV's turret is now available.
    * Script changes
      * Made the 'Fuel check' script more succinct, for less cognitive burden during those buckyballing runs.
  * VoiceAttack
    * Added the following new variables
      * `{TXT:Gender}` the preferred gender of the commander for pronouns and titles. One of "Male", "Female", or "Neither".

### 3.0.0-b1
  * UI
    * If EDDI is run as a standalone app, its entire window state is now preserved. If EDDI is invoked via VoiceAttack commands, we only remember whether it was maximised and don't disturb the rest.
  * Core
    * Added Alliance Chieftan.
	* Added decontamination limpets.
    * Added recon limpets.
    * EDDI will now more readily notice if your SRV or fighter was destroyed (EDDI couldn't always tell before).
  * Shipyard
    * Speculative fix for the concurrency bug that messes up shipmonitor.json when you buy a ship.
  * NPC Comms
	* Reporting of NPC comms is much more succint.
		Was: Message received from "name".  Message reads: "blah".
		Now: From "name": "blah".
  * Speech Responder
    * Added new 'Near surface' event, triggered when you enter or depart the gravity well around a surface
	* ~~Added new 'SRV under ship' event, triggered when your SRV enters or leaves the proximity zone around your ship~~
	* Added new 'SRV turret' event, triggered when you deploy or retract your SRV's turret
	* Added new 'Ship fsd' event, triggered when there is a change to the status of your ship's fsd
	* Added new 'Ship low fuel' event, triggered when your fuel level falls below 25%
    * Added new 'Under attack' event
    * Add new event 'Shutdown', triggered on a clean shut down of the game.
    * The 'Vehicle destroyed' event now includes the variable `vehicle`, describing the vehicle that was destroyed.
    * Add a new top level `status` object, which contains the following new variables
      * `vehicle` the vehicle that is under the commander's control.  Can be one of "Ship", "SRV" or "Fighter"
      * `being_interdicted` a boolean value indicating whether the commander is currently being interdicted
      * `in_danger` a boolean value indicating whether the commander is currently in danger
      * `near_surface` a boolean value indicating whether the commander is near a landable surface (within it's gravity well)
      * `overheating` a boolean value indicating whether the commander's vehicle is overheating
      * `low_fuel` a boolean value indicating whether the commander has less than 25% fuel remaining
      * `fsd_status` the current status of the ship's frame shift drive. Can be one of "ready", "cooldown", "charging", or "masslock"
      * `srv_drive_assist` a boolean value indicating whether SRV drive assist is active
      * `srv_under_ship` a boolean value indicating whether the SRV in within the proximity zone around the ship
      * `srv_turret_deployed` a boolean value indicating whether the SRV's turret has been deployed
      * `srv_handbrake_activated` a boolean value indicating whether the SRV's handbrake has been activated
      * `scooping_fuel` a boolean value indicating whether the ship is currently scooping fuel
      * `silent_running` a boolean value indicating whether silent running is active
      * `cargo_scoop_deployed` a boolean value indicating whether the cargo scoop has been deployed
      * `lights_on` a boolean value indicating whether the vehicle's external lights are active
      * `in_wing` a boolean value indicating whether the commander is currently in a wing
      * `hardpoints_deployed` a boolean value indicating whether hardpoints are currently deployed
      * `flight_assist_off` a boolean value indicating whether flight assistance has been deactivated
      * `supercruise` a boolean value indicating whether the ship is currently in supercruise
      * `shields_up` a boolean value indicating whether the ship's shields are maintaining their integrity
      * `landing_gear_down` a boolean value indicating whether the ship's landing gears have been deployed
      * `landed` a boolean value indicating whether the ship is currently landed (on a surface)
      * `docked` a boolean value indicating whether the ship is currently docked (at a station)
      * `pips_sys` a decimal value indicating the power distributor allocation to systems
      * `pips_eng` a decimal value indicating the power distributor allocation to engines
      * `pips_wea` a decimal value indicating the power distributor allocation to weapons
      * `firegroup` an integer value indicating the ship's currently selected firegroup
      * `gui_focus` the commander's current focus. Can be one of "none", "internal panel" (right panel), "external panel" (left panel), "communications panel" (top panel), "role panel" (bottom panel), "station services", "galaxy map", or "system map"
      * `latitude` a decimal value indicating the ship's current latitude (if near a landable surface)
      * `longitude` a decimal value indicating the ship's current longitude (if near a landable surface)
      * `altitude` a decimal value indicating the ship's current altitude (if in flight near a landable surface)
      * `heading` a decimal value indicating the ship's current heading (if near a landable surface)
  * VoiceAttack
    * Added the following new variables
      * {TXT:Status vehicle}: the vehicle that is under the commander's control. Can be one of "Ship", "SRV" or "Fighter"
      * {BOOL:Status being interdicted} a boolean value indicating whether the commander is currently being interdicted
      * {BOOL:Status in danger} a boolean value indicating whether the commander is currently in danger
      * {BOOL:Status near surface} a boolean value indicating whether the commander is near a landable surface (within it's gravity well)
      * {BOOL:Status overheating} a boolean value indicating whether the commander's vehicle is overheating
      * {BOOL:Status low fuel} a boolean value indicating whether the commander has less than 25% fuel remaining
      * {TXT:Status fsd status} the current status of the ship's frame shift drive. Can be one of "ready", "cooldown", "charging", or "masslock"
      * {BOOL:Status srv drive assist} a boolean value indicating whether SRV drive assist is active
      * {BOOL:Status srv under ship} a boolean value indicating whether the SRV in within the proximity zone around the ship
      * {BOOL:Status srv turret deployed} a boolean value indicating whether the SRV's turret has been deployed
      * {BOOL:Status srv handbrake activated} a boolean value indicating whether the SRV's handbrake has been activated
      * {BOOL:Status scooping fuel} a boolean value indicating whether the ship is currently scooping fuel
      * {BOOL:Status silent running} a boolean value indicating whether silent running is active
      * {BOOL:Status cargo scoop deployed} a boolean value indicating whether the cargo scoop has been deployed
      * {BOOL:Status lights on} a boolean value indicating whether the vehicle's external lights are active
      * {BOOL:Status in wing} a boolean value indicating whether the commander is currently in a wing
      * {BOOL:Status hardpoints deployed} a boolean value indicating whether hardpoints are currently deployed
      * {BOOL:Status flight assist off} a boolean value indicating whether flight assistance has been deactivated
      * {BOOL:Status supercruise} a boolean value indicating whether the ship is currently in supercruise
      * {BOOL:Status shields up} a boolean value indicating whether the ship's shields are maintaining their integrity
      * {BOOL:Status landing gear down} a boolean value indicating whether the ship's landing gears have been deployed
      * {BOOL:Status landed} a boolean value indicating whether the ship is currently landed (on a surface)
      * {BOOL:Status docked} a boolean value indicating whether the ship is currently docked (at a station)
      * {DEC:Status pips sys} a decimal value indicating the power distributor allocation to systems
      * {DEC:Status pips eng} a decimal value indicating the power distributor allocation to engines
      * {DEC:Status pips wea} a decimal value indicating the power distributor allocation to weapons
      * {INT:Status firegroup} an integer value indicating the ship's currently selected firegroup
      * {TXT:Status gui focus} the commander's current focus. Can be one of "none", "internal panel" (right panel), "external panel" (left panel), "communications panel" (top panel), "role panel" (bottom panel), "station services", "galaxy map", or "system map"
      * {DEC:Status latitude} a decimal value indicating the ship's current latitude (if near a surface)
      * {DEC:Status longitude} a decimal value indicating the ship's current longitude (if near a surface)
      * {DEC:Status altitude} a decimal value indicating the ship's current altitude (if in flight near a surface)
      * {DEC:Status heading} a decimal value indicating the ship's current heading (if near a surface)

### 2.4.6-b3
  * Core
    * Improved window size and position handling for multi-display setups. 
    * EDDI's UI now clearly shows whether EDDI has found your home system and station (if they haven't been found, the associated objects will not be populated). If EDDI cannot find a match, the textbox will display a red border and the contents will not be saved.
    * Fixed an error that could occur when a response isn't received from EDSM.
    * Fixed a bug that would cause EDDI to write to the shipyard before it had finished processing shipyard related actions (adding and removing ships)
    * Fixed a bug caused by a structure change for the 'Bond redeemed' faction name and amount in the Beyond beta.
  * Speech Responder
    * Updated 'Bond redeemed' script for better handling with redemption via interstellar factors contacts.
    * Updated 'Bounty redeemed' script for better handling with redemption via interstellar factors contacts.
    * Added a description for the 'Blueprint' object in [the Variables documentation](https://github.com/EDCD/EDDI/blob/master/SpeechResponder/Variables.md)
    * Updated the description of the 'Material' object in [the Variables documentation](https://github.com/EDCD/EDDI/blob/master/SpeechResponder/Variables.md)
    * Fixed out-of-date context for the following scripts
      * 'Material collected' 
      * 'Material discarded' 
      * 'Material discovered' 
      * 'Material donated'
    * Added new event 'VA initialized', triggered when the VoiceAttack plugin is fully initialized. You can respond to this event in VoiceAttack by creating a '((EDDI va initialized))' command.
  * Text-to-Speech
    * Re-enabled text-to-speech distortion on ship damage. If this option is enabled, EDDI will now increase voice processing effects as damage to the ship increases.
    * Revised text-to-speech audio gain to compensate for volume losses when voice processing effects are applied.
  * VoiceAttack
    * Augmented VoiceAttack commands to manipulate the EDDI user interface. The following commands are now included in the updated EDDI.vap file: 
      * 'Configure EDDI', 
      * 'Open EDDI', 
      * 'Close EDDI'
      * 'Minimize EDDI', 
      * 'Maximize EDDI', 
      * 'Restore EDDI' and 
      * 'Initialize EDDI'
    * If there is a problem with a script, EDDI will now tell you which script has the problem rather than leaving you to play the guessing game.
  * Galnet Monitor
    * Default the galnet monitor plugin to 'off' in favor of the in-game Galnet Audio. The plugin still can be enabled if desired.

### 2.4.6-b2
  * Core
	* Added support for the large AX weapons and the Type 10 Defender (export to EDShipyard and Coriolis should be compatible and work just as soon as they are ready).
    * EDDI will now remember and restore its window size and position, the selected tab, and its minimized / maximized status on startup (and there was much rejoicing).
    * You can now specify your commander's gender in the "Commander Details" tab. Currently this is only relevant for titles of nobility in the Empire. You can specify "Neither" if you prefer to be addressed as "Commander" in situations where convention would otherwise require a gendered form of address.
    * Changes to your home system / station will now be honoured immediately rather than after the next app restart.
    * Hardened EDDI against a crash that could occur if the folder containing player journals doesn't exist.
    * Smarter vehicle state tracking.
    * Found a way to improve support for Cereproc voices. These should now support more of the functions described in [the SpeechResponder documentation](https://github.com/EDCD/EDDI/blob/master/SpeechResponder/Help.md)
    * The status of friends observed during your play session is now available under the `cmdr` object.
  * Speech Responder
    * Add new event 'Vehicle destroyed' *(it does not perfectly track vehicle destruction since there are no official player journal events for SRV or fighter destruction - we have to infer vehicle destruction. Still, it's better than nothing.)*.
    * Amended the descriptions for the 'Module arrived' and 'Ship arrived' station and system variables.
    * Fixed a bug that was causing parsing all promotion events to fail.
    * Fixed a bug with the 'Friends' event. This event will no longer repeat the status of friends when no change has been observed in their status. Deprecated variable `friend` and replaced with variable `name`.
    * Fixed a bug that was causing the 'Ship arrived' event to report bad arrival locations.
    * 'Message received' event:
      * Fixed a bug that caused the 'Message received' event to not recognize messages from multicrew commanders as being from players. 
      * New channel: 'multicrew'. New source: 'Crew mate'.
    * 'Docked' event:
      * New variables 'allegiance' and 'state'. 'State' is a new variable that is currently used to describe damaged stations and stations under repair.
    * 'Mission completed' event:
      * Added variables `rewardCommodity` and `rewardAmount`. Useful for cargo tracking.
    * 'Search and rescue' event:
      * Added variable `commodityname` to provide the name of the commodity turned in, free of the commodity object. Accessible to VoiceAttack as `{TXT:EDDI search and rescue commodityname}` 
      * Updated 'Search and rescue' event to better distinguish between occupied and damaged escape pods, and to fix a bug in handling wreckage commodities.
    * Script changes
      * New script 'Report last scan value' to report the estimated value of the last scan with variations - used by 'Star scanned' and 'Body scanned'.
      * 'Body scanned' leaves naming the body in question to 'Body report', which no longer repeats the base system name if it doesn't have to.
      * Add new script 'Vehicle destroyed'
      * Updated 'Data voucher redeemed' script for events where the faction is not defined (such as INRA sites).
      * Updated 'Docked' script to report emergency docking differently.
      * Removed deprecated 'Jumping' script (replaced by 'FSD engaged' in prior updates)
      * Renamed 'Crew member role change' event to 'Crew member role changed' to correct a bug that caused the event to be un-editable. Since the VoiceAttack documentation already indicated to use 'Crew member role changed', there should be no affect on VoiceAttack configurations. 
      * Updated 'Friends event' to use the new `name` variable. For users of the default script, the default script is now re-enabled.
      * Updated 'Honorific' script to respect your chosen gender.
      * Updated 'Jumped' event to fix a typo that was preventing a call to the new 'Fuel check' script.
      * Refined script 'Module arrived'
      * Refined script 'Ship arrived'
      * Moved empire honorific logic into new script 'Empire honorific'.
  * Galnet monitor
    * Add a checkbox to the Galnet monitor to toggle whether the Galnet monitor will update all of the time or only if the game has posted a journal event in the last ten minutes. This option prevents Galnet spam upon starting EDDI.

### 2.4.6-b1
  * The Galnet monitor will now check the player journal for recent activity prior to updating - VoiceAttack users rejoice!
  * VoiceAttack
    * Updated EDDI.VAP to include a new command for marking Galnet article categories as read (documented in [the Galnet Monitor documentation](https://github.com/EDCD/EDDI/wiki/Galnet-Monitor).
  * Speech Responder
    * 'Body scanned' and 'Star scanned' events - added new calculated variable "estimatedvalue". 
    * 'Star scanned' event - added new calculated variables "estimatedhabzoneinner" and "estimatedhabzoneouter" to provide calculated values for the habitable zone of a scanned star. Note: calculations are most accurate for star systems containing a single star (multiple close proximity stars will make these calculations less reliable).
    * 'Bounty incurred' event - added new variable 'crime' with a more humanized description of the crime committed.
    * 'Fine incurred' event - added new variable 'crime' with a more humanized description of the crime committed.
    * Added new 'Jet cone boost' event
    * Added new 'Module arrived' event
    * Added new 'Ship arrived' event
    * Revised speech responder UI to clarify that the default personality is read-only and a new personality must be generated via the 'Copy personality' prior to editing. 
    * Revised speech responder UI to clarify for users of custom personalities when a script can be disabled and/or deleted. 
    * The 'Enabled' checkbox in the Speech responder UI shall now be enabled only for scripts which are triggered by events, not for scripts which are only triggered by other scripts.
    * Script changes
      * Added new script 'Galnet mark read' to allow users to bulk mark news articles as read.
      * Revised script 'Galnet news' to mark the article as read after reading.
      * Revised script 'Galnet news published' to mark all articles summarized by this script as read.
      * 'Body scanned' - revised to report estimated scan value
      * 'Star scanned' - revised to report estimated scan value and calculated habitable zone 
      * Updated 'Bounty incurred' to describe your crimes with the new 'crime' variable.
      * Updated 'Fine incurred' to describe your crimes with the new 'crime' variable.
      * Added new script 'Fuel check'.
      * Updated 'Jumped' event and 'Ship refueled' event. With the new 'Fuel check' script, 'Ship refueled' will no longer repeat for every 5T refueled.
      * Added new script 'Jet cone boost'
      * Added new script 'Module arrived'
      * Added new script 'Ship arrived'
    * Added the following Cottle functions, documented in [the SpeechResponder documentation](https://github.com/EDCD/EDDI/blob/master/SpeechResponder/Help.md):
      * `List()`: returns a humanised list of items from an array (e.g. "this, that, and the other thing").

### 2.4.5
  * Core
    * Added defensive coding so that EDDI will not crash on startup if it has trouble reading the configuration files.
  * Material Monitor
    * Added definitions for some previously unknown materials found at crash sites.
    * Added defensive coding so that EDDI will not crash when unknown materials are encountered in future.

### 2.4.4
  * Speech Responder
    * Fixed a bug that was causing some SSML related functions (e.g. Pause()) to not render correctly.
    * Fixed unit conversion of the star's age in star scans. They should no longer report every star as "one of the oldest".

### 2.4.3
  * Core
    * We will no longer ask users to send logs for commodity definition errors (and there was much rejoicing). 
    * Fixed a time zone snafu that was causing the "Report an issue" button to export empty log files for west of GMT locales.
  * EDSM
    * Fixed a bug that was preventing EDSM comments from being updated and read.
  * Speech Responder
    * Added the following Cottle functions, documented in [the SpeechResponder documentation](https://github.com/EDCD/EDDI/blob/master/SpeechResponder/Help.md):
      * `Emphasize()`
      * `SpeechPitch()`
      * `SpeechRate()`
      * `SpeechVolume()`
    * 'FSD jump' event - reduced the pause between jumping and speaking.
    * Script changes
      * 'Star report' 
        * Amended the age calculations for the fact that age is reported in millions of years, not years.
        * Amended reporting of stars less than a million years old.
        * Amended the test for Herbig-Haro objects.
        * Enhanced the reporting of Wolf-Rayet stars.
        * Sundry punctuation tweaks to make the speech more natural.
      * 'Entered signal source' 
        * Thoroughly re-written to better report both human and Thargoid signal sources.
    * Worked around non-compliance of CereProc voices with industry standards that would cause EDDI to revert to a system default voice.
    * Fixed a bug that was preventing the Play() function from working properly

### 2.4.2
  * Core
    * Revised EDDN updating for naming changes in ED 2.4. This makes EDDI 2.4.2 a mandatory update.
    * Revised error reporting. The 'Send EDDI log to developers' button is now called 'Report an Issue' and routes users to our Github issues page. If verbose logging is enabled, a zipped and truncated log file is placed on the desktop so that it may be attached to the Github issue.
  * Material Monitor
    * Fixed a bug that prevented EDDI from recognizing and removing old versions of some data from the Material Monitor.

### 2.4.1
  * We just needed to bump the version number to flush out 2.4.0 builds that didn't understand that 'rc' means 'release candidate'. (Because it's a computer and, guess what, we have to tell it stuff like that.)

### 2.4.0
  * Core
    * Eliminated the approx 7 second delay on app startup that was introduced in rc1.
  * Speech Responder
    * Script changes
      * 'FSD engaged' 
        * Amended test for white dwarf arrival stars to match all subtypes, courtesy of CMDR J. Calvert (Joshua).
        * Simplified logic for testing for scoopable stars.
      * 'Market information updated'
        * Delay 4.5 seconds before speaking market data on docking.

### 2.4.0-rc1
  * Core
    * EDDI will now take commander ratings/rankings from the journal in addition to from the API.
    * EDDN market and outfitting updating restored, accomodating 2.4 cAPI changes. Bonus - now sending shipyard data to EDDN!
    * Updated Variables.md to include a description of commodities objects and their available properties.
    * Fixed a bug where some commanders weren't receiving updates to their EDSM profiles. 
  * Shipyard
    * Export to both Coriolis and EDShipyard is now supported.
    * Fixed a bug that was preventing EDDI from retaining full data from the API, thus mucking up exports to 3rd party services.
    * The 'Export' button is now disabled when EDDI doesn't have the necessary information about the ship in question.
    * Information about engineer modifications will be exported provided you have used the ship at least once in ED 2.4 or later.
    * EDDI can now tell you what's in a ship's fighter bays and vehicle hangars.
  * Coriolis Export
    * Fixed a bug that was preventing EDDI from retaining full data from the API, thus mucking up exports to Coriolis.
    * The 'Export to Coriolis' button is now disabled when EDDI doesn't have the necessary information about the ship in question.
  * Events
    * 'Empire promotion' event added
    * 'Federation promotion' event added
    * 'Star scanned' event now reports the star's luminosity class and any rings that it has.
  * Speech Responder
    * Disabled speech for the 'Community goal' event to prevent cg spam (the event still triggers, but it'll be silent until we rework the code for it).
    * Fixed the 'Message Received' event for the new 2.4 journal format. EDDI now reads direct messages, local chat and wing comms again.
    * Script changes
      * 'Empire promotion' - new script
      * 'Federation promotion' - new script

### 2.4.0-b5
  * Core
    * EDDI can once again track how many limpets you have (and there was much rejoicing).
    * The shipyard should now be populated correctly.
    * Updated Variables.md to include a description of commodities objects and their available properties.
    * EDDI will now capture a timestamp that can be used internally by EDDI to compare journal and API data.
  * Events
    * Revised 'Community goal' event - event expiry is now given in seconds from now.
    * Update 'Message received' event for Frontier's (undocumented) changes to player message entries
    * 'Location' event & 'Jumped' event - fixed a bug that would cause some high population systems to report negative populations.
    * 'Search and rescue' event - the commodity is now a commodity object with all applicable commodity information included
  * Speech Responder
    * Script changes
    * 'Community goal' - fixed a bug that would claim you could expect a reward without contributing, the script is also now aware of the time remaining in the community goal
      * 'Community goal' - fixed a bug that would claim you could expect a reward without contributing, the script is also now aware of the time remaining in the community goal
      * 'Search and rescue' event - revised script to use commodity object variables (beta users, please refresh the default script)
      * 'Touchdown' event - latitude & longitude are only written to the journal when the ship is player controlled, script revised to not give erroneous information if the ship isn't player controlled
      
### 2.4.0-b4
  * Core
    * Revised EDDI's methods for detecting in-game betas
  * Events
    * Fixed a bug that would cause the 'Ship transfer initiated' event to be silent
    * 'Community goal' event - refined the default script, it'll (probably) be coherent now :-)
  * Speech Responder
    * 'Community goal' event - fixed a bug that was causing EDDI to describe every goal twice
    * 'Ship transfer initiated' event - revised to include both the transfer cost and the time to arrival
    * Script changes
      * If you scan without a DSS, the 'Body Report' script no longer falsely claims that all bodies are unsuitable for landing
      * 'Module swapped' event - revised script to better handle swapping to an empty slot

### 2.4.0-b3
  * Core
    * EDDI's version number is now shown in the application's title bar
    * Module events now update the ship object
    * Internal clean-up: now 100% green on unit tests, compiler warnings and code analyser issues
  * Events
    * Add 'Module sold remote' event
    * Add 'Module transfer' event
    * Add 'Modules stored' event
    * Revised variable names for module events
  * Speech Responder
    * 'Module purchased' event - new script
    * 'Module retrieved' event - new script
    * 'Module sold' event - new script
    * 'Module sold remote' event - new script
    * 'Module stored' event - new script
    * 'Module swapped' event - new script
    * 'Module transfer' event new script
    * 'Modules stored' event - new script

### 2.4.0-b2
  * Installer
    * Fixed: the installer was missing some of the documentation files. This was causing the app to pine for the fjords. And the documentation files.

### 2.4.0-b1
  * Core
    * Add 'DataScan' definition for types of datalink scans
    * Add new roles to the Ship Monitor
    * Add material type information to the Material Monitor
    * Add hyperlinks to EDDI.exe, linking to readme.md & the EDDI wiki
    * Change Log incorporated via hyperlink in the main window - no more need to check the forums when something changes
    * Changed the format for calling ring composition. Was 'composition.name', is 'composition'
    * Stellar belt clusters are now included in the definition of rings.
    * Update Readme.md to use revised hyperlinks pointing to the new project page.
    * Update Troubleshooting.md to use revised hyperlinks pointing to the new project page.
    * Update Variables.md to include information available for rings in 'Body scanned' and 'Star scanned' events
  * Events
    * Add 'AFMU repairs' event
    * Add 'Community goal' event
    * Add 'Data scanned' event when some type of datalinks (Data Links, Data Posts, Abandoned Data Logs, Listerning Posts, Wrecked Ships) are scanned
    * Add 'Data voucher awarded' event when you are awarded a data voucher
    * Add 'Friends status' event when a friendly commander changes status
    * Add 'Module purchased' event
    * Add 'Module retrieved' event
    * Add 'Module sold' event
    * Add 'Module stored' event
    * Add 'Module swapped' event
    * Add 'Mission redirected' event
    * Add 'Nav beacon scan' event
    * Add 'Music' event (triggered when the game music 'mood' changes)
    * Add 'Repair drone' event
    * Add 'Search and rescue' event when delivering items to a Search and Rescue contact
    * Add 'Ship sold on rebuy' event when when you sell a ship to raise funds on the insurance / rebuy screen
    * Clarified "channel" in 'Message received' to include 'npc' 
    * Update 'Body scanned' event to check whether BodyDetails successfully located the body
    * Update 'Message received' event, EDDI now distinguishes between a larger variety of message sources
    * Update 'Bond redeemed', 'Bounty redeemed', 'Fine paid', 'Data voucher redeemed', and 'Trade voucher redeemed' events to add Broker Percentage when redeemed via broker
    * Update 'Docked' event, now includes a list of station services under 'stationservices'
    * Update 'Mission accepted' event to correct a bug preventing wanted passengers from being detected
    * Update 'Jumped' and 'Location' events to include system population, when present
    * Update 'Screenshot' event, now contains longitude & latitude, when appropriate
    * Update 'Ship sold' event, now contains a value for the system where the ship was sold
    * Update 'Ship transfer initiated' event, now includes transfer time
    * Update 'Star scanned' event to add luminosity class property
  * Speech Responder
    * Add Spacialise() Cottle function.  Details on how to use this are in the SpeechResponder documentation
    * Add StartsWithVowel() Cottle function. Details on how to use this are in the SpeechResponder documentation
    * Script changes:
      * 'AFMU repairs' - new script
      * 'Community goal' - new script'
      * 'Bond redeemed' - revised to correctly get faction names and faction amounts
      * 'Data scanned' - new script
      * 'Data voucher awarded' - new script
      * 'Died' - new script
      * 'Docking granted' script revised to recognize asteroid bases
      * 'Friends status' - new script
      * 'Galnet news published' script revised to only ready out the titles & content of interesting articles
      * 'Limpet purchased' - new script
      * 'Limpet sold' - new script
      * 'Mission redirected' - new script
      * 'Music' - new script
      * 'Nav beacon scan' - new script
      * 'Power expansion vote cast' - new script
      * 'Repair drone' - new script
      * 'Search and rescue' - new script
      * 'Ship sold' - revised to include location for ships sold remotely
      * 'Sold ship on rebuy' - new script

## 2.3.0
  * Core
    * Tidy ups for reading from and writing to files to catch potential exceptions
    * Do not send data to EDSM or EDDN if in a multicrew session
    * Better handling of unknown commodities
    * Attempt to handle messages coming from unknown ships with the prefix "$ShipName_" 
    * Update internal list of commodities
    * Update internal list of commodities to include all known items
    * Fix error when caching starsystem information
    * Fix potential crash when comparing current and future star systems
    * Fix typo in test event for 'Commander continued'
    * Ignore nameplates when obtaining modules from journal
    * Add 'Enable ICAO' option on text-to-speech tab.  When enabled, planets and starsystems with alphanumeric qualifiers (e.g. the "AB 1" in "Shinrarta Dezhra AB 1") will be spoken phonetically (e.g. "Alpha Bravo One")
    * Catch corner cases where ship name could come back empty
    * Fix issue where 'Test script' button would not activate with custom scripts
    * Changing verbose logging checkbox updates immediately
    * Better updating of ship information from combined journal and API data sources
    * Add ship role 'Taxi'
    * Rename 'Companion App' tab to 'Frontier API' and update relevant text to clarify its use and operation
    * Volcanism for bodies is now an object.  For details of its fields check the relevant documentation
    * Add ancient artifact commodity definitions
    * Add ship value 'ident' which is the user-defined identification string for a ship
    * Allow monitors to handle events, and generate their own events in turn
    * New monitor: Material monitor.  This allows you to set minimum/desired/maximum limits for materials and generate events when the limits are exceeded.  Materials are tracked automatically in EDDI.  Full details of the material monitor operations are available at https://github.com/cmdrmcdonald/EliteDangerousDataProvider/wiki/Material-monitor
    * Remove the Netlog monitor.  This was only used to obtain destination system when jumping and is no longer required due to additional information made available in the journal for this purpose
    * Fix exploration role 'Trailblazer' to have correct name (was showing up as 'Explorer')
    * Add reset button to Frontier API configuration panel
  * EDDN Responder
    * Migrate to new EDDN endpoint
    * Avoid use of data from Frontier API when setting starsystem information
  * EDSM Responder
    * Provide error message when attempt to obtain logs fails
    * Provide numeirc progress information rather than system name when syncing logs
    * Add upload of materials, ship, etc.
  * Events
    * Update 'Body scanned' event - added axial tilt.  Added earth mass, radius and information on reserve level of rings.  Made a number of items optional as they are no longer present if a DSS is not used to scan the body
    * Update 'Bond awarded' event to provide details of the awarding faction
    * Add 'Bond redeemed' event when a combat bond is redeemed
    * Add 'Bounty redeemed' event when a bounty voucher is redeemed
    * Update 'Commander continued' event - added fuel level of current ship
    * Add 'Crew joined' event when you join a crew
    * Add 'Crew left' event when you leave a crew
    * Add 'Crew member joined' event when someone joins your crew
    * Add 'Crew member left' event when someone leaves your crew
    * Add 'Crew member launched' event when a crewmember launches a fighter
    * Add 'Crew member removed' event when you remove someone from your crew
    * Add 'Crew member role changed' event when a crewmember changes their role
    * Add 'Crew role changed' event when your role on someone's crew changes
    * Add 'Data voucher redeemed' event when a data voucher is redeemed
    * Updated 'Docked' event to include distance from start
    * Add 'File Header' event when a new journal file is found.  This is usually just for internal use
    * Add 'FSD engaged' event when the FSD is engaged to jump to supercruise or hyperspace.  This replaces the 'Jumping' event and has a similar script
    * Deprecate 'Jumping' event.  This is part of the netlog monitor, which is no longer required.  The functionality has been replaced by the 'FSD engaged' event
    * Update 'Liftoff' event to record if the ship lifting off is player controlled or not
    * Update 'Location' event to add longitude and latitude if the location is on the ground
    * Add 'Material inventory' event when material information is supplied
    * Add 'Material threshold' event when a threshold set in the material monitor is breached
    * Update 'Message received' event to include NPC messages.  Additional field 'Source' provides more details about the source of the message
    * Update 'Mission accepted' event to include the number of kills for massacre missions
    * Add 'Settlement approached' event
    * Add 'Ship renamed' event to record when ship names and idents are changed
    * Add 'Ship repurchased' event to record when player resurrects with their existing ship
    * Update 'System state report' to say nothing if the system is not in any particular state
    * Update 'Touchdown' event to record if the ship touching down is player controlled or not
    * Add 'Trade voucher redeemed' event when a trade voucher is redeemed
  * Galnet Monitor
    * Galnet monitor now categories and stores news articles
  * Material Monitor
    * Update locking conditions for inventory
  * Ship monitor
    * Track cargo using loadout event.  This only gives a rough idea of cargo as it only triggers with certain events (docking, swapping ship etc.)
    * Track limpets.  This gives an approximation of how many limpets are on board and is useful when docked but does not track limpets as they are used
    * Update locking conditions for shipyard
    * Lock updates to ship monitor data structures to prevent corruption
    * Do not update ship name or ident if it contains filtered sequences (***)
  * Speech Responder
    * Script changes:
      * 'Blueprint make report' - new script to report how many of a blueprint can be made
      * 'Blueprint material report' - new script to report which materials are required for a blueprint
      * 'Body report' - add details of volcanism; handle retrograde rotation
      * 'Body scanned' - remove name of body so that it is not repeated in following report
      * 'Bond redeemed' - new script
      * 'Bounty redeemed' - new script
      * 'Commodity sale check' - various updates to give more reliable results
      * 'Commodity collected' - fix bug where 'cargo' was used instead of 'commodity'
      * 'Commodity sold' - do not report profit when purchase price is 0 (mined/stolen/mission commodities)
      * 'Crew fired' - add context
      * 'Crew hired' - add context
      * 'Crew member joined' - new script
      * 'Crew member left' - new script
      * 'Crew member launched' - new script
      * 'Crew member removed' - new script
      * 'Crew member role changed' - new script
      * 'Crew role changed' - new script
      * 'Crew joined' - new script
      * 'Crew left' - new script
      * 'Data voucher redeemed' - new script
      * 'Docked' - moved information messages to the 'Market information updated' script to trigger at a better time
      * 'Entered normal space' - add context
      * 'Entered supercruise' - add context
      * 'FSD engaged' - new script
      * 'Galnet news' - new script
      * 'Galnet news published' - updated script to only report on latest non-status news reports; by default does not read contents
      * 'Galnet latest news' - new script
      * 'Galnet oldest news' - new script
      * 'Galnet unread report' - new script
      * 'Jumped' - call system security report here rather than in 'Jumping' to guarantee up-to-date information
      * 'Liftoff' - change speech depending on if player is controlling ship or not
      * 'Limpet check' - correctly select singular or plural of limpet
      * 'Location' - add context
      * 'Market information updated' - new script taken from the end of the previous 'Docked' script
      * 'Material discard report' - new script to report how much of a particular material can be discarded (as per the material monitor settings)
      * 'Material inventory report' - new script to report how much of a particular material as defined by state or context is on board
      * 'Material location report' - new script to report where to obtain a particular material as defined by state or context
      * 'Material required report' - new script to report how much of a particular material as defined by state or context is required (as per the material monitor settings)
      * 'Material use report' - new script to the blueprint uses of a particular material as defined by state or context
      * 'Materials discard report' - new script to report which materials can be discarded (as per the material monitor settings)
      * 'Materials required report' - new script to report which materials can be discarded (as per the material monitor settings)
      * 'Message received' - updated to only respond to player messages, and to use appropriate source
      * 'Settlement approached' - new script
      * 'Ship refuelled' - state when ship is fully refuelled from scooping
      * 'Ship renamed' - new script
      * 'Ship swapped' - add reminders for limpets and crew if appropriate
      * 'Star scanned' - remove name of star so that it is not repeated in following report
      * 'System state report' - fixed a couple of typos
      * 'Touchdown' - change speech depending on if player is controlling ship or not.  Name body on which the ship has touched down
      * 'Trade voucher redeemed' - new script
    * Fix crash when showing "Changes from default" window
    * Handle additional conditions for "changes from default" windows when editing templates in the speech responder
    * When renaming scripts ensure that they are renamed not copied
    * Update default templates to current latest version when reading in a custom personality
    * Attempt to ignore invalid system names if presented in BodyDetails()
    * Add "Log" function to write information to EDDI's log.  This is an aid when debugging templates
    * Fix issue where new templates might show up in custom personalities blank rather than with the contents of the default template
    * Change edit window's "Show default" button to "Compare to default"; allowing diff-style comparison between the current and default scripts for templates
    * Added 'ICAO' function to allow ICAO-style speech of ship identifiers, sector names etc.
    * Added 'Play' function to play an audio file instead of a speech
  * VoiceAttack Responder
    * Use defensive copies of arrays to avoid potential exceptions when they are modified whilst we are reading them
    * Add other VoiceAttack commands
      * 'Tell me about this sytem' - Find out about the current system
    * Add "Ship ident" and "Ship ident (spoken)"
    * Update 'disablespeechresponder' and 'enablespeechresponder' plugin contexts to continue to work in the background but just be quiet
    * Add VoiceAttack commands for the new speech responder plugin contexts:
      * "Be quiet" - Speech responder will not talk unless explicitly asked for information
      * "You may talk" - Speech responder will talk about events occuring in-game without prompting (this is the default behaviour)
    * Add VoiceAttack commands for the material monitor:
      * 'How many <material> are on board' - Find out how many units of a particular material is on board
      * 'How many <material> do I need' - Find out how many units of a particular material are required to meet your desired level as set in the material monitor
      * 'What use is <material>' - Find out the blueprints that use a particular material
      * 'Where can I obtain <material>- Find out where to obtain a particular material
      * 'Which materials can I discard' - Find out how many units of materials can be discarded due to being above your maximum or desired level as set in the material monitor
      * 'Which materials do I need' - Find out how many units of materials are still required due to being below your minimum or desired level as set in the material monitor
    * Add VoiceAttack commands for the Galnet monitor:
      * 'Is there any news?' - Report the number of unread articles
      * 'Read the latest community goal [news;]' - Read the latest community goal article
      * 'Read the latest conflict [news;report]' - Read the latest weekly conflict report
      * 'Read the latest democracy [news;report]' - Read the latest weekly democracy report
      * 'Read the latest economy [news;report]' - Read the latest weekly economy report
      * 'Read the latest expansion [news;report]' - Read the latest weekly expansion report
      * 'Read the latest health [news;report]' - Read the latest weekly health report
      * 'Read the latest news' - Read the latest news article
      * 'Read the latest security [news;report]' - Read the latest weekly security report
      * 'Read the latest starport status [news;report]' - Read the latest starport status update
      * 'Read the latest community goal [news;]' - Read the latest community goal article
      * 'Read the [next;oldest] conflict [news;report]' - Read the oldest weekly conflict report
      * 'Read the [next;oldest] democracy [news;report]' - Read the oldest weekly democracy report
      * 'Read the [next;oldest] economy [news;report]' - Read the oldest weekly economy report
      * 'Read the [next;oldest] expansion [news;report]' - Read the oldest weekly expansion report
      * 'Read the [next;oldest] health [news;report]' - Read the oldest weekly health report
      * 'Read the [next;oldest] news' - Read the oldest news article
      * 'Read the [next;oldest] security [news;report]' - Read the oldest weekly security report
      * 'Read the [next;oldest] starport status [news;report]' - Read the oldest starport status update
    * Add other VoiceAttack commands
      * 'What do I need for <blueprint>' - Find out the materials required for a particular blueprint
      * 'How many <blueprint> can I make' - Find out how many of a particular blueprint you can make with your current inventory

## 2.2.3
  * Fix issue where undocumented change in Frontier API would cause EDDI to crash
  * Update netlog monitor to handle new log format
  * Add ship definition for Dolphin
  * Add module definitions for Dolphin bulkheads

## 2.2.2
  * Make a nuber of scan items optional for compatibility with Elite 2.3

## 2.2.1
  * Add mechanism to see if game version is beta or production, using remote production build list if available
  * Update EDSM responder to not send data if game version is beta
  * Update EDDN responder to send data to /test schemas if game version is beta

## 2.2.0
  * Core
    * Fix issue where commander insurance % is not set internally
    * Add assisated upgrade for new versions of EDDI
    * Allow opt-in to beta versions of EDDI
    * Incorporate data from Body scanned and Star scanned events in to local database
    * Ensure that location script is always triggered on first login
    * Add CQC rating to commander
    * Fix issue where hull damage events were not always triggered
    * Add module definitions for Module Reinforcement Packages
    * Initial addition of EDDI context.  Context attempts to keep track of what EDDI is talking about, to provide the possiblity of two-way dialogue.  Details about context can be found in the speech responder documentation
    * Station objects no longer have economy arrays, instead they just have a primaryeconomy item
    * Attempting to copy a personality over an existing personality will no longer succeed
  * Events
    * Add 'Mission failed' event
    * Add 'System faction changed' and 'System state changed' EDDP events
  * EDDP monitor
    * Add EDDP monitor.  This monitor watches EDDP for state and ownership information about systems and generates events when changes are spotted.  See the 'EDDP monitor' tab for more information
  * Speech Responder
    * Add 'View' button for all scripts
    * Add speech responder function 'BodyDetails' to obtain body details.  Details of this function are in the SpeechResponder help documentation
    * Script changes:
      * 'Body report' - new script that uses context information to report on a body
      * 'Body scanned' - added context information.  Call new function 'Body report' to provide body details
      * 'Bounty awarded' - added context information
      * 'Bounty incurred' - added context information
      * 'Cleared save' - added context information
      * 'Cockpit breached' - added context information
      * 'Combat promotion' - added context information
      * 'Commodity collected' - added context information
      * 'Commodity ejected' - added context information
      * 'Commodity refined' - added context information
      * 'Commodity sale check' - avoid overly-long response if there are lots of commodities that can be sold
      * 'Commodity sold' - added context information
      * 'Docked' - do not pause just to find out that there is no commodity/swapout/limpet check result
      * 'Docking granted' - added context information.  Call new function 'Landing pad report' to provide pad details
      * 'Fine incurred' - added context information
      * 'Jumping' - added context information
      * 'Landing pad report' - use context information to report on a landing pad
      * 'Material collected' - added context information
      * 'Material discarded' - added context information
      * 'Material discovered' - added context information
      * 'Material donated' - added context information
      * 'Message received' - added context information
      * 'Message sent' - added context information
      * 'Repeat last speech' - new script that repeats the last speech
      * 'Star report' - new script that uses context information to report on a star
      * 'Star scanned' - added context information.  Call new function 'Star report' to provide body details
      * 'System distance report' - use context information to report on the distance to a system
      * 'System faction changed' - new script triggered when there is a change in the controlling faction of a system
      * 'System report' - use context information to report on a system
      * 'System state changed' - new script triggered when there is a change in the controlling faction of a system
      * 'System state report' - use context information to report on the state of a system
      * 'Undocked' - do not pause just to find out that there is no limpet check result
  * VoiceAttack Responder
    * Ensure that state changes are picked up by VoiceAttack plugin immediately
    * Update VoiceAttack with context-related commands:
      * 'Please repeat that/What was that?/Could you say that again?/Say that again' - repeat EDDI's last scripted response
      * 'Remind me of that landing pad/Which landing pad was it?' - repeat the landing pad name and location when docking
      * 'Tell me about it/Tell me more' - provide more information about the last item EDDI mentioned
      * 'Tell me about the/that system' - provide more information about the last system EDDI mentioned
      * 'Tell me about the/that planet/body' - provide more information about the last body EDDI mentioned
      * 'Tell me about the/that star' - provide more information about the last star EDDI mentioned
      * 'How far is that system?' - provide a distance report for the last system EDDI mentioned
  * Misc
    * Send additional data to EDSM

## 2.1.0
  * Core
    * Add 'stolen' flag and 'missionid' identifier to cargo
    * Use more intelligent method to work from FD names to definitions for commodities, and provide better fallback names if missing
    * Ensure that there is a space either side when using the word 'dash' in place of the symbol '-'
    * Unconditionally disable EDDN and EDSM responders whilst in CQC.  This ensures that no data is accidentally sent to these systems
    * Add option to write speech responder output to a file.  This is an option that can be checked in the speech responder tab, and writes all speech to %APPDATA%\EDDI\speechresponder.out
    * Allow speech responder to not speak when subtitles are written
    * Retry companion API profile call if returned information is outdated.  This should help to avoid situations where outfitting and market data is out-of-date
    * Ensure that distance from home is updated whenever system co-ordinates are updated
    * Ensure that personality names do not contain illegal file or path characters to avoid issues when saving them
    * Send EDDN messages with "Unknown Commander" when commander name is not known
    * Provide new API for EDDI versioning.  This allows EDDI to provide more information about updates to users, and is a precursor for automatic updates
    * Track current vehicle which commander is controlling, and make the value available to the speech and VoiceAttack responders
    * Update local system database with details from the journal and companion API.  This ensures that this data is always as up-to-date as possible
    * Add unknown ship materials
    * Update Zinc good and great percentages with latest data
    * Fix material name for Cadmium
    * Add 'Engineer' government type for engineer bases
    * Enable update and outdate messages
  * Events
    * Add event when being scanned for cargo by an NPC
    * Add event when being attacked by an NPC
    * Add event when being interdicted by an NPC
    * Add 'Ship shutdown' event
    * Add 'Power voucher received' event
    * Add 'target' to BountyAwardedEvent for Elite release 2.2.03 and above
    * Add 'distance' to JumpedEvent.  This provides the distance jumped, in light years
    * ModificationCraftedEvent now has details of commodities used in crafting as well as materials
    * Update mission accepted and completed events to contain more fields.  Details on the new fields are in the relevant documentation
    * Update location event with details of station name and type where available
    * Add events when entering and leaving a station's no-fire zone.  Details about the individual events are in the relevant documentation
    * Add powerplay events.  Details on the new events are in the relevant documentation
    * Provide correct name of repaired item in ShipRepairedEvent
  * Speech Responder
    * Ensure that speech responder's P() Cottle function works for ships
    * Add SetState() Cottle function.  Details on how to use this are in the SpeechResponder documentation
    * Fix implementation of ShipDetails() Cottle function so that it works according to its documentation
    * Use fixed-width font for edit script window
  * Speech Responder scripts
    * Update Commodity sale check' script to not suggest that stolen goods or mission-specific goods can be sold
    * Added 'Synthesised' script
    * Fix 'Mission completed' script to correctly provide information about commodities received as mission rewards
    * Update 'Hull damaged' script to only report damage if the player is in the vehicle that is damaged
    * Update 'Docking granted' script to provide clearer information on the location of pads
    * Update 'Touchdown' script to use "coordinates" rather than "co-ordinates" as the latter can cause problems with some TTS voices
    * Update 'Swapout check' script to use modules' modified flag to see if a module has modifications
    * Update 'Commodity sale check' script to ensure that a commodity is in demand at the target station before reporting on a sale
    * Add scripts for new events 'Entered CQC', 'Power commodity fast tracked', 'Power commodity delivered', 'Power commodity obtained', 'Power salary claimed', 'Power expansion vote cast', 'Power defected', 'Power left', and 'Power joined'
    * Update script for 'Mission completed' event to provide more detail on the accepting faction and reward
    * Update script for 'Mission accepted' event to provide a warning about illegal passengers
    * Update script for 'Jumping' event to recognise when last system's allegiance is null (as opposed to empty)
    * Update 'Docking denied' script to provide info on the reason
    * Update 'Location' script to provide correct information when docked at ground stations
    * Update 'Body scanned' script to provide additional information about rotation period of the planetary
    * Update 'Galnet news published' script to read full contents of interesting items
  * VocieAtack integration
    * Add 'Voice' parameter for VoiceAttack's 'say' and 'speech' commands to allow individual over-rides of default Voice
    * Add VoiceAttack 'setspeechresponderpersonality' context to change the speech responder's personality.  Details on this is in the VoiceAttack documentation
    * Add VoiceAttack 'disablespeechresponder' and 'enablespeechresponder' contexts to temporarily disable and enable the speech responder.  Details on these are in the VoiceAttack documentation
    * Remove reference to 'last jump' in VoiceAttack documentation (this is provided by the JumpedEvent)
    * Use recursive/dynamic method to populate VoiceAttack variables.  This provides many more VoiceAttack variables than were previously available; details are in the relevent documentation
    * Add module definitions for SRV, fighter and training loadouts
    * Provide update and MOTD information in VoiceAttack window if applicable
    * Add ability to set state variables from VoiceAttack.  Details on how to use this are in the VoiceAttack documentation
    * Ensure that VoiceAttack decimal values are not written as integers

## 2.0.13
  * Fix issue where engineer rank journal message without rank would cause a crash
  * Allow non-string sample events for testing scripts
  * Add sample galnet news event for testing
  * Do not throw spurious errors when shutting down
  * Ensure that stellar class VoiceAttack variable is unset if the information available

## 2.0.12
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

## 2.0.11
  * Further fixes for renamed and missing fields in 2.2.02
  * Add "log out" option for companion API tab in UI
  * Provide internal Nullable values in VoiceAttack
  * Update VoiceAttack documentation with new variables
  * Credentials are removed on EDDI uninstall

## 2.0.10
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

## 2.0.9
  * Do not update ship configuration when data is not available from the companion API
  * Be a little harsher when shutting down speech threads on close
  * Make home station accessible to scripts
  * Make current station more dynamic to match reality
  * Fix crash when bringing up help windows from VoiceAttack

## 2.0.8
  * Update coriolis export with additional properties for improved accuracy
  * Handle "Profile unavailable" response from companion API
  * Provide ship model rather than manufacturer to Coriolis
  * Update star class information with better probability distributions
  * Add stellar age and temperature probabilities
  * Catch bad allegiance data sent from companion API
  * Avoid repetition of docking information when still docked at the same station
  * Fix potential crash when station model is undefined
  * Update default docking and swapout scripts to make them less chatty

## 2.0.7
  * Add ship's main and total fuel tank capacities
  * Add capability to upload logs to EDDI server
  * Add ability to access EDDI's confguration UI from VoiceAttack
  * Update VoiceAttack profile with fuel tank variables
  * Provide better translation for VESPER-M4 when speaking that system's name
  * Add information about the VoiceAttack 'profile' command
  * Update good and great percentage values for materials (thanks to Baroness Galaxy)
  * Update coriolis export to include modifications
  * Fix gravity and terraformstate for planets (thanks to Michael Werle)

## 2.0.6
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

## 2.0.5
  * Added fuel used and fuel remaining to JumpedEvent
  * Handle missing data in body scans
  * Fix incorrect name of key in ShipyardNew
  * Update text on speech responder tab to be more informative about copying personalities
  * Fix oddity in jumping script where EDDI thought that 'None' was a real allegiance

## 2.0.4
   * Add FDev IDs for new ships and modules
   * Add phonetic pronunciation for Lakon
   * Fix issue where unknown IDs could cause a crash

## 2.0.3
   * Fix issue where jumps are not announced if the companion API is unavailable

## 2.0.2
  * Fix crash when companion API does not return correct information
  * Fix crash when starting EDDI after Elite has started
  * Attempt to patch around missing information when companion API does not provide it

## 2.0.1
  * Complete rework of product and VoiceAttack integration
  * EDDI no longer dependent on VoiceAttack for spoken responses to events
  * Use Frontier journal in addition to netlog to provide more events
  * Plugin architecture for monitors (that provide events) and responders (that act on events)
  * Introduction of personalities - bundles of responses for the speech responder 
  * Fix issue with text-to-speech engines mispronouncing sector names with two-letter phrases such as DR and NY

## 1.4.0
  * Rework network communications to provide better debug information
  * Unify logging to a single file
  * Always use the selected voice's default language when using phonemes
  * Validate spoken ship names for valid IPA
  * Provide more details when obtaining EDSM logs from configuration UI
  * Use local store as primary for star system data and update EDSM asynchronously
  * Silently drop existing phonetic ship names with invalid IPA

## 1.3.5
  * Tweaks to increase volume of processed voice
  * Fix issue with last utility slots on large ships being missed
  * Add verbose logging option to configuration UI
  * Add definition for huge burst lasers (fixed, gimballed)
  * Avoid hang when sending to EDSM
  * Avoid hang when sending to EDDP
  * Provide local co-ordinates when systems are not available in EDSM
  * Never write credentials to log file

## 1.3.1
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

## 1.3.0
  * Fix issue where unknown systems provided an incorrect JSON result
  * Add command to show the current system in EDDB
  * Add command to show the current station in EDDB
  * Add timeout for EDDP queries
  * Fix isue where hardpoints returned out of order resulted in incorrect export URL for Coriolis
  * Log engineer modifications as part of module definition

## 1.2.2
  * Fix issue where shipyard would not be populated in some situations
  * Close down HTTP responses sooner, hopefully fixing occasional hangs
  * Populate the email address field in the configuration GUI from stored data if available
  * Ensure that co-ordinates are sent to EDSM in US format regardless of client locale
  * Added 'System orbital stations' value

## 1.2.1
  * Added modules from 1.6/2.1
  * Added commodities from 1.6/2.1
  * Support for new modules when exporting to Coriolis
  * Handle situation where companion API is available but not responding with data
  * Added more star system translations
  * Removed requirement for verbose logging

## 1.2.0
  * Compatibility with Elite: Dangerous 1.6/2.1
  * Fixed issue where unknown station models would cause EDDI to crash
  * Fixed issue where commander's home system and insurance discount were reset on configuration startup
  * Added relative volume for text-to-speech voice
  * Added support for 1.6/2.1 new-style system change messages
  * Send system co-ordinates to EDSM as part of the travel log if available
  * Added more star system translations

## 1.1.0
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

## 1.0.0
  * Fix minor VoiceAttack script issues

## 0.9.4
  * Added 'System distance from home'
  * Provide information about primary faction, number of stations and distance from home with the system report
  * Added 'Tell me about this system' command to trigger the system report
  * Reworked star system name translation routines to provide correct result in more situations

## 0.9.3
  * Move to internal speech generation routines for computer voice

## 0.9.2
  * Fix issue where sold ships still showed up in the shipyard
  * Add information on modules and hardpoints to damage and outfitting reports
  * Add missing IDs for some EDDB->Coriolis mappings
  * Fix issue where removed ship names are not treated as absent

## 0.9.1
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

## 0.9.0
  * Fix issue where setting a home system caused scripts not to report system information
  * Do not create 'system change' or 'location change' events when in CQC 

## 0.8.9
  * Provide starsystem information even if profile is not available

## 0.8.8
  * Fix crash if starsystem has no stations

## 0.8.7
  * Avoid VoiceAttack bug that causes VoiceAttack to crash

## 0.8.6
  * Added more checks for correct system data prior to triggering system change event
  * Added debug value for the event loop
  * Separated the event loop from the startup, to provide better reliability for the event loop [B]Anyone updating will need to change the startup command in their profile from EDDI event loop to EDDI startup[/B]
  * Added 'System minutes since previous visit' value

## 0.8.5
  * Added callsigns, names and roles for ships
  * Added home system and station
  * Added more events in the handler.  These are often triggered by voice actions themselves, but have been built in this way to allow for the future when these events become available directly to EDDI
  * Added the name of the last station the commander docked at
  * Added the ship's fuel tank capacity
  * Add ship name and callsign to coriolis export
  * Provided a number of voice-activated commands; see the README for details

## 0.8.0
  * Added EDSM integration: provide the ability for EDDI to send data to EDSM, keeping a log of every system you have visited
  * Ensure that "Last system rank" is set appropriately
  * Move from 'Login' to 'Configuration' binary for setting up EDDI

## 0.7.3
  * Event loop only returns if there is a new event to handle
  * Avoid problems if VA_Init1() is called multiple times
  * Added decimal variables 'Stored ship *n* distance' for the distance to each stored ship from the current system

## 0.7.2
  * Ensure that Environment is set on startup
  * Ensure that cached system data is refreshed correctly

## 0.7.1
  * Fix issue where Empire rating was showing incorrectly with Federation names
  * Catch errors when authenticating and obtaining the first profile and display a suitable error message
  * Added text variables "Ship model (spoken)", "System name (spoken)", "System power (spoken)", "Last system name (spoken)" and "Last system power (spoken)" to provide values that fit the text-to-speech engine
  * Renamed string variables "Credits", "Debt", "Ship value", "System population", "Last system population", "Ship <module> station discount" to have the " (spoken)" suffix.  The old names of the variables will be removed in the next major release.

## 0.7.0

  * Deprecated int variables "Credits", "Debt", "Ship value", "System population", "Last system population" in favour of decimal values with the same name.  The int variables were in thousands due to int limits and the decimal values are in units.  The int variables will be removed in the next major release
  * Added event loop to monitor the netlog
  * Added decimal variables "Ship * cost" "Ship * value" and "Ship * discount" for each of the ship's standard modules (e.g. "Ship power plant cost", "Ship power plant value" and "Ship power plant discount").  Cost is the number of credits the commander spent purchasing the module, Value is the undiscounted cost of the module and discount is (1 - cost / value)
  * Added decimal varaibles "Ship * station cost" for the cost of the ship's existing modules at the currently docked station.
  * Added text varaibles "Ship * station discount" for the discount possible by purchasing the ship's existing modules at the currently docked station.
  * Added int variable "System visits" that contains the number of times that the commander has visited the current system since the plugin was initialised
  * Added datetime variable "Previous system visit" that contains the date and time of the the commander previously visited this system
  * Added text variable "Environment" that lists the current environment ("Normal space" or "Supercruise")
  * Fixed issue where templates were updated with commander data, resulting in incorrect data after the first refresh

## 0.6.1

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

## 0.6.0

  * Added decimal variables 'System X' 'System Y', 'System Z', 'Last system X', 'Last system Y', 'Last system Z' using EDDB co-ordinates
  * Added decimal variable 'Just jump' with the distance of the last jump in LY to two decimal places
  * Fixed incorrect trade rank (showed 'Elite' as 'Pioneer')
  * Added int variable 'Stored ships' with the number of ships in storage
  * Added string variables 'Stored ship *n* model', 'Stored ship *n* system' and 'Stored ship *n* location' for each ship in storage

## 0.5.0
Initial release

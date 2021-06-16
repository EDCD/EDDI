# Change Log

Full details of the variables available for each noted event, and VoiceAttack integrations, are available in the individual [event pages](https://github.com/EDCD/EDDI/wiki/Events).

### 4.0.1-b1
  * Speech Responder
    * Scripts
      * Updated the `Embark`, `SRV docked`, `SRV launched`, and `Vehicle destroyed` events to fix a bug with th the SRV deployed warning.
  * VoiceAttack Responder
    * Fixed scripts invoked from the VoiceAttack `speech` context having priority 5 rather than priority 3 by default.

### 4.0.0
  * Core
    * Promote 4.0.0-rc1 to final

### 4.0.0-rc1
  * Inara Responder
    * Incorporated Inara API updates for Odyssey (except suit loadouts will be implemented later)
  * Speech Responder
    * Events
      * `Backpack` event added
      * `Backpack changed` event added
      * `Disembark` event, revised the `fromtaxi` property to `fromtransport` (since it will be true for both Apex taxis and Frontline dropships)
      * `Dropship deployment` event added
      * `Embark` event, revised the `totaxi` property to `totransport` (since it will be true for both Apex taxis and Frontline dropships)
      * `Mission accepted` event variables updated to include micro-resources (on foot items)
      * `Mission completed` event variables updated to include micro-resources (on foot items)
      * `Ship locker` event added
    * Functions
      * `Emphasize()` function tweaked to try to improve compatibility with more voices.
      * `Play()` function revised to permit a wide variety of audio formats, permit asynchronous playback, and permit custom volumes.
    * Scripts
      * `Carrier jumped` script, revised to correct missing "Information:" verbiage when the engineer report is the only applicable report.
      * `Crime report` script, updated to resolve scripting errors.
      * `Engineer report` script, revised to refine grammar.
      * `Jumped` script, revised to correct missing "Information:" verbiage when the engineer report is the only applicable report.
      * `Mission check system` script, fixed a bug that could prevent the station model from being referenced.
      * `Mission completed` script, updated to include micro-resources (on foot items)
      * `System report` script, updated to indicate the security level of the system.
    * User Interface
      * The current script selection is now remembered after accepting an edited script.

### 4.0.0-b3
  * Core
    * Fixed a bug that caused EDDI to fail to look up engineer systems after new engineers were added to the game.
    * Added new microbiologist and mercenary ranks
  * Speech Responder
    * Events
      * `Commander promotion` event added
      * `Combat promotion` event removed (obsolete)
      * `Empire promotion` event removed (obsolete)
      * `Exploration promotion` event removed (obsolete)
      * `Federation promotion` event removed (obsolete)
      * `Trade promotion` event removed (obsolete)
    * Scripts
      * `Engineer report` script updated to more gracefully handle engineers without defined specialties.

### 4.0.0-b2
  * Inara Responder
    * Fixed a missing .dll file that caused the Inara Responder not to load. 
  * Speech Responder
    * Events
      * `Suit purchased` event added

### 4.0.0-b1
  * Core
    * Added 3 new vehicle types:
      * On Foot
      * MultiCrew
      * Taxi
    * EDDI beta releases no longer send to test endpoints for EDDN or the EDSM Responder.
    * Fixed a bug that could double-count signal sources when re-logging. 
    * Fixed a bug that could cause undockable approached settlements to be counted as stations.
    * Fixed a bug that had broken access to the `category` property in material objects. 
    * "Conflict zone" signal sources are no longer described as "Combat zone" signal sources.
    * `Starsystem` object properties updated to add `scannedbodies` and `mappedbodies` counts.
  * Crime Monitor
    * Fixed a bug that caused us to forget the names of known factions when we couldn't connect to a server providing faction data.
    * Fixed a parsing error that could occur when trying to parse data for new factions not present in EDDB data dumps. 
  * Galnet Monitor
    * Reduced Galnet article polling from 30s / 2min to 5min / 15min.
    * Delayed active monitoring until 5 minutes after we become active in game rather than 5 minutes after EDDI is launched.
  * Material Monitor
    * Expanded conditions that can trigger a `Material threshold` event (for example, if the material amount is increased above the minimum)
    * Fixed a bug that could prevent the `Material threshold` from firing when we meet but do not exceed the maximum material threshold.
    * Fixed a bug that could prevent material counts from being updated for materials no longer in inventory.
  * Mission Monitor
    * Fixed a bug that temporarily stripped community goal missions from the mission list.
    * Fixed a bug that prevented community goal missions from sometimes being identified as community goal missions. 
  * Speech Responder
    * Events
      * `Book transport` event added
      * `Cancel transport` event added
      * `Commander continued` event, new properties added from Odyssey data
      * `Died` event, revised properties structure
      * `Disembark` event added
      * `Embark` event added
      * `Liftoff` event, new properties added from Odyssey data
      * `Location` event, new properties added from Odyssey data
      * `Micro resources purchased` event added
      * `Touchdown` event, new properties added from Odyssey data
    * Functions
      * Revised the `EngineerDetails` function to accept a system name as an input.
      * Updated the ShipCallSign() function to improve formatting and use an optional second parameter to customize the response.
      * Updated the ShipName() function to provide appropriate responses when in a taxi or similar, and to document its second argument (which is optional).
    * Personalities
      * Portuguese default personality updated (thanks to @Kenjiro). 
    * Scripts
      * `Bond awarded` script revised to more gracefully handle Odyssey contexts.
      * `Bounty awarded` script revised to more gracefully handle Odyssey contexts.
      * `Carrier jumped` script updated to announce engineer systems.
      * `Community goal` script revised and re-enabled. The event is now written only in response to specific changes in community goal status.
      * `Community goals` script added, updated whenever the game provides updated information on community goals.
      * `Died` script revised to more gracefully handle Odyssey contexts.
      * `Engineer report` script added.
      * `Entered normal space` script updated to provide easier access to invariant bodytype names and to correct a bug around filtering unknown station types.
      * `Fuel check` script revised to more gracefully handle Odyssey contexts.
      * `Jumped` script updated to announce engineer systems.
      * `Material threshold` script updated for expanded triggering conditions.
      * `Message received` script and properties updated to better support localization.
      * `Mission check galaxy` script updated to more gracefully handle community goals.
      * `Mission check station` script updated to more gracefully handle community goals.
      * `Mission check system` script updated to more gracefully handle community goals.
      * `Ring hotspots detected` script updated to correct a bug preventing the script from triggering for miners.
      * `Ship arrived` event updated to use the `ShipName()` function and provide improved phonetics. 
      * `Ship rebooted` event updated to provide localized and invariant module names rather than ship slots.
      * `Signal detected` script revised to reference `conflict zone` signal sources rather than `combat zone` signal sources.
      * `SRV docked` script revised to add a context variable.
      * `SRV launched` script revised to add a context variable.
      * `Swapout check` script revised to clarify that swapping out the module will reduce your re-buy.
    * User Interface
      * It is now possible to sort scripts by priority and enabled status.
      * It is now possible to search script names, descriptions, and contents with a new filter box.
      * It is now possible to disable all scripts at once (either when copying a personality or by accessing a context menu on the `Enabled` column header)
      * Fixed the selected personality combo box losing track of the current selected item when a personality was deleted.
    * Variables
      * Added boolean values for `odyssey` and `horizons` game states.
  * Status Monitor
    * Updated the Status Monitor for Odyssey data and 
    * Updated status documentation in Variables.md
  * VoiceAttack
    * Added system variables `{INT:System scanned bodies}` and `{INT:System mapped bodies}`.
    * Added boolean values for `odyssey` and `horizons` game states.

### 3.7.3
  * Treat alpha game clients just the same as beta game clients, i.e. do not upload data to live endpoints.

### 3.7.2
  * Promote 3.7.2-rc3 to final

### 3.7.2-rc3
  * Core
    * Fixed a bug with parsing float / decimal commodity prices.

### 3.7.2-rc2
  * Core
    * Fixed a bug that caused the `Signal detected` event to identify non-unique signals as unique rather than the reverse.

### 3.7.2-rc1
  * Frontier API
    * Fixed a bug that would re-play the speech "Frontier API connection operational" when the token was refreshed.
  * Speech Responder
    * Scripts
      * `Bond awarded` updated to more consistently apply the P() function.
      * `Commander continued` updated to move mission check to new `Missions` event.
      * `Carrier jump engaged` updated to fix indentation.  
      * `Crime check system` updated to more consistently apply the P() function.
      * `Data voucher awarded` updated to more consistently apply the P() function.
      * `Fuel check` updated to more consistently apply the P() function.
      * `Mission abandoned` updated to more consistently apply the P() function.
      * `Mission check galaxy` updated to reduce verbosity.
      * `Mission check station` updated to reduce verbosity.
      * `Mission check system` updated to reduce verbosity and more consistently apply the P() function.
      * `Mission completed` updated to more consistently apply the P() function. 
      * `Mission expired` updated to more consistently apply the P() function.
      * `Missions` added, triggered at startup when mission information has been updated.
      * `Star report` updated to apply the List() function for notable features.
      * `System state report` updated to remove "the" prefixing faction names (to correct pronunciation of faction names like "The Fatherhood"). 

### 3.7.2-b2
  * Speech Responder
    * Functions
      * `Humanise()` revised to leave the interpretation of simple whole numbers like 1000 and 10000 to the culture-specific voice.
    * Scripts
      * `Body report summary` updated to correct some script redundancies and formatting errors.
      * `Community goal` updated to fix a formatting issue.
      * `Mission accepted` updated to more consistently apply the P() function with faction names.
      * `Mission completed` updated to correct typos.
      * `Mission failed` updated to more consistently apply the P() function.
      * `Mission redirected` updated to more consistently apply the P() function. 
      * `Mission warning` updated to more consistently apply the P() function.
      * `Module arrived` updated to more consistently apply the P() function.
      * `Power commodity delivered` updated to correct a script formatting error.
      * `Route details` updated to more consistently apply the P() function.
      * `Ship arrived` updated to more consistently apply the P() function.
      * `Ship sold` updated to more consistently apply the P() function.
      * `Ship sold on rebuy` updated to more consistently apply the P() function.
      * `Signal detected` updated to more consistently apply the P() function.
      * `System state report` updated to more consistently apply the P() function.

### 3.7.2-b1
  * Core
    * Fixed a bug that caused certain faction names (e.g. "Brazilian Armada X") to throw an exception when passed through the `P()` function.
  * Cargo Monitor
    * Cargo value (per unit) is now calculated as a weighted average of acquisition costs (rather than using the galactic average price).
  * EDSM Responder
    * Fixed a bug that could prevent sending pending sync data to EDSM after a request to stop the responder (e.g. on closing).
  * Inara Responder
    * Fixed a bug that could prevent sending pending sync data to Inara after a request to stop the responder (e.g. on closing).
  * Mission Monitor
    * Fixed a bug that could cause the Mission monitor to only process the first stacked mission in a set if all were updated at the same time.
    * Revised all missions to use the "Claim" status (rather than using "Complete" in some instances and "Claim" in others) after mission conditions are satisfied.
  * Speech Responder
    * Events
      * `Modules stored` updated to prevent an exception while testing.
      * `Signal detected` updated to fix a bug that could allow non-unique signals to be flagged as unique. 
    * Functions
      * Added an optional hint string 2nd parameter to the P() function to specify the type of pronunciation override to apply.
      * Disabling phonetic speech no longer disables all SSML based functions (e.g. `{Pause()}` will no longer cease to work with phonetic speech disabled)
      * Reorganized the way we store EDDI's custom Cottle functions. 
      * `Humanise()` revised to fix a bug that created inaccurate descriptions for certain numbers, to return short decimal numbers when they are able to accurately and succinctly describe the number, and to round a little more aggressively for reduced verbosity.
    * Scripts
      * `Bodies mapped` updated to default to the current star system if context variable `eddi_context_system_name` is not set and updated the P() function utilization.
      * `Bodies to map` updated to default to the current star system if context variable `eddi_context_system_name` is not set and updated the P() function utilization.
      * `Body materials report` updated the P() function utilization.
      * `Body report` updated the P() function utilization.
      * `Body report summary` updated the P() function utilization.
      * `Body volcanism report` updated the P() function utilization.
      * `Bond redeemed` updated the P() function utilization.
      * `Bounty awarded` updated the P() function utilization.
      * `Bounty incurred` updated the P() function utilization.
      * `Bounty redeemed` updated the P() function utilization.
      * `Carrier jump engaged` updated the P() function utilization.
      * `Carrier jump request` updated the P() function utilization.
      * `Carrier jumped` updated the P() function utilization.
      * `Commander continued` updated to set context variable `eddi_context_system_name`.
      * `Community goal` updated the P() function utilization.
      * `Data voucher redeemed` updated the P() function utilization.
      * `Discovery scan` updated to default to the current star system if context variable `eddi_context_system_name` is not set.
      * `Empire promotion` updated the P() function utilization.
      * `Entered normal space` updated the P() function utilization.
      * `Exploration data purchased` updated the P() function utilization.
      * `Federation promotion` updated the P() function utilization.
      * `Fine incurred` updated the P() function utilization.
      * `FSD engaged` updated the P() function utilization.
      * `Glide` updated the P() function utilization.
      * `Jumped` updated the P() function utilization.
      * `Launchbay report` updated the P() function utilization.
      * `Location` updated the P() function utilization.
      * `Mission accepted` updated to fix a typo preventing warnings about wanted passengers and to make the mission count both occasional and less frequent for higher commander combat ranks and updated the P() function utilization.
      * `Mission check galaxy` updated the P() function utilization.
      * `Mission completed` updated to summarize rewards more succinctly and include permit rewards. Community goals now use the localized name rather than "MISSION_CommunityGoal" and updated the P() function utilization.
      * `Mission redirected` updated to filter duplicate similar mission redirects (e.g. from stacked similar missions) and updated the P() function utilization.
      * `Module purchased` updated to better pronounce module class & grade.
      * `Module retrieved` updated to better pronounce module class & grade.
      * `Module sold` updated to better pronounce module class & grade.
      * `Module sold from storage` updated to better pronounce module class & grade.
      * `Module stored` updated to better pronounce module class & grade.
      * `Module swapped` updated to better pronounce module class & grade.
      * `Module transfer` updated to better pronounce module class & grade.
      * `Modules stored` updated to prevent an exception while testing and to better pronounce module class & grade.
      * `Power commodity delivered` updated the P() function utilization.
      * `Power commodity fast tracked` updated the P() function utilization.
      * `Power commodity obtained` updated the P() function utilization.
      * `Power defected` updated the P() function utilization.
      * `Power joined` updated the P() function utilization.
      * `Power left` updated the P() function utilization.
      * `Power salary claimed` updated the P() function utilization.
      * `Powerplay` updated the P() function utilization.
      * `Ship purchased` updated the P() function utilization.
      * `Ship renamed` updated the P() function utilization.
      * `Ship transfer initiated` updated the P() function utilization.
      * `Signal detected` Spanish translation updated to fix a typo (missing paranthesis).
      * `Star report` updated the P() function utilization.
      * `System distance report` updated the P() function utilization.
      * `System report` updated to default to the current star system if context variable `eddi_context_system_name` is not set and updated the P() function utilization.
      * `System state changed` updated the P() function utilization.
      * `System state report` updated to default to the current star system if context variable `eddi_context_system_name` is not set and updated the P() function utilization.
      * `Touchdown` updated the P() function utilization.
      * `Trade data purchased` updated the P() function utilization.
      * `Trade voucher redeemed` updated the updated the P() function utilization.
    * VoiceAttack Responder
      * Revised `$-` output to more clearly render commander phonetic name
      * Updated VoiceAttack wiki documentation to document implicit variables `$=` and `$-` as variables representing phonetic ship and commander names.

### 3.7.1
  * Core
    Fixed an exception when calculating distances if the second system were null (for example if a home star system were not set).
  * Mission monitor
    * Fixed a bug that could cause certain types of missions to flip from "Active" to "Claim" when logging into the game.
    * Fixed a bug that caused the station to not be recorded correctly for community goal missions.
  * Speech responder
    * Scripts
      * Updated the `Location` script to fix reporting station crimes and missions when it should be instead reporting system crimes and missions.
  * VoiceAttack responder
    * Updated EDDI.vap to correct an issue with landing pads not being reported correctly when queried.

### 3.7.1-b1
  * Core
    * If you cancel a jump in your fleet carrier, a one minute cooldown is initiated. A `Carrier cooldown` event is now triggered to signal that this cooldown is complete.
    * Integrated monitors can no longer be disabled. The EDDP and Galnet monitors operate independently and can still be disabled.
    * Rollbar telemetry service can now optionally be disabled by editing configuration file at %appdata%/EDDI/eddi.json.
    * Various bug and stability fixes.
  * EDDN Responder
    * Fixed a bug that could cause the incorrect commodity symbols to be forwarded to EDDN.
  * EDSM Responder
    * Fixed an issue with queued messages not being sent when the EDSM Responder was stopped.
  * Inara Responder
    * Fixed an issue with queued messages not being sent when the Inara Responder was stopped.
  * Speech responder
    * Test scripts can now be canceled by clicking on the "Test" button a second time while test speech is in progress.
    * Variables
      * Added `factions` (faction objects) to the documented properties for the `system` object.
      * Added `imports` (array of Commodity objects) to the `station` object
      * Added `exports` (array of Commodity objects) to the `station` object
      * Added `prohibited` (array of Commodity objects, requires Frontier API access) to the `station object`
      * Added `planetarystations` and `orbitalstations` to the `system` object
      * Added `carriersignalsources` to the `system` object
      * Added `solarday` and `solarsurfacevelocity` to the `body` object. 
      * The `alreadymapped` and `alreadydiscovered` properties of the `body` object are now nullable - a null value indicates that the exploration status is not yet known. 
    * Scripts
      * Updated the `Asteroid prospected` script to fix a typo.
      * Updated the `Bond redeemed` script to better utilize the `Humanise()` function.
      * Updated the `Bounty incurred` script to better utilize the `Humanise()` function.
      * Updated the `Bounty redeemed` script to better utilize the `Humanise()` function.
      * Updated the `Cargo report` event and script to use commodity objects for the station prohibited list.
      * Updated the `Commodity purchased` script to better utilize the `Humanise()` function.
      * Updated the `Commodity sale check` script to better utilize the `Humanise()` function.
      * Updated the `Commodity sold` script to better utilize the `Humanise()` function.
      * Updated the `Crime check station` script to resolve an occasional grammar issue.
      * Updated the `Data voucher awarded` script to better utilize the `Humanise()` function.
      * Updated the `Discovery scan script` script improve grammar around recommending a single body to be scanned.
      * Updated the `Market information` event and script to restore purchase, sales, and swapout checks when appropriate
      * Updated the `Mission check galaxy` script to fix a typo.
      * Updated the `Module purchased` script to better utilize the `Humanise()` function.
      * Updated the `Module retrieved` script to better utilize the `Humanise()` function.
      * Updated the `Module sold` script to better utilize the `Humanise()` function.
      * Updated the `Module sold from storage` script to better utilize the `Humanise()` function.
      * Updated the `Module stored` script to better utilize the `Humanise()` function.
      * Updated the `Module transfer` script to better utilize the `Humanise()` function.
      * Updated the `Ship repaired` event and script to remove hard-coded english strings, standardize handling across stations and fleet carriers, and simplify redundant variables.
      * Updated the `Ship transfer initiated` script to better utilize the `Humanise()` function.
      * Updated the `Signal detected` event and script to include a new `unique` property and fix a typo
      * Updated the `System report` script to separate carriers from stations

### 3.7.0
  * Promote 3.7.0-rc1 to final
  * Fixed a bug that could cause the `Ship loadout` event not to fire if piloting an Eagle with a module slotted in the military slot
  * Added an alpha channel to the splash screen

### 3.7.0-rc1
  * Core
    * EDDI standalone now displays a splash screen while bringing up the full UI, to give users immediate feedback that it is launching.
    * `Carrier cooldown` event is now sent to the carrier's owner even if they were not aboard for the jump.
    * Corrected the timing of the `Carrier cooldown` event.
    * `Carrier pads locked` and `Carrier jump engaged` events are now correctly cancelled if the carrier's jump is cancelled.

### 3.5.3-b7
  * Speech responder
    * Fixed text-to-speech errors introduced in b5 and b6 by reverting the Cottle text rendering package to the old version..

### 3.5.3-b6
  * Frontier API
    * Fixed missing client ID in 3.5.3-b5
  * Speech responder
    * ~~Fixed Speech Responder always reporting that script errors are at line zero.~~
    * Fixed a typo in the `Mission check galaxy` script.

### 3.5.3-b5
  * Core
    * ~~Updated Rollbar telemetry service to reduce web traffic when idle and to add some additional context from preceeding eddi.log entries.~~
    * Various bug fixes, including fixing a bug that could cause the UI to become de-coupled from the true speech configuration. 
  * Speech responder
    * ~~Updated Cottle text rendering package to the latest version.~~

### 3.5.3-b4
  * Core
    * Implemented new `Asteroid cracked` and `Asteroid prospected` events. 
    * Nanomedicines are now designated as a rare commodity.
    * Disregard bogus "Galactic Mean Price" from Fleet carriers.
    * Reduced filesystem polling when Elite is not active.
    * Defended against some corner-case IO exceptions.
  * Speech Responder
    * When the update server is unreachable, the voice message now says that "EDDI" rather than "I" could not reach it.
    * Fixed some edge cases around ship model pronounciation, notably Roman numerals.

### 3.5.3-b3
  * Cargo Monitor
    * Fixed a bug where the `need` property of mission-related cargo did not properly update for `Collect` mission types.
  * Core
    * Implemented a speculative fix for system visit counts sometimes becoming reset.
  * Mission Monitor
    * Added new `claim` state for missions where the expiration timer ceases upon completion of mission requirements. `Assassinate` and `Massacre` mission types, as an example. 
  * Speech Responder
    * Updated scripts to improve reporting of crime and mission related information: 
      * `Crime check station` 
      * `Crime check system`
      * `Entered normal space`
      * `Location`
      * `Mission check galaxy` 
      * `Mission check system` 
      * `Mission check station`
      * `Mission completed`
      * `Mission report`
      * `Touchdown`
    * Updated the community translation of the default personality for Brazilian Portuguese.

### 3.5.3-b2
  * Frontier API
    * Fixed an issue whereby the login process would try to launch a second instance of EDDI and fail.

### 3.5.3-b1
  * Core
    * Added support for all documented events etc for the Fleet Carriers update.
    * Behave gracefully rather than crashing to desktop when the EDSM servers are timing out.
  * Events
    * Added new event `Carrier cooldown`, triggered when you were docked at a fleet carrier during a jump and it completes its cooldown.
    * Added new event `Carrier jump cancelled`, triggered when you cancel a scheduled fleet carrier jump.
    * Added new event `Carrier jump engaged`, triggered when your fleet carrier performs a jump.
    * Added new event `Carrier jump request`, triggered when you request that your fleet carrier performs a jump.
    * Added new event `Carrier jump jumped`, triggered when you are docked at a fleet carrier as it completes a jump.
    * Added new event `Carrier pads locked`, triggered when your fleet carrier locks landing pads prior to a jump.
    * Added new event `Flight assist`, triggered when you toggle flight assist on or off.
    * Added new event `Hardpoints`, triggered when you deploy or retract your hardpoints.
    * Updated `Ship repaired` script .
  * Inara Responder
    * Many performance improvements.
  * Speech Responder
    * The `scoopable` property now considers all stars in the system, not just the primary star.
    * Fixed vocalization of whitespace characters by `Spacialise()`.
    * Fixed `event.station` not being populated in docking events.
    * Fixed `event.startlanded` and `status.vehicle` not being populated on startup.
    * The "Copy Personality" dialog now checks the name that you supply against the existing ones, to stop you from accidentally over-writing one.
    * Fixed a crash to desktop if the personality JSON files were hand-edited into an invalid state. Instead an error is logged and the offending JSON file is renamed.
  * Text-to-Speech
    * Ensured that voice effects (except radio effects and audio gain) are completely omitted when "level of voice processing" is set to zero.
  * VoiceAttack
    * Fixed an exception that could occur when closing VoiceAttack.
    * Fixed a bug ([#1666](https://github.com/EDCD/EDDI/issues/1666)) that could prevent variables `{DEC:System X} {DEC:System Y} {DEC:System Z}` from populating upon first entry into a system.

### 3.5.2
  * Speech responder
    * UI
      * **Implemented syntax coloring** (and there was much rejoicing). The engine is fully customizable: the UI for that will come later.
      * Cancel and OK buttons in secondary windows are now correctly bound to `Esc` and `Enter`.
      * Added context menus (aka right-click menus) to the view, edit and diff windows for scripts.
    * Extended `Spacialise()` to accept text as well as numbers. 
      * If SSML is enabled, it will render the text using the SSML function `SayAsLettersOrNumbers`. 
      * If not, it will add spaces between letters in a string and convert to uppercase. 
    * Script fixes:
      * Fixed a syntax error in the "FSD engaged" script.
      * Fixed missing colons in "System report" script.
    * Sundry:
      * Enabled Cottle code within the `Transmit()` function.
      * Only ships with the "Explorer" role will recommend bodies to map/scan after honking the FSS in inhabited space.
      * Fixed "Sagittarius A *" pronunciation in Cereproc voices.
      * Fixed pronunciation of ship names with mark numbers in Roman numerals. Your "Cobra Mk III" is now spoken correctly.
      * Fixed pronunciation of "Krait Phantom".
      * Applied workaround for Cereproc voices not supporting IPA properly.
      * Fixed `Engineer progressed` reporting rank but not stage when an engineer is unlocked [#1629](https://github.com/EDCD/EDDI/issues/1629).
      * Applied `Humanise()` to the credits reported by the `Bond awarded` script
  * Mission monitor
    * Fixed a rare edge case with the "nearest route" algorithm ([#1651](https://github.com/EDCD/EDDI/issues/1651), [#1652](https://github.com/EDCD/EDDI/issues/1652)). 
  * Core
    * Fixed shield state events not triggering ([#1605](https://github.com/EDCD/EDDI/issues/1605)).

### 3.5.1
  * Core
    * Ensured that all translation resources are now correctly incorporated.
  * EDSM responder
    * Added defensive code to fail gracefully should the EDSM server have a hiccup and not respond.
  * Speech Responder
    * Don't repeat the `System state report` when returning to a system that we've visited recently.
    * Fixed a typo in the `Engineer Progressed` script that could cause the Engineer's name to be omitted.
  * VoiceAttack Responder
    * Fixed a regression in 3.5.0: EDDI not remembering window position or tab position when running as a VoiceAttack plugin.

### 3.5.0
  * Promote 3.5.0-rc1 to final

### 3.5.0-rc1
  * Core
    * Updated commodity definitions, including adding Agronomic Treatments.
  * EDSM Responder
    * Fixed EDSM star map credentials not being reloaded when the EDSM responder is reloaded.
  * Frontier API
    * Fixed an issue whereby loading commander data from the Frontier API could clear other types of commander data.
  * VoiceAttack Responder
    * Added a VoiceAttack system variable for total bodies.

### 3.5.0-b2
  * Core
    * Fixed incorrect conversion to the local time zone when parsing timestamps from the journal and cAPI.
  * Ship Monitor
    * Fixed alignment of ship status (clean vs hot) to conform with the column header. 
  * VoiceAttack Responder
    * Fixed memory leaks when opening and closing EDDI's main window under VoiceAttack 

### 3.5.0-b1
  * Core
    * Added new `Inara Responder`.
    * Language setting are now preserved across both Standalone and Voice Attack modes of operation.
    * Fixed a general bug concerning order of initialisation, which manifested as the Material monitor only showing owned materials on first run.
    * Ensure exploration results are preserved when refreshing a 'stale' star system and the data is not yet available on the server.
    * Star system name now available as `systemname` in `Body scanned` and `Star scanned` events.
    * `totalbodies` now referenceable (within `StarSystem` object) after a discovery scan (honk).
    * Added support for new faction state `Blight` (written as "Drought" in the journals).
  * Cargo Monitor
    * Fixed #1465 whereby (for example) when a limpet launch frees cargo space which the refinery immediately uses, the Cargo monitor got out of sync.
  * Events
    * Added new event `Commander loading`, triggered at the very beginning of loading a game.
    * Added new event `Commander reputation`, triggered when your reputation is reported.
    * Added new event `Ring hotspots detected`, triggered when hotspot signals are detected in a ring during a SAA scan.
    * Added new event `Powerplay`, triggered while loading the game (if pledged).
    * Added new event `Statistics`, triggered while loading a game.
    * Added new event `Surface signals detected`, triggered when surface biological and/or geological signals are detected on a body during SAA scan.
    * Updated the `Body scanned` and `Star scanned` events with new property `scantype` (AutoScan, Basic, Detailed, NavBeacon, NavBeaconDetail)
    * Updated the `Commander continued` event with new properties `startlanded` and `startdead` (true if starting the game either landed or dead, respectively).
    * Updated the `Community goal` event with new properties `maxtier` and `maxtierrewards`.
    * Updated the `Jumped` event to remove the properties `destination` and `destdistance` (this data is now available via the `destinationsystem` object).
    * Updated the `Liftoff` and `Touchdown` events with new `nearestdestination` property.
    * Updated the `Location` and `Jumped` events contain new properties `power` and `powerstate` (if pledged).
    * Updated the `Next jump` (`FSDTarget` journal) event with new `jumpsremaininginroute` property.
    * Updated the `Ship targeted` event with new `power` property (if pledged).
  * Inara Responder
    * Simply paste in your Inara API key to have EDDI upload your Commander's progress to Inara. Uploads are batched for every 5 minutes to save bandwidth on both your machine and the Inara servers, and any outstanding events are sent upon game exit.
    * EDDI can now also get commander details from Inara, which are accessible via Cottle and VA functions as described below.
  * Speech Responder
    * UI revised to either `Delete` or `Reset` as script, as appropriate.
    * Replaced the `List launchbays` script with script `Launchbay report` (changed to conform to naming conventions for similar scripts). Added protection in script against empty launchbay data.
    * Fixed inadvertently disabled hyperlink in the UI "Read about the speech responder's functions here".
    * Updated the `Bodies mapped` script to correct a typo.
    * Updated the `Engineer progressed` event to stay silent by default when written at startup (with empty values, signaling that engineer data has been loaded).
    * Updated the `Fuel check` script to reduce verbosity and when very low on fuel to recommend the nearest known star system with a scoopable primary star.
    * Updated the `FSD engaged` script to report jumps remaining until you arrive at your selected destination. 
    * Updated the `Next jump` script to provide contextual information for the `FSD engaged` script.
    * Updated the `Jumped` script to use `jumpsremaininginroute` and to remove obsolete properties `destination` and `destdistance`.
    * Updated the `System report` script to enhance the description of powerplay status.
  * Speech Service
    * Added new function `InaraDetails` for looking up commander details on [Inara](https://inara.cz).
  * Status Monitor
    * Added new `hyperspace` and `srv_high_beams` properties.
    * Added new `slope` property (relative to the horizon and only if near a surface)
  * Translations
    * Italian translation is now complete.
  * VoiceAttack Responder
    * Added new plugin function `inara`, allowing commanders to look up the Inara profiles of other commanders in their browsers.
    * Added new `Status hyperspace` and `Status srv high beams` properties.
    * Added new `Status slope` property (relative to the horizon and only if near a surface)
    * Fixed 'cadmium' typo in EDDI.vap file

### 3.4.3
  * Core
    * Removed in-process .dll accidentally included with release 3.4.3-b1.

### 3.4.3-b1
  * Core
    * Reinstated the `Glide` event.
    * Fixed a bug preventing the `Material use report` script from executing correctly. 
  * Speech responder
    * Fixed display artefacts on the priority column when scrolling.
    * Touched up spacing irregularities in the `Material use report`.

### 3.4.2
  * Core
    * Fixed issues around navigation routing, including the `Jumped` script not updating correctly.
  * Material monitor
    * Added `Set` and `Clear` destination buttons.
  * Speech responder
    * Fixed a bug with speech queues that could in some circumstances cause EDDI to crash in the middle of combat.
    * Revised `Route details` script.

### 3.4.2-b1
  * Core
    * Application startup no longer waits for network operations to respond before displaying the UI.
    * Added vehicle (SLF & SRV) definition and loadout description localization
    * Fixed a crash that could occur when looking up information about specific factions. 
    * Fixed a bug that could cause EDDI to crash / not load with some Windows language settings. 
  * Speech responder
    * Increased EDDI's maximum volume level. Users who prefer prior volume levels should set volume to approximately 80% of their former setting.
    * Updated the `Docking granted` script to include basic landing pad info for surface ports.
    * Revised `Blueprint make report` and `Blueprint material report` scripts to reference localized and updated blueprint data.
    * Updated documentation for the `BlueprintDetails()` function and `blueprint` object, available via [Help](https://github.com/EDCD/EDDI/blob/master/SpeechResponder/Help.md) and [Variables](https://github.com/EDCD/EDDI/blob/master/SpeechResponder/Variables.md) in the SpeechResponder.
    * The `FSDJump` event now includes a `conflicts` property, containing a list of `conflict` objects.
    * Revised the `System state report` script to describe all factions in the system rather than just the controlling faction and to describe any conflicts between system factions. 
    * Updated the `Galnet news published` script to add an option (enabled by default) to read article titles rather than full article text.
    * Fixed a bug with the `Bodies mapped` script. 
  * VoiceAttack responder
    * Updated EDDI.vap commands for invoking the `Blueprint make report` and `Blueprint material report` scripts.

### 3.4.1
  * Amended a configuration error in the Frontier API module.

### 3.4.1-rc2
  * Cargo monitor
    * Fixed a bug that could disable the cargo monitor with translated versions of EDDI.
  * EDSM responder
    * Fixed a bug that caused EDSM synchronization to slow to a crawl, and optimized database access. Resynchronizing with EDSM will speed database access and is recommended for all pilots.
  * Speech responder
    * Fixed the character encoding in the French personality file.
    * Revised the `System report` script to gracefully handle systems with no controlling power.
    * The `Body mapped` event now makes available all of the same body data as `Body scanned`.
    * Added new event `Ring mapped` (the `Body mapped` event will no longer trigger when probing a ring).
  * Ship monitor
    * Reinstated ship export to [EDShipyard](https://www.edsy.org), as its developer has returned. 

### 3.4.1-rc1
  * EDSM responder
    * Improved EDSM synchronization for system visits and comments
  * Speech responder
    * Fixed an issue where star chromaticity wasn't being translated correctly.
    * Fixed an issue where certain scan scripts such as 'Star scanned' would sometimes give an error message.
    * Revised the `Star report` script to resolve a couple of minor bugs. 
    * Revised the `Body report summary script` script to resolve a couple of minor bugs.
    * Revised the `System scan complete` script to reduce verbosity and increase variety.

### 3.4.1-b2
  * Core
    * Reconfigured the installer to improve backwards compatibility between EDDI versions
    * Fixed a bug with deep-space system look-ups of EDSM data (where EDSM returns no results)
  * Speech responder
    * Revised the `Mission faction state` script to gracefully handle faction states like "None".
    * Fixed a bug with the `MaterialDetails()` function when only one parameter was supplied.

### 3.4.1-b1
  * Core
    * All 'Location' events are now processed (repeats of this event are no longer suppressed).
    * Added `Docked` and `Landed` Environment states. Note that the `Environment` variable follows the ship and `Vehicle` variable follows the commander.
    * Added `destinationsystem` (similar to `system`), `destinationstation` (similar to `station`), and `destinationdistance` variables 
    * Revised `body` object definition returned by the `BodyDetails()` function and revised `Body scanned` and `Star scanned` event values for better interchangeability of object properties with `Body scanned` and `Star scanned` events.
      * WAS: `name`*, IS: `bodyname` 
      * WAS: `type`*, IS: `bodytype`
        * Expanded `bodytype` values to separate `Planet` and `Moon` body types
      * WAS: `atmospherecomposition`, IS: `atmospherecompositions` (planets and moons only)
      * WAS: `axialtilt`*, IS: `tilt`
      * WAS: `bodyclass`*, IS: `planettype` (planets and moons only)
      * WAS: `distancefromarrival`*, IS: `distance`
      * WAS `orbitalinclination`*, IS `inclination`
      * WAS `rotationperiod`*, IS: `rotationalperiod`
      * WAS: `solidcomposition`, IS: `solidcompositions` (planets and moons only)
      * Added `stellarsubclass` (stars only)
      * Added `density`
      * Added `scanned`
      * Added `mapped`
      * Added `alreadydiscovered` (true if another commander has already submitted a scan of that body to Universal Cartographics)
      * Added `alreadymapped` (true if another commander has already submitted mapping details of that body to Universal Cartographics)
      * Added `estimatedvalue` (this was previously only available from the event variables)
      * Added `massprobability`
      * Added `radiusprobability`
      * Added `tempprobability`
      * Added `orbitalperiodprobability`
      * Added `semimajoraxisprobability`
      * Added `eccentricityprobability`
      * Added `inclinationprobability`
      * Added `periapsisprobability`
      * Added `rotationalperiodprobability`
      * Added `tiltprobability`
      * Added `densityprobability`
      * Added `ageprobability` (stars only)
      * Added `absolutemagnitudeprobability` (stars only)
      * Added `gravityprobability` (planets and moons only)
      * Added `pressureprobability` planets and moons only)
    * Revised `system` object definition
      * Added `isgreen`, true if bodies in this starsystem contain all elements required for FSD synthesis
      * Added `isgold`, true if bodies in this starsystem contain all elements available from surface prospecting
      * Added `estimatedvalue`, the estimated exploration value of the starsystem (includes bonuses for fully scanning and mapping)
    * Updated estimated scanning and mapping value calculations.
  * Crime Monitor
    * New monitor tracks all bond & bounty awards and fines & bounties incurred.
    * Monitor attempts to determine the minor faction's 'home system' via its name, but defaults to system presence with highest influence.
    * Minor faction's 'home system' may be manually entered and is archived for future use.
    * 'Add Record' button allows manual addition of claims, fines & bounties.
    * 'Find Legal Facilities' button allows standalone users to locate the nearest 'Legal Facilities' contact. 
    * New `FactionRecord` and `FactionReport` properties, available via Cottle scripting. See the `Variables` window for details.
    * Tracks all ships you have targeted within the current system. Data available in `shiptargets` as a list of `Target` properties.
  * Galnet monitor
    
    * Fixed a bug causing the Galnet monitor to occasionally reset the read status of articles.
  * Material Monitor
    
    * Added 'Find' buttons for the nearest `encoded`, `manufactured` & `raw`  Materials Traders and `guardian` & `human` Technology Brokers.
  * Mission Monitor
    
    * Added 'Find Route', 'Next Route', 'Update Route', and 'Clear Route' buttons to give standalone users access to missions routing functionality.
  * Navigation Service
    * Consolidated all `RouteDetails()` functionality.
    * Added `facilitator` route type to `RouteDetails()`, which finds and sets the `Destination` properties to the nearest 'Legal Facilities' contact.
    * Added `encoded`, `manufactured` & `raw` to `RouteDetails()`, which finds and sets the `Destination` properties to the nearest Materials Trader.
    * Added `guardian` & `human` to `RouteDetails()`, which finds and sets the `Destination` properties to the nearest Technology Broker.
    * Destination system, distance & station data populated & maintained by `RouteDetails()`. Distance re-calculated after each jump.
    * The `missionsRouteList` & `missionsRouteDistance` properties simplified to `RouteList` & `RouteDistance`, respectively.
  * Ship Monitor
    * 3.4 Update `Loadout`journal event additions `unladenmass` and `maxjumprange` included in the `Ship` object.
    * FSD `optimalmass` retrieved from engineering data and used to calculate `maxfuelperjump` property.
  * Speech responder
    * The `Humanise()` function now supports negative numbers.
    * The `MaterialDetails()` function has been revised to optionally recommend the body with highest concentration of a material, given a material name and star system name.
    * The `P()` function now converts roman numerals in planet classes (e.g. Class II gas giant) into numbers (e.g. Class 2 gas giant) to ensure proper pronunciation.
    * The `P()` function has been revised to correct mispronunciations of body names ending in "a" or "g"
    * The `Spacialise()` function no longer adds an extra space at the end of the string.
    * Added `JumpDetails()` Cottle function call to provide useful jump infomation based on ship loadout and fuel level. See `Help` & `Variables` windows for details.
    * Added `TrafficDetails()` Cottle function call to provide information on traffic, deaths, and hostilities in a star system. See `Help` & `Variables` windows for details.
    * Added `unladenmass` and `maxjumprange` properties to the `Loadout` event handler.
    * Added `distancefromstar` property to the `Location` event.
    * Added vehicle ID for SLF/SRV related events.
    * Added `Crime check system` script to report wanted status and 'legal facilities', upon entering the system.
    * Added `Crime check station` script to report 'legal facilities', upon entering normal space, next to station.
    * Added `Bodies mapped` script to allow reporting which bodies in the system have already been mapped.
    * Added `Bodies to map` script to allow reporting recommendations of bodies to map (configurable in the script).
    * Added `Body report summary` script to allow reporting of summary body data, taking into account statistically unusual bodies.
    * Revised `Entered normal space`, `Glide`, `Location`, and `Near surface` event variables for better interchangeability with the `BodyDetails` function.
      * WAS: `body`*, IS: `bodyname` 
      * WAS: `system`*, IS: `systemname`
    * Revised `Approached settlement` event to include the `bodyname` of the settlement. 
    * `Star scanned` script revised. Preference added for reporting stellar class. Corrected edit scars. Refactored to reduce redundancies. 
    * `Body volcanism script` revised. Corrected edit scars and added a little more variety to the script.
    * Revised `Exploration data sold` event and revised script.
      * Added variable `total`, describing the total credits received (after any wages paid to crew and including for example the 200% bonus if rank 5 with Li Yong Rui)
      * Removed variable `firsts` (it is no longer supported by post 3.3 batch selling of exploration data).
    * Revised `Discovery scan` script to report the number of bodies remaining to be scanned while your ship's role is either `exploration` or `multipurpose`.
    * Revised `Star report` script to incorporate new variables documented above.
    * Revised `System scan complete` script to recommend bodies for mapping (using the new `Bodies to map` script) and to identify `green` and `gold` system discoveries while your ship's role is either `exploration` or `multipurpose`
    * Revised `Body atmosphere report` for better handling of Earth-like worlds.
    * Revised `Body mapped` script. By default, the full `Body report` script is now given after this event completes rather than after `Body scanned`. Optionally recommends other bodies in the system for mapping.
    * Revised `Body materials report` script to optionally report material percent concentrations.
    * Revised `Body report` script to correct some errors identified by users (terraformable bodies will now be reported as such).
    * Revised `Body scanned` script to include option to use `Body report summary` script.
    * Revised `Body volcanism report` to touch it up.
    * Revised `Signal detected` script to allow users to better customize signal detection (particularly for rare signal types).
    * Variables from the following scripts have been revised to add new variables and improve consistency between events.
      * `Glide` event (body => bodyname, system => systemname)
      * `Location` event (body => bodyname, system => systemname)
      * `Near surface` event (body => bodyname, system => systemname)
    * Revised `Jumped` script to provide a (reasonably) accurate jump range, based on total ship mass.
    * Revised `Route details` script to handle new `encoded`, guardian`, `human`, `manufactured` and `raw` route queries.
    * Revised `Ship targeted` script to utilize new `shiptargets` object to preclude reporting on previously scanned ships.
    * `Signal detected` events are no longer suppressed outside of fss mode.
  * Status monitor
    - Added `legalstatus`, the ship's current legal status. Can be one of 
      - "Clean", 
      - "Illegal cargo", 
      - "Speeding", 
      - "Wanted", 
      - "Hostile", 
      - "Passenger wanted", or 
      - "Warrant"
    - Added `bodyname`, the name of the current body (if landed or in an srv)
    - Added `planetradius`, the radius of the current body (if landed or in an srv)
    - Added `altitude_from_average_radius`, true if the altitude is computed relative to the average radius (which is used at higher altitudes) rather than surface directly below the srv
  * Voice Attack
    * Added `Destination system`, `Destination system distance`, and `Destination station` properties.
    * Added `{TXT:Status legal status}`
    * Added `{TXT:Status body name}`
    * Added `{DEC:Status planet radius}`
    * Added `{BOOL:Status altitude from average radius}`
    * Added `jumpdetails` plugin invocation to provide useful jump infomation based on ship loadout and fuel level.

    \* For noted properties, old property names are preserved for legacy script compatibility

### 3.4
  * Core
    * Added localised names for the Advanced Docking Computer and Supercruise Assist modules.
  * Galnet monitor
    * Restored multi-lingual access.
    * No longer loses its place if a web request times out or fails.
  * Speech responder
    * Added event `System scan complete`, triggered when all bodies in the star system have been discovered.
    * Added new function `GetFaction()` to obtain details about a faction.
    * Revised faction object to allow reporting faction data spanning multiple star systems.
  * Voice Attack
    * Fixed a bug that would incorrectly disable invoked speech while `disablespeechresponder` was set.

### 3.3.7
  * Speech responder
    * Preliminary Portuguese version of the default personality script.

### 3.3.7-rc1
  * Speech responder
    * Fixed some minor errors re plurals etc in the mission reports and fuel check scripts.

### 3.3.7-b2
  * Commander details
    * Added auto-complete for the home system and squadron system and catered for two-character system names such as 'Ix'.
  * GalNet monitor
    * Restored access using the new RSS feed, but it's English-only for now unfortunately.
  * Journal monitor
    * Fixed a bug that would prevent new file header events from registering when Elite Dangerous was restarted with EDDI running.
  * Ship monitor
    * Dropped access for EDShipyard.com as it is no longer being maintained.
  * Speech responder
    * Fixed a bug that would cause Test scripts to only be written to file and not voiced.
    * Amended layout of the 'Delete' button.
  * VoiceAttack responder
    * Reduced CPU utilization significantly in some cases, particularly when interacting with the ship monitor and when loading the last journal file.

### 3.3.7-b1
  * Core
    * Fixed a bug whereby names of materials (Carbon, Iron, Conductive Components, etc) were not always localized.
  * Cargo & Mission monitors
    * Improved efficiency in 'LogLoad' handling of mission-related events.
    * Minimize cargo & mission monitor.json writes.
  * Localization
    * Added Brazilian Portuguese localization.
    * Increased coverage of the Russian localization.
    * Updated the Spanish default speech personality.
  * Ship monitor
    * Fixed a lock-up that could occur when opening the ship monitor from VoiceAttack.
  * Speech responder
    * Implemented speech priorities as follows:
      * 1 - high priority, interrupts other speech.
      * 2 - high priority.
      * 3 - standard priority.
      * 4 - low priority.
      * 5 - low priority, interruptible by any higher priority speech.
    * Added new types to the `RouteDetails()` Cottle function:
      * 'cancel' - Cancel the currently stored route.
      * 'next' - Get the next destination in the currently stored route.
      * 'set' - Set the destination route to a single system.
    * All `RouteDetails()` types now update the `Missions route` event `route` and `distance` variables.
    * `RouteDetails()` 'update' function re-calculates the route when other than the 'next' system is removed.
  * VoiceAttack responder
    * Improved event responsiveness.
    * Reduced CPU utilization.
    * Restored missing home system variables.
    * Revised VoiceAttack integration documents with updated guidance on accessing home and squadron variables.
    * Documented methods for using speech priorities with `say` and `speech` plugin functions. 

### 3.3.6
  * Frontier API
    * Fixed missing client ID in 3.3.5

### 3.3.5
  * Core
    * Miscellaneous minor bug fixes.
    * Added Russian translations.
    * `MarketInformationUpdated` event no longer triggers when cAPI is down. As a result, this event will not consistently trigger following a `Docked` event. 
  * Mission monitor
    * Fixed a 'deadlock' vulnerability in the derived `Mission expired` event creator which would freeze EDDI.
  * Speech responder
    * Fixed "Engineer progressed" script to account for stages other than getting a new blueprint level.
    * Fixed punctuation in the "Fuel check" script.
  * VoiceAttack
    * Improved variable setting efficiency by adding granularity via 'Update' event handling.
    * Added information command to EDDI.vap to test status variables.

### 3.3.4
  * Speech responder
    * Fixed a bug that would cause star scans to accumulate until the FSS scanner was opened.
    * Fixed a bug that could prevent proper lookup of bodies using the `BodyDetails()` function.
    * Added new `Next jump` event
    * Added new top level object `nextsystem` - like `lastsystem` but for the next system you are visiting. e.g. "Our next waypoint is \{nextsystem.name\}".
  * VoiceAttack  
    * Added new `Next system` variables
    
### 3.3.4-rc3
  * Frontier API
    * Fixed missing client ID in 3.3.4-rc2.

### 3.3.4-rc2
  * Core
    * Added `SquadronStartup` event handler.
    * `Docked`, `FSDJump`, and `Location` events now pass Faction objects. Localized state, government & allegiance properties are still derived for use in Cottle & VA.
  * Localization
    * Fixed crashing bug in the Italian localization.
    * Added Russian localization (no translations yet).

### 3.3.4-rc1
  * Bug fixes for database transactions
  * Bug fixes for changes to journal events in Elite Dangerous patch 3.3.03
  * Speech responder
    * Easier control for the `Message received` script:
      * Up-front settings for `spokenChannels` and `spokenNpcSources`: each item can be set `true` (spoken) or `false` (not spoken).
      * the npc sources "Cruise liner", "Passenger liner", "Station" and "Wedding convoy" default to false -- and there was great rejoicing.
      * the "starsystem" channel defaults to false to prevent overwhelming EDDI with messages in systems with high player counts.
    * Revised the `System report` script so that it won't report a "None" government type.

### 3.3.4-b1
  * Core
    * Added manual market, outfitting and shipyard updates, upon docking. Associated EDDN message is sent.
    * EDDI will now sync to the most current journal on startup (dramatically improving the accuracy of data available after startup)
  * Event responder
    * `Empire promotion` Added the integer rank rating associated with the current promotion.
    * `Federation promotion` Added the integer rank rating associated with the current promotion.
  * Ship monitor
    * Added new values to the current ship: `hullhealth` & `hot`
    * Added `Hot` property to the Ship monitor UI
    * Added new values to modules: `hot`, `modification`, `engineerlevel`, and `engineerquality`
    * Updated module-related events to handle engineering modification data
    * Added `Stored ships` event handling, triggered when the `Shipyard` screen is selected in-game.
    * Added `Stored modules` event handling, triggered when the `Outfitting` screen is selected in-game. New `storedmodule` object properties are available in Variables.md
  * Speech responder
    * Revised `Empire promotion` script to include rank specific rewards.
    * Revised `Federation promotion` script to include rank specific rewards.
    * Revised `Mission completed` script to remove rank rewards.
  * VoiceAttack plugin
    * EDDI will no longer lose track of position data when you pass through a system where your squadron minor faction has influence.

### 3.3.3
  * Emergency hotfix for a breaking change in the way fuel levels are reported.


### 3.3.2
  * EDDN responder
    * Made revisions to guard against reporting inaccurate star position data to EDDN.
    * The EDDN responder will now provide an audible warning if it detects that location data is in an invalid state.
  * Mission monitor
    * Updated the mission monitor to fix a parsing error for donation mission entries created by a change in how these are recorded. 

### 3.3.1
  * EDDN responder
    * `CodexEntry` events are bugged and always return a SystemAddress of 1. These must be ignored by the EDDN responder to prevent sending bad data.

### 3.3
  * Speech responder
    * Fixed a bug in the way that the status of friends is tracked, and fixed the broken `Test` button for the `Friends status` event.

### 3.3-rc2
  * Core
    * All drop-down menus are now sorted appropriately.
    * Internal fixes and efficiency improvements.
  * EDDN responder
    * Corrected an issue that was preventing certain event types from being sent to EDDN.

### 3.3-rc1
  * Core
    * Fixed a bug that could result in rotational and orbital periods being reported as much faster than they really were.
    * Fixed very low hab zone values in `Star scanned` events
    * The `body` object now includes a new `shortname` property (removing the system name if it is part of the body name)
    * The `body` object now includes `estimatedhabzoneinner` and `estimatedhabzoneouter` for stars.
    * The `Body scanned` event has been revised to include new data. 
    * Added `faction` object. Check the `Variables` window for details.
    * Added squadron data to Commander details UI & the `cmdr` object. Details in Variables.md
    * Squadron data updated in `Location` and `FSDJump` events, when in squadron home system.
  * Frontier API
    * This has been disabled for the time being, due to changes that Frontier made to the authentication protocol without any transition period.
      * Unfortunately this means that EDDI is temporarily blind to market, outfitting and shipyard data. We will rectify this in EDDI 3.3.1 by reading the local journal files, but that will require visiting the screens in question in-game.
  * Ship monitor
    
    * Added new value to the current ship: `ident`
  * Speech responder
    * Fixed system messages triggering the `Message received` event.
    * Added new channels to the `Message received` event: "squadron" and "starsystem"
    * Added new event `Body mapped`, triggered after mapping a body with the Surface Area Analysis scanner
    * Added new event `Discovery scan`, triggered when performing a full system scan (honk) with the FSS
    * Added new event `Signal detected`, triggered when a signal source is detected
    * Added new event `Squadron status`, triggered status of squadron has changed (`applied`, `created`, `disbanded`, `invited`, `joined`, `kicked`, `left`).
    * Added new event `Squadron rank`, triggered when squadron rank has changed.
    * Updated `Jumped` event to contains the name of the star at which you've arrived
    * Updated `Jumped` and `Location` events to contain details about the status of factions.
    * Updated `Entered signal source` event by adding new variable `localizedsource`
    * Revised `Body report` event script. (1) (2)
    * Revised `Body scanned` script.
    * Revised `Star scanned` event script
    * Revised `Star report` script (1) (2)
    * Added `Body atmosphere report` script. (2)
    * Added `Body materials report` script. (2)
    * Added `Body volcanism report` script. 
    * Added `Star habitable zone` script.

    (1) In the interest of brevity, the default `Body report` and `Star report` scripts now will only be used if your ship's role is set to either `Multipurpose` or `Exploration`. 
    (2) Script contains a `Preferences` section for setting user preferences about the details reported.

  * Status monitor 
    * The status object has new values for: 
      * HUD analysis mode
      * Night Vision mode
      * New GUI modes
        * Orrery view
        * FSS mode 
        * SAA mode
        * Codex
      * Fuel (for ship or SRV)
      * Fuel percentage
      * Fuel time remaining (in seconds)
      * Cargo carried (in tons)

### 3.3-b1
  * Core
    * Fixed a bug that could reset system comments, visits, and the date last visited while refreshing star system data. 
  * Material monitor
    * Revised material rarities and default material maxima (material maxima are recalulated to the following values if not set): 
      * (Thargoid) Propulsion Elements (Very Rare - 100 MAX)
      * (Thargoid) Weapon Parts (Rare - 150 MAX)
      * Pattern Alpha Obelisk Data (Rare - 150 MAX)
      * Pattern Beta Obelisk Data (Rare - 150 MAX)
      * Pattern Gamma Obelisk Data (Rare - 150 MAX)
      * Polonium (Rare - 150 MAX)
      * Antimony (Rare - 150 MAX)
  * Speech responder
    * Added new event `EngineerContributed`, triggered when contributing resources to an engineer in exchange for access.
    * Added new function `EngineerDetails()` for accessing information about relations with engineers.
    * The `cmdr` object now includes `engineers`.
    * Updated documentation to describe `Engineer` object and `EngineerDetails` function.
    * `Docked event`: Added `wanted`, `activefine`, and `cockpitbreach`.
    * `Engineer progressed` event: Added `stage`, `rankprogress`, and `progresstype`.
    * `Modification applied` event: Removed. This event was redundant with `Modification crafted` and is no longer written by the game as of Elite Dangerous version 3.0.
    * `Modification crafted` event: Added `module`, `quality`, and `experimentaleffect`.
    * `Settlement approached`: Added `latitude` and `longitude`.
    * The `Voice()` function now tolerates incomplete voice names (EDDI will return the first matching voice) and casing no longer matters.
    * Added new function `VoiceDetails()` for accessing details of installed voices while scripting.

### 3.1.2
  * Localization
    * Material locations have been moved from the update server to the app and are now a translatable resource.
  * Speech responder
    * Tweaked pronunciations of "Megaship" and "Orbis" in English. Tweaked pronuncations are available via the `P()` function.

### 3.1.1
  * Core
    * Fixed crash to desktop when the folder `%APPDATA%\EDDI` does not exist.
  * Localization
    * Imported latest translation files.
  * Speech responder
    * Tweaked pronunciations of "Megaship" and "Orbis" in English.
    * Amended "Entered normal space" script for cases when dropping near a non-station settlement.

### 3.1
  * Core
    * Fixed scan message spam upon scanning a Nav Beacon.
  * Localization
    * Added translation files for Mission Monitor.
  * VoiceAttack responder
    * Fixed a bug that prevented the `shutup` command from firing correctly.

### 3.1-rc1
  * Localization
    * Added resource files for Japanese (no translations yet).

### 3.1.0-b5
  * Core
    * EDDI now uses EDSM as the primary backend for server data (rather than the no longer maintained server set up by EDDI's original creator)
    * Strings returned from the player journal and from server data have been standardized and should match each other much more closely than they have in the past.
    * Variables.md has been updated with new fields available for our core data objects: Body, Station, and System
      * Body: Atmospheric composition data, made available from `atmospherecompositions`
      * Body: Solid body compositon data, made available from `solidcompositions`
      * StarSystem: Starsystem permit data, made available from `requirespermit` and `permitname` (for the SystemDetails() method only - this info is not published to the player journal)
      * The semi-major axis of the planetary orbit is now given in light seconds, rather than in meters
  * Localization: new translatable strings pertaining to body, station, and system data have been added
  * Speech responder
    * `Docked event`: Added `secondeconomy`.
  * VoiceAttack responder
    * Amended reporting of cargo & limpets carried.

### 3.1.0-b4
  * Speech responder
    * Fixed an error converting a string, such as a ship ID, to the ICAO alphabet that was empty or all symbols (an empty string should result).
  * Ship monitor
    * Fixed a bug that would cause fighters to be written to the ship monitor
  * Status monitor
    * Recalibrated the `ShipFSD` event 
      * `charging complete` now triggers at a more appropriate time.
      * Added new FSD status - `charging cancelled`.
  * VoiceAttack responder
    * Added command to open ships in EDShipyard
    * When commands for EDDB, EDShipyard, or Coriolis are invoked, the applicable uri will be written to `{TXT:EDDI uri}`.
    * Updated EDDI.vap to set the optional boolean `{BOOL:EDDI open uri in browser}` in applicable commands for EDDB, EDShipyard, and Coriolis. If set to false, EDDI shall not open the browser.
    
### 3.1.0-b3
  * Core
    * Fixed a bug in our JSON deserialization code that led to variables changing which were expected to remain constant. This manifested in various ways, including:
      * Ship roles changing in unexpected ways
      * The ship role drop-down menu becoming scrambled
      * System state reporting becoming scrambled or fixated on a particular state
  * Cargo monitor
    * Fixed double report of 'Cargo updated'.
  * Mission monitor
    * Fixed Mission Warning event reporting wrong expiry #875
    * Fixed Entered Normal Space event incorrectly reporting I'm near my last station #876
    * Moved body gravity reporting to 'Glide' event script.
  * Speech responder
    * Fixed Mission warning 'minutes' remaining and 'Entered normal space' script for planetary ports.

### 3.1.0-b2
  * Core
    * Implemented log rotation. EDDI will now create a new log file every time it starts, and shall retain no more than 10 log files before it starts clearing the old logs. Immense log files are a thing of the past.  
    * Added data definitions for Guardian fighters.
  * EDDP monitor
    * Fixed a bug that prevented selection of any state other than `(anything)` and blocked filter input when any state other than `(anything)` was selected.
  * Galnet Monitor
    * Updated category assignments to match updated article format from Frontier Developments
  * Localization
    * Fixed a bug preventing localized module names from being displayed correctly within the ship object. 
    * The filters used to assign categories to Galnet articles in the Galnet monitor are now a translatable resource.
    * The category names for categories assigned to Galnet articles in the Galnet monitor are now a translatable resource.
    * The source url used by the Galnet monitor for each language is now a translatable resource. For languages that do not have official Galnet feeds, translators may create unofficial feeds that follow the same format as the official Galnet feeds for use with EDDI.
  * Ship Monitor
    * Added 'Module info' event
  * Speech responder
    * Revised `Galnet news published` script to identify interesting articles by category.
    * Added new event: `Glide`, triggered when your ship enters or exits glide near a planet's surface
    * Revised script: `Entered normal space` to omit a pause if it is immediately following a glide event and to check whether gravity is set prior to reporting gravity.
    * Revised script: `Star scanned` to allow us to fix pronunciations of stellar class (some voices rendered `TTS` class stars as `text-to-speech` class stars).
  * UI
    * Enhanced IPA help with additional links.
    * Corrected window size to prevent UI elements from being hidden / obscured at the default window size. 
    * Minor visual enhancements.  
  * Voice Attack plugin / Speech synthesizer
    * Improved stability by switching to using a single speech synthesizer instance (kudos to VoiceAttack's developer for working with us on this). 

### 3.1.0-b1
  * UI
    * Tabs are now shown on the the left, and all but the first "EDDI" tab are sorted alphabetically according to the rules of the current locale.
  * New Mission monitor feature, tracking all mission parameters. Check the Speech responder `Variables` window for details.
    * New `Mission expired` event, triggers when a mission has expired.
    * New `Mission warning' event, triggers when a mission expiration is within the 'warning' threshold, set in the `Mission monitor` tab.
  * Speech responder
    * New `MissionDetails()` function for Cottle scripting to access mission data.
    * New `RouteDetails()` function for Cottle scripting to query for various mission routes.
      * `expiring` - The destination of your next expiring mission.
      * `farthest` - The mission destination farthest from your current location.
      * `most`     - The system with the most missions.
      * `nearest`  - The mission destination nearest to your current location.
      * `route`    - The "Traveling Salesman" (RNNA) route for all active missions.
      * `update`   - The next mission destination, once all missions in current system are completed.
    * New `Missions route` event to provide pertinent data for mission route queries.
    * New `Missions route` script to report the results of the `Missions route` event.
    * New `Mission check galaxy` script, which reports all active missions.
    * New `Mission check system` script, which reports active and completed missions in your current system.
    * New `Mission check station`, which reports active & completed missions, if docked or station in vicinity.
    * Revised `Commander continued` script to include the `Mission check galaxy` script.
    * Revised `Location` and `Jumped` scripts to include the `Mission check system` script.
    * Revised `Location` & `Entered normal space` scripts to include the `Mission check station` script.
  * Voice Attack plugin
    * New `missionsroute` external function to query for various mission routes from VA. See RouteDetails() above for query types.

### 3.0.3
  * Core
    * Added data definitions for Beyond Chapter 3 (game v3.2).
  * Cargo monitor
    * Added support for solo delivery missions using the mission depot.

### 3.0.2
  * Additional Italian translations.
  * Fixed an exception that occurs during location events if not docked.

### 3.0.2-b1
  * Core
    * Fixed a bug that was preventing some events from being passed to EDSM.
    * Added more Italian localization.
  * Cargo monitor
    * Fixed tracking of black market cargo.
    * Fixed tracking of cargo sold to the mission depot.
    * Added CargoDetails() and HaulageDetails() functions for Cottle scripting.
  * EDSM responder
    * Fixed a crash when parsing an invalid EDSM configuration.
    * Now sends SystemAddress and MarketId information.
  * EDDN responder
    * Removed the `Wanted` tag from the data we send to EDDN, since all data submissions should be anonymized.
    * Now sends SystemAddress and MarketId information.
    * Alpha and beta builds will now use EDDN's test endpoints.
  * Ship monitor
    * EDDI will now open shipyard links to coriolis.io rather than edcd.coriolis.io (at Coriolis dev's request). 
      * You will need to **MIGRATE** your ships!
      * The Coriolis dev team has prepared [a handy video to show you how](https://youtu.be/4SvnLcefhtI).
    * Fixed a bug that was preventing full module details from being saved to our config files correctly. 
  * Speech responder
    * Revised the `Ship targeted` script so that it won't repeat whenever switching targeted subsystems.
    * Added `Cargo scoop` event
    * Added `Landing gear` event
    * Added `Lights` event
    * Added `Bounty paid` event
    * Updated the properties available from the `Fines paid` event. (Note that legacy fines were discontinued with Elite Dangerous version 3.0, the `legacy` boolean has been removed from this event.)
  * Status monitor
    * `Status.hardpoints_deployed` is now locked to false while we are in supercruise.
  * VoiceAttack responder
    * EDDI.vap (for VoiceAttack users)
      * Fixed a typo in the description of the `((EDDI: station variables))` command
      * Fixed a missing `;` in the command `Please repeat that;What was that?;Could you say that again?;Say that again`

### 3.0.1
  * Released!

### 3.0.1-rc6
  * EDDN responder
    * Fixed symbol for Krait Mk II in shipyard data.
    * Release builds of EDDI will now use the live EDDN endpoints.

### 3.0.1-rc5
  * Core
    * Fixed some sitations on which hull health was incorrectly being set to 100%. Unfortuntely not all sources of hull damage are currently reported in real-time in the journal, so EDDI may sometimes remain unaware of new damage for a while.
    * Fixed a bug that could occur when hull and module values aren't present in the `Loadout` event.
  * EDDN responder
    * Fixed an issue whereby average prices where not being sent.
    * Fixed an issue with the parsing of stock and demand bracket JSON which can be either `int` or `string`.
    * Added test coverage for the above.
  * VoiceAttack responder
    * Found and fixed the remaining cause of excessive CPU load. It should now be back to 3.0.0 levels.

### 3.0.1-rc4
  * Core
    * Removed `insurance excess` from the user interface and the `insurance` property from the top level `commander` variable. FDev now gives us rebuy values directly, so this is no longer needed.
  * Ship monitor
    * Fix ship value and model (for unnamed ships) not being updated correctly in the Ship Monitor.
  * Speech responder
    * Updated the `Loadout` event to include new properties.
      * "hullvalue" The value of the ship's hull (less modules)
      * "modulesvalue" The value of the ship's modules (less hull)
      * "value" The total value of the ship (hull + modules)
      * "rebuy" The rebuy value of the ship
    * Revised `Insurance check` script to take advantage of the new ship "rebuy" property.
    * Added variety to the `Ship targeted` script and made it less verbose, as it fires a lot in the heat of combat.
  * EDDN responder
    * Fixed an issue whereby incomplete commodity data could be sent to EDDN.
  * VoiceAttack responder
    * Dramatically reduced CPU load. 

### 3.0.1-rc3
  * Core
    * Added data definitions for the new ships and modules in Chapter 2.

### 3.0.1-rc2
  * Core
    * Revised EDDI's logging code and removed an unintended recursion that could cause the log to bloat.

### 3.0.1-rc1
  * Core
    * The Search and rescue event was having its `commodity` property set to just the commodity name, rather than the commodity definition object that scripts expect. Fixed.
    * Updated the `Ship refueled` event to include new boolean value `full`. True if this is a full refill and false if this is a partial refill.
    * Updated system definition to include new variable `lastVisitSeconds`. 
  * Speech responder
    * Added the following Cottle function, documented in [the SpeechResponder documentation](https://github.com/EDCD/EDDI/blob/master/SpeechResponder/Help.md):
       * `CommodityMarketDetails()` for retrieving market information about commodities.
    * Updated the following events to include new properties `stationtype` and `stationDefinition`:
      * `Docking cancelled`
      * `Docking denied`
      * `Docking requested`
      * `Docking granted`
      * `Docking timed out`
    * Updated `Commodity sales check` script to make use of `CommodityMarketDetails()` function.
    * Updated `Docking granted` script to make use of of `stationtype` property.
    * Fixed a bug that had made `{ship.role}` inaccessible via scripts.
    * Updated `FSD engaged` script to correct a bug that was preventing sub-function `System report` from ever running.
    * Updated `Ship refueled` script to correct a bug that would cause it to sometimes report more than 100% fuel after refueling.
    * Revised punctuation in the various module scripts to speak more naturally (mostly removing commas as in `You have sold a 4-D, Power Plant`).
  * Cargo monitor
    * Discard all cargo if your ship is destroyed.
  * Localization
    * Fixed the English pronunciation of "Biowaste" by localizing it to "bio-waste". And there was much rejoicing.
  * VoiceAttack
    * The Search and rescue event has a new variable `{TXT:localizedcommodityname}` because the `commodity` varaible is now an object.

### 3.0.1-b4
  * Core
    * Fixed issues arising in betas 2 and 3: data written to file by old code was not being read correctly by new code. This manifested in various ways: too many to list.
    * As a by-product, we now have rather more test coverage.
  * Speech Responder 
    * Added `Ship targeted` event.
    * Fixed `Docked` event not firing.
    * Fixed planet mass and radius not being reported.
    * Fixed inconsistencises in ship name pronunciation in the default personality scripts.
  * Localization
    * Updated the localizations for French, Spanish, German, Hungarian and Italian. 
      * You can join up at https://crowdin.com/project/eddi to help.

### 3.0.1-b3
  * Core 
    * Fixed a bug that was preventing chromaticity and various stellar probabilities from being set in some circumstances.
    * EDDI will now track the nearest stellar body and make that data available to EDDI's Speech Responder via the `body` variable (planet, moon, etc.).  
  * EDSM Responder
    * Fixed a bug that was resetting system visit totals during syncs with EDSM. Please re-obtain logs from EDSM to update the information in your local database.
  * Localization
    * Added partial localizations for French, Spanish, German, Hungarian and Italian. 
      * **Please be aware that all these are incomplete work in progress.**
      * You can join up at https://crowdin.com/project/eddi to help.
  * Speech Responder 
    * Added `Silent running` event.
    * Updated the 'Near surface' event to include the name of the body that you are approaching or exiting. 
    * Add a new top level `body` object, which contains details of the nearest stellar body. Any values might be missing, depending on EDDI's configuration and the information available about the body. 
  * Text-to-Speech
    * The `Tranmit()` function no longer insists on being the first speech rendered by your script. 
    * The `Play()` function no longer insists on being the only element rendered by your script.
    * Add new audio function `Voice`, which allows you to render speech within a script using any installed voice which EDDI recognizes (not just the one selected in the `Text-to-Speech` tab). 
  * VoiceAttack 
    * Added the following new variables describing details of the nearest stellar body, with values as described by the 'Body' object 
      * `{DEC:Body EDDB id}`
      * `{TXT:Body type}`
      * `{TXT:Body name}` 
      * `{TXT:Body system name}`
      * `{DEC:Body age}`
      * `{DEC:Body distance}`
      * `{BOOL:Body landable}` 
      * `{BOOL:Body tidally locked}` 
      * `{DEC:Body temperature}` 
      * `{BOOL:Body main star}` 
      * `{TXT:Body stellar class}` 
      * `{TXT:Body luminosity class}` 
      * `{DEC:Body solar mass}` 
      * `{DEC:Body solar radius}` 
      * `{TXT:Body chromaticity}` 
      * `{DEC:Body radius probability}` 
      * `{DEC:Body mass probability}` 
      * `{DEC:Body temp probability}` 
      * `{DEC:Body age probability}` 
      * `{DEC:Body periapsis}` 
      * `{TXT:Body atmosphere}` 
      * `{DEC:Body tilt}` 
      * `{DEC:Body earth mass}` 
      * `{DEC:Body gravity}` 
      * `{DEC:Body eccentricity}` 
      * `{DEC:Body inclination}` 
      * `{DEC:Body orbital period}` 
      * `{DEC:Body radius}` 
      * `{DEC:Body rotational period}` 
      * `{DEC:Body semi major axis}` 
      * `{DEC:Body pressure}` 
      * `{TXT:Body terraform state}` 
      * `{TXT:Body planet type}` 
      * `{TXT:Body reserves}` 

### 3.0.1-b2
  * Localization
    * EDDI is now localized! You can choose your language in the EDDI tab or just go with the system default.
    * Added French and Spanish localizations, complete at the time of writing, except for the personality scripts.
    * Added Italian localization, 4% work in progress at the time of writing.
    * The default personality file `EDDI.json` has been generalized into `EDDI.fr.json` etc. However it is clear that this will not scale with the number of supported languages and we will be looking at re-working this in future.
  * Text-to-Speech 
    * Add new audio function `Transmit`, which adds a radio effect to speech output. Details on this new function are described in [the SpeechResponder documentation](https://github.com/EDCD/EDDI/blob/develop/SpeechResponder/Help.md#transmit).
  * Ship monitor
    * Made sure we are using human readable ship names in all scripts (e.g. "Imperial Eagle" rather than "Empire_Eagle").

### 3.0.1-b1
  * Completely re-written Cargo Monitor. Cargo and limpets should now be tracked accurately.
  * Exports to Coriolis and EDShipyard now send data in `Loadout` journal event format, rather than in the old Frontier API format.

### 3.0.0
  * Core
    * Fixed the reporting of the Location event to EDDB. Was using the current body when it should have used the current station.
    * Tuned the new logging to Rollbar for quicker startup in some situaitons.
    * Added missing definitions for:
      * Lavigny Garrison Supplies.
      * Shock Cannon
      * FdL Cargo Hatch
      * Type-10 cockpit
      * String of white cockpit lights
      * Figher cockpit weapons and armor
      * Meta Alloy Hull Reinforcement

### 3.0.0-rc2
  * Core
    * The EDSM responder has been updated to send data to EDSM per their revised API. 
    * Switched error reporting to [Rollbar](https://rollbar.com/).
  * Incorporated new material transaction events
  * EDDI's Material Monitor will now auto-calculate maximum material limits when they are not otherwise defined, provided the material rarity is known.
  * Speech Responder
    * Added `Fighter rebuilt` event
    * Added `Material trade` event
    * Added `Technology broker` event

### 3.0.0-rc1
  * Core
    * Incorporated new data definitions for 3.0.
  * Installer
    * First installations will now take any custom VoiceAttack installation location into account when proposing a location for EDDI.
    * Upgrade installations will continue to use whatever location was selected in the first installation.
  * Speech Responder
    * Added `Jet cone damage` event
    * Script changes
      * Added new script `Jet cone damage`
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


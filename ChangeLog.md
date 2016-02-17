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

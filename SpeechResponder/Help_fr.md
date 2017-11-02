# Scripter avec EDDI

Le processeur vocal de EDDI utilise Cottle pour les scripts. Cottle a un grand nombre de possibilités dont:

    * Capacité de définir et de mettre à jour des variables, y compris des tableaux
    * Gestion de boucles
    * tests conditionnels
    * Liaisons entre scripts

Vous pouves trouver les information sur l'utilisation et l'écriture de scripts en Cottle à : http://r3c.github.io/cottle/#toc-2, et EDDI utilise beaucoup des fonctionnalités existantes.

## Variables

Cottle ne transfère pas les variables locales entre les scripts, mais dans un même scripts vous pouvez utiliser ces variables pour vous simplifier la vie.

Notez que ces variables ne sont pas persistantes et que leur valeur sera effacée à chaque redémarrage de EDDI. En outre, comme les scripts EDDI fonctionnent de manière asynchrone et simultanée, il n'y a aucune garantie que, par exemple, le répondeur vocal d'un événement se terminera avant que le répondeur VoiceAttack démarre un événement (ou vice versa).

exemple de création : {set Nom_de_ma_variable to Valeur)} ==> si la valeur est "test"
exemple de lecture : {Nom_de_ma_variable} ==> renverra le texte "test"


## Contexte

EDDI utilise l'idée de contexte pour essayer de garder une trace de ce dont il parle. Cela peut améliorer l'expérience lorsqu'il est utilisé avec VoiceAttack en permettant la répétition et des informations plus détaillées à fournir.

exemple : entête du script "FSD engaged" (qui se lance lorsque l'on fait un saut d'un systême à un autre)
{_ Context }
{SetState('eddi_context_last_subject', 'fsd')}
{SetState('eddi_context_last_action', 'engage')}
{SetState('eddi_context_system_name', event.system)}
{SetState('eddi_context_system_system', event.system)}
{SetState('eddi_context_fsd_target', event.target)}


## Fonctions d'EDDI

En plus des fonctions de base de Cottle, EDDI dispose d'un certain nombre de fonctions informations spécifiques pour Elite: Dangerous. Les détails de ces fonctions sont les suivants:

### P()

Cette fonction essaiera de fournir une prononciation phonétique pour le texte fourni.

P() prend pour seul argument le texte à prononcer.

L'usage courant est pour prononcer les noms des planètes, des puissances, des vaisseaux etc., par exemple:

    Vous arrivez dans le système {P(system.name)}.

### OneOf()

Cette fonction prendra l'un des arguments disponibles, en choisissant aléatoirement.

OneOf() peut avoir autant d'arguments que vous le souhaitez.

L'usage courant est de fournir une variation au texte parlé, par exemple:

	{OneOf("Établissement d'une liaison avec", "Contact établi avec", "Connection avec", "Accès avec")}
	{OneOf("le réseau informatique", "le réseau", "les réseaux", " les ordinateurs", "les services")}.
	{OneOf("de la base", "de la station", "de {event.station}")}
	{OneOf("Réussi", ": Validé", ": Conforme", ": fonctionnel")}.

### Occasionally()

Occasionally() utilise deux arguments, un chiffre n et un texte.

Son fonctionnement est que le texte a 1/n chance d'être prononcé.

L'usage courant de ceci est de fournir du texte supplémentaire qui est dit de temps en temps mais qui deviendrait irritant si on le disait tout le temps, par exemple:

   une étoile de type W plutôt agée {Occasionally(4, "approchant de sa fin de vie et ne consommant plus d'hydrogène")}.

Notez que Occasionally() fonctionne avec des nombres aléatoires et non des compteurs, donc dans l'exemple ci-dessus, le texte supplémentaire ne s'affichera pas tous les 4 fois, mais a 25% de chance d'être dit à chaque fois.

### Pause()

Cette fonction mettra le discours en pause pendant une durée donnée.

Pause() Utilise une variable numérique : le temps que vous voulez attendre.

L'usage courant de ceci est de permettre à la parole de se synchroniser avec les sons du jeu, par exemple d'attendre une réponse connue à une phrase avant de continuer, par exemple:

    Bonjour.  {Pause(2000)} Oui.

### SetState()

Cette fonction définira une valeur globale. La valeur sera disponible en tant que propriété de l'objet 'state' dans les futurs scripts de la même session EDDI.

SetState prend deux arguments: le nom de la valeur d'état à définir et sa valeur. Le nom de la valeur d'état sera converti en minuscule et les espaces remplacés par des traits de soulignement (eg 'Mon Journal de Vol' deviendra 'mon_journal_de_vol'). La valeur doit être un booléen (true ou false), un nombre ou un texte; les autres valeurs seront ignorées.

L'utilisation courante de ceci est de garder une trace de l'information cumulative ou persistante dans une session, par exemple:

    {SetState("distance_parcourue_ce_jour", state.distance_parcourue_ce_jour + event.distance)}

### Emphasize()

Cette fonction vous permet de mettre l'accent sur des mots spécifiques (dans la mesure où la voix que vous utilisez le permet, non automatiquement avec les voix windows). Cette fonction utilise des balises SSML.

Emphasize () prend un argument obligatoire: le texte à dire avec emphase. Si aucun argument secondaire n'est spécifié, il doit par défaut être fortement accentué.
Emphasize () prend également un argument facultatif: le degré d'emphase à placer sur le texte (les valeurs légales pour le degré d'emphase incluent "strong" = fort, "moderate" = moyen, "none" = aucun and "reduced" = réduit).

L'utilisation courante de ceci est de fournir une lecture plus humaine du texte en permettant l'application de l'emphase:

   C'est un {Emphasize ('beau', 'strong')} vaisseau que vous avez là.
   
### Humanise()

Cette fonction transformera son argument en un nombre plus humain, par exemple en transformant 31245 en «un peu plus de trente mille».

Humanise() utilise un argument: le nombre à traiter.

L'utilisation générale est de fournir des nombres sonnant mieu à l'oreille humaine que d'égrener chaque chiffre, par exemple:

   Vou avez {Humanise(cmdr.credits)} crédits.

### Spacialise()

Cette fonction va ajouter des espaces entre les lettres d'une chaîne et les convertir en majuscules, afin de permettre aux lettres d'une chaîne d'être prononcées individuellement.

Spacialise() le texte à traiter.

L'usage courant en est de fournir une lecture plus humaine d'une suite de lettres qui ne font pas partie du mot connu:

   Classe de luminosité de l'étoile: {Spacialise(event.luminosityclass)}.

### StartsWithVowel()

Cette conction teste la première lettre d'un mot et renvoie true (vrai) si le mot commence par une voyelle ou false (faux) sinon.

StartsWithVowel() utilise une argumlent : le mot à tester.

L'utilisation courante de ceci est de sélectionner le mot qui devrait procéder à la chaîne (par exemple ** de ** eau comparée à  ** d' ** eau).
   
   il y a des geysers {if StartsWithVowel(reportbody.volcanism.composition): d' |else: de } {reportbody.volcanism.composition}

### SpeechPitch()

Cette fonction vous permet d'ajuster dynamiquement la hauteur du discours parlé. Cette fonction utilise des balises SSML.

SpeechPitch () utilises deux variables obligatoires: le texte à parler et la hauteur à laquelle parler (les valeurs légales pour la modification incluent "x-low", "low", "medium", "high", "x-high" , "default", ainsi que des pourcentages tels que "-20%" ou "+ 10%").

L'utilisation courante de ceci est de fournir une lecture plus humaine du texte avec une variation dans la hauteur du discours:   
																															  

   {SpeechPitch('Ok, who added helium to the life support unit?', 'high')}
   {Pause(1000)}
   {SpeechPitch('Countering with sodium hexa-flouride.', 'x-low')}
   Equilibrium restored.

### SpeechRate()

This function allows you to dynamically adjust the rate of the spoken speech. This function uses SSML tags.

SpeechRate() takes two mandatory arguments: the text to speak and the speech rate at which to speak it (legal values for the speech rate include "x-slow", "slow", "medium", "fast", "x-fast", "default", as well as percentage values like "-20%" or "+20%").

Common usage of this is to provide a more human-sounding reading of text with variation in the speech rate:

   {SpeechRate('The quick brown fox', 'x-slow')}
   {SpeechRate('jumped over the lazy dog', 'fast')}.

### SpeechVolume()

Cette fonction vous permet d'ajuster dynamiquement le volume du discours parlé. Cette fonction utilise des balises SSML.

##### Veuillez prendre soin des valeurs de décibel. Nulle responsabilités ne sera prise par les développeurs si vous détériorez vos haut-parleurs.
SpeechRate () prend deux variables obligatoires: le texte à dire et le valume à appliquer (les valeurs nominatives pour le volume incluent "silent", "x-soft", "soft", "medium", "loud", "x-loud", "default", ainsi que des valeurs relatives de décibels comme "-6dB").
Une valeur de "+ 0dB" signifie pas de changement de volume, "+ 6dB" signifie environ deux fois l'amplitude actuelle (on double), "-6dB" signifie environ la moitié de l'amplitude actuelle (on diminue de moitié).
L'utilisation courante de ceci est de fournir une lecture plus humaine du texte avec une variation du volume de la parole:
   
   {SpeechVolume('The quick brown fox', 'loud')}
   {SpeechVolume('jumped over the lazy dog', 'x-soft')}.

### Play()

Cette fonction jouera un fichier audio tel que fourni dans l'argument. Si cela est dans le résultat d'un script alors tout autre texte est annulé; EDDI ne peut pas lire un fichier audio et parler dans la même réponse de script.

Play () prend un argument: le chemin vers le fichier à lire. Ce fichier doit être un fichier ".wav". Chaque barre oblique inverse pour les séparateurs de chemin doit être doublée, donc '\\' doit être écrit comme '\\\\'

L'utilisation courante de ceci est de fournir un fichier audio personnalisé préenregistré plutôt que d'utiliser le texte-à-parole d'EDDI, par exemple:

    {Play('C:\\Users\\CmdrMcDonald\\Desktop\\Warning.wav')}

### ICAO()

Cette fonction transformera son argument en valeur vocale OACI, par exemple "NCC" deviendra "November Charlie Charlie".

ICAO() utilise un argument: la valeur à donner à l'OACI.

L'utilisation commune est de fournir des indicatifs et des identités claires pour les vaisseaux, par exemple:

   Identificateur de vaisseau : {ICAO(ship.ident)}.

### ShipName()

Cette fonction fournira le nom de votre vaisseau.

Si vous avez configuré un nom phonétique pour votre navire, il renverra cela, sinon, si vous avez configuré un nom pour votre navire, il le renverra.

ShipName () prend un ID de navire facultatif pour lequel fournir le nom. Si aucun argument n'est fourni alors il fournit le nom de votre vaisseau actuel.

Si vous n'avez pas défini de nom pour votre bateau, il vous suffit de retourner "votre vaisseau".

### ShipCallsign()

Cette fonction fournira l'indicatif de votre vaisseau de la même manière que Elite le fournit (c'est-à-dire le fabricant suivi des trois premières lettres du nom de votre commandant).

ShipCallsign () prend un ID de navire facultatif pour lequel l'indicatif doit être fourni. Si aucun argument n'est fourni alors il fournit l'indicatif pour votre navire actuel.

Cela ne fonctionnera que si EDDI est connecté à l'API Frontier.

### ShipDetails()

Cette fonction fournira des informations complètes pour un vaisseau donné.

ShipDetails () utilise pour seul argument le modèle du vaisseau pour lequel vous voulez plus d'informations.

L'utilisation courante de ceci est de fournir de plus amples informations sur un vaisseau, par exemple:

    le Vulture est construit par {ShipDetails("Vulture").manufacturer}.

### SecondsSince()

Cette fonction fournira le nombre de secondes depuis un instant donné.

SecondsSince() utilise pour seul argument un horodatage UNIX.

L'usage courant de ceci est de vérifier combien de temps il s'est passé depuis un instant donné, par exemple:

    Les données de la la station datent de {SecondsSince(station.updatedat) / 3600} heures.

### CombatRatingDetails()

Cette fonction fournira des informations complètes pour un grade de combat fonction de son apélation.

CombatRatingDetails() utilise comme seul argument l'apélation du rang dont vous voulez des informations.

L'usage courant de ceci est de fournir plus d'informations sur votre grade, par exemple:

    You have been promoted {CombatRatingDetails("Expert").rank} times.

### TradeRatingDetails()

Cette fonction fournira des informations complètes pour un rang de commerce fonction de son apélation.

TradeRatingDetails() utilise comme seul argument l'apélation du rang dont vous voulez des informations.

L'usage courant de ceci est de fournir plus d'informations sur votre grade, par exemple:

    You have been promoted {TradeRatingDetails("Peddler").rank} times.

### ExplorationRatingDetails()

Cette fonction fournira des informations complètes pour un statut d'explorateur fonction de son apélation.

ExplorationRatingDetails() utilise comme seul argument l'apélation du rang dont vous voulez des informations.

L'usage courant de ceci est de fournir plus d'informations sur votre grade, par exemple:

    You have been promoted {ExplorationRatingDetails("Surveyor").rank} times.

### EmpireRatingDetails()

Cette fonction fournira des informations complètes pour un grade dans l'empire fonction de son apélation.

EmpireRatingDetails() utilise comme seul argument l'apélation du rang dont vous voulez des informations.

L'usage courant de ceci est de fournir plus d'informations sur votre grade, par exemple:

    You have been promoted {EmpireRatingDetails("Lord").rank} times.

### FederationRatingDetails()

Cette fonction fournira des informations complètes pour un grade dans la fédération fonction de son apélation.

FederationRatingDetails() utilise comme seul argument l'apélation du rang dont vous voulez des informations.

L'usage courant de ceci est de fournir plus d'informations sur votre grade, par exemple:

    You have been promoted {FederationRatingDetails("Post Commander").rank} times.

### SystemDetails()

Cette fonction fournie des informations sur un système stellaire fonction de son nom.

SystemDetails() utilise pour seul argument le nom du système dont vous voulez les informations.

L'utilisation courante de ceci est de fournir de plus amples informations sur un système stellaire, par exemple:

    Sol has {len(SystemDetails("Sol").bodies)} bodies.

### StationDetails()

Cette fonction fournira des informations complètes pour une station donnée avec son nom et optionnellement le nom de son système.

StationDetails () utilise un argument obligatoire du nom de la station pour laquelle vous voulez plus d'informations. Si la station n'est pas dans le système actuel, vous pouvez alors donner un deuxième paramètre du nom du système.

L'utilisation courante de ceci est de fournir des informations supplémentaires sur une station, par exemple:

    {set station to StationDetails("Jameson Memorial", "Shinrarta Dezhra")}
    Jameson Memorial est à {station.distancefromstar} années lumière de l'étoile principale de son système.

### BodyDetails()

Cette fonction fourni un ensemble de donnée sur les corps célestes (planètes, lunes et géantes gazeuses).

BodyDetails() utilise comme seul paramètre obligatoire le nom du corps dont vous voules les informations.  Si le corps céleste n'est pas dans votre système actuel, vous pouvez ajouter un paramètre optionnel du nom du système d'appartenance.

L'usage courant de ceci est de fournir de plus amples informations sur un corps céleste, par exemple:

    {set body to BodyDetails("Earth", "Sol")}
    le Terre est à {body.distancefromstar} années lumière de son étoile, le Soleil.
	
	Remarquez que les paramètres utilisent les nom anglophones en usage dans le jeu.

### MaterialDetails()

Cette fonction fournira des informations complètes pour un matériau donné.

MaterialDetails () prend pour seul argument le nom du matériau dont vous voulez les informations.

L'usage courant de ceci est de fournir de plus amples informations sur un matériau, par exemple:

    le fer est un materiau {MaterialDetails("Iron").rarity.name}.
	
### SuperpowerDetails()

Cette fonction fournira des informations complètes pour une superpuissance en fonction de son nom.

SuperpowerDetails() utilise pour seul argument le nom de la superpuissance.

À l'heure actuelle, cela n'a pas beaucoup d'utilité car l'objet SuperpowerDetails() ne contient que son nom, mais on peut espérer à ce qu'il soit développé à l'avenir.

### StateDetails()

Cette fonction fournira des informations complètes pour un état en fonction de son nom.

StateDetails() prend un seul argument de l'état pour lequel vous voulez plus d'informations.

A l'heure actuelle, cela n'a pas beaucoup d'utilité car l'objet state ne contient que son nom, mais on peut espérer qu'il soit développé à l'avenir.

### EconomyDetails()

Cette fonction fournira des informations complètes pour une économie en fonction de son nom.

EconomyDetails() prend pour seul argument l'économie pour laquelle vous voulez plus d'informations.

A l'heure actuelle, cela n'a pas beaucoup d'utilité car l'objet state ne contient que son nom, mais on peut espérer qu'il soit développé à l'avenir.

### GovernmentDetails()

Cette fonction fournira des informations complètes pour un gouvernement en fonction de son nom.

GovernmentDetails() prend pour seul argument le gouvernement pour lequel vous voulez plus d'informations.

A l'heure actuelle, cela n'a pas beaucoup d'utilité car l'objet state ne contient que son nom, mais on peut espérer qu'il soit développé à l'avenir.

### SecurityLevelDetails()

Cette fonction fournira des informations complètes pour un niveau de sécurité en fonction de son nom.

SecurityLevelDetails() prend pour seul argument le niveau de sécurité pour lequel vous voulez plus d'informations.

A l'heure actuelle, cela n'a pas beaucoup d'utilité car l'objet state ne contient que son nom, mais on peut espérer qu'il soit développé à l'avenir.


### Log()

Cette fonction écrira le texte choisi dans le log de EDDI.

Log() utilise pour seul argument le texte que vous vouler introduire dans le log.
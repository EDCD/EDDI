# EDDI: The Elite Dangerous Data Interface

Version actuelle francisation de la: 2.3.0

EDDI est une application complémentaire pour Elite: Dangerous, fournissant des réponses aux événements qui se produisent dans le jeu en utilisant les données du jeu ainsi que divers outils tiers.

EDDI lit les données provenant de diverses sources pour fournir aux joueurs des informations supplémentaires pendant leur jeu, ainsi que des événements pouvant déclencher des actions telles que les réponses parlé ou les actions VoiceAttack. Le flux de base d'EDDI est présenté ci-dessous:

![](images/Architecture.png)

Les "Monitors" sont des morceaux de code qui vérifient les informations, par exemple un nouvel article de Galnet publié ou une entrée dans le journal d'Elite. Les moniteurs transmettent des informations sur l'événement à EDDI.

EDDI prend des événements et exécute des opérations en fonction de celles-ci. Par exemple, si l'événement indique que le joueur a changé de système, EDDI récupérera les informations du système stellaire mises à jour depuis EDDB. Une fois qu'il a rassemblé toutes les informations requises, il informera chaque opérateur du logiciel pour "dire" les informations.

Les "Plug-in" captent les événements, ainsi que toute information recueillie par EDDI et réalisent des actions. Par exemple, le "EDSM responder" envoie les détails des sauts que le joueur effectue à EDSM afin qu'ils aient un enregistrement permanent de leurs journaux de vol, le plu-in répond à des événements, le plug-in "VoiceAttack responder" fournit des variables aux scripts VoiceAttack et exécute des scripts spécifiques Lorsque les événements se produisent, etc....

Les "monitors" et les "responder" peuvent être configurés à partir de l'interface utilisateur EDDI et peuvent être activés ou désactivés individuellement au besoin.
## Installation et configuration de EDDI

EDDI Peut être utilisé seul, ou en plug-in de VoiceAttack.

Vous pouvez télécharger la V.O. (en anglais) de EDDI à : [http://www.mcdee.net/elite/EDDI.exe](http://www.mcdee.net/elite/EDDI.exe) et la version francisé à  [http://dl.fre.fr/EDDI-2.3.0.french-translation-test.exe](http://dl.free.fr/ktgCDjfVG), pour l'instant ce n'est que la version test. Cela sera édité plus tard si évolution.  Par défaut, l'insatalltion se fait dans le répertoire C:\Program Files (x86)\VoiceAttack\Apps\EDDI, ce qui fonctionne bien, que vous ayez VoiceAttack ou non, mais bien sûr, vous pouvez le modifier si vous le souhaitez (Notez toutefois que si vous l'installez dans un autre répertoire, EDDI ne pourra pas fonctionner en tant que plug-in de VoiceAttack).
Avant de faire l'installation, pour ceux ayant déjà installer une version en anglais de EDDI il est conseillé d'aller dans le répetoire C:\Users\xxXXxx\AppData\Roaming\EDDI\  où xxXXxx est votre nom d'utilisateur du PC et d'affacer le fichier "materialmonitor.json" ou de le renommer en "materialmonitor.json.VO" par exemple.
Vous pouvez recomplier le code source 
Version Originale : [https://github.com/cmdrmcdonald/EliteDangerousDataProvider](https://github.com/cmdrmcdonald/EliteDangerousDataProvider).
Francisation :  [https://github.com/Astilane/EliteDangerousDataProvider](https://github.com/Astilane/EliteDangerousDataProvider).

Lorsque vous démarrez EDDI, il y aura une fenêtre avec plusieurs onglets. Chaque onglet explique sa fonction et sa configuration, de sorte que vous serez mieux servi à utiliser chaque onglet et à configurer selon vos envies.

## Utilisation EDDI avec VoiceAttack

L'intégration initiale d'EDDI avec VoiceAttack est automatique, mais il existe beaucoup de choses que vous pouvez faire pour intégrer EDDI avec vos propres scripts VoiceAttack. Des détails complets sur ce que vous pouvez faire avec EDDI et VoiceAttack sont sur le (en anglais) [VoiceAttack EDDI page](https://github.com/cmdrmcdonald/EliteDangerousDataProvider/blob/master/VoiceAttack.md#using-eddi-with-voiceattack).

## Passage EDDI1 vers EDDI2

Si vous effectuez une mise à niveau depuis EDDI1, il est recommandé de désinstaller votre version existante d'EDDI1 et de supprimer votre répertoire %APPDATA%\EDDI avant de comancer la nouvelle instalation.  Cela fera une installation plus propre et évitera de futurs problèmes d'utilisation.

Si vous utilisiez EDDI1 avec VoiceAttack, il vous est conseillé de suivre les instruction (en anglais) [https://github.com/cmdrmcdonald/EliteDangerousDataProvider/blob/master/VoiceAttack.md#upgrading-from-eddi-1x](https://github.com/cmdrmcdonald/EliteDangerousDataProvider/blob/master/VoiceAttack.md#upgrading-from-eddi-1x).

Si vous faite une mise à jour de EDDI2 il n'y a qu'a suivre les instruction de l'intallateur, execpté pour le fichier "materialmonitor.json" vu plus haut.

## EDDI les Voix

EDDI utilise les voix standard de Windows TTS (Text To Speech ==> lire ce qui est écrit). Pour être admissible à EDDI, la voix doit prendre en charge le discours phonétique. Outre les voix par défaut de Windows, il existe des voix commerciales disponibles auprès de IVONA et Cereproc, entre autres (pour les voix en français, je sais pas trop où), qui peuvent être utilisées. La voix doit être visible pour que le système TTS de Windows soit mis à la disposition d'EDDI: cela se produit généralement lorsque vous installez la voix. Si vous ne voyez pas une voix dans EDDI, vérifiez les paramètres de Windows TTS.

# Problèmes

Si vous rencontrez des problèmes avec EDDI regardez en premier (en anglais) [troubleshooting page](https://github.com/cmdrmcdonald/EliteDangerousDataProvider/blob/master/TROUBLESHOOTING.md#troubleshooting).  Si cela ne résout pas votre problème, vérifiez les problèmes connus ci-dessous:

  * EDDI s'appuie sur l'API du jeu Elite: Dangerous pour beaucoup de ses informations. Parfois, EDDI perd la connexion à l'API et doit se ré-authentifier. Si vous pensez que c'est un problème, vous pouvez ré-exécuter 'EDDI.exe' et si la connexion est mauvaise, elle demandera une ré-authentification
  
  * EDDI est incapable de savoir avec certitude si vous avez fourni le chemin d'accès correct au répertoire Logs. La seule façon de savoir cela avec certitude est de faire un suat en Hyper-espace et de voir si EDDI vous dicte un rapport sur votre destination.
  
  
Si vous avez un problème avec EDDI, veuillez le signaler au https://github.com/cmdrmcdonald/EliteDangerousDataProvider/issues en anglais, ou si c'est un problème de traduction, le signaler ici.

Si vous avez rencontré un problème, veuillez fournir le rapport d'erreur (shift-control-alt-e) pour aider à résoudre le problème.

# Désinstaller EDDI
Si vous souhaitez désinstaller EDDI, vous pouvez le faire via le panneau de contrôle Windows. Toutes les données générées par EDDI sont stockées dans le répertoire %APPDATA%\EDDI, ce qui peut également être supprimé lors de la désinstallation

using System.Collections.Generic;
using Utilities;

namespace EddiDataDefinitions
{
    public class MissionFactionEffect
    {
        [PublicAPI("The influenced faction")]
        public string faction { get; }

        public List<MissionEffect> effects { get; }

        [PublicAPI("Influences, as a list")]
        public List<MissionInfluence> influences { get; }

        [PublicAPI("The reputation impact (in plusses)")]
        public string reputation { get; } // e.g. "+++"

        public MissionFactionEffect(string faction, List<MissionEffect> effects, List<MissionInfluence> influences, string reputation)
        {
            // Replace `$#MinorFaction;` with the affected faction name
            effects.ForEach(e => 
                e.localizedEffect = e.localizedEffect.Replace("$#MinorFaction;", faction));
            // TODO: Update the localized effects to replace `$#System;` with the affected system name

            this.faction = faction;
            this.effects = effects;
            this.influences = influences;
            this.reputation = reputation;

        }
    }

    public class MissionEffect
    {
        public string edEffect { get; }

        public string localizedEffect { get; set; }

        public MissionEffect(string edEffect, string localizedEffect)
        {
            this.edEffect = edEffect;
            this.localizedEffect = localizedEffect;
        }
    }

    public class MissionInfluence
    {
        [PublicAPI("The system address of the influenced system")]
        public long? systemAddress { get; }

        [PublicAPI("The influence impact (in plusses)")]
        public string influence { get; } // e.g. "++"

        public MissionInfluence(long? systemAddress, string influence)
        {
            this.systemAddress = systemAddress;
            this.influence = influence;
        }
    }
}

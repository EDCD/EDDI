using EddiDataDefinitions.Properties;
using JetBrains.Annotations;
using MathNet.Numerics;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Threading;
using Utilities;

namespace EddiDataDefinitions
{
    public class CodexEntry
    {
        public OrganicItem organic;            // TODO:#2212........[Change to CodexOrganicItem?]
        public AstrometricItem astrology;    // TODO:#2212........[Change to CodexAstrometricItem?]
        public GeologyItem geology;            // TODO:#2212........[Change to CodexGeologyItem?]
        //public GuardianItem guardian;        // TODO:#2212........[Add Guardian codex entries]
        //public ThargoidItem thargoid;        // TODO:#2212........[Add Thargoid codex entries]

        public long entryId;
        public string edname;

        public string subCategory;
        public string category;
        public string region;
        public string system;

        public CodexEntry ( long entryId, string edname, string subCategory, string category, string region, string system )
        {
            organic = new OrganicItem();
            astrology = new AstrometricItem();
            geology = new GeologyItem();

            this.entryId = entryId;
            this.edname = edname;
            this.subCategory = subCategory;
            this.category = category;
            this.region = region;
            this.system = system;


            if ( category == "Biology" ) {
                if ( subCategory == "Organic_Structures" )
                {
                    // TODO:#2212........[Move these to 'organic' object]

                    // Intended primary source (EntryIds have changed?)
                    //OrganicItem organicItem = OrganicInfo.LookupByEntryId (entryId);

                    // Fallback
                    organic = OrganicInfo.LookupByVariant( edname );
                }
                else if ( subCategory == "Geology_and_Anomalies" ) {
                    // TODO:#2212........[Add Geology codex entries]
                    geology = GeologyInfo.LookupByName( edname );
                }
            }
            else if ( category == "StellarBodies" )
            {
                astrology = AstrometricInfo.LookupByName( edname );
            }
            else if ( category == "Civilisations" ) {
                // TODO:#2212........[Possibly combine Thargoid and Guardian?]
                if ( subCategory == "Guardian" )
                {
                    // TODO:#2212........[Add Guardian codex entries]
                }
                else if ( subCategory == "Thargoid" )
                {
                    // TODO:#2212........[Add Thargoid codex entries]
                }
            }
        }
    }
}

using EddiConfigService.Configurations;
using EddiDataDefinitions;
using System.Collections.Generic;
using System;
using Utilities;

namespace EddiDiscoveryMonitor
{
    public class ExobiologyPredictions
    {
        private readonly StarSystem _currentSystem;
        private readonly Body body;
        private readonly DiscoveryMonitorConfiguration configuration;

        public ExobiologyPredictions ( StarSystem starSystem, Body body, DiscoveryMonitorConfiguration configuration )
        {
            this._currentSystem = starSystem;
            this.body = body;
            this.configuration = configuration;
        }

        /// <summary>
        /// This currently works but gives incorrect predictions
        /// Prediction data needs adjustment to use this
        /// </summary>
        public HashSet<OrganicGenus> PredictByVariants ()
        {
            String log = "";
            bool enableLog = configuration.enableLogging;

            // Create a list to store predicted variants
            var listPredicted = new List<OrganicVariant>();

            // Iterate though species
            foreach ( var variant in OrganicVariant.AllOfThem )
            {
                if ( enableLog ) { log += $"[Predictions] CHECKING VARIANT {variant}: "; }
                
                // Get conditions for current variant
                if ( variant != null )
                {
                    // Check if body meets max gravity requirements
                    // maxG: Maximum gravity
                    if ( variant.maxG != 0 )
                    {
                        if ( variant.maxG != 0 && variant.minG != 0 )
                        {
                            if ( body.gravity < variant.minG )
                            {
                                if ( enableLog )
                                { log += $"\tPURGE (gravity: {body.gravity} < {variant.minG})\r\n"; }
                                goto Skip_To_Purge;
                            }
                            else if ( body.gravity > variant.maxG )
                            {
                                if ( enableLog )
                                { log += $"\tPURGE (gravity: {body.gravity} > {variant.maxG})\r\n"; }
                                goto Skip_To_Purge;
                            }
                        }
                    }

                    // Check if body meets temperature (K) requirements
                    //  - data.kRange: 'None'=No K requirements; 'Min'=K must be greater than minK; 'Max'=K must be less than maxK; 'MinMax'=K must be between minK and maxK
                    //  - data.minK: Minimum temperature
                    //  - data.maxK: Maximum temperature
                    if ( variant.maxK != 0 && variant.minK != 0 )
                    {
                        if ( body.temperature < variant.minK )
                        {
                            if ( enableLog )
                            { log += $"\tPURGE (temperature: {body.temperature} < {variant.minK})\r\n"; }
                            goto Skip_To_Purge;
                        }
                        else if ( body.temperature > variant.maxK )
                        {
                            if ( enableLog )
                            { log += $"\tPURGE (temperature: {body.temperature} > {variant.maxK})\r\n"; }
                            goto Skip_To_Purge;
                        }
                    }

                    // Check if body has appropriate class
                    bool bodyClassMatches = false;
                    if ( variant.planetClass.Count > 0 )
                    {
                        foreach ( string planetClass in variant.planetClass )
                        {
                            if ( planetClass == body.planetClass.edname )
                            {
                                bodyClassMatches = true;
                                break;  // If found then we don't care about the rest
                            }
                        }

                        if ( !bodyClassMatches )
                        {
                            if ( enableLog )
                            { log += $"\tPURGE (planet class: {body.planetClass.edname} != [{string.Join( ",", variant.planetClass )}])\r\n"; }
                            goto Skip_To_Purge;
                        }
                    }

                    // Check if body has appropriate astmosphere
                    {
                        bool atmosphereMatches = false;
                        //if ( enableLog ) { log += $"\tatmosphereClass.Count = {check.atmosphereClass.Count}\r\n"; }
                        if ( variant.atmosphereClass.Count > 0 )
                        {
                            foreach ( string atmosphereClass in variant.atmosphereClass )
                            {
                                if ( atmosphereClass == body.atmosphereclass.edname )
                                {
                                    atmosphereMatches = true;
                                    break;  // If found then we don't care about the rest
                                }
                            }

                            if ( !atmosphereMatches )
                            {
                                if ( enableLog )
                                { log += $"\tPURGE (atmosphere class: {body.atmosphereclass.edname} != [{string.Join( ",", variant.atmosphereClass )}])\r\n"; }
                                goto Skip_To_Purge;
                            }
                        }
                    }

                    // Check if body has appropriate volcanism
                    {
                        bool volcanismMatches = false;
                        if ( variant.volcanism.Count > 0 )
                        {
                            foreach ( string volcanism in variant.volcanism )
                            {
                                string amount = null;
                                string composition = "";
                                string type = "";

                                string[] parts = volcanism.Split(',');
                                if ( parts.Length > 0 )
                                {
                                    if ( parts.Length == 2 )
                                    {
                                        // amount 'null' is normal
                                        composition = parts[ 0 ];
                                        type = parts[ 1 ];
                                    }
                                    else if ( parts.Length == 3 )
                                    {
                                        amount = parts[ 0 ];
                                        composition = parts[ 1 ];
                                        type = parts[ 2 ];
                                    }
                                }

                                // Check if amount, composition and type matc hthe current body
                                if ( amount == body.volcanism.invariantAmount && composition == body.volcanism.invariantComposition && type == body.volcanism.invariantType )
                                {
                                    volcanismMatches = true;
                                    break;  // If found then we don't care about the rest
                                }
                            }

                            if ( !volcanismMatches )
                            {
                                if ( enableLog )
                                { log += $"\tPURGE (volcanism: {body.volcanism.invariantAmount} {body.volcanism.invariantComposition} {body.volcanism.invariantType} != [{string.Join( ",", variant.volcanism )}])\r\n"; }
                                goto Skip_To_Purge;
                            }
                        }
                    }

                    // Check if body has appropriate parent star
                    {
                        bool found = false;
                        string foundClass = "";
                        if ( variant.starClass.Count > 0 )
                        {
                            bool foundParent = false;
                            foreach ( var parent in body.parents )
                            {
                                foreach ( string key in parent.Keys )
                                {
                                    if ( key == "Star" )
                                    {
                                        foundParent = true;
                                        long starId = (long)parent[ key ];

                                        Body starBody = _currentSystem.BodyWithID( starId );
                                        string starClass = starBody.stellarclass;
                                        foundClass = starClass;

                                        foreach ( string checkClass in variant.starClass )
                                        {
                                            if ( checkClass == starClass )
                                            {
                                                found = true;
                                                goto ExitParentStarLoop;
                                            }
                                        }

                                    }
                                    else if ( key == "Null" )
                                    {
                                        long baryId = (long)parent[ key ];
                                        var barys = _currentSystem.GetChildBodyIDs( baryId );

                                        foreach ( long bodyId in barys )
                                        {
                                            if ( _currentSystem.BodyWithID( bodyId ).bodyType.edname == "Star" )
                                            {
                                                long starId = bodyId;

                                                Body starBody = _currentSystem.BodyWithID( starId );
                                                string starClass = starBody.stellarclass;
                                                foundClass = starClass;

                                                foreach ( string checkClass in variant.starClass )
                                                {
                                                    if ( checkClass == starClass )
                                                    {
                                                        found = true;
                                                        goto ExitParentStarLoop;
                                                    }
                                                }
                                            }

                                            if ( found )
                                            {
                                                goto ExitParentStarLoop;
                                            }
                                        }
                                    }
                                    if ( foundParent )
                                    {
                                        goto ExitParentStarLoop;
                                    }
                                }
                            }

                            ExitParentStarLoop:
                            ;

                            if ( !found )
                            {
                                if ( enableLog )
                                { log = log + $"\tPURGE (parent star: {foundClass} != {string.Join( ",", variant.starClass )})\r\n"; }
                                goto Skip_To_Purge;
                            }
                        }
                    }

                    log += $"OK\r\n";
                    listPredicted.Add( variant );
                    goto Skip_To_End;
                }

                Skip_To_Purge:
                ;

                Skip_To_End:
                ;

                if ( enableLog )
                {
                    Logging.Debug( log );
                }
            }

            // Create a list of only the unique genus' found
            if ( enableLog ) { log = "[Predictions] Genus List:"; }
            var genus = new HashSet<OrganicGenus>();
            foreach ( var variant in listPredicted )
            {
                if ( !genus.Contains( variant.genus ) )
                {
                    if ( enableLog ) { log += $"\r\n\t{variant.genus.edname}"; }
                    genus.Add( variant.genus );
                }
            }

            if ( enableLog )
            {
                Logging.Info( log );
            }

            return genus;
        }

        /// <summary>
        /// This currently works and provides fairly accurate predictions
        /// </summary>
        public HashSet<OrganicGenus> PredictBySpecies ()
        {
            String log = "";
            bool enableLog = true;

            if ( enableLog )
            { log += $"[Predictions] Body '{body.bodyname}'\r\n"; }

            // Create temporary list of ALL species possible
            var listPredicted = new List<OrganicSpecies>();

            // Iterate though species
            foreach ( var species in OrganicSpecies.AllOfThem )
            {
                if ( enableLog )
                { log += $"\tCHECKING '{species.edname}': "; }

                // Handle ignored species
                if ( ( configuration.exobiology.predictions.skipCrystallineShards && species.genus == OrganicGenus.GroundStructIce ) ||
                     ( configuration.exobiology.predictions.skipBrainTrees && species.genus == OrganicGenus.Brancae ) ||
                     ( configuration.exobiology.predictions.skipBarkMounds && species.genus == OrganicGenus.Cone ) ||
                     ( configuration.exobiology.predictions.skipTubers && species.genus == OrganicGenus.Tubers ) )
                {
                    if ( enableLog )
                    { log += $"IGNORE '{species.genus.edname}'\r\n"; }
                    goto Skip_To_Purge;
                }

                // Iterate through conditions
                // Get conditions for current variant
                if ( species != null )
                {
                    // Check if body meets max gravity requirements
                    {
                        // maxG: Maximum gravity
                        if ( species.maxG != null && species.maxG != 0 )
                        {
                            if ( body.gravity > species.maxG )
                            {
                                if ( enableLog ) { log += $"PURGE (gravity: {body.gravity} > {species.maxG})\r\n"; }
                                goto Skip_To_Purge;
                            }
                        }
                    }

                    // Check if body meets temperature (K) requirements
                    {
                        //  - data.kRange: 'None'=No K requirements; 'Min'=K must be greater than minK; 'Max'=K must be less than maxK; 'MinMax'=K must be between minK and maxK
                        //  - data.minK: Minimum temperature
                        //  - data.maxK: Maximum temperature
                        if ( species.kRange != "" && species.kRange != "None" )
                        {
                            if ( species.kRange == "<k" )
                            {
                                if ( body.temperature < species.minK )
                                {
                                    if ( enableLog ) { log += $"PURGE (temp: {body.temperature} < {species.minK})\r\n"; }

                                    goto Skip_To_Purge;
                                }
                            }
                            else if ( species.kRange == "k<" )
                            {
                                if ( body.temperature > species.maxK )
                                {
                                    if ( enableLog ) { log += $"PURGE (temp: {body.temperature} > {species.maxK})\r\n"; }
                                    goto Skip_To_Purge;
                                }
                            }
                            else if ( species.kRange == "<k<" )
                            {
                                if ( body.temperature < species.minK || body.temperature > species.maxK )
                                {
                                    if ( enableLog ) { log += $"PURGE (temp: {body.temperature} < {species.minK} || {body.temperature} > {species.maxK})\r\n"; }
                                    goto Skip_To_Purge;
                                }
                            }
                        }
                    }

                    // Check if body has appropriate class
                    {
                        bool found = false;
                        if ( species.planetClass.Count > 0 )
                        {
                            foreach ( string planetClass in species.planetClass )
                            {
                                if ( planetClass == body.planetClass.edname )
                                {
                                    found = true;
                                    break;  // If found then we don't care about the rest
                                }
                            }

                            if ( !found )
                            {
                                if ( enableLog )
                                { log += $"\tPURGE (planet class: {body.planetClass.edname} != [{string.Join( ",", species.planetClass )}])\r\n"; }
                                goto Skip_To_Purge;
                            }
                        }
                    }

                    // Check if body has appropriate astmosphere
                    {
                        bool found = false;
                        //if ( enableLog ) { log += $"\tatmosphereClass.Count = {check.atmosphereClass.Count}\r\n"; }
                        if ( species.atmosphereClass.Count > 0 )
                        {
                            foreach ( string atmosphereClass in species.atmosphereClass )
                            {
                                if ( ( atmosphereClass == "Any" && body.atmosphereclass.edname != "None" ) ||
                                     ( atmosphereClass == body.atmosphereclass.edname ) )
                                {
                                    found = true;
                                    break;  // If found then we don't care about the rest
                                }
                            }

                            if ( !found )
                            {
                                if ( enableLog ) { log += $"\tPURGE (atmosphere class: {body.atmosphereclass.edname} != [{string.Join( ",", species.atmosphereClass )}])\r\n"; }
                                goto Skip_To_Purge;
                            }
                        }
                    }

                    // Check if body has appropriate volcanism
                    {
                        bool found = false;
                        if ( species.volcanism.Count > 0 )
                        {
                            foreach ( string composition in species.volcanism )
                            {
                                if ( body.volcanism != null )
                                {
                                    // If none but we got this far then the planet has an atmosphere
                                    if ( composition == "None" )
                                    {
                                        break;
                                    }
                                    else if ( composition == "Any" || composition == body.volcanism.invariantComposition )
                                    {
                                        found = true;
                                        break;  // If found then we don't care about the rest
                                    }
                                }
                                else if ( composition == "None" )
                                {
                                    found = true;
                                    break;
                                }
                            }

                            if ( !found )
                            {
                                if ( enableLog )
                                {
                                    if ( body.volcanism != null )
                                    {
                                        log += $"\tPURGE (volcanism: {body.volcanism.invariantComposition} != [{string.Join( ",", species.volcanism )}])\r\n";
                                    }
                                    else
                                    {
                                        log += $"\tPURGE (volcanism: null != [{string.Join( ",", species.volcanism )}])\r\n";
                                    }
                                }

                                goto Skip_To_Purge;
                            }
                        }
                    }

                    // Check if body has appropriate parent star
                    {
                        bool found = false;
                        string foundClass = "";

                        if ( species.starClass.Count > 0 )
                        {
                            bool foundParent = false;
                            foreach ( var parent in body.parents )
                            {
                                foreach ( string key in parent.Keys )
                                {
                                    if ( key == "Star" )
                                    {
                                        foundParent = true;
                                        long starId = (long)parent[ key ];

                                        Body starBody = _currentSystem.BodyWithID( starId );
                                        string starClass = starBody.stellarclass;
                                        foundClass = starClass;

                                        foreach ( string checkClass in species.starClass )
                                        {
                                            if ( checkClass == starClass )
                                            {
                                                found = true;
                                                goto ExitParentStarLoop;
                                            }
                                        }

                                    }
                                    else if ( key == "Null" )
                                    {
                                        long baryId = (long)parent[ key ];
                                        var barys = _currentSystem.GetChildBodyIDs( baryId );

                                        foreach ( long bodyId in barys )
                                        {
                                            if ( _currentSystem.BodyWithID( bodyId ) != null )
                                            {
                                                if ( _currentSystem.BodyWithID( bodyId ).bodyType.edname == "Star" )
                                                {
                                                    long starId = bodyId;

                                                    Body starBody = _currentSystem.BodyWithID( starId );
                                                    string starClass = starBody.stellarclass;
                                                    foundClass = starClass;

                                                    foreach ( string checkClass in species.starClass )
                                                    {
                                                        if ( checkClass == starClass )
                                                        {
                                                            found = true;
                                                            goto ExitParentStarLoop;
                                                        }
                                                    }
                                                }
                                            }

                                            if ( found )
                                            {
                                                goto ExitParentStarLoop;
                                            }
                                        }
                                    }
                                    if ( foundParent )
                                    {
                                        goto ExitParentStarLoop;
                                    }
                                }
                            }

                            ExitParentStarLoop:
                            ;

                            if ( !found )
                            {
                                if ( enableLog ) { log += $"\tPURGE (parent star: {foundClass} != {string.Join( ",", species.starClass )})\r\n"; }
                                goto Skip_To_Purge;
                            }
                        }
                    }

                    // TODO:#2212........[Implement special case predictions if possible]
                    {
                        // Brain Trees
                        //  - Near system with guardian structures
                        //if ( genus == "Brancae" )
                        //{
                        //    if ( ? ? ? )
                        //    {
                        //        if ( enableLog ) { log = log + $"\tPURGE (?: ? ? ? )\r\n"; }
                        //        goto Skip_To_Purge;
                        //    }
                        //}

                        // Electricae radialem:
                        //  - Near nebula (how close is near?)
                        //if ( genus == "Electricae" )
                        //{
                        //    if ( ? ? ? )
                        //    {
                        //        if ( enableLog ) { log = log + $"\tPURGE (?: ? ? ? )\r\n"; }
                        //        goto Skip_To_Purge;
                        //    }
                        //}

                        // Crystalline Shards:
                        //  - Must be >12000 Ls from nearest star.
                        //if ( genus == "GroundStructIce" )
                        //{
                        //    if ( ? ? ? )
                        //    {
                        //        if ( enableLog ) { log = log + $"\tPURGE (?: ? ? ? )\r\n"; }
                        //        goto Skip_To_Purge;
                        //    }
                        //}

                        // Bark Mounds
                        //  - Seems to always have 3 geologicals
                        //  - Should be within 150Ly from a nebula
                        if ( species.genus == OrganicGenus.Cone )
                        {
                            if ( body.surfaceSignals.geosignals.Count < 3 )
                            {
                                if ( enableLog )
                                { log = log + $"\tPURGE (geo signals: {body.surfaceSignals.geosignals.Count} < 3)\r\n"; }
                                goto Skip_To_Purge;
                            }
                        }
                    }

                    if ( enableLog ) { log += $"OK\r\n"; }
                    listPredicted.Add( species );
                    goto Skip_To_End;
                }

                Skip_To_Purge:
                ;

                Skip_To_End:
                ;
            }

            // Create a list of only the unique genus' found
            if ( enableLog ) { log += "[Predictions] Genus List:"; }
            var genusList = new HashSet<OrganicGenus>();
            foreach ( var species in listPredicted )
            {
                var genus = species.genus;
                if ( !genusList.Contains( genus ) )
                {
                    if ( enableLog ) { log += $"\r\n\t{species.genus.edname}"; }
                    genusList.Add( genus );
                }
            }
            if ( enableLog ) { Logging.Debug( log ); }

            return genusList;
        }
    }
}

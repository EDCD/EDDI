﻿using EddiConfigService.Configurations;
using EddiDataDefinitions;
using JetBrains.Annotations;
using System.Collections.Generic;
using System;
using System.Linq;
using Utilities;
using System.ServiceModel.Security;

namespace EddiDiscoveryMonitor
{
    public class ExobiologyPredictions
    {
        private readonly StarSystem _currentSystem;
        private readonly Body body;
        private readonly Body parentStar;
        private readonly DiscoveryMonitorConfiguration configuration;

        public ExobiologyPredictions ( [NotNull] StarSystem starSystem, [NotNull] Body body, [NotNull] Body parentStar, [NotNull] DiscoveryMonitorConfiguration configuration )
        {
            this._currentSystem = starSystem;
            this.body = body;
            this.parentStar = parentStar;
            this.configuration = configuration;
        }

        /// <summary>
        /// This currently works and provides fairly accurate predictions
        /// </summary>
        public HashSet<OrganicGenus> PredictBySpecies ()
        {
            Logging.Debug( $"Generating predictions by species for {body.bodyname} in {_currentSystem.systemname}.");

            // Create temporary list of ALL species possible
            var predictedSpecies = new List<OrganicSpecies>();

            // Iterate though species
            foreach ( var species in OrganicSpecies.AllOfThem )
            {
                var log = $"Checking species {species.edname} (genus: {species.genus}): ";

                if ( !species.isPredictable )
                {
                    log += "SKIP. No known criteria.";
                    Logging.Debug( log );
                    continue;
                }

                if ( !TryCheckConfiguration( species.genus, ref log ) )
                {
                    Logging.Debug( log );
                    continue;
                }

                if ( TryCheckGravity( 0, species.maxG, ref log ) && 
                     TryCheckTemperature( species.minK, species.maxK, ref log ) && 
                     TryCheckPlanetClass( species.planetClass, ref log ) && 
                     TryCheckAtmosphere( species.atmosphereClass, ref log ) && 
                     TryCheckVolcanism( species.volcanism, ref log ) && 
                     TryCheckPrimaryStar( species.starClass, ref log ) && 
                     TryCheckSpecialSpecies( species, ref log ) )
                {
                    log += "OK";
                    predictedSpecies.Add( species );
                }

                Logging.Debug( log );
            }

            // Create a distinct genus list
            List<OrganicGenus> listGenus = predictedSpecies.Select(s => s.genus).Distinct().ToList();

            // Iterate over all predicted species, set the min/max values for the genus list from all predicted species
            for ( int i = 0; i < listGenus.Count(); i++ )
            {
                foreach ( var species in predictedSpecies )
                {
                    if ( listGenus[ i ].edname == species.genus.edname )
                    {
                        // Set initial value
                        if ( listGenus[ i ].predictedMinimumValue == 0 )
                        {
                            listGenus[ i ].predictedMinimumValue = species.value;
                        }

                        if ( listGenus[ i ].predictedMaximumValue == 0 )
                        {
                            listGenus[ i ].predictedMaximumValue = species.value;
                        }

                        // If new minimum detected, overwrite old
                        if ( species.value < listGenus[ i ].predictedMinimumValue )
                        {
                            listGenus[ i ].predictedMinimumValue = species.value;
                        }

                        // If new maximum detected, overwrite old
                        if ( species.value > listGenus[ i ].predictedMaximumValue )
                        {
                            listGenus[ i ].predictedMaximumValue = species.value;
                        }
                    }
                }
            }

            // Return a list of only the unique genus' found
            //return predictedSpecies.Select(s => s.genus).Distinct().ToHashSet();
            return listGenus.ToHashSet();
        }

        /// <summary>
        /// This currently works and provides fairly accurate predictions
        /// </summary>
        public HashSet<OrganicGenus> PredictByVariant ()
        {
            Logging.Debug( $"Generating predictions by variant for {body.bodyname} in {_currentSystem.systemname}.");

            // Create temporary list of ALL variant possible
            var predictedVariants = new List<OrganicVariant>();

            // Iterate though variant
            foreach ( var variant in OrganicVariant.AllOfThem )
            {
                var log = $"Checking variant {variant.edname} (genus: {variant.genus}): ";

                if ( !variant.isPredictable )
                {
                    log += "SKIP. No known criteria.";
                    Logging.Debug( log );
                    continue;
                }

                if ( !TryCheckConfiguration( variant.genus, ref log ) )
                {
                    Logging.Debug( log );
                    continue;
                }

                if ( TryCheckGravity( variant.minG, variant.maxG, ref log ) && 
                     TryCheckTemperature( variant.minK, variant.maxK, ref log ) && 
                     TryCheckPressure( variant.minP, variant.maxP, ref log ) && 
                     TryCheckPlanetClass( variant.planetClass, ref log ) && 
                     TryCheckAtmosphere( variant.atmosphereClass, ref log ) && 
                     TryCheckVolcanismAdvanced( variant.volcanism, ref log ) && 
                     TryCheckPrimaryStar( variant.primaryStarClass, ref log ) && 
                     TryCheckLocalStar( variant.localStarClass, ref log ) && 
                     TryCheckSpecialVariants( variant, ref log ) )
                {
                    log += "OK";
                    predictedVariants.Add( variant );
                }

                Logging.Debug( log );
            }

            // Create a distinct genus list
            List<OrganicGenus> listGenus = predictedVariants.Select(s => s.genus).Distinct().ToList();

            // Iterate over all predicted variants, set the min/max values for the genus list
            for ( int i = 0; i < listGenus.Count(); i++ )
            {
                foreach ( var variant in predictedVariants )
                {
                    if ( listGenus[ i ].edname == variant.genus.edname )
                    {
                        var species = OrganicSpecies.FromEDName( variant.species.edname );
                        if(species != null) {

                            if ( listGenus[ i ].predictedMinimumValue == 0 )
                            {
                                listGenus[ i ].predictedMinimumValue = species.value;
                            
                            }

                            if ( listGenus[ i ].predictedMaximumValue == 0 )
                            {
                                listGenus[ i ].predictedMaximumValue = species.value;
                            }

                            // If new minimum detected, overwrite old
                            if ( species.value < listGenus[ i ].predictedMinimumValue )
                            {
                                listGenus[ i ].predictedMinimumValue = species.value;
                            }

                            // If new maximum detected, overwrite old
                            if ( species.value > listGenus[ i ].predictedMaximumValue )
                            {
                                listGenus[ i ].predictedMaximumValue = species.value;
                            }
                        }
                    }
                }
            }

            // Return a list of only the unique genus' found
            //return predictedSpecies.Select(s => s.genus).Distinct().ToHashSet();
            return listGenus.ToHashSet();
        }

        private bool TryCheckConfiguration ( OrganicGenus genus, ref string log )
        {
            // Check if species should be ignored per configuration settings
            try
            {
                if ( ( configuration.exobiology.predictions.skipCrystallineShards && genus == OrganicGenus.GroundStructIce ) ||
                     ( configuration.exobiology.predictions.skipBrainTrees && genus == OrganicGenus.Brancae ) ||
                     ( configuration.exobiology.predictions.skipBarkMounds && genus == OrganicGenus.Cone ) ||
                     ( configuration.exobiology.predictions.skipTubers && genus == OrganicGenus.Tubers ) )
                {
                    log += "SKIP. Per configuration preferences.";
                    return false;
                }
            }
            catch ( Exception e )
            {
                Logging.Error("Failed to read configuration", e );
            }
            return true;
        }

        private bool TryCheckGravity ( decimal? minG, decimal? maxG, ref string log )
        {
            if ( minG > 0 )
            {
                if ( body.gravity < minG )
                {
                    log += $"REJECT. Gravity: {body.gravity} < {minG}";
                    return false;
                }
            }

            if ( maxG > 0 )
            {
                if ( body.gravity > maxG )
                {
                    log += $"REJECT. Gravity: {body.gravity} > {maxG}";
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Evaluate whether a candidate organic's temperature range matches a given body.
        /// </summary>
        /// <param name="minK">Minimum temperature in Kelvin</param>
        /// <param name="maxK">Maximum temperature in Kelvin</param>
        /// <param name="log"></param>
        /// <returns></returns>
        private bool TryCheckTemperature(decimal? minK, decimal? maxK, ref string log )
        {
            if ( body.temperature < minK )
            {
                log += $"REJECT. Temp: {body.temperature} K < {minK} K.";
                return false;
            }

            if ( body.temperature > maxK )
            {
                log += $"REJECT. Temp: {body.temperature} K > {maxK} K.";
                return false;
            }

            return true;
        }

        private bool TryCheckPressure(decimal? minP, decimal? maxP, ref string log )
        {
            if ( body.pressure < minP )
            {
                log += $"REJECT. Pressure: {body.pressure} atm. < {minP} atm.";
                return false;
            }

            if ( body.pressure > maxP )
            {
                log += $"REJECT. Pressure: {body.pressure} atm. > {maxP} atm.";
                return false;
            }

            return true;
        }

        private bool TryCheckPlanetClass(ICollection<string> checkPlanetClasses, ref string log )
        {
            // Check if body has appropriate planet class
            if ( checkPlanetClasses.Count > 0 )
            {
                if ( checkPlanetClasses.Any( c =>
                        ( ( c == "None" || c == string.Empty ) && ( body.planetClass == null || body.planetClass == PlanetClass.None ) ) ||
                            c == "Any" ||
                            c == body.planetClass.edname ) )
                {
                    return true;
                }
                log += $"REJECT. Planet class: {( body.planetClass ?? PlanetClass.None )?.edname} not in {string.Join( ",", checkPlanetClasses )}.";
                return false;
            }

            return true;
        }

        private bool TryCheckAtmosphere(ICollection<string> checkAtmosphereClasses, ref string log )
        {
            // Check if body has appropriate astmosphere
            if ( checkAtmosphereClasses.Count > 0 )
            {
                if ( checkAtmosphereClasses.Any( c =>
                        ( ( c == "None" || c == string.Empty ) && ( body.atmosphereclass == null || body.atmosphereclass == AtmosphereClass.None ) ) ||
                            c == "Any" ||
                            c == body.atmosphereclass.edname ) )
                {
                    return true;
                }
                log += $"REJECT. Atmosphere class: {( body.atmosphereclass ?? AtmosphereClass.None )?.edname} not in {string.Join( ",", checkAtmosphereClasses )}.";
                return false;
            }

            return true;
        }

        private bool TryCheckVolcanism(ICollection<string> checkVolcanismCompositions, ref string log )
        {
            // Check if body has appropriate volcanism
            if ( checkVolcanismCompositions.Count > 0 )
            {
                if ( checkVolcanismCompositions.Any( c => 
                        ( ( c == "None" || c == string.Empty ) && body.volcanism == null ) ||
                            c == "Any" ||
                            c == body.volcanism?.edComposition ) )
                {
                    return true;
                }
                log += $"REJECT. Volcanism composition: {body.volcanism?.edComposition} not in {string.Join( ",", checkVolcanismCompositions )}.";
                return false;
            }

            return true;
        }

        // Check the amount, composition and type of volcanism
        //private bool TryCheckVolcanismAdvanced(IList<Volcanism> checkVolcanismCompositions, ref string log )
        //{
        //    // Check if body has appropriate volcanism
        //    if ( checkVolcanismCompositions.Count > 0 )
        //    {
        //        if ( checkVolcanismCompositions.Any( c => ( ( c == null ) || c == body.volcanism ) ) )
        //        {
        //            return true;
        //        }
        //        if(body.volcanism is null) {
        //            log += $"REJECT. Volcanism composition: 'None' not in [{String.Join("; ", checkVolcanismCompositions.Select(s => s.ToString()).ToList().ToArray()  )}].";
        //        }
        //        else {
        //            log += $"REJECT. Volcanism composition: '{body.volcanism?.ToString()}' not in [{String.Join("; ", checkVolcanismCompositions.Select(s => s.ToString()).ToList().ToArray()  )}].";
        //        }
        //        return false;
        //    }

        //    return true;
        //}
        private bool TryCheckVolcanismAdvanced(IList<string> checkVolcanismCompositions, ref string log )
        {
            // Check if body has appropriate volcanism
            if ( checkVolcanismCompositions.Count > 0 )
            {
                foreach(var composition in checkVolcanismCompositions) {
                    Volcanism volcanism = Volcanism.FromName(composition);

                    if( (volcanism is null && body.volcanism is null) || volcanism == body.volcanism) {
                        return true;
                    }
                }

                if(body.volcanism is null) {
                    log += $"REJECT. Volcanism composition: 'None' not in [{String.Join("; ", checkVolcanismCompositions)}].";
                }
                else {
                    log += $"REJECT. Volcanism composition: '{body.volcanism?.ToString()}' not in [{String.Join("; ", checkVolcanismCompositions)}].";
                }
                return false;
            }

            return true;
        }

        private bool TryCheckPrimaryStar ( ICollection<string> checkStarClasses, ref string log )
        {
            // Check if body has appropriate parent star
            if ( checkStarClasses.Count > 0 )
            {
                //_currentSystem.bodies.Where(x=>x.type=
                List<Body> systemBodies = _currentSystem.bodies.ToList();
                
                
                //BodyWithID

                if ( checkStarClasses.Any(s => s == parentStar.stellarclass) )
                {
                    return true;
                }
                log += $"REJECT. Parent star {parentStar?.stellarclass} not in {string.Join( ",", checkStarClasses )}.";
                return false;
            }

            return true;
        }

        private bool TryCheckLocalStar ( ICollection<string> checkStarClasses, ref string log )
        {
            // TODO:2212_bt - Possible future implementation

            //// Check if body has appropriate parent star
            //if ( checkStarClasses.Count > 0 )
            //{
            //    //_currentSystem.bodies.Where(x=>x.type=
            //    List<Body> systemBodies = _currentSystem.bodies.ToList();
                
                
            //    //BodyWithID

            //    if ( checkStarClasses.Any(s => s == parentStar.stellarclass) )
            //    {
            //        return true;
            //    }
            //    log += $"REJECT. Parent star {parentStar?.stellarclass} not in {string.Join( ",", checkStarClasses )}.";
            //    return false;
            //}

            return true;
        }

        private bool TryCheckSpecialSpecies ( OrganicSpecies species, ref string log )
        {
            // TODO: Implement special case predictions where possible

            // Brain Trees
            //  - Near system with guardian structures
            if ( species.genus == OrganicGenus.Brancae )
            { }

            // Electricae radialem:
            //  - Near nebula (how close is near?)
            if ( species.genus == OrganicGenus.Electricae )
            { }

            // Crystalline Shards:
            //  - Must be >12000 Ls from nearest star.
            if ( species.genus == OrganicGenus.GroundStructIce )
            { }

            // Bark Mounds
            //  - Seems to always have 3 geologicals
            //  - Should be within 150Ly from a nebula
            if ( species.genus == OrganicGenus.Cone )
            {
                if ( body.surfaceSignals.reportedGeologicalCount < 3 )
                {
                    log += $"REJECT. Body geological count: {body.surfaceSignals.reportedGeologicalCount} < 3.";
                    return false;
                }
            }

            return true;
        }

        private bool TryCheckSpecialVariants ( OrganicVariant variant, ref string log )
        {
            // TODO: Implement special case predictions where possible

            // Brain Trees
            //  - Near system with guardian structures
            if ( variant.genus == OrganicGenus.Brancae )
            { }

            // Electricae radialem:
            //  - Near nebula (how close is near?)
            if ( variant.genus == OrganicGenus.Electricae )
            { }

            // Crystalline Shards:
            //  - Must be >12000 Ls from nearest star.
            if ( variant.genus == OrganicGenus.GroundStructIce )
            { }

            // Bark Mounds
            //  - Seems to always have 3 geologicals
            //  - Should be within 150Ly from a nebula
            if ( variant.genus == OrganicGenus.Cone )
            {
                if ( body.surfaceSignals.reportedGeologicalCount < 3 )
                {
                    log += $"REJECT. Body geological count: {body.surfaceSignals.reportedGeologicalCount} < 3.";
                    return false;
                }
            }

            return true;
        }
    }
}
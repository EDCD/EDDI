using EddiConfigService.Configurations;
using EddiDataDefinitions;
using JetBrains.Annotations;
using System.Collections.Generic;
using System;
using System.Linq;
using Utilities;

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

                if ( TryCheckGravity( species.maxG, ref log ) && 
                     TryCheckTemperature( species.minK, species.maxK, ref log ) && 
                     TryCheckPlanetClass( species.planetClass, ref log ) && 
                     TryCheckAtmosphere( species.atmosphereClass, ref log ) && 
                     TryCheckVolcanism( species.volcanism, ref log ) && 
                     TryCheckStar( species.starClass, ref log ) && 
                     TryCheckSpecialSpecies( species, ref log ) )
                {
                    log += "OK";
                    predictedSpecies.Add( species );
                }

                Logging.Debug( log );
            }

            // Return a list of only the unique genus' found
            return predictedSpecies.Select(s => s.genus).Distinct().ToHashSet();
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

        private bool TryCheckGravity ( decimal? maxG, ref string log )
        {
            // Check if body meets max gravity requirements
            // maxG: Maximum gravity
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
            String log = "";
            bool enableLog = false;

            if ( enableLog )
            { log += $"[Predictions] Body '{body.bodyname}'\r\n"; }

            // Create temporary list of ALL species possible
            var listPredicted = new List<OrganicSpecies>();

            // Iterate though species
            foreach ( var species in OrganicSpecies.AllOfThem )
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

        private bool TryCheckStar ( ICollection<string> checkStarClasses, ref string log )
        {
            // Check if body has appropriate parent star
            if ( checkStarClasses.Count > 0 )
            {
                if ( checkStarClasses.Any(s => s == parentStar.stellarclass) )
                {
                    return true;
                }
                log += $"REJECT. Parent star {parentStar?.stellarclass} not in {string.Join( ",", checkStarClasses )}.";
                return false;
            }

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
    }
}

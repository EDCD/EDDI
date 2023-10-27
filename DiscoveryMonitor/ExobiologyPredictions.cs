﻿using EddiConfigService.Configurations;
using EddiDataDefinitions;
using JetBrains.Annotations;
using System.Collections.Generic;
using System;
using System.Linq;
using Utilities;
using System.ServiceModel.Security;
using MathNet.Numerics.Distributions;
using System.Runtime.ExceptionServices;
using System.Windows.Media;

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
        //public HashSet<OrganicGenus> PredictBySpecies ()
        //{
        //    Logging.Debug( $"Generating predictions by species for {body.bodyname} in {_currentSystem.systemname}.");

        //    // Create temporary list of ALL species possible
        //    var predictedSpecies = new List<OrganicSpecies>();

        //    // Iterate though species
        //    foreach ( var species in OrganicSpecies.AllOfThem )
        //    {
        //        var log = $"Checking species {species.edname} (genus: {species.genus}): ";

        //        if ( !species.isPredictable )
        //        {
        //            log += "SKIP. No known criteria.";
        //            Logging.Debug( log );
        //            continue;
        //        }

        //        if ( !TryCheckConfiguration( species.genus, ref log ) )
        //        {
        //            Logging.Debug( log );
        //            continue;
        //        }

        //        if ( TryCheckGravity( 0, species.maxG, ref log ) && 
        //             TryCheckTemperature( species.minK, species.maxK, ref log ) && 
        //             TryCheckPlanetClass( species.planetClass, ref log ) && 
        //             TryCheckAtmosphere( species.atmosphereClass, ref log ) && 
        //             TryCheckVolcanism( species.volcanism, ref log ) && 
        //             TryCheckPrimaryStar( species.starClass, ref log ) && 
        //             TryCheckSpecialSpecies( species, ref log ) )
        //        {
        //            log += "OK";
        //            predictedSpecies.Add( species );
        //        }

        //        Logging.Debug( log );
        //    }

        //    // Create a distinct genus list
        //    List<OrganicGenus> listGenus = predictedSpecies.Select(s => s.genus).Distinct().ToList();

        //    // Iterate over all predicted species, set the min/max values for the genus list from all predicted species
        //    for ( int i = 0; i < listGenus.Count(); i++ )
        //    {
        //        foreach ( var species in predictedSpecies )
        //        {
        //            if ( listGenus[ i ].edname == species.genus.edname )
        //            {
        //                // Set initial value
        //                if ( listGenus[ i ].predictedMinimumValue == 0 )
        //                {
        //                    listGenus[ i ].predictedMinimumValue = species.value;
        //                }

        //                if ( listGenus[ i ].predictedMaximumValue == 0 )
        //                {
        //                    listGenus[ i ].predictedMaximumValue = species.value;
        //                }

        //                // If new minimum detected, overwrite old
        //                if ( species.value < listGenus[ i ].predictedMinimumValue )
        //                {
        //                    listGenus[ i ].predictedMinimumValue = species.value;
        //                }

        //                // If new maximum detected, overwrite old
        //                if ( species.value > listGenus[ i ].predictedMaximumValue )
        //                {
        //                    listGenus[ i ].predictedMaximumValue = species.value;
        //                }
        //            }
        //        }
        //    }

        //    // Return a list of only the unique genus' found
        //    //return predictedSpecies.Select(s => s.genus).Distinct().ToHashSet();
        //    return listGenus.ToHashSet();
        //}

        /// <summary>
        /// This currently works and provides fairly accurate predictions
        /// </summary>
        public HashSet<OrganicGenus> PredictByVariant ()
        {
            Logging.Debug( $"Generating predictions by variant for {body.bodyname} in {_currentSystem.systemname}.");

            var log = "";

            // Create temporary list of ALL variant possible
            var predictedVariants = new List<OrganicVariant>();

            // Iterate though variant
            foreach ( var variant in OrganicVariant.AllOfThem )
            {
                log = $"Checking variant {variant.edname} (genus: {variant.genus}): ";

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

                // TODO:2212_bt - This needs some tuning, there are some special conditions that include StarClass and Luminosity which will skip the normal StarClass logic below
                // specialConditions contains a list separated by ";" which will contain groups matching the predictionType (i.e. predictionType="StarClass,StarLuminosty" => specialConditions="B,I;B,II;B,III")
                // Both conditions in a group must be matched for successful prediction
                //var predictionType = variant.predictionType;
                //bool predictParentStar = predictionType.Count()==1 && predictionType[0] == "StarClass";
                //bool predictMaterials = predictionType.Count()==1 && predictionType[0] == "Material";
                //bool predictSpecial = !predictParentStar && !predictMaterials && predictionType.Count()>0;

                //log += $"VARIANT SPECIALS=[{string.Join(";", variant.specialConditions)}] ";

                if ( TryCheckGravity( variant.minG, variant.maxG, ref log ) && 
                     TryCheckTemperature( variant.minK, variant.maxK, ref log ) && 
                     TryCheckPressure( variant.minP, variant.maxP, ref log ) && 
                     TryCheckPlanetClass( variant.planetClass, ref log ) && 
                     TryCheckAtmosphere( variant.atmosphereClass, ref log ) && 
                     TryCheckAtmosphereComposition( variant.atmosphereComposition, ref log ) &&
                     TryCheckVolcanismAdvanced( variant.volcanism, ref log ) && 
                     TryCheckPrimaryStar( variant.primaryStar, ref log ) &&
                     TryCheckMaterials( variant.materials, ref log ) &&
                     TryCheckBodyTypePresent( variant.systemBodies, ref log ) &&
                     TryCheckNebulaDistance( variant.nebulaDistance, ref log ) &&
                     TryCheckDistanceFromArrival( variant.distanceFromArrival, ref log ) &&
                     TryCheckGeologyNum( variant.geologicalsPresent, ref log ) &&
                     TryCheckRegion( variant.regions, ref log ) )
                {
                    log += "OK";
                    predictedVariants.Add( variant );
                }

                Logging.Debug( log );
            }

            // Create a distinct genus list
            List<OrganicGenus> listGenus = predictedVariants.Select(s => s.genus).Distinct().ToList();

            if ( listGenus.Count()==0 )
            {
                log += $"No predictions were made, adding Unknown.";
                listGenus.Add( OrganicGenus.Unknown );
                listGenus[0].predictedMinimumValue = 0;
                listGenus[0].predictedMaximumValue = 0;
            }
            else
            {
                log = $"Setting Min/Max values:\r\n";

                // Iterate over all predicted variants, set the min/max values for the genus list
                for ( int i = 0; i < listGenus.Count(); i++ )
                {
                    log += $"\t[{listGenus[ i ].edname}]\r\n";
                    foreach ( var variant in predictedVariants )
                    {
                        if ( listGenus[ i ].edname == variant.genus.edname )
                        {
                            log += $"\t\t{variant.edname} ";
                            var species = OrganicSpecies.FromEDName( variant.species.edname );
                            if(species != null) {
                                //listGenus[ i ].predictedMinimumValue = (listGenus[ i ].predictedMinimumValue==0 || species.value < listGenus[ i ].predictedMinimumValue) ? species.value : listGenus[ i ].predictedMinimumValue;
                                //listGenus[ i ].predictedMaximumValue = (listGenus[ i ].predictedMaximumValue==0 || species.value > listGenus[ i ].predictedMaximumValue) ? species.value : listGenus[ i ].predictedMaximumValue;

                                if(listGenus[ i ].predictedMinimumValue == 0 || species.value < listGenus[ i ].predictedMinimumValue) {
                                    listGenus[ i ].predictedMinimumValue = species.value;
                                }

                                if(listGenus[ i ].predictedMaximumValue == 0 || species.value > listGenus[ i ].predictedMaximumValue) {
                                    listGenus[ i ].predictedMaximumValue = species.value;
                                }

                                log += $": value={species.value}, predictedMinimum={listGenus[ i ].predictedMinimumValue}, predictedMaximum={listGenus[ i ].predictedMaximumValue}\r\n";
                            }
                        }
                    }
                }
            }

            Logging.Debug( log );

            // Return an ordered list of only the unique genus' found
            //return predictedSpecies.Select(s => s.genus).Distinct().ToHashSet();
            return listGenus.OrderBy(o => o.invariantName).ToHashSet();
        }

        private bool TryCheckConfiguration ( OrganicGenus genus, ref string log )
        {
            // Check if species should be ignored per configuration settings
            try
            {
                if ( ( configuration.exobiology.predictions.skipGroundStructIce && genus == OrganicGenus.GroundStructIce ) ||
                     ( configuration.exobiology.predictions.skipBrancae && genus == OrganicGenus.Brancae ) ||
                     ( configuration.exobiology.predictions.skipCone && genus == OrganicGenus.Cone ) ||
                     ( configuration.exobiology.predictions.skipTubers && genus == OrganicGenus.Tubers ) ||
                     ( configuration.exobiology.predictions.skipAleoids && genus == OrganicGenus.Aleoids ) ||
                     ( configuration.exobiology.predictions.skipVents && genus == OrganicGenus.Vents ) ||
                     ( configuration.exobiology.predictions.skipSphere && genus == OrganicGenus.Sphere ) ||
                     ( configuration.exobiology.predictions.skipBacterial && genus == OrganicGenus.Bacterial ) ||
                     ( configuration.exobiology.predictions.skipCactoid && genus == OrganicGenus.Cactoid ) ||
                     ( configuration.exobiology.predictions.skipClypeus && genus == OrganicGenus.Clypeus ) ||
                     ( configuration.exobiology.predictions.skipConchas && genus == OrganicGenus.Conchas ) ||
                     ( configuration.exobiology.predictions.skipElectricae && genus == OrganicGenus.Electricae ) ||
                     ( configuration.exobiology.predictions.skipFonticulus && genus == OrganicGenus.Fonticulus ) ||
                     ( configuration.exobiology.predictions.skipShrubs && genus == OrganicGenus.Shrubs ) ||
                     ( configuration.exobiology.predictions.skipFumerolas && genus == OrganicGenus.Fumerolas ) ||
                     ( configuration.exobiology.predictions.skipFungoids && genus == OrganicGenus.Fungoids ) ||
                     ( configuration.exobiology.predictions.skipOsseus && genus == OrganicGenus.Osseus ) ||
                     ( configuration.exobiology.predictions.skipRecepta && genus == OrganicGenus.Recepta ) ||
                     ( configuration.exobiology.predictions.skipStratum && genus == OrganicGenus.Stratum ) ||
                     ( configuration.exobiology.predictions.skipTubus && genus == OrganicGenus.Tubus ) ||
                     ( configuration.exobiology.predictions.skipTussocks && genus == OrganicGenus.Tussocks ) )
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

        private bool TryCheckRegion(ICollection<string> checkRegions, ref string log )
        {
            if (checkRegions.Count() > 0)
            {
                var currentRegion = Utilities.RegionMap.RegionMap.FindRegion((double)_currentSystem.x, (double)_currentSystem.y, (double)_currentSystem.z);
                if (checkRegions.Any( a => a == currentRegion.name ) )
                {
                    log += $"ACCEPT. '{currentRegion.name}' is in '{string.Join(",", checkRegions)}'. ";
                    return true;
                }
                log += $"REJECT. Region: '{currentRegion.name}' not in '{string.Join(",", checkRegions)}'";
                return false;
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

        private bool TryCheckAtmosphereComposition(ICollection<string> checkAtmosphereCompositions, ref string log )
        {
            // Check if body has appropriate astmosphere
            if ( checkAtmosphereCompositions.Count > 0 )
            {
                foreach(var checkAtmosphereGroup in checkAtmosphereCompositions)
                {
                    var checkParts = checkAtmosphereGroup.Split( ',' );

                    if( checkParts.Count() >= 1 )
                    {
                        // Check composition
                        if( body.atmospherecompositions.Any( x => x.edname == checkParts[0] ) )
                        { 
                            return true;
                        }
                    }
                    else if(checkParts.Count() >= 2 ) {
                        // Check composition and amount
                        if (Decimal.TryParse( checkParts[1], out decimal checkPercent ))
                        {
                            if( body.atmospherecompositions.Any( x=> x.edname == checkParts[0] && x.percent >= checkPercent ) )
                            { 
                                return true;
                            }
                        }
                    }
                }
                log += $"REJECT. Atmosphere composition: {string.Join(";", body.atmospherecompositions.Select( x => string.Join(",", (new { x.edname, x.percent })) ).ToList()) } not in {string.Join( ";", checkAtmosphereCompositions )}.";
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

        private bool TryCheckVolcanismAdvanced(IList<string> checkVolcanismCompositions, ref string log )
        {
            // Check if body has appropriate volcanism
            if ( checkVolcanismCompositions.Count > 0 )
            {
                foreach(var composition in checkVolcanismCompositions) {
                    Volcanism volcanism = Volcanism.FromName(composition);

                    if( ( composition=="Any" && body.volcanism != null) || (volcanism is null && body.volcanism is null) || volcanism == body.volcanism) {
                        return true;
                    }
                }

                log += $"REJECT. Volcanism composition: '{(body.volcanism is null ? "None" : body.volcanism?.ToString())}' not in [{String.Join("; ", checkVolcanismCompositions)}].";
                return false;
            }

            return true;
        }

        //private bool TryCheckPrimaryStar ( ICollection<string> checkStarClasses, ref string log )
        //{
        //    // Check if body has appropriate parent star
        //    if ( checkStarClasses.Count > 0 )
        //    {
        //        HashSet<Body> parentStars = new HashSet<Body>();
        //        var result = _currentSystem.TryGetParentStars( body.bodyId, out parentStars );

        //        foreach ( var parentStar in parentStars ) {
        //            if ( checkStarClasses.Any( c => c == parentStar.starClass.edname ) )
        //            {
        //                return true;
        //            }
        //        }
        //        log += $"REJECT. Parent star [{string.Join(",", parentStars)}] not in [{string.Join( ",", checkStarClasses )}].";

        //        //List<string> checkedStarClass = new List<string>();
        //        //foreach ( var parent in body.parents )
        //        //{
        //        //    // TODO:2212_bt - Currently assuming if there is a star
        //        //    // If there is a star lets use that, otherwise use barycentre
        //        //    if ( parent.ContainsKey( BodyType.Star.edname ) ) {
        //        //        if ( checkStarClasses.Any( c => c == _currentSystem.BodyWithID( parent["Star"] ).starClass.edname ) )
        //        //        {
        //        //            return true;
        //        //        }
        //        //        checkedStarClass.Add( _currentSystem.BodyWithID( parent["Star"] ).starClass.edname );
        //        //    }
        //        //    else if ( parent.ContainsKey("Null") ) {
        //        //        foreach ( var key in parent.Keys ) {
        //        //            if ( key == BodyType.Barycenter.edname ) // Parent is a barycentre, check barycentre children
        //        //            {
        //        //                foreach ( var bodyId in _currentSystem.GetChildBodyIDs( parent[ key ] )
        //        //                             .Where( bodyId => _currentSystem.BodyWithID( bodyId )?.bodyType == BodyType.Star ) )
        //        //                {
        //        //                    if ( checkStarClasses.Any( c => c == _currentSystem.BodyWithID( bodyId ).starClass.edname ) )
        //        //                    {
        //        //                        return true;
        //        //                    }
        //        //                    checkedStarClass.Add(_currentSystem.BodyWithID( bodyId ).starClass.edname);
        //        //                }
        //        //            }
        //        //        }
        //        //    }
        //        //}

        //        //log += $"REJECT. Parent star [{string.Join(",", checkedStarClass)}] not in [{string.Join( ",", checkStarClasses )}].";
        //        return false;
        //    }

        //    return true;
        //}

        private bool TryCheckPrimaryStar ( ICollection<string> checkStar, ref string log )
        {
            if(checkStar.Count() > 0 ) {

                HashSet<Body> parentStars = new HashSet<Body>();
                var result = _currentSystem.TryGetParentStars( body.bodyId, out parentStars );
                
                log += $"(CHECK STAR: '{string.Join(";", parentStars.Select( w => string.Join(",", (new { w.starClass.edname, w.luminosityclass }) ) ) )}')\r\n";

                if(parentStars.Count()>0) {
                    foreach( var starGroup in checkStar) {
                        IList<string> starParts = starGroup.Split( ',' ).ToList();

                        log += $"\t[starParts={string.Join(",",starParts)}]\r\n";

                        foreach ( var parentStar in parentStars ) {
                            if ( starParts[0] == parentStar.starClass.edname )
                            {
                                log += $"\t\tClass => {starParts[0]}=={parentStar.starClass.edname}, ";
                                if(starParts.Count >= 2) {
                                    if ( parentStar.luminosityclass.Contains(starParts[1]) ) {
                                        log += $"Luminosity => {starParts[1]} ? {parentStar.luminosityclass}, ";
                                        return true;
                                    }
                                }
                                else {
                                    log += $"Luminosity => SKIP, ";
                                    return true;
                                }
                                log += "\r\n";
                            }
                        }
                    }
                }
                else
                {
                    // Failed to get parent stars, return True as this check isn't valid anymore
                    log += $"FAILED. Did not get any parent stars, pass by default. ";
                    return true;
                }

                log += $"REJECT. Parent star/luminosity [{string.Join(",", parentStars.Select( x => x.starClass.edname ) ) }] not in {string.Join(";", checkStar)}.";
                return false;
            }
            return true;
        }

        private bool TryCheckPrimaryStarClass ( string checkStarClass, ref string log )
        {
            // Check if body has appropriate parent star
            if ( checkStarClass != null && checkStarClass != "" )
            {
                HashSet<Body> parentStars = new HashSet<Body>();
                var result = _currentSystem.TryGetParentStars( body.bodyId, out parentStars );

                foreach ( var parentStar in parentStars ) {
                    if ( checkStarClass == parentStar.starClass.edname )
                    {
                        return true;
                    }
                }
                log += $"REJECT. Parent star [{string.Join(",", parentStars.Select( x => x.starClass.edname ) ) }] not in {checkStarClass}.";

                return false;
            }

            return true;
        }

        private bool TryCheckPrimaryStarLuminosity ( string checkStarLuminosity, ref string log )
        {
            // Check if body has appropriate parent star
            if ( checkStarLuminosity != null && checkStarLuminosity != "" )
            {
                HashSet<Body> parentStars = new HashSet<Body>();
                var result = _currentSystem.TryGetParentStars( body.bodyId, out parentStars );

                foreach ( var parentStar in parentStars ) {
                    if ( parentStar.luminosityclass.Contains(checkStarLuminosity) )
                    {
                        return true;
                    }
                }
                log += $"REJECT. Parent star luminosity [{string.Join(",", parentStars.Select(x => x.luminosity))}] not in {checkStarLuminosity}.";

                return false;
            }

            return true;
        }

        private bool TryCheckBodyTypePresent ( ICollection<string> checkBodyTypes, ref string log )
        {
            if ( checkBodyTypes.Count() > 0 )
            {
                foreach( var body in _currentSystem.bodies ) {
                    if(body != null && checkBodyTypes.Any( s => s == body.planetClass.edname ) ) {
                        return true;
                    }
                }
                log += $"REJECT. Body with type present [{string.Join(",", _currentSystem.bodies.Select( x => x.planetClass.edname ) ) }] not in {string.Join( ",", checkBodyTypes) }.";

                return false;
            }

            return true;
        }

        private bool TryCheckMaterials ( ICollection<string> checkMaterials, ref string log )
        {
            // Check if body has appropriate rare materials
            if ( checkMaterials.Count > 0 )
            {
                var bodyMaterials = body.materials.Select(x => x.name ).ToList();
                foreach(var mat in bodyMaterials) {
                    if(checkMaterials.Any( s => s == mat)) {
                        return true;
                    }
                }
                log += $"REJECT. Material [{string.Join( ",", body.materials.Select(x => x.name).ToList())}] not in {string.Join( ",", checkMaterials )}.";
                return false;
            }

            return true;
        }

        private bool TryCheckMaterial ( string checkMaterial, ref string log )
        {
            // Check if body has appropriate rare materials
            if ( checkMaterial != null && checkMaterial != "" )
            {
                if(body.materials.Any(x => x.name == checkMaterial && x.percentage>0)) {
                    return true;
                }
                log += $"REJECT. Material [{string.Join( ",", body.materials.Select(x => x.name).ToList())}] not in {string.Join( ",", checkMaterial )}.";
                return false;
            }
            return true;
        }

        private bool TryCheckGeologyNum ( decimal? checkGeologyNum, ref string log )
        {
            // Check if body has appropriate rare materials
            if ( checkGeologyNum != null && checkGeologyNum != 0 )
            {               
                if( body.surfaceSignals.reportedGeologicalCount >= checkGeologyNum )
                {
                    return true;
                }

                log += $"REJECT. Geology number present {body.surfaceSignals.reportedGeologicalCount} < {checkGeologyNum}.";
                return false;
            }
            return true;
        }

        private bool TryCheckNebulaDistance ( decimal? checkNebulaDistance, ref string log )
        {
            if( checkNebulaDistance != null && checkNebulaDistance != 0 ) {
                var nearestNebula = Nebula.TryGetNearestNebula( _currentSystem );
                if (nearestNebula != null) {
                    if ( nearestNebula.distance < checkNebulaDistance ) {
                        return true;
                    }
                }
                log += $"REJECT. Nebula distance [{(nearestNebula is null ? "Null" : nearestNebula.name)} @ {(nearestNebula is null ? "Null" : nearestNebula.distance.ToString())} Ly] > {checkNebulaDistance}.";
                return false;
            }

            return true;
        }

        private bool TryCheckDistanceFromArrival ( decimal? checkDistanceFromArrival, ref string log )
        {
            if( checkDistanceFromArrival != null && checkDistanceFromArrival != 0 ) {
                if( body.distance >= checkDistanceFromArrival ) {
                    return true;
                }
                log += $"REJECT. Distance from arrival [{body.distance}] < {checkDistanceFromArrival}.";
                return false;
            }
            return true;
        }

        //private bool TryCheckLocalStar ( ICollection<string> checkStarClasses, ref string log )
        //{
        //    // TODO:2212_bt - Possible future implementation, unknown if this would provide a benefit yet
        //    return true;
        //}

        //private bool TryCheckSpecialSpecies ( OrganicSpecies species, ref string log )
        //{
        //    // TODO: Implement special case predictions where possible

        //    // Brain Trees
        //    //  - Near system with guardian structures
        //    if ( species.genus == OrganicGenus.Brancae )
        //    { }

        //    // Electricae radialem:
        //    //  - Near nebula (how close is near?)
        //    if ( species.genus == OrganicGenus.Electricae )
        //    { }

        //    // Crystalline Shards:
        //    //  - Must be >12000 Ls from nearest star.
        //    if ( species.genus == OrganicGenus.GroundStructIce )
        //    { }

        //    // Bark Mounds
        //    //  - Seems to always have 3 geologicals
        //    //  - Should be within 150Ly from a nebula
        //    if ( species.genus == OrganicGenus.Cone )
        //    {
        //        if ( body.surfaceSignals.reportedGeologicalCount < 3 )
        //        {
        //            log += $"REJECT. Body geological count: {body.surfaceSignals.reportedGeologicalCount} < 3.";
        //            return false;
        //        }
        //    }

        //    return true;
        //}

        //private bool TryCheckSpecialVariants ( OrganicVariant variant, ref string log )
        //{
        //    // TODO: Implement special case predictions
        //    // Special case predictions, these can vary and have multiple conditions so we need some extra logic here
        //    //              None - No special conditions
        //    //         StarClass - Required parent star must be present (Seems to be more restrictive than biostats would suggest)
        //    //    StarLuminosity - Parent star must have this luminosity
        //    //          Material - This rare material must be present
        //    //   BodyTypePresent - Body types must be present in system (hard to predict if the system hasn't been fully scanned)
        //    //        NearNebula - Must be within the distance (Ly) from nebula. Is this even possible to detect?
        //    //  DistanceFromStar - Body must be this distance (Ls) from the nearest star
        //    //        GeologyNum - Must have this amount of geological signals present

        //    log += $"(SPECIAL: {string.Join( ";", variant.specialConditions)} [{variant.specialConditions.Count()}]) ";

        //    if(variant.specialConditions.Count() > 0 ) {

        //        // i.e. Loop over groups ["B","I"],["B","II"],["B","III"]
        //        for(int r=0; r<variant.specialConditions.Count(); r++) {

        //            // i.e. Create list ["B"],["I"]
        //            IList<string> partConditions = variant.specialConditions[r].Split( ',' ).ToList();

        //            // The size of the group and the size of the predictions types should match
        //            if( partConditions.Count == variant.predictionType.Count() ) {

        //                int predictionPartsPassed = 0;

        //                log += $"[Special Conditions] ";

        //                // i.e. Loop over ["StarClass","StarLuminosity"]
        //                for(int s=0; s<variant.predictionType.Count(); s++) {
        //                    var predictionType = variant.predictionType[s];

        //                    // Do Checks
        //                    if( predictionType == "StarClass") {
        //                        predictionPartsPassed += TryCheckPrimaryStar( partConditions[s], ref log ) ? 1 : 0;
        //                    }
        //                    else if ( predictionType == "StarLuminosity" ) {
        //                        predictionPartsPassed += TryCheckPrimaryStarLuminosity( partConditions[s], ref log ) ? 1 : 0;
        //                    }
        //                    else if ( predictionType == "Material" ) {
        //                        predictionPartsPassed += TryCheckMaterial( partConditions[s], ref log ) ? 1 : 0;
        //                    }
        //                    else if ( predictionType == "BodyTypePresent" ) {
        //                        // TODO: This is limited, can only check bodies that have actually been scanned in system
        //                        predictionPartsPassed += TryCheckBodyTypePresent( partConditions[s].Split( ',' ).ToList(), ref log ) ? 1 : 0;
        //                    }
        //                    else if ( predictionType == "GeologyNum" ) {
        //                        // TODO: This may not be simple to determine, possibly only need to check the parent star of the body?
        //                        predictionPartsPassed += TryCheckGeologyNum( partConditions[s], ref log ) ? 1 : 0;
        //                    }
        //                    else if ( predictionType == "DistanceFromStar" ) {
        //                        // TODO: This may not be simple to determine, possibly only need to check the parent star of the body?
        //                        predictionPartsPassed += 1;
        //                    }
        //                    else if ( predictionType == "NearNebula" ) {
        //                        // TODO: Is this detectable? Just pass it for now.
        //                        predictionPartsPassed += 1;
        //                    }
        //                    else {
        //                        // Unknown condition, reject it so we can see it in logs.
        //                        //predictionPartsPassed += 1;
        //                        log += $"REJECT. Unknown prediction type: {predictionType}.";
        //                    }
        //                }

        //                // If the number of passed prediction parts matches the number of parts in the group, then it was successful
        //                if(predictionPartsPassed == partConditions.Count() ) {
        //                    return true;
        //                }
        //            }
        //        }

        //        //log += $"REJECT. Special conditions: prediction types='{string.Join( ",", variant.predictionType)}', prediction conditions='{string.Join( ",", variant.specialConditions)}.'";
        //        return false;
        //    }


        //    //// Brain Trees
        //    ////  - Near system with guardian structures
        //    //if ( variant.genus == OrganicGenus.Brancae )
        //    //{ }

        //    //// Electricae radialem:
        //    ////  - Near nebula (how close is near?)
        //    //if ( variant.genus == OrganicGenus.Electricae )
        //    //{ }

        //    //// Crystalline Shards:
        //    ////  - Must be >12000 Ls from nearest star.
        //    //if ( variant.genus == OrganicGenus.GroundStructIce )
        //    //{ }

        //    //// Bark Mounds
        //    ////  - Seems to always have 3 geologicals
        //    ////  - Should be within 150Ly from a nebula
        //    //if ( variant.genus == OrganicGenus.Cone )
        //    //{
        //    //    if ( body.surfaceSignals.reportedGeologicalCount < 3 )
        //    //    {
        //    //        log += $"REJECT. Body geological count: {body.surfaceSignals.reportedGeologicalCount} < 3.";
        //    //        return false;
        //    //    }
        //    //}

        //    return true;
        //}
    }
}
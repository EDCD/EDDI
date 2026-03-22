using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class SurfaceSignalsEvent (
        DateTime timestamp,
        string detectionType,
        ulong? systemAddress,
        string bodyName,
        long bodyId,
        List<SignalAmount> surfaceSignals,
        List<OrganicGenus> genera )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Surface signals detected";
        public const string DESCRIPTION = "Triggered when surface signal sources are detected";
        public static readonly string[] SAMPLES =
        [
            @"{ ""timestamp"":""2019-08-19T00:36:40Z"", ""event"":""SAASignalsFound"", ""BodyName"":""Oponner 6 a"", ""SystemAddress"":3721345878371, ""BodyID"":30, ""Signals"":[ { ""Type"":""$SAA_SignalType_Geological;"", ""Type_Localised"":""Geological"", ""Count"":48 } ] }",
            @"{ ""timestamp"":""2025-05-26T05:23:29Z"", ""event"":""SAASignalsFound"", ""BodyName"":""Col 69 Sector TP-E c12-4 B 11"", ""SystemAddress"":1184102126066, ""BodyID"":25, ""Signals"":[ { ""Type"":""$SAA_SignalType_Biological;"", ""Type_Localised"":""Biological"", ""Count"":1 } ], ""Genuses"":[ { ""Genus"":""$Codex_Ent_Bacterial_Genus_Name;"", ""Genus_Localised"":""Bacterium"" } ] }",
            @"{ ""timestamp"":""2022-12-08T04:32:21Z"", ""event"":""SAASignalsFound"", ""BodyName"":""Gurabru 4 a"", ""SystemAddress"":2553098013019, ""BodyID"":18, ""Signals"":[ { ""Type"":""$SAA_SignalType_Biological;"", ""Type_Localised"":""Biological"", ""Count"":8 } ], ""Genuses"":[ { ""Genus"":""$Codex_Ent_Bacterial_Genus_Name;"", ""Genus_Localised"":""Bacterium"" }, { ""Genus"":""$Codex_Ent_Cactoid_Genus_Name;"", ""Genus_Localised"":""Cactoida"" }, { ""Genus"":""$Codex_Ent_Clypeus_Genus_Name;"", ""Genus_Localised"":""Clypeus"" }, { ""Genus"":""$Codex_Ent_Conchas_Genus_Name;"", ""Genus_Localised"":""Concha"" }, { ""Genus"":""$Codex_Ent_Fungoids_Genus_Name;"", ""Genus_Localised"":""Fungoida"" }, { ""Genus"":""$Codex_Ent_Osseus_Genus_Name;"", ""Genus_Localised"":""Osseus"" }, { ""Genus"":""$Codex_Ent_Shrubs_Genus_Name;"", ""Genus_Localised"":""Frutexa"" }, { ""Genus"":""$Codex_Ent_Tussocks_Genus_Name;"", ""Genus_Localised"":""Tussock"" } ] }"
        ];

        [PublicAPI("The signal detection type (either 'FSS' or 'SAA'")]
        public string detectionType { get; private set; } = detectionType;

        [PublicAPI("The body where surface signals were detected")]
        public string bodyname { get; private set; } = bodyName;

        [PublicAPI( "The numeric ID of the body where surface signals were detected" )]
        public long bodyId { get; private set; } = bodyId;

        [PublicAPI( "The numeric system address of the star system containing the surface signal" )]
        public ulong? systemAddress { get; private set; } = systemAddress;

        [PublicAPI("A list of signals (as objects)")]
        public List<SignalAmount> surfacesignals { get; private set; } = surfaceSignals ?? [ ];

        [PublicAPI( "A list of the genus of each detected biological signal (as objects, when biological signals are detected after mapping the surface)" )]
        public List<OrganicGenus> genera { get; private set; } = genera ?? [ ];

        public static bool Handle ( DateTime timestamp, string edType, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            var systemAddress = JsonParsing.getULong(data, "SystemAddress");
            var bodyName = JsonParsing.getString(data, "BodyName");
            var bodyId = JsonParsing.getLong(data, "BodyID");

            data.TryGetValue( "Signals", out var signalsVal );
            if ( signalsVal == null ) { return false; }

            if ( edType.Contains( "SAA", StringComparison.OrdinalIgnoreCase ) )
            {
                if ( bodyName.EndsWith( " Ring" ) )
                {
                    // This is the mining hotspots from a ring that we've mapped
                    var hotspots = new List<CommodityAmount>();
                    foreach ( var signal in ( (List<object>)signalsVal ).Cast<IDictionary<string, object>>() )
                    {
                        var commodityEdName = JsonParsing.getString( signal, "Type" );
                        var type = CommodityDefinition.FromEDName( commodityEdName ) ??
                                   throw new ArgumentException( $@"Unknown ring signal type: {commodityEdName}",
                                       nameof(commodityEdName) );
                        type.fallbackLocalizedName = JsonParsing.getString( signal, "Type_Localised" );
                        var amount = JsonParsing.getInt( signal, "Count" );
                        hotspots.Add( new CommodityAmount( type, amount ) );
                    }
                    hotspots = hotspots.OrderByDescending( h => h.amount ).ToList();
                    events.Add( new RingHotspotsEvent( timestamp, systemAddress, bodyName, bodyId, hotspots )
                    {
                        raw = line, fromLoad = fromLogLoad
                    } );
                }
                else
                {
                    // This is surface signal sources from a body that we've mapped
                    var surfaceSignals = new List<SignalAmount>();
                    foreach ( var signal in ( (List<object>)signalsVal ).Cast<IDictionary<string, object>>() )
                    {
                        var signalSource = JsonParsing.getString( signal, "Type" );
                        var source = SignalSource.FromEDName( signalSource ) ?? new SignalSource();
                        var localizedName = JsonParsing.getString( data, "Type_Localised" );
                        if ( !string.IsNullOrEmpty( localizedName ) && !localizedName.Contains( '$' ) )
                        {
                            source.fallbackLocalizedName = localizedName;
                        }

                        var amount = JsonParsing.getInt( signal, "Count" );
                        surfaceSignals.Add( new SignalAmount( source, amount ) );
                    }

                    surfaceSignals = surfaceSignals.OrderByDescending( s => s.amount ).ToList();
                    
                    var genera = new List<OrganicGenus>();
                    data.TryGetValue( "Genuses", out var genusesVal );
                    if ( genusesVal != null )
                    {
                        foreach ( var genusVal in ( (List<object>)genusesVal ).Cast<IDictionary<string, object>>() )
                        {
                            var genusEdName = JsonParsing.getString( genusVal, "Genus" );
                            var genus = OrganicGenus.FromEDName( genusEdName );
                            if ( genus != null )
                            {
                                genus.fallbackLocalizedName = JsonParsing.getString( genusVal, "Genus_Localised" );
                                genera.Add( genus );
                            }
                        }
                    }
                    events.Add( new SurfaceSignalsEvent( timestamp, "SAA", systemAddress, bodyName, bodyId,
                        surfaceSignals, genera ) { raw = line, fromLoad = fromLogLoad } );
                }

                return true;
            }

            if ( edType.Contains( "FSS", StringComparison.OrdinalIgnoreCase ) )
            {
                // These are surface signal sources from a body that we've scanned
                var surfaceSignals = new List<SignalAmount>();
                foreach ( var signal in ( (List<object>)signalsVal ).Cast<IDictionary<string, object>>() )
                {
                    var signalSource = JsonParsing.getString(signal, "Type");
                    var source = SignalSource.FromEDName(signalSource) ?? new SignalSource();
                    var localizedName = JsonParsing.getString(data, "Type_Localised");
                    if ( !string.IsNullOrEmpty( localizedName ) && !localizedName.Contains( '$' ) )
                    {
                        source.fallbackLocalizedName = localizedName;
                    }
                    var amount = JsonParsing.getInt(signal, "Count");
                    surfaceSignals.Add( new SignalAmount( source, amount ) );
                }
                surfaceSignals = surfaceSignals.OrderByDescending( s => s.amount ).ToList();
                events.Add( new SurfaceSignalsEvent( timestamp, "FSS", systemAddress, bodyName, bodyId, surfaceSignals, null ) { raw = line, fromLoad = fromLogLoad } );
                return true;
            }

            return false;
        }
    }
}

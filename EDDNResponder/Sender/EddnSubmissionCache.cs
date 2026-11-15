using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;

namespace EddiEddnResponder.Sender
{
    internal sealed class EddnSubmissionCache ( TimeProvider clock )
    {
        private const int capacity = 4096;
        private static readonly TimeSpan retention = TimeSpan.FromMinutes( 10 );
        private readonly object cacheLock = new();
        private readonly Dictionary<string, Reservation> entries = new( StringComparer.Ordinal );
        private readonly LinkedList<string> completed = new();

        internal sealed class Reservation ( string fingerprint )
        {
            internal readonly string Fingerprint = fingerprint;
            internal long CompletedAt;
        }

        internal bool TryReserve ( string fingerprint, out Reservation reservation )
        {
            lock ( cacheLock )
            {
                var now = clock.GetTimestamp();
                while ( completed.First != null && clock.GetElapsedTime( entries[completed.First.Value].CompletedAt, now ) >= retention )
                {
                    RemoveOldestCompleted();
                }
                reservation = null;
                if ( entries.ContainsKey( fingerprint ) ) { return false; }
                if ( entries.Count == capacity )
                {
                    // Never evict an in-flight reservation or discard a unique observation.
                    // If all slots are pending, send without tracking this submission.
                    if ( completed.First == null ) { return true; }
                    RemoveOldestCompleted();
                }
                reservation = new Reservation( fingerprint );
                entries.Add( fingerprint, reservation );
                return true;
            }
        }

        internal void Complete ( Reservation reservation, bool accepted )
        {
            if ( reservation == null ) { return; }
            lock ( cacheLock )
            {
                if ( accepted )
                {
                    reservation.CompletedAt = clock.GetTimestamp();
                    completed.AddLast( reservation.Fingerprint );
                }
                else { entries.Remove( reservation.Fingerprint ); }
            }
        }

        private void RemoveOldestCompleted ()
        {
            entries.Remove( completed.First.Value );
            completed.RemoveFirst();
        }

        internal static string Fingerprint ( string json )
        {
            // Hash a canonical copy of the final serialized envelope. The original bytes
            // remain untouched for signing and transport. Numeric tokens retain precision.
            using var document = JsonDocument.Parse( json );
            using var stream = new MemoryStream();
            using var writer = new Utf8JsonWriter( stream );
            WriteCanonical( document.RootElement, writer );
            writer.Flush();
            return Convert.ToHexString( SHA256.HashData( stream.ToArray() ) );
        }

        private static void WriteCanonical ( JsonElement element, Utf8JsonWriter writer )
        {
            switch ( element.ValueKind )
            {
                case JsonValueKind.Object:
                    writer.WriteStartObject();
                    foreach ( var property in element.EnumerateObject().OrderBy( p => p.Name, StringComparer.Ordinal ) )
                    {
                        writer.WritePropertyName( property.Name );
                        WriteCanonical( property.Value, writer );
                    }
                    writer.WriteEndObject();
                    break;
                case JsonValueKind.Array:
                    writer.WriteStartArray();
                    foreach ( var item in element.EnumerateArray() ) { WriteCanonical( item, writer ); }
                    writer.WriteEndArray();
                    break;
                default:
                    element.WriteTo( writer );
                    break;
            }
        }
    }
}

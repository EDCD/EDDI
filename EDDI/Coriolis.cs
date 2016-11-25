using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;
using Utilities;

namespace Eddi
{
    public class Coriolis
    {
        public static string ShipUri(Ship ship)
        {
            // Generate a Coriolis import URI to retain as much information as possible
            string uri = "https://coriolis.edcd.io/import?";

            // Take the ship's JSON, gzip it, then turn it in to base64 and attach it to the base uri

            var bytes = Encoding.UTF8.GetBytes(ship.json);
            using (var streamIn = new MemoryStream(bytes))
            using (var streamOut = new MemoryStream())
            {
                using (var gzipStream = new GZipStream(streamOut, CompressionLevel.Optimal, true))
                {
                    streamIn.CopyTo(gzipStream);
                }
                uri += "data=" + Uri.EscapeDataString(Convert.ToBase64String(streamOut.ToArray()));
            }

            // Add the ship's name
            string bn;
            if (ship.name == null)
            {
                bn = ship.role + " " + ship.model;
            }
            else
            {
                bn = ship.name;
            }
            uri += "&bn=" + Uri.EscapeDataString(bn);

            return uri;
        }
    }
}

using System;
using System.Net;
using System.Text.Json;

namespace Utilities
{
    namespace RegionMap
    {
        public class SystemData
        {
            public string Name { get; set; }
            public long ID64 { get; set; }
            public double? X { get; set; }
            public double? Y { get; set; }
            public double? Z { get; set; }
            public Region Region { get; set; }
            public SystemBoxel Boxel { get; set; }
        }

        public class Region
        {
            public int id { get; set; }
            public string name { get; set; }
        }

        public class SystemBoxel
        {
            public double X { get; set; }
            public double Y { get; set; }
            public double Z { get; set; }
            public Region Region { get; set; }
        }

        public static partial class RegionMap
        {
            private class EDSMSystem
            {
                public string name { get; set; }
                public long id64 { get; set; }
                public EDSMSystemCoords coords { get; set; }
            }

            private class EDSMSystemCoords
            {
                public double x { get; set; }
                public double y { get; set; }
                public double z { get; set; }
            }

            private const double x0 = -49985;
            private const double y0 = -40985;
            private const double z0 = -24105;

            public static Region FindRegion(double x, double y, double z)
            {
                var px = (int)((x - x0) * 83 / 4096);
                var pz = (int)((z - z0) * 83 / 4096);

                if (px < 0 || pz < 0 || pz >= RegionMapLines.Length)
                {
                    return default;
                }
                else
                {
                    var row = RegionMapLines[pz];
                    var rx = 0;
                    var pv = 0;

                    foreach (var (rl, rv) in row)
                    {
                        if (px < rx + rl)
                        {
                            pv = rv;
                            break;
                        }
                        else
                        {
                            rx += rl;
                        }
                    }

                    if (pv == 0)
                    {
                        return null;
                    }
                    else
                    {
                        return new Region { id = pv, name = RegionNames[pv] };
                    }
                }
            }

            public static SystemBoxel FindRegionForBoxel(long id64)
            {
                int masscode = (int)(id64 & 7);
                double x = (((id64 >> (30 - masscode * 2)) & (0x3FFF >> masscode)) << masscode) * 10 + x0;
                double y = (((id64 >> (17 - masscode)) & (0x1FFF >> masscode)) << masscode) * 10 + y0;
                double z = (((id64 >> 3) & (0x3FFF >> masscode)) << masscode) * 10 + z0;

                return new SystemBoxel
                {
                    X = x,
                    Y = y,
                    Z = z,
                    Region = FindRegion(x, y, z)
                };
            }

            public static SystemData[] FindRegionsForSystems(string sysname)
            {
                var url = $"https://www.edsm.net/api-v1/systems?systemName={Uri.EscapeDataString(sysname)}&coords=1&showId=1";
                var client = new WebClient();
                var jsonstr = client.DownloadString(url);
                var systems = JsonSerializer.Deserialize<EDSMSystem[]>(jsonstr);
                var sysregions = new SystemData[systems.Length];

                for (int i = 0; i < systems.Length; i++)
                {
                    var system = systems[i];

                    var sysdata = sysregions[i] = new SystemData
                    {
                        Name = system.name,
                        ID64 = system.id64
                    };

                    if (system.coords != null)
                    {
                        double x = system.coords.x;
                        double y = system.coords.y;
                        double z = system.coords.z;
                        sysdata.X = x;
                        sysdata.Y = y;
                        sysdata.Z = z;

                        sysdata.Region = FindRegion(x, y, z);
                    }

                    if (system.id64 != 0)
                    {
                        sysdata.Boxel = FindRegionForBoxel((long)system.id64);
                    }
                }

                return sysregions;
            }

            //private static void Main(string[] args)
            //{
            //    if (args.Length == 0)
            //    {
            //        var exe = System.Reflection.Assembly.GetExecutingAssembly().Location;
            //        Console.WriteLine($"Usage: {exe} \"System 1\" [...]");
            //        return;
            //    }

            //    foreach (var name in args)
            //    {
            //        foreach (var sysdata in FindRegionsForSystems(name))
            //        {
            //            if (sysdata.X != null)
            //            {
            //                var x = sysdata.X;
            //                var y = sysdata.Y;
            //                var z = sysdata.Z;

            //                if (sysdata.Region != null)
            //                {
            //                    Console.WriteLine($"System {sysdata.Name} at ({x},{y},{z}) is in region {sysdata.Region.Id} ({sysdata.Region.Name})");
            //                }
            //                else
            //                {
            //                    Console.WriteLine($"System {sysdata.Name} at ({x},{y},{z}) is outside the region map");
            //                }
            //            }

            //            if (sysdata.Boxel != null && sysdata.Boxel.Region.Id != sysdata.Region?.Id)
            //            {

            //                var boxel = sysdata.Boxel;
            //                var x = boxel.X;
            //                var y = boxel.Y;
            //                var z = boxel.Z;

            //                if (boxel.Region != null)
            //                {
            //                    Console.WriteLine($"Boxel of system {sysdata.Name} at ({x},{y},{z}) is in region {boxel.Region.Id} ({boxel.Region.Name})");
            //                }
            //                else
            //                {
            //                    Console.WriteLine($"Boxel of system {sysdata.Name} at ({x},{y},{z}) is outside the region map");
            //                }
            //            }
            //        }
            //    }
            //}
        }
    }
}
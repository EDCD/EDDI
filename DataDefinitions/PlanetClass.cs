using MathNet.Numerics.Distributions;
using Newtonsoft.Json;

namespace EddiDataDefinitions
{
    /// <summary> Details of a planet class </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class PlanetClass : ResourceBasedLocalizedEDName<PlanetClass>
    {
        static PlanetClass()
        {
            resourceManager = Properties.PlanetClass.ResourceManager;
            resourceManager.IgnoreCase = true;
            missingEDNameHandler = (edname) => new PlanetClass(edname);

            None = new PlanetClass("None");
            var AmmoniaWorld = new PlanetClass("AmmoniaWorld", 0.3123M, new Normal(1.1548, 1.0934), new Normal(4.7429, 15.3999), new Normal(7706.3238, 4861.0153), new Normal(206.0490, 39.8921), new Normal(1160.4007, 8690.0557), new Normal(1878.7448, 4560.4068), new Normal(2.1193, 3.0631), new Normal(3.6437e-2, 7.1206e-2), new Normal(-0.1382, 17.0028), new Normal(179.1337, 104.2279), new Normal(25.4918, 57.9982), new Normal(4.1421e-4, 0.8974), new Normal(7028.1881, 1996.3085));
            var EarthLikeBody = new PlanetClass("EarthLikeBody", 0.2723M, new Normal(0.9354, 0.2605), new Normal(0.7597, 0.5583), new Normal(5329.7947, 1091.9602), new Normal(284.4655, 17.4888), new Normal(1.1569, 0.9930), new Normal(827.3997, 1819.1862), new Normal(1.3503, 1.4910), new Normal(2.1571e-2, 5.1353e-2), new Normal(8.7936e-2, 12.9121), new Normal(179.8770, 102.2282), new Normal(28.4548, 57.2558), new Normal(2.8918e-2, 0.9191), new Normal(8099.7210, 622.5465));
            var GasGiantWithAmmoniaBasedLife = new PlanetClass("GasGiantWithAmmoniaBasedLife", 0.5980M, new Normal(1.8417, 1.0997), new Normal(192.1256, 175.4923), new Normal(56160.6006, 18051.4844), new Normal(122.6228, 14.2031), null, new Normal(4042.2885, 17345.7750), new Normal(3.3352, 5.2861), new Normal(5.3972e-2, 0.1047), new Normal(0.1584, 27.0812), new Normal(178.6083, 103.9895), new Normal(34.0701, 94.3677), new Normal(1.9512e-3, 1.0046), new Normal(1553.5583, 846.2195));
            var GasGiantWithWaterBasedLife = new PlanetClass("GasGiantWithWaterBasedLife", 1.1251M, new Normal(3.4142, 1.8578), new Normal(448.8002, 296.6373), new Normal(65891.7804, 17114.6433), new Normal(190.6014, 28.3373), null, new Normal(3350.7423, 15246.0939), new Normal(2.9444, 5.4514), new Normal(6.3357e-2, 0.1191), new Normal(-0.0947, 30.8046), new Normal(178.9332, 104.0836), new Normal(31.4701, 102.4892), new Normal(5.6598e-4, 1.1232), new Normal(2337.7948, 1071.3133));
            var HeliumGasGiant = new PlanetClass("HeliumGasGiant", 2.1279e-5M, new Normal(8.1477, 9.6224), new Normal(912.0411, 1024.2987), new Normal(61732.7896, 15814.8905), new Normal(430.4601, 389.3979), null, new Normal(2328.6383, 3078.8603), new Normal(2.2791, 3.8520), new Normal(5.6486e-2, 8.8234e-2), new Normal(0.2306, 20.4621), new Normal(175.0387, 105.0612), new Normal(21.9514, 32.5545), new Normal(-0.1129, 1.1962), new Normal(5832.8341, 6835.2384));
            var HeliumRichGasGiant = new PlanetClass("HeliumRichGasGiant", 3.5832e-2M, new Normal(9.5467, 10.8359), new Normal(1046.4235, 1115.1350), new Normal(61605.7437, 15999.3785), new Normal(506.7451, 452.5697), null, new Normal(2070.4948, 8201.6187), new Normal(1.8815, 3.1391), new Normal(0.0565, 0.0882), new Normal(0.9270, 23.3570), new Normal(177.9651, 102.7341), new Normal(21.3246, 58.2273), new Normal(-2.8867e-2, 1.3379), new Normal(6922.1131, 7871.6406));
            var HighMetalContentBody = new PlanetClass("HighMetalContentBody", 26.9583M, new Normal(0.7574, 0.6654), new Normal(0.9283, 2.3641), new Normal(4118.6975, 2685.2019), new Normal(611.5611, 805.8881), new Normal(98439.8022, 1213363.0205), new Normal(1188.4574, 32511.4174), new Normal(1.0921, 3.3068), new Normal(2.6956e-2, 7.1737e-2), new Normal(-3.9191e-2, 17.5806), new Normal(179.2720, 103.6839), new Normal(21.3528, 50.9409), new Normal(4.5957e-3, 0.8601), new Normal(7787.9761, 1325.6113));
            var IcyBody = new PlanetClass("IcyBody", 38.0792M, new Normal(0.3387, 0.4522), new Normal(0.7870, 3.6518), new Normal(3880.4014, 3669.7774), new Normal(93.0446, 121.6459), new Normal(12387.9959, 188545.4555), new Normal(2616.6390, 104199.4671), new Normal(1.5656, 3.5784), new Normal(2.2709e-2, 5.5544e-2), new Normal(3.5528e-2, 20.8048), new Normal(179.2599, 103.7380), new Normal(18.2233, 48.3132), new Normal(-4.9764e-3, 0.8605), new Normal(3378.0653, 814.6928));
            var MetalRichBody = new PlanetClass("MetalRichBody", 1.7568M, new Normal(0.9331, 0.6868), new Normal(0.4478, 0.8256), new Normal(2810.8673, 1817.7660), new Normal(1436.8814, 877.4298), new Normal(359011.4422, 1807209.0895), new Normal(16.5832, 281.1093), new Normal(6.0549e-2, 0.2085), new Normal(1.6672e-2, 5.2818e-2), new Normal(-0.1688, 18.4677), new Normal(179.6440, 103.9150), new Normal(6.9544, 18.5970), new Normal(-1.2225e-2, 0.7308), new Normal(14454.0993, 1860.3522));
            var RockBody = new PlanetClass("RockyBody", 15.7792M, new Normal(0.1364, 9.7038e-2), new Normal(1.2838e-2, 0.1858), new Normal(1122.6465, 698.6448), new Normal(253.3024, 178.4054), new Normal(1451.6671, 253898.6847), new Normal(23.8259, 65.0141), new Normal(1.1409e-2, 2.1381e-2), new Normal(1.5040e-2, 3.8328e-2), new Normal(-1.7268e-2, 24.4822), new Normal(179.3849, 103.7902), new Normal(12.1285, 19.4956), new Normal(-6.1599e-3, 0.7723), new Normal(5565.0782, 210.4425));
            var RockyIceBody = new PlanetClass("RockyIceBody", 3.2579M, new Normal(0.5185, 0.3526), new Normal(0.5868, 1.1411), new Normal(4574.2366, 2601.4506), new Normal(203.7416, 247.6169), new Normal(13490.4823, 160859.3042), new Normal(1065.5715, 14209.6236), new Normal(1.2907, 2.0726), new Normal(2.1521e-2, 4.7924e-2), new Normal(-0.1311, 15.7283), new Normal(179.3857, 103.9038), new Normal(24.6987, 51.6531), new Normal(-7.8485e-3, 0.9007), new Normal(5033.4308, 813.2718));
            var SudarskyClassIGasGiant = new PlanetClass("SudarskyClassIGasGiant", 4.7711M, new Normal(1.3929, 0.8849), new Normal(115.3564, 138.7524), new Normal(47923.0688, 18380.2167), new Normal(84.5850, 33.5099), null, new Normal(7786.3556, 86914.7768), new Normal(4.5846, 8.1773), new Normal(5.9450e-2, 0.1266), new Normal(-0.2461, 34.1609), new Normal(179.0596, 103.7772), new Normal(30.7628, 4708.2685), new Normal(-3.2069e-3, 0.9477), new Normal(1458.3687, 898.4180));
            var SudarskyClassIIGasGiant = new PlanetClass("SudarskyClassIIGasGiant", 1.1360M, new Normal(3.4349, 1.8671), new Normal(452.0805, 298.6761), new Normal(65943.9968, 17196.3018), new Normal(190.7857, 28.2302), null, new Normal(1689.8706, 12254.3041), new Normal(2.9854, 5.6926), new Normal(0.0623, 0.1199), new Normal(0.0159, 30.2541), new Normal(178.2564, 103.8388), new Normal(17.4351, 68.8098), new Normal(-4.1174e-3, 1.1377), new Normal(2353.5006, 1078.4117));
            var SudarskyClassIIIGasGiant = new PlanetClass("SudarskyClassIIIGasGiant", 2.5678M, new Normal(9.5768, 5.9371), new Normal(1267.9623, 731.8796), new Normal(70988.4017, 12075.3685), new Normal(433.7274, 147.5358), null, new Normal(1689.8706, 12254.3041), new Normal(2.1318, 2.9985), new Normal(5.8732e-2, 9.6729e-2), new Normal(-4.0831e-2, 25.1673), new Normal(179.9623, 103.4193), new Normal(17.4351, 68.8098), new Normal(-6.4170e-3, 1.3505), new Normal(6195.0788, 3879.1195));
            var SudarskyClassIVGasGiant = new PlanetClass("SudarskyClassIVGasGiant", 0.4383M, new Normal(19.4542, 11.9882), new Normal(2208.4505, 1244.1274), new Normal(68025.3643, 8167.7542), new Normal(990.4996, 146.5534), null, new Normal(1222.1598, 10407.6930), new Normal(1.5819, 2.3800), new Normal(6.7363e-2, 0.1088), new Normal(0.5305, 32.9848), new Normal(179.6396, 104.0353), new Normal(8.8473, 99.2297), new Normal(-3.0639e-2, 1.3873), new Normal(13455.5229, 8608.7944));
            var SudarskyClassVGasGiant = new PlanetClass("SudarskyClassVGasGiant", 6.1378e-2M, new Normal(12.0933, 11.9283), new Normal(1406.1064, 1210.6903), new Normal(67135.6080, 8992.6644), new Normal(2052.1886, 892.2670), null, new Normal(334.0625, 22293.9979), new Normal(0.8220, 7.6364), new Normal(0.1033, 0.1534), new Normal(4.6149e-3, 62.3735), new Normal(178.5875, 104.2103), new Normal(9.7667, 28.8242), new Normal(2.5705e-2, 1.0688), new Normal(8312.1421, 9339.1802));
            var WaterGiant = new PlanetClass("WaterGiant", 8.1120e-2M, new Normal(5.3080, 3.3623), new Normal(65.8850, 58.4701), new Normal(21329.6452, 2370.0698), new Normal(183.6258, 71.5508), new Normal(29156216.3493, 102866064.8199), new Normal(6538.9388, 5126.1833), new Normal(6.4901, 5.0402), new Normal(0.0425, 0.0560), new Normal(-0.0509, 19.7915), new Normal(178.9997, 103.3792), new Normal(2.1756, 20.3291), new Normal(0.0579, 1.2211), new Normal(11041.4140, 5268.0982));
            var WaterWorld = new PlanetClass("WaterWorld", 2.7692M, new Normal(1.0149, 0.5509), new Normal(1.4536, 3.2840), new Normal(6060.1273, 2386.3639), new Normal(315.8040, 78.5399), new Normal(208.1560, 1156.7376), new Normal(996.7270, 3293.1695), new Normal(1.4441, 2.2203), new Normal(2.7974e-2, 7.1226e-2), new Normal(-1.6338, 18.5013), new Normal(178.8534, 103.8955), new Normal(28.7552, 65.6105), new Normal(6.4935e-3, 0.9339), new Normal(7748.9696, 1620.4547));
            var WaterGiantWithLife = new PlanetClass("WaterGiantWithLife", 0M); // This is described by the journal manual, but may not really exist.
        }

        public static readonly PlanetClass None;

        public decimal? percentage { get; private set; }

        public IUnivariateDistribution gravitydistribution { get; private set; }

        public IUnivariateDistribution massdistribution { get; private set; }

        public IUnivariateDistribution radiusdistribution { get; private set; }

        public IUnivariateDistribution tempdistribution { get; private set; }

        public IUnivariateDistribution pressuredistribution { get; private set; }

        public IUnivariateDistribution orbitalperioddistribution { get; private set; }

        public IUnivariateDistribution semimajoraxisdistribution { get; private set; }

        public IUnivariateDistribution eccentricitydistribution { get; private set; }

        public IUnivariateDistribution inclinationdistribution { get; private set; }

        public IUnivariateDistribution periapsisdistribution { get; private set; }

        public IUnivariateDistribution rotationalperioddistribution { get; private set; }

        public IUnivariateDistribution tiltdistribution { get; private set; }

        public IUnivariateDistribution densitydistribution { get; private set; }

        // dummy used to ensure that the static constructor has run
        public PlanetClass() : this("")
        { }

        private PlanetClass(string edname, decimal? percentage = null, IUnivariateDistribution gravitydistribution = null, IUnivariateDistribution massdistribution = null, IUnivariateDistribution radiusdistribution = null, IUnivariateDistribution tempdistribution = null, IUnivariateDistribution pressuredistribution = null, IUnivariateDistribution orbitalperioddistribution = null, IUnivariateDistribution semimajoraxisdistribution = null, IUnivariateDistribution eccentricitydistribution = null, IUnivariateDistribution inclinationdistribution = null, IUnivariateDistribution periapsisdistribution = null, IUnivariateDistribution rotationalperioddistribution = null, IUnivariateDistribution tiltdistribution = null, IUnivariateDistribution densitydistribution = null) : base(edname, edname)
        {
            this.percentage = percentage;
            this.gravitydistribution = gravitydistribution;
            this.massdistribution = massdistribution;
            this.radiusdistribution = radiusdistribution;
            this.tempdistribution = tempdistribution;
            this.pressuredistribution = pressuredistribution;
            this.orbitalperioddistribution = orbitalperioddistribution;
            this.semimajoraxisdistribution = semimajoraxisdistribution;
            this.eccentricitydistribution = eccentricitydistribution;
            this.inclinationdistribution = inclinationdistribution;
            this.periapsisdistribution = periapsisdistribution;
            this.rotationalperioddistribution = rotationalperioddistribution;
            this.tiltdistribution = tiltdistribution;
            this.densitydistribution = densitydistribution;
        }

        new public static PlanetClass FromEDName(string edname)
        {
            if (edname == null)
            {
                return null;
            }

            string normalizedEDName = edname.Replace(" ", "").Replace("-", "");
            return ResourceBasedLocalizedEDName<PlanetClass>.FromEDName(normalizedEDName);
        }
    }
}

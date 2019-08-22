using MathNet.Numerics.Distributions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiDataDefinitions
{
    /// <summary> Details of a star class </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class StarClass
    {
        static StarClass()
        {
            // Percentages are obtained from analysis of EDSM data dumps

            // Main sequence stars
            var O = new StarClass("O", "O", "blue", 0.1834M, new Normal(-8.4550, 2.1942), new Normal(27.1891, 14.7688), new Normal(20.2039, 43.1263), new Normal(41212.5620, 7125.1786), new Normal(18.2509, 84.8378), new Normal(134233.8846, 324608.1888), new Normal(51.5228, 99.9063), new Normal(0.1490, 0.1197), new Normal(-0.1118, 45.0957), new Normal(179.1174, 104.0169), new Normal(7.5061, 16.1527), new Normal(-2.0662e-2, 0.1472), new Normal(39683.5446, 5775322.0368));
            var B = new StarClass("B", "B", "blue_white", 1.4560M, new Normal(-1.2921, 1.9796), new Normal(5.6894, 5.8101), new Normal(5.2954, 29.7446), new Normal(15840.2149, 5121.1246), new Normal(199.3136, 219.3162), new Normal(344704.3158, 913919.3256), new Normal(42.4924, 110.6806), new Normal(0.1471, 0.1204), new Normal(-5.5085e-2, 44.7691), new Normal(179.6894, 103.6515), new Normal(1.8814, 10.9151), new Normal(-9.1644e-2, 1.1441), new Normal(189688.5973, 39409448.6809));
            var A = new StarClass("A", "A", "blue_white", 4.7231M, new Normal(2.6924, 0.5138), new Normal(1.8456, 3.4379), new Normal(2.6596, 22.0422), new Normal(8130.6223, 915.2419), new Normal(1503.7856, 842.0704), new Normal(667764.2729, 1407608.1673), new Normal(48.0823, 83.6455), new Normal(0.1500, 0.1200), new Normal(0.1771, 45.0112), new Normal(179.5838, 103.7883), new Normal(1.1867, 2.8746), null, new Normal(10216.6301, 3265921.9741));
            var G = new StarClass("G", "G", "yellow_white", 7.3987M, new Normal(5.1069, 0.4008), new Normal(0.9292, 0.1118), new Normal(0.9568, 0.1923), new Normal(5542.6589, 226.6528), new Normal(4486.2856, 3844.6356), new Normal(835803.0344, 1925153.7693), new Normal(54.9342, 97.6532), new Normal(0.1506, 0.1192), new Normal(0.1455, 45.1510), new Normal(178.9063, 104.0538), new Normal(3.8664, 2.0384), null, new Normal(2064.8964, 421.6909));
            var F = new StarClass("F", "F", "white", 9.5431M, new Normal(3.8300, 0.4698), new Normal(1.2901, 0.1773), new Normal(1.1714, 0.2568), new Normal(6707.5279, 430.4878), new Normal(2073.1217, 1575.9385), new Normal(754992.6894, 1616817.7040), new Normal(52.2959, 90.9813), new Normal(0.1514, 0.1200), new Normal(-0.3446, 45.1041), new Normal(179.2635, 103.6452), new Normal(3.2169, 1.6456), null, new Normal(1560.1034, 334.7799));
            var K = new StarClass("K", "K", "yellow_orange", 19.8426M, new Normal(6.5861, 0.6876), new Normal(0.6659, 0.1193), new Normal(0.7819, 0.5673), new Normal(4418.8893, 429.4767), new Normal(6125.2321, 4075.7881), new Normal(889094.0075, 2035903.0155), new Normal(59.0996, 109.6969), new Normal(0.1492, 0.1199), new Normal(-6.4607e-2, 45.1440), new Normal(179.6191, 103.9237), new Normal(3.0940, 1.1331), null, new Normal(2821.3429, 669.8912));
            var M = new StarClass("M", "M", "orange_red", 34.3706M, new Normal(9.5648, 1.0983), new Normal(0.3238, 0.0880), new Normal(0.4898, 8.8581e-2), new Normal(2798.9563, 465.8892), new Normal(6682.4941, 4038.6619), new Normal(1078299.6528, 2684999.8725), new Normal(62.3285, 122.0310), new Normal(0.1463, 0.1200), new Normal(4.3450e-2, 45.0180), new Normal(179.6047, 103.9123), new Normal(1.9912, 1.7259), null, new Normal(5484.0347, 1662.6711));

            // Brown dwarf stars
            var L = new StarClass("L", "L", null, 7.8959M, new Normal(12.9648, 0.8246), new Normal(0.1405, 3.0319e-2), new Normal(0.2917, 4.2993e-2), new Normal(1657.0901, 201.1004), new Normal(6888.3289, 4016.7241), new Normal(938213.2182, 2667563.6741), new Normal(71.0564, 144.4794), new Normal(0.1410, 0.1192), new Normal(-0.2232, 44.7961), new Normal(180.4346, 104.0666), new Normal(1.1707, 0.8863), null, new Normal(11107.8391, 2908.0800));
            var T = new StarClass("T", "T", null, 3.7150M, new Normal(16.004, 1.1360), new Normal(6.9330e-2, 1.7902e-2), new Normal(0.1870, 3.2881e-2), new Normal(1057.8342, 164.6561), new Normal(6867.6947, 4046.7556), new Normal(923899.9637, 2879264.1381), new Normal(67.6764, 148.2340), new Normal(0.1363, 0.1188), new Normal(0.1625, 44.4773), new Normal(179.9514, 104.0567), new Normal(0.7980, 4.6342), null, new Normal(21125.0376, 6690.6589));
            var Y = new StarClass("Y", "Y", null, 2.2311M, new Normal(21.6191, 1.7406), new Normal(1.8062e-2, 7.9157e-3), new Normal(7.9670e-2, 2.2312e-2), new Normal(454.5517, 121.0346), new Normal(4374.2114, 3916.8039), new Normal(30618.0910, 593634.7381), new Normal(5.6111, 34.0202), new Normal(5.4489e-2, 6.9789e-2), new Normal(3.9017e-2, 20.5480), new Normal(179.0760, 103.5028), new Normal(1.6996, 87.4808), new Normal(7.7326e-3, 1.7308), new Normal(74894.1913, 31745.0274));

            // White dwarf stars
            var DAO = new StarClass("DAO", "DAO", null, 0M);
            var DO = new StarClass("DO", "DO", null, 0M);
            var DOV = new StarClass("DOV", "DOV", null, 0M);
            var DX = new StarClass("DX", "DX", null, 0M);
            var DBZ = new StarClass("DBZ", "DBZ", null, 4.6212e-4M, new Normal(9.2289, 0.7910), new Normal(0.5156, 7.9121e-2), new Normal(1.3496e-2, 1.9557e-3), new Normal(18439.1493, 3144.4248), new Normal(7111.5522, 1479.7271), new Normal(1158971.8951, 2748448.6707), new Normal(25.3692, 46.3077), new Normal(0.1860, 0.1222), new Normal(7.6927, 50.6874), new Normal(167.8366, 111.8558), new Normal(0.9199, 0.3912), null, new Normal(456100655.1289, 216568472.7127));
            var DQ = new StarClass("DQ", "DQ", null, 7.8396e-5M, new Normal(12.8552, 4.0487), new Normal(0.9141, 0.3696), new Normal(8.7890e-3, 5.7166e-3), new Normal(12498.6667, 7309.7840), new Normal(9109.2222, 3694.9320), new Normal(955721.7409, 1067383.5630), new Normal(48.1855, 51.8881), new Normal(0.2024, 0.1090), new Normal(-8.5979, 41.5963), new Normal(160.1997, 95.9973), new Normal(1067383.5629, 0.4157), new Normal(488195744738.3169, 1782489111033.1870));
            var DBV = new StarClass("DBV", "DBV", null, 4.6707e-3M, new Normal(9.6290, 0.8904), new Normal(0.4789, 8.8277e-2), new Normal(1.4179e-2, 2.1681e-3), new Normal(16251.1878, 3060.9711), new Normal(9051.2665, 1996.0478), new Normal(1104002.7280, 2431844.1589), new Normal(51.2858, 84.2065), new Normal(0.1349, 0.1121), new Normal(3.8307, 45.7360), new Normal(179.1716, 104.6074), new Normal(0.9373, 0.4093), null, new Normal(382811019.4130, 234952364.7493));
            var DAZ = new StarClass("DAZ", "DAZ", null, 1.9475e-3M, new Normal(9.3204, 1.0121), new Normal(0.5204, 0.1084), new Normal(1.3358e-2, 2.1293e-3), new Normal(18139.2930, 3380.8947), new Normal(7162.5414, 1367.8960), new Normal(1137766.0793, 2359711.2237), new Normal(50.7803, 78.3046), new Normal(0.1488, 0.1232), new Normal(-3.0276, 41.9832), new Normal(176.2405, 101.2225), new Normal(1.0163, 0.5936), null, new Normal(714183796.6605, 3223923166.5304));
            var D = new StarClass("D", "D", null, 4.5140e-3M, new Normal(11.6698, 1.8483), new Normal(0.4859, 9.9120e-2), new Normal(1.4100e-2, 2.1892e-3), new Normal(11040.4501, 4634.5036), new Normal(10592.8930, 1972.0722), new Normal(1215541.5700, 2701452.2651), new Normal(49.1024, 87.2530), new Normal(0.1581, 0.1196), new Normal(2.5428, 47.1292), new Normal(176.5749, 101.4257), new Normal(0.9674, 0.3823), new Normal(6.2776e-2, 1.5753), new Normal(514131448.1576, 2811823682.7295));
            var DAV = new StarClass("DAV", "DAV", null, 1.4107e-2M, new Normal(9.6115, 0.8684), new Normal(0.4724, 8.6191e-2), new Normal(1.4356e-2, 2.2332e-3), new Normal(16439.4053, 3055.8190), new Normal(9118.5064, 1985.9490), new Normal(1110785.5855, 2559411.1899), new Normal(43.2329, 78.5159), new Normal(0.1484, 0.1199), new Normal(-8.8104e-2, 45.8943), new Normal(175.9063, 103.4433), new Normal(0.9355, 0.3916), new Normal(-2.2449, 3.6210e-3), new Normal(369264965.5077, 243511402.4034));
            var DCV = new StarClass("DCV", "DCV", null, 1.6702e-2M, new Normal(11.8480, 0.6247), new Normal(0.4895, 8.5065e-2), new Normal(1.4116e-2, 2.1187e-3), new Normal(9796.7916, 1141.6063), new Normal(10545.4430, 1397.5634), new Normal(1095292.4572, 2498911.7625), new Normal(43.7295, 79.2821), new Normal(0.1443, 0.1165), new Normal(-1.7272, 45.6747), new Normal(182.0218, 104.7879), new Normal(0.9476, 0.3974), new Normal(0.2337, 1.8311), new Normal(393456191.6603, 236134688.9734));
            var DAB = new StarClass("DAB", "DAB", null, 5.4828e-2M, new Normal(11.1990, 1.2322), new Normal(0.4852, 8.8866e-2), new Normal(1.4113e-2, 2.1137e-3), new Normal(12061.2517, 3439.9123), new Normal(10267.6623, 1816.7558), new Normal(1106033.9294, 2479477.9333), new Normal(50.9140, 88.4360), new Normal(0.1437, 0.1230), new Normal(-9.6205, 45.3002), new Normal(182.5184, 108.1781), new Normal(0.9431, 0.4029), null, new Normal(390415523.5837, 237931708.9277));
            var DB = new StarClass("DB", "DB", null, 2.2595e-2M, new Normal(9.6200, 0.8649), new Normal(0.4603, 8.5613e-2), new Normal(1.4333e-2, 2.0524e-3), new Normal(16440.3231, 3081.3114), new Normal(9198.5231, 1801.3424), new Normal(1159293.3442, 2545237.3765), new Normal(56.9400, 76.0696), new Normal(0.1121, 9.6670e-2), new Normal(-5.2186, 40.2297), new Normal(177.7932, 108.3856), new Normal(0.9500, 0.5422), null, new Normal(350312527.7430, 350312527.7430));
            var DA = new StarClass("DA", "DA", null, 0.1276M, new Normal(10.0551, 1.221), new Normal(0.4722, 8.5986e-2), new Normal(1.4261e-2, 2.1408e-3), new Normal(14969.1778, 3777.3808), new Normal(9554.1079, 1931.5951), new Normal(1125793.1358, 2520500.8386), new Normal(40.1752, 86.3788), null, null, null, new Normal(0.9419, 0.4366), null, new Normal(371487890.8157, 237774782.0029));
            var DC = new StarClass("DC", "DC", null, 0.1928M, new Normal(13.3273, 1.1815), new Normal(0.5056, 8.8179e-2), new Normal(1.3862e-2, 2.0229e-3), new Normal(7381.2066, 1842.5083), new Normal(11695.9151, 1248.2293), new Normal(1137943.8425, 2506046.0783), new Normal(48.0194, 88.8171), new Normal(0.1465, 0.1203), new Normal(0.3344, 44.0084), new Normal(183.0967, 102.6031), new Normal(0.9418, 0.4655), null, new Normal(424315218.5469, 244114720.47301));

            // Proto stars
            var AEBE = new StarClass("AEBE", "AEBE", null, 0.1163M, new Normal(9.5380, 1.2857), new Normal(8.9825, 11.4324), new Normal(0.1774, 0.1068), new Normal(4628.1680, 733.2804), null, new Normal(238455.4183, 598722.1378), new Normal(57.4783, 109.6160), new Normal(0.1608, 0.1240), new Normal(0.7267, 43.8504), new Normal(181.9092, 104.2027), new Normal(7.9475e-2, 6.4660e-2), null, new Normal(3397333.5146, 1266732.6165));
            var TTS = new StarClass("TTS", "TTS", null, 2.4201M, new Normal(12.4027, 5.6633), new Normal(0.3087, 0.3772), new Normal(0.4397, 0.3489), new Normal(2235.6449, 1799.1580), new Normal(88.7435, 63.1993), new Normal(552237.0510, 1973269.4252), new Normal(35.5394, 100.6776), new Normal(0.1047, 0.1071), new Normal(-0.2773, 34.7394), new Normal(179.3374, 103.6242), new Normal(2.0343, 10.0648), new Normal(1.8614e-3, 1.6576), new Normal(18592.2340, 22950.8921));

            // Giant stars
            var B_BlueWhiteSuperGiant = new StarClass("B_BlueWhiteSuperGiant", "B_BlueWhiteSuperGiant", "blue_white", 0M);
            var G_WhiteSuperGiant = new StarClass("G_WhiteSuperGiant", "G_WhiteSuperGiant", "yellow_white", 4.9307e-3M, new Normal(-3.0212, 0.9502), new Normal(2.7421, 0.9242), new Normal(41.6796, 21.8015), new Normal(5599.6067, 227.7230), new Normal(1281.3333, 1382.3582), new Normal(583797.4313, 1116559.9363), new Normal(55.0959, 85.3018), new Normal(0.1470, 0.1195), new Normal(-5.3075, 45.7029), new Normal(180.3232, 102.8751), new Normal(23.7534, 57.9751), null, new Normal(5.4819, 67.7240));
            var M_RedSuperGiant = new StarClass("M_RedSuperGiant", "M_RedSuperGiant", "red", 4.6459e-3M, new Normal(-2.5554, 0.6366), new Normal(2.1794, 1.6778), new Normal(75.3733, 155.7319), new Normal(4643.1662, 745.0649), new Normal(2621.7213, 2968.7888), new Normal(749591.2234, 1395061.6018), new Normal(49.4475, 80.7911), new Normal(0.1618, 0.1322), new Normal(-2.0251, 44.6415), new Normal(178.9133, 104.1287), new Normal(75.6222, 415.9329), new Normal(0.1061, 0.1048), new Normal(5.9106e-2, 3.9906e-2));
            var F_WhiteSuperGiant = new StarClass("F_WhiteSuperGiant", "F_WhiteSuperGiant", "white", 6.7792e-3M, new Normal(-3.7696, 1.4394), new Normal(3.4230, 1.4584), new Normal(45.7071, 38.2061), new Normal(6465.9952, 889.7577), new Normal(1780.2359, 3057.6187), new Normal(546269.4507, 1085797.9185), new Normal(59.5249, 93.1840), new Normal(0.1515, 0.1181), new Normal(-0.5819, 46.4306), new Normal(186.2737, 102.8597), new Normal(35.5550, 81.4572), new Normal(-0.4966, 1.0641), new Normal(3.3610, 19.8130));
            var K_OrangeGiant = new StarClass("K_OrangeGiant", "K_OrangeGiant", "yellow_orange", 6.4396e-2M, new Normal(-0.2758, 1.5275), new Normal(0.7694, 0.4340), new Normal(16.2841, 14.8475), new Normal(4223.4790, 325.3952), new Normal(7369.1095, 3313.3936), new Normal(975659.6948, 3065550.2861), null, null, null, null, new Normal(39.9008, 42.8250), null, new Normal(303.0301, 902.8853));
            var A_BlueWhiteSuperGiant = new StarClass("A_BlueWhiteSuperGiant", "A_BlueWhiteSuperGiant", "blue_white", 3.1226e-2M, new Normal(-9.1858, 4.4672), new Normal(37.7831, 30.1749), new Normal(231.7884, 193.1341), new Normal(16387.9706, 7462.0295), new Normal(580.7508, 714.7829), new Normal(210070.2103, 544613.7130), new Normal(49.0752, 93.4346), new Normal(0.1588, 0.1285), new Normal(0.8903, 46.1076), new Normal(181.8246, 99.8729), new Normal(86.5242, 69.6737), null, new Normal(153339.8823, 3408197.4065));
            var M_RedGiant = new StarClass("M_RedGiant", "M_RedGiant", "red", 0.1637M, new Normal(6.9335, 4.8211), new Normal(0.3664, 0.2577), new Normal(4.0907, 10.1551), new Normal(2429.3432, 230.5779), new Normal(7493.5638, 3924.2886), new Normal(1072124.3879, 2588950.7977), new Normal(70.7003, 134.0570), new Normal(0.1420, 0.1212), new Normal(1.5043, 44.3672), new Normal(180.2272, 104.7395), new Normal(42.4517, 64.9349), null, new Normal(4190.7421, 1555.3823));

            // Wolf-Rayet stars
            var W = new StarClass("W", "W", null, 3.1771e-4M, new Normal(-5.7707, 6.5439), new Normal(1.2379, 0.4688), new Normal(6.3622, 2.1490), new Normal(109647.8000, 51422.2262), new Normal(8158.3067, 3616.1604), new Normal(1088441.7564, 2013377.5842), new Normal(62.7814, 87.4087), new Normal(0.1681, 0.1192), new Normal(-8.7813, 44.9720), new Normal(193.1803, 102.3475), new Normal(17.3861, 12.8196), new Normal(5.0072e-2, 0.1489), new Normal(21.2786, 27.2465));
            var WO = new StarClass("WO", "WO", null, 3.0368e-2M, new Normal(-5.7012, 4.5963), new Normal(88.6796, 17.3809), new Normal(6.5232, 2.0170), new Normal(35732.1662, 18889.7295), null, new Normal(93211.1211, 195417.7830), null, null, null, null, new Normal(2.4226, 0.8050), null, new Normal(1191.7744, 1402.3804));
            var WN = new StarClass("WN", "WN", null, 9.6675e-3M, new Normal(-5.6953, 4.6679), new Normal(77.8179, 23.8955), new Normal(6.5185, 2.0472), new Normal(35197.0518, 19537.9707), new Normal(2679.3333, 4450.9920), new Normal(90586.5828, 202444.2051), new Normal(52.0221, 100.7424), new Normal(0.1536, 0.1197), new Normal(-3.4974, 44.5403), new Normal(180.0902, 105.6131), new Normal(2.4593, 1.1676), null, new Normal(1072.5309, 1330.4830));
            var WNC = new StarClass("WNC", "WNC", null, 1.0014e-2M, new Normal(-5.6767, 4.4937), new Normal(78.4792, 23.3926), new Normal(6.5661, 2.0409), new Normal(35889.3904, 19223.2174), null, new Normal(101661.9124, 234380.0432), new Normal(52.2781, 93.2270), new Normal(0.1499, 0.1181), new Normal(0.5864, 47.4476), new Normal(181.3716, 104.6943), new Normal(2.4668, 1.3747), null, new Normal(1046.4698, 1294.5276));
            var WC = new StarClass("WC", "WC", null, 1.3583e-2M, new Normal(-5.8631, 4.3224), new Normal(49.5434, 6.6576), new Normal(6.5385, 2.0138), new Normal(36451.9228, 19666.7883), new Normal(2351.0526, 4062.7411), new Normal(99724.5033, 232516.3450), new Normal(57.6766, 108.7744), new Normal(0.1490, 0.1206), new Normal(1.0021, 45.7077), new Normal(181.2468, 104.0646), new Normal(2.5201, 1.7086), null, new Normal(658.6533, 740.4629));

            // Late sequence stars
            var MS = new StarClass("MS", "MS", null, 3.5793e-2M, new Normal(0.1571, 0.4188), new Normal(1.0989, 0.1339), new Normal(31.0954, 1.4445), new Normal(3045.1761, 229.0416), new Normal(9544.9245, 2419.3438), new Normal(857421.8320, 1758463.5428), new Normal(48.8584, 81.6000), new Normal(0.1542, 0.1204), new Normal(-0.1486, 45.0807), new Normal(181.8764, 103.4754), new Normal(115.5589, 28.3344), null, new Normal(6.8523e-2, 1.4679e-3));
            var S = new StarClass("S", "S", null, 3.7539e-2M, new Normal(0.1585, 0.4353), new Normal(1.1030, 0.1408), new Normal(31.2911, 4.1332), new Normal(3046.8202, 246.0410), new Normal(9509.9362, 2470.5269), new Normal(891556.1526, 1809579.0565), new Normal(52.2572, 86.5575), new Normal(0.1569, 0.1224), new Normal(1.3237, 44.3526), new Normal(181.3828, 103.5767), new Normal(115.7976, 28.9487), null, new Normal(6.8355e-2, 3.3666e-3));

            // Carbon stars
            var CS = new StarClass("CS", "CS", null, 0M);
            var CH = new StarClass("CH", "CH", null, 0M);
            var CHd = new StarClass("CHd", "CHd", null, 0M);
            var CN = new StarClass("CN", "CN", null, 2.3824e-2M, new Normal(0.6428, 6.6840e-2), new Normal(0.9554, 1.6781e-2), new Normal(29.4276, 0.2187), new Normal(2793.0654, 32.9817), new Normal(12888.4661, 132.1427), new Normal(906408.8017, 1870324.5639), new Normal(51.1178, 88.9208), new Normal(0.1486, 0.1169), new Normal(-1.1287, 45.3480), new Normal(180.9031, 101.8753), new Normal(117.4473, 19.6791), null, new Normal(7.0527e-2, 3.3368e-4));
            var CJ = new StarClass("CJ", "CJ", null, 2.9873e-3M, new Normal(0.6452, 6.7451e-2), new Normal(0.9545, 1.7022e-2), new Normal(29.4164, 0.2218), new Normal(2792.5595, 32.5797), new Normal(12879.6973, 137.5649), new Normal(880840.7829, 1819463.2659), new Normal(52.2986, 87.1051), new Normal(0.1637, 0.1157), new Normal(1.4396, 44.1753), new Normal(178.1415, 99.5373), new Normal(116.6318, 18.7382), null, new Normal(7.0544e-2, 3.3807e-3));
            var C = new StarClass("C", "C", null, 6.7668e-4M, new Normal(0.7140, 2.0102), new Normal(1.2418, 0.5164), new Normal(75.2803, 42.6220), new Normal(2240.4675, 1050.9834), new Normal(11524.3506, 2253.7199), new Normal(1012280.8736, 1942698.5229), new Normal(52.6739, 87.6374), new Normal(0.1711, 0.1328), new Normal(0.6793, 41.9012), new Normal(175.0839, 117.1690), new Normal(147.6019, 110.3984), new Normal(-6.9215e-2, 1.4357e-2), new Normal(3.1400e-2, 6.3184e-2));

            // Compact stars
            var N = new StarClass("N", "N", null, 4.7256M, new Normal(4.8352, 0.4062), new Normal(0.8092, 0.4086), new Normal(1.6034, 1.7300), new Normal(5513871.4142, 8066331.3790), new Normal(6328.6593, 3804.3761), new Normal(1035553.3541, 2180569.5099), new Normal(49.8844, 87.0028), new Normal(0.1514, 0.1209), new Normal(0.3736, 44.8805), new Normal(179.6551, 103.6221), new Normal(195399.3299, 208382562.4237), null, new Normal(5.0945, 7.7931));
            var H = new StarClass("H", "H", null, 0.4975M, null, new Normal(7.7959, 9.4156), new Normal(3.3066e-5, 3.9937e-5), null, new Normal(6351.6006, 4723.0639), new Normal(355982.3056, 849188.4918), new Normal(40.0794, 84.7873), new Normal(0.1481, 0.1191), new Normal(-0.3157, 45.7025), new Normal(179.8994, 104.1987), new Normal(1.2929e-6, 9.6806e-7), null, new Normal(1.5814e18, 1.2975e18));
            var SuperMassiveBlackHole = new StarClass("SuperMassiveBlackHole", "SuperMassiveBlackHole", null, 0M);

            // Exotic stellar entities
            var X = new StarClass("X", "X", null, 0M);
            var RoguePlanet = new StarClass("RoguePlanet", "RoguePlanet", null, 0M);
            var Nebula = new StarClass("Nebula", "Nebula", null, 0M);
            var StellarRemnantNebula = new StarClass("StellarRemnantNebula", "StellarRemnantNebula", null, 0M);
        }

        // Scan habitable zone constants
        public const double maxHabitableTempKelvin = 315;
        public const double minHabitableTempKelvin = 223.15;

        public class Chromaticity : ResourceBasedLocalizedEDName<StarClass.Chromaticity>
        {
            static Chromaticity()
            {
                resourceManager = Properties.StarColors.ResourceManager;
                resourceManager.IgnoreCase = false;
                missingEDNameHandler = (edname) => new Chromaticity(edname);

                var blue = new Chromaticity("blue");
                var blue_white = new Chromaticity("blue_white");
                var orange_red = new Chromaticity("orange_red");
                var red = new Chromaticity("red");
                var yellow_orange = new Chromaticity("yellow_orange");
                var yellow_white = new Chromaticity("yellow_white");
                var white = new Chromaticity("white");
            }

            // dummy used to ensure that the static constructor has run
            public Chromaticity() : this("")
            { }

            public Chromaticity(string edname) : base(edname, edname)
            { }
        }

        private static readonly List<StarClass> CLASSES = new List<StarClass>();

        [JsonRequired]
        public string edname { get; private set; }

        public string name { get; private set; }

        public Chromaticity chromaticity { get; private set; } = new Chromaticity();

        public decimal? percentage { get; private set; }

        public IUnivariateDistribution absolutemagnitudedistribution { get; private set; }

        public IUnivariateDistribution massdistribution { get; private set; }

        public IUnivariateDistribution radiusdistribution { get; private set; }

        public IUnivariateDistribution tempdistribution { get; private set; }

        public IUnivariateDistribution agedistribution { get; private set; }

        public IUnivariateDistribution orbitalperioddistribution { get; private set; }

        public IUnivariateDistribution semimajoraxisdistribution { get; private set; }

        public IUnivariateDistribution eccentricitydistribution { get; private set; }

        public IUnivariateDistribution inclinationdistribution { get; private set; }

        public IUnivariateDistribution periapsisdistribution { get; private set; }

        public IUnivariateDistribution rotationalperioddistribution { get; private set; }

        public IUnivariateDistribution tiltdistribution { get; private set; }

        public IUnivariateDistribution densitydistribution { get; private set; }

        private StarClass(string edname, string name, string chromaticityEdName, decimal? percentage = null, IUnivariateDistribution absolutemagnitudedistribution = null, IUnivariateDistribution massdistribution = null, IUnivariateDistribution radiusdistribution = null, IUnivariateDistribution tempdistribution = null, IUnivariateDistribution agedistribution = null, IUnivariateDistribution orbitalperioddistribution = null, IUnivariateDistribution semimajoraxisdistribution = null, IUnivariateDistribution eccentricitydistribution = null, IUnivariateDistribution inclinationdistribution = null, IUnivariateDistribution periapsisdistribution = null, IUnivariateDistribution rotationalperioddistribution = null, IUnivariateDistribution tiltdistribution = null, IUnivariateDistribution densitydistribution = null)
        {
            this.edname = edname;
            this.name = name;
            this.percentage = percentage;
            this.absolutemagnitudedistribution = absolutemagnitudedistribution;
            this.massdistribution = massdistribution;
            this.radiusdistribution = radiusdistribution;
            this.tempdistribution = tempdistribution;
            this.agedistribution = agedistribution;
            this.orbitalperioddistribution = orbitalperioddistribution;
            this.semimajoraxisdistribution = semimajoraxisdistribution;
            this.eccentricitydistribution = eccentricitydistribution;
            this.inclinationdistribution = inclinationdistribution;
            this.periapsisdistribution = periapsisdistribution;
            this.rotationalperioddistribution = rotationalperioddistribution;
            this.tiltdistribution = tiltdistribution;
            this.densitydistribution = densitydistribution;
            this.chromaticity = Chromaticity.FromEDName(chromaticityEdName);

            CLASSES.Add(this);
        }

        public static StarClass FromName(string from)
        {
            return CLASSES.FirstOrDefault(v => v.name == from);
        }

        public static StarClass FromEDName(string from)
        {
            if (from == null)
            {
                return null;
            }
            return CLASSES.FirstOrDefault(v => v.edname == from);
        }

        /// <summary>
        /// Convert radius in m in to stellar radius
        /// </summary>
        public static decimal? solarradius(decimal? radiusKm)
        {
            return radiusKm == null ? null : radiusKm * 1000 / Constants.solarRadiusMeters;
        }

        /// <summary>
        /// Convert absolute magnitude in to luminosity
        /// </summary>
        public static decimal? luminosity(decimal? absoluteMagnitude)
        {
            return absoluteMagnitude == null ? null : (decimal?)Math.Pow(Math.Pow(100, 0.2), (Constants.solAbsoluteMagnitude - (double)absoluteMagnitude));
        }

        public static decimal temperature(decimal luminosity, decimal radius)
        {
            return (decimal)Math.Pow(((double)luminosity * Constants.solLuminosity) /
                (4 * Math.PI * Math.Pow((double)radius, 2) * Constants.stefanBoltzmann), 0.25);
        }

        public static decimal DistanceFromStarForTemperature(double targetTempKelvin, double stellarRadiusKilometers, double stellarTemperatureKelvin)
        {
            // Derived from Jackie Silver's Habitable Zone Calculator (https://forums.frontier.co.uk/showthread.php?t=127522&highlight=), used with permission
            double top = Math.Pow(stellarRadiusKilometers * 1000, 2.0) * Math.Pow(stellarTemperatureKelvin, 4.0);
            double bottom = 4.0 * Math.Pow(targetTempKelvin, 4.0);
            double distanceMeters = Math.Pow(top / bottom, 0.5);
            double distancels = (distanceMeters) / Constants.lightSpeedMetersPerSecond;
            return Convert.ToDecimal(distancels);
        }
    }
}

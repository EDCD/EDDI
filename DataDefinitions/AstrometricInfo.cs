using JetBrains.Annotations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading;
using System.Xml.Linq;
using Utilities;

namespace EddiDataDefinitions
{

    public class AstrometricData
    {
        public string name;
        public string description;

        public AstrometricData ()
        {
            this.name = "";
            this.description = "";
        }

        public AstrometricData ( string name, string desc )
        {
            this.name = name;
            this.description = desc;
        }
    }

    //public class AstrometricData : AstrometricData
    //{
    //    public Dictionary<string, AstrometricData> subType = new Dictionary<string, AstrometricData>();

    //    public AstrometricData ( string name, string desc )
    //    {
    //        this.name = name;
    //        this.description = desc;
    //    }

    //    public void add ( string name, string desc )
    //    {
    //        AstrometricData mydata = new AstrometricData(name, desc);
    //        this.subType.Add( name, mydata );
    //    }
    //}

    public class AstrometricInfo
    {
        private static Dictionary<string, AstrometricData> stars = new Dictionary<string, AstrometricData>();
        private static Dictionary<string, AstrometricData> gas = new Dictionary<string, AstrometricData>();
        private static Dictionary<string, AstrometricData> terrestrial = new Dictionary<string, AstrometricData>();

        //private static Dictionary<string, AstrometricData> gas = new Dictionary<string, AstrometricData>();
        //private static Dictionary<string, AstrometricData> terrestrial = new Dictionary<string, AstrometricData>();

        //private static OrganicGenus Aleoida           = new OrganicGenus("Aleoida",             150, "These are extremely hardy photosynthetic organisms that thrive in arid environments. " +
        //                                                                                                        "Thick, waxy leaf structures protect them from extreme surroundings. When " +
        //                                                                                                        "gaseous exchange becomes unfavourable.the leaves can completely shut off " +
        //                                                                                                        "the organism from the atmosphere causing a state of hibernation. The " +
        //                                                                                                        "pointed leaves create precipitation slopes, which draw liquids to the heart " +
        //                                                                                                        "of the organism.Here they are absorbed through a series of specialised cells, " +
        //                                                                                                        "and stored in the root structure until needed.");

        static AstrometricInfo ()
        {
            //stars.Add( "O", new AstrometricData( "O", "O Type Star", "O type stars are the most luminous and massive main sequence stars. They usually range from 15 to 90 solar masses, with a surface temperature reaching 52,000 Kelvin, so they appear very blue. They are very short lived with lifetimes of 1-10 million years, ending in a supernova.\r\n" ) );

            stars.Add( "Codex_Ent_O_Type_Name", new AstrometricData( "O Type Star", "O type stars are the most luminous and massive main sequence stars. They usually range from 15 to 90 solar masses, with a surface temperature reaching 52,000 Kelvin, so they appear very blue. They are very short lived with lifetimes of 1-10 million years, ending in a supernova." ) );
            stars.Add( "Codex_Ent_O_TypeGiant_Name", new AstrometricData( "O Type Giant", "O type stars are the most luminous and massive main sequence stars. They usually range from 15 to 90 solar masses, with a surface temperature reaching 52,000 Kelvin, so they appear very blue. They are very short lived with lifetimes of 1-10 million years, ending in a supernova.This particular star is a giant." ) );
            stars.Add( "Codex_Ent_O_TypeSuperGiant_Name", new AstrometricData( "O Type SuperGiant", "O type stars are the most luminous and massive main sequence stars. They usually range from 15 to 90 solar masses, with a surface temperature reaching 52,000 Kelvin, so they appear very blue. They are very short lived with lifetimes of 1-10 million years, ending in a supernova.This particular star is a supergiant." ) );
            stars.Add( "Codex_Ent_B_Type_Name", new AstrometricData( "B Type Star", "B type stars are very luminous blue-white stars. They usually range in mass from 2 to 16 solar masses and have a surface temperature reaching 30,000 Kelvin. Their lifetimes are shorter than most main sequence stars." ) );
            stars.Add( "Codex_Ent_B_TypeGiant_Name", new AstrometricData( "B Type Giant", "B type stars are very luminous blue-white stars. They usually range in mass from 2 to 16 solar masses and have a surface temperature reaching 30,000 Kelvin. Their lifetimes are shorter than most main sequence stars.This particular star is a giant." ) );
            stars.Add( "Codex_Ent_B_TypeSuperGiant_Name", new AstrometricData( "B Type SuperGiant", "B type stars are very luminous blue-white stars. They usually range in mass from 2 to 16 solar masses and have a surface temperature reaching 30,000 Kelvin. Their lifetimes are shorter than most main sequence stars.This particular star is a supergiant." ) );
            stars.Add( "Codex_Ent_A_Type_Name", new AstrometricData( "A Type Star", "A type stars are hot white or bluish-white main sequence stars. They usually range in mass from 1.4 to 2.1 solar masses and have a surface temperature reaching 10,000 Kelvin." ) );
            stars.Add( "Codex_Ent_A_TypeGiant_Name", new AstrometricData( "A Type Giant", "A type stars are hot white or bluish-white main sequence stars. They usually range in mass from 1.4 to 2.1 solar masses and have a surface temperature reaching 10,000 Kelvin.This particular star is a giant." ) );
            stars.Add( "Codex_Ent_A_TypeSuperGiant_Name", new AstrometricData( "A Type SuperGiant", "A type stars are hot white or bluish-white main sequence stars. They usually range in mass from 1.4 to 2.1 solar masses and have a surface temperature reaching 10,000 Kelvin.This particular star is a supergiant." ) );
            stars.Add( "Codex_Ent_F_Type_Name", new AstrometricData( "F Type Star", "F type stars are white main sequence stars. They usually range in mass from 1 to 1.4 solar masses and have a surface temperature reaching 7,600 Kelvin." ) );
            stars.Add( "Codex_Ent_F_TypeGiant_Name", new AstrometricData( "F Type Giant", "F type stars are white main sequence stars. They usually range in mass from 1 to 1.4 solar masses and have a surface temperature reaching 7,600 Kelvin.This particular star is a giant." ) );
            stars.Add( "Codex_Ent_F_TypeSuperGiant_Name", new AstrometricData( "F Type SuperGiant", "F type stars are white main sequence stars. They usually range in mass from 1 to 1.4 solar masses and have a surface temperature reaching 7,600 Kelvin.This particular star is a supergiant." ) );
            stars.Add( "Codex_Ent_G_Type_Name", new AstrometricData( "G Type Star", "G type stars are white-yellow main sequence stars. They usually range in mass from 0.8 to 1.2 solar masses and have a surface temperature reaching 6,000 Kelvin." ) );
            stars.Add( "Codex_Ent_G_TypeGiant_Name", new AstrometricData( "G Type Giant", "G type stars are white-yellow main sequence stars. They usually range in mass from 0.8 to 1.2 solar masses and have a surface temperature reaching 6,000 Kelvin.This particular star is a giant." ) );
            stars.Add( "Codex_Ent_G_TypeSuperGiant_Name", new AstrometricData( "G Type SuperGiant", "G type stars are white-yellow main sequence stars. They usually range in mass from 0.8 to 1.2 solar masses and have a surface temperature reaching 6,000 Kelvin.This particular star is a supergiant." ) );
            stars.Add( "Codex_Ent_K_Type_Name", new AstrometricData( "K Type Star", "K type stars are yellow-orange main sequence stars with a long and generally stable life. They usually range in mass from 0.6 to 0.9 solar masses and have a surface temperature reaching 5,000 Kelvin." ) );
            stars.Add( "Codex_Ent_K_TypeGiant_Name", new AstrometricData( "K Type Giant", "K type stars are yellow-orange main sequence stars with a long and generally stable life. They usually range in mass from 0.6 to 0.9 solar masses and have a surface temperature reaching 5,000 Kelvin.This particular star is a giant." ) );
            stars.Add( "Codex_Ent_K_TypeSuperGiant_Name", new AstrometricData( "K Type SuperGiant", "K type stars are yellow-orange main sequence stars with a long and generally stable life. They usually range in mass from 0.6 to 0.9 solar masses and have a surface temperature reaching 5,000 Kelvin.This particular star is a supergiant." ) );
            stars.Add( "Codex_Ent_M_Type_Name", new AstrometricData( "M Type Star", "M type stars are red stars that comprise the bulk of the galaxy’s main sequence stars. They usually range from 0.075 to 0.50 solar masses, and their surface temperature is less than 4,000 Kelvin." ) );
            stars.Add( "Codex_Ent_M_TypeGiant_Name", new AstrometricData( "M Type Giant", "M type stars are red stars that comprise the bulk of the galaxy’s main sequence stars. They usually range from 0.075 to 0.50 solar masses, and their surface temperature is less than 4,000 Kelvin.This particular star is a giant." ) );
            stars.Add( "Codex_Ent_M_TypeSuperGiant_Name", new AstrometricData( "M Type SuperGiant", "M type stars are red stars that comprise the bulk of the galaxy’s main sequence stars. They usually range from 0.075 to 0.50 solar masses, and their surface temperature is less than 4,000 Kelvin.This particular star is a supergiant." ) );
            stars.Add( "Codex_Ent_L_Type_Name", new AstrometricData( "L Type Star", "L type stars are dwarf stars on the cusp of supporting hydrogen fusion in their cores. Their temperatures range from 1,300 to 2,400 Kelvin. Cool enough to have alkaline metals and metal hydrides in their atmospheres." ) );
            stars.Add( "Codex_Ent_T_Type_Name", new AstrometricData( "T Type Star", "T type stars are brown dwarfs with a surface temperature between 700 and 1,300 Kelvin. They are sometimes known as methane dwarfs due to the prominence of methane in their composition." ) );
            stars.Add( "Codex_Ent_TTS_Type_Name", new AstrometricData( "T Tauri Star", "T Tauri stars are very young stars in the process of gravitational contraction prior to their cores beginning hydrogen fusion." ) );
            stars.Add( "Codex_Ent_AEBE_Type_Name", new AstrometricData( "AEBE Type Star", "Herbig Ae/Be stars are young stars typically less than 10 million years old, with characteristics of either class A or B main sequence stars. They are usually between 2 and 8 solar masses. The mass of the proto-star determines its spectral class when it joins the main sequence." ) );
            stars.Add( "Codex_Ent_Y_Type_Name", new AstrometricData( "Y Type Star", "Y type stars are the coolest of the brown dwarf stars. Surface temperatures are less than 700 Kelvin, and they are effectively very large gas giant planets with some stellar properties." ) );
            stars.Add( "Codex_Ent_C_Type_Name", new AstrometricData( "C Type Star", "C type stars are carbon stars approaching the end of their lives. Hydrogen fusion is beginning to stop, and can alternate. They can be identified by the carbon component in the atmosphere being greater than oxygen. The surface temperature is rarely high than 4,300 Kelvin." ) );
            stars.Add( "Codex_Ent_C_TypeGiant_Name", new AstrometricData( "C Type Giant", "C type stars are carbon stars approaching the end of their lives. Hydrogen fusion is beginning to stop, and can alternate. They can be identified by the carbon component in the atmosphere being greater than oxygen. The surface temperature is rarely high than 4,300 Kelvin.This particular star is a giant." ) );
            stars.Add( "Codex_Ent_C_TypeSuperGiant_Name", new AstrometricData( "C Type SuperGiant", "C type stars are carbon stars approaching the end of their lives. Hydrogen fusion is beginning to stop, and can alternate. They can be identified by the carbon component in the atmosphere being greater than oxygen. The surface temperature is rarely high than 4,300 Kelvin.This particular star is a supergiant." ) );
            stars.Add( "Codex_Ent_C_TypeHyperGiant_Name", new AstrometricData( "C Type HyperGiant", "C type stars are carbon stars approaching the end of their lives. Hydrogen fusion is beginning to stop, and can alternate. They can be identified by the carbon component in the atmosphere being greater than oxygen. The surface temperature is rarely high than 4,300 Kelvin.This particular star is a hypergiant." ) );
            stars.Add( "Codex_Ent_CN_Type_Name", new AstrometricData( "CN Type Star", "C N type stars are variants of carbon stars, which are stars approaching the end of their stellar lives as hydrogen fusion begins to stop. They were once K or M type stars." ) );
            stars.Add( "Codex_Ent_CJ_Type_Name", new AstrometricData( "CJ Type Star", "C J type stars are variants of carbon stars, which are stars approaching the end of their stellar lives as hydrogen fusion begins to stop. The C J variant has much more carbon-13 in its atmosphere." ) );
            stars.Add( "Codex_Ent_MS_Type_Name", new AstrometricData( "MS Type Star", "M S Type stars are late sequence stars that have progressed from the M class stage and are heading towards becoming carbon stars. They are nearing the end of their stellar lives." ) );
            stars.Add( "Codex_Ent_S_Type_Name", new AstrometricData( "S Type Star", "S type stars are late sequence stars that were once M class stars. They have begun the cycle towards becoming carbon stars, and are nearing the end of their stellar lives." ) );
            stars.Add( "Codex_Ent_S_TypeGiant_Name", new AstrometricData( "S Type Giant", "S type stars are late sequence stars that were once M class stars. They have begun the cycle towards becoming carbon stars, and are nearing the end of their stellar lives.This particular star is a giant." ) );
            stars.Add( "Codex_Ent_W_Type_Name", new AstrometricData( "W Type Star", "Wolf-Rayet stars are massive stars nearing the end of their life cycle, past their hydrogen-burning phase. They were once over 20 solar masses but now shed material through solar wind. Their surface temperature can reach 200,000 Kelvin, so they appear a brilliant blue." ) );
            stars.Add( "Codex_Ent_WC_Type_Name", new AstrometricData( "WC Type Star", "Wolf-Rayet stars are massive stars nearing the end of their life cycle, past their hydrogen-burning phase. They were once over 20 solar masses but now shed material through solar wind. Their surface temperature can reach 200,000 Kelvin, so they appear a brilliant blue. The W C type are variants of wolf-rayet stars whose spectrum is dominated by ionised carbon lines." ) );
            stars.Add( "Codex_Ent_WN_Type_Name", new AstrometricData( "WN Type Star", "Wolf-Rayet stars are massive stars nearing the end of their life cycle, past their hydrogen-burning phase. They were once over 20 solar masses but now shed material through solar wind. Their surface temperature can reach 200,000 Kelvin, so they appear a brilliant blue. The W N type are variants of wolf-rayet stars whose spectrum is dominated by ionised nitrogen and helium lines." ) );
            stars.Add( "Codex_Ent_WNC_Type_Name", new AstrometricData( "WNC Type Star", "Wolf-Rayet stars are massive stars nearing the end of their life cycle, past their hydrogen-burning phase. They were once over 20 solar masses but now shed material through solar wind. Their surface temperature can reach 200,000 Kelvin, so they appear a brilliant blue. The W N C type are variants of wolf-rayet stars whose spectrum is dominated by ionised nitrogen, carbon-oxygen and and helium lines." ) );
            stars.Add( "Codex_Ent_WO_Type_Name", new AstrometricData( "WO Type Star", "Wolf-Rayet stars are massive stars nearing the end of their life cycle, past their hydrogen-burning phase. They were once over 20 solar masses but now shed material through solar wind. Their surface temperature can reach 200,000 Kelvin, so they appear a brilliant blue. The W O type are variants of wolf-rayet stars whose spectrum is dominated by ionised oxygen lines." ) );
            stars.Add( "Codex_Ent_D_Type_Name", new AstrometricData( "D Type Star", "White dwarf stars are stellar remnants. Nuclear fusion has now ceased, and in the absence of radiation pressure the core has collapsed, heating it up greatly before it begins its slow cooling-down phase. Surface temperatures are usually between 8,000 and 40,000 Kelvin, so these stellar remnants are blue-white." ) );
            stars.Add( "Codex_Ent_DA_Type_Name", new AstrometricData( "DA Type Star", "White dwarf stars are stellar remnants. Nuclear fusion has now ceased, and in the absence of radiation pressure the core has collapsed, heating it up greatly before it begins its slow cooling-down phase. Surface temperatures are usually between 8,000 and 40,000 Kelvin, so these stellar remnants are blue-white. D A type stars are white dwarf stars with a hydrogen-rich atmosphere." ) );
            stars.Add( "Codex_Ent_DAB_Type_Name", new AstrometricData( "DAB Type Star", "White dwarf stars are stellar remnants. Nuclear fusion has now ceased, and in the absence of radiation pressure the core has collapsed, heating it up greatly before it begins its slow cooling-down phase. Surface temperatures are usually between 8,000 and 40,000 Kelvin, so these stellar remnants are blue-white. D A B type stars are white dwarf stars with hydrogen and helium-rich atmospheres and neutral helium emission lines." ) );
            stars.Add( "Codex_Ent_DAV_Type_Name", new AstrometricData( "DAV Type Star", "White dwarf stars are stellar remnants. Nuclear fusion has now ceased, and in the absence of radiation pressure the core has collapsed, heating it up greatly before it begins its slow cooling-down phase. Surface temperatures are usually between 8,000 and 40,000 Kelvin, so these stellar remnants are blue-white. D A V type stars are known as pulsating white dwarfs. They have hydrogen-rich atmospheres and their luminosity changes according to non-radial gravity waves within the star." ) );
            stars.Add( "Codex_Ent_DAZ_Type_Name", new AstrometricData( "DAZ Type Star", "White dwarf stars are stellar remnants. Nuclear fusion has now ceased, and in the absence of radiation pressure the core has collapsed, heating it up greatly before it begins its slow cooling-down phase. Surface temperatures are usually between 8,000 and 40,000 Kelvin, so these stellar remnants are blue-white. D A Z type stars are white dwarfs, which are hydrogen-rich metallic stars." ) );
            stars.Add( "Codex_Ent_DB_Type_Name", new AstrometricData( "DB Type Star", "White dwarf stars are stellar remnants. Nuclear fusion has now ceased, and in the absence of radiation pressure the core has collapsed, heating it up greatly before it begins its slow cooling-down phase. Surface temperatures are usually between 8,000 and 40,000 Kelvin, so these stellar remnants are blue-white. D B type stars are white dwarfs with a helium-rich atmosphere with neutral helium emission lines." ) );
            stars.Add( "Codex_Ent_DBV_Type_Name", new AstrometricData( "DBV Type Star", "White dwarf stars are stellar remnants. Nuclear fusion has now ceased, and in the absence of radiation pressure the core has collapsed, heating it up greatly before it begins its slow cooling-down phase. Surface temperatures are usually between 8,000 and 40,000 Kelvin, so these stellar remnants are blue-white. D B V type stars are known as pulsating white drwafs. They have helium-rich atmospheres and their luminosity changes according to non-radial gravity waves within the star." ) );
            stars.Add( "Codex_Ent_DBZ_Type_Name", new AstrometricData( "DBZ Type Star", "White dwarf stars are stellar remnants. Nuclear fusion has now ceased, and in the absence of radiation pressure the core has collapsed, heating it up greatly before it begins its slow cooling-down phase. Surface temperatures are usually between 8,000 and 40,000 Kelvin, so these stellar remnants are blue-white. D B Z type stars are helium-rich and metallic white dwarf stars." ) );
            stars.Add( "Codex_Ent_DC_Type_Name", new AstrometricData( "DC Type Star", "White dwarf stars are stellar remnants. Nuclear fusion has now ceased, and in the absence of radiation pressure the core has collapsed, heating it up greatly before it begins its slow cooling-down phase. Surface temperatures are usually between 8,000 and 40,000 Kelvin, so these stellar remnants are blue-white. D C type stars are white dwarfs without spectral lines strong enough to classify their atmospheres." ) );
            stars.Add( "Codex_Ent_DCV", new AstrometricData( "DCV Type Star", "White dwarf stars are stellar remnants. Nuclear fusion has now ceased, and in the absence of radiation pressure the core has collapsed, heating it up greatly before it begins its slow cooling-down phase. Surface temperatures are usually between 8,000 and 40,000 Kelvin, so these stellar remnants are blue-white. D C V type stars are white dwarfs with varying luminosity." ) );
            stars.Add( "Codex_Ent_DDQ", new AstrometricData( "DQ Type Star", "White dwarf stars are stellar remnants. Nuclear fusion has now ceased, and in the absence of radiation pressure the core has collapsed, heating it up greatly before it begins its slow cooling-down phase. Surface temperatures are usually between 8,000 and 40,000 Kelvin, so these stellar remnants are blue-white. D Q type stars are white dwarfs with a carbon-rich atmosphere." ) );
            stars.Add( "Codex_Ent_Neutron_Stars_Name", new AstrometricData( "Neutron Star", "Neutron stars are the stellar remnants of massive stars. Once nuclear fision was exhausted, the star collapsed into an extreme high-density state made up entirely of neutrons, though with insufficient mass to become a black hole." ) );
            stars.Add( "Codex_Ent_Black_Holes_Name", new AstrometricData( "Black Hole", "Black holes are the stellar remnants of supermassive stars of 20 solar masses or more, collapsed to the point where gravity  is so extreme that light can no longer escape. Typically a black hole is only visible by the gravitational distortion in its vicinity." ) );
            stars.Add( "Codex_Ent_SupermassiveBlack_Holes_Name", new AstrometricData( "Supermassive Black Hole", "Since modern FSD technology made it possible to travel the extreme depths of space, Sag-A* has been visited by hundreds of explorers. Some come here as part of a speed race or challenge, others to gain the experience (and credits) of scanning the most massive stellar body in the galaxy. Some come because their wanderlust drives them ever further from the regions around Sol, and others come as part of one of the many community expeditions that have made their way through this system.  Sag-A* was first reached by CMDR Zulu Romeo in late November 3300 while he was doing a scouting mission for the First Great Expedition (FGE) towards the galactic core. At the time no one expected that it would be possible to reach the galactic core before massive FSD failure would be experienced, but Zulu proved the resilience of modern drive technology by continuing on and on.  A later notable event here at the center of the galaxy, was the record breaking meet-up during the Distant Worlds Expedition of 3302." ) );

            gas.Add( "Codex_Ent_Standard_Water_Giant_Name", new AstrometricData( "Standard Water Giant Planet", "A gas giant with a very thick atmosphere dominated by water vapour." ) );
            gas.Add( "Codex_Ent_Standard_Giant_With_Water_Life_Name", new AstrometricData( "Standard Gas Giant Planet with Water-Based Life", "A gas giant with hydrogen and helium-based atmosphere. Vivid colours are produced by oxygen and carbon-based compounds. This planet contains life forms in a hot layer of high-pressure water, with vast quantities of free-floating radioplankton. Tiny carbon-based algae." ) );
            //gas.Add( "", new AstrometricData( "Green Gas Giant Planet with Water-Based Life", "A gas giant with a primarily hydrogen and helium atmosphere and bioluminescent life, based in the water-cloud layer just below the atmospheric surface. An excess of oxygen and many carbon-based compounds produces vivid colours. There are vast quantities of free-floating radioplankton. Tiny carbon-based algae." ) );
            gas.Add( "Codex_Ent_Standard_Giant_With_Ammonia_Life_Name", new AstrometricData( "Standard Gas Giant Planet with Ammonia-Based Life", "A gas giant with a hydrogen and helium-based atmosphere. Vivid colors are produced by oxygen and carbon-based compounds. This planet contains life forms in the ammonia-cloud layer, with vast quantities of free-floating radioplankton. Tiny carbon-based algae." ) );
            gas.Add( "Codex_Ent_Standard_Sudarsky_Class_I_Name", new AstrometricData( "Standard Class I Gas Giant Planet", "A gas giant with a primarily hydrogen and helium atmosphere. Vivid colours are produced by clouds in the upper atmosphere of ammonia, water vapour, hydrogren sulphide, phosphine and sulfur. Upper cloud layer temperatures are typically less than 150 Kelvin." ) );
            //gas.Add( "", new AstrometricData( "Green Class I Gas Giant Planet", "A gas giant with a primarily hydrogen and helium atmosphere with clouds of ammonia, water vapour, hydrogren sulphide, phosphine and sulfur. Upper cloud layer temperatures are typically less than 150 Kelvin. Vivid cloud layor coloration suggests bioluminescent organisms. There are vast quantities of free-floating radioplankton. Tiny carbon-based algae." ) );
            gas.Add( "Codex_Ent_Standard_Sudarsky_Class_II_Name", new AstrometricData( "Standard Class II Gas Giant Planet", "A gas giant with a primarily hydrogen and helium atmosphere. Water vapor in the upper cloud layers gives a much higher albedo. Surface temperatures are typically around 250 Kelvin." ) );
            gas.Add( "Codex_Ent_Standard_Sudarsky_Class_III_Name", new AstrometricData( "Standard Class III Gas Giant Planet", "A gas giant with primarily hydrogen and helium atmoshpere but without distinctive cloud layers. Optical scattering causes a blue color, with the chance of wispy cloud layers from sulphides and chlorides. Surface temperatures range between 350 and 800 Kelvin." ) );
            //gas.Add( "", new AstrometricData( "Green Class III Gas Giant Planet", "A gas giant with primarily hydrogen and helium atmoshpere but without distinctive cloud layers. Surface temperatures range between 350 and 800 Kelvin. Optical scattering causes a blue color, with wispy cloud layers from sulphides and chlorides. Vivid, luminous vapor in the upper atmosphere suggests extremophile biouminescent life." ) );
            gas.Add( "Codex_Ent_Standard_Sudarsky_Class_IV_Name", new AstrometricData( "Standard Class IV Gas Giant Planet", "A gas giant with a primarily hydrogen and helium atmosphere., with carbon monoxide and upper clouds of alkali metals above lower cloud layers of silicates and iron compounds, hence the brighter colors. Upper cloud layer temperatures are typically above 900 Kelvin." ) );
            gas.Add( "Codex_Ent_Standard_Sudarsky_Class_V_Name", new AstrometricData( "Standard Class V Gas Giant Planet", "A gas giant with a primarily hydrogen and helium atmosphere, with thick clouds of silicates and iron compounds, even metallic iron. They are the hottest type of gas giants with upper cloud temperatures aboce 1,400 Kelvin and much hotter lower layers." ) );
            gas.Add( "Codex_Ent_Standard_Helium_Rich_Name", new AstrometricData( "Standard Helium-Rich Gas Giant Planet", "A gas giant with primarily helium atmosphere. Most of all hydrogen has been lost due to insufficient mass to hold on to it. Past temperatures may have been mich higher, driving off the hydrogen at a greater rate." ) );
            gas.Add( "Codex_Ent_Standard_Helium_Name", new AstrometricData( "Standard Helium Gas Giant Planet", "A gas giant with more helium in its atmosphere compared to hydrogen. Most or all hydrogen has been lost due to insufficient mass to hold on to it. Past temperatures may have been much higher, driving off the hydrogen at a greater rate." ) );

            terrestrial.Add( "Codex_Ent_Earth_Likes_Name", new AstrometricData( "Earth-Like Planet", "A terrestrial planet with an active water-based chemistry and indigenous carbon-water-based life. This planet’s nitogen-oxygen atmosphere is breathable for humans." ) );
            terrestrial.Add( "Codex_Ent_Standard_Ammonia_Worlds_Name", new AstrometricData( "Ammonia Planet", "A terrestrial ammonia planet with an active ammonia-based chemistry and carbon-ammonia-based life." ) );
            terrestrial.Add( "Codex_Ent_TRF_Ammonia_Worlds_Name", new AstrometricData( "Ammonia Planet", "A terrestrial ammonia planet with an active ammonia-based chemistry and carbon-ammonia-based life.This particular planet is terraformable." ) );
            terrestrial.Add( "Codex_Ent_Standard_Water_Worlds_Name", new AstrometricData( "Water Planet", "A terrestrial water planet with an active water-based chemistry and carbon-water-based life." ) );
            terrestrial.Add( "Codex_Ent_TRF_Water_Worlds_Name", new AstrometricData( "Water Planet", "A terrestrial water planet with an active water-based chemistry and carbon-water-based life.This particular planet is terraformable." ) );
            terrestrial.Add( "Codex_Ent_Standard_Metal_Rich_No_Atmos_Name", new AstrometricData( "Metal-Rich Planet", "A metal-rich planet that has a large metallic core, with plentiful metallic ores even at the surface. Some high metals can be found in their elemental form, especially around areas of past or current volcanism or liquid erosion. This body has no atmosphere." ) );
            terrestrial.Add( "Codex_Ent_Standard_High_Metal_Content_No_Atmos_Name", new AstrometricData( "High Metal Content Planet", "A high metal content planet with a metallic core. Planets like this can have metallic ores near the surface in places, especially around areas of past volcanism. This body has no atmosphere." ) );
            terrestrial.Add( "Codex_Ent_TRF_High_Metal_Content_No_Atmos_Name", new AstrometricData( "High Metal Content Planet", "A high metal content planet with a metallic core. Planets like this can have metallic ores near the surface in places, especially around areas of past volcanism. This body has no atmosphere.This particular planet is terraformable." ) );
            terrestrial.Add( "Codex_Ent_Standard_Rocky_No_Atmos_Name", new AstrometricData( "Rocky Planet", "A rocky planet with little or no surface metal content. Planets like this have lost most of their volatiles due to past heating, and any metallic content will form a small central core. This body has no atmosphere." ) );
            terrestrial.Add( "Codex_Ent_TRF_Rocky_No_Atmos_Name", new AstrometricData( "Rocky Planet", "A rocky planet with little or no surface metal content. Planets like this have lost most of their volatiles due to past heating, and any metallic content will form a small central core. This body has no atmosphere.This particular planet is terraformable." ) );
            terrestrial.Add( "Codex_Ent_Standard_Rocky_Ice_No_Atmos_Name", new AstrometricData( "Rocky Ice Planet", "A rocky ice planet with a small metal core and thick rocky mantle with a crust of very deep ice. Geological activity is common because of the large quantities of volatiles in the crust, often creating a thin, sometimes seasonal atmosphere. Otherwise, this body has no atmosphere." ) );
            terrestrial.Add( "Codex_Ent_Standard_Ice_No_Atmos_Name", new AstrometricData( "Ice Planet", "An ice planet composed mainly of water ice. Planets like this form in the cooler regions of a star system, and retain many volatiles as solids within their crust. This body has no atmosphere." ) );
            terrestrial.Add( "Codex_Ent_Standard_Ter_Metal_Rich_Name", new AstrometricData( "Metal-Rich Planet", "A metal-rich planet that has a large metallic core, with plentiful metallic ores even at the surface. Some high metals can be found in their elemental form, especially around areas of past or current volcanism or liquid erosion. This body has an atmosphere." ) );
            terrestrial.Add( "Codex_Ent_TRF_Ter_Metal_Rich_Name", new AstrometricData( "Metal-Rich Planet", "A metal-rich planet that has a large metallic core, with plentiful metallic ores even at the surface. Some high metals can be found in their elemental form, especially around areas of past or current volcanism or liquid erosion. This body has an atmosphere.This particular planet is terraformable." ) );
            terrestrial.Add( "Codex_Ent_Standard_Ter_High_Metal_Content_Name", new AstrometricData( "High Metal Content Planet", "A high metal content planet with a metallic core. Planets like this can have metallic ores near the surface in places, especially around areas of past volcanism. This body has an atmosphere." ) );
            terrestrial.Add( "Codex_Ent_TRF_Ter_High_Metal_Content_Name", new AstrometricData( "High Metal Content Planet", "A high metal content planet with a metallic core. Planets like this can have metallic ores near the surface in places, especially around areas of past volcanism. This body has an atmosphere.This particular planet is terraformable." ) );
            terrestrial.Add( "Codex_Ent_Standard_Ter_Rocky_Name", new AstrometricData( "Rocky Planet", "A rocky planet with little or no surface metal content. Planets like this have lost most of their volatiles due to past heating, and any metallic content will form a small central core. This body has an atmosphere." ) );
            terrestrial.Add( "Codex_Ent_TRF_Ter_Rocky_Name", new AstrometricData( "Rocky Planet", "A rocky planet with little or no surface metal content. Planets like this have lost most of their volatiles due to past heating, and any metallic content will form a small central core. This body has an atmosphere.This particular planet is terraformable." ) );
            terrestrial.Add( "Codex_Ent_Standard_Ter_Rocky_Ice_Name", new AstrometricData( "Rocky Ice Planet", "A rocky ice planet with a small metal core and thick rocky mantle with a crust of very deep ice. Geological activity is common because of the large quantities of volatiles in the crust, often creating a thin, sometimes seasonal atmosphere. Otherwise, this body has an atmosphere." ) );
            terrestrial.Add( "Codex_Ent_Standard_Ter_Ice_Name", new AstrometricData( "Ice Planet", "An ice planet composed mainly of water ice. Planets like this form in the cooler regions of a star system, and retain many volatiles as solids within their crust. This body has an atmosphere." ) );


        }

        //public static AstrometricData LookupByVariant ( string localisedVariant )
        //{
        //    bool found = false;
        //    string genus = "";
        //    string species = "";

        //    string[] variantSplit = localisedVariant.Split( '-' );
        //    if (variantSplit != null)
        //    {
        //        species = variantSplit[ 0 ];
        //        species = species.Trim();
        //    }

        //    if ( Fonticulua.species.TryGetValue( species, out _ ) )
        //    {
        //        genus = "Fonticulua";
        //        found = true;
        //    }

        //    if (found)
        //    {
        //        return GetData( genus, species );
        //    }

        //    return null;
        //}

        public static AstrometricData GetData ( string subCategory, string codex )
        {
            AstrometricData myData = new AstrometricData();
            //AstrometricData myData;
            bool result = false;

            try
            {
                switch ( subCategory )
                {
                    case "Stars":
                        result = stars.TryGetValue( codex, out myData );
                        break;
                    case "Gas giant planets":
                        result = gas.TryGetValue( codex, out myData );
                        break;
                    case "Terrestrial planets":
                        result = terrestrial.TryGetValue( codex, out myData );
                        break;
                }

                if ( myData == null )
                {
                    myData = new AstrometricData( "In-valid, not found.", "" );
                    return myData;
                }

                return myData;
            }
            catch
            {
                return new AstrometricData( "In-valid, catch invoked.", "" );
            }
        }
    }
}

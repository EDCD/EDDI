using System.Text.RegularExpressions;

namespace Utilities
{
    public static partial class GeneratedRegex
    {
        [GeneratedRegex( @"[A-Z]\.", RegexOptions.Compiled )]
        public static partial Regex ALPHA_DOT ();

        [GeneratedRegex( @"[A-Za-z]+[0-9]+", RegexOptions.Compiled )]
        public static partial Regex ALPHA_THEN_NUMERIC ();

        [GeneratedRegex( @"(?<lookup>!?(?:[A-Za-z_]\w*(?:\([^{}\r\n]*\))?)(?:(?:\.[A-Za-z_]\w*)|\[[^\]\r\n]+\])*)\s*\.$", RegexOptions.CultureInvariant )]
        public static partial Regex CottleCompletionLookupRegex ();

        [GeneratedRegex( @"(?=\().*", RegexOptions.Compiled )]
        public static partial Regex CottleFunctionArgs ();
        
        [GeneratedRegex( @"\[(?<key>(?:""[^""\r\n]*"")|(?:'[^'\r\n]*')|(?:[^\]\r\n]+))\]", RegexOptions.CultureInvariant )]
        public static partial Regex CottleIndexerRegex ();
        
        [GeneratedRegex( @"\{for\s+(?:(?<key>[A-Za-z_]\w*)\s*,\s*)?(?<value>[A-Za-z_]\w*)\s+in\s+(?<collection>[^:{}\r\n]+?)\s*:", RegexOptions.CultureInvariant )]
        public static partial Regex CottleForEnumerationRegex ();

        [GeneratedRegex( @"\{set\s+(?<key>[A-Za-z_]\w*)\s+to\s+(?<expression>[^{}\r\n]+?)\s*\}", RegexOptions.CultureInvariant )]
        public static partial Regex CottleSetExpressionRegex ();

        [GeneratedRegex( @"(?<![A-Za-z0-9_])(F|Humanise|P|Spacialise)\(", RegexOptions.Compiled )]
        public static partial Regex DeprecatedCottleFunctionsRegex ();

        [GeneratedRegex( @"\d+(?:\s|$)", RegexOptions.Compiled )]
        public static partial Regex DIGIT ();

        [ GeneratedRegex( @"(?<major>\d+).(?<minor>\d+).(?<patch>\d+)(-(?<phase>[a-z]+)(?<iteration>\d+))?", RegexOptions.Compiled ) ]
        public static partial Regex EddiVersionRegex ();
        
        [GeneratedRegex( @"\s\(\d\)$", RegexOptions.Compiled )] 
        public static partial Regex EndingCountRegex (); // e.g. "Ancient Ruins (3)"

        [GeneratedRegex( @"(?>\s*<break time=""\d+[ms]+""\s*\/>)+\s*$", RegexOptions.Compiled )]
        public static partial Regex EndingPauseRegex ();

        [GeneratedRegex( @"\\", RegexOptions.Compiled )]
        public static partial Regex EscapeCharacterRegex ();

        [GeneratedRegex( @"[[a-zA-Z0-9]{3}-[[a-zA-Z0-9]{3}$", RegexOptions.Compiled )]
        public static partial Regex FleetCarrierIdRegex ();

        [GeneratedRegex( @"^(.+)(?> )([A-Za-z0-9]{3}-[A-Za-z0-9]{3})$", RegexOptions.Compiled )]
        public static partial Regex FleetCarrierNameAndIdRegex ();

        [GeneratedRegex( @"<[^>]*>", RegexOptions.Compiled )]
        public static partial Regex HtmlRegex ();

        [GeneratedRegex( @"[\x00-\x1F\x7f]", RegexOptions.Compiled )]
        public static partial Regex HtmlControlCodesRegex ();

        [GeneratedRegex( @"[0-9]", RegexOptions.Compiled )]
        public static partial Regex IsIntegerRegex ();

        [GeneratedRegex( @"^{.*}$", RegexOptions.Compiled | RegexOptions.Singleline )]
        public static partial Regex JsonWrappedRegex ();
        
        [GeneratedRegex( @"^[a-z]$", RegexOptions.Compiled )]
        public static partial Regex MOON ();

        [GeneratedRegex( "\r?\n", RegexOptions.Compiled )]
        public static partial Regex NewLineRegex ();

        [GeneratedRegex( @"\S", RegexOptions.Compiled )]
        public static partial Regex NonWordRegex ();

        [GeneratedRegex( "<phoneme.*?>", RegexOptions.Compiled )]
        public static partial Regex OpenPhonemeRegex ();

        [GeneratedRegex( @"(?<=\s)((?>\d+.){1,3}(?>\d+)(?>\/\d+)?)$", RegexOptions.Compiled )]
        public static partial Regex OsVersionRegex ();

        [GeneratedRegex( @"^(?<SYSTEM>(?<SECTOR>[\w\s'.()-]+) (?<COORDINATES>(?<l1>[A-Za-z])(?<l2>[A-Za-z])-(?<l3>[A-Za-z]) (?<mcode>[A-Za-z])(?:(?<n1>\d+)-)?(?<n2>\d+))) ?(?<BODY>.*)$", RegexOptions.Compiled )]
        public static partial Regex PROC_GEN_SYSTEM_BODY ();

        [ GeneratedRegex( @"^(?<engine>0|[1-9]\d*)\.(?<major>0|[1-9]\d*)(?:\.(?<minor>\d*))?(?:\.(?<patch>\d*))?", RegexOptions.Compiled ) ]
        public static partial Regex SemanticVersionRegex ();
        
        [GeneratedRegex( @"(?=\S)(?<STARS>(?<=^|\s)[A-E]+)? ?(?<PLANET>(?<=^|\s)\d{1,2})? ?(?<MOON>(?<=^|\s)[a-z])? ?(?<SUBMOON>(?<=^|\s)[a-z])? ?(?>(?<=^|\s)(?<RINGORBELTGROUP>[A-Z]) (?<RINGORBELTTYPE>Belt|Ring))? ?(?>(?<=^|\s)(?<CLUSTER>Cluster) (?<CLUSTERNUMBER>\d*))?$", RegexOptions.Compiled )]
        public static partial Regex SHORTBODY ();

        [GeneratedRegex( @"Slot([0-9]{1,2})_Size([0-9]+)", RegexOptions.Compiled )]
        public static partial Regex ShipSlotSizeRegex ();

        [GeneratedRegex( @"(Cargo|Military|Passenger|LimpetController)([0-9]{2})", RegexOptions.Compiled )]
        public static partial Regex ShipSpecialtySlotRegex ();

        [GeneratedRegex( @"(<.+?src="")(.:)(.*?\/>)", RegexOptions.Compiled )]
        public static partial Regex SrcFixRegex ();

        [GeneratedRegex( @"<.*?>", RegexOptions.Compiled )]
        public static partial Regex SsmlTagRegex ();

        [GeneratedRegex( @"/^(Stronghold Carrier|Porte-vaisseaux de forteresse|Transportadora da potência|Носитель-база|Hochburg-Carrier|Portanaves bastión|\$ShipName_StrongholdCarrier(.*?))$/i", RegexOptions.Compiled )]
        public static partial Regex StrongholdCarrierRegex ();

        [GeneratedRegex( @"^(.+)(?> \| )([A-Za-z0-9]{0,4})$", RegexOptions.Compiled )]
        public static partial Regex SquadronCarrierRegex ();

        [GeneratedRegex( @"(?<STARS>(?<=^|\s)[A-E]+)", RegexOptions.Compiled )]
        public static partial Regex STARS ();

        [GeneratedRegex( @"([A-Z])|(\d+)|([a-z])|(\S)", RegexOptions.Compiled )]
        public static partial Regex StellarBodyRegex ();

        [GeneratedRegex( @"^\bA[BCDE]?[CDE]?[DE]?[E]?\b|\bB[CDE]?[DE]?[E]?\b|\bC[DE]?[E]?\b|\bD[E]?\b$", RegexOptions.Compiled )]
        public static partial Regex SUBSTARS ();

        [GeneratedRegex( @"\t| {2,}", RegexOptions.Compiled )] // Intentionally preserves vertical line whitespace characters like new lines for speechresponder.out.
        public static partial Regex TabsOrTwoOrMoreSpacesRegex ();

        [GeneratedRegex( @"([A-Za-z]{1,3}(?:\s|$))", RegexOptions.Compiled )]
        public static partial Regex TEXT ();

        [GeneratedRegex( @"\d{3,}", RegexOptions.Compiled )]
        public static partial Regex THREE_OR_MORE_DIGITS ();

        [GeneratedRegex( @"\$.+;", RegexOptions.Compiled )]
        public static partial Regex UnlocalizedEdNameRegex ();

        [GeneratedRegex( @"([A-Z]{2,})|(?:([A-Z])(?:\s|$))", RegexOptions.Compiled )]
        public static partial Regex UPPERCASE ();

        // Matches recognized SSML tags <tag ...> or </tag>
        // Cereproc uses some additional custom SSML tags (documented in https://www.cereproc.com/files/CereVoiceCloudGuide.pdf)
        [GeneratedRegex( "<(audio.*?|break.*?|play.*?|phoneme.*?|/phoneme|prosody.*?|/prosody|emphasis.*?|/emphasis|transmit.*?|/transmit|voice.*?|/voice|say-as.*?|/say-as|usel.*?|/usel|spurt.*?|/spurt)>", RegexOptions.Compiled )]
        public static partial Regex ValidTagRegex ();

        [GeneratedRegex( @"\[[^\]]*\]|[^\[\]]+", RegexOptions.Compiled )]
        public static partial Regex VoiceAttackCommandPermutationsRegex ();

        [GeneratedRegex( @"\{(?:TXT|INT|DEC|BOOL|DATE):(?<key>[^}\r\n]+)\}", RegexOptions.Compiled )]
        public static partial Regex VoiceAttackVariableLineRegex ();

        [GeneratedRegex( @"(?<=<voice name=\"")(.*?)(?=\"")", RegexOptions.Compiled )]
        public static partial Regex VoiceNameRegex ();

        [GeneratedRegex( @"(?<=>)(.+)(?=<\/voice>)", RegexOptions.Compiled )]
        public static partial Regex VoiceSpeechExtractionRegex ();

        [GeneratedRegex( @"\s+", RegexOptions.Compiled )]
        public static partial Regex WhiteSpaceRegex ();

        [GeneratedRegex( @"\s{2,}", RegexOptions.Compiled )]
        public static partial Regex WhiteSpaceTwoOrMoreRegex ();

        [GeneratedRegex( @"\w", RegexOptions.Compiled )]
        public static partial Regex WordCharacterRegex ();
        
        [GeneratedRegex( @"\b\w+\b", RegexOptions.Compiled )]
        public static partial Regex WordsRegex ();

        // Regexes we do not currently use but might at some point
        /*
        private static readonly Regex DECIMAL_DIGITS = new Regex(@"( point )(\d{2,})");
        private static readonly Regex SECTOR = new Regex("(.*) ([A-Za-z][A-Za-z]-[A-Za-z] .*)");
        private static readonly Regex SYSTEMBODY = new Regex(@"^(.*?) ([A-E]+ ){0,2}(Belt(?:\s|$)|Cluster(?:\s|$)|Ring|\d{1,2}(?:\s|$)|[A-Za-z](?:\s|$)){1,12}$");
        private static readonly Regex PROC_GEN_SYSTEM = new Regex(@"^(?<SECTOR>[\w\s'.()-]+) (?<COORDINATES>(?<l1>[A-Za-z])(?<l2>[A-Za-z])-(?<l3>[A-Za-z]) (?<mcode>[A-Za-z])(?:(?<n1>\d+)-)?(?<n2>\d+))$");
        */
    }
}
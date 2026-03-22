namespace EddiSpeechService.SpeechConversions
{
    public static partial class SpeechConversions
    {
        /// <summary>Fix up power names</summary>
        public static string getPhoneticPower(string power)
        {
            if (power == null)
            {
                return null;
            }

            return power switch
            {
                "Archon Delaine" => "<phoneme alphabet=\"ipa\" ph=\"" + Properties.Phonetics.archon +
                                    "\">Archon</phoneme> <phoneme alphabet=\"ipa\" ph=\"" +
                                    Properties.Phonetics.delaine + "\">Delaine</phoneme>",
                "Aisling Duval" => "<phoneme alphabet=\"ipa\" ph=\"" + Properties.Phonetics.aisling +
                                   "\">Aisling</phoneme> <phoneme alphabet=\"ipa\" ph=\"" + Properties.Phonetics.duval +
                                   "\">Duval</phoneme>",
                "Arissa Lavigny-Duval" => "<phoneme alphabet=\"ipa\" ph=\"" + Properties.Phonetics.arissa +
                                          "\">Arissa</phoneme> <phoneme alphabet=\"ipa\" ph=\"" +
                                          Properties.Phonetics.lavigny +
                                          "\">Lavigny</phoneme> <phoneme alphabet=\"ipa\" ph=\"" +
                                          Properties.Phonetics.duval + "\">Duval</phoneme>",
                "Denton Patreus" => "<phoneme alphabet=\"ipa\" ph=\"" + Properties.Phonetics.denton +
                                    "\">Denton</phoneme> <phoneme alphabet=\"ipa\" ph=\"" +
                                    Properties.Phonetics.patreus + "\">Patreus</phoneme>",
                "Edmund Mahon" => "<phoneme alphabet=\"ipa\" ph=\"" + Properties.Phonetics.edmund +
                                  "\">Edmund</phoneme> <phoneme alphabet=\"ipa\" ph=\"" + Properties.Phonetics.mahon +
                                  "\">Mahon</phoneme>",
                "Felicia Winters" => "<phoneme alphabet=\"ipa\" ph=\"" + Properties.Phonetics.felicia +
                                     "\">Felicia</phoneme> <phoneme alphabet=\"ipa\" ph=\"" +
                                     Properties.Phonetics.winters + "\">Winters</phoneme>",
                "Pranav Antal" => "<phoneme alphabet=\"ipa\" ph=\"" + Properties.Phonetics.pranav +
                                  "\">Pranav</phoneme> <phoneme alphabet=\"ipa\" ph=\"" + Properties.Phonetics.antal +
                                  "\">Antal</phoneme>",
                "Zachary Hudson" => "<phoneme alphabet=\"ipa\" ph=\"" + Properties.Phonetics.zachary +
                                    "\">Zachary</phoneme> <phoneme alphabet=\"ipa\" ph=\"" +
                                    Properties.Phonetics.hudson + "\">Hudson</phoneme>",
                "Zemina Torval" => "<phoneme alphabet=\"ipa\" ph=\"" + Properties.Phonetics.zemina +
                                   "\">Zemina</phoneme> <phoneme alphabet=\"ipa\" ph=\"" + Properties.Phonetics.torval +
                                   "\">Torval</phoneme>",
                "Li Yong-Rui" => "<phoneme alphabet=\"ipa\" ph=\"" + Properties.Phonetics.li +
                                 "\">Li</phoneme> <phoneme alphabet=\"ipa\" ph=\"" + Properties.Phonetics.yong +
                                 "\">Yong</phoneme> <phoneme alphabet=\"ipa\" ph=\"" + Properties.Phonetics.rui +
                                 "\">Rui</phoneme>",
                "Yuri Grom" => "<phoneme alphabet=\"ipa\" ph=\"" + Properties.Phonetics.yuri +
                               "\">Yuri</phoneme> <phoneme alphabet=\"ipa\" ph=\"" + Properties.Phonetics.grom +
                               "\">Grom</phoneme>",
                _ => power
            };
        }
    }
}

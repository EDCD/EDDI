namespace EddiSpeechService
{
    public partial class Translations
    {
        /// <summary>Fix up power names</summary>
        public static string getPhoneticPower(string power)
        {
            if (power == null)
            {
                return null;
            }

            switch (power)
            {
                case "Archon Delaine":
                    return "<phoneme alphabet=\"ipa\" ph=\"" + Properties.Phonetics.archon + "\">Archon</phoneme> <phoneme alphabet=\"ipa\" ph=\"" + Properties.Phonetics.delaine + "\">Delaine</phoneme>";
                case "Aisling Duval":
                    return "<phoneme alphabet=\"ipa\" ph=\"" + Properties.Phonetics.aisling + "\">Aisling</phoneme> <phoneme alphabet=\"ipa\" ph=\"" + Properties.Phonetics.duval + "\">Duval</phoneme>";
                case "Arissa Lavigny-Duval":
                    return "<phoneme alphabet=\"ipa\" ph=\"" + Properties.Phonetics.arissa + "\">Arissa</phoneme> <phoneme alphabet=\"ipa\" ph=\"" + Properties.Phonetics.lavigny + "\">Lavigny</phoneme> <phoneme alphabet=\"ipa\" ph=\"" + Properties.Phonetics.duval + "\">Duval</phoneme>";
                case "Denton Patreus":
                    return "<phoneme alphabet=\"ipa\" ph=\"" + Properties.Phonetics.denton + "\">Denton</phoneme> <phoneme alphabet=\"ipa\" ph=\"" + Properties.Phonetics.patreus + "\">Patreus</phoneme>";
                case "Edmund Mahon":
                    return "<phoneme alphabet=\"ipa\" ph=\"" + Properties.Phonetics.edmund + "\">Edmund</phoneme> <phoneme alphabet=\"ipa\" ph=\"" + Properties.Phonetics.mahon + "\">Mahon</phoneme>";
                case "Felicia Winters":
                    return "<phoneme alphabet=\"ipa\" ph=\"" + Properties.Phonetics.felicia + "\">Felicia</phoneme> <phoneme alphabet=\"ipa\" ph=\"" + Properties.Phonetics.winters + "\">Winters</phoneme>";
                case "Pranav Antal":
                    return "<phoneme alphabet=\"ipa\" ph=\"" + Properties.Phonetics.pranav + "\">Pranav</phoneme> <phoneme alphabet=\"ipa\" ph=\"" + Properties.Phonetics.antal + "\">Antal</phoneme>";
                case "Zachary Hudson":
                    return "<phoneme alphabet=\"ipa\" ph=\"" + Properties.Phonetics.zachary + "\">Zachary</phoneme> <phoneme alphabet=\"ipa\" ph=\"" + Properties.Phonetics.hudson + "\">Hudson</phoneme>";
                case "Zemina Torval":
                    return "<phoneme alphabet=\"ipa\" ph=\"" + Properties.Phonetics.zemina + "\">Zemina</phoneme> <phoneme alphabet=\"ipa\" ph=\"" + Properties.Phonetics.torval + "\">Torval</phoneme>";
                case "Li Yong-Rui":
                    return "<phoneme alphabet=\"ipa\" ph=\"" + Properties.Phonetics.li + "\">Li</phoneme> <phoneme alphabet=\"ipa\" ph=\"" + Properties.Phonetics.yong + "\">Yong</phoneme> <phoneme alphabet=\"ipa\" ph=\"" + Properties.Phonetics.rui + "\">Rui</phoneme>";
                case "Yuri Grom":
                    return "<phoneme alphabet=\"ipa\" ph=\"" + Properties.Phonetics.yuri + "\">Yuri</phoneme> <phoneme alphabet=\"ipa\" ph=\"" + Properties.Phonetics.grom + "\">Grom</phoneme>";
                default:
                    return power;
            }
        }
    }
}

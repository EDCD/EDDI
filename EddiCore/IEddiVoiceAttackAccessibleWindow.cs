using System.Windows;

namespace EddiCore
{
    public interface IEddiVoiceAttackAccessibleWindow
    {
        void RefreshTextToSpeechConfiguration();

        void ApplyVoiceAttackWindowState( WindowState state, bool minimizeCheck );
    }
}

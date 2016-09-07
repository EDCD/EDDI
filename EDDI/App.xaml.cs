using EliteDangerousCompanionAppService;
using EliteDangerousDataDefinitions;
using EliteDangerousDataProviderService;
using EliteDangerousJournalMonitor;
using EliteDangerousNetLogMonitor;
using EliteDangerousSpeechService;
using EliteDangerousStarMapService;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Utilities;

namespace EDDI
{

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private Eddi eddi;
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            eddi = Eddi.Instance;
            eddi.Start();
        }
    }
}

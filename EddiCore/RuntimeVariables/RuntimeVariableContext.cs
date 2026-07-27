using EddiCompanionAppService;
using EddiConfigService;
using EddiCore.GameState;

namespace EddiCore.RuntimeVariables
{
    internal interface IRuntimeVariableContext
    {
        IEddiGameState GameState { get; }
        bool FromVA { get; }
        bool CapiActive { get; }
        bool IcaoActive { get; }
        bool IpaActive { get; }
    }

    internal sealed class EddiRuntimeVariableContext : IRuntimeVariableContext
    {
        public IEddiGameState GameState => EDDI.Instance.GameState;
        public bool FromVA => EDDI.Instance.FromVA;
        public bool CapiActive => CompanionAppService.Instance?.active ?? false;
        public bool IcaoActive => ConfigService.Instance.speechServiceConfiguration.EnableIcao;
        public bool IpaActive => !ConfigService.Instance.speechServiceConfiguration.DisableIpa;
    }
}

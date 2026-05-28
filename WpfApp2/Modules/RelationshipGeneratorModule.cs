using Som3a_WPF_UI.Services;

namespace Som3a_WPF_UI.Modules
{
    public class RelationshipGeneratorModule : IModule
    {
        public string ModuleId => "relationship-generator";
        public string Name => "Relationship Generator";
        public int Priority => 110;

        public void Initialize(IServiceContainer container, IEventBus eventBus)
        {
        }
    }
}

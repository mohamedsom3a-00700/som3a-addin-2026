using Som3a_WPF_UI.Services;

namespace Som3a_WPF_UI.Modules
{
    public class BOQActivityGeneratorModule : IModule
    {
        public string ModuleId => "boq-activity-generator";
        public string Name => "BOQ Activity Generator";
        public int Priority => 100;

        public void Initialize(IServiceContainer container, IEventBus eventBus)
        {
        }
    }
}

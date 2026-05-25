using System.Collections.Generic;

namespace Som3a_WPF_UI.Services
{
    public class ModuleRibbonActionsAvailableEvent
    {
        public string ModuleId { get; }
        public IReadOnlyList<object> Actions { get; }

        public ModuleRibbonActionsAvailableEvent(string moduleId, IReadOnlyList<object> actions)
        {
            ModuleId = moduleId;
            Actions = actions;
        }
    }
}

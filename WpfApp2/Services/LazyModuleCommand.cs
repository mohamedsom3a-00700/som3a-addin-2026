using System;
using System.Windows.Input;

namespace Som3a_WPF_UI.Services
{
    public class LazyModuleCommand : ICommand
    {
        private readonly string _moduleId;
        private readonly ICommand _innerCommand;
        private readonly ModuleLoadOrchestrator _orchestrator;
        private bool _moduleLoaded;

        public event EventHandler? CanExecuteChanged
        {
            add { _innerCommand.CanExecuteChanged += value; }
            remove { _innerCommand.CanExecuteChanged -= value; }
        }

        public LazyModuleCommand(string moduleId, ICommand innerCommand, ModuleLoadOrchestrator orchestrator)
        {
            _moduleId = moduleId ?? throw new ArgumentNullException(nameof(moduleId));
            _innerCommand = innerCommand ?? throw new ArgumentNullException(nameof(innerCommand));
            _orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
        }

        public bool CanExecute(object? parameter)
        {
            return _innerCommand.CanExecute(parameter);
        }

        public void Execute(object? parameter)
        {
            try
            {
                if (!_moduleLoaded)
                {
                    var module = _orchestrator.EnsureModuleLoaded(_moduleId);
                    _moduleLoaded = module is not null || _orchestrator.IsModuleLoaded(_moduleId);
                }

                _innerCommand.Execute(parameter);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LazyModuleCommand.Execute failed for module '{_moduleId}': {ex.Message}");
            }
        }
    }
}

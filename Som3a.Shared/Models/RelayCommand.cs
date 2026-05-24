using System;
using System.Windows.Input;

namespace Som3a.Shared.Models
{
    public sealed class RelayCommand : ICommand
    {
        private readonly Func<object?, bool>? _can;
        private readonly Action<object?> _exec;

        public RelayCommand(Action<object?> exec, Func<object?, bool>? can = null)
        {
            _exec = exec ?? throw new ArgumentNullException(nameof(exec));
            _can = can;
        }

        public RelayCommand(Action exec, Func<bool>? can = null)
        {
            if (exec is null) throw new ArgumentNullException(nameof(exec));
            _exec = _ => exec();
            _can = can is null ? null : _ => can();
        }

        public bool CanExecute(object? parameter) => _can?.Invoke(parameter) ?? true;
        public void Execute(object? parameter) => _exec(parameter);

        public event EventHandler? CanExecuteChanged;
        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}

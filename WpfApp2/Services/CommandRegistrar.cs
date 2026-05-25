using System;
using System.Collections.Generic;
using System.Windows.Input;
using Som3a_WPF_UI.Contracts;

namespace Som3a_WPF_UI.Services
{
    public class CommandRegistrar : ICommandRegistrar
    {
        private readonly Dictionary<string, ICommand> _commands = new(StringComparer.OrdinalIgnoreCase);

        public IReadOnlyDictionary<string, ICommand> RegisteredCommands => _commands;

        public void RegisterCommand(string name, ICommand command)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            _commands[name] = command;
        }

        public ICommand? FindCommand(string name)
        {
            return _commands.TryGetValue(name, out var cmd) ? cmd : null;
        }
    }
}

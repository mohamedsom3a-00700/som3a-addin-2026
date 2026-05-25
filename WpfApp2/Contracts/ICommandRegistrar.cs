using System.Windows.Input;

namespace Som3a_WPF_UI.Contracts
{
    public interface ICommandRegistrar
    {
        void RegisterCommand(string name, ICommand command);
    }
}

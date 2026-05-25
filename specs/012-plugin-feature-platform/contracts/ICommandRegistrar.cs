/// <summary>
/// Allows modules to register commands that can be invoked programmatically or via keyboard shortcuts.
/// </summary>
public interface ICommandRegistrar
{
    /// <summary>Registers a command that can be dispatched by name.</summary>
    void RegisterCommand(string name, ICommand command);
}

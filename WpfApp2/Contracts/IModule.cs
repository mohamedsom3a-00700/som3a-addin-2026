namespace Som3a_WPF_UI.Contracts
{
    public interface IModule
    {
        string Id { get; }
        string Version { get; }
        string DisplayName { get; }
        string Description { get; }
        void Initialize(IModuleInitializationContext context);
    }
}

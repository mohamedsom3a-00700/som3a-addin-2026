namespace Som3a_WPF_UI.Contracts
{
    public interface IModuleInitializationContext
    {
        T ResolveService<T>() where T : class;
        INavigationRegistrar Navigation { get; }
        IRibbonRegistrar Ribbon { get; }
        ICommandRegistrar Commands { get; }
    }
}

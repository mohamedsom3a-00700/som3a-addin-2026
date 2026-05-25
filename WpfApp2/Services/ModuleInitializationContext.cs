using System;
using Som3a_WPF_UI.Contracts;

namespace Som3a_WPF_UI.Services
{
    public class ModuleInitializationContext : IModuleInitializationContext
    {
        private readonly IServiceContainer _container;

        public ModuleInitializationContext(
            IServiceContainer container,
            INavigationRegistrar navigation,
            IRibbonRegistrar ribbon,
            ICommandRegistrar commands)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
            Navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
            Ribbon = ribbon ?? throw new ArgumentNullException(nameof(ribbon));
            Commands = commands ?? throw new ArgumentNullException(nameof(commands));
        }

        public INavigationRegistrar Navigation { get; }
        public IRibbonRegistrar Ribbon { get; }
        public ICommandRegistrar Commands { get; }

        public T ResolveService<T>() where T : class
        {
            return _container.Resolve<T>();
        }
    }
}

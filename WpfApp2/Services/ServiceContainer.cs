using System;
using System.Collections.Generic;
using System.Linq;

namespace Som3a_WPF_UI.Services
{
    public enum ServiceLifetime
    {
        Singleton,
        Transient,
        Scoped
    }

    public class ServiceResolutionEventArgs : EventArgs
    {
        public Type ServiceType { get; }
        public ServiceLifetime Lifetime { get; }

        public ServiceResolutionEventArgs(Type serviceType, ServiceLifetime lifetime)
        {
            ServiceType = serviceType;
            Lifetime = lifetime;
        }
    }

    public class ServiceRegistrationEventArgs : EventArgs
    {
        public Type ServiceType { get; }
        public Type ImplementationType { get; }
        public ServiceLifetime Lifetime { get; }

        public ServiceRegistrationEventArgs(Type serviceType, Type implementationType, ServiceLifetime lifetime)
        {
            ServiceType = serviceType;
            ImplementationType = implementationType;
            Lifetime = lifetime;
        }
    }

    public interface IServiceContainer
    {
        void RegisterSingleton<TService, TImplementation>() where TImplementation : TService;
        void RegisterSingleton<TService>(TService instance);
        void RegisterTransient<TService, TImplementation>() where TImplementation : TService;
        void RegisterScoped<TService, TImplementation>() where TImplementation : TService;
        TService Resolve<TService>();
        object Resolve(Type serviceType);
        IServiceScope CreateScope();

        event EventHandler<ServiceResolutionEventArgs> ServiceResolved;
        event EventHandler<ServiceRegistrationEventArgs> ServiceRegistered;
    }

    public interface IServiceScope : IDisposable
    {
        TService Resolve<TService>();
        object Resolve(Type serviceType);
    }

    internal sealed class ServiceRegistration
    {
        public Type ServiceType { get; }
        public Type ImplementationType { get; }
        public ServiceLifetime Lifetime { get; }
        public object? SingletonInstance { get; set; }

        public ServiceRegistration(Type serviceType, Type implementationType, ServiceLifetime lifetime)
        {
            ServiceType = serviceType;
            ImplementationType = implementationType;
            Lifetime = lifetime;
        }
    }

    public sealed class ServiceContainer : IServiceContainer
    {
        private readonly List<ServiceRegistration> _registrations = new();
        private readonly Dictionary<Type, object> _singletonInstances = new();
        private readonly HashSet<Type> _resolutionStack = new();

        public event EventHandler<ServiceResolutionEventArgs>? ServiceResolved;
        public event EventHandler<ServiceRegistrationEventArgs>? ServiceRegistered;

        public void RegisterSingleton<TService, TImplementation>() where TImplementation : TService
        {
            var serviceType = typeof(TService);
            var implType = typeof(TImplementation);
            ValidateRegistration(serviceType, implType);
            _registrations.Add(new ServiceRegistration(serviceType, implType, ServiceLifetime.Singleton));
            OnServiceRegistered(serviceType, implType, ServiceLifetime.Singleton);
        }

        public void RegisterSingleton<TService>(TService instance)
        {
            var serviceType = typeof(TService);
            var registration = new ServiceRegistration(serviceType, instance!.GetType(), ServiceLifetime.Singleton)
            {
                SingletonInstance = instance
            };
            _registrations.Add(registration);
            _singletonInstances[serviceType] = instance;
            OnServiceRegistered(serviceType, instance.GetType(), ServiceLifetime.Singleton);
        }

        public void RegisterTransient<TService, TImplementation>() where TImplementation : TService
        {
            var serviceType = typeof(TService);
            var implType = typeof(TImplementation);
            ValidateRegistration(serviceType, implType);
            _registrations.Add(new ServiceRegistration(serviceType, implType, ServiceLifetime.Transient));
            OnServiceRegistered(serviceType, implType, ServiceLifetime.Transient);
        }

        public void RegisterScoped<TService, TImplementation>() where TImplementation : TService
        {
            var serviceType = typeof(TService);
            var implType = typeof(TImplementation);
            ValidateRegistration(serviceType, implType);
            _registrations.Add(new ServiceRegistration(serviceType, implType, ServiceLifetime.Scoped));
            OnServiceRegistered(serviceType, implType, ServiceLifetime.Scoped);
        }

        public TService Resolve<TService>()
        {
            return (TService)Resolve(typeof(TService));
        }

        public object Resolve(Type serviceType)
        {
            if (_resolutionStack.Contains(serviceType))
            {
                var chain = string.Join(" -> ", _resolutionStack.Select(t => t.Name).Concat(new[] { serviceType.Name }));
                throw new InvalidOperationException($"Circular dependency detected: {chain}");
            }

            var registration = _registrations.LastOrDefault(r => r.ServiceType == serviceType);
            if (registration == null)
            {
                throw new InvalidOperationException($"Service not registered: {serviceType.Name}. Call RegisterSingleton/RegisterTransient/RegisterScoped first.");
            }

            _resolutionStack.Add(serviceType);

            try
            {
                object instance;
                switch (registration.Lifetime)
                {
                    case ServiceLifetime.Singleton:
                        if (!_singletonInstances.TryGetValue(serviceType, out var singleton))
                        {
                            singleton = CreateInstance(registration.ImplementationType);
                            _singletonInstances[serviceType] = singleton;
                        }
                        instance = singleton;
                        break;

                    case ServiceLifetime.Transient:
                        instance = CreateInstance(registration.ImplementationType);
                        break;

                    case ServiceLifetime.Scoped:
                        instance = CreateInstance(registration.ImplementationType);
                        break;

                    default:
                        throw new InvalidOperationException($"Unknown lifetime: {registration.Lifetime}");
                }

                ServiceResolved?.Invoke(this, new ServiceResolutionEventArgs(serviceType, registration.Lifetime));
                return instance;
            }
            finally
            {
                _resolutionStack.Remove(serviceType);
            }
        }

        public IServiceScope CreateScope()
        {
            return new ServiceScope(this);
        }

        private void ValidateRegistration(Type serviceType, Type implementationType)
        {
            if (_registrations.Any(r => r.ServiceType == serviceType))
            {
                throw new InvalidOperationException($"Service already registered: {serviceType.Name}. A service type can only be registered once.");
            }

            if (!serviceType.IsAssignableFrom(implementationType))
            {
                throw new InvalidOperationException($"Type {implementationType.Name} does not implement {serviceType.Name}.");
            }
        }

        private object CreateInstance(Type type)
        {
            var constructors = type.GetConstructors();
            if (constructors.Length == 0)
            {
                return Activator.CreateInstance(type)!;
            }

            var constructor = constructors[0];
            var parameters = constructor.GetParameters();
            var resolvedParams = new object[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                resolvedParams[i] = Resolve(parameters[i].ParameterType);
            }

            return constructor.Invoke(resolvedParams);
        }

        private void OnServiceRegistered(Type serviceType, Type implType, ServiceLifetime lifetime)
        {
            ServiceRegistered?.Invoke(this, new ServiceRegistrationEventArgs(serviceType, implType, lifetime));
        }

        internal object ResolveScoped(Type serviceType)
        {
            if (_resolutionStack.Contains(serviceType))
            {
                var chain = string.Join(" -> ", _resolutionStack.Select(t => t.Name).Concat(new[] { serviceType.Name }));
                throw new InvalidOperationException($"Circular dependency detected: {chain}");
            }

            var registration = _registrations.LastOrDefault(r => r.ServiceType == serviceType);
            if (registration == null)
            {
                throw new InvalidOperationException($"Service not registered: {serviceType.Name}. Call RegisterSingleton/RegisterTransient/RegisterScoped first.");
            }

            if (registration.Lifetime == ServiceLifetime.Singleton)
            {
                return Resolve(serviceType);
            }

            _resolutionStack.Add(serviceType);
            try
            {
                if (registration.Lifetime == ServiceLifetime.Scoped)
                {
                    var instance = CreateInstance(registration.ImplementationType);
                    ServiceResolved?.Invoke(this, new ServiceResolutionEventArgs(serviceType, registration.Lifetime));
                    return instance;
                }

                var transient = CreateInstance(registration.ImplementationType);
                ServiceResolved?.Invoke(this, new ServiceResolutionEventArgs(serviceType, registration.Lifetime));
                return transient;
            }
            finally
            {
                _resolutionStack.Remove(serviceType);
            }
        }
    }

    public sealed class ServiceScope : IServiceScope
    {
        private readonly ServiceContainer _container;
        private bool _disposed;

        public ServiceScope(ServiceContainer container)
        {
            _container = container;
        }

        public TService Resolve<TService>()
        {
            return (TService)Resolve(typeof(TService));
        }

        public object Resolve(Type serviceType)
        {
            return _container.ResolveScoped(serviceType);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
            }
        }
    }
}

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Som3a_WPF_UI.Services;

namespace Som3a_WPF_UI.Tests
{
    [TestClass]
    public class ServiceContainerTests
    {
        public interface ITestService { int Id { get; }
        }
        public class TestService : ITestService
        {
            private static int _nextId;
            public int Id { get; } = ++_nextId;
        }

        public interface IDepService { }
        public class DepService : IDepService { }

        public class ServiceWithDependency
        {
            public IDepService Dep { get; }
            public ServiceWithDependency(IDepService dep) { Dep = dep; }
        }

        public class CircularA
        {
            public CircularA(CircularB b) { }
        }
        public class CircularB
        {
            public CircularB(CircularA a) { }
        }

        [TestMethod]
        public void T041_Singleton_ReturnsSameInstance()
        {
            var container = new ServiceContainer();
            container.RegisterSingleton<ITestService, TestService>();

            var a = container.Resolve<ITestService>();
            var b = container.Resolve<ITestService>();

            Assert.AreSame(a, b);
        }

        [TestMethod]
        public void T041_Transient_ReturnsDifferentInstances()
        {
            var container = new ServiceContainer();
            container.RegisterTransient<ITestService, TestService>();

            var a = container.Resolve<ITestService>();
            var b = container.Resolve<ITestService>();

            Assert.AreNotSame(a, b);
        }

        [TestMethod]
        public void T041_Scoped_SameInstanceWithinScope()
        {
            var container = new ServiceContainer();
            container.RegisterScoped<ITestService, TestService>();

            using (var scope = container.CreateScope())
            {
                var a = scope.Resolve<ITestService>();
                var b = scope.Resolve<ITestService>();
                Assert.AreSame(a, b);
            }
        }

        [TestMethod]
        public void T041_Scoped_DifferentAcrossScopes()
        {
            var container = new ServiceContainer();
            container.RegisterScoped<ITestService, TestService>();

            ITestService a, b;
            using (var s1 = container.CreateScope())
                a = s1.Resolve<ITestService>();
            using (var s2 = container.CreateScope())
                b = s2.Resolve<ITestService>();

            Assert.AreNotSame(a, b);
        }

        [TestMethod]
        public void T042_CircularDependency_ThrowsInvalidOperationException()
        {
            var container = new ServiceContainer();
            container.RegisterTransient<CircularA, CircularA>();
            container.RegisterTransient<CircularB, CircularB>();

            var ex = Assert.ThrowsException<InvalidOperationException>(() =>
                container.Resolve<CircularA>());

            Assert.IsTrue(ex.Message.Contains("CircularA"));
            Assert.IsTrue(ex.Message.Contains("CircularB"));
        }

        [TestMethod]
        public void T043_UnregisteredService_ThrowsInvalidOperationException()
        {
            var container = new ServiceContainer();

            var ex = Assert.ThrowsException<InvalidOperationException>(() =>
                container.Resolve<ITestService>());

            Assert.IsTrue(ex.Message.Contains("ITestService"));
        }

        [TestMethod]
        public void T044_ServiceResolvedEvent_Fires()
        {
            var container = new ServiceContainer();
            container.RegisterSingleton<ITestService, TestService>();

            Type? resolvedType = null;
            container.ServiceResolved += (_, e) => resolvedType = e.ServiceType;

            container.Resolve<ITestService>();

            Assert.AreEqual(typeof(ITestService), resolvedType!);
        }

        [TestMethod]
        public void T044_ServiceRegisteredEvent_Fires()
        {
            var container = new ServiceContainer();

            Type? registeredType = null;
            container.ServiceRegistered += (_, e) => registeredType = e.ServiceType;

            container.RegisterSingleton<ITestService, TestService>();

            Assert.AreEqual(typeof(ITestService), registeredType!);
        }
    }
}

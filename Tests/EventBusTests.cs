using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Som3a_WPF_UI.Services;

namespace Som3a_WPF_UI.Tests
{
    [TestClass]
    public class EventBusTests
    {
        public class TestEvent { public string? Data { get; set; } }
        public class OtherEvent { }

        [TestMethod]
        public void T045_Publish_DeliversToMatchingSubscribers()
        {
            var bus = new EventBus();
            string? received = null;
            bus.Subscribe<TestEvent>(e => received = e.Data);

            bus.Publish(new TestEvent { Data = "hello" });

            Assert.AreEqual("hello", received);
        }

        [TestMethod]
        public void T045_Publish_DoesNotDeliverToNonMatchingSubscribers()
        {
            var bus = new EventBus();
            bool otherReceived = false;
            bus.Subscribe<OtherEvent>(e => otherReceived = true);

            bus.Publish(new TestEvent());

            Assert.IsFalse(otherReceived);
        }

        [TestMethod]
        public void T046_WeakReference_PrunesDeadSubscribers()
        {
            var bus = new EventBus();
            var token = bus.Subscribe<TestEvent>(e => { });

            var wr = new WeakReference(token);
            token.Dispose();
            token = null!;
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();

            bus.Publish(new TestEvent());

            bool eventFired = false;
            bus.EventPublished += (_, _) => eventFired = true;
            bus.Publish(new TestEvent());
            Assert.IsTrue(eventFired);
        }

        [TestMethod]
        public void T047_SubscriberIsolation_OneExceptionDoesNotBlockOthers()
        {
            var bus = new EventBus();
            var received = new System.Collections.Generic.List<string>();

            bus.Subscribe<TestEvent>(e => throw new System.Exception("fail"));
            bus.Subscribe<TestEvent>(e => received.Add("ok"));

            bus.Publish(new TestEvent { Data = "test" });

            Assert.AreEqual(1, received.Count);
            Assert.AreEqual("ok", received[0]);
        }

        [TestMethod]
        public void T047_SubscriberError_EventReportsException()
        {
            var bus = new EventBus();
            System.Exception? reported = null;
            bus.SubscriberError += (_, e) => reported = e.Exception;

            bus.Subscribe<TestEvent>(e => throw new System.InvalidOperationException("test error"));
            bus.Publish(new TestEvent());

            Assert.IsNotNull(reported);
            Assert.IsInstanceOfType(reported, typeof(System.InvalidOperationException));
        }

        [TestMethod]
        public void T048_Filter_PredicateFiltersEvents()
        {
            var bus = new EventBus();
            var received = new System.Collections.Generic.List<string>();

            bus.Subscribe<TestEvent>(e => received.Add(e.Data!), e => e.Data == "pass");

            bus.Publish(new TestEvent { Data = "fail" });
            bus.Publish(new TestEvent { Data = "pass" });

            Assert.AreEqual(1, received.Count);
            Assert.AreEqual("pass", received[0]);
        }
    }
}

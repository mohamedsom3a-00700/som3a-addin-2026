using Microsoft.VisualStudio.TestTools.UnitTesting;
using Som3a_WPF_UI.ViewModels;

namespace Som3a_WPF_UI.Tests
{
    [TestClass]
    public class ViewModelBaseTests
    {
        private class TestViewModel : ViewModelBase
        {
            private string? _name;
            public string? Name
            {
                get => _name;
                set => SetProperty(ref _name, value);
            }

            private int _count;
            public int Count
            {
                get => _count;
                set => SetProperty(ref _count, value);
            }
        }

        [TestMethod]
        public void T049_SetProperty_RaisesPropertyChanged_WhenValueDiffers()
        {
            var vm = new TestViewModel();
            var changed = new System.Collections.Generic.List<string?>();
            vm.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

            vm.Name = "Alice";

            Assert.AreEqual(1, changed.Count);
            Assert.AreEqual("Name", changed[0]);
        }

        [TestMethod]
        public void T049_SetProperty_DoesNotRaise_WhenValueSame()
        {
            var vm = new TestViewModel();
            vm.Name = "Alice";
            var changed = new System.Collections.Generic.List<string?>();
            vm.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

            vm.Name = "Alice";

            Assert.AreEqual(0, changed.Count);
        }

        [TestMethod]
        public void T049_SetProperty_RaisesForDifferentTypes()
        {
            var vm = new TestViewModel();
            var changed = new System.Collections.Generic.List<string?>();
            vm.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

            vm.Count = 42;

            Assert.AreEqual(1, changed.Count);
            Assert.AreEqual("Count", changed[0]);
        }
    }
}

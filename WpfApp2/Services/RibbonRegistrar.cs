using System;
using System.Collections.Generic;
using System.Diagnostics;
using Som3a_WPF_UI.Contracts;

namespace Som3a_WPF_UI.Services
{
    public class RibbonRegistrar : IRibbonRegistrar
    {
        private readonly List<object> _actions = new();

        public IReadOnlyList<object> RegisteredActions => _actions.AsReadOnly();

        public void AddButton(string id, string label, string tooltip, Action onClick)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentNullException(nameof(id));
            if (string.IsNullOrWhiteSpace(label))
                throw new ArgumentNullException(nameof(label));
            if (onClick is null)
                throw new ArgumentNullException(nameof(onClick));

            _actions.Add(new RibbonButtonEntry
            {
                Id = id,
                Label = label,
                Tooltip = tooltip,
                OnClick = SafeInvoke(onClick, $"RibbonButton '{id}'")
            });
        }

        public void AddMenu(string id, string label, IReadOnlyList<RibbonMenuItem> items)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentNullException(nameof(id));
            if (string.IsNullOrWhiteSpace(label))
                throw new ArgumentNullException(nameof(label));
            if (items is null)
                throw new ArgumentNullException(nameof(items));

            var wrappedItems = new List<RibbonMenuItem>();
            foreach (var item in items)
            {
                wrappedItems.Add(new RibbonMenuItem
                {
                    Id = item.Id,
                    Label = item.Label,
                    Tooltip = item.Tooltip,
                    OnClick = SafeInvoke(item.OnClick, $"RibbonMenuItem '{item.Id}'")
                });
            }

            _actions.Add(new RibbonMenuEntry { Id = id, Label = label, Items = wrappedItems.AsReadOnly() });
        }

        public void AddToggleButton(string id, string label, Action<bool> onToggle, bool initialState)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentNullException(nameof(id));
            if (string.IsNullOrWhiteSpace(label))
                throw new ArgumentNullException(nameof(label));
            if (onToggle is null)
                throw new ArgumentNullException(nameof(onToggle));

            _actions.Add(new RibbonToggleButtonEntry
            {
                Id = id,
                Label = label,
                OnToggle = SafeInvoke(onToggle, $"RibbonToggleButton '{id}'"),
                InitialState = initialState
            });
        }

        private static Action SafeInvoke(Action inner, string description)
        {
            return () =>
            {
                try
                {
                    inner();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Fault isolation caught exception in {description}: {ex.Message}");
                }
            };
        }

        private static Action<bool> SafeInvoke(Action<bool> inner, string description)
        {
            return (arg) =>
            {
                try
                {
                    inner(arg);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Fault isolation caught exception in {description}: {ex.Message}");
                }
            };
        }

        public class RibbonButtonEntry
        {
            public string Id { get; set; } = "";
            public string Label { get; set; } = "";
            public string Tooltip { get; set; } = "";
            public Action OnClick { get; set; } = () => { };
        }

        public class RibbonMenuEntry
        {
            public string Id { get; set; } = "";
            public string Label { get; set; } = "";
            public IReadOnlyList<RibbonMenuItem> Items { get; set; } = Array.Empty<RibbonMenuItem>();
        }

        public class RibbonToggleButtonEntry
        {
            public string Id { get; set; } = "";
            public string Label { get; set; } = "";
            public Action<bool> OnToggle { get; set; } = _ => { };
            public bool InitialState { get; set; }
        }
    }
}

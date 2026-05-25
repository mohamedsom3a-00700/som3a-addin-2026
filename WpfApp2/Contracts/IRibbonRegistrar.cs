using System;
using System.Collections.Generic;

namespace Som3a_WPF_UI.Contracts
{
    public interface IRibbonRegistrar
    {
        void AddButton(string id, string label, string tooltip, Action onClick);
        void AddMenu(string id, string label, IReadOnlyList<RibbonMenuItem> items);
        void AddToggleButton(string id, string label, Action<bool> onToggle, bool initialState);
    }

    public class RibbonMenuItem
    {
        public string Id { get; set; }
        public string Label { get; set; }
        public string Tooltip { get; set; }
        public Action OnClick { get; set; }
    }
}

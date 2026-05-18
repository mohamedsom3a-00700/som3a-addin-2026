using Som3a.Shared.Core;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Drawing;
using System.Windows.Media;
using Som3a_WPF_UI.Controls;

namespace Som3a_WPF_UI
{
    public partial class StyleSelectorWindow : ModernWindow
    {
        public StyleSelectorWindow()
        {
            InitializeComponent();
            StyleCombo.SelectedIndex = 0;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (StyleCombo.SelectedItem is ComboBoxItem item)
            {
                int selected = int.Parse(item.Tag.ToString());
                UserSettings.SelectedStyle = selected;
            }

            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public class StylePreviewItem
        {
            public int Level { get; set; }
            public string Hex { get; set; }
            public System.Windows.Media.Brush Brush { get; set; }
        }
        public class StyleOption
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
        private void StyleCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StyleCombo.SelectedItem is ComboBoxItem item)
            {
                int styleId = int.Parse(item.Tag.ToString());

                var style = WbsStyleFactory.GetStyle(styleId);

                var list = new List<StylePreviewItem>();

                foreach (var kv in style)
                {
                    var c = kv.Value.Fill;

                    list.Add(new StylePreviewItem
                    {
                        Level = kv.Key,
                        Hex = $"#{c.R:X2}{c.G:X2}{c.B:X2}",
                        Brush = new SolidColorBrush(
                            System.Windows.Media.Color.FromRgb(c.R, c.G, c.B))
                    });
                }

                PreviewList.ItemsSource = list;
            }
        }

        private void TitleBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
                DragMove();
        }

        private void BtnMin_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            WindowState = System.Windows.WindowState.Minimized;
        }

        private void BtnClose_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Close();
        }
    }
}
using System;
using System.Reflection;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Som3a_WPF_UI.ViewModels.Dashboard
{
    public sealed partial class VersionWidgetViewModel : WidgetViewModel
    {
        [ObservableProperty]
        private string _appVersion;

        [ObservableProperty]
        private string _dotNetVersion;

        [ObservableProperty]
        private string _osVersion;

        public VersionWidgetViewModel()
        {
            Title = "Current Version";
            Icon = "Info";
        }

        protected override Task LoadAsync()
        {
            AppVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown";
            DotNetVersion = Environment.Version.ToString();
            OsVersion = Environment.OSVersion.VersionString;
            return Task.CompletedTask;
        }
    }
}

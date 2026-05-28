using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Som3a_WPF_UI.ViewModels.Dashboard
{
    public sealed class VersionWidgetViewModel : WidgetViewModel
    {
        private string _appVersion;
        private string _dotNetVersion;
        private string _osVersion;

        public string AppVersion
        {
            get => _appVersion;
            set => SetProperty(ref _appVersion, value);
        }

        public string DotNetVersion
        {
            get => _dotNetVersion;
            set => SetProperty(ref _dotNetVersion, value);
        }

        public string OsVersion
        {
            get => _osVersion;
            set => SetProperty(ref _osVersion, value);
        }

        public VersionWidgetViewModel()
        {
            Title = "Current Version";
            Icon = "\U000F05D2";
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

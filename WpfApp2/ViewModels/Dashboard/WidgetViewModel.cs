using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Som3a_WPF_UI.ViewModels.Dashboard
{
    public abstract partial class WidgetViewModel : ViewModelBase
    {
        private readonly SemaphoreSlim _loadLock = new SemaphoreSlim(1, 1);

        [ObservableProperty]
        private string _title;

        [ObservableProperty]
        private string _icon;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _errorMessage;

        [ObservableProperty]
        private bool _isLoaded;

        public async Task LoadDataAsync()
        {
            if (IsLoaded) return;
            await RunLoadAsync();
        }

        public async Task RefreshAsync()
        {
            await RunLoadAsync();
        }

        private async Task RunLoadAsync()
        {
            if (!await _loadLock.WaitAsync(0))
                return;

            try
            {
                IsLoading = true;
                ErrorMessage = null;
                await LoadAsync();
                IsLoaded = true;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsLoading = false;
                _loadLock.Release();
            }
        }

        protected abstract Task LoadAsync();

        public virtual void Cleanup()
        {
        }
    }
}

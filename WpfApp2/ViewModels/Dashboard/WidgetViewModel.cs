using System;
using System.Threading;
using System.Threading.Tasks;

namespace Som3a_WPF_UI.ViewModels.Dashboard
{
    public abstract class WidgetViewModel : ViewModelBase
    {
        private readonly SemaphoreSlim _loadLock = new SemaphoreSlim(1, 1);
        private string _title;
        private string _icon;
        private bool _isLoading;
        private string _errorMessage;
        private bool _isLoaded;

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public string Icon
        {
            get => _icon;
            set => SetProperty(ref _icon, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public bool IsLoaded
        {
            get => _isLoaded;
            set => SetProperty(ref _isLoaded, value);
        }

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

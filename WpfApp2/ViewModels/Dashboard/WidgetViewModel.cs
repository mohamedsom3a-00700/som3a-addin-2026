using System.Threading.Tasks;
using Som3a_WPF_UI.ViewModels;

namespace Som3a_WPF_UI.ViewModels.Dashboard
{
    public abstract class WidgetViewModel : ViewModelBase
    {
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
            IsLoading = true;
            ErrorMessage = null;
            try
            {
                await LoadAsync();
                IsLoaded = true;
            }
            catch (System.Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task RefreshAsync()
        {
            IsLoading = true;
            ErrorMessage = null;
            try
            {
                await LoadAsync();
                IsLoaded = true;
            }
            catch (System.Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        protected abstract Task LoadAsync();

        public virtual void Cleanup()
        {
        }
    }
}

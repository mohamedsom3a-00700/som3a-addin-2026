using Som3a_WPF_UI.Services;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Input;

namespace Som3a_WPF_UI.ViewModels
{
    public sealed class SettingControlViewModel : ViewModelBase
    {
        private readonly SettingDefinition _definition;
        private readonly SettingsValidator _validator;
        private object? _currentValue;
        private string? _validationMessage;
        private bool _hasValidationError;

        public SettingControlViewModel(SettingDefinition definition, SettingsValidator validator)
        {
            _definition = definition;
            _validator = validator;
            _currentValue = definition.CurrentValue ?? definition.DefaultValue;

            BrowseCommand = new RelayCommand(_ => OnBrowse());
        }

        public string Key => _definition.Key;
        public string DisplayName => _definition.DisplayName;
        public string? Description => _definition.Description;
        public SettingValueType ValueType => _definition.ValueType;
        public bool IsReadOnly => _definition.IsReadOnly;
        public bool IsEncrypted => _definition.IsEncrypted;
        public object? DefaultValue => _definition.DefaultValue;
        public List<string>? EnumValues => _definition.EnumValues;
        public double? MinValue => _definition.MinValue;
        public double? MaxValue => _definition.MaxValue;
        public double? StepSize => _definition.StepSize;
        public List<ValidationRule>? ValidationRules => _definition.ValidationRules;

        public object? CurrentValue
        {
            get => _currentValue;
            set
            {
                if (SetProperty(ref _currentValue, value))
                {
                    Validate();
                }
            }
        }

        public string? ValidationMessage
        {
            get => _validationMessage;
            private set => SetProperty(ref _validationMessage, value);
        }

        public bool HasValidationError
        {
            get => _hasValidationError;
            private set => SetProperty(ref _hasValidationError, value);
        }

        public ICommand BrowseCommand { get; }

        public void Validate()
        {
            var result = _validator.Validate(_definition, _currentValue);
            if (result.Errors.Count > 0)
            {
                ValidationMessage = result.Errors[0].Message;
                HasValidationError = true;
            }
            else if (result.Warnings.Count > 0)
            {
                ValidationMessage = result.Warnings[0].Message;
                HasValidationError = true;
            }
            else
            {
                ValidationMessage = null;
                HasValidationError = false;
            }
        }

        public void RefreshValue()
        {
            _currentValue = _definition.CurrentValue ?? _definition.DefaultValue;
            OnPropertyChanged(nameof(CurrentValue));
            Validate();
        }

        private static void OnBrowse()
        {
            using var dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
            }
        }
    }
}

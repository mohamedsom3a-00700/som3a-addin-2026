namespace Som3a.Contracts
{
    public interface ISettingsModule
    {
        string ModuleId { get; }
        string ModuleName { get; }

        void RegisterSettings(ISettingsRegistry registry);
        Task<ValidationResult> ValidateAsync();
        Task ExportAsync(string filePath);
        Task ImportAsync(string filePath);
    }
}

using Som3a.Contracts;

namespace Som3a.Validation.Export
{
    public class ExportValidator
    {
        public ValidationResult Validate(ExportRequest request)
        {
            if (request == null)
                return ValidationResult.Failure("Export request cannot be null.");

            if (string.IsNullOrWhiteSpace(request.TargetPath))
                return ValidationResult.Failure("Target path must not be empty.");

            if (request.Data == null || !request.Data.Any())
                return ValidationResult.Failure("No data to export.");

            var dir = Path.GetDirectoryName(request.TargetPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                return ValidationResult.Failure($"Directory does not exist: {dir}");

            return ValidationResult.Success();
        }
    }
}

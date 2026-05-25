namespace Som3a.Contracts
{
    public class ValidationResult
    {
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public List<string> Warnings { get; set; } = new();

        public static ValidationResult Success() => new() { IsSuccess = true };

        public static ValidationResult Failure(string errorMessage) => new()
        {
            IsSuccess = false,
            ErrorMessage = errorMessage
        };

        public static ValidationResult Warning(string warning) => new()
        {
            IsSuccess = true,
            Warnings = new List<string> { warning }
        };
    }
}

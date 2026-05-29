using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Som3a.Bridge
{
    public interface IDurationEstimatorBridge
    {
        Task<DurationCalculationResponse> CalculateAsync(DurationCalculationRequest request, CancellationToken ct = default);
        Task<DurationExportResponse> ExportToExcelAsync(DurationExportRequest request, CancellationToken ct = default);
        Task<BenchmarkSearchResponse> SearchBenchmarksAsync(BenchmarkSearchRequest request, CancellationToken ct = default);
        bool IsAvailable { get; }
    }

    public class DurationCalculationRequest : InteropDtoBase
    {
        public string ActivityId { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal ProductivityRate { get; set; }
        public int CrewSize { get; set; }
        public decimal HoursPerDay { get; set; }
        public List<string>? WorkingDays { get; set; }
        public List<string>? Holidays { get; set; }
        public string? StartDate { get; set; }

        public override string Serialize() => JsonSerializer.Serialize(this, DefaultOptions);
        public override void Deserialize(string json)
        {
            var other = JsonSerializer.Deserialize<DurationCalculationRequest>(json, DefaultOptions);
            if (other != null)
            {
                Id = other.Id; ActivityId = other.ActivityId; Quantity = other.Quantity;
                ProductivityRate = other.ProductivityRate; CrewSize = other.CrewSize;
                HoursPerDay = other.HoursPerDay; WorkingDays = other.WorkingDays;
                Holidays = other.Holidays; StartDate = other.StartDate;
            }
        }

        private static readonly JsonSerializerOptions DefaultOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    }

    public class DurationCalculationResponse
    {
        public string ActivityId { get; set; } = string.Empty;
        public decimal DurationWorkingDays { get; set; }
        public int CalendarDurationDays { get; set; }
        public string? EndDate { get; set; }
        public string? ErrorMessage { get; set; }
        public bool IsSuccess { get; set; }

        public string Serialize() => JsonSerializer.Serialize(this, DefaultOptions);
        public static DurationCalculationResponse? Deserialize(string json) =>
            JsonSerializer.Deserialize<DurationCalculationResponse>(json, DefaultOptions);

        private static readonly JsonSerializerOptions DefaultOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    }

    public class DurationExportRequest : InteropDtoBase
    {
        public List<DurationCalculationResponse> Estimates { get; set; } = new();
        public string FilePath { get; set; } = string.Empty;

        public override string Serialize() => JsonSerializer.Serialize(this, DefaultOptions);
        public override void Deserialize(string json)
        {
            var other = JsonSerializer.Deserialize<DurationExportRequest>(json, DefaultOptions);
            if (other != null)
            {
                Id = other.Id;
                Estimates = other.Estimates ?? new List<DurationCalculationResponse>();
                FilePath = other.FilePath;
            }
        }

        private static readonly JsonSerializerOptions DefaultOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    }

    public class DurationExportResponse
    {
        public bool IsSuccess { get; set; }
        public string? FilePath { get; set; }
        public int RowCount { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class BenchmarkSearchRequest : InteropDtoBase
    {
        public string? TradeCategoryId { get; set; }
        public string? SearchQuery { get; set; }

        public override string Serialize() => JsonSerializer.Serialize(this, DefaultOptions);
        public override void Deserialize(string json)
        {
            var other = JsonSerializer.Deserialize<BenchmarkSearchRequest>(json, DefaultOptions);
            if (other != null)
            {
                Id = other.Id;
                TradeCategoryId = other.TradeCategoryId;
                SearchQuery = other.SearchQuery;
            }
        }

        private static readonly JsonSerializerOptions DefaultOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    }

    public class BenchmarkSearchResponse
    {
        public List<BenchmarkDto> Benchmarks { get; set; } = new();
        public bool IsSuccess { get; set; }
    }

    public class BenchmarkDto
    {
        public string Id { get; set; } = string.Empty;
        public string TradeCategoryId { get; set; } = string.Empty;
        public string ActivityDescription { get; set; } = string.Empty;
        public decimal ProductivityValue { get; set; }
        public string UnitOfMeasure { get; set; } = string.Empty;
        public int CrewSize { get; set; }
    }
}

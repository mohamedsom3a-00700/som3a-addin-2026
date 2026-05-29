using Som3a.Bridge;
using Som3a.DurationEstimator.Benchmarks;
using Som3a.DurationEstimator.Calendar;
using Som3a.DurationEstimator.Engine;

namespace Som3a.DurationEstimator;

public class DurationEstimatorBridgeService : IDurationEstimatorBridge
{
    private readonly IDurationCalculator _calculator;
    private readonly IBenchmarkLibrary _benchmarkLibrary;
    private readonly ICalendarEngine _calendarEngine;
    private volatile bool _isAvailable;

    public bool IsAvailable => _isAvailable;

    public DurationEstimatorBridgeService(
        IDurationCalculator calculator,
        IBenchmarkLibrary benchmarkLibrary,
        ICalendarEngine calendarEngine)
    {
        _calculator = calculator;
        _benchmarkLibrary = benchmarkLibrary;
        _calendarEngine = calendarEngine;
        _isAvailable = true;
    }

    public Task<DurationCalculationResponse> CalculateAsync(DurationCalculationRequest request, CancellationToken ct = default)
    {
        try
        {
            CalendarConfig? calendar = null;

            if (request.WorkingDays?.Count > 0)
            {
                var workingDays = request.WorkingDays
                    .Select(d => Enum.Parse<DayOfWeek>(d, true))
                    .ToList();

                var holidays = request.Holidays?
                    .Select(h => DateTime.Parse(h))
                    .ToList() ?? new List<DateTime>();

                DateTime.TryParse(request.StartDate, out var startDate);

                calendar = new CalendarConfig
                {
                    WorkingDays = workingDays,
                    Holidays = holidays,
                    StartDate = startDate != default ? startDate : DateTime.Today
                };
            }

            DateTime? parsedStartDate = null;
            if (DateTime.TryParse(request.StartDate, out var sd))
                parsedStartDate = sd;

            var result = _calculator.Calculate(
                request.ActivityId,
                request.Quantity,
                request.ProductivityRate,
                request.CrewSize,
                request.HoursPerDay,
                calendar,
                parsedStartDate);

            return Task.FromResult(new DurationCalculationResponse
            {
                ActivityId = result.ActivityId,
                DurationWorkingDays = result.DurationWorkingDays,
                CalendarDurationDays = result.CalendarDurationDays,
                EndDate = result.EndDate?.ToString("yyyy-MM-dd"),
                IsSuccess = true
            });
        }
        catch (Exception ex)
        {
            return Task.FromResult(new DurationCalculationResponse
            {
                ActivityId = request.ActivityId,
                IsSuccess = false,
                ErrorMessage = ex.Message
            });
        }
    }

    public Task<DurationExportResponse> ExportToExcelAsync(DurationExportRequest request, CancellationToken ct = default)
    {
        return Task.FromResult(new DurationExportResponse
        {
            IsSuccess = false,
            ErrorMessage = "Export not implemented in bridge mode — use direct plugin API."
        });
    }

    public Task<BenchmarkSearchResponse> SearchBenchmarksAsync(BenchmarkSearchRequest request, CancellationToken ct = default)
    {
        try
        {
            IEnumerable<ProductivityRate> results;

            if (!string.IsNullOrEmpty(request.TradeCategoryId))
            {
                results = _benchmarkLibrary.GetByTradeCategory(request.TradeCategoryId);
            }
            else if (!string.IsNullOrEmpty(request.SearchQuery))
            {
                results = _benchmarkLibrary.Search(request.SearchQuery);
            }
            else
            {
                results = _benchmarkLibrary.GetAllActive();
            }

            var dtos = results.Select(r => new BenchmarkDto
            {
                Id = r.Id,
                TradeCategoryId = r.TradeCategoryId,
                ActivityDescription = r.ActivityDescription,
                ProductivityValue = r.ProductivityValue,
                UnitOfMeasure = r.UnitOfMeasure,
                CrewSize = r.CrewSize
            }).ToList();

            return Task.FromResult(new BenchmarkSearchResponse
            {
                Benchmarks = dtos,
                IsSuccess = true
            });
        }
        catch (Exception ex)
        {
            return Task.FromResult(new BenchmarkSearchResponse { IsSuccess = false });
        }
    }
}

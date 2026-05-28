using Som3a.Bridge;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Som3a_WPF_UI.Services
{
    public class DurationCalculationService : IDurationEstimatorBridge
    {
        private static readonly List<BenchmarkDto> _benchmarks = new();

        static DurationCalculationService()
        {
            SeedBenchmarks();
        }

        public bool IsAvailable => true;

        public Task<DurationCalculationResponse> CalculateAsync(DurationCalculationRequest request, CancellationToken ct = default)
        {
            try
            {
                var dailyOutput = request.ProductivityRate * request.CrewSize * request.HoursPerDay;
                var duration = request.Quantity / dailyOutput;

                return Task.FromResult(new DurationCalculationResponse
                {
                    ActivityId = request.ActivityId,
                    DurationWorkingDays = Math.Round(duration, 2),
                    CalendarDurationDays = (int)Math.Ceiling(duration),
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
                ErrorMessage = "Excel export not available in this context. Use the Export button in the Duration Estimator page."
            });
        }

        public Task<BenchmarkSearchResponse> SearchBenchmarksAsync(BenchmarkSearchRequest request, CancellationToken ct = default)
        {
            IEnumerable<BenchmarkDto> results = _benchmarks;

            if (!string.IsNullOrEmpty(request.TradeCategoryId))
                results = results.Where(b => b.TradeCategoryId == request.TradeCategoryId);

            if (!string.IsNullOrEmpty(request.SearchQuery))
                results = results.Where(b =>
                    b.ActivityDescription.IndexOf(request.SearchQuery, StringComparison.OrdinalIgnoreCase) >= 0);

            return Task.FromResult(new BenchmarkSearchResponse
            {
                Benchmarks = results.ToList(),
                IsSuccess = true
            });
        }

        private static void SeedBenchmarks()
        {
            _benchmarks.AddRange(new[]
            {
                New("conc-001", "concrete", "Formwork Installation", 25m, "m²", 4),
                New("conc-002", "concrete", "Rebar Installation", 0.8m, "ton", 4),
                New("conc-003", "concrete", "Concrete Pouring", 30m, "m³", 5),
                New("conc-004", "concrete", "Concrete Finishing", 40m, "m²", 3),
                New("conc-005", "concrete", "Curing Application", 200m, "m²", 2),
                New("stl-001", "steel", "Structural Steel Erection", 5m, "ton", 5),
                New("stl-002", "steel", "Steel Welding", 15m, "m", 2),
                New("stl-003", "steel", "Bolted Connections", 20m, "connection", 2),
                New("stl-004", "steel", "Steel Deck Installation", 80m, "m²", 4),
                New("stl-005", "steel", "Metal Roof Panels", 60m, "m²", 4),
                New("mas-001", "masonry", "Concrete Block Laying", 80m, "block", 3),
                New("mas-002", "masonry", "Brick Laying", 400m, "brick", 3),
                New("mas-003", "masonry", "Stone Veneer", 10m, "m²", 2),
                New("mep-001", "mep", "Conduit Installation", 40m, "m", 2),
                New("mep-002", "mep", "Wire Pulling", 200m, "m", 2),
                New("mep-003", "mep", "Electrical Outlet", 10m, "outlet", 1),
                New("mep-004", "mep", "PVC Pipe Installation", 30m, "m", 2),
                New("mep-005", "mep", "Copper Pipe Installation", 15m, "m", 2),
                New("fin-001", "finishes", "Drywall Installation", 50m, "m²", 2),
                New("fin-002", "finishes", "Taping & Mudding", 30m, "m²", 2),
                New("fin-003", "finishes", "Painting", 100m, "m²", 2),
                New("fin-004", "finishes", "Ceramic Tile", 8m, "m²", 2),
                New("fin-005", "finishes", "Suspended Ceiling", 25m, "m²", 3),
                New("ert-001", "earthwork", "Excavation", 200m, "m³", 3),
                New("ert-002", "earthwork", "Backfilling", 100m, "m³", 3),
                New("ert-003", "earthwork", "Trenching", 50m, "m", 3),
                New("roof-001", "roofing", "Asphalt Shingles", 30m, "m²", 3),
                New("roof-002", "roofing", "Membrane Roofing", 25m, "m²", 3),
                New("glz-001", "glazing", "Window Installation", 4m, "unit", 2),
                New("glz-002", "glazing", "Curtain Wall", 5m, "m²", 4),
                New("flr-001", "flooring", "Vinyl Flooring", 30m, "m²", 2),
                New("flr-002", "flooring", "Carpet Installation", 35m, "m²", 2),
            });
        }

        private static BenchmarkDto New(string id, string cat, string desc, decimal rate, string unit, int crew) =>
            new() { Id = id, TradeCategoryId = cat, ActivityDescription = desc, ProductivityValue = rate, UnitOfMeasure = unit, CrewSize = crew };
    }
}

using Som3a.DurationEstimator.Calendar;
using Som3a.DurationEstimator.Engine;
using Som3a.DurationEstimator.Variance;
using Xunit;

namespace Som3a.DurationEstimator.UnitTests
{
    public class ProductivityEngineTests
    {
        private readonly ProductivityEngine _engine = new();

        [Fact]
        public void CalculateWorkingDays_Basic()
        {
            var result = _engine.CalculateWorkingDays(100m, 10m, 2, 8m);
            Assert.Equal(0.625m, result);
        }

        [Fact]
        public void CalculateWorkingDays_ZeroQuantity_ReturnsZero()
        {
            var result = _engine.CalculateWorkingDays(0m, 10m, 2, 8m);
            Assert.Equal(0m, result);
        }

        [Fact]
        public void CalculateWorkingDays_ThrowsOnNegativeQuantity()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                _engine.CalculateWorkingDays(-1m, 10m, 2, 8m));
        }

        [Fact]
        public void CalculateWorkingDays_ThrowsOnZeroRate()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                _engine.CalculateWorkingDays(100m, 0m, 2, 8m));
        }

        [Fact]
        public void CalculateWorkingDays_ThrowsOnZeroCrew()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                _engine.CalculateWorkingDays(100m, 10m, 0, 8m));
        }

        [Fact]
        public void ApplyModifiers_AdditivePercentage()
        {
            var modifiers = new[]
            {
                new ProductivityModifier { Percentage = 10m },
                new ProductivityModifier { Percentage = 5m }
            };
            var result = _engine.ApplyModifiers(100m, modifiers);
            Assert.Equal(115m, result);
        }

        [Fact]
        public void ApplyModifiers_NegativePercentage()
        {
            var modifiers = new[] { new ProductivityModifier { Percentage = -10m } };
            var result = _engine.ApplyModifiers(100m, modifiers);
            Assert.Equal(90m, result);
        }

        [Fact]
        public void ApplyModifiers_ClampedToMinimum()
        {
            var modifiers = new[] { new ProductivityModifier { Percentage = -99.9m } };
            var result = _engine.ApplyModifiers(0.01m, modifiers);
            Assert.True(result > 0);
        }
    }

    public class CalendarEngineTests
    {
        private readonly CalendarEngine _engine = new();

        [Fact]
        public void IsWorkingDay_Monday_ReturnsTrue()
        {
            var config = new CalendarConfig();
            _engine.Configure(config);
            var monday = new DateTime(2026, 6, 1);
            Assert.True(_engine.IsWorkingDay(monday));
        }

        [Fact]
        public void IsWorkingDay_Saturday_ReturnsFalse()
        {
            var config = new CalendarConfig();
            _engine.Configure(config);
            var saturday = new DateTime(2026, 6, 6);
            Assert.False(_engine.IsWorkingDay(saturday));
        }

        [Fact]
        public void IsWorkingDay_Holiday_ReturnsFalse()
        {
            var config = new CalendarConfig
            {
                Holidays = new List<DateTime> { new DateTime(2026, 7, 4) }
            };
            _engine.Configure(config);
            Assert.False(_engine.IsWorkingDay(new DateTime(2026, 7, 4)));
        }

        [Fact]
        public void CalculateEndDate_SkipsWeekends()
        {
            var config = new CalendarConfig();
            _engine.Configure(config);
            var monday = new DateTime(2026, 6, 1);
            var result = _engine.CalculateEndDate(monday, 5m);
            Assert.Equal(new DateTime(2026, 6, 5), result);
        }

        [Fact]
        public void CalculateEndDate_SpansWeekend()
        {
            var config = new CalendarConfig();
            _engine.Configure(config);
            var friday = new DateTime(2026, 6, 5);
            var result = _engine.CalculateEndDate(friday, 3m);
            Assert.Equal(new DateTime(2026, 6, 9), result);
        }

        [Fact]
        public void CalculateEndDate_SkipsHoliday()
        {
            var config = new CalendarConfig
            {
                Holidays = new List<DateTime> { new DateTime(2026, 7, 4) }
            };
            _engine.Configure(config);
            var start = new DateTime(2026, 6, 29);
            var result = _engine.CalculateEndDate(start, 10m);
            Assert.Equal(new DateTime(2026, 7, 10), result);
        }

        [Fact]
        public void CountWorkingDays_WeekRange()
        {
            var config = new CalendarConfig();
            _engine.Configure(config);
            var count = _engine.CountWorkingDays(new DateTime(2026, 6, 1), new DateTime(2026, 6, 5));
            Assert.Equal(5, count);
        }
    }

    public class DurationCalculatorTests
    {
        [Fact]
        public void Calculate_ReturnsDurationEstimate()
        {
            var engine = new ProductivityEngine();
            var calendar = new CalendarEngine();
            var calc = new DurationCalculator(engine, calendar);

            var result = calc.Calculate("ACT-001", 100m, 10m, 2, 8m);

            Assert.Equal("ACT-001", result.ActivityId);
            Assert.Equal(0.625m, result.DurationWorkingDays);
            Assert.Equal(100m, result.Quantity);
            Assert.Equal(10m, result.AppliedProductivityRate);
        }

        [Fact]
        public void Calculate_WithCalendar_ReturnsEndDate()
        {
            var engine = new ProductivityEngine();
            var calendar = new CalendarEngine();
            var calc = new DurationCalculator(engine, calendar);

            var config = new CalendarConfig { StartDate = new DateTime(2026, 6, 1) };
            var result = calc.Calculate("ACT-002", 100m, 10m, 2, 8m, config);

            Assert.NotNull(result.EndDate);
            Assert.True(result.CalendarDurationDays > 0);
        }

        [Fact]
        public void Calculate_WithModifiers_AdjustsRate()
        {
            var engine = new ProductivityEngine();
            var calendar = new CalendarEngine();
            var calc = new DurationCalculator(engine, calendar);

            var modifiers = new[] { new ProductivityModifier { Percentage = 100m } };
            var result = calc.Calculate("ACT-003", 100m, 10m, 2, 8m, modifiers: modifiers);

            Assert.True(result.AppliedProductivityRate > 10m);
            Assert.True(result.DurationWorkingDays < 0.625m);
        }
    }

    public class VarianceAnalyzerTests
    {
        private readonly VarianceAnalyzer _analyzer = new();

        [Fact]
        public void CalculateThreePoint_ReturnsExpectedValues()
        {
            var result = _analyzer.CalculateThreePoint(15m, 10m, 5m, 100m, 2, 8m);

            Assert.Equal(0.42m, result.OptimisticDuration);
            Assert.Equal(0.62m, result.MostLikelyDuration);
            Assert.Equal(1.25m, result.PessimisticDuration);
            Assert.True(result.ExpectedDuration > 0.6m && result.ExpectedDuration < 0.75m);
            Assert.True(result.StandardDeviation > 0.1m && result.StandardDeviation < 0.15m);
            Assert.True(result.Confidence95Lower < result.ExpectedDuration);
            Assert.True(result.Confidence95Upper > result.ExpectedDuration);
        }

        [Fact]
        public void CalculateThreePoint_ThrowsOnInvalidRates()
        {
            Assert.Throws<ArgumentException>(() =>
                _analyzer.CalculateThreePoint(5m, 10m, 15m, 100m, 2, 8m));
        }

        [Fact]
        public void CalculateFromSingle_RatesEqual()
        {
            var result = _analyzer.CalculateFromSingle(10m, 100m, 2, 8m);

            Assert.True(result.HasSingleRate);
            Assert.Equal(result.OptimisticDuration, result.MostLikelyDuration);
            Assert.Equal(result.OptimisticDuration, result.PessimisticDuration);
            Assert.Equal(0m, result.StandardDeviation);
        }
    }
}

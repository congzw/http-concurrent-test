using System.Collections.Generic;
using System.Linq;

namespace HttpTestWin.ViewModel
{
    public class TestResults
    {
        public TestResults()
        {
            Items = new List<TestResult>();
        }

        public int FailExpiredMs { get; set; }
        public string Uri { get; set; }
        public string Data { get; set; }
        public IList<TestResult> Items { get; set; }
    }

    public class TestResult
    {
        public bool Success { get; set; }
        public double ElapsedMs { get; set; }
        public string Message { get; set; }
    }

    public class TestResultsSummary
    {
        public int SuccessCount { get; set; }
        public int FailCount { get; set; }
        public int TotalCount { get; set; }
        public double MaxElapsedMs { get; set; }
        public double MinElapsedMs { get; set; }
        public double AvgElapsedMs { get; set; }

        public static TestResultsSummary Create(IList<TestResult> results)
        {
            var summary = new TestResultsSummary();
            summary.SuccessCount = results.Count(x => x.Success);
            summary.FailCount = results.Count(x => !x.Success);
            summary.TotalCount = results.Count;
            summary.MaxElapsedMs = results.Max(x => x.ElapsedMs);
            summary.MinElapsedMs = results.Min(x => x.ElapsedMs);
            summary.AvgElapsedMs = results.Average(x => x.ElapsedMs);
            return summary;
        }
    }
}

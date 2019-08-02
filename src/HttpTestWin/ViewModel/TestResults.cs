using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;

namespace HttpTestWin.ViewModel
{
    public class TestResults
    {
        public TestResults()
        {
            Items = new List<TestResult>();
        }

        public string HttpMethod { get; set; }
        public int MaxParallelCount { get; set; }
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

    public class TestResultsHelper
    {
        public static string CreateResultsDesc(TestResults results, TestResultsSummary summary)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Test Case");
            sb.AppendLine("===================");
            sb.AppendLine(string.Format("CreateAt: {0}", DateHelper.Instance.GetNowAsFormat()));
            sb.AppendLine(string.Format("Uri:{0}", results.Uri));
            sb.AppendLine(string.Format("HttpMethod:{0}", results.HttpMethod));
            sb.AppendLine(string.Format("MaxParallelCount:{0}", results.MaxParallelCount));
            sb.AppendLine(string.Format("ConcurrentCount:{0}", results.Items.Count));
            sb.AppendLine(string.Format("FailExpiredMs:{0}", results.FailExpiredMs));
            sb.AppendLine(string.Format("Data:{0}", results.Data));

            sb.AppendLine();
            sb.AppendLine("===================");
            sb.AppendLine(string.Format("Passed: {0}/{1} = {2:0.00}%",
                summary.SuccessCount,
                summary.TotalCount,
                ((double)summary.SuccessCount / summary.TotalCount * 100)));

            sb.AppendLine(string.Format("Elapsed: Max:{0:0.00} ms Min:{1:0.00} ms Avg:{2:0.00} ms",
                summary.MaxElapsedMs,
                summary.MinElapsedMs,
                summary.AvgElapsedMs));

            sb.AppendLine();
            sb.AppendLine(string.Format("Total Fail: {0}", summary.FailCount));
            sb.AppendLine("===================");
            var failResults = results.Items.Where(x => !x.Success);
            foreach (var item in failResults)
            {
                sb.AppendLine(item.Message);
            }

            sb.AppendLine();
            sb.AppendLine(string.Format("Total Success: {0}", summary.SuccessCount));
            sb.AppendLine("===================");
            var successResults = results.Items.Where(x => x.Success);
            foreach (var item in successResults)
            {
                sb.AppendLine(item.Message);
            }
            
            return sb.ToString();
        }
    }
}

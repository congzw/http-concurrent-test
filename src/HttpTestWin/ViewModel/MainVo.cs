using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common;

namespace HttpTestWin.ViewModel
{
    public class MainVo
    {
        private readonly ISimpleConfigFile _simpleConfigFile;
        private readonly IWebApiTester _webApiHelper;
        private readonly ISimpleLog _simpleLog;

        public MainVo(ISimpleConfigFile simpleConfigFile, IWebApiTester webApiHelper)
        {
            _simpleConfigFile = simpleConfigFile;
            _webApiHelper = webApiHelper;
            _simpleLog = SimpleLogFactory.Instance.CreateLogFor(this);
        }

        public HttpTestConfig LoadConfig()
        {
            var config = AsyncHelper.RunSync(() => _simpleConfigFile.ReadFile<HttpTestConfig>(null));
            if (config == null)
            {
                config = HttpTestConfig.Instance;
            }
            return config;
        }

        public void SaveConfig(HttpTestConfig config)
        {
            AsyncHelper.RunSync(() => _simpleConfigFile.SaveFile(config));
        }

        public Task<TestResults> StartTest(HttpTestConfig config, CancellationToken? ct = null)
        {
            var testResults = new TestResults();
            if (config == null)
            {
                return Task.FromResult(testResults);
            }

            testResults.FailExpiredMs = config.FailExpiredMs;
            testResults.Uri = config.LastTestUri;
            testResults.Data = config.LastTestData;
            testResults.MaxParallelCount = config.MaxParallelCount;
            testResults.HttpMethod = config.HttpMethod;

            #region not real parallel test

            ////this will not block current thread
            //for (int i = 0; i < config.ConcurrentCount; i++)
            //{
            //    var index = i;
            //    var testResult = AsyncHelper.RunSync(() => RunHttpTest(index, testResults.Uri, testResults.FailExpiredMs));
            //    results.Add(testResult);
            //}
            //_simpleLog.Log("Concurrent Test Completed.");

            #endregion

            #region parallel demos

            //Threads on a non-threaded machine:
            //=> Parallel
            //=>     --  --  --
            //=> /              \
            //=> >---- --  --  --  -- ---->>

            //Threads on a threaded machine:
            //=>     ------
            //=>    /      \
            //=> >-------------->>

            #endregion

            var theToken = ct ?? CancellationToken.None;
            var results = new ConcurrentBag<TestResult>();
            var taskCompletionSource = new TaskCompletionSource<TestResults>();

            var parallelOptions = new ParallelOptions() { CancellationToken = theToken, MaxDegreeOfParallelism = config.MaxParallelCount };
            Parallel.For(0, config.ConcurrentCount, parallelOptions, i =>
            {
                var index = i;
                var testResult = AsyncHelper.RunSync(() => RunHttpTest(index, testResults.Uri, testResults.FailExpiredMs));
                results.Add(testResult);
            });

            testResults.Items = results.ToList();
            taskCompletionSource.SetResult(testResults);
            return taskCompletionSource.Task;
        }

        private async Task<TestResult> RunHttpTest(int index, string uri, int failExpiredMs)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            //var isOk = AsyncHelper.RunSync(() => _webApiHelper.CheckTargetStatus(uri, failExpiredMs));
            var isOk = await _webApiHelper.TestHttpGet(uri, failExpiredMs).ConfigureAwait(false);
            stopwatch.Stop();
            var testResult = new TestResult();
            testResult.Success = isOk;
            testResult.ElapsedMs = stopwatch.ElapsedMilliseconds;
            testResult.Message = string.Format("{0} => {1}, take {2:0.00} ms",
                index,
                isOk ? "Success" : "Fail",
                stopwatch.ElapsedMilliseconds);
            _simpleLog.Log(testResult.Message);
            return testResult;
        }

        public string CreateResultsDesc(TestResults results, TestResultsSummary summary)
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

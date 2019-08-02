using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common;

namespace HttpTestWin.ViewModel
{
    public class TestClientSpanApiVo
    {
        protected readonly ISimpleConfigFile SimpleConfigFile;
        protected readonly IWebApiTester WebApiHelper;
        protected readonly ISimpleLog SimpleLog;

        public TestClientSpanApiVo(ISimpleConfigFile simpleConfigFile, IWebApiTester webApiHelper)
        {
            SimpleConfigFile = simpleConfigFile;
            WebApiHelper = webApiHelper;
            SimpleLog = SimpleLogFactory.Instance.CreateLogFor(this);
        }

        public Task<TestResults> StartTest(HttpTestConfig config, IList<SaveSpansArgs> saveSpansArgs, CancellationToken? ct = null)
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
            
            var theToken = ct ?? CancellationToken.None;
            var results = new ConcurrentBag<TestResult>();
            var taskCompletionSource = new TaskCompletionSource<TestResults>();

            var parallelOptions = new ParallelOptions() { CancellationToken = theToken, MaxDegreeOfParallelism = config.MaxParallelCount };

            Parallel.ForEach(saveSpansArgs, parallelOptions, (span) =>
            {
                var testResult = AsyncHelper.RunSync(() =>
                    RunTestClientSpan(WebApiHelper, testResults.FailExpiredMs, span, config));
                results.Add(testResult);
            });

            testResults.Items = results.ToList();
            taskCompletionSource.SetResult(testResults);
            return taskCompletionSource.Task;
        }

        public IList<SaveSpansArgs> CreateTestClientSpans(HttpTestConfig config)
        {
            var result = new List<SaveSpansArgs>();
            var now = DateHelper.Instance.GetDateNow();
            var ticks = now.Ticks;
            var traceId = "trace_" + ticks;
            var tracerId = "ConcurrentTest-Tracer-" + now.ToString("yyyyMMddHHmmss");

            for (int i = 0; i < config.ConcurrentCount; i++)
            {
                var spanId = "span_" + i.ToString("");
                var saveClientSpan = SaveClientSpan.Create(now, now.AddMilliseconds(10), tracerId, traceId, null, spanId, "FooOp");
                var saveSpansArgs = new SaveSpansArgs();
                saveSpansArgs.Items.Add(saveClientSpan);
                result.Add(saveSpansArgs);
            }
            return result;
        }

        private async Task<TestResult> RunTestClientSpan(IWebApiTester webApiHelper, int failExpiredMs, SaveSpansArgs saveSpansArgs, HttpTestConfig config)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var jsonData = saveSpansArgs.ToJson(false);
            var saveOk = await webApiHelper.TestHttpPost(config.GetSaveSpansApiUri(), jsonData, failExpiredMs).ConfigureAwait(false);

            stopwatch.Stop();
            var testResult = new TestResult();
            testResult.Success = saveOk;
            testResult.ElapsedMs = stopwatch.ElapsedMilliseconds;
            var itemsCount = saveSpansArgs.Items.Count;
            var saveClientSpan = saveSpansArgs.Items.First();
            testResult.Message = string.Format("{0}(1/{1}) => {2} , take {3:0.00} ms",
                saveClientSpan.SpanId,
                itemsCount,
                saveOk ? "Success" : "Fail",
                stopwatch.ElapsedMilliseconds);
            SimpleLog.Log(testResult.Message);
            return testResult;
        }
    }
}
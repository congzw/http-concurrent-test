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

        public Task<TestResults> StartTest(HttpTestConfig config, IList<ClientSpan> clientSpans, CancellationToken? ct = null)
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

            Parallel.ForEach(clientSpans, parallelOptions, (span) =>
            {
                var testResult = AsyncHelper.RunSync(() => 
                    RunTestClientSpan(WebApiHelper, testResults.FailExpiredMs, span, config));
                results.Add(testResult);
            });

            testResults.Items = results.ToList();
            taskCompletionSource.SetResult(testResults);
            return taskCompletionSource.Task;
        }

        public IList<ClientSpan> CreateTestClientSpans(HttpTestConfig config)
        {
            var clientSpans = new List<ClientSpan>();
            var now = DateHelper.Instance.GetDateNow();
            var ticks = now.Ticks;
            var traceId = "trace_" + ticks;
            var tracerId = "ConcurrentTest-Tracer-" + now.ToString("yyyyMMddHHmmss");

            for (int i = 0; i < config.ConcurrentCount; i++)
            {
                var spanId = "span_" + i.ToString("");
                var clientSpan = ClientSpan.Create(tracerId, traceId, null, spanId, "FooOp");
                clientSpans.Add(clientSpan);
            }

            return clientSpans;
        }

        private async Task<TestResult> RunTestClientSpan(IWebApiTester webApiHelper, int failExpiredMs,  ClientSpan clientSpan, HttpTestConfig config)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var jsonData = clientSpan.ToJson(false);

            var isOk = false;
            var startOk = await webApiHelper.TestHttpPost(config.GetStartSpanApiUri(), jsonData, failExpiredMs).ConfigureAwait(false);
            if (startOk)
            {
                isOk = await webApiHelper.TestHttpPost(config.GetFinishSpanApiUri(), jsonData, failExpiredMs).ConfigureAwait(false);
            }

            stopwatch.Stop();
            var testResult = new TestResult();
            testResult.Success = isOk;
            testResult.ElapsedMs = stopwatch.ElapsedMilliseconds;
            testResult.Message = string.Format("{0} => {1}, take {2:0.00} ms",
                clientSpan.SpanId,
                isOk ? "Success" : "Fail",
                stopwatch.ElapsedMilliseconds);
            SimpleLog.Log(testResult.Message);
            return testResult;
        }
    }
}
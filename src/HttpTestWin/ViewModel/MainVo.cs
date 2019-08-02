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
        protected readonly ISimpleConfigFile SimpleConfigFile;
        protected readonly IWebApiTester WebApiHelper;
        protected readonly ISimpleLog SimpleLog;

        public MainVo(ISimpleConfigFile simpleConfigFile, IWebApiTester webApiHelper)
        {
            SimpleConfigFile = simpleConfigFile;
            WebApiHelper = webApiHelper;
            SimpleLog = SimpleLogFactory.Instance.CreateLogFor(this);
        }

        public HttpTestConfig LoadConfig()
        {
            var config = AsyncHelper.RunSync(() => SimpleConfigFile.ReadFile<HttpTestConfig>(null));
            if (config == null)
            {
                config = HttpTestConfig.Instance;
            }
            return config;
        }

        public void SaveConfig(HttpTestConfig config)
        {
            AsyncHelper.RunSync(() => SimpleConfigFile.SaveFile(config));
        }

        public virtual Task<TestResults> StartTest(HttpTestConfig config, CancellationToken? ct = null)
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
                var testResult = AsyncHelper.RunSync(() => RunHttpTest(index, testResults.Uri, testResults.FailExpiredMs, testResults.HttpMethod, testResults.Data));
                results.Add(testResult);
            });

            testResults.Items = results.ToList();
            taskCompletionSource.SetResult(testResults);
            return taskCompletionSource.Task;
        }
        
        private async Task<TestResult> RunHttpTest(int index, string uri, int failExpiredMs, string httpMethod, string jsonData = null)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            bool isOk = false;
            if (httpMethod == "Get")
            {
                isOk = await WebApiHelper.TestHttpGet(uri, failExpiredMs).ConfigureAwait(false);
            }
            else
            {
                isOk = await WebApiHelper.TestHttpPost(uri, jsonData, failExpiredMs).ConfigureAwait(false);
            }
            stopwatch.Stop();
            var testResult = new TestResult();
            testResult.Success = isOk;
            testResult.ElapsedMs = stopwatch.ElapsedMilliseconds;
            testResult.Message = string.Format("{0:000} => {1}, take {2:0.00} ms",
                index,
                isOk ? "Success" : "Fail",
                stopwatch.ElapsedMilliseconds);
            SimpleLog.Log(testResult.Message);
            return testResult;
        }
    }
}

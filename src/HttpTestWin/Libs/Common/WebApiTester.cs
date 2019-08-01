using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Common
{
    public interface IWebApiTester
    {
        Task<bool> TestHttpGet(string uri, int timeoutMs);
        Task<bool> TestHttpPost(string uri, string jsonData, int timeoutMs);
    }

    public class WebApiTester: IWebApiTester
    {
        private HttpClient _testHttpClient = null;
        private readonly ISimpleLog _simpleLog;

        public WebApiTester(SimpleLogFactory simpleLogFactory)
        {
            //todo: refactor code
            //var response = await httpClient.GetAsync(uri).ConfigureAwait(false);
            //var handler = new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip };
            //_httpClient = new HttpClient(handler);
            _simpleLog = simpleLogFactory.CreateLogFor(this);
        }

        public async Task<bool> TestHttpGet(string uri, int timeoutMs)
        {
            var timeout = new TimeSpan(0, 0, 0, 0, timeoutMs);
            if (_testHttpClient == null)
            {
                _testHttpClient = new HttpClient { Timeout = timeout };
            }
            else
            {
                if (_testHttpClient.Timeout != timeout)
                {
                    _testHttpClient.Dispose();
                    _testHttpClient = new HttpClient { Timeout = timeout };
                }
            }
            
            try
            {
                var response = await _testHttpClient.GetAsync(uri).ConfigureAwait(false);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _simpleLog.Log(string.Format("GET => {0} 通讯异常: {1}", uri, ex.Message));
                return false;
            }
        }
        
        public async Task<bool> TestHttpPost(string uri, string jsonData, int timeoutMs)
        {
            var timeout = new TimeSpan(0, 0, 0, 0, timeoutMs);
            if (_testHttpClient == null)
            {
                _testHttpClient = new HttpClient { Timeout = timeout };
            }
            else
            {
                if (_testHttpClient.Timeout != timeout)
                {
                    _testHttpClient.Dispose();
                    _testHttpClient = new HttpClient { Timeout = timeout };
                }
            }

            try
            {
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                var response = await _testHttpClient.PostAsync(uri, content).ConfigureAwait(false);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _simpleLog.Log(string.Format("POST => {0} 通讯异常: {1}", uri, ex.Message));
                return false;
            }
        }


        #region for di extensions

        private static readonly IWebApiTester Instance = new WebApiTester(SimpleLogFactory.Instance);
        public static Func<IWebApiTester> Resolve { get; set; } = () => Instance;

        #endregion
    }
}

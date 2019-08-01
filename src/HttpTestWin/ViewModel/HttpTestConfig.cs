using Common;

namespace HttpTestWin.ViewModel
{
    public class HttpTestConfig : SimpleConfig
    {
        public HttpTestConfig()
        {
            MaxParallelCount = 1;
            ConcurrentCount = 10;
            HttpMethod = "Get";
            FailExpiredMs = 50;
        }

        public int MaxParallelCount { get; set; }
        public int ConcurrentCount { get; set; }
        public string HttpMethod { get; set; }
        public int FailExpiredMs { get; set; }

        public string LastTestUri { get; set; }
        public string LastTestData { get; set; }

        public static HttpTestConfig Instance = new HttpTestConfig();
    }
}
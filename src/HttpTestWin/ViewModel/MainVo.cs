using Common;

namespace HttpTestWin.ViewModel
{
    public class MainVo
    {
        private readonly ISimpleConfigFile _simpleConfigFile;

        public MainVo(ISimpleConfigFile simpleConfigFile)
        {
            _simpleConfigFile = simpleConfigFile;
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
    }

    public class HttpTestConfig : SimpleConfig
    {
        public HttpTestConfig()
        {
            ConcurrentCount = 10;
            HttpMethod = "Get";
            FailExpiredMs = 50;
        }

        public int ConcurrentCount { get; set; }
        public string HttpMethod { get; set; }
        public int FailExpiredMs { get; set; }

        public string LastTestUri { get; set; }
        public string LastTestData { get; set; }

        public static HttpTestConfig Instance = new HttpTestConfig();
    }
}

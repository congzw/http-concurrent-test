using Common;
using HttpTestWin.ViewModel;

namespace HttpTestWin.App
{
    public static class SimpleIocExtensions
    {
        public static void InitHttpTest(this SimpleIoc simpleIoc)
        {
            var simpleConfigFile = SimpleConfigFactory.ResolveFile();
            var httpTestConfig =AsyncHelper.RunSync(() => simpleConfigFile.ReadFile<HttpTestConfig>(null));
            if (httpTestConfig == null)
            {
                httpTestConfig = HttpTestConfig.Instance;
                simpleConfigFile.SaveFile(HttpTestConfig.Instance);
            }

            simpleIoc.Register<ISimpleConfigFile>(() => simpleConfigFile);
            simpleIoc.Register<HttpTestConfig>(() => httpTestConfig);

            simpleIoc.Register<IWebApiTester>(() => WebApiTester.Resolve());
            simpleIoc.Register<WebApiTester>(() => (WebApiTester)simpleIoc.Resolve<IWebApiTester>());
            simpleIoc.Register<MainVo>(() => new MainVo(simpleIoc.Resolve<ISimpleConfigFile>(), simpleIoc.Resolve<IWebApiTester>()));

            var logFileEnabled = httpTestConfig.LogFileEnabled;
            SimpleLogFactory.Instance.LogFileEnabledFunc = (category) => logFileEnabled;
        }
    }
}

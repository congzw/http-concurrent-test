using Common;
using HttpTestWin.ViewModel;

namespace HttpTestWin.App
{
    public static class SimpleIocExtensions
    {
        public static void InitHttpTest(this SimpleIoc simpleIoc)
        {
            var httpTestConfig = HttpTestConfig.Instance;
            simpleIoc.Register<ISimpleConfigFile>(() => SimpleConfigFactory.ResolveFile());
            simpleIoc.Register<HttpTestConfig>(() => httpTestConfig);

            simpleIoc.Register<IWebApiTester>(() => WebApiTester.Resolve());
            simpleIoc.Register<WebApiTester>(() => (WebApiTester)simpleIoc.Resolve<IWebApiTester>());
            simpleIoc.Register<MainVo>(() => new MainVo(simpleIoc.Resolve<ISimpleConfigFile>(), simpleIoc.Resolve<IWebApiTester>()));
        }
    }
}

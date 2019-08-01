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

            simpleIoc.Register<IWebApiHelper>(() => WebApiHelper.Resolve());
            simpleIoc.Register<WebApiHelper>(() => (WebApiHelper)simpleIoc.Resolve<IWebApiHelper>());
            simpleIoc.Register<MainVo>(() => new MainVo(simpleIoc.Resolve<ISimpleConfigFile>(), simpleIoc.Resolve<IWebApiHelper>()));
        }
    }
}

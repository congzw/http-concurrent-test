using System;

namespace EzConfigs
{
    public class SimpleConfigFactory
    {
        #region for di extensions

        private static readonly ISimpleConfig Instance = new SimpleConfig();
        private static readonly ISimpleConfigFile InstanceFile = new SimpleConfigFile(SimpleJson.ResolveSimpleJsonFile());
        public static Func<ISimpleConfig> Resolve { get; set; } = () => Instance;
        public static Func<ISimpleConfigFile> ResolveFile { get; set; } = () => InstanceFile;

        #endregion
    }
}
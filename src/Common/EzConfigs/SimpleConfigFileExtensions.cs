using System;
using System.IO;
using System.Threading.Tasks;

namespace EzConfigs
{
    public static class SimpleConfigFileExtensions
    {
        public static string GetDefaultConfigFilePath<T>(this ISimpleConfigFile simpleConfigFile)
        {
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, typeof(T).Name + ".json");
            return filePath;
        }

        public static Task<T> ReadFile<T>(this ISimpleConfigFile simpleConfigFile, T defaultValue) where T : ISimpleConfig
        {
            var filePath = GetDefaultConfigFilePath<T>(simpleConfigFile);
            return simpleConfigFile.ReadFile(filePath, defaultValue);
        }

        public static Task SaveFile<T>(this ISimpleConfigFile simpleConfigFile, T value) where T : ISimpleConfig
        {
            var filePath = GetDefaultConfigFilePath<T>(simpleConfigFile);
            return simpleConfigFile.SaveFile(filePath, value);
        }
    }
}

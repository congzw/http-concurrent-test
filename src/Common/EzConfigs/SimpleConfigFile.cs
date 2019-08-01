using System.Threading.Tasks;

namespace EzConfigs
{
    public class SimpleConfigFile : ISimpleConfigFile
    {
        private readonly ISimpleJsonFile _simpleJsonFile;

        public SimpleConfigFile(ISimpleJsonFile simpleJsonFile)
        {
            _simpleJsonFile = simpleJsonFile;
        }

        public Task<T> ReadFile<T>(string filePath, T defaultValue) where T : ISimpleConfig
        {
            return _simpleJsonFile.ReadFileAsSingle<T>(filePath);
        }

        public Task SaveFile<T>(string filePath, T config) where T : ISimpleConfig
        {
            return _simpleJsonFile.SaveFileAsSingle(filePath, config, true);
        }
    }
}
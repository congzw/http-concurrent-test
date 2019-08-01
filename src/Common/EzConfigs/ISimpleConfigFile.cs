using System.Threading.Tasks;

namespace EzConfigs
{
    public interface ISimpleConfigFile
    {
        Task<T> ReadFile<T>(string filePath, T defaultValue) where T : ISimpleConfig;
        Task SaveFile<T>(string filePath, T config) where T : ISimpleConfig;
    }
}
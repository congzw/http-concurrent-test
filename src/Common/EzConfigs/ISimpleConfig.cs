namespace EzConfigs
{
    public interface ISimpleConfig
    {
        void AddOrUpdate<T>(string key, T value);
        T TryGet<T>(string key, T defaultValue);
    }
}

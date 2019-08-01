namespace EzConfigs
{
    public static class SimpleConfigExtensions
    {
        public static T TryGetModel<T>(this ISimpleConfig simpleConfig, T defaultValue)
        {
            return simpleConfig.TryGet(typeof(T).FullName, defaultValue);
        }

        public static void AddOrUpdateModel<T>(this ISimpleConfig simpleConfig, T value)
        {
            simpleConfig.AddOrUpdate(typeof(T).FullName, value);
        }
    }
}

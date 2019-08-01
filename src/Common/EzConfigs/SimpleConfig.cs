using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace EzConfigs
{
    public class SimpleConfig : ISimpleConfig
    {
        private readonly object _lock = new object();

        public SimpleConfig()
        {
            Items = new ConcurrentDictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        }

        public IDictionary<string, object> Items { get; set; }

        public void AddOrUpdate<T>(string key, T value)
        {
            lock (_lock)
            {
                Items[key] = value;
            }
        }

        public T TryGet<T>(string key, T defaultValue)
        {
            lock (_lock)
            {
                if (!Items.ContainsKey(key))
                {
                    return defaultValue;
                }
                return (T)Items[key];
            }
        }
    }
}
using System;

namespace Helios.Common.Concepts.Configuration
{
    public abstract class Configuration
    {
        protected static T ReadOrDefault<T>(string appSetting, T defaultValue)
        {
            try
            {
                if (typeof(T) == typeof(int))
                    return (T)Convert.ChangeType(int.Parse(appSetting), typeof(T));
                if (typeof(T) == typeof(uint))
                    return (T)Convert.ChangeType(uint.Parse(appSetting), typeof(T));
                if (typeof(T) == typeof(string))
                    return (T)Convert.ChangeType(appSetting, typeof(T));

                return defaultValue;
            }
            catch (Exception e)
            {
                return defaultValue;
            }
        }
    }
}
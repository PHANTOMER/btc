using System;
using Newtonsoft.Json;

namespace Btc.Common.Extensions
{
    public static class StringExtensions
    {
        public static T ToEnum<T>(this string source, T defaultValue = default(T)) where T : struct
        {
            return !Enum.TryParse(source, true, out T result) ? defaultValue : result;
        }

        public static bool IsNullOrEmpty(this string source)
        {
            return string.IsNullOrEmpty(source);
        }

        public static ulong ToULong(this string source, ulong defaultValue = default(ulong))
        {
            return ulong.TryParse(source, out ulong result) ? result : defaultValue;
        }

        public static T FromJson<T>(this string source)
        {
            return JsonConvert.DeserializeObject<T>(source);
        }

        public static string ToJson<T>(this T source)
        {
            var settings = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented
            };

            return JsonConvert.SerializeObject(source, settings);
        }
    }
}

using Microsoft.Extensions.Configuration;

namespace Extensions {
    public static class ConfigurationNullExtension {
        public static string TryGet(this IConfiguration config, string key) {
            var value = config.GetValue<string>(key);
            if (value is null) {
                throw new ArgumentNullException($"Please provide a value for key: {key}");
            }
            return value;
        }

    }
}
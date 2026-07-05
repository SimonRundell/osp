/**
 * Reads application configuration from config.json in the application directory.
 *
 * © 2026 Exeter College — Creative Commons NC-BY-SA 4.0
 */
using System;
using System.IO;
using Newtonsoft.Json;

namespace OSPTracker.Services
{
    public class AppSettings
    {
        [JsonProperty("apiBaseUrl")] public string ApiBaseUrl { get; set; }
    }

    public static class AppConfig
    {
        private static AppSettings _settings;

        /// <summary>Returns the loaded application settings, loading from disk on first call.</summary>
        public static AppSettings Settings
        {
            get
            {
                if (_settings == null) Load();
                return _settings;
            }
        }

        private static void Load()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
            if (!File.Exists(path))
                throw new FileNotFoundException("config.json not found next to the executable.", path);

            string json = File.ReadAllText(path);
            _settings = JsonConvert.DeserializeObject<AppSettings>(json)
                        ?? throw new InvalidOperationException("config.json is empty or invalid.");
        }
    }
}

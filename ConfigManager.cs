using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SanyaLex
{
    class ConfigManager
    {
        private static JsonObject? _cachedSettings = null;

        public static string GetConfigDirectory() 
        {
            string userConfig = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string sanyaDir = Path.Combine(userConfig, "sanya");

            if (!Directory.Exists(sanyaDir))
            {
                Directory.CreateDirectory(sanyaDir);
            }

            return sanyaDir;
        }

        public static string GetDataDirectory()
        {
            string localShare = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string sanyaDataDir = Path.Combine(localShare, "sanya", "models");

            if (!Directory.Exists(sanyaDataDir))
            {
                Directory.CreateDirectory(sanyaDataDir);
            }

            return sanyaDataDir;
        }

        public static string GetSettingsPath()
        {
            return Path.Combine(GetConfigDirectory(), "settings.json");
        }

        private static JsonObject LoadSettings()
        {
            if (_cachedSettings != null) return _cachedSettings;

            string settingsPath = GetSettingsPath();
            if (!File.Exists(settingsPath))
            {
                CreateDefaultSettingsFile(settingsPath);
            }

            try
            {
                string jsonContent = File.ReadAllText(settingsPath);
                _cachedSettings = JsonNode.Parse(jsonContent)?.AsObject() ?? new JsonObject();
            }
            catch
            {
                _cachedSettings = new JsonObject();
            }

            return _cachedSettings;
        }

        private static void CreateDefaultSettingsFile(string path)
        {
            var defaultSettings = new JsonObject
            {
                ["_comment"] = "Sanya-Lex Configuration. Update paths or leave defaults.",
                ["database_path"] = Path.Combine(GetConfigDirectory(), "sanya-ltm.db"),
                ["model_path"] = Path.Combine(GetDataDirectory(), "model.onnx"),
                ["tokenizer_path"] = Path.Combine(GetDataDirectory(), "tokenizer.json")
            };

            var options = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(path, defaultSettings.ToJsonString(options));
        }

        public static string GetDatabasePath()
        {
            string? envPath = Environment.GetEnvironmentVariable("SANYA_LEX_DB_PATH");
            if (!string.IsNullOrWhiteSpace(envPath)) return envPath;

            var settings = LoadSettings();
            if (settings["database_path"] != null)
            {
                return ExpandPath(settings["database_path"]!.GetValue<string>());
            }

            return Path.Combine(GetConfigDirectory(), "sanya-ltm.db");
        }

        public static string GetModelPath()
        {
            string? envPath = Environment.GetEnvironmentVariable("SANYA_LEX_MODEL_PATH");
            if (!string.IsNullOrWhiteSpace(envPath)) return envPath;

            var settings = LoadSettings();
            if (settings["model_path"] != null)
            {
                return ExpandPath(settings["model_path"]!.GetValue<string>());
            }

            return Path.Combine(GetDataDirectory(), "model.onnx");
        }

        public static string GetTokenizerPath()
        {
            string? envPath = Environment.GetEnvironmentVariable("SANYA_LEX_TOKENIZER_PATH");
            if (!string.IsNullOrWhiteSpace(envPath)) return envPath;

            var settings = LoadSettings();
            if (settings["tokenizer_path"] != null)
            {
                return ExpandPath(settings["tokenizer_path"]!.GetValue<string>());
            }

            return Path.Combine(GetDataDirectory(), "tokenizer.json");
        }

        private static string ExpandPath(string path)
        {
            if (path.StartsWith("~"))
            {
                string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                return Path.Combine(home, path.Substring(2));
            }
            return path;
        }
    }
}
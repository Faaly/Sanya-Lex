namespace SanyaLex
{
    class ConfigManager
{
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

        public static string GetModelPath()
        {
            string? envPath = Environment.GetEnvironmentVariable("SANYA_LEX_MODEL_PATH");
            if (!string.IsNullOrWhiteSpace(envPath))
            {
                return envPath;
            }

            return Path.Combine(GetDataDirectory(), "model.onnx");
        }

        public static string GetTokenizerPath()
        {
            string? envPath = Environment.GetEnvironmentVariable("SANYA_LEX_TOKENIZER_PATH");
            if (!string.IsNullOrWhiteSpace(envPath))
            {
                return envPath;
            }

            return Path.Combine(GetDataDirectory(), "tokenizer.json");
        }

        public static string GetDatabasePath()
        {
            string? envPath = Environment.GetEnvironmentVariable("SANYA_LEX_DB_PATH");
            if (!string.IsNullOrWhiteSpace(envPath))
            {
                return envPath;
            }

            return Path.Combine(GetConfigDirectory(), "sanya-ltm.db");
        }
    }
}
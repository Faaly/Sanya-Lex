using SanyaLex.Database;

namespace SanyaLex
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length == 0) return;

            string dbPath = "/home/faaly/SANYA/sanya-ltm.db";
            string modelPath = "/home/faaly/SANYA/sanya-lex_model/model.onnx";
            string tokenizerPath = "/home/faaly/SANYA/sanya-lex_model/tokenizer.json";

            using var dbManager = new DbManager(dbPath);
            using var embManager = new EmbeddingManager(modelPath, tokenizerPath);

            dbManager.Initialize();

            dbManager.GetAllKnowledge();

            if (args[0] == "--ltmAdd")
            {
                string textToStore = string.Join(" ", args.Skip(1));
                try
                {
                    float[] vector = embManager.GetEmbedding(textToStore);
                    dbManager.SaveEntry(textToStore, vector);
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    Console.Error.WriteLine($"[ERROR] {ex.ParamName}");
                    Console.Error.WriteLine($"[STACKTRACE] {ex.StackTrace}");
                }
            }
            else
            {
                string userInput = string.Join(" ", args);
                try
                {
                    float[] vector = embManager.GetEmbedding(userInput);
                    var matches = dbManager.FindMatch(vector);

                    if (matches != null && matches.Length > 0) Console.Write("- " + string.Join("\n- ", matches));
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    Console.Error.WriteLine($"[ERROR] {ex.ParamName}");
                    Console.Error.WriteLine($"[STACKTRACE] {ex.StackTrace}");
                }
            }
        }
    }
}
using SanyaLex.Database;

namespace SanyaLex
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length == 0) return;

            string dbPath = "/home/faaly/SANYA/sanya-ltm.db";
            string modelPath = "/home/faaly/Repos/Sanya-Lex/sanya-lex_model/model.onnx";
            string tokenizerPath = "/home/faaly/Repos/Sanya-Lex/sanya-lex_model/tokenizer.json";

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
                string match = "";
                try
                {
                    float[] vector = embManager.GetEmbedding(userInput);
                    match = dbManager.FindMatch(vector);

                    if (!string.IsNullOrEmpty(match)) Console.Write(match);
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    Console.Error.WriteLine($"[ERROR] {ex.ParamName}");
                    Console.Error.WriteLine($"[STACKTRACE] {ex.StackTrace}");
                }
            }

            // Console.WriteLine("\n\n\nSanya-Lex is ready. Type something! (end with Strg+C)");

            // while (true)
            // {
            //     Console.Write("> ");
            //     string? input = await Console.In.ReadLineAsync();

            //     if (string.IsNullOrWhiteSpace(input)) continue;

            //     try
            //     {
            //         float[] vector = embManager.GetEmbedding(input);
            //         //dbManager.SaveEntry(input, vector);
            //          dbManager.FindMatch(vector);
            //     }
            //     catch (ArgumentOutOfRangeException ex)
            //     {
            //         Console.WriteLine($"[ERROR] {ex.ParamName}");
            //         Console.WriteLine($"[STACKTRAEC] {ex.StackTrace}");
            //     }
            // }
        }
    }
}

/*
If input has invoke-command remember/-r  > Data will be saved in db
If input has no command > RAG


*/
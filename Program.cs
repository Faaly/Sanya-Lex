using SanyaLex.Database;

namespace SanyaLex
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string dbPath = "/home/faaly/SANYA/sanya-ltm.db";
            string modelPath = "/home/faaly/Repos/Sanya-Lex/sanya-lex_model/model.onnx";
            string tokenizerPath = "/home/faaly/Repos/Sanya-Lex/sanya-lex_model/tokenizer.json";

            using var dbManager = new DbManager(dbPath);
            using var embManager = new EmbeddingManager(modelPath, tokenizerPath);

            dbManager.Initialize();

            dbManager.GetAllKnowledge();
            Console.WriteLine("\n\n\nSanya-Lex is ready. Type something! (end with Strg+C)");

            while (true)
            {
                Console.Write("> ");
                string? input = await Console.In.ReadLineAsync();
                
                if (string.IsNullOrWhiteSpace(input)) continue;

                try
                {
                    float[] vector = embManager.GetEmbedding(input);
                    dbManager.SaveEntry(input, vector);
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    Console.WriteLine($"[ERROR] {ex.ParamName}");
                    Console.WriteLine($"[STACKTRAEC] {ex.StackTrace}");
                }
            }
        }
    }

}
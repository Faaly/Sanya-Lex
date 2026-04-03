using SanyaLex.Database;

namespace SanyaLex
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length == 0) return;

            if (args.Length < 2) 
            {
                Console.WriteLine("Usage: sanya-lex <flag> <content>");
                return;
            }

            string dbPath = "/home/faaly/SANYA/sanya-ltm.db";
            string modelPath = "/home/faaly/SANYA/sanya-lex_model/model.onnx";
            string tokenizerPath = "/home/faaly/SANYA/sanya-lex_model/tokenizer.json";

            using var dbManager = new DbManager(dbPath);
            using var embManager = new EmbeddingManager(modelPath, tokenizerPath);

            dbManager.Initialize();

            string flag = args[0];
            string content = string.Join(" ", args.Skip(1));

            try
            {
                switch (flag)
                {
                    case "--ltmAdd":

                        if (args.Length < 3) 
                            {
                                Console.Error.WriteLine("[ERROR] Missing arguments. Usage: --ltmAdd [--fact|--tool] <content>");
                                return; 
                            }
                            
                        string category = args[1] == "--tool" ? "Tool" : "Fact";
                        content = string.Join(" ", args.Skip(2));

                        dbManager.SaveEntry(content, embManager.GetEmbedding(content), category);
                        break;

                    case "--tool":
                        var toolMatches = dbManager.FindMatch(embManager.GetEmbedding(content), "Tool");
                        if (toolMatches?.Length > 0) Console.Write("- " + string.Join("\n- ", toolMatches));
                        break;

                    case "--fact":
                        var factMatches = dbManager.FindMatch(embManager.GetEmbedding(content), "Fact");
                        if (factMatches?.Length > 0) Console.Write("- " + string.Join("\n- ", factMatches));
                        break;
                        
                    default:
                        string fullInput = string.Join(" ", args);
                        var generalMatches = dbManager.FindMatch(embManager.GetEmbedding(fullInput), "Fact"); 
                        if (generalMatches?.Length > 0) Console.Write("- " + string.Join("\n- ", generalMatches));
                        break;
                }
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Console.Error.WriteLine($"[ERROR] {ex.ParamName}");
                Console.Error.WriteLine($"[STACKTRACE] {ex.StackTrace}");
            }
        }
    }
}
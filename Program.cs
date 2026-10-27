using SanyaLex.Database;
using System.Text.Json.Nodes;

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

            string dbPath = ConfigManager.GetDatabasePath();
            
            string modelPath = ConfigManager.GetModelPath();
            if (!File.Exists(modelPath))
            {
                Console.Error.WriteLine($"[ERROR] ONNX model not found at: {modelPath}");
                Console.Error.WriteLine("[HINT] Please download the model file or set SANYA_LEX_MODEL_PATH.");
                Environment.Exit(1);
            }

            string tokenizerPath = ConfigManager.GetTokenizerPath();
            if (!File.Exists(tokenizerPath))
            {
                Console.Error.WriteLine($"[ERROR] Tokenizer file not found at: {tokenizerPath}");
                Console.Error.WriteLine("[HINT] Please provide tokenizer.json or set SANYA_LEX_TOKENIZER_PATH.");
                Environment.Exit(1);
            }

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
                        var toolMatches = dbManager.FindMatch(embManager.GetEmbedding(content), "Tool", content);

                        var toolEnvelope = new JsonObject
                        {
                            ["type"] = "tool"
                        };

                        var jsonArray = new JsonArray();

                        if (toolMatches != null)
                        {
                            foreach (var match in toolMatches)
                            {
                                
                                var functionObject = new JsonObject
                                {
                                    ["name"] = match.FunctionName,
                                    ["description"] = match.Content,
                                    ["parameters"] = JsonNode.Parse(match.ToolArgs ?? "{}")
                                    
                                };
                                var toolNode = new JsonObject
                                {
                                    ["type"] = "function",
                                    ["function"] = functionObject
                                };

                                jsonArray.Add((JsonNode)toolNode);
                            
                            }
                        }
                        toolEnvelope["data"] = jsonArray;
                        Console.Write(toolEnvelope.ToJsonString());
                        break;

                    case "--fact":
                        var factMatches = dbManager.FindMatch(embManager.GetEmbedding(content), "Fact", content);
                        
                        var factEnvelope = new JsonObject
                        {
                            ["type"] = "fact",
                            ["data"] = factMatches?.Length > 0 ? "- " + string.Join(" - ", factMatches.Select(m => m.Content)) : ""
                        };

                        Console.Write(factEnvelope.ToJsonString());
                        break;
                        
                    default:
                        string fullInput = string.Join(" ", args);
                        var generalMatches = dbManager.FindMatch(embManager.GetEmbedding(fullInput), "Fact", fullInput); 
                        var defaultEnvelope = new JsonObject
                        {
                            ["type"] = "fact",
                            ["data"] = generalMatches?.Length > 0 ? "- " + string.Join(" - ", generalMatches.Select(m => m.Content)) : ""
                        };
                        Console.Write(defaultEnvelope.ToJsonString());
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
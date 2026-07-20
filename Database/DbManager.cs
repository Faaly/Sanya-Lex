using System.Numerics;
using Microsoft.Data.Sqlite;

namespace SanyaLex.Database
{
    class DbManager : IDisposable
    {
        private readonly SqliteConnection _connection;
        public DbManager(string dbPath)
        {
            _connection = new SqliteConnection($"Data Source={dbPath}");
        }

        public void Initialize()
        {
            _connection.Open();

            var createTableSql = @"
                CREATE TABLE IF NOT EXISTS Knowledge (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    FunctionName TEXT,
                    Content TEXT NOT NULL,
                    Vector BLOB NOT NULL,
                    Category TEXT NOT NULL,
                    ToolArgs TEXT
                );";

            using var command = _connection.CreateCommand();
            command.CommandText = createTableSql;
            command.ExecuteNonQuery();
            

            try {
                using var migrationCmd = _connection.CreateCommand();
                migrationCmd.CommandText = "ALTER TABLE Knowledge ADD COLUMN Category TEXT NOT NULL DEFAULT 'General';";
                migrationCmd.ExecuteNonQuery();
            } catch { }
            
        }

        public void SaveEntry(string content, float[] vector, string category, string? functionName = null, string? toolArgs = null)
        {
            byte[] bytes = new byte[vector.Length * sizeof(float)];
            Buffer.BlockCopy(vector, 0, bytes, 0, bytes.Length);

            var insertSql = "INSERT INTO Knowledge (Content, Vector, Category, FunctionName, ToolArgs) VALUES ($content, $vector, $category, $functionName, $toolArgs)";

            using var command = _connection.CreateCommand();
            command.CommandText = insertSql;
            command.Parameters.AddWithValue("$content", content);
            command.Parameters.AddWithValue("$vector", bytes);
            command.Parameters.AddWithValue("$category", category);
            command.Parameters.AddWithValue("$functionName", (object?)functionName ?? DBNull.Value);
            command.Parameters.AddWithValue("$toolArgs", (object?)toolArgs ?? DBNull.Value);
            command.ExecuteNonQuery();
        }

        public List<(string text, float[] extractVector,string? functionName, string? toolArgs)> GetAllKnowledge(string category)
        {
            var extractSql = "SELECT Content, Vector, FunctionName, ToolArgs FROM Knowledge WHERE Category = $category";
            using var command = _connection.CreateCommand();
            command.CommandText = extractSql;
            command.Parameters.AddWithValue("$category", category);

            using var reader = command.ExecuteReader();
            var list = new List<(string text, float[] extractVector, string? functionName, string? toolArgs)>();
            while (reader.Read())
            {
                string text = reader.GetString(0);

                byte[] bytes = (byte[])reader.GetValue(1);
                //float4byte
                float[] extractVector = new float[bytes.Length / sizeof(float)];

                Buffer.BlockCopy(bytes, 0, extractVector, 0, bytes.Length);

                string? functionName = reader.IsDBNull(2) ? null : reader.GetString(2);
                string? toolArgs = reader.IsDBNull(3) ? null : reader.GetString(3);
                list.Add((text, extractVector, functionName, toolArgs));
            }

            return list;
        }

        public float CalculateSimilarity(float[] vectorA, float[] vectorB)
        {
            float score = 0;
            for (int i = 0; i < vectorA.Length; i++)
            {
                score += vectorA[i] * vectorB[i];
            }

            return score;
        }

        public KnowledgeMatch[]? FindMatch(float[] questionVector, string category, string rawQuestionText = "")
        {
            List<(float Score, KnowledgeMatch Match)> candidates = new();

            var memories = GetAllKnowledge(category);
            
            float threshold = (category == "Tool") ? 0.65f : 0.85f;

            string[]? questionWords = rawQuestionText?.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);

            foreach (var memory in memories)
            {
                float currentScore = CalculateSimilarity(questionVector, memory.extractVector);

                int keywordIndex = memory.text.IndexOf("|||KEYWORDS:", StringComparison.OrdinalIgnoreCase);

                if (keywordIndex != -1 && category == "Tool" && questionWords != null)
                {

                    string pureKeywords = memory.text.Substring(keywordIndex).ToLower();
                    // Console.WriteLine($"DEBUG of Tool: Keywords: {pureKeywords}");
                    

                    foreach(var word in questionWords)
                    {
                        // Console.WriteLine($"DEBUG of Tool: single QuestionWord: {word}");
                        if (word.Length > 3 && pureKeywords.Contains(word))
                        {
                            currentScore += 0.05f;
                            // Console.WriteLine($"DEBUG of Tool: hit: {word}");
                        }
                    }
                }

                // Console.WriteLine($"DEBUG: Current Score of Match {memory.text} : {currentScore}");
                if (currentScore > threshold) //Threshold
                {
                    string TextWithoutKeyWords = (keywordIndex != -1) ? memory.text.Substring(0, keywordIndex).Trim() : memory.text;

                    var matchObj = new KnowledgeMatch
                    {
                        FunctionName = memory.functionName,
                        Content = TextWithoutKeyWords,
                        ToolArgs = memory.toolArgs
                    };

                    // Console.WriteLine($"DEBUG: Best Score of Match {memory.text} : {currentScore}");
                    candidates.Add((currentScore, matchObj));
                }
            }

            if (candidates.Count == 0) return null;

            return candidates
                .OrderByDescending(c => c.Score)
                .Take(3)
                .Select(c => c.Match)
                .ToArray();
        }

        public void Dispose() => _connection.Dispose();
    }
}
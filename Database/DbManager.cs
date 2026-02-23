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
                    Content TEXT NOT NULL,
                    Vector BLOB NOT NULL
                );";

            using var command = _connection.CreateCommand();
            command.CommandText = createTableSql;
            command.ExecuteNonQuery();
        }

        public void SaveEntry(string content, float[] vector)
        {
            byte[] bytes = new byte[vector.Length * sizeof(float)];
            Buffer.BlockCopy(vector, 0, bytes, 0, bytes.Length);

            var insertSql = "INSERT INTO Knowledge (Content, Vector) VALUES ($content, $vector)";

            using var command = _connection.CreateCommand();
            command.CommandText = insertSql;
            command.Parameters.AddWithValue("$content", content);
            command.Parameters.AddWithValue("$vector", bytes);
            command.ExecuteNonQuery();
        }

        public List<(string text, float[] extractVector)> GetAllKnowledge()
        {
            var extractSql = "SELECT Content, Vector FROM Knowledge";
            using var command = _connection.CreateCommand();
            command.CommandText = extractSql;

            using var reader = command.ExecuteReader();
            var list = new List<(string text, float[] extractVector)>();
            while (reader.Read())
            {
                string text = reader.GetString(0);

                byte[] bytes = (byte[])reader.GetValue(1);
                //float4byte
                float[] extractVector = new float[bytes.Length / sizeof(float)];

                Buffer.BlockCopy(bytes, 0, extractVector, 0, bytes.Length);
                list.Add((text, extractVector));
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

        public string[]? FindMatch(float[] questionVector)
        {
            List<(float Score, string Text)> candidates = new();

            var memories = GetAllKnowledge();
            foreach (var memory in memories)
            {
                float currentScore = CalculateSimilarity(questionVector, memory.extractVector);
                //Console.WriteLine($"DEBUG: Current Score of Match {memory.text} : {currentScore}");
                if (currentScore > 0.91f) //Threshold
                {
                    //Console.WriteLine($"DEBUG: Best Score of Match {memory.text} : {currentScore}");
                    candidates.Add((currentScore, memory.text));
                }
            }

            if (candidates.Count == 0) return null;

            return candidates
                .OrderByDescending(c => c.Score)
                .Take(3)
                .Select(c => c.Text)
                .ToArray();
        }

        public void Dispose() => _connection.Dispose();
    }
}
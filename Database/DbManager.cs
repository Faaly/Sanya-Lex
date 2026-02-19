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

        public Dictionary<string, float[]> GetAllKnowledge()
        {
            var extractSql = "SELECT Content, Vector FROM Knowledge";
            using var command = _connection.CreateCommand();
            command.CommandText = extractSql;

            using var reader = command.ExecuteReader();
            Dictionary<string, float[]> kvp = new();
            while (reader.Read())
            {
                string text = reader.GetString(0);

                byte[] bytes = (byte[])reader.GetValue(1);
                //float4byte
                float[] extractVector = new float[bytes.Length / sizeof(float)];

                Buffer.BlockCopy(bytes, 0, extractVector, 0, bytes.Length);
                kvp.Add(text, extractVector);
            }

            return kvp;
        }

        public float CalcuateSimilarity(float[] vectorA, float[] vectorB)
        {
            float score = 0;
            for (int i = 0; i < vectorA.Length; i++)
            {
                score += vectorA[i] * vectorB[i];
            }

            return score;
        }

        public void WriteMatch(float[] questionVector)
        {
            float bestScore = 0f;
            string bestMatch = "No match";

            var memories = GetAllKnowledge();
            foreach (var entry in memories)
            {
                float currentScore = CalcuateSimilarity(questionVector, entry.Value);
                if (currentScore > bestScore)
                {
                    bestScore = currentScore;
                    bestMatch = entry.Key;
                }
            }

            if (bestScore > 0.69f)
            {
                Console.WriteLine($"Top Match: {bestScore}: {bestMatch}");
            }
        }

        public void Dispose() => _connection.Dispose();
    }
}
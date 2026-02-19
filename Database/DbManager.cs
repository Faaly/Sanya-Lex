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

        public void GetAllKnowledge()
        {
            var extractSql = "SELECT Content, Vector FROM Knowledge";
            using var command = _connection.CreateCommand();
            command.CommandText = extractSql;

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                string content = reader.GetString(0);

                byte[] bytes = (byte[])reader.GetValue(1);
                    //float4byte
                float[] extractVector = new float[bytes.Length / sizeof(float)];

                Buffer.BlockCopy(bytes, 0, extractVector, 0, bytes.Length);

                Console.WriteLine($"Output: {content}");
            }   
        }

        public void Dispose() => _connection.Dispose();
    }
}
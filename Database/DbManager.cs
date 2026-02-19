using Microsoft.Data.Sqlite;

namespace SanyaLex.Database
{
    class DbManager : IDisposable
    {
        private readonly SqliteConnection _connection;
        // static string dbPath = "/home/faaly/SANYA/sanya-ltm.db";

        public DbManager(string dbPath)
        {
            _connection = new SqliteConnection($"Data Source={dbPath}");
        }

        public void Initialize()
        {
            _connection.Open();
            //code



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
            // Hier müssen wir den float[] Vektor in ein byte[] konvertieren
            // Ein float hat 4 Bytes. Ein 384-Vektor braucht also 1536 Bytes.
            byte[] bytes = new byte[vector.Length * sizeof(float)];
            Buffer.BlockCopy(vector, 0, bytes, 0, bytes.Length);

            var insertSql = "INSERT INTO Knowledge (Content, Vector) VALUES ($content, $vector)";
            
            using var command = _connection.CreateCommand();
            command.CommandText = insertSql;
            command.Parameters.AddWithValue("$content", content);
            command.Parameters.AddWithValue("$vector", bytes);
            command.ExecuteNonQuery();
        }

        public void Dispose() => _connection.Dispose();
    }
}


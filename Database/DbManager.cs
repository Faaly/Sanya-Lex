using Microsoft.Data.Sqlite;

namespace sanya_lex.Database
{
    class DbManager : IDisposable
    {
        private readonly SqliteConnection _connection;
        static string dbPath = "/home/faaly/SANYA/sanya-ltm.db";

        public DbManager(string dbPath)
        {
            string connectionString = $"Data Source={dbPath}";
            _connection = new SqliteConnection(connectionString);
        }

        public void Initialize()
        {
            _connection.Open();
            //code
        }

        public void Dispose()
        {
            _connection.Dispose();
        }
    }
}


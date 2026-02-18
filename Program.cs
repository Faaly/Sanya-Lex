using sanya_lex.Database;

string connectionString = "Data Source=sanya_lexikon.db";

using (var connection = new DbManager(connectionString))
{
    connection.Initialize();
    // Je nachdem was in args[] steht:
    // db.AddInformation(payload);
    // oder
    // var context = db.Search(query);
}

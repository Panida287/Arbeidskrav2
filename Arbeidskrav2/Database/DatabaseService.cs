using Microsoft.Data.Sqlite;
namespace Arbeidskrav2.Database;

public class DatabaseService
{
     private readonly string _connectionString;

    public DatabaseService()
    {
        string dbPath = "marketplace.db";
        _connectionString = $"Data Source={dbPath}";
        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS Users (
                Username    TEXT PRIMARY KEY,
                Password    TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Listings (
                Id              INTEGER PRIMARY KEY AUTOINCREMENT,
                Title           TEXT NOT NULL,
                Description     TEXT NOT NULL,
                Price           REAL NOT NULL,
                Category        INTEGER NOT NULL,
                Condition       INTEGER NOT NULL,
                Status          INTEGER NOT NULL DEFAULT 0,
                SellerUsername  TEXT NOT NULL,
                FOREIGN KEY (SellerUsername) REFERENCES Users(Username)
            );

            CREATE TABLE IF NOT EXISTS Transactions (
                Id              INTEGER PRIMARY KEY AUTOINCREMENT,
                ListingId       INTEGER NOT NULL,
                BuyerUsername   TEXT NOT NULL,
                SellerUsername  TEXT NOT NULL,
                Price           REAL NOT NULL,
                Date            TEXT NOT NULL,
                FOREIGN KEY (ListingId)      REFERENCES Listings(Id),
                FOREIGN KEY (BuyerUsername)  REFERENCES Users(Username),
                FOREIGN KEY (SellerUsername) REFERENCES Users(Username)
            );

            CREATE TABLE IF NOT EXISTS Reviews (
                Id              INTEGER PRIMARY KEY AUTOINCREMENT,
                TransactionId   INTEGER NOT NULL UNIQUE,
                BuyerUsername   TEXT NOT NULL,
                SellerUsername  TEXT NOT NULL,
                Rating          INTEGER NOT NULL,
                Comment         TEXT,
                FOREIGN KEY (TransactionId)  REFERENCES Transactions(Id)
            );
        ";
        cmd.ExecuteNonQuery();
    }
}
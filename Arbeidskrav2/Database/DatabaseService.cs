using Microsoft.Data.Sqlite;
using Arbeidskrav2.Models;
using Arbeidskrav2.Enums;
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
    
    // ── Users ──────────────────────────────────────────

public void SaveUser(string username, string password)
{
    using var connection = new SqliteConnection(_connectionString);
    connection.Open();

    var cmd = connection.CreateCommand();
    cmd.CommandText = @"
        INSERT OR IGNORE INTO Users (Username, Password)
        VALUES (@Username, @Password);
    ";
    cmd.Parameters.AddWithValue("@Username", username);
    cmd.Parameters.AddWithValue("@Password", password);
    cmd.ExecuteNonQuery();
}

public List<(string Username, string Password)> LoadUsers()
{
    var users = new List<(string, string)>();

    using var connection = new SqliteConnection(_connectionString);
    connection.Open();

    var cmd = connection.CreateCommand();
    cmd.CommandText = "SELECT Username, Password FROM Users;";

    using var reader = cmd.ExecuteReader();
    while (reader.Read())
    {
        users.Add((reader.GetString(0), reader.GetString(1)));
    }

    return users;
}

// ── Listings ────────────────────────────────────────

public void SaveListing(Listing listing)
{
    using var connection = new SqliteConnection(_connectionString);
    connection.Open();

    var cmd = connection.CreateCommand();
    cmd.CommandText = @"
        INSERT OR IGNORE INTO Listings (Title, Description, Price, Category, Condition, Status, SellerUsername)
        VALUES (@Title, @Description, @Price, @Category, @Condition, @Status, @SellerUsername);
    ";
    cmd.Parameters.AddWithValue("@Title",          listing.ItemName);
    cmd.Parameters.AddWithValue("@Description",    listing.ItemDescription);
    cmd.Parameters.AddWithValue("@Price",          listing.ItemPrice);
    cmd.Parameters.AddWithValue("@Category",       (int)listing.Category);
    cmd.Parameters.AddWithValue("@Condition",      (int)listing.Condition);
    cmd.Parameters.AddWithValue("@Status",         (int)listing.Status);
    cmd.Parameters.AddWithValue("@SellerUsername", listing.Seller.Username);
    cmd.ExecuteNonQuery();
}

public void UpdateListingStatus(int listingId, ListingStatus status)
{
    using var connection = new SqliteConnection(_connectionString);
    connection.Open();

    var cmd = connection.CreateCommand();
    cmd.CommandText = "UPDATE Listings SET Status = @Status WHERE Id = @Id;";
    cmd.Parameters.AddWithValue("@Status", (int)status);
    cmd.Parameters.AddWithValue("@Id",     listingId);
    cmd.ExecuteNonQuery();
}

public void DeleteListing(int listingId)
{
    using var connection = new SqliteConnection(_connectionString);
    connection.Open();

    var cmd = connection.CreateCommand();
    cmd.CommandText = "DELETE FROM Listings WHERE Id = @Id;";
    cmd.Parameters.AddWithValue("@Id", listingId);
    cmd.ExecuteNonQuery();
}

// ── Transactions ─────────────────────────────────────

public void SaveTransaction(Transaction transaction)
{
    using var connection = new SqliteConnection(_connectionString);
    connection.Open();

    var cmd = connection.CreateCommand();
    cmd.CommandText = @"
        INSERT OR IGNORE INTO Transactions (ListingId, BuyerUsername, SellerUsername, Price, Date)
        VALUES (@ListingId, @BuyerUsername, @SellerUsername, @Price, @Date);
    ";
    cmd.Parameters.AddWithValue("@ListingId",      transaction.Listing.Id);
    cmd.Parameters.AddWithValue("@BuyerUsername",  transaction.Buyer.Username);
    cmd.Parameters.AddWithValue("@SellerUsername", transaction.Seller.Username);
    cmd.Parameters.AddWithValue("@Price",          transaction.Listing.ItemPrice);
    cmd.Parameters.AddWithValue("@Date",           transaction.Date.ToString("o"));
    cmd.ExecuteNonQuery();
}

// ── Reviews ──────────────────────────────────────────

public void SaveReview(Review review)
{
    using var connection = new SqliteConnection(_connectionString);
    connection.Open();

    var cmd = connection.CreateCommand();
    cmd.CommandText = @"
        INSERT OR IGNORE INTO Reviews (TransactionId, BuyerUsername, SellerUsername, Rating, Comment)
        VALUES (@TransactionId, @BuyerUsername, @SellerUsername, @Rating, @Comment);
    ";
    cmd.Parameters.AddWithValue("@TransactionId",  review.TransactionId);
    cmd.Parameters.AddWithValue("@BuyerUsername",  review.Buyer.Username);
    cmd.Parameters.AddWithValue("@SellerUsername", review.Seller.Username);
    cmd.Parameters.AddWithValue("@Rating",         review.ReviewScore);
    cmd.Parameters.AddWithValue("@Comment",       review.ReviewText ?? (object)DBNull.Value);
    cmd.ExecuteNonQuery();
}

public List<(int Id, string ItemName, string ItemDescription, double ItemPrice, Category Category, Condition Condition, ListingStatus Status, string SellerUsername)> LoadListings()
{
    var result = new List<(int, string, string, double, Category, Condition, ListingStatus, string)>();

    using var connection = new SqliteConnection(_connectionString);
    connection.Open();

    var cmd = connection.CreateCommand();
    cmd.CommandText = "SELECT Id, Title, Description, Price, Category, Condition, Status, SellerUsername FROM Listings;";

    using var reader = cmd.ExecuteReader();
    while (reader.Read())
    {
        result.Add((
            reader.GetInt32(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.GetDouble(3),
            (Category)reader.GetInt32(4),
            (Condition)reader.GetInt32(5),
            (ListingStatus)reader.GetInt32(6),
            reader.GetString(7)
        ));
    }
    return result;
}

public List<(int Id, int ListingId, string BuyerUsername, string SellerUsername, double Price, DateTime Date)> LoadTransactions()
{
    var result = new List<(int, int, string, string, double, DateTime)>();

    using var connection = new SqliteConnection(_connectionString);
    connection.Open();

    var cmd = connection.CreateCommand();
    cmd.CommandText = "SELECT Id, ListingId, BuyerUsername, SellerUsername, Price, Date FROM Transactions;";

    using var reader = cmd.ExecuteReader();
    while (reader.Read())
    {
        result.Add((
            reader.GetInt32(0),
            reader.GetInt32(1),
            reader.GetString(2),
            reader.GetString(3),
            reader.GetDouble(4),
            DateTime.Parse(reader.GetString(5))
        ));
    }
    return result;
}

public List<(int TransactionId, string BuyerUsername, string SellerUsername, int Rating, string Comment)> LoadReviews()
{
    var result = new List<(int, string, string, int, string)>();

    using var connection = new SqliteConnection(_connectionString);
    connection.Open();

    var cmd = connection.CreateCommand();
    cmd.CommandText = "SELECT TransactionId, BuyerUsername, SellerUsername, Rating, Comment FROM Reviews;";

    using var reader = cmd.ExecuteReader();
    while (reader.Read())
    {
        result.Add((
            reader.GetInt32(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.GetInt32(3),
            reader.IsDBNull(4) ? null : reader.GetString(4)
        ));
    }
    return result;
}
}
using Arbeidskrav2.Enums;
using Arbeidskrav2.Models;
using Arbeidskrav2.Database;

namespace Arbeidskrav2.Services;

/// <summary>
/// Manages the marketplace, including users, listings and transactions.
/// </summary>
public class Marketplace
{
    // ── Fields ──────────────────────────────
    private DatabaseService db;
    private List<User> users;
    private List<Listing> listings;
    private List<Transaction> transactions;
    private User loggedInUser;

    // ── Constructor ─────────────────────────
    public Marketplace()
    {
        db = new DatabaseService();
        users = new List<User>();
        listings = new List<Listing>();
        transactions = new List<Transaction>();
        loggedInUser = null;
        LoadFromDatabase();
    }
    
    // ── Properties ──────────────────────────
    public List<User> Users
    {
        get => users;
    }

    public List<Listing> Listings
    {
        get => listings;
    }

    public List<Transaction> Transactions
    {
        get => transactions;
    }

    public User LoggedInUser
    {
        get => loggedInUser;
    }
    
    // ── Public Methods ───────────────────────

    /// <summary>Registers a new user.</summary>
    /// <param name="username">The username for the new account.</param>
    /// <param name="password">The password for the new account.</param>
    public string Register(string username, string password)
    {
        if (users.Any(u => u.Username == username))
            throw new InvalidOperationException("Username already taken");

        users.Add(new User(username, password));
        db.SaveUser(username, password);
        return "User successfully registered";
    }

    /// <summary>Logs in a user with the given credentials.</summary>
    /// <param name="username">The username to log in with.</param>
    /// <param name="password">The password to log in with.</param>
    public string Login(string username, string password)
    {
        User user = users.FirstOrDefault(u => u.Username == username);

        if (user == null)
            throw new InvalidOperationException("Username not found");

        if (!user.CheckPassword(password))
            throw new InvalidOperationException("Incorrect password");

        loggedInUser = user;
        return "Logged in success";
    }

    /// <summary>Logs out the currently logged in user.</summary>
    public string Logout()
    {
        {
            loggedInUser = null;
            return "Logged out success";
        }
    }

    /// <summary>Creates a new listing for the logged in user.</summary>
    /// <param name="name">The name of the item.</param>
    /// <param name="description">The description of the item.</param>
    /// <param name="price">The price in NOK.</param>
    /// <param name="category">The category of the item.</param>
    /// <param name="condition">The condition of the item.</param>
    public string CreateListing(string name, string description, double price, Category category, Condition condition)
    {
        string error = CheckIfLoggedIn();
        if (error != null)
            return error;

        Listing listing = new Listing(name, description, price, category, condition, loggedInUser);
        listings.Add(listing);
        loggedInUser.AddListing(listing);
        db.SaveListing(listing);
        return "Listing added successfully";
    }
    
    /// <summary>Returns all listings including sold ones.</summary>
    public List<Listing> GetAllListings()
    {
        return listings.ToList();
    }
    
    /// <summary>Returns only available listings.</summary>
    public List<Listing> GetAvailableListings()
    {
        return listings.Where(l => l.Status == ListingStatus.Available).ToList();
    }

    /// <summary>Searches listings by keyword in title or description.</summary>
    /// <param name="searchTerm">The keyword to search for.</param>
    public List<Listing> SearchListings(string searchTerm)
    {
        return listings.Where(l => 
                l.ItemName.ToLower().Contains(searchTerm.ToLower()) || 
                l.ItemDescription.ToLower().Contains(searchTerm.ToLower()))
            .ToList();
    }

    /// <summary>Returns all available listings in a given category.</summary>
    /// <param name="category">The category to filter by.</param>
    public List<Listing> GetListingsByCategory(Category category)
    {
        return listings.Where(l => l.Category == category).ToList();
    }

    /// <summary>Purchases a listing for the logged in user.</summary>
    /// <param name="listing">The listing to purchase.</param>
    public string Purchase(Listing listing)
    {
        string error = CheckIfLoggedIn()
                       ?? CheckIfAvailable(listing)
                       ?? CheckIfSeller(listing);

        if (error != null)
            throw new InvalidOperationException(error);
        
        listing.Status = ListingStatus.Sold;
        Transaction transaction = new Transaction(listing, loggedInUser, listing.Seller);
        loggedInUser.AddTransaction(transaction);
        transactions.Add(transaction);
        listing.Seller.AddTransaction(transaction);
        db.UpdateListingStatus(listing.Id, ListingStatus.Sold);
        db.SaveTransaction(transaction);
        return "Item successfully purchased";
    }

    /// <summary>Writes a review for a completed transaction.</summary>
    /// <param name="transaction">The transaction to review.</param>
    /// <param name="score">The rating score from 1 to 6.</param>
    /// <param name="reviewText">The optional text comment.</param>
    public string WriteReview(Transaction transaction, int score, string reviewText)
    {
        string error = CheckIfLoggedIn()
                       ?? CheckIfBuyer(transaction)
                       ?? CheckIfReviewed(transaction);

        if (error != null)
            throw new InvalidOperationException(error);

        Review review = new Review(score, reviewText, loggedInUser, transaction.Seller, transaction.Listing, transaction.Id);
        transaction.AddReview(review);
        transaction.Seller.AddReview(review);
        db.SaveReview(review);
        return "Review submitted successfully";
    }

    /// <summary>Edits an existing listing owned by the logged in user.</summary>
    /// <param name="listing">The listing to edit.</param>
    /// <param name="name">The new name.</param>
    /// <param name="description">The new description.</param>
    /// <param name="price">The new price.</param>
    /// <param name="category">The new category.</param>
    /// <param name="condition">The new condition.</param>
    public string EditListing(Listing listing, string name, string description, double price, Category category,
        Condition condition)
    {
        string error = CheckIfLoggedIn();
        if (error != null)
            throw new InvalidOperationException(error);

        if (listing.Seller != loggedInUser)
            throw new InvalidOperationException("You can only edit your own listings");

        if (!string.IsNullOrWhiteSpace(name))
            listing.ItemName = name;
        if (!string.IsNullOrWhiteSpace(description))
            listing.ItemDescription = description;
        if (price > 0)
            listing.ItemPrice = price;
        listing.Category = category;
        listing.Condition = condition;

        return "Listing updated successfully";
    }

    /// <summary>Deletes a listing owned by the logged in user.</summary>
    /// <param name="listing">The listing to delete.</param>
    public string DeleteListing(Listing listing)
    {
        string error = CheckIfLoggedIn();
        if (error != null)
            throw new InvalidOperationException(error);

        if (listing.Seller != loggedInUser)
            throw new InvalidOperationException("You can only delete your own listings");

        listings.Remove(listing);
        loggedInUser.Listings.Remove(listing);
        db.DeleteListing(listing.Id);
        return "Listing deleted successfully";
    }
    
    
    // ── Private Helpers ──────────────────────
    
    /// <summary>Checks if a user is currently logged in.</summary>
    /// <returns>An error message if not logged in, otherwise null.</returns>
    private string CheckIfLoggedIn()
    {
        if (loggedInUser == null)
            return "You must be logged in";
        return null;
    }
    
    /// <summary>Checks if a listing is available for purchase.</summary>
    /// <param name="listing">The listing to check.</param>
    /// <returns>An error message if not available, otherwise null.</returns>
    private string CheckIfAvailable(Listing listing)
    {
        if (listing.Status != ListingStatus.Available)
            return "This listing is not available";
        return null;
    }

    /// <summary>Checks if the logged in user is the seller of a listing.</summary>
    /// <param name="listing">The listing to check.</param>
    /// <returns>An error message if the user is the seller, otherwise null.</returns>
    private string CheckIfSeller(Listing listing)
    {
        if (listing.Seller == loggedInUser)
            return "You cannot buy your own listing";
        return null;
    }
    
    /// <summary>Checks if the logged in user is the buyer of a transaction.</summary>
    /// <param name="transaction">The transaction to check.</param>
    /// <returns>An error message if the user is not the buyer, otherwise null.</returns>
    private string CheckIfBuyer(Transaction transaction)
    {
        if (transaction.Buyer != loggedInUser)
            return "You can only review your own purchases";
        return null;
    }

    /// <summary>Checks if a transaction has already been reviewed.</summary>
    /// <param name="transaction">The transaction to check.</param>
    /// <returns>An error message if already reviewed, otherwise null.</returns>
    private string CheckIfReviewed(Transaction transaction)
    {
        if (transaction.Review != null)
            return "You have already reviewed this transaction";
        return null;
    }
    
    /// <summary>Loads all persisted data from the SQLite database into memory on startup.</summary>
    private void LoadFromDatabase()
{
    // Load users
    foreach (var (username, password) in db.LoadUsers())
    {
        users.Add(new User(username, password));
    }

    // Load listings
    foreach (var (id, itemName, itemDescription, itemPrice, category, condition, status, sellerUsername) in db.LoadListings())
    {
        User seller = users.FirstOrDefault(u => u.Username == sellerUsername);
        if (seller == null) continue;

        Listing listing = new Listing(itemName, itemDescription, itemPrice, category, condition, seller);
        listing.SetId(id);
        listings.Add(listing);
        seller.AddListing(listing);
    }

    // Load transactions
    foreach (var (id, listingId, buyerUsername, sellerUsername, price, date) in db.LoadTransactions())
    {
        User buyer = users.FirstOrDefault(u => u.Username == buyerUsername);
        User seller = users.FirstOrDefault(u => u.Username == sellerUsername);
        Listing listing = listings.FirstOrDefault(l => l.Id == listingId);
        if (buyer == null || seller == null || listing == null) continue;

        Transaction transaction = new Transaction(listing, buyer, seller);
        transaction.SetId(id);
        transaction.SetDate(date);
        transactions.Add(transaction);
        buyer.AddTransaction(transaction);
        seller.AddTransaction(transaction);
    }

    // Load reviews
    foreach (var (transactionId, buyerUsername, sellerUsername, rating, comment) in db.LoadReviews())
    {
        Transaction transaction = transactions.FirstOrDefault(t => t.Id == transactionId);
        User buyer = users.FirstOrDefault(u => u.Username == buyerUsername);
        User seller = users.FirstOrDefault(u => u.Username == sellerUsername);
        if (transaction == null || buyer == null || seller == null) continue;

        Review review = new Review(rating, comment, buyer, seller, transaction.Listing, transactionId);
        transaction.AddReview(review);
        seller.AddReview(review);
    }
}
    
}
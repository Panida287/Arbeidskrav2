using Arbeidskrav2.Enums;
using Arbeidskrav2.Models;

namespace Arbeidskrav2.Services;

/// <summary>
/// Manages the marketplace, including users, listings and transactions.
/// </summary>
public class Marketplace
{
    // ── Fields ──────────────────────────────
    private List<User> users;
    private List<Listing> listings;
    private List<Transaction> transactions;
    private User loggedInUser;

    // ── Constructor ─────────────────────────
    public Marketplace()
    {
        users = new List<User>();
        listings = new List<Listing>();
        transactions = new List<Transaction>();
        loggedInUser = null;
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

    private string CheckIfLoggedIn()
    {
        if (loggedInUser == null)
            return "You must be logged in";
        return null;
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

        Review review = new Review(score, reviewText, loggedInUser, transaction.Seller, transaction.Listing);
        transaction.AddReview(review);
        transaction.Seller.AddReview(review);
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
        return "Listing deleted successfully";
    }
    
    
    // ── Private Helpers ──────────────────────
    
    private string CheckIfAvailable(Listing listing)
    {
        if (listing.Status != ListingStatus.Available)
            return "This listing is not available";
        return null;
    }

    private string CheckIfSeller(Listing listing)
    {
        if (listing.Seller == loggedInUser)
            return "You cannot buy your own listing";
        return null;
    }

    private string CheckIfBuyer(Transaction transaction)
    {
        if (transaction.Buyer != loggedInUser)
            return "You can only review your own purchases";
        return null;
    }

    private string CheckIfReviewed(Transaction transaction)
    {
        if (transaction.Review != null)
            return "You have already reviewed this transaction";
        return null;
    }
    
}
using Arbeidskrav2.Enums;
using Arbeidskrav2.Models;

namespace Arbeidskrav2.Services;

public class Marketplace
{
    private List<User> users;
    private List<Listing> listings;
    private List<Transaction> transactions;
    private User loggedInUser;

    public Marketplace()
    {
        users = new List<User>();
        listings = new List<Listing>();
        transactions = new List<Transaction>();
        loggedInUser = null;
    }

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

    public string Register(string username, string password)
    {
        if (users.Any(u => u.Username == username))
            throw new InvalidOperationException("Username already taken");

        users.Add(new User(username, password));
        return "User successfully registered";
    }

    public string Login(string username, string password)
    {
        User user = users.FirstOrDefault(u => u.Username == username);

        if (user == null)
            return "Username not found";

        if (!user.CheckPassword(password))
            return "Incorrect password";

        loggedInUser = user;
        return "Logged in success";
    }

    private string CheckIfLoggedIn()
    {
        if (loggedInUser == null)
            return "You must be logged in";
        return null;
    }

    public string Logout()
    {
        {
            loggedInUser = null;
            return "Logged out success";
        }
    }

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

    public List<Listing> GetListings()
    {
        return listings.Where(l => l.Status == ListingStatus.Available).ToList();
    }

    public List<Listing> SearchListings(string searchTerm)
    {
        return listings.Where(l => l.ItemName.Contains(searchTerm) || l.ItemDescription.Contains(searchTerm)).ToList();
    }

    public List<Listing> GetListingsByCategory(Category category)
    {
        return listings.Where(l => l.Category == category).ToList();
    }

    public string Purchase(Listing listing)
    {
        string error = CheckIfLoggedIn()
                       ?? CheckIfAvailable(listing)
                       ?? CheckIfSeller(listing);

        if (error != null)
            return error;
        listing.Status = ListingStatus.Sold;
        Transaction transaction = new Transaction(listing, loggedInUser, listing.Seller);
        loggedInUser.AddTransaction(transaction);
        transactions.Add(transaction);
        listing.Seller.AddTransaction(transaction);
        return "Item successfully purchased";
    }

    public string WriteReview(Transaction transaction, int score, string reviewText)
    {
        string error = CheckIfLoggedIn()
                       ?? CheckIfBuyer(transaction)
                       ?? CheckIfReviewed(transaction);

        if (error != null)
            return error;

        Review review = new Review(score, reviewText, loggedInUser, transaction.Seller, transaction.Listing);
        transaction.AddReview(review);
        transaction.Seller.AddReview(review);
        return "Review submitted successfully";
    }

    public string EditListing(Listing listing, string name, string description, double price, Category category,
        Condition condition)
    {
        string error = CheckIfLoggedIn();
        if (error != null)
            return error;

        if (listing.Seller != loggedInUser)
            return "You can only edit your own listings";

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

    public string DeleteListing(Listing listing)
    {
        string error = CheckIfLoggedIn();
        if (error != null)
            return error;

        if (listing.Seller != loggedInUser)
            return "You can only delete your own listings";

        listings.Remove(listing);
        loggedInUser.Listings.Remove(listing);
        return "Listing deleted successfully";
    }
}
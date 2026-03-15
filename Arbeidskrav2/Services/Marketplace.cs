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
    
    public List<User> Users { get => users; }
    public List<Listing> Listings { get => listings; }
    public List<Transaction> Transactions { get => transactions; }
    public User LoggedInUser { get => loggedInUser; }

    public string Register(string username, string password)
    {
        if (users.Any(u => u.Username == username))
            return "Username already taken";
    
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

    public string Logout()
    {
        if (loggedInUser  != null)
        {
            loggedInUser = null;
            return  "Logged out success";
        }
        return "Please log in";
    }

    public string CreateListing(string name, string description, double price, Category category, Condition condition)
    {
        if (loggedInUser == null)
            return "You must be logged in to create a listing";

        Listing listing = new Listing(name, description, price, category, condition, loggedInUser);
        listings.Add(listing);
        loggedInUser.AddListing(listing);
        return "Listing added successfully";
    }

    public List<Listing> GetListings()
    {
        return  listings.Where(l => l.Status == ListingStatus.Available).ToList();
    }

    public List<Listing> SearchListings(string searchTerm)
    {
        return listings.Where(l => l.ItemName.Contains(searchTerm) || l.ItemDescription.Contains(searchTerm)).ToList();
    }

    public List<Listing> GetListingsByCategory(Category category)
    {
        return listings.Where(l => l.Category == category).ToList();
    }
}



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
}
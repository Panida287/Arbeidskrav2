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
}



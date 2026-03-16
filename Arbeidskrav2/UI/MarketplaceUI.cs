using Arbeidskrav2.Enums;
using Arbeidskrav2.Models;
using Arbeidskrav2.Services;

namespace Arbeidskrav2.UI;

public class MarketplaceUI
{
    private Marketplace marketplace;

    public MarketplaceUI(Marketplace marketplace)
    {
        this.marketplace = marketplace;
    }

    public void ShowMainMenu()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("=== Second Hand Market ===");
            Console.WriteLine("1. Login");
            Console.WriteLine("2. Register");
            Console.WriteLine("3. Visit Marketplace as guest");
            Console.Write("Select an option: ");

            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    Login();
                    break;
                case "2":
                    Register();
                    break;
                case "3":
                    ViewMarketplace();
                    break;
                default:
                    Console.WriteLine("Invalid option, try again!");
                    break;
            }
        }
    }

    private string ReadPassword()
    {
        string password = "";
        ConsoleKeyInfo key;
        do
        {
            key = Console.ReadKey(intercept: true);
            if (key.Key != ConsoleKey.Enter)
            {
                password += key.KeyChar;
                Console.Write("*");
            }
        } while (key.Key != ConsoleKey.Enter);

        Console.WriteLine();
        return password;
    }

    public void Register()
    {
        Console.Clear();
        Console.Write("Enter your name: "); // TODO add char condition
        string name = Console.ReadLine();

        Console.Write("Enter your password: "); // TODO add char condition
        string password = ReadPassword();

        while (true)
        {
            Console.Write("Repeat your password: ");
            string passwordRepeat = ReadPassword(); // TODO add char condition

            if (password == passwordRepeat)
                break;
            Console.WriteLine("Passwords don't match, try again!");
            // TODO option to write new password
        }

        string result = marketplace.Register(name, password);
        Console.WriteLine(result);
    }

    public void Login()
    {
        Console.Clear();
        Console.Write("Enter username: ");
        string name = Console.ReadLine();

        Console.Write("Enter password: ");
        string password = ReadPassword();

        string result = marketplace.Login(name, password);
        Console.WriteLine(result);

        if (result == "Logged in success")
            ShowLoggedInMenu();
    }

    public void ShowLoggedInMenu()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine($"Welcome, {marketplace.LoggedInUser.Username}! What would you like to do?");
            Console.WriteLine("1. Enter marketplace");
            Console.WriteLine("2. View my profile and manage listings");
            Console.WriteLine("3. Logout");
            Console.Write("Select an option: ");

            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    Console.Clear();
                    ViewMarketplace();
                    break;
                case "2":
                    ViewProfile();
                    break;
                case "3":
                    string result = marketplace.Logout();
                    Console.WriteLine(result);
                    return;
                default:
                    Console.WriteLine("Invalid option, try again!");
                    break;
            }
        }
    }

    public void ViewMarketplace()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("== Welcome to marketplace ==!");
            Console.WriteLine("1. Browse all listings");
            Console.WriteLine("2. Browse by category");
            Console.WriteLine("3. Search listings");
            Console.WriteLine("4. Go back");
            Console.Write("Select an option: ");

            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    Console.Clear();
                    ShowAllListing();
                    break;
                case "2":
                    BrowseByCategory();
                    break;
                case "3":
                    SearchListing();
                    break;
                case "4":
                    return;
                default:
                    Console.WriteLine("Invalid option, try again!");
                    break;
            }
        }
    }

    private void ShowListingsAndSelect(List<Listing> listings)
    {
        if (listings.Count == 0)
        {
            Console.WriteLine("No listings available. Press any key to go back.");
            Console.ReadKey();
            return;
        }

        Console.WriteLine($"{"#",-5} {"Title",-20} {"Category",-15} {"Condition",-12} {"Price"}");
        Console.WriteLine(new string('-', 60));

        for (int i = 0; i < listings.Count; i++)
        {
            Console.WriteLine(
                $"{i + 1,-5} {listings[i].ItemName,-20} {listings[i].Category,-15} {listings[i].Condition,-12} {listings[i].ItemPrice:N0} kr");
        }

        Console.WriteLine("\n0. Go back");

        while (true)
        {
            Console.Write("Select a listing to view: ");
            string input = Console.ReadLine();

            if (input == "0") return;

            if (int.TryParse(input, out int index) && index >= 1 && index <= listings.Count)
            {
                ShowListingDetails(listings[index - 1]);
                return;
            }

            Console.WriteLine("Invalid selection, try again!");
        }
    }

    public void ShowAllListing()
    {
        List<Listing> availableListings = marketplace.GetListings();
        Console.Clear();
        Console.WriteLine("=== Available Listings ===");
        ShowListingsAndSelect(availableListings);
    }

    public void ShowListingDetails(Listing listing) //TODO : block guests from purchasing
    {
        Console.Clear();
        Console.WriteLine($"=== {listing.ItemName} ===");
        Console.WriteLine($"{"Seller:",-15} {listing.Seller.Username}");
        Console.WriteLine($"{"Category:",-15} {listing.Category}");
        Console.WriteLine($"{"Condition:",-15} {listing.Condition}");
        Console.WriteLine($"{"Price:",-15} {listing.ItemPrice:N0} kr");
        Console.WriteLine($"{"Description:",-15} {listing.ItemDescription}");

        Console.WriteLine("\n1. Buy this item");
        Console.WriteLine("0. Go back");
        Console.Write("Select an option: ");

        string choice = Console.ReadLine();

        switch (choice)
        {
            case "1": // TODO block seller to buy own item
                Console.Write("Are you sure you want to buy this item? (y/n): ");
                string confirm = Console.ReadLine().ToLower();
                if (confirm == "y")
                {
                    string result = marketplace.Purchase(listing);
                    Console.WriteLine(result);
                    Console.ReadKey();
                }
                break;
            case "0":
                return;
            default:
                Console.WriteLine("Invalid option!");
                break;
        }
    }

    public void BrowseByCategory()
    {
        Console.Clear();
        Console.WriteLine("=== Browse by Category ===");

        Category[] categories = (Category[])Enum.GetValues(typeof(Category));

        for (int i = 0; i < categories.Length; i++)
        {
            int count = marketplace.GetListingsByCategory(categories[i]).Count;
            Console.WriteLine($"{i + 1,-5} {categories[i],-20} ({count} items)");
        }

        Console.WriteLine("\n0. Go back");

        while (true)
        {
            Console.Write("Select a category: ");
            string input = Console.ReadLine();

            if (input == "0") return;

            if (int.TryParse(input, out int index) && index >= 1 && index <= categories.Length)
            {
                Category selected = categories[index - 1];
                List<Listing> categoryListings = marketplace.GetListingsByCategory(selected);
                Console.WriteLine($"\n=== {selected} Listings ===");
                ShowListingsAndSelect(categoryListings);
                return;
            }

            Console.WriteLine("Invalid selection, try again!");
        }
    }

    public void SearchListing()
    {
        Console.Clear();
        Console.WriteLine("=== Search listings===");
        Console.Write("What are you looking for?: ");
        string searchTerm = Console.ReadLine();

        List<Listing> searchedListing = marketplace.SearchListings(searchTerm);
        ShowListingsAndSelect(searchedListing);
    }
    
    private string GetStarRating(double rating)
    {
        int fullStars = (int)Math.Round(rating);
        int emptyStars = 6 - fullStars;
    
        return new string('★', fullStars) + new string('☆', emptyStars);
    }
    
    //TODO : add option to review purchased items
    private void ShowTransactions(List<Transaction> transactions, string otherPartyLabel)
    {
        if (transactions.Count == 0)
        {
            Console.WriteLine("No transactions found. Press any key to go back.");
            Console.ReadKey();
            return;
        }

        Console.WriteLine($"{"#",-5} {"Item",-20} {"Price",-10} {otherPartyLabel}");
        Console.WriteLine(new string('-', 60));

        for (int i = 0; i < transactions.Count; i++)
        {
            string otherParty = transactions[i].Buyer == marketplace.LoggedInUser 
                ? transactions[i].Seller.Username 
                : transactions[i].Buyer.Username;
            
            Console.WriteLine($"{i + 1,-5} {transactions[i].Listing.ItemName,-20} {transactions[i].Listing.ItemPrice:N0,-10} {otherParty}");
        }
    
        Console.ReadKey();
    }
    
    private void ShowReviews(List<Review> reviews)
    {
        if (reviews.Count == 0)
        {
            Console.WriteLine("No reviews yet. Press any key to go back.");
            Console.ReadKey();
            return;
        }

        Console.WriteLine($"{"#",-5} {"Rating",-10} {"From",-15} {"Comment"}");
        Console.WriteLine(new string('-', 60));

        for (int i = 0; i < reviews.Count; i++)
        {
            string stars = GetStarRating(reviews[i].ReviewScore);
            Console.WriteLine($"{i + 1,-5} {stars,-10} {reviews[i].Buyer.Username,-15} {reviews[i].ReviewText}");
        }

        Console.ReadKey();
    }

    public void ViewProfile()
    {
        while (true)
        {
            double averageRating = marketplace.LoggedInUser.Reviews.Any()
                ? marketplace.LoggedInUser.Reviews.Average(r => r.ReviewScore)
                : 0;

            string stars = GetStarRating(averageRating);

            List<Listing> myListings = marketplace.LoggedInUser.Listings;
            List<Transaction> myPurchases = marketplace.LoggedInUser.Transactions
                .Where(t => t.Buyer == marketplace.LoggedInUser)
                .ToList();
            List<Transaction> mySales = marketplace.LoggedInUser.Transactions
                .Where(t => t.Seller == marketplace.LoggedInUser)
                .ToList();
            List<Review> reviews = marketplace.LoggedInUser.Reviews;

            Console.Clear();
            Console.WriteLine($"===Profile: {marketplace.LoggedInUser.Username}===");
            Console.WriteLine($"Average Rating: {stars} ({averageRating}/6)");
            Console.WriteLine();
            Console.WriteLine($"{"1. My Listings:",-20} ({(myListings.Count)})");
            Console.WriteLine($"{"2. My Purchases",-20} ({(myPurchases.Count)})");
            Console.WriteLine($"{"3. Listings Sold",-20} ({(mySales.Count)})");
            Console.WriteLine($"{"4. Reviews Received",-20} ({(reviews.Count)})");
            Console.WriteLine("5. Create New Listing");
            Console.WriteLine("6. Go back");
            Console.WriteLine("0. Logout");
            Console.Write("Select an option : ");

            string choice = Console.ReadLine();

            switch (choice)
            {
                case "0":
                    string result = marketplace.Logout();
                    Console.WriteLine(result);
                    return;
                case "1":
                    Console.Clear();
                    ShowListingsAndSelect(myListings);
                    break;
                case "2":
                    Console.Clear();
                    ShowTransactions(myPurchases, "Seller");
                    break;
                case "3":
                    Console.Clear();
                    ShowTransactions(mySales, "Buyer");
                    break;
                case "4":
                    Console.Clear();
                    ShowReviews(reviews);
                    break;
                case "5":
                    Console.Clear();
                    // CreateListing();
                    break;
                case "6":
                    return;
            }
        }
    }

    public void CreateListing()
    {
        Category[] categories = (Category[])Enum.GetValues(typeof(Category));
        Condition[] conditions = (Condition[])Enum.GetValues(typeof(Condition));
        
        Console.Clear();
        Console.WriteLine("=== Create New Listing ===");
        Console.WriteLine();
        Console.WriteLine("Item name: ");
        string name = Console.ReadLine(); //TODO limit char
        Console.WriteLine("Item description: ");
        string description = Console.ReadLine(); //TODO limit char
        Console.WriteLine("Price (kr)");
        double price = double.Parse(Console.ReadLine()); //TODO only legit int
        Category selectedCategory = categories[0];
        Condition selectedCondition = conditions[0];
        
        Console.Write("Select a category: ");
        for (int i = 0; i < categories.Length; i++)
            Console.WriteLine($"{i + 1,-5} {categories[i]}");
        
        while (true)
        {
            string input = Console.ReadLine();

            if (int.TryParse(input, out int index) && index >= 1 && index <= categories.Length)
            {
                selectedCategory = categories[index - 1];
                break;
            }

            Console.WriteLine("Invalid selection, try again!");
        }
        
        
        Console.WriteLine("Select a condition:");
        for (int i = 0; i < categories.Length; i++)
            Console.WriteLine($"{i + 1,-5} {conditions[i]}");
        while (true)
        {
            Console.Write("Select a category: ");
            string input = Console.ReadLine();

            if (int.TryParse(input, out int index) && index >= 1 && index <= conditions.Length)
            {
                selectedCondition = conditions[index - 1];
                break;
            }

            Console.WriteLine("Invalid selection, try again!");
        }
        
        string result = marketplace.CreateListing(name, description, price, selectedCategory, selectedCondition);
        Console.WriteLine(result);
    }
    
    
    
    
}
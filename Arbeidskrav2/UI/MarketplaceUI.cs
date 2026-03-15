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
                    // TODO: call login menu
                    break;
                case "2":
                    Register();
                    break;
                case "3":
                    // TODO: browse as guest
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
        Console.Write("Enter your name: ");  // TODO add char condition
        string name = Console.ReadLine();

        Console.Write("Enter your password: "); // TODO add char condition
        string password = ReadPassword();
        
        while (true)
        {
            Console.Write("Repeat your password: ");
            string passwordRepeat = ReadPassword();  // TODO add char condition

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
            ShowLoggedInMenu(); //TODO write this method
    }
    public void ShowLoggedInMenu()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine($"Welcome, {marketplace.LoggedInUser.Username}! What would you like to do?");
            Console.WriteLine("1. Enter marketplace");
            Console.WriteLine("2. View my profile");
            Console.WriteLine("3. Logout");
            Console.Write("Select an option: ");
        
            string choice = Console.ReadLine();
        
            switch (choice)
            {
                case "1":
                    Console.Clear();
                    ViewMarketplace(); //TODO write this method
                    break;
                case "2":
                    //ViewProfile(); //TODO write this method
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
                    ShowAllListing(); //TODO write this method
                    break;
                case "2":
                    BrowseByCategory(); //TODO write this method
                    break;
                case "3":
                    //SearchListing(); //TODO write this method
                    break;
                case "4":
                    ShowLoggedInMenu();
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
            Console.WriteLine($"{i + 1,-5} {listings[i].ItemName,-20} {listings[i].Category,-15} {listings[i].Condition,-12} {listings[i].ItemPrice:N0} kr");
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
    
    public void ShowListingDetails(Listing listing)
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
            case "1":
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
}
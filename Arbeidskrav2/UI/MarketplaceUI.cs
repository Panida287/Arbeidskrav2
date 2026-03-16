using Arbeidskrav2.Enums;
using Arbeidskrav2.Models;
using Arbeidskrav2.Services;

namespace Arbeidskrav2.UI;

public class MarketplaceUI
{
    
    // ── Fields ──────────────────────────────────────────
    private Marketplace marketplace;

    
    // ── Constructor ─────────────────────────────────────
    /// <summary>
    /// Handles all console UI interactions for the marketplace application.
    /// </summary>
    public MarketplaceUI(Marketplace marketplace)
    {
        this.marketplace = marketplace;
    }
    
    
    // ── Public Methods ───────────────────────────────────
    /// <summary>Displays the main menu for unauthenticated users.</summary>
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

    /// <summary>Handles user registration with username and password validation.</summary>
    public void Register()
    {
        Console.Clear();
        Console.WriteLine("=== Register ===");
        Console.WriteLine();

        string name = "";
        while (true)
        {
            Console.Write("Enter username (or 0 to go back): ");
            name = Console.ReadLine();
            if (name == "0") return;
            string error = ValidateUsername(name);
            if (error == null) break;
            Console.WriteLine(error);
        }

        string password = "";
        while (true)
        {
            Console.Write("Enter password (4 digits): ");
            password = ReadPassword();
            string error = ValidatePassword(password);
            if (error == null) break;
            Console.WriteLine(error);
        }

        while (true)
        {
            Console.Write("Repeat your password: ");
            string passwordRepeat = ReadPassword();
            if (!string.IsNullOrWhiteSpace(password)) break;
            if (password == passwordRepeat)
                break;
            Console.WriteLine("Passwords don't match, try again!");
        }

        try
        {
            string result = marketplace.Register(name, password);
            Console.WriteLine(result);
        }
        catch (InvalidOperationException e)
        {
            Console.WriteLine(e.Message);
        }

        Console.ReadKey();
    }

    /// <summary>Handles user login with credentials validation.</summary>
    public void Login()
    {
        Console.Clear();
        Console.WriteLine("=== Login ===");
        Console.WriteLine();

        string name = "";
        while (true)
        {
            Console.Write("Enter username (or 0 to go back): ");
            name = Console.ReadLine();
            if (name == "0") return;
            if (!string.IsNullOrWhiteSpace(name)) break;
            Console.WriteLine("Username cannot be empty!");
        }

        string password = "";
        while (true)
        {
            Console.Write("Enter password: ");
            password = ReadPassword();
            if (!string.IsNullOrWhiteSpace(password)) break;
            Console.WriteLine("Password cannot be empty!");
        }

        try
        {
            string result = marketplace.Login(name, password);
            Console.WriteLine(result);
            ShowLoggedInMenu();
        }
        catch (InvalidOperationException e)
        {
            Console.WriteLine(e.Message);
            Console.ReadKey();
        }
    }

    /// <summary>Displays the main menu for logged in users.</summary>
    public void ShowLoggedInMenu()
    {
        while (true)
        {
            if (marketplace.LoggedInUser == null) return;
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

    /// <summary>Displays the marketplace browsing menu.</summary>
    public void ViewMarketplace()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("== Welcome to marketplace ==!");
            Console.WriteLine("1. Browse available listings");
            Console.WriteLine("2. Browse all listings");
            Console.WriteLine("3. Browse by category");
            Console.WriteLine("4. Search listings");
            Console.WriteLine("5. Go back");
            Console.Write("Select an option: ");

            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    Console.Clear();
                    ShowAllListing();
                    break;
                case "2":
                    Console.Clear();
                    ShowAllListings();
                    break;
                case "3":
                    BrowseByCategory();
                    break;
                case "4":
                    SearchListing();
                    break;
                case "5":
                    return;
                default:
                    Console.WriteLine("Invalid option, try again!");
                    break;
            }
        }
    }

    /// <summary>Displays all listings including sold items.</summary>
    public void ShowAllListing()
    {
        List<Listing> availableListings = marketplace.GetAvailableListings();
        Console.Clear();
        Console.WriteLine("=== Available Listings ===");
        ShowListingsAndSelect(availableListings);
    }

    /// <summary>Displays the details of a specific listing.</summary>
    /// <param name="listing">The listing to display.</param>
    public void ShowListingDetails(Listing listing)
    {
        Console.Clear();
        Console.WriteLine($"=== {listing.ItemName} ===");
        Console.WriteLine($"{"Seller:",-15} {listing.Seller.Username}");
        Console.WriteLine($"{"Category:",-15} {FormatCategory(listing.Category)}");
        Console.WriteLine($"{"Condition:",-15} {FormatCondition(listing.Condition)}");
        Console.WriteLine($"{"Price:",-15} {listing.ItemPrice:N0} kr");
        Console.WriteLine($"{"Description:",-15} {listing.ItemDescription}");

        Console.WriteLine();

        if (listing.Status == ListingStatus.Sold)
        {
            Console.WriteLine("This item has been sold.");
        }
        else if (marketplace.LoggedInUser != null)
        {
            Console.WriteLine("1. Buy this item");
        }
        else
        {
            Console.WriteLine("1. Login to purchase this item");
        }

        Console.WriteLine("0. Go back");
        Console.Write("Select an option: ");

        string choice = Console.ReadLine();

        switch (choice)
        {
            case "1":
                if (listing.Status == ListingStatus.Sold)
                {
                    Console.WriteLine("Invalid option!");
                    break;
                }

                if (marketplace.LoggedInUser == null)
                {
                    Login();
                    return;
                }

                Console.Write("Are you sure you want to buy this item? (y/n): ");
                string confirm = Console.ReadLine().ToLower();
                if (confirm == "y")
                {
                    try
                    {
                        string result = marketplace.Purchase(listing);
                        Console.WriteLine(result);
                        Console.ReadKey();
                        return;
                    }
                    catch (InvalidOperationException e)
                    {
                        Console.WriteLine(e.Message);
                        Console.ReadKey();
                    }
                }

                break;
            case "0":
                return;
            default:
                Console.WriteLine("Invalid option!");
                break;
        }
    }

    /// <summary>Displays listings filtered by category.</summary>
    public void BrowseByCategory()
    {
        Console.Clear();
        Console.WriteLine("====== Browse by Category ======");

        Category[] categories = (Category[])Enum.GetValues(typeof(Category));

        for (int i = 0; i < categories.Length; i++)
        {
            int count = marketplace.GetListingsByCategory(categories[i]).Count;
            Console.WriteLine($"{i + 1,-5} {FormatCategory(categories[i]),-30} ({count} items)");
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
                Console.WriteLine($"\n=== {FormatCategory(selected)} Listings ===");
                ShowListingsAndSelect(categoryListings);
                return;
            }

            Console.WriteLine("Invalid selection, try again!");
        }
    }

    /// <summary>Allows user to search listings by keyword.</summary>
    public void SearchListing()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("=== Search listings===");
            Console.Write("What are you looking for? (or 0 to go back): ");
            string searchTerm = Console.ReadLine();
            
            if (searchTerm == "0") return;

            List<Listing> searchedListing = marketplace.SearchListings(searchTerm);
            if (searchedListing.Count == 0)
            {
                Console.WriteLine("No listings found. Press any key to search again.");
                Console.ReadKey();
                continue;
            }
            ShowListingsAndSelect(searchedListing);
        }
    }
    
    /// <summary>Displays the logged in user's profile and management options.</summary>
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
                    ShowMyListings(myListings);
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
                    CreateListing();
                    break;
                case "6":
                    return;
            }
        }
    }

    /// <summary>Displays the logged in user's profile and management options.</summary>
    public void CreateListing()
    {
        Category[] categories = (Category[])Enum.GetValues(typeof(Category));
        Condition[] conditions = (Condition[])Enum.GetValues(typeof(Condition));

        Console.Clear();
        Console.WriteLine("=== Create New Listing ===");
        Console.WriteLine();

        string name = "";
        while (true)
        {
            Console.Write("Item name: ");
            name = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(name))
            {
                Console.WriteLine("Item name cannot be empty!");
                continue;
            }

            if (name.Length > 50)
            {
                Console.WriteLine("Item name cannot exceed 50 characters!");
                continue;
            }

            break;
        }

        string description = "";
        while (true)
        {
            Console.Write("Item description: ");
            description = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(description))
            {
                Console.WriteLine("Description cannot be empty!");
                continue;
            }

            if (description.Length > 200)
            {
                Console.WriteLine("Description cannot exceed 200 characters!");
                continue;
            }

            break;
        }

        double price = 0;
        while (true)
        {
            Console.Write("Price (kr): ");
            if (double.TryParse(Console.ReadLine(), out price) && price > 0)
                break;
            Console.WriteLine("Please enter a valid price!");
        }

        Category selectedCategory = categories[0];
        Console.WriteLine("\nSelect a category:");
        for (int i = 0; i < categories.Length; i++)
            Console.WriteLine($"{i + 1,-5} {categories[i]}");

        while (true)
        {
            Console.Write("Select: ");
            string input = Console.ReadLine();
            if (int.TryParse(input, out int index) && index >= 1 && index <= categories.Length)
            {
                selectedCategory = categories[index - 1];
                break;
            }

            Console.WriteLine("Invalid selection, try again!");
        }

        Condition selectedCondition = conditions[0];
        Console.WriteLine("\nSelect a condition:");
        for (int i = 0; i < conditions.Length; i++)
            Console.WriteLine($"{i + 1,-5} {conditions[i],-12} {GetConditionDescription(conditions[i])}");

        while (true)
        {
            Console.Write("Select: ");
            string input = Console.ReadLine();
            if (int.TryParse(input, out int index) && index >= 1 && index <= conditions.Length)
            {
                selectedCondition = conditions[index - 1];
                break;
            }

            Console.WriteLine("Invalid selection, try again!");
        }

        try
        {
            string result = marketplace.CreateListing(name, description, price, selectedCategory, selectedCondition);
            Console.WriteLine(result);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }

        Console.ReadKey();
    }

    /// <summary>Handles writing a review for a purchased item.</summary>
    /// <param name="transaction">The transaction to review.</param>
    public void WriteReview(Transaction transaction)
    {
        string[] meanings = { "Very poor", "Poor", "Fair", "Good", "Very good", "Excellent" };
        Console.Clear();
        Console.WriteLine($"=== Writing review for {transaction.Listing.ItemName} ===");
        Console.WriteLine($"To seller: {transaction.Seller.Username}");
        Console.WriteLine();
        Console.WriteLine("=== Rating guideline ===");
        for (int i = 0; i < meanings.Length; i++)
        {
            Console.WriteLine($"{i + 1,-5} {meanings[i]}");
        }

        int rating = 0;
        while (rating < 1 || rating > 6)
        {
            Console.Write("Select rating (1-6): ");
            if (int.TryParse(Console.ReadLine(), out rating) && rating >= 1 && rating <= 6)
                break;
            Console.WriteLine("Invalid rating, try again!");
            rating = 0;
        }

        Console.WriteLine("Add comment to your review:");
        string comment = "";
        while (true)
        {
            Console.Write("Add comment (max 100 chars, or press Enter to skip): ");
            comment = Console.ReadLine();

            if (comment.Length <= 100)
                break;

            Console.WriteLine($"Too long! {comment.Length}/100 characters. Try again!");
        }

        if (comment == "")
            comment = "No comment left";

        try
        {
            string result = marketplace.WriteReview(transaction, rating, comment);
            Console.WriteLine(result);
        }
        catch (InvalidOperationException e)
        {
            Console.WriteLine(e.Message);
        }
    }
    
    
    // ── Private Helpers ──────────────────────────────────
        
    private string FormatCategory(Category category)
    {
        switch (category)
        {
            case Category.Electronics: return "Electronics";
            case Category.ClothingAndAccessories: return "Clothing & Accessories";
            case Category.FurnitureAndHome: return "Furniture & Home";
            case Category.BooksAndMedia: return "Books & Media";
            case Category.SportsAndOutdoors: return "Sports & Outdoors";
            case Category.Other: return "Other";
            default: return category.ToString();
        }
    }
    
    private string FormatCondition(Condition condition)
    {
        switch (condition)
        {
            case Condition.New: return "New";
            case Condition.LikeNew: return "Like New";
            case Condition.Good: return "Good";
            case Condition.Fair: return "Fair";
            default: return condition.ToString();
        }
    }

    public void ShowAllListings()
    {
        List<Listing> allListings = marketplace.GetAllListings();
        Console.Clear();
        Console.WriteLine("=== All Listings ===");

        if (allListings.Count == 0)
        {
            Console.WriteLine("No listings found. Press any key to go back.");
            Console.ReadKey();
            return;
        }

        Console.WriteLine($"{"#",-5} " +
                          $"{"Title",-30} " +
                          $"{"Category",-25} " +
                          $"{"Condition",-15} " +
                          $"{"Price",-15} " +
                          $"{"Status"}");
        Console.WriteLine(new string('-', 110));

        for (int i = 0; i < allListings.Count; i++)
        {
            string status = allListings[i].Status == ListingStatus.Sold ? "SOLD" : "Available";
            Console.WriteLine(
                $"{i + 1,-5} " +
                $"{allListings[i].ItemName,-30} " +
                $"{FormatCategory(allListings[i].Category),-25} " +
                $"{FormatCondition(allListings[i].Condition),-15} " +
                $"{(allListings[i].ItemPrice.ToString("N0") + " kr"),-15} " +
                $"{status}");
        }

        Console.WriteLine("\n0. Go back");

        while (true)
        {
            Console.Write("Select a listing to view: ");
            string input = Console.ReadLine();

            if (input == "0") return;

            if (int.TryParse(input, out int index) && index >= 1 && index <= allListings.Count)
            {
                ShowListingDetails(allListings[index - 1]);
                return;
            }

            Console.WriteLine("Invalid selection, try again!");
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

        Console.WriteLine($"{"#",-5} {"Title",-30} {"Category",-25} {"Condition",-12} {"Price"}");
        Console.WriteLine(new string('-', 90));

        for (int i = 0; i < listings.Count; i++)
        {
            Console.WriteLine(
                $"{i + 1,-5} " +
                $"{listings[i].ItemName,-30} " +
                $"{FormatCategory(listings[i].Category),-25} " +
                $"{FormatCondition(listings[i].Condition),-12} " +
                $"{listings[i].ItemPrice:N0} kr");
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
    
    private string GetStarRating(double rating)
    {
        int fullStars = (int)Math.Round(rating);
        int emptyStars = 6 - fullStars;

        return new string('★', fullStars) + new string('☆', emptyStars);
    }

    private void ShowTransactions(List<Transaction> transactions, string otherPartyLabel)
    {
        while (true)
        {
            Console.Clear();

            if (transactions.Count == 0)
            {
                Console.WriteLine("No transactions found.");
                Console.WriteLine("\n0. Go back");
                Console.ReadLine();
                return;
            }

            Console.WriteLine($"{"#",-5} {"Item",-30} {"Price",-12} {"Date",-15} {otherPartyLabel,-15} {"Review"}");
            Console.WriteLine(new string('-', 85));

            for (int i = 0; i < transactions.Count; i++)
            {
                string otherParty = transactions[i].Buyer == marketplace.LoggedInUser
                    ? transactions[i].Seller.Username
                    : transactions[i].Buyer.Username;

                string reviewed = transactions[i].Review != null ? "✓ Reviewed" : "Not reviewed";

                Console.WriteLine(
                    $"{i + 1,-5} " +
                    $"{transactions[i].Listing.ItemName,-30} " +
                    $"{(transactions[i].Listing.ItemPrice.ToString("N0") + " kr"),-12} " +
                    $"{transactions[i].Date.ToString("dd/MM/yyyy"),-15} " +
                    $"{otherParty,-15} " +
                    $"{reviewed}"
                );
            }

            if (otherPartyLabel == "Seller")
                ShowUnreviewedAndSelect(transactions);

            Console.WriteLine("\n0. Go back");
            Console.Write("Select: ");
            string choice = Console.ReadLine();

            if (choice == "0") return;

            if (choice == "1" && otherPartyLabel == "Seller")
                HandleReviewSelection(transactions);
        }
    }

    private void ShowUnreviewedAndSelect(List<Transaction> transactions)
    {
        List<Transaction> unreviewed = transactions
            .Where(t => t.Review == null)
            .ToList();

        if (unreviewed.Count == 0) return;

        Console.WriteLine();
        Console.WriteLine($"You have {unreviewed.Count} unreviewed purchase(s).");
        Console.WriteLine("1. Leave a review");
    }

    private void HandleReviewSelection(List<Transaction> transactions)
    {
        List<Transaction> unreviewed = transactions
            .Where(t => t.Review == null)
            .ToList();

        Console.WriteLine("\nSelect item to review:");
        for (int i = 0; i < unreviewed.Count; i++)
            Console.WriteLine($"{i + 1,-5} {unreviewed[i].Listing.ItemName}");

        Console.Write("Select: ");
        if (int.TryParse(Console.ReadLine(), out int index) && index >= 1 && index <= unreviewed.Count)
            WriteReview(unreviewed[index - 1]);
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
        Console.WriteLine("Press any key to go back.");
        Console.ReadKey();
    }
    
    private string GetConditionDescription(Condition condition)
    {
        switch (condition)
        {
            case Condition.New: return "Unused, still in original packaging";
            case Condition.LikeNew: return "Used briefly, no visible wear";
            case Condition.Good: return "Some signs of use, fully functional";
            case Condition.Fair: return "Noticeable wear, but still works";
            default: return "";
        }
    }
    
    private void ShowMyListings(List<Listing> listings)
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("=== My Listings ===");

            if (listings.Count == 0)
            {
                Console.WriteLine("You have no listings.");
                Console.WriteLine("\n0. Go back");
                Console.ReadLine();
                return;
            }

            Console.WriteLine($"{"#",-5} {"Title",-30} {"Category",-25} {"Condition",-12} {"Price",-12} {"Status"}");
            Console.WriteLine(new string('-', 90));

            for (int i = 0; i < listings.Count; i++)
            {
                Console.WriteLine(
                    $"{i + 1,-5} " +
                    $"{listings[i].ItemName,-30} " +
                    $"{FormatCategory(listings[i].Category),-25} " +
                    $"{FormatCondition(listings[i].Condition),-12} " +
                    $"{(listings[i].ItemPrice.ToString("N0") + " kr"),-12} " +
                    $"{listings[i].Status}");
            }

            Console.WriteLine("\n0. Go back");
            Console.Write("Select a listing to manage: ");

            string input = Console.ReadLine();
            if (input == "0") return;

            if (int.TryParse(input, out int index) && index >= 1 && index <= listings.Count)
                ManageListing(listings[index - 1]);
            else
            {
                Console.WriteLine("Invalid selection, try again!");
            }
        }
    }

    private void ManageListing(Listing listing)
    {
        Console.Clear();
        Console.WriteLine($"=== {listing.ItemName} ===");
        Console.WriteLine($"{"Category:",-15} {FormatCategory(listing.Category)}");
        Console.WriteLine($"{"Condition:",-15} {FormatCondition(listing.Condition)}");
        Console.WriteLine($"{"Description:",-15} {listing.ItemDescription}");
        Console.WriteLine($"{"Price:",-15} {listing.ItemPrice:N0} kr");
        Console.WriteLine($"{"Status:",-15} {listing.Status}");
        Console.WriteLine();
        
        if (listing.Status == ListingStatus.Sold)
        {
            Console.WriteLine("This listing has been sold and cannot be edited or deleted.");
            Console.WriteLine("\n0. Go back");
            Console.ReadLine();
            return;
        }
        
        Console.WriteLine("1. Edit listing");
        Console.WriteLine("2. Delete listing");
        Console.WriteLine("0. Go back");
        Console.Write("Select: ");

        switch (Console.ReadLine())
        {
            case "1":
                EditListing(listing);
                break;
            case "2":
                DeleteListing(listing);
                break;
            case "0":
                return;
        }
    }

    private void EditListing(Listing listing)
    {
        Console.Clear();
        Console.WriteLine($"=== Edit {listing.ItemName} ===");
        Console.WriteLine("Press Enter to keep current value");
        Console.WriteLine();

        Console.Write($"Item name ({listing.ItemName}): ");
        string name = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(name))
            name = listing.ItemName;

        Console.Write($"Description ({listing.ItemDescription}): ");
        string description = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(description))
            description = listing.ItemDescription;

        double price = listing.ItemPrice;
        while (true)
        {
            Console.Write($"Price ({listing.ItemPrice}): ");
            string input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
                break;
            if (double.TryParse(input, out double newPrice) && newPrice > 0)
            {
                price = newPrice;
                break;
            }

            Console.WriteLine("Invalid price, try again!");
        }

        Category[] categories = (Category[])Enum.GetValues(typeof(Category));
        Console.WriteLine($"\nCurrent category: {listing.Category}");
        for (int i = 0; i < categories.Length; i++)
            Console.WriteLine($"{i + 1,-5} {FormatCategory(categories[i])}");

        Category category = listing.Category;
        while (true)
        {
            Console.Write("Select category (or press Enter to keep current): ");
            string input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
                break;
            if (int.TryParse(input, out int index) && index >= 1 && index <= categories.Length)
            {
                category = categories[index - 1];
                break;
            }

            Console.WriteLine("Invalid selection, try again!");
        }

        Condition[] conditions = (Condition[])Enum.GetValues(typeof(Condition));
        Console.WriteLine($"\nCurrent condition: {listing.Condition}");
        for (int i = 0; i < conditions.Length; i++)
            Console.WriteLine($"{i + 1,-5} {conditions[i],-12} {GetConditionDescription(conditions[i])}");

        Condition condition = listing.Condition;
        while (true)
        {
            Console.Write("Select condition (or press Enter to keep current): ");
            string input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
                break;
            if (int.TryParse(input, out int index) && index >= 1 && index <= conditions.Length)
            {
                condition = conditions[index - 1];
                break;
            }

            Console.WriteLine("Invalid selection, try again!");
        }

        try
        {
            string result = marketplace.EditListing(listing, name, description, price, category, condition);
            Console.WriteLine(result);
        }
        catch (InvalidOperationException e)
        {
            Console.WriteLine(e.Message);
        }
    }

    private void DeleteListing(Listing listing)
    {
        Console.Clear();
        Console.WriteLine($"=== Delete {listing.ItemName} ===");
        Console.WriteLine("Are you sure you want to delete this listing? (y/n): ");

        if (Console.ReadLine().ToLower() == "y")
        {
            try
            {
                string result = marketplace.DeleteListing(listing);
                Console.WriteLine(result);
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine(e.Message);
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

    private string ValidateUsername(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "Username cannot be empty";
        if (name.Length < 3) return "Username must be at least 3 characters";
        if (name.Length > 20) return "Username cannot exceed 20 characters";
        return null;
    }

    private string ValidatePassword(string password)
    {
        if (password.Length != 4) return "Password must be exactly 4 digits";
        if (!password.All(char.IsDigit)) return "Password must contain numbers only";
        return null;
    }
}
using Arbeidskrav2.Enums;
using Arbeidskrav2.Models;
using Arbeidskrav2.Services;
using Spectre.Console;

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
            string choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold]== Second Hand Market ==[/]")
                    .AddChoices("Login", "Register", "Visit as guest", "Exit"));

            switch (choice)
            {
                case "Login":
                    Login();
                    break;
                case "Register":
                    Register();
                    break;
                case "Visit as guest":
                    ViewMarketplace();
                    break;
                case "Exit":
                    return;
            }
        }
    }

    /// <summary>Handles user registration with username and password validation.</summary>
    public void Register()
    {
        AnsiConsole.Clear();
        AnsiConsole.MarkupLine("[bold]=== Register ===[/]");
        AnsiConsole.WriteLine();

        string name = AnsiConsole.Prompt(
            new TextPrompt<string>($"Enter username [grey](or press Enter to go back)[/]:{Environment.NewLine}>")
                .AllowEmpty()
                .Validate(input =>
                {
                    if (input == "") return ValidationResult.Success();
                    string error = ValidateUsername(input);
                    return error == null
                        ? ValidationResult.Success()
                        : ValidationResult.Error(error);
                }));

        if (string.IsNullOrWhiteSpace(name)) return;

        string password = AnsiConsole.Prompt(
            new TextPrompt<string>("Enter password (4 digits):")
                .Secret('*')
                .Validate(input =>
                {
                    string error = ValidatePassword(input);
                    return error == null
                        ? ValidationResult.Success()
                        : ValidationResult.Error(error);
                }));

        _ = AnsiConsole.Prompt(
            new TextPrompt<string>("Repeat your password:")
                .Secret('*')
                .Validate(input => input == password
                    ? ValidationResult.Success()
                    : ValidationResult.Error("Passwords don't match!")));

        try
        {
            string result = marketplace.Register(name, password);
            AnsiConsole.MarkupLine($"[green]{result}[/]");
        }
        catch (InvalidOperationException e)
        {
            AnsiConsole.MarkupLine($"[red]{e.Message}[/]");
        }

        Console.ReadKey();
    }

    /// <summary>Handles user login with credentials validation.</summary>
    public void Login()
    {
        AnsiConsole.Clear();
        AnsiConsole.MarkupLine("[bold]=== Login ===[/]");
        AnsiConsole.WriteLine();

        string name = AnsiConsole.Prompt(
            new TextPrompt<string>($"Enter username [grey](or press Enter to go back)[/]:{Environment.NewLine}>")
                .AllowEmpty()
                .Validate(input =>
                {
                    if (input == "") return ValidationResult.Success();
                    if (string.IsNullOrWhiteSpace(input))
                        return ValidationResult.Error("Username cannot be empty!");
                    return ValidationResult.Success();
                }));

        if (string.IsNullOrWhiteSpace(name)) return;

        string password = AnsiConsole.Prompt(
            new TextPrompt<string>($"Enter password:{Environment.NewLine}>")
                .Secret('*')
                .Validate(input =>
                {
                    if (string.IsNullOrWhiteSpace(input))
                        return ValidationResult.Error("Password cannot be empty!");
                    return ValidationResult.Success();
                }));

        try
        {
            string result = marketplace.Login(name, password);
            AnsiConsole.MarkupLine($"[green]{result}[/]");
            ShowLoggedInMenu();
        }
        catch (InvalidOperationException e)
        {
            AnsiConsole.MarkupLine($"[red]{e.Message}[/]");
            Console.ReadKey();
        }
    }

    /// <summary>Displays the main menu for logged-in users.</summary>
    public void ShowLoggedInMenu()
    {
        while (true)
        {
            if (marketplace.LoggedInUser == null) return;

            string choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"[bold]Welcome, {marketplace.LoggedInUser.Username}![/] What would you like to do?")
                    .AddChoices(
                        "Enter marketplace",
                        "View my profile and manage listings",
                        "Logout"));

            switch (choice)
            {
                case "Enter marketplace":
                    ViewMarketplace();
                    break;
                case "View my profile and manage listings":
                    ViewProfile();
                    break;
                case "Logout":
                    string result = marketplace.Logout();
                    AnsiConsole.MarkupLine($"[green]{result}[/]");
                    return;
            }
        }
    }

    /// <summary>Displays the marketplace browsing menu.</summary>
    public void ViewMarketplace()
    {
        while (true)
        {
            string choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold]== Welcome to Marketplace ==[/]")
                    .AddChoices(
                        "Browse available listings",
                        "Browse all listings",
                        "Browse by category",
                        "Search listings",
                        "Go back"));

            switch (choice)
            {
                case "Browse available listings":
                    ShowAllListing();
                    break;
                case "Browse all listings":
                    ShowAllListings();
                    break;
                case "Browse by category":
                    BrowseByCategory();
                    break;
                case "Search listings":
                    SearchListing();
                    break;
                case "Go back":
                    return;
            }
        }
    }

    /// <summary>Displays all listings including sold items.</summary>
    public void ShowAllListings()
    {
        List<Listing> allListings = marketplace.GetAllListings();
        AnsiConsole.Clear();

        if (allListings.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]No listings found.[/] Press any key to go back.");
            Console.ReadKey();
            return;
        }

        var table = new Table();
        table.AddColumn("#");
        table.AddColumn("Title");
        table.AddColumn("Category");
        table.AddColumn("Condition");
        table.AddColumn("Price");
        table.AddColumn("Status");

        for (int i = 0; i < allListings.Count; i++)
        {
            string status = allListings[i].Status == ListingStatus.Sold
                ? "[red]SOLD[/]"
                : "[green]Available[/]";

            table.AddRow(
                $"{i + 1}",
                allListings[i].ItemName,
                FormatCategory(allListings[i].Category),
                FormatCondition(allListings[i].Condition),
                $"{allListings[i].ItemPrice:N0} kr",
                status);
        }

        AnsiConsole.Write(table);

        string[] choices = allListings
            .Select((l, i) => $"{i + 1}. {l.ItemName}")
            .Append("Go back")
            .ToArray();

        string choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select a listing to view:")
                .AddChoices(choices));

        if (choice == "Go back") return;

        int index = int.Parse(choice.Split('.')[0]) - 1;
        ShowListingDetails(allListings[index]);
    }

    /// <summary>Displays all listings including sold items.</summary>
    public void ShowAllListing()
    {
        List<Listing> availableListings = marketplace.GetAvailableListings();
        AnsiConsole.Clear();
        AnsiConsole.MarkupLine("[bold]=== Available Listings ===[/]");
        ShowListingsAndSelect(availableListings);
    }

    /// <summary>Displays the details of a specific listing.</summary>
    /// <param name="listing">The listing to display.</param>
    public void ShowListingDetails(Listing listing)
    {
        AnsiConsole.Clear();
        AnsiConsole.MarkupLine($"[bold]=== {listing.ItemName} ===[/]");
        AnsiConsole.MarkupLine($"{"Seller:",-15} {listing.Seller.Username}");
        AnsiConsole.MarkupLine($"{"Category:",-15} {FormatCategory(listing.Category)}");
        AnsiConsole.MarkupLine($"{"Condition:",-15} {FormatCondition(listing.Condition)}");
        AnsiConsole.MarkupLine($"{"Price:",-15} {listing.ItemPrice:N0} kr");
        AnsiConsole.MarkupLine($"{"Description:",-15} {listing.ItemDescription}");
        AnsiConsole.WriteLine();

        if (listing.Status == ListingStatus.Sold)
        {
            AnsiConsole.MarkupLine("[red]This item has been sold.[/]");
            AnsiConsole.MarkupLine("\nPress any key to go back.");
            Console.ReadKey();
            return;
        }

        var choices = new List<string>();
        choices.Add(marketplace.LoggedInUser != null ? "Buy this item" : "Login to purchase this item");
        choices.Add("Go back");

        string choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("What would you like to do?")
                .AddChoices(choices));

        if (choice == "Go back") return;

        if (marketplace.LoggedInUser == null)
        {
            Login();
            return;
        }

        if (choice == "Buy this item")
            PurchaseListing(listing);
    }

    /// <summary>Displays listings filtered by category.</summary>
    public void BrowseByCategory()
    {
        Category[] categories = (Category[])Enum.GetValues(typeof(Category));

        string[] choices = categories
            .Select(c => $"{FormatCategory(c)} ({marketplace.GetListingsByCategory(c).Count} items)")
            .Append("Go back")
            .ToArray();

        string choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[bold]====== Browse by Category ======[/]")
                .AddChoices(choices));

        if (choice == "Go back") return;

        Category selected = categories[Array.IndexOf(choices, choice)];
        List<Listing> categoryListings = marketplace.GetListingsByCategory(selected);
        AnsiConsole.MarkupLine($"\n[bold]=== {FormatCategory(selected)} Listings ===[/]");
        ShowListingsAndSelect(categoryListings);
    }

    /// <summary>Allows user to search listings by keyword.</summary>
    public void SearchListing()
    {
        while (true)
        {
            AnsiConsole.Clear();

            string searchTerm = AnsiConsole.Prompt(
                new TextPrompt<string>($"[bold]=== Search Listings ===[/]" +
                                       $"{Environment.NewLine}" +
                                       $"What are you looking for? [grey](or press Enter to go back)[/]" +
                                       $"{Environment.NewLine}>")
                    .AllowEmpty());

            if (string.IsNullOrWhiteSpace(searchTerm)) return;

            List<Listing> searchedListing = marketplace.SearchListings(searchTerm);

            if (searchedListing.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]No listings found.[/] Press any key to search again.");
                continue;
            }

            ShowListingsAndSelect(searchedListing);
        }
    }

    /// <summary>Displays the logged-in user's profile and management options.</summary>
    public void ViewProfile()
    {
        while (true)
        {
            int averageRating = marketplace.LoggedInUser.Reviews.Any()
                ? (int)Math.Round(marketplace.LoggedInUser.Reviews.Average(r => r.ReviewScore))
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

            string choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"[bold]=== Profile: {marketplace.LoggedInUser.Username} ===[/]\n" +
                           $"Average Rating: {stars} ({averageRating}/6)\n")
                    .AddChoices(
                        $"My Listings ({myListings.Count})",
                        $"My Purchases ({myPurchases.Count})",
                        $"Listings Sold ({mySales.Count})",
                        $"Reviews Received ({reviews.Count})",
                        "Create New Listing",
                        "Go back",
                        "Logout"));

            switch (choice)
            {
                case "Logout":
                    string result = marketplace.Logout();
                    AnsiConsole.MarkupLine($"[green]{result}[/]");
                    return;
                case var c when c.StartsWith("My Listings"):
                    ShowMyListings(myListings);
                    break;
                case var c when c.StartsWith("My Purchases"):
                    ShowTransactions(myPurchases, "Seller");
                    break;
                case var c when c.StartsWith("Listings Sold"):
                    ShowTransactions(mySales, "Buyer");
                    break;
                case var c when c.StartsWith("Reviews Received"):
                    ShowReviews(reviews);
                    break;
                case "Create New Listing":
                    CreateListing();
                    break;
                case "Go back":
                    return;
            }
        }
    }

    /// <summary>Handles creation of a new listing.</summary>
    public void CreateListing()
    {
        Category[] categories = (Category[])Enum.GetValues(typeof(Category));
        Condition[] conditions = (Condition[])Enum.GetValues(typeof(Condition));

        AnsiConsole.Clear();
        AnsiConsole.MarkupLine("[bold]=== Create New Listing ===[/]");
        AnsiConsole.WriteLine();

        string name = AnsiConsole.Prompt(
            new TextPrompt<string>("Item name:")
                .Validate(input =>
                {
                    if (string.IsNullOrWhiteSpace(input))
                        return ValidationResult.Error("Item name cannot be empty!");
                    if (input.Length > 50)
                        return ValidationResult.Error("Item name cannot exceed 50 characters!");
                    return ValidationResult.Success();
                }));

        string description = AnsiConsole.Prompt(
            new TextPrompt<string>("Item description:")
                .Validate(input =>
                {
                    if (string.IsNullOrWhiteSpace(input))
                        return ValidationResult.Error("Description cannot be empty!");
                    if (input.Length > 200)
                        return ValidationResult.Error("Description cannot exceed 200 characters!");
                    return ValidationResult.Success();
                }));

        double price = AnsiConsole.Prompt(
            new TextPrompt<double>("Price (kr):")
                .Validate(input =>
                {
                    if (input <= 0)
                        return ValidationResult.Error("Please enter a valid price!");
                    return ValidationResult.Success();
                }));

        Category selectedCategory = AnsiConsole.Prompt(
            new SelectionPrompt<Category>()
                .Title("Select a category:")
                .AddChoices(categories)
                .UseConverter(c => FormatCategory(c)));

        Condition selectedCondition = AnsiConsole.Prompt(
            new SelectionPrompt<Condition>()
                .Title("Select a condition:")
                .AddChoices(conditions)
                .UseConverter(c => $"{FormatCondition(c),-12} {GetConditionDescription(c)}"));

        try
        {
            string result = marketplace.CreateListing(name, description, price, selectedCategory, selectedCondition);
            AnsiConsole.MarkupLine($"[green]{result}[/]");
        }
        catch (Exception e)
        {
            AnsiConsole.MarkupLine($"[red]{e.Message}[/]");
        }
    }

    /// <summary>Handles writing a review for a purchased item.</summary>
    /// <param name="transaction">The transaction to review.</param>
    public void WriteReview(Transaction transaction)
    {
        string[] meanings = { "Very poor", "Poor", "Fair", "Good", "Very good", "Excellent" };

        AnsiConsole.Clear();
        AnsiConsole.MarkupLine($"[bold]=== Writing review for {transaction.Listing.ItemName} ===[/]");
        AnsiConsole.MarkupLine($"To seller: [blue]{transaction.Seller.Username}[/]");
        AnsiConsole.WriteLine();

        string rating = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[bold]=== Select Rating ===[/]")
                .AddChoices(meanings.Select((m, i) => $"{i + 1} - {m}")));

        int ratingScore = int.Parse(rating.Split(' ')[0]);

        string comment = AnsiConsole.Prompt(
            new TextPrompt<string>("Add a comment [grey](or press Enter to skip)[/]:")
                .AllowEmpty()
                .Validate(input =>
                {
                    if (input.Length > 100)
                        return ValidationResult.Error($"Too long! {input.Length}/100 characters.");
                    return ValidationResult.Success();
                }));

        if (string.IsNullOrWhiteSpace(comment))
            comment = "No comment left";

        try
        {
            string result = marketplace.WriteReview(transaction, ratingScore, comment);
            AnsiConsole.MarkupLine($"[green]{result}[/]");
        }
        catch (InvalidOperationException e)
        {
            AnsiConsole.MarkupLine($"[red]{e.Message}[/]");
        }
    }

    // ── Private Helpers ──────────────────────────────────

    // Formats category enum value to a readable string with spaces
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

    // Formats condition enum value to a readable string with spaces
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

    // Displays listings in a table and allows the user to select one to view
    private void ShowListingsAndSelect(List<Listing> listings)
    {
        if (listings.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]No listings available.[/] Press any key to go back.");
            Console.ReadKey();
            return;
        }

        var table = new Table();
        table.AddColumn("#");
        table.AddColumn("Title");
        table.AddColumn("Category");
        table.AddColumn("Condition");
        table.AddColumn("Price");
        table.AddColumn("Status"); 

        for (int i = 0; i < listings.Count; i++)
        {
            string status = listings[i].Status == ListingStatus.Sold
                ? "[red]SOLD[/]"
                : "[green]Available[/]";
            
            table.AddRow(
                $"{i + 1}",
                listings[i].ItemName,
                FormatCategory(listings[i].Category),
                FormatCondition(listings[i].Condition),
                $"{listings[i].ItemPrice:N0} kr",
                status);
        }

        AnsiConsole.Write(table);

        string[] choices = listings
            .Select((l, i) => $"{i + 1}. {l.ItemName}")
            .Append("Go back")
            .ToArray();

        string choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select a listing to view:")
                .AddChoices(choices));

        if (choice == "Go back") return;

        int index = int.Parse(choice.Split('.')[0]) - 1;
        ShowListingDetails(listings[index]);
    }
    
    // Handles the purchase flow for a listing including optional review
    private void PurchaseListing(Listing listing)
    {
        string confirm = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title($"Are you sure you want to buy [bold]{listing.ItemName}[/]?")
                .AddChoices("Yes", "No"));

        if (confirm != "Yes") return;

        string result = marketplace.Purchase(listing);
        AnsiConsole.MarkupLine($"[green]{result}[/]");

        Transaction transaction = marketplace.LoggedInUser.Transactions
            .Last(t => t.Listing == listing);

        string reviewChoice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Would you like to leave a review?\n[grey](You can always review later via Profile > My Purchases)[/]")
                .AddChoices("Review now", "Review later"));

        if (reviewChoice == "Review now")
            WriteReview(transaction);
    }

    // Converts a numeric rating to a star display string using ★ and ☆ characters
    private string GetStarRating(int rating)
    {
        int fullStars = rating;
        int emptyStars = 6 - fullStars;

        return new string('★', fullStars) + new string('☆', emptyStars);
    }

    // Displays a list of transactions in a table format with review status
    private void ShowTransactions(List<Transaction> transactions, string otherPartyLabel)
    {
        while (true)
        {
            AnsiConsole.Clear();

            if (transactions.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]No transactions found.[/] Press any key to go back.");
                Console.ReadKey();
                return;
            }

            var table = new Table();
            table.AddColumn("#");
            table.AddColumn("Item");
            table.AddColumn("Price");
            table.AddColumn("Date");
            table.AddColumn(otherPartyLabel);
            table.AddColumn("Review");

            for (int i = 0; i < transactions.Count; i++)
            {
                string otherParty = transactions[i].Buyer == marketplace.LoggedInUser
                    ? transactions[i].Seller.Username
                    : transactions[i].Buyer.Username;

                string reviewed = transactions[i].Review != null
                    ? "[green]✓ Reviewed[/]"
                    : "[red]Not reviewed[/]";

                table.AddRow(
                    $"{i + 1}",
                    transactions[i].Listing.ItemName,
                    $"{transactions[i].Listing.ItemPrice:N0} kr",
                    transactions[i].Date.ToString("dd/MM/yyyy"),
                    otherParty,
                    reviewed);
            }

            AnsiConsole.Write(table);

            var choices = new List<string>();
            if (otherPartyLabel == "Seller" && transactions.Any(t => t.Review == null))
                choices.Add("Leave a review");
            choices.Add("Go back");

            int unreviewedCount = transactions.Count(t => t.Review == null);
            string title = otherPartyLabel == "Seller" && unreviewedCount > 0
                ? $"You have [yellow]{unreviewedCount}[/] unreviewed purchase(s)."
                : "What would you like to do?";

            string choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title(title)
                    .AddChoices(choices));

            if (choice == "Go back") return;

            if (choice == "Leave a review")
                HandleReviewSelection(transactions);
        }
    }

    // Handles selection of an unreviewed transaction to write a review for
    private void HandleReviewSelection(List<Transaction> transactions)
    {
        List<Transaction> unreviewed = transactions
            .Where(t => t.Review == null)
            .ToList();

        string choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select item to review:")
                .AddChoices(unreviewed.Select(t => t.Listing.ItemName)));

        Transaction selected = unreviewed.First(t => t.Listing.ItemName == choice);
        WriteReview(selected);
    }

    // Displays all reviews received by the logged-in user
    private void ShowReviews(List<Review> reviews)
    {
        AnsiConsole.Clear();

        if (reviews.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]No reviews yet.[/]");
            AnsiConsole.Prompt(new SelectionPrompt<string>().AddChoices("Go back"));
            return;
        }

        var table = new Table();
        table.AddColumn("#");
        table.AddColumn("Rating");
        table.AddColumn("From");
        table.AddColumn("Comment");

        for (int i = 0; i < reviews.Count; i++)
        {
            string stars = GetStarRating(reviews[i].ReviewScore);
            table.AddRow(
                $"{i + 1}",
                stars,
                reviews[i].Buyer.Username,
                reviews[i].ReviewText);
        }

        AnsiConsole.Write(table);
    }

    // Returns a human readable description of a condition value for display in listing forms
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

    // Displays the logged-in user's listings and allows them to manage each one
    private void ShowMyListings(List<Listing> listings)
    {
        while (true)
        {
            AnsiConsole.Clear();
            AnsiConsole.MarkupLine("[bold]=== My Listings ===[/]");

            if (listings.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]You have no listings.[/]");
                AnsiConsole.Prompt(new SelectionPrompt<string>().AddChoices("Go back"));
                return;
            }

            var table = new Table();
            table.AddColumn("#");
            table.AddColumn("Title");
            table.AddColumn("Category");
            table.AddColumn("Condition");
            table.AddColumn("Price");
            table.AddColumn("Status");

            for (int i = 0; i < listings.Count; i++)
            {
                string status = listings[i].Status == ListingStatus.Sold
                    ? "[red]Sold[/]"
                    : "[green]Available[/]";

                table.AddRow(
                    $"{i + 1}",
                    listings[i].ItemName,
                    FormatCategory(listings[i].Category),
                    FormatCondition(listings[i].Condition),
                    $"{listings[i].ItemPrice:N0} kr",
                    status);
            }

            AnsiConsole.Write(table);

            string[] choices = listings
                .Select((l, i) => $"{i + 1}. {l.ItemName}")
                .Append("Go back")
                .ToArray();

            string choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select a listing to manage:")
                    .AddChoices(choices));

            if (choice == "Go back") return;

            int index = int.Parse(choice.Split('.')[0]) - 1;
            ManageListing(listings[index]);
        }
    }

    // Displays listing details and allows the seller to edit or delete it
    private void ManageListing(Listing listing)
    {
        AnsiConsole.Clear();
        AnsiConsole.MarkupLine($"[bold]=== {listing.ItemName} ===[/]");
        AnsiConsole.MarkupLine($"{"Category:",-15} {FormatCategory(listing.Category)}");
        AnsiConsole.MarkupLine($"{"Condition:",-15} {FormatCondition(listing.Condition)}");
        AnsiConsole.MarkupLine($"{"Description:",-15} {listing.ItemDescription}");
        AnsiConsole.MarkupLine($"{"Price:",-15} {listing.ItemPrice:N0} kr");
        AnsiConsole.MarkupLine($"{"Status:",-15} {listing.Status}");
        AnsiConsole.WriteLine();

        if (listing.Status == ListingStatus.Sold)
        {
            AnsiConsole.MarkupLine("[red]This listing has been sold and cannot be edited or deleted.[/]");
            return;
        }

        string choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("What would you like to do?")
                .AddChoices("Edit listing", "Delete listing", "Go back"));

        switch (choice)
        {
            case "Edit listing":
                EditListing(listing);
                break;
            case "Delete listing":
                DeleteListing(listing);
                break;
            case "Go back":
                return;
        }
    }

    // Handles editing of an existing listing with current values shown as defaults
    private void EditListing(Listing listing)
    {
        Category[] categories = (Category[])Enum.GetValues(typeof(Category));
        Condition[] conditions = (Condition[])Enum.GetValues(typeof(Condition));

        AnsiConsole.Clear();
        AnsiConsole.MarkupLine($"[bold]=== Edit {listing.ItemName} ===[/]");
        AnsiConsole.MarkupLine("[grey]Press Enter to keep current value[/]");
        AnsiConsole.WriteLine();

        string name = AnsiConsole.Prompt(
            new TextPrompt<string>($"Item name [grey]({listing.ItemName})[/]:")
                .AllowEmpty()
                .Validate(input =>
                {
                    if (input == "") return ValidationResult.Success();
                    if (input.Length > 50)
                        return ValidationResult.Error("Item name cannot exceed 50 characters!");
                    return ValidationResult.Success();
                }));
        if (string.IsNullOrWhiteSpace(name)) name = listing.ItemName;

        string description = AnsiConsole.Prompt(
            new TextPrompt<string>($"Description [grey]({listing.ItemDescription})[/]:")
                .AllowEmpty()
                .Validate(input =>
                {
                    if (input == "") return ValidationResult.Success();
                    if (input.Length > 200)
                        return ValidationResult.Error("Description cannot exceed 200 characters!");
                    return ValidationResult.Success();
                }));
        if (string.IsNullOrWhiteSpace(description)) description = listing.ItemDescription;

        string priceInput = AnsiConsole.Prompt(
            new TextPrompt<string>($"Price [grey]({listing.ItemPrice:N0} kr)[/]:")
                .AllowEmpty()
                .Validate(input =>
                {
                    if (input == "") return ValidationResult.Success();
                    if (!double.TryParse(input, out double p) || p <= 0)
                        return ValidationResult.Error("Please enter a valid price!");
                    return ValidationResult.Success();
                }));
        double price = string.IsNullOrWhiteSpace(priceInput)
            ? listing.ItemPrice
            : double.Parse(priceInput);

        Category category = AnsiConsole.Prompt(
            new SelectionPrompt<Category>()
                .Title($"Select category [grey](current: {FormatCategory(listing.Category)})[/]:")
                .AddChoices(categories)
                .UseConverter(c => FormatCategory(c)));

        Condition condition = AnsiConsole.Prompt(
            new SelectionPrompt<Condition>()
                .Title($"Select condition [grey](current: {FormatCondition(listing.Condition)})[/]:")
                .AddChoices(conditions)
                .UseConverter(c => $"{FormatCondition(c),-12} {GetConditionDescription(c)}"));

        try
        {
            string result = marketplace.EditListing(listing, name, description, price, category, condition);
            AnsiConsole.MarkupLine($"[green]{result}[/]");
        }
        catch (InvalidOperationException e)
        {
            AnsiConsole.MarkupLine($"[red]{e.Message}[/]");
        }
    }

    // Handles deletion of a listing with confirmation prompt
    private void DeleteListing(Listing listing)
    {
        AnsiConsole.Clear();
        AnsiConsole.MarkupLine($"[bold]=== Delete {listing.ItemName} ===[/]");

        string confirm = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title($"Are you sure you want to delete [red]{listing.ItemName}[/]?")
                .AddChoices("Yes, delete it", "No, go back"));

        if (confirm == "Yes, delete it")
        {
            try
            {
                string result = marketplace.DeleteListing(listing);
                AnsiConsole.MarkupLine($"[green]{result}[/]");
            }
            catch (InvalidOperationException e)
            {
                AnsiConsole.MarkupLine($"[red]{e.Message}[/]");
            }
        }
    }

    // Validates username length and format requirements
    private string ValidateUsername(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "Username cannot be empty";
        if (name.Length < 3) return "Username must be at least 3 characters";
        if (name.Length > 20) return "Username cannot exceed 20 characters";
        return null;
    }

    // Validates password is exactly 4 numeric digits
    private string ValidatePassword(string password)
    {
        if (password.Length != 4) return "Password must be exactly 4 digits";
        if (!password.All(char.IsDigit)) return "Password must contain numbers only";
        return null;
    }
}
using Arbeidskrav2.Enums;
using Arbeidskrav2.Models;
using Arbeidskrav2.Services;
using Arbeidskrav2.UI;

Marketplace marketplace = new Marketplace();

// ── Register users ──
marketplace.Register("maria", "1234");
marketplace.Register("erik", "1234");
marketplace.Register("anna", "1234");

// ── Maria's listings ──
marketplace.Login("maria", "1234");
marketplace.CreateListing("iPhone 14 Pro", "128GB Space Black, minor scratches. Includes charger. No original box.", 5500, Category.Electronics, Condition.Good);
marketplace.CreateListing("IKEA Kallax Shelf", "White 4x4 grid. Some wear on edges.", 400, Category.FurnitureAndHome, Condition.LikeNew);
marketplace.CreateListing("Vintage Denim Jacket", "Size M, barely worn. Great condition.", 350, Category.ClothingAndAccessories, Condition.LikeNew);
marketplace.Logout();

// ── Erik's listings ──
marketplace.Login("erik", "1234");
marketplace.CreateListing("Running Shoes Size 42", "Nike Air Max, used twice.", 250, Category.SportsAndOutdoors, Condition.LikeNew);
marketplace.CreateListing("MacBook Pro 2021", "M1 chip, 16GB RAM, 512GB SSD. Minor wear.", 12000, Category.Electronics, Condition.Good);
marketplace.CreateListing("Harry Potter Complete Set", "All 7 books, paperback. Good condition.", 300, Category.BooksAndMedia, Condition.Good);
marketplace.Logout();

// ── Anna's listings ──
marketplace.Login("anna", "1234");
marketplace.CreateListing("Yoga Mat", "Purple, 6mm thick. Used a few times.", 150, Category.SportsAndOutdoors, Condition.Good);
marketplace.CreateListing("Coffee Table", "IKEA Lack, white. Small scratch on top.", 200, Category.FurnitureAndHome, Condition.Fair);
marketplace.Logout();

// ── Erik buys from Maria ──
marketplace.Login("erik", "1234");
// get maria's listings
var iphone = marketplace.GetAvailableListings().First(l => l.ItemName == "iPhone 14 Pro");
var denim = marketplace.GetAvailableListings().First(l => l.ItemName == "Vintage Denim Jacket");
marketplace.Purchase(iphone);   // erik buys iphone - will leave review
marketplace.Purchase(denim);    // erik buys denim jacket - NO review left
marketplace.Logout();

// ── Anna buys from Erik ──
marketplace.Login("anna", "1234");
var macbook = marketplace.GetAvailableListings().First(l => l.ItemName == "MacBook Pro 2021");
var harrypotter = marketplace.GetAvailableListings().First(l => l.ItemName == "Harry Potter Complete Set");
marketplace.Purchase(macbook);      // anna buys macbook - will leave review
marketplace.Purchase(harrypotter);  // anna buys harry potter - NO review left
marketplace.Logout();

// ── Maria buys from Anna ──
marketplace.Login("maria", "1234");
var coffeeTable = marketplace.GetAvailableListings().First(l => l.ItemName == "Coffee Table");
marketplace.Purchase(coffeeTable);  // maria buys coffee table - will leave review
marketplace.Logout();

// ── Erik leaves review for Maria (iPhone purchase) ──
marketplace.Login("erik", "1234");
var erikTransactions = marketplace.LoggedInUser.Transactions
    .Where(t => t.Buyer == marketplace.LoggedInUser)
    .ToList();
var iphoneTransaction = erikTransactions.First(t => t.Listing.ItemName == "iPhone 14 Pro");
marketplace.WriteReview(iphoneTransaction, 5, "Fast response, item exactly as described!");
marketplace.Logout();

// ── Anna leaves review for Erik (MacBook purchase) ──
marketplace.Login("anna", "1234");
var annaTransactions = marketplace.LoggedInUser.Transactions
    .Where(t => t.Buyer == marketplace.LoggedInUser)
    .ToList();
var macbookTransaction = annaTransactions.First(t => t.Listing.ItemName == "MacBook Pro 2021");
marketplace.WriteReview(macbookTransaction, 6, "Amazing seller, MacBook is perfect!");
marketplace.Logout();

// ── Maria leaves review for Anna (Coffee Table purchase) ──
marketplace.Login("maria", "1234");
var mariaTransactions = marketplace.LoggedInUser.Transactions
    .Where(t => t.Buyer == marketplace.LoggedInUser)
    .ToList();
var coffeeTableTransaction = mariaTransactions.First(t => t.Listing.ItemName == "Coffee Table");
marketplace.WriteReview(coffeeTableTransaction, 4, "Good seller, table had a bigger scratch than described.");
marketplace.Logout();

MarketplaceUI ui = new MarketplaceUI(marketplace);
ui.ShowMainMenu();
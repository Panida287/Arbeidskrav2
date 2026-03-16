# Second Hand Market

A command-line marketplace application for buying and selling second-hand items, inspired by Finn.no.

## Description

This C# console application simulates an online second-hand marketplace. Users can register accounts, list items for sale, browse and search listings, purchase items, and leave reviews for sellers.

### Features

- Register and login with password masking (4-digit PIN)
- Browse available listings or all listings including sold items
- Browse listings by category with item count
- Search listings by keyword (case insensitive)
- View listing details and purchase items
- Leave reviews (1-6 rating) after purchasing
- View purchase and sales history with dates
- Create, edit and delete your own listings
- View profile with average star rating display
- Guest browsing without login

## How to Run

1. Clone the repository
2. Open the solution in JetBrains Rider
3. Press the green Run button, or use terminal: `dotnet run`

The application starts with pre-loaded test data so you can explore all features immediately.

## Test Accounts

All accounts use password: **1234**

| Username | Listings | Sold | Purchased | Reviews Received | Unreviewed Purchases |
|---|---|---|---|---|---|
| maria | 3 (1 sold) | 1 | 1 | 1 (avg ★★★★☆☆) | 0 |
| erik | 3 (2 sold) | 2 | 2 | 2 (avg ★★★★★☆) | 1 (denim jacket) |
| anna | 2 (1 sold) | 1 | 2 | 1 (avg ★★★★☆☆) | 1 (harry potter) |

You can also register a new account from the main menu.

## Project Structure

```
Arbeidskrav2/
├── Enums/
│   ├── Category.cs          # Electronics, Clothing, Furniture, Books, Sports, Other
│   ├── Condition.cs         # New, LikeNew, Good, Fair
│   └── ListingStatus.cs     # Available, Sold
├── Models/
│   ├── User.cs              # User account with listings, transactions and reviews
│   ├── Listing.cs           # Item listed for sale
│   ├── Transaction.cs       # Purchase record between buyer and seller
│   └── Review.cs            # Buyer review for a seller
├── Services/
│   └── Marketplace.cs       # Business logic and data management
├── UI/
│   └── MarketplaceUI.cs     # All console input/output and menus
├── Program.cs               # Entry point and seed data
└── README.md
```

## Application Sitemap

```
=== Main Menu ===
├── 1. Login
│   └── → Logged In Menu
│       ├── 1. Enter Marketplace
│       │   ├── 1. Browse available listings
│       │   │   └── Select listing → Listing Details → Buy
│       │   ├── 2. Browse all listings (including sold)
│       │   │   └── Select listing → Listing Details → Buy (if available)
│       │   ├── 3. Browse by category
│       │   │   └── Select category → Listings → Listing Details → Buy
│       │   └── 4. Search listings
│       │       └── Results → Listing Details → Buy
│       ├── 2. View Profile
│       │   ├── 1. My Listings → Select listing → Edit / Delete
│       │   ├── 2. My Purchases → Leave review (if unreviewed)
│       │   ├── 3. Listings Sold
│       │   ├── 4. Reviews Received
│       │   ├── 5. Create New Listing
│       │   └── 0. Logout
│       └── 3. Logout
├── 2. Register
└── 3. Visit Marketplace as guest
    ├── 1. Browse available listings
    ├── 2. Browse all listings
    ├── 3. Browse by category
    └── 4. Search listings
        └── Select listing → View Details → Login to purchase
```

## Design Decisions

### OOP Concepts Used

**Classes and Objects**
All core entities are modelled as classes: `User`, `Listing`, `Transaction`, `Review`. Each has private fields, public properties and a constructor.

**Encapsulation**
Private fields are exposed through public properties. Business rule validation is handled in the `Marketplace` service class, keeping model classes clean.

**Inheritance**
Not used in this project as there were no natural "is-a" relationships between the core entities.

**Interfaces**
Not used in this project as the classes serve distinct purposes and share no common contract that would benefit from an interface.

**Enums**
Used for `Category`, `Condition` and `ListingStatus` to represent fixed sets of values safely without magic strings.

**Generic Collections**
`List<T>` is used throughout to store users, listings, transactions and reviews.

**LINQ**
Used extensively for filtering listings by category, searching by keyword, calculating average ratings, finding unreviewed transactions and sorting data.

**Exception Handling**
`try/catch` blocks are used in all UI methods. The `Marketplace` service throws `InvalidOperationException` for business rule violations such as buying your own listing, duplicate usernames, and reviewing the same transaction twice.

### Separation of Concerns
Console input/output is kept entirely in `MarketplaceUI`. Model classes contain only data and simple helper methods. Business logic lives in `Marketplace`. This means if the UI ever changes, the logic doesn't need to be touched.

### Helper Methods
Private helper methods like `CheckIfLoggedIn()`, `CheckIfAvailable()` and `CheckIfSeller()` are used in `Marketplace` to avoid deeply nested if statements and keep public methods clean.

## What I Learned

- **Encapsulation**: Why private fields with public properties are better than public fields
- **LINQ**: How to filter, sort and transform collections cleanly instead of manual loops
- **Exception Handling**: The difference between returning error strings and throwing exceptions, and when each is appropriate
- **Separation of concerns**: Keeping UI code separate from business logic makes the code much easier to maintain
- **Git workflow**: Using branches, meaningful commit messages and pull requests

## Challenges I Faced

1. **Designing the class structure**: Unlike Arbeidskrav 1, there was no prescribed hierarchy. Deciding what belongs in models vs services took planning.

2. **Exception handling vs return strings**: Initially all methods returned error strings. Refactoring to throw exceptions made the code cleaner but required understanding when exceptions are appropriate.

3. **LINQ syntax**: The lambda `=>` syntax was confusing at first. Learning that `listings.Where(l => l.Status == ListingStatus.Available)` is just a cleaner foreach loop made it click.

4. **Navigation flow**: Managing multiple menus in a console app without losing track of where the user is was tricky. Using `return` vs `break` correctly in nested loops required careful thinking.

## AI Usage

This project was developed with assistance from Claude (Anthropic).

> "What is encapsulation and why do we use private fields with public properties instead of just making everything public?"

> "What is the difference between get and set in properties? And what is the shorthand auto property?"

> "What is the value keyword inside set?"

> "What does static mean and when do I use it?"

> "What is the difference between List and Dictionary and when do I use each one?"

> "Can you explain LINQ and lambda expressions? What does the => mean?"

> "What is the difference between abstract and virtual?"

> "What is an interface and why would I use it over inheritance?"

> "What is the difference between throwing an exception and returning an error string?"

> "How do I use git via terminal instead of GitHub Desktop?"

> "Can you help me fix my commit msgs to be more detailed."

> "This is my terminal output design, do you have suggestion to make it look better or easier to use."

> "Help me refine my XML documentation comments for my public methods and properties."

**Author**: Panida Finstad
**Course**: Backend Programming Year 1, Gokstad Academy
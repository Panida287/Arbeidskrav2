# Second Hand Market

A command-line marketplace application for buying and selling second-hand items, inspired by Finn.no.

## Description

This C# console application simulates an online second-hand marketplace. Users can register accounts, list items for sale, browse and search listings, purchase items, and leave reviews for sellers. The application uses [Spectre.Console](https://spectreconsole.net/) for an enhanced terminal UI with arrow-key navigation, colored output and formatted tables. Data is persisted to a local SQLite database so all users, listings, transactions and reviews survive between sessions.

### Features

- Arrow-key navigable menus powered by Spectre.Console
- Register and login with password masking (4-digit PIN)
- Browse available listings or all listings including sold items
- Browse listings by category with item count
- Search listings by keyword (case insensitive)
- View listing details and purchase items
- Leave a review immediately after purchase or later from profile
- View purchase and sales history with dates and review status
- Create, edit and delete your own listings
- View profile with average star rating display
- Guest browsing without login
- **Data persistence via SQLite — all data survives app restarts**

## How to Run

1. Clone the repository
2. Open the solution in JetBrains Rider
3. Press the green Run button, or use terminal: `dotnet run`

The application creates a `marketplace.db` file on first run and loads all data from it on every subsequent run. The database file is not included in the repository — it is created locally on your machine.

The application starts with pre-loaded test data so you can explore all features immediately.

## Test Accounts

| Username | Password| Listings | Sold | Purchased | Reviews Received | Unreviewed Purchases |
|---|---|---|---|---|---|---|
| maria | 1234 | 3 (1 sold) | 1 | 1 | 1 (avg ★★★★☆☆) | 0 |
| erik | 1234 | 3 (2 sold) | 2 | 2 | 2 (avg ★★★★★☆) | 1 (denim jacket) |
| anna | 1234 | 2 (1 sold) | 1 | 2 | 1 (avg ★★★★☆☆) | 1 (harry potter) |

You can also register a new account from the main menu.

## Project Structure

```
Arbeidskrav2/
├── Database/
│   └── DatabaseService.cs   # All SQLite communication — save, load, update, delete
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
├── Program.cs               # Entry point
├── README.md
└── AIUsage.md
```

## Application Sitemap

```
=== Main Menu ===
├── Login
│   └── → Logged In Menu
│       ├── Enter Marketplace
│       │   ├── Browse available listings
│       │   │   └── Select listing → Listing Details
│       │   │       ├── Buy → Confirm → Review now / Review later
│       │   │       └── Go back
│       │   ├── Browse all listings (including sold)
│       │   │   └── Select listing → Listing Details
│       │   │       ├── Buy (if available) → Confirm → Review now / Review later
│       │   │       └── Go back
│       │   ├── Browse by category
│       │   │   └── Select category → Listings → Listing Details
│       │   └── Search listings
│       │       └── Results → Listing Details
│       ├── View Profile
│       │   ├── My Listings → Select listing → Edit / Delete (if available)
│       │   ├── My Purchases → Leave review (if unreviewed)
│       │   ├── Listings Sold
│       │   ├── Reviews Received
│       │   ├── Create New Listing
│       │   ├── Go back
│       │   └── Logout
│       └── Logout
├── Register
└── Visit Marketplace as guest
    ├── Browse available listings
    ├── Browse all listings
    ├── Browse by category
    └── Search listings
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
`try/catch` blocks are used in UI methods. The `Marketplace` service throws `InvalidOperationException` for business rule violations such as buying your own listing, duplicate usernames, and reviewing the same transaction twice.

### Separation of Concerns
Console input/output is kept entirely in `MarketplaceUI`. Model classes contain only data and simple helper methods. Business logic lives in `Marketplace`. Database communication is isolated in `DatabaseService` — no other class references SQLite directly.

### SQLite Persistence
After completing the core application, data persistence was added using SQLite via the `Microsoft.Data.Sqlite` NuGet package. The `DatabaseService` class handles all database communication:

- **`InitializeDatabase()`** runs on startup and creates the four tables (`Users`, `Listings`, `Transactions`, `Reviews`) if they do not already exist, using `CREATE TABLE IF NOT EXISTS`
- **Save methods** are called immediately after each state change (register, create listing, purchase, review, delete) so the database stays in sync with the in-memory state at all times
- **Load methods** run once on startup inside `LoadFromDatabase()` in `Marketplace`, restoring all data into the existing `List<T>` collections in the correct order: Users → Listings → Transactions → Reviews. Order matters because each table references the one before it via foreign keys
- **Parameterised queries** (`@Username`, `@Price` etc.) are used throughout instead of string concatenation to prevent SQL injection
- Enums are stored as integers in the database and cast back to their C# types on load
- Nullable values (such as review comments) use `DBNull.Value` for proper SQL `NULL` storage
- Dates are stored as ISO 8601 strings since SQLite has no native date type

The `marketplace.db` file is excluded from version control via `.gitignore` since it is generated locally and changes with every run.

### Spectre.Console
After completing the core application with standard `Console.WriteLine` and `Console.ReadLine`, the UI was refactored to use Spectre.Console. This was an interesting experience because:

- Arrow-key `SelectionPrompt` replaced all number-based menus, removing the need for manual input parsing and invalid option handling
- `TextPrompt` with built-in `.Validate()` replaced manual while loops for input validation
- `.Secret()` on `TextPrompt` replaced the custom `ReadPassword()` helper method entirely
- `SelectionPrompt<Category>` and `SelectionPrompt<Condition>` with `.UseConverter()` replaced manual enum display loops
- The `ShowUnreviewedAndSelect` helper method was deleted entirely as Spectre handled the logic more cleanly

Building the original version first was valuable — understanding why validation loops and password masking are needed made it clearer what Spectre.Console was actually solving. The refactoring process itself was a good exercise in recognizing when a library can simplify your code.

### Helper Methods
Private helper methods like `CheckIfLoggedIn()`, `CheckIfAvailable()` and `CheckIfSeller()` are used in `Marketplace` to avoid deeply nested if statements and keep public methods clean.

## What I Learned

- **Encapsulation**: Why private fields with public properties are better than public fields
- **LINQ**: How to filter, sort and transform collections cleanly instead of manual loops
- **Exception Handling**: The difference between returning error strings and throwing exceptions, and when each is appropriate
- **Separation of concerns**: Keeping UI code separate from business logic makes the code much easier to maintain
- **Third-party libraries**: How to install a NuGet package and integrate Spectre.Console and Microsoft.Data.Sqlite into an existing project
- **Refactoring**: How to improve existing working code — and that building the naive version first is often the best way to understand what you actually need
- **Git workflow**: Using branches, meaningful commit messages and pull requests
- **SQLite and databases**: How to create tables, write parameterised queries, and persist data to a file-based database
- **Database design**: Why load order matters (foreign key dependencies), why AUTOINCREMENT columns should not be manually inserted, and how to store enums and dates in SQL

## Challenges I Faced

1. **Designing the class structure**: Unlike Arbeidskrav 1, there was no prescribed hierarchy. Deciding what belongs in models vs services took planning.

2. **Exception handling vs return strings**: Initially all methods returned error strings. Refactoring to throw exceptions made the code cleaner but required understanding when exceptions are appropriate.

3. **LINQ syntax**: The lambda `=>` syntax was confusing at first. Learning that `listings.Where(l => l.Status == ListingStatus.Available)` is just a cleaner foreach loop made it click.

4. **Navigation flow**: Managing multiple menus in a console app without losing track of where the user is was tricky. Using `return` vs `break` correctly in nested loops required careful thinking.

5. **Spectre.Console integration**: Replacing the existing UI required understanding both the old code and the new library at the same time. Some patterns like `SelectionPrompt<EnumType>` with `.UseConverter()` took experimentation to get right.

6. **SQLite persistence**: Matching model property names to database column names took several iterations. Understanding that `AUTOINCREMENT` columns must not be included in `INSERT` statements, and that load order matters due to foreign key dependencies, were the key lessons from this step.

**Author**: Panida Finstad
**Course**: Backend Programming Year 1, Gokstad Academy

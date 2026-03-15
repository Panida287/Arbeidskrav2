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
    
}
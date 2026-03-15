namespace Arbeidskrav2.Models
{
    public class User
    {
        private string username;
        private string password;
        private List<Listing> listings;
        private List<Transaction> transactions;
        private List<Review> reviews;

        public User(string username, string password)
        {
            this.username = username; // TODO: add null check and duplicate username validation
            this.password = password; // TODO: add null, char, password matching and forgot password
            listings = new List<Listing>();
            transactions = new List<Transaction>();
            reviews = new List<Review>();
        }
        
        public string Username { get => username; }
        public List<Listing> Listings { get => listings; }
        public List<Transaction> Transactions { get => transactions; }
        public List<Review> Reviews { get => reviews; }

        public bool CheckPassword(string input)
        {
            return password == input;
        }
    }
}
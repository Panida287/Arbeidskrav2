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
            this.username = username;
            this.password = password;
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
        
        public void AddListing(Listing listing)
        {
            listings.Add(listing);
        }

        public void AddTransaction(Transaction transaction)
        {
            transactions.Add(transaction);
        }

        public void AddReview(Review review)
        {
            reviews.Add(review);
        }
        
    }
}
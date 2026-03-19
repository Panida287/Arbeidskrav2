namespace Arbeidskrav2.Models
{
    /// <summary>
    /// Represents a purchase transaction between a buyer and a seller.
    /// </summary>
    public class Transaction
    {
        private int id;
        private static int nextId = 1;
        private Listing listing;
        private User buyer;
        private User seller;
        private Review? review;
        private DateTime date;
        
        public Transaction(Listing listing, User buyer, User seller)
        {
            this.id = nextId++;
            this.listing = listing;
            this.buyer = buyer;
            this.seller = seller;
            this.date = DateTime.Now;
        }
        
        public int Id { get => id; }
        public void SetId(int id) { this.id = id; }
        public void SetDate(DateTime date) { this.date = date; }
        public Listing Listing { get => listing; }
        public User Buyer { get => buyer; }
        public User Seller { get => seller; }
        public Review Review { get => review; }
        public DateTime Date { get => date; }
        public void AddReview(Review review)
        {
            this.review = review;
        }
    }
}
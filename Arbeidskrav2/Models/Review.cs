namespace Arbeidskrav2.Models
{
    /// <summary>
    /// Represents a review left by a buyer after a transaction.
    /// </summary>
    public class Review
    {
        private int transactionId;
        private User buyer;
        private User seller;
        private Listing listing;
        private int reviewScore;
        private string reviewText;

        public Review(int reviewScore, string reviewText,  User buyer, User seller, Listing listing, int transactionId)
        {   this.buyer = buyer;
            this.seller = seller;
            this.listing = listing;
            this.transactionId = transactionId;
            if (reviewScore >= 1 && reviewScore <= 6)
                this.reviewScore = reviewScore;
            else
                this.reviewScore = 1;
            this.reviewText = reviewText;
        }
        
        public int TransactionId { get => transactionId; }
        public int ReviewScore { get => reviewScore; }
        public string ReviewText { get => reviewText; }
        public User Buyer { get => buyer; }
        public User Seller { get => seller; }
        public Listing Listing { get => listing; }
    }
}
namespace Arbeidskrav2.Models
{
    public class Review
    {
        private User buyer;
        private User seller;
        private Listing listing;
        private int reviewScore;
        private string reviewText;

        public Review(int reviewScore, string reviewText,  User buyer, User seller, Listing listing)
        {   this.buyer = buyer;
            this.seller = seller;
            this.listing = listing;
            if (reviewScore >= 1 && reviewScore <= 6)
                this.reviewScore = reviewScore;
            else
                this.reviewScore = 1; // TODO: throw exception later
            this.reviewText = reviewText;
        }
        
        public int ReviewScore { get => reviewScore; }
        public string ReviewText { get => reviewText; }
        public User Buyer { get => buyer; }
        public User Seller { get => seller; }
        public Listing Listing { get => listing; }
    }
}
using Arbeidskrav2.Enums;

namespace Arbeidskrav2.Models
{
    /// <summary>
    /// Represents an item listed for sale in the marketplace.
    /// </summary>
    public class Listing
    {
        private string itemName;
        private string itemDescription;
        private double itemPrice;
        private Category category;
        private Condition condition;
        private ListingStatus status;
        private User user;
        
        public Listing(string itemName, string itemDescription, double itemPrice, Category category, Condition condition, User user)
        {
            this.itemName = itemName;
            this.itemDescription = itemDescription;
            this.itemPrice = itemPrice;
            this.category = category;
            this.condition = condition;
            this.status = ListingStatus.Available;
            this.user = user;
        }
        
        public string ItemName { get => itemName; set => itemName = value; }
        public string ItemDescription { get => itemDescription; set => itemDescription = value; }
        public double ItemPrice { get => itemPrice; set => itemPrice = value; }
        public Category Category { get => category; set => category = value; }
        public Condition Condition { get => condition; set => condition = value; }
        public ListingStatus Status { get => status; set => status = value; } 
        public User Seller { get => user; }
        
    }
}
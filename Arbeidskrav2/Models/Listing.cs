using Arbeidskrav2.Enums;

namespace Arbeidskrav2.Models
{
    public class Listing
    {
        private static int nextId = 1;
        private int itemId;
        private string itemName;
        private string itemDescription;
        private double itemPrice;
        private Category category;
        private Condition condition;
        private ListingStatus status;
        private User user;
        
        public Listing(string itemName, string itemDescription, double itemPrice, Category category, Condition condition, User user)
        {
            itemId = nextId++;
            this.itemName = itemName;
            this.itemDescription = itemDescription;
            this.itemPrice = itemPrice;
            this.category = category;
            this.condition = condition;
            this.status = ListingStatus.Available;
            this.user = user;
        }
        
        public int ItemId { get => itemId; }
        public string ItemName { get => itemName; }
        public string ItemDescription { get => itemDescription; }
        public double ItemPrice { get => itemPrice; }
        public Category Category { get => category; }
        public Condition Condition { get => condition; }
        public ListingStatus Status { get => status; set => status = value; } 
        public User Seller { get => user; }
    }
}
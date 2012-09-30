using System.ComponentModel.DataAnnotations;

namespace otr_project.Models
{
    public class CartItemModel
    {
        [Key]
        public int id { get; set; }
        public string CartId { get; set; }
        public string ItemId { get; set; }
        
        public System.DateTime DateCreated { get; set; }

        [Required(ErrorMessage = "Item Pickup Date is required")]
        public System.DateTime PickupDate { get; set; }

        [Required(ErrorMessage = "Item Return Date is required")]
        public System.DateTime DropoffDate { get; set; }

        public decimal itemTotal { get; set; }
        public virtual ItemModel Item { get; set; }
    }
}
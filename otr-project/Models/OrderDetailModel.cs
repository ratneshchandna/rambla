using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System;

namespace otr_project.Models
{
    public class OrderDetailModel
    {
        [Key]
        [ScaffoldColumn(false)]
        public int OrderDetailId { get; set; }
        
        [ScaffoldColumn(false)]
        public string OrderId { get; set; }
        
        [ScaffoldColumn(false)]
        public string ItemModelId { get; set; }

        [Required]
        [MaxLength(56)]
        public string ItemName { get; set; }

        [Required]
        public string ItemDesc { get; set; }

        [Display(Name = "Category")]
        [Required(ErrorMessage = "Item category is required")]
        public int CategoryId { get; set; }

        public virtual Category Category { get; set; }
        /*
        public string ItemImageFileModelId { get; set; }

        public virtual ItemImageFileModel ItemImage { get; set; }
        */
        [DisplayName("Pickup Date")]
        [Required(ErrorMessage = "Item Pickup Date is required")]
        public DateTime PickupDate { get; set; }

        [DisplayName("Return Date")]
        [Required(ErrorMessage = "Item Return Date is required")]
        public DateTime DrofoffDate { get; set; }

        [DisplayName("Number Of Days")]
        public int NumberOfDays { get; set; }

        [DisplayName("Cost Per Day")]
        public decimal UnitPrice { get; set; }

        [DisplayName("Security Deposit")]
        public decimal SecurityDeposit { get; set; }

        [ScaffoldColumn(false)]
        public string OwnerCode { get; set; }

        [ScaffoldColumn(false)]
        public string RenterCode { get; set; }

        public virtual ItemModel Item { get; set; }
        
        [ScaffoldColumn(false)]
        public virtual Order Order { get; set; }

        public int? Status { get; set; }
    }

    public enum OrderStatus
    {
        // Order has been placed by borrower. Database entry created and borrower has paid using PayPal
        ORDER_TENTATIVE = 0, 
        
        // Items have been picked up and security codes have been enterd. Rental amount is added to Owner's account
        ORDER_COMPLETE = 1,
        
        // Removed cause it doesnt make sense.
        //ORDER_OPEN = 2,
        
        // Current date > Pickup date and no security code has been entered. 
        // An order can stay in this state for 2 days before it is cancelled.
        ORDER_LATE = 2,
        
        // State to describe if the order was cancelled by the system due to security codes not being entered.
        // Renter is reimbursed for the full amount and owner/renter get email notifications
        ORDER_TERMINATED = 3,

        // When there is a dispute between owner and borrower. Security Deposit is at play here.
        // Item is marked inactive from Rambla and is no longer searchable/rentable
        // Manual intervension! w00t w00t
        ORDER_DISPUTE = 4,

        // Current date > Return date and owner has not initiated dispute process
        // Order can stay in this state for no more than 2 days
        // Automatically jumps to ORDER_CLOSED if no action is taken
        ORDER_DELAYED_RETURN = 5,

        // Owner has received item in good condition. Happy scenario.
        // Security Deposit returned to borrower.
        // System sends emails to renter/borrower for rating each other
        ORDER_CLOSED_HAPPY = 6,
        
        // Next step after dispute resolution. Once the security deposit has been sent one way or another
        // Previous step *HAS* to be ORDER_DISPUTE
        // We need to track how many unhappy orders we have.
        ORDER_CLOSED_UNHAPPY = 7,
        
        //ORDER_NOTCOMPLETE = 5,
        //ORDER_LIMBO = 6
    }
}
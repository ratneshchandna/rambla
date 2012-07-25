using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Collections;
using System.Web.Mvc;
using System.Collections.Generic;

namespace otr_project.Models
{
    public class ItemModel
    {
        [Key]
        public string Id { get; set; }

        [Required]
        [MaxLength(56)]
        [Display(Name = "Item Name")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Description")]
        public string Desc { get; set; }

        [Display(Name = "Category")]
        [Required(ErrorMessage = "Item category is required")]
        public int CategoryId { get; set; }

        public int AddressId { get; set; }

        public int RegionId { get; set; }

        public int CountryId { get; set; }

        [Required]
        [Range(0.01,999999,ErrorMessage = "Please enter a valid price amount")]
        [Display(Name = "Cost/day")]
        public decimal CostPerDay { get; set; }

        [Range(0.01, 999999, ErrorMessage = "Please enter a valid price amount")]
        [Display(Name = "Security Deposit")]
        public decimal SecurityDeposit { get; set; }

        [Editable(false)]
        public bool isActive { get; set; }

        [Editable(false)]
        public string UserModelEmail { get; set; }

        public virtual UserModel Owner { get; set; }

        [Display(Name = "Agreement")]
        public string AgreementId { get; set; }

        public virtual Agreement Agreement { get; set; }

        [Display(Name = "Upload Picture")]
        [ScaffoldColumn(false)]
        public int ImageCount { get; set; }

        [DisplayName("Blackout Dates")]
        public virtual ICollection<BlackoutDate> BlackoutDates { get; set; }

        [DataType(DataType.DateTime)]
        [ScaffoldColumn(false)]
        [Editable(false)]
        public virtual System.DateTime DateCreated { get; set; }

        [DisplayName("Pickup Location")]
		public virtual Address PickupLocation { get; set; }

        public virtual Region ItemRegion { get; set; }

        public virtual Country ItemCountry { get; set; }

        public virtual Category Category { get; set; }

        public virtual ICollection<OrderDetailModel> OrderDetails { get; set; }

        public virtual ICollection<ItemImageFileModel> ItemImages { get; set; }
    }

    public class BlackoutDate
    {
        [Key]
        public int id { get; set; }

        [DataType(DataType.DateTime)]
        public virtual System.DateTime Date { get; set; }
       
        public string ItemModelId { get; set; }
    }

    public class Category
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public virtual ICollection<ItemModel> Items { get; set; }
    }
}
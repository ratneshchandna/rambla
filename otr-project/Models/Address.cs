using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace otr_project.Models
{
    public class Address
    {
        [Key]
        public int Id { get; set; }

        public string ItemModelId { get; set; }

        [Required]
        [RegularExpression("^[0-9a-zA-Z-.,\\s]{1,100}$", ErrorMessage = "Please enter a valid address")]
        [Display(Name = "Address 1")]
        public string StreetAddress1 { get; set; }

        [RegularExpression("^[0-9a-zA-Z-.,\\s]{1,100}$", ErrorMessage = "Please enter a valid address")]
        [Display(Name = "Address 2")]
        public string StreetAddress2 { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "City")]
        public string City { get; set; }

        [Required]
        [RegularExpression("^[abceghjklmnprstvxyABCEGHJKLMNPRSTVXY]{1}\\d{1}[abceghjklmnprstvwxyzABCEGHJKLMNPRSTVWXYZ]{1} *\\d{1}[abceghjklmnprstvwxyzABCEGHJKLMNPRSTVWXYZ]{1}\\d{1}$", ErrorMessage = "Please enter a valid postal code")]
        [Display(Name = "Postal Code")]
        public string PostalCode { get; set; }
}

    /*public class City
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "City")]
        public string Name { get; set; }

        public int RegionId { get; set; }
    }*/

    public class Region
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Province/State")]
        public string Name { get; set; }

        [Required]
        public int CountryId { get; set; }

        public virtual Country Country { get; set; }
    }

    public class Country
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Country")]
        public string Name { get; set; }

        public virtual ICollection<Region> Regions { get; set; }
    }
}
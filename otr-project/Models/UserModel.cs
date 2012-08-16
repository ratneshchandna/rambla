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
    public class UserModel
    {
        [Key]
        [RegularExpression(@"^(([^<>()[\]\\.,;:\s@\""]+(\.[^<>()[\]\\.,;:\s@\""]+)*)|(\"".+\""))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$", ErrorMessage = "Please enter a valid email address")]
        [Display(Name = "Email Address")]
        public string Email { get; set; }

        public string ActivationId { get; set; }

        [Editable(false)]
        public bool isFacebookUser { get; set; }

        public string FacebookUserId { get; set; }

        [Required]
        [RegularExpression("^[a-zA-Z-'.\\s]{1,15}$", ErrorMessage = "Please enter a valid first name")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [RegularExpression("^[a-zA-Z-'.\\s]{1,40}$", ErrorMessage = "Please enter a valid last name")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [RegularExpression("^[0-9a-zA-Z-.,\\s]{1,100}$", ErrorMessage = "Please enter a valid address")]
        [Display(Name = "Address 1")]
        public string Address1 { get; set; }

        [RegularExpression("^[0-9a-zA-Z-.,\\s]{1,100}$", ErrorMessage = "Please enter a valid address")]
        [Display(Name = "Address 2")]
        public string Address2 { get; set; }

        [RegularExpression("^[a-zA-Z'-,\\s]{1,40}$", ErrorMessage = "Please enter a valid city")]
        [Display(Name = "City")]
        public string City { get; set; }

        [Display(Name = "Province")]
        public int RegionId { get; set; }

        [Display(Name = "Country")]
        public int CountryId { get; set; }

        public string UserImageFileModelId { get; set; }

        [RegularExpression("^[abceghjklmnprstvxyABCEGHJKLMNPRSTVXY]{1}\\d{1}[abceghjklmnprstvwxyzABCEGHJKLMNPRSTVWXYZ]{1} *\\d{1}[abceghjklmnprstvwxyzABCEGHJKLMNPRSTVWXYZ]{1}\\d{1}$", ErrorMessage = "Please enter a valid postal code")]
        [Display(Name = "Postal Code")]
        public string PostalCode { get; set; }

        [RegularExpression("\\d{10}", ErrorMessage = "Please enter a valid cell phone number")]
        [Display(Name = "Cell Phone Number")]
        public string CellPhone { get; set; }

        [RegularExpression("\\d{10}", ErrorMessage = "Please enter a valid home phone number")]
        [Display(Name = "Home Phone Number")]
        public string HomePhone { get; set; }

        [Display(Name = "Earnings")]
        public decimal Earnings { set; get; }

        public virtual ICollection<Agreement> Agreements { get; set; }

        public virtual ICollection<ItemModel> Items { get; set; }

        public virtual ICollection<Badge> Badges { get; set; }

        public virtual FacebookUser FacebookProfile { get; set; }

        public virtual Region UserRegion { get; set; }

        public virtual Country UserCountry { get; set; }

        public virtual UserImageFileModel ProfilePicture { get; set; }
    }

    public class Agreement
    {
        [Key]
        public string Id { get; set; }

        [Display(Name = "File Name")]
        public string FileName { get; set; }

        public int ContentLength { get; set; }

        public string ContentType { get; set; }

        public virtual ICollection<ItemModel> Items { get; set; }

        public virtual ICollection<Category> Categories { get; set; }

        public string UserModelEmail { get; set; }
    }

    public class Badge
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public virtual ICollection<UserModel> Users { get; set; }
    }
}
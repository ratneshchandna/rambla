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
    public class FacebookUser
    {
        [Key]
        public string Id { get; set; }

        [Required]
        public string AccessToken { get; set; }

        [Required]
        public virtual DateTime Expires { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        public string UserModelEmail { get; set; }
    }
}
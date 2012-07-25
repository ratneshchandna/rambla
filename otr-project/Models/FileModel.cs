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
    public class FileModelBase
    {
        [Key]
        public string Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public virtual byte[] Contents { get; set; }

        [Required]
        public string ContentType { get; set; }
    }

    public class UserImageFileModel : FileModelBase 
    {
        public string UserModelEmail { get; set; }
    }

    public class ItemImageFileModel : FileModelBase
    {
        public string ItemModelId { get; set; }
    }
}


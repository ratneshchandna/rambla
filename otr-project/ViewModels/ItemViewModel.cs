using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using otr_project.Models;

namespace otr_project.ViewModels
{
    public class ItemViewModel
    {
        public virtual SelectList CategoryId { get; set; }
        public virtual SelectList AgreementId { get; set; }
        public virtual SelectList RegionId { get; set; }
        public virtual ItemModel item { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using otr_project.Models;

namespace otr_project.ViewModels
{
    public class SearchItemViewModel
    {
        public bool isItemBlocked { get; set; }

        public virtual ItemModel Item { get; set; }
    }
}

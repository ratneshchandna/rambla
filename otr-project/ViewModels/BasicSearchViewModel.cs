using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using otr_project.Models;
using Webdiyer.WebControls.Mvc;

namespace otr_project.ViewModels
{
    public class BasicSearchViewModel
    {
        public string Keyword { get; set; }
        public string Location { get; set; }

        public bool ItemWithImages { get; set; }

        public int CategoryId { get; set; }
        public int DistanceFromMe { get; set; }
        public int NumberOfItems { get; set; }
        
        public decimal MaximumPrice { get; set; }
        public decimal SecurityDeposit { get; set; }

        public virtual DateTime Pickup { get; set; }
        public virtual DateTime Dropoff { get; set; }
        
        public virtual Category Category { get; set; }

        public virtual PagedList<ItemModel> Items { get; set; }
    }
}

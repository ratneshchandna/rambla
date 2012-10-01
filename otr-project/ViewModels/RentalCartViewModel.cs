using System.Collections.Generic;
using otr_project.Models;

namespace otr_project.ViewModels
{
    public class RentalCartViewModel
    {
        public List<CartItemModel> CartItems { get; set; }
        public decimal CartTotal { get; set; }
    }
}
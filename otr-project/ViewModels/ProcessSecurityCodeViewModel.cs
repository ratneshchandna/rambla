using System.Collections.Generic;
using otr_project.Models;

namespace otr_project.ViewModels
{
    public class ProcessSecurityCodeViewModel
    {
        public virtual Order Order { get; set; }
        public virtual OrderDetailModel OrderDetail { get; set; }
        public string SecurityCode { get; set; }
        public bool IsOwner { get; set; }
    }
}
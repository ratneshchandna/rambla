using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Mvc.Mailer;
using System.Net.Mail;

namespace otr_project.Mailers
{ 
    public interface INotifyMailer
    {

        MailMessage OwnerPickupReminder(string toEmail, otr_project.Models.OrderDetailModel orderDeatil);
		MailMessage RenterPickupReminder(string toEmail, otr_project.Models.OrderDetailModel orderDeatil);
        MailMessage OwnerReturnReminder(string toEmail, otr_project.Models.OrderDetailModel orderDeatil);
        MailMessage RenterReturnReminder(string toEmail, otr_project.Models.OrderDetailModel orderDeatil);
        MailMessage OwnerOrderTerminated(string toEmail, otr_project.Models.OrderDetailModel orderDeatil);
        MailMessage RenterOrderTerminated(string toEmail, otr_project.Models.OrderDetailModel orderDeatil);
        MailMessage OwnerOrderClosed(string toEmail, otr_project.Models.OrderDetailModel orderDeatil);
        MailMessage RenterOrderClosed(string toEmail, otr_project.Models.OrderDetailModel orderDeatil);
	}
}
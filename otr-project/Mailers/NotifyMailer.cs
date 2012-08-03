using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Mvc.Mailer;
using System.Net.Mail;

namespace otr_project.Mailers
{ 
    public class NotifyMailer : MailerBase, INotifyMailer     
	{
		public NotifyMailer():
			base()
		{
			MasterName="_Layout";
		}


        public virtual MailMessage OwnerPickupReminder(string toEmail, otr_project.Models.OrderDetailModel orderDeatil)
		{
            var mailMessage = new MailMessage { Subject = "Rambla - Pickup Reminder" };
			
			mailMessage.To.Add(toEmail);
			ViewData = new System.Web.Mvc.ViewDataDictionary(orderDeatil);
			PopulateBody(mailMessage, viewName: "OwnerPickupReminder");

			return mailMessage;
		}


        public virtual MailMessage RenterPickupReminder(string toEmail, otr_project.Models.OrderDetailModel orderDeatil)
		{
            var mailMessage = new MailMessage { Subject = "Rambla - Pickup Reminder" };
			
			mailMessage.To.Add(toEmail);
            ViewData = new System.Web.Mvc.ViewDataDictionary(orderDeatil);
            PopulateBody(mailMessage, viewName: "RenterPickupReminder");

			return mailMessage;
		}

        public virtual MailMessage OwnerReturnReminder(string toEmail, otr_project.Models.OrderDetailModel orderDeatil)
        {
            var mailMessage = new MailMessage { Subject = "Rambla - Return Reminder" };

            mailMessage.To.Add(toEmail);
            ViewData = new System.Web.Mvc.ViewDataDictionary(orderDeatil);
            PopulateBody(mailMessage, viewName: "OwnerReturnReminder");

            return mailMessage;
        }

        public virtual MailMessage RenterReturnReminder(string toEmail, otr_project.Models.OrderDetailModel orderDeatil)
        {
            var mailMessage = new MailMessage { Subject = "Rambla - Return Reminder" };

            mailMessage.To.Add(toEmail);
            ViewData = new System.Web.Mvc.ViewDataDictionary(orderDeatil);
            PopulateBody(mailMessage, viewName: "RenterReturnReminder");

            return mailMessage;
        }

        public virtual MailMessage OwnerOrderTerminated(string toEmail, otr_project.Models.OrderDetailModel orderDeatil)
        {
            var mailMessage = new MailMessage { Subject = "Rambla - Order Terminated" };

            mailMessage.To.Add(toEmail);
            ViewData = new System.Web.Mvc.ViewDataDictionary(orderDeatil);
            PopulateBody(mailMessage, viewName: "OwnerOrderTerminated");

            return mailMessage;
        }

        public virtual MailMessage RenterOrderTerminated(string toEmail, otr_project.Models.OrderDetailModel orderDeatil)
        {
            var mailMessage = new MailMessage { Subject = "Rambla - Order Terminated" };

            mailMessage.To.Add(toEmail);
            ViewData = new System.Web.Mvc.ViewDataDictionary(orderDeatil);
            PopulateBody(mailMessage, viewName: "RenterOrderTerminated");

            return mailMessage;
        }

        public virtual MailMessage OwnerOrderClosed(string toEmail, otr_project.Models.OrderDetailModel orderDeatil)
        {
            var mailMessage = new MailMessage { Subject = "Rambla - Rental Complete" };

            mailMessage.To.Add(toEmail);
            ViewData = new System.Web.Mvc.ViewDataDictionary(orderDeatil);
            PopulateBody(mailMessage, viewName: "OwnerOrderClosed");

            return mailMessage;
        }

        public virtual MailMessage RenterOrderClosed(string toEmail, otr_project.Models.OrderDetailModel orderDeatil)
        {
            var mailMessage = new MailMessage { Subject = "Rambla - Rental Complete" };

            mailMessage.To.Add(toEmail);
            ViewData = new System.Web.Mvc.ViewDataDictionary(orderDeatil);
            PopulateBody(mailMessage, viewName: "RenterOrderClosed");

            return mailMessage;
        }
	}
}
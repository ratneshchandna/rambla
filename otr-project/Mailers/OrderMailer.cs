using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Mvc.Mailer;
using System.Net.Mail;

namespace otr_project.Mailers
{ 
    public class OrderMailer : MailerBase, IOrderMailer     
	{
		public OrderMailer():
			base()
		{
			MasterName="_Layout";
		}


        public virtual MailMessage SendOwnerCode(string toEmail, otr_project.Models.OrderDetailModel orderDeatil)
		{
            var mailMessage = new MailMessage { Subject = "Rambla Rental Security Code" };
			
			mailMessage.To.Add(toEmail);
			ViewData = new System.Web.Mvc.ViewDataDictionary(orderDeatil);
			PopulateBody(mailMessage, viewName: "SendOwnerCode");

			return mailMessage;
		}


        public virtual MailMessage SendRenterCode(string toEmail, otr_project.Models.OrderDetailModel orderDeatil)
		{
			var mailMessage = new MailMessage{Subject = "Rambla Rental Security Code"};
			
			mailMessage.To.Add(toEmail);
            ViewData = new System.Web.Mvc.ViewDataDictionary(orderDeatil);
            PopulateBody(mailMessage, viewName: "SendRenterCode");

			return mailMessage;
		}

		
	}
}
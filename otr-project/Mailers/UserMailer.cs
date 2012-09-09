using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Mvc.Mailer;
using System.Net.Mail;
using otr_project.Models;

namespace otr_project.Mailers
{ 
    public class UserMailer : MailerBase, IUserMailer     
	{
		public UserMailer():
			base()
		{
			MasterName="_Layout";
		}

		
		public virtual MailMessage Welcome(UserModel newUser)
		{
			var mailMessage = new MailMessage{Subject = "Welcome to Rambla!"};
			
			mailMessage.To.Add(newUser.Email);
            ViewData = new System.Web.Mvc.ViewDataDictionary(newUser);
			PopulateBody(mailMessage, viewName: "Welcome");

			return mailMessage;
		}

		
	}
}
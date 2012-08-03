using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Mvc.Mailer;
using System.Net.Mail;

namespace otr_project.Mailers
{ 
    public interface IOrderMailer
    {

        MailMessage SendOwnerCode(string toEmail, otr_project.Models.OrderDetailModel orderDeatil);
		
				
		MailMessage SendRenterCode(string toEmail, otr_project.Models.OrderDetailModel orderDeatil);
		
		
	}
}
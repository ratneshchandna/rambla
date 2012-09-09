using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Mvc.Mailer;
using System.Net.Mail;

namespace otr_project.Mailers
{ 
    public interface IUserMailer
    {
				
		MailMessage Welcome(otr_project.Models.UserModel newUser);
		
		
	}
}
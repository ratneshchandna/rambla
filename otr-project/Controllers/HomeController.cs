using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using otr_project.Models;
using otr_project.ViewModels;

namespace otr_project.Controllers
{
    [LoggingFilter]
    public class HomeController : Controller
    {
        MarketPlaceEntities market = new MarketPlaceEntities();
        ErrorMessageViewModel ErrorMessage = new ErrorMessageViewModel();

        public ActionResult Index()
        {
            if (Request.IsAuthenticated)
            {
                try
                {
                    var user = market.Users.Find(User.Identity.Name);
                }
                catch (Exception ex)
                {
                    ErrorMessage.ErrorCode = ErrorCode.UNKNOWN;
                    return View("ErrorMessage", ErrorMessage);
                }
            }
            ViewBag.Message = "Welcome to Rambla!";
            ViewBag.RegionId = new SelectList(market.Regions, "Id", "Name");
            
            return View();
        }

        public ActionResult About()
        {
            return View();
        }
    }
}
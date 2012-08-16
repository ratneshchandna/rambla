using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using otr_project_internal.Models;

namespace otr_project_internal.Controllers
{
    
    public class HomeController : Controller
    {
        Entities db = new Entities();

        public ActionResult Index()
        {
            ViewBag.Message = "Welcome to ASP.NET MVC!";

            return View();
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult Items()
        {
            var model = db.ItemModels;
            return View(model);
        }
    }
}

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
    public class ErrorController : Controller
    {
        ErrorMessageViewModel ErrorMessage = new ErrorMessageViewModel();

        public ActionResult Http404()
        {
            ViewBag.ErrorHeading = "Oops! Sorry, there is nothing here.";
            ViewBag.GoBack = Url.Action("Index", "Home");
            return View("../Shared/Error");
        }

        public ActionResult Http500()
        {
            ViewBag.ErrorHeading = "Oops! We've been bit by the beta bug.";
            ViewBag.ErrorMessage = "We're in beta and we're working diligently to fix such errors.";
            ViewBag.GoBack = Url.Action("Index", "Home");
            return View("../Shared/Error");
        }

        public ActionResult Http400()
        {
            ViewBag.ErrorHeading = "Oops! We couldn't quite understand your request.";
            ViewBag.ErrorMessage = "We apologize for the inconvinience. If you feel this error is unexpected, please let us know using the feedback button.";
            ViewBag.GoBack = Url.Action("Index", "Home");
            return View("../Shared/Error");
        }
    }
}
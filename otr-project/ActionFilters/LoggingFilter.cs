using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Principal;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using System.IO;
using System.Data;
using log4net;
using Elmah;

public class LoggingFilter : ActionFilterAttribute, IExceptionFilter
{
    private static readonly ILog log = LogManager.GetLogger(typeof(otr_project.MvcApplication));
    
    public override void OnActionExecuting(ActionExecutingContext filterContext)
    {
        /*
        filterContext.HttpContext.Trace.Write("(Logging Filter)Action Executing: " +
            filterContext.ActionDescriptor.ActionName);
        */
        log.Info("Executing " + filterContext.ActionDescriptor.ActionName + " Action - " + 
            filterContext.ActionDescriptor.ControllerDescriptor.ControllerName);
        base.OnActionExecuting(filterContext);
    }
    
    public override void OnActionExecuted(ActionExecutedContext filterContext)
    {
        if (filterContext.Exception != null)
        {
            //filterContext.HttpContext.Trace.Write("(Logging Filter)Exception thrown");
            log.Error(filterContext.Exception.GetType().ToString() + " Exception in " + filterContext.ActionDescriptor.ActionName +
                " Action - " + filterContext.ActionDescriptor.ControllerDescriptor.ControllerName);
            if (filterContext.ExceptionHandled)
                throw filterContext.Exception;
        }

        base.OnActionExecuted(filterContext);
    }
    
    void IExceptionFilter.OnException(ExceptionContext filterContext)
    {
        //Raise the exception signal so ELMAH can log the fucker. Otherwise ELMAH doesn't log when CustomErrors is turned on.
        ErrorSignal.FromCurrentContext().Raise(filterContext.Exception);
    }
}
using System.Web;
using System.Web.Mvc;

namespace JN.Web
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new ExceptionAttribute());
        }
    }

    public class ExceptionAttribute : FilterAttribute, IExceptionFilter
    {
        public virtual void OnException(ExceptionContext filterContext)
        {
            //记录日志
            JN.Services.Manager.logs.WriteErrorLog(filterContext.HttpContext.Request.Url.ToString(), filterContext.Exception);

            //转向
            filterContext.ExceptionHandled = true;
            filterContext.Result = new RedirectResult("~/Home/Error");
        }
    }
}

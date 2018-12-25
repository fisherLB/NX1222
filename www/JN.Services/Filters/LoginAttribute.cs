using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace JN.Services.Filters
{
    public class LoginAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            return UserLoginHelper.CurrentUser() != null;
        }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext);
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Controller.TempData["message"] = "你还没有登录或登录超时,请重新登录！";
            filterContext.Result = new RedirectResult("/Home/Index/");
            //base.HandleUnauthorizedRequest(filterContext);
        }
    }
}

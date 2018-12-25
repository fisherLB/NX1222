using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace JN.Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return RedirectToAction("Login", "UserCenter");
            //设置语言版本
            //string lang = Request["lang"];
            //if (!string.IsNullOrEmpty(lang))
            //{
            //    JN.Web.Framework.ResourceProvider.Culture = lang;
            //}
            //return View();
        }

        public ActionResult Error()
        {
            return View();
        }
    }
}
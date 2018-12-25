using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net.Http;
using JN.Data;
using JN.Data.Service;
using MvcCore.Controls;


namespace JN.Web.Areas.UserCenter.Controllers
{
    public class EnterPasswordController : BaseController
    {
        public ActionResult Index()
        {
            if (Session["enterpwd_pl"] != null && Session["enterpwd_url"] != null)
            {
                ViewBag.Url = Session["enterpwd_url"];
                ViewBag.PwdLevel = Session["enterpwd_pl"];
                return View();
            }
            else
            {
                ViewBag.ErrorMsg = "访问申请来路信息丢失，请重新进入。";
                return View("Error");
            }
        }

        [HttpPost]
        public JsonResult Index(string oldurl, string pwdlevel, string password)
        {
            if (string.IsNullOrEmpty(password))
                return Json(new { result = "err", refMsg = "请输入密码！" }, JsonRequestBehavior.AllowGet);

            if (pwdlevel.ToInt() < 2 || pwdlevel.ToInt() > 3)
                return Json(new { result = "err", refMsg = "只支持2、3级密码校验！" }, JsonRequestBehavior.AllowGet);

            if (pwdlevel.ToInt() == 2)
            {
                if (Umodel.Password2 == password.ToMD5().ToMD5())
                {
                    Session["enpwd2"] = "true";
                    return Json(new { result = "ok", refMsg = oldurl ?? "/usercenter/home"}, JsonRequestBehavior.AllowGet);
                }
            }
            if (pwdlevel.ToInt() == 3)
            {
                if (Umodel.Password3 == password.ToMD5().ToMD5())
                {
                    Session["enpwd3"] = "true";
                    return Json(new { result = "ok", refMsg = oldurl ?? "/usercenter/home"}, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new { result = "err", refMsg = "您输入的密码有误！"}, JsonRequestBehavior.AllowGet);
        }
    }
}
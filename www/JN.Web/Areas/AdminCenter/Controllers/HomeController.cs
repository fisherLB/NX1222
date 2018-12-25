using JN.Services.Manager;
using JN.Services.Tool;
using MvcCore.Controls;
using System;
using System.Collections.Generic;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace JN.Web.Areas.AdminCenter.Controllers
{
    public class HomeController : BaseController
    {
        //
        // GET: /AdminCenter/Index/
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult NoAuthority()
        {
            return View();
        }

        //退出
        public ActionResult Logout()
        {
            ActMessage = "退出系统";
            Services.AdminLoginHelper.AdminUserLogout();
            return Redirect(Url.Action("Index", "Login"));
        }

        public ActionResult ClearAllCache()
        {
            List<String> caches = MvcCore.Extensions.CacheExtensions.GetAllCache();
            foreach (var cachename in caches)
                MvcCore.Extensions.CacheExtensions.ClearCache(cachename);
            List<String> caches2 = Services.Tool.DataCache.GetAllCache();
            foreach (var cachename in caches2)
                Services.Tool.DataCache.ClearCache(cachename);
            Services.AdminLoginHelper.AdminUserLogout();
            return Redirect(Url.Action("Index", "Home"));
        }
        public ActionResult ChangePassword()
        {
            ActMessage = "修改密码";
            return View();
        }

        [HttpPost]
        public ActionResult ChangePassword(FormCollection fc)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                string oldpassword = fc["oldpassword"];
                string newpassword = fc["newpassword"];
                string confirmpassword = fc["confirmpassword"];
                string newpassword2 = fc["newpassword2"];
                string confirmpassword2 = fc["confirmpassword2"];

                if (oldpassword.Trim().Length == 0 || newpassword.Trim().Length == 0 || newpassword2.Trim().Length == 0)
                    throw new Exception("所填项不能为空");
                if (newpassword != confirmpassword) throw new Exception("新登录密码与确认登录密码不相符");
                if (newpassword != confirmpassword) throw new Exception("新二级密码与确认二级密码不相符");
                if (Amodel.Password2 != oldpassword.ToMD5().ToMD5()) throw new Exception("原二级密码不正确");

                Amodel.Password = newpassword.ToMD5().ToMD5();
                Amodel.Password2 = newpassword2.ToMD5().ToMD5();
                MvcCore.Unity.Get<Data.Service.IAdminUserService>().Update(Amodel);
                MvcCore.Unity.Get<Data.Service.ISysDBTool>().Commit();
                result.Status = 200;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
            return Json(result);
        }

        public JsonResult getchardata()
        {
            List<int> kdata = new List<int>();
            List<string> kdate = new List<string>();
            var lst = MvcCore.Unity.Get<Data.Service.IUserService>().List().Select(x => x.CreateTime).Distinct().ToList(); 
            foreach (var item in lst)
            {
                kdata.Add(MvcCore.Unity.Get<Data.Service.IUserService>().List(x => SqlFunctions.DateDiff("DAY", x.CreateTime, item) == 0).Count());
                kdate.Add(item.ToShortDateString());
                //kdata.Add(new double[] { DateTimeDiff.DateTimeToStamp(item), MvcCore.Unity.Get<Data.Service.IUserService>().List(x => SqlFunctions.DateDiff("DAY", x.CreateTime, item) == 0).Count() });
            }
            return Json(new { datas = kdata, dates = kdate }, JsonRequestBehavior.AllowGet);
        }
    }
}
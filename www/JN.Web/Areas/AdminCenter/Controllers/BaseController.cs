using JN.BLL;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace JN.Web.Areas.AdminCenter.Controllers
{
    public class BaseController : Controller
    {
        public Data.AdminUser Amodel { get; set; }
        public object ActPacket { get; set; }
        public string ActMessage { get; set; }
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            //校验用户是否已经登录
            var model = Services.AdminLoginHelper.CurrentAdmin();
            if (model != null && model.IsPassed)
            {
                Amodel = model;
                if (model.RoleID != 0) //排除超管
                {
                    string controllerName = filterContext.RouteData.Values["controller"].ToString().ToLower();
                    string actionName = filterContext.RouteData.Values["action"].ToString().ToLower();
                    if (controllerName != "home")
                    {
                        //需要验证的模块
                        if (MvcCore.Unity.Get<Data.Service.IAdminAuthorityService>().List(x => x.ControllerName == controllerName && x.ActionName == actionName).Count() > 0)
                        {
                            var adminVerify = MvcCore.Unity.Get<Data.Service.IAdminAuthorityService>().Single(x => x.ControllerName == controllerName && x.ActionName == actionName);

                            var role = MvcCore.Unity.Get<Data.Service.IAdminRoleService>().Single(model.RoleID);
                            if (role != null)
                            {
                                if (!(role.AuthorityIds + ",").Contains("," + adminVerify.ID + ","))
                                {
                                    Response.Redirect(Url.Action("NoAuthority", "Home"));
                                    filterContext.Result = new EmptyResult();
                                }
                            }
                            else
                            {
                                Response.Redirect(Url.Action("NoAuthority", "Home"));
                                filterContext.Result = new EmptyResult();
                            }
                        }
                        else
                        {
                            Response.Redirect(Url.Action("NoAuthority", "Home"));
                            filterContext.Result = new EmptyResult();
                        }
                    }
                }
            }
            else
            {
                Response.Redirect(Url.Action("Index", "Login"));
                filterContext.Result = new EmptyResult();
            }
        }

        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            base.OnActionExecuted(filterContext);
            ViewBag.Title = ActMessage;
            var model = Services.AdminLoginHelper.CurrentAdmin();
            if (model != null)
            {
                if (!string.IsNullOrEmpty(ActMessage))
                {
                    string controllerName = filterContext.RouteData.Values["controller"].ToString().ToLower();
                    string actionName = filterContext.RouteData.Values["action"].ToString().ToLower();

                    if (MvcCore.Unity.Get<Data.Service.IAdminAuthorityService>().List(x => x.ControllerName == controllerName && x.ActionName == actionName).Count() == 0)
                    {
                        MvcCore.Unity.Get<Data.Service.IAdminAuthorityService>().Add(new Data.AdminAuthority
                        {
                            ActionName = actionName,
                            Remark = "",
                            ControllerName = controllerName,
                            LastUpdateTime = DateTime.Now,
                            Name = ActMessage
                        });
                        MvcCore.Unity.Get<Data.Service.ISysDBTool>().Commit();
                    }

                    //记录操作日志，写进操作日志中
                    var log = new Data.ActLog();
                    log.ActContent = ActMessage;
                    log.CreateTime = DateTime.Now;
                    log.IP = Services.Tool.IPHelper.GetClientIp();
                    log.Location = controllerName + "/" + actionName;
                    //if (ActPacket != null)
                    //    log.PacketFile = Newtonsoft.Json.JsonConvert.SerializeObject(ActPacket);
                    //else
                    log.PacketFile = "";
                    log.Platform = "后台";
                    log.Source = "";
                    log.UID = model.ID;
                    log.UserName = model.AdminName;
                    MvcCore.Unity.Get<Data.Service.IActLogService>().Add(log);
                    MvcCore.Unity.Get<Data.Service.ISysDBTool>().Commit();
                    if (Request.UrlReferrer != null)
                    {
                        ViewBag.FormUrl = Request.UrlReferrer.ToString();
                    }
                }
            }
            else
            {
                Response.Redirect(Url.Action("Index", "Login"));
                filterContext.Result = new EmptyResult();
            }

        }


        protected NameValueCollection FormatQueryString(NameValueCollection nameValues)
        {
            NameValueCollection nvcollection = new NameValueCollection();
            foreach (var item in nameValues.AllKeys)
            {
                if (item.Equals("datefiled"))
                {
                    if (!String.IsNullOrEmpty(nameValues["dateform"]) && !String.IsNullOrEmpty(nameValues["dateto"]))
                    {
                        DateTime startDate = nameValues["dateform"].ToDateTime().AddSeconds(-1);
                        DateTime endDate = nameValues["dateto"].ToDateTime().AddDays(1);
                        nvcollection.Add(nameValues[item] + ">", startDate.ToString());
                        nvcollection.Add(nameValues[item] + "<", endDate.ToString());
                    }
                }
                 if (item.Equals("numberfiled"))
                {
                    if (!String.IsNullOrEmpty(nameValues["numberform"]) && !String.IsNullOrEmpty(nameValues["numberto"]))
                    {
                        int startNumber = nameValues["numberform"].ToInt() - 1;
                        int endNumber = nameValues["numberto"].ToInt() + 1;
                        nvcollection.Add(nameValues[item] + ">", startNumber.ToString());
                        nvcollection.Add(nameValues[item] + "<", endNumber.ToString());
                    }
                }
                 if (item.Equals("keyfiled"))
                {
                    if (!string.IsNullOrEmpty(nameValues["keyword"]))
                        nvcollection.Add(nameValues[item], nameValues["keyword"]);
                }
                 if (item.Equals("isasc") || !string.IsNullOrEmpty(nameValues["isasc"]))
                    nvcollection.Add(item, nameValues["isasc"]);
                 if (item.Equals("orderfiled") || !string.IsNullOrEmpty(nameValues["orderfiled"]))
                    nvcollection.Add(item, nameValues["orderfiled"]);
            }

            return nvcollection;
        }
    }


    //无法记录操作结果,暂不用
    public class LogActionFilter : ActionFilterAttribute
    {
        public string Message { get; set; }
        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            base.OnResultExecuted(filterContext);

            //TAdmin model = admins.IsLogin();
            //if (model != null)
            //{
            //    string controllerName = filterContext.RouteData.Values["controller"].ToString().ToLower();
            //    string actionName = filterContext.RouteData.Values["action"].ToString().ToLower();
            //    //记录操作日志，写进操作日志中
            //    TActLog log = new TActLog();
            //    log.ActContent = Message;
            //    log.CreateTime = DateTime.Now;
            //    log.IP = Common.Tool.GetClientIp();
            //    log.Location = controllerName + "/" + actionName;
            //    log.Packet = "";
            //    log.Platform = "后台";
            //    log.Source = "";
            //    log.UID = model.ID;
            //    log.UserName = model.AdminName;
            //    actlogs.Add(log);
            //}
        }
    }
}
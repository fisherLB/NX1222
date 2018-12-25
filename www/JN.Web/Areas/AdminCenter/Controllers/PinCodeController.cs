using JN.Data;
using JN.Data.Service;
using MvcCore.Controls;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;

namespace JN.Web.Areas.AdminCenter.Controllers
{
    public class PinCodeController : BaseController
    {
        private readonly IPINCodeService PINCodeService;
        private readonly IUserService UserService;
        private readonly ISysDBTool SysDBTool;
        private readonly IActLogService ActLogService;
      

        public PinCodeController(ISysDBTool SysDBTool,
            IPINCodeService PINCodeService,
            IUserService UserService,
            IActLogService ActLogService
            )
        {
            this.PINCodeService = PINCodeService;
            this.UserService = UserService;
            this.SysDBTool = SysDBTool;
            this.ActLogService = ActLogService;
            this.SysDBTool = SysDBTool;
        }


        public ActionResult UnUse(int? page)
        {
            ActMessage = "未使用活码列表";
            var list = PINCodeService.List(x => x.IsUsed == false).WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).ToList();
            if (Request["IsExport"] == "1")
            {
                string FileName = string.Format("{0}_{1}_{2}_{3}", DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute);
                MvcCore.Extensions.ExcelHelperV2.ToExcel(list.ToList()).SaveToExcel(Server.MapPath("/upfile/" + FileName + ".xls"));
                return File(Server.MapPath("/upfile/" + FileName + ".xls"), "application/ms-excel", FileName + ".xls");
            }
            return View("Index", list.ToPagedList(page ?? 1, 20));
        }

        public ActionResult Used(int? page)
        {
            ActMessage = "已使用PIN码列表";
            var list = PINCodeService.List(x => x.IsUsed == true).WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).ToList();
            if (Request["IsExport"] == "1")
            {
                string FileName = string.Format("{0}_{1}_{2}_{3}", DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute);
                MvcCore.Extensions.ExcelHelperV2.ToExcel(list.ToList()).SaveToExcel(Server.MapPath("/upfile/" + FileName + ".xls"));
                return File(Server.MapPath("/upfile/" + FileName + ".xls"), "application/ms-excel", FileName + ".xls");
            }
            return View("Index", list.ToPagedList(page ?? 1, 20));
        }

        public ActionResult Delivery()
        {
            ActMessage = "派送PIN码";
            return View();
        }

        [HttpPost]
        public ActionResult Delivery(FormCollection form)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                string username = form["username"];
                string buynumber = form["buynumber"];
                string investment = form["investment"];

                var onUser = UserService.Single(x => x.UserName == username.Trim());
                if (onUser == null) throw new Exception("用户不存在");
                if (buynumber.ToInt() <= 0) throw new Exception("派送数量不正确");

                for (int i = 0; i < buynumber.ToInt(); i++)
                {
                    //生成PIN码
                    string keycode = GetPINNumber(15);
                    var model = new Data.PINCode { 
                        CreateTime = DateTime.Now, 
                        OriginDescribe = "后台派送", 
                        Investment = 0, 
                        IsUsed = false, 
                        KeyCode = keycode, 
                        UID = onUser.ID, 
                        UserName = onUser.UserName,
                        OriginUID = onUser.ID,
                        OriginUserName = onUser.UserName 
                    };
                    PINCodeService.Add(model);
                    SysDBTool.Commit();
                }
                result.Status = 200;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                Services.Manager.logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
            return Json(result);
        }

        public ActionResult Delete(int id)
        {
            var model = PINCodeService.Single(id);
            if (model != null)
            {
                if (model.IsUsed)
                {
                    ViewBag.ErrorMsg = "已使用的PIN码不能删除！";
                    return View("Error");
                }
                else
                {
                    PINCodeService.Delete(id);
                    SysDBTool.Commit();
                    if (Request.UrlReferrer != null)
                    {
                        ViewBag.FormUrl = Request.UrlReferrer.ToString();
                    }
                    ViewBag.SuccessMsg = "PIN码“" + model.KeyCode + "”已被删除！";
                    return View("Success");
                }
            }
            ViewBag.ErrorMsg = "记录不存在或已被删除！";
            return View("Error");
        }

        //生成真实订单号
        public string GetPINNumber(int num)
        {
            DateTime dateTime = DateTime.Now;
            string result = JN.Services.Tool.StringHelp.GettRandomCode(num);//4位随机数字
            if (IsHave(result))
            {
                return GetPINNumber(num);
            }
            return result;
        }

        //检查订单号是否重复
        public bool IsHave(string number)
        {
            return PINCodeService.List(x => x.KeyCode == number).Count() > 0;
        }
    }
}

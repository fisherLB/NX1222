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
    public class AgentController : BaseController
    {
        private readonly IUserService UserService;
        private readonly ISysDBTool SysDBTool;
        private readonly IActLogService ActLogService;
     

        public AgentController(ISysDBTool SysDBTool,
            IUserService UserService,
            IActLogService ActLogService
          )
        {
            this.UserService = UserService;
            this.SysDBTool = SysDBTool;
            this.ActLogService = ActLogService;
            this.SysDBTool = SysDBTool;
        }
        public ActionResult Delivery()
        {
            ActMessage = "指定一个经理";
            return View();
        }

        [HttpPost]
        public ActionResult Delivery(FormCollection form)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                string username = form["username"];
                string agentname = form["agentname"];
                string type = form["type"];
                string remark = form["remark"];

                var onUser = UserService.Single(x => x.UserName == username.Trim());
                if (onUser == null) throw new Exception("用户不存在");
                //if (string.IsNullOrEmpty(agentname)) throw new Exception("经理编号不能为空");
                //if (UserService.List(x => x.AgentName == agentname.Trim()).Count() > 0) throw new Exception("经理编号已被使用");
                if (remark.Trim().Length > 100) throw new Exception("备注长度不能超过100个字节");
                //if (onUser.IsAgent ?? false) throw new Exception("该会员已是经理，无需要重复指定");
                if ((onUser.AgentLevel ?? 0) == type.ToInt() && type.ToInt()==1) throw new Exception("该会员已经是经理，无需要重复指定");
                if (onUser.IsProTeam == true && type.ToInt() == 2) throw new Exception("该会员已经是专业团队，无需重复指定");

                if (type.ToInt() == 1)
                {
                    onUser.AgentLevel = type.ToInt();
                    onUser.AgentName = onUser.UserName;
                    onUser.IsAgent = true;
                    onUser.ApplyAgentTime = DateTime.Now;
                    onUser.ApplyAgentRemark = remark;
                }
                else
                {
                    onUser.IsProTeam = true;
                }
                UserService.Update(onUser);
                SysDBTool.Commit();
                result.Status = 200;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                Services.Manager.logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
            return Json(result);
        }
        public ActionResult Index(int? page)
        {
            ActMessage = "商务中心列表";
            var list = UserService.List(x => (x.IsAgent ?? false)).WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).OrderByDescending(x => x.ID).ToList();
            if (Request["IsExport"] == "1")
            {
                string FileName = string.Format("{0}_{1}_{2}_{3}", DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute);
                MvcCore.Extensions.ExcelHelperV2.ToExcel(list.ToList()).SaveToExcel(Server.MapPath("/upfile/" + FileName + ".xls"));
                return File(Server.MapPath("/upfile/" + FileName + ".xls"), "application/ms-excel", FileName + ".xls");
            }
            return View(list.ToPagedList(page ?? 1, 20));
        }

        public ActionResult UnPassed(int? page)
        {
            ActMessage = "待审核的商务中心";
            var list = UserService.List(x => !(x.IsAgent ?? false) && !string.IsNullOrEmpty(x.AgentName)).WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).OrderByDescending(x => x.ID).ToList();
            if (Request["IsExport"] == "1")
            {
                string FileName = string.Format("{0}_{1}_{2}_{3}", DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute);
                MvcCore.Extensions.ExcelHelperV2.ToExcel(list.ToList()).SaveToExcel(Server.MapPath("/upfile/" + FileName + ".xls"));
                return File(Server.MapPath("/upfile/" + FileName + ".xls"), "application/ms-excel", FileName + ".xls");
            }
            return View(list.ToPagedList(page ?? 1, 20));
        }

        public ActionResult doCancel(int id)
        {
            var model = UserService.Single(id);
            if (model != null)
            {
                model.IsAgent = false;
                model.AgentName = "";
                model.ApplyAgentRemark = "商务中心被取消";
                UserService.Update(model);
                SysDBTool.Commit();
                ViewBag.SuccessMsg = "会员“" + model.UserName + "”商务中心资格已被取消！";
                return View("Success");
            }
            ViewBag.ErrorMsg = "系统异常！请查看系统日志。";
            return View("Error");
        }

        public ActionResult doPass(int id)
        {
            var model = UserService.Single(id);
            if (model != null)
            {
                model.IsAgent = true;
                UserService.Update(model);
                SysDBTool.Commit();
                ViewBag.SuccessMsg = "审核通过会员“" + model.UserName + "”成为商务中心！";
                return View("Success");
            }
            ViewBag.ErrorMsg = "系统异常！请查看系统日志。";
            return View("Error");
        }
    }
}

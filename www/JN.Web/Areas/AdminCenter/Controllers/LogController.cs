using JN.Data;
using JN.Data.Service;
using JN.Services.Tool;
using MvcCore.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using PagedList;

namespace JN.Web.Areas.AdminCenter.Controllers
{
    public class LogController : BaseController
    {
        private readonly ISysDBTool SysDBTool;
        private readonly IActLogService ActLogService;
        private readonly ISysLogService SysLogService;
        private readonly IWarningLogService WarningLogService;
        private readonly ISMSLogService SMSLogService;
        

        public LogController(ISysDBTool SysDBTool,
            IActLogService ActLogService, 
            ISysLogService SysLogService,
            IWarningLogService WarningLogService,
            ISMSLogService SMSLogService
        )
        {
            this.SysDBTool = SysDBTool;
            this.ActLogService = ActLogService;
            this.SysLogService = SysLogService;
            this.WarningLogService = WarningLogService;
            this.SMSLogService = SMSLogService;
            this.SysDBTool = SysDBTool;
        }

        public ActionResult SysState()
        {
            return View();
        }

        public ActionResult UserActLog(int? page)
        {
            ViewBag.Title = "用户行为日志";
            var list = ActLogService.List(x => x.Platform == "会员").WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).OrderByDescending(x => x.ID).ToList();
            return View("ActLog", list.ToPagedList(page ?? 1, 20));
        }

        public ActionResult AdminActLog(int? page)
        {
            ViewBag.Title = "管理员行为日志";
            var list = ActLogService.List(x => x.Platform == "后台").WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).OrderByDescending(x => x.ID).ToList();
            return View("ActLog", list.ToPagedList(page ?? 1, 20));
        }
        public ActionResult SysLog(int? page)
        {
            ViewBag.Title = "系统日志";
            var list = SysLogService.List(x => x.Type == "系统日志").WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).OrderByDescending(x => x.ID).ToList();
            return View(list.ToPagedList(page ?? 1, 20));
        }

        public ActionResult WarningLog(int? page)
        {
            ViewBag.Title = "预警日志";
            var list = WarningLogService.List().WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).OrderByDescending(x => x.ID).ToList();
            return View(list.ToPagedList(page ?? 1, 20));
        }

        public ActionResult SMSLog(int? page)
        {
            ViewBag.Title = "短信发送日志";
            var list = SMSLogService.List().WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).OrderByDescending(x => x.ID).ToList();
            return View(list.ToPagedList(page ?? 1, 20));
        }


        public ActionResult timingplanlog(int? page)
        {
            ViewBag.Title = "作业计划日志";
            var list = SysLogService.List(a => a.Type == "作业日志").WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).OrderByDescending(x => x.ID).ToList();
            return View("SysLog", list.ToPagedList(page ?? 1, 20));
        }
    }
}

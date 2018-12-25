using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using JN.Data;
using JN.Data.Service;
using MvcCore.Controls;
using PagedList;

namespace JN.Web.Areas.UserCenter.Controllers
{
    public class NoticeController : BaseController
    {
        private readonly INoticeService NoticeService;
        private readonly ISysDBTool SysDBTool;
        private readonly IActLogService ActLogService;
     

        public NoticeController(ISysDBTool SysDBTool,
            INoticeService NoticeService,
            IActLogService ActLogService
            )
        {
            this.NoticeService = NoticeService;
            this.SysDBTool = SysDBTool;
            this.ActLogService = ActLogService;
            this.SysDBTool = SysDBTool;
        }

        public ActionResult Index(int? page)
        {
            ViewBag.Title = "公告列表";
            var list = NoticeService.List().OrderByDescending(x => x.ID).ToList();
            return View(list.ToPagedList(page ?? 1, 20));
        }

        public ActionResult Detail(int id)
        {
            ActMessage = "公告信息";
            var model = NoticeService.Single(id);
            if (model != null)
                return View(model);
            else
            {
                ViewBag.ErrorMsg = "记录不存在或已被删除";
                return View("Error");
            }
        }
    }
}

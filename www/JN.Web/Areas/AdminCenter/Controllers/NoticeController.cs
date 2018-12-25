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
    [ValidateInput(false)]
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
            ActMessage = "公告列表";
            var list = NoticeService.List().OrderByDescending(x => x.ID).ToList();
            return View(list.ToPagedList(page ?? 1, 20));
        }

        public ActionResult Modify(int? id)
        {
            ActMessage = "发布公告";
            var model = new Data.Notice();
            if (id.HasValue)
            {
                ActMessage = "修改公告";
                model = NoticeService.Single(id);
            }
            return View(model);
        }

        public ActionResult NoticeCommand(int id, string commandtype)
        {
            var model = NoticeService.Single(id);
            if (commandtype.ToLower() == "ontop")
                model.IsTop = true;
            else if (commandtype.ToLower() == "untop")
                model.IsTop = false;
            NoticeService.Update(model);
            SysDBTool.Commit();
            return RedirectToAction("Index", "Notice");
        }

        public ActionResult Delete(int id)
        {
            var model = NoticeService.Single(id);
            if (model != null)
            {
                ActPacket = model;
                NoticeService.Delete(id);
                SysDBTool.Commit();
                ViewBag.SuccessMsg = "“" + model.Title + "”已被删除！";
                return View("Success");
            }
            ViewBag.ErrorMsg = "记录不存在或已被删除！";
            return View("Error");
        }


        [HttpPost]
        public ActionResult Modify(FormCollection fc)
        {
            var result = new ReturnResult();
            try
            {
                var entity = NoticeService.SingleAndInit(fc["ID"].ToInt());
                TryUpdateModel(entity, fc.AllKeys);
                if (entity.ID > 0)
                {
                    NoticeService.Update(entity);
                }
                else
                {
                    entity.CreateTime = DateTime.Now;
                    NoticeService.Add(entity);
                }
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
    }
}

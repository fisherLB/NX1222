using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using JN.Data;
using JN.Data.Service;
using MvcCore.Controls;
using Webdiyer.WebControls.Mvc;
using JN.Services.Manager;
using JN.Services.Tool;
using System.IO;

namespace JN.Web.Areas.UserCenter.Controllers
{
    [ValidateInput(false)]
    public class MailController : BaseController
    {
        private readonly IUserService UserService;
        private readonly IMessageService MessageService;
        private readonly ISysDBTool SysDBTool;
        private readonly IActLogService ActLogService;
       

        public MailController(ISysDBTool SysDBTool,
            IMessageService MessageService,
            IUserService UserService,
            IActLogService ActLogService
            )
        {
            this.UserService = UserService;
            this.MessageService = MessageService;
            this.SysDBTool = SysDBTool;
            this.ActLogService = ActLogService;
            this.SysDBTool = SysDBTool;
        }

        public ActionResult Write(int? page)
        {
            ViewBag.recipient = "Admin";
            if (!string.IsNullOrEmpty(Request["r"]))
            {
                var model = MessageService.Single(Request["r"].ToInt());
                if (model != null)
                {
                    ViewBag.recipient = model.FormUserName;
                    ViewBag.subject = "回复：" + model.Title;
                    ViewBag.content = "\n\n\n\n\n----------------------------原文---------------------------- \n" + model.Content;
                }
            }

            if (!string.IsNullOrEmpty(Request["f"]))
            {
                var model = MessageService.Single(Request["f"].ToInt());
                if (model != null)
                {
                    ViewBag.recipient = "";
                    ViewBag.subject = "转发：" + model.Title;
                    ViewBag.content = "\n\n\n\n\n----------------------------原文---------------------------- \n" + model.Content;
                }
            }

            ActMessage = "写邮件";

            var list = MessageService.List(x => x.UID == Umodel.ID).OrderByDescending(x => x.ID).WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).ToList().ToPagedList(page ?? 1, 20);
            if (Request.IsAjaxRequest())
                return PartialView("_Write", list);
            return View(list);
        }

        [HttpPost]
        public ActionResult Write(FormCollection form)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                string recipient = form["recipient"];
                string subject = form["subject"];
                string content = form["content"];
                string messagetype = form["messagetype"];

                string imgurl = "";

                if (Request.Files.Count > 0)
                {
                    HttpPostedFileBase file = Request.Files[0];
                    if ((file != null) && (file.ContentLength > 0))
                    {
                        if (!FileValidation.IsAllowedExtension(file, new FileExtension[] { FileExtension.PNG, FileExtension.JPG, FileExtension.BMP }))
                            throw new Exception("非法上传，您只可以上传图片格式的文件！");
                        var newfilename = Guid.NewGuid() + Path.GetExtension(file.FileName).ToLower();
                        if (!Directory.Exists(Request.MapPath("~/upfile")))
                            Directory.CreateDirectory(Request.MapPath("~/upfile"));
                        var fileName = Path.Combine(Request.MapPath("~/upfile"), newfilename);
                        try
                        {
                            file.SaveAs(fileName);
                            var thumbnailfilename = UploadPic.MakeThumbnail(fileName, Request.MapPath("~/upfile/"), 1024, 768, "EQU");
                            System.IO.File.Delete(fileName); //删除原文件
                            imgurl = "/upfile/" + thumbnailfilename;
                        }
                        catch
                        {
                            //
                        }
                    }
                }

                if (string.IsNullOrEmpty(recipient) || string.IsNullOrEmpty(subject)) throw new Exception("邮件标题不能为空");

                int toUID = 0;
                if (recipient == "Admin") recipient = "管理员";
                if (recipient.Trim() != "管理员")
                {
                    if (UserService.List(x => x.UserName.Equals(recipient.Trim())).Count() <= 0)
                        throw new Exception("收件人不存在");
                    else
                        toUID = UserService.Single(x => x.UserName.Equals(recipient.Trim())).ID;

                }

                var model = new Data.Message();
                model.Attachment = imgurl;
                model.MessageType = messagetype;
                model.Content = content;
                model.CreateTime = DateTime.Now;
                model.FormUID = Umodel.ID;
                model.FormUserName = Umodel.UserName;
                model.ForwardID = 0;
                model.IsFlag = false;
                model.IsForward = false;
                model.IsRead = false;
                model.IsReply = false;
                model.IsSendSuccessful = true;
                model.IsStar = false;
                model.ReplyID = 0;
                model.Title = subject;
                model.ToUID = toUID;
                model.ToUserName = recipient.Trim();
                model.UID = Umodel.ID;

                var model2 = new Data.Message();
                model2.Attachment = model.Attachment;
                model2.MessageType = model.MessageType;
                model2.Content = model.Content;
                model2.CreateTime = model.CreateTime;
                model2.FormUID = model.FormUID;
                model2.FormUserName = model.FormUserName;
                model2.ForwardID = model.ForwardID;
                model2.IsFlag = model.IsFlag;
                model2.IsForward = model.IsForward;
                model2.IsRead = model.IsRead;
                model2.IsReply = model.IsReply;
                model2.IsSendSuccessful = model.IsSendSuccessful;
                model2.IsStar = model.IsStar;
                model2.ReplyID = model.ReplyID;
                model2.Title = model.Title;
                model2.ToUID = model.ToUID;
                model2.ToUserName = model.ToUserName;
                model2.UID = model.ToUID;

                MessageService.Add(model);
                MessageService.Add(model2);
                SysDBTool.Commit();

                if (model.ID > 0 && model2.ID > 0)
                {
                    model.RelationID = model2.ID;
                    MessageService.Update(model);
                    SysDBTool.Commit();
                    model2.RelationID = model.ID;
                    MessageService.Update(model2);
                    SysDBTool.Commit();
                    result.Status = 200;
                }
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
            return Json(result);
        }

        //加载邮件内容视图
        public ActionResult _PartialContent(int id)
        {
            var model = MessageService.Single(x => x.ID == id && (x.UID == Umodel.ID || x.ToUID == Umodel.ID || x.FormUID == Umodel.ID));
            if (model == null)
            {
                return Content("记录不存在或已被删除!");
            }
            model.IsRead = true;
            MessageService.Update(model);
            SysDBTool.Commit();
            ActMessage = "查看邮件";
            return View(model);
        }
    }
}

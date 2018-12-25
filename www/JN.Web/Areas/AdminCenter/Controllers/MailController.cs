using JN.Data;
using JN.Data.Service;
using MvcCore.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using JN.Services.Tool;
using System.IO;

namespace JN.Web.Areas.AdminCenter.Controllers
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

        public ActionResult Inbox(int? page)
        {
            ActMessage = "收件箱";
            var list = MessageService.List(x => x.UID == 0 && x.ToUID == 0).WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).OrderByDescending(x => x.ID).ToList();
            return View(list.ToPagedList(page ?? 1, 20));
        }

        public ActionResult Sent(int? page)
        {
            ActMessage = "发件箱";
            var list = MessageService.List(x => x.UID == 0 && x.FormUID == 0 && x.IsSendSuccessful).WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).OrderByDescending(x => x.ID).ToList();
            return View(list.ToPagedList(page ?? 1, 20));
        }

        public ActionResult Draft(int? page)
        {
            ActMessage = "草稿箱";
            var list = MessageService.List(x => x.UID == 0 && x.FormUID == 0 && !x.IsSendSuccessful).WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).OrderByDescending(x => x.ID).ToList();
            return View(list.ToPagedList(page ?? 1, 20));
        }

        public ActionResult Write()
        {
            if (!string.IsNullOrEmpty(Request["r"]))
            {
                var model = MessageService.Single(Request["r"].ToInt());
                if (model != null)
                {
                    ViewBag.recipient = model.FormUserName;
                    ViewBag.subject = "回复：" + model.Title;
                    ViewBag.messagetype = model.MessageType;
                    ViewBag.content = "\n\n\n\n\n----------------------------原文---------------------------- \n" + model.Content;
                }
            }

            if (!string.IsNullOrEmpty(Request["f"]))
            {
                var model = MessageService.Single(Request["f"].ToInt());
                if (model != null)
                {
                    ViewBag.recipient = "";
                    ViewBag.messagetype = model.MessageType;
                    ViewBag.subject = "转发：" + model.Title;
                    ViewBag.content = "\n\n\n\n\n----------------------------原文---------------------------- \n" + model.Content;
                }
            }
            ActMessage = "写邮件";
            return View();
        }

        [HttpPost]
        public ActionResult Write(FormCollection form)
        {
            string recipient = form["recipient"];
            string messagetype = form["messagetype"];
            string subject = form["subject"];
            string content = form["content"];
            string strErr = "";

            if (recipient.Trim().Length == 0 || subject.Trim().Length == 0)
            {
                strErr += "收件人、标题不能为空 <br />";
            }

            if (UserService.List(x => x.UserName.Equals(recipient.Trim())).Count() <= 0)
            {
                strErr += "收件人不存在 <br />";
            }

            if (strErr != "")
            {
                ViewBag.ErrorMsg = "抱赚您填写的信息有误： <br />" + strErr + "请核实后重新提交！";
                return View("Error");
            }

            HttpPostedFileBase file = Request.Files["imgurl"];
            string imgurl = "";
            if (!string.IsNullOrEmpty(file.FileName))
            {
                if (!FileValidation.IsAllowedExtension(file, new FileExtension[] { FileExtension.PNG, FileExtension.JPG, FileExtension.BMP }))
                {
                    ViewBag.ErrorMsg = "非法上传，您只可以上传图片格式的文件！";
                    return View("Error");
                }
                var newfilename = Guid.NewGuid() + Path.GetExtension(file.FileName).ToLower();
                var fileName = Path.Combine(Request.MapPath("~/upfile"), newfilename);
                try
                {
                    file.SaveAs(fileName);
                    imgurl = "/upfile/" + newfilename;
                }
                catch
                {
                    //
                }
            }

            var model = new Data.Message();
            model.Attachment = imgurl;
            model.MessageType = messagetype;
            model.Content = content;
            model.CreateTime = DateTime.Now;
            model.FormUID = 0;
            model.FormUserName = "管理员";
            model.ForwardID = 0;
            model.IsFlag = false;
            model.IsForward = false;
            model.IsRead = false;
            model.IsReply = false;
            model.IsSendSuccessful = true;
            model.IsStar = false;
            model.ReplyID = 0;
            model.Title = subject;
            model.ToUID = UserService.Single(x => x.UserName.Equals(recipient.Trim())).ID;
            model.ToUserName = recipient.Trim();
            model.UID = 0;

            var model2 = new Data.Message();
            model2.Attachment = model.Attachment;
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
            model2.MessageType = model.MessageType;

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
                ViewBag.SuccessMsg = "邮件发送成功！";
                ActPacket = model;
                return View("Success");
            }

            ViewBag.ErrorMsg = "系统异常！请查看系统日志。";
            return View("Error");
        }

        //加载邮件内容视图
        public ActionResult SubMailContent(int id)
        {
            var model = MessageService.Single(id);
            if (model == null)
            {
                ViewBag.ErrorMsg = "记录不存在或已被删除！";
                return View("Error");
            }
            model.IsRead = true;
            MessageService.Update(model);
            SysDBTool.Commit();
            ActMessage = "查看邮件：" + model.Title;
            return View(model);
        }

        public ActionResult Delete(int id)
        {
            var model = MessageService.Single(id);
            if (model != null)
            {
                ActPacket = model;
                MessageService.Delete(id);
                SysDBTool.Commit();
                Data.Message realtion = MessageService.Single(x => x.RelationID == id);
                if (realtion != null)
                    MessageService.Delete(realtion.ID);

                ViewBag.SuccessMsg = "邮件“" + model.Title + "”已被删除！";
            }
            ViewBag.ErrorMsg = "记录不存在或已被删除！";
            return View("Error");
        }

        public ActionResult doCommand(string ids, string commandtype)
        {
            Dictionary<string, string> dicCommand = new Dictionary<string, string>();
            if (commandtype.ToLower() == "makeread")
                dicCommand.Add("IsRead", "1");
            else if (commandtype.ToLower() == "makeunread")
                dicCommand.Add("IsRead", "0");
            else if (commandtype.ToLower() == "makeflag")
                dicCommand.Add("IsFlag", "1");
            else if (commandtype.ToLower() == "makeunflag")
                dicCommand.Add("IsFlag", "0");
            MessageService.Update(new Data.Message(), dicCommand, "id in (" + ids.TrimEnd(',').TrimStart(',') + ")");

            SysDBTool.Commit();
            return RedirectToAction("Index", "Notice");
        }


        public ActionResult MultiDelete(string ids)
        {
            string[] mids = ids.TrimEnd(',').TrimStart(',').Split(',');
            foreach (string id in mids)
            {
                int m_id = id.ToInt();
                if (m_id > 0)
                {
                    MessageService.Delete(m_id);
                    SysDBTool.Commit();
                    Data.Message realtion = MessageService.Single(x => x.RelationID == m_id);
                    if (realtion != null) MessageService.Delete(realtion.ID);
                    SysDBTool.Commit();
                }
            }
            return Content("ok");
        }
    }
}

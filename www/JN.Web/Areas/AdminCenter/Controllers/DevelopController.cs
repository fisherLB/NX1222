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

namespace JN.Web.Areas.AdminCenter.Controllers
{
    public class DevelopController : BaseController
    {
        private readonly ITimingPlanService TimingPlanService;
        private readonly ISysDBTool SysDBTool;
        private readonly IActLogService ActLogService;
      

        public DevelopController(ISysDBTool SysDBTool,
            IActLogService ActLogService,
        ITimingPlanService TimingPlanService)
        {
            this.SysDBTool = SysDBTool;
            this.ActLogService = ActLogService;
            this.SysDBTool = SysDBTool;
            this.TimingPlanService = TimingPlanService;
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Icons()
        {
            return View();
        }

        public ActionResult UserIcons()
        {
            return View();
        }

        public ActionResult UI()
        {
            return View();
        }

        public ActionResult Widgets()
        {
            return View();
        }
        public ActionResult AdvancedProfile()
        {
            return View();
        }

        public ActionResult SysState()
        {
            return View();
        }

        public ActionResult TimingPlan(int? page)
        {
            ViewBag.Title = "定时作业计划";
            return View(TimingPlanService.List().ToList());
        }

        [HttpPost]
        public ActionResult TimingPlan(FormCollection fc)
        {
            try
            {
                var entity = TimingPlanService.SingleAndInit(fc["theid"].ToInt());
                TryUpdateModel(entity, fc.AllKeys);
                if (entity.ID > 0)
                    TimingPlanService.Update(entity);
                else
                {
                    if (TimingPlanService.List(x => x.PlanName == entity.PlanName).Count() > 0)
                    {
                        ViewBag.ErrorMsg = "任务计划已被使用，请不要重复添加";
                        return View("Error");
                        //return Json(new { result = "err", refMsg = "任务计划已被使用，请不要重复添加" }, JsonRequestBehavior.AllowGet);
                    }
                    entity.Status = 1;
                    TimingPlanService.Add(entity);
                }
                SysDBTool.Commit();
                MakePlanXML();
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMsg = ex.Message;
                return View("Error");
                // return Json(new { result = "err", refMsg = ex.Message }, JsonRequestBehavior.AllowGet);
            }

            ViewBag.SuccessMsg = "操作成功！";
            return View("Success");
            // return Json(new { result = "ok" }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ModifyPlan(int id)
        {
            var model = TimingPlanService.Single(id);
            if (model != null)
                return Json(new { result = "ok", data = model }, JsonRequestBehavior.AllowGet);
            else
                return Json(new { result = "err", refMsg = "记录不存在或已被删除" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeletePlan(int id)
        {
            var model = TimingPlanService.Single(id);
            if (model != null)
            {
                TimingPlanService.Delete(id);
                ViewBag.SuccessMsg = "操作成功！";
                return View("Success");
            }
            ViewBag.ErrorMsg = "记录不存在或已被删除！";
            return View("Error");
        }

        public ActionResult SetPlanStatus(int id)
        {
            var model = TimingPlanService.Single(id);
            if (model != null)
            {
                if (model.Status == 1)
                    model.Status = 0;
                else
                    model.Status = 1;
                TimingPlanService.Update(model);
                SysDBTool.Commit();
                MakePlanXML();
                return RedirectToAction("TimingPlan", "Develop");

            }
            ViewBag.ErrorMsg = "记录不存在或已被删除！";
            return View("Error");
        }

        /// <summary>
        /// 生成语言XML
        /// </summary>
        /// <returns></returns>
        public void MakePlanXML()
        {
            var paramList = TimingPlanService.List(x => x.Status == 1).ToList();
            var Path = System.Web.HttpContext.Current.Server.MapPath("/TimingPlan/");
            //IOHelper.DeleteDirectory(Path);
            //IOHelper.CreateDirectory(Path);
            IOHelper.DeleteFile(Path + "/Plan.xml");
            XmlHelper.CreateXmlDocument(Path + "/Plan.xml", "plans", "utf-8");
            foreach (var param in paramList)
            {

                Hashtable ht = new Hashtable();
                ht.Add("name", param.PlanName);
                ht.Add("cycletype", param.CycleType);
                ht.Add("cyclevalue", param.CycleValue);
                ht.Add("time", param.PlanTime);
                XmlHelper.InsertNode(Path + "/Plan.xml", "plan", true, "plans", ht, null);
            }

        }

        public ActionResult Theme(int? page)
        {
            ViewBag.Title = "会员平台主题";
            ActMessage = ViewBag.Title;

            int pageIndex = page ?? 1;
            DataSet ds = XmlDataSet.GetDataSetByXml(Server.MapPath("/Theme/Theme.xml"));

            Webdiyer.WebControls.Mvc.PagedList<DataRow> pagedlist = new Webdiyer.WebControls.Mvc.PagedList<DataRow>(ds.Tables[0].Select(), 1, 15, ds.Tables[0].Rows.Count);
            return View(pagedlist);
        }

        //public ActionResult SetTheme(string themename)
        //{
        //    ConfigHelper.UpdatetConfig("Theme", themename);

        //    ViewBag.SuccessMsg = "设置新主题“" + themename + "”";
        //    ActMessage = ViewBag.SuccessMsg;
        //    return View("Success");
        //}

        //public ActionResult LoginStyle(int? page)
        //{
        //    ViewBag.Title = "会员登录页面样式";
        //    ActMessage = ViewBag.Title;

        //    int pageIndex = page ?? 1;

        //    DataSet ds = XmlDataSet.GetDataSetByXml(Server.MapPath("/LoginStyle/Login.xml"));
        //    PagedList<DataRow> pagedlist = new PagedList<DataRow>(ds.Tables[0].Select(), 1, 15, ds.Tables[0].Rows.Count);
        //    return View(pagedlist);
        //}

        //public ActionResult SetLoginStyle(string stylename)
        //{
        //    ConfigHelper.UpdatetConfig("LoginStyle", stylename);

        //    ViewBag.SuccessMsg = "设置新登录样式“" + stylename + "”";
        //    ActMessage = ViewBag.SuccessMsg;
        //    return View("Success");
        //}

        public ActionResult Setting()
        {
            ViewBag.Title = "系统设置";
            ActMessage = ViewBag.Title;

            return View();
        }

        [HttpPost]
        public ActionResult Setting(FormCollection form)
        {
            string isdevelopmode = form["isdevelopmode"];
            string ismultilanguage = form["ismultilanguage"];
            string memberatlas = form["memberatlas"];
            string dtbonus = form["dtbonus"];
            string dtsettlementway = form["dtsettlementway"];
            string staticbonus = form["staticbonus"];
            string unoperatetime = form["unoperatetime"];
            string startunoperatetime = form["startunoperatetime"];
            string userlevel = form["userlevel"];
            string isapplyagent = form["isapplyagent"];
            string agentlevel = form["agentlevel"];
            string issubaccount = form["issubaccount"];
            string adminpath = form["adminpath"];

            string pattern = @"^[a-zA-Z0-9]+$";
            if (!Regex.Match(adminpath, pattern).Success)
            {
                ViewBag.ErrorMsg = "后台路径只能是数字和字母组成。";
                return View("Error");
            }


            //ConfigHelper.UpdatetConfig("DevelopMode", TypeConverter.StrToBool(isdevelopmode, false).ToString());
            //ConfigHelper.UpdatetConfig("AdminPath", adminpath);
            //ConfigHelper.UpdatetConfig("MultiLanguage", TypeConverter.StrToBool(ismultilanguage, false).ToString());
            //ConfigHelper.UpdatetConfig("MemberAtlas", memberatlas);
            //ConfigHelper.UpdatetConfig("DtBonus", dtbonus);
            //ConfigHelper.UpdatetConfig("DtSettlementWay", dtsettlementway);
            //ConfigHelper.UpdatetConfig("StaticBonus", staticbonus);
            //ConfigHelper.UpdatetConfig("UnOperateTime", unoperatetime);
            //ConfigHelper.UpdatetConfig("StartUnOperateTime", startunoperatetime);
            //ConfigHelper.UpdatetConfig("UserLevel", TypeConverter.StrToInt(userlevel).ToString());
            //ConfigHelper.UpdatetConfig("ApplyAgent", TypeConverter.StrToBool(isapplyagent, false).ToString());
            //ConfigHelper.UpdatetConfig("AgentLevel", TypeConverter.StrToInt(agentlevel).ToString());
            //ConfigHelper.UpdatetConfig("SubAccount", TypeConverter.StrToBool(issubaccount, false).ToString());

            ActMessage = ViewBag.SuccessMsg;
            return View("Success");
        }

        //public ActionResult Theme()
        //{
        //    ViewBag.Title = "主题";
        //    ActMessage = ViewBag.Title;

        //    return View();
        //}

        public ActionResult Err()
        {
            ViewBag.Title = "主题";
            ActMessage = ViewBag.Title;

            return View();
        }
        public ActionResult Chart()
        {
            ViewBag.Title = "主题";
            ActMessage = ViewBag.Title;

            return View();
        }
    }
}

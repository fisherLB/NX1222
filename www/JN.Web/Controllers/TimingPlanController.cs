using JN.BLL;
using JN.Services.Tool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using Webdiyer.WebControls.Mvc;

namespace JN.Web.Controllers
{
    public class TimingPlanController : Controller
    {
        //
        // GET: /Admin/User/
        public ActionResult Index()
        {
            bool isExec = false;
            DateTime starttime = DateTime.Now;
            string ExecProcess = Request["ExecProcess"];
            switch (ExecProcess)
            {
                case "plan1":
                    isExec = timingplans.plan1();
                    break;
                case "plan2":
                    isExec = timingplans.plan2();
                    break;
                case "plan3":
                    isExec = timingplans.plan3();
                    break;
                case "plan4":
                    isExec = timingplans.plan4();
                    break;
                case "plan5":
                    isExec = timingplans.plan5();
                    break;
            }
            ViewBag.msg = (isExec ? "成功" : "失败") + "执行作业计划“" + ExecProcess + "”，时间在" + DateTime.Now.ToString() + "，用时：" + DateTimeDiff.DateDiff(starttime, DateTime.Now, "ms") + "毫秒";
            //logs.WritePlanLog(ViewBag.msg);
            return View();
        }
    }
}

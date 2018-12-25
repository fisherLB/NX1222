using JN.Data;
using JN.Data.Service;
using JN.Services.Manager;
using MvcCore.Controls;
using MvcCore.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using PagedList;
using JN.Services.Tool;

namespace JN.Web.Areas.AdminCenter.Controllers
{
    public class SettlementController : BaseController
    {
        private static List<Data.SysParam> cacheSysParam = null;
        private readonly ISettlementService SettlementService;
        private readonly ISupplyHelpService SupplyHelpService;
        private readonly IAcceptHelpService AcceptHelpService;
        private readonly IUserService UserService;
        private readonly ISysDBTool SysDBTool;
        private readonly IActLogService ActLogService;
        private readonly IWalletLogService WalletLogService;
        private readonly IBonusDetailService BonusDetailService;
      

        public SettlementController(ISysDBTool SysDBTool,
            ISettlementService SettlementService,
            ISupplyHelpService SupplyHelpService,
            IUserService UserService,
            IActLogService ActLogService,
            IWalletLogService WalletLogService,
            IBonusDetailService BonusDetailService
           )
        {
            this.SettlementService = SettlementService;
            this.SupplyHelpService = SupplyHelpService;
            this.UserService = UserService;
            this.SysDBTool = SysDBTool;
            this.ActLogService = ActLogService;
            this.WalletLogService = WalletLogService;
            this.BonusDetailService = BonusDetailService;
         
            cacheSysParam = MvcCore.Unity.Get<ISysParamService>().ListCache("sysparams", x => x.ID < 4000).ToList();
        }

        public ActionResult List(int? page)
        {
            ViewBag.Title = "历史分红列表";
            var list = SettlementService.List().WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).ToList();
            return View(list.ToPagedList(page ?? 1, 20));
        }

        public ActionResult ManualBalance()
        {
            ViewBag.Title = "手动分红";
            ActMessage = ViewBag.Title;
            DataCache.SetCache("TransInfo", "任务进度(未开始)");
            var model = SettlementService.List().ToList().FirstOrDefault();
            if (model != null)
                return View(model);
            else
                return View();
        }



        public ActionResult doFH(int? fhzl)
        {
            //线程里无法使用session
            DataCache.SetCache("StartTime", DateTime.Now);
            DataCache.SetCache("TotalRow", 0);
            DataCache.SetCache("CurrentRow", 0);
            DataCache.SetCache("TransInfo", "开始进行结算");
            //Thread thread = new Thread(new ParameterizedThreadStart(delegate { threadproc((fhzl ?? 0)); }));
            //thread.Start();
            MMM.CheckQueuing();
            Thread thread = new Thread(new ThreadStart(threadproc));
            thread.Start();


            return Content("ok");
        }

        public ActionResult DayBalance()
        {
            #region 事务操作
            //using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
            //{
            //        // users.DayBalance();
            //        ts.Complete();
            ViewBag.SuccessMsg = "进行手动分红";
            ActMessage = ViewBag.SuccessMsg;
            ActPacket = "";
            return View("Success");
            //}
            #endregion
        }


        private void threadproc()
        {

          MMM.CalculateFLX(UserService,
          WalletLogService,
          BonusDetailService,
          SettlementService,
          SupplyHelpService,
          SysDBTool,
        
          cacheSysParam,
          1);
        }

        public JsonResult getproc()
        {
            var proc = new
            {
                StartTime = DataCache.GetCache("StartTime"),
                CurrentRow = DataCache.GetCache("CurrentRow"),
                TotalRow = DataCache.GetCache("TotalRow"),
                TransInfo = DataCache.GetCache("TransInfo")
            };
            return Json(new { result ="ok", data = proc}, JsonRequestBehavior.AllowGet);
        }
    }
}

using JN.Data;
using JN.Data.Service;
using MvcCore.Controls;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using JN.Services.Filters;
using JN.Services.Manager;

namespace JN.Web.Areas.AdminCenter.Controllers
{
    public class FinanceController : BaseController 
    {
        private static List<Data.SysParam> cacheSysParam = null; 

        private readonly IUserService UserService;
        private readonly IBonusDetailService BonusDetailService;
        private readonly ISysDBTool SysDBTool;
        private readonly IActLogService ActLogService;
        private readonly IWalletLogService WalletLogService;
       
        public FinanceController(ISysDBTool SysDBTool, 
            IUserService UserService,
            IBonusDetailService BonusDetailService,
            IActLogService ActLogService, 
            IWalletLogService WalletLogService 
          )
        {
            this.UserService = UserService;
            this.BonusDetailService = BonusDetailService;
            this.SysDBTool = SysDBTool;
            this.ActLogService = ActLogService;
            this.WalletLogService = WalletLogService;
          
            cacheSysParam = MvcCore.Unity.Get<JN.Data.Service.ISysParamService>().ListCache("sysparams", x => x.ID < 4000).ToList();
        }

        public ActionResult Account(int? page)
        {
            //动态构建查询
            var list = UserService.List().WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).OrderByDescending(x => x.ID).ToList();
            return View(list.ToPagedList(page ?? 1, 20));
        }

        public ActionResult Delivery()
        {
            ActMessage = "赠送电子币";
            ViewData["SysParamList"] = new SelectList(cacheSysParam.Where(x => x.PID == 2000 && x.IsUse).OrderBy(x => x.ID).ToList(), "ID", "Name");
            return View();
        }

        [HttpPost]
        public ActionResult Delivery(FormCollection form)
        {
            string username = form["username"];
            string coinid = form["coinid"];
            string deliverynumber = form["deliverynumber"];
            string password2 = form["password2"];
            string remark = form["remark"];
            var result = new ReturnResult();
            try
            {
                if (Amodel.Password2 != password2.ToMD5().ToMD5()) throw new Exception("二级密码不正确");

                var onUser = UserService.Single(x => x.UserName.Equals(username.Trim()));
                if (onUser == null) throw new Exception("用户不存在");
                if (remark.Trim().Length > 100) throw new Exception("备注长度不能超过100个字节");
                using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
                {
                    Wallets.changeWallet(onUser.ID, deliverynumber.ToDecimal(), coinid.ToInt(), "ZH：" + remark);
                    result.Status = 200;
                    ts.Complete();
                }
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
            return Json(result);
       
        }

        public ActionResult DeliveryDetail(int? coin, int? page)
        {
            int coinID = coin ?? 2002;
            ViewBag.ParamList = cacheSysParam.Where(x => x.PID == 2000 && x.IsUse == true).OrderBy(x => x.Sort).ToList();
            var list = WalletLogService.List(x => x.CoinID == coinID && x.Description.Contains("ZH：")).WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).OrderByDescending(x => x.ID).ToList();
            if (Request["IsExport"] == "1")
            {
                string FileName = string.Format("{0}_{1}_{2}_{3}", DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute);
                MvcCore.Extensions.ExcelHelperV2.ToExcel(list.ToList()).SaveToExcel(Server.MapPath("/upfile/" + FileName + ".xls"));
                return File(Server.MapPath("/upfile/" + FileName + ".xls"), "application/ms-excel", FileName + ".xls");
            }
            return View(list.ToPagedList(page ?? 1, 20));
        }

        public ActionResult Statistics(int? page)
        {
            var parameters = new[]{
                new System.Data.SqlClient.SqlParameter(){ ParameterName="1", Value=1 }
            };
            var list = SysDBTool.Execute<Data.Extensions.View_Statistics>("SELECT [jstime],[newuser],[takecash],[income],[outlay],[profit] FROM [View_Statistics]", parameters);
            return View(list.ToPagedList(page ?? 1, 20));
        }

        public ActionResult AccountDetail(int? coin, int? page)
        {
            int coinID = coin ?? 2002;
            ViewBag.ParamList = cacheSysParam.Where(x => x.PID == 2000 && x.IsUse == true).OrderBy(x => x.Sort).ToList();
            var list = WalletLogService.List(x => x.CoinID == coinID).OrderByDescending(x => x.ID).WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).ToList();
            if (Request["IsExport"] == "1")
            {
                string FileName = string.Format("{0}_{1}_{2}_{3}", DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute);
                MvcCore.Extensions.ExcelHelperV2.ToExcel(list.ToList()).SaveToExcel(Server.MapPath("/upfile/" + FileName + ".xls"));
                return File(Server.MapPath("/upfile/" + FileName + ".xls"), "application/ms-excel", FileName + ".xls");
            }
            return View(list.ToPagedList(page ?? 1, 20));
        }

        public ActionResult BonusDetail(int? bonusid, int? page)
        {
            ViewBag.ParamList = cacheSysParam.Where(x => x.PID == 1100 && x.IsUse == true).OrderBy(x => x.Sort).ToList();
            var list = BonusDetailService.List().WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).OrderByDescending(x => x.ID).ToList();
            if (bonusid.HasValue)
                list = list.Where(x => x.BonusID == (bonusid ?? 0)).ToList();

            if (Request["IsExport"] == "1")
            {
                string FileName = string.Format("{0}_{1}_{2}_{3}", DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute);
                MvcCore.Extensions.ExcelHelperV2.ToExcel(list.ToList()).SaveToExcel(Server.MapPath("/upfile/" + FileName + ".xls"));
                return File(Server.MapPath("/upfile/" + FileName + ".xls"), "application/ms-excel", FileName + ".xls");
            }
            return View(list.ToPagedList(page ?? 1, 20));
        }
    }
}
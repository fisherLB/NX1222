using JN.Data.Service;
using MvcCore.Controls;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Webdiyer.WebControls.Mvc;
using JN.Services.Filters;
using JN.Services.Manager;
using JN.Web.Areas.UserCenter.Models;
using JN.Web.Areas.AdminCenter.Models;
using System.Data.Entity.SqlServer;

namespace JN.Web.Areas.AdminCenter.Controllers
{
    public class MarketController : BaseController
    {
        private static List<Data.SysParam> cacheSysParam = null;

        private readonly IUserService UserService;
        private readonly ISupplyHelpService SupplyHelpService;
        private readonly IAcceptHelpService AcceptHelpService;
        private readonly IMatchingService MatchingService;
        private readonly ISysSettingService SysSettingService;
        private readonly IBonusDetailService BonusDetailService;
        private readonly ISysDBTool SysDBTool;
        private readonly IActLogService ActLogService;
       

        public MarketController(ISysDBTool SysDBTool,
            IUserService UserService,
            ISupplyHelpService SupplyHelpService,
            IAcceptHelpService AcceptHelpService,
            IMatchingService MatchingService,
            ISysSettingService SysSettingService,
            IBonusDetailService BonusDetailService,
            IActLogService ActLogService
            )
        {
            this.UserService = UserService;
            this.SupplyHelpService = SupplyHelpService;
            this.AcceptHelpService = AcceptHelpService;
            this.MatchingService = MatchingService;
            this.SysSettingService = SysSettingService;
            this.BonusDetailService = BonusDetailService;
            this.SysDBTool = SysDBTool;
            this.ActLogService = ActLogService;
           
            cacheSysParam = MvcCore.Unity.Get<JN.Data.Service.ISysParamService>().ListCache("sysparams", x => x.ID < 4000).ToList();
        }
        /// <summary>
        /// 已匹配数据
        /// </summary>
        public ActionResult MatchdList(int? page)
        {
            string status = Request["st"];
            string ac = Request["ac"];
         
            ActMessage = "已匹配数据";
            //隐藏的不显示
            var list = MatchingService.List().WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).Where(x => x.IsHide!=true).OrderByDescending(x => x.ID).ToList();
            if (!String.IsNullOrEmpty(status))
                list = list.Where(x => x.Status == status.ToInt()).ToList();
            if (!String.IsNullOrEmpty(ac))
            {
                if (ac == "overduepayment")
                    list = list.Where(x => x.Status == (int)JN.Data.Enum.MatchingStatus.UnPaid).Where(x =>x.PayEndTime< DateTime.Now).ToList();
                if (ac == "overdueverified")
                    list = list.Where(x => x.Status == (int)JN.Data.Enum.MatchingStatus.UnPaid).Where(x => x.VerifiedEndTime<DateTime.Now).ToList();

            }

            if (Request.IsAjaxRequest())
                return PartialView("_MatchdList", list.ToPagedList(page ?? 1, 20));

            return View(list.ToPagedList(page ?? 1, 20));
        }



        /// <summary>
        /// 匹配管理  20%单
        /// </summary>
        public ActionResult PPList(int SupplyHelpPage = 1, int AcceptHelpPage = 1)
        {
            const int pageSize = 10;
            #region 查询
            var SupplyHelps = Request.QueryString["SupplyHelps"] == null ? "" : Request.QueryString["SupplyHelps"];
            var AcceptHelps = Request.QueryString["AcceptHelps"] == null ? "" : Request.QueryString["AcceptHelps"];
            #endregion
            int dt = cacheSysParam.SingleAndInit(a => a.ID == 3101).Value.ToInt();
            int dt2 = cacheSysParam.SingleAndInit(a => a.ID == 3102).Value.ToInt();
            #region 多组异步
            if (Request.IsAjaxRequest())
            {
                var target = Request.QueryString["target"];
                if (target == "SupplyHelp")
                {
                    return PartialView("_SupPPList",
                        MvcCore.Unity.Get<JN.Data.Service.ISupplyHelpService>().List(x => (x.OrderType == 1 && x.Status > 0 && x.HaveMatchingAmount < x.ExchangeAmount && x.Status < (int)JN.Data.Enum.HelpStatus.AllDeal && x.ReserveInt2==1 && System.Data.Entity.SqlServer.SqlFunctions.DateDiff("minute", x.CreateTime, DateTime.Now) >= dt) && (x.SupplyNo.Contains(SupplyHelps) || x.CreateTime.ToString().Contains(SupplyHelps))).OrderByDescending(x => x.ID).OrderByDescending(x => x.IsTop).ThenByDescending(x => x.IsRepeatQueuing).ThenBy(x => x.CreateTime).ToList().ToPagedList(SupplyHelpPage, pageSize));
                }

                if (target == "AcceptHelp")
                {
                    return PartialView("_AccPPList",
                        MvcCore.Unity.Get<JN.Data.Service.IAcceptHelpService>().List(x => (x.Status > 0 && x.HaveMatchingAmount < x.ExchangeAmount && x.Status < (int)JN.Data.Enum.HelpStatus.AllDeal && System.Data.Entity.SqlServer.SqlFunctions.DateDiff("minute", x.CreateTime, DateTime.Now) >= dt) && (x.AcceptNo.Contains(AcceptHelps) || x.CreateTime.ToString().Contains(AcceptHelps))).OrderByDescending(x => x.ID).OrderByDescending(x => x.IsTop).ThenByDescending(x => x.IsRepeatQueuing).ThenBy(x => x.CreateTime).ToList().ToPagedList(AcceptHelpPage, pageSize));
                }


            }
            #endregion

            #region 存入模型
            var model = new CompositeppList
            {
                SupplyHelp = MvcCore.Unity.Get<JN.Data.Service.ISupplyHelpService>().List(x => (x.OrderType == 1 && x.Status > 0 && x.HaveMatchingAmount < x.ExchangeAmount && x.Status < (int)JN.Data.Enum.HelpStatus.AllDeal && x.ReserveInt2==1 && System.Data.Entity.SqlServer.SqlFunctions.DateDiff("minute", x.CreateTime, DateTime.Now) >= dt) && (x.SupplyNo.Contains(SupplyHelps) || x.CreateTime.ToString().Contains(SupplyHelps))).OrderByDescending(x => x.ID).OrderByDescending(x => x.IsTop).ThenByDescending(x => x.IsRepeatQueuing).ThenBy(x => x.CreateTime).ToList().ToPagedList(SupplyHelpPage, pageSize),
                AcceptHelp = MvcCore.Unity.Get<JN.Data.Service.IAcceptHelpService>().List(x => (x.Status > 0 && x.HaveMatchingAmount < x.ExchangeAmount && x.Status < (int)JN.Data.Enum.HelpStatus.AllDeal && System.Data.Entity.SqlServer.SqlFunctions.DateDiff("minute", x.CreateTime, DateTime.Now) >= dt2) && (x.AcceptNo.Contains(AcceptHelps) || x.CreateTime.ToString().Contains(AcceptHelps))).OrderByDescending(x => x.ID).OrderByDescending(x => x.IsTop).ThenByDescending(x => x.IsRepeatQueuing).ThenBy(x => x.CreateTime).ToList().ToPagedList(AcceptHelpPage, pageSize)
            };
            #endregion
            ActMessage = "预付单匹配管理";
            return View(model);
        }

        //找出80%单
        public ActionResult PPList2(int SupplyHelpPage = 1, int AcceptHelpPage = 1)
        {
            const int pageSize = 10;
            #region 查询
            var SupplyHelps = Request.QueryString["SupplyHelps"] == null ? "" : Request.QueryString["SupplyHelps"];
            var AcceptHelps = Request.QueryString["AcceptHelps"] == null ? "" : Request.QueryString["AcceptHelps"];
            #endregion
            int dt = cacheSysParam.SingleAndInit(a => a.ID == 3101).Value.ToInt();
            int dt2 = cacheSysParam.SingleAndInit(a => a.ID == 3102).Value.ToInt();

            List<Data.SupplyHelp> slist = MvcCore.Unity.Get<JN.Data.Service.ISupplyHelpService>().List(x => (x.OrderType == 1 && x.Status >= (int)JN.Data.Enum.HelpStatus.AllMatching && x.ReserveInt2 == 1 && x.HaveMatchingAmount == x.ExchangeAmount)).AsQueryable<Data.SupplyHelp>().ToList();
            //既不是20%也不是80%，抢单、订单转移
            var otherlist =MvcCore.Unity.Get<JN.Data.Service.ISupplyHelpService>().List(x=> (x.OrderType == 1 && x.Status > 0 && x.HaveMatchingAmount < x.ExchangeAmount && x.Status < (int)JN.Data.Enum.HelpStatus.AllDeal && x.ReserveInt2!=1 && x.ReserveInt2!=2 && System.Data.Entity.SqlServer.SqlFunctions.DateDiff("minute", x.CreateTime, DateTime.Now) >= dt));
            List<Data.SupplyHelp> lst=new List<Data.SupplyHelp>();

            foreach (var item in slist)
            {
                var itm = MvcCore.Unity.Get<JN.Data.Service.ISupplyHelpService>().Single(x => x.OrderType == 1 && x.Status > 0 && x.HaveMatchingAmount < x.ExchangeAmount && x.Status < (int)JN.Data.Enum.HelpStatus.AllDeal && System.Data.Entity.SqlServer.SqlFunctions.DateDiff("minute", x.CreateTime, DateTime.Now) >= dt && x.ReserveInt2 == 2 && x.ReserveStr2 == item.ReserveStr2);
                if (itm != null)
                {
                    lst.Add(itm);
                    MvcCore.Unity.Get<JN.Data.Service.ISysDBTool>().Commit();
                }
                
            }
            foreach (var tm in otherlist)
            {
                lst.Add(tm);
                MvcCore.Unity.Get<JN.Data.Service.ISysDBTool>().Commit();
            }
            #region 多组异步
            if (Request.IsAjaxRequest())
            {
                var target = Request.QueryString["target"];
                if (target == "SupplyHelp")
                {

                    //return PartialView("_SupPPList",
                    //    MvcCore.Unity.Get<JN.Data.Service.ISupplyHelpService>().List(x => (x.OrderType == 1 && x.Status > 0 && x.HaveMatchingAmount < x.ExchangeAmount && x.Status < (int)JN.Data.Enum.HelpStatus.AllDeal && x.ReserveInt2 != 1 && System.Data.Entity.SqlServer.SqlFunctions.DateDiff("minute", x.CreateTime, DateTime.Now) >= dt) && (x.SupplyNo.Contains(SupplyHelps) || x.CreateTime.ToString().Contains(SupplyHelps))).OrderByDescending(x => x.ID).OrderByDescending(x => x.IsTop).ThenByDescending(x => x.IsRepeatQueuing).ThenBy(x => x.CreateTime).ToList().ToPagedList(SupplyHelpPage, pageSize));

                    return PartialView("_SupPPList",
                       lst.Where(x=>x.ID>0 && (x.SupplyNo.Contains(SupplyHelps) || x.CreateTime.ToString().Contains(SupplyHelps))).OrderByDescending(x => x.ID).OrderByDescending(x => x.IsTop).ThenByDescending(x => x.IsRepeatQueuing).ThenBy(x => x.CreateTime).ToList().ToPagedList(SupplyHelpPage, pageSize));
                }

                if (target == "AcceptHelp")
                {
                    return PartialView("_AccPPList",
                        MvcCore.Unity.Get<JN.Data.Service.IAcceptHelpService>().List(x => (x.Status > 0 && x.HaveMatchingAmount < x.ExchangeAmount && x.Status < (int)JN.Data.Enum.HelpStatus.AllDeal && System.Data.Entity.SqlServer.SqlFunctions.DateDiff("minute", x.CreateTime, DateTime.Now) >= dt) && (x.AcceptNo.Contains(AcceptHelps) || x.CreateTime.ToString().Contains(AcceptHelps))).OrderByDescending(x => x.ID).OrderByDescending(x => x.IsTop).ThenByDescending(x => x.IsRepeatQueuing).ThenBy(x => x.CreateTime).ToList().ToPagedList(AcceptHelpPage, pageSize));
                }

               
            }
            #endregion

            #region 存入模型
            var model = new CompositeppList
            {
                //SupplyHelp = MvcCore.Unity.Get<JN.Data.Service.ISupplyHelpService>().List(x => (x.OrderType == 1 && x.Status > 0 && x.HaveMatchingAmount < x.ExchangeAmount && x.Status < (int)JN.Data.Enum.HelpStatus.AllDeal && x.ReserveInt2!=1 && System.Data.Entity.SqlServer.SqlFunctions.DateDiff("minute", x.CreateTime, DateTime.Now) >= dt) && (x.SupplyNo.Contains(SupplyHelps) || x.CreateTime.ToString().Contains(SupplyHelps))).OrderByDescending(x => x.ID).OrderByDescending(x => x.IsTop).ThenByDescending(x => x.IsRepeatQueuing).ThenBy(x => x.CreateTime).ToList().ToPagedList(SupplyHelpPage, pageSize),
                SupplyHelp=  lst.Where(x=>x.ID>0 && (x.SupplyNo.Contains(SupplyHelps) || x.CreateTime.ToString().Contains(SupplyHelps))).OrderByDescending(x => x.ID).OrderByDescending(x => x.IsTop).ThenByDescending(x => x.IsRepeatQueuing).ThenBy(x => x.CreateTime).ToList().ToPagedList(SupplyHelpPage, pageSize),
                AcceptHelp = MvcCore.Unity.Get<JN.Data.Service.IAcceptHelpService>().List(x => (x.Status > 0 && x.HaveMatchingAmount < x.ExchangeAmount && x.Status < (int)JN.Data.Enum.HelpStatus.AllDeal && System.Data.Entity.SqlServer.SqlFunctions.DateDiff("minute", x.CreateTime, DateTime.Now) >= dt2) && (x.AcceptNo.Contains(AcceptHelps) || x.CreateTime.ToString().Contains(AcceptHelps))).OrderByDescending(x => x.ID).OrderByDescending(x => x.IsTop).ThenByDescending(x => x.IsRepeatQueuing).ThenBy(x => x.CreateTime).ToList().ToPagedList(AcceptHelpPage, pageSize)
            };
            #endregion
            ActMessage = "全额匹配管理";
            return View(model);
        }

        /// <summary>
        /// 提供帮助列表
        /// </summary>
        public ActionResult SupplyHelp(int? page)
        {
            var list = SupplyHelpService.List().WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).OrderByDescending(x => x.ID).ToList();

            if (Request["IsExport"] == "1")
            {
                string FileName = string.Format("{0}_{1}_{2}_{3}", DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute);
                MvcCore.Extensions.ExcelHelperV2.ToExcel(list.ToList()).SaveToExcel(Server.MapPath("/upfile/" + FileName + ".xls"));
                return File(Server.MapPath("/upfile/" + FileName + ".xls"), "application/ms-excel", FileName + ".xls");
            }
            if (Request.IsAjaxRequest())
                return View("_SupplyHelp", list.ToPagedList(page ?? 1, 10));

            return View(list.ToPagedList(page ?? 1, 10));
        }


        /// <summary>
        /// 接受帮助列表
        /// </summary>
        public ActionResult AcceptHelp(int? page)
        {
            var list = AcceptHelpService.List().WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).OrderByDescending(x => x.ID).ToList();
            if (Request["IsExport"] == "1")
            {
                string FileName = string.Format("{0}_{1}_{2}_{3}", DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute);
                MvcCore.Extensions.ExcelHelperV2.ToExcel(list.ToList()).SaveToExcel(Server.MapPath("/upfile/" + FileName + ".xls"));
                return File(Server.MapPath("/upfile/" + FileName + ".xls"), "application/ms-excel", FileName + ".xls");
            }
            if (Request.IsAjaxRequest())
                return View("_AcceptHelp", list.ToPagedList(page ?? 1, 10));

            return View(list.ToPagedList(page ?? 1, 10));
        }

        /// <summary>
        /// 点击匹配按钮时的事件
        /// </summary>
        public ActionResult doMatching(string ids, string ida)
        {
            if (string.IsNullOrEmpty(ids) || string.IsNullOrEmpty(ida))
                return Json(new { result = "erro", refMsg = "请选择匹配供单及受单！" }, JsonRequestBehavior.AllowGet);

            #region 事务操作
            using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
            {
                string outMsg = "";
                MMM.Matching(ids, ida, ref outMsg); //匹配处理
                ts.Complete();
                return Json(new { result = "ok", refMsg = outMsg }, JsonRequestBehavior.AllowGet);
            }
            #endregion
        }


        /// <summary>
        /// 点击惩罚按钮时的事件
        /// </summary>
        public ActionResult Punish(int matchid)
        {
            var mModel = MatchingService.Single(matchid);
            if (mModel == null)
            {
                ViewBag.ErrorMsg = "记录不存在。";
                return View("Error");
            }

            #region 事务操作
            using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
            {
                MMM.Punish(mModel.ID); //虚假信息处理

                ts.Complete();
                ViewBag.SuccessMsg = "操作成功！";
                return View("Success");
            }
            #endregion
        }

        /// <summary>
        /// 点击取消按钮时的事件
        /// </summary>
        public ActionResult Cancel(int matchid)
        {
            var mModel = MatchingService.Single(matchid);
            if (mModel == null)
            {
                ViewBag.ErrorMsg = "记录不存在。";
                return View("Error");
            }

            if (mModel.Status == (int)Data.Enum.MatchingStatus.UnPaid)
            {
                #region 事务操作
                using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
                {
                    MMM.CancelMatching(mModel, "管理员取消", false);
                    ts.Complete();
                    ViewBag.SuccessMsg = "操作成功！";
                    return View("Success");
                }
                #endregion
            }
            else
            {
                ViewBag.ErrorMsg = "订单当前状态不可取消。";
                return View("Error");
            }
        }


        /// <summary>
        /// 供单置顶
        /// </summary>
        /// <returns></returns>
        public ActionResult SupplyHelpCommand(int id, string commandtype)
        {
            var model = SupplyHelpService.Single(id);
            if (commandtype.ToLower() == "ontop")
                model.IsTop = true;
            else if (commandtype.ToLower() == "untop")
                model.IsTop = false;
            SupplyHelpService.Update(model);
            SysDBTool.Commit();
            ViewBag.SuccessMsg = "操作成功！";
            return View("Success");
        }

        /// <summary>
        /// 受单置顶
        /// </summary>
        /// <returns></returns>
        public ActionResult AcceptHelpCommand(int id, string commandtype)
        {
            var model = AcceptHelpService.Single(id);
            if (commandtype.ToLower() == "ontop")
                model.IsTop = true;
            else if (commandtype.ToLower() == "untop")
                model.IsTop = false;
            AcceptHelpService.Update(model);
            SysDBTool.Commit();
            ViewBag.SuccessMsg = "操作成功！";
            return View("Success");
        }

        /// <summary>
        /// 追加付款截止时间
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        //public ActionResult DelayPay(int matchid, int hour)
        //{
        //    var mModel = MatchingService.Single(matchid);
        //    if (mModel != null)
        //    {
        //        mModel.PayEndTime = (mModel.PayEndTime ?? DateTime.Now).AddHours(hour);
        //        MatchingService.Update(mModel);
        //        SysDBTool.Commit();
        //        return Content("ok");
        //    }
        //    return Content("Error");
        //}

        #region 确认收款
        /// <summary>
        /// 确认收款
        /// </summary>
        /// <returns></returns>
        public ActionResult FinshPay(int matchid)
        {
            var mModel = MatchingService.Single(matchid);
            if (mModel == null)
            {
                ViewBag.ErrorMsg = "记录不存在。";
                return View("Error");
            }

            if (mModel.Status == (int)Data.Enum.MatchingStatus.Paid || mModel.Status == (int)Data.Enum.MatchingStatus.Falsehood)
            {
                using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
                {
                    //结算提供单利息，奖金并更新成交状态
                    Bonus.Settlement(mModel);
                    mModel.Status = (int)Data.Enum.MatchingStatus.Verified;

                    MatchingService.Update(mModel);
                    SysDBTool.Commit();
                    ts.Complete();
                    ViewBag.SuccessMsg = "操作成功！";
                    return View("Success");
                }
            }
            else
            {
                ViewBag.ErrorMsg = "订单状态不可确认。";
                return View("Error");
            }
        }
        #endregion

        #region 取消退出队列

        /// <summary>
        /// 退出队列（供单)
        /// </summary>
        /// <returns></returns>
        public ActionResult CancelSupplyQueuing(int id)
        {
            var sModel = SupplyHelpService.Single(id);
            if (sModel != null)
            {
                if (sModel.Status == (int)Data.Enum.HelpStatus.NoMatching)
                {
                    var list = SupplyHelpService.List(x => x.ReserveStr2 == sModel.ReserveStr2);  //找出同一组提供单（20% 和80%分单）
                    using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
                    {
                        foreach(var item in list)
                        { 
                        
                        //删除利息,奖金
                            BonusDetailService.Delete(x => x.SupplyNo == item.SupplyNo && x.IsBalance == false);
                            SysDBTool.Commit();

                            item.Status = (int)Data.Enum.HelpStatus.Cancel;
                            item.IsAccruaCount = false;
                            item.CancelTime = DateTime.Now;
                            SupplyHelpService.Update(item);
                            SysDBTool.Commit();
                        }
                        ts.Complete();
                    }
                    ViewBag.SuccessMsg = "操作成功！";
                    return View("Success");
                }
                else
                {
                    ViewBag.ErrorMsg = "当前提供订单状态不可退出。";
                    return View("Error");
                }
            }
            else
            {
                ViewBag.ErrorMsg = "记录不存在。";
                return View("Error");
            }
        }

         /// <summary>
        /// 退出队列（供单)
        /// </summary>
        /// <returns></returns>
        public ActionResult CancelAcceptQueuing(int id)
        {
            var aModel = AcceptHelpService.Single(id);
            if (aModel != null)
            {
                if (aModel.Status == (int)Data.Enum.HelpStatus.NoMatching)
                {
                    using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
                    {
                        Wallets.changeWallet(aModel.UID, aModel.ExchangeAmount, aModel.CoinID, "取消接受帮助“" + aModel.AcceptNo + "”订单返还");
                        aModel.Status = (int)Data.Enum.HelpStatus.Cancel;
                        aModel.CancelTime = DateTime.Now;
                        AcceptHelpService.Update(aModel);
                        SysDBTool.Commit();
                        ts.Complete();
                    }
                    ViewBag.SuccessMsg = "操作成功！";
                    return View("Success");
                }
                else
                {
                    ViewBag.ErrorMsg = "当前提供订单状态不可退出。";
                    return View("Error");
                }
            }
            else
            {
                ViewBag.ErrorMsg = "记录不存在。";
                return View("Error");
            }
        }
        #endregion

        #region 恢复排队（只有在被系统自动取消或虚假信息取消时才可恢复）

        /// <summary>
        /// 退出队列（供单)
        /// </summary>
        /// <returns></returns>
        public ActionResult RecoverySupplyQueuing(int id)
        {
            var sModel = SupplyHelpService.Single(id);
            if (sModel != null)
            {
                if (sModel.Status == (int)Data.Enum.HelpStatus.Cancel && sModel.HaveMatchingAmount < sModel.ExchangeAmount)
                {
                    using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
                    {
                        //重新修正状态
                        if (sModel.HaveMatchingAmount == 0)
                            sModel.Status = (int)Data.Enum.HelpStatus.NoMatching;
                        else if (sModel.HaveMatchingAmount > 0 && sModel.HaveMatchingAmount < sModel.ExchangeAmount)
                            sModel.Status = (int)Data.Enum.HelpStatus.PartOfMatching;
                        SupplyHelpService.Update(sModel);
                        SysDBTool.Commit();
                        ts.Complete();
                    }
                    ViewBag.SuccessMsg = "操作成功！";
                    return View("Success");
                }
                else
                {
                    ViewBag.ErrorMsg = "当前提供订单状态不可恢复。";
                    return View("Error");
                }
            }
            else
            {
                ViewBag.ErrorMsg = "记录不存在。";
                return View("Error");
            }
        }
        #endregion

        public ActionResult ChangeMathingMode()
        {
            var sysEntity = SysSettingService.Single(1);
            sysEntity.MatchingMode = sysEntity.MatchingMode == 0 ? 1 : 0;
            SysSettingService.Update(sysEntity);
            SysDBTool.Commit();
            ViewBag.SuccessMsg = "操作成功！";
            return View("Success");
        }

        public ActionResult Hide(int id)
        {
          
            var model = MatchingService.Single(id);
            if (model != null)
            {
                if (model.IsHide!=true)
                {
                    model.IsHide = true;
                    MatchingService.Update(model);
                    SysDBTool.Commit();
                    ViewBag.SuccessMsg = "隐藏成功！";
                    ActMessage = "隐藏成功";
                    return View("Success");
                }
                else
                {
                    ViewBag.ErrorMsg = "该数据已隐藏。";
                    return View("Error");
                }
            }
            ViewBag.ErrorMsg = "数据不存在或已被删除。";
            return View("Error");
        }
    }
}

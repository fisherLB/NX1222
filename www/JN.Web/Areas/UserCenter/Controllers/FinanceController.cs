using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using JN.Data;
using JN.Data.Service;
using MvcCore.Controls;
using Webdiyer.WebControls.Mvc;
using JN.Services.Manager;
using System.Data.Entity.SqlServer;
using JN.Web.Areas.UserCenter.Models;

namespace JN.Web.Areas.UserCenter.Controllers
{
    public class FinanceController : BaseController
    {
        private static List<Data.SysParam> cacheSysParam = null; 

        private readonly IUserService UserService;
        private readonly IBonusDetailService BonusDetailService;
        private readonly ISysDBTool SysDBTool;
        private readonly IActLogService ActLogService;
        private readonly IWalletLogService WalletLogService;
      
        private readonly IAcceptHelpService AcceptHelpService;
        private readonly ISupplyHelpService SupplyHelpService;

        public FinanceController(ISysDBTool SysDBTool,
            IUserService UserService,
            IBonusDetailService BonusDetailService,
            IActLogService ActLogService,
            IWalletLogService WalletLogService,
          
            IAcceptHelpService AcceptHelpService,
             ISupplyHelpService SupplyHelpService )
        {
            this.UserService = UserService;
            this.BonusDetailService = BonusDetailService;
            this.SysDBTool = SysDBTool;
            this.ActLogService = ActLogService;
            this.WalletLogService = WalletLogService;
           
            this.AcceptHelpService=AcceptHelpService;
            this.SupplyHelpService = SupplyHelpService;
            cacheSysParam = MvcCore.Unity.Get<ISysParamService>().ListCache("sysparams", x => x.ID < 4000).ToList();

        }

        public ActionResult PINCode()
        {
            ActMessage = "激活码";
            return View();
        }

        [HttpPost]
        public ActionResult BuyPINCode(FormCollection form)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                string buyNum = form["buyNum"];
                if (Umodel.IsLock) throw new Exception("您的帐号受限，无法进行相关操作");
                if (buyNum.ToInt() <= 0) throw new Exception("请填写正确的数量");
                decimal totalmoney = buyNum.ToInt() * cacheSysParam.SingleAndInit(x => x.ID == 1001).Value.ToDecimal();
                if (Umodel.Wallet2002 < totalmoney) throw new Exception("您的帐户余额不足");
                for (int i = 0; i < buyNum.ToInt(); i++)
                {
                    //生成PIN码
                    string keycode = GetPINNumber(15);
                    var model = new Data.PINCode
                    {
                        CreateTime = DateTime.Now,
                        OriginDescribe = "用户制作",
                        Investment = 0,
                        IsUsed = false,
                        KeyCode = keycode,
                        UID = Umodel.ID,
                        UserName = Umodel.UserName,
                        OriginUID = Umodel.ID,
                        OriginUserName = Umodel.UserName
                    };
                    MvcCore.Unity.Get<IPINCodeService>().Add(model);
                    SysDBTool.Commit();
                    Wallets.changeWallet(Umodel.ID, 0 - totalmoney, 2002, "制作PIN码");
                }
                result.Status = 200;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
            return Json(result);
        }

        //生成真实订单号
        public string GetPINNumber(int num)
        {
            DateTime dateTime = DateTime.Now;
            string result = JN.Services.Tool.StringHelp.GettRandomCode(num);//4位随机数字
            if (IsHave(result))
            {
                return GetPINNumber(num);
            }
            return result;
        }

        //检查订单号是否重复
        public bool IsHave(string number)
        {
            return MvcCore.Unity.Get<IPINCodeService>().List(x => x.KeyCode == number).Count() > 0;
        }

        [HttpPost]
        public ActionResult TransferPinCode(FormCollection form)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                string transferUser = form["transferUser"];
                string transferNum = form["transferNum"];
                var shUser = UserService.Single(x => x.UserName == transferUser.Trim());
                if (Umodel.IsLock) throw new Exception("您的帐号受限，无法进行相关操作");
                if (shUser == null) throw new Exception("用户不存在");
                if (transferNum.ToInt() <= 0) throw new Exception("请填写正确的转让数量");

                if (Umodel.RefereePath.Contains("," + shUser.ID + ",") || shUser.RefereePath.Contains("," + Umodel.ID + ",") || shUser.RefereeID == Umodel.ID || Umodel.RefereeID == shUser.ID)
                {

                    var codes = MvcCore.Unity.Get<IPINCodeService>().List(x => x.UID == Umodel.ID && x.IsUsed == false).OrderBy(x => x.ID).ToList();
                    if (codes.Count < transferNum.ToInt()) throw new Exception("数量不足");
                    for (int i = 0; i < transferNum.ToInt(); i++)
                    {
                        var model = codes[i];
                        model.UID = shUser.ID;
                        model.UserName = shUser.UserName;
                        MvcCore.Unity.Get<IPINCodeService>().Update(model);
                        SysDBTool.Commit();
                    }
                    result.Status = 200;
                }
                else
                    throw new Exception("只有推荐关系的会员才可以转让");
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
            return Json(result);
        }

        public ActionResult SupplyList(int? page)
        {
            ActMessage = "提供帮助列表";
            var list = SupplyHelpService.List(x => x.UID == Umodel.ID).OrderByDescending(x => x.ID).WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).ToList().ToPagedList(page ?? 1, 20);
            if (Request.IsAjaxRequest())
                return PartialView("_Supply", list);
            return View(list);
        }

        public ActionResult AcceptList(int? page)
        {
            ActMessage = "接受帮助列表";
            var list = AcceptHelpService.List(x => x.UID == Umodel.ID).OrderByDescending(x => x.ID).WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).ToList().ToPagedList(page ?? 1, 20);
            if (Request.IsAjaxRequest())
                return PartialView("_Accept", list);
            return View(list);
        }

        public ActionResult GrabSingle()
        {
            ActMessage = "抢单专区";
            return View();
        }

        [HttpPost]
        public ActionResult GrabSingle(FormCollection form)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                string id = form["mid"];
                var mModel = MvcCore.Unity.Get<IMatchingService>().Single(id.ToInt());
                if (mModel.SupplyUID == Umodel.ID || mModel.AcceptUID == Umodel.ID) throw new Exception("不能抢自己的关联单！");
                if (mModel.Status != (int)JN.Data.Enum.MatchingStatus.UnPaid) throw new Exception("订单状态已发生改变,请尝试其它的订单");
                using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
                {
                    //删除提供单奖金
                    MvcCore.Unity.Get<Data.Service.BonusDetailService>().Delete(x => x.SupplyNo == mModel.SupplyNo);


                    //同时生成一个提供单才可计算利息
                    var model = new Data.SupplyHelp();
                    model.UID = Umodel.ID;
                    model.UserName = Umodel.UserName;
                    model.SupplyAmount = mModel.MatchAmount; //申请金额
                    model.ExchangeAmount = mModel.MatchAmount; //汇率金额
                    model.CreateTime = DateTime.Now;
                    model.Status = (int)Data.Enum.HelpStatus.AllMatching;  //状态
                    model.IsTop = false;  //是否置顶
                    model.IsRepeatQueuing = false; //是否重新排队
                    model.HaveMatchingAmount = mModel.MatchAmount; //已匹配数量
                    model.HaveAcceptAmount = 0; //
                    model.PayWay = "";  //付款方式
                    model.EndTime = DateTime.Now.AddMinutes(cacheSysParam.SingleAndInit(x => x.ID == 3103).Value.ToInt());  //订单到期时间
                    model.SupplyNo = SupplyHelps.GetSupplyNo();  //单号
                    model.AccrualDay = 0; //已结算利息天数
                    model.SurplusAccrualDay = 0; //(天)
                    model.AccrualMoney = 0; //已产生的利息
                    model.IsAccrualEffective = false; //利息是否生效（匹配并验证付款后才生效）
                    model.IsAccruaCount = true; //是否还计算利息 (超过30天或有接受订单产生后不再计算利息)
                    model.TotalMoney = model.ExchangeAmount; //本单总额（含利息）
                    model.AccruaRate = cacheSysParam.SingleAndInit(x => x.ID == 3007).Value.ToDecimal();  //基础利息
                    model.OrderType = 1;

                    model.OrderMoney = mModel.MatchAmount;
                    model.Remark = "来自会员抢单后创建的新单";
                    MvcCore.Unity.Get<ISupplyHelpService>().Add(model);
                    MvcCore.Unity.Get<ISysDBTool>().Commit();

                    var newMatchItem = mModel.ToModel<Data.Matching>();
                    newMatchItem.SupplyUID = Umodel.ID;
                    newMatchItem.SupplyUserName = Umodel.UserName;
                    newMatchItem.MatchingNo = Matchings.GetOrderNumber();
                    newMatchItem.SupplyNo = model.SupplyNo;
                    newMatchItem.CreateTime = DateTime.Now;
                    newMatchItem.PayEndTime = DateTime.Now.AddMinutes(cacheSysParam.SingleAndInit(x => x.ID == 3813).Value.ToInt()); //付款截止时间
                    newMatchItem.Status = (int)Data.Enum.MatchingStatus.UnPaid; //未付款
                    newMatchItem.IsOpenBuying = false;
                    newMatchItem.Remark = "来自会员抢单后创建的新单";
                    newMatchItem.FromUID = 0;
                    newMatchItem.FromUserName = "";
                    MvcCore.Unity.Get<IMatchingService>().Add(newMatchItem);
                    MvcCore.Unity.Get<ISysDBTool>().Commit();

                    mModel.Status = (int)Data.Enum.MatchingStatus.Cancel;
                    mModel.CancelTime = DateTime.Now;
                    mModel.CanceReason = "抢单成功（新单号：" + newMatchItem.MatchingNo + "）,系统自动取消原单";
                    MvcCore.Unity.Get<IMatchingService>().Update(mModel);
                    MvcCore.Unity.Get<ISysDBTool>().Commit();

                    ts.Complete();
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

        public ActionResult Wallet(int Wallet2001Page = 1, int Wallet2002Page = 1, int Wallet2003Page = 1, int Wallet2004Page = 1, int BonusDetailPage = 1)
        {
            const int pageSize = 5;

            #region 查询
            var wallet2001s = Request.QueryString["wallet2001s"] == null ? "" : Request.QueryString["wallet2001s"];
            var wallet2002s = Request.QueryString["wallet2002s"] == null ? "" : Request.QueryString["wallet2002s"];
            var wallet2003s = Request.QueryString["wallet2003s"] == null ? "" : Request.QueryString["wallet2003s"];
            var wallet2004s = Request.QueryString["wallet2004s"] == null ? "" : Request.QueryString["wallet2004s"];
            var PendBonuss = Request.QueryString["PendBonuss"] == null ? "" : Request.QueryString["PendBonuss"];
            #endregion

            #region 多组异步
            if (Request.IsAjaxRequest())
            {
                var target = Request.QueryString["target"];
                if (target == "Wallet2001")
                {
                    return PartialView("_Wallet2001",
                        MvcCore.Unity.Get<JN.Data.Service.IWalletLogService>().List(x => (x.UID == Umodel.ID && x.CoinID == 2001) && (x.Description.Contains(wallet2001s) || x.CreateTime.ToString().Contains(wallet2001s))).OrderByDescending(x => x.ID).ToList().ToPagedList(Wallet2001Page, pageSize));
                }

                if (target == "Wallet2002")
                {
                    return PartialView("_Wallet2002",
                        MvcCore.Unity.Get<JN.Data.Service.IWalletLogService>().List(x => (x.UID == Umodel.ID && x.CoinID == 2002) && (x.Description.Contains(wallet2002s) || x.CreateTime.ToString().Contains(wallet2002s))).OrderByDescending(x => x.ID).ToList().ToPagedList(Wallet2002Page, pageSize));
                }

                if (target == "PendBonus")
                {
                    return PartialView("_PendBonus",
                       MvcCore.Unity.Get<JN.Data.Service.IBonusDetailService>().List(x => (x.UID == Umodel.ID && x.IsBalance == false) && (x.BonusName.Contains(PendBonuss) || x.Description.Contains(PendBonuss) || x.CreateTime.ToString().Contains(PendBonuss))).OrderByDescending(x => x.ID).ToList().ToPagedList(BonusDetailPage, pageSize));
                }

                if (target == "Wallet2003")
                {
                    return PartialView("_Wallet2003",
                           MvcCore.Unity.Get<JN.Data.Service.IWalletLogService>().List(x => (x.UID == Umodel.ID && x.CoinID == 2003) && (x.Description.Contains(wallet2003s) || x.CreateTime.ToString().Contains(wallet2003s))).OrderByDescending(x => x.ID).ToList().ToPagedList(Wallet2003Page, pageSize));
                }
                if (target == "Wallet2004")
                {
                    return PartialView("_Wallet2004",
                           MvcCore.Unity.Get<JN.Data.Service.IWalletLogService>().List(x => (x.UID == Umodel.ID && x.CoinID == 2004) && (x.Description.Contains(wallet2004s) || x.CreateTime.ToString().Contains(wallet2004s))).OrderByDescending(x => x.ID).ToList().ToPagedList(Wallet2004Page, pageSize));
                }
            }
            #endregion

            #region 存入模型
            var model = new CompositeWalletLog
            {
                Wallet2001 = MvcCore.Unity.Get<JN.Data.Service.IWalletLogService>().List(x => (x.UID == Umodel.ID && x.CoinID == 2001) && (x.Description.Contains(wallet2001s) || x.CreateTime.ToString().Contains(wallet2001s))).OrderByDescending(x => x.ID).ToList().ToPagedList(Wallet2001Page, pageSize),
                Wallet2002 = MvcCore.Unity.Get<JN.Data.Service.IWalletLogService>().List(x => (x.UID == Umodel.ID && x.CoinID == 2002) && (x.Description.Contains(wallet2002s) || x.CreateTime.ToString().Contains(wallet2002s))).OrderByDescending(x => x.ID).ToList().ToPagedList(Wallet2002Page, pageSize),
                Wallet2003 = MvcCore.Unity.Get<JN.Data.Service.IWalletLogService>().List(x => (x.UID == Umodel.ID && x.CoinID == 2003) && (x.Description.Contains(wallet2003s) || x.CreateTime.ToString().Contains(wallet2003s))).OrderByDescending(x => x.ID).ToList().ToPagedList(Wallet2003Page, pageSize),
                Wallet2004 = MvcCore.Unity.Get<JN.Data.Service.IWalletLogService>().List(x => (x.UID == Umodel.ID && x.CoinID == 2004) && (x.Description.Contains(wallet2004s) || x.CreateTime.ToString().Contains(wallet2004s))).OrderByDescending(x => x.ID).ToList().ToPagedList(Wallet2004Page, pageSize),
                BonusDetailList = MvcCore.Unity.Get<JN.Data.Service.IBonusDetailService>().List(x => (x.UID == Umodel.ID && x.IsBalance == false) && (x.BonusName.Contains(PendBonuss) || x.Description.Contains(PendBonuss) || x.CreateTime.ToString().Contains(PendBonuss))).OrderByDescending(x => x.ID).ToList().ToPagedList(BonusDetailPage, pageSize)
            };
            #endregion

            ActMessage = "钱包";
            return View(model);
        }

        #region 收益明细列表
        public ActionResult BonusTotal(int? page)
        {
            ActMessage = "我的收益";
            var list = BonusDetailService.List(x => x.UID == Umodel.ID).WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).OrderByDescending(x => x.ID).ToList();
            return View(list.ToPagedList(page ?? 1, 20));
        }
        #endregion

        #region 帐户明细列表
        public ActionResult AccountDetail(int? coin, int? page)
        {

            int coinID = coin ?? 2002;
            ViewBag.ParamList = cacheSysParam.Where(x => x.PID == 2000 && x.IsUse == true).OrderBy(x => x.Sort).ToList();
            ActMessage = "个人钱包";
          
            var list = WalletLogService.List(x => x.CoinID == coinID && x.UID == Umodel.ID).OrderByDescending(x => x.ID).ToList();
            return View(list.ToPagedList(page ?? 1, 20));
        }
        #endregion

        #region 奖金明细列表
        public ActionResult BonusDetail(int? bonusid, int? page)
        {
            ActMessage = "奖金明细表";
            ViewBag.ParamList = cacheSysParam.Where(x => x.PID == 1100 && x.IsUse == true).OrderBy(x => x.Sort).ToList();
            var list = BonusDetailService.List(x => x.UID == Umodel.ID).OrderByDescending(x => x.ID).WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).ToList().ToPagedList(page ?? 1, 20);

            if (bonusid.HasValue)
                list = list.Where(x => x.BonusID == (bonusid ?? 0)).ToPagedList(page ?? 1, 20);

            if (Request.IsAjaxRequest())
                return PartialView("_BonusDetail", list);
            return View(list);
        }
        #endregion

        #region 兑换
        public ActionResult ApplyExchange()
        {
            int fromid = Request["fromCoin"].ToInt();
            int toid = Request["toCoin"].ToInt();

            var fromCoin = cacheSysParam.Single(x => x.ID == fromid);
            var toCoin = cacheSysParam.Single(x => x.ID == toid);

            if (fromCoin != null && toCoin != null)
            {
                ViewBag.Title = fromCoin.Name + "兑换" + toCoin.Name;
                ActMessage = ViewBag.Title;
                ViewBag.Poundage = cacheSysParam.Single(x => x.ID == 1903).Value2;
                ViewBag.Beisu = cacheSysParam.Single(x => x.ID == 1903).Value;

                ViewBag.fromCoin = fromCoin.ID;
                ViewBag.toCoin = toCoin.ID;
                return View(Umodel);
            }
            else
            {
                ViewBag.ErrorMsg = "错误的参数。";
                return View("Error");
            }
        }

        [HttpPost]
        public ActionResult ApplyExchange(FormCollection form)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                string drawmoney = form["drawmoney"];
                string formcoin = form["fromCoin"];
                string tocoin = form["toCoin"];
                if (drawmoney.ToDecimal() <= 0) throw new Exception("请输入正确的兑换数量");

                var fromCoin = cacheSysParam.Single(x => x.ID == formcoin.ToInt());
                var toCoin = cacheSysParam.Single(x => x.ID == tocoin.ToInt());
                if (fromCoin == null || toCoin == null) throw new Exception("错误的参数");
                decimal fromWallet = 0;
                if (fromCoin.ID == 2002) fromWallet = Umodel.Wallet2002;
                if (fromWallet < drawmoney.ToDecimal()) throw new Exception("您的帐户余额不足");

                #region 事务操作
                using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
                {
                    Wallets.doExchange(Umodel.ID, drawmoney.ToDecimal(), fromCoin.ID, toCoin.ID);
                    ts.Complete();
                    result.Status = 200;
                }
                #endregion
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
            return Json(result);
        }
        #endregion

        #region 幸运 抽奖

        public ActionResult LuckDraw()
        {
            return View();
        }
        /// <summary>
        ///幸运 抽奖
        /// </summary>
        /// <returns></returns>
        public ActionResult LuckDrawSum()
        {
            //当天推荐人数
            int tjrs = UserService.List(x => x.RefereeID == Umodel.ID && SqlFunctions.DateDiff("day", x.CreateTime, DateTime.Now) == 0).Count();

            if (MvcCore.Unity.Get<JN.Data.Service.IWalletLogService>().List(x => x.ID == Umodel.ID && x.Description.Contains("抽奖获得") && SqlFunctions.DateDiff("day", x.CreateTime, DateTime.Now) == 0).Count() >= tjrs)
                return Json(new { result = "err", msg = "您已经没有抽奖资格" });

            int number = 341;
            int level = 5;
            string proname = "";
            Random rd = new Random();
            int r = rd.Next(0, cacheSysParam.SingleAndInit(x => x.ID == 2406).Value.ToInt());
            if (r <= cacheSysParam.SingleAndInit(x => x.ID == 2401).Value2.ToInt())
            {
                number = 55;
                level = 1;
                proname = cacheSysParam.SingleAndInit(x => x.ID == 2401).Value;
            }
            else if (r < cacheSysParam.SingleAndInit(x => x.ID == 2402).Value2.ToInt())
            {
                number = 140;
                level = 2;
                proname = cacheSysParam.SingleAndInit(x => x.ID == 2402).Value;
            }
            else if (r < cacheSysParam.SingleAndInit(x => x.ID == 2403).Value2.ToInt())
            {
                number = 199;
                level = 3;
                proname = cacheSysParam.SingleAndInit(x => x.ID == 2403).Value;
            }
            else if (r < cacheSysParam.SingleAndInit(x => x.ID == 2404).Value2.ToInt())
            {
                number = 269;
                level = 4;
                proname = cacheSysParam.SingleAndInit(x => x.ID == 2404).Value;
            }
            else
            {
                number = 341;
                level = 5;
                proname = cacheSysParam.SingleAndInit(x => x.ID == 2405).Value;
            }

            //写入数据库，奖金表
            if (level != 1)
                Wallets.changeWallet(Umodel.ID, proname.ToDecimal(), 2002, "抽奖获得");


            return Json(new { result = "ok", angle = number, arrangement = level, name = proname, msg = "抽奖成功" });
        }
        #endregion


        #region 排单币转账
        public ActionResult MenPiaoTransfer()
        {
            //int fromid = Request["fromCoin"].ToInt();
            //int toid = Request["toCoin"].ToInt();

            //var fromCoin = cacheSysParam.Single(x => x.ID == 2003);
            // var toCoin = cacheSysParam.Single(x => x.ID == toid);

            //if (fromCoin != null && toCoin != null)
            //{
            ViewBag.Title = "排单币转账";// fromCoin.Name + "兑换" + toCoin.Name;
            ActMessage = ViewBag.Title;
            ViewBag.Poundage = cacheSysParam.Single(x => x.ID == 2003).Value2;
            ViewBag.Beisu = cacheSysParam.Single(x => x.ID == 2003).Value3;

            return View(Umodel);
            //}
            //else
            //{
            //    ViewBag.ErrorMsg = "错误的参数。";
            //    return View("Error");
            //}
        }

        [HttpPost]
        public ActionResult MenPiaoTransfer(FormCollection form)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                string drawmoney = form["drawmoney"];
                string tousername = form["tousername"];
                if (drawmoney.ToDecimal() <= 0) throw new Exception("请输入正确的兑换数量");
                if (tousername.Length <= 0) throw new Exception("请输入转账会员");
                if (MvcCore.Unity.Get<JN.Data.Service.IUserService>().List(x => x.UserName == tousername && x.IsLock == false && x.UserName != Umodel.UserName).Count() <= 0) throw new Exception("对不起，转账会员不存在！");
                var toUser = MvcCore.Unity.Get<JN.Data.Service.IUserService>().Single(x => x.UserName == tousername && x.IsLock == false && x.UserName != Umodel.UserName);
                //var PARAM = cacheSysParam.Single(x => x.ID == 2005);

                if (Umodel.Wallet2003 < drawmoney.ToDecimal()) throw new Exception("您的帐户余额不足");

                #region 事务操作
                using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
                {
                    Wallets.changeWallet(Umodel.ID, -drawmoney.ToDecimal(), 2003, "向会员【" + toUser.UserName + "】转账" + drawmoney + "个");
                    Wallets.changeWallet(toUser.ID, drawmoney.ToDecimal(), 2003, "会员【" + Umodel.UserName + "】向你转账" + drawmoney + "个");
                    ts.Complete();
                    result.Status = 200;
                }
                #endregion
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
            return Json(result);
        }
        #endregion

        #region 激活币转账
        public ActionResult JihuoTransfer()
        {
            //int fromid = Request["fromCoin"].ToInt();
            //int toid = Request["toCoin"].ToInt();

            //var fromCoin = cacheSysParam.Single(x => x.ID == 2003);
            // var toCoin = cacheSysParam.Single(x => x.ID == toid);

            //if (fromCoin != null && toCoin != null)
            //{
            ViewBag.Title = "激活币转账";// fromCoin.Name + "兑换" + toCoin.Name;
            ActMessage = ViewBag.Title;
            ViewBag.Poundage = cacheSysParam.Single(x => x.ID == 2004).Value2;
            ViewBag.Beisu = cacheSysParam.Single(x => x.ID == 2004).Value3;

            return View(Umodel);
            //}
            //else
            //{
            //    ViewBag.ErrorMsg = "错误的参数。";
            //    return View("Error");
            //}
        }

        [HttpPost]
        public ActionResult JihuoTransfer(FormCollection form)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                string drawmoney = form["drawmoney"];
                string tousername = form["tousername"];
                if (drawmoney.ToDecimal() <= 0) throw new Exception("请输入正确的兑换数量");
                if (tousername.Length <= 0) throw new Exception("请输入转账会员");
                if (MvcCore.Unity.Get<JN.Data.Service.IUserService>().List(x => x.UserName == tousername && x.IsLock == false && x.UserName != Umodel.UserName).Count() <= 0) throw new Exception("对不起，转账会员不存在！");
                var toUser = MvcCore.Unity.Get<JN.Data.Service.IUserService>().Single(x => x.UserName == tousername && x.IsLock == false && x.UserName != Umodel.UserName);
                //var PARAM = cacheSysParam.Single(x => x.ID == 2005);

                if (Umodel.Wallet2004 < drawmoney.ToDecimal()) throw new Exception("您的帐户余额不足");

                #region 事务操作
                using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
                {
                    Wallets.changeWallet(Umodel.ID, -drawmoney.ToDecimal(), 2004, "向会员【" + toUser.UserName + "】转账" + drawmoney + "个");
                    Wallets.changeWallet(toUser.ID, drawmoney.ToDecimal(), 2004, "会员【" + Umodel.UserName + "】向你转账" + drawmoney + "个");
                    ts.Complete();
                    result.Status = 200;
                }
                #endregion
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
            return Json(result);
        }
        #endregion
    }
}

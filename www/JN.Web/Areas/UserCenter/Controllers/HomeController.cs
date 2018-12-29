using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using JN.Data.Service;
using JN.Services.Tool;
using System.Data.Entity.SqlServer;
using JN.Services.Manager;
using System.IO;
using Webdiyer.WebControls.Mvc;
using MvcCore.Controls;

namespace JN.Web.Areas.UserCenter.Controllers
{

    /// <summary>
    /// 此版本有以下功能：
    /// 1、此版本为国内3M体系
    /// 2、12小时内付款有奖励，每个提供单只有一次   查找关键词“12小时内付款有奖励”定位
    /// 3、奖金有冻结期
    /// 4、奖金有烧伤
  
    /// </summary>
    public class HomeController : BaseController
    {
        private readonly IUserService UserService;
        private readonly ISupplyHelpService SupplyHelpService;
        private readonly IAcceptHelpService AcceptHelpService;
        private readonly IMatchingService MatchingService;
        private readonly IBonusDetailService BonusDetailService;
        private readonly ISysSettingService SysSettingService;
        private readonly ISysDBTool SysDBTool;
        private readonly IActLogService ActLogService;
       
        private static List<Data.SysParam> cacheSysParam = null;
        public HomeController(ISysDBTool SysDBTool,
            IUserService UserService,
            ISupplyHelpService SupplyHelpService,
            IAcceptHelpService AcceptHelpService,
            IMatchingService MatchingService,
            IBonusDetailService BonusDetailService,
            ISysSettingService SysSettingService,
            IActLogService ActLogService
            )
        {
            this.UserService = UserService;
            this.SupplyHelpService = SupplyHelpService;
            this.AcceptHelpService = AcceptHelpService;
            this.MatchingService = MatchingService;
            this.BonusDetailService = BonusDetailService;
            this.SysSettingService = SysSettingService;
            this.SysDBTool = SysDBTool;
            this.ActLogService = ActLogService;
          
            cacheSysParam = MvcCore.Unity.Get<ISysParamService>().ListCache("sysparams", x => x.ID < 4000).ToList();
        }


        #region 首页获取列表

        public ActionResult Index(int? page)
        {
            //Bonus.ExpireBonus(Umodel); //到期的一些奖金(有冻结期的)
            MMM.CheckPayEndTime();

            int hideminute = cacheSysParam.SingleAndInit(x => x.ID == 3804).Value.ToInt(); //交易成功后隐藏记录参数
            var list = MatchingService.List(x => (x.SupplyUID == Umodel.ID || x.AcceptUID == Umodel.ID) && (x.IsOpenBuying ?? false) == false).WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).OrderByDescending(x => x.ID).ToList().ToPagedList(page ?? 1, 20);
            if (hideminute > 0)
                list = list.Where(x => x.Status < (int)Data.Enum.MatchingStatus.Verified || (x.Status == (int)JN.Data.Enum.MatchingStatus.Verified && SqlFunctions.DateDiff("minute", (x.VerifiedEndTime ?? DateTime.Now), DateTime.Now) <= hideminute)).ToList().ToPagedList(page ?? 1, 20);
           
            if (Request.IsAjaxRequest())
                return PartialView("_PartialMatchList", list);

            //var supplylists = MvcCore.Unity.Get<JN.Data.Service.ISupplyHelpService>().List().ToList();
            //foreach (var item in supplylists)
            //{
            //    var onUser = MvcCore.Unity.Get<Data.Service.IUserService>().Single(item.UID);
            //    onUser.ReserveInt1 = 1;
            //    MvcCore.Unity.Get<Data.Service.IUserService>().Update(onUser);
            //    MvcCore.Unity.Get<Data.Service.ISysDBTool>().Commit();
            //}


            MMM.MustBeRegisteredAfterSupplyHelp(Umodel); //注册48小时内必须提供帮助，否侧冻结帐号（参数调为0不生效）
            //MMM.MustBeReCastAfterAcceptHelp(Umodel);//提现完成后必须进行复投，否侧冻结帐号（参数调为0不生效）
            //MMM.CheckPayEndTime();
            //MMM.CheckVerifiedEndTime();
            //Bonus.JIEDONGBENJILIXI();//自动解冻本机和利息
            var sysEntity = MvcCore.Unity.Get<ISysSettingService>().Single(1);
            string viewname = sysEntity.Theme;
            //会员升级
            //Users.UpdateUser();
            
            return View("index" + viewname, list);
        }
        #endregion

      

        #region 匹配订单列表
        public ActionResult MatchingList(int? page)
        {
            ActMessage = "匹配订单列表";
            int hideminute = cacheSysParam.SingleAndInit(x => x.ID == 3804).Value.ToInt(); //交易成功后隐藏记录参数
            var list = MatchingService.List(x => (x.SupplyUID == Umodel.ID || x.AcceptUID == Umodel.ID) && (x.IsOpenBuying ?? false) == false).WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).OrderByDescending(x => x.ID).ToList();
            if (hideminute > 0)
                list = list.Where(x => x.Status < (int)Data.Enum.MatchingStatus.Verified || (x.Status == (int)JN.Data.Enum.MatchingStatus.Verified && SqlFunctions.DateDiff("minute", (x.VerifiedEndTime ?? DateTime.Now), DateTime.Now) <= hideminute)).ToList();

            return View(list.ToPagedList(page ?? 1, 20));
        }
        #endregion

        #region Partial引用模块
        public ActionResult _PartialSubmitSupplyHelp()
        {
            return View();
        }

        public ActionResult _PartialSubmitAcceptHelp()
        {
            return View();
        }

        public ActionResult _PartialConfirmPay()
        {
            return View();
        }

        public ActionResult _PartialVerifyPay()
        {
            return View();
        }

        public ActionResult _PartialSubmitLeaveWord(int rid)
        {
            var model = MatchingService.Single(rid);
            if (model != null) return View(model);
            return showmsg("记录不存在");
        }

        public ActionResult _PartialPayDetail(int rid)
        {
            var model = MatchingService.Single(rid);
            if (model != null) return View(model);
            return showmsg("记录不存在");
        }

        #endregion

        #region 提供帮助
        [HttpPost]
        public ActionResult SupplyHelp(FormCollection fc)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                string SupplyAmount = fc["supplyamount"];
                string PayWay = fc["payway"];

              

                if (string.IsNullOrEmpty(Umodel.BankCard) && string.IsNullOrEmpty(Umodel.WeiXin) && string.IsNullOrEmpty(Umodel.AliPay))
                    throw new Exception("您还未填写任何一个收款帐号（银行卡、支付宝），请到“帐号管理”处修改个人资料！");
                if (string.IsNullOrEmpty(PayWay)) throw new Exception("请选择付款方式！");
                if (SupplyAmount.ToDecimal() <= 0) throw new Exception("请您填写提供帮助的金额！");
                //if (SupplyHelpService.List(x => x.UID == Umodel.ID && x.Status < (int)Data.Enum.HelpStatus.AllDeal && x.Status > 0).Count() > 0)
                //    throw new Exception("对不起，你有一单提供帮助没有完成，无法提供帮助！");

                //排单间隔3110
                var SupolyModel = MvcCore.Unity.Get<Data.Service.ISupplyHelpService>().List(x => x.UID == Umodel.ID && x.Status > 0).OrderByDescending(x => x.ID).SingleOrDefault();//查找上一单

                if (SupolyModel != null)
                {
                    if (SupolyModel.CreateTime.AddMinutes(cacheSysParam.SingleAndInit(x => x.ID == 3110).Value.ToInt()) > DateTime.Now)
                    {
                        throw new Exception("对不起，排单时间要与上一单间隔" + cacheSysParam.SingleAndInit(x => x.ID == 3110).Value.ToInt() + "小时,请于【" + SupolyModel.CreateTime.AddHours(cacheSysParam.SingleAndInit(x => x.ID == 3110).Value.ToInt()) + "】进行排单");
                    }
                }

                decimal rate= cacheSysParam.SingleAndInit(x => x.ID == 3801).Value.ToDecimal();
                decimal ExchangeAmount = SupplyAmount.ToDecimal() * rate;　//汇率参数
                //decimal minmoney = cacheSysParam.SingleAndInit(x => x.ID == 3001).Value.Split('-')[0].ToDecimal();　//提供帮助金额限制参数(基准最小)
                //decimal maxmoney = cacheSysParam.SingleAndInit(x => x.ID == 3001).Value.Split('-')[1].ToDecimal();　//提供帮助金额限制参数(基准最大)
                var param = cacheSysParam.Single(x => x.ID==2101);
                //if(Umodel.IsAgent==true)
                //    param = cacheSysParam.Single(x => x.ID == 2102);
                var supplycount= MvcCore.Unity.Get<Data.Service.ISupplyHelpService>().List(x => x.UID == Umodel.ID && x.Status > 0).OrderByDescending(x => x.ID).Count();//
                decimal minmoney = param.Value.ToDecimal();
                decimal maxmoney = param.Value2.ToDecimal();
                var maxmessage = "对不起，提供帮助金额不能大于" + maxmoney + "";
                if (supplycount == 0)
                {
                    maxmoney = cacheSysParam.Single(x => x.ID == 2102).Value.ToDecimal();
                    maxmessage = "对不起，首次提供帮助金额不能大于" + maxmoney + "";

                }
                   

                //if (Umodel.IsAgent == true)
                //{
                //       var param2103 = cacheSysParam.Single(x => x.ID==2103);
                //    int num = MvcCore.Unity.Get<ISupplyHelpService>().List(x => x.ReserveInt2 == 2 && x.Status >= (int)Data.Enum.HelpStatus.AllDeal).Count();
                //    decimal newmax = minmoney + (decimal)num * param2103.ToDecimal();
                //    if (newmax < maxmoney)
                //        maxmoney = newmax;
                //}


                if (ExchangeAmount < minmoney) throw new Exception("对不起，提供帮助金额不能少于" + minmoney + "");
                if (ExchangeAmount > maxmoney) throw new Exception(maxmessage);

                decimal pdb = ExchangeAmount * cacheSysParam.SingleAndInit(x => x.ID == 2104).Value.ToDecimal();
                //检验排单币余额
                if (pdb > Umodel.Wallet2003) { throw new Exception("您的排单币不足！"); }

                //每一轮增加5000，最高60000
                //int tgcount = SupplyHelpService.List(x => x.UID == Umodel.ID && x.Status >= (int)Data.Enum.HelpStatus.AllDeal).Count(); //已经完成的提供帮助次数
                //decimal maxsupplymoney = cacheSysParam.SingleAndInit(x => x.ID == 3001).Value3.ToDecimal(); //充许提供帮助的最大金额60000
                //decimal stepmoney = cacheSysParam.SingleAndInit(x => x.ID == 3001).Value2.ToDecimal(); //每完成一次提供可累加金额
                //maxmoney = Math.Min(maxsupplymoney, (tgcount * stepmoney + maxmoney));

                //int _maxmonthsupplycount = cacheSysParam.SingleAndInit(x => x.ID == 3805).Value.ToInt(); //每月最多可提供订单
                //if (_maxmonthsupplycount > 0)
                //{
                //    if (SupplyHelpService.List(x => x.UID == Umodel.ID && x.OrderType == 1 && x.Status > 0 && SqlFunctions.DateDiff("month", x.CreateTime, DateTime.Now) == 0).Count() > _maxmonthsupplycount)
                //        throw new Exception("对不起，每月最多只可提供帮助次数为：" + _maxmonthsupplycount + "次");
                //}

                //排单必须等于或大于上一单。如果小于上一单，无法排单。
                //decimal EndMoney = MvcCore.Unity.Get<Data.Service.ISupplyHelpService>().List(x => x.UID == Umodel.ID && x.Status > 0).OrderByDescending(x => x.ID).Count() == 0 ? 0 : MvcCore.Unity.Get<Data.Service.ISupplyHelpService>().List(x => x.UID == Umodel.ID && x.Status > 0).OrderByDescending(x => x.ID).SingleOrDefault().SupplyAmount;    //查找上一单
                var latstsupply = MvcCore.Unity.Get<Data.Service.ISupplyHelpService>().List(x => x.UID == Umodel.ID && x.Status >= (int)Data.Enum.HelpStatus.AllDeal && x.RoundAmount > 0).OrderByDescending(x => x.ID).FirstOrDefault();
                if (latstsupply != null)
                {
                    if (SupplyAmount.ToDecimal() < (latstsupply.RoundAmount ?? 0))
                        throw new Exception("挂单金额不能低于上一单");
                    
                }
               // if (SupplyAmount.ToDecimal() < EndMoney) throw new Exception("必须大于上一单的金额");
               // int _nextsupplytime = cacheSysParam.SingleAndInit(x => x.ID == 3807).Value.ToInt(); //下次提供帮助间隔时间
                //if (_nextsupplytime > 0)
                //{
                //    if (SupplyHelpService.List(x => x.UID == Umodel.ID && x.Status > 0 && SqlFunctions.DateDiff("minute", x.CreateTime, DateTime.Now) < _nextsupplytime).Count() > 0)
                //        throw new Exception("对不起，提供帮助间隔时间不足，请在达到排单间隔天数后再试");
                //}

                //个人日排单限制
                //var mytodaysupplylist = SupplyHelpService.List(x => x.UID == Umodel.ID && x.Status > 0 && SqlFunctions.DateDiff("day", x.CreateTime, DateTime.Now) == 0);
                //decimal mytodaysupplymoney = mytodaysupplylist.Count() > 0 ? mytodaysupplylist.Sum(x => x.ExchangeAmount) : 0;
                //if (ExchangeAmount < minmoney) throw new Exception("对不起，提供帮助金额不能少于" + minmoney + "");
                //if ((mytodaysupplymoney + ExchangeAmount) > maxmoney) throw new Exception("对不起，今日提供帮助金额不能大于" + maxmoney + "");

                int beisu = param.Value3.ToInt(); //金额倍数
                if (ExchangeAmount % beisu != 0) throw new Exception("金额必须是" + beisu + "的倍数！");

                ////系统日排单上限
                //decimal PARAM_SYSPDSX = cacheSysParam.SingleAndInit(x => x.ID == 3006).Value.ToDecimal();
                //if (PARAM_SYSPDSX > 0)
                //{
                //    var todaysupplylist = SupplyHelpService.List(x => x.Status > 0 && SqlFunctions.DateDiff("day", x.CreateTime, DateTime.Now) == 0);
                //    decimal todaysupplymoney = todaysupplylist.Count() > 0 ? todaysupplylist.Sum(x => x.ExchangeAmount) : 0;
                //    if ((todaysupplymoney + ExchangeAmount) > PARAM_SYSPDSX) throw new Exception("对不起，已超出系统今日系统排单上限，请明天再排单");
                //}

                using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
                {
                    //decimal PARAM_FIRSTBL = cacheSysParam.SingleAndInit(x => x.ID == 3105).Value.ToDecimal();
                    //decimal firstMoney = ExchangeAmount * PARAM_FIRSTBL;
                    string str = "";
                    for (int i = 0; i < 2; i++)
                    {
                     

                        var model = new Data.SupplyHelp();
                        model.SupplyNo = SupplyHelps.GetSupplyNo();  //单号
                        if (i == 0)  //存贮第一单标识
                        {
                            str = model.SupplyNo;
                            model.ReserveInt2 = 1;
                            ExchangeAmount = SupplyAmount.ToDecimal() * rate * (decimal)0.2;
                            
                            
                        }
                        else
                        {
                            model.ReserveInt2 = 2;
                            ExchangeAmount = SupplyAmount.ToDecimal() * rate * (decimal)0.8;
                        }
                        model.UID = Umodel.ID;
                        model.UserName = Umodel.UserName;
                        model.SupplyAmount = ExchangeAmount.ToDecimal(); //申请金额
                        model.ExchangeAmount = ExchangeAmount.ToDecimal();// ExchangeAmount - firstMoney; //汇率金额
                        model.CreateTime = DateTime.Now;
                        model.Status = 1;  //状态
                        model.IsTop = false;  //是否置顶
                        model.IsRepeatQueuing = false; //是否重新排队
                        model.HaveMatchingAmount = 0; //已匹配数量
                        model.HaveAcceptAmount = 0; //
                        model.PayWay = PayWay;  //付款方式
                        model.EndTime = DateTime.Now.AddMinutes(cacheSysParam.SingleAndInit(x => x.ID == 3103).Value.ToInt());  //订单到期时间
                        model.RoundAmount = SupplyAmount.ToDecimal() * rate;  //本轮金额
                      
                     
                        model.ReserveStr2 = str;
                       

                        model.AccrualDay = 0; //已结算利息天数
                        model.SurplusAccrualDay = cacheSysParam.SingleAndInit(x => x.ID == 1102).Value.ToInt(); //(天)
                        model.AccrualMoney = 0; //已产生的利息
                        model.IsAccrualEffective = false; //利息是否生效（匹配并验证付款后才生效）
                        model.IsAccruaCount = true; //是否还计算利息 (超过10天或有接受订单产生后不再计算利息)
                        model.TotalMoney = model.ExchangeAmount; //本单总额（含利息）
                        model.AccruaRate = cacheSysParam.SingleAndInit(x => x.ID == 1102).Value2.ToDecimal();  //基础利息
                        model.OrderType = 1;
                        model.OrderMoney = ExchangeAmount;
                        model.Remark = "";
                      
                        SupplyHelpService.Add(model);
                        SysDBTool.Commit();
                    }
                    Bonus.Bonus1103(str);//计算动态奖

                    //扣除排单币
                    Wallets.changeWallet(Umodel.ID, -pdb, 2003, "来自提供订单【" + str + "】扣除");

                    //var model2 = model.ToModel<Data.SupplyHelp>();　//副单
                    //model2.OrderType = 0;
                    //model2.MainSupplyID = model.ID;
                    //model2.SupplyNo = SupplyHelps.GetSupplyNo2();  //单号
                    //model2.ExchangeAmount = firstMoney; //汇率金额
                    //model2.OrderMoney = ExchangeAmount;
                    //SupplyHelpService.Add(model2);
                    //SysDBTool.Commit();

                    if (SysSettingService.Single(1).MatchingMode == 1)
                    {
                        string outMsg = "";
                        MMM.Matching("", "", ref outMsg); //自动匹配
                    }
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

        #endregion

        #region 接受帮助
        [HttpPost]
        public ActionResult AcceptHelp(FormCollection fc)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                string PayWay = fc["payway"];
                decimal AcceptAmount = fc["acceptamount"].ToDecimal();
                int CoinID = fc["coinid"].ToInt();
                string FormUrl = fc["formurl"];

                decimal acceptWallet = 0;
                int tx_num = 0;
                int tx_limit = 0;
                if (CoinID == 2001)
                {
                    acceptWallet = Umodel.Wallet2001;
                    if (Umodel.Wallet2001Lock ?? false) throw new Exception("你的钱包已被冻结，请联系管理员！");


                    tx_num = AcceptHelpService.List(x => x.Status >= (int)JN.Data.Enum.HelpStatus.NoMatching && x.CoinID == 2002 && x.UID == Umodel.ID && SqlFunctions.DateDiff("DAY", x.CreateTime, DateTime.Now) == 0).Count();

                    tx_limit = cacheSysParam.SingleAndInit(x => x.ID == 3003).Value.ToInt();
                }
                else if (CoinID == 2002)
                {
                    acceptWallet = Umodel.Wallet2002;
                    if (Umodel.Wallet2002Lock ?? false) throw new Exception("你的钱包已被冻结，请联系管理员！");
                 
                }
                else if (CoinID == 2003)
                {
                    acceptWallet = Umodel.Wallet2003;
                    if (Umodel.Wallet2003Lock ?? false) throw new Exception("你的钱包已被冻结，请联系管理员！");
                }

                if (string.IsNullOrEmpty(Umodel.BankCard) && string.IsNullOrEmpty(Umodel.WeiXin) && string.IsNullOrEmpty(Umodel.AliPay))
                    throw new Exception("您还未填写任何一个收款帐号（银行卡、支付宝、微信），请到“帐号管理”处修改个人资料！");
                if (string.IsNullOrEmpty(PayWay)) throw new Exception("请选择付款方式！");
                if (AcceptAmount <= 0) throw new Exception("请充输入接受帮助金额！");


                //if (MatchingService.List(x => x.SupplyUID == Umodel.ID && x.Status < (int)JN.Data.Enum.MatchingStatus.Verified && x.Status >= 0).Count() > 0 || 
                //    AcceptHelpService.List(x => x.UID == Umodel.ID && x.Status < (int)JN.Data.Enum.HelpStatus.AllDeal && x.Status >=0).Count() > 0 || 
                //    SupplyHelpService.List(x => x.UID == Umodel.ID && x.Status < (int)JN.Data.Enum.HelpStatus.AllDeal && x.Status >=0).Count() >0)
                //    throw new Exception("对不起，你有一单提供帮助或接受帮助没有确认成功，无法接受帮助！");



                //有匹配没完成的提供单不能提现
                if (AcceptHelpService.List(x => x.UID == Umodel.ID && x.Status < (int)JN.Data.Enum.HelpStatus.AllDeal && x.Status >= 0).Count() > 0)
                    throw new Exception("对不起，你有一单接受帮助没有完成交易，无法接受帮助！");

                int anum = cacheSysParam.SingleAndInit(x => x.ID == 3008).Value.ToInt();
                if (AcceptHelpService.List(x => x.Status > 0 && x.UID == Umodel.ID && SqlFunctions.DateDiff("DAY", x.CreateTime, DateTime.Now) == 0).Count() > anum)
                    throw new Exception("每天只可申请"+anum+"次接受帮助！");

                decimal ExchangeAmount = AcceptAmount * cacheSysParam.SingleAndInit(x => x.ID == 3801).Value.ToDecimal(); //汇率
                if (acceptWallet < ExchangeAmount) throw new Exception("你的余额不足！");

                decimal minmoney = cacheSysParam.SingleAndInit(x => x.ID == 3002).Value.Split('-')[0].ToDecimal();
                decimal maxmoney = cacheSysParam.SingleAndInit(x => x.ID == 3002).Value.Split('-')[1].ToDecimal();

                if (CoinID == 2001 && ExchangeAmount < minmoney || ExchangeAmount > maxmoney)
                    throw new Exception("接受金额需在" + minmoney + "~" + maxmoney + "之间！");
                else if (CoinID == 2002)
                {
                    minmoney = cacheSysParam.SingleAndInit(x => x.ID == 3003).Value.Split('-')[0].ToDecimal();
                    //maxmoney = cacheSysParam.SingleAndInit(x => x.ID == 3003).Value.Split('-')[1].ToDecimal();
                    maxmoney= (cacheSysParam.SingleAndInit(x => x.ID == 3003).Value2.ToDecimal())*Umodel.Wallet2002;
                    if (ExchangeAmount < minmoney ) throw new Exception("接受金额不能小于" + minmoney + "");
                    if (ExchangeAmount > maxmoney) throw new Exception("接受金额不能大于当前动态钱包的50%，不能大于"+maxmoney+"，且必须为100的整数！");
                }
               

                int beisu = cacheSysParam.SingleAndInit(x => x.ID == 3005).Value.ToInt();
                if (ExchangeAmount % beisu != 0) throw new Exception("金额必须是" + beisu + "的倍数！");

              

                #region 事务操作
                using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
                {
                    var model = new Data.AcceptHelp();
                    model.UID = Umodel.ID;
                    model.UserName = Umodel.UserName;
                    model.CoinID = CoinID;  //币种
                    model.CoinName = cacheSysParam.SingleAndInit(x => x.ID == CoinID).Value;  //币种名称
                    model.AcceptAmount = (decimal)AcceptAmount; //接受金额
                    model.ExchangeAmount = ExchangeAmount; //汇力转换后金额
                    model.HaveMatchingAmount = 0;  //已匹配金额
                    model.CreateTime = DateTime.Now;
                    model.Status = 1;
                    model.PayWay = PayWay;  //付款方式
                    model.IsTop = false; //是否置顶
                    model.IsRepeatQueuing = false; //是否重新排队
                    model.EndTime = DateTime.Now.AddMinutes(cacheSysParam.SingleAndInit(x => x.ID == 3103).Value.ToInt()); //订单到期时间
                    model.AcceptNo = AcceptHelps.GetAcceptNo();
                    AcceptHelpService.Add(model);//向接受表添加纪录
                    SysDBTool.Commit();
                    Wallets.changeWallet(Umodel.ID, 0 - model.ExchangeAmount, model.CoinID, "接受帮助订单“" + model.AcceptNo + "”扣除");

                    //接受订单要停止对应提供单利息
                    if (CoinID == 2001)
                    {
                        var supplylist = SupplyHelpService.List(x => x.UID == Umodel.ID && (x.TotalMoney - x.HaveAcceptAmount) > 0 && x.Status >= (int)Data.Enum.HelpStatus.PartOfMatching).OrderBy(x => x.ID).ToList();
                        if (supplylist.Count > 0)
                        {
                            decimal totalhaveacceptamount = 0;
                            string usesupplyno = "";
                            foreach (var item in supplylist)
                            {
                                totalhaveacceptamount += (item.TotalMoney - item.HaveAcceptAmount);
                                usesupplyno += item.SupplyNo + ",";
                                var sModel = SupplyHelpService.Single(item.ID);
                                sModel.HaveAcceptAmount = sModel.HaveAcceptAmount + totalhaveacceptamount;
                                sModel.IsAccruaCount = false;
                                sModel.AccrualStopReason = "接受订单“" + model.AcceptNo + "”创建后停止";
                                SupplyHelpService.Update(sModel);
                                SysDBTool.Commit();
                                if (totalhaveacceptamount >= ExchangeAmount) break;
                            }

                            var aModel = AcceptHelpService.Single(x => x.AcceptNo == model.AcceptNo);
                            aModel.UseSupplyNo = usesupplyno.TrimEnd(',');
                            AcceptHelpService.Update(aModel);
                            SysDBTool.Commit();
                        }
                    }

                    if (SysSettingService.Single(1).MatchingMode == 1)
                    {
                        string outMsg = "";
                        MMM.Matching("", "", ref outMsg); //自动匹配
                    }
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

        #region 留言评论
        /// <summary>
        /// 留言评论
        /// </summary>
        /// <returns></returns>
        public ActionResult SendLeavword()
        {
            string matchingno = Request["matchingno"];
            string msgcontent = Request["msgcontent"];
            if (string.IsNullOrEmpty(msgcontent))
                return Json(new { result = "error", msg = "对不起，请填写内容！" });

            var entity = new Data.LeaveWord();
            entity.CreateTime = DateTime.Now;
            entity.Content = msgcontent;
            entity.UID = Umodel.ID;
            entity.UserName = Umodel.UserName;
            entity.MatchingNo = matchingno;
            entity.MsgType = "咨询";

            MvcCore.Unity.Get<ILeaveWordService>().Add(entity);
            SysDBTool.Commit();
            if (entity.ID > 0)
                return Json(new { result = "ok", msg = "留言成功！" });
            else
                return Json(new { result = "error", msg = "留言错误！" });
        }

        #endregion

        #region 取消

        /// <summary>
        /// 退出队列（供单)
        /// </summary>
        /// <returns></returns>
        public ActionResult CancelSupplyQueuing(int id)
        {
            var sModel = SupplyHelpService.Single(id);
            if (sModel != null)
            {
                if (sModel.UID != Umodel.ID) return showmsg("非法操作");
                var list = MvcCore.Unity.Get<Data.Service.ISupplyHelpService>().List(x => x.ReserveStr2 == sModel.ReserveStr2);
                if (sModel.Status == (int)Data.Enum.HelpStatus.NoMatching)
                {
                    using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
                    {
                        foreach (var item in list)
                        {
                            MMM.CancelSupplyHelp(item.SupplyNo, "自行取消");
                        }
                        ts.Complete();
                    }
                    return showmsg("成功退出队列");
                }
                else
                    return showmsg("当前提供订单状态不可退出");
            }
            else
                return showmsg("不存在的记录");
        }

        /// <summary>
        /// 退出队列（受单)
        /// </summary>
        /// <returns></returns>
        public ActionResult CancelAcceptQueuing(int id)
        {
            var aModel = AcceptHelpService.Single(id);
            if (aModel != null)
            {
                if (aModel.UID != Umodel.ID) return showmsg("非法操作");
                if (aModel.Status == (int)Data.Enum.HelpStatus.NoMatching)
                {
                    using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
                    {
                        MMM.CancelAcceptHelp(aModel.AcceptNo, "自行取消");
                        ts.Complete();
                    }
                    return showmsg("成功退出队列");
                }
                else
                    return showmsg("当前接受订单状态不可退出");
            }
            else
                return showmsg("不存在的记录");
        }
        #endregion

        #region 确认付款
        /// <summary>
        /// 确认拨款
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ConfirmPay(FormCollection form)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                string id = form["id"];
                string content = form["content"];
                var mModel = MatchingService.Single(id.ToInt());
                if (mModel.SupplyUID != Umodel.ID) throw new Exception("非法操作");
                if (mModel == null) throw new Exception("记录不存在");
                string imgurl = "";
                if (Request.Files.Count == 0) throw new Exception("请您上传凭证！");
                HttpPostedFileBase file = Request.Files[0];
                if ((file != null) && (file.ContentLength > 0))
                {
                    if (!FileValidation.IsAllowedExtension(file, new FileExtension[] { FileExtension.PNG, FileExtension.JPG, FileExtension.BMP }))
                        throw new Exception("非法上传，您只可以上传图片格式的文件！");
                    var newfilename = Guid.NewGuid() + Path.GetExtension(file.FileName).ToLower();
                    if (!Directory.Exists(Request.MapPath("~/Content/Resource")))
                        Directory.CreateDirectory(Request.MapPath("~/Content/Resource"));

                    var fileName = Path.Combine(Request.MapPath("~/Content/Resource"), newfilename);
                    try
                    {
                        file.SaveAs(fileName);
                        var thumbnailfilename = UploadPic.MakeThumbnail(fileName, Request.MapPath("~/Content/Resource/"), 1024, 768, "EQU");
                        System.IO.File.Delete(fileName); //删除原文件
                        imgurl = "/Content/Resource/" + thumbnailfilename;
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("上传失败：" + ex.Message);
                    }
                }

                if (mModel.Status > (int)Data.Enum.MatchingStatus.Delayed) throw new Exception("当前订单状态不可付款");
                using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
                {
                    if (!string.IsNullOrEmpty(content))
                    {
                        var entity = new Data.LeaveWord();
                        entity.CreateTime = DateTime.Now;
                        entity.Content = content;
                        entity.UID = Umodel.ID;
                        entity.UserName = Umodel.UserName;
                        entity.MatchingNo = mModel.MatchingNo;
                        entity.MsgType = "付款留言";

                        MvcCore.Unity.Get<ILeaveWordService>().Add(entity);
                        SysDBTool.Commit();
                    }
                    mModel.ProofImageUrl = imgurl;
                    mModel.Status = (int)Data.Enum.MatchingStatus.Paid;
                    mModel.PayTime = DateTime.Now;
                    mModel.VerifiedEndTime = DateTime.Now.AddMinutes(cacheSysParam.SingleAndInit(x => x.ID == 3108).Value.ToInt()); //订单付款后确认时限参数
                    MatchingService.Update(mModel);
                    SysDBTool.Commit();

                    //6小时内付款有奖励
                    Bonus.Bonus1105(mModel);
                 


                    if (MvcCore.Unity.Get<ISysParamService>().SingleAndInit(x => x.ID == 4505).Value == "1") //付款成功是否通知接受单会员
                    {
                        var acceptUser = UserService.Single(x => x.ID == mModel.AcceptUID);
                        if (acceptUser != null)
                            SMSHelper.WebChineseMSM(acceptUser.Mobile, MvcCore.Unity.Get<ISysParamService>().SingleAndInit(x => x.ID == 4505).Value2.Replace("{ORDERNUMBER}", mModel.MatchingNo).Replace("{USERNAME}", acceptUser.UserName));
                    }

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

        /// <summary>
        /// 延时付款
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DelayedPay(int id)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                var mModel = MatchingService.Single(id);
                if (mModel.SupplyUID != Umodel.ID) throw new Exception("非法操作");
                if (mModel == null) throw new Exception("记录不存在");
                if (mModel.Status != (int)Data.Enum.MatchingStatus.UnPaid) throw new Exception("当前订单状态不可延时付款");
                mModel.Status = (int)Data.Enum.MatchingStatus.Delayed;
                mModel.PayEndTime = (mModel.PayEndTime ?? DateTime.Now).AddMinutes(cacheSysParam.SingleAndInit(x => x.ID == 3005).Value2.ToInt()); //付款截止时间;
                MatchingService.Update(mModel);
                SysDBTool.Commit();
                result.Status = 200;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
            return Json(result);
        }

        /// <summary>
        /// 拒绝付款
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult RefusePay(int id)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                var mModel = MatchingService.Single(id);
                if (mModel.SupplyUID != Umodel.ID) throw new Exception("非法操作");
                if (mModel == null) throw new Exception("记录不存在");


                if ((mModel.FromUID ?? 0) == 0) //订单没转移过
                {
                    var onUser = MvcCore.Unity.Get<IUserService>().Single(mModel.SupplyUID);
                    if (onUser != null)
                    {
                        //订单转移到推荐人
                        if (onUser.RefereeID > 0)
                        {
                            //同时生成一个提供单才可计算利息
                            var model = new Data.SupplyHelp();
                            model.UID = onUser.RefereeID;
                            model.UserName = onUser.RefereeUser;
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
                            model.AccruaRate = cacheSysParam.SingleAndInit(x => x.ID == 1102).Value2.ToDecimal();  //基础利息
                            model.OrderType = 1;
                            model.OrderMoney = mModel.MatchAmount;
                            MvcCore.Unity.Get<ISupplyHelpService>().Add(model);
                            MvcCore.Unity.Get<ISysDBTool>().Commit();

                            var newMatchItem = mModel.ToModel<Data.Matching>();
                            newMatchItem.SupplyUID = onUser.RefereeID;
                            newMatchItem.SupplyUserName = onUser.RefereeUser;
                            newMatchItem.SupplyNo = model.SupplyNo;
                            newMatchItem.MatchingNo = Matchings.GetOrderNumber();
                            newMatchItem.CreateTime = DateTime.Now;
                            newMatchItem.PayEndTime = DateTime.Now.AddMinutes(cacheSysParam.SingleAndInit(x => x.ID == 3106).Value.ToInt()); //付款截止时间
                            newMatchItem.Status = (int)Data.Enum.MatchingStatus.UnPaid; //未付款
                            newMatchItem.Remark = "来自“" + mModel.SupplyUserName + "”拒绝付款的订单转移，原单号：" + mModel.MatchingNo;
                            newMatchItem.FromUID = mModel.SupplyUID;
                            newMatchItem.FromUserName = mModel.SupplyUserName;
                            MvcCore.Unity.Get<IMatchingService>().Add(newMatchItem);
                            MvcCore.Unity.Get<ISysDBTool>().Commit();

                            mModel.Status = (int)JN.Data.Enum.MatchingStatus.Cancel;
                            mModel.CancelTime = DateTime.Now;
                            mModel.CanceReason = "拒绝付款,订单转移到推荐人“" + newMatchItem.SupplyUserName + "”，新单号为：" + newMatchItem.MatchingNo;
                            MvcCore.Unity.Get<IMatchingService>().Update(mModel);
                            MvcCore.Unity.Get<ISysDBTool>().Commit();
                        }

                        //对供单用户帐号冻结处理
                        onUser.IsLock = true;
                        onUser.LockTime = DateTime.Now;
                        onUser.LockReason = "拒绝付款后触发冻结，单号：" + mModel.MatchingNo + "";
                        MvcCore.Unity.Get<IUserService>().Update(onUser);
                        MvcCore.Unity.Get<ISysDBTool>().Commit();
                    }

                }
                else //进入抢单池
                {
                    //对推荐人扣除100元
                    decimal kqje = cacheSysParam.SingleAndInit(x => x.ID == 3106).Value3.ToDecimal();
                    kqje = Math.Min(kqje, mModel.MatchAmount * Convert.ToDecimal(0.05));
                    Wallets.changeWallet(mModel.SupplyUID, 0 - kqje, 2003, "拒绝付款下属会员转移的订单");

                    var newMatchItem = MvcCore.Unity.Get<IMatchingService>().Single(mModel.ID);
                    newMatchItem.Remark = "拒绝付款下属会员“" + newMatchItem.FromUserName + "”转移的订单“" + newMatchItem.MatchingNo + "”，扣除奖金并进入抢单池";
                    newMatchItem.IsOpenBuying = true;
                    MvcCore.Unity.Get<IMatchingService>().Update(newMatchItem);
                    MvcCore.Unity.Get<ISysDBTool>().Commit();
                }
                SysDBTool.Commit();
                result.Status = 200;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
            return Json(result);
        }

        #endregion

        #region 确认收款
        [HttpPost]
        public ActionResult FinshPay(FormCollection fc)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                int comfir = fc["comfir"].ToInt();
                int id = fc["id"].ToInt();
                var mModel = MatchingService.Single(id);
                if (mModel.AcceptUID != Umodel.ID) throw new Exception("非法操作");
                if (mModel == null) throw new Exception("记录不存在");
                if (mModel.Status != (int)Data.Enum.MatchingStatus.Paid) throw new Exception("当前订单状态不可确认");
                JN.Data.SupplyHelp sModel = SupplyHelpService.Single(x => x.SupplyNo==mModel.SupplyNo);
                using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
                {
                    if (comfir == 1) //选择确认收到汇款
                    {

                        Bonus.UpdateUserWallet((sModel.OrderMoney ?? 0), sModel.SupplyNo, 2001, "确认收款获得", "确认收款时获得，来自匹配单：" + mModel.MatchingNo, sModel.UID, sModel.UID, "", false, false, DateTime.Now);
                        //结算提供单利息，奖金并更新成交状态
                        Bonus.Settlement(mModel);
                        mModel.VerifiedEndTime = DateTime.Now;
                        mModel.Status = (int)Data.Enum.MatchingStatus.Verified;
                    }
                    else
                    {
                        //没有收到汇款
                        mModel.Status = (int)Data.Enum.MatchingStatus.Falsehood;
                    }
                    MatchingService.Update(mModel);
                    SysDBTool.Commit();
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
        #endregion

        public ActionResult Wait()
        {
            return View();
        }

        public ActionResult Logout()
        {
            ActMessage = "会员退出";
            Services.UserLoginHelper.UserLogout();
            return Redirect("/UserCenter/Login");
        }

        #region 修改密码(登录及二级密码一起修改)
        public ActionResult ChangePassword()
        {
            ViewBag.Title = "修改密码";
            ActMessage = ViewBag.Title;
            return View();
        }

        [HttpPost]
        public ActionResult ChangePassword(FormCollection form)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                string oldpassword = form["oldpassword"];
                string newpassword = form["newpassword"];
                string newpassword2 = form["newpassword2"];
                string connewpassword = form["connewpassword"];
                string connewpassword2 = form["connewpassword2"];

                if (oldpassword.Trim().Length == 0 || newpassword.Trim().Length == 0 || newpassword2.Trim().Length == 0)
                    throw new Exception("原二级密码、新一级密码、新二级密码不能为空");
                if (newpassword != connewpassword) throw new Exception("新一级密码与确认密码不相符");
                if (newpassword2 != connewpassword2) throw new Exception("新二级密码与确认密码不相符");
                if (Umodel.Password2 != oldpassword.ToMD5().ToMD5()) throw new Exception("原二级密码不正确");

                Umodel.Password = newpassword.ToMD5().ToMD5();
                Umodel.Password2 = newpassword2.ToMD5().ToMD5();
                UserService.Update(Umodel);
                SysDBTool.Commit();
                result.Status = 200;
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

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
using JN.Data.Extensions;
using JN.Data.Enum;
using JN.Services.Manager;
using System.Text.RegularExpressions;
using JN.Services.Tool;
using System.Data.Entity.SqlServer;

namespace JN.Web.Areas.AdminCenter.Controllers
{
    public class UserController : BaseController 
    {
        private readonly IUserService UserService;
        private readonly ISysDBTool SysDBTool;
        private readonly IActLogService ActLogService;
        
        private readonly ISupplyHelpService SupplyHelpService;
        private readonly IAcceptHelpService AcceptHelpService;
        private readonly ISysSettingService SysSettingService;
        private static List<Data.SysParam> cacheSysParam = null;
        public UserController(
            ISysDBTool SysDBTool, 
            IUserService UserService, 
            ISupplyHelpService SupplyHelpService,
            ISysSettingService SysSettingService,
            IAcceptHelpService AcceptHelpService,
            IActLogService ActLogService
            )
        {
            this.UserService = UserService;
            this.SysDBTool = SysDBTool;
            this.SupplyHelpService = SupplyHelpService;
            this.AcceptHelpService = AcceptHelpService;
            this.SysSettingService = SysSettingService;
            this.ActLogService = ActLogService;
           
            cacheSysParam = MvcCore.Unity.Get<ISysParamService>().ListCache("sysparams", x => x.ID < 4000).ToList();
        }



        public ActionResult Index()
        {
            return View();
        }

        public ActionResult _PartialAddUser()
        {
            return View();
        }

        [HttpPost]
        public ActionResult _PartialAddUser(FormCollection fc)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                var entity = new Data.User();
                TryUpdateModel(entity, fc.AllKeys);
                if (!Regex.IsMatch(entity.UserName, @"^[A-Za-z0-9_]+$")) throw new Exception("用户名只能为字母、数字和下划线");
                //if (fc["password"] != fc["confirmpassword"]) throw new Exception("一级密码与确认密码不相符");
                //if (fc["password2"] != fc["confirmpassword2"]) throw new Exception("二级密码与确认密码不相符");
                if (string.IsNullOrEmpty(entity.UserName) || string.IsNullOrEmpty(entity.RealName) || string.IsNullOrEmpty(entity.Mobile)) throw new Exception("会员编号、真实姓名、手机号码不能为空");
                if (string.IsNullOrEmpty(entity.Email)) throw new Exception("电子邮箱不能为空");

                if (string.IsNullOrEmpty(entity.Password) || string.IsNullOrEmpty(entity.Password2)) throw new Exception("一级密码、二级密码不能为空");
                if (UserService.List(x => x.UserName == entity.UserName.Trim()).Count() > 0) throw new Exception("用户名已被使用");
                //if (string.IsNullOrEmpty(entity.BankCard) || string.IsNullOrEmpty(entity.BankUser) || string.IsNullOrEmpty(entity.BankOfDeposit)) throw new Exception("请输入银行卡信息");
                //if (entity.RealName != entity.BankUser) throw new Exception("真实姓名与银行卡户名不相符");
                if (UserService.List(x => x.UserName == entity.RefereeUser && x.IsActivation).Count() <= 0) throw new Exception("推荐人不存在或未激活");

                if (ConfigHelper.GetConfigString("MemberAtlas") != "sun")
                {
                    if (UserService.List(x => x.UserName == entity.ParentUser && x.IsActivation).Count() <= 0) throw new Exception("安置人不存在或未激活");
                }
                if (ConfigHelper.GetConfigBool("HaveAgent"))
                {
                    if (UserService.List(x => x.AgentName == entity.AgentUser && x.IsActivation).Count() <= 0) throw new Exception("商务中心不存在");
                    var agentUser = UserService.Single(x => x.AgentName == entity.AgentUser);
                    entity.AgentID = agentUser.ID;
                }

                Data.User _parentUser = null;
                if (ConfigHelper.GetConfigString("MemberAtlas") == "double")
                {
                    _parentUser = UserService.Single(x => x.UserName == entity.ParentUser);
                    if (_parentUser != null)
                    {
                        if (UserService.List(x => x.ParentID == _parentUser.ID).Count() >= 2) throw new Exception("安置人安置名额已满");
                    }
                    if (entity.ChildPlace > 2 || entity.ChildPlace < 1) throw new Exception("安置参数不正确");
                    if (UserService.List(x => x.ParentUser == entity.ParentUser && x.ChildPlace == entity.ChildPlace).Count() > 0) throw new Exception("当前位置无法安置");
                }
                else if (ConfigHelper.GetConfigString("MemberAtlas") == "three")
                {
                    _parentUser = UserService.Single(x => x.UserName == entity.ParentUser);
                    if (_parentUser != null)
                    {
                        if (UserService.List(x => x.ParentID == _parentUser.ID).Count() >= 3) throw new Exception("安置人安置名额已满");
                    }

                    if (entity.ChildPlace > 3 || entity.ChildPlace < 1) throw new Exception("安置参数不正确");
                    if (UserService.List(x => x.ParentUser == entity.ParentUser && x.ChildPlace == entity.ChildPlace).Count() > 0) throw new Exception("当前位置无法安置");
                }
                else
                    _parentUser = UserService.Single(x => x.UserName == entity.RefereeUser);

                entity.IsActivation = false;
                entity.IsAgent = false;
                entity.IsLock = false;
                entity.Investment = 0;

                //节点部分
                var _refereeUser = UserService.Single(x => x.UserName == entity.RefereeUser);

                //if (_parentUser.RootID != _refereeUser.RootID || Umodel.RootID != _parentUser.RootID) throw new Exception("推荐人和安置人以及您自己必须同一网内会员");
                entity.RefereeDepth = _refereeUser.RefereeDepth + 1;
                entity.RefereePath = _refereeUser.RefereePath + "," + _refereeUser.ID;
                entity.RefereeID = _refereeUser.ID;
                entity.RefereeUser = _refereeUser.UserName;

                //节点部分
                entity.ParentID = _parentUser.ID;
                entity.ParentUser = _parentUser.UserName;
                entity.RootID = _parentUser.RootID;
                entity.Depth = _parentUser.Depth + 1;
                entity.ParentPath = _parentUser.ParentPath + "," + _parentUser.ID;
                entity.Child = 0;

                entity.Password = entity.Password.ToMD5().ToMD5();
                entity.Password2 = entity.Password2.ToMD5().ToMD5();
                entity.CreateTime = DateTime.Now;
                entity.CreateBy = "Admin";
                entity.Wallet2001 = 0;
                entity.Wallet2002 = 0;
                entity.Wallet2003 = 0;
                entity.Wallet2004 = 0;
                entity.Wallet2005 = 0;
                entity.AgentLevel = 0;
                entity.UserLevel = 0;


                if (ConfigHelper.GetConfigString("MemberAtlas") == "double")
                    entity.DepthSort = (_parentUser.DepthSort - 1) * 2 + entity.ChildPlace;
                else if (ConfigHelper.GetConfigString("MemberAtlas") == "three")
                    entity.DepthSort = (_parentUser.DepthSort - 1) * 3 + entity.ChildPlace;
                else
                {
                    entity.DepthSort = 0;
                    entity.ChildPlace = UserService.List(x => x.ParentID == _parentUser.ID).Count() > 0 ? UserService.List(x => x.ParentID == _parentUser.ID).Max(x => x.ChildPlace) + 1 : 1;
                }


                using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
                {
                    UserService.Add(entity);
                    _parentUser.Child = _parentUser.Child + 1;
                    UserService.Update(_parentUser);
                    SysDBTool.Commit();
                    if (!entity.IsActivation) Users.ActivationAccount(entity);
                    ts.Complete();
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

        public ActionResult _PartialSubmitSupplyHelp()
        {
            return View();
        }

        #region 提供帮助
        [HttpPost]
        public ActionResult _PartialSubmitSupplyHelp(FormCollection fc)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                string SupplyAmount = fc["supplyamount"];
                string PayWay = fc["payway"];
                string uID = fc["uid"];
                var Umodel = UserService.Single(uID.ToInt());

                if (string.IsNullOrEmpty(Umodel.BankCard) && string.IsNullOrEmpty(Umodel.WeiXin) && string.IsNullOrEmpty(Umodel.AliPay))
                    throw new Exception("您还未填写任何一个收款帐号（银行卡、支付宝、微信），请到“帐号管理”处修改个人资料！");
                if (string.IsNullOrEmpty(PayWay)) throw new Exception("请选择付款方式！");
                if (SupplyAmount.ToDecimal() <= 0) throw new Exception("请您填写提供帮助的金额！");
                //if (SupplyHelpService.List(x => x.UID == Umodel.ID && x.Status < (int)Data.Enum.HelpStatus.PartOfMatching && x.Status > 0).Count() > 0)
                //    throw new Exception("对不起，你有一单提供帮助没有匹配成功，无法提供帮助！");

                decimal ExchangeAmount = SupplyAmount.ToDecimal() * cacheSysParam.SingleAndInit(x => x.ID == 3801).Value.ToDecimal();　//汇率参数
                decimal minmoney = cacheSysParam.SingleAndInit(x => x.ID == 3001).Value.Split('-')[0].ToDecimal();　//提供帮助金额限制参数(基准最小)
                decimal maxmoney = cacheSysParam.SingleAndInit(x => x.ID == 3001).Value.Split('-')[1].ToDecimal();　//提供帮助金额限制参数(基准最大)

                //每一轮增加5000，最高60000
                int tgcount = SupplyHelpService.List(x => x.UID == Umodel.ID && x.Status >= (int)Data.Enum.HelpStatus.AllDeal).Count(); //已经完成的提供帮助次数
                decimal maxsupplymoney = cacheSysParam.SingleAndInit(x => x.ID == 3001).Value3.ToDecimal(); //充许提供帮助的最大金额60000
                decimal stepmoney = cacheSysParam.SingleAndInit(x => x.ID == 3001).Value2.ToDecimal(); //每完成一次提供可累加金额
                maxmoney = Math.Min(maxsupplymoney, (tgcount * stepmoney + maxmoney));

                int _maxmonthsupplycount = cacheSysParam.SingleAndInit(x => x.ID == 3805).Value.ToInt(); //每月最多可提供订单
                if (_maxmonthsupplycount > 0)
                {
                    if (SupplyHelpService.List(x => x.UID == Umodel.ID && x.Status > 0 && SqlFunctions.DateDiff("month", x.CreateTime, DateTime.Now) == 0).Count() > _maxmonthsupplycount)
                        throw new Exception("对不起，每月最多只可提供帮助次数为：" + _maxmonthsupplycount + "次");
                }

                int _nextsupplytime = cacheSysParam.SingleAndInit(x => x.ID == 3807).Value.ToInt(); //下次提供帮助间隔时间
                if (_nextsupplytime > 0)
                {
                    if (SupplyHelpService.List(x => x.UID == Umodel.ID && x.Status > 0 && SqlFunctions.DateDiff("minute", x.CreateTime, DateTime.Now) < _nextsupplytime).Count() > 0)
                        throw new Exception("对不起，下次提供帮助的需在：" + _nextsupplytime + "分钟以后");
                }

                //个人日排单限制
                var mytodaysupplylist = SupplyHelpService.List(x => x.UID == Umodel.ID && x.Status > 0 && SqlFunctions.DateDiff("day", x.CreateTime, DateTime.Now) == 0);
                decimal mytodaysupplymoney = mytodaysupplylist.Count() > 0 ? mytodaysupplylist.Sum(x => x.ExchangeAmount) : 0;
                if (ExchangeAmount < minmoney) throw new Exception("对不起，提供帮助金额不能少于" + minmoney + "");
                if ((mytodaysupplymoney + ExchangeAmount) > maxmoney) throw new Exception("对不起，今日提供帮助金额不能大于" + maxmoney + "");

                int beisu = cacheSysParam.SingleAndInit(x => x.ID == 3005).Value.ToInt(); //金额倍数
                if (ExchangeAmount % beisu != 0) throw new Exception("金额必须是" + beisu + "的倍数！");

                //系统日排单上限
                decimal PARAM_SYSPDSX = cacheSysParam.SingleAndInit(x => x.ID == 3006).Value.ToDecimal();
                if (PARAM_SYSPDSX > 0)
                {
                    var todaysupplylist = SupplyHelpService.List(x => x.Status > 0 && SqlFunctions.DateDiff("day", x.CreateTime, DateTime.Now) == 0);
                    decimal todaysupplymoney = todaysupplylist.Count() > 0 ? todaysupplylist.Sum(x => x.ExchangeAmount) : 0;
                    if ((todaysupplymoney + ExchangeAmount) > PARAM_SYSPDSX) throw new Exception("对不起，已超出系统今日系统排单上限，请明天再排单");
                }

                using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
                {
                    decimal PARAM_FIRSTBL = cacheSysParam.SingleAndInit(x => x.ID == 3105).Value.ToDecimal();
                    decimal firstMoney = ExchangeAmount * PARAM_FIRSTBL;


                    var model = new Data.SupplyHelp();
                    model.UID = Umodel.ID;
                    model.UserName = Umodel.UserName;
                    model.SupplyAmount = SupplyAmount.ToDecimal(); //申请金额
                    model.ExchangeAmount = ExchangeAmount - firstMoney; //汇率金额
                    model.CreateTime = DateTime.Now;
                    model.Status = 1;  //状态
                    model.IsTop = false;  //是否置顶
                    model.IsRepeatQueuing = false; //是否重新排队
                    model.HaveMatchingAmount = 0; //已匹配数量
                    model.HaveAcceptAmount = 0; //
                    model.PayWay = PayWay;  //付款方式
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
                    model.OrderMoney = ExchangeAmount;
                    SupplyHelpService.Add(model);
                    SysDBTool.Commit();


                    Bonus.Bonus1103("");//计算管理奖
                    //Bonus.Bonus1104(model.ID); //计算经理奖


                    var model2 = model.ToModel<Data.SupplyHelp>();　//副单
                    model2.OrderType = 0;
                    model2.MainSupplyID = model.ID;
                    model2.SupplyNo = SupplyHelps.GetSupplyNo2();  //单号
                    model2.ExchangeAmount = firstMoney; //汇率金额
                    model2.OrderMoney = ExchangeAmount;
                    SupplyHelpService.Add(model2);
                    SysDBTool.Commit();

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

        public ActionResult _PartialSubmitAcceptHelp()
        {
            return View();
        }

        #region 接受帮助
        [HttpPost]
        public ActionResult _PartialSubmitAcceptHelp(FormCollection fc)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                string PayWay = fc["payway"];
                decimal AcceptAmount = fc["acceptamount"].ToDecimal();
                int CoinID = fc["coinid"].ToInt();
                string uID = fc["uid"];
                var Umodel = UserService.Single(uID.ToInt());

                decimal acceptWallet = 0;
                if (CoinID == 2001)
                {
                    acceptWallet = Umodel.Wallet2001;
                    if (Umodel.Wallet2001Lock ?? false) throw new Exception("你的钱包已被冻结，请联系管理员！");
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

                //if (SupplyHelpService.List(x => x.Status > 0 && x.UID == Umodel.ID && SqlFunctions.DateDiff("DAY", x.CreateTime, DateTime.Now) == 0).Count() > 0)
                //    throw new Exception("提供帮助24小时后才可以接受帮助！");

                decimal ExchangeAmount = AcceptAmount * cacheSysParam.SingleAndInit(x => x.ID == 3801).Value.ToDecimal(); //汇率
                if (acceptWallet < ExchangeAmount) throw new Exception("你的余额不足！");

                decimal minmoney = cacheSysParam.SingleAndInit(x => x.ID == 3002).Value.Split('-')[0].ToDecimal();
                decimal maxmoney = cacheSysParam.SingleAndInit(x => x.ID == 3002).Value.Split('-')[1].ToDecimal();
                if (CoinID == 2001 && ExchangeAmount < minmoney || ExchangeAmount > maxmoney)
                    throw new Exception("接受金额需在" + minmoney + "~" + maxmoney + "之间！");
                else if (CoinID == 2003)
                {
                    minmoney = cacheSysParam.SingleAndInit(x => x.ID == 3003).Value.Split('-')[0].ToDecimal();
                    maxmoney = cacheSysParam.SingleAndInit(x => x.ID == 3003).Value.Split('-')[1].ToDecimal();
                    if (ExchangeAmount < minmoney || ExchangeAmount > maxmoney) throw new Exception("接受金额需在" + minmoney + "~" + maxmoney + "之间！");
                }
                else if (CoinID == 2002)
                {
                    minmoney = cacheSysParam.SingleAndInit(x => x.ID == 3004).Value.Split('-')[0].ToDecimal();
                    maxmoney = cacheSysParam.SingleAndInit(x => x.ID == 3004).Value.Split('-')[1].ToDecimal();
                    if (ExchangeAmount < minmoney || ExchangeAmount > maxmoney) throw new Exception("接受金额需在" + minmoney + "~" + maxmoney + "之间！");
                }

                int beisu = cacheSysParam.SingleAndInit(x => x.ID == 3005).Value.ToInt();
                if (ExchangeAmount % beisu != 0) throw new Exception("金额必须是" + beisu + "的倍数！");

                //系统日排单上限
                decimal PARAM_SYSPDSX = cacheSysParam.SingleAndInit(x => x.ID == 3006).Value2.ToDecimal();
                if (PARAM_SYSPDSX > 0)
                {
                    var todayacceptlist = SupplyHelpService.List(x => x.Status > 0 && SqlFunctions.DateDiff("day", x.CreateTime, DateTime.Now) == 0);
                    decimal todayacceptmoney = todayacceptlist.Count() > 0 ? todayacceptlist.Sum(x => x.ExchangeAmount) : 0;
                    if ((todayacceptmoney + ExchangeAmount) > PARAM_SYSPDSX) throw new Exception("对不起，已超出系统今日系统排单上限，请明天再排单");
                }

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
        public ActionResult List(int? page)
        {
            //动态构建查询
            var list = UserService.List().WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).OrderByDescending(x => x.ID).ToList();
            string isactivation = Request["isactivation"];
            if (!string.IsNullOrEmpty(isactivation))
            {
                bool isactive = (isactivation == "1");
                list = list.Where(x => x.IsActivation == isactive && x.IsLock == false).ToList();
            }

            if (Request["IsExport"] == "1")
            {
                string FileName = string.Format("{0}_{1}_{2}_{3}", DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute);
                MvcCore.Extensions.ExcelHelperV2.ToExcel(list.ToList()).SaveToExcel(Server.MapPath("/upfile/" + FileName + ".xls"));
                return File(Server.MapPath("/upfile/" + FileName + ".xls"), "application/ms-excel", FileName + ".xls");
            }
            return View(list.ToList().ToPagedList(page ?? 1, 20));
        }

        /// <summary>
        /// 被冻结的会员
        /// </summary>
        /// <returns></returns>
        public ActionResult Lock(int? page)
        {
            ActMessage = "被冻结的会员";
            var list = UserService.List(x => x.IsLock).WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).OrderByDescending(x => x.ID).ToList();
            if (Request["IsExport"] == "1")
            {
                string FileName = string.Format("{0}_{1}_{2}_{3}", DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute);
                MvcCore.Extensions.ExcelHelperV2.ToExcel(list.ToList()).SaveToExcel(Server.MapPath("/upfile/" + FileName + ".xls"));
                return File(Server.MapPath("/upfile/" + FileName + ".xls"), "application/ms-excel", FileName + ".xls");
            }
            return View(list.ToPagedList(page ?? 1, 20));
        }

        public ActionResult Modify(int? id)
        {
            if (id.HasValue)
            {
                ActMessage = "编辑会员资料";
                return View(UserService.Single(id));
            }
            else
            {
                ActMessage = "新增初始会员";
                return View(new Data.User());
            }
        }

        [HttpPost]
        public ActionResult Modify(FormCollection fc)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                var entity = UserService.SingleAndInit(fc["ID"].ToInt());
                var onUser = entity.ToModel<Data.User>();
                TryUpdateModel(entity, fc.AllKeys);

                if (entity.ID > 0)
                {
                    if (string.IsNullOrEmpty(entity.UserName) || string.IsNullOrEmpty(entity.RealName) || string.IsNullOrEmpty(entity.Mobile)) throw new Exception("会员编号、真实姓名、联系电话不能为空");
                    if (string.IsNullOrEmpty(entity.BankCard) || string.IsNullOrEmpty(entity.BankUser) || string.IsNullOrEmpty(entity.BankOfDeposit)) throw new Exception("请输入银行卡信息");
                    if (entity.RealName != entity.BankUser) throw new Exception("真实姓名与银行卡户名不相符");
                    string resetpwd = fc["resetpassowrd"];
                    if (!string.IsNullOrEmpty(resetpwd))
                    {
                        entity.Password = resetpwd.ToMD5().ToMD5();
                    }
                    string resetpwd2 = fc["resetpassowrd2"];
                    if (!string.IsNullOrEmpty(resetpwd2))
                    {
                        entity.Password2 = resetpwd2.ToMD5().ToMD5();
                    }

                    string msg = "";
                    if (onUser.Mobile != entity.Mobile) msg += "手机变更：" + onUser.Mobile + " => " + entity.Mobile;
                    if (onUser.RealName != entity.RealName) msg += "　姓名变更：" + onUser.RealName + " => " + entity.RealName;
                    if (onUser.AliPay != entity.AliPay) msg += "　支付宝变更：" + onUser.AliPay + " => " + entity.AliPay;
                    if (onUser.BankCard != entity.BankCard) msg += "　银行卡变更：" + onUser.BankCard + " => " + entity.BankCard;

                    var wlog2 = new Data.WarningLog();
                    wlog2.CreateTime = DateTime.Now;
                    wlog2.IP = Request.UserHostAddress;
                    if (Request.UrlReferrer != null)
                        wlog2.Location = Request.UrlReferrer.ToString();
                    wlog2.Platform = "后台";
                    wlog2.WarningMsg = "由管理员“" + Amodel.AdminName + "”修改会员资料" + (!string.IsNullOrEmpty(msg) ? ",涉及改动信息：" + msg : "");
                    wlog2.WarningLevel = "一般";
                    wlog2.ResultMsg = "通过";
                    wlog2.UserName = entity.UserName;
                    MvcCore.Unity.Get<IWarningLogService>().Add(wlog2);
                    SysDBTool.Commit();

                    UserService.Update(entity);
                }
                else
                {
                    if (!Regex.IsMatch(entity.UserName, @"^[A-Za-z0-9_]+$")) throw new Exception("用户名只能为字母、数字和下划线");
                    if (string.IsNullOrEmpty(entity.UserName) || string.IsNullOrEmpty(entity.RealName) || string.IsNullOrEmpty(entity.Mobile)) throw new Exception("会员编号、真实姓名、联系电话不能为空");
                    if (string.IsNullOrEmpty(entity.Password) || string.IsNullOrEmpty(entity.Password2)) throw new Exception("登录密码、支付密码不能为空");
                    if (string.IsNullOrEmpty(entity.BankCard) || string.IsNullOrEmpty(entity.BankUser) || string.IsNullOrEmpty(entity.BankOfDeposit)) throw new Exception("请输入银行卡信息");
                    if (UserService.List(x => x.UserName == entity.UserName.Trim()).Count() > 0) throw new Exception("会员编号已被使用");

                    entity.IsActivation = true;
                    entity.IsAgent = true;
                    entity.AgentLevel = 2;
                    entity.AgentName = entity.UserName;
                    entity.IsLock = false;
                    entity.ActivationTime = DateTime.Now;
                    //节点部分
                    entity.RootID = 0;
                    entity.ParentID = 0;
                    entity.ParentUser = "";
                    entity.ParentPath = ",0";
                    entity.Depth = 0;
                    entity.DepthSort = 1;
                    entity.Child = 0;
                    entity.ChildPlace = 1;

                    entity.RefereeID = 0;
                    entity.RefereeUser = "";
                    entity.RefereeDepth = 0;
                    entity.RefereePath = ",0";

                    entity.Investment = 0;
                    entity.UserLevel = (int)Data.Enum.UserLeve.Level1;

                    entity.Password = entity.Password.ToMD5().ToMD5();
                    entity.Password2 = entity.Password2.ToMD5().ToMD5();
                    entity.CreateTime = DateTime.Now;
                    UserService.Add(entity);
          
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

        /// <summary>
        /// 删除用户，未激活时才可以
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Delete(int id)
        {
            var model = UserService.Single(id);
            if (model != null)
            {
                if (!model.IsActivation)
                {
                    UserService.Delete(id);
                    SysDBTool.Commit();
                    ViewBag.SuccessMsg = "删除成功！";
                    ActMessage = "删除会员“" + model.UserName + "”";
                    return View("Success");
                }
                else
                {
                    ViewBag.ErrorMsg = "该会员已被激活，无法删除。";
                    return View("Error");
                }
            }
            ViewBag.ErrorMsg = "ID不存在或已被删除。";
            return View("Error");
        }

        public ActionResult doCommand(int id, string commandtype)
        {
            var model = UserService.Single(id);
            if (commandtype.ToLower() == "lock")
            {
                model.IsLock = true;
                model.LockReason = Request["reason"];
            }
            else if (commandtype.ToLower() == "unlock")
                model.IsLock = false;
            else if (commandtype.ToLower() == "resetinputwrong")
                model.InputWrongPwdTimes = 0;
            else if (commandtype.ToLower() == "walletlock2001")
                model.Wallet2001Lock = true;
            else if (commandtype.ToLower() == "unwalletlock2001")
                model.Wallet2001Lock = false;
            UserService.Update(model);
            SysDBTool.Commit();
            ViewBag.SuccessMsg = "操作成功！";
            return View("Success");
        }


        /// <summary>
        /// 会员帐号冻结
        /// </summary>
        /// <param name="id"></param>
        /// <param name="reason"></param>
        /// <returns></returns>
        public ActionResult MakeLock(int id, string reason)
        {
            var model = UserService.Single(id);
            if (model != null)
            {
                model.IsLock = true;
                model.LockReason = reason;
                UserService.Update(model);
                SysDBTool.Commit();
                //对该会员的所有提供单和接受单进行取消处理
                var supplylist = MvcCore.Unity.Get<ISupplyHelpService>().List(x => x.UID == id && x.Status == 1);
                foreach (var item in supplylist)
                    MMM.CancelSupplyHelp(item.SupplyNo, "因帐号冻结被自动取消");

                var acceptlylist = MvcCore.Unity.Get<IAcceptHelpService>().List(x => x.UID == id && x.Status == 1);
                foreach (var item in acceptlylist)
                    MMM.CancelAcceptHelp(item.AcceptNo, "因帐号冻结被自动取消");

                return Content("ok");
            }
            return Content("Error");
        }

        /// <summary>
        /// 设置会员级别
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult MakeLevel(int id, string level)
        {
            var model = UserService.Single(id);
            if (model != null)
            {
                model.UserLevel = level.ToInt();
                if (model.UserLevel > 7) model.UserLevel = 1;
                if (model.UserLevel < 0) model.UserLevel = 1;
                UserService.Update(model);
                SysDBTool.Commit();
                ActPacket = model;
                return Content("ok");
            }
            return Content("Error");
        }
        #region 双轨视图
        [HttpPost]
        public ActionResult DoubleTrack(FormCollection form)
        {
            string kv = form["kv"];

            if (UserService.List(x => x.UserName == kv).Count() > 0)
            {
                return RedirectToAction("DoubleTrack", new { ID = UserService.Single(x => x.UserName == kv).ID });
            }
            else
            {
                ViewBag.ErrorMsg = "查询的用户不存在。";
                return View("Error");
            }
        }
        /// <summary>
        /// 双轨视图
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult DoubleTrack(int id)
        {
            ActMessage = "会员双轨视图";
            var model = UserService.Single(id);
            string keyword = Request["kv"];
            if (Request["kf"] == "username" && !string.IsNullOrEmpty(keyword))
            {
                var sUser = UserService.Single(x => x.UserName == keyword);
                if (sUser != null) return Redirect(Url.Action("DoubleTrack", new { ID = sUser.ID }));
            }
            else if (Request["kf"] == "mobile" && !string.IsNullOrEmpty(keyword))
            {
                var sUser = UserService.Single(x => x.Mobile == keyword);
                if (sUser != null) return Redirect(Url.Action("DoubleTrack", new { ID = sUser.ID }));
            }
            ViewBag.Title = "“" + model.UserName + "”会员双轨视图";
            ActMessage = ViewBag.Title;

            var lst = new List<Data.User>();
            if (model != null)
            {
                lst.Add(model);

                //第一层
                var U11 = UserService.Single(x => x.ParentID == model.ID && x.ChildPlace == 1);
                if (U11 == null) U11 = new Data.User { ID = -1, ParentID = model.ID, ParentUser = model.UserName, AgentUser = model.AgentUser };
                lst.Add(U11);

                var U12 = UserService.Single(x => x.ParentID == model.ID && x.ChildPlace == 2);
                if (U12 == null) U12 = new Data.User { ID = -1, ParentID = model.ID, ParentUser = model.UserName, AgentUser = model.AgentUser };
                lst.Add(U12);


                //第二层
                var U21 = UserService.Single(x => x.ParentID == U11.ID && x.ChildPlace == 1);
                if (U21 == null) U21 = new Data.User { ID = -1, ParentID = U11.ID, ParentUser = U11.UserName, AgentUser = U11.AgentUser };
                lst.Add(U21);

                var U22 = UserService.Single(x => x.ParentID == U11.ID && x.ChildPlace == 2);
                if (U22 == null) U22 = new Data.User { ID = -1, ParentID = U11.ID, ParentUser = U11.UserName, AgentUser = U11.AgentUser };
                lst.Add(U22);

                var U23 = UserService.Single(x => x.ParentID == U12.ID && x.ChildPlace == 1);
                if (U23 == null) U23 = new Data.User { ID = -1, ParentID = U12.ID, ParentUser = U12.UserName, AgentUser = U12.AgentUser };
                lst.Add(U23);

                var U24 = UserService.Single(x => x.ParentID == U12.ID && x.ChildPlace == 2);
                if (U24 == null) U24 = new Data.User { ID = -1, ParentID = U12.ID, ParentUser = U12.UserName, AgentUser = U12.AgentUser };
                lst.Add(U24);

                //第三层
                var U31 = UserService.Single(x => x.ParentID == U21.ID && x.ChildPlace == 1);
                if (U31 == null) U31 = new Data.User { ID = -1, ParentID = U21.ID, ParentUser = U21.UserName, AgentUser = U21.AgentUser };
                lst.Add(U31);

                var U32 = UserService.Single(x => x.ParentID == U21.ID && x.ChildPlace == 2);
                if (U32 == null) U32 = new Data.User { ID = -1, ParentID = U21.ID, ParentUser = U21.UserName, AgentUser = U21.AgentUser };
                lst.Add(U32);

                var U33 = UserService.Single(x => x.ParentID == U22.ID && x.ChildPlace == 1);
                if (U33 == null) U33 = new Data.User { ID = -1, ParentID = U22.ID, ParentUser = U22.UserName, AgentUser = U22.AgentUser };
                lst.Add(U33);

                var U34 = UserService.Single(x => x.ParentID == U22.ID && x.ChildPlace == 2);
                if (U34 == null) U34 = new Data.User { ID = -1, ParentID = U22.ID, ParentUser = U22.UserName, AgentUser = U22.AgentUser };
                lst.Add(U34);

                var U35 = UserService.Single(x => x.ParentID == U23.ID && x.ChildPlace == 1);
                if (U35 == null) U35 = new Data.User { ID = -1, ParentID = U23.ID, ParentUser = U23.UserName, AgentUser = U23.AgentUser };
                lst.Add(U35);

                var U36 = UserService.Single(x => x.ParentID == U23.ID && x.ChildPlace == 2);
                if (U36 == null) U36 = new Data.User { ID = -1, ParentID = U23.ID, ParentUser = U23.UserName, AgentUser = U23.AgentUser };
                lst.Add(U36);

                var U37 = UserService.Single(x => x.ParentID == U24.ID && x.ChildPlace == 1);
                if (U37 == null) U37 = new Data.User { ID = -1, ParentID = U24.ID, ParentUser = U24.UserName, AgentUser = U24.AgentUser };
                lst.Add(U37);

                var U38 = UserService.Single(x => x.ParentID == U24.ID && x.ChildPlace == 2);
                if (U38 == null) U38 = new Data.User { ID = -1, ParentID = U24.ID, ParentUser = U24.UserName, AgentUser = U24.AgentUser };
                lst.Add(U38);

                //第四层
                var U41 = UserService.Single(x => x.ParentID == U31.ID && x.ChildPlace == 1);
                if (U41 == null) U41 = new Data.User { ID = -1, ParentID = U31.ID, ParentUser = U31.UserName, AgentUser = U31.AgentUser };
                lst.Add(U41);

                var U42 = UserService.Single(x => x.ParentID == U31.ID && x.ChildPlace == 2);
                if (U42 == null) U42 = new Data.User { ID = -1, ParentID = U31.ID, ParentUser = U31.UserName, AgentUser = U31.AgentUser };
                lst.Add(U42);

                var U43 = UserService.Single(x => x.ParentID == U32.ID && x.ChildPlace == 1);
                if (U43 == null) U43 = new Data.User { ID = -1, ParentID = U32.ID, ParentUser = U32.UserName, AgentUser = U32.AgentUser };
                lst.Add(U43);

                var U44 = UserService.Single(x => x.ParentID == U32.ID && x.ChildPlace == 2);
                if (U44 == null) U44 = new Data.User { ID = -1, ParentID = U32.ID, ParentUser = U32.UserName, AgentUser = U32.AgentUser };
                lst.Add(U44);

                var U45 = UserService.Single(x => x.ParentID == U33.ID && x.ChildPlace == 1);
                if (U45 == null) U45 = new Data.User { ID = -1, ParentID = U33.ID, ParentUser = U33.UserName, AgentUser = U33.AgentUser };
                lst.Add(U45);

                var U46 = UserService.Single(x => x.ParentID == U33.ID && x.ChildPlace == 2);
                if (U46 == null) U46 = new Data.User { ID = -1, ParentID = U33.ID, ParentUser = U33.UserName, AgentUser = U33.AgentUser };
                lst.Add(U46);

                var U47 = UserService.Single(x => x.ParentID == U34.ID && x.ChildPlace == 1);
                if (U47 == null) U47 = new Data.User { ID = -1, ParentID = U34.ID, ParentUser = U34.UserName, AgentUser = U34.AgentUser };
                lst.Add(U47);

                var U48 = UserService.Single(x => x.ParentID == U34.ID && x.ChildPlace == 2);
                if (U48 == null) U48 = new Data.User { ID = -1, ParentID = U34.ID, ParentUser = U34.UserName, AgentUser = U34.AgentUser };
                lst.Add(U48);

                var U49 = UserService.Single(x => x.ParentID == U35.ID && x.ChildPlace == 1);
                if (U49 == null) U49 = new Data.User { ID = -1, ParentID = U35.ID, ParentUser = U35.UserName, AgentUser = U35.AgentUser };
                lst.Add(U49);

                var U50 = UserService.Single(x => x.ParentID == U35.ID && x.ChildPlace == 2);
                if (U50 == null) U50 = new Data.User { ID = -1, ParentID = U35.ID, ParentUser = U35.UserName, AgentUser = U35.AgentUser };
                lst.Add(U50);

                var U51 = UserService.Single(x => x.ParentID == U36.ID && x.ChildPlace == 1);
                if (U51 == null) U51 = new Data.User { ID = -1, ParentID = U36.ID, ParentUser = U36.UserName, AgentUser = U36.AgentUser };
                lst.Add(U51);

                var U52 = UserService.Single(x => x.ParentID == U36.ID && x.ChildPlace == 2);
                if (U52 == null) U52 = new Data.User { ID = -1, ParentID = U36.ID, ParentUser = U36.UserName, AgentUser = U36.AgentUser };
                lst.Add(U52);

                var U53 = UserService.Single(x => x.ParentID == U37.ID && x.ChildPlace == 1);
                if (U53 == null) U53 = new Data.User { ID = -1, ParentID = U37.ID, ParentUser = U37.UserName, AgentUser = U37.AgentUser };
                lst.Add(U53);

                var U54 = UserService.Single(x => x.ParentID == U37.ID && x.ChildPlace == 2);
                if (U54 == null) U54 = new Data.User { ID = -1, ParentID = U37.ID, ParentUser = U37.UserName, AgentUser = U37.AgentUser };
                lst.Add(U54);

                var U55 = UserService.Single(x => x.ParentID == U38.ID && x.ChildPlace == 1);
                if (U55 == null) U55 = new Data.User { ID = -1, ParentID = U38.ID, ParentUser = U38.UserName, AgentUser = U38.AgentUser };
                lst.Add(U55);

                var U56 = UserService.Single(x => x.ParentID == U38.ID && x.ChildPlace == 2);
                if (U56 == null) U56 = new Data.User { ID = -1, ParentID = U38.ID, ParentUser = U38.UserName, AgentUser = U38.AgentUser };
                lst.Add(U56);

                return View(lst);
            }
            ViewBag.ErrorMsg = "记录不存在或已被删除！";
            return View("Error");
        }
        #endregion

        #region 推荐树视图
        /// <summary>
        /// 树状视图
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult TreeView(int id)
        {
            var model = UserService.Single(id);
            ActMessage = "会员推荐图谱";
            if (model == null)
            {
                ViewBag.ErrorMsg = "记录不存在或已被删除！";
                return View("Error");
            }
            return View(model);
        }

        /// <summary>
        /// 获取树状节点数据json格式
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public JsonResult GetTreeJson(int id)
        {
            //int allchild;
            int child;
            IList<TreeNode> list = new List<TreeNode>();

            //取所有父ID与参数相符数据封装到 List<TUser> 并以JSON格式返回
            var ulst = UserService.List(x => x.RefereeID == id).ToList();
            foreach (var mt in ulst)
            {
                child = UserService.List(x => x.RefereeID == mt.ID).Count();
                //child = Users.GetAllRefereeChild(mt, 1).Where(x => x.AddupSupplyAmount > 0).Count();
                //allchild = users.GetRecordCount("ParentPath like '%," + mt.ID.ToString() + ",%' ") + child;
                TreeNode p = new TreeNode();
                p.id = mt.ID;
                if (mt.IsActivation)
                {
                    if (mt.IsAgent ?? false)
                        p.text = "<i style='color:#f00'><a href=\"/UserCenter/Home/Index?adminloginname=" + mt.UserName + "\" target=\"_blank\">" + mt.UserName + "</a> (经理" + mt.RealName + ",有效直推" + child + "人)</i>  <a href=\"javascript: void(0);\" onclick=\"javascript: winopen('注册会员', '" + Url.Action("_PartialAddUser", new { parentuser = mt.UserName, refereeuser = mt.UserName, agentuser = mt.AgentUser, childplace = 1 }) + "', 500); \">注册</a> <a href=\"javascript: void(0);\" onclick=\"javascript: winopen('提供帮助(" + mt.UserName + ")', '" + Url.Action("_PartialSubmitSupplyHelp", new { uid = mt.ID }) + "', 500); \">提供(" + SupplyHelpService.List(x => x.UID == mt.ID && x.Status > 0).Count() + ")</a> <a href=\"javascript: void(0);\" onclick=\"javascript: winopen('接受帮助(" + mt.UserName + ")', '" + Url.Action("_PartialSubmitAcceptHelp", new { uid = mt.ID }) + "', 500); \">接受(" + AcceptHelpService.List(x => x.UID == mt.ID && x.Status > 0).Count() + ")</a>";
                    else
                        p.text = "<a href=\"/UserCenter/Home/Index?adminloginname=" + mt.UserName + "\" target=\"_blank\">" + mt.UserName + "</a> (" + mt.RealName + ",有效直推" + child + "人) <a href=\"javascript: void(0);\" onclick=\"javascript: winopen('注册会员', '" + Url.Action("_PartialAddUser", new { parentuser = mt.UserName, refereeuser = mt.UserName, agentuser = mt.AgentUser, childplace = 1 }) + "', 500); \">注册</a> <a href=\"javascript: void(0);\" onclick=\"javascript: winopen('提供帮助(" + mt.UserName + ")', '" + Url.Action("_PartialSubmitSupplyHelp", new { uid = mt.ID }) + "', 500); \">提供(" + SupplyHelpService.List(x => x.UID == mt.ID && x.Status > 0).Count() + ")</a> <a href=\"javascript: void(0);\" onclick=\"javascript: winopen('接受帮助(" + mt.UserName + ")', '" + Url.Action("_PartialSubmitAcceptHelp", new { uid = mt.ID }) + "', 500); \">接受(" + AcceptHelpService.List(x => x.UID == mt.ID && x.Status > 0).Count() + ")</a>";
                }
                else
                    p.text = "<em style='color:#999'>" + mt.UserName + "(未激活)</em>";

                if (mt.RefereeID == 0)
                {
                    p.type = "root";
                }

                if (child > 0)
                {
                    p.icon = "fa fa-users";
                    p.children = true;
                }
                else
                {
                    p.icon = "fa fa-user";
                    //p.state = "disabled=true";
                    p.children = false;
                }
                list.Add(p);
            }
            return Json(list, JsonRequestBehavior.AllowGet);
        }
    }
    #endregion
}


using JN.Data.Service;
using JN.Services.Tool;
/**
* Member.cs
*
* 功 能： N/A
* 类 名： Member
*
* Ver    变更日期             负责人  变更内容
* ───────────────────────────────────
* V0.01  2014/9/30            N/A      初版
*
* Copyright (c) 2012 GxBlessing Corporation. All rights reserved.
*┌──────────────────────────────────┐
*│　此技术信息为本公司机密信息，未经本公司书面同意禁止向第三方披露．　│
*│　版权所有：www.yinkee.net　　　　　　　　　　　　                　│
*└──────────────────────────────────┘
*/
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
namespace JN.Services.Manager
{
    public partial class MMM
    {


        #region 利息发放过程（手动结算）
        /// <summary>
        /// 利息发放
        /// </summary>
        /// <param name="balancemode">发放模式，手动/自动</param>
        public static void CalculateFLX(
            IUserService UserService,
            IWalletLogService WalletLogService,
            IBonusDetailService BonusDetailService,
            ISettlementService SettlementService,
            ISupplyHelpService SupplyHelpService,
            ISysDBTool SysDBTool,
            List<Data.SysParam> cacheSysParam,
            int balancemode)
        {
         
            int period = SettlementService.List().Count() > 0 ? SettlementService.List().Max(x => x.Period) + 1 : 1;  //分红总期数
            var param = cacheSysParam.SingleAndInit(x => x.ID == 1102);
            int param_day=param.Value.ToInt();
            //统计所有未进行结算的投资
            //int PARAM_LXDAY = param.Value.ToInt();
            var supplylist = SupplyHelpService.List(x => x.IsAccruaCount && x.SurplusAccrualDay > 0 && x.Status > 0 && x.OrderType == 1 && x.ReserveInt2 == 1 && x.AccrualDay < param_day).ToList();  //分单的第一单发利息
            //supplyhelps.GetModelList("IsAccruaCount=1 and AccrualDay<" + PARAM_LXDAY + " and Status>0");
            DataCache.SetCache("TotalRow", supplylist.Count);
            decimal totalLX = 0;
            int j = 1;
            foreach (var item in supplylist)
            {
                var onUser = UserService.Single(item.UID);
                //float PARAM_LXRL = TypeConverter.StrToFloat(sysparams.GetModel(1102).Value2);
                DataCache.SetCache("CurrentRow", j);
                DataCache.SetCache("TransInfo", "正在结算“" + item.SupplyNo + "”提供单利息，用时：" + Tool.DateTimeDiff.DateDiff_Sec(DateTime.Now, (DateTime)DataCache.GetCache("StartTime")) + "秒");

                decimal PARAM_LXRL = param.Value2.ToDecimal();
                //if (onUser.IsAgent == true)
                //{
                //    PARAM_LXRL = param.Value3.ToDecimal();
                //}
                //用户当期利息
                decimal bonusMoney = (item.RoundAmount ?? 0) * PARAM_LXRL;
                //给当前用户的分红
                totalLX += bonusMoney;
                string bonusDesc = "来算订单【" + item.SupplyNo + "】【" + DateTime.Now.ToShortDateString() + "】的利息";
                bool isbalance = false;
                if (item.IsAccrualEffective) isbalance = true; //如果利息已经生效直接进帐户（提供单已经确认收款）
                if (bonusMoney > 0)
                {
                    //是否马上结算
                    if (isbalance)
                    {
                        //写入明细
                        WalletLogService.Add(new Data.WalletLog
                        {
                            ChangeMoney = bonusMoney,
                            Balance = onUser.Wallet2001 + bonusMoney,
                            CoinID = 2001,
                            CoinName = cacheSysParam.Single(x => x.ID == 2001).Name,
                            CreateTime = DateTime.Now,
                            Description = bonusDesc,
                            UID = onUser.ID,
                            UserName = onUser.UserName
                        });
                        SysDBTool.Commit();

                        //更新用户钱包
                        onUser.Wallet2001 = onUser.Wallet2001 + bonusMoney;
                        UserService.Update(onUser);
                        SysDBTool.Commit();
                    }
                    //写入奖金表
                    BonusDetailService.Add(new Data.BonusDetail
                    {
                        Period = param.Value.ToInt() - item.SurplusAccrualDay + 1,
                        BalanceTime = DateTime.Now,
                        BonusMoney = bonusMoney,
                        BonusID = 1102,
                        BonusName = cacheSysParam.Single(x => x.ID == 1102).Name,
                        CreateTime = DateTime.Now,
                        Description = bonusDesc,
                        IsBalance = isbalance,
                        UID = onUser.ID,
                        UserName = onUser.UserName,
                        SupplyNo = item.SupplyNo
                    });
                    SysDBTool.Commit();
                }

                var updateEntity = SupplyHelpService.Single(item.ID);
                updateEntity.AccrualMoney = updateEntity.AccrualMoney + bonusMoney;
                updateEntity.AccrualDay = updateEntity.AccrualDay + 1;
                updateEntity.SurplusAccrualDay = updateEntity.SurplusAccrualDay - 1;
                updateEntity.TotalMoney = updateEntity.ExchangeAmount + updateEntity.AccrualMoney;
                SupplyHelpService.Update(updateEntity);
                SysDBTool.Commit();
                j++;
            }
            SettlementService.Add(new Data.Settlement { BalanceMode = balancemode, CreateTime = DateTime.Now, Period = period, TotalBonus = totalLX, TotalUser = supplylist.Count });
            SysDBTool.Commit();
            DataCache.SetCache("TransInfo", "成功对" + supplylist.Count + "条提供帮助订单发利息，用时：" + Tool.DateTimeDiff.DateDiff_Sec(DateTime.Now, (DateTime)DataCache.GetCache("StartTime")) + "秒");
        }
        #endregion

        #region 利息发放过程（自动结算）
        /// <summary>
        /// 利息发放
        /// </summary>
        /// <param name="balancemode">发放模式，手动/自动</param>
        public static void CalculateFLXZD(int balancemode)
        {
            List<Data.SysParam> cacheSysParam = MvcCore.Unity.Get<JN.Data.Service.ISysParamService>().List(x => x.ID < 4000).ToList();

            int period = MvcCore.Unity.Get<ISettlementService>().List().Count() > 0 ? MvcCore.Unity.Get<ISettlementService>().List().ToList().Max(x => x.Period) + 1 : 1;  //分红总期数
            var param = cacheSysParam.SingleAndInit(x => x.ID == 1102);
            //统计所有未进行结算的投资
            //int PARAM_LXDAY = param.Value.ToInt();
            var supplylist = MvcCore.Unity.Get<ISupplyHelpService>().List(x => x.IsAccruaCount && x.SurplusAccrualDay > 0 && x.Status > 0 && x.OrderType == 1 && x.ReserveInt2==1 && x.AccrualDay<param.Value.ToInt()).ToList();  //分单的第一单发利息
            //supplyhelps.GetModelList("IsAccruaCount=1 and AccrualDay<" + PARAM_LXDAY + " and Status>0");
            decimal totalLX = 0;
            int j = 1;
            foreach (var item in supplylist)
            {
                var onUser = MvcCore.Unity.Get<IUserService>().Single(item.UID);
                decimal PARAM_LXRL = param.Value2.ToDecimal();
                //if (onUser.IsAgent == true)
                //{
                //    PARAM_LXRL = param.Value3.ToDecimal();
                //}
              

               // float PARAM_LXRL = (sysparams.GetModel(1102).Value2);
                //用户当期利息
                //decimal bonusMoney = (item.OrderMoney ?? 0) * (item.AccruaRate ?? 0);
                decimal bonusMoney = (item.RoundAmount ?? 0) * PARAM_LXRL;
                //给当前用户的分红
                totalLX += bonusMoney;
                string bonusDesc = "来算订单【" + item.SupplyNo + "】【" + DateTime.Now.ToShortDateString() + "】的利息";
                bool isbalance = false;
                if (item.IsAccrualEffective) isbalance = true; //如果利息已经生效直接进帐户（提供单已经到解冻时间）
                if (bonusMoney > 0)
                {
                    //是否马上结算
                    if (isbalance)
                    {
                        //写入明细
                        MvcCore.Unity.Get<IWalletLogService>().Add(new Data.WalletLog
                        {
                            ChangeMoney = bonusMoney,
                            Balance = onUser.Wallet2001 + bonusMoney,
                            CoinID = 2001,
                            CoinName = cacheSysParam.Single(x => x.ID == 2001).Name,
                            CreateTime = DateTime.Now,
                            Description = bonusDesc,
                            UID = onUser.ID,
                            UserName = onUser.UserName
                        });
                        MvcCore.Unity.Get<SysDBTool>().Commit();

                        //更新用户钱包
                        onUser.Wallet2001 = onUser.Wallet2001 + bonusMoney;
                        MvcCore.Unity.Get<IUserService>().Update(onUser);
                        MvcCore.Unity.Get<ISysDBTool>().Commit();
                    }
                    //写入奖金表
                    MvcCore.Unity.Get<IBonusDetailService>().Add(new Data.BonusDetail
                    {
                        Period = param.Value.ToInt() - item.SurplusAccrualDay + 1,
                        BalanceTime = DateTime.Now,
                        BonusMoney = bonusMoney,
                        BonusID = 1102,
                        BonusName = cacheSysParam.Single(x => x.ID == 1102).Name,
                        CreateTime = DateTime.Now,
                        Description = bonusDesc,
                        IsBalance = isbalance,
                        UID = onUser.ID,
                        IsEffective = true,
                        EffectiveTime = DateTime.Now,
                        UserName = onUser.UserName,
                        SupplyNo = item.SupplyNo
                    });
                    MvcCore.Unity.Get<ISysDBTool>().Commit();
                }

                var updateEntity = MvcCore.Unity.Get<ISupplyHelpService>().Single(item.ID);
                updateEntity.AccrualMoney = updateEntity.AccrualMoney + bonusMoney;
                updateEntity.AccrualDay = updateEntity.AccrualDay + 1;
                updateEntity.SurplusAccrualDay = updateEntity.SurplusAccrualDay - 1;
                updateEntity.TotalMoney = updateEntity.ExchangeAmount + updateEntity.AccrualMoney;
                MvcCore.Unity.Get<ISupplyHelpService>().Update(updateEntity);
                MvcCore.Unity.Get<ISysDBTool>().Commit();
                j++;
            }
            MvcCore.Unity.Get<ISettlementService>().Add(new Data.Settlement { BalanceMode = balancemode, CreateTime = DateTime.Now, Period = period, TotalBonus = totalLX, TotalUser = supplylist.Count });
            MvcCore.Unity.Get<ISysDBTool>().Commit();
        }
        #endregion

        #region 检查排队时间，排队期结束后进行重排处理
        /// <summary>
        /// 审核提供和接受帮助是否达到15天
        /// </summary>
        public static void CheckQueuing()
        {
           

            //int PARAM_QUEUINGDAY = cacheSysParam.SingleAndInit(x => x.ID == 3103).Value.ToInt(); //排队期参数（以计息周期算）
            ////对未匹配的到期供单重新排队，利息归0
            //Dictionary<string, string> updateParam = new Dictionary<string, string>();
            //updateParam.Add("AccrualMoney", "0");
            //updateParam.Add("AccrualDay", "0");
            //updateParam.Add("IsRepeatQueuing", "1");
            //updateParam.Add("RepeatQueuingTime", DateTime.Now.ToString());
            //updateParam.Add("EndTime", DateTime.Now.AddMinutes(PARAM_QUEUINGDAY).ToString());
            //MvcCore.Unity.Get<ISupplyHelpService>().Update(new Data.SupplyHelp(), updateParam, "Status=" + (int)Data.Enum.HelpStatus.NoMatching + " and EndTime<='" + DateTime.Now.ToString() + "'");
            //MvcCore.Unity.Get<ISysDBTool>().Commit();
            ////supplyhelps.Update("AccrualMoney=0,AccrualDay=0,IsRepeatQueuing=1,RepeatQueuingTime=getdate(),EndTime='" + DateTime.Now.AddMinutes(PARAM_QUEUINGDAY).ToString() + "'", "Status=" + (int)Data.Enum.HelpStatus.NoMatching + " and EndTime<='" + DateTime.Now.ToString() + "'");

            ////对未匹配的到期受单重新排队
            //Dictionary<string, string> updateParam2 = new Dictionary<string, string>();
            //updateParam2.Add("IsRepeatQueuing", "1");
            //updateParam2.Add("RepeatQueuingTime", DateTime.Now.ToString());
            //updateParam2.Add("EndTime", DateTime.Now.AddMinutes(PARAM_QUEUINGDAY).ToString());
            //MvcCore.Unity.Get<IAcceptHelpService>().Update(new Data.AcceptHelp(), updateParam2, "Status=" + (int)Data.Enum.HelpStatus.NoMatching + " and EndTime<='" + DateTime.Now.ToString() + "'");
            //MvcCore.Unity.Get<ISysDBTool>().Commit();
            ////accepthelps.Update("IsRepeatQueuing=1,RepeatQueuingTime=getdate(),EndTime='" + DateTime.Now.AddMinutes(PARAM_QUEUINGDAY).ToString() + "'", "Status=" + (int)Data.Enum.HelpStatus.NoMatching + " and EndTime<='" + DateTime.Now.ToString() + "'");
        }
        #endregion

        #region 检查匹配单是否在付款时限内付款
        /// <summary>
        /// 检查匹配单是否在付款时限内付款，超出侧进行处理
        /// </summary>
        public static void CheckPayEndTime()
        {
            List<Data.SysParam> cacheSysParam = MvcCore.Unity.Get<JN.Data.Service.ISysParamService>().List(x => x.ID < 4000).ToList();

            int PARAM_PAYENDHOUR = cacheSysParam.SingleAndInit(x => x.ID == 1323).Value.ToInt(); //付款时限参数 48小时

            int PARAM_QD = cacheSysParam.SingleAndInit(x => x.ID == 3813).Value.ToInt();  //抢单之后的付款时限
            int PARAM_PAYENDHOUR_DELAYED = cacheSysParam.SingleAndInit(x => x.ID == 3106).Value2.ToInt(); //付款延时参数 48小时
            //找出超时未付款配单(包括超时未付款及延时后超时未付款)
            var matchlist = MvcCore.Unity.Get<IMatchingService>().List(x => (x.Status == (int)Data.Enum.MatchingStatus.UnPaid && SqlFunctions.DateDiff("minute", x.CreateTime, DateTime.Now) > PARAM_PAYENDHOUR) || (x.Status == (int)Data.Enum.MatchingStatus.Delayed && SqlFunctions.DateDiff("minute", x.CreateTime, DateTime.Now) > (PARAM_PAYENDHOUR + PARAM_PAYENDHOUR_DELAYED)) || (x.Status == (int)Data.Enum.MatchingStatus.UnPaid && SqlFunctions.DateDiff("minute", x.CreateTime, DateTime.Now) > PARAM_QD)).ToList();
            foreach (var item in matchlist)
            {
                CancelMatching(item, "提供方超时未付款", true);
              


                //删除奖金，保留本单利息
                var sup = MvcCore.Unity.Get<ISupplyHelpService>().Single(x => x.SupplyNo == item.SupplyNo);
                var otherSup = MvcCore.Unity.Get<ISupplyHelpService>().List(x => x.ReserveStr2 == sup.ReserveStr2 && x.SupplyNo != sup.SupplyNo).FirstOrDefault();
                if (otherSup != null)
                {
                    if (otherSup.Status == 1)//分单的第二单没有匹配，取消
                    {
                        otherSup.Status = (int)JN.Data.Enum.HelpStatus.Cancel;
                        MvcCore.Unity.Get<ISupplyHelpService>().Update(otherSup);
                        MvcCore.Unity.Get<ISysDBTool>().Commit();

                    }
                }
                MvcCore.Unity.Get<IBonusDetailService>().Delete(x => x.SupplyNo == sup.ReserveStr2);
                MvcCore.Unity.Get<ISysDBTool>().Commit();
                var onUser = MvcCore.Unity.Get<IUserService>().Single(item.SupplyUID);
                //对供单用户帐号冻结处理
                onUser.IsLock = true;
                onUser.LockTime = DateTime.Now;
                onUser.LockReason = "超时未付款触发冻结，单号：" + item.MatchingNo + "";
                MvcCore.Unity.Get<IUserService>().Update(onUser);
                MvcCore.Unity.Get<ISysDBTool>().Commit();

                if (MvcCore.Unity.Get<ISysParamService>().SingleAndInit(x => x.ID == 4508).Value == "1")
                    SMSHelper.WebChineseMSM(onUser.Mobile, MvcCore.Unity.Get<ISysParamService>().SingleAndInit(x => x.ID == 4508).Value2.Replace("{SUPPLYNO}", item.SupplyNo));

                var RefereeUser = MvcCore.Unity.Get<IUserService>().Single(onUser.RefereeID);
                if (MvcCore.Unity.Get<ISysParamService>().SingleAndInit(x => x.ID == 4509).Value == "1")
                    SMSHelper.WebChineseMSM(RefereeUser.Mobile, MvcCore.Unity.Get<ISysParamService>().SingleAndInit(x => x.ID == 4509).Value2.Replace("{SUPPLYNO}", item.SupplyNo));
            }
        }

        #endregion

        #region 检查匹配单是否在收款时限内确认
        /// <summary>
        /// 检查匹配单是否在收款时限内确认
        /// </summary>
        public static void CheckVerifiedEndTime()
        {
            List<Data.SysParam> cacheSysParam = MvcCore.Unity.Get<JN.Data.Service.ISysParamService>().List(x => x.ID < 4000).ToList();

            int PARAM_VerifiedENDHOUR = cacheSysParam.SingleAndInit(x => x.ID == 3108).Value.ToInt(); //付款后确认时限参数 48小时
            //找出超时未收款配单(包括超时未付款及延时后超时未付款)
            var matchlist = MvcCore.Unity.Get<IMatchingService>().List(x => x.Status == (int)Data.Enum.MatchingStatus.Paid && SqlFunctions.DateDiff("minute", (x.PayEndTime ?? DateTime.Now), DateTime.Now) > PARAM_VerifiedENDHOUR).ToList();
            //matchings.GetModelList("Status=" + (int)Data.Enum.MatchingStatus.Paid + " and datediff(minute,[PayEndTime],getdate())>" + PARAM_VerifiedENDHOUR);
            foreach (var item in matchlist)
            {
                //对受单用户帐号冻结处理
                //users.Update("IsLock=1,LockTime=getdate(),LockReason='超时未进行确认收款，单号：" + item.MatchingNo + "'", "ID=" + item.AcceptUID);

                //超时未确认自动进行确认
                //提供单会员钱包进帐
                //Wallets.changeWallet(item.SupplyUID, item.MatchAmount, 2001, "确认收款时获得，来自匹配单单：" + item.MatchingNo);
                //结算提供单利息，奖金并更新成交状态
                Bonus.Settlement(item);

                var updateEnitity = MvcCore.Unity.Get<IMatchingService>().Single(item.ID);
                updateEnitity.Status = (int)Data.Enum.MatchingStatus.Verified;
                MvcCore.Unity.Get<IMatchingService>().Update(updateEnitity);
                MvcCore.Unity.Get<ISysDBTool>().Commit();

                var onUser = MvcCore.Unity.Get<IUserService>().Single(item.AcceptUID);
                if (onUser != null)
                {
                    //对受单用户帐号冻结处理
                    onUser.IsLock = true;
                    onUser.LockTime = DateTime.Now;
                    onUser.LockReason = "超时未进行确认收款，单号：" + item.MatchingNo + "";
                    MvcCore.Unity.Get<IUserService>().Update(onUser);
                    MvcCore.Unity.Get<ISysDBTool>().Commit();
                }
            }
        }

        #endregion

        #region 匹配处理
        /// <summary>
        /// 匹配处理，ids和ida都为空时作自动匹配处理
        /// </summary>
        /// <param name="ids">提供订单ID集，“,”间隔</param>
        /// <param name="ida">接受订单ID集，“,”间隔</param>
        public static void Matching(string ids, string ida, ref string outMsg)
        {
            List<Data.SysParam> cacheSysParam = MvcCore.Unity.Get<JN.Data.Service.ISysParamService>().List(x => x.ID < 4000).ToList();

            int matchcount = 0;
            string errmsg = "";

            string supplymobile = ""; //群发手机号（提供方,","分隔）
            string acceptmobile = ""; //群发手机号（接受方,","分隔

            //循环读取所有符合的受单
            var acceptlist = MvcCore.Unity.Get<IAcceptHelpService>().List(x => x.HaveMatchingAmount < x.ExchangeAmount && x.Status < (int)Data.Enum.HelpStatus.AllDeal && x.Status > 0)
                .OrderByDescending(x => x.IsTop)
                .ThenByDescending(x => x.IsRepeatQueuing)
                .ThenBy(x => x.CreateTime).Take(300).ToList();

            if (!string.IsNullOrEmpty(ida)) //指定ID(接受方)
            {
                string[] idas = ida.TrimEnd(',').TrimStart(',').Split(',');
                acceptlist = acceptlist.Where(x => idas.Contains(x.ID.ToString())).ToList();
            }
            if (acceptlist.Count() <= 0) errmsg += "没有符合条件的接受单记录";
            foreach (var acceptModel in acceptlist)
            {
                if (ids == "" && ida == "" && matchcount > cacheSysParam.SingleAndInit(x => x.ID == 3403).Value.ToInt()) break; //自动匹配每次20条

                //循环读取所有符合的供单
                var supplylist = MvcCore.Unity.Get<ISupplyHelpService>().List(x => x.HaveMatchingAmount < x.ExchangeAmount && x.Status < (int)Data.Enum.HelpStatus.AllDeal && x.Status > 0)
                    .OrderByDescending(x => x.IsTop)
                    .ThenByDescending(x => x.IsRepeatQueuing)
                    .ThenBy(x => x.CreateTime).ToList();
                //errmsg += "提取" + supplylist.Count() + "条提供单记录";
                if (string.IsNullOrEmpty(ids)) //自动匹配时
                {
                    int PARAM_PDZXSJ = cacheSysParam.SingleAndInit(x => x.ID == 3101).Value.ToInt();
                    if (PARAM_PDZXSJ > 0) //提供帮助入匹配列表的时间
                        supplylist = supplylist.Where(x => SqlFunctions.DateDiff("minute", x.CreateTime, DateTime.Now) >= PARAM_PDZXSJ).ToList();
                }

                if (!string.IsNullOrEmpty(ids)) //指定ID(提供方)
                {
                    //errmsg += "ids:" + ids.TrimEnd(',').TrimStart(',') + "";
                    //errmsg += "id集" + string.Join(",", supplylist.Select(x => x.ID).ToList());
                    string[] idss = ids.TrimEnd(',').TrimStart(',').Split(',');
                    supplylist = supplylist.Where(x => idss.Contains(x.ID.ToString())).ToList();
                }
                if (supplylist.Count() <= 0) errmsg += "没有符合条件的提供单记录";
                foreach (var supplyModel in supplylist)
                {
                    if (acceptModel.UID == supplyModel.UID)
                    {
                        errmsg += "提供单和匹配单是同一个用户“" + acceptModel.UserName + "”在订单号：" + supplyModel.SupplyNo;
                        break; //提供和接受同一个用户跳过
                    }
                    var newacceptModel = MvcCore.Unity.Get<IAcceptHelpService>().Single(acceptModel.ID);  //重读受单数据，才可以取到最新匹配余量
                    decimal _acceptppamount = (newacceptModel.ExchangeAmount - newacceptModel.HaveMatchingAmount); //受单可匹配的金额
                    decimal _supplyppamount = (supplyModel.ExchangeAmount - supplyModel.HaveMatchingAmount); //供单可匹配的金额
                    decimal _matchamount2 = 0; //匹配量

                    if (_supplyppamount <= _acceptppamount)
                        _matchamount2 = _supplyppamount;  //提供单全部匹配,接受单全部或部分匹配
                    else
                        _matchamount2 = _acceptppamount;  //提供单部分匹配


                    DateTime _payendtime = DateTime.Now.AddMinutes(cacheSysParam.SingleAndInit(x => x.ID == 3106).Value.ToInt()); //付款截止时间
                    if (supplyModel.OrderType == 0)
                        _payendtime = DateTime.Now.AddMinutes(cacheSysParam.SingleAndInit(x => x.ID == 3105).Value2.ToInt()); //付款截止时间

                    //增加匹配记录
                    MvcCore.Unity.Get<IMatchingService>().Add(new Data.Matching
                    {
                        AcceptNo = acceptModel.AcceptNo,  //接受单单号
                        MatchingNo = Matchings.GetOrderNumber(),  //匹配单单号
                        AcceptUID = acceptModel.UID, //接受者
                        AcceptUserName = acceptModel.UserName,  //接受者
                        CreateTime = DateTime.Now,
                        MatchAmount = _matchamount2,  //匹配数量
                        PayEndTime = _payendtime, //付款截止时间
                        Status = (int)Data.Enum.MatchingStatus.UnPaid, //未付款
                        SupplyNo = supplyModel.SupplyNo, //提供单单号
                        SupplyUID = supplyModel.UID, //提供者
                        SupplyUserName = supplyModel.UserName,
                        City = supplyModel.City,
                        County = supplyModel.County,
                        Province = supplyModel.Province
                    });
                    matchcount++;

                    var supplyUser = MvcCore.Unity.Get<IUserService>().Single(supplyModel.UID);
                    //supplymobile += "," + supplyUser.Mobile;

                    var acceptUser = MvcCore.Unity.Get<IUserService>().Single(acceptModel.UID);
                    //acceptmobile += "," + acceptUser.Mobile;

                    //更新提供单状态及匹配余量
                    var updateSModel = MvcCore.Unity.Get<ISupplyHelpService>().Single(supplyModel.ID);
                    if (supplyModel.HaveMatchingAmount + _matchamount2 >= supplyModel.ExchangeAmount)
                    {
                        updateSModel.HaveMatchingAmount = updateSModel.ExchangeAmount;
                        updateSModel.Status = (int)Data.Enum.HelpStatus.AllMatching;
                    }
                    else
                    {
                        updateSModel.HaveMatchingAmount = updateSModel.HaveMatchingAmount + _matchamount2;
                        updateSModel.Status = (int)Data.Enum.HelpStatus.PartOfMatching;
                    }
                    MvcCore.Unity.Get<ISupplyHelpService>().Update(updateSModel);
                    MvcCore.Unity.Get<ISysDBTool>().Commit();

                    //更新接受单状态及匹配余量
                    if (newacceptModel.HaveMatchingAmount + _matchamount2 >= newacceptModel.ExchangeAmount)
                    {
                        var updateAModel = MvcCore.Unity.Get<IAcceptHelpService>().Single(newacceptModel.ID);
                        updateAModel.HaveMatchingAmount = updateAModel.ExchangeAmount;
                        updateAModel.Status = (int)Data.Enum.HelpStatus.AllMatching;
                        MvcCore.Unity.Get<IAcceptHelpService>().Update(updateAModel);
                        MvcCore.Unity.Get<ISysDBTool>().Commit();
                        break;  //全部匹配后跳出循环
                    }
                    else
                    {
                        var updateAModel = MvcCore.Unity.Get<IAcceptHelpService>().Single(newacceptModel.ID);
                        updateAModel.HaveMatchingAmount = updateAModel.HaveMatchingAmount + _matchamount2;
                        updateAModel.Status = (int)Data.Enum.HelpStatus.PartOfMatching;
                        MvcCore.Unity.Get<IAcceptHelpService>().Update(updateAModel);
                        MvcCore.Unity.Get<ISysDBTool>().Commit();
                    }

                    if (MvcCore.Unity.Get<ISysParamService>().SingleAndInit(x => x.ID == 4503).Value == "1")
                        SMSHelper.WebChineseMSM(supplyUser.Mobile, MvcCore.Unity.Get<ISysParamService>().SingleAndInit(x => x.ID == 4503).Value2.Replace("{SUPPLYNO}", supplyModel.SupplyNo));

                    if (MvcCore.Unity.Get<ISysParamService>().SingleAndInit(x => x.ID == 4504).Value == "1")
                        SMSHelper.WebChineseMSM(acceptUser.Mobile, MvcCore.Unity.Get<ISysParamService>().SingleAndInit(x => x.ID == 4504).Value2.Replace("{ACCEPTNO}", acceptModel.AcceptNo));
                }
            }

            if (matchcount > 0)
            {
                //if (MvcCore.Unity.Get<ISysParamService>().SingleAndInit(x => x.ID == 4503).Value == "1")
                //    SMSHelper.WebChineseMSM(supplymobile, MvcCore.Unity.Get<ISysParamService>().SingleAndInit(x => x.ID == 4503).Value2);

                //if (MvcCore.Unity.Get<ISysParamService>().SingleAndInit(x => x.ID == 4504).Value == "1")
                //    SMSHelper.WebChineseMSM(acceptmobile, MvcCore.Unity.Get<ISysParamService>().SingleAndInit(x => x.ID == 4504).Value2);
                outMsg = "已匹配成功" + matchcount + "条提供单！";
            }
            else
                outMsg = "本次操作没有匹配任何记录.";

            if (!string.IsNullOrEmpty(errmsg))
                outMsg += "\n\r提示：" + errmsg;
        }
        #endregion

        #region 惩罚处理
        /// <summary>
        /// 惩罚处理（提供虚假汇款信息时）
        /// </summary>
        /// <param name="matchid">匹配单号</param>
        public static void Punish(int matchid)
        {
            var mModel = MvcCore.Unity.Get<IMatchingService>().Single(matchid);
            CancelMatching(mModel, "虚假汇款信息", true);

            var pUser = MvcCore.Unity.Get<IUserService>().Single(mModel.SupplyUID);
            pUser.IsLock = true;
            pUser.LockTime = DateTime.Now;
            pUser.LockReason = "虚假汇款信息封号处理，匹配订单：" + mModel.MatchingNo + "";
            MvcCore.Unity.Get<IUserService>().Update(pUser);
            MvcCore.Unity.Get<ISysDBTool>().Commit();
        }
        #endregion

        #region 取消匹配
        /// <summary>
        /// 
        /// </summary>
        /// <param name="item">匹配单</param>
        /// <param name="Reason">取消原因</param>
        /// <param name="isSupplyCancel">虚假汇款时提供单直接消费</param>
        public static void CancelMatching(Data.Matching item, string Reason, Boolean isSupplyCancel)
        {


            //供单退回处理
            string alog = "";
            string slog = "";
            bool err = false;
            var sModel = MvcCore.Unity.Get<ISupplyHelpService>().Single(x => x.SupplyNo == item.SupplyNo);
            slog = Reason + "后修正，原：" + sModel.HaveMatchingAmount;
            if (isSupplyCancel)
            {
                sModel.HaveMatchingAmount = sModel.HaveMatchingAmount - item.MatchAmount;  //减掉已匹配的金额
                sModel.CancelTime = DateTime.Now;
                sModel.Status = (int)Data.Enum.HelpStatus.Cancel;
            }
            else
            {
                //找已匹配的总量
                decimal havesmatching = MvcCore.Unity.Get<IMatchingService>().List(x => x.SupplyNo == sModel.SupplyNo && x.Status > 0).Count() > 0 ? MvcCore.Unity.Get<IMatchingService>().List(x => x.SupplyNo == sModel.SupplyNo && x.Status > 0).Sum(x => x.MatchAmount) : 0;
                slog += "，新：" + havesmatching;
                sModel.HaveMatchingAmount = havesmatching - item.MatchAmount;
                //重新修正状态
                if (sModel.HaveMatchingAmount == 0)
                    sModel.Status = (int)Data.Enum.HelpStatus.NoMatching;
                else if (sModel.HaveMatchingAmount > 0 && sModel.HaveMatchingAmount < sModel.ExchangeAmount)
                    sModel.Status = (int)Data.Enum.HelpStatus.PartOfMatching;
                else
                {
                    sModel.Status = (int)Data.Enum.HelpStatus.Cancel;
                    slog += "已匹配金额异常，系统取消处理";
                }
            }
            slog += "，修复为：" + sModel.HaveMatchingAmount;
            sModel.ReserveStr2 = slog;
            MvcCore.Unity.Get<ISupplyHelpService>().Update(sModel);
            MvcCore.Unity.Get<ISysDBTool>().Commit();

            //受单退回处理
            var aModel = MvcCore.Unity.Get<IAcceptHelpService>().Single(x => x.AcceptNo == item.AcceptNo);
            alog = "原：" + aModel.HaveMatchingAmount;
            //找已匹配的总量
            decimal havematching = MvcCore.Unity.Get<IMatchingService>().List(x => x.AcceptNo == aModel.AcceptNo && x.Status > 0).Count() > 0 ? MvcCore.Unity.Get<IMatchingService>().List(x => x.AcceptNo == aModel.AcceptNo && x.Status > 0).Sum(x => x.MatchAmount) : 0;
            alog += "，新：" + havematching;
            if (isSupplyCancel && item.Status == (int)Data.Enum.MatchingStatus.Falsehood) //虚假订单时不扣除当前匹配单金额，因为havematching不包含标记为虚假订单的部分
                aModel.HaveMatchingAmount = havematching;
            else
                aModel.HaveMatchingAmount = havematching - item.MatchAmount;
            //重新修正状态
            if (aModel.HaveMatchingAmount == 0)
                aModel.Status = (int)Data.Enum.HelpStatus.NoMatching;
            else if (aModel.HaveMatchingAmount > 0 && aModel.HaveMatchingAmount < aModel.ExchangeAmount)
                aModel.Status = (int)Data.Enum.HelpStatus.PartOfMatching;
            else
            {
                err = true;
                //aModel.Status = (int)Data.Enum.HelpStatus.Cancel;
                alog += "已匹配金额异常，系统自动修复";
            }
            alog += "，修复为：" + aModel.HaveMatchingAmount;
            aModel.ReserveStr2 = alog;
            aModel.IsTop = true;
            MvcCore.Unity.Get<IAcceptHelpService>().Update(aModel);
            MvcCore.Unity.Get<ISysDBTool>().Commit();

            //配单取消
            var mModel = MvcCore.Unity.Get<IMatchingService>().Single(item.ID);
            mModel.Status = (int)Data.Enum.MatchingStatus.Cancel;
            mModel.CancelTime = DateTime.Now;
            mModel.CanceReason = Reason;
            MvcCore.Unity.Get<IMatchingService>().Update(mModel);
            MvcCore.Unity.Get<ISysDBTool>().Commit();


            if (err) //系统自动修复
            {
                var _aModel = MvcCore.Unity.Get<IAcceptHelpService>().Single(x => x.AcceptNo == item.AcceptNo);
                _aModel.HaveMatchingAmount = MvcCore.Unity.Get<IMatchingService>().List(x => x.AcceptNo == aModel.AcceptNo && x.Status > 0).Count() > 0 ? MvcCore.Unity.Get<IMatchingService>().List(x => x.AcceptNo == aModel.AcceptNo && x.Status > 0).Sum(x => x.MatchAmount) : 0;
                if (_aModel.HaveMatchingAmount <= 0)
                    _aModel.Status = (int)Data.Enum.HelpStatus.NoMatching;
                else if (_aModel.HaveMatchingAmount > 0 && _aModel.HaveMatchingAmount < _aModel.ExchangeAmount)
                    _aModel.Status = (int)Data.Enum.HelpStatus.PartOfMatching;
                else
                    _aModel.Status = (int)Data.Enum.HelpStatus.AllMatching;
                MvcCore.Unity.Get<IAcceptHelpService>().Update(aModel);
                MvcCore.Unity.Get<ISysDBTool>().Commit();
            }
        }
        #endregion

        #region 注册48小时内必须提供帮助
        public static void MustBeRegisteredAfterSupplyHelp(Data.User onUser)
        {
            List<Data.SysParam> cacheSysParam = MvcCore.Unity.Get<JN.Data.Service.ISysParamService>().List(x => x.ID < 4000).ToList();

            //注册48小时内必须提供帮助，否侧冻结帐号
            int jctgqx = cacheSysParam.SingleAndInit(x => x.ID == 3806).Value.ToInt();
            if (jctgqx > 0)
            {
                if ((onUser.ActivationTime ?? DateTime.Now).AddMinutes(jctgqx) < DateTime.Now)
                {
                    if (MvcCore.Unity.Get<JN.Data.Service.ISupplyHelpService>().List(x => x.UID == onUser.ID).Count() <= 0)
                    {
                        var updateUserEntity = MvcCore.Unity.Get<IUserService>().Single(onUser.ID);
                        if ((onUser.LockReason ?? "abc").Contains("注册成功后未在限时内提供帮助") && !onUser.IsLock)
                            updateUserEntity.LockReason = "";
                        else
                        {
                            updateUserEntity.IsLock = true;
                            updateUserEntity.LockTime = DateTime.Now;
                            updateUserEntity.LockReason = "注册成功后未在限时内提供帮助";
                        }
                        MvcCore.Unity.Get<IUserService>().Update(updateUserEntity);
                        MvcCore.Unity.Get<ISysDBTool>().Commit();
                    }
                }
            }
        }
        #endregion

        #region 提现完成后必须进行复投
        public static string MustBeReCastAfterAcceptHelp(Data.User onUser)
        {
            List<Data.SysParam> cacheSysParam = MvcCore.Unity.Get<JN.Data.Service.ISysParamService>().List(x => x.ID < 4000).ToList();

            //接受订单完成后24小时必须进行复投
            int ftqx = cacheSysParam.SingleAndInit(x => x.ID == 3803).Value.ToInt();
            if (ftqx > 0)
            {
                var ahelps = MvcCore.Unity.Get<JN.Data.Service.IAcceptHelpService>().List(x => x.UID == onUser.ID && x.Status == (int)JN.Data.Enum.HelpStatus.AllDeal && x.CoinID == 2001).OrderByDescending(x => x.AllDealTime).ToList();
                var shelps = MvcCore.Unity.Get<JN.Data.Service.ISupplyHelpService>().List(x => x.UID == onUser.ID).OrderByDescending(x => x.CreateTime).ToList();
               // var shelps = MvcCore.Unity.Get<JN.Data.Service.ISupplyHelpService>().List(x => x.UID == 7).OrderByDescending(x => x.CreateTime).AsQueryable<Data.SupplyHelp>().ToList();
              
                if (ahelps.Count > 0 && shelps.Count > 0)
                {
                    if ((ahelps.FirstOrDefault().AllDealTime ?? DateTime.Now).AddMinutes(ftqx) < DateTime.Now && (ahelps.FirstOrDefault().AllDealTime ?? DateTime.Now) > shelps.FirstOrDefault().CreateTime)
                    {
                        //封号处理，初始帐号除外
                        if (onUser.ParentID > 0)
                        {
                            var updateUserEntity = MvcCore.Unity.Get<JN.Data.Service.IUserService>().Single(onUser.ID);
                            if ((onUser.LockReason ?? "abc").Contains("超时未进行复投") && !onUser.IsLock)
                                updateUserEntity.LockReason = "";
                            else
                            {
                                updateUserEntity.IsLock = true;
                                updateUserEntity.LockTime = DateTime.Now;
                                updateUserEntity.LockReason = "超时未进行复投，单号：" + ahelps.FirstOrDefault().AcceptNo;
                            }
                            MvcCore.Unity.Get<JN.Data.Service.IUserService>().Update(updateUserEntity);
                            MvcCore.Unity.Get<JN.Data.Service.ISysDBTool>().Commit();
                            return "<p style=\"color:#f00\" > 注意，请马上进行复投，刷新此页面您的帐号将被冻结，来自已完成的接受订单“" + ahelps.FirstOrDefault().AcceptNo + "”需要马上进行复投！</p>";
                        }
                    }
                    else if ((ahelps.FirstOrDefault().AllDealTime ?? DateTime.Now) > shelps.FirstOrDefault().CreateTime)
                    {
                        DateTime endtime = (ahelps.FirstOrDefault().AllDealTime ?? DateTime.Now).AddMinutes(ftqx);
                        return "<p style=\"color:#f00\">请在时限内进行提供帮助，来自已完成的接受订单“" + ahelps.FirstOrDefault().AcceptNo + "”<i class=\"fa pe-7s-clock\"></i> 剩余时间：<span class=\"time_countdown\" style=\"color:#f00\" data=\"" + endtime + "\"></span></p>";
                    }
                    else
                    {
                        return "<p style=\"color:#f00\" > 受订单全部成时，您需要在限时内进行复投，否则您的帐号将被冻结。</p>";
                    }
                }
            }
            return "";
        }
        #endregion

        #region 取消提供单
        /// <summary>
        /// 
        /// </summary>
        public static void CancelSupplyHelp(string SupplyNo, string Reason)
        {
            //删除利息,奖金
            MvcCore.Unity.Get<IBonusDetailService>().Delete(x => x.SupplyNo == SupplyNo && x.IsBalance == false);
            MvcCore.Unity.Get<ISysDBTool>().Commit();

            var sModel = MvcCore.Unity.Get<ISupplyHelpService>().Single(x => x.SupplyNo == SupplyNo);
            if (sModel != null)
            {
                sModel.Status = (int)Data.Enum.HelpStatus.Cancel;
                sModel.CancelTime = DateTime.Now;
                sModel.CancelReason = Reason;
                sModel.IsAccruaCount = false;
                MvcCore.Unity.Get<ISupplyHelpService>().Update(sModel);
                MvcCore.Unity.Get<ISysDBTool>().Commit();
            }
        }
        #endregion

        #region 取消接受单
        /// <summary>
        /// 
        /// </summary>
        public static void CancelAcceptHelp(string AcceptNo, string Reason)
        {
            //删除利息,奖金
            var aModel = MvcCore.Unity.Get<IAcceptHelpService>().Single(x => x.AcceptNo == AcceptNo);
            if (aModel != null)
            {
                Wallets.changeWallet(aModel.UID, aModel.ExchangeAmount, aModel.CoinID, "取消接受帮助“" + aModel.AcceptNo + "”订单返还");
                aModel.Status = (int)Data.Enum.HelpStatus.Cancel;
                aModel.CancelTime = DateTime.Now;
                aModel.CancelReason = Reason;
                MvcCore.Unity.Get<IAcceptHelpService>().Update(aModel);
                MvcCore.Unity.Get<ISysDBTool>().Commit();
            }
        }
        #endregion
    }
}

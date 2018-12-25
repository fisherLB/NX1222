using JN.Services.Tool;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.SqlServer;
using System.Linq;

namespace JN.Services.Manager
{
    /// <summary>
    ///奖金结算（用到以下奖金）
    ///直推奖Bonus1103  直推1人循环拿直推奖人投资金额的4%，每增加直推1人，直推奖增加1%，直推奖15%封顶
    ///管理奖Bonus1104  管理奖 推荐1人拿1代，10代封顶，管理奖50%冻结半个月，另50%冻结1个月
    /// </summary>
    public partial class Bonus
    {
        List<Data.SysParam> cacheSysParam = MvcCore.Unity.Get<JN.Data.Service.ISysParamService>().List(x => x.ID < 4000).ToList();


        #region 写入奖金明细表并更新用户钱包
        /// <summary>
        /// 写入资金明细表并更新用户钱包（未结算只写入奖金表不进钱包及资金明细）
        /// </summary>
        /// <param name="bonusmoney">奖金金额</param>
        /// <param name="period">期数（发放利息时）</param>
        /// <param name="bonusid">奖金ID（对应参数表SysParam的ID）</param>
        /// <param name="bonusname">奖金名称（对应参数表SysParam的Name）</param>
        /// <param name="bonusdesc">获奖描述来源</param>
        /// <param name="onUserID">会员ID</param>
        /// <param name="addupfield">累计奖金字段（对应用户表User的Addup1101-1107/1802）</param>
        /// <param name="isbalance">是否结算,Ture时写入钱包明细表WalletDetails及更新User表中用户钱包余额Wallet2001-2005，Falsh时只记入奖金明细表BonusDetails</param>
        public static void UpdateUserWallet(decimal bonusmoney, string supplyno, int bonusid, string bonusname, string bonusdesc, int onUserID, int formUserID, string addupfield, bool isbalance, bool isEffective, DateTime effectiveTime)
        {
            List<Data.SysParam> cacheSysParam = MvcCore.Unity.Get<JN.Data.Service.ISysParamService>().List(x => x.ID < 4000).ToList();

            var onUser = MvcCore.Unity.Get<JN.Data.Service.IUserService>().Single(onUserID);
            var fromUser = MvcCore.Unity.Get<JN.Data.Service.IUserService>().Single(formUserID);
            if (bonusmoney > 0)
            {
                //是否马上结算
                if (isbalance)
                {
                    //5%冻结到加密数字资产钱包
                    if (bonusid == 1103 || bonusid == 1104 || bonusid == 1106)
                    {
                        decimal PARAM_SZQBBL = cacheSysParam.SingleAndInit(x => x.ID == 2005).Value2.ToDecimal();// 进入加密数字资产钱包比例
                        Wallets.changeWallet(onUser.ID, bonusmoney * PARAM_SZQBBL, 2005, bonusdesc);
                        Wallets.changeWallet(onUser.ID, bonusmoney * (1 - PARAM_SZQBBL), 2003, bonusdesc);
                    }
                    else if (bonusid == 1108)
                        Wallets.changeWallet(onUser.ID, bonusmoney, 2002, bonusdesc);
                    else
                        Wallets.changeWallet(onUser.ID, bonusmoney, 2001, bonusdesc);
                }
                //写入奖金表
                MvcCore.Unity.Get<Data.Service.IBonusDetailService>().Add(new Data.BonusDetail
                {
                    Period = 0,
                    BalanceTime = DateTime.Now,
                    SupplyNo = supplyno,
                    BonusMoney = bonusmoney,
                    BonusID = bonusid,
                    BonusName = bonusname,
                    CreateTime = DateTime.Now,
                    Description = bonusdesc,
                    IsBalance = isbalance,
                    UID = onUser.ID,
                    IsEffective = isEffective,
                    EffectiveTime = effectiveTime,
                    UserName = onUser.UserName,
                    FromUID = fromUser.ID,
                    FromUserName = fromUser.UserName
                });
                MvcCore.Unity.Get<Data.Service.ISysDBTool>().Commit();
            }
        }
        #endregion

        #region 动态奖 在wl16071701中
        /// <summary>
        ///动态奖
        /// </summary>
        /// <param name="supplyid"></param>
        public static void Bonus1103(string SupplyNo)
        {
            List<Data.SysParam> cacheSysParam = MvcCore.Unity.Get<JN.Data.Service.ISysParamService>().List(x => x.ID < 4000).ToList();

            decimal PARAM_TJJBL = 0; //推荐奖比例
            var sModel = MvcCore.Unity.Get<Data.Service.ISupplyHelpService>().List(x => x.SupplyNo == SupplyNo).OrderBy(x => x.CreateTime).FirstOrDefault();
            var onUser = MvcCore.Unity.Get<Data.Service.IUserService>().Single(sModel.UID);
            //查找本会员的一条线
            var userlist = MvcCore.Unity.Get<Data.Service.IUserService>().List(x => x.RefereePath.Contains("," + onUser.ID + ",") && x.IsActivation == true).OrderByDescending(x => x.ID);

            var refereeUser = MvcCore.Unity.Get<Data.Service.IUserService>().Single(onUser.RefereeID);//推荐人
            if (refereeUser != null)
            {
                var param = cacheSysParam.SingleAndInit(x => x.ID == 1103);
                //int tjrs = Users.GetAllRefereeChild(refereeUser, 1).Where(x => x.AddupSupplyAmount > 0 && x.IsLock == false).Count();

                ////奖金没有冻结期
                //bool iseffective = true;
                //DateTime effectivetime = DateTime.Now;
                //PARAM_TJJBL = Math.Min((param.Value.ToDecimal() + tjrs * param.Value2.ToDecimal()), param.Value3.ToDecimal());

                #region 找出5代会员算动态奖
                //找出5代会员算动态奖（多代管理奖或推荐奖）
                var tjjlist = Users.GetAllRefereeParent(onUser, 5);
                foreach (var item in tjjlist)
                {
                    //推荐人数（完成订单才算）
                    //int tjrs = Users.GetAllRefereeChild(item, 1).Count();
                    bool iseffective = true;
                    DateTime effectivetime = DateTime.Now;
                    int PARAMID = 0;
                    //if (tjrs >= (onUser.RefereeDepth - item.RefereeDepth))//推荐几人拿几代
                    //{
                    switch (onUser.RefereeDepth - item.RefereeDepth) //几代推荐奖
                    {
                        case 1:
                            PARAMID = 1301;
                            break;
                        //    if (item.IsAgent == true)
                        //    {
                        //        PARAMID = 1301;
                        //    }
                        //    else
                        //    {
                        //        PARAMID = 1321;
                        //    }
                        //    break;
                        //case 2:
                        //    if (item.IsAgent == true)
                        //    {
                        //        PARAMID = 1302;
                        //    }
                        //    else
                        //    {
                        //        PARAMID = 1322;
                        //    }
                        //    break;
                        //case 3:
                        //    if (item.IsAgent == true)
                        //    {
                        //        PARAMID = 1303;
                        //    }
                        //    else
                        //    {
                        //        PARAMID = 1323;
                        //    }
                        //    break;
                        //case 4:
                        //    if (item.IsAgent == true)
                        //    {
                        //        PARAMID = 1304;
                        //    }
                        //    else
                        //    {
                        //        PARAMID = 1324;
                        //    }
                        //    break;
                        //case 5:
                        //    if (item.IsAgent == true)
                        //    {
                        //        PARAMID = 1305;
                        //    }
                        //    else
                        //    {
                        //        PARAMID = 0;
                        //    }
                        //    break;
                        default: //6代以上直推奖
                            //if (item.IsAgent == true)
                            //    PARAMID = 1306;
                            //else
                                PARAMID = 0;
                            break;
                    }
                    //}
                #endregion
                    if (PARAMID != 0)
                    {
                        PARAM_TJJBL = cacheSysParam.SingleAndInit(x => x.ID == PARAMID).Value.ToDecimal();
                        //小单推大单限制
                        var latstsupply = MvcCore.Unity.Get<Data.Service.ISupplyHelpService>().List(x => x.UID == item.ID && x.Status >= (int)Data.Enum.HelpStatus.AllDeal && x.RoundAmount >0).OrderByDescending(x => x.ID).FirstOrDefault();
                        if (latstsupply != null) //不投资不拿动态奖
                        {
                            Decimal CurrentMoney = sModel.RoundAmount?? 0;
                            decimal LastModelMoney =latstsupply.RoundAmount??0;


                            decimal jsMoney = CurrentMoney; //Math.Min(LastModelMoney, CurrentMoney);
                            decimal bonusMoney = jsMoney * PARAM_TJJBL;
                            if (bonusMoney > 0)
                            {
                                string bonusDesc = "来自会员【" + sModel.UserName + "】提供帮助订单【" + sModel.SupplyNo + "】的" + param.Name + "(" + jsMoney + "×" + PARAM_TJJBL + ")";
                                UpdateUserWallet(bonusMoney, sModel.SupplyNo, param.ID, param.Name, bonusDesc, item.ID, onUser.ID, "Addup1103", false, iseffective, effectivetime);


                            }
                        }
                    }


                }
            }
        }
        #endregion

        #region 管理奖 wl16071701专属奖项
        /// <summary>
        /// 因为下属会员获得动态奖而获得管理奖
        /// </summary>
        /// <param name="supplyid">提供订单ID</param>
        public static void Bonus1104(decimal bonusMoney, int supplyid, int id)
        {
            List<Data.SysParam> cacheSysParam = MvcCore.Unity.Get<JN.Data.Service.ISysParamService>().List(x => x.ID < 4000).ToList();
            var param = cacheSysParam.SingleAndInit(x => x.ID == 1104);
            var sModel = MvcCore.Unity.Get<Data.Service.ISupplyHelpService>().Single(supplyid);
            var onUser = MvcCore.Unity.Get<Data.Service.IUserService>().Single(id);

            decimal PARAM_GLJBL = 0;
            //找出2代会员算管理奖（多代管理
            var tjjlist = Users.GetAllRefereeParent(onUser, 2);
            foreach (var item in tjjlist)
            {
                int PARAMID = 0;
                switch (onUser.RefereeDepth - item.RefereeDepth)
                {
                    case 1:
                        PARAMID = 1401;
                        break;
                    case 2:
                        PARAMID = 1402;
                        break;
                    default: //3代以上管理奖
                        PARAMID = 0;
                        break;
                }
                if (PARAMID != 0)
                {
                    PARAM_GLJBL = cacheSysParam.SingleAndInit(x => x.ID == PARAMID).Value.ToDecimal();
                    decimal GLJMoney = bonusMoney * PARAM_GLJBL;
                    if (GLJMoney > 0)
                    {
                        string bonusDesc = "来自【" + onUser.UserName + "】的动态奖产生的" + param.Name + "(" + bonusMoney + "×" + PARAM_GLJBL + ")";
                        UpdateUserWallet(GLJMoney, sModel.SupplyNo, 1104, param.Name, bonusDesc, item.ID, id, "Addup1105", false, true, DateTime.Now);
                    }
                }

            }
        }
        #endregion

        #region 动态奖
        /// <summary>
        /// 动态奖：自己至少完成过一单提供帮助才可以提现动态奖，100倍数提现
        ///推荐1人—享受第1代提供援助金额1%
        ///推荐2人—享受第1、2代提供援助金额1%
        ///推荐4人—享受第1、2、3代提供援助金额1%
        ///推荐6人—享受第1、2、3、4代提供援助金额1%
        ///推荐8人—享受第1、2、3、4、5代提供援助金额1%
        ///推荐10人，团队达到100人—享受第1-5代1%，6-10代提供援助金额0.3%
        ///推荐20人，团队达到300人—享受第1-5代1%，6-10代提供援助金额0.3%，第11-20代提供援助金额0.1%
        ///推荐30人，团队达到500人—享受第1-5代1%，6-10代提供援助金额0.3%，第11-20代提供援助金额0.1%，第21-无限代提供援助金额0.01%
        /// </summary>
        /// <param name="supplyid">提供订单ID</param>
        //public static void Bonus1104(int supplyid)
        //{
        //    List<Data.SysParam> cacheSysParam = MvcCore.Unity.Get<JN.Data.Service.ISysParamService>().List(x => x.ID < 4000).ToList();

        //    var sModel = MvcCore.Unity.Get<Data.Service.ISupplyHelpService>().Single(supplyid);
        //    var onUser = MvcCore.Unity.Get<Data.Service.IUserService>().Single(sModel.UID);
        //    var param = cacheSysParam.SingleAndInit(x => x.ID == 1104);
        //    int number = 1;
        //    var gljlist = Users.GetAllRefereeParent(onUser, 0).Where(x => x.IsActivation).ToList();//查找整条线上的人
        //    foreach (var item in gljlist)
        //    {
        //        //推荐人数（完成订单才算）
        //        int tjrs = Users.GetAllRefereeChild(item, 1).Where(x => x.AddupSupplyAmount > 0 && x.IsLock == false).Count();
        //        int tdrs = MvcCore.Unity.Get<Data.Service.IUserService>().List(x=>x.RefereePath.Contains(","+item.ID+",") && x.IsActivation==true && x.IsLock==false).Count();

        //        decimal PARAM_JLJBL = 0; //管理奖比例
        //        if (tjrs == 1)
        //        {
        //            if(number<=cacheSysParam.SingleAndInit(x => x.ID == 1401).Value2.ToInt())PARAM_JLJBL= cacheSysParam.SingleAndInit(x => x.ID == 1401).Value3.ToDecimal();
        //        }
        //        if (tjrs >= 2 && tjrs < 4) 
        //            if(number<=cacheSysParam.SingleAndInit(x => x.ID == 1402).Value2.ToInt())PARAM_JLJBL= cacheSysParam.SingleAndInit(x => x.ID == 1402).Value3.ToDecimal();

        //        if (tjrs >= 4 && tjrs < 6) 
        //            if(number<=cacheSysParam.SingleAndInit(x => x.ID == 1403).Value2.ToInt())PARAM_JLJBL= cacheSysParam.SingleAndInit(x => x.ID == 1403).Value3.ToDecimal();

        //        if (tjrs >= 6 && tjrs < 8) 
        //            if(number<=cacheSysParam.SingleAndInit(x => x.ID == 1404).Value2.ToInt())PARAM_JLJBL= cacheSysParam.SingleAndInit(x => x.ID == 1404).Value3.ToDecimal();

        //        if (tjrs >= 8 && tjrs < 10)
        //            if(number<=cacheSysParam.SingleAndInit(x => x.ID == 1405).Value2.ToInt())PARAM_JLJBL= cacheSysParam.SingleAndInit(x => x.ID == 1405).Value3.ToDecimal();

        //        if (tjrs >= 10 && tjrs < 20 && tdrs>cacheSysParam.SingleAndInit(x => x.ID == 1406).Value.Split('|')[1].ToInt())
        //            if (number >= cacheSysParam.SingleAndInit(x => x.ID == 1406).Value.Split('|')[2].Split('-')[0].ToInt() && number <= cacheSysParam.SingleAndInit(x => x.ID == 1406).Value.Split('|')[2].Split('-')[1].ToInt()) PARAM_JLJBL = cacheSysParam.SingleAndInit(x => x.ID == 1406).Value2.ToDecimal();
        //            else if (number >= cacheSysParam.SingleAndInit(x => x.ID == 1406).Value.Split('|')[3].Split('-')[0].ToInt() && number <= cacheSysParam.SingleAndInit(x => x.ID == 1406).Value.Split('|')[3].Split('-')[1].ToInt()) PARAM_JLJBL = cacheSysParam.SingleAndInit(x => x.ID == 1406).Value3.ToDecimal();

        //        if (tjrs >= 20 && tjrs < 30 && tdrs > cacheSysParam.SingleAndInit(x => x.ID == 1407).Value.Split('|')[1].ToInt())
        //            if (number >= cacheSysParam.SingleAndInit(x => x.ID == 1407).Value.Split('|')[2].Split('-')[0].ToInt() && number <= cacheSysParam.SingleAndInit(x => x.ID == 1407).Value.Split('|')[2].Split('-')[1].ToInt()) PARAM_JLJBL = cacheSysParam.SingleAndInit(x => x.ID == 1407).Value2.Split('|')[0].ToDecimal();
        //            else if (number >= cacheSysParam.SingleAndInit(x => x.ID == 1407).Value.Split('|')[3].Split('-')[0].ToInt() && number <= cacheSysParam.SingleAndInit(x => x.ID == 1407).Value.Split('|')[3].Split('-')[1].ToInt()) PARAM_JLJBL = cacheSysParam.SingleAndInit(x => x.ID == 1407).Value2.Split('|')[1].ToDecimal();
        //            else if (number >= cacheSysParam.SingleAndInit(x => x.ID == 1407).Value.Split('|')[4].Split('-')[0].ToInt() && number <= cacheSysParam.SingleAndInit(x => x.ID == 1407).Value.Split('|')[4].Split('-')[1].ToInt()) PARAM_JLJBL = cacheSysParam.SingleAndInit(x => x.ID == 1407).Value3.ToDecimal();

        //        if (tjrs >= 30  && tdrs > cacheSysParam.SingleAndInit(x => x.ID == 1408).Value.Split('|')[1].ToInt())
        //            if (number >= cacheSysParam.SingleAndInit(x => x.ID == 1408).Value.Split('|')[2].Split('-')[0].ToInt() && number <= cacheSysParam.SingleAndInit(x => x.ID == 1408).Value.Split('|')[2].Split('-')[1].ToInt()) PARAM_JLJBL = cacheSysParam.SingleAndInit(x => x.ID == 1407).Value2.Split('|')[0].ToDecimal();
        //            else if (number >= cacheSysParam.SingleAndInit(x => x.ID == 1408).Value.Split('|')[3].Split('-')[0].ToInt() && number <= cacheSysParam.SingleAndInit(x => x.ID == 1408).Value.Split('|')[3].Split('-')[1].ToInt()) PARAM_JLJBL = cacheSysParam.SingleAndInit(x => x.ID == 1407).Value2.Split('|')[1].ToDecimal();
        //            else if (number >= cacheSysParam.SingleAndInit(x => x.ID == 1408).Value.Split('|')[4].Split('-')[0].ToInt() && number <= cacheSysParam.SingleAndInit(x => x.ID == 1408).Value.Split('|')[4].Split('-')[1].ToInt()) PARAM_JLJBL = cacheSysParam.SingleAndInit(x => x.ID == 1407).Value3.Split('|')[0].ToDecimal();
        //            else if (number >= cacheSysParam.SingleAndInit(x => x.ID == 1408).Value.Split('|')[5].ToInt() ) PARAM_JLJBL = cacheSysParam.SingleAndInit(x => x.ID == 1408).Value3.Split('|')[1].ToDecimal();



        //        if (PARAM_JLJBL > 0)
        //        {
        //            ////小单推大单限制
        //            //var latstsupply = MvcCore.Unity.Get<Data.Service.ISupplyHelpService>().List(x => x.UID == item.ID && x.Status >= (int)Data.Enum.HelpStatus.AllDeal).OrderByDescending(x => x.ID).FirstOrDefault();
        //            //if (latstsupply != null) //不投资不拿动态奖
        //            //{
        //              //  decimal jsMoney = Math.Min((latstsupply.OrderMoney ?? 0), (sModel.OrderMoney ?? 0));

        //                bool iseffective = true;
        //                DateTime effectivetime = DateTime.Now;// DateTime.Now.AddMinutes(cacheSysParam.SingleAndInit(x => x.ID == 1104).Value2.ToInt());
        //                //decimal firstbonusMoney = jsMoney * PARAM_JLJBL * cacheSysParam.SingleAndInit(x => x.ID == 1104).Value.ToDecimal();
        //              //  DateTime effectivetime2 = DateTime.Now.AddMinutes(cacheSysParam.SingleAndInit(x => x.ID == 1104).Value3.ToInt());
        //                //decimal nextbonusMoney = jsMoney * PARAM_JLJBL - firstbonusMoney;

        //                decimal changMoney = PARAM_JLJBL * sModel.SupplyAmount;

        //                string bonusDesc = "来自【" + sModel.UserName + "】提供帮助订单【" + sModel.SupplyNo + "】的" + param.Name + "(" + sModel.SupplyAmount + "×" + PARAM_JLJBL + ")";
        //                UpdateUserWallet(changMoney, sModel.SupplyNo, param.ID, param.Name, bonusDesc , item.ID, onUser.ID, "Addup1104", false, iseffective, effectivetime);
        //              //  UpdateUserWallet(nextbonusMoney, sModel.SupplyNo, param.ID, param.Name, bonusDesc + "×" + (1 - cacheSysParam.SingleAndInit(x => x.ID == 1104).Value.ToDecimal()), item.ID, onUser.ID, "Addup1104", false, iseffective, effectivetime2);
        //           // }

        //        }
        //        number++;
        //    }
        //}
        #endregion

        #region 限时付款奖励 在wl1617001中跟随利息
        /// <summary>
        /// 限时付款奖励
        /// </summary>
        /// <param name="supplyid">提供订单ID</param>
        public static void Bonus1105(Data.Matching mModel)
        {
            List<Data.SysParam> cacheSysParam = MvcCore.Unity.Get<JN.Data.Service.ISysParamService>().List(x => x.ID < 4000).ToList();

            //if (MvcCore.Unity.Get<Data.Service.IBonusDetailService>().List(x => x.SupplyNo == mModel.SupplyNo && x.BonusID == 1105).Count() <= 0)
            //{
            var param = cacheSysParam.SingleAndInit(x => x.ID == 1105);
            int sec = DateTimeDiff.DateDiff_Sec(mModel.CreateTime, DateTime.Now); //秒
            var sModel = MvcCore.Unity.Get<Data.Service.ISupplyHelpService>().Single(x => x.SupplyNo == mModel.SupplyNo);
            decimal PARAM_JJBL = param.Value.ToDecimal();
            int PARAM_PAYSEC = param.Value2.ToInt();
            if (sec <= PARAM_PAYSEC * 60)
            {
                decimal bonusMoney = mModel.MatchAmount * PARAM_JJBL;
                string bonusDesc = "来自【" + sModel.UserName + "】提供帮助订单【" + sModel.SupplyNo + "】的" + param.Name + "(" + mModel.MatchAmount + "×" + PARAM_JJBL + ")";
                UpdateUserWallet(bonusMoney, sModel.SupplyNo, param.ID, param.Name, bonusDesc, mModel.SupplyUID, mModel.SupplyUID, "Addup1105", false, true, DateTime.Now);
            }
            //}

        }
        #endregion

        #region 经理奖，在提供订单生成时计算管理奖(未用)
        /// <summary>
        /// 经理奖，在提供订单生成时计算管理奖
        /// </summary>
        /// <param name="supplyid">提供订单ID</param>
        public static void Bonus1108(int supplyid)
        {
            //var sModel = MvcCore.Unity.Get<Data.Service.ISupplyHelpService>().Single(supplyid);
            //var onUser = MvcCore.Unity.Get<Data.Service.IUserService>().Single(sModel.UID);
            //var param = cacheSysParam.SingleAndInit(x => x.ID == 1108);

            //bool iseffective = false;
            //DateTime effectivetime = DateTime.Now.AddMinutes(cacheSysParam.SingleAndInit(x => x.ID == 1803).Value2.ToInt());
            //var gljlist = Users.GetAllRefereeParent(onUser).Where(x => (x.IsAgent ?? false)).ToList();
            //foreach (var item in gljlist)
            //{
            //    decimal PARAM_JLJBL = 0; //经理奖比例
            //    int PARAMID = 0;
            //    switch (onUser.RefereeDepth - item.RefereeDepth) //几代经理奖
            //    {
            //        case 1: 
            //            PARAMID = 1803;
            //            break;
            //        case 2: 
            //            PARAMID = 1804;
            //            break;
            //        case 3:
            //            PARAMID = 1805;
            //            break;
            //        case 4:
            //            PARAMID = 1806;
            //            break;
            //        default:
            //            PARAMID = 1807;
            //            break;
            //    }

            //    PARAM_JLJBL = item.AgentLevel == 1 ? cacheSysParam.SingleAndInit(x => x.ID == PARAMID).Value.ToDecimal() : cacheSysParam.SingleAndInit(x => x.ID == PARAMID).Value2.ToDecimal();
            //    iseffective = false;
            //    effectivetime = DateTime.Now.AddMinutes(cacheSysParam.SingleAndInit(x => x.ID == PARAMID).Value3.ToInt());

            //    decimal bonusMoney = sModel.ExchangeAmount * PARAM_JLJBL;
            //    string bonusDesc = "来自提供帮助订单【" + sModel.SupplyNo + "】的" + param.Name + "(" + sModel.ExchangeAmount + "×" + PARAM_JLJBL + ")";
            //    UpdateUserWallet(bonusMoney, sModel.SupplyNo, param.ID, param.Name, bonusDesc, item.ID, "Addup1108", false, iseffective, effectivetime);
            //}
        }
        #endregion

        #region 确认收款时结算利息及奖金
        /// <summary>
        /// 提供订单完成付款并被确认时进行利息及奖金结算
        /// </summary>
        /// <param name="mModel">匹配实体</param>
        public static void Settlement(Data.Matching mModel)
        {
            List<Data.SysParam> cacheSysParam = MvcCore.Unity.Get<JN.Data.Service.ISysParamService>().List(x => x.ID < 4000).ToList();

            var sModel = MvcCore.Unity.Get<Data.Service.ISupplyHelpService>().Single(x => x.SupplyNo == mModel.SupplyNo);

            //  var bonuslist = MvcCore.Unity.Get<Data.Service.IBonusDetailService>().List(x => x.SupplyNo == mModel.SupplyNo && x.IsBalance == false && (x.IsEffective ?? false)).ToList();
            ////给提供订单会员结算利息
            //decimal bonusmoney1102 = bonuslist.Where(x => x.BonusID == 1102).Count() > 0 ? bonuslist.Where(x => x.BonusID == 1102).Sum(x => x.BonusMoney) : 0;
            //if (bonusmoney1102 > 0)
            //{
            //    string bonusdesc = "来自提供单“" + sModel.SupplyNo + "”的利息结算";
            //    Wallets.changeWallet(sModel.UID, bonusmoney1102, 2001, bonusdesc);
            //    //结算完成，更新奖金表（利息部分）
            //    Dictionary<string, string> dictBonus1102 = new Dictionary<string, string>();
            //    dictBonus1102.Add("IsBalance", "1");
            //    dictBonus1102.Add("BalanceTime", "getdate()");
            //    MvcCore.Unity.Get<Data.Service.IBonusDetailService>().Update(new Data.BonusDetail(), dictBonus1102, "SupplyNo='" + mModel.SupplyNo + "' and BonusID=1102 and IsBalance=0");
            //    MvcCore.Unity.Get<Data.Service.ISysDBTool>().Commit();
            //}

            var supplyMatchlist = MvcCore.Unity.Get<Data.Service.IMatchingService>().List(x => (x.SupplyNo == mModel.SupplyNo) && x.Status == (int)Data.Enum.MatchingStatus.Verified);
            decimal supplyYCJ = supplyMatchlist.Count() > 0 ? supplyMatchlist.Sum(x => x.MatchAmount) : 0;
            //sModel.IsAccrualEffective = true;
            if (supplyYCJ + mModel.MatchAmount >= sModel.ExchangeAmount) //全部成交
            {
                sModel.Status = (int)Data.Enum.HelpStatus.AllDeal;

                var param3809 = cacheSysParam.SingleAndInit(x => x.ID == 3809);
                int sec = DateTimeDiff.DateDiff_Sec(sModel.CreateTime, DateTime.Now); //秒

                sModel.ReserveDate2 = DateTime.Now.AddMinutes(cacheSysParam.SingleAndInit(x => x.ID == 3809).Value.ToInt());//本金利息冻结时间


            }
            else
                sModel.Status = (int)Data.Enum.HelpStatus.PartOfDeal;
            MvcCore.Unity.Get<Data.Service.ISupplyHelpService>().Update(sModel);
            MvcCore.Unity.Get<Data.Service.ISysDBTool>().Commit();

            //更新接受单状态
            var aModel = MvcCore.Unity.Get<Data.Service.IAcceptHelpService>().Single(x => x.AcceptNo == mModel.AcceptNo);
            var acceptMatchlist = MvcCore.Unity.Get<Data.Service.IMatchingService>().List(x => x.AcceptNo == mModel.AcceptNo && x.Status == (int)Data.Enum.MatchingStatus.Verified);
            decimal acceptYCJ = acceptMatchlist.Count() > 0 ? acceptMatchlist.Sum(x => x.MatchAmount) : 0;
            if (acceptYCJ + mModel.MatchAmount >= aModel.ExchangeAmount)//全部成交
            {
                aModel.AllDealTime = DateTime.Now;
                aModel.Status = (int)Data.Enum.HelpStatus.AllDeal;
            }
            else
            {
                aModel.Status = (int)Data.Enum.HelpStatus.PartOfDeal;
            }
            MvcCore.Unity.Get<Data.Service.IAcceptHelpService>().Update(aModel);
            MvcCore.Unity.Get<Data.Service.ISysDBTool>().Commit();

            if (sModel.Status == (int)Data.Enum.HelpStatus.AllDeal && sModel.OrderType == 1) //在整个提供订单都完成交易时才结算（主单）
            {
                //提供单会员钱包进帐
                 //Wallets.changeWallet(sModel.UID, (sModel.OrderMoney ?? 0), 2001, "本金收入,来自订单：" + sModel.SupplyNo);
                //if (sModel.Remark.Contains("订单转移给推荐人") || sModel.Remark.Contains("来自会员抢单后创建的新单"))
                //{

                //    UpdateUserWallet((sModel.OrderMoney ?? 0), sModel.SupplyNo, 2001, "本金结算", "来自提供订单【" + sModel.SupplyNo + "】的结算", sModel.UID, sModel.UID, "", false, false, DateTime.Now);
                //}
                //else {
                //    UpdateUserWallet((sModel.OrderMoney ?? 0), sModel.SupplyNo, 2001, "本金结算", "来自提供订单【" + sModel.SupplyNo + "】的结算", sModel.UID, sModel.UID, "", false, false, DateTime.Now);
                //}
                MvcCore.Unity.Get<Data.Service.ISysDBTool>().Commit();//提交到数据库

                //更新用户累计提供
                var updateUser = MvcCore.Unity.Get<Data.Service.IUserService>().Single(mModel.SupplyUID);
                updateUser.AddupSupplyAmount = (updateUser.AddupSupplyAmount ?? 0) + (sModel.OrderMoney ?? 0);
                MvcCore.Unity.Get<Data.Service.IUserService>().Update(updateUser);
                MvcCore.Unity.Get<Data.Service.ISysDBTool>().Commit();

                if (sModel.ReserveInt2 == 1 && MvcCore.Unity.Get<Data.Service.ISupplyHelpService>().List(x => x.ReserveInt2 == 1).Count() == 1)
                {
                    Wallets.changeWallet(sModel.UID, cacheSysParam.SingleAndInit(x => x.ID == 3810).Value.ToDecimal(), 2002, "返还注册激活费用");
                }

                if (sModel.Remark != "来自会员抢单后创建的新单" && sModel.Remark != "订单转移给推荐人")
                {
                    //两个分单完成结算奖金
                    if (MvcCore.Unity.Get<Data.Service.ISupplyHelpService>().List(x => x.ReserveStr2 == sModel.ReserveStr2).Where(x => x.Status == (int)Data.Enum.HelpStatus.AllDeal).Count() == 2)
                    {
                        var bonuslist = MvcCore.Unity.Get<Data.Service.IBonusDetailService>().List(x => x.SupplyNo == sModel.ReserveStr2 && x.IsBalance == false && (x.IsEffective ?? false)).ToList();
                        //结算提供订单产生的直推奖
                        var tjjlist = bonuslist.Where(x => x.BonusID == 1103);
                        foreach (var item in tjjlist)
                        {
                            Wallets.changeWallet(item.UID, item.BonusMoney, 2002, "来自提供单“" + item.SupplyNo + "”的动态奖结算");
                            MvcCore.Unity.Get<Data.Service.ISysDBTool>().Commit();//提交到数据库
                        }
                        //结算完成，更新奖金表（奖金部分）
                        MvcCore.Unity.Get<Data.Service.IBonusDetailService>().Update(new Data.BonusDetail(), new Dictionary<string, string>() { { "IsBalance", "1" }, { "BalanceTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") } }, "SupplyNo='" + sModel.ReserveStr2 + "' and BonusID=1103 and IsBalance=0");
                        MvcCore.Unity.Get<Data.Service.ISysDBTool>().Commit();

                        var list = MvcCore.Unity.Get<Data.Service.ISupplyHelpService>().List(x => x.Status == (int)Data.Enum.HelpStatus.AllDeal && x.ReserveStr2 == sModel.ReserveStr2).ToList();
                        foreach (var item in list)
                        {
                            var supitm = MvcCore.Unity.Get<Data.Service.ISupplyHelpService>().Single(item.ID);
                            //所有本金利息
                            decimal CountMoney = 0;
                            var dbList = MvcCore.Unity.Get<Data.Service.IBonusDetailService>().List(x => (x.BonusID == 1102 || x.BonusID == 2001) && x.IsBalance == false && x.SupplyNo == item.SupplyNo).ToList();
                            if (dbList.Count > 0)
                                CountMoney = dbList.Sum(x => x.BonusMoney);

                            if (CountMoney > 0)
                            { 
                                Wallets.changeWallet(item.UID, CountMoney, 2001, "来自提供单【" + item.SupplyNo + "】利息+本金结算");
                                MvcCore.Unity.Get<Data.Service.ISysDBTool>().Commit();//提交到数据库
                            }
                             supitm.Status = (int)Data.Enum.HelpStatus.AllDeal;//全部成交
                              supitm.ReserveDate1 = DateTime.Now;//完全成交时间
                            //利息生效
                             supitm.IsAccrualEffective = true;

                            MvcCore.Unity.Get<Data.Service.ISupplyHelpService>().Update(supitm);
                            MvcCore.Unity.Get<Data.Service.ISysDBTool>().Commit();//提交到数据库
                            //改变记录的状态
                            MvcCore.Unity.Get<Data.Service.IBonusDetailService>().Update(new Data.BonusDetail(), new Dictionary<string, string>() { { "IsBalance", "1" }, { "BalanceTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }, { "IsEffective", "1" }, { "EffectiveTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") } }, "SupplyNo='" + item.SupplyNo + "' and (BonusID=1102 or BonusID=2001) and IsBalance=0");
                            MvcCore.Unity.Get<Data.Service.ISysDBTool>().Commit();//提交到数据库
                            
                         

                        }

                    }
                }
                else
                {

                    var list = MvcCore.Unity.Get<Data.Service.ISupplyHelpService>().List(x => x.Status == (int)Data.Enum.HelpStatus.AllDeal && x.SupplyNo == sModel.SupplyNo).ToList();
                    foreach (var item in list)
                    {

                        var supitm = MvcCore.Unity.Get<Data.Service.ISupplyHelpService>().Single(item.ID);
                        //所有本金利息
                        decimal CountMoney = 0;
                        var dbList = MvcCore.Unity.Get<Data.Service.IBonusDetailService>().List(x => (x.BonusID == 1102 || x.BonusID==2001) && x.IsBalance == false && x.SupplyNo == item.SupplyNo).ToList();
                        if (dbList.Count > 0)
                            CountMoney = dbList.Sum(x => x.BonusMoney);

                        if (CountMoney > 0)
                        {
                            Wallets.changeWallet(item.UID, CountMoney, 2001, "来自提供单【" + item.SupplyNo + "】利息+本金结算");
                            MvcCore.Unity.Get<Data.Service.ISysDBTool>().Commit();//提交到数据库
                        }
                        supitm.Status = (int)Data.Enum.HelpStatus.AllDeal;//全部成交
                        supitm.ReserveDate1 = DateTime.Now;//完全成交时间
                        //改变记录的状态
                        MvcCore.Unity.Get<Data.Service.IBonusDetailService>().Update(new Data.BonusDetail(), new Dictionary<string, string>() { { "IsBalance", "1" }, { "BalanceTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }, { "IsEffective", "1" }, { "EffectiveTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") } }, "SupplyNo='" + item.SupplyNo + "' and (BonusID=1102 or BonusID=2001) and IsBalance=0");
                        MvcCore.Unity.Get<Data.Service.ISysDBTool>().Commit();//提交到数据库
                        //利息生效
                        supitm.IsAccrualEffective = true;
                        MvcCore.Unity.Get<Data.Service.ISupplyHelpService>().Update(supitm);
                     
                        MvcCore.Unity.Get<Data.Service.ISysDBTool>().Commit();//提交到数据库

                        //排单币奖励
                        var list1109 = MvcCore.Unity.Get<Data.Service.IBonusDetailService>().List(x => x.BonusID == 1109 && x.IsBalance == false && x.SupplyNo == item.SupplyNo).ToList();
                        foreach (var itm in list1109)
                        {

                            Wallets.changeWallet(itm.UID, itm.BonusMoney, 2003, "抢单" + itm.SupplyNo + "获得排单币奖励");
                            MvcCore.Unity.Get<Data.Service.ISysDBTool>().Commit();//提交到数据库

                            MvcCore.Unity.Get<Data.Service.IBonusDetailService>().Update(new Data.BonusDetail(), new Dictionary<string, string>() { { "IsBalance", "1" }, { "BalanceTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }, { "IsEffective", "1" }, { "EffectiveTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") } }, "SupplyNo='" + itm.SupplyNo + "' and BonusID=1109 and IsBalance=0");
                            MvcCore.Unity.Get<Data.Service.ISysDBTool>().Commit();//提交到数据库


                        }

                    }
                }

            }

            //更新匹配单最后成交时间
            var updateMatch = MvcCore.Unity.Get<Data.Service.IMatchingService>().Single(mModel.ID);
            updateMatch.AllDealTime = DateTime.Now;
            MvcCore.Unity.Get<Data.Service.IMatchingService>().Update(updateMatch);
            MvcCore.Unity.Get<Data.Service.ISysDBTool>().Commit();
        }

        #endregion

        #region 到期时结算利息及奖金
        /// <summary>
        /// 到期时结算利息及奖金
        /// </summary>
        public static void ExpireBonus(Data.User onUser)
        {
            List<Data.SysParam> cacheSysParam = MvcCore.Unity.Get<JN.Data.Service.ISysParamService>().List(x => x.ID < 4000).ToList();

            var bonuslist = MvcCore.Unity.Get<Data.Service.IBonusDetailService>().List(x => x.UID == onUser.ID && x.IsBalance == true && (x.IsEffective ?? false) == false && SqlFunctions.DateDiff("minute", (x.EffectiveTime ?? DateTime.Now), DateTime.Now) >= 0);
            decimal PARAM_SZQBBL = cacheSysParam.SingleAndInit(x => x.ID == 2005).Value2.ToDecimal();// 进入加密数字资产钱包比例

            //本金
            //decimal bonusmoney1101 = bonuslist.Where(x => x.BonusID == 1101).Count() > 0 ? bonuslist.Where(x => x.BonusID == 1101).Sum(x => x.BonusMoney) : 0;
            //if (bonusmoney1101 > 0)
            //{
            //    Wallets.changeWallet(onUser.ID, bonusmoney1101, 2001, "“" + DateTime.Now.ToShortDateString() + "”的到期本金");
            //    //结算完成，更新奖金表（奖金部分）
            //    MvcCore.Unity.Get<Data.Service.IBonusDetailService>().Update(new Data.BonusDetail(), new Dictionary<string, string>() { { "IsEffective", "1" } },
            //    "UID='" + onUser.ID + "' and BonusID=1101 and IsBalance=1 and IsEffective=0 and datediff(minute,ISNULL(EffectiveTime, getdate()),getdate()) >= 0");
            //    MvcCore.Unity.Get<Data.Service.ISysDBTool>().Commit();
            //}

            //利息
            int bonusid = 1102;
            decimal bonusmoney = bonuslist.Where(x => x.BonusID == bonusid).Count() > 0 ? bonuslist.Where(x => x.BonusID == bonusid).Sum(x => x.BonusMoney) : 0;
            if (bonusmoney > 0)
            {
                Wallets.changeWallet(onUser.ID, bonusmoney, 2001, "“" + DateTime.Now.ToShortDateString() + "”到期利息");
                //结算完成，更新奖金表（奖金部分）
                MvcCore.Unity.Get<Data.Service.IBonusDetailService>().Update(new Data.BonusDetail(), new Dictionary<string, string>() { { "IsEffective", "1" } },
                "UID=" + onUser.ID + " and BonusID=" + bonusid + " and IsBalance=1 and IsEffective=0 and datediff(minute,ISNULL(EffectiveTime, getdate()),getdate()) >= 0");
                MvcCore.Unity.Get<Data.Service.ISysDBTool>().Commit();
            }

            //推荐奖
            bonusid = 1103;
            bonusmoney = bonuslist.Where(x => x.BonusID == bonusid).Count() > 0 ? bonuslist.Where(x => x.BonusID == bonusid).Sum(x => x.BonusMoney) : 0;
            if (bonusmoney > 0)
            {
                Wallets.changeWallet(onUser.ID, bonusmoney * PARAM_SZQBBL, 2005, "“" + DateTime.Now.ToShortDateString() + "”到期推荐奖");
                Wallets.changeWallet(onUser.ID, bonusmoney * (1 - PARAM_SZQBBL), 2003, "“" + DateTime.Now.ToShortDateString() + "”到期推荐奖");

                //结算完成，更新奖金表（奖金部分）
                MvcCore.Unity.Get<Data.Service.IBonusDetailService>().Update(new Data.BonusDetail(), new Dictionary<string, string>() { { "IsEffective", "1" } },
                "UID=" + onUser.ID + " and BonusID=" + bonusid + " and IsBalance=1 and IsEffective=0 and datediff(minute,ISNULL(EffectiveTime, getdate()),getdate()) >= 0");
                MvcCore.Unity.Get<Data.Service.ISysDBTool>().Commit();
            }

            //管理奖
            bonusid = 1104;
            bonusmoney = bonuslist.Where(x => x.BonusID == bonusid).Count() > 0 ? bonuslist.Where(x => x.BonusID == bonusid).Sum(x => x.BonusMoney) : 0;
            if (bonusmoney > 0)
            {
                Wallets.changeWallet(onUser.ID, bonusmoney * PARAM_SZQBBL, 2005, "“" + DateTime.Now.ToShortDateString() + "”到期管理奖");
                Wallets.changeWallet(onUser.ID, bonusmoney * (1 - PARAM_SZQBBL), 2003, "“" + DateTime.Now.ToShortDateString() + "”到期管理奖");

                //结算完成，更新奖金表（奖金部分）
                MvcCore.Unity.Get<Data.Service.IBonusDetailService>().Update(new Data.BonusDetail(), new Dictionary<string, string>() { { "IsEffective", "1" } },
                "UID=" + onUser.ID + " and BonusID=" + bonusid + " and IsBalance=1 and IsEffective=0 and datediff(minute,ISNULL(EffectiveTime, getdate()),getdate()) >= 0");
                MvcCore.Unity.Get<Data.Service.ISysDBTool>().Commit();
            }

            //诚信奖
            bonusid = 1105;
            bonusmoney = bonuslist.Where(x => x.BonusID == bonusid).Count() > 0 ? bonuslist.Where(x => x.BonusID == bonusid).Sum(x => x.BonusMoney) : 0;
            if (bonusmoney > 0)
            {
                Wallets.changeWallet(onUser.ID, bonusmoney, 2001, "“" + DateTime.Now.ToShortDateString() + "”到期诚信奖");

                //结算完成，更新奖金表（奖金部分）
                MvcCore.Unity.Get<Data.Service.IBonusDetailService>().Update(new Data.BonusDetail(), new Dictionary<string, string>() { { "IsEffective", "1" } },
                "UID=" + onUser.ID + " and BonusID=" + bonusid + " and IsBalance=1 and IsEffective=0 and datediff(minute,ISNULL(EffectiveTime, getdate()),getdate()) >= 0");
                MvcCore.Unity.Get<Data.Service.ISysDBTool>().Commit();
            }

            //经理奖
            //bonusid = 1108;
            //bonusmoney = bonuslist.Where(x => x.BonusID == bonusid).Count() > 0 ? bonuslist.Where(x => x.BonusID == bonusid).Sum(x => x.BonusMoney) : 0;
            //if (bonusmoney > 0)
            //{
            //    Wallets.changeWallet(onUser.ID, bonusmoney, 2002, "“" + DateTime.Now.ToShortDateString() + "”到期管理奖");
            //    //结算完成，更新奖金表（奖金部分）
            //    MvcCore.Unity.Get<Data.Service.IBonusDetailService>().Update(new Data.BonusDetail(), new Dictionary<string, string>() { { "IsEffective", "1" } },
            //    "UID='" + onUser.ID + "' and BonusID=" + bonusid + " and IsBalance=1 and IsEffective=0 and datediff(minute,ISNULL(EffectiveTime, getdate()),getdate()) >= 0");
            //    MvcCore.Unity.Get<Data.Service.ISysDBTool>().Commit();
            //}
        }
        #endregion


        #region 解冻本金和利息
        /// <summary>
        /// 解冻本金和利息
        /// </summary>
        public static void JIEDONGBENJILIXI()
        {

            //找出所有到时间的提供单（状态为成交未解冻并且解冻时间到期的）
            var SupplyModelList = MvcCore.Unity.Get<Data.Service.ISupplyHelpService>().List(x => x.Status == (int)Data.Enum.HelpStatus.AllDeal && x.ReserveDate2 < DateTime.Now && x.ReserveInt2 == 2).ToList();  //找出分单第二单已完成的
            foreach (var item in SupplyModelList)
            {

                var list = MvcCore.Unity.Get<Data.Service.ISupplyHelpService>().List(x => x.Status == (int)Data.Enum.HelpStatus.AllDeal && x.ReserveDate2 < DateTime.Now && x.ReserveStr2 == item.ReserveStr2).ToList();
                foreach (var itm in list)
                {
                    var items = MvcCore.Unity.Get<Data.Service.ISupplyHelpService>().Single(itm.ID);

                    //查找全部利息
                    decimal CountMoney = 0;
                    var dbList = MvcCore.Unity.Get<Data.Service.IBonusDetailService>().List(x => (x.BonusID == 1102 || x.BonusID == 2001) && x.IsBalance == false && x.SupplyNo == items.SupplyNo).ToList();
                    if (dbList.Count > 0)
                        CountMoney = dbList.Sum(x => x.BonusMoney);

                    //解冻
                    using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
                    {
                        if (CountMoney > 0)
                            Wallets.changeWallet(items.UID, CountMoney, 2001, "来自提供单【" + items.SupplyNo + "】本金+利息解冻");
                        items.Status = (int)Data.Enum.HelpStatus.AllDeal;//全部成交
                        items.ReserveDate1 = DateTime.Now;//完全成交时间
                        //改变记录的状态
                        MvcCore.Unity.Get<Data.Service.IBonusDetailService>().Update(new Data.BonusDetail(), new Dictionary<string, string>() { { "IsBalance", "1" }, { "BalanceTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }, { "IsEffective", "1" }, { "EffectiveTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") } }, "SupplyNo='" + items.SupplyNo + "' and (BonusID=1102 or BonusID=2001) and IsBalance=0");
                        MvcCore.Unity.Get<Data.Service.ISysDBTool>().Commit();//提交到数据库

                        //停止利息发放(未用)
                        //items.IsAccruaCount =false;
                        //利息生效
                        items.IsAccrualEffective = true;

                        MvcCore.Unity.Get<Data.Service.ISupplyHelpService>().Update(items);
                        MvcCore.Unity.Get<Data.Service.ISysDBTool>().Commit();//提交到数据库

                        ts.Complete();//此为事物提交
                    }
                }

            }
        }
        #endregion

    }
}
using System;
using System.Collections.Generic;
using System.Linq;
namespace JN.Services.Manager
{
	/// <summary>
	 	///钱包管理
		/// </summary>
    public partial class Wallets
    {

        #region 币种操作
        /// <summary>
        /// 直接对币种进行操作
        /// </summary>
        /// <param name="onUser">操作对像</param>
        /// <param name="changeMoney">变更金额</param>
        /// <param name="changeCoin">变更币种</param>
        /// <param name="description">备注</param>
        public static void changeWallet(int onUserID, decimal changeMoney, int coinid, string description)
        {
            List<Data.SysParam> cacheSysParam = MvcCore.Unity.Get<JN.Data.Service.ISysParamService>().List(x => x.ID < 4000).ToList();

            Data.User onUser = MvcCore.Unity.Get<Data.Service.IUserService>().Single(onUserID);
            decimal changeWallet = 0;
            switch (coinid)
            {
                case 2001:
                    changeWallet = onUser.Wallet2001;
                    onUser.Wallet2001 = onUser.Wallet2001 + changeMoney;
                    break;
                case 2002:
                    changeWallet = onUser.Wallet2002;
                    onUser.Wallet2002 = onUser.Wallet2002 + changeMoney;
                    break;
                case 2003:
                    changeWallet = onUser.Wallet2003;
                    onUser.Wallet2003 = onUser.Wallet2003 + changeMoney;
                    break;
                case 2004:
                    changeWallet = (onUser.Wallet2004 ?? 0);
                    onUser.Wallet2004 = (onUser.Wallet2004 ?? 0) + changeMoney;
                    break;
                case 2005:
                    changeWallet = (onUser.Wallet2005 ?? 0);
                    onUser.Wallet2005 = (onUser.Wallet2005 ?? 0) + changeMoney;
                    break;
            }

            //写入明细
            MvcCore.Unity.Get<JN.Data.Service.IWalletLogService>().Add(new Data.WalletLog
            {
                ChangeMoney = changeMoney,
                Balance = changeWallet + changeMoney,
                CoinID = coinid,
                CoinName = cacheSysParam.Single(x => x.ID == coinid).Value,
                CreateTime = DateTime.Now,
                Description = description,
                UID = onUser.ID,
                UserName = onUser.UserName
            });
            MvcCore.Unity.Get<Data.Service.ISysDBTool>().Commit();
            //更新用户钱包
            MvcCore.Unity.Get<JN.Data.Service.IUserService>().Update(onUser);
            MvcCore.Unity.Get<Data.Service.ISysDBTool>().Commit();
        }
        #endregion

        #region 币种互兑
        /// <summary>
        /// 互兑
        /// </summary>
        public static void doExchange(int onUserID, decimal drawmoney, int fromCoin, int toCoin)
        {
            List<Data.SysParam> cacheSysParam = MvcCore.Unity.Get<JN.Data.Service.ISysParamService>().List(x => x.ID < 4000).ToList();

            Data.User onUser = MvcCore.Unity.Get<Data.Service.IUserService>().Single(onUserID);
            //币种
            decimal PARAM_POUNDAGEBL = cacheSysParam.Single(x => x.ID == 1903).Value2.ToDecimal(); //手续费

            decimal actualChangeMoney = drawmoney * (1 - PARAM_POUNDAGEBL);
            decimal poundage = drawmoney * PARAM_POUNDAGEBL;
            string description = "兑换";
            Wallets.changeWallet(onUser.ID, 0 - actualChangeMoney, fromCoin, description);
            Wallets.changeWallet(onUser.ID, actualChangeMoney, toCoin, description);
        }
        #endregion


    }
}
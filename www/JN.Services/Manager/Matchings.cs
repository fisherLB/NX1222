using System;
using System.Linq;
namespace JN.Services.Manager
{
    public partial class Matchings
    {
        #region 生成随机编号
        //生成随机编号
        public static string GetOrderNumber()
        {
            DateTime dateTime = DateTime.Now;
            string result = "R" + Tool.StringHelp.GetRandomNumber(7);//7位随机数字
            //int maxid = MvcCore.Unity.Get<JN.Data.Service.IMatchingService>().List().Count() > 0 ? MvcCore.Unity.Get<JN.Data.Service.IMatchingService>().List().Max(x => x.MatchingNo.Substring(x.AcceptNo.Length - 7)).ToInt() : 0;
            //if (maxid < 10000) maxid = 10000;
            //result += (maxid + 1).ToString().PadLeft(7, '0');
            if (IsHave(result))
            {
                return GetOrderNumber();
            }
            return result;
        }

        //检查订单号是否重复
        private static bool IsHave(string number)
        {
            return MvcCore.Unity.Get<JN.Data.Service.IMatchingService>().List(x => x.MatchingNo == number).Count() > 0;
        }
        #endregion

    }
}
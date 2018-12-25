using System;
using System.Linq;
namespace JN.Services.Manager
{
    public partial class SupplyHelps
    {
        #region 生成真实订单号
        public static string GetSupplyNo()
        {
            DateTime dateTime = DateTime.Now;
            string result = "S";

            int maxid = MvcCore.Unity.Get<Data.Service.ISupplyHelpService>().List().Count() > 0 ? MvcCore.Unity.Get<Data.Service.ISupplyHelpService>().List().Max(x => x.SupplyNo.Substring(x.SupplyNo.Length - 7)).ToInt() : 0;
            // TypeConverter.ObjectToInt(supplyhelps.GetFieldValue("ISNULL(MAX(RIGHT(SupplyNo,7)),0)", "1=1"));
            if (maxid < 10000) maxid = 10000;
            result += (maxid + 1).ToString().PadLeft(7, '0');
            if (IsHaveSupplyNo(result))
            {
                return GetSupplyNo();
            }
            return result;
        }

        public static string GetSupplyNo2()
        {
            DateTime dateTime = DateTime.Now;
            string result = "Y";

            int maxid = MvcCore.Unity.Get<Data.Service.ISupplyHelpService>().List().Count() > 0 ? MvcCore.Unity.Get<Data.Service.ISupplyHelpService>().List().Max(x => x.SupplyNo.Substring(x.SupplyNo.Length - 7)).ToInt() : 0;
            if (maxid < 10000) maxid = 10000;
            result += (maxid + 1).ToString().PadLeft(7, '0');
            if (IsHaveSupplyNo(result))
            {
                return GetSupplyNo2();
            }
            return result;
        }

        //检查订单号是否重复
        private static bool IsHaveSupplyNo(string number)
        {
            return MvcCore.Unity.Get<Data.Service.ISupplyHelpService>().List(x => x.SupplyNo == number).Count() > 0;
        }
        #endregion

    }
}
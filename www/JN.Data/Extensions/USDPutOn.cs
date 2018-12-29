using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JN.Data.Extensions
{
    /// <summary>
    /// CellOrders
    /// </summary>
    public class USDPutOn
    {
        #region 生成真实订单号
        public static string GetPutOnNumber()
        {
            DateTime dateTime = DateTime.Now;
            string result = "P";

            int maxid = MvcCore.Unity.Get<Data.Service.IUSDPutOnService>().List().Count() > 0 ? MvcCore.Unity.Get<Data.Service.IUSDPutOnService>().List().Max(x => x.PutonNumber.Substring(x.PutonNumber.Length - 7)).ToInt() : 0;
            // TypeConverter.ObjectToInt(supplyhelps.GetFieldValue("ISNULL(MAX(RIGHT(SupplyNo,7)),0)", "1=1"));
            if (maxid < 10000) maxid = 10000;
            result += (maxid + 1).ToString().PadLeft(7, '0');
            if (IsHaveNumber(result))
            {
                return GetPutOnNumber();
            }
            return result;
        }

        //检查订单号是否重复
        private static bool IsHaveNumber(string number)
        {
            return MvcCore.Unity.Get<Data.Service.IUSDPutOnService>().List(x => x.PutonNumber == number).Count() > 0;
        }
        #endregion
    }
}

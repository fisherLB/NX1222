using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JN.Data.Extensions
{
    /// <summary>
    /// CellOrders
    /// </summary>
    public class SeekNumber
    {
        #region 生成真实订单号
        public static string GetSeekNumber()
        {
            DateTime dateTime = DateTime.Now;
            string result = "S";

            int maxid = MvcCore.Unity.Get<Data.Service.IUSDSeekService>().List().Count() > 0 ? MvcCore.Unity.Get<Data.Service.IUSDSeekService>().List().Max(x => x.SeekNumber.Substring(x.SeekNumber.Length - 7)).ToInt() : 0;
            // TypeConverter.ObjectToInt(supplyhelps.GetFieldValue("ISNULL(MAX(RIGHT(SupplyNo,7)),0)", "1=1"));
            if (maxid < 10000) maxid = 10000;
            result += (maxid + 1).ToString().PadLeft(7, '0');
            if (IsHaveNumber(result))
            {
                return GetSeekNumber();
            }
            return result;
        }

        //检查订单号是否重复
        private static bool IsHaveNumber(string number)
        {
            return MvcCore.Unity.Get<Data.Service.IUSDSeekService>().List(x => x.SeekNumber == number).Count() > 0;
        }
        #endregion
    }
}

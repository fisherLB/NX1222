using System;
using System.Linq;
namespace JN.Services.Manager
{
    public partial class AcceptHelps
    {
        #region 生成真实订单号
        public static string GetAcceptNo()
        {
            DateTime dateTime = DateTime.Now;
            string result = "A";
            int maxid = MvcCore.Unity.Get<Data.Service.IAcceptHelpService>().List().Count() > 0 ? MvcCore.Unity.Get<Data.Service.IAcceptHelpService>().List().Max(x => x.AcceptNo.Substring(x.AcceptNo.Length - 7)).ToInt() : 0;
            if (maxid < 10000) maxid = 10000;
            result += (maxid + 1).ToString().PadLeft(7, '0');
            if (IsHaveAcceptNo(result))
            {
                return GetAcceptNo();
            }
            return result;
        }

        //检查订单号是否重复
        private static bool IsHaveAcceptNo(string number)
        {
            return MvcCore.Unity.Get<Data.Service.IAcceptHelpService>().List(x => x.AcceptNo == number).Count() > 0;
        }
        #endregion

    }
}
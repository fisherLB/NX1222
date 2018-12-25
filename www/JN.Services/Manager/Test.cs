using System;
using System.Collections.Generic;
using System.Linq;
namespace JN.Services.Manager
{
    /// <summary>
    /// 各种调用范例
    /// </summary>
    public partial class Test
    {
        void test()
        {
            //直接执行SQL语句（带参数）
            var parameters = new[]{
                new System.Data.SqlClient.SqlParameter(){ ParameterName="period", Value=1 }
            };
            MvcCore.Unity.Get<Data.Service.ISysDBTool>().ExecuteSQL("update CFBSubscribe set ApplyNumber=ApplyNumber*2,SubscribeNumber=ISNULL(SubscribeNumber,0)*2 where status<=1 and period=@period", parameters);
            MvcCore.Unity.Get<Data.Service.ISysDBTool>().Commit();
        }

        void test2() //取得一个ID集
        {
            List<int> ids = MvcCore.Unity.Get<Data.Service.IUserService>().List().Select(x => x.ID).ToList();

            //以,拼接
            //string.Join(",", ids.Select(x => x.ID).ToList());
        }
    }
}
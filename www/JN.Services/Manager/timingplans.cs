using JN.Services.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web;
using JN.Services.Tool;
using System.Transactions;

namespace JN.BLL
{
    /// <summary>
    ///定时计划任务
    /// </summary>
    public partial class timingplans
    {
      
        ////定时计划1
        //public static bool plan1()
        //{
        //    #region 事务操作
        //    try
        //    {
        //        using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
        //        {

        //            if ((int)DateTime.Now.DayOfWeek != 6 && (int)DateTime.Now.DayOfWeek != 7)
        //            {
        //                users.CalculateFH(0);

        //           // users.DayBalance();
        //            ts.Complete();
        //            }
        //        }
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        string str = "访问类名:" + MethodBase.GetCurrentMethod().DeclaringType.FullName + "\r\n执行方法:" + MethodBase.GetCurrentMethod().Name + "\r\n";
        //        logs.WriteErrorLog(str, ex);
        //        return false;
        //    }
        //    #endregion
        //}

      
        //定时计划1
        public static bool plan1()
        {
            return threadproc();
        }

        private static bool threadproc()
        {
            bool b = false;
            object obj = new object();
            using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,new TimeSpan(0,30,0)))
            {
                lock (obj)
                {
                    try
                    {
                        MMM.CalculateFLXZD(0);
                        b = true;
                    }
                    catch (Exception ex)
                    {
                        string str = "访问地址：" + HttpContext.Current.Request.Url.PathAndQuery + "\r\n访问类名:" + MethodBase.GetCurrentMethod().DeclaringType.FullName + "\r\n执行方法:" + MethodBase.GetCurrentMethod().Name + "\r\n";
                        b = false;
                       //logs.WritePlanLog("自动抢单过程出错：" + str);
                    }
                    ts.Complete();
                    return b;
                    
                }
            }
        }

        //定时计划2
        public static bool plan2()
        {
            try
            {
                MMM.CheckPayEndTime();
                MMM.CheckVerifiedEndTime();
                //Bonus.JIEDONGBENJILIXI();//自动解冻本机和利息
                Users.UpdateUser();

                return true;
            }
            catch (Exception xp)
            {
                return false;
            }
        }

        //定时计划3
        public static bool plan3()
        {
            return false;
        }

        //定时计划4
        public static bool plan4()
        {
            return false;
        }

        //定时计划5
        public static bool plan5()
        {
            return false;
        }
    }
}

using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MvcCore;
using JN.Data;

namespace JN.Web
{
    public static class MvcCoreConfig
    {

        /*
         * 此处须在 /Global.asax.cs 文件中的 Application_Start() 函数中添加一行代码：
         * 
         * MvcCoreConfig.RegisterMvcCore();
         * 
         */
        public static void RegisterMvcCore()
        {
            MvcCore.UnityHelper.Init(RegisterType);
        }


        /*
         * 
         * 该函数需放到 /Global.asax.cs 文件中的 Application_EndRequest(Object sender, EventArgs e) 函数中。
         * 
         *         protected void Application_EndRequest(Object sender, EventArgs e)
         *         {
         *             MvcCoreConfig.Request_End();
         *         }
         * 
         */
        public static void Request_End()
        {
            //有多个数据库的，请将不同的数据库工厂都释放掉，否则将会造成内存暴增的情况。

            MvcCore.UnityHelper.RequestEnd<ISysDbFactory>();
           // MvcCore.UnityHelper.RequestEnd<ILogDbFactory>();
        }

        /// <summary>
        /// 注册 IoC 映射
        /// </summary>
        /// <param name="container"></param>
        private static void RegisterType(IUnityContainer container)
        {
            /*
             * 
             * 这里注册的接口与实现类，要取出来有两种方式
             * 1.继承有 Controller 或者 ApiController 的控制器、接口，在构造函数上加上要取出的接口类型的参数，构造函数的参数类型、数量、顺序不限制。
             * 2.通过 MvcCore.Unity.Get<T>() 取出对应的接口，这里的泛型 T 既为需要取出的接口类型。
             * 
             * 注意:以上两种方式取出的接口必须是下面绑定的接口！
             * 
             */


            container
            #region 基础绑定
                //系统库
                .Bind<ISysDbFactory, SysDbFactory>()
                //日志库
               // .Bind<ILogDbFactory, LogDbFactory>()
            #endregion

            #region 数据层绑定
.LoadAssemblyAndBind("JN.Data", "JN.Data.Service")
            #endregion
;

        }
    }
}
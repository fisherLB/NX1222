using JN.Data.Enum;
using JN.Services.Manager;
using JN.Services.Tool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Security;

namespace JN.Services
{
    public static class UserLoginHelper
    {
        //登录缓存键前缀
        /// <summary>
        /// 登录缓存键前缀
        /// </summary>
        static string cacheUserName { get { return typeof(Data.User).Name + "_"; } }
        static Data.User _adminloginuser = null;
        static string CookieName_User = (ConfigHelper.GetConfigString("DBName") + "_User").ToMD5();
        static string CookieName_Tonen = (ConfigHelper.GetConfigString("DBName") + "_Tonen").ToMD5();

        public static Data.User GetUserLoginBy(string keyword, string password)
        {

            string pwd = password.ToMD5().ToMD5();
            var account = MvcCore.Unity.Get<Data.Service.IUserService>().Single(a => a.UserName == keyword.Trim() && a.Password == pwd);
            if (account != null)
            {
                MvcCore.Extensions.CookieExtensions.WriteCookie(CookieName_User, keyword, 60);
                MvcCore.Extensions.CookieExtensions.WriteCookie(CookieName_Tonen, keyword.ToMD5() + "" + pwd, 60);
                return account;
            }
            return null;
        }

        public static Data.User GetSysLoginBy(Data.User account, Data.AdminUser adminuser)
        {
            if (adminuser != null && adminuser.IsPassed)
            {
                _adminloginuser = account;
                if (account != null)
                {
                    MvcCore.Extensions.CookieExtensions.WriteCookie(CookieName_User, account.UserName, 60);
                    MvcCore.Extensions.CookieExtensions.WriteCookie(CookieName_Tonen, account.UserName.ToMD5() + "" + account.Password, 60);
                    return account;
                }
                return account;
            }
            return null;
        }

        //获取当前会员登录对象
        /// <summary>
        /// 获取当前会员登录对象
        /// <para>当没登陆或者登录信息不符时，这里返回 null </para>
        /// </summary>
        /// <returns></returns>
        public static Data.User CurrentUser()
        {
            //校验用户是否已经登录
            var user = HttpContext.Current.Session[CookieName_User] as Data.User;
            if (user != null) return user;
            else
            {
                if (HttpContext.Current.Request.Cookies[CookieName_User] != null && HttpContext.Current.Request.Cookies[CookieName_Tonen] != null)
                {
                    string keyword = HttpContext.Current.Request.Cookies[CookieName_User].Value;
                    string token = HttpContext.Current.Request.Cookies[CookieName_Tonen].Value;
                    string pwd = token.Substring(32);
                    var account = MvcCore.Unity.Get<Data.Service.IUserService>().Single(a => a.UserName == keyword.Trim() && a.Password == pwd);
                    if (account != null) return account;
                }
            }
            return null;
        }

        //登出
        /// <summary>
        /// 登出
        /// </summary>
        public static void UserLogout()
        {
            if (CheckUserLogin())
            {
                //获取会员ID
                var id = HttpContext.Current.User.Identity.Name;
                FormsAuthentication.SignOut();
                _adminloginuser = null;
                RemoveUser(id);
            }
        }

        //移除指定会员ID的登录缓存
        /// <summary>
        /// 移除指定会员ID的登录缓存
        /// </summary>
        /// <param name="ID"></param>
        public static void RemoveUser(string ID)
        {
            //MvcCore.Extensions.CacheExtensions.ClearCache(cacheUserName + ID);

            HttpContext.Current.Session.Clear();
            HttpCookie hc1 = HttpContext.Current.Request.Cookies[CookieName_User];
            hc1.Expires = DateTime.Now.AddDays(-1);
            HttpContext.Current.Response.Cookies.Add(hc1);

            HttpCookie hc2 = HttpContext.Current.Request.Cookies[CookieName_Tonen];
            hc2.Expires = DateTime.Now.AddDays(-1);
            HttpContext.Current.Response.Cookies.Add(hc2);
        }

        //判断当前访问是否有会员登录
        /// <summary>
        /// 判断当前访问是否有会员登录
        /// </summary>
        /// <returns></returns>
        public static bool CheckUserLogin()
        {

            var user = HttpContext.Current.Session[CookieName_User] as Data.AdminUser;
            if (user != null)
            {
                return true;
            }
            else
            {
                if (HttpContext.Current.Request.Cookies[CookieName_User] != null && HttpContext.Current.Request.Cookies[CookieName_Tonen] != null)
                {
                    string keyword = HttpContext.Current.Request.Cookies[CookieName_User].Value;
                    string token = HttpContext.Current.Request.Cookies[CookieName_Tonen].Value;
                    string pwd = token.Substring(32);
                    var account = MvcCore.Unity.Get<Data.Service.IUserService>().Single(a => a.UserName == keyword.Trim() && a.Password == pwd);
                    if (account != null) return true;
                }
            }
            return false;
        }

        //当前在线会员数量
        /// <summary>
        /// 当前在线会员数量
        /// </summary>
        public static int UserCount
        {
            get
            {
                return MvcCore.Extensions.CacheExtensions.GetAllCache().Where(s => s.StartsWith(cacheUserName)).Count();
            }
        }
    }

    public static class AdminLoginHelper
    {
        //登录缓存键前缀
        /// <summary>
        /// 登录缓存键前缀
        /// </summary>
        static string cacheAdminName { get { return typeof(Data.AdminUser).Name + "_"; } }
        static string CookieName_AdUser = (ConfigHelper.GetConfigString("DBName") + "_AdUser").ToMD5();
        static string CookieName_AdTonen = (ConfigHelper.GetConfigString("DBName") + "_AdTonen").ToMD5();
        public static Data.AdminUser GetAdminLoginBy(string keyword, string password)
        {
            string pwd = password.ToMD5().ToMD5();
            var account = MvcCore.Unity.Get<Data.Service.IAdminUserService>().Single(a => a.AdminName == keyword.Trim() && a.Password == pwd);
            if (account != null)
            {
                //MvcCore.Extensions.CacheExtensions.SetCache(cacheAdminName + account.ID, account, MvcCore.Extensions.CacheTimeType.ByMinutes, 60);//写入缓存
                MvcCore.Extensions.CookieExtensions.WriteCookie(CookieName_AdUser, keyword, 30);
                MvcCore.Extensions.CookieExtensions.WriteCookie(CookieName_AdTonen, keyword.ToMD5() + "" + pwd, 30);
                return account;
            }
            return null;
        }

        //获取当前会员登录对象
        /// <summary>
        /// 获取当前会员登录对象
        /// <para>当没登陆或者登录信息不符时，这里返回 null </para>
        /// </summary>
        /// <returns></returns>
        public static Data.AdminUser CurrentAdmin()
        {
            if (!CheckAdminLogin()) return null;

            //校验用户是否已经登录
            var adminuser = HttpContext.Current.Session[CookieName_AdUser] as Data.AdminUser;
            if (adminuser != null) return adminuser;
            else
            {
                if (HttpContext.Current.Request.Cookies[CookieName_AdUser] != null && HttpContext.Current.Request.Cookies[CookieName_AdTonen] != null)
                {
                    string keyword = HttpContext.Current.Request.Cookies[CookieName_AdUser].Value;
                    string token = HttpContext.Current.Request.Cookies[CookieName_AdTonen].Value;
                    string pwd = token.Substring(32);
                    var account = MvcCore.Unity.Get<Data.Service.IAdminUserService>().Single(a => a.AdminName == keyword.Trim() && a.Password == pwd);
                    if (account != null) return account;
                }
            }
            return null;
        }

        //登出
        /// <summary>
        /// 登出
        /// </summary>
        public static void AdminUserLogout()
        {
            if (CheckAdminLogin())
            {
                HttpContext.Current.Session.Clear();

                HttpCookie hc1 = HttpContext.Current.Request.Cookies[CookieName_AdUser];
                hc1.Expires = DateTime.Now.AddDays(-1);
                HttpContext.Current.Response.Cookies.Add(hc1);

                HttpCookie hc2 = HttpContext.Current.Request.Cookies[CookieName_AdTonen];
                hc2.Expires = DateTime.Now.AddDays(-1);
                HttpContext.Current.Response.Cookies.Add(hc2);
            }
        }


        //判断当前访问是否有会员登录
        /// <summary>
        /// 判断当前访问是否有会员登录
        /// </summary>
        /// <returns></returns>
        public static bool CheckAdminLogin()
        {
            //校验用户是否已经登录
            var adminuser = HttpContext.Current.Session[CookieName_AdUser] as Data.AdminUser;
            if (adminuser != null)
            {
                return true;
            }
            else
            {
                if (HttpContext.Current.Request.Cookies[CookieName_AdUser] != null && HttpContext.Current.Request.Cookies[CookieName_AdTonen] != null)
                {
                    string keyword = HttpContext.Current.Request.Cookies[CookieName_AdUser].Value;
                    string token = HttpContext.Current.Request.Cookies[CookieName_AdTonen].Value;
                    string pwd = token.Substring(32);
                    var account = MvcCore.Unity.Get<Data.Service.IAdminUserService>().Single(a => a.AdminName == keyword.Trim() && a.Password == pwd);
                    if (account != null) return true;
                }
            }

            return false;
        }
    }
}

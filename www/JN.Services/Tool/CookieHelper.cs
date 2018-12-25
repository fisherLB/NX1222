using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Text;
using System.Web;

namespace JN.Services.Tool
{
    public static class CookieHelper
    {

        /// <summary>写Cookie
        /// 
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="content">内容</param>
        /// <param name="day">失效天数</param>
        public static void SetCookie(string key, string content, int day)
        {
            HttpCookie cookie = HttpContext.Current.Response.Cookies[key];
            if (cookie != null)
            {
                cookie.Value = content;
                cookie.Expires = DateTime.Now.AddDays(day);
            }
            else
            {
                HttpCookie nCookie = new HttpCookie(key, content) { Expires = DateTime.Now.AddDays(day) };
                HttpContext.Current.Response.Cookies.Add(nCookie);
            }

        }


     

        /// <summary>清除Cookie
        /// 
        /// </summary>
        /// <param name="key">键</param>
        public static void ClearCookie(string key)
        {
            HttpCookie cookie = HttpContext.Current.Response.Cookies[key];
            if (cookie != null)
            {
                HttpCookie nCookie = new HttpCookie(key) { Expires = DateTime.Now.AddDays(-1) };
                HttpContext.Current.Response.Cookies.Add(nCookie);
                
            }
        }



    }
}

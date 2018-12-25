using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;

using System.Xml.XPath;
using System.Xml;
using JN.Services.Tool;

namespace JN.Web.Framework
{
    //使用的静态类.当然使用单例模式最好.
    public static class ResourceProvider
    {
        /// <summary>
        /// 默认为英文
        /// </summary>
        private const string DefaultCulture = "zh-CN";
        private const string LanguageCahceName = "LanguagePack_";


        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string R(string culture, string name)
        {
            Dictionary<string, string> res = null;

            //语言标签采集模式下自动记录下语言资源项
            if (ConfigHelper.GetConfigBool("CollectLanguage"))
            {
                var langList = MvcCore.Unity.Get<JN.Data.Service.ISysParamService>().ListCache("langList", x => x.PID == 4000 && x.IsUse);
                foreach (var param in langList)
                {
                    if (MvcCore.Unity.Get<Data.Service.ILanguageService>().List(x => x.Name == name && x.LanguageName == param.Value2).Count() <= 0)
                    {
                        string value = "";
                        if (param.Value2 == "zh-CN") value = name;
                        MvcCore.Unity.Get<Data.Service.ILanguageService>().Add(new Data.Language() { LanguageName = param.Value2, Location = HttpContext.Current.Request.Url.AbsolutePath, Name = name, Value = value });
                        MvcCore.Unity.Get<Data.Service.SysDBTool>().Commit();
                    }
                }
            }

            if (HttpRuntime.Cache[LanguageCahceName + culture] == null)//缓存到内存中这样会很快
            {
                if (string.IsNullOrEmpty(culture)) culture = DefaultCulture;
                culture = "zh-CN";
                var fullPath = HttpContext.Current.Server.MapPath("/Language/" + culture + ".xml");
                if (System.IO.File.Exists(fullPath))
                {
                    //var doc = XDocument.Load(fullPath);
                    XmlDocument doc = new XmlDocument();
                    doc.Load(fullPath);
                    var dicts = doc.SelectNodes("dicts/dict");
                    res = new Dictionary<string, string>(dicts.Count);
                    foreach (XmlNode item in dicts)//递归资源
                    {
                        res.Add(item.Attributes["name"].Value, item.Attributes["value"].Value);
                    }
                    //插入缓存依赖于文件本身
                    HttpRuntime.Cache.Insert(LanguageCahceName + culture,
                        res, new System.Web.Caching.CacheDependency(fullPath));
                }
            }
            else
            {
                res = (Dictionary<string, string>)
                    HttpRuntime.Cache[LanguageCahceName + culture];
            }


            if (res.ContainsKey(name))
                return res[name];
            else
                return name;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string R(string name)
        {
            return R(Culture, name);
        }

        private const string CultureCookieName = "Culture";
        /// <summary>
        /// 获取或设置语言配置.(这里使用的是Cookie.当然也能自己实现登录账号的profile配置)
        /// </summary>
        public static string Culture
        {
            get
            {
                var cookie = System.Web.HttpContext.Current.Request.Cookies[CultureCookieName];
                if (cookie == null)
                {
                    return DefaultCulture; // CultureInfo.CurrentCulture.Name;
                }
                return cookie.Value == "cn" ? "zh-CN" : cookie.Value;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    var cookie = HttpContext.Current.Request.Cookies[CultureCookieName];
                    if (cookie != null)
                    {
                        cookie.Expires = DateTime.Now.AddDays(-1);
                        HttpContext.Current.Response.Cookies.Add(cookie);
                    }

                    cookie = new HttpCookie(CultureCookieName, value)
                    {
                        Expires = DateTime.Now.AddYears(1)//有效期一个年
                    };
                    HttpContext.Current.Response.Cookies.Add(cookie);
                }
            }
        }
    }
}
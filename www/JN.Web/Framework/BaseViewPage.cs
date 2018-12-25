using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace JN.Web.Framework
{

    public class BaseViewPage<TModel> : WebViewPage<TModel> where TModel : class
    {
        /// <summary>
        /// 返回 an HtmlString 本地化字符串
        /// </summary>
        /// <param name="neutralValue">The text to be localized</param>
        public virtual IHtmlString T(string args)
        {
            return MvcHtmlString.Create(S(args));
        }


        /// <summary>
        /// 返回本地化字符串
        /// </summary>
        /// <param name="neutralValue">The text to be localized</param>
        public virtual string S(string args)
        {
            if (string.IsNullOrEmpty(Request["lang"]))
            {
                if (!string.IsNullOrEmpty(args))
                {
                    return string.Format(ResourceProvider.R(args), args);
                }
                else
                {
                    return "";
                }
            }
            else
            {
                if (string.IsNullOrEmpty(args))
                    return "";
                else
                    return string.Format(ResourceProvider.R(Request["lang"], args), args);
            }
        }

        ///// <summary>
        ///// 返回本地化字符串
        ///// </summary>
        ///// <param name="neutralValue">The text to be localized</param>
        //public virtual string S(string neutralValue)
        //{
        //    return ResourceProvider.R(neutralValue);
        //}

        public override void Execute()
        {

        }
        /// <summary>
        /// 页面初始化设置UICulture
        /// </summary>
        protected override void InitializePage()
        {
            //设置UICulture

            System.Threading.Thread.CurrentThread.CurrentUICulture = new CultureInfo(ResourceProvider.Culture);
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo(ResourceProvider.Culture);

            base.InitializePage();
        }
    }
}
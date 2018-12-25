using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.Drawing;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Net.Mail;
using System.Text;


namespace JN.Services.Tool
{
    public static class SMSHelper
    {


        #region 网建接口，支持批量
        /// <summary>
        /// 发送手机短信（网建接口，支持批量） http://www.smschinese.cn/
        /// </summary>
        /// <param name="mobile">手机号码,多个手机号以,号相隔</param>
        /// <param name="body">短信内容</param>
        public static bool WebChineseMSM(string mobile, string body)
        {
            var sys = MvcCore.Unity.Get<JN.Data.Service.ISysSettingService>().Single(1);
            if (!string.IsNullOrEmpty(mobile))
            {
                string url = "http://utf8.sms.webchinese.cn/?Uid=" + sys.SMSUid + "&Key=" + sys.SMSKey + "&smsMob=" + mobile + "&smsText=" + body;
                string targeturl = url.Trim();
                try
                {
                    bool result = false;
                    HttpWebRequest hr = (HttpWebRequest)WebRequest.Create(targeturl);
                    hr.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1)";
                    hr.Method = "GET";
                    hr.Timeout = 30 * 60 * 1000;
                    WebResponse hs = hr.GetResponse();
                    Stream sr = hs.GetResponseStream();
                    StreamReader ser = new StreamReader(sr, Encoding.Default);
                    string content = ser.ReadToEnd();
                    string msg = "";
                    if (content.Substring(0, 1) == "0" || content.Substring(0, 1) == "1")
                    {
                        result = true;
                        msg = "发送成功";
                    }
                    else
                    {
                        if (content.Substring(0, 1) == "2") //余额不足
                        {
                            //"手机短信余额不足";
                        }
                        else
                        {
                            //短信发送失败的其他原因，请参看官方API
                        }
                        result = false;
                        msg = "发送失败，结果：" + content;
                    }
                    MvcCore.Unity.Get<JN.Data.Service.ISMSLogService>().Add(new Data.SMSLog {  Mobile = mobile, SMSContent = body, CreateTime = DateTime.Now, ReturnValue = msg });
                    MvcCore.Unity.Get<JN.Data.Service.ISysDBTool>().Commit();

                    return result;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            else
                return false;
        }
        #endregion

        #region  C123接口
        /// <summary>
        /// C123接口 http://www.c123.cn/
        /// </summary>
        /// <param name="mobile"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        public static bool C123SMS(string mobile, string body)
        {
            var sys = MvcCore.Unity.Get<JN.Data.Service.ISysSettingService>().Single(1);
            if (!string.IsNullOrEmpty(mobile))
            {
                string url = "http://wapi.c123.cn/tx/?uid=" + sys.SMSUid + "&pwd=" + sys.SMSKey + "&mobile=" + mobile + "&content=" + System.Web.HttpUtility.UrlEncode(body, Encoding.GetEncoding("GB2312")).ToUpper();
                string targeturl = url.Trim();
                try
                {
                    bool result = false;
                    HttpWebRequest hr = (HttpWebRequest)WebRequest.Create(targeturl);
                    hr.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1)";
                    hr.Method = "GET";
                    hr.Timeout = 30 * 60 * 1000;
                    WebResponse hs = hr.GetResponse();
                    Stream sr = hs.GetResponseStream();
                    StreamReader ser = new StreamReader(sr, Encoding.Default);
                    string content = ser.ReadToEnd();
                    string msg = "";
                    if (content.Substring(0, 1) == "100")
                    {
                        result = true;
                        msg = "发送成功";
                    }
                    else
                    {
                        if (content.Substring(0, 1) == "102") //余额不足
                        {
                            //"手机短信余额不足";
                        }
                        else
                        {
                            //短信发送失败的其他原因，请参看官方API
                        }
                        result = false;
                        msg = "发送失败，结果：" + content;
                    }
                    MvcCore.Unity.Get<JN.Data.Service.ISMSLogService>().Add(new Data.SMSLog { Mobile = mobile, SMSContent = body, CreateTime = DateTime.Now, ReturnValue = msg });
                    MvcCore.Unity.Get<JN.Data.Service.ISysDBTool>().Commit();

                    return result;

                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            else
                return false;
        }

        #endregion

        #region 容联接口 (支持短信,语言验证码)
        ///// <summary>
        ///// C123接口 http://www.yuntongxun.com/
        ///// </summary>
        ///// <param name="mobile"></param>
        ///// <param name="body"></param>
        ///// <returns></returns>
        //public static bool VoiceVerify(string mobile, string yzm, string displayNum)
        //{
        //    string ret = null;
        //    CCPRestSDK.CCPRestSDK api = new CCPRestSDK.CCPRestSDK();
        //    bool isInit = api.init("sandboxapp.cloopen.com", "8883");
        //    api.setAccount(ConfigHelper.GetConfigString("Voice_Uid"), ConfigHelper.GetConfigString("Voice_Key"));
        //    api.setAppId(ConfigHelper.GetConfigString("Voice_Appid"));
        //    try
        //    {
        //        if (isInit)
        //        {
        //            Dictionary<string, object> retData = api.VoiceVerify(mobile, yzm, displayNum, "2", "");
        //            ret = getDictionaryData(retData);
        //        }
        //        else
        //        {
        //            ret = "初始化失败";
        //        }
        //        return true;
        //    }
        //    catch (Exception exc)
        //    {
        //        ret = exc.Message;
        //        return false;
        //    }
        //}

        //private static string getDictionaryData(Dictionary<string, object> data)
        //{
        //    string ret = null;
        //    foreach (KeyValuePair<string, object> item in data)
        //    {
        //        if (item.Value != null && item.Value.GetType() == typeof(Dictionary<string, object>))
        //        {
        //            ret += item.Key.ToString() + "={";
        //            ret += getDictionaryData((Dictionary<string, object>)item.Value);
        //            ret += "};";
        //        }
        //        else
        //        {
        //            ret += item.Key.ToString() + "=" +
        //            (item.Value == null ? "null" : item.Value.ToString()) + ";";
        //        }
        //    }
        //    return ret;
        //}
        #endregion
    }
}

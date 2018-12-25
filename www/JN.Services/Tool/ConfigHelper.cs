using MvcCore.Extensions;
using System;
using System.Configuration;
using System.Web.Configuration;

namespace JN.Services.Tool
{
	/// <summary>
	/// web.config操作类
    /// Copyright (C) Maticsoft
	/// </summary>
	public sealed class ConfigHelper
	{
		/// <summary>
		/// 得到AppSettings中的配置字符串信息
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public static string GetConfigString(string key)
		{
            string CacheKey = "AppSettings-" + key;
            object objModel = CacheExtensions.GetCache<object>(CacheKey);
            if (objModel == null)
            {
                try
                {
                    objModel = ConfigurationManager.AppSettings[key]; 
                    if (objModel != null)
                    {
                        CacheExtensions.SetCache(CacheKey, objModel);
                    }
                }
                catch
                { }
            }
            return objModel.ToString();
		}

		/// <summary>
		/// 得到AppSettings中的配置Bool信息
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public static bool GetConfigBool(string key)
		{
			bool result = false;
			string cfgVal = GetConfigString(key);
			if(null != cfgVal && string.Empty != cfgVal)
			{
				try
				{
					result = bool.Parse(cfgVal);
				}
				catch(FormatException)
				{
					// Ignore format exceptions.
				}
			}
			return result;
		}
		/// <summary>
		/// 得到AppSettings中的配置Decimal信息
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public static decimal GetConfigDecimal(string key)
		{
			decimal result = 0;
			string cfgVal = GetConfigString(key);
			if(null != cfgVal && string.Empty != cfgVal)
			{
				try
				{
					result = decimal.Parse(cfgVal);
				}
				catch(FormatException)
				{
					// Ignore format exceptions.
				}
			}

			return result;
		}
		/// <summary>
		/// 得到AppSettings中的配置int信息
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public static int GetConfigInt(string key)
		{
			int result = 0;
			string cfgVal = GetConfigString(key);
			if(null != cfgVal && string.Empty != cfgVal)
			{
				try
				{
					result = int.Parse(cfgVal);
				}
				catch(FormatException)
				{
					// Ignore format exceptions.
				}
			}

			return result;
		}

        /// <summary>
        /// 修改AppSettings中的配置信息
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static void UpdatetConfig(string key, string value)
        {
            Configuration objConfig = WebConfigurationManager.OpenWebConfiguration("~");
            AppSettingsSection objAppSettings = (AppSettingsSection)objConfig.GetSection("appSettings");
            if (objAppSettings != null)
            {
                objAppSettings.Settings[key].Value = value;
                objConfig.Save();
            }  
        }
	}
}

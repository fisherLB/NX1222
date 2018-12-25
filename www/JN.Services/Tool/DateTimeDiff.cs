using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JN.Services.Tool
{
    public class DateTimeDiff
    {
        public DateTimeDiff()
        {
            //
            // TODO: 在此处添加构造函数逻辑
            //
        }
        /// <summary>
        /// 把秒转换成分钟
        /// </summary>
        /// <returns></returns>
        public static int SecondToMinute(int Second)
        {
            decimal mm = (decimal)((decimal)Second / (decimal)60);
            return Convert.ToInt32(Math.Ceiling(mm));
        }
        /// <summary>
        /// 计算两个时间的时间间隔
        /// </summary>
        /// <param name="DateTimeOld">较早的日期和时间</param>
        /// <param name="DateTimeNew">较后的日期和时间</param>
        /// <returns></returns>
        public static string DateDiff(DateTime DateTimeOld, DateTime DateTimeNew)
        {
            string dateDiff = "";
            TimeSpan ts1 = new TimeSpan(DateTimeOld.Ticks);
            TimeSpan ts2 = new TimeSpan(DateTimeNew.Ticks);
            TimeSpan ts = ts1.Subtract(ts2).Duration();
            int day = ts.Days;
            int hou = ts.Hours;
            int minu = ts.Minutes;
            int sec = ts.Seconds;
            if (day > 0)
            {
                if (day > 30)
                {
                    if (day > 364)
                    {
                        dateDiff += day / 365 + "年";
                    }
                    else
                    {
                        dateDiff += day / 30 + "个月";
                    }
                }
                else
                {
                    dateDiff += day.ToString() + "天";
                }
            }
            else
            {
                if (hou > 0)
                {
                    dateDiff += hou.ToString() + "小时";
                }
                else
                {
                    if (minu > 0)
                    {
                        dateDiff += minu.ToString() + "分钟";
                    }
                    else
                    {
                        if (sec > 0)
                        {
                            dateDiff += sec.ToString() + "秒";
                        }
                        else
                        {
                            dateDiff += "0秒";
                        }
                    }
                }
            }
            if (DateTimeNew.CompareTo(DateTimeOld) > 0)
            {
                dateDiff += "前";
            }
            else
            {
                dateDiff += "后";
            }
            return dateDiff;
        }

        ///   <summary>   
        ///   返回两个日期之间的时间间隔（y：年份间隔、M：月份间隔、d：天数间隔、h：小时间隔、m：分钟间隔、s：秒钟间隔、ms：微秒间隔）   
        ///   </summary>   
        ///   <param   name="Date1">开始日期</param>   
        ///   <param   name="Date2">结束日期</param>   
        ///   <param   name="Interval">间隔标志</param>   
        ///   <returns>返回间隔标志指定的时间间隔</returns>   
        public static int DateDiff(System.DateTime Date1, System.DateTime Date2, string Interval)
        {
            double dblYearLen = 365;//年的长度，365天   
            double dblMonthLen = (365 / 12);//每个月平均的天数   
            System.TimeSpan objT;
            objT = Date2.Subtract(Date1);
            switch (Interval)
            {
                case "y"://返回日期的年份间隔   
                    return System.Convert.ToInt32(objT.Days / dblYearLen);
                case "M"://返回日期的月份间隔   
                    return System.Convert.ToInt32(objT.Days / dblMonthLen);
                case "d"://返回日期的天数间隔   
                    return objT.Days;
                case "h"://返回日期的小时间隔   
                    return objT.Hours;
                case "m"://返回日期的分钟间隔   
                    return objT.Minutes;
                case "s"://返回日期的秒钟间隔   
                    return objT.Seconds;
                case "ms"://返回时间的微秒间隔   
                    return objT.Milliseconds;
                default:
                    break;
            }
            return 0;
        }

        /// <summary>
        ///判断是否于5分钟之前
        /// </summary>
        /// <param name="DateTimeOld">较早的日期和时间</param>
        /// <returns></returns>
        public static bool DateDiff_minu(DateTime DateTimeOld)
        {
            TimeSpan ts1 = new TimeSpan(DateTimeOld.Ticks);
            TimeSpan ts2 = new TimeSpan(DateTime.Now.Ticks);
            TimeSpan ts = ts1.Subtract(ts2).Duration();
            int minu = ts.Minutes;
            if (minu > 5)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static bool DateDiff_1minu(DateTime DateTimeOld)
        {
            TimeSpan ts1 = new TimeSpan(DateTimeOld.Ticks);
            TimeSpan ts2 = new TimeSpan(DateTime.Now.Ticks);
            TimeSpan ts = ts1.Subtract(ts2).Duration();
            int minu = ts.Seconds;
            if (minu > 10)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 与当前时间比较,重载时间比较函数,只有一个参数
        /// </summary>
        /// <param name="DateTimeOld">较早的日期和时间</param>
        /// <returns></returns>
        public static string DateDiff(DateTime DateTimeOld)
        {
            return DateDiff(DateTimeOld, DateTime.Now);
        }
        /// <summary>
        /// 日期比较,返回精确的几分几秒
        /// </summary>
        /// <param name="DateTime1">较早的日期和时间</param>
        /// <param name="DateTime2">较迟的日期和时间</param>
        /// <returns></returns>
        public static string DateDiff_full(DateTime DateTime1, DateTime DateTime2)
        {
            string dateDiff = null;
            TimeSpan ts1 = new TimeSpan(DateTime1.Ticks);
            TimeSpan ts2 = new TimeSpan(DateTime2.Ticks);
            TimeSpan ts = ts1.Subtract(ts2).Duration();
            dateDiff = ts.Days.ToString() + "天" + ts.Hours.ToString() + "时" + ts.Minutes.ToString() + "分" + ts.Seconds.ToString() + "秒";
            return dateDiff;
        }
        /// <summary>
        /// 时间比较,返回精确的几秒
        /// </summary>
        /// <param name="DateTime1">较早的日期和时间</param>
        /// <param name="DateTime2">较迟的日期和时间</param>
        /// <returns></returns>
        public static int DateDiff_Sec(DateTime DateTime1, DateTime DateTime2)
        {
            TimeSpan ts1 = new TimeSpan(DateTime1.Ticks);
            TimeSpan ts2 = new TimeSpan(DateTime2.Ticks);
            TimeSpan ts = ts1.Subtract(ts2).Duration();
            int dateDiff = ts.Days * 86400 + ts.Hours * 3600 + ts.Minutes * 60 + ts.Seconds;
            return dateDiff;
        }

        /// <summary>
        /// 日期比较
        /// </summary>
        /// <param name="today">当前日期</param>
        /// <param name="writeDate">输入日期</param>
        /// <param name="n">比较天数</param>
        /// <returns>大于天数返回true，小于返回false</returns>
        public static bool CompareDate(string today, string writeDate, int n)
        {
            DateTime Today = Convert.ToDateTime(today);
            DateTime WriteDate = Convert.ToDateTime(writeDate);
            WriteDate = WriteDate.AddDays(n);
            if (Today >= WriteDate)
                return false;
            else
                return true;
        }

        public static string FormatProgress(DateTime StartTime, DateTime EndTime)
        {


            if (DateTime.Now > DateTime.Parse(EndTime.ToShortDateString() + " 23:59:59"))
                return "活动结束";
            else if (DateTime.Now < DateTime.Parse(StartTime.ToShortDateString() + " 0:0:0"))
                return "即将开始";
            else
            {
                int totalDay = DateTimeDiff.DateDiff(StartTime, EndTime, "d");
                int inDay = DateTimeDiff.DateDiff(StartTime, DateTime.Now, "d");
                if ((float)inDay / (float)totalDay < 0.2f)
                    return "刚刚开始";
                else if ((float)inDay / (float)totalDay >= 0.2f && (float)inDay / (float)totalDay < 0.4)
                    return "正在进行";
                else if ((float)inDay / (float)totalDay >= 0.4 && (float)inDay / (float)totalDay < 0.6)
                    return "活动过半";
                else if ((float)inDay / (float)totalDay > 0.6 && (float)inDay / (float)totalDay <= 0.8)
                    return "进入尾声";
                else
                    return "即将结束";
            }
        }

        // 时间戳转为C#格式时间
        public static DateTime StampToDateTime(string timeStamp)
        {
            DateTime dateTimeStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long lTime = long.Parse(timeStamp + "0000000");
            TimeSpan toNow = new TimeSpan(lTime);

            return dateTimeStart.Add(toNow);
        }

        // DateTime时间格式转换为Unix时间戳格式
        public static double DateTimeToStamp(System.DateTime time)
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
            return (time - startTime).TotalMilliseconds;
        }
    }
}

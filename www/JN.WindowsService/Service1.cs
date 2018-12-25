using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace JN.WindowsService
{
    public partial class Service1 : ServiceBase
    {
        DataTable dt;
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        //public void OnStart()
        {
            //EventLog.WriteEntry("我的服务启动"); //系统事件上查看器里的应用程序事件里来源描述
            WriteLog("服务启动");



            System.Timers.Timer t = new System.Timers.Timer();
            t.Interval = 1000 * 60;
            t.Elapsed += new System.Timers.ElapsedEventHandler(ChkSrv); //到达时间执行事件;
            t.AutoReset = true; //设置是执行一次（false)还是一直执行(true);
            t.Enabled = true; //是否执行System.Timers.Timer.Elapsed事件;

            System.Timers.Timer t2 = new System.Timers.Timer();
            t2.Interval = 1000 * 60 * 2;
            t2.Elapsed += new System.Timers.ElapsedEventHandler(ChkSrvYz); //到达时间执行事件;
            t2.AutoReset = true; //设置是执行一次（false)还是一直执行(true);
            t2.Enabled = true; //是否执行System.Timers.Timer.Elapsed事件;

        }

        protected override void OnStop()
        {
            WriteLog("服务停止");
            //EventLog.WriteEntry("我的服务停止");
        }

        public void ChkSrv(object source, System.Timers.ElapsedEventArgs e)
        {
            //int intHour = e.SignalTime.Hour;
            //int intMinute = e.SignalTime.Minute;
            //int intSecond = e.SignalTime.Second;

            string PlanXML = AppDomain.CurrentDomain.BaseDirectory + "Plan.xml";
            if (System.IO.File.Exists(PlanXML))
            {
                dt = XmlDataSet.GetDataSetByXml(PlanXML).Tables[0];
            }
            WriteLog("读取" + PlanXML + "(" + dt.Rows.Count + "条作业)");
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    bool isExec = false;
                    DateTime starttime = DateTime.Now;

                    bool tj1 = Convert.ToInt16(dt.Rows[i]["cycletype"]) == 0 && DateTime.Now.ToString("HH:mm") == Convert.ToDateTime(dt.Rows[i]["time"]).ToString("HH:mm");  //符合日结
                    bool tj2 = Convert.ToInt16(dt.Rows[i]["cycletype"]) == 1 && DateTime.Now.DayOfWeek.ToString() == dt.Rows[i]["cyclevalue"].ToString() && DateTime.Now.ToString("HH:mm") == Convert.ToDateTime(dt.Rows[i]["time"]).ToString("HH:mm"); //符合周结
                    bool tj3 = Convert.ToInt16(dt.Rows[i]["cycletype"]) == 2 && DateTime.Now.Day.ToString() == dt.Rows[i]["cyclevalue"].ToString() && DateTime.Now.ToString("HH:mm") == Convert.ToDateTime(dt.Rows[i]["time"]).ToString("HH:mm"); //符合月结

                    if (tj1 || tj2 || tj3)
                    {
                        WriteLog("触发定时作业");
                        string url = ReadFile(AppDomain.CurrentDomain.BaseDirectory + "url.txt");
                        if (!string.IsNullOrEmpty(url))
                        {
                            url += "timingplan/index?execprocess=" + dt.Rows[i]["name"].ToString();
                            WriteLog("读取访问地址：" + url);
                            try
                            {
                                System.Net.HttpWebRequest myHttpWebRequest = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);
                                System.Net.HttpWebResponse myHttpWebResponse = (System.Net.HttpWebResponse)myHttpWebRequest.GetResponse();
                                System.IO.Stream receiveStream = myHttpWebResponse.GetResponseStream();//得到回写的字节流
                                //WriteLog(receiveStream.ToString());
                                isExec = true;
                            }
                            catch (Exception ex)
                            {
                                WriteLog(ex.ToString());
                            }
                            if (isExec)
                                WriteLog("成功执行作业计划“" + dt.Rows[i]["name"].ToString() + "”，时间在" + DateTime.Now.ToString() + "");
                        }
                    }
                }
            }
            dt.Dispose();



            //SendMessage();
            //if (intHour == 08 && intMinute == 00 && intSecond == 00)  //判断分时秒，8点发送消息
            //if (intSecond % 2 == 0)
            //{
            //    try
            //    {
            //        //System.Timers.Timer tt = (System.Timers.Timer)source;
            //        //tt.Enabled = false;
            //        SendMessage();
            //        //tt.Enabled = true;
            //    }
            //    catch (Exception err) 
            //    {
            //        WriteLog(err.Message);
            //    }
            //}
        }

        public void ChkSrvYz(object source, System.Timers.ElapsedEventArgs e)
        {
            bool isExec = false;

            WriteLog("触发检查定时作业");
            string url = ReadFile(AppDomain.CurrentDomain.BaseDirectory + "url.txt");
            if (!string.IsNullOrEmpty(url))
            {
                url += "timingplan/index?execprocess=plan2";
                WriteLog("读取访问地址：" + url);
                try
                {
                    System.Net.HttpWebRequest myHttpWebRequest = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);
                    System.Net.HttpWebResponse myHttpWebResponse = (System.Net.HttpWebResponse)myHttpWebRequest.GetResponse();
                    System.IO.Stream receiveStream = myHttpWebResponse.GetResponseStream();//得到回写的字节流
                    //WriteLog(receiveStream.ToString());
                    isExec = true;
                }
                catch (Exception ex)
                {
                    WriteLog(ex.ToString());
                }
                if (isExec)
                    WriteLog("成功执行实时监控");
            }

        }

        //public void SendMessage()
        //{
        //    string url = "http://198.44.249.86:8099/";
        //    try
        //    {
        //        System.Net.HttpWebRequest myHttpWebRequest = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);
        //        System.Net.HttpWebResponse myHttpWebResponse = (System.Net.HttpWebResponse)myHttpWebRequest.GetResponse();
        //        System.IO.Stream receiveStream = myHttpWebResponse.GetResponseStream();//得到回写的字节流
        //        WriteLog("- " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " 模拟打开" + url + "\r\n");
        //    }
        //    catch (Exception ex)
        //    {
        //        WriteLog("- " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ex.Message + "\r\n");
        //    }
        //}

        /// <summary>
        /// 从指定位置的文件中读取字符串。
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <returns></returns>
        public string ReadFile(string path)
        {
            if (!File.Exists(path))
                return string.Empty;
            string rs = string.Empty;
            using (StreamReader sr = new StreamReader(path, Encoding.UTF8))
            {
                rs = sr.ReadToEnd();
                sr.Close();
            }
            return rs;
        }

        public void WriteLog(string readme)
        {

            //logs.WriteTimepLanLog("- " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + readme + "\r\n", AppDomain.CurrentDomain.BaseDirectory + "Log\\");
            StreamWriter dout = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "WServ_InnPointLog.txt", true);
            dout.Write(System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " >>" + readme + "\r\n");
            dout.Close(); //关闭StreamWriter对象
        }
    }

    /// <summary>
    /// OperateXmlByDataSet 的摘要说明。
    /// </summary>
    public class XmlDataSet
    {
        public XmlDataSet()
        {
            //
            // TODO: 在此处添加构造函数逻辑
            //
        }

        #region GetDataSetByXml
        /// <summary>
        /// 读取xml直接返回DataSet
        /// </summary>
        /// <param name="strXmlPath">xml文件相对路径</param>
        /// <returns></returns>
        public static DataSet GetDataSetByXml(string strXmlPath)
        {
            try
            {
                DataSet ds = new DataSet();
                ds.ReadXml(strXmlPath);
                if (ds.Tables.Count > 0)
                {
                    return ds;
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }
        #endregion

        #region GetDataViewByXml
        /// <summary>
        /// 读取Xml返回一个经排序或筛选后的DataView
        /// </summary>
        /// <param name="strXmlPath"></param>
        /// <param name="strWhere">筛选条件，如："name = 'kgdiwss'"</param>
        /// <param name="strSort">排序条件，如："Id desc"</param>
        /// <returns></returns>
        public static DataView GetDataViewByXml(string strXmlPath, string strWhere, string strSort)
        {
            try
            {
                DataSet ds = new DataSet();
                ds.ReadXml(strXmlPath);
                DataView dv = new DataView(ds.Tables[0]);
                if (strSort != null)
                {
                    dv.Sort = strSort;
                }
                if (strWhere != null)
                {
                    dv.RowFilter = strWhere;
                }
                return dv;
            }
            catch (Exception)
            {
                return null;
            }
        }
        #endregion

    }
}

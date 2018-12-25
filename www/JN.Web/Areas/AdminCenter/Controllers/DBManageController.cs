using JN.Data;
using JN.Data.Service;
using JN.Services.Tool;
using MvcCore.Controls;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using PagedList;

namespace JN.Web.Areas.AdminCenter.Controllers
{
    public class DBManageController : BaseController
    {
        private readonly ISysDBTool SysDBTool;
        private readonly IActLogService ActLogService;
      
        public DBManageController(ISysDBTool SysDBTool, IActLogService ActLogService)
        {
            this.SysDBTool = SysDBTool;
            this.ActLogService = ActLogService;
            this.SysDBTool = SysDBTool;
        }

        public ActionResult Index(int? page)
        {
            ViewBag.Title = "数据库备份与恢复";
            ActMessage = ViewBag.Title;

            int pageIndex = page ?? 1;
            IOHelper.CreateDirectory(Server.MapPath("/DBBackUp/"));

            List<FileInfo> files = IOHelper.GetAllFilesInDirectory(Server.MapPath("/DBBackUp/"));
            List<Data.Extensions.DbBackFile> lst = new List<Data.Extensions.DbBackFile>();
            foreach (FileInfo info in files)
            {
                lst.Add(new Data.Extensions.DbBackFile { BackFileName = info.Name, BackFileFullName = info.FullName, BackFileSize = info.Length > 0 ? info.Length / 1024 : 0, CreateTime = info.LastWriteTime });
            }

            //Linq进行排序
            IEnumerable<Data.Extensions.DbBackFile> query = from items in lst orderby items.CreateTime descending select items;
            return View(query.ToPagedList(page ?? 1, 20));
        }

        public ActionResult Backup()
        {
            ActMessage = "备份数据库";
            IOHelper.CreateDirectory(Server.MapPath("/DBBackUp/"));
            SqlParameter[] para_sys = new SqlParameter[]
            {
            new SqlParameter ("@BKPATH", Server.MapPath("/DBBackUp/") + DateTime.Now.ToString("yyyyMMddHHmmss") + "_SYS.bak" ),
            };

            SqlParameter[] para_log = new SqlParameter[]
            {
                new SqlParameter ("@BKPATH", Server.MapPath("/DBBackUp/") + DateTime.Now.ToString("yyyyMMddHHmmss") + "_LOG.bak" ),
            };

            int count = MvcCore.Unity.Get<ISysDBTool>().Execute<object>("backup database " + ConfigHelper.GetConfigString("DBName") + "_SYS to disk=@BKPATH", para_sys).Count();
            int count2 = MvcCore.Unity.Get<ISysDBTool>().Execute<object>("backup database " + ConfigHelper.GetConfigString("DBName") + "_Log to disk=@BKPATH", para_log).Count();

            if (count >= 0 && count2 >= 0)
            {
                ViewBag.SuccessMsg = "数据库备份成功！";
                return View("Success");
            }
            ViewBag.ErrorMsg = "数据库备份失败！请查看系统日志。";
            return View("Error");
        }

        public ActionResult Restore(string backfilename)
        {
            ActMessage = "恢复数据库";
            SqlParameter[] para = new SqlParameter[]
            {
                new SqlParameter ("@BKFILE", Server.MapPath("/DBBackUp/") + backfilename),
            };
            int count = 0;
            if (backfilename.Contains("_SYS"))
                count = MvcCore.Unity.Get<ISysDBTool>().Execute<object>("ALTER DATABASE [" + ConfigHelper.GetConfigString("DBName") + "_SYS] SET OFFLINE WITH ROLLBACK IMMEDIATE; restore database " + ConfigHelper.GetConfigString("DBName") + "_SYS from disk=@BKFILE WITH REPLACE;ALTER DATABASE [" + ConfigHelper.GetConfigString("DBName") + "_SYS] SET ONLINE", para).Count();
            else
                count = MvcCore.Unity.Get<ISysDBTool>().Execute<object>("ALTER DATABASE [" + ConfigHelper.GetConfigString("DBName") + "_Log] SET OFFLINE WITH ROLLBACK IMMEDIATE; restore database " + ConfigHelper.GetConfigString("DBName") + "_Log from disk=@BKFILE WITH REPLACE;ALTER DATABASE [" + ConfigHelper.GetConfigString("DBName") + "_LOG] SET ONLINE", para).Count();

            if (count >= 0)
            {
                ViewBag.SuccessMsg = "数据库恢复成功！";
                return View("Success");
            }
            ViewBag.ErrorMsg = "数据库恢复失败！请查看系统日志。";
            return View("Error");
        }

        public ActionResult DBClear(string password)
        {
            ActMessage = "清空数据库";

            //if (password != ConfigHelper.GetConfigString("DBName"))
            //{
            //    ViewBag.ErrorMsg = "请输入正确的经理密码。";
            //    return View("Error");
            //}

            if ((MvcCore.Unity.Get<ISysSettingService>().Single(1).DevelopMode ?? 0) != 0)
            {
                var log = new Data.WarningLog();
                log.CreateTime = DateTime.Now;
                log.IP = IPHelper.GetClientIp();
                log.Location = Request.Url.ToString();
                log.Platform = "后台";
                log.UserName = Amodel.AdminName;
                log.WarningLevel = "特别严重";
                log.ResultMsg = "拒绝";
                log.WarningMsg = "尝试清空数据库";
                MvcCore.Unity.Get<IWarningLogService>().Add(log);
                MvcCore.Unity.Get<ISysDBTool>().Commit();

                string mobile = MvcCore.Unity.Get<ISysSettingService>().Single(1).SysMobile;
                bool issms = MvcCore.Unity.Get<ISysSettingService>().Single(1).IsWarnningSMS ?? false;
                if (issms && !string.IsNullOrEmpty(mobile))
                    SMSHelper.WebChineseMSM(mobile, "管理员帐号“" + Amodel.AdminName + "”尝试清空数据库！");

                ViewBag.ErrorMsg = "当前系统模式不充许清空数据库，您的行为已被记录。";
                return View("Error");
            }

            //清空前先备份
            IOHelper.CreateDirectory(Server.MapPath("/DBBackUp/"));
            SqlParameter[] para_sys = new SqlParameter[]
            {
            new SqlParameter ("@BKPATH", Server.MapPath("/DBBackUp/") + DateTime.Now.ToString("yyyyMMddHHmmss") + "_SYS.bak" ),
            };

            SqlParameter[] para_log = new SqlParameter[]
            {
                new SqlParameter ("@BKPATH", Server.MapPath("/DBBackUp/") + DateTime.Now.ToString("yyyyMMddHHmmss") + "_LOG.bak" ),
            };

           MvcCore.Unity.Get<ISysDBTool>().Execute<object>("backup database " + ConfigHelper.GetConfigString("DBName") + " to disk=@BKPATH", para_sys).Count();
            //MvcCore.Unity.Get<ISysDBTool>().Execute<object>("backup database " + ConfigHelper.GetConfigString("DBName") + "_Log to disk=@BKPATH", para_log).Count();

            DbParameter[] param = new DbParameter[] { };
            SysDBTool.ExecuteSQL("TRUNCATE TABLE [WalletLog]", param);
            SysDBTool.ExecuteSQL("TRUNCATE TABLE [ActLog]", param);
            SysDBTool.ExecuteSQL("TRUNCATE TABLE [SysLog]", param);

            SysDBTool.ExecuteSQL("TRUNCATE TABLE [Settlement]", param);
            SysDBTool.ExecuteSQL("TRUNCATE TABLE [BonusDetail]", param);
            SysDBTool.ExecuteSQL("TRUNCATE TABLE [AcceptHelp]", param);
            SysDBTool.ExecuteSQL("TRUNCATE TABLE [LeaveWord]", param);
            SysDBTool.ExecuteSQL("TRUNCATE TABLE [Matching]", param);
            SysDBTool.ExecuteSQL("TRUNCATE TABLE [SupplyHelp]", param);
            SysDBTool.ExecuteSQL("TRUNCATE TABLE [Message]", param);
            SysDBTool.ExecuteSQL("TRUNCATE TABLE [Notice]", param);
            SysDBTool.ExecuteSQL("TRUNCATE TABLE [User]", param);
            SysDBTool.ExecuteSQL("TRUNCATE TABLE [ShopInfo]", param);
            SysDBTool.ExecuteSQL("TRUNCATE TABLE [ShopOrder]", param);
            SysDBTool.ExecuteSQL("TRUNCATE TABLE [ShopOrderDetail]", param);
            SysDBTool.ExecuteSQL("TRUNCATE TABLE [ShopProduct]", param);
            SysDBTool.ExecuteSQL("TRUNCATE TABLE [PINCode]", param);
            ViewBag.SuccessMsg = "除管理员表及系统参数设置外全部数据已全部清空！清空数据后会员平台必须重新登录。";
            return View("Success");
        }
    }
}

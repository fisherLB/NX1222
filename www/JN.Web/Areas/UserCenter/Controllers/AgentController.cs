
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using JN.Data;
using JN.Data.Service;
using MvcCore.Controls;
using PagedList;

namespace JN.Web.Areas.UserCenter.Controllers
{
    public class AgentController : BaseController
    {

        private readonly IUserService UserService;
        private readonly ISysParamService SysParamService;
        private readonly ISysDBTool SysDBTool;
        private readonly IActLogService ActLogService;
     

        public AgentController(ISysDBTool SysDBTool, IUserService UserService, ISysParamService SysParamService, IActLogService ActLogService )
        {
            this.UserService = UserService;
            this.SysParamService = SysParamService;
            this.SysDBTool = SysDBTool;
            this.ActLogService = ActLogService;
            this.SysDBTool = SysDBTool;
        }

        public ActionResult Index(int? page)
        {
            ActMessage = "我管辖的会员";
            var list = UserService.List(x => x.AgentID == Umodel.ID).WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).OrderByDescending(x => x.ID).ToList();
            return View(list.ToPagedList(page ?? 1, 20));
        }

        public ActionResult ApplyAgent()
        {
            //if (!(Umodel.IsAgent ?? false))
            //{
            //    ViewBag.ErrorMsg = "你的用户级别无法申请商务中心。";
            //    return View("Error");
            //}
            return View(Umodel);
        }

        [HttpPost]
        public ActionResult ApplyAgent(FormCollection form)
        {
            List<Data.SysParam> cacheSysParam = MvcCore.Unity.Get<JN.Data.Service.ISysParamService>().List(x => x.ID < 4000).ToList();

            ReturnResult result = new ReturnResult();
            try
            {
                string type = form["type"];
                //string username = form["username"];
                string agentname = form["agentname"];
                string remark = form["agentremark"];

                //var onUser = UserService.Single(x => x.UserName == username.Trim());
                //if (onUser == null) throw new Exception("用户不存在");
                if (string.IsNullOrEmpty(agentname)) throw new Exception("商务中心编号不能为空");
                if (UserService.List(x => x.AgentName == agentname.Trim()).Count() > 0) throw new Exception("商务中心编号已被使用");
                if (remark.Trim().Length > 100) throw new Exception("备注长度不能超过100个字节");
                //if ((onUser.IsAgent ?? false)) throw new Exception("该会员已是商务中心，无需要重复申请");
                //if (!String.IsNullOrEmpty(onUser.AgentName)) throw new Exception("您已提交过申请，请耐心等待审核");
                //var ids = onUser.RefereePath.Split(',');
                //if (UserService.List(x => ids.Contains(x.ID.ToString()) || x.ID == onUser.RefereeID).Count() <= 0) throw new Exception("只充许给伞下推荐关系的会员申请商务中心");
                int PARAM_TJRS = 0;
                int tjrs =0;
                int tdrs = 0;
                int PARAM_TDRS = 0;
                if (type.ToInt() == 1)
                {
                    //推荐人数
                    PARAM_TJRS = cacheSysParam.SingleAndInit(x => x.ID == 1801).Value.ToInt(); //推荐要求
                    tjrs = JN.Services.Manager.Users.GetAllRefereeChild(Umodel, 1).Where(x => (x.AddupSupplyAmount ?? 0) > 0).Count(); //如累计提供0以上的直推人数
                    //tjrs = JN.Services.Manager.Users.GetAllRefereeChild(Umodel, 1).Count();
                    //团队人数
                    tdrs = JN.Services.Manager.Users.GetAllRefereeChild(Umodel, 0).Where(x => (x.AddupSupplyAmount ?? 0) > 0).Count();//累计提供0以上的团队人数
                    //tdrs = JN.Services.Manager.Users.GetAllRefereeChild(Umodel, 0).Count();
                    PARAM_TDRS = cacheSysParam.SingleAndInit(x => x.ID == 1801).Value2.ToInt(); //团队人数
                }
                //else if (type.ToInt() == 2)
                //{
                //    PARAM_TJRS = cacheSysParam.SingleAndInit(x => x.ID == 1802).Value.ToInt(); //推荐要求
                //    tjrs = JN.Services.Manager.Users.GetAllRefereeChild(Umodel, 1).Where(x => (x.AddupSupplyAmount ?? 0) > 0).Count(); //如累计提供0以上的直推人数
                //    //团队人数
                //    tdrs = JN.Services.Manager.Users.GetAllRefereeChild(Umodel, 0).Where(x => (x.AddupSupplyAmount ?? 0) > 0).Count();//累计提供0以上的团队人数
                //    PARAM_TDRS = cacheSysParam.SingleAndInit(x => x.ID == 1802).Value2.ToInt(); //团队人数
                //}
                string msg = "";
                //if (JN.Services.Manager.Users.HaveApplyAgent(Umodel.ID, type.ToInt(), ref msg))
                if (tjrs >= PARAM_TJRS && tdrs >= PARAM_TDRS)
                {
                    //更新申请状态
                    Umodel.AgentName = agentname.Trim();
                    Umodel.ApplyAgentRemark = remark;
                    //Umodel.UserLevel = type.ToInt();
                    Umodel.ApplyAgentTime = DateTime.Now;
                    UserService.Update(Umodel);
                    SysDBTool.Commit();
                    result.Status = 200;
                }
                else
                    throw new Exception("您没达到条件申请经理！详情：符合直推：" + tjrs + "/" + PARAM_TJRS + "，团队" + tdrs + "/" + PARAM_TDRS);
                //throw new Exception("您没达到条件申请" + (type.Equals("1") ? "经理" : "高级经理") + "！详情：符合直推：" + tjrs + "/" + PARAM_TJRS + "，团队" + tdrs + "/" + PARAM_TDRS);
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                Services.Manager.logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
            return Json(result);
        }
    }
}

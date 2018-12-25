using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using JN.Data;
using JN.Data.Service;
using MvcCore.Controls;
using System.Web.Security;
using JN.Data.Enum;
using JN.Services;
using JN.Services.Tool;
using System.Text.RegularExpressions;
using JN.Services.Manager;

namespace JN.Web.Areas.UserCenter.Controllers
{
    public class RegController : Controller
    {
        private static List<Data.SysParam> cacheSysParam = null;

        private readonly IUserService UserService;
        private readonly ISysDBTool SysDBTool;
        private readonly IActLogService ActLogService;
     

        public RegController(ISysDBTool SysDBTool, IUserService UserService, IActLogService ActLogService)
        {
            this.UserService = UserService;
            this.SysDBTool = SysDBTool;
            this.ActLogService = ActLogService;
         
            cacheSysParam = MvcCore.Unity.Get<JN.Data.Service.ISysParamService>().ListCache("sysparams", x => x.ID < 4000).ToList();
        }
        public ActionResult Index()
        {
            string username = Request["r"];
            var sysEntity = MvcCore.Unity.Get<JN.Data.Service.ISysSettingService>().Single(1);
            if (!sysEntity.IsRegisterOpen)
            {
                Response.Write(sysEntity.CloseRegisterHint);
                Response.End();
            }

            if (!string.IsNullOrEmpty(username))
                ViewBag.username = username;
            return View();
        }


        #region 添加会员

        /// <summary>
        /// 添加会员
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult index(FormCollection fc)
        {
            string vcode = (Session["UserValidateCode"] ?? "").ToString();
            Session.Remove("UserValidateCode");
            string code = fc["code"];
            ReturnResult result = new ReturnResult();
            try
            {
                var sysEntity = MvcCore.Unity.Get<ISysSettingService>().Single(1);
                if (string.IsNullOrEmpty(vcode) || string.IsNullOrEmpty(code) || !vcode.Equals(code, StringComparison.InvariantCultureIgnoreCase))
                    throw new Exception("验证码错误");

                var entity = new Data.User();
                TryUpdateModel(entity, fc.AllKeys);
                if (sysEntity.RegistNeedSMSVerification ?? false)
                {
                    string ageUser = fc["ageUser"];
                    if (Session["GetSMSUser"] != null && Session["SMSValidateCode"] != null)
                    {
                        if (Session["GetSMSUser"].ToString() != entity.Mobile.Trim() || Session["SMSValidateCode"].ToString() != ageUser)
                        {
                            throw new Exception("手机验证码错误");
                        }
                    }
                    else
                    {
                        throw new Exception("手机验证码错误");
                    }
                }
                if (!Regex.IsMatch(entity.UserName, @"^[A-Za-z0-9_]+$")) throw new Exception("用户名只能为字母、数字和下划线");
                if (string.IsNullOrEmpty(entity.Password) || string.IsNullOrEmpty(entity.Password2)) throw new Exception("登录密码、二级密码不能为空");
                if (fc["password"] != fc["confirmpassword"]) throw new Exception("登录密码与确认密码不相符");
                if (fc["password2"] != fc["confirmpassword2"]) throw new Exception("二级密码与确认二级密码不相符");
                if (string.IsNullOrEmpty(entity.UserName)) throw new Exception("会员编号不能为空");
                if (UserService.List(x => x.UserName == entity.UserName.Trim()).Count() > 0) throw new Exception("用户名已被使用");


                if (string.IsNullOrEmpty(entity.NickName) && sysEntity.RegistNotNullItems.Contains(",NickName,")) throw new Exception("昵称不能为空");
                if (string.IsNullOrEmpty(entity.RealName) && sysEntity.RegistNotNullItems.Contains(",RealName,")) throw new Exception("真实姓名不能为空");
                if (string.IsNullOrEmpty(entity.Mobile) && sysEntity.RegistNotNullItems.Contains(",Mobile,")) throw new Exception("手机号码不能为空");
                if (string.IsNullOrEmpty(entity.Email) && sysEntity.RegistNotNullItems.Contains(",Email,")) throw new Exception("邮箱不能为空");
                if (string.IsNullOrEmpty(entity.IDCard) && sysEntity.RegistNotNullItems.Contains(",IDCard,")) throw new Exception("身份证号码不能为空");
                if (string.IsNullOrEmpty(entity.AliPay) && sysEntity.RegistNotNullItems.Contains(",AliPay,")) throw new Exception("昵称不能为空");
                if (string.IsNullOrEmpty(entity.BankName) && sysEntity.RegistNotNullItems.Contains(",BankName,")) throw new Exception("银行名称不能为空");
                if (string.IsNullOrEmpty(entity.BankCard) && sysEntity.RegistNotNullItems.Contains(",BankCard,")) throw new Exception("银行卡卡号不能为空");
                if (string.IsNullOrEmpty(entity.BankUser) && sysEntity.RegistNotNullItems.Contains(",BankUser,")) throw new Exception("银行卡户名不能为空");
                if (string.IsNullOrEmpty(entity.BankOfDeposit) && sysEntity.RegistNotNullItems.Contains(",BankOfDeposit,")) throw new Exception("银行卡开户行不能为空");
                if (string.IsNullOrEmpty(entity.Question) && sysEntity.RegistNotNullItems.Contains(",Question,")) throw new Exception("取回密码问题不能为空");
                if (string.IsNullOrEmpty(entity.Answer) && sysEntity.RegistNotNullItems.Contains(",Answer,")) throw new Exception("取回密码答案不能为空");

                if (UserService.List(x => x.NickName == entity.NickName.Trim()).Count() > 0 && sysEntity.RegistOnlyOneItems.Contains(",NickName,")) throw new Exception("昵称已被使用");
                if (UserService.List(x => x.Mobile == entity.Mobile.Trim()).Count() > 0 && sysEntity.RegistOnlyOneItems.Contains(",Mobile,")) throw new Exception("手机号码已被使用");
                if (UserService.List(x => x.Email == entity.Email.Trim()).Count() > 0 && sysEntity.RegistOnlyOneItems.Contains(",Email,")) throw new Exception("邮箱已被使用");
                if (UserService.List(x => x.IDCard == entity.IDCard.Trim()).Count() > 0 && sysEntity.RegistOnlyOneItems.Contains(",IDCard,")) throw new Exception("身份证号码已被使用");
                if (UserService.List(x => x.AliPay == entity.AliPay.Trim()).Count() > 0 && sysEntity.RegistOnlyOneItems.Contains(",AliPay,")) throw new Exception("支付宝帐号已被使用");
                if (UserService.List(x => x.WeiXin == entity.WeiXin.Trim()).Count() > 0 && sysEntity.RegistOnlyOneItems.Contains(",WeiXin,")) throw new Exception("微信帐号已被使用");
                if (UserService.List(x => x.BankCard == entity.BankCard.Trim()).Count() > 0 && sysEntity.RegistOnlyOneItems.Contains(",BankCard,")) throw new Exception("银行卡号已被使用");

                if (!string.IsNullOrEmpty(entity.BankUser) && entity.RealName != entity.BankUser) throw new Exception("真实姓名与银行卡户名不相符");
                if (UserService.List(x => x.UserName == entity.RefereeUser && x.IsActivation).Count() <= 0) throw new Exception("推荐人不存在或未激活");

                if (ConfigHelper.GetConfigString("MemberAtlas") != "sun")
                {
                    if (UserService.List(x => x.UserName == entity.ParentUser && x.IsActivation).Count() <= 0) throw new Exception("安置人不存在或未激活");
                }
                //if (ConfigHelper.GetConfigBool("HaveAgent"))
                //{
                //    if (UserService.List(x => x.AgentName == entity.AgentUser && x.IsActivation).Count() <= 0) throw new Exception("商务中心不存在");
                //}

                Data.User _parentUser = null;
                if (ConfigHelper.GetConfigString("MemberAtlas") == "double")
                {
                    _parentUser = UserService.Single(x => x.UserName == entity.ParentUser);
                    if (_parentUser != null)
                    {
                        if (UserService.List(x => x.ParentID == _parentUser.ID).Count() >= 2) throw new Exception("安置人安置名额已满");
                    }
                    if (entity.ChildPlace > 2 || entity.ChildPlace < 1) throw new Exception("安置参数不正确");
                    if (UserService.List(x => x.ParentUser == entity.ParentUser && x.ChildPlace == entity.ChildPlace).Count() > 0) throw new Exception("当前位置无法安置");
                }
                else if (ConfigHelper.GetConfigString("MemberAtlas") == "three")
                {
                    _parentUser = UserService.Single(x => x.UserName == entity.ParentUser);
                    if (_parentUser != null)
                    {
                        if (UserService.List(x => x.ParentID == _parentUser.ID).Count() >= 3) throw new Exception("安置人安置名额已满");
                    }

                    if (entity.ChildPlace > 3 || entity.ChildPlace < 1) throw new Exception("安置参数不正确");
                    if (UserService.List(x => x.ParentUser == entity.ParentUser && x.ChildPlace == entity.ChildPlace).Count() > 0) throw new Exception("当前位置无法安置");
                }
                else
                    _parentUser = UserService.Single(x => x.UserName == entity.RefereeUser);

                entity.IsActivation = false;
                entity.ActivationTime = DateTime.Now;
                entity.IsAgent = false;
                entity.IsLock = false;
                entity.Investment = 0;//cacheSysParam.SingleAndInit(x => x.ID == 1001).Value.ToDecimal();

                //节点部分
                var _refereeUser = UserService.Single(x => x.UserName == entity.RefereeUser);

                //if (_parentUser.RootID != _refereeUser.RootID || Umodel.RootID != _parentUser.RootID) throw new Exception("推荐人和安置人以及您自己必须同一网内会员");
                entity.RefereeDepth = _refereeUser.RefereeDepth + 1;
                entity.RefereePath = _refereeUser.RefereePath + "," + _refereeUser.ID;
                entity.RefereeID = _refereeUser.ID;
                entity.RefereeUser = _refereeUser.UserName;

                //节点部分
                entity.ParentID = _parentUser.ID;
                entity.ParentUser = _parentUser.UserName;
                entity.RootID = _parentUser.RootID;
                entity.Depth = _parentUser.Depth + 1;
                entity.ParentPath = _parentUser.ParentPath + "," + _parentUser.ID;
                entity.Child = 0;

                entity.Password = entity.Password.ToMD5().ToMD5();
                entity.Password2 = entity.Password2.ToMD5().ToMD5();
                entity.CreateTime = DateTime.Now;
                entity.CreateBy = entity.UserName;
                entity.Wallet2001 = 0;
                entity.Wallet2002 = 0;
                entity.Wallet2003 = 0;
                entity.Wallet2004 = 0;
                entity.Wallet2005 = 0;
                entity.AgentLevel = 0;
                entity.UserLevel = 0;

                if (ConfigHelper.GetConfigString("MemberAtlas") == "double")
                    entity.DepthSort = (_parentUser.DepthSort - 1) * 2 + entity.ChildPlace;
                else if (ConfigHelper.GetConfigString("MemberAtlas") == "three")
                    entity.DepthSort = (_parentUser.DepthSort - 1) * 3 + entity.ChildPlace;
                else
                {
                    entity.ChildPlace = UserService.List(x => x.ParentID == _parentUser.ID).Count() > 0 ? UserService.List(x => x.ParentID == _parentUser.ID).Max(x => x.ChildPlace) + 1 : 1;
                    entity.DepthSort = 0;
                }

                UserService.Add(entity);
                _parentUser.Child = _parentUser.Child + 1;
                UserService.Update(_parentUser);
                SysDBTool.Commit();

                if (MvcCore.Unity.Get<ISysParamService>().SingleAndInit(x => x.ID == 4502).Value == "1")
                    SMSHelper.WebChineseMSM(entity.Mobile, MvcCore.Unity.Get<ISysParamService>().SingleAndInit(x => x.ID == 4502).Value2.Replace("{SYSNAME}", sysEntity.SysName).Replace("{USERNAME}", entity.UserName));

                result.Status = 200;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
            return Json(result);
        }

        #endregion


        public ActionResult SendMobileMsm()
        {
            ReturnResult result = new ReturnResult();
            try
            {
                string phone = Request["myphone"];
                if (string.IsNullOrEmpty(phone)) throw new Exception("请您填写手机号码");
                if (!StringHelp.IsPhone(phone)) throw new Exception("请输入正确的手机号码");

                if (Session["SMSSendTime"] != null)
                {
                    if (!DateTimeDiff.DateDiff_minu(DateTime.Parse(Session["SMSSendTime"].ToString())))
                        throw new Exception("每次发送短信间隔不能少于1分钟");
                }

                //ValidateCode vEmailCode = new ValidateCode();
                //string smscode = vEmailCode.CreateRandomCode(5);
                CodeRend vEmailCode = new CodeRend();
                string smscode = vEmailCode.CreateVerifyCode(5);
                Session["SMSValidateCode"] = smscode;
                Session["GetSMSUser"] = phone;
                Session["SMSSendTime"] = DateTime.Now;

                bool b = Services.Tool.SMSHelper.WebChineseMSM(phone, "您本次操作的验证码为：" + smscode + "");
                if (b)  
                    result.Status = 200;
                else
                {
                    Session["SMSValidateCode"] = null;
                    Session["GetSMSUser"] = null;
                    result.Message = "短信发送失败,详情请查阅发送记录";
                    result.Status = 500;
                }
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

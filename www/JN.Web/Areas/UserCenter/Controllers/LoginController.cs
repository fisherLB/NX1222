using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net.Http;
using JN.Data;
using JN.Data.Service;
using MvcCore.Controls;
using System.Web.Security;
using JN.Data.Enum;
using JN.Services;
using JN.Services.Tool;

namespace JN.Web.Areas.UserCenter.Controllers
{
    public class LoginController : Controller
    {
        private static List<Data.SysParam> cacheSysParam = null;
        private readonly IUserService UserService;
        private readonly ISysDBTool SysDBTool;
        private readonly IActLogService ActLogService;
      

        public LoginController(ISysDBTool SysDBTool, IUserService UserService, IActLogService ActLogService)
        {
            this.UserService = UserService;
            this.SysDBTool = SysDBTool;
            this.ActLogService = ActLogService;
          
            cacheSysParam = MvcCore.Unity.Get<ISysParamService>().ListCache("sysparams", x => x.ID < 4000).ToList();

        }
        public ActionResult Index()
        {
            var sys = MvcCore.Unity.Get<JN.Data.Service.ISysSettingService>().Single(1);
            if (!sys.IsOpenUp)
            {
                Response.Write(sys.CloseHint);
                Response.End();
            }

            return View("0227");
        }

        [HttpPost]
        public JsonResult Index(FormCollection fc)
        {
            string username = fc["username"];
            string password = fc["password"];
            string lang = fc["lang"];
            string code = fc["code"];
            string oldurl = Request["oldUrl"];
            string vcode = (Session["UserValidateCode"] ?? "").ToString();
            Session.Remove("UserValidateCode");

            ReturnResult result = new ReturnResult();
            try
            {
                if (string.IsNullOrEmpty(vcode) || string.IsNullOrEmpty(code) || !vcode.Equals(code, StringComparison.InvariantCultureIgnoreCase))
                    throw new Exception("验证码错误");

                if (string.IsNullOrEmpty(username) | string.IsNullOrEmpty(password)) throw new Exception("用户名或密码不能为空");
                var entity = UserLoginHelper.GetUserLoginBy(username, password);
                if (entity != null)
                {
                    if (entity.IsLock) throw new Exception("您的帐号已被冻结,请联系你的推荐人!");
                    if (!entity.IsActivation) throw new Exception("您的帐号没有激活,请联系你的推荐人激活帐号!");
                    var log = new ActLog();
                    log.ActContent = "会员“" + username + "”登录成功！";
                    log.CreateTime = DateTime.Now;
                    log.IP = Request.UserHostAddress;
                    if (Request.UrlReferrer != null)
                        log.Location = Request.UrlReferrer.ToString();
                    log.Platform = "会员";
                    log.Source = "";
                    log.UID = entity.ID;
                    log.UserName = entity.UserName;
                    ActLogService.Add(log);
                    SysDBTool.Commit();

                    result.Status = 200;
                    if (entity.IsActivation)
                        result.Message = oldurl ?? "/UserCenter/Home?lang=" + lang;
                    else
                        result.Message = oldurl ?? "/UserCenter/User/doPass";

                    //如果勾选记住密码，则保存密码一个星期
                    DateTime expiration = DateTime.Now.AddMinutes(20);
                    //if (rp == "1")
                    //    expiration = DateTime.Now.AddDays(7);

                    // 设置Ticket信息
                    FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
                        1,
                        entity.ID.ToString(),
                        DateTime.Now,
                        expiration,
                        false, LoginInfoType.User.ToString());

                    // 加密验证票据
                    string strTicket = FormsAuthentication.Encrypt(ticket);

                    // 使用新userdata保存cookie
                    HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName, strTicket);
                    cookie.Expires = ticket.Expiration;
                    this.Response.Cookies.Add(cookie);
                }
                else
                    throw new Exception("用户名或密码错误");
            }
            catch (Exception ex)
            {
                result.Status = 500;
                result.Message = ex.Message;
            }
            return Json(result);
        }


        public FileResult GetCodeImage(int? width, int? height)
        {
            // change by chenzw :改为如果前台请求有指定尺寸的，则生成指定尺寸的验证码
            if (MvcCore.Unity.Get<ISysSettingService>().Single(1).VerificationCodeType == "001")
            {
                string code = ValidateWhiteBlackImgCode.RandemCode((MvcCore.Unity.Get<ISysSettingService>().Single(1).VerificationCodeLength ?? 5));
                Session["UserValidateCode"] = code;
                return File(ValidateWhiteBlackImgCode.Img(code, width ?? 200, height ?? 75), "image/png");
            }
            else 
            {
                ValidateCode vCode = new ValidateCode();
                string code = vCode.CreateRandomCode((MvcCore.Unity.Get<ISysSettingService>().Single(1).VerificationCodeLength ?? 5));
                Session["UserValidateCode"] = code;
                byte[] bytes = vCode.CreateValidateGraphic(code);
                return File(bytes, @"image/jpeg");
            }
        }

        public ActionResult GetPwd()
        {
            var sys = MvcCore.Unity.Get<JN.Data.Service.ISysSettingService>().Single(1);
            if (!sys.IsOpenUp)
            {
                Response.Write(sys.CloseHint);
                Response.End();
            }
            return View();
        }

        [HttpPost]
        public JsonResult GetPwd(string username, string mobile)
        {
            if (string.IsNullOrEmpty(username) | string.IsNullOrEmpty(mobile))
                return Json(new { status = "err", data = "用户名或取回手机号码不能为空！" }, JsonRequestBehavior.AllowGet);
            var onUser = UserService.Single(x => x.UserName == username);
            if (onUser == null) return Json(new { status = "err", data = "用户名不存在！" }, JsonRequestBehavior.AllowGet);
            if (onUser.Mobile != mobile)
            {
                var wlog = new WarningLog();
                wlog.CreateTime = DateTime.Now;
                wlog.IP = Request.UserHostAddress;
                if (Request.UrlReferrer != null)
                    wlog.Location = Request.UrlReferrer.ToString();
                wlog.Platform = "会员";
                wlog.WarningMsg = "试图通过取回密码修改会员“" + username + "”密码，使用手机号：" + mobile;
                wlog.WarningLevel = "一般";
                wlog.ResultMsg = "拒绝";
                wlog.UserName = username;
                MvcCore.Unity.Get<IWarningLogService>().Add(wlog);
                SysDBTool.Commit();

                return Json(new { status = "err", data = "非法操作！" }, JsonRequestBehavior.AllowGet);
            }

            if (string.IsNullOrEmpty(onUser.Mobile))
                return Json(new { status = "err", data = "用户未设置手机号码，无法取回密码，请联系管理员重置密码。" }, JsonRequestBehavior.AllowGet);
            else
            {
                if (onUser.Mobile == mobile)
                {
                    //ValidateCode vEmailCode = new ValidateCode();
                    //string emailcode = vEmailCode.CreateRandomCode(5);
                    CodeRend vEmailCode = new CodeRend();
                    string emailcode = vEmailCode.CreateVerifyCode(5);
                    Session["EmailValidateCode"] = emailcode;
                    Session["GetPwdUser"] = username;
                    if (Services.Tool.SMSHelper.WebChineseMSM(onUser.Mobile, MvcCore.Unity.Get<ISysParamService>().SingleAndInit(x => x.ID == 4507).Value2.Replace("{USERNAME}",username).Replace("{CODE}",emailcode)))
                        return Json(new { status = "ok", data = "/UserCenter/Login/GetPwd2" }, JsonRequestBehavior.AllowGet);
                    else
                        return Json(new { status = "err", data = "短信发送失败，请联系管理员" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = "err", data = "用户名与手机号码不匹配" }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public ActionResult GetPwd2()
        {
            return View();
        }

        [HttpPost]
        public JsonResult GetPwd2(string smscode, string password, string password2, string password3)
        {
            if (string.IsNullOrEmpty(smscode))
                return Json(new { result = "err", refMsg = "验证码不能为空！" }, JsonRequestBehavior.AllowGet);

            if (Session["EmailValidateCode"] != null && Session["GetPwdUser"] != null && smscode.ToLower() == Session["EmailValidateCode"].ToString().ToLower())
            {
                var username = Session["GetPwdUser"].ToString();
                var chUser = UserService.Single(x => x.UserName == username);
                if (chUser == null) return Json(new { result = "err", refMsg = "用户信息已丢失，请重新找回密码" }, JsonRequestBehavior.AllowGet);

                chUser.Password = password.ToMD5().ToMD5();
                chUser.Password2 = password2.ToMD5().ToMD5();
                //chUser.Password3 = password3.ToMD5().ToMD5();
                UserService.Update(chUser);
                SysDBTool.Commit();
                return Json(new { result = "ok", refMsg = "/UserCenter/Login/GetPwd3" }, JsonRequestBehavior.AllowGet);
            }
            else
                return Json(new { result = "err", refMsg = "验证码不正确或已过期" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetPwd3()
        {
            return View();
        }

    }
}

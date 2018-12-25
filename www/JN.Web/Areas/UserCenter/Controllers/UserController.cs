using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using JN.Data.Service;
using MvcCore.Controls;
using Webdiyer.WebControls.Mvc;
using JN.Data.Extensions;
using JN.Services.Tool;
using JN.Services.Manager;
using System.IO;
using System.Text.RegularExpressions;

namespace JN.Web.Areas.UserCenter.Controllers
{
    public class UserController : BaseController
    {
        private static List<Data.SysParam> cacheSysParam = null;
      
        private readonly IUserService UserService;
        private readonly ISysDBTool SysDBTool;
        private readonly IActLogService ActLogService;
    
        public UserController(ISysDBTool SysDBTool, IUserService UserService, IActLogService ActLogService)
        {
            this.UserService = UserService;
            this.SysDBTool = SysDBTool;
            this.ActLogService = ActLogService;
           
            cacheSysParam = MvcCore.Unity.Get<ISysParamService>().ListCache("sysparams", x => x.ID < 4000).ToList();

        }

        #region 已激活会员列表
        public ActionResult Activation(int? page)
        {
            ActMessage = "已激活会员列表";
            ViewBag.Title = ActMessage;
            var list = UserService.List(x => x.IsActivation && (x.CreateBy == Umodel.UserName || x.RefereeID == Umodel.ID)).WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).OrderByDescending(x => x.ID).ToList();
            
            if (Request.IsAjaxRequest())
                return PartialView("_Activation", list.ToPagedList(page ?? 1, 20));

            return View("Index", list.ToPagedList(page ?? 1, 20));
        }
        #endregion

        #region 未激活会员列表
        /// <summary>
        /// 未激活会员
        /// </summary>
        /// <param name="page">页码</param>
        /// <returns></returns>
        public ActionResult UnActivation(int? page)
        {
            ActMessage = "待激活会员列表";
            ViewBag.Title = ActMessage;
            var list = UserService.List(x => x.IsActivation == false && (x.CreateBy == Umodel.UserName || x.RefereeID == Umodel.ID)).WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).OrderByDescending(x => x.ID).ToList();
            return View("index", list.ToPagedList(page ?? 1, 20));
        }
        #endregion

        #region 删除用户（未激活状态下）
        /// <summary>
        /// 删除用户，未激活时才可以
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Delete(int id)
        {
            //只允许删除您推荐的用户
            var onUser = UserService.Single(id);
            if (onUser != null)
            {
                if (Umodel.UserName == onUser.CreateBy || Umodel.AgentID == onUser.AgentID)
                {
                    if (!onUser.IsActivation)
                    {
                        //删除时上级子节点减少
                        if (onUser.ParentID > 0)
                        {
                            Data.User _parentUser = UserService.Single(onUser.ParentID);
                            _parentUser.Child = _parentUser.Child - 1;
                            UserService.Update(_parentUser);
                        }
                        UserService.Delete(id);
                        SysDBTool.Commit();
                            ViewBag.SuccessMsg = "会员“" + onUser.UserName + "”已被删除！";
                            ActMessage = "删除会员";
                            return View("Success");
                    }
                    else
                    {
                        ViewBag.ErrorMsg = "该会员已被激活，无法删除。";
                        return View("Error");
                    }
                }
                else
                {
                    ViewBag.ErrorMsg = "只允许删除您创建的用户。";
                    return View("Error");
                }
            }
            ViewBag.ErrorMsg = "系统异常！请查看系统日志。";
            return View("Error");
        }
        #endregion

        #region 激活用户
        public ActionResult doPass()
        {
            return View();
        }

        [HttpPost]
        /// <summary>
        /// 激活用户
        /// </summary>
        /// <returns></returns>
        public ActionResult doPass(FormCollection fc)
        {
              ReturnResult result = new ReturnResult();
            try
            {
                //string pincode = fc["pincode"];
                string id = fc["id"];
                //if (MvcCore.Unity.Get<IPINCodeService>().List(x => x.UID == Umodel.ID && x.IsUsed == false && x.KeyCode == pincode.Trim()).Count() <= 0)
                //    throw new Exception("激活码无效！");
                int jh = cacheSysParam.SingleAndInit(x => x.ID==2004).Value2.ToInt();
                if(Umodel.Wallet2004<jh)
                    throw new Exception("对不起，您的激活币不足，无法激活！");
                var onUser = UserService.Single(id.ToInt());
                if (onUser == null) throw new Exception("无效会员！");
                if (!onUser.IsActivation)
                {
                    //激活所需要的金币

                    #region 事务操作
                    using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
                    {
                        Wallets.changeWallet(Umodel.ID, -jh, 2004, "激活会员" + onUser.UserName + "扣除激活币" + jh + "个");

                        //激活会员
                        Users.ActivationAccount(onUser);

                        //var codeEntity = MvcCore.Unity.Get<IPINCodeService>().Single(x => x.KeyCode == pincode);
                        //codeEntity.IsUsed = true;
                        //codeEntity.UseUID = onUser.ID;
                        //codeEntity.UseUserName = onUser.UserName;
                        //codeEntity.UseTime = DateTime.Now;
                        //MvcCore.Unity.Get<IPINCodeService>().Update(codeEntity);
                        //SysDBTool.Commit();
                        ts.Complete();
                        result.Status = 200;
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                Services.Manager.logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
            return Json(result);
        }
        #endregion

        public ActionResult MyLink()
        {
            return View();
        }

        #region 添加一个会员
        public ActionResult Add()
        {
            var sys = MvcCore.Unity.Get<JN.Data.Service.ISysSettingService>().Single(1);
            if (!sys.IsRegisterOpen)
            {
                ViewBag.ErrorMsg = sys.CloseRegisterHint;
                return View("Error");
            }


            ViewBag.Title = "添加一个新会员";
            ActMessage = ViewBag.Title;

            ViewBag.CurrentUser = Umodel.UserName;
            if (Umodel.IsAgent ?? false)
                ViewBag.AgentName = Umodel.AgentName;
            else
                ViewBag.AgentName = Umodel.AgentUser;

            int _parentid = Request["ParentID"].ToInt();

            if (UserService.List(x => x.ID == _parentid && x.IsActivation).Count() > 0)
                ViewBag.ParentUser = UserService.Single(_parentid).UserName;

            return View();
        }

        [HttpPost]
        public ActionResult Add(FormCollection fc)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                var sysEntity = MvcCore.Unity.Get<JN.Data.Service.ISysSettingService>().Single(1);
                var entity = new Data.User();


                //20160711安全更新 ---------------- start
                if (!string.IsNullOrEmpty(fc["wallet2001"]) || !string.IsNullOrEmpty(fc["wallet2002"]) || !string.IsNullOrEmpty(fc["wallet2003"]))
                {
                    var wlog = new Data.WarningLog();
                    wlog.CreateTime = DateTime.Now;
                    wlog.IP = Request.UserHostAddress;
                    if (Request.UrlReferrer != null)
                        wlog.Location = Request.UrlReferrer.ToString();
                    wlog.Platform = "会员";
                    wlog.WarningMsg = "试图在添加会员时篡改数据(试图篡改钱包等敏感数据)";
                    wlog.WarningLevel = "严重";
                    wlog.ResultMsg = "拒绝";
                    wlog.UserName = Umodel.UserName;
                    MvcCore.Unity.Get<IWarningLogService>().Add(wlog);
                    SysDBTool.Commit();

                    Umodel.IsLock = true;
                    Umodel.LockTime = DateTime.Now;
                    Umodel.LockReason = "试图在添加会员时篡改数据(详情查看日志)";
                    MvcCore.Unity.Get<IUserService>().Update(Umodel);
                    MvcCore.Unity.Get<ISysDBTool>().Commit();
                    throw new Exception("非法数据请求，您的帐号已被冻结");
                }
                //20160711安全更新  --------------- end

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
                if (Umodel.IsLock) throw new Exception("您的帐号受限，无法进行相关操作");
                if (Umodel.Password2 != fc["mypassword2"].ToMD5().ToMD5()) throw new Exception("您的安全密码不正确");
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
                //    if (UserService.List(x => x.AgentName == entity.ParentUser && x.IsActivation).Count() <= 0) throw new Exception("商务中心不存在");
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
                entity.CreateBy = Umodel.UserName;
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
                    entity.ChildPlace =UserService.List(x => x.ParentID == _parentUser.ID).Count() > 0?  UserService.List(x => x.ParentID == _parentUser.ID).Max(x => x.ChildPlace) + 1 : 1;
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

        public ActionResult Account()
        {
            ActMessage = "币主帐号";
            return View(Umodel);
        }

        #region 修改个人资料
        public ActionResult Modify()
        {
            ViewBag.Title = "修改个人资料";
            ActMessage = ViewBag.Title;
            return View(Umodel);
        }

        [HttpPost]
        public JsonResult Modify(FormCollection fc)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                var entity = UserService.SingleAndInit(Umodel.ID);
                var onUser = entity.ToModel<Data.User>();

                //20160711安全更新 ---------------- start
                if (!string.IsNullOrEmpty(fc["wallet2001"]) || !string.IsNullOrEmpty(fc["wallet2002"]) || !string.IsNullOrEmpty(fc["wallet2003"]))
                {
                    var wlog = new Data.WarningLog();
                    wlog.CreateTime = DateTime.Now;
                    wlog.IP = Request.UserHostAddress;
                    if (Request.UrlReferrer != null)
                        wlog.Location = Request.UrlReferrer.ToString();
                    wlog.Platform = "会员";
                    wlog.WarningMsg = "试图在修改资料时篡改数据(试图篡改钱包等敏感数据)";
                    wlog.WarningLevel = "严重";
                    wlog.ResultMsg = "拒绝";
                    wlog.UserName = Umodel.UserName;
                    MvcCore.Unity.Get<IWarningLogService>().Add(wlog);
                    SysDBTool.Commit();

                    Umodel.IsLock = true;
                    Umodel.LockTime = DateTime.Now;
                    Umodel.LockReason = "试图在修改资料时篡改数据(详情查看日志)";
                    MvcCore.Unity.Get<IUserService>().Update(Umodel);
                    MvcCore.Unity.Get<ISysDBTool>().Commit();
                    throw new Exception("非法数据请求，您的帐号已被冻结");
                }
                //20160711安全更新  --------------- end

                string vierypassword = fc["vierypassword"];
                if (Umodel.Password2 != vierypassword.ToMD5().ToMD5()) throw new Exception("安全密码不正确");
                if (Umodel.IsLock) throw new Exception("您的帐号受限，无法进行相关操作");
                TryUpdateModel(entity, fc.AllKeys);
                UserService.Update(entity);
                SysDBTool.Commit();

                string msg = "";
                if (onUser.Mobile != entity.Mobile) msg += "手机变更：" + onUser.Mobile + " => " + entity.Mobile;
                if (onUser.RealName != entity.RealName) msg += "　姓名变更：" + onUser.RealName + " => " + entity.RealName;
                if (onUser.AliPay != entity.AliPay) msg += "　支付宝变更：" + onUser.AliPay + " => " + entity.AliPay;
                if (onUser.BankCard != entity.BankCard) msg += "　银行卡变更：" + onUser.BankCard + " => " + entity.BankCard;

                var wlog2 = new Data.WarningLog();
                wlog2.CreateTime = DateTime.Now;
                wlog2.IP = Request.UserHostAddress;
                if (Request.UrlReferrer != null)
                    wlog2.Location = Request.UrlReferrer.ToString();
                wlog2.Platform = "会员";
                wlog2.WarningMsg = "会员自主修改资料，验证成功" + (!string.IsNullOrEmpty(msg) ? ",涉及改动信息：" + msg : "");
                wlog2.WarningLevel = "一般";
                wlog2.ResultMsg = "通过";
                wlog2.UserName = entity.UserName;
                MvcCore.Unity.Get<IWarningLogService>().Add(wlog2);
                SysDBTool.Commit();

                result.Status = 200;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                Services.Manager.logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
            return Json(result);
        }

        [HttpPost]
        public JsonResult ChangePassword(FormCollection form)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                string vierypassword = form["vierypassword"];
                string newpassword = form["newpassword"];
                string connewpassword = form["connewpassword"];
                var entity = UserService.SingleAndInit(Umodel.ID);
                if (string.IsNullOrEmpty(newpassword.Trim()) || string.IsNullOrEmpty(connewpassword.Trim())) throw new Exception("新登录密码与确认新登录密码不能为空");
                if (newpassword != connewpassword) throw new Exception("新登录密码与确认新登录密码不相符");
                if (Umodel.Password2 != vierypassword.ToMD5().ToMD5()) throw new Exception("安全密码不正确");

                entity.Password = newpassword.ToMD5().ToMD5();
                UserService.Update(entity);
                SysDBTool.Commit();
                result.Status = 200;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
            return Json(result);
        }

        [HttpPost]
        public JsonResult ChangePassword2(FormCollection form)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                string vierypassword = form["safepwd"];
                string newpassword2 = form["newpassword2"];
                string connewpassword2 = form["connewpassword2"];
                var entity = UserService.SingleAndInit(Umodel.ID);
                if (string.IsNullOrEmpty(newpassword2.Trim()) || string.IsNullOrEmpty(connewpassword2.Trim())) throw new Exception("新安全密码与确认新安全密码不能为空");
                if (newpassword2 != connewpassword2) throw new Exception("新安全密码与确认新安全密码不相符");
                if (Umodel.Password2 != vierypassword.ToMD5().ToMD5()) throw new Exception("旧安全密码不正确");

                entity.Password2 = newpassword2.ToMD5().ToMD5();
                UserService.Update(entity);
                SysDBTool.Commit();
                result.Status = 200;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                Services.Manager.logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
            return Json(result);
        }
        #endregion


        #region 个人信息
        public ActionResult MyInfo()
        {
            ViewBag.Title = "个人信息";
            ActMessage = ViewBag.Title;
            return View(Umodel);
        }
        #endregion

        #region 验证会员信息(返回JSON)
        /// <summary>
        /// 验证会员信息(返回JSON)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public JsonResult CheckUserInfo(string username)
        {
            var model = UserService.Single(x => x.UserName == username);
            if (model != null)
            {
                string okmsg = "";// "会员编号：" + model.UserName + "";
                //if (model.RealName.Length > 0)
                //    okmsg += "，姓名：*" + model.RealName.Substring(1, model.RealName.Length - 1);

                // if (model.Mobile.Length > 7)
                //     okmsg += "，电话：" + model.Mobile.Substring(0,3) + "****" + model.Mobile.Substring(7, model.Mobile.Length - 7);

                return Json(new {result = "ok", refMsg = "会员验证成功" + okmsg}, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { result = "err", refMsg = "该用户不存在" }, JsonRequestBehavior.AllowGet);
            }

        }

        public JsonResult CheckUserName(string username)
        {
            var model = UserService.Single(x => x.UserName == username);
            if (model != null)
            {
                string okmsg = "会员编号已被使用";
                return Json(new { result = "err", refMsg = okmsg }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { result = "ok", refMsg = "恭喜您，该会员编号可以使用" }, JsonRequestBehavior.AllowGet);
            }

        }
        #endregion

        #region 树视图（太阳线）
        /// <summary>
        /// 树状视图
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult TreeView()
        {
            if (!Umodel.IsActivation) return RedirectToAction("doPass", "User");
            ViewBag.Title = "会员图谱";
            ActMessage = ViewBag.Title;
            return View(Umodel);
        }


        /// <summary>
        /// 获取树状节点数据json格式
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public JsonResult GetTreeJson(int id)
        {
            //int allchild;
            int child;
            IList<TreeNode> list = new List<TreeNode>();
            int tjrs = MvcCore.Unity.Get<JN.Data.Service.IUserService>().List(x => x.RefereeID == Umodel.ID && x.IsActivation).Count();
            tjrs = Math.Min(7, tjrs);
            //取所有父ID与参数相符数据封装到 List<TUser> 并以JSON格式返回
            var ulst = UserService.List(x => x.RefereeID == id).ToList();
            foreach (var mt in ulst)
            {
                child = UserService.List(x => x.RefereeID == mt.ID).Count();
                //allchild = users.GetRecordCount("ParentPath like '%," + mt.ID.ToString() + ",%' ") + child;
                TreeNode p = new TreeNode();
                p.id = mt.ID;
                if (mt.IsActivation)
                {
                    if (mt.IsAgent ?? false)
                        p.text = "<i style='color:#f00'>" + mt.UserName + "(经理" + mt.RealName + ",推荐" + child + "人)</i>";
                    else
                        p.text = "" + mt.UserName + "(" + mt.RealName + ",推荐" + child + "人)";
                }
                else
                    p.text = "<em style='color:#999'>" + mt.UserName + "(未激活)</em>";

                if (mt.RefereeID == 0)
                    p.type = "root";

                //if (child > 0 && mt.RefereeDepth < (Umodel.RefereeDepth + tjrs))
                if (child > 0)
                {
                    p.icon = "fa fa-users";
                    p.children = true;
                }
                else
                {
                    p.icon = "fa fa-user";
                    //p.state = "disabled=true";
                    p.children = false;
                }
                list.Add(p);
            }
            return Json(list, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 列表视图（太阳线）
        /// <summary>
        /// 列表视图
        /// </summary>
        /// <param name="df">日期字段</param>
        /// <param name="dr">日期范围</param>
        /// <param name="kf">查询字段</param>
        /// <param name="kv">查询关键字</param>
        /// <param name="page">页码</param>
        /// <returns></returns>
        public ActionResult ListView(int? page)
        {
            ActMessage = "自属业绩";
            var list = UserService.List(x => x.RefereeID == Umodel.ID && x.IsActivation).WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).OrderByDescending(x => x.ID).ToList();
            return View(list.ToPagedList(page ?? 1, 20));
        }
        #endregion

    }
}

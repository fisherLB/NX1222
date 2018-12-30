using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using JN.Data.Service;
using MvcCore.Controls;
using PagedList;
using JN.Data.Extensions;
using JN.Services.Tool;
using JN.Services.Manager;
using System.IO;
using JN.Services.CustomException;
using System.Data.Entity.Validation;

namespace JN.Web.Areas.UserCenter.Controllers
{
    public class USDController : BaseController
    {
        private static List<Data.SysParam> cacheSysParam = null;
        private static Data.SysSetting syssetting = MvcCore.Unity.Get<ISysSettingService>().Single(1);
        private readonly IUserService UserService;
        private readonly IUSDPurchaseService USDPurchaseService;
        private readonly IUSDPutOnService USDPutOnService;
        private readonly IUSDSeekService USDSeekService;
        private readonly ISysDBTool SysDBTool;
        private readonly IActLogService ActLogService;

        public USDController(ISysDBTool SysDBTool,
            IUserService UserService,
            IUSDPurchaseService USDPurchaseService,
            IUSDPutOnService USDPutOnService,
            IUSDSeekService USDSeekService,
            IActLogService ActLogService)
        {
            this.UserService = UserService;
            this.USDPurchaseService = USDPurchaseService;
            this.USDPutOnService = USDPutOnService;
            this.USDSeekService = USDSeekService;
            this.SysDBTool = SysDBTool;
            this.ActLogService = ActLogService;
            //TODO: 应在 ISysParamService 做个缓存函数，所有地方可以统一调用。
            cacheSysParam = MvcCore.Unity.Get<JN.Data.Service.ISysParamService>().ListCache("sysparams", x => x.ID < 4000).ToList();
        }

        public ActionResult TTC()
        {
            ViewBag.Title = "EP交易大厅";
            ViewBag.Path = "交易市场";
            ActMessage = ViewBag.Title;

            return View(Umodel);
        }
        public ActionResult Buy(int id)
        {
            var model = USDPutOnService.Single(id);
            if (model != null)
            {
                ViewBag.Title = "购买EP";
                ViewBag.Path = "交易市场";
                ActMessage = ViewBag.Title;
                return View(model);
            }
            else
            {
                ViewBag.ErrorMsg = "记录不存在或已被删除";
                return View("Error");
            }
        }

        [HttpPost]
        public ActionResult Buy(FormCollection form)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                string buynumber = form["buynumber"];
                string money = form["money"];
                string phone = form["phone"];
                string pid = form["pid"];
                if (buynumber.ToDecimal() <= 0) throw new CustomException("交易数量不正确");

                Data.USDPutOn puton = USDPutOnService.Single(pid.ToInt());
                if (puton == null) throw new CustomException("记录不存在或已被删除");
                if (puton.UID == Umodel.ID) throw new CustomException("无法购买自己的订单");
                string tradePassword = form["tradeinPassword"];
                if (Umodel.Password2 != tradePassword.ToMD5().ToMD5()) throw new CustomException("二级密码不正确");
                if (Umodel.IsLock) throw new CustomException("您的帐号受限，无法进行相关操作");
                var sellUser = UserService.Single(puton.UID);
                if (sellUser == null || sellUser.IsLock) throw new CustomException("会员状态异常,无法交易");
             

                if (puton.Status != 1) throw new CustomException("交易状态已发生改变，无法进行交易");

                if (USDPutOnService.Single(puton.ID).Status == 1)
                {
                    string identification_code = Umodel.UserName.ToMD5();
                    //写入买入表
                    USDPurchaseService.Add(new Data.USDPurchase { BuyAmount = puton.PutAmount, OrderNumber = USDPurchase.GetOrderNumber(), BuyMoney = puton.PutMoney, PutOnID = puton.ID, CreateTime = DateTime.Now, Status = 1, UID = Umodel.ID, UserName = Umodel.UserName, SellUID = puton.UID, SellUserName = puton.UserName, SeekID = puton.SeekID, AgentUser = Umodel.AgentUser, ReserveStr2 = "交易大厅买入(从卖出信息)，识别码：" + identification_code });
                    //改新卖出记录
                    puton.DealAmount = puton.PutAmount; // puton.DealAmount + buyamount;
                    puton.Status = (int)Data.Enum.USDStatus.Transaction;
                    USDPutOnService.Update(puton);
                    SysDBTool.Commit();
                    result.Status = 200;
                }


                var onUser = UserService.Single(puton.UID);
                if (onUser != null && !string.IsNullOrEmpty(onUser.Mobile))
                {
                    SMSHelper.WebChineseMSM(onUser.Mobile, "账户：" + Umodel.UserName + "购买了你的激活币，请留意收款");
                    //SMSHelper.WebChineseMSM(Umodel.Mobile, "你购买了" + onUser.UserName + "的激活币，请在" + (cacheSysParam.SingleAndInit(x => x.ID == 2202).Value.ToInt() / 60) + "小时内完成付款");

                    SMSHelper.WebChineseMSM(Umodel.Mobile, "你购买了" + onUser.UserName + "的激活币，请及时完成付款");
                }

            }
            catch (CustomException ex)
            {
                result.Message = ex.Message;
            }
            catch (DbEntityValidationException ex)
            {
                result.Message = "网络系统繁忙，请稍候再试!";
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
            catch (Exception ex)
            {
                result.Message = "网络系统繁忙，请稍候再试!";
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
            return Json(result);
        }

        /// <summary>
        /// 市场成交明细
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public ActionResult Purchase(int page = 1)
        {
            ActMessage = "我买入的交易";
            var list = USDPurchaseService.List(x => x.UID == Umodel.ID && x.Status < 3).OrderByDescending(x => x.ID).ToList();
            return View(list.ToPagedList(page, 20));
        }

        /// <summary>
        /// 市场成交明细
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public ActionResult HPurchase(int page = 1)
        {
            ActMessage = "我的交易记录";
            var list = USDPurchaseService.List(x => (x.UID == Umodel.ID || x.SellUID == Umodel.ID) && x.Status == 3).OrderByDescending(x => x.ID).ToList();
            return View(list.ToPagedList(page, 20));
        }

        /// <summary>
        /// 我卖出的交易
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public ActionResult PutOn(int page = 1)
        {
            ActMessage = "我卖出的交易";
            var list = USDPutOnService.List(x => x.UID == Umodel.ID).OrderByDescending(x => x.ID).ToList();
            return View(list.ToPagedList(page, 20));
        }

        public ActionResult AllPutOn(int page = 1)
        {
            ActMessage = "交易大厅";
            var list = USDPutOnService.List(x => x.Status == 1).OrderByDescending(x => x.ID).ToList();
            return View(list.ToPagedList(page, 20));
        }
        public ActionResult Seek(int page = 1)
        {
            ActMessage = "我发布的求购";
            var list = USDSeekService.List(x => x.UID == Umodel.ID).OrderByDescending(x => x.ID).ToList();
            return View(list.ToPagedList(page, 20));
        }

        public ActionResult ApplyPutOn()
        {
            string seekid = Request["seekid"];

            if (string.IsNullOrEmpty(seekid))
            {
                ViewBag.Title = "挂单卖出";
                ViewBag.Path = "交易市场";
                ViewBag.SeekAmount = 0;
                ViewBag.SeekMoney = 0;
                ViewBag.SeekUnitPrice = cacheSysParam.SingleAndInit(x => x.ID == 2203).Value;
                ActMessage = ViewBag.Title;
                return View(Umodel);
            }
            else
            {
                var seek = USDSeekService.Single(seekid.ToInt());
                if (seek != null)
                {
                    ViewBag.Title = "挂单卖出";
                    ViewBag.Path = "交易市场";
                    ViewBag.SeekID = seekid;
                    ViewBag.SeekAmount = (seek.SeekAmount - (seek.HaveDeal ?? 0));
                    ViewBag.SeekMoney = seek.SeekMoney;
                    ViewBag.SeekUnitPrice = seek.SeekUnitPrice;
                    ViewBag.MoneyType = seek.MoneyType;
                    ViewBag.PayType = seek.PayType;
                    ActMessage = ViewBag.Title;
                    return View(Umodel);
                }
                else
                {
                    ViewBag.ErrorMsg = "记录不存在或已被删除";
                    return View("Error");
                }
            }
        }

        [HttpPost]
        public ActionResult ApplyPutOn(FormCollection form)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                //string putonnumber = form["putonnumber"];
                string putonnumber = form["putonnumber"];
                string moneytype = form["moneytype"];
                //string remark = form["remark"];
                string paytype = form["paytype"];
                //string phone = form["phone"];
                string seekid = form["seekid"];
                string tradePassword = form["tradeoutPassword"];
                string putonmoney= form["rmb"];  
                //decimal PARAM_LowNumber = cacheSysParam.Single(x => x.ID == 2201).Value.ToDecimal(); //最低交易数量

                if (Umodel.Password2 != tradePassword.ToMD5().ToMD5()) throw new CustomException("二级密码不正确");
                if (Umodel.IsLock || !Umodel.IsActivation) throw new CustomException("您的帐号受限，无法进行相关操作");
                if (putonnumber.ToDecimal() <= 0) throw new CustomException("交易数量不正确");
                //if (putonmoney.ToDecimal() < PARAM_LowNumber) throw new CustomException("申请交易金额不能低于" + PARAM_LowNumber + "");
                //系统参数
                //decimal PARAM_POUNDAGEBL = cacheSysParam.SingleAndInit(x => x.ID == 2202).Value.ToDecimal(); //交易手续费
                decimal changeMoney = putonmoney.ToDecimal();
                //decimal poundage = changeMoney * PARAM_POUNDAGEBL;
                //decimal actualChangeMoney = changeMoney + poundage;
                string description = "卖出激活币(从交易大厅)";//（手续费：" + poundage + "）";
                if (Umodel.Wallet2004 < putonnumber.ToDecimal()) throw new CustomException("您的激活币余额不足");

                var Seek = USDSeekService.Single(seekid.ToInt());
                if (Seek != null)
                {
                    if (Seek.Status > 2) throw new CustomException("交易状态已发生改变，无法进行交易");
                    if (Seek.UID == Umodel.ID) throw new CustomException("无法交易自己的订单");

                    var buyUser = UserService.Single(Seek.UID);
                    if (buyUser == null || buyUser.IsLock) throw new CustomException("会员状态异常,无法交易");
                    moneytype = Seek.MoneyType;
                    if (putonnumber.ToDecimal() > (Seek.SeekAmount - (Seek.HaveDeal ?? 0))) throw new CustomException("卖出金额不能高于求购金额");

                    //20160721 新增使用缓存机制解决并发卖出超匹配总额的问题
                    decimal totalmch = 0;
                    if (MvcCore.Extensions.CacheExtensions.CheckCache("seek_" + Seek.ID))
                    {
                        totalmch = MvcCore.Extensions.CacheExtensions.GetCache<decimal>("seek_" + Seek.ID);
                    }
                    else
                    {
                        //TODO: 这里最好加上 ToList() ，避免过多访问数据库。
                        var mchs = MvcCore.Unity.Get<IUSDPurchaseService>().List(x => x.SeekID == Seek.ID && x.Status != -1);
                        totalmch = mchs.Count() > 0 ? mchs.Sum(x => x.BuyMoney) : 0;
                        MvcCore.Extensions.CacheExtensions.SetCache("seek_" + Seek.ID, totalmch);
                    }

                    if (putonmoney.ToDecimal() + totalmch > Seek.SeekMoney) throw new CustomException("卖出金额已超出求购金额");

                    decimal newtotalmch = totalmch + putonmoney.ToDecimal();
                    MvcCore.Extensions.CacheExtensions.SetCache("seek_" + Seek.ID, newtotalmch);
                    //20160721 新增使用缓存机制解决并发卖出超匹配总额的问题
                }

                lock (this)
                {
                    using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
                    {
                        //写入交易表
                        var newmodel = new Data.USDPutOn { MoneyType = moneytype, PutonNumber = USDPutOn.GetPutOnNumber(), PutMoney = putonmoney.ToDecimal(), Phone = Umodel.Mobile, PayType = paytype, CreateTime = DateTime.Now, DealAmount = 0, Poundage = 0, PutAmount = putonnumber.ToDecimal(), Status = 1, UID = Umodel.ID, UserName = Umodel.UserName, SeekID = seekid.ToInt() };
                        USDPutOnService.Add(newmodel);
                        SysDBTool.Commit();
                        Wallets.changeWallet(Umodel.ID, 0 - putonnumber.ToDecimal(), 2004, description);

                        //求购挂单交易
                        if (Seek != null)
                        {
                            var puton = USDPutOnService.Single(newmodel.ID);
                            if (puton != null && puton.Status == (int)Data.Enum.USDStatus.Sales)
                            {
                                //decimal lx = putonmoney.ToDecimal() * Math.Max(0, DateTimeDiff.DateDiff(Seek.CreateTime, DateTime.Now, "d")) * cacheSysParam.SingleAndInit(x => x.ID == 2204).Value.ToDecimal();
                                var buyUser = UserService.Single(Seek.UID);
                                string identification_code = Umodel.UserName.ToMD5();
                                //写入买入表
                                USDPurchaseService.Add(new Data.USDPurchase { BuyAmount = puton.PutAmount, BuyMoney = puton.PutMoney, OrderNumber = USDPurchase.GetOrderNumber(), PutOnID = puton.ID, CreateTime = DateTime.Now, Status = 1, UID = Seek.UID, UserName = Seek.UserName, SellUID = puton.UID, SellUserName = puton.UserName, SeekID = Seek.ID, ReserveDecamal = 0, AgentUser = buyUser.AgentUser, ReserveStr2 = "交易大厅卖出(从购买信息)，识别码：" + identification_code });
                                //改新卖出记录
                                puton.DealAmount = puton.PutAmount; // puton.DealAmount + buyamount;
                                puton.Status = (int)Data.Enum.USDStatus.Transaction;
                                USDPutOnService.Update(puton);

                                Seek.HaveDeal = (Seek.HaveDeal ?? 0) + putonmoney.ToDecimal();
                                Seek.ReserveDecamal = 0;
                                Seek.Status = (int)Data.Enum.USDStatus.Transaction;
                                USDSeekService.Update(Seek);
                                SysDBTool.Commit();
                            }
                        }
                        result.Status = 200;
                        ts.Complete();
                    }
                }

                //var seekUser = UserService.Single(Seek.UID);
                //if (seekUser != null && !string.IsNullOrEmpty(seekUser.Mobile))
                //{
                //    SMSHelper.WebChineseMSM(seekUser.Mobile, "帐户：" + seekUser.UserName + "，您购买的激活币已匹配成功，请在" + (cacheSysParam.SingleAndInit(x => x.ID == 2202).Value.ToInt() / 60) + "小时内完成付款");
                //    SMSHelper.WebChineseMSM(Umodel.Mobile, "账户：" + Umodel.UserName + "您卖出的激活币已匹配成功，请留意收款");
                //}
            }
            catch (CustomException ex)
            {
                result.Message = ex.Message;
            }
            catch (DbEntityValidationException ex)
            {
                result.Message = "网络系统繁忙，请稍候再试!";
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
            catch (Exception ex)
            {
                result.Message = "网络系统繁忙，请稍候再试!";
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
            return Json(result);
        }

        public ActionResult ApplySeek()
        {
            ViewBag.Title = "发布求购";
            ViewBag.Path = "交易市场";
            ActMessage = ViewBag.Title;
            return View(Umodel);
        }

        [HttpPost]
        public ActionResult ApplySeek(FormCollection form)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                string putoutnumber = form["putoutnumber"];  //数量
                string seekmoney = form["rmb2"]; 
                string moneytype = form["moneytype"];
                string paytype = form["paytype"];
                //string phone = form["phone"];
                string remark = form["remark"];
                string tradePassword = form["tradeinPassword"];
                if (Umodel.Password2 != tradePassword.ToMD5().ToMD5()) throw new CustomException("二级密码不正确");
                if (seekmoney.ToDecimal() <= 0) throw new CustomException("求购数量不正确");
             
                if (Umodel.IsLock) throw new CustomException("您的帐号受限，无法进行相关操作");
               

                int ckupgrade = 0;
             
                //写入交易表
                USDSeekService.Add(new Data.USDSeek
                {
                    CreateTime = DateTime.Now,
                    SeekNumber = SeekNumber.GetSeekNumber(),
                    PayType = paytype,
                    Phone = Umodel.Mobile,
                    MoneyType = moneytype,
                    PutOnID = 0,
                    Remark = "",
                    SeekAmount = putoutnumber.ToDecimal(),
                    SeekMoney =
                    seekmoney.ToDecimal(),
                    Status = 1,
                    UID = Umodel.ID,
                    ReserveInt = ckupgrade,
                    UserName = Umodel.UserName
                });
                SysDBTool.Commit();

                result.Status = 200;
            }
            catch (CustomException ex)
            {
                result.Message = ex.Message;
            }
            catch (DbEntityValidationException ex)
            {
                result.Message = "网络系统繁忙，请稍候再试!";
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
            catch (Exception ex)
            {
                result.Message = "网络系统繁忙，请稍候再试!";
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
            return Json(result);
        }

        /// <summary>
        /// 取消交易　
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult CancelPutOn(int id)
        {
            var model = USDPutOnService.Single(id);
            if (model != null)
            {
                if (model.UID == Umodel.ID)
                {
                    if (model.Status < 2 && model.Status >= 0)
                    {
                        Wallets.changeWallet(model.UID, model.PutAmount, 2002, "取消交易，本人取消");
                        model.Status = -1;
                        USDPutOnService.Update(model);
                        SysDBTool.Commit();
                        if (Request.UrlReferrer != null)
                        {
                            ViewBag.FormUrl = Request.UrlReferrer.ToString();
                        }
                        ViewBag.SuccessMsg = "成功取消交易！";
                        ActMessage = ViewBag.SuccessMsg;
                        return View("Success");
                    }
                    else
                    {
                        if (model.Status >= 2)
                            ViewBag.ErrorMsg = "交易已成交，无法取消操作。";
                        else
                            ViewBag.ErrorMsg = "交易已取消，请不要重复取消。";
                        return View("Error");
                    }
                }
                else
                {
                    ViewBag.ErrorMsg = "错误的参数。";
                    return View("Error");
                }
            }
            ViewBag.ErrorMsg = "记录不存在或已被删除！";
            return View("Error");
        }

        /// <summary>
        /// 中止交易
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult CancelPurchase(int id)
        {
            //Data.USDPurchase model = USDPurchaseService.Single(id);
            //if (model != null)
            //{
            //    if (model.UID == Umodel.ID)
            //    {

            //        if (model.Status == 1)
            //        {
            //            model.Status = -1;
            //            USDPurchaseService.Update(model);
            //            SysDBTool.Commit();

            //            var updatePutOn = USDPutOnService.Single(model.PutOnID);
            //            if (updatePutOn != null)
            //            {
            //                updatePutOn.Status = 1;
            //                USDPutOnService.Update(updatePutOn);
            //                SysDBTool.Commit();
            //            }
            //            if (Request.UrlReferrer != null)
            //            {
            //                ViewBag.FormUrl = Request.UrlReferrer.ToString();
            //            }
            //            ViewBag.SuccessMsg = "成功取消交易，交易状态将重新挂单！";
            //            ActMessage = ViewBag.SuccessMsg;
            //            return View("Success");
            //        }
            //        else
            //        {
            //            if (model.Status >= 2)
            //                ViewBag.ErrorMsg = "交易已付款或成交，当前交易状态无法取消。";
            //            else
            //                ViewBag.ErrorMsg = "交易已取消，请不要重复取消。";
            //            return View("Error");
            //        }
            //    }
            //    else
            //    {
            //        ViewBag.ErrorMsg = "错误的参数。";
            //        return View("Error");
            //    }
            //}
            ViewBag.ErrorMsg = "交易无法取消，请联系管理员！";
            return View("Error");
        }

        /// <summary>
        /// 删除求购
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult DelSeek(int id)
        {
            var model = USDSeekService.Single(id);
            if (model != null && model.UID == Umodel.ID)
            {
                if (model.Status == (int)Data.Enum.USDStatus.Sales)
                {
                    ActPacket = model;
                    USDSeekService.Delete(id);
                    SysDBTool.Commit();

                    if (Request.UrlReferrer != null)
                    {
                        ViewBag.FormUrl = Request.UrlReferrer.ToString();
                    }
                    ViewBag.SuccessMsg = "成功删除求购信息！";
                    ActMessage = ViewBag.SuccessMsg;
                    return View("Success");
                }
                else
                {
                    ViewBag.ErrorMsg = "订单当前状态不可删除！";
                    return View("Error");
                }
            }
            ViewBag.ErrorMsg = "记录不存在或已被删除！";
            return View("Error");
        }

        public ActionResult Pay(int id)
        {
            ViewBag.Title = "进行付款";
            ViewBag.Path = "交易市场";
            ActMessage = ViewBag.Title;

            var model = USDPurchaseService.Single(id);
            if (model != null)
            {
                var puton = USDPutOnService.Single(model.PutOnID);
                if (puton != null)
                {
                    ViewBag.PayType = puton.PayType;
                    ViewBag.Remark = puton.Remark;
                    ViewBag.Phone = puton.Phone;

                    return View(model);
                }
            }
            ViewBag.ErrorMsg = "记录不存在或已被删除！";
            return View("Error");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Pay()
        {
            ReturnResult result = new ReturnResult();
            try
            {
                string purid = Request["purid"];
                string paytime = Request["paytime"];
                string payremark = Request["payremark"];
                string imgurl = "";

                if (Request.Files.Count == 0) throw new CustomException("请上传付款截图");
                HttpPostedFileBase file = Request.Files[0];

                if ((file != null) && (file.ContentLength > 0))
                {
                    if (!FileValidation.IsAllowedExtension(file, new FileExtension[] { FileExtension.PNG, FileExtension.JPG, FileExtension.BMP }))
                        throw new CustomException("非法上传，您只可以上传图片格式的文件！");

                    //20160711安全更新 ---------------- start
                    var newfilename = "USD_" + Umodel.UserName + "_" + Guid.NewGuid() + Path.GetExtension(file.FileName).ToLower();
                    if (!Directory.Exists(Request.MapPath("~/upfile")))
                        Directory.CreateDirectory(Request.MapPath("~/upfile"));

                    if (Path.GetExtension(file.FileName).ToLower().Contains("aspx"))
                    {
                        var wlog = new Data.WarningLog();
                        wlog.CreateTime = DateTime.Now;
                        wlog.IP = Request.UserHostAddress;
                        if (Request.UrlReferrer != null)
                            wlog.Location = Request.UrlReferrer.ToString();
                        wlog.Platform = "会员";
                        wlog.WarningMsg = "试图上传木马文件";
                        wlog.WarningLevel = "严重";
                        wlog.ResultMsg = "拒绝";
                        wlog.UserName = Umodel.UserName;
                        MvcCore.Unity.Get<IWarningLogService>().Add(wlog);

                        Umodel.IsLock = true;
                        Umodel.LockTime = DateTime.Now;
                        Umodel.LockReason = "试图上传木马文件";
                        MvcCore.Unity.Get<IUserService>().Update(Umodel);
                        MvcCore.Unity.Get<ISysDBTool>().Commit();
                        throw new CustomException("试图上传木马文件，您的帐号已被冻结");
                    }

                    var fileName = Path.Combine(Request.MapPath("~/upfile"), newfilename);
                    try
                    {
                        file.SaveAs(fileName);
                        var thumbnailfilename = UploadPic.MakeThumbnail(fileName, Request.MapPath("~/upfile/"), 1024, 768, "EQU");
                        imgurl = "/upfile/" + thumbnailfilename;
                    }
                    catch (Exception ex)
                    {
                        throw new CustomException("上传失败：" + ex.Message);
                    }
                    finally
                    {
                        System.IO.File.Delete(fileName); //删除原文件
                    }
                    //20160711安全更新  --------------- end

                    //if (!Common.Utils.IsDateString(paytime))
                    //    strErr += "日期格式不正确 <br />";
                    string tradePassword = Request["tradeinPassword"];
                    if (Umodel.IsLock) throw new CustomException("您的帐号受限，无法进行相关操作");
                    if (Umodel.Password2 != tradePassword.ToMD5().ToMD5()) throw new CustomException("二级密码不正确");
                    //if (payremark.Trim().Length > 100) throw new CustomException("备注长度不能超过100个字节");

                    var model = USDPurchaseService.Single(purid.ToInt());
                    if (model == null) throw new CustomException("记录不存在或已被删除");
                    if (model.Status >= 2) throw new CustomException("该交易已付款，无需重复提交");

                    model.Status = 2;
                    model.PayTime = DateTime.Now;
                    model.PayRemark = "汇款时间：" + paytime;
                    model.PayImg = imgurl;
                    USDPurchaseService.Update(model);
                    SysDBTool.Commit();

                    result.Status = 200;

                    var putonUser = UserService.Single(model.SellUID);
                    if (putonUser != null && !string.IsNullOrEmpty(putonUser.Mobile))
                    {
                        SMSHelper.WebChineseMSM(Umodel.Mobile, "账户:" + Umodel.UserName + ", 您已成功付款,请等待卖家(账户:" + putonUser.RealName + ",手机:" + putonUser.Mobile + "确认订单");
                        SMSHelper.WebChineseMSM(putonUser.Mobile, "账户:" + model.SellUserName + ", 您卖出的EP订单买家已付款,请尽快登陆平台确认,超时将封停账户");
                    }
                }
            }
            catch (CustomException ex)
            {
                result.Message = ex.Message;
            }
            catch (DbEntityValidationException ex)
            {
                result.Message = "网络系统繁忙，请稍候再试!";
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
            catch (Exception ex)
            {
                result.Message = "网络系统繁忙，请稍候再试!";
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
            return Json(result);
        }

        /// <summary>
        /// 确认收款
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult FinishBuy(int id)
        {
            var model = USDPurchaseService.Single(id);
            if (model != null)
            {
                if (model.Status == 2)
                {
                    if (USDPutOnService.Single(model.PutOnID).UID == Umodel.ID)
                    {
                        decimal ChangeMoney = model.BuyAmount;
                        string description = "激活币交易入帐,来自“" + model.SellUserName + "”的确认收款";
                        var onUser = UserService.Single(model.UID);
                        if (onUser != null)
                        {

                            Wallets.changeWallet(onUser.ID, model.BuyAmount, 2004, description);
                            if ((model.ReserveDecamal ?? 0) > 0) Wallets.changeWallet(onUser.ID, (model.ReserveDecamal ?? 0), 2001, "求购等候利息");

                            var newSeek = USDSeekService.Single(model.SeekID ?? 0);
                            if (newSeek != null)
                            {
                                if (newSeek.SeekAmount == (newSeek.HaveDeal ?? 0))
                                {
                                    newSeek.Status = (int)Data.Enum.USDStatus.Deal;
                                    USDSeekService.Update(newSeek);
                                }
                            }

                           

                            model.Status = (int)Data.Enum.USDStatus.Deal;
                            model.ReserveDate = DateTime.Now;
                            USDPurchaseService.Update(model);
                            var putOn = USDPutOnService.Single(model.PutOnID);
                            putOn.Status = (int)Data.Enum.USDStatus.Deal;
                            USDPutOnService.Update(putOn);
                            MvcCore.Unity.Get<ISysDBTool>().Commit();

                            if (Request.UrlReferrer != null)
                            {
                                ViewBag.FormUrl = Request.UrlReferrer.ToString();
                            }
                            ViewBag.SuccessMsg = "交易确认成功！";
                            ActMessage = ViewBag.SuccessMsg;
                            return View("Success");
                        }
                    }
                    else
                    {
                        ViewBag.ErrorMsg = "错误的参数。";
                        return View("Error");
                    }
                }
                else
                {
                    ViewBag.ErrorMsg = "当前交易订状态无法进行确认。";
                    return View("Error");
                }
            }
            ViewBag.ErrorMsg = "记录不存在或已被删除！";
            return View("Error");
        }

        /// <summary>
        /// 投诉
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Complaint(int id)
        {
            var model = USDPurchaseService.Single(id);
            if (model != null)
            {
                if (model.Status == 2)
                {
                    var puton = USDPutOnService.Single(model.PutOnID);
                    if (puton.UID == Umodel.ID)
                    {

                        //var onUser = UserService.Single(model.UID);
                        //if (onUser != null)
                        //{
                        //    onUser.IsLock = true;
                        //    onUser.LockReason = model.SellUserName + "对EP交易订单进行投诉";
                        //    UserService.Update(onUser);
                        //}

                        //var doUser = UserService.Single(model.SellUID);
                        //if (doUser != null)
                        //{
                        //    doUser.IsLock = true;
                        //    doUser.LockReason = model.SellUserName + "对EP交易订单进行投诉";
                        //    UserService.Update(doUser);
                        //}

                        puton.Status = -2;
                        USDPutOnService.Update(puton);

                        model.Status = -2;
                        USDPurchaseService.Update(model);
                        SysDBTool.Commit();

                        ViewBag.SuccessMsg = "投诉成功，请耐心等待系统审查！";
                        ActMessage = ViewBag.SuccessMsg;
                        return View("Success");
                    }
                    else
                    {
                        ViewBag.ErrorMsg = "错误的参数。";
                        return View("Error");
                    }
                }
                else
                {
                    ViewBag.ErrorMsg = "当前交易订状态无法进行投诉。";
                    return View("Error");
                }
            }
            ViewBag.ErrorMsg = "记录不存在或已被删除！";
            return View("Error");
        }
    }
}
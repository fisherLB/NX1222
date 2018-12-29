using JN.Data;
using JN.Data.Service;
using MvcCore.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using JN.Services.Manager;
using JN.Services.Tool;

namespace JN.Web.Areas.AdminCenter.Controllers
{
    public class USDController : BaseController
    {
        private static List<Data.SysParam> cacheSysParam = null; 

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

        //public ActionResult EPMatch(int pid, int sid)
        //{
        //    var puton = USDPutOnService.Single(pid);
        //    var seek = USDSeekService.Single(sid);
        //    if (puton != null && puton.Status == (int)Data.Enum.USDStatus.Sales && seek != null)
        //    {
        //        decimal lx = Math.Min(0, DateTimeDiff.DateDiff(seek.CreateTime, DateTime.Now, "d")) * cacheSysParam.SingleAndInit(x => x.ID == 2204).Value.ToDecimal();

        //        var buyUser = UserService.Single(seek.UID);
        //        if (buyUser == null) throw new Exception("会员丢失,无法购买");
        //        //写入买入表
        //        USDPurchaseService.Add(new Data.USDPurchase { BuyAmount = puton.PutAmount, OrderNumber = Data.Extensions.USDPurchase.GetOrderNumber(), BuyMoney = puton.PutMoney, PutOnID = puton.ID, CreateTime = DateTime.Now, Status = 1, UID = seek.UID, UserName = seek.UserName, SellUID = puton.UID, SellUserName = puton.UserName, SeekID = seek.ID, ReserveDecamal = lx, AgentUser = buyUser.AgentUser, ReserveStr2 = "后台匹配" });
        //        SysDBTool.Commit();
        //        //改新卖出记录
        //        puton.DealAmount = puton.PutAmount; // puton.DealAmount + buyamount;
        //        puton.Status = (int)Data.Enum.USDStatus.Transaction;
        //        USDPutOnService.Update(puton);
        //        SysDBTool.Commit();

        //        seek.ReserveDecamal = lx;
        //        seek.Status = (int)Data.Enum.USDStatus.Transaction;
        //        USDSeekService.Update(seek);
        //        SysDBTool.Commit();

        //        var seekUser = UserService.Single(seek.UID);
        //        if (seekUser != null && !string.IsNullOrEmpty(seekUser.Mobile))
        //        {
        //            SMSHelper.WebChineseMSM(seekUser.Mobile, "帐户：" + seekUser.UserName + "，您购买的EP已匹配成功，请在" + (cacheSysParam.SingleAndInit(x => x.ID == 2202).Value.ToInt() / 60) + "小时内完成付款");
        //        }

        //        var putonUser = UserService.Single(puton.UID);
        //        if (putonUser != null && !string.IsNullOrEmpty(putonUser.Mobile))
        //        {
        //            SMSHelper.WebChineseMSM(putonUser.Mobile, "账户：" + putonUser.UserName + "您卖出的EP已匹配成功，请留意收款");
        //        }
        //    }

        //    return RedirectToAction("Seek", "USD");
        //}

        public ActionResult MultiEPMatch(string ids, int seekid, string totalmoney)
        {
            string[] mids = ids.TrimEnd(',').TrimStart(',').Split(',');
            foreach (string id in mids)
            {
                int pid = id.ToInt();
                if (pid > 0)
                {
                    var puton = USDPutOnService.Single(pid);
                    var seek = USDSeekService.Single(seekid);
                    if (puton != null && puton.Status == (int)Data.Enum.USDStatus.Sales && seek != null)
                    {
                        if (puton.PutMoney > (seek.SeekMoney - (seek.HaveDeal ?? 0))) throw new Exception("卖出金额不能高于求购金额");

                        var mchs = MvcCore.Unity.Get<IUSDPurchaseService>().List(x => x.SeekID == seek.ID && x.Status != -1);
                        var totalmch = mchs.Count() > 0 ? mchs.Sum(x => x.BuyMoney) : 0;
                        if (puton.PutMoney + totalmch > seek.SeekMoney) throw new Exception("卖出金额不能高于求购金额");

                        using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
                        {
                            decimal lx = puton.PutMoney * Math.Max(0, DateTimeDiff.DateDiff(seek.CreateTime, DateTime.Now, "d")) * cacheSysParam.SingleAndInit(x => x.ID == 2204).Value.ToDecimal();

                            var buyUser = UserService.Single(seek.UID);
                            string identification_code = buyUser.UserName.ToMD5();
                            //写入买入表
                            USDPurchaseService.Add(new Data.USDPurchase { BuyAmount = puton.PutAmount, OrderNumber = Data.Extensions.USDPurchase.GetOrderNumber(), BuyMoney = puton.PutMoney, PutOnID = puton.ID, CreateTime = DateTime.Now, Status = 1, UID = seek.UID, UserName = seek.UserName, SellUID = puton.UID, SellUserName = puton.UserName, SeekID = seek.ID, ReserveDecamal = lx, AgentUser = buyUser.AgentUser, ReserveStr2 = "后台匹配" });
                            SysDBTool.Commit();//TODO:此处 Commit() 可以取消
                            //改新卖出记录
                            puton.DealAmount = puton.PutAmount; // puton.DealAmount + buyamount;
                            puton.Status = (int)Data.Enum.USDStatus.Transaction;
                            USDPutOnService.Update(puton);
                            SysDBTool.Commit();//TODO:此处 Commit() 可以取消

                            seek.HaveDeal = (seek.HaveDeal ?? 0) + puton.PutAmount;
                            seek.ReserveDecamal = lx;
                            seek.Status = (int)Data.Enum.USDStatus.Transaction;
                            USDSeekService.Update(seek);
                            SysDBTool.Commit();
                            ts.Complete();
                        }
                        var seekUser = UserService.Single(seek.UID);
                        if (seekUser != null && !string.IsNullOrEmpty(seekUser.Mobile))
                        {
                            SMSHelper.WebChineseMSM(seekUser.Mobile, "帐户：" + seekUser.UserName + "，您购买的EP已匹配成功，请在" + (cacheSysParam.SingleAndInit(x => x.ID == 2202).Value.ToInt() / 60) + "小时内完成付款");
                        }

                        var putonUser = UserService.Single(puton.UID);
                        if (putonUser != null && !string.IsNullOrEmpty(putonUser.Mobile))
                        {
                            SMSHelper.WebChineseMSM(putonUser.Mobile, "账户：" + putonUser.UserName + "您卖出的EP已匹配成功，请留意收款");
                        }
                    }
                }
            }
            return Content("ok");
        }

        public ActionResult Exchange()
        {
            ActMessage = "EP交易市场开启关闭";
            return View();
        }

        public ActionResult ExchangeCommand(string commandtype)
        {
            //var sysEntity = MvcCore.Unity.Get<JN.Data.Service.ISysSettingService>().Single(1);
            //if (commandtype.ToLower() == "open")
            //    sysEntity.IsOpenEP = true;
            //else if (commandtype.ToLower() == "close")
            //    sysEntity.IsOpenEP = false;
            //MvcCore.Unity.Get<JN.Data.Service.ISysSettingService>().Update(sysEntity);
            //SysDBTool.Commit();
            return RedirectToAction("Exchange");
        }

        public ActionResult USDPutOnCommand(int id, string commandtype)
        {
            ActMessage = "置顶EP交易";
            var model = USDPutOnService.Single(id);
            if (commandtype.ToLower() == "ontop")
                model.IsTop = true;
            else if (commandtype.ToLower() == "untop")
                model.IsTop = false;
            USDPutOnService.Update(model);
            SysDBTool.Commit();
            return RedirectToAction("PutOn", "USD");
        }
        /// <summary>
        /// 市场成交明细
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public ActionResult Purchase(int? page)
        {
            ActMessage = "已卖出交易明细";

            //TODO: 此处最好别要 ToList() ，因为一调用 ToList() ，查询语句就会传到数据库中将所有数据一并查出。此处的 ToList() 可以省略。
            var list = USDPurchaseService.List().WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).OrderByDescending(x => x.ID).ToList();
            string status = Request["status"];
            if (!string.IsNullOrEmpty(status))
            {
                list = list.Where(x => x.Status == status.ToInt()).ToList();
            }

            ViewBag.TotalMoney = list.Count() > 0 ? list.Sum(x => x.BuyMoney) : 0;
            if (Request["IsExport"] == "1")
            {
                string FileName = string.Format("{0}_{1}_{2}_{3}", DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute);
                MvcCore.Extensions.ExcelHelperV2.ToExcel(list.ToList()).SaveToExcel(Server.MapPath("/upfile/" + FileName + ".xls"));
                return File(Server.MapPath("/upfile/" + FileName + ".xls"), "application/ms-excel", FileName + ".xls");
            }
            return View(list.ToPagedList(page ?? 1, 20));
        }

        /// <summary>
        /// 市场成交明细
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public ActionResult Seek(int? page)
        {
            ActMessage = "EP求购信息";
            //TODO: 此处最好别要 ToList() ，因为一调用 ToList() ，查询语句就会传到数据库中将所有数据一并查出。此处的 ToList() 可以省略。
            var list = USDSeekService.List().WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).OrderByDescending(x => x.ID).ToList();
            string status = Request["status"];
            if (!string.IsNullOrEmpty(status))
            {
                list = list.Where(x => x.Status == status.ToInt()).ToList();
            }

            ViewBag.TotalMoney = list.Count() > 0 ? list.Sum(x => x.SeekMoney) : 0;
            if (Request["IsExport"] == "1")
            {
                string FileName = string.Format("{0}_{1}_{2}_{3}", DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute);
                MvcCore.Extensions.ExcelHelperV2.ToExcel(list.ToList()).SaveToExcel(Server.MapPath("/upfile/" + FileName + ".xls"));
                return File(Server.MapPath("/upfile/" + FileName + ".xls"), "application/ms-excel", FileName + ".xls");
            }
            return View(list.ToPagedList(page ?? 1, 20));
        }

        /// <summary>
        /// 市场挂单销售表
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public ActionResult PutOn(int? page)
        {
            ActMessage = "已卖出EP交易明细";
            //TODO: 此处最好别要 ToList() ，因为一调用 ToList() ，查询语句就会传到数据库中将所有数据一并查出。此处的 ToList() 可以省略。
            var list = USDPutOnService.List().WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).OrderByDescending(x => x.ID).ToList();
            string status = Request["status"];
            if (!string.IsNullOrEmpty(status))
            {
                list = list.Where(x => x.Status == status.ToInt()).ToList();
            }
            if (Request["IsExport"] == "1")
            {
                string FileName = string.Format("{0}_{1}_{2}_{3}", DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute);
                MvcCore.Extensions.ExcelHelperV2.ToExcel(list.ToList()).SaveToExcel(Server.MapPath("/upfile/" + FileName + ".xls"));
                return File(Server.MapPath("/upfile/" + FileName + ".xls"), "application/ms-excel", FileName + ".xls");
            }
            return View(list.ToPagedList(page ?? 1, 20));
        }

        /// <summary>
        /// 确认收款
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult doFinishBuy(int id)
        {
            ActMessage = "确认EP交易";
            var model = USDPurchaseService.Single(id);
            if (model != null)
            {
                if (model.Status == 2 || model.Status == -2)
                {

                    decimal ChangeMoney = model.BuyAmount;
                    string description = "购买交易入帐";
                    var onUser = UserService.Single(model.UID);
                    if (onUser != null)
                    {
                        using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
                        {
                            Wallets.changeWallet(onUser.ID, model.BuyAmount, 2002, description);
                            if ((model.ReserveDecamal ?? 0) > 0) Wallets.changeWallet(onUser.ID, (model.ReserveDecamal ?? 0), 2001, "求购等候利息");

                            var newSeek = USDSeekService.Single(model.SeekID ?? 0);
                            if (newSeek != null)
                            {
                                if (newSeek.SeekMoney == (newSeek.HaveDeal ?? 0))
                                {
                                    newSeek.Status = (int)Data.Enum.USDStatus.Deal;
                                    USDSeekService.Update(newSeek);
                                }
                            }


                            //非首次交易时
                            if (USDPurchaseService.List(x => x.UID == onUser.ID && x.Status == (int)Data.Enum.USDStatus.Deal).Count() > 0)
                            {
                                //用户升级
                                //Users.UpdateLevel(onUser.ID);

                                //更新整线会员的对碰余量（双轨时）
                                decimal addmoney = model.BuyAmount;
                                if (!string.IsNullOrEmpty(onUser.ParentPath))
                                {
                                    string[] ids_dp = onUser.ParentPath.Split(',');
                                    //TODO: 此处 List() 可换成 ListWithTacking() 函数,在 foreach 里面就可以直接使用 dpUser 来操作数据，而不用另外 Single
                                    //TODO: 里面零碎读取的操作应集中一起取出来，尽量别零散调用，这样会增加不必要的数据负担，造成数据库访问性能下降
                                    var lst_DPUser = MvcCore.Unity.Get<IUserService>().List(x => ids_dp.Contains(x.ID.ToString())).OrderBy(x => x.Depth).ThenBy(x => x.ChildPlace).ToList();
                                    foreach (var dpUser in lst_DPUser)
                                    {
                                        Data.User updateUser = MvcCore.Unity.Get<IUserService>().Single(dpUser.ID);
                                        if (onUser.Depth - dpUser.Depth == 1)
                                        {
                                            if (onUser.ChildPlace == 1)
                                            {
                                                updateUser.LeftDpMargin = (updateUser.LeftDpMargin ?? 0) + addmoney;
                                                updateUser.LeftAchievement = (updateUser.LeftAchievement ?? 0) + addmoney;
                                            }
                                            else
                                            {
                                                updateUser.RightDpMargin = (updateUser.RightDpMargin ?? 0) + addmoney;
                                                updateUser.RightAchievement = (updateUser.RightAchievement ?? 0) + addmoney;
                                            }
                                        }
                                        else
                                        {
                                            //左区安置点
                                            var leftchild = MvcCore.Unity.Get<IUserService>().Single(x => x.ParentID == dpUser.ID && x.ChildPlace == 1);
                                            //如果出现在左区安置点
                                            if (leftchild != null && (onUser.ParentPath + ",").Contains("," + leftchild.ID + ","))
                                            {
                                                updateUser.LeftDpMargin = (updateUser.LeftDpMargin ?? 0) + addmoney;
                                                updateUser.LeftAchievement = (updateUser.LeftAchievement ?? 0) + addmoney;
                                            }
                                            else
                                            {
                                                updateUser.RightDpMargin = (updateUser.RightDpMargin ?? 0) + addmoney;
                                                updateUser.RightAchievement = (updateUser.RightAchievement ?? 0) + addmoney;
                                            }
                                        }
                                        MvcCore.Unity.Get<IUserService>().Update(updateUser);
                                    }
                                    MvcCore.Unity.Get<ISysDBTool>().Commit();//TODO: 此处 Commit() 可以省略
                                }
                                //Bonus.Bonus1104(onUser);
                            }
                            model.Status = (int)Data.Enum.PurchaseStatus.Deal;
                            model.ReserveDate = DateTime.Now;
                            USDPurchaseService.Update(model);

                            var putOn = USDPutOnService.Single(model.PutOnID);
                            putOn.Status = (int)Data.Enum.USDStatus.Deal;
                            USDPutOnService.Update(putOn);
                            SysDBTool.Commit();

                            ts.Complete();
                        }
                        if (Request.UrlReferrer != null)
                        {
                            ViewBag.FormUrl = Request.UrlReferrer.ToString();
                        }
                        ViewBag.SuccessMsg = "交易确认成功！";
                        return View("Success");
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
        /// 取消求购
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult CancelSeek(int id)
        {
            ActMessage = "取消求购";
            var model = USDSeekService.Single(id);
            if (model != null)
            {
                var mchs = MvcCore.Unity.Get<IUSDPurchaseService>().List(x => x.SeekID == model.ID && x.Status != -1);
                if (model.Status != 3 || mchs.Count() == 0)
                {
                    using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
                    {
                        model.Status = (int)Data.Enum.USDStatus.Cancel;
                        USDSeekService.Update(model);
                        SysDBTool.Commit();
                        ts.Complete();
                    }
                    if (Request.UrlReferrer != null)
                    {
                        ViewBag.FormUrl = Request.UrlReferrer.ToString();
                    }
                    ViewBag.SuccessMsg = "成功取消交易，您的交易状态将重新挂单！";
                    return View("Success");
                }
                else
                {
                    ViewBag.ErrorMsg = "当前交易状态无法取消。";
                    return View("Error");
                }
            }
            ViewBag.ErrorMsg = "记录不存在或已被删除！";
            return View("Error");
        }

        public ActionResult MultiCancelPuton(string ids)
        { 
            string[] mids = ids.TrimEnd(',').TrimStart(',').Split(',');
            foreach (string id in mids)
            {
                int pid = id.ToInt();
                if (pid > 0)
                {
                    var model = USDPutOnService.Single(pid);
                    if (model != null && (model.Status == 1 || model.Status == 2) && USDPurchaseService.List(x => x.PutOnID == model.ID && x.Status != -1).Count() == 0)
                    {
                        Wallets.changeWallet(model.UID, model.PutAmount, 2002, "取消交易");
                        model.Status = -1;
                        USDPutOnService.Update(model);
                        SysDBTool.Commit();
                    }
                }
            }
            return Content("ok");
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
                if ((model.Status == 1 || model.Status == 2) && USDPurchaseService.List(x => x.PutOnID == model.ID && x.Status != -1).Count() == 0)
                {
                    Wallets.changeWallet(model.UID, model.PutAmount, 2002, "取消交易");
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
            ActMessage = "取消EP交易";
            var model = USDPurchaseService.Single(id);
            if (model != null)
            {
                if (model.Status < 3)
                {
                    using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
                    {
 
                        var newPuton = USDPutOnService.Single(model.PutOnID);
                        if (newPuton != null)
                        {
                            Wallets.changeWallet(newPuton.UID, newPuton.PutAmount, 2002, "后台取消交易");
                            newPuton.Status = (int)Data.Enum.USDStatus.Cancel;
                            USDPutOnService.Update(newPuton);
                        }

                        var chSeek = USDSeekService.Single(model.SeekID ?? 0);
                        if (chSeek != null)
                        {
                            //newSeek.Status = (int)Data.Enum.USDStatus.Cancel;
                            var mchs = MvcCore.Unity.Get<IUSDPurchaseService>().List(x => x.SeekID == chSeek.ID && x.Status != -1);
                            decimal _matchmoney = mchs.Count() > 0 ? mchs.Sum(x => x.BuyMoney) : 0;
                            chSeek.HaveDeal = Math.Max(0, Math.Min(chSeek.SeekMoney, (_matchmoney - model.BuyMoney)));
                            USDSeekService.Update(chSeek);
                        }

                        model.Status = -1;
                        USDPurchaseService.Update(model);
                        SysDBTool.Commit();
                        ts.Complete();
                    }
                    if (Request.UrlReferrer != null)
                    {
                        ViewBag.FormUrl = Request.UrlReferrer.ToString();
                    }
                    ViewBag.SuccessMsg = "成功取消交易，您的交易状态将重新挂单！";
                    return View("Success");
                }
                else
                {
                    ViewBag.ErrorMsg = "当前交易状态无法取消。";
                    return View("Error");
                }
            }
            ViewBag.ErrorMsg = "记录不存在或已被删除！";
            return View("Error");
        }

        public ActionResult _PutOn()
        {
            return View();
        }

    }
}

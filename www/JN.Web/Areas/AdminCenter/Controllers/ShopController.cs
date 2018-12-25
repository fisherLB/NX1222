using JN.Data;
using JN.Data.Service;
using MvcCore.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using JN.Services.Tool;
using System.IO;
using JN.Services.Manager;
using System.Data.Entity.Validation;

namespace JN.Web.Areas.AdminCenter.Controllers
{
    [ValidateInput(false)]
    public class ShopController : BaseController
    {
        private readonly IUserService UserService;
        private readonly IShopInfoService ShopInfoService;
        private readonly IShopOrderService ShopOrderService;
        private readonly IShopOrderDetailService ShopOrderDetailService;
        private readonly IShopProductService ShopProductService;
        private readonly ISysDBTool SysDBTool;
        private readonly IActLogService ActLogService;
       

        public ShopController(ISysDBTool SysDBTool,
            IUserService UserService,
            IShopInfoService ShopInfoService,
            IShopOrderService ShopOrderService,
            IShopOrderDetailService ShopOrderDetailService,
            IShopProductService ShopProductService,
            IActLogService ActLogService)
        {
            this.UserService = UserService;
            this.ShopInfoService = ShopInfoService;
            this.ShopOrderService = ShopOrderService;
            this.ShopOrderDetailService = ShopOrderDetailService;
            this.ShopProductService = ShopProductService;
            this.SysDBTool = SysDBTool;
            this.ActLogService = ActLogService;
            
        }

        public ActionResult ModifyShop(int? id)
        {
            ActMessage = "编辑店铺";
            if (id.HasValue)
                return View(ShopInfoService.Single(id));
            else
            {
                ActMessage = "添加店铺";
                return View(new Data.User());
            }
        }

        [HttpPost]
        public ActionResult ModifyShop(FormCollection fc)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                var entity = ShopInfoService.SingleAndInit(fc["ID"].ToInt());
                TryUpdateModel(entity, fc.AllKeys);

                if (entity.ID > 0)
                    ShopInfoService.Update(entity);
                else
                {
                    entity.IsActivation = false;
                    entity.CreateTime = DateTime.Now;
                    ShopInfoService.Add(entity);
                }
                SysDBTool.Commit();
                result.Status = 200;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }
            return Json(result);
        }

        public ActionResult ShopList(int? active, int? page)
        {
            ActMessage = "店铺列表";
            int theActive = active ?? 0;
            var list = ShopInfoService.List(x => x.IsActivation == theActive.ToBool()).WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).ToList();
            if (Request["IsExport"] == "1")
            {
                string FileName = string.Format("{0}_{1}_{2}_{3}", DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute);
                MvcCore.Extensions.ExcelHelperV2.ToExcel(list.ToList()).SaveToExcel(Server.MapPath("/upfile/" + FileName + ".xls"));
                return File(Server.MapPath("/upfile/" + FileName + ".xls"), "application/ms-excel", FileName + ".xls");
            }
            return View(list.ToPagedList(page ?? 1, 20));
        }

        public ActionResult ShopStatistics(int? page)
        {
            ActMessage = "店铺销售统计";
            var list = ShopInfoService.List(x => x.IsActivation).WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).ToList();
            return View(list.ToPagedList(page ?? 1, 20));
        }

        public ActionResult ShopLock(int? islock, int? page)
        {
            int theLock = islock ?? 0;
            var list = ShopInfoService.List(x => x.IsLock == theLock.ToBool()).WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).ToList();
            if (Request["IsExport"] == "1")
            {
                string FileName = string.Format("{0}_{1}_{2}_{3}", DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute);
                MvcCore.Extensions.ExcelHelperV2.ToExcel(list.ToList()).SaveToExcel(Server.MapPath("/upfile/" + FileName + ".xls"));
                return File(Server.MapPath("/upfile/" + FileName + ".xls"), "application/ms-excel", FileName + ".xls");
            }
            return View(list.ToPagedList(page ?? 1, 20));
        }

        //public ActionResult ShopExport(string df, string dr, string kf, string kw, string kv)
        //{
        //    List<TUser> lst = users.GetModelList("IsAgent=1" + SqlWhere(df, dr, kf, kw, kv));
        //    return new ExcelResult<TUser>(lst, new string[] { "ID", "UserName", "AgentName", "RealName", "Mobile", "AgentLevel", "AddupBonus6", "ApplyAgentTime" });
        //}

        public ActionResult ShopCommand(int id, string commandtype)
        {
            Data.ShopInfo model = ShopInfoService.Single(id);
            if (commandtype.ToLower() == "dolock")
                model.IsLock = true;
            else if (commandtype.ToLower() == "dounlock")
            {
                model.IsLock = false;
            }
            else if (commandtype.ToLower() == "dopass")
            {
                model.IsActivation = true;
                //users.Update("IsShop=1,ShopLevel=" + shopinfos.GetModel(id).ShopLevel, "ID=" + shopinfos.GetModel(id).UID);
            }
            ShopInfoService.Update(model);
            SysDBTool.Commit();
            return RedirectToAction("ShopList", "Shop");
        }

        public ActionResult Modify(int? id)
        {
            ActMessage = "编辑商品";
            if (id.HasValue)
                return View(ShopProductService.Single(id));
            else
            {
                ActMessage = "发布商品";
                return View(new Data.ShopProduct());
            }
        }

        [HttpPost]
        public ActionResult Modify(FormCollection fc)
        {
            try
            {
                var entity = ShopProductService.SingleAndInit(fc["ID"].ToInt());
                TryUpdateModel(entity, fc.AllKeys);
                //if (ClassId.Trim().Length == 0)
                //    strErr += "请选择商品分类 <br />";

                HttpPostedFileBase file = Request.Files["imgurl"];
                string imgurl = "";
                if (!string.IsNullOrEmpty(file.FileName))
                {
                    if (!FileValidation.IsAllowedExtension(file, new FileExtension[] { FileExtension.PNG, FileExtension.JPG, FileExtension.BMP }))
                    {
                        ViewBag.ErrorMsg = "非法上传，您只可以上传图片格式的文件！";
                        return View("Error");
                    }
                    var newfilename = Guid.NewGuid() + Path.GetExtension(file.FileName).ToLower();
                    var fileName = Path.Combine(Request.MapPath("~/upfile"), newfilename);
                    try
                    {
                        file.SaveAs(fileName);
                        imgurl = "/upfile/" + newfilename;
                    }
                    catch
                    {
                        //
                    }
                }
                entity.ImageUrl = imgurl;
                if (entity.ID > 0)
                    ShopProductService.Update(entity);
                else
                {
                    entity.IsSales = true;
                    entity.SaleCount = 0;
                    entity.Hits = 0;
                    entity.CreateTime = DateTime.Now;
                    ShopProductService.Add(entity);
                }
                SysDBTool.Commit();
                ViewBag.SuccessMsg = "商品修改/发布成功！";
                return View("Success");
            }
            catch (DbEntityValidationException ex)
            {
                //foreach (var item in ex.EntityValidationErrors)
                //{
                //    foreach (var item2 in item.ValidationErrors)
                //        error += string.Format("{0}:{1}\r\n", item2.PropertyName, item2.ErrorMessage);
                //}
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
                ViewBag.ErrorMsg = "系统异常！请查看系统日志。";
                return View("Error");
            }
        }

        public ActionResult Product(int? page)
        {
            ActMessage = "商品管理";
            var list = ShopProductService.List(x => x.IsSales).WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).ToList();
            if (Request["IsExport"] == "1")
            {
                string FileName = string.Format("{0}_{1}_{2}_{3}", DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute);
                MvcCore.Extensions.ExcelHelperV2.ToExcel(list.ToList()).SaveToExcel(Server.MapPath("/upfile/" + FileName + ".xls"));
                return File(Server.MapPath("/upfile/" + FileName + ".xls"), "application/ms-excel", FileName + ".xls");
            }
            return View(list.ToPagedList(page ?? 1, 20));
        }

        public ActionResult OffSales(int? page)
        {
            ActMessage = "下架商品管理";
            var list = ShopProductService.List(x => x.IsSales == false).WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).ToList();
            return View("Product", list.ToPagedList(page ?? 1, 20));
        }

        public ActionResult ProductCommand(int id, string commandtype)
        {
            Data.ShopProduct model = ShopProductService.Single(id);

            if (commandtype.ToLower() == "elite")
                model.IsElite = true;
            else if (commandtype.ToLower() == "unelite")
                model.IsElite = false;
            else if (commandtype.ToLower() == "ontop")
                model.IsTop = true;
            else if (commandtype.ToLower() == "untop")
                model.IsTop = false;
            else if (commandtype.ToLower() == "onsales")
                model.IsSales = true;
            else if (commandtype.ToLower() == "offsales")
                model.IsSales = false;
            ShopProductService.Update(model);
            SysDBTool.Commit();
            return RedirectToAction("Product", "Shop");
        }

        public ActionResult Delete(int id)
        {
            var model = ShopProductService.Single(id);
            if (model != null)
            {
                ActPacket = model;
                ShopProductService.Delete(id);
                SysDBTool.Commit();
                ViewBag.SuccessMsg = "“" + model.ProductName + "”已被删除！";
                return View("Success");
            }
            ViewBag.ErrorMsg = "记录不存在或已被删除！";
            return View("Error");
        }
        public ActionResult Order(int? page, int? status)
        {
            ActMessage = "订单管理";
            var list = ShopOrderService.List(x => x.Status == (status ?? 1)).WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).ToList();
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
        public ActionResult doFinishOrder(int id)
        {
            var model = ShopOrderService.Single(id);
            if (model != null)
            {
                if (model.Status == (int)Data.Enum.OrderStatus.Transaction)
                {

                    //TShopInfo shop = shopinfos.GetModel(model.ShopID);
                    //TUser Smodel = users.GetModel(shop.UID);
                    using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
                    {
                        model.Status = (int)JN.Data.Enum.OrderStatus.Deal;
                        ShopOrderService.Update(model);
                        SysDBTool.Commit();
                        //给卖家打款
                        //users.changeWallet(Smodel, (float)model.TotalPrice, 2006, "商品销售收入");
                        //执行返利
                        //users.CalculateMallBonus((int)model.ID);
                        ts.Complete();
                    }
                    ActPacket = model;
                    if (Request.UrlReferrer != null)
                    {
                        ViewBag.FormUrl = Request.UrlReferrer.ToString();
                    }
                    ViewBag.SuccessMsg = "确认收货成功！";
                    return View("Success");
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
        public ActionResult CancelOrder(int id)
        {
            var model = ShopOrderService.Single(id);
            if (model != null)
            {
                if (model.Status == (int)Data.Enum.OrderStatus.Sales)
                {
                    using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
                    {
                        var model2 = ShopOrderDetailService.Single(x => x.OrderNumber == model.OrderNumber);
                        if (model2 != null)
                        {
                            var product = ShopProductService.Single(model2.ProductID);
                            product.SaleCount = product.SaleCount - model2.ByCount;
                            product.Stock = product.Stock + model2.ByCount;
                            ShopProductService.Update(product);
                        }
                        model.Status = (int)Data.Enum.OrderStatus.Cancel;
                        ShopOrderService.Update(model);
                        SysDBTool.Commit();

                        Wallets.changeWallet(model.UID, model.TotalPrice, 2005, "商城退款");
                        ts.Complete();
                    }
                    //退款

                    ActPacket = model;
                    if (Request.UrlReferrer != null)
                    {
                        ViewBag.FormUrl = Request.UrlReferrer.ToString();
                    }
                    ViewBag.SuccessMsg = "成功取消订单！";
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

        public ActionResult doDeliver(int id, string logistics)
        {
            var model = ShopOrderService.Single(id);
            if (model != null)
            {
                model.Logistics = logistics;
                model.Status = (int)Data.Enum.OrderStatus.Transaction;
                ShopOrderService.Update(model);
                SysDBTool.Commit();
                ActMessage = "订单发货成功！";
                return Content("ok");
            }
            return Content("Error");
        }
    }
}

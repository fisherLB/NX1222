using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using JN.Data;
using JN.Data.Service;
using MvcCore.Controls;
using PagedList;
using JN.Services.Manager;

namespace JN.Web.Areas.UserCenter.Controllers
{
    public class ShopController : BaseController
    {
        object obj = new object();
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

        public ActionResult Index()
        {
            ActMessage = "商城首页";
            return View();
        }

        public ActionResult Product(int? page, int? c)
        {
            ActMessage = "所有商品";
            var list = new List<ShopProduct>();
            if (c == null)
            {
                list = ShopProductService.List(x => x.IsSales).WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).OrderByDescending(x => x.ID).ToList();
            }
            else
            {
                list = ShopProductService.List(x => x.IsSales && x.ClassId == c).WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).OrderByDescending(x => x.ID).ToList();
            }

            return View(list.ToPagedList(page ?? 1, 20));
        }

        public ActionResult ShopList(int? page)
        {
            ActMessage = "营业中的店铺";
            var list = ShopInfoService.List(x => x.IsActivation && x.IsLock == false).OrderByDescending(x => x.ID).ToList();
            return View(list.ToPagedList(page ?? 1, 20));
        }

        public ActionResult Detail(int id)
        {
            ActMessage = "商品详情";
            var entity = ShopProductService.Single(id);
            if (entity != null)
                return View(entity);
            else
            {
                ViewBag.ErrorMsg = "记录不存在或已被删除";
                return View("Error");
            }
        }

        public ActionResult ShopDetail(int id)
        {
            ActMessage = "店铺详情";
            var entity = ShopInfoService.Single(id);
            if (entity != null)
                return View(entity);
            else
            {
                ViewBag.ErrorMsg = "记录不存在或已被删除";
                return View("Error");
            }
        }


        [HttpPost]
        public ActionResult Buy(FormCollection form)
        {
            string buynumber = form["txtCount"];
            string goodsid = form["goodsId"];
            var product = ShopProductService.Single(goodsid.ToInt());
            string strErr = "";
            if (buynumber.ToInt() <= 0)
                strErr += "交易数量正确 <br />";
            if (buynumber.ToInt() > product.Stock)
                strErr += "商品库存不足 <br />";
            if (strErr != "")
            {
                ViewBag.ErrorMsg = "抱赚您填写的信息有误： <br />" + strErr + "请核实后重新提交！";
                return View("Error");
            }

            ViewBag.Title = "确认订单并支付";
            ViewBag.Path = "在线商城";
     
            if (product != null)
            {
                ViewBag.BuyNumber = buynumber.ToInt();
                ViewBag.TotalPrice = ViewBag.BuyNumber * product.RealPrice;
                ViewBag.Fhsm = "";// shopinfos.GetModel(product.ShopID).ReserveStr1;

                //有过购买记录
                if (ShopOrderService.List(x => x.UID == Umodel.ID).Count() > 0)
                {
                    var lastOrder = ShopOrderService.List(x => x.UID == Umodel.ID).OrderByDescending(x => x.ID).FirstOrDefault();
                    if (lastOrder != null)
                    {
                        ViewBag.Province = lastOrder.Province;
                        ViewBag.City = lastOrder.City;
                        ViewBag.County = lastOrder.Town;
                        ViewBag.RecAddress = lastOrder.RecAddress;
                        ViewBag.RecLinkMan = lastOrder.RecLinkMan;
                        ViewBag.RecPhone = lastOrder.RecPhone;
                    }
                }

                return View(product);
            }
            else
            {
                ViewBag.ErrorMsg = "记录不存在或已被删除";
                return View("Error");
            }
        }

        /// <summary>
        /// 我卖出的交易
        /// </summary>
        /// <param name="df"></param>
        /// <param name="dr"></param>
        /// <param name="kf"></param>
        /// <param name="kv"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public ActionResult Order(int? page)
        {
            ActMessage = "我的订单";
            var list = ShopOrderService.List(x => x.UID == Umodel.ID).WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).OrderByDescending(x => x.ID).ToList();
            return View(list.ToPagedList(page ?? 1, 20));
        }
        

        [HttpPost]
        public ActionResult Pay(FormCollection form)
        {
            string GoodsId = form["GoodsId"];
            string BuyNumber = form["BuyNumber"];
            string province = form["hidprovince"];
            string city = form["hidcity"];
            string county = form["hidcounty"];
            string RecAddress = form["RecAddress"];
            string RecLinkMan = form["RecLinkMan"];
            string RecPhone = form["RecPhone"];
            int CoinID = form["coinid"].ToInt();
            var product = ShopProductService.Single(GoodsId.ToInt());
            string strErr = "";
            decimal acceptWallet = 0;

            if (BuyNumber.ToInt() <= 0)
                strErr += "购买数量正确 <br />";

            if (String.IsNullOrEmpty(RecAddress))
                strErr += "收货地址不能为空 <br />";

            if (String.IsNullOrEmpty(RecPhone))
                strErr += "联系电话不能为空 <br />";

            if (BuyNumber.ToInt() > product.Stock)
                strErr += "商品库存不足 <br />";

            if (CoinID==2001)
                acceptWallet = Umodel.Wallet2001;
            else
                acceptWallet = Umodel.Wallet2002;

            if (strErr != "")
            {
                ViewBag.ErrorMsg = "抱赚您填写的信息有误： <br />" + strErr + "请核实后重新提交！";
                return View("Error");
            }
      
            if (product != null)
            {
                if (acceptWallet >= product.RealPrice * BuyNumber.ToInt())
                {
                    using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
                    {
                        lock (obj)
                        {
                            string orderNumber = GetOrderNumber();//创建临时单号
                            var model = new Data.ShopOrderDetail();
                            model.ByCount = BuyNumber.ToInt();
                            model.CreateTime = DateTime.Now;
                            model.OrderNumber = orderNumber;
                            model.ProducName = product.ProductName;
                            model.ProductID = product.ID;
                            model.Remark = "";
                            model.Spec = product.Spec;
                            model.TotaPrice = product.RealPrice * BuyNumber.ToInt();
                            model.Unit = product.Unit;
                            model.UnitPrice = product.RealPrice;
                            ShopOrderDetailService.Add(model);

                            var order = new Data.ShopOrder();
                            order.BuyMsg = "";
                            order.CreateTime = DateTime.Now;
                            order.City = city;
                            order.OrderNumber = orderNumber;
                            order.Province = province;
                            order.RecAddress = RecAddress;
                            order.RecLinkMan = RecLinkMan;
                            order.RecPhone = RecPhone;
                            order.ShopID = product.ShopID;
                            order.ShopName = product.ShopName;
                            order.Status = (int)Data.Enum.OrderStatus.Sales;
                            order.ShipFreight = product.FreightPrice;
                            order.TotalCount = model.ByCount;
                            order.TotalPrice = (model.TotaPrice + (product.FreightPrice ?? 0));
                            order.Town = county;
                            order.UID = Umodel.ID;
                            order.UserName = Umodel.UserName;
                            ShopOrderService.Add(order);

                            product.SaleCount = product.SaleCount + model.ByCount;
                            product.Stock = product.Stock - model.ByCount;
                            ShopProductService.Update(product);
                            SysDBTool.Commit();

                            Wallets.changeWallet(Umodel.ID, 0 - order.TotalPrice, CoinID, "商城消费");
                            ts.Complete();

                            ViewBag.mFormUrl = "/UserCenter/Shop/Order";
                            ViewBag.SuccessMsg = "交易订单“" + orderNumber + "”支付成功！";
                            ActMessage = ViewBag.SuccessMsg;
                            return View("Success");
                        }
                    }
                }
                else
                {
                    ViewBag.ErrorMsg = "您的帐号余额不足。";
                    return View("Error");
                }
            }
            else
            {
                ViewBag.ErrorMsg = "记录不存在或已被删除";
                return View("Error");
            }
        }

        //生成真实订单号
        public string GetOrderNumber()
        {
            DateTime dateTime = DateTime.Now;
            string result = "M";
            result += dateTime.Year.ToString().Substring(2, 2) + dateTime.Month + dateTime.Day;//年月日
            int hour = dateTime.Hour;
            int minu = dateTime.Minute;
            int secon = dateTime.Second;
            int count = hour * 20 + minu * 10 + secon * 5;
            int lengh = count.ToString().Length;
            string temp = count.ToString();
            string bu = "";
            if (lengh < 5)//不足5位前面补0
            {
                for (int i = 1; i < 5 - lengh; i++)
                {
                    bu += "0";
                }
            }
            result += bu + temp;
            result += Services.Tool.StringHelp.GetRandomNumber(4);//4位随机数字
            if (IsHave(result))
            {
                return GetOrderNumber();
            }
            return result;
        }

        //检查订单号是否重复
        private bool IsHave(string number)
        {
            return ShopOrderService.List(x => x.OrderNumber == number).Count() > 0;
        }

        /// <summary>
        /// 确认收款
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult FinishBuy(int id)
        {
            var model = ShopOrderService.Single(id);
            if (model != null)
            {
                if (model.Status == (int)Data.Enum.OrderStatus.Transaction)
                {
                    if (model.UID == Umodel.ID)
                    {
                        //TShopInfo shop = shopinfos.GetModel(model.ShopID);
                        //TUser Smodel = users.GetModel(shop.UID);
                        //users.doDelivery(onUser, (float)model.BuyAmount, 2002, description);
                        using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
                        {
                            lock (obj)
                            {
                                model.Status = (int)JN.Data.Enum.OrderStatus.Deal;
                                ShopOrderService.Update(model);
                                SysDBTool.Commit();
                                //给卖家打款
                                //users.doDelivery(Smodel, (float)model.TotalPrice, 2002, "商品销售收入");
                                //执行返利
                                //users.CalculateMallBonus((int)model.ID);
                                ts.Complete();
                            }
                        }

                        ActPacket = model;

                        if (Request.UrlReferrer != null)
                        {
                            ViewBag.FormUrl = Request.UrlReferrer.ToString();
                        }
                        ViewBag.SuccessMsg = "交易收货成功！";
                        ActMessage = ViewBag.SuccessMsg;
                        return View("Success");
                    }
                }
                else
                {
                    ViewBag.ErrorMsg = "当前交易状态无法进行确认。";
                    return View("Error");
                }
            }
            ViewBag.ErrorMsg = "记录不存在或已被删除！";
            return View("Error");
        }
    }
}

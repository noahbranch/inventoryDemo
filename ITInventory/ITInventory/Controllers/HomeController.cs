using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using ITInventory.Models;
using System.Web.Mvc;
using Newtonsoft.Json;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using Newtonsoft.Json.Linq;
using HDUtilities;
using System.Net.Mail;

namespace ITInventory.Controllers
{
    public class HomeController : HDController
    {
        ITInventoryEntities dbITInventory = new ITInventoryEntities();
        public ActionResult Index()
        {
            ViewBag.VendorEmails = new SelectList(dbITInventory.Vendors.Where(x => x.Email != null), "Email", "Email");
            var orderList = dbITInventory.OrderProducts.Where(x => x.Order.Submitted == false).ToList();

            ViewBag.categories = dbITInventory.Categories.ToList();

            ViewBag.inline = GetInlineData();

            

            return View(orderList);
        }

        private IEnumerable<Category> GetInlineData()
        {
            List<Category> inline = new List<Category>();

            var categoryList = dbITInventory.Categories.ToList();
            var productList = dbITInventory.Products.ToList();

            inline = categoryList.Select(
                x => new Category()
                {
                    CategoryName = x.CategoryName,
                    Id = x.Id,
                    LastEditDateTime = x.LastEditDateTime,
                    LastEditUser = x.LastEditUser,
                    Products = productList.Where(r => r.CategoryId == x.Id && r.Active == true).ToList()
                }).ToList();

            return inline;
        }

        public ActionResult Cart()
        {
            var orderList = dbITInventory.OrderProducts.Where(x => x.Order.Submitted == false).ToList();

            return View(orderList);
        }

        [HttpGet]
        public ActionResult Products_Read([DataSourceRequest]DataSourceRequest request)
        {
            var productlist = dbITInventory.Products.ToList();
            var orderList = dbITInventory.OrderProducts.ToList();
            var vendor = new List<Vendor>();
            var categoryList = dbITInventory.Categories.ToList();
            var lot = new List<Lot>();
            var lotList = dbITInventory.Lots.ToList();
            var kits = dbITInventory.KitProducts.ToList();
            var user = HDUtilities.UserInformation.GetCurrentUserName();


            dbITInventory.Configuration.ProxyCreationEnabled = false;

            List<AllocationViewModel> productmodel = new List<AllocationViewModel>();

            productmodel = productlist.Where(x => x.Active == true).OrderByDescending(x => x.Active).ThenBy(x => x.ProductName).ToList().Select(
                x => new AllocationViewModel()
                {
                    Id = x.Id,
                    ProductId = x.Id,
                    ProductName = x.ProductName,
                    StockLevel = (x.Lots.Where(r => r.ProductId == x.Id).Select(m => m.Quantity).Sum() > 0 ? x.Lots.Where(r => r.ProductId == x.Id).Select(m => m.Quantity).Sum() : 0),
                    OnOrder = orderList.Where(r => r.Order.Submitted == true && r.CheckedIn == false && r.ProductId == x.Id).Sum(r => r.Quantity),
                    Quantity = (orderList.Where(r => r.ProductId == x.Id && r.LastEditUser == user && r.Order.Submitted != true).Select(r => r.Quantity).Sum() > 0 ? orderList.Where(r => r.ProductId == x.Id && r.LastEditUser == user && r.Order.Submitted != true).Select(r => r.Quantity).Sum() : 0),
                    CategoryId = x.CategoryId,
                    CategoryName = categoryList.Where(r => r.Id == x.CategoryId).FirstOrDefault().CategoryName,
                    SellPrice = x.IsKit == null ? lotList.Where(r => r.ProductId == x.Id).OrderBy(m => m.Price).FirstOrDefault().Price : kits.Where(r => r.KitId == x.KitId).Sum(r => r.Product.Lots.OrderBy(m => m.Price).FirstOrDefault().Price)
                }).ToList();

            DataSourceResult result = productmodel.ToDataSourceResult(request);

            return Json(result, JsonRequestBehavior.AllowGet); 

        }

        [HttpGet]
        public ActionResult Create_SubPrices([DataSourceRequest]DataSourceRequest request, int ProductId)
        {
            List<AllocationViewModel> lotModel = new List<AllocationViewModel>();
            var allocationLots = dbITInventory.AllocationLots.ToList();
            var products = dbITInventory.Products.ToList();
            var lots = dbITInventory.Lots.ToList();
            var kitprods = dbITInventory.KitProducts.ToList();

            var product = products.Where(r => r.Id == ProductId).FirstOrDefault();

            if (product.IsKit == null || product.IsKit == false)
            {

                lotModel = lots.Where(r => r.ProductId == ProductId && r.Product.IsKit != true).Select(
                    x => new AllocationViewModel()
                    {
                        LotId = x.Id,
                        Quantity = x.Quantity,
                        SellPrice = x.Price,
                        CategoryName = "Product"
                    }).ToList();
            }
            else if (product.IsKit == true)
            {
                lotModel = kitprods.Where(r => r.KitId == product.KitId).Select(
                    x => new AllocationViewModel()
                    {
                        ProductName = x.Product.ProductName,
                        LotId = x.Product.Lots.OrderBy(r => r.Price).FirstOrDefault().Id,
                        SellPrice = x.Product.Lots.OrderBy(r => r.Price).FirstOrDefault().Price,
                        Quantity = x.Product.Lots.OrderBy(r => r.Price).FirstOrDefault().Quantity,
                        CategoryName = "Kit"
                    }).ToList();
            }

            DataSourceResult result = lotModel.ToDataSourceResult(request);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Get_Order([DataSourceRequest]DataSourceRequest request)
        {
            List<OrdersViewModel> orderModel = new List<OrdersViewModel>();
            var orderProds = dbITInventory.OrderProducts.ToList();

            orderModel = orderProds.Where(r => HDUtilities.UserInformation.GetCurrentUserName().Contains(r.LastEditUser) && r.Order.Submitted != true).Select(
                x => new OrdersViewModel()
                {
                    Id = x.Id,
                    Quantity = x.Quantity,
                    ProductId = x.ProductId,
                    OrderId = x.OrderId,
                    ProductName = x.Product.ProductName
                }).ToList();
            DataSourceResult result = orderModel.ToDataSourceResult(request);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DeleteFromOrder(OrderProduct order, AddtoCart cartRemove)
        {
            bool successful = false;

            try
            {
                order = dbITInventory.OrderProducts.Find(cartRemove.ProductId);
                dbITInventory.OrderProducts.Remove(order);
                dbITInventory.SaveChanges();
                successful = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return Json(new { Result = successful }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult AddToOrder(OrderProduct orderProduct, Order order, AddtoCart productAdd)
        {

            var user = HDUtilities.UserInformation.GetCurrentUserName();
            OrderProduct ordProdCheck = dbITInventory.OrderProducts.Where(r => r.LastEditUser == user && r.Order.Submitted != true && r.ProductId == productAdd.ProductId).FirstOrDefault();
            var productList = dbITInventory.Products.ToList();
            var kitList = dbITInventory.Kits.ToList();
            var kitProductList = dbITInventory.KitProducts.ToList();

            bool successful = false;

            if (ordProdCheck != null)
            {
                if (productAdd.OrderQty == 0)
                {
                    dbITInventory.OrderProducts.Remove(ordProdCheck);
                }
                else if (productList.Where(r => r.Id == productAdd.ProductId).FirstOrDefault().IsKit == true)
                {
                    var kitId = productList.Where(r => r.Id == productAdd.ProductId).FirstOrDefault().KitId;
                    foreach (var kitProd in kitProductList.Where(r => r.KitId == kitId))
                    ordProdCheck.Quantity += kitProd.Quantity;
                }
                else
                {
                    ordProdCheck.Quantity = productAdd.OrderQty;
                }
                successful = true;
                dbITInventory.SaveChanges();
            }
            else if(productList.Where(r => r.Id == productAdd.ProductId).FirstOrDefault().IsKit == true){
                var kitId = productList.Where(r => r.Id == productAdd.ProductId).FirstOrDefault().KitId;

                foreach (var kitProd in kitProductList.Where(r => r.KitId == kitId))
                {
                    var orderproductList = dbITInventory.OrderProducts.ToList();
                    var orderList = dbITInventory.Orders.ToList();
                    var lotList = dbITInventory.Lots.ToList();
                    var orderId = 0;
                    var lastOrder = orderList.Any(r => r.Submitted != true && r.LastEditUser == HDUtilities.UserInformation.GetCurrentUserName());

                    if (lastOrder == false)
                    {
                        order.Submitted = false;
                        order.Checkedin = false;
                        order.LastEditDateTime = DateTime.Now;
                        order.LastEditUser = HDUtilities.UserInformation.GetCurrentUserName();
                        dbITInventory.Orders.Add(order);
                        dbITInventory.SaveChanges();
                        orderId = orderList.Max(r => r.Id) + 1;
                    }
                    else
                    {
                        orderId = orderList.Max(r => r.Id);
                    }

                    orderProduct.ProductId = kitProd.ProductId;
                    orderProduct.Quantity = kitProd.Quantity * productAdd.OrderQty;
                    orderProduct.OrderCost = lotList.Where(r => r.ProductId == kitProd.ProductId).FirstOrDefault().Price * (kitProd.Quantity * productAdd.OrderQty);
                    orderProduct.OrderId = orderId;
                    orderProduct.LastEditDateTime = DateTime.Now;
                    orderProduct.LastEditUser = HDUtilities.UserInformation.GetCurrentUserName();
                    orderProduct.Product = productList.Where(r => r.Id == orderProduct.ProductId).FirstOrDefault();
                    orderProduct.Order = orderList.Where(r => r.Id == orderProduct.OrderId).FirstOrDefault();

                    try
                    {
                        dbITInventory.OrderProducts.Add(orderProduct);
                        dbITInventory.SaveChanges();
                        successful = true;
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
            }
            else
            {

                var orderproductList = dbITInventory.OrderProducts.ToList();
                var orderList = dbITInventory.Orders.ToList();
                var lotList = dbITInventory.Lots.ToList();
                var productsList = dbITInventory.Products.ToList();
                var orderId = 0;
                var lastOrder = orderList.Any(r => r.Submitted != true && r.LastEditUser == HDUtilities.UserInformation.GetCurrentUserName());

                if (lastOrder == false)
                {
                    order.Submitted = false;
                    order.Checkedin = false;
                    order.LastEditDateTime = DateTime.Now.Date;
                    order.LastEditUser = HDUtilities.UserInformation.GetCurrentUserName();
                    dbITInventory.Orders.Add(order);
                    dbITInventory.SaveChanges();
                    orderId = order.Id;
                }
                else
                {
                    orderId = orderList.Max(r => r.Id);
                }

                orderProduct.ProductId = productAdd.ProductId;
                orderProduct.Quantity = productAdd.OrderQty;
                orderProduct.OrderCost = lotList.Where(r => r.ProductId == productAdd.ProductId).FirstOrDefault().Price * productAdd.OrderQty;
                orderProduct.OrderId = orderId;
                orderProduct.LastEditDateTime = DateTime.Now.Date;
                orderProduct.LastEditUser = HDUtilities.UserInformation.GetCurrentUserName();
                orderProduct.Product = productsList.Where(r => r.Id == orderProduct.ProductId).FirstOrDefault();
                orderProduct.Order = orderList.Where(r => r.Id == orderProduct.OrderId).FirstOrDefault();
                try
                {
                    dbITInventory.OrderProducts.Add(orderProduct);
                    dbITInventory.SaveChanges();
                    successful = true;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return Json(new { Result = successful }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ConfirmOrder(ProductSelection products, [Bind(Include="Id,OrderCost,ProductId,QtyOrdered,LastEditUser,LastEditDateTime")] OrderProduct orderProduct, Order orders)
        {
            var inventory = dbITInventory.Products.ToList();
            bool successful = false;
            var orderList = dbITInventory.Orders.ToList();
            var orderProds = dbITInventory.OrderProducts.ToList();
            var lastUserOrder = orderProds.Where(x => x.LastEditUser == HDUtilities.UserInformation.GetCurrentUserName() && x.Order.Submitted != true);
            var lastOrderId = lastUserOrder.FirstOrDefault().OrderId;

            foreach (var order in orderProds.Where(r => r.OrderId == lastOrderId))
            {
                inventory.Where(r => r.Id == order.ProductId).FirstOrDefault().OnOrder += order.Quantity;
            }

            orderList.Where(r => r.Id == lastOrderId).FirstOrDefault().Submitted = true;

            foreach (var order in lastUserOrder)
            {
                order.Order.Submitted = true;
            }

            try
            {
                dbITInventory.SaveChanges();
                successful = true;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return Json(new { Result = successful }, JsonRequestBehavior.AllowGet);
        }
        public void Masquerade(String masqueradeUser)
        {
            if (!HDUtilities.RoleProvider.IsUserInSiteRole("Admin", false))
            {
                HDUtilities.LogError.LogErrorMessage(String.Format(@"Unuathorized access to {0}\{1} by {2}", ControllerName, ActionName, HDUtilities.UserInformation.GetCurrentUserName()));
                return;
            }

            HDUtilities.Masquerade.MasqueradeAs(masqueradeUser);
        }

        public void MasqueradeSwitchBack()
        {
            if (!HDUtilities.RoleProvider.IsUserInSiteRole("Admin", false))
            {
                HDUtilities.LogError.LogErrorMessage(String.Format(@"Unuathorized access to {0}\{1} by {2}", ControllerName, ActionName, HDUtilities.UserInformation.GetCurrentUserName()));
                return;
            }

            HDUtilities.Masquerade.MasqueradeSwitchBack();
        }
    }
}

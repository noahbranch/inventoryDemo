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

            return View(orderList);
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

            var vendor = new List<Vendor>();
            var lot = new List<Lot>();
            var lotList = dbITInventory.Lots.ToList();

            dbITInventory.Configuration.ProxyCreationEnabled = false;

            List<AllocationViewModel> productmodel = new List<AllocationViewModel>();

            productmodel = productlist.Where(x => x.Active == true && x.Lots.Where(r => r.ProductId == x.Id).Sum(r => r.Quantity) > 0).OrderByDescending(x => x.Active).ThenBy(x => x.ProductName).ToList().Select(
                x => new AllocationViewModel()
                {
                    Id = x.Lots.Where(r => r.ProductId == x.Id).FirstOrDefault().Id,
                    ProductId = x.Id,
                    ProductName = x.ProductName,
                    StockLevel = x.Lots.Where(r => r.ProductId == x.Id).Select(m => m.Quantity).Sum(),
                    OnOrder = x.OnOrder,
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

            lotModel = lots.Where(r => r.ProductId == ProductId).Select(
                x => new AllocationViewModel()
                {
                    LotId = x.Id,
                    Quantity = x.ProductId,
                    SellPrice = x.Price
                }).ToList();

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
        public ActionResult DeleteFromOrder(OrderProduct order)
        {
            bool successful = false;

            try
            {
                order = dbITInventory.OrderProducts.Find(order.Id);
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
            bool successful = false;

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
                orderId = orderList.Max(r => r.Id) + 1;
            }
            else
            {
                orderId = orderList.Max(r => r.Id);
            }

            orderProduct.ProductId = productAdd.ProductId;
            orderProduct.Quantity = productAdd.OrderQty;
            orderProduct.OrderCost = lotList.Where(r => r.Id == productAdd.ProductId).FirstOrDefault().Price * productAdd.OrderQty;
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

            //if (products.vendorMessages.Length != 0 && products.vendorMessages[0] != "")
            //{
            //    for (var x = 0; x < products.vendorMessages.Length; x++)
            //    {
            //        MailAddress to = new MailAddress("nbranch@heartland.com"/*products.vendorEmails[x]*/);
            //        MailAddress from = new MailAddress("noreply@heartland.com");
            //        MailMessage mail = new MailMessage(from, to);

            //        mail.IsBodyHtml = false;

            //        mail.Body = products.vendorMessages[x];

            //        SmtpClient smtp = new SmtpClient();
            //        smtp.Send(mail);
            //    }
            //}

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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ITInventory.Models;
using HDUtilities;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;

namespace ITInventory.Controllers
{
    public class ConfirmationController : Controller
    {
        private ITInventoryEntities dbIT = new ITInventoryEntities();
        private OfficeInfoLocalEntities dbOfficeInfo = new OfficeInfoLocalEntities();
        //
        // GET: /Confirmation/
        public ActionResult Order()
        {
            var user = HDUtilities.UserInformation.GetCurrentUserName();

            var ordProds = dbIT.OrderProducts.Where(r => r.LastEditUser == user && r.Order.Submitted != true).ToList();

            return View(ordProds);
        }
        public ActionResult Allocate(DDB_HDC_OFFICEINFO officeInfo)
        {
            var user = HDUtilities.UserInformation.GetCurrentUserName();
            var ordProds = dbIT.OrderProducts.Where(r => r.LastEditUser == user && r.Order.Submitted != true).ToList();
            ViewBag.orderProducts = new SelectList(ordProds.Where(r => r.LastEditUser == user && r.Order.Submitted != true));
            var offices = dbOfficeInfo.DDB_HDC_OFFICEINFO;
            ViewBag.OfficeName = new SelectList(offices.Where(x => x.OfficeName != null).OrderBy(x => x.OfficeName), "RSCDB", "OfficeName");

            return View(officeInfo);
        }

        [HttpGet]
        public ActionResult Get_Order([DataSourceRequest]DataSourceRequest request)
        {
            var user = HDUtilities.UserInformation.GetCurrentUserName();
            List<OrdersViewModel> orderModel = new List<OrdersViewModel>();
            var orders = dbIT.OrderProducts.Where(r => r.LastEditUser == user && r.Order.Submitted != true).ToList();

            orderModel = orders.ToList().Select(
                x => new OrdersViewModel()
                {
                    Id = x.Id,
                    LastEditUser = x.LastEditUser,
                    OrderId = x.OrderId,
                    ProductId = x.ProductId,
                    ProductName = x.Product.ProductName,
                    Quantity = x.Quantity
                }).ToList();

            DataSourceResult result = orderModel.ToDataSourceResult(request);

            return Json(result, JsonRequestBehavior.AllowGet);

        }
	}
}
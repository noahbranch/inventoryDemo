using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using ITInventory.Models;

namespace ITInventory.Controllers
{
    public class OrderHistoryController : Controller
    {

        ITInventoryEntities db = new ITInventoryEntities();
        //
        // GET: /OrderHistory/
        public ActionResult Index()
        {
            var orderHist = db.OrderHistories.OrderBy(x => x.LastEditDateTime).ToList();

            return View(orderHist);
        }
        [HttpPost]
        public ActionResult Read_OrderConfirm(OrderHistoryConfirm OrderHistConfirm, [Bind(Include = "Id, StockLevel")]Product Products, [Bind(Include = "Id,ProductId,QtyOrdered,LastEditUser,LastEditDateTime")] OrderHistory orderHistory)
        {
            var x = OrderHistConfirm.ProductId;
            var orders = db.OrderHistories.ToList();
            var prod = db.Products.ToList();

            foreach (var item in prod)
            {
                if (item.Id == x)
                {
                    item.StockLevel += OrderHistConfirm.QtyConfirmed;
                }
            }
            foreach (var order in orders)
            {
                if (order.Id == OrderHistConfirm.CheckId)
                {
                    order.IsCheckedIn = OrderHistConfirm.CheckedIn;
                }
            }
            db.SaveChanges();

            return View();
        }
	}
}
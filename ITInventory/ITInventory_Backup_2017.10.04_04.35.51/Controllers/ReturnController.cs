using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using ITInventory.Models;
using HDUtilities;

namespace ITInventory.Controllers
{
    public class ReturnController : Controller
    {

        ITInventoryEntities db = new ITInventoryEntities();

        public ActionResult Index()
        {
            var returnHistory = db.Returns.OrderBy(x => x.LastEditDateTime).ToList();

            return View(returnHistory);
        }
        [HttpPost]
        public ActionResult CheckIn_Return(OrderHistoryConfirm confirmReturn, Product Products, Return Returns)
        {
            var returns = db.Returns.ToList();
            var productsList = db.Products.ToList();
            var returnItem = productsList.Where(x => x.Id == confirmReturn.ProductId).FirstOrDefault();

            returnItem.StockLevel += confirmReturn.QtyConfirmed;

            var returning = returns.Where(x => x.Id == confirmReturn.CheckId).FirstOrDefault();

            returning.InInventory = confirmReturn.CheckedIn;

            db.SaveChanges();

            return View();
        }
    }
}

//if ((allocate.OrderCost / allocate.ProductCount) != Convert.ToDouble(returnProduct.SellPrice))
//{
//    returnProduct.NewPrice = Convert.ToDecimal(allocate.OrderCost / allocate.ProductCount);
//    returnProduct.NewStockLevel = allocate.ProductCount;
//}
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

namespace ITInventory.Controllers
{
    public class HomeController : Controller
    {
        ITInventoryEntities dbITInventory = new ITInventoryEntities();
        public ActionResult Index()
        {
            return View();
        }
        
        [HttpGet]
        public ActionResult Products_Read([DataSourceRequest]DataSourceRequest request)
        {
            var productlist = dbITInventory.Products.ToList();

            dbITInventory.Configuration.ProxyCreationEnabled = false;

            List<ProductViewModel> productmodel = new List<ProductViewModel>();

            productmodel = productlist.OrderByDescending(x => x.Active).ThenBy(x => x.ProductName).ToList().Select(
                x => new ProductViewModel()
                {
                    Active = x.Active,
                    Catalog = x.Catalog,
                    Cost = x.Cost,
                    DefaultOrderSize = x.DefaultOrderSize,
                    ProductName = x.ProductName,
                    SellPrice = x.SellPrice,
                    StockLevel = x.StockLevel,
                    ReorderThreshold = x.ReorderThreshold,
                    Id = x.Id,
                    ImagePath = x.ImagePath,
                    LastEditDateTime = x.LastEditDateTime,
                    LastEditUser = x.LastEditUser,
                    LastOrderedDate = x.LastOrderedDate,
                    ListPrice = x.ListPrice,
                    ManufacturerId = x.ManufacturerId,
                    OnOrder = x.OnOrder,
                    PackageHeight = x.PackageHeight,
                    PackageLength = x.PackageLength,
                    PackageWeight = x.PackageWeight,
                    PackageUnits = x.PackageUnits,
                    QtyPerPackage = x.QtyPerPackage,
                    SKU = x.SKU,
                    Weight = x.Weight,
                    WeightUnit = x.WeightUnit
                }).ToList();

            var products = productmodel.ToList();
            IQueryable<Product> productQuery = dbITInventory.Products;
            DataSourceResult result = productmodel.ToDataSourceResult(request);

            return Json(result, JsonRequestBehavior.AllowGet); 

        }

        public ActionResult About()
        {
            var productList = dbITInventory.Allocations.ToList();

            return View(productList);
        }

        public ActionResult Contact(int[] productList)
        {
            ViewBag.Products = new SelectList(dbITInventory.Products.OrderBy(r => r.ProductName), "Id", "ProductName");
            ViewBag.Vendors = new SelectList(dbITInventory.Vendors.OrderBy(x => x.Name), "Id", "Name");

            var inventory = dbITInventory.Products.ToList();

            var ordered = inventory.Where(x => productList.Contains(x.Id)).ToList();

            return View(ordered);
        }

        [HttpPost]
        public ActionResult Create(ProductSelection products, [Bind(Include="Id,ProductId,QtyOrdered,LastEditUser,LastEditDateTime")] OrderHistory orderHistory)
        {
            var inventory = dbITInventory.Products.ToList();

            for (var x = 0; x < products.productList.Length; x++)
            {
                var ProductId = inventory.Where(m => m.ProductName == products.productList[x]).Select(m => m.Id).FirstOrDefault();
                var orderQty = products.orderQty[x];

                orderHistory.ProductId = ProductId;
                orderHistory.QtyOrdered = orderQty;
                orderHistory.LastEditUser = User.Identity.Name;
                orderHistory.LastEditDateTime = DateTime.Now;
                orderHistory.IsCheckedIn = false;
                try
                {
                    dbITInventory.OrderHistories.Add(orderHistory);
                    dbITInventory.SaveChanges();
                }
                catch (Exception ec)
                {
                    Console.WriteLine(ec.Message);
                }
            };

            return RedirectToAction("About", "Home");
        }
        
    }
}

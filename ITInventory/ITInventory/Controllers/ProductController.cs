using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Kendo.Mvc.UI;
using ITInventory.Models;
using HDUtilities;

namespace ITInventory.Controllers
{
    public class ProductController : HDController
    {
        private ITInventoryEntities db = new ITInventoryEntities();

        // GET: /Product/
        public ActionResult Index()
        {
            var products = db.Products.Include(p => p.Vendor).Include(p => p.Vendor);
            return View(products.OrderBy(x => x.ProductName).ToList());
        }

        // GET: /Product/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // GET: /Product/Create
        public ActionResult Create()
        {
            //ViewBag.KitId = new SelectList(db.KitItems, "Id", "KitName");
            ViewBag.ManufacturerId = new SelectList(db.Vendors, "Id", "LastEditUser");
            ViewBag.ManufacturerId = new SelectList(db.Vendors, "Id", "LastEditUser");
            return View();
        }

        // POST: /Product/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="Id,ProductName,SKU,SellPrice,Weight,WeightUnit,PackageWeight,PackageHeight,PackageLength,PackageUnits,QtyPerPackage,DefaultOrderSize,ManufacturerId,VendorId,ImagePath,OnOrder,Catalog,StockLevel,ReorderThreshold,KitId,Active,LastOrderedDate,LastEditDateTime,LastEditUser")] Product product)
        {
            if (ModelState.IsValid)
            {
                db.Products.Add(product);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ManufacturerId = new SelectList(db.Vendors, "Id", "LastEditUser", product.ManufacturerId);
            return View(product);
        }

        // GET: /Product/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            ViewBag.ManufacturerId = new SelectList(db.Vendors, "Id", "LastEditUser", product.ManufacturerId);
            ViewBag.CategoryId = new SelectList(db.Categories, "Id", "CategoryName", product.CategoryId);
            return View(product);
        }

        // POST: /Product/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="Id,ProductName,CategoryId,SKU,Cost,Weight,WeightUnit,PackageWeight,PackageHeight,PackageLength,PackageUnits,QtyPerPackage,DefaultOrderSize,ManufacturerId,VendorId,ImagePath,OnOrder,Catalog,StockLevel,ReorderThreshold,KitId,Active,LastOrderedDate,LastEditDateTime,LastEditUser")] Product product, decimal Cost, Lot lot, int? id)
        {
            var lotlist = db.Lots.ToList();

            ViewBag.CategoryId = new SelectList(db.Categories, "Id", "CategoryName", product.CategoryId);
            if (ModelState.IsValid)
            {
                bool newLot = false;

                foreach (var item in product.Lots.Where(r => r.ProductId == product.Id))
                {
                    if(item.Price != Cost){
                        newLot = true;
                        break;
                    }
                }

                if (newLot == true)
                {
                        lot.ProductId = product.Id;
                        lot.Price = Cost;
                        lot.Quantity = 0;
                        lot.LastEditDateTime = DateTime.Now.Date;
                        lot.LastEditUser = HDUtilities.UserInformation.GetCurrentUserName();
                        db.Lots.Add(lot);
                }
                db.Entry(product).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ManufacturerId = new SelectList(db.Vendors, "Id", "LastEditUser", product.ManufacturerId);
            ViewBag.ManufacturerId = new SelectList(db.Vendors, "Id", "LastEditUser", product.ManufacturerId);
            return View(product);
        }

        // GET: /Product/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // POST: /Product/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Product product = db.Products.Find(id);
            db.Products.Remove(product);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult KitCreate()
        {
            var Products = db.Products.ToList();

            ViewBag.Products = new SelectList(Products.Where(x => x.Active != false && x.IsKit != false).OrderBy(x => x.Id), "Id", "ProductName");

            return View();
        }

        [HttpPost]
        public ActionResult CreateKit(ProductSelection KitBuild, Product products, Kit Kits, KitProduct KitProducts)
        {
            bool successful = false;
            var kitList = db.Kits.ToList();
            var kitId = 0;
            if (kitList.FirstOrDefault().Id != null)
            {
                kitId = kitList.Max(r => r.Id) + 1;
                Kits.KitName = KitBuild.KitName;
                Kits.LastEditDateTime = DateTime.Now;
                Kits.LastEditUser = HDUtilities.UserInformation.GetCurrentUserName();
            }
            else
            {
                kitId = 1;
                Kits.KitName = KitBuild.KitName;
                Kits.LastEditDateTime = DateTime.Now;
                Kits.LastEditUser = HDUtilities.UserInformation.GetCurrentUserName();
            }
            db.Kits.Add(Kits);
            db.SaveChanges();

            for (var x = 0; x < KitBuild.productList.Length; x++)
            {
                KitProducts.KitId = kitId;
                KitProducts.ProductId = Convert.ToInt32(KitBuild.productList[x]);
                KitProducts.Quantity = Convert.ToInt32(KitBuild.orderQty[x]);
                KitProducts.LastEditDateTime = DateTime.Now;
                KitProducts.LastEditUser = HDUtilities.UserInformation.GetCurrentUserName();
                //decimal prodPrice =  db.Products.Where(r => r.Id == KitProducts.ProductId).FirstOrDefault().SellPrice;
                //kitPrice += Convert.ToDecimal(KitBuild.orderQty[x]) * prodPrice;
                try
                {
                    db.KitProducts.Add(KitProducts);
                    db.SaveChanges();
                    successful = true;
                }
                catch(Exception ex)
                {
                    throw ex;
                }
            }

            products.ProductName = KitBuild.KitName;
            products.SKU = "kit";
            //products.SellPrice = kitPrice;
            products.Weight = 0;
            products.WeightUnit = "lbs";
            products.PackageWeight = 0;
            products.PackageLength = 0;
            products.PackageHeight = 0;
            products.PackageUnits = "lbs";
            products.CategoryId = 1;
            products.QtyPerPackage = 0;
            products.DefaultOrderSize = 0;
            products.VendorId = 1;
            products.OnOrder = 0;
            products.StockLevel = 20;
            products.ReorderThreshold = 0;
            products.IsKit = true;
            products.KitId = kitId;
            products.Active = true;
            products.LastOrderedDate = DateTime.Now;
            products.LastEditDateTime = DateTime.Now;
            products.LastEditUser = HDUtilities.UserInformation.GetCurrentUserName();
            try
            {
                db.Products.Add(products);
                db.SaveChanges();
                successful = true;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            
            return Json(new { Result = successful }, JsonRequestBehavior.AllowGet);

        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

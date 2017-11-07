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

namespace ITInventory.Controllers
{
    public class ProductController : Controller
    {
        private ITInventoryEntities db = new ITInventoryEntities();

        // GET: /Product/
        public ActionResult Index()
        {
            var products = db.Products.Include(p => p.Vendor).Include(p => p.Vendor);
            return View(products.ToList());
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
            ViewBag.KitId = new SelectList(db.KitItems, "Id", "KitName");
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
            ViewBag.ManufacturerId = new SelectList(db.Vendors, "Id", "LastEditUser", product.ManufacturerId);
            return View(product);
        }

        // POST: /Product/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="Id,ProductName,SKU,SellPrice,Weight,WeightUnit,PackageWeight,PackageHeight,PackageLength,PackageUnits,QtyPerPackage,DefaultOrderSize,ManufacturerId,VendorId,ImagePath,OnOrder,Catalog,StockLevel,ReorderThreshold,KitId,Active,LastOrderedDate,LastEditDateTime,LastEditUser")] Product product)
        {
            if (ModelState.IsValid)
            {
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

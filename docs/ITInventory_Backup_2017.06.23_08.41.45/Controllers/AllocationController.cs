using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ITInventory.Models;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;

namespace ITInventory.Controllers
{
    public class AllocationController : Controller
    {
        private ITInventoryEntities db = new ITInventoryEntities();
        OfficeInfoLocalEntities dbOffice = new OfficeInfoLocalEntities();

        // GET: /Allocation/
        public ActionResult Index()
        {
            var itAllocations = db.Allocations.ToList();
            var itProducts = db.Products.ToList();
            var officeInfo = dbOffice.DDB_HDC_OFFICEINFO.ToList();

            ViewBag.OfficeName = new SelectList(dbOffice.DDB_HDC_OFFICEINFO.Where(x => x.OfficeName != null).OrderBy(x => x.OfficeName), "RSCDB", "OfficeName");

            List<AllocatedInventory> allocationModel = new List<AllocatedInventory>();

            allocationModel = itAllocations.ToList().Select(
                x => new AllocatedInventory()
                {
                    Id = x.Id,
                    OfficeName = officeInfo.Where(m => m.RSCDB == x.RSCDB).FirstOrDefault().OfficeName,
                    RSCDB = x.RSCDB,
                    ProductCount = x.ProductCount,
                    ProductId = x.ProductId,
                    ProductName = itProducts.Where(m => m.Id == x.ProductId).FirstOrDefault().ProductName,
                    LastEditDateTime = x.LastEditDateTime
                }).ToList();

            return View(allocationModel);
        }

        // GET: /Allocation/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Allocation allocation = db.Allocations.Find(id);
            if (allocation == null)
            {
                return HttpNotFound();
            }
            return View(allocation);
        }

        // GET: /Allocation/Create
        public ActionResult Read_Create([DataSourceRequest]DataSourceRequest request)
        {
            ViewBag.OfficeName = new SelectList(dbOffice.DDB_HDC_OFFICEINFO.Where(x => x.OfficeName != null).OrderBy(x => x.OfficeName), "RSCDB", "OfficeName");

            var products = db.Products.ToList();

            db.Configuration.ProxyCreationEnabled = false;

            List<ProductViewModel> productmodel = new List<ProductViewModel>();

            productmodel = products.OrderByDescending(x => x.Active).ThenBy(x => x.ProductName).ToList().Select(
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

            var productList = productmodel.ToList();
            IQueryable<Product> productQuery = db.Products;
            DataSourceResult result = productmodel.ToDataSourceResult(request);

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Create()
        {
            

                ViewBag.OfficeName = new SelectList(dbOffice.DDB_HDC_OFFICEINFO.Where(x => x.OfficeName != null).OrderBy(x => x.OfficeName), "RSCDB", "OfficeName");

            return View();
        }

        // POST: /Allocation/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public ActionResult AllocateProducts(Allocate allocatedStuff, [Bind(Include="Id,OfficeId,ProductId,ProductCount,ManufacturerId,VendorId,IsSplit,LastEditDateTime,LastEditUser")] Allocation allocation)
        {
            var product = db.Products.ToList();

            for (int x = 0; x < allocatedStuff.ProductCount.Length; x++)
            {
                var b = allocatedStuff.ProductCount[x];

                allocation.RSCDB = allocatedStuff.RSCDB[x];
                allocation.ProductId = allocatedStuff.ProductId[x];
                allocation.ProductCount = allocatedStuff.ProductCount[x];
                allocation.IsSplit = allocatedStuff.IsSplit;
                allocation.LastEditDateTime = DateTime.Now;
                allocation.LastEditUser = User.Identity.Name;
                product.Where(m => m.Id == allocatedStuff.ProductId[x]).FirstOrDefault().StockLevel -= allocatedStuff.ProductCount[x];
                try
                {
                    db.Allocations.Add(allocation);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

            return View(allocation);
        }

        // GET: /Allocation/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Allocation allocation = db.Allocations.Find(id);
            if (allocation == null)
            {
                return HttpNotFound();
            }
            return View(allocation);
        }

        // POST: /Allocation/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="Id,OfficeId,ProductId,ProductCount,ManufacturerId,VendorId,IsSplit,LastEditDateTime,LastEditUser")] Allocation allocation)
        {
            if (ModelState.IsValid)
            {
                db.Entry(allocation).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(allocation);
        }

        // GET: /Allocation/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Allocation allocation = db.Allocations.Find(id);
            if (allocation == null)
            {
                return HttpNotFound();
            }
            return View(allocation);
        }

        // POST: /Allocation/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Allocation allocation = db.Allocations.Find(id);
            db.Allocations.Remove(allocation);
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
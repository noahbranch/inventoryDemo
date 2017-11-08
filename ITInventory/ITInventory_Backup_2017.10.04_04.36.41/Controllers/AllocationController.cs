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
using HDUtilities;

namespace ITInventory.Controllers
{
    public class AllocationController : HDController
    {
        private ITInventoryEntities db = new ITInventoryEntities();
        OfficeInfoLocalEntities dbOffice = new OfficeInfoLocalEntities();

        // GET: /Allocation/
        public ActionResult Index()
        {
            var itAllocations = db.AllocationLots.ToList();
            var itProducts = db.Products.ToList();
            var officeInfo = dbOffice.DDB_HDC_OFFICEINFO.ToList();

            ViewBag.OfficeName = new SelectList(dbOffice.DDB_HDC_OFFICEINFO.Where(x => x.OfficeName != null).OrderBy(x => x.OfficeName), "RSCDB", "OfficeName");

            List<AllocatedInventory> allocationModel = new List<AllocatedInventory>();

            allocationModel = itAllocations.ToList().Select(
                x => new AllocatedInventory()
                {
                    Id = x.Allocation.Id,
                    OfficeName = officeInfo.Where(m => m.RSCDB == x.Allocation.AllocationOffices.Where(r => r.AllocationId == x.Allocation.Id).FirstOrDefault().OfficeId).FirstOrDefault().OfficeName,
                    RSCDB = x.Allocation.AllocationOffices.Where(r => r.AllocationId == x.Allocation.Id).FirstOrDefault().OfficeId,
                    ProductId = x.Lot.ProductId,
                    ProductName = x.Lot.Product.ProductName,
                    OrderCost = x.Quantity * x.Lot.Price,
                    ProductCount = Convert.ToInt32(x.Quantity),
                    LastEditDateTime = x.Allocation.LastEditDateTime,
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

        public ActionResult Create(DDB_HDC_OFFICEINFO officeInfo)
        {
            ViewBag.OfficeName = new SelectList(dbOffice.DDB_HDC_OFFICEINFO.Where(x => x.OfficeName != null).OrderBy(x => x.OfficeName), "RSCDB", "OfficeName");
            var products = db.Products.ToList();
            var lots = db.Lots.ToList();

            return View(officeInfo);
        }
        // GET: /Allocation/Create
        [HttpGet]
        public ActionResult Read_Create([DataSourceRequest]DataSourceRequest request)
        {
            ViewBag.OfficeName = new SelectList(dbOffice.DDB_HDC_OFFICEINFO.Where(x => x.OfficeName != null).OrderBy(x => x.OfficeName), "RSCDB", "OfficeName");

            var products = db.Products.ToList();
            var lot = db.Lots.ToList();

            db.Configuration.ProxyCreationEnabled = false;

            List<AllocationViewModel> productmodel = new List<AllocationViewModel>();

            productmodel = products.Where(x => x.Active == true && x.Lots.Where(r => r.ProductId == x.Id).Sum(r => r.Quantity) > 0).OrderByDescending(x => x.Active).ThenBy(x => x.ProductName).ToList().Select(
                x => new AllocationViewModel()
                {
                    Id = x.Lots.Where(r => r.ProductId == x.Id).FirstOrDefault().Id,
                    ProductId = x.Id,
                    ProductName = x.ProductName,
                    StockLevel = x.Lots.Where(r => r.ProductId == x.Id).Select(m => m.Quantity).Sum()
                }).ToList();

            DataSourceResult result = productmodel.ToDataSourceResult(request);

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        //SubPrices
        [HttpGet]
        public ActionResult Create_SubPrices([DataSourceRequest]DataSourceRequest request, int ProductId)
        {
            List<AllocationViewModel> lotModel = new List<AllocationViewModel>();
            var allocationLots = db.AllocationLots.ToList();
            var products = db.Products.ToList();
            var lots = db.Lots.ToList();

            lotModel = lots.Where(r => r.ProductId == ProductId).Select(
                x => new AllocationViewModel()
                {
                    Id = x.ProductId,
                    Quantity = x.Quantity,
                    SellPrice = x.Price
                }).ToList();

            DataSourceResult result = lotModel.ToDataSourceResult(request);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        // POST: /Allocation/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public ActionResult AllocateProducts(Allocate allocatedStuff, [Bind(Include="Id,OfficeId,ProductId,ProductCount,ManufacturerId,VendorId,IsSplit,LastEditDateTime,LastEditUser")] Allocation allocation, AllocationLot allocationLot, Lot lots, AllocationOffice allocationOffices)
        {
            var productlist = db.Products.Where(x=> allocatedStuff.ProductId.Any(d=>d == x.Id)).ToList();
            bool successful = false;
            var offices = dbOffice.DDB_HDC_OFFICEINFO.ToList();
            string user = HDUtilities.UserInformation.GetCurrentUserName();

            Allocation allocationList = new Allocation();
            /*try
            {*/
                for (int y = 0; y < allocatedStuff.RSCDB.Length; y++)
                {
                    var office = allocatedStuff.RSCDB[y];

                    for (int x = 0; x < allocatedStuff.ProductCount.Length; x++)
                    {
                        int ProductCount = allocatedStuff.ProductCount[x];
                        decimal totalOrdered = ProductCount;

                        int splitCount = 1;
                        if (allocatedStuff.IsSplit == true)
                        {
                            splitCount = allocatedStuff.RSCDB.Count();
                        }

                        allocationList.LastEditUser = user;
                        allocationList.LastEditDateTime = DateTime.Now;                       


                        while (ProductCount > 0)
                        {
                            var product = productlist.Where(r => r.Id == allocatedStuff.ProductId[x] && r.Lots.Any(d => d.Quantity != 0)).FirstOrDefault();

                            Lot lot = product.Lots.Where(r => r.Quantity != 0).OrderBy(d => d.Price).FirstOrDefault();

                            allocationList.AllocationLots.Add(new AllocationLot() { 
                                AllocationId = allocation.Id,
                                Quantity = ProductCount,
                                LotId = lot.Id,
                                LastEditDateTime = DateTime.Now,
                                LastEditUser = user
                            });

                           allocationList.AllocationOffices.Add(new AllocationOffice()
                            {
                                AllocationId = allocation.Id,
                                OfficeId = office,
                                LastEditDateTime = DateTime.Now,
                                LastEditUser = user
                            });

                             var price = lot.Price;
                            if (ProductCount < lot.Quantity)
                            {
                                lot.Quantity -= ProductCount;
                                ProductCount = 0;
                            }
                            else
                            {
                                ProductCount -= lot.Quantity;
                                lot.Quantity = 0;
                            }
                        }
                    }
                }
                db.Allocations.Add(allocationList);
                db.SaveChanges();
            //}
            /*catch (Exception ex)
            {
                throw ex;
            }*/

            return Json(new { Result = successful }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult ReturnAllocation(AllocatedInventory returnAllocation, [Bind(Include = "Id,ProductName,SKU,SellPrice,Weight,WeightUnit,PackageWeight,PackageHeight,PackageLength,PackageUnits,QtyPerPackage,DefaultOrderSize,ManufacturerId,VendorId,ImagePath,OnOrder,Catalog,StockLevel,ReorderThreshold,KitId,Active,LastOrderedDate,LastEditDateTime,LastEditUser")] Product product, Return returned, int entityId = 0, int Office1 = 0, int Office2 = 0)
        {
            var productList = db.Products.ToList();
            var id = returnAllocation.Id;
            bool successful = false;
            var allocation = db.Allocations.ToList();

            try
            {
                if (allocation.Where(r => r.AllocationOffices.Where(x => x.Allocation.Id == id).FirstOrDefault().OfficeId == returnAllocation.RSCDB).Count() > 1)
                {
                    foreach (var item in allocation)
                    {
                        //var returnProduct = productList.Where(r => r.Id == item.ProductId).FirstOrDefault();
                        Allocation allocate = db.Allocations.Where(r => r.Id == item.Id).FirstOrDefault();
                        db.Allocations.Remove(allocate);

                        //returned.OfficeId = item.RSCDB;
                        //returned.ProductId = item.ProductId;
                        returned.LastEditDateTime = DateTime.Now.Date;
                        returned.LastEditUser = HDUtilities.UserInformation.GetCurrentUserName();
                        db.Returns.Add(returned);
                        //if ((allocate.OrderCost / allocate.ProductCount) != Convert.ToDouble(returnProduct.SellPrice))
                        //{
                        //    returnProduct.NewPrice = Convert.ToDecimal(allocate.OrderCost / allocate.ProductCount);
                        //    returnProduct.NewStockLevel = allocate.ProductCount;
                        //}
                        db.SaveChanges();
                    }
                }
                else
                {
                    var allocationsingle = db.Allocations.Where(x => x.Id == returnAllocation.Id).FirstOrDefault();
                    //var returnProduct = productList.Where(r => r.Id == allocation.ProductId).FirstOrDefault();
                    Allocation allocate = db.Allocations.Where(m => m.Id == returnAllocation.Id).FirstOrDefault();
                    db.Allocations.Remove(allocate);

                    db.Returns.Add(returned);
                    //returned.OfficeId = allocationsingle.RSCDB;
                    //returned.ProductId = allocation.ProductId;
                    //returned.ProductCost = productList.Where(x => x.Id == allocation.ProductId).FirstOrDefault().SellPrice;
                    returned.LastEditDateTime = DateTime.Now.Date;
                    returned.LastEditUser = HDUtilities.UserInformation.GetCurrentUserName();
                    db.SaveChanges();
                }

                successful = true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return Json(new { Result = successful }, JsonRequestBehavior.AllowGet);
        }

        //// GET: /Allocation/Edit/5
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
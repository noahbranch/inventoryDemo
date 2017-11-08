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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

namespace ITInventory.Controllers
{
    public class AllocationController : HDController
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

            List<Allocation> allocationModel = new List<Allocation>();

            allocationModel = itAllocations.ToList().Select(
                x => new Allocation()
                {
                    Id = x.Id,
                    AllocationLots = x.AllocationLots,
                    AllocationOffices = x.AllocationOffices,
                    LastEditUser = x.LastEditUser
                }).ToList();

            return View(allocationModel);
        }

        [HttpGet]
        public ActionResult Allocation_Read([DataSourceRequest]DataSourceRequest request)
        {

            db.Configuration.ProxyCreationEnabled = false;

            var itAllocations = db.Allocations.ToList();
            var itAllocationOffices = db.AllocationOffices.ToList();
            var itAlloctionLots = db.AllocationLots.ToList();
            var Offices = dbOffice.DDB_HDC_OFFICEINFO.ToList();

            List<AllocatedInventory> allocationModel = new List<AllocatedInventory>();

            allocationModel = itAllocations.Where(r => r.AllocationLots.Where(m => m.Quantity != 0).Count() != 0).ToList().Select(
                x => new AllocatedInventory()
                {
                    Id = x.Id,
                    OfficeName = ConverOfficeIdtoOfficeNames(x.AllocationOffices.Where(r => r.AllocationId == x.Id).Select(r => r.OfficeId).ToArray()),
                    LastEditDateTime = x.LastEditDateTime

                }).ToList();

            DataSourceResult result = allocationModel.ToDataSourceResult(request);

            return Json(result, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public ActionResult Create_Allocate([DataSourceRequest]DataSourceRequest request, int Id)
        {
            db.Configuration.ProxyCreationEnabled = false;
            List<AllocationViewModel> lotModel = new List<AllocationViewModel>();
            var allocationLots = db.AllocationLots.ToList();
            var lots = db.Lots.ToList();
            var products = db.Products.ToList();

            lotModel = allocationLots.Where(r => r.AllocationId == Id && r.Quantity != 0).Select(
                x => new AllocationViewModel()
                {
                    Id = x.Id,
                    ProductName = x.Lot.Product.ProductName,
                    Quantity = x.Quantity,
                    SellPrice = x.Lot.Price,
                    ProductId = x.Lot.ProductId,
                    LotId = x.Lot.Id
                }).ToList();

            DataSourceResult result = lotModel.ToDataSourceResult(request);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        // GET: /Allocation/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AllocationLot allocationLot = db.AllocationLots.Find(id);
            if (allocationLot == null)
            {
                return HttpNotFound();
            }
            return View(allocationLot);
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
        public ActionResult AllocateProducts(Allocate allocatedStuff, [Bind(Include="Id,OfficeId,ProductId,ProductCount,ManufacturerId,VendorId,IsSplit,LastEditDateTime,LastEditUser")] Allocation allocation, AllocationLot allocationLot, Lot lots, AllocationOffice allocationOffices, OrderProduct orderProducts)
        {
            string user = HDUtilities.UserInformation.GetCurrentUserName();
            var ordProdsArray = db.OrderProducts.Where(r => r.LastEditUser == user && r.Order.Submitted != true).ToArray();
            var ordProdIdsList = db.OrderProducts.Where(r => r.LastEditUser == user && r.Order.Submitted != true).Select(r => r.ProductId).ToList();
            var productlist = db.Products.Where(x=> ordProdIdsList.Any(d=>d == x.Id)).ToList();
            bool successful = false;
            var offices = dbOffice.DDB_HDC_OFFICEINFO.ToList();


            Allocation allocationList = new Allocation();
            
                for (int y = 0; y < allocatedStuff.RSCDB.Length; y++)
                {
                    var office = allocatedStuff.RSCDB[y];


                    allocationList.AllocationOffices.Add(new AllocationOffice()
                    {
                        AllocationId = allocation.Id,
                        OfficeId = office,
                        LastEditDateTime = DateTime.Now,
                        LastEditUser = user
                    });


                }
                    for (int x = 0; x < ordProdsArray.Length; x++)
                    {
                        int ProductCount = ordProdsArray[x].Quantity;
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
                            var product = productlist.Where(r => r.Id == ordProdsArray[x].ProductId && r.Lots.Any(d => d.Quantity != 0)).FirstOrDefault();

                            Lot lot = product.Lots.Where(r => r.Quantity != 0).OrderBy(d => d.Price).FirstOrDefault();

                            allocationList.AllocationLots.Add(new AllocationLot() { 
                                AllocationId = allocation.Id,
                                Quantity = (ProductCount > lot.Quantity ? lot.Quantity : ProductCount),
                                LotId = lot.Id,
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
                        var order = db.OrderProducts.Find(ordProdsArray[x].Id);
                        db.OrderProducts.Remove(order);
                    }

                db.Allocations.Add(allocationList);
                db.SaveChanges();
                successful = true;

            return Json(new { Result = successful }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult ReturnAllocation(AllocatedInventory returnAllocation, [Bind(Include = "Id,ProductName,SKU,SellPrice,Weight,WeightUnit,PackageWeight,PackageHeight,PackageLength,PackageUnits,QtyPerPackage,DefaultOrderSize,ManufacturerId,VendorId,ImagePath,OnOrder,Catalog,StockLevel,ReorderThreshold,KitId,Active,LastOrderedDate,LastEditDateTime,LastEditUser")] Product product, Return returned, int entityId = 0, int Office1 = 0, int Office2 = 0)
        {
            var productList = db.Products.ToList();
            var allocationId = returnAllocation.Id;
            var user = HDUtilities.UserInformation.GetCurrentUserName();
            bool successful = false;
            var allocation = db.Allocations.Where(r => r.Id == allocationId).ToList();
            var allocationLots = db.AllocationLots.Where(r => r.AllocationId == allocationId && r.LotId == returnAllocation.LotId).FirstOrDefault();

            allocationLots.Quantity -= returnAllocation.ProductCount;


            if (allocationLots.Quantity <= 0)
            {
                returned.IsPartial = false;
            }
            else
            {
                returned.IsPartial = true;
            }

            returned.AllocationLotId = allocationLots.Id;
            returned.InInventory = false;
            returned.LastEditDateTime = DateTime.Now;
            returned.LastEditUser = user;
            returned.Quantity = returnAllocation.ProductCount;

            db.Returns.Add(returned);

            try
            {
                db.SaveChanges();
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

        public string ConverOfficeIdtoOfficeNames(int[] array)
        {
            var offices = dbOffice.DDB_HDC_OFFICEINFO.ToList();
            
            StringBuilder builder = new StringBuilder();

            int lastOffId = array.Last();

            foreach (int OfficeId in array)
            {
                var offName = offices.Where(r => r.RSCDB == OfficeId).FirstOrDefault().OfficeName;
                builder.Append(offName);
                if (OfficeId != lastOffId)
                {
                    builder.Append(", ");
                }
            }

            return builder.ToString();
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
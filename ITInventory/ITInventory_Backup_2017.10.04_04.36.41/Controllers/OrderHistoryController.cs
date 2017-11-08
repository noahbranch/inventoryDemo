using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using ITInventory.Models;
using HDUtilities;
using System.Net;
using System.Data;

namespace ITInventory.Controllers
{
    public class OrderHistoryController : HDController
    {

        ITInventoryEntities db = new ITInventoryEntities();
        //
        // GET: /OrderHistory/
        public ActionResult Index()
        {
            var orderHist = db.OrderProducts.Where(x => x.Order.Submitted != false).OrderBy(x => x.LastEditDateTime).ToList();

            return View(orderHist);
        }
        [HttpPost]
        public ActionResult Update_OrderConfirm(OrderHistoryConfirm OrderHistConfirm, [Bind(Include = "Id, StockLevel")]Product Products, [Bind(Include = "Id,OrderCost,ProductId,QtyOrdered,LastEditUser,LastEditDateTime")] OrderProduct orderProduct, Order ordere, Lot lot)
        {
            var x = OrderHistConfirm.ProductId;
            var orderedProduct = db.OrderProducts.Where(r => r.Id == OrderHistConfirm.CheckId).FirstOrDefault();
            var prod = db.Products.ToList();
            var lotList = db.Lots.ToList();
            var order = db.Orders.Where(r => r.Id == OrderHistConfirm.CheckId).ToList();

            orderedProduct.CheckedIn = true;

            lotList.Where(r => r.Id == OrderHistConfirm.LotId);

            db.SaveChanges();

            return View();
        }

         // GET: /OrderProduct/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            OrderProduct orderProduct = db.OrderProducts.Find(id);
            if (orderProduct == null)
            {
                return HttpNotFound();
            }
            return View(orderProduct);
        }

        // GET: /OrderProduct/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: /OrderProduct/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name,IsOrderProduct,LastEditDateTime,LastEditUser")] OrderProduct orderProduct)
        {
            if (ModelState.IsValid)
            {
                db.OrderProducts.Add(orderProduct);
                orderProduct.LastEditDateTime = DateTime.Now;
                orderProduct.LastEditUser = HDUtilities.UserInformation.GetCurrentUserName();
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(orderProduct);
        }

        // GET: /OrderProduct/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            OrderProduct orderProduct = db.OrderProducts.Find(id);
            if (orderProduct == null)
            {
                return HttpNotFound();
            }
            return View(orderProduct);
        }

        // POST: /OrderProduct/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,IsOrderProduct,LastEditDateTime,LastEditUser")] OrderProduct orderProduct)
        {
            if (ModelState.IsValid)
            {
                db.Entry(orderProduct).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(orderProduct);
        }

        // GET: /OrderProduct/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            OrderProduct orderProduct = db.OrderProducts.Find(id);
            if (orderProduct == null)
            {
                return HttpNotFound();
            }
            return View(orderProduct);
        }

        // POST: /OrderProduct/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            OrderProduct orderProduct = db.OrderProducts.Find(id);
            db.OrderProducts.Remove(orderProduct);
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
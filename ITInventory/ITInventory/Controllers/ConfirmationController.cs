using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ITInventory.Models;
using HDUtilities;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using System.Net.Mail;

namespace ITInventory.Controllers
{
    public class ConfirmationController : Controller
    {
        private ITInventoryEntities dbIT = new ITInventoryEntities();
        private OfficeInfoLocalEntities dbOfficeInfo = new OfficeInfoLocalEntities();
        //
        // GET: /Confirmation/
        public ActionResult Order(Vendor vendorInfo)
        {
            var vendorList = dbIT.Vendors.ToList();
            var user = HDUtilities.UserInformation.GetCurrentUserName();
            ViewBag.Vendors = new SelectList(vendorList.Where(r => r.Email != null), "Email", "Email");
            var ordProds = dbIT.OrderProducts.Where(r => r.LastEditUser == user && r.Order.Submitted != true).ToList();

            return View(vendorInfo);
        }
        public ActionResult Allocate(DDB_HDC_OFFICEINFO officeInfo)
        {
            var user = HDUtilities.UserInformation.GetCurrentUserName();
            var ordProds = dbIT.OrderProducts.Where(r => r.LastEditUser == user && r.Order.Submitted != true).ToList();
            ViewBag.orderProducts = new SelectList(ordProds.Where(r => r.LastEditUser == user && r.Order.Submitted != true));

            List<DDB_HDC_OFFICEINFO> officeModel = new List<DDB_HDC_OFFICEINFO>();

            var offices = dbOfficeInfo.DDB_HDC_OFFICEINFO;
            officeModel = offices.Where(r => r.OfficeType != "Closed" && r.OfficeType != "Merged" && r.OfficeType != "Sold" && r.SpeedDial != null).ToList().Select(
                x => new DDB_HDC_OFFICEINFO()
                {
                    OfficeName = "(" + x.SpeedDial + ") " + x.OfficeName,
                    RSCDB = x.RSCDB,
                    SpeedDial = x.SpeedDial
                }).ToList();

            ViewBag.OfficeName = new SelectList(officeModel.Where(x => x.OfficeName != null).OrderBy(x => x.SpeedDial), "RSCDB", "OfficeName");

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
        [HttpPost]
        public ActionResult SendEmail([DataSourceRequest]DataSourceRequest request, VendorEmail vendorEmail)
        {
            //var user = 
            MailAddress to = new MailAddress("nbranch@heartland.com"/*vendorEmail.ToAddress*/);
            MailAddress from = new MailAddress(vendorEmail.FromAddress);

            MailMessage mail = new MailMessage(from, to);

            mail.IsBodyHtml = false;
            //mail.CC.Add("dwebb@heartland.com");

            mail.Subject = vendorEmail.Subject;
            mail.Body = vendorEmail.messageBody;

            SmtpClient smtp = new SmtpClient();

            smtp.Send(mail);


            return Json(new { Result = true });
        }
	}
}
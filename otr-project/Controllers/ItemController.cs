using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Principal;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using System.IO;
using System.Data;
using otr_project.Models;
using System.Net.Mail;
using System.Data.Entity;
using otr_project.ViewModels;
using log4net;

namespace otr_project.Controllers
{
    [OutputCache(Duration = 0)]
    [LoggingFilter]
    public class ItemController : Controller
    {
        MarketPlaceEntities market = new MarketPlaceEntities();
        ErrorMessageViewModel ErrorMessage = new ErrorMessageViewModel();
        private static readonly ILog log = LogManager.GetLogger("otr_project.MvcApplication.Controllers");

        //
        // GET: /Item/
        public ActionResult Index()
        {
            var items = market.Items.Include(i => i.Owner).ToList();
            return View(items);
        }

        //POST: /Item/Search
        [HttpPost]
        public ActionResult Search(FormCollection collection)
        {
            string search = collection.Get("search");

            if (string.IsNullOrEmpty(search))
                return RedirectToAction("Index");

            var items = market.Items.Include(i => i.Owner).Where(i => i.Name.ToUpper().Contains(search.ToUpper())
                || i.Desc.ToUpper().Contains(search.ToUpper()));

            return View(items.ToList());
        }

        //
        // GET: /Item/Details/5
        public ActionResult Details(string id, string errMsg)
        {
            if (id == null)
            {
                log.Error("Item - Invalid item ID");
                ErrorMessage.ErrorCode = ErrorCode.UNKNOWN;
                return View("ErrorMessage", ErrorMessage);
            }

            var item = market.Items.Include(i => i.ItemImages).Include(i => i.BlackoutDates).SingleOrDefault(i => i.Id == id);
            if (item == null)
            {
                log.Error("Item - Item requested does not exist (" + id + ")");
                ErrorMessage.ErrorCode = ErrorCode.UNKNOWN;
                return View("ErrorMessage", ErrorMessage);
            }

            if (String.IsNullOrEmpty(errMsg) == false)
            {
                //Using Microsoft Security encoder to ensure we're not vulnerable to XSS
                ViewBag.ErrorMessage = Microsoft.Security.Application.Encoder.HtmlEncode(errMsg);
            }
            /*
            if (!item.isActive)
                return RedirectToAction("Index", "Home");
            */
            return View(item);
        }

        public ActionResult GetBlockedDates(string id)
        {
            List<string> dates = new List<string>();

            var item = market.Items.SingleOrDefault(i => i.Id == id);
            
            if (item == null)
            {
                log.Error("Item - Item does not exist (" + id + ")");
                return Json(dates);
            }

            foreach (BlackoutDate bd in item.BlackoutDates)
            {
                dates.Add(bd.Date.Date.ToShortDateString());
            }

            foreach (OrderDetailModel o in item.OrderDetails)
            {
                if (o.Status == null)
                    continue;

                if (o.Status == (int)OrderStatus.ORDER_TERMINATED)
                    continue;

                var date = o.PickupDate.Date;

                for (int i = 0; i <= (o.DrofoffDate - o.PickupDate).Days; i++)
                {
                    dates.Add(date.AddDays(i).Date.ToShortDateString());
                }
            }

            return Json(dates);
        }

        public ActionResult GetBlackOutDates(string id)
        {
            List<string> dates = new List<string>();

            var item = market.Items.Find(id);

            foreach (BlackoutDate bd in item.BlackoutDates)
            {
                dates.Add(bd.Date.Date.ToShortDateString());
            }

            return Json(dates);
        }

        public ActionResult GetBookedDates(string id)
        {
            List<string> dates = new List<string>();

            var item = market.Items.Find(id);

            foreach (OrderDetailModel o in item.OrderDetails)
            {
                var date = o.PickupDate.Date;
                for (int i = 0; i <= (o.DrofoffDate - o.PickupDate).Days; i++)
                {
                    dates.Add(date.AddDays(i).Date.ToShortDateString());
                }
            }

            return Json(dates);
        }


        public ActionResult GetItemImage(string id)
        {
            var image = market.ItemImages.SingleOrDefault(i => i.Id == id);
            if (image != null)
            {
                return File(image.Contents, image.ContentType);
            }

            return null;
        }
        //
        // GET: /Item/Create
        [Authorize]
        public ActionResult Create()
        {
            Session["CategoryId"] = new SelectList(market.Categories, "Id", "Name");
            Session["AgreementId"] = new SelectList(market.Agreements.Where(a => a.UserModelEmail == User.Identity.Name), "Id", "FileName");
            Session["RegionId"] = new SelectList(market.Regions, "Id", "Name");

            return View();
        }

        //
        // POST: /Item/Create

        [HttpPost]
        [Authorize]
        public ActionResult Create(ItemModel item)
        {
            Session["CategoryId"] = new SelectList(market.Categories, "Id", "Name");
            Session["AgreementId"] = new SelectList(market.Agreements.Where(a => a.UserModelEmail == User.Identity.Name), "Id", "FileName");
            Session["RegionId"] = new SelectList(market.Regions, "Id", "Name");
            if (ModelState.IsValid)
            {
                // So somehow by setting Session here as well prevents fatal exceptions. Don't ask me how. 
                

                var user = market.Users.SingleOrDefault(u => u.Email == User.Identity.Name);

                string itemID = System.Guid.NewGuid().ToString();
                string blackOutDates = Request.Params.Get("datepicker");
                string upload_result = string.Empty;
                int valid_file_count = 0;

                item.Id = itemID;

                if (Request.Files.Count > 0)
                {
                    for (int i = 0; i < Request.Files.Count; i++)
                    {
                        if (Request.Files[i].ContentLength <= 0)
                            continue;

                        valid_file_count++;

                        upload_result = UploadPicture(Request.Files[i], itemID);
                        if (upload_result == "valid")
                        {
                            item.ImageCount += 1;
                        }
                        else if (upload_result == "exception_wrong_format")
                        {
                            ErrorMessage.ErrorCode = ErrorCode.IMAGE_FORMAT_NOT_SUPPORTED;
                            return View("ErrorMessage", ErrorMessage);
                        }
                        else if (upload_result == "exception_image_too_big")
                        {
                            ErrorMessage.ErrorCode = ErrorCode.IMAGE_SIZE_TOO_BIG;
                            return View("ErrorMessage", ErrorMessage);
                        }
                        else if (upload_result == "exception")
                        {
                            ErrorMessage.ErrorCode = ErrorCode.IMAGE_UPLOAD_FAILED;
                            return View("ErrorMessage", ErrorMessage);
                        }
                    }
                }

                if (valid_file_count == 0)
                {
                    ErrorMessage.ErrorCode = ErrorCode.NO_IMAGE_UPLOADED;
                    return View("ErrorMessage", ErrorMessage);
                }

                if (String.IsNullOrEmpty(blackOutDates) == false)
                {
                    string[] dates = blackOutDates.Split(',');
                    foreach (string d in dates)
                    {
                        DateTime bOutDate = DateTime.Parse(d);
                        market.BlackoutDates.Add(new BlackoutDate { Date = bOutDate, ItemModelId = item.Id });
                    }
                }

                item.UserModelEmail = User.Identity.Name;
                item.DateCreated = System.DateTime.Now;
                item.isActive = true;

                item.CountryId = 1;

                market.Addresses.Add(new Address
                {
                    ItemModelId = itemID,
                    StreetAddress1 = Request.Params.Get("PickupLocation.StreetAddress1"),
                    StreetAddress2 = Request.Params.Get("PickupLocation.StreetAddress2"),
                    City = Request.Params.Get("PickupLocation.City"),
                    PostalCode = Request.Params.Get("PickupLocation.PostalCode")
                });

                market.Items.Add(item);
                market.SaveChanges();

                // Save item's pickup location to user profile if requested by user
                if (String.IsNullOrEmpty(Request.Params.Get("saveLocationToProfile")) == false)
                {
                    user.Address1 = Request.Params.Get("PickupLocation.StreetAddress1");
                    user.Address2 = Request.Params.Get("PickupLocation.StreetAddress2");
                    user.City = Request.Params.Get("PickupLocation.City");
                    user.PostalCode = Request.Params.Get("PickupLocation.PostalCode");
                    user.RegionId = item.RegionId;
                    user.CountryId = item.CountryId;
                    market.Entry(user).State = EntityState.Modified;
                    market.SaveChanges();
                }
                log.Info("Item - New item " + item.Id + " created by user " + User.Identity.Name);
                return RedirectToAction("Details", new { id = item.Id });
            }
            else
            {
                return View(item);
            }
        }

        [Authorize]
        [HttpPost]
        public ActionResult DeleteImageFromItem(string id, string image)
        {
            var errorResult = Json(new KeyValuePair<string, bool>("isError", true));
            var goodResult = Json(new KeyValuePair<string, bool>("isError", false));
            //var lastImage = Json(new KeyValuePair<string, string>("result", "lastImage"));

            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(image))
            {
                return errorResult;
            }

            var item = market.Items.SingleOrDefault(i => i.Id == id);
            if (item == null)
                return errorResult;
            /*
                        if (!item.isActive)
                            return RedirectToAction("Index", "Home");
            */
            if (item.Owner.Email != User.Identity.Name)
                return errorResult;

            /*
            if (item.ItemImages.Count() == 1)
            {
                return lastImage;
            }
            */

            foreach (var i in item.ItemImages)
            {
                if (i.Id == image)
                {
                    item.ImageCount--;
                    market.Entry(item).State = EntityState.Modified;
                    market.Entry(i).State = EntityState.Deleted;
                    market.SaveChanges();
                    log.Info("Item - User " + User.Identity.Name + " deleted image " + i.Name + " from " + item.Id + " item.");
                    return goodResult;
                }
            }

            return errorResult;
        }
        //
        // GET: /Item/Edit/5
        [Authorize]
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                ErrorMessage.ErrorCode = ErrorCode.UNKNOWN;
                return View("ErrorMessage", ErrorMessage);
            }

            var item = market.Items.Find(id);
            if (item == null)
            {
                ErrorMessage.ErrorCode = ErrorCode.UNKNOWN;
                return View("ErrorMessage", ErrorMessage);
            }
            /*
                        if (!item.isActive)
                            return RedirectToAction("Index", "Home");
            */
            if (item.Owner.Email != User.Identity.Name)
                return RedirectToAction("Index", "Home");

            Session["CategoryId"] = new SelectList(market.Categories, "Id", "Name", item.CategoryId);
            Session["AgreementId"] = new SelectList(market.Agreements.Where(a => a.UserModelEmail == User.Identity.Name), "id", "FileName", item.AgreementId);
            Session["RegionId"] = new SelectList(market.Regions, "Id", "Name", item.RegionId);
            Session["ITEM_IS_ACTIVE"] = item.isActive;
            return View(item);
        }

        //
        // POST: /Item/Edit/5
        [Authorize]
        [HttpPost]
        public ActionResult Edit(string id, ItemModel mItem)
        {
            ValidateModel(mItem);
            var location = market.Addresses.SingleOrDefault(i => i.ItemModelId == mItem.Id);
            string blackOutDates = Request.Params.Get("datepicker");
            string imagepath = string.Empty;
            string upload_result = string.Empty;

            if (ModelState.IsValid)
            {
                // Failing as Owner isnt populated.
                //market.Items.Attach(mItem);
                if (Request.Files.Count > 0)
                {
                    for (int i = 0; i < Request.Files.Count; i++)
                    {
                        if (Request.Files[i].ContentLength <= 0)
                            continue;

                        upload_result = UploadPicture(Request.Files[i], id);
                        if (upload_result == "valid")
                        {
                            mItem.ImageCount += 1;
                        }
                        else if (upload_result == "exception_wrong_format")
                        {
                            log.Error("Item - Uploaded image is of the wrong format (Item: " + mItem.Id + ")");
                            ErrorMessage.ErrorCode = ErrorCode.IMAGE_FORMAT_NOT_SUPPORTED;
                            return View("ErrorMessage", ErrorMessage);
                        }
                        else if (upload_result == "exception_image_too_big")
                        {
                            log.Error("Item - Uploaded image is too large (Item: " + mItem.Id + ")");
                            ErrorMessage.ErrorCode = ErrorCode.IMAGE_SIZE_TOO_BIG;
                            return View("ErrorMessage", ErrorMessage);
                        }
                        else if (upload_result == "exception")
                        {
                            log.Error("Item - Error uploading image to item (Item: " + mItem.Id + ")");
                            ErrorMessage.ErrorCode = ErrorCode.IMAGE_UPLOAD_FAILED;
                            return View("ErrorMessage", ErrorMessage);
                        }
                    }
                }

                List<BlackoutDate> currentDates = market.BlackoutDates.Where(b => b.ItemModelId == mItem.Id).ToList();
                    
                foreach (BlackoutDate bd in currentDates)
                {
                    market.BlackoutDates.Remove(bd);
                }
                    
                if (String.IsNullOrEmpty(blackOutDates) == false)
                {
                    string[] dates = blackOutDates.Split(',');
                    foreach (string d in dates)
                    {
                        DateTime bOutDate = DateTime.Parse(d);
                        market.BlackoutDates.Add(new BlackoutDate { Date = bOutDate, ItemModelId = mItem.Id });
                    }
                }
                mItem.isActive = (bool)Session["ITEM_IS_ACTIVE"];
                mItem.AddressId = location.Id;
                //mItem.PickupLocation.Id = location.Id;
                //mItem.PickupLocation.ItemModelId = mItem.Id;
                location.StreetAddress1 = Request.Params.Get("PLStreetAddress1");
                location.StreetAddress2 = Request.Params.Get("PLStreetAddress2");
                location.City = Request.Params.Get("PLCity");
                location.PostalCode = Request.Params.Get("PLPostalCode");

                mItem.CountryId = 1;

                market.Entry(location).State = EntityState.Modified;
                market.Entry(mItem).State = EntityState.Modified;
                market.SaveChanges();

                return RedirectToAction("Details", new { id = mItem.Id });
            }
            return View(mItem);
        }

        /*
        [Authorize]
        public ActionResult Delete(string id)
        {
            ItemModel item = market.GetItem(id);
            if (item == null)
            {
                ErrorMessage.ErrorCode = ErrorCode.UNKNOWN;
                return View("ErrorMessage", ErrorMessage);
            }

            if (item.Owner.Email != User.Identity.Name)
                return RedirectToAction("Index", "Home");

            return View(item);
        }
        */

        //
        // POST: /Item/Delete/5
        [Authorize]
        public ActionResult Delete(string id, string returnUrl)
        {
            ItemModel item = market.Items.SingleOrDefault(i => i.Id == id);

            if (item != null)
            {
                if (item.Owner.Email != User.Identity.Name)
                {
                    log.Error("Item - Item delete unauthorized user " + User.Identity.Name); 
                    ErrorMessage.ErrorCode = ErrorCode.ITEM_DELETE_FAILED;
                    return View("ErrorMessage", ErrorMessage);
                }

                item.isActive = false;
                market.Entry(item).State = EntityState.Modified;
                market.SaveChanges();
                log.Info("Item - Item successfully deleted (id = " + id + ")");
                if (Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                else
                {
                    return RedirectToAction("Items", "User");
                }
            }

            ErrorMessage.ErrorCode = ErrorCode.ITEM_DELETE_FAILED;
            return View("ErrorMessage", ErrorMessage);
            
        }


        //Helper method to upload a picture.
        private string UploadPicture(HttpPostedFileBase file, string itemID)
        {
            try
            {
                if (!
                    (file.ContentType == "image/jpeg" ||
                     file.ContentType == "image/gif" ||
                     file.ContentType == "image/png" ||
                     file.ContentType == "image/bmp" ||
                     file.ContentType == "image/x-icon")
                    )
                {
                    return "exception_wrong_format";
                }

                if (file.ContentLength > 512000)
                {
                    return "exception_image_too_big";
                }

                if (file.ContentLength > 0 && file != null)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        file.InputStream.CopyTo(ms);
                        string id = Guid.NewGuid().ToString();
                        market.ItemImages.Add(new ItemImageFileModel
                        {
                            Id = id,
                            Name = Path.GetFileName(file.FileName),
                            Contents = ms.GetBuffer(),
                            ItemModelId = itemID,
                            ContentType = file.ContentType
                        });
                        return "valid";
                    }
                }
                return "empty";
            }
            catch (Exception ex)
            {
                return "exception";
            }
        }

        private bool IsImageDirEmpty(string dir)
        {
            try
            {
                string dirPath = Path.Combine(HttpContext.Server.MapPath("~/Uploads"), dir);
                if (Directory.EnumerateFiles(dirPath).Count() == 0)
                    return true;

                return false;
            }
            catch (Exception ex)
            {
                return true;
            }
        }

        protected override void Dispose(bool disposing)
        {
            market.Dispose();
            base.Dispose(disposing);
        }
    }
}
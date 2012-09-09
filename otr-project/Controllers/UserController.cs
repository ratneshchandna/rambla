using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using otr_project.Models;
using otr_project.ViewModels;
using otr_project.Utils;

namespace otr_project.Controllers
{ 
    public class UserController : Controller
    {
        public IFormsAuthenticationService FormsService { get; set; }
        public IMembershipService MembershipService { get; set; }
        private MarketPlaceEntities db = new MarketPlaceEntities();
        ErrorMessageViewModel ErrorMessage = new ErrorMessageViewModel();

        //public UserModel user { get; set; }
        //
        // GET: /User/
        public string CurrentUserIPAddress
        {
            get
            {
                return HttpContext.Request.UserHostAddress;
            }
        }

        [Authorize]
        public ActionResult Index()
        {
            var user = db.Users.SingleOrDefault(i => i.Email == User.Identity.Name);

            if (user == null)
            {
                ErrorMessage.ErrorCode = ErrorCode.UNKNOWN;
                return View("ErrorMessage", ErrorMessage);
            }

            ViewBag.RegionId = new SelectList(db.Regions, "Id", "Name", user.RegionId);
            ViewBag.UserEarnings = user.Earnings;
            ViewBag.ProfilePicture = (user.ProfilePicture != null);
            ViewBag.Name = user.FirstName + " " + user.LastName;
            ViewBag.isFacebookUser = user.isFacebookUser;
            if (user.isFacebookUser)
            {
                ViewBag.FacebookId = user.FacebookProfile.Id;
            }

            return View("Dashboard", user);
        }

        //
        // GET: /User/Items
        [Authorize]
        public ActionResult Items()
        {
            var user = db.Users.SingleOrDefault(i => i.Email == User.Identity.Name);

            if (user == null)
            {
                //User doesnt exist in our Database. Should Delete user.
                //MembershipService.DeleteUser(User.Identity.Name);
                //FormsService.SignOut();
                //return RedirectToAction("LogOn", "Account");

                ErrorMessage.ErrorCode = ErrorCode.UNKNOWN;
                return View("ErrorMessage", ErrorMessage);
            }
            /*
            if (user.Items.Count() == 0)
                return RedirectToAction("Index", "Home");
            */

            ViewBag.UserEarnings = user.Earnings;
            ViewBag.ProfilePicture = (user.ProfilePicture != null);
            ViewBag.Name = user.FirstName + " " + user.LastName;
            ViewBag.isFacebookUser = user.isFacebookUser;
            if (user.isFacebookUser)
            {
                ViewBag.FacebookId = user.FacebookProfile.Id;
            }

            return View(user.Items.ToList());
        }

        [Authorize]
        public ActionResult ItemsBorrowed()
        {
            var user = db.Users.SingleOrDefault(i => i.Email == User.Identity.Name);
            if (user == null)
            {
                //User doesnt exist in our Database. Should Delete user.
                //MembershipService.DeleteUser(User.Identity.Name);
                //FormsService.SignOut();
                //return RedirectToAction("LogOn", "Account");

                ErrorMessage.ErrorCode = ErrorCode.UNKNOWN;
                return View("ErrorMessage", ErrorMessage);
            }
            /*
            if (user.Items.Count() == 0)
                return RedirectToAction("Index", "Home");
            */

            var orders = db.Orders.Where(u => u.User.Email == user.Email && u.Confirmed == true).ToList();

            ViewBag.UserEarnings = user.Earnings;
            ViewBag.ProfilePicture = (user.ProfilePicture != null);
            ViewBag.Name = user.FirstName + " " + user.LastName;
            ViewBag.isFacebookUser = user.isFacebookUser;
            if (user.isFacebookUser)
            {
                ViewBag.FacebookId = user.FacebookProfile.Id;
            }

            return View(orders);
        }

        //
        // GET: /User/Edit/5
        [Authorize]
        public ActionResult Edit()
        {
            UserModel user = db.Users.Find(User.Identity.Name);

            if (user == null)
            {
                ErrorMessage.ErrorCode = ErrorCode.UNKNOWN;
                return View("ErrorMessage", ErrorMessage);
            }

            ViewBag.RegionId = new SelectList(db.Regions, "Id", "Name", user.RegionId);
            ViewBag.UserEarnings = user.Earnings;
            ViewBag.ProfilePicture = (user.ProfilePicture != null);
            ViewBag.Name = user.FirstName + " " + user.LastName;
            ViewBag.isFacebookUser = user.isFacebookUser;
            if (user.isFacebookUser)
            {
                ViewBag.FacebookId = user.FacebookProfile.Id;
            }

            return View(user);
        }

        //
        // POST: /User/Edit/5
        [Authorize]
        [HttpPost]
        public ActionResult Edit(UserModel usermodel)
        {
            try
            {
                var user = db.Users.SingleOrDefault(u => u.Email == User.Identity.Name);
                ViewBag.RegionId = new SelectList(db.Regions, "Id", "Name", usermodel.RegionId);
                ViewBag.Name = usermodel.FirstName + " " + usermodel.LastName;
                ViewBag.UserEarnings = user.Earnings;
                ViewBag.ProfilePicture = (user.ProfilePicture != null);
                ViewBag.isFacebookUser = user.isFacebookUser;
                if (user.isFacebookUser)
                {
                    ViewBag.FacebookId = user.FacebookProfile.Id;
                }

                foreach (string inputTagName in Request.Files)
                {
                    HttpPostedFileBase file = Request.Files[inputTagName];

                    if (file != null && file.ContentLength > 0)
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            file.InputStream.CopyTo(ms);
                            if (user.UserImageFileModelId != null)
                            {
                                var profilePic = db.Files.SingleOrDefault(f => f.UserModelEmail == User.Identity.Name);
                                profilePic.Name = Path.GetFileName(file.FileName);
                                profilePic.Contents = ms.GetBuffer();
                                profilePic.ContentType = file.ContentType;
                                db.Entry(profilePic).State = EntityState.Modified;
                            }
                            else
                            {
                                string id = Guid.NewGuid().ToString();
                                db.Files.Add(new UserImageFileModel
                                {
                                    Id = id,
                                    Name = Path.GetFileName(file.FileName),
                                    Contents = ms.GetBuffer(),
                                    UserModelEmail = user.Email,
                                    ContentType = file.ContentType
                                });
                                usermodel.UserImageFileModelId = id;
                            }
                        }
                    }
                    /* This is redundant logic. 
                    else
                    {
                        usermodel.UserImageFileModelId = user.UserImageFileModelId;
                    }*/
                }

                if (ModelState.IsValid)
                {
                    usermodel.Email = user.Email;
                    usermodel.isFacebookUser = user.isFacebookUser;
                    usermodel.FacebookUserId = user.FacebookUserId;
                    usermodel.Earnings = user.Earnings;
                    if (user.UserImageFileModelId != null)
                    {
                        usermodel.UserImageFileModelId = user.UserImageFileModelId;
                    }
                    usermodel.CountryId = 1;
                    db.Entry(user).State = EntityState.Detached;
                    db.SaveChanges();
                    db.Users.Attach(usermodel);
                    db.Entry(usermodel).State = EntityState.Modified;
                    db.SaveChanges();
                    Session["USER_F_NAME"] = usermodel.FirstName;
                    return RedirectToAction("Index");
                }


                return View(usermodel);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error has occured.");
                ViewBag.RegionId = new SelectList(db.Regions, "Id", "Name", usermodel.RegionId);
                ViewBag.UserEarnings = usermodel.Earnings;
                //do something
                return View(usermodel);
            }
        }

        //
        // GET: /User/GetProfilePicture
        //[Authorize]
        public ActionResult GetProfilePicture(string id) // parameter: itemid
        {
            UserModel user;

            if (id != null)
                user = db.Items.SingleOrDefault(i => i.Id == id).Owner;
            else
                user = db.Users.SingleOrDefault(u => u.Email == User.Identity.Name);

            if (user != null)
            {
                if (user.ProfilePicture != null)
                {
                    return File(user.ProfilePicture.Contents, user.ProfilePicture.ContentType);
                }
            }
            //Ideally we should return a blank face image here.
            return null;
        }
        /*
        //
        // GET: /User/Delete/5
        [Authorize]
        public ActionResult Delete()
        {
            try
            {
                UserModel usermodel = db.Users.Find(User.Identity.Name);
                if (usermodel == null)
                {
                    ErrorMessage.ErrorCode = ErrorCode.UNKNOWN;
                    return View("ErrorMessage", ErrorMessage);
                }

                db.Entry(usermodel).State = EntityState.Deleted;
                FormsService.SignOut();
                MembershipService.DeleteUser(usermodel.Email);

                db.SaveChanges();
            }
            catch (Exception ex)
            {
                ErrorMessage.ErrorCode = ErrorCode.UNKNOWN;
                return View("ErrorMessage", ErrorMessage);
            }

            return RedirectToAction("Index", "Home");
        }

        //
        // POST: /User/Delete/5
        [Authorize]
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed()
        {
            try
            {
                UserModel usermodel = db.Users.Find(User.Identity.Name);
                db.Users.Remove(usermodel);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                ErrorMessage.ErrorCode = ErrorCode.UNKNOWN;
                return View("ErrorMessage", ErrorMessage);
            }

            return RedirectToAction("Index");
        }
        */

        [Authorize]
        public ActionResult GetMessage(string id)
        {
            var message = db.Messages.Include(m => m.Messages).SingleOrDefault(m => m.Id == id);
            if (message != null)
            {
                try
                {
                    message.isRead = true;
                    db.Entry(message).State = EntityState.Modified;
                    db.SaveChanges();
                    return PartialView(message);
                }
                catch (Exception ex)
                {
                    ErrorMessage.ErrorCode = ErrorCode.UNKNOWN;
                    return View("ErrorMessage", ErrorMessage);
                }
            }

            return RedirectToAction("MessageBox");
        }

        [Authorize]
        [HttpPost]
        public ActionResult SubmitReply(FormCollection collection)
        {
            /*
            MessageModel message = db.Messages.Find(collection.Get("Id"));

            message.isRead = false;
            db.Entry(message).State = EntityState.Modified;
            //db.SaveChanges();
            */
            try
            {
                var msgThread = new ThreadModel()
                {
                    Message = collection.Get("Reply"),
                    MessageModelId = collection.Get("Id"),
                    Author = db.Users.Find(User.Identity.Name),
                    Date = DateTime.Now
                };

                ValidateModel(msgThread);
                db.Threads.Add(msgThread);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                ErrorMessage.ErrorCode = ErrorCode.UNKNOWN;
                return View("ErrorMessage", ErrorMessage);
            }

            return RedirectToAction("MessageBox");
        }

        [Authorize]
        public ActionResult MessageBox()
        {
            var user = db.Users.SingleOrDefault(u => u.Email == User.Identity.Name);

            if (user == null)
            {
                ErrorMessage.ErrorCode = ErrorCode.UNKNOWN;
                return View("ErrorMessage", ErrorMessage);
            }

            ViewBag.UserEarnings = user.Earnings;
            ViewBag.ProfilePicture = (user.ProfilePicture != null);
            ViewBag.Name = user.FirstName + " " + user.LastName;
            ViewBag.isFacebookUser = user.isFacebookUser;
            if (user.isFacebookUser)
            {
                ViewBag.FacebookId = user.FacebookProfile.Id;
            }

            return View("Inbox", db.Messages.Where(m => m.To == User.Identity.Name || m.From == User.Identity.Name).ToList());
        }

        [Authorize]
        [ChildActionOnly]
        public ActionResult Inbox()
        {
            var user = db.Users.SingleOrDefault(u => u.Email == User.Identity.Name);

            if (user == null)
            {
                ErrorMessage.ErrorCode = ErrorCode.UNKNOWN;
                return View("ErrorMessage", ErrorMessage);
            }

            ViewBag.UserEarnings = user.Earnings;
            ViewBag.ProfilePicture = (user.ProfilePicture != null);
            ViewBag.Name = user.FirstName + " " + user.LastName;
            ViewBag.isFacebookUser = user.isFacebookUser;
            if (user.isFacebookUser)
            {
                ViewBag.FacebookId = user.FacebookProfile.Id;
            }

            return PartialView(db.Messages.Where(m => m.To == User.Identity.Name || m.From == User.Identity.Name).ToList());
        }

        [Authorize]
        [ChildActionOnly]
        public ActionResult SendMessage(string item)
        {
            try
            {
                if (String.IsNullOrEmpty(item) != true)
                {
                    var itemOwner = db.Items.Find(item).Owner;
                    var message = new MessageModel()
                    {
                        Id = System.Guid.NewGuid().ToString(),
                        From = User.Identity.Name,
                        To = itemOwner.Email
                    };
                    Session["Message"] = message;
                    ViewBag.Item = item;
                    return PartialView(message);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage.ErrorCode = ErrorCode.UNKNOWN;
                return View("ErrorMessage", ErrorMessage);
            }

            return PartialView();
        }

        [Authorize]
        [HttpPost]
        public ActionResult SendMessage(FormCollection collection, string item)
        {
            try
            {
                MessageModel message = (MessageModel)Session["Message"];

                var msgThread = new ThreadModel()
                {
                    Message = collection.Get("Message"),
                    MessageModelId = message.Id,
                    Author = db.Users.Find(User.Identity.Name),
                    Date = DateTime.Now
                };

                message.Subject = collection.Get("Subject");
                message.isRead = false;

                db.Messages.Add(message);
                db.Threads.Add(msgThread);

                db.SaveChanges();
            }
            catch (Exception ex)
            {
                ErrorMessage.ErrorCode = ErrorCode.UNKNOWN;
                return View("ErrorMessage", ErrorMessage);
            }

            return RedirectToAction("Details", "Item", new { id = item });
        }

        [Authorize]
        public ActionResult GetThumb(string id)
        {
            return File(Url.Content("~/Content/images/default_thumb.jpg"), "image/jpg");
        }

        [Authorize]
        public ActionResult Agreements()
        {
            var user = db.Users.SingleOrDefault(u => u.Email == User.Identity.Name);
            if (user != null)
            {
                ViewBag.UserEarnings = user.Earnings;
                ViewBag.ProfilePicture = (user.ProfilePicture != null);
                ViewBag.Name = user.FirstName + " " + user.LastName;
                ViewBag.isFacebookUser = user.isFacebookUser;
                if (user.isFacebookUser)
                {
                    ViewBag.FacebookId = user.FacebookProfile.Id;
                }
            }
            return View();
        }

        [Authorize]
        public ActionResult GetAllAgreements()
        {
            var resultList = new List<FilesStatus>();
            var allAgreements = db.Agreements.Where(a => a.UserModelEmail == User.Identity.Name);
            
            if (allAgreements != null)
            {
                for (int i = 0; i < allAgreements.Count(); i++)
                {
                    resultList.Add(new FilesStatus()
                    {
                        thumbnail_url = Url.Content("~/User/GetThumb/") + allAgreements.ToList()[i].Id,
                        url = Url.Content("~/User/GetAgreement/") + allAgreements.ToList()[i].Id,
                        name = allAgreements.ToList()[i].FileName,
                        size = allAgreements.ToList()[i].ContentLength,
                        type = allAgreements.ToList()[i].ContentType,
                        delete_url = Url.Content("~/User/DeleteAgreement/") + allAgreements.ToList()[i].Id,
                        delete_type = "POST"
                    });
                }
            }
            var resultStatus = new UploadRentalAgreementViewModel()
            {
                Status = resultList
            };
            return Json(resultList, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpGet]
        public ActionResult GetAgreement (string id)
        {
            var fileHelper = new RentalAgreements();

            if (String.IsNullOrEmpty(id))
            {
                return RedirectToAction("GetAllAgreements");
            }

            var agreementQuery = db.Users.Find(User.Identity.Name).Agreements.Where(a => a.Id == id);
            
            if (agreementQuery == null || agreementQuery.Count() == 0)
            {
                return HttpNotFound();
            }

            var findAgreement = agreementQuery.ToList()[0];

            return File(fileHelper.Download(findAgreement.FileName, User.Identity.Name, HttpContext), findAgreement.ContentType);
        }

        [Authorize]
        [HttpPost]
        public void DeleteAgreement(string id)
        {
            var fileHelper = new RentalAgreements();

            if (String.IsNullOrEmpty(id))
            {
                fileHelper.DeleteAll(User.Identity.Name, HttpContext);
                foreach (Agreement a in db.Agreements.ToList())
                {
                    db.Entry(a).State = EntityState.Deleted;
                }
            }
            else
            {
                var agreement = db.Agreements.Find(id);
                if (agreement != null)
                {
                    fileHelper.Delete(agreement.FileName, User.Identity.Name, HttpContext);
                    db.Entry(agreement).State = EntityState.Deleted;
                }
            }

            db.SaveChanges();
        }

        [Authorize]
        [HttpGet]
        public ActionResult UploadAgreement(string id)
        {
            if (String.IsNullOrEmpty(id))
            {
                return RedirectToAction("GetAllAgreements");
            }
            else
            {
                return RedirectToAction("GetAgreement", new { id = id });
            }
        }

        [Authorize]
        [HttpPost]
        public ActionResult UploadAgreement()
        {
            var resultList = new List<FilesStatus>();
            var filesToUpload = this.HttpContext.Request.Files;
            var uploadHelper = new RentalAgreements();
            Agreement newAgreement;

            for (int i = 0; i < filesToUpload.Count; i++)
            {
                newAgreement = uploadHelper.Upload(filesToUpload[i], User.Identity.Name, HttpContext);
                if (newAgreement != null)
                {
                    var status = new FilesStatus()
                    {
                        thumbnail_url = Url.Content("~/User/GetThumb/") + newAgreement.Id,
                        url = Url.Content("~/User/GetAgreement/") + newAgreement.Id,
                        name = newAgreement.FileName,
                        size = filesToUpload[i].ContentLength,
                        type = filesToUpload[i].ContentType,
                        delete_url = Url.Content("~/User/DeleteAgreement/") + newAgreement.Id,
                        delete_type = "POST",
                        progress = "1.0"
                    };
                    
                    resultList.Add(status);
                    
                    db.Agreements.Add(newAgreement);
                    db.SaveChanges();
                }
            }

            var resultStatus = new UploadRentalAgreementViewModel()
            {
                Status = resultList
            };

            return Json(resultList);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
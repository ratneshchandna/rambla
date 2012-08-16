using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using otr_project.Models;
using otr_project.ViewModels;

namespace otr_project.Controllers
{
    public class RentalCartController : Controller
    {
        MarketPlaceEntities market = new MarketPlaceEntities();
        ErrorMessageViewModel ErrorMessage = new ErrorMessageViewModel();

        //
        // GET: /RentalCart/
        public ActionResult Index()
        {
            var cart = RentalCart.GetCart(this.HttpContext);

            // Set up our ViewModel
            var viewModel = new RentalCartViewModel
            {
                CartItems = cart.GetCartItems(),
                CartTotal = cart.GetTotal()
            };

            // Return the view
            return View(viewModel);
        }

        //
        // Post: /RentalCart/AddToCart
        [HttpPost]
        public ActionResult AddToCart(FormCollection collection)
        {            
            string id = collection.Get("itemId");
            string pickup = collection.Get("pickupdate");
            string dropoff = collection.Get("dropoffdate");
            DateTime pickupDate;
            DateTime dropoffDate;

            if (String.IsNullOrEmpty(id) || String.IsNullOrEmpty(pickup) || String.IsNullOrEmpty(dropoff))
            {
                string error = "Plase select accurate dates and try again.";
                return RedirectToAction("Details", "Item", new { id = collection.Get("itemid"), errMsg = error });
            }

            try
            {
                pickupDate = DateTime.Parse(pickup);
                dropoffDate = DateTime.Parse(dropoff);
            }
            catch (Exception ex)
            {
                string error = "Please select accurate dates and try again.";
                return RedirectToAction("Details", "Item", new { id = id, errMsg = error });
            }
            //Invalid Dates
            if (pickupDate.Date < DateTime.Today.Date || dropoffDate.Date < DateTime.Today.Date)
            {
                string error = "Please select accurate dates and try again.";
                return RedirectToAction("Details", "Item", new { id = id, errMsg = error });
            }

            int rentalPeriod = (dropoffDate - pickupDate).Days;
            if (rentalPeriod <= 0)
            {
                string error = "Dropoff date cannot be earlier than or the same as the pickup date.";
                return RedirectToAction("Details", "Item", new { id = id, errMsg = error });
            }

            // Retrieve the album from the database
            var addedItem = market.Items.SingleOrDefault(m => m.Id == id);

            if (addedItem == null)
            {
                string error = "The item you specified doesn't exist on Rambla.";
                return RedirectToAction("Details", "Item", new { id = id, errMsg = error });
            }

            if (User.Identity.IsAuthenticated)
            {
                if (addedItem.Owner.Email == User.Identity.Name)
                {
                    string error = "You cannot add your own item to your Rental cart.";
                    return RedirectToAction("Details", "Item", new { id = id, errMsg = error });
                }
            }

            foreach (DateTime d in GetBlockedDates(id))
            {
                if (pickupDate.Date <= d.Date && dropoffDate.Date >= d.Date)
                {
                    string error = "Your proposed rental period includes blocked dates. Please select different dates and try again.";
                    return RedirectToAction("Details", "Item", new { id = id, errMsg = error });
                }

                if (pickupDate.AddDays(1).Date == d.Date)
                {
                    string error = "Your pickup date has to be atlest two days before a blocked date.";
                    return RedirectToAction("Details", "Item", new { id = id, errMsg = error });
                }
            }

            // Add it to the shopping cart
            RentalCart cart = null;
            try
            {
                cart = RentalCart.GetCart(this.HttpContext);
            }
            catch (Exception ex)
            {
                ErrorMessage.ErrorCode = ErrorCode.UNKNOWN;
                return View("ErrorMessage", ErrorMessage);
            }

            if (cart.AddToCart(addedItem, DateTime.Parse(collection.Get("pickupdate")),
                DateTime.Parse(collection.Get("dropoffdate"))))
            {
                Session["RentalCartItems"] = cart.GetCartItems().Count;
                // Go back to the main store page for more shopping
                return RedirectToAction("Index");
            }
            else
            {
                //ModelState.AddModelError("", "This item is already in your cart. Select a different item");
                string error = "This item is already in your cart. Select a different item";
                return RedirectToAction("Details", "Item", new { id = id, errMsg = error });
            }
        }

        // AJAX: /ShoppingCart/RemoveFromCart/5

        [HttpPost]
        public ActionResult RemoveFromCart(int id)
        {
            string msg;
            // Remove the item from the cart
            RentalCart cart = null;
            string itemName = "";
            try
            {
                cart = RentalCart.GetCart(this.HttpContext);

                // Get the name of the Item to display confirmation
                itemName = market.CartItems
                    .Single(item => item.id == id).Item.Name;

                // Remove from cart
                cart.RemoveFromCart(id);
            }
            catch (Exception ex)
            {
                ErrorMessage.ErrorCode = ErrorCode.UNKNOWN;
                return View("ErrorMessage", ErrorMessage);
            }

            if (cart.GetCartItems().Count == 0)
            {
                msg = "Your Rental Cart is Empty.";
            }
            else
            {
                msg = Server.HtmlEncode(itemName) +
                    " has been removed from your shopping cart.";
            }
            // Display the confirmation message
            var results = new RentalCartRemoveViewModel
            {
                Message = msg,
                CartTotal = cart.GetTotal(),
                DeleteId = id,
                CartCount = cart.GetCartItems().Count
            };
            Session["RentalCartItems"] = results.CartCount;
            return Json(results);
        }
        
        //
        // GET: /RentalCart/Edit/5
 
        public ActionResult Edit(int id)
        {
            return View();
        }

        //
        // POST: /RentalCart/Edit/5

        [HttpPost]
        public ActionResult EditCartItem(RentalCart CartModel, int id, string pickupdate, string dropoffdate)
        {
            try
            {
                DateTime pickupDate = DateTime.Parse(pickupdate);
                DateTime dropoffDate = DateTime.Parse(dropoffdate);

                // TODO: Add update logic here
                RentalCart cart = null;
                try
                {
                    cart = RentalCart.GetCart(this.HttpContext);
                }
                catch
                {
                    ErrorMessage.ErrorCode = ErrorCode.UNKNOWN;
                    return View("ErrorMessage", ErrorMessage);
                }

                if (pickupDate < DateTime.Today || dropoffDate < DateTime.Today)
                {
                    var results = new RentalCartUpdateViewModel
                    {
                        Error = true,
                        Message = "Please enter valid dates"
                    };

                    return Json(results);
                }

                int rentalPeriod = (dropoffDate - pickupDate).Days;
                if (rentalPeriod <= 0)
                {
                    var results = new RentalCartUpdateViewModel
                    {
                        Error = true,
                        Message = "Your dropoff date cannot be earlier than or the same as your pickup date"
                    };

                    return Json(results);
                }

                var cartItem = market.CartItems.Single(i => i.id == id);

                foreach (DateTime d in GetBlockedDates(cartItem.ItemId))
                {
                    if (pickupDate.Date <= d.Date && dropoffDate.Date >= d.Date)
                    {
                        var results = new RentalCartUpdateViewModel
                        {
                            Error = true,
                            Message = "Your proposed rental period includes blocked dates. Please select different dates and try again."
                        };

                        return Json(results);
                    }

                    if (pickupDate.AddDays(1).Date == d.Date)
                    {
                        var results = new RentalCartUpdateViewModel
                        {
                            Error = true,
                            Message = "Your pickup date has to be atlest two days before a blocked date."
                        };

                        return Json(results);
                    }
                }

                // Get the name of the Item to display confirmation
                string itemName = cartItem.Item.Name;
                decimal itemCost = cartItem.Item.CostPerDay;

                if (cart.EditInCart(id, DateTime.Parse(pickupdate),
                    DateTime.Parse(dropoffdate)))
                {
                    var result = new RentalCartUpdateViewModel
                    {
                        Error = false,
                        CartTotal = cart.GetTotal(),
                        UpdatedItemTotal = (rentalPeriod * itemCost) + cartItem.Item.SecurityDeposit,
                        UpdatedId = id,
                        Message = "",
                        NumberOfDays = rentalPeriod
                    };

                    return Json(result);
                }
                else
                {
                    var results = new RentalCartUpdateViewModel
                    {
                        Error = true,
                        Message = "There already exists an item in your rental cart during the same period"
                    };
                    ModelState.AddModelError("", "There already exists an item in your rental cart during the same period");
                    return Json(results);
                }
            }
            catch (Exception ex)
            {
                /*
                ErrorMessage.ErrorCode = ErrorCode.UNKNOWN;
                return View("ErrorMessage", ErrorMessage);
                 */
                var results = new RentalCartUpdateViewModel
                {
                    Error = true,
                    Message = "Something went wrong on our side."
                };
                ModelState.AddModelError("", "There already exists an item in your rental cart during the same period");
                return Json(results);
            }
        }

        private List<DateTime> GetBlockedDates(string id)
        {
            List<DateTime> dates = new List<DateTime>();

            var item = market.Items.Find(id);

            foreach (BlackoutDate bd in item.BlackoutDates)
            {
                dates.Add(bd.Date.Date);
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
                    dates.Add(date.AddDays(i).Date);
                }
            }

            return dates;
        }
    }
}

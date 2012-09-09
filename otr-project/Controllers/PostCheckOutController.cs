using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using otr_project.Models;
using otr_project.ViewModels;
using otr_project.Utils;
using System.Net.Mail;

namespace otr_project.Controllers
{
    public class PostCheckOutController : Controller
    {
        MarketPlaceEntities market = new MarketPlaceEntities();
        ErrorMessageViewModel ErrorMessage = new ErrorMessageViewModel();

        // **************************************
        // URL: /PostCheckOut/ProcessSecurityCode
        // **************************************
        [Authorize]
        public ActionResult ProcessSecurityCode(string order, string item)
        {
            var getOrder = market.Orders.SingleOrDefault(o => o.OrderId == order);
            
            if (getOrder == null)
            {
                ErrorMessage.ErrorCode = ErrorCode.UNKNOWN;
                return View("ErrorMessage", ErrorMessage);
            }

            var orderDetail = getOrder.OrderDetails.SingleOrDefault(m => m.OrderDetailId.ToString() == item);

            if (orderDetail == null)
            {
                ErrorMessage.ErrorCode = ErrorCode.UNKNOWN;
                return View("ErrorMessage", ErrorMessage);
            }

            else if (orderDetail.Status == (int) OrderStatus.ORDER_COMPLETE)
                return RedirectToAction("RentalAlreadyComplete", new { Order = getOrder.OrderId, Item = orderDetail.OrderDetailId });

            var result = new ProcessSecurityCodeViewModel
            {
                Order = getOrder,
                OrderDetail = orderDetail,
            };

            if (User.Identity.Name == getOrder.User.Email)
            {
                result.IsOwner = false;
            }
            else if (User.Identity.Name == orderDetail.Item.Owner.Email)
            {
                result.IsOwner = true;
            }
            return View(result);
        }
               
        [HttpPost]
        [Authorize]
        public ActionResult ProcessSecurityCode(string order, string item, ProcessSecurityCodeViewModel UserCode)
        {
            var getOrder = market.Orders.SingleOrDefault(o => o.OrderId == order);
            
            if (getOrder == null)
            {
                ErrorMessage.ErrorCode = ErrorCode.UNKNOWN;
                return View("ErrorMessage", ErrorMessage);
            }
            
            var orderDetail = getOrder.OrderDetails.SingleOrDefault(m => m.OrderDetailId.ToString() == item);

            if (orderDetail == null)
            {
                ErrorMessage.ErrorCode = ErrorCode.UNKNOWN;
                return View("ErrorMessage", ErrorMessage);
            }

            var result = new ProcessSecurityCodeViewModel
            {
                Order = getOrder,
                OrderDetail = orderDetail
            };

            var changedOrder = orderDetail;

            try
            {
                if (User.Identity.Name == changedOrder.Item.Owner.Email)
                {
                    if (UserCode.SecurityCode == changedOrder.RenterCode)
                    {
                        changedOrder.Status = (int)OrderStatus.ORDER_COMPLETE;
                        market.Entry(changedOrder).Property(o => o.Status).IsModified = true;
						market.SaveChanges();
						PayOwner(changedOrder);
                        return RedirectToAction("SecurityCodeCorrect", new { Order = getOrder.OrderId, Item = orderDetail.OrderDetailId });
                        //return Redirect("/PostCheckOut/SecurityCodeCorrect/?Order=" + getOrder.OrderId + "&Item=" + orderDetail.ToList()[0].OrderDetailId);
                    }
                    else
                    {
                        ModelState.AddModelError("", "The Security Code you entered is not valid. Please enter the correct Security Code");
                        return View(result);
                    }
                }
                else if (User.Identity.Name == getOrder.User.Email)
                {
                    if (UserCode.SecurityCode == orderDetail.OwnerCode)
                    {
                        changedOrder.Status = (int)OrderStatus.ORDER_COMPLETE;
                        market.Entry(changedOrder).Property(o => o.Status).IsModified = true;
                        market.SaveChanges();
						PayOwner(changedOrder);
                        return RedirectToAction("SecurityCodeCorrect", new { Order = getOrder.OrderId, Item = orderDetail.OrderDetailId });
                    }
                    else
                    {
                        ModelState.AddModelError("", "The Security Code you entered is not valid. Please enter the correct Security Code");
                        return View(result);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage.ErrorCode = ErrorCode.UNKNOWN;
                return View("ErrorMessage", ErrorMessage);
            }
            
            return View(result);
         }

        private void PayOwner(OrderDetailModel order)
        {
            var owner = market.Users.SingleOrDefault(u => u.Email == order.Item.UserModelEmail);
            if (owner != null)
            {
                owner.Earnings += order.UnitPrice * order.NumberOfDays;
                market.Entry(owner).Property(o => o.Earnings).IsModified = true;
                market.SaveChanges();
            }
            //We cant find the owner of the item that is just rented. Something fucked up happened.
        }

        public ActionResult RentalAlreadyComplete(string order, string item)
        {
            var getOrder = market.Orders.Find(order);
            if (getOrder == null)
            {
                ErrorMessage.ErrorCode = ErrorCode.UNKNOWN;
                return View("ErrorMessage", ErrorMessage);
            }
            var orderDetail = getOrder.OrderDetails.SingleOrDefault(m => m.OrderDetailId.ToString() == item);
            if (orderDetail == null)
            {
                ErrorMessage.ErrorCode = ErrorCode.UNKNOWN;
                return View("ErrorMessage", ErrorMessage);
            }

            var result = new ProcessSecurityCodeViewModel
            {
                Order = getOrder,
                OrderDetail = orderDetail,
            };

            return View(result);
        }

        public ActionResult SecurityCodeCorrect(string order, string item)
        {
            var getOrder = market.Orders.Find(order);
            if (getOrder == null)
            {
                ErrorMessage.ErrorCode = ErrorCode.UNKNOWN;
                return View("ErrorMessage", ErrorMessage);
            }

            var orderDetail = getOrder.OrderDetails.SingleOrDefault(m => m.OrderDetailId.ToString() == item);

            if (orderDetail == null)
            {
                ErrorMessage.ErrorCode = ErrorCode.UNKNOWN;
                return View("ErrorMessage", ErrorMessage);
            }

            var result = new ProcessSecurityCodeViewModel
            {
                Order = getOrder,
                OrderDetail = orderDetail,
            };

            if (User.Identity.Name == getOrder.User.Email)
            {
                result.IsOwner = false;
            }
            else if (User.Identity.Name == orderDetail.Item.Owner.Email)
            {
                result.IsOwner = true;
            }

            return View(result); 
        }

        [Authorize]
        public ActionResult InitiateDispute(string order, string item)
        {
            var getOrder = market.Orders.SingleOrDefault(o => o.OrderId == order);

            if (getOrder == null)
            {
                ErrorMessage.ErrorCode = ErrorCode.UNKNOWN;
                return View("ErrorMessage", ErrorMessage);
            }

            var orderDetail = getOrder.OrderDetails.SingleOrDefault(m => m.OrderDetailId.ToString() == item && m.Item.UserModelEmail == User.Identity.Name);

            if (orderDetail == null)
            {
                ErrorMessage.ErrorCode = ErrorCode.UNKNOWN;
                return View("ErrorMessage", ErrorMessage);
            }

            if (orderDetail.Status == (int)OrderStatus.ORDER_DELAYED_RETURN || orderDetail.Status == (int)OrderStatus.ORDER_COMPLETE)
            {
                // Changing orderstatus to Dispute
                orderDetail.Status = (int)OrderStatus.ORDER_DISPUTE;
                market.Entry(orderDetail).State = System.Data.EntityState.Modified;
                market.SaveChanges();

                // Marking item as inactive on RAMBLA
                orderDetail.Item.isActive = false;
                market.Entry(orderDetail.Item).State = System.Data.EntityState.Modified;
                market.SaveChanges();
            }


            // Todo: redirect user to a nice info page assuring them that the dispute is in process.
            return RedirectToAction("Index", "Home");
        }
    }
}


using System;
using System.Collections.Generic;
using System.Web.Routing;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using otr_project.Models;
using otr_project.ViewModels;
using otr_project.Utils;
using System.Net.Mail;
using otr_project.Mailers;
using Mvc.Mailer;

namespace otr_project.Controllers
{
    public class CheckOutController : Controller
    {
        MarketPlaceEntities market = new MarketPlaceEntities();
        private IOrderMailer _orderMailer = new OrderMailer();

        public IOrderMailer OrderMailer
        {
            get { return _orderMailer; }
            set { _orderMailer = value; }
        }
        //
        // GET: /CheckOut/PayPalCheckOut

        [Authorize]
        public ActionResult PayPalCheckOut()
        {
            var cart = RentalCart.GetCart(this.HttpContext);
            string retMsg = "";
            string token = "";

            if (cart.GetCartItems().Count == 0)
            {
                return RedirectToAction("ErrorMessage", "Checkout", new { ErrorCode = ErrorCode.EMPTY_CART });
            }

            foreach (CartItemModel c in cart.GetCartItems())
            {
                if (c.Item.Owner.Email == User.Identity.Name)
                {
                    return RedirectToAction("ErrorMessage", "Checkout", new { ErrorCode = ErrorCode.OWNER_ITEM_IN_CART});
                }
            }

            NVPAPICaller checkout = new NVPAPICaller(complete: Url.Action("Complete", "CheckOut", null, Request.Url.Scheme, Request.Url.Host), cancel: Url.Action("Cancel", "CheckOut", null, Request.Url.Scheme, Request.Url.Host));
            
            var order = new Order()
            {
                OrderId = System.Guid.NewGuid().ToString(),
                OrderDate = DateTime.Now,
                User = market.Users.Find(User.Identity.Name),
                Confirmed = false
            };

            market.Orders.Add(order);
            market.SaveChanges();
            cart.CreateOrder(order);

            string amt = order.Total.ToString();
            bool ret = checkout.ShortcutExpressCheckout(amt, checkout.GetNVPFromOrder(order), ref token, ref retMsg);
            if (ret)
            {
                order.PayPalToken = token;
                market.Entry(order).State = System.Data.EntityState.Modified;
                market.SaveChanges();
                return Redirect(retMsg);
            }
            else
            {
                return Redirect(Url.Action("ErrorMessage", "Checkout") + retMsg + "&Order=" + order.OrderId);
            }
        }

        //
        // GET: /CheckOut/ErrorMessage
        public ActionResult ErrorMessage(ErrorCode ErrorCode, string Header, string Description, string Order)
        {
            ErrorMessageViewModel ErrorMessage = new ErrorMessageViewModel();
            var order = market.Orders.SingleOrDefault(o=>o.OrderId == Order);
            
            if (order != null)
            {
                var orderItems = market.OrderDetails.Where(o => o.OrderId == Order).ToList();

                if (market.Orders.Find(Order).User.Email == User.Identity.Name)
                {
                    for (int i=0; i< orderItems.Count; i++)
                    {
                        market.OrderDetails.Remove(orderItems[i]);
                    }
                    
                    market.Orders.Remove(order);
                    market.SaveChanges();
                }
            }

            ErrorMessage.ErrorCode = ErrorCode;
            if (Description != null)
            {
                ErrorMessage.Description = Description;
            }
            else
            {
                ErrorMessage.Description = ErrorMessage.Descriptions[(int)ErrorCode];
            }
            if (Header != null)
            {
                ErrorMessage.Header = Header;
            }
            else
            {
                ErrorMessage.Header = ErrorMessage.Headers[(int)ErrorCode];
            }

            return View(ErrorMessage);
        }

        public ActionResult Cancel(string token)
        {
            ErrorMessageViewModel ErrorMessage = new ErrorMessageViewModel();
            var order = market.Orders.SingleOrDefault(o => o.PayPalToken == token);
            
            if (order != null)
            {
                var orderItems = market.OrderDetails.Where(o => o.OrderId == order.OrderId).ToList();

                if (order.User.Email == User.Identity.Name)
                {
                    for (int i = 0; i < orderItems.Count; i++)
                    {
                        market.OrderDetails.Remove(orderItems[i]);
                    }

                    market.Orders.Remove(order);
                    market.SaveChanges();
                }
            }
            return RedirectToAction("Index", "RentalCart");
        }

        //
        // GET: /CheckOut/Complete
        [Authorize]
        public ActionResult Complete(string token, string PayerID)
        {
            if (string.IsNullOrEmpty(token) && string.IsNullOrEmpty(PayerID))
                //Need to specify error handling
                return RedirectToAction("ErrorMessage", "Checkout", new { ErrorCode = ErrorCode.PAYPAL_ERROR });

            var order = market.Orders.SingleOrDefault(o => o.PayPalToken == token);
            var decoder = new NVPCodec();
            string retMsg = "";

            // This is where we call DoExpressCheckoutPayment
            NVPAPICaller completeCheckout = new NVPAPICaller(token: token, PayerID: PayerID, total: order.Total.ToString());
            bool ret = completeCheckout.ConfirmPayment(order.Total.ToString(), ref decoder, ref retMsg);

            if (ret)
            {
                var cart = RentalCart.GetCart(this.HttpContext);
                cart.EmptyCart();
                Session["RentalCartItems"] = 0;


                order.Confirmed = true;
                market.Entry(order).State = System.Data.EntityState.Modified;
                market.SaveChanges();

                var orderDetails = market.OrderDetails.Where(o => o.OrderId == order.OrderId).ToList();

                if (order != null)
                {
                    foreach (OrderDetailModel o in orderDetails)
                    {
                        if (EmailSecurityCode(User.Identity.Name, o.OrderDetailId, false))
                        { }
                        //Need to specify error handling
                        else return RedirectToAction("ErrorMessage", "Checkout", new { ErrorCode = ErrorCode.UNKNOWN });
                        if (EmailSecurityCode(market.OrderDetails.Find(o.OrderDetailId).Item.Owner.Email, o.OrderDetailId, true))
                        { }
                        //Need to specify error handling
                        else return RedirectToAction("ErrorMessage", "Checkout", new { ErrorCode = ErrorCode.UNKNOWN });
                        o.Status = (int)OrderStatus.ORDER_TENTATIVE;
                        market.Entry(o).State = System.Data.EntityState.Modified;
                        market.SaveChanges();
                    }
                    return View(order.OrderDetails);
                }
                return RedirectToAction("ErrorMessage", "Checkout", new { ErrorCode = ErrorCode.UNKNOWN });
            }
            else
            {
                //PayPal payment didn't go through
                return Redirect(Url.Content("~/CheckOut/ErrorMessage") + retMsg);
            }
            //Need to specify error handling
            return RedirectToAction("ErrorMessage", "Checkout", new { ErrorCode = ErrorCode.UNKNOWN });
        }

        private bool EmailSecurityCode(string email, int orderDetailId, Boolean IsOwner)
        {
            var order = market.OrderDetails.Find(orderDetailId);
            
            if (IsOwner == false)
            {
                try
                {
                    var RenterSecurityCode = GenerateSecurityCode(orderDetailId);
                    order.RenterCode = RenterSecurityCode;
                    market.SaveChanges();
                    OrderMailer.SendRenterCode(toEmail: User.Identity.Name, orderDeatil: order).Send();
                }
                catch (Exception ex)
                {
                    //Need to specify error handling
                    return false;
                }               
            }
            if (IsOwner == true)
            {
                try
                {
                    var OwnerSecurityCode = GenerateSecurityCode(orderDetailId);
                    order.OwnerCode = OwnerSecurityCode;
                    market.SaveChanges();
                    OrderMailer.SendOwnerCode(toEmail: order.Item.Owner.Email, orderDeatil: order).Send();
                }
                catch (Exception ex)
                {
                    //Need to specify error handling
                    return false;
                }                
            }
            return true;
        }

        //Helper method to generate security confirmation code
        private string GenerateSecurityCode(int OrderDetailId)
        {
            Random code = new Random();
            int SecurityCode = code.Next();
            return SecurityCode.ToString();
        }
    }
}

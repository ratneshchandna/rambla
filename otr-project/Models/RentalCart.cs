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
using Mvc.Mailer;
using otr_project.Mailers;
using System.Data.Entity;

namespace otr_project.Models
{
    public partial class RentalCart
    {
        MarketPlaceEntities market = new MarketPlaceEntities();

        string RentalCartId { get; set; }

        public const string CartSessionKey = "CartId";

        public static RentalCart GetCart(HttpContextBase context)
        {
            var cart = new RentalCart();
            cart.RentalCartId = cart.GetCartId(context);
            return cart;
        }

        // Helper method to simplify shopping cart calls
        public static RentalCart GetCart(Controller controller)
        {
            return GetCart(controller.HttpContext);
        }

        public Boolean AddToCart(ItemModel item, DateTime pickup, DateTime dropoff)
        {
            // Get the matching cart and item instances for the same picup and dropff dates
            var cartItem = market.CartItems.SingleOrDefault(
c => c.CartId == RentalCartId
&& c.ItemId == item.Id);

            if (cartItem == null || ((cartItem.PickupDate.Date != pickup.Date && cartItem.DropoffDate.Date != dropoff.Date)))
            {
                if (cartItem != null)
                {
                    if ((cartItem.PickupDate.Date <= pickup.Date && (DateTime)cartItem.DropoffDate.Date >= pickup.Date) || (cartItem.PickupDate.Date <= dropoff.Date && cartItem.DropoffDate.Date >= dropoff.Date))
                    {
                        return false;
                    }
                    else if (pickup.Date <= cartItem.PickupDate.Date && dropoff.Date >= cartItem.DropoffDate.Date)
                    {
                        return false;
                    }
                    else
                    {
                        // Create a new cart item if no cart item exists
                        cartItem = new CartItemModel
                        {
                            ItemId = item.Id,
                            CartId = RentalCartId,
                            PickupDate = pickup,
                            DropoffDate = dropoff,
                            itemTotal = ((item.CostPerDay) * (dropoff - pickup).Days),
                            DateCreated = DateTime.Now
                        };
                        market.CartItems.Add(cartItem);
                        market.SaveChanges();
                        return true;
                    }
                }
                else
                {
                    // Create a new cart item if no cart item exists
                    cartItem = new CartItemModel
                    {
                        ItemId = item.Id,
                        CartId = RentalCartId,
                        PickupDate = pickup,
                        DropoffDate = dropoff,
                        itemTotal = ((item.CostPerDay) * (dropoff - pickup).Days) + item.SecurityDeposit,
                        DateCreated = DateTime.Now
                    };
                    market.CartItems.Add(cartItem);
                    market.SaveChanges();
                    return true;
                }
            }
            else
            {
                // If the item does exist in the cart, then add we check if the dates are the same.
                return false;
            }
        }

        public Boolean EditInCart(int id, DateTime pickup, DateTime dropoff)
        { 
            var cartItem = market.CartItems.Single(cart => cart.CartId == RentalCartId && cart.id == id);
            var Same_Name_Item = market.CartItems.Where(cart => cart.CartId == RentalCartId && cart.Item.Name == cartItem.Item.Name && cart.id != id);

            for (int i = 0; i < (int)Same_Name_Item.Count(); i++)
            {
                if ((Same_Name_Item.ToList()[i].PickupDate.Date <= pickup.Date && Same_Name_Item.ToList()[i].DropoffDate.Date >= pickup.Date) || (Same_Name_Item.ToList()[i].PickupDate.Date <= dropoff.Date && Same_Name_Item.ToList()[i].DropoffDate.Date >= dropoff.Date))
                {
                    return false;
                }
                else if (pickup.Date <= Same_Name_Item.ToList()[i].PickupDate.Date && dropoff.Date >= Same_Name_Item.ToList()[i].DropoffDate.Date)
                {
                    return false;
                }
            }

            //If we get this far, then this is a valid edit
            market.Entry(cartItem).State = System.Data.EntityState.Modified;

            if (cartItem != null)
            {
                cartItem.PickupDate = pickup;
                cartItem.DropoffDate = dropoff;
                cartItem.itemTotal = ((dropoff - pickup).Days * cartItem.Item.CostPerDay) + cartItem.Item.SecurityDeposit;
            }
            market.SaveChanges();
            return true;
        }

        public void RemoveFromCart(int id)
        {
            // Get the cart
            var cartItem = market.CartItems.Single(cart => cart.CartId == RentalCartId && cart.id == id);

            //int itemCount = 0;

            if (cartItem != null)
            {
                /*
                if (cartItem.Count > 1)
                {
                    cartItem.Count--;
                    itemCount = cartItem.Count;
                }
                else
                {*/
                    market.CartItems.Remove(cartItem);
                //}
                
                // Save changes
                market.SaveChanges();
            }

            //return itemCount;
        }

        public void EmptyCart()
        {
            var cartItems = market.CartItems.Where(cart => cart.CartId == RentalCartId);

            foreach (var cartItem in cartItems)
            {
                market.CartItems.Remove(cartItem);
            }

            // Save changes
            market.SaveChanges();
        }

        public List<CartItemModel> GetCartItems()
        {
            return market.CartItems.Where(cart => cart.CartId == RentalCartId).ToList();
        }

        /*
        public int GetCount()
        {
            // Get the total number of items in our Rental cart
            int? count = market.CartItems.Count();
                
            // Return 0 if all entries are null
            return count ?? 0;
        }
        */

        public decimal GetTotal()
        {
            // Multiply item cost per day plus the security deposit by the number of days
            // the current price for each of those albums in the cart
            // sum all album price totals to get the cart total
            var total = (from cartItems in market.CartItems
                              where cartItems.CartId == RentalCartId
                              select cartItems.itemTotal).ToList();

            if (total == null)
                return decimal.Zero;

            return total.Sum();
        }

        
        public string CreateOrder(Order order)
        {
            decimal orderTotal = 0;

            var cartItems = GetCartItems();

            // Iterate over the items in the cart, adding the order details for each
            foreach (var item in cartItems)
            {
                var orderDetail = new OrderDetailModel
                {
                    ItemModelId = item.ItemId,
                    ItemName = item.Item.Name,
                    ItemDesc = item.Item.Desc,
                    CategoryId = item.Item.CategoryId,
                    //ItemImageFileModelId = (item.Item.ItemImages.Count != 0) ? item.Item.ItemImages.ToList()[0].Id : null,
                    OrderId = order.OrderId,
                    UnitPrice = item.Item.CostPerDay,
                    SecurityDeposit = item.Item.SecurityDeposit,
                    PickupDate = item.PickupDate,
                    DrofoffDate = item.DropoffDate,
                    Status = null,
                    NumberOfDays = (item.DropoffDate.Date - item.PickupDate.Date).Days
                };

                // Set the order total of the shopping cart
                orderTotal += (orderDetail.UnitPrice * orderDetail.NumberOfDays) + orderDetail.SecurityDeposit;

                market.OrderDetails.Add(orderDetail);
                //order.OrderDetails.Add(orderDetail);
            }

            // Set the order's total to the orderTotal count
            order.Total = orderTotal;

            // Save the order
            market.SaveChanges();

            // Return the OrderId as the confirmation number
            return order.OrderId;
        }
        
        
        // We're using HttpContextBase to allow access to cookies.
        public string GetCartId(HttpContextBase context)
        {
            if (context.Session[CartSessionKey] == null)
            {
                if (!string.IsNullOrWhiteSpace(context.User.Identity.Name))
                {
                    context.Session[CartSessionKey] = context.User.Identity.Name;
                }
                else
                {
                    // Generate a new random GUID using System.Guid class
                    Guid tempCartId = Guid.NewGuid();

                    // Send tempCartId back to client as a cookie
                    context.Session[CartSessionKey] = tempCartId.ToString();
                }
            }

            return context.Session[CartSessionKey].ToString();
        }

        // When a user has logged in, migrate their shopping cart to
        // be associated with their username
        public void MigrateCart(string userName)
        {
            var rentalCart = market.CartItems.Where(c => c.CartId == RentalCartId);

            foreach (CartItemModel item in rentalCart)
            {
                item.CartId = userName;
            }
            market.SaveChanges();
        }
    }
}
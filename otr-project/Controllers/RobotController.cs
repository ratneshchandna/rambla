using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Mvc.Mailer;
using otr_project.Mailers;
using otr_project.Models;
using log4net;

namespace otr_project.Controllers
{
    public class RobotController : Controller
    {
        //
        // GET: /Robot/
        private MarketPlaceEntities market = new MarketPlaceEntities();
        private INotifyMailer _notifyMailer = new NotifyMailer();
        private static readonly ILog log = LogManager.GetLogger("otr_project.MvcApplication.Controllers");

        public INotifyMailer NotifyMailer
        {
            get { return _notifyMailer; }
            set { _notifyMailer = value; }
        }

        private enum EmailType
        {
            PICKUP_REMINDER = 0,
            RETURN_REMINDER = 1,
            ORDER_TERMINATED = 2,
            ORDER_CLOSED = 3
        }

        public ActionResult Index()
        {
            // Iterate through all the order details in the DB
            IList<OrderDetailModel> OrderList = market.OrderDetails.ToList();

            foreach (OrderDetailModel order in OrderList)
            {
                if (order.Status == null)
                {
                    market.OrderDetails.Remove(order);
                    market.SaveChanges();
                    log.Info("Order for " + order.ItemName + " removed (ID: " + order.OrderDetailId + ")");
                    continue;
                }

                if (order.Status == (int)OrderStatus.ORDER_TERMINATED ||
                    order.Status == (int)OrderStatus.ORDER_DISPUTE ||
                    order.Status == (int)OrderStatus.ORDER_CLOSED_HAPPY ||
                    order.Status == (int)OrderStatus.ORDER_CLOSED_UNHAPPY)
                {
                    continue;
                }

                else
                {
                    switch (order.Status)
                    {
                        case (int) OrderStatus.ORDER_TENTATIVE:
                            // Check if today is pickup date. If so send reminder
                            if (order.PickupDate.Date.Equals(DateTime.Today.Date))
                            {
                                // Send reminder email with links to complete the order
                                SendNotificationEmail(EmailType.PICKUP_REMINDER, true, order);
                                log.Info("Pickup reminder sent to owner for ordered item " + order.ItemName + " (ID: " + order.OrderDetailId + "; Status: " + order.Status.ToString() + ")");
                                SendNotificationEmail(EmailType.PICKUP_REMINDER, false, order);
                                log.Info("Pickup reminder sent to borrower for ordered item " + order.ItemName + " (ID: " + order.OrderDetailId + "; Status: " + order.Status.ToString() + ")");
                                break;
                            }
                            if ((DateTime.Today.Date - order.PickupDate.Date).Days > 0)
                            {
                                // Mark the order as Late
                                order.Status = (int) OrderStatus.ORDER_LATE;
                                market.Entry(order).State = System.Data.EntityState.Modified;
                                market.SaveChanges();
                                log.Info("Order for " + order.ItemName + " (ID: " + order.OrderDetailId + "; Status: " + order.Status.ToString() + ")");
                                break;
                            }
                            break;

                        case (int) OrderStatus.ORDER_LATE:
                            if ((DateTime.Today.Date - order.PickupDate.Date).Days > 1)
                            {
                                // Mark the order as Terminated
                                order.Status = (int) OrderStatus.ORDER_TERMINATED;
                                market.Entry(order).State = System.Data.EntityState.Modified;
                                market.SaveChanges();

                                // Send email to owner and renter informing them of the news
                                SendNotificationEmail(EmailType.ORDER_TERMINATED, true, order);
                                log.Info("Order Terminated Notification sent to owner for ordered item " + order.ItemName + " (ID: " + order.OrderDetailId + "; Status: " + order.Status.ToString() + ")");
                                SendNotificationEmail(EmailType.ORDER_TERMINATED, false, order);
                                log.Info("Order Terminated Notification sent to borrower for ordered item " + order.ItemName + " (ID: " + order.OrderDetailId + "; Status: " + order.Status.ToString() + ")");
                                
                                // Todo: Refund the renter for the full amount
                                break;
                            }
                            break;

                        case (int) OrderStatus.ORDER_COMPLETE:
                            if (order.DrofoffDate.Date.Equals(DateTime.Today.Date))
                            {
                                // Todo: Send reminder email with links to Owner to close the order and indicate receipt of items
                                SendNotificationEmail(EmailType.RETURN_REMINDER, true, order);
                                log.Info("Return reminder sent to owner for ordered item " + order.ItemName + " (ID: " + order.OrderDetailId + "; Status: " + order.Status.ToString() + ")");
                                SendNotificationEmail(EmailType.RETURN_REMINDER, false, order);
                                log.Info("Return reminder sent to borrower for ordered item " + order.ItemName + " (ID: " + order.OrderDetailId + "; Status: " + order.Status.ToString() + ")");
                                break;
                            }
                            if ((DateTime.Today.Date - order.DrofoffDate.Date).Days > 0)
                            {
                                // Mark the order as Delayed_Return
                                order.Status = (int) OrderStatus.ORDER_DELAYED_RETURN;
                                market.Entry(order).State = System.Data.EntityState.Modified;
                                market.SaveChanges();
                                log.Info("Order for " + order.ItemName + " (ID: " + order.OrderDetailId + "; Status: " + order.Status.ToString() + ")");
                                break;
                            }
                            break;

                        case (int) OrderStatus.ORDER_DELAYED_RETURN:
                            if ((DateTime.Today.Date - order.DrofoffDate.Date).Days > 1)
                            {
                                // Mark the order as Close_Happy. No action from Owner means the transaction went fine.
                                order.Status = (int) OrderStatus.ORDER_CLOSED_HAPPY;
                                market.Entry(order).State = System.Data.EntityState.Modified;
                                market.SaveChanges();
                                log.Info("Order for " + order.ItemName + " (ID: " + order.OrderDetailId + "; Status: " + order.Status.ToString() + ")");

                                // Send thank you email to owner and renter informing them that order was successful.
                                SendNotificationEmail(EmailType.ORDER_CLOSED, true, order);
                                log.Info("Order closed notification sent to owner for ordered item " + order.ItemName + " (ID: " + order.OrderDetailId + "; Status: " + order.Status.ToString() + ")");
                                SendNotificationEmail(EmailType.ORDER_CLOSED, false, order);
                                log.Info("Order closed notification sent to borrower for ordered item " + order.ItemName + " (ID: " + order.OrderDetailId + "; Status: " + order.Status.ToString() + ")");

                                break;
                            }
                            break;

                        default:
                            break;
                    }
                }
            }
            return RedirectToAction("Index", "Home");
        }

        private void SendNotificationEmail(EmailType type, bool isOwner, OrderDetailModel order)
        {
            switch (type)
            {
                case EmailType.PICKUP_REMINDER:
                    if (isOwner)
                    {
                        NotifyMailer.OwnerPickupReminder(order.Item.Owner.Email, order).Send();
                        break;
                    }
                    else
                    {
                        NotifyMailer.RenterPickupReminder(order.Order.User.Email, order).Send();
                        break;
                    }

                case EmailType.RETURN_REMINDER:
                    if (isOwner)
                    {
                        NotifyMailer.OwnerReturnReminder(order.Item.Owner.Email, order).Send();
                        break;
                    }
                    else
                    {
                        NotifyMailer.RenterReturnReminder(order.Order.User.Email, order).Send();
                        break;
                    }

                case EmailType.ORDER_TERMINATED:
                    if (isOwner)
                    {
                        NotifyMailer.OwnerOrderTerminated(order.Item.Owner.Email, order).Send();
                        break;
                    }
                    else
                    {
                        NotifyMailer.RenterOrderTerminated(order.Order.User.Email, order).Send();
                        break;
                    }

                case EmailType.ORDER_CLOSED:
                    if (isOwner)
                    {
                        NotifyMailer.OwnerOrderClosed(order.Item.Owner.Email, order).Send();
                        break;
                    }
                    else
                    {
                        NotifyMailer.RenterOrderClosed(order.Order.User.Email, order).Send();
                        break;
                    }

                default:
                    break;
            }
        }
    }
}

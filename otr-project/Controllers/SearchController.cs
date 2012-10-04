using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using otr_project.Models;
using otr_project.ViewModels;
using otr_project.Utils;
using System.Data.Entity;
using System.Text.RegularExpressions;
using System.Globalization;
using Webdiyer.WebControls.Mvc;

namespace otr_project.Controllers
{
    public class SearchController : Controller
    {
        const int ITEMS_PER_PAGE = 12;
        MarketPlaceEntities market = new MarketPlaceEntities();
        ErrorMessageViewModel ErrorMessage = new ErrorMessageViewModel();

        //
        // GET: /Search/
        public ActionResult Index(string search, int? id, FormCollection collection)
        {
            try
            {
                List<SearchItemViewModel> items = new List<SearchItemViewModel>();
                int category = 0;

                foreach (ItemModel i in market.Items)
                {
                    // If item is marked as inactive we shouldn't display it.
                    if (!i.isActive)
                        continue;

                    items.Add(new SearchItemViewModel
                    {
                        Item = i,
                        isItemBlocked = false
                    });
                }

                //if (String.IsNullOrEmpty(collection.Get("SortType")) == false)
                if (Session["SEARCH_SORT_TYPE"] != null)
                {
                    if (String.IsNullOrEmpty(collection.Get("SortType")) == false)
                    {
                        Session["SEARCH_SORT_TYPE"] = collection.Get("SortType");
                    }

                    if (Session["SEARCH_SORT_TYPE"].ToString() == "2")
                    {
                        items = items.OrderByDescending(i => i.Item.CostPerDay).ToList();
                    }
                    else if (Session["SEARCH_SORT_TYPE"].ToString() == "1")
                    {
                        items = items.OrderBy(i => i.Item.CostPerDay).ToList();
                    }
                }
                else
                {
                    Session["SEARCH_SORT_TYPE"] = "1";
                    items = items.OrderBy(i => i.Item.CostPerDay).ToList();
                }

                if (string.IsNullOrEmpty(search) == false)
                {
                    Session["SEARCH_KEYWORD"] = search;
                    items = items.Where(i => i.Item.Name.ToUpper().Contains(search.ToUpper())).ToList();
                    /* items = items.Where(i => i.Item.Name.ToUpper().Contains(search.ToUpper())
                        || i.Item.Desc.ToUpper().Contains(search.ToUpper())).ToList();*/
                }
                else if (Request.IsAjaxRequest() || search == null)
                {
                    if (Session["SEARCH_KEYWORD"] != null)
                    {
                        if (Session["SEARCH_KEYWORD"].ToString() != "")
                        {
                            items = items.Where(i => i.Item.Name.ToUpper().Contains(Session["SEARCH_KEYWORD"].ToString().ToUpper())
                                || i.Item.Desc.ToUpper().Contains(Session["SEARCH_KEYWORD"].ToString().ToUpper())).ToList();
                        }
                    }
                }
                else
                {
                    Session["SEARCH_KEYWORD"] = "";
                }

                if (collection.Get("price") != null)
                {
                    Session["SEARCH_PRICE"] = collection.Get("price").ToString().Trim('$');
                }
                if (Session["SEARCH_PRICE"] != null)
                {
                    if (Session["SEARCH_PRICE"].ToString() != "")
                    {
                        decimal price = decimal.Parse(Session["SEARCH_PRICE"].ToString());
                        if (price < 500)
                        {
                            items = items.Where(i => i.Item.CostPerDay <= price).ToList();
                        }
                    }

                }

                if (collection.Get("securitydeposit") != null)
                {
                    Session["SEARCH_SECURITY_DEPOSIT"] = collection.Get("securitydeposit").ToString().Trim('$');
                }
                if (Session["SEARCH_SECURITY_DEPOSIT"] != null)
                {
                    if (Session["SEARCH_SECURITY_DEPOSIT"].ToString() != "")
                    {
                        decimal security = decimal.Parse(Session["SEARCH_SECURITY_DEPOSIT"].ToString());
                        if (security < 500)
                        {
                            items = items.Where(i => i.Item.SecurityDeposit <= security).ToList();
                        }
                    }
                }

                DateTime pickup = DateTime.MinValue;
                DateTime dropoff = DateTime.MinValue;
                if (!String.IsNullOrEmpty(collection.Get("pickup")) && !String.IsNullOrEmpty(collection.Get("dropoff")))
                {
                    pickup = DateTime.Parse(collection.Get("pickup"));
                    dropoff = DateTime.Parse(collection.Get("dropoff"));
                    Session["SEARCH_PICKUP"] = pickup;
                    Session["SEARCH_DROPOFF"] = dropoff;
                }
                else if (Request.IsAjaxRequest())
                {
                    Session["SEARCH_PICKUP"] = "";
                    Session["SEARCH_DROPOFF"] = "";
                }
                else if (Session["SEARCH_PICKUP"] != null && Session["SEARCH_DROPOFF"] != null)
                {
                    if (Session["SEARCH_PICKUP"].ToString() != "" && Session["SEARCH_DROPOFF"].ToString() != "")
                    {
                        pickup = DateTime.Parse(Session["SEARCH_PICKUP"].ToString());
                        dropoff = DateTime.Parse(Session["SEARCH_DROPOFF"].ToString());
                    }
                }
                if (pickup != DateTime.MinValue && dropoff != DateTime.MinValue)
                {
                    foreach (SearchItemViewModel i in items)
                    {
                        foreach (BlackoutDate date in i.Item.BlackoutDates)
                        {
                            if (date.Date.Date >= pickup.Date && date.Date.Date <= dropoff.Date)
                            {
                                i.isItemBlocked = true;
                                break;
                            }
                        }

                        if (i.isItemBlocked == true)
                        {
                            continue;
                        }

                        foreach (OrderDetailModel o in i.Item.OrderDetails)
                        {
                            if (pickup.Date >= o.PickupDate.Date && pickup.Date <= o.DrofoffDate.Date)
                            {
                                i.isItemBlocked = true;
                                break;
                            }

                            if (dropoff.Date >= o.PickupDate.Date && dropoff.Date <= o.DrofoffDate.Date)
                            {
                                i.isItemBlocked = true;
                                break;
                            }
                        }
                    }
                }

                if (String.IsNullOrEmpty(collection.Get("requireImages")) == false)
                {
                    Session["SEARCH_REQUIRE_IMAGES"] = collection.Get("requireImages");
                }
                else
                {
                    if (Session["SEARCH_REQUIRE_IMAGES"] == null)
                    {
                        Session["SEARCH_REQUIRE_IMAGES"] = "off";
                    }
                }
                if (Session["SEARCH_REQUIRE_IMAGES"].ToString() == "on")
                {
                    items = items.Where(i => i.Item.ItemImages.Count > 0).ToList();
                }


                if (Session["SEARCH_CATEGORY_ID"] != null)
                {
                    if (String.IsNullOrEmpty(collection.Get("CategoryId")) == false)
                    {
                        Session["SEARCH_CATEGORY_ID"] = collection.Get("CategoryId");
                    }
                    else if (Request.IsAjaxRequest())
                    {
                        Session["SEARCH_CATEGORY_ID"] = "0";
                    }

                    if (Session["SEARCH_CATEGORY_ID"].ToString() != "0")
                    {
                        category = int.Parse(Session["SEARCH_CATEGORY_ID"].ToString());
                        items = items.Where(i => i.Item.CategoryId == category).ToList();
                    }
                }
                else
                {
                    Session["SEARCH_CATEGORY_ID"] = "0";
                }

                /*** Preparing the page to be returned, depending on the type of the request ***/
                var pageNumber = id ?? 1;
                var onePageOfItems = items.AsQueryable().ToPagedList(pageNumber, ITEMS_PER_PAGE);

                //int sorttype = !String.IsNullOrEmpty(collection.Get("SortType")) ? Int32.Parse(collection.Get("SortType")) : 1;
                int sorttype = Session["SEARCH_SORT_TYPE"] != null ? Int32.Parse(Session["SEARCH_SORT_TYPE"].ToString()) : 1;
                var sortTypes = new SelectList
                (
                    new List<SelectListItem>{
                        new SelectListItem { Text = "Price: Low to High", Value = "1"}, 
                        new SelectListItem { Text = "Price: High to Low", Value = "2"}
                        }
                    , "Value"
                    , "Text"
                    , sorttype
                );

                ViewBag.CategoryId = new SelectList(market.Categories, "Id", "Name", category);
                ViewBag.SortType = sortTypes;

                if (Request.IsAjaxRequest())
                    return PartialView("_PaginatedSearchResults", onePageOfItems);

                return View(onePageOfItems);
            }
            catch (Exception ex)
            {
                ErrorMessage.ErrorCode = ErrorCode.UNKNOWN;
                return View("ErrorMessage", ErrorMessage);
            }
        }
    }
}
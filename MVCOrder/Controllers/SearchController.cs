using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVCOrder.Models;
using MVCOrder.Views.Shared.Components.SearchBar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MVCOrder.Controllers
{
    public class SearchController : Controller
    {
        private readonly aspnetMVCOrderContext _context;

        public SearchController(aspnetMVCOrderContext context)
        {
            _context = context;
        }

        public IActionResult SearchOrders(string SearchText = "", int pg = 1)
        {
            DateTime dateTime;
            List<Order> orders;
            if (SearchText != "" && SearchText != null && DateTime.TryParse(SearchText, out dateTime))
            {
                orders = _context.Orders.Include(u => u.User).Include(s => s.Service)
                    .Where(p => p.OrderDateTime < dateTime)
                    .ToList();
            }
            else if (SearchText != "" && SearchText != null)
            {
                orders = _context.Orders.Include(u => u.User).Include(s => s.Service)
                    .Where(p => p.User.Email.Contains(SearchText))
                    .ToList();
            }
            else
                orders = _context.Orders.Include(u => u.User).Include(s => s.Service).ToList();

            const int pageSize = 10;
            if (pg < 1) pg = 1;

            int recsCount = orders.Count();
            int recSkip = (pg - 1) * pageSize;

            List<Order> retOrders = orders.Skip(recSkip).Take(pageSize).ToList();

            SPager SearchPager = new SPager(recsCount, pg, pageSize) { Action = "SearchOrders", Controller = "Search", SearchText = SearchText };

            ViewBag.SearchPager = SearchPager;

            return View(retOrders);
        }
    }
}

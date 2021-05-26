using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MVCOrder.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MVCOrder.ViewModels;
using System.Dynamic;

namespace MVCOrder.Controllers
{
    [Authorize(Roles = "Manager")]
    public class ManagerController : Controller
    {
        private static string tempUserId;
        private readonly aspnetMVCOrderContext _context;

        public ManagerController(aspnetMVCOrderContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult ListOrders()
        {
            var orders = _context.Orders.Include(u =>u.User).Include(s => s.Service);
            return View(orders);
        }

        [HttpGet]
        public IActionResult ListUsers()
        {
            var check = CheckLimit();
            if (check)
            {
                var users = _context.AspNetUsers;
                return View(users);
            }
            else
            {
                TempData["ErrorMessage"] = "You can not place an order, because there is a limit․";
                return RedirectToAction("GetLimit");
            }
        }

        [HttpGet]
        public IActionResult SelectService(string id)
        {
            tempUserId = id;
            var service = _context.Services;
            return View(service);
        }

        [HttpGet] 
        public IActionResult ListServices()
        {
            var service = _context.Services.Include(o => o.Orders);
            return View(service);
        }

        [HttpGet]
        public IActionResult CreateService()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateService(CreateServiceViewModel model)
        {
            if (ModelState.IsValid)
            {
                Service service = new Service
                {
                    Name = model.Name,
                    Price = model.Price
                };

                var result = _context.Services.Add(service);

                if (result.State == EntityState.Added)
                {
                    _context.SaveChanges();
                    return RedirectToAction("ListServices", "Manager");
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult EditService(int id)
        {
            var service = _context.Services.Find(id);

            if (service == null)
            {
                ViewBag.ErrorMessage = $"Service with Id = {id} can not be found";
                return View("NotFound");
            }

            var model = new EditServiceViewModel
            {
                Id = service.Id,
                Name = service.Name,
                Price = service.Price
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult EditService(EditServiceViewModel model)
        {
            if (ModelState.IsValid)
            {
                var service = _context.Services.Find(model.Id);

                if (service == null)
                {
                    ViewBag.ErrorMessage = $"Service with Id = {model.Id} can not be found";
                    return View("NotFound");
                }
                else
                {
                    service.Name = model.Name;
                    service.Price = model.Price;

                    var result = _context.Services.Update(service);

                    if (result.State == EntityState.Modified)
                    {
                        _context.SaveChanges();
                        return RedirectToAction("ListServices");
                    }

                    return View(model);
                }
            }

            return View();
        }

        [HttpGet]
        public IActionResult DeleteService(int id)
        {
            var service = _context.Services.Find(id);

            if (service == null)
            {
                ViewBag.ErrorMessage = $"Service with Id = {id} can not be found";
                return View("NotFound");
            }
            else
            {
                var result = _context.Services.Remove(service);

                if (result.State == EntityState.Deleted)
                {
                    _context.SaveChanges();
                    return RedirectToAction("ListServices");
                }

                return View();
            }
        }

        [HttpGet]
        public IActionResult CreateOrder(int id)
        {
            var user = _context.AspNetUsers.Find(tempUserId);
            var service = _context.Services.Find(id);

            CreateOrderViewModel order = new CreateOrderViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserEmail = user.Email,
                ServiceName = service.Name,
                EntityPrice = service.Price,
                OrderDateTime = DateTime.Now
            };

            return View(order);
        }

        [HttpPost]
        public IActionResult CreateOrder(CreateOrderViewModel model, int id)
        {
            if (ModelState.IsValid && model.Quantity != 0)
            {
                Order order = new Order
                {
                    UserId = tempUserId,
                    ServiceId = id,
                    Quantity = model.Quantity,
                    TotalAmount = model.Quantity * model.EntityPrice,
                    OrderDateTime = model.OrderDateTime
                };

                var result = _context.Orders.Add(order);

                if (result.State == EntityState.Added)
                {
                    _context.SaveChanges();
                    return RedirectToAction("ListOrders", "Manager");
                }
            }

            return View();
        }

        [HttpGet]
        public IActionResult DeleteOrder(int id)
        {
            var order = _context.Orders.Find(id);

            if (order == null)
            {
                ViewBag.ErrorMessage = $"Order with Id = {id} can not be found";
                return View("NotFound");
            }
            else
            {
                var result = _context.Orders.Remove(order);

                if (result.State == EntityState.Deleted)
                {
                    _context.SaveChanges();
                    return RedirectToAction("ListOrders");
                }

                return View();
            }
        }

        [HttpGet]
        public IActionResult GetLimit()
        {
            ViewBag.ErrorMessage = TempData["ErrorMessage"] as string;
            var limit = _context.Limits.FirstOrDefault();
            return View(limit);
        }

        [HttpGet]
        public IActionResult CreateLimit()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateLimit(CreateLimitViewModel model)
        {
            if(ModelState.IsValid)
            {
                Limit limit = new Limit
                {
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    Quantity = model.Quantity
                };

                var result = _context.Limits.Add(limit);

                if (result.State == EntityState.Added)
                {
                    _context.SaveChanges();
                    return RedirectToAction("GetLimit", "Manager");
                }
            }

            return View();
        }

        [HttpGet]
        public IActionResult DeleteLimit(int id)
        {
            var limit = _context.Limits.Find(id);

            if (limit == null)
            {
                ViewBag.ErrorMessage = $"Limit with Id = {id} can not be found";
                return View("NotFound");
            }
            else
            {
                var result = _context.Limits.Remove(limit);

                if (result.State == EntityState.Deleted)
                {
                    _context.SaveChanges();
                    return RedirectToAction("GetLimit");
                }

                return View();
            }
        }

        private bool CheckLimit()
        {
            var orders = _context.Orders;
            var tempData = DateTime.Now;
            int? totalQuantity = 0;
            foreach(var order in  orders)
            {
                totalQuantity += order.Quantity;
            }

            var limit = _context.Limits.FirstOrDefault();
            if (limit != null)
            {
                if (limit.Quantity < totalQuantity && limit.EndDate > tempData)
                {
                    return false;
                }
                else
                {
                    _context.Limits.Remove(limit);
                    _context.SaveChanges();
                    return true;
                }
            }
            else
                return true;
        }
    }
}

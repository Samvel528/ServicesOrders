using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVCOrder.Data;
using MVCOrder.Models;
using MVCOrder.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVCOrder.Controllers
{
    [Authorize(Roles = "SuperManager")]
    public class SuperManagerController : Controller
    {
        private readonly aspnetMVCOrderContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public SuperManagerController(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager, aspnetMVCOrderContext context)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> ExportToCSV(int id)
        {
            var order = await _context.Orders.Include(u => u.User).Include(s => s.Service).FirstAsync(p => p.Id == id);

            var builder = new StringBuilder();
            builder.AppendLine("Id,UserId,FirstName,LastName,Email,ServiceId,Name,Price,OrderDateTime,Quantity,TotalAmount,");
            builder.AppendLine($"{order.Id},{order.UserId},{order.User.FirstName},{order.User.LastName},{order.User.Email},{order.ServiceId},{order.Service.Name},{order.Service.Price},{order.OrderDateTime},{order.Quantity},{order.TotalAmount}");

            return File(Encoding.UTF8.GetBytes(builder.ToString()), "text/csv", $"Order{id}Info.csv");
        }

        [HttpGet]
        public async Task<IActionResult> ListUsers()
        {
            var adminRole = GetAdminRole();
            var adminUsersRole = _context.AspNetUserRoles.Where(userRole => userRole.RoleId == adminRole.Id).ToList();
            var users = _userManager.Users.ToList();
            for (int i = 0; i < adminUsersRole.Count; i++)
            {
                var admin = await _userManager.FindByIdAsync(adminUsersRole[i].UserId);
                users.Remove(admin);
            }
            return View(users);
        }

        [HttpGet]
        public IActionResult CreateUser()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email, EmailConfirmed = true };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    return RedirectToAction("ListUsers", "SuperManager");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {id} can not be found";
                return View("NotFound");
            }
            else
            {
                var result = await _userManager.DeleteAsync(user);

                if (result.Succeeded)
                {
                    return RedirectToAction("ListUsers");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View();
            }
        }

        [HttpGet]
        public IActionResult ListOrders()
        {
            var orders = _context.Orders.Include(u => u.User).Include(s => s.Service);
            return View(orders);
        }

        private AspNetRole GetAdminRole()
        {
            var adminRole = _context.AspNetRoles.FirstOrDefault(role => role.Name == "Admin");

            return adminRole;
        }
    }
}

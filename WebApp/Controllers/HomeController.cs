using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Yuffie.WebApp;

namespace WebApp.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View("Index", YuffieApp.Config);
        }

        public IActionResult Admin()
        {
            return View("Admin", YuffieApp.Config);
        }
        
        public IActionResult Error()
        {
            return View();
        }
    }
}

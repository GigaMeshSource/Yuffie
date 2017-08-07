using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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

        [HttpPost]
        public IActionResult PushData(string data)
        {
            var parsed = JsonConvert.DeserializeObject<List<YuffieFrontValue>>(data);
            //TODO ALT object is here 
            return Redirect("/Home/Index");
        }
        
        public IActionResult Error()
        {
            return View();
        }

        public class YuffieFrontValue 
        {
            public string Key {get;set;}
            public object Value {get;set;}
        } 
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ServiceStack;
using Yuffie.WebApp;
using Yuffie.WebApp.Models;

namespace WebApp.Controllers
{
    public class HomeController : Controller
    {
        private IHostingEnvironment HostingEnv {get;set;}

        public HomeController(IHostingEnvironment hostingEnv)
        {
            HostingEnv = hostingEnv;
        }
        public IActionResult Index()
        {
            return View("Index", YuffieApp.Config);
        }

        public IActionResult Admin()
        {
            return View("Admin");
        }

        public async Task<IActionResult> Download()
        {
            var data = new List<object>(); //recup info

            var fileName = DateTime.Now.ToString("yyyy-MM-dd HH:mm") + ".csv";            
            var fileData = UTF8Encoding.UTF8.GetBytes(data.ToCsv());
                
            return File(fileData, "text/plain", fileName);
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

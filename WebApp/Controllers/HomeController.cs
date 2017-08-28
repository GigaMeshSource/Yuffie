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
        private readonly Yuffie.WebApp.Models.AppContext _context;


        public HomeController(IHostingEnvironment hostingEnv, Yuffie.WebApp.Models.AppContext context)
        {
            HostingEnv = hostingEnv;
            _context = context;    
        }

        public IActionResult Index()
        {
            return View("Index", YuffieApp.Config);
        }

        public IActionResult Event()
        {
            return View("Event", YuffieApp.Config);
        }

        public IActionResult Admin()
        {
            return View("Admin");
        }

        public async Task<IActionResult> Download()
        {
            var data = new List<Entity>(); //recup info
            data = _context.Entity.ToList();
            
             //write in csv file            
            var fileName = DateTime.Now.ToString("yyyy-MM-dd HH:mm") + ".csv";            
            var fileData = UTF8Encoding.UTF8.GetBytes(data.ToCsv());
                
            return File(fileData, "text/plain", fileName);
        }

        [HttpPost]
        public IActionResult PushData(string data)
        {
            var parsed = JsonConvert.DeserializeObject<List<YuffieFrontValue>>(data);
            //TODO ALT object is here 
           
                if (parsed != null) {
                    var dataList = new List<Data>();

                foreach (var item in parsed)
                {
                    if (item.Key != null && item.Value != null) {

                        dataList.Add (new Data {
                        Key = item.Key,
                        Value = item.Value.ToString()
                        });
                    }
                }

                var entity = new Entity {
                    Data = dataList                
                };
                _context.Add(entity);
                _context.SaveChanges(); //await
                }
            
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

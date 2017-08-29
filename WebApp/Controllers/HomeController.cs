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
            var test = "{\"Service_DCO\":null,\"Distributeur\":null,\"CRCM\":null,\"PSC\":null,\"ASSU\":null,\"Type_d'intervention\":null,\"Intervention_IP\":null,\"Formation\":null,\"Type_d'intervention_2\":null,\"Date\":null,\"Laps\":null,\"Heure_début\":null,\"Heure_fin\":null,\"Thème\":null,\"Sous_thème\":null,\"Sujet\":null,\"Commentaire_ASSU\":null,\"Thème_CRCM/PSC\":null,\"Numéro_vivier\":null}";
            var parsed = JsonConvert.DeserializeObject<Intervenants>(data);
            //TODO ALT object is here 
            
            if (parsed != null) 
            {    
                var dataList = new List<Data>();

                // foreach (var item in parsed.KeyValue)
                // {
                //     //boucle sur keyx et values en meme temps pour inserer les bonnes valeurs
                //     // verifier que le model est bon
                //     foreach(var idx in item.Keys)
                //     {
                //             dataList.Add (new Data {
                //             Key = item.Key,
                //             Value = item.Value
                //             });
                        
                //     }
                // }

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

        public class Intervenants {
           public  Dictionary<string,object>[] intervenants {get;set;}
            public Dictionary<object, object> rest {get;set;}
        }
    }
}
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
using System.Data.SqlClient;
using Neo4j.Driver.V1;

namespace WebApp.Controllers
{
    public class HomeController : Controller
    {
        private IHostingEnvironment HostingEnv {get;set;}

        private Graph Graph {get;set;}

        public HomeController(IHostingEnvironment hostingEnv, Yuffie.WebApp.Models.AppContext context)
        {

            HostingEnv = hostingEnv;

            Graph =  new Graph(new Uri("bolt://database:7687/"), "", "");
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
        
        private string Export(List<Entity> Entity)
        {
            var array = new Newtonsoft.Json.Linq.JArray();
            bool repeat = false;
 
            
            var separator = ";";
            var tmp = new StringBuilder();
            var first = new StringBuilder();
            var last = new StringBuilder();

            var dataCsv = new StringBuilder(CreateHeader() + "\n");
            
            //Graph.GetEntities();

            foreach (var item in Entity)
            {
                var elements = YuffieApp.Config.Pages.SelectMany(p => p.Sections != null ? p.Sections.SelectMany(s => s.Elements) : new List<YCPSElement>()).ToList();                

                var deserialized = JsonConvert.DeserializeObject<Dictionary<string, object>>(item.Value);
                foreach (var element in elements)
                {
                    var parsed = element.Name.Replace(" ", "_").Replace("'", "").Replace("/", "_");
                    if (element.Type == "List" || element.Type == "Text")
                    {
                       if (deserialized.ContainsKey(parsed))
                        {
                            if (repeat)
                                last.Append(deserialized[parsed] + separator);
                            else
                                first.Append(deserialized[parsed] + separator);
                        }
                        else 
                        {
                            if (repeat)
                                last.Append(separator);
                            else
                                first.Append(separator);
                        }
                    }
                    if (element.Type == "SubElement")
                    {
                        var subElementEncode = element.Name.Replace(" ", "_").Replace("'", "").Replace("/", "_");
                        if(deserialized.ContainsKey(subElementEncode))
                        {
                            array = deserialized[subElementEncode] as Newtonsoft.Json.Linq.JArray;
                            if (array != null) 
                                repeat = true;
                        }
                        else
                        {
                            if (repeat)
                                last.Append(separator);
                            else
                                first.Append(separator);
                        }

                    }
                    if (element.Type == "Tree")
                    {
                        foreach(var level in element.Levels)
                        {
                            var levelEncode = level.Replace(" ", "_").Replace("'", "").Replace("/", "_");
                            if (deserialized.ContainsKey(levelEncode))
                            {
                                if (repeat)
                                    last.Append(deserialized[levelEncode] + separator);
                                else
                                    first.Append(deserialized[levelEncode] + separator);
                            }
                            else 
                            {
                                if (repeat)
                                    last.Append(separator);
                                else
                                    first.Append(separator);
                            }
                        }
                    }
                } 
                if (repeat)
                {
                    foreach (var slot in array)
                    {
                        foreach(var elem in slot)
                        {
                            tmp.Append((string)elem + separator);
                        }
                        var line = first.ToString() + tmp.ToString() + last.ToString() + "\n";
                        dataCsv.Append(line);
                        tmp.Clear(); 
                    }
                    first.Clear();
                    last.Clear();
                    repeat = false;
                }
                else
                {
                    dataCsv.Append(first + "\n");
                }
            }            
            return dataCsv.ToString();
        }

        private string CreateHeader()
        {
            var header = "";
            var separator = ";";

            var json = System.IO.File.ReadAllText(@"yuffieconfig.json");
            var deserializedObject = JsonConvert.DeserializeObject<YuffieConfiguration>(json);

            foreach (var pages in deserializedObject.Pages)
            {
                foreach(var sections in pages.Sections)
                {
                    foreach (var element in sections.Elements)
                    {
                        if (element.Type == "List" || element.Type == "Text")
                        {
                            header += element.Name + separator;
                        }
                        if (element.Type == "SubElement")
                        {   
                            foreach(var subElement in element.Elements)
                            {
                                header += subElement.Name + separator;
                            }
                        }
                        if (element.Type == "Tree")
                        {   
                            foreach(var tree in element.Levels)
                            {
                                header += tree + separator;
                            }
                        }
                    }
                }
            }
            return header;

        }

        public async Task<IActionResult> Download()
        {
           var entity = new List<Entity>();

            // try {
            //     using (var connection = new SqlConnection(@"Server=tcp:anime-co-db.database.windows.net,1433;Initial Catalog=yuffie-anim;Persist Security Info=False;User ID=azureworker;Password=Tennis94;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30"))
            //     {
            //         connection.Open();
            //         using (var sqlCommand = new SqlCommand("SELECT * FROM JsonFile" , connection))
            //         {
            //           using (var reader = await sqlCommand.ExecuteReaderAsync())
            //             {
            //                 while (reader.Read())
            //                 {
            //                     var e = new Entity();
            //                     e.Id = reader.GetInt32(0);
            //                     e.Date = reader.GetDateTime(1);
            //                     e.Value = reader.GetString(2);

            //                     entity.Add(e);
            //                 }
            //             }
            //         }
            //         connection.Close();  
            //     }
            // }
            // catch(Exception ex)
            // {
            //     var res  = ex.Message;
            // }
    
            // var data = Export(entity);
            var data = Graph.GetEntities();
             //write in csv file            
            var fileName = DateTime.Now.ToString("yyyy-MM-dd HH:mm") + ".csv";            
            var fileData = UTF8Encoding.UTF8.GetBytes(data);
            return File(fileData, "text/plain", fileName);
        }

        [HttpPost]
        public async Task<IActionResult> PushData(string data)
        {
            try {
                Graph.CreateEntities(data);
                // using (var connection = new SqlConnection(@"Server=tcp:anime-co-db.database.windows.net,1433;Initial Catalog=yuffie-anim;Persist Security Info=False;User ID=azureworker;Password=Tennis94;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30"))
                // {
                //     //useless to open with using ?
                //     connection.Open();
                //     using (var sqlCommand = new SqlCommand("INSERT INTO JsonFile VALUES(@Date, @Json)" , connection))
                //     {
                //         sqlCommand.Parameters.Add(new SqlParameter("Date", DateTime.UtcNow));
                //         sqlCommand.Parameters.Add(new SqlParameter("Json", data));

                //          sqlCommand.ExecuteNonQuery();
                //     }
                //     connection.Close();   
                // }
            }
            catch (Exception ex)
            {
                return NotFound(ex);
             }
            return Ok();
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
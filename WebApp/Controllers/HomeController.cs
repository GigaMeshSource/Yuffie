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
        private static string RemoveSpecialCharacters(string str)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < str.Length; i++)
            {
                    if (str[i] != '{' && str[i] != '\\' && str[i] != '}')
                    {
                        sb.Append(str[i]);
                    }
            }

            return sb.ToString();
        }
        
        private string Export(List<Entity> Entity)
        {
            var array = new Newtonsoft.Json.Linq.JArray();
            bool repeat = false;

            var dataCsv = "";
            var tmp = "";
            var separator = ";";
            var first = "";
            var last = "";

             foreach (var item in Entity)
                {
                    var deserialized = JsonConvert.DeserializeObject<Dictionary<string, object>>(item.Value);
                    foreach (var keyVal in deserialized)
                    {
                        if (keyVal.Value.GetType() == typeof(Newtonsoft.Json.Linq.JArray))
                        {
                            array = keyVal.Value as Newtonsoft.Json.Linq.JArray;
                            repeat = true;
                        }

                        if (keyVal.Value.GetType() == typeof(string))
                        {
                            if (repeat)
                                last += (string)keyVal.Value + separator;
                            else   
                                first += (string)keyVal.Value + separator;
                        }
                    }
                    if (repeat)
                    {
                        foreach (var slot in array)
                        {
                            foreach(var elem in slot)
                            {
                                tmp += (string)elem + separator;
                            }
                            dataCsv += first + tmp + last + "\n";
                        }
                        repeat = false;
                    }
                    else {
                        dataCsv += first + "\n";
                    }
                }
            return dataCsv;
        }

        private void CreateHeader()
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
                        if (element.Type == "List")
                        {
                            header += element.Name + separator;
                        }
                        if (element.Type == "SubElement")
                        {
                            foreach(var subElement in element.Elements)
                            {
                                header += subElement.Name;
                            }
                        }
                    }
                }
            }
        }

        public async Task<IActionResult> Download()
        {
           var entity = new List<Entity>();

            try {
                using (var connection = new SqlConnection(@"Server=tcp:anime-co-db.database.windows.net,1433;Initial Catalog=yuffie-anim;Persist Security Info=False;User ID=azureworker;Password=Tennis94;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30"))
                {
                    connection.Open();
                    using (var sqlCommand = new SqlCommand("SELECT * FROM JsonFile" , connection))
                    {
                      using (var reader = await sqlCommand.ExecuteReaderAsync())
                        {
                            while (reader.Read())
                            {
                                var e = new Entity();
                                e.Id = reader.GetInt32(0);
                                e.Date = reader.GetDateTime(1);
                                e.Value = reader.GetString(2);

                                entity.Add(e);
                            }
                        }
                    }
                    connection.Close();  
                }
            }
            catch(Exception ex)
            {
                var res  = ex.Message;
            }
            CreateHeader();
            var data = Export(entity);
             //write in csv file            
            var fileName = DateTime.Now.ToString("yyyy-MM-dd HH:mm") + ".csv";            
            var fileData = UTF8Encoding.UTF8.GetBytes(data);
            return File(fileData, "text/plain", fileName);
        }

        [HttpPost]
        public async Task<IActionResult> PushData(string data)
        {
            try {
                using (var connection = new SqlConnection(@"Server=tcp:anime-co-db.database.windows.net,1433;Initial Catalog=yuffie-anim;Persist Security Info=False;User ID=azureworker;Password=Tennis94;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30"))
                {
                    //useless to open with using ?
                    connection.Open();
                    using (var sqlCommand = new SqlCommand("INSERT INTO JsonFile VALUES(@Date, @Json)" , connection))
                    {
                        sqlCommand.Parameters.Add(new SqlParameter("Date", DateTime.UtcNow));
                        sqlCommand.Parameters.Add(new SqlParameter("Json", data));

                         sqlCommand.ExecuteNonQuery();
                    }
                    connection.Close();   
                }
            }
            catch (Exception ex)
            {
                return NotFound(ex);
            }
            return Ok();
        }

        
        public IActionResult Errore()
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
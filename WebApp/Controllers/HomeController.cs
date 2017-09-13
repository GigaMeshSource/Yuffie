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
        private List<Entity> Entity {get;set;}

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
        
        //stringbuilder instead of =+
        private string Export(List<Entity> Entity)
        {
            var array = new Newtonsoft.Json.Linq.JArray();
            bool repeat = false;
 
            var dataCsv = CreateHeader() + "\n";
            var tmp = "";
            var separator = ";";
            var first = "";
            var last = "";
            var header = CreateHeader().Split(';');

            
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
                                last += deserialized[parsed] + separator;
                            else
                                first += deserialized[parsed] + separator;
                        }
                        else 
                        {
                            if (repeat)
                                last += separator;
                            else
                                first += separator;
                        }
                    }
                    if (element.Type == "SubElement")
                    {
                        var subElementEncode = element.Name.Replace(" ", "_").Replace("'", "").Replace("/", "_");
                        if(deserialized.ContainsKey(subElementEncode))
                        {
                            array = deserialized[subElementEncode] as Newtonsoft.Json.Linq.JArray;
                            if (array != null) 
                            {
                                repeat = true;       
                            }
                        }
                        else
                        {
                            if (repeat)
                                last += separator;
                            else
                                first += separator;
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
                                    last += deserialized[levelEncode] + separator;
                                else
                                    first += deserialized[levelEncode] + separator;
                            }
                            else 
                            {
                                if (repeat)
                                    last += separator;
                                else
                                    first += separator;
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
                            tmp += (string)elem + separator;
                        }
                        dataCsv += first + tmp + last + "\n";
                        tmp = ""; 
                    }
                    first = "";
                    last = "";
                    repeat = false;
                }
                else
                {
                    dataCsv += first + "\n";
                }
            }            
            return dataCsv;
        }

        private void Test(StringBuilder data, object obj = null)
        {
            if (obj != null)
            {
                var dico = obj as Dictionary<string, string>;
                foreach (var key in dico)
                {
                    // check header
                    data.Append(key.Value + ";");                    
                }
                return ;
            }

           Dictionary<string, object> deserializedObject;

           foreach(var entity in Entity)
           {
               var first = "";
               deserializedObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(entity.Value);
                foreach(var keyVal in deserializedObject)
                {
                    if (keyVal.Value.GetType() == typeof(string))
                    {
                        first += (string)keyVal.Value + ";";

                        //data.Append((string)keyVal.Value + ";");
                    }
                    if (keyVal.Value.GetType() == typeof(Newtonsoft.Json.Linq.JArray))
                    {
                        var o = (Newtonsoft.Json.Linq.JArray)keyVal.Value;
                        for(int i = 0; i < o.Count(); i++)
                        {
                            var csvData = new StringBuilder();
                            Test(csvData, o[i]);
                            data.Append(first);
                            data.Append(csvData);
                        }
                    }
                }
                data.Append("\n");
           }
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
            var data = Export(entity);
            // Test(new StringBuilder(), null);
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
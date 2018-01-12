using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Neo4j.Driver.V1;
using Newtonsoft.Json;
using WebApp;

namespace Yuffie.WebApp.Models {

    public class Graph
    {
        public const string ENTITY_NAME = "Entity";
        public const string ENTITY_PARAM = "entity";
        public const string VALUE_NAME = "Value";
        public const string VALUE_PARAM = "val";
        public const string VALUE_PROPERTY = "value";
        public const string REL_NAME = "NAME";
        public const string REL_PARAM = "rel";

        private StringBuilder Header = new StringBuilder();
        private List<string> ListHeader = new List<string>();
        private Dictionary<string, List<string>> csv = new Dictionary<string, List<string>>();
        private StringBuilder sbFormat = new StringBuilder();
        private StringBuilder arrayFormat = new StringBuilder();
        private List<string> headerArrayData;
        private Dictionary<string, List<string>> arrayData = new Dictionary<string, List<string>>();
        private static readonly string[] tab = {""};
        int consultantCount = 0;
        
        private IDriver _db;

        public Graph(Uri uri, string login, string password)
        {
            _db = GraphDatabase.Driver(uri, AuthTokens.Basic(login, password));
        }

        public async Task<bool> Execute(string query)
        {
            using (var session = _db.Session())
            {
                session.Run(new Statement(query));
            }
            return true;
        }

        public  void CreateEntities(string data)
        {
            int id = 0;
            bool comma = true;
            var deserializedObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(data);

            try
            {
                var query = new StringBuilder();
                var parameters = new Dictionary<string, object>();
              
                var queryBuilder = new StringBuilder("CREATE (" + ENTITY_PARAM + ":" + ENTITY_NAME + "{DateTime: '" + DateTime.UtcNow + "'}) RETURN ID(entity)");
                
                using (var session = _db.Session())
                {
                    // var request = session.Run(queryBuilder.ToString());
                    var request = session.WriteTransaction(tx =>
                    {
                        var result = tx.Run(queryBuilder.ToString());
                        return result.ToList();
                    });

                    if (request != null)
                        id = Convert.ToInt32(request.FirstOrDefault().Values["ID(entity)"]);              
                    queryBuilder.Clear();

                    queryBuilder.Append("MATCH (" + ENTITY_PARAM + ":" + ENTITY_NAME + ") WHERE ID(" + ENTITY_PARAM + ") = $pid\n CREATE");
                    parameters.Add("pid", id);

                    foreach (var item in deserializedObject)
                    {
                        if (deserializedObject.ContainsKey("Conseiller") && (string.Equals(item.Key,"Conseiller") || string.Equals(item.Key,"CONSEILLER")))
                        {
                            var check = true;
                            var res = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(item.Value.ToString());
                            query.Append("CREATE (e: " + ENTITY_NAME + "),\n");                            
                            
                            foreach(var dico in res)
                            {
                                foreach (var element in dico)
                                {
                                    if (check)
                                        check = false;
                                    else
                                        query.Append(",\n"); 
                                    query.Append("(e)-[:LINKS {name:'" + element.Key + "'}]->(:" + VALUE_NAME + " {" + VALUE_PROPERTY + ":'" + element.Value.ToString() + "'})");
                                }
                            }
                            query.Append("\nWITH e\nMATCH (" + ENTITY_PARAM + ":" + ENTITY_NAME + ") WHERE ID(" + ENTITY_PARAM + ") = $pid\n");
                            query.Append("CREATE (" + ENTITY_PARAM + ")-[:CONSEILLER]->(e)");
                            continue;
                        }
                        if (comma)
                            comma = false;
                        else
                            queryBuilder.Append(",\n");

                        queryBuilder.Append(" (" + ENTITY_PARAM + ")-[:LINKS {name: '" + item.Key.ToUpper() + "'}]->");
                        queryBuilder.Append("(:"+ VALUE_NAME + " {" + VALUE_PROPERTY + ": '"  + item.Value + "'})");
                       
                    }
                    
                    request.Clear();
                    // session.Run(queryBuilder.ToString(), new {pid = id});
                    // session.Run(query.ToString(), new {pid = id});
                    request = session.WriteTransaction(tx => 
                    {
                        var result = tx.Run(queryBuilder.ToString(), new {pid = id});
                        return result.ToList();
                    });

                    request.Clear();
                    request = session.WriteTransaction(tx => 
                    {
                        var result = tx.Run((query).ToString(), new {pid = id});
                        return result.ToList();
                    });
                }
            }
            catch(Exception ex)
            {
                var err = ex.Message + " " + ex.StackTrace;
            }           
        }

        public string GetEntities()
        {
            var queryBuilder = new StringBuilder();
            var parameters = new Dictionary<string, object>();       

            List<entity> entityList = new List<entity>();
           
            List<string> csvData = new List<string>();
            List<string> arrayData = new List<string>();
            var arrayHeader = new StringBuilder();

            INode childNode = null;
            INode parentNode = null;

            try {
                using (var session = _db.Session())
                {
                    queryBuilder.Append("MATCH (" + ENTITY_PARAM + ")-[" + REL_PARAM + "]->(" + VALUE_PARAM + ") RETURN "
                    + ENTITY_PARAM + "," + REL_PARAM + ", " + VALUE_PARAM + " ORDER BY ID(" + VALUE_PARAM + ") ASC");
                    
                    // var request = session.Run(queryBuilder.ToString());
                    var request = session.ReadTransaction(tx =>
                    {
                        var result = tx.Run(queryBuilder.ToString());
                        return result.ToList();
                    });
                    
                    CreateHeader();
                    // see response structure to create adequate model
                    var e = new entity();
                    var cnt = 1;
                   
                    foreach (var item in request)
                    {
                        if (item.Values.Count == 3)
                        {
                            INode node = null;
                            if (item.Values.ElementAt(0).Value is INode)
                                node = item.Values.ElementAt(0).Value as INode;

                            if (!string.IsNullOrEmpty(e.Date) && node.Properties.Count > 0)
                            {
                                if (!string.Equals(e.Date, node.Properties.Single().As<KeyValuePair<string, object>>().Value.ToString()))
                                {
                                    parentNode = null;
                                   
                                   
                                }
                            }

                            if (parentNode == null && item.Values.ElementAt(0).Value is INode)
                            {
                                parentNode = item.Values.ElementAt(0).Value as INode;
                                if (parentNode != null && parentNode.Properties.Count > 0)
                                {          
                                    e.Id = parentNode.Id;
                                    e.Date = parentNode.Properties.Single().As<KeyValuePair<string, object>>().Value.ToString();
                                }
                            }

                            var relationship = item.Values[REL_PARAM] as IRelationship;


                            if (e.Id == node.Id && relationship.Properties.Count > 0)
                            {
                                try {
                                    var relationshipName = relationship.Properties.Single().As<KeyValuePair<string, object>>().Value.ToString();

                                    var value = item.Values[VALUE_PARAM] as INode;
                                    var valueProperty = value.Properties.Single().As<KeyValuePair<string, object>>().Value.ToString();
                                    if(!csv.ContainsKey(relationshipName))
                                        continue;
                                    csv[relationshipName].Add(valueProperty);
                                    continue;
                                }
                                catch (Exception ex)
                                {
                                    return ex.Message + " " + ex.StackTrace;
                                }                                
                            }

                            if (relationship.Type == "CONSEILLER")
                            {
                                foreach (var element in csv)
                                {
                                    if (element.Value.Count() < cnt)
                                    {
                                        element.Value.Add("");
                                    }
                                }
                                    
                                cnt++;
                                CreateConsultantData(relationship.EndNodeId);
                                continue;
                            }
                        }
                    }
                }
                FormatCSV();
                
                return sbFormat.ToString();
            }
            catch(Exception ex)
            {
                return "KIKOO " + ex.Message + " " + ex.StackTrace;
            }
        }

        private void CreateConsultantData(long id)
        {
            var parameters = new Dictionary<string, object>();
            var nb = 0;
            var dico = new Dictionary<string, List<string>>();

            try 
            {
                using (var session = _db.Session())
                {
                    var query = "MATCH (" + ENTITY_PARAM + ")-[" + REL_PARAM + "]->(" + VALUE_PARAM + ") WHERE ID(" + ENTITY_PARAM + ") = $pid RETURN " + ENTITY_PARAM + ", " + REL_PARAM + ",  " + VALUE_PARAM
                    + " ORDER BY " + VALUE_PARAM + " ASC";

                    var queryCountNodes = "MATCH(" + ENTITY_PARAM + ")-[" + REL_PARAM + "]->(" + VALUE_PARAM + ") WHERE ID(" + ENTITY_PARAM + ") = $pid RETURN COUNT(*)";
                    parameters.Add("pid", id);

                    var responseCountNodes = session.ReadTransaction(tx =>
                    {
                        var result = tx.Run(queryCountNodes.ToString(), new {pid = id});
                        return result.ToList();
                    });

                     if (responseCountNodes != null)
                        nb = Convert.ToInt32(responseCountNodes.FirstOrDefault().Values["COUNT(*)"]);              


                    var response = session.ReadTransaction(tx =>
                    {
                        var result = tx.Run(query.ToString(), new {pid = id});
                        return result.ToList();
                    });


                    string[] header =  {"Nom_conseiller", "Prénom_conseiller", "Matricule_conseiller", "Fonction_conseiller", "Code_Agence", "Fonction_CRC_PSC"};
                    arrayData = new Dictionary<string, List<string>>();
                    foreach(var h in header)
                    {
                        arrayData.Add(h, new List<string>());
                    }

                    foreach (var item in response)
                    {
                        if (item.Values.Count == 3)
                        {
                            INode node = null;
                            if (item.Values.ElementAt(0).Value is INode)
                                node = item.Values.ElementAt(0).Value as INode;

                            var relationship = item.Values[REL_PARAM] as IRelationship;
                            if (relationship.Properties.Count > 0)
                            {
                                var relationshipName = relationship.Properties.Single().As<KeyValuePair<string, object>>().Value.ToString();

                                var value = item.Values[VALUE_PARAM] as INode;
                                var valueProperty = value.Properties.Single().As<KeyValuePair<string, object>>().Value.ToString();
                                if (arrayData.Keys.Contains(relationshipName))
                                {
                                    arrayData[relationshipName].Add(valueProperty);
                                }

                                // arrayData.Add(relationshipName, new List<string>{valueProperty});


                                // var relationshipName = relationship.Properties.Single().As<KeyValuePair<string, object>>().Value.ToString();
                                // var value = item.Values[VALUE_PARAM] as INode;
                                // var valueProperty = value.Properties.Single().As<KeyValuePair<string, object>>().Value.ToString();
                                // arrayData.Add(valueProperty);
                                // headerArrayData.Add(relationshipName);
                            }                                    
                        }
                    }
                     
                    foreach (var array in arrayData)
                    {
                        var tmp = array.Value.Count();

                        if (tmp > consultantCount)
                        {
                            consultantCount = tmp;
                        }
                    }                    

                    for(var i = 0; i < consultantCount; ++i)
                    {
                        foreach(var element in arrayData)
                        {  
                            if (element.Value.Count() < consultantCount)
                            {
                                element.Value.Add("");
                            } 
                        }
                    }

                    headerArrayData = new List<string>();
                    for (var x = 0; x < consultantCount;++x)
                    {
                        foreach (var item in arrayData)
                        {
                            headerArrayData.Add(item.Key);
                        }   
                    }
                }
                var cnt = arrayData.Values.FirstOrDefault().Count();
                for(var x = 0; x < cnt; x++)
                {
                    foreach(var array in arrayData)
                    {
                        arrayFormat.Append(array.Value[x]);
                        arrayFormat.Append(";");
                    }    
                }              
                arrayFormat.Append("\n");
            }
            catch(Exception ex)
            {
                var err = ex.Message;
            }
        }



        private void FormatCSV()
        {
            var separator = ";";
            var index = 0;

            try 
            {
                foreach(var item in ListHeader)
                {
                    sbFormat.Append(item + separator);
                }

                foreach (var h in headerArrayData)
                {
                    sbFormat.Append(h + separator);
                }
                sbFormat.Append("\n");

                var cnt = csv.Values.FirstOrDefault().Count();
                var arrayTab = arrayFormat.ToString().Split('\n');
                for(var x = 0; x < cnt; x++)
                {
                    foreach(var item in csv)
                    {
                        sbFormat.Append(item.Value[x] + separator);
                    }
                    sbFormat.Append(arrayTab[x]);
                    sbFormat.Append("\n");
                }
               
            
            }
            
            catch (Exception ex)
            {
                var err = ex.Message;
            }
        }

        private void CreateHeader()
        {
            var separator = ";";
            var elements = YuffieApp.Config.Pages.SelectMany(p => p.Sections != null ? p.Sections.SelectMany(s => s.Elements) : new List<YCPSElement>()).ToList();

            foreach (var element in elements)
            {
                if (element.Type == "List" || element.Type == "Text")
                {
                    var parsed = element.Name.Replace(" ", "_").Replace("'", "").Replace("/", "_");
                    Header.Append(parsed.ToUpper() + separator);
                    csv.Add(parsed.ToUpper(), new List<string>());
                    ListHeader.Add(parsed.ToUpper());
                }

                if (element.Type == "Tree")
                {   
                    foreach(var tree in element.Levels)
                    {
                        var parsed = tree.Replace(" ", "_").Replace("'", "").Replace("/", "_");   
                        Header.Append(parsed.ToUpper() + separator);
                        csv.Add(parsed.ToUpper(), new List<string>());
                        ListHeader.Add(parsed.ToUpper());
                    }
                }
            }
        }

        private void Format(List<string> csvData)
        {
            foreach (var d in csvData)
            {
                sbFormat.Append(d);
                sbFormat.Append(";");
            }

            sbFormat.Append("\n");
        }

        private void FormatDico()
        {
            foreach (var item in csv)
            {
                foreach (var l in item.Value)
                {
                    sbFormat.Append(l + ";");
                }
            }
            sbFormat.Append("\n");
        }
    }
}
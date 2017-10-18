using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Neo4j.Driver.V1;
using Newtonsoft.Json;

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
        private StringBuilder sbFormat = new StringBuilder();

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
                var err = ex.Message;
            }           
        }

        public string GetEntities()
        {
            var queryBuilder = new StringBuilder();
            var parameters = new Dictionary<string, object>();       

            List<entity> entityList = new List<entity>();
           
            List<string> csvData = new List<string>();
            List<string> arrayData = new List<string>();

            INode childNode = null;
            INode parentNode = null;
            bool headerComplete = false;

            try {
                using (var session = _db.Session())
                {
                    queryBuilder.Append("MATCH (" + ENTITY_PARAM + ")-[" + REL_PARAM + "]->(" + VALUE_PARAM + ") RETURN "
                    + ENTITY_PARAM + "," + REL_PARAM + ", " + VALUE_PARAM + " ORDER BY ID(" + VALUE_PARAM + ") ASC");
                    
                    var request = session.ReadTransaction(tx =>
                    {
                        var result = tx.Run(queryBuilder.ToString());
                        return result.ToList();
                    });
                    
                    // CreateHeader(request);
                    // see response structure to create adequate model
                    var e = new entity();

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
                                    if (csvData.Count > 0)
                                    {
                                        csvData.AddRange(arrayData);
                                        Format(csvData);
                                        parentNode = null;

                                        csvData.Clear();
                                        arrayData.Clear();
                                        headerComplete = true;
                                    }
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
                            if (!headerComplete && relationship.Properties.Count > 0)
                                AddItemToHeader(relationship.Properties.Single().As<KeyValuePair<string, object>>().Value.ToString());

                            if (e.Id != node.Id && relationship.Properties.Count > 0)
                            {
                                childNode = item.Values.ElementAt(0).Value as INode;     
                                
                                var childValue = item.Values[VALUE_PARAM] as INode;
                                var childValueProperty = childValue.Properties.Single().As<KeyValuePair<string, object>>().Value.ToString();
                                arrayData.Add(childValueProperty);
                                continue;
                            }

                            if (relationship.Properties.Count > 0)
                            {
                                var value = item.Values[VALUE_PARAM] as INode;
                                var valueProperty = value.Properties.Single().As<KeyValuePair<string, object>>().Value.ToString();
                                csvData.Add(valueProperty);
                                
                            }
                            if (relationship.Type == "CONSEILLER")
                                continue;
                            
                        }
                    }
                    csvData.AddRange(arrayData);
                    Format(csvData);
                    Header.Append("\n");
                    Header.Append(sbFormat.ToString());
                    return Header.ToString();
                }
            }
            catch(Exception ex)
            {
                return ex.Message;
            }
        }

        private void AddItemToHeader(string item)
        {
            Header.Append(item);
            Header.Append(";");
        }

        private void CreateHeader(List<IRecord> header)
        {
            var tmp = new StringBuilder();

            foreach (var item in header)
            {
                if (item.Values.Count == 3)
                {
                    var relationship = item.Values[REL_PARAM] as Neo4j.Driver.V1.IRelationship;
                    if (relationship.Properties.Count > 0)
                    {
                        var title = relationship.Properties.Single().As<KeyValuePair<string, object>>().Value.ToString();
                        Header.Append(title);
                        Header.Append(";");
                    }
                }
            }

            // if (tmp.Length > Header.Length)
            // {
            //     Header = new StringBuilder(tmp.ToString());

            //     Header.Clear();
            //     Header.Append(tmp);
            // }

            Header.Append("\n");
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
    }
}
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
                    
                    Console.WriteLine(queryBuilder);
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
            var sb = new StringBuilder();            

            List<entity> entityList = new List<entity>();
            List<string> header = new List<string>();
            List<string> csvData = new List<string>();
            List<string> arrayData = new List<string>();

            Neo4j.Driver.V1.INode childNode = null;
            Neo4j.Driver.V1.INode parentNode = null;
            try {
                using (var session = _db.Session())
                {
                    queryBuilder.Append("MATCH (" + ENTITY_PARAM + ")-[" + REL_PARAM + "]->(" + VALUE_PARAM + ") RETURN "
                    + ENTITY_PARAM + "," + REL_PARAM + ", " + VALUE_PARAM);
                    
                    var request = session.ReadTransaction(tx =>
                    {
                        var result = tx.Run(queryBuilder.ToString());
                        return result.ToList();
                    });
                    
                    // see response structure to create adequate model
                    var e = new entity();

                    foreach (var item in request)
                    {
                        if (item.Values.Count == 3)
                        {
                            Neo4j.Driver.V1.INode node = null;
                            if (item.Values.ElementAt(0).Value is Neo4j.Driver.V1.INode)
                                node = item.Values.ElementAt(0).Value as Neo4j.Driver.V1.INode;


                            if (parentNode == null && item.Values.ElementAt(0).Value is Neo4j.Driver.V1.INode)
                            {
                                parentNode = item.Values.ElementAt(0).Value as Neo4j.Driver.V1.INode;
                                if (parentNode != null && parentNode.Properties.Count > 0)
                                {          
                                    e.Id = parentNode.Id;
                                    e.Date = parentNode.Properties.Single().As<KeyValuePair<string, object>>().Value.ToString();
                                }
                            }
                            if (e.Id != node.Id) //&& node.Properties.Count == 0
                            {
                                childNode = item.Values.ElementAt(0).Value as Neo4j.Driver.V1.INode;
                                var childRelationship = item.Values[REL_PARAM] as Neo4j.Driver.V1.IRelationship;
                                var childRelationshipProperty = childRelationship.Properties.Single().As<KeyValuePair<string, object>>().Value.ToString();                             
                                header.Add(childRelationshipProperty);
                                var childValue = item.Values[VALUE_PARAM] as Neo4j.Driver.V1.INode;
                                var childValueProperty = childValue.Properties.Single().As<KeyValuePair<string, object>>().Value.ToString();
                                arrayData.Add(childValueProperty);
                                continue;
                            }

                            var relationship = item.Values[REL_PARAM] as Neo4j.Driver.V1.IRelationship;

                            if (relationship.Properties.Count > 0)
                            {
                                var relationshipProperty = relationship.Properties.Single().As<KeyValuePair<string, object>>().Value.ToString();
                                header.Add(relationshipProperty);
                                var value = item.Values[VALUE_PARAM] as Neo4j.Driver.V1.INode;
                                var valueProperty = value.Properties.Single().As<KeyValuePair<string, object>>().Value.ToString();
                                csvData.Add(valueProperty);
                            }
                        }
                    }
                    csvData.AddRange(arrayData);
                    csvData.Add("\n");
                    foreach(var h in header)
                    {
                        sb.Append(h);
                        sb.Append(";");
                    }
                    sb.Append("\n");
                    foreach (var d in csvData)
                    {
                        sb.Append(d);
                        sb.Append(";");
                    }
                    csvData.Clear();
                    sb.Append("\n");
                    
                    return sb.ToString();
                }
            }
            catch(Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
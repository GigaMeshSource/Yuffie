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
            int id = 11;
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

                    queryBuilder.Append("MATCH (" + ENTITY_PARAM + ":" + ENTITY_NAME + ") WHERE ID(" + ENTITY_PARAM + ") = $pid\n");
                    parameters.Add("pid", id);

                    foreach (var item in deserializedObject)
                    {
                        if (deserializedObject.ContainsKey("Conseiller") && string.Equals(item.Key,"Conseiller"))
                        {
                            var res = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(item.Value.ToString());
                            query.Append("CREATE ( e: " + ENTITY_NAME + ")\n");                            

                            foreach(var dico in res)
                            {
                                foreach (var element in dico)
                                {
                                    query.Append("CREATE (e)-[:LINKS {name:'" + element.Key + "'}]->(:" + VALUE_NAME + " {" + VALUE_PROPERTY + ":'" + element.Value.ToString() + "'})\n");
                                }
                            }
                            query.Append("CREATE (" + ENTITY_PARAM + ")-[:CONSEILLER]->(e)");
                            continue;
                        }

                        queryBuilder.Append(" CREATE (" + ENTITY_PARAM + ")-[:LINKS {name: '" + item.Key.ToUpper() + "'}]->");
                        queryBuilder.Append("(:"+ VALUE_NAME + " {" + VALUE_PROPERTY + ": '"  + item.Value + "'})\n");
                    }
                    Console.WriteLine(queryBuilder.Append(query));

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

        public void GetEntities()
        {
            var queryBuilder = new StringBuilder();
            var parameters = new Dictionary<string, object>();
            List<entity> entityList = new List<entity>();
            List<string> header = new List<string>();
            List<string> csvData = new List<string>();

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
                    foreach (var item in request)
                    {
                        foreach (var element in item.Values)
                        {
                            var e = new entity();

                            var obj = element.Value;
                            var node = element.Value as Neo4j.Driver.V1.INode;
                            object entity = null;
                            if (node != null && node.Properties.Count > 0)
                                entity = node;
                            
                            
                            // if (obj is Neo4j.Driver.V1.INode)
                            // {
                            //     var startNode = obj as Neo4j.Driver.V1.INode;
                            //     e.Id = startNode.Id;
                            //     var relationship = item.Values[REL_PARAM] as Neo4j.Driver.V1.IRelationship;
                            //     var endNode = item.Values[VALUE_PARAM] as Neo4j.Driver.V1.INode;
                            //     e.Data.Add(relationship.Type, endNode.Labels);
                            // }
                            // entityList.Add(e);

                            var relationship = item.Values[REL_PARAM] as Neo4j.Driver.V1.IRelationship;
                            var relationshipProperty = relationship.Properties.Single().As<KeyValuePair<string, object>>().Value.ToString();
                            header.Add(relationshipProperty);
                            var value = item.Values[VALUE_PARAM] as Neo4j.Driver.V1.INode;
                            var valueProperty = value.Properties.Single().As<KeyValuePair<string, object>>().Value.ToString();
                            csvData.Add(valueProperty);
                        }
                        csvData.Add("\n");
                    }
                }
            }
            catch(Exception ex)
            {
                var err = ex.Message;
            }
        }
    }
}
using Neo4j.Driver.V1;

namespace Yuffie.WebApp.Models {

    public class Graph
    {
        private IDriver _db;

        public Graph(string uri, string login, string password)
        {
            _db = GraphDatabase.Driver(uri, AuthTokens.Basic(login, password));
        }

        
    }
}
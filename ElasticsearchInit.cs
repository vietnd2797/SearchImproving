using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nest;
using Elasticsearch.Net;
using System.Security.Cryptography.X509Certificates;

namespace SearchImproving
{
    public class ElasticsearchInit
    {
        private readonly List<string> indexNames = new List<string>()
        {
                "questions",
                "questions_alt"
        };
        public List<ElasticClient> ElasticsearchClient()
        {

            var uri = new Uri("http://localhost:9200");

            var connectionPool = new SingleNodeConnectionPool(uri);

            List<ElasticClient> clients = new List<ElasticClient>();
            foreach (var indexName in indexNames)
            {
                var settings = new ConnectionSettings(connectionPool).DefaultIndex(indexName);
                clients.Add(new ElasticClient(settings));
            }    

            //var client = new ElasticClient(settings);
            return clients;
        }

    }
}

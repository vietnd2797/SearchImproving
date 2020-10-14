using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Nest;
using SearchImproving.Models;
using Newtonsoft.Json;
using MongoDB.Bson;
using MongoDB.Driver;



namespace SearchImproving.Controllers
{
    public class HomeController : Controller
    {
        
        private readonly ILogger<HomeController> _logger;
        private readonly ElasticsearchInit _initializer;


        private readonly MongoClient mongoClient;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
            _initializer = new ElasticsearchInit();
            mongoClient = new MongoClient("mongodb://localhost:27017");
        }


        public async Task<IActionResult> Index()
        {
            List<ElasticClient> _clients = _initializer.ElasticsearchClient();

            if ((await _clients.ElementAt(0).IndexExistsAsync("questions")).Exists)
            {
                await _clients.ElementAt(0).DeleteIndexAsync("questions");
            }

            var createIndexResponse1 = _clients.ElementAt(0).CreateIndex("questions", c => c
                                                 .Settings(s => s
                                                     .NumberOfReplicas(0)
                                                     .NumberOfShards(1)
                                                     .Analysis(a => a
                                                        .Analyzers(an => an
                                                            .Custom("my_analyzer", ca => ca
                                                                .CharFilters("html_strip")
                                                                .Tokenizer("vi_tokenizer")
                                                                .Filters("lowercase")))))
                                                  .Mappings(m => m
                                                     .Map<Question>(mm => mm
                                                        .AutoMap()
                                                        .Properties(p => p
                                                            .Text(t => t
                                                                .Name(n => n.Content)
                                                                .Analyzer("my_analyzer"))))));

            if ((await _clients.ElementAt(1).IndexExistsAsync("questions_alt")).Exists)
            {
                await _clients.ElementAt(1).DeleteIndexAsync("questions_alt");
            }

            var createIndexResponse2 = _clients.ElementAt(1).CreateIndex("questions_alt", c => c
                                             .Settings(s => s
                                                 .NumberOfReplicas(0)
                                                 .NumberOfShards(1)
                                                 .Analysis(a => a
                                                    .Analyzers(an => an
                                                        .Custom("my_analyzer_alt", ca => ca
                                                            .CharFilters("html_strip")
                                                            .Tokenizer("standard")
                                                            .Filters("lowercase")))))
                                              .Mappings(m => m
                                                 .Map<Question>(mm => mm
                                                    .AutoMap()
                                                    .Properties(p => p
                                                        .Text(t => t
                                                            .Name(n => n.Content)
                                                            .Analyzer("my_analyzer_alt"))))));


            //var indexResponse1 = await _clients.ElementAt(0).IndexManyAsync(questions.getQuestions1());
            //var indexResponse2 = await _clients.ElementAt(1).IndexManyAsync(questions.getQuestions2());

            IMongoDatabase db = mongoClient.GetDatabase("Question");

            IMongoCollection<BsonDocument> collection1 = db.GetCollection<BsonDocument>("questions");

            List<BsonDocument> documents1 = collection1.Find(new BsonDocument()).ToList();

            List<Question> questions1 = new List<Question>();

            foreach (var doc in documents1)
            {
                Question question = new Question()
                {
                    ID = (int)doc.GetElement("ID").Value,
                    Content = (string)doc.GetElement("Content").Value,
                    Tokenizer = (string)doc.GetElement("Tokenizer").Value,
                    Timestamp = (string)doc.GetElement("Timestamp").Value
                };
                questions1.Add(question);
                var indexResponse = await _clients.ElementAt(0).IndexAsync(question);
                if (!indexResponse.IsValid)
                {
                    var errorMsg = "Problem inserting document to Elasticsearch!";
                    _logger.LogError(indexResponse.OriginalException, errorMsg);
                    throw new Exception(errorMsg);
                }
                
            }

            IMongoCollection<BsonDocument> collection2 = db.GetCollection<BsonDocument>("questions_alt");

            List<BsonDocument> documents2 = collection2.Find(new BsonDocument()).ToList();

            List<Question> questions2 = new List<Question>();

            foreach (var doc in documents2)
            {
                Question question = new Question()
                {
                    ID = (int)doc.GetElement("ID").Value,
                    Content = (string)doc.GetElement("Content").Value,
                    Tokenizer = (string)doc.GetElement("Tokenizer").Value,
                    Timestamp = (string)doc.GetElement("Timestamp").Value
                };
                questions2.Add(question);
                var indexResponse = await _clients.ElementAt(1).IndexAsync(question);
                if (!indexResponse.IsValid)
                {
                    var errorMsg = "Problem inserting document to Elasticsearch!";
                    _logger.LogError(indexResponse.OriginalException, errorMsg);
                    throw new Exception(errorMsg);
                }

            }


            //if (!indexResponse1.IsValid || !indexResponse2.IsValid)
            //{
            //var errorMsg = "Problem inserting document to Elasticsearch!";
            //_logger.LogError(indexResponse1.OriginalException, errorMsg);
            //_logger.LogError(indexResponse2.OriginalException, errorMsg);
            //throw new Exception(errorMsg);
            //}

            var viewModel = new HomeViewModel
            {
                InsertedData = JsonConvert.SerializeObject(questions1.Concat(questions2).ToList(), Formatting.Indented)
            };
            return View(viewModel);
        }
        
       


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

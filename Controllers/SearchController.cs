using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Nest;
using SearchImproving.Models;


namespace SearchImproving.Controllers
{
    public class SearchController : Controller
    {
        private readonly ILogger<SearchController> _logger;
        private readonly ElasticsearchInit _initializer;
        // GET: SearchController

        public SearchController(ILogger<SearchController> logger)
        {
            _logger = logger;
            _initializer = new ElasticsearchInit();
        }

        [HttpGet]
        public async Task<IActionResult> Index(string key)
        {
            List<ElasticClient> _clients = _initializer.ElasticsearchClient();
            if (string.IsNullOrEmpty(key))
            {
                var emptyViewModel = new SearchViewModel
                {
                    Term = "[No results]"
                };
                return View(emptyViewModel);
            }
            
       
            var response1 = await _clients.ElementAt(0).SearchAsync<Question>(s => s
                                 .Index("questions")
                                 .Size(1000)
                                 .Query(q => q
                                  .MatchPhrase(m => m
                                  .Field(f => f.Content).Query(key))));

            var response2 = await _clients.ElementAt(0).SearchAsync<Question>(s => s
                     .Index("questions")
                     .Size(1000)
                     .Query(q => q
                     .Match(m => m
                     .Field(f => f.Content).Query(key))));
            
            //var analyzeResponse1 = await _clients.ElementAt(0).AnalyzeAsync(a => a
                     //.Index("questions")
                     //.Analyzer("my_analyzer")
                     //.Text(key));

            var analyzeResponse2 = await _clients.ElementAt(1).AnalyzeAsync(a => a
                     .Index("questions_alt")
                     .Analyzer("my_analyzer_alt")
                     .Text(key));



            //List<string> tokens = new List<string>();

            //List<string> words = new List<string>();

            //List<int> counts = new List<int>();

            //foreach (var analyzeToken in analyzeResponse2.Tokens)
            //{
                //tokens.Add(analyzeToken.Token);
            //}    

            //for (int i = 0; i < tokens.Count(); i++)
            //{
                //if (!words.Exists(cond => cond.Equals(tokens.ElementAt(i))))
                //{
                    //words.Add(tokens.ElementAt(i));
                    //int count = 1;
                    //for (int j = i + 1; j < tokens.Count(); j++)
                    //{
                        //if (tokens.ElementAt(j) == tokens.ElementAt(i))
                        //{
                            //count++;
                        //}
                    //}
                    //counts.Add(count);
                //}
                //if (i == tokens.Count() - 1 && !words.Exists(cond => cond.Equals(tokens.ElementAt(i))))
                //{
                    //words.Add(tokens.ElementAt(i));
                    //counts.Add(1);
                //}    
            //}

            //var dic = words.Zip(counts, (x, y) => new { x, y }).ToDictionary(d => d.x, d => d.y);
            //dic = dic.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
            //List<string> sortedWords = dic.Keys.ToList();

            List<ISearchResponse<Question>> responses3 = new List<ISearchResponse<Question>>();

            foreach (var analyzeToken in analyzeResponse2.Tokens)
            {
                var response3 = await _clients.ElementAt(1).SearchAsync<Question>(s => s
                          .Index("questions_alt")
                          .Size(1000)
                            .Query(q => q
                            .Match(m => m
                            .Field(f => f.Content).Query(analyzeToken.Token))));

                responses3.Add(response3);
            }

          
            List<IHit<Question>> results = new List<IHit<Question>>();

            foreach (var item in response1.Hits.ToList())
            {
                results.Add(item);
            }

            foreach (var item in response2.Hits.ToList())
            {
                results.Add(item);
            }

            
            foreach (var response3 in responses3)
            {
                foreach (var item in response3.Hits.ToList())
                {
                    results.Add(item);
                }
            }


            List<IHit<Question>> last_results = new List<IHit<Question>>();

            if (results.Count() > 0)
            {
                for (int j = 0; j < results.Count(); j++)
                {
                    if (!last_results.Exists(cond => cond.Source.ID.Equals(results.ElementAt(j).Source.ID)))
                    {
                        last_results.Add(results.ElementAt(j));
                    }
                }
            }


            var viewModel = new SearchViewModel
            {
                Term = key
            };

            string errorMsg = "Problem searching Elasticsearch for term {0}";

            if (response1.IsValid && response2.IsValid && responses3.TrueForAll(r => r.IsValid) && analyzeResponse2.IsValid)
            {
                viewModel.Results = last_results.Select(s => s.Source).ToList();
            }
            else if (!response1.IsValid)
            {
                _logger.LogError(response1.OriginalException, errorMsg, key);
                throw new Exception(errorMsg);

            }
            else if (!response2.IsValid)
            {
                _logger.LogError(response2.OriginalException, errorMsg, key);
                throw new Exception(errorMsg);
            }
            else if (!analyzeResponse2.IsValid)
            {
                _logger.LogError(analyzeResponse2.OriginalException, errorMsg, key);
                throw new Exception(errorMsg);
            }
            else //if (!responses2.TrueForAll(r => r.IsValid))
            {
                for (int i = 0; i < responses3.Count(); i++)
                {
                    if (!responses3.ElementAt(i).IsValid)
                    {
                        _logger.LogError(responses3.ElementAt(i).OriginalException, errorMsg, key);
                        throw new Exception(errorMsg);

                    }
                }
            }

            return View(viewModel);

        }


    }
}

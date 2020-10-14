using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SearchImproving.Models
{
    public class SearchViewModel
    {
        public string Term { get; set; }
        public List<Question> Results { get; set; }
       
    }
}

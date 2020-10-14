using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SearchImproving.Models
{
    public class Question
    {
        public int ID { get; set; }
      
        public string Content { get; set; }
       // public string Score { get; set; }
        public string Tokenizer { get; set; }
        public string Timestamp { get; set; } = DateTime.Now.ToString();
    }
}

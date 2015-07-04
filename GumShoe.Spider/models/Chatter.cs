using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GumShoe.Spider.models
{
    public class Chatter
    {
        public string Keyword { get; set; }
        public DateTime Date { get; set; }
        public long PagesProcessed { get; set; }
        public long Volume { get; set; } // counts can be adjusted by thier location
        
        public Chatter()
        {
            Volume = 0;
            Date = DateTime.Now;
        }
    }
}

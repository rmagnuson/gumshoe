using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace GumShoe.Spider.models
{
    public class WebNode
    {
        public Uri NodeUri { get; set; }
        public HtmlDocument Page { get; set; }
        public int CrawlAttempts { get; set; }
        public Boolean IsCrawled { get; set; }
        public Boolean IsParsed { get; set; }
        public Boolean IsClue { get; set; }

        public WebNode()
        {
            IsCrawled = false;
            IsParsed = false;
            IsClue = false;
            CrawlAttempts = 0;
            
        }
    }
}

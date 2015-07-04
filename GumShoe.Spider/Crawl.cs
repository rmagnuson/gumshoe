using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GumShoe.Spider.models;
using HtmlAgilityPack;

namespace GumShoe.Spider
{
    public class Crawl
    {
        private int MaxCrawlAttempts = 2;
        private int SecondsDelay = 5;

        private const bool LeaveSeed = false;

        private Uri _seedUri;
        public ObservableCollection<WebNode> WebNodes;
        public List<String> Keywords;
        public ObservableCollection<Chatter> Chatters;
        public long UncrawledCount;

        private DateTime _startTime;

        public void Seed(string urlToStart, string keyword, int maxAttempts, int secondsDelay)
        {
            try
            {
                _seedUri = new Uri(urlToStart);
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Invalid URI supplied as seed", ex);
            }
            if (!_seedUri.IsWellFormedOriginalString() || !_seedUri.IsAbsoluteUri)
            {
                throw new ArgumentException("Invliad URI supplied as seed");
            }
            var seedNode = new WebNode { NodeUri = new Uri(urlToStart) };
            WebNodes.Add(seedNode);
            Keywords.Add(keyword);
            var chatter = new Chatter()
            {
                Keyword = keyword
            };
            Chatters.Add(chatter);
            MaxCrawlAttempts = maxAttempts;
            SecondsDelay = secondsDelay;
        }

        public void Start()
        {
            Continue();
        }

        public void Continue()
        {
            List<WebNode> uncrawled = WebNodes.Where(w => w.IsCrawled == false && w.CrawlAttempts < MaxCrawlAttempts).ToList();
            foreach (var node in uncrawled)
            {
                // Make sure we're not hitting the site too fast
                var delayTime = Math.Abs(Convert.ToDouble((DateTime.Now - _startTime).TotalSeconds));
                if (delayTime < SecondsDelay)
                {
                    Task.Delay((int)(SecondsDelay - delayTime) * 1000); // nonblocking Wait
                }
                // Get the document
                var doc = GetDocument(node);
                _startTime = DateTime.Now;
                // pull relevant links from it
                AddNodes(doc);
                // parse it and count Keywords
                ParseForKeyword(doc, node);
                // Mark it crawled
                node.IsCrawled = true;
                node.CrawlAttempts++;

                foreach (string word in Keywords)
                {
                    Chatters.First(c => c.Keyword == word).PagesProcessed ++;
                }
            }
            UncrawledCount = WebNodes.Count(w => w.IsCrawled == false && w.CrawlAttempts < MaxCrawlAttempts);
        }

        public Crawl()
        {
            WebNodes = new ObservableCollection<WebNode>();
            Keywords = new List<string>();
            Chatters = new ObservableCollection<Chatter>();
            _startTime = new DateTime();
            _startTime = DateTime.Now;
            UncrawledCount = 0;
        }

        private HtmlDocument GetDocument(WebNode node)
        {
            var htmlWeb = new HtmlWeb();
            Console.WriteLine(node.NodeUri.AbsoluteUri);
            var doc = htmlWeb.Load(node.NodeUri.AbsoluteUri);
            return doc;
        }

        private void AddNodes(HtmlDocument doc)
        {
            var links = doc.DocumentNode.SelectNodes(".//a[@href]");  // standard anchor links
            foreach (var link in links) //HtmlNode
            {
                var href = link.GetAttributeValue("href", string.Empty);
                if (href == string.Empty)
                {
                    continue; // skip this one
                }
                Uri newUri;
                try
                {
                    newUri = new Uri(href, UriKind.RelativeOrAbsolute);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Unrepairable Uri 1: " + href + " error: " + ex.Message);
                    continue; // This is a bad URL.
                }

                if (!newUri.IsWellFormedOriginalString())
                {
                    Console.WriteLine("Unrepairable Uri 2: " + href);
                    continue;
                }

                if (!newUri.IsAbsoluteUri)
                {
                    var absUri = new Uri(_seedUri, newUri);
                    if (absUri.IsAbsoluteUri)
                    {
                        newUri = absUri; // we fixed the uri
                    }
                    else
                    {
                        Console.WriteLine("Unrepairable Uri 3: " + href);
                        continue; // Skip, we cannot fix it.
                    }
                }
                else if ((newUri.GetLeftPart(UriPartial.Authority) != _seedUri.GetLeftPart(UriPartial.Authority)) & (!LeaveSeed))
                {
                    continue;  // This new Uri is not in the same domain and leaveSeed = false so skip this one.
                }
                if (DontHaveThisLink(newUri))
                {
                    WebNodes.Add(new WebNode()
                    {
                        NodeUri = newUri
                    });
                }
            }
        }

        private bool DontHaveThisLink(Uri newUri)
        {
            foreach (var node in WebNodes)
            {
                if (node.NodeUri.AbsoluteUri == newUri.AbsoluteUri)
                {
                    return false;
                }
            }
            return true;
        }

        private void ParseForKeyword(HtmlDocument doc, WebNode node)
        {
            List<String> keywordList = new List<string>();
            var textNodes = doc.DocumentNode.SelectNodes("//text()");
            foreach (HtmlNode text in textNodes)
            {
                var word = text.InnerText.Trim();
                if (word == "")
                {
                    continue;
                }
                if (word.IndexOf(" ", StringComparison.InvariantCultureIgnoreCase) > -1) // more than one word in this text node
                {
                    var allwords = word.Split(' ');
                    foreach (var w in allwords)
                    {
                        if (w.Length > 1)
                        {
                            keywordList.Add(w.ToLower());
                        }
                    }
                }
                else  // just one word in this text node
                {
                    keywordList.Add(text.InnerText);
                }
            }
            foreach (string word in Keywords)
            {
                var thisWord = word.ToLower();
                var volume = keywordList.Count(k => k.Contains(thisWord));
                Chatters.First(c => c.Keyword == word).Volume += volume;
            }
            node.IsParsed = true;
            
        }

    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Data.SqlTypes;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GumShoe.DAL;
using GumShoe.Spider.models;
using HtmlAgilityPack;

namespace GumShoe.Spider
{
    public class Crawl
    {
        #region Private Properties
        private int _maxCrawlAttempts = 2;
        private int _secondsDelay = 5;
        private int _maxSteps = 0;
        private long _snapShotId = 0;
        private const bool LeaveSeed = false;
        private Uri _seedUri;
        private DateTime _startTime;
        private SnapShot _snapShot;
        private List<string> _seedPageWords; 
        #endregion

        #region Public Properties
        public ObservableCollection<WebNode> WebNodes;
        public long UncrawledCount;
        #endregion

        #region Public Members
        /// <summary>
        /// Instantiate the crawl, reset all counters
        /// </summary>
        public Crawl()
        {
            WebNodes = new ObservableCollection<WebNode>();
            _startTime = new DateTime();
            _startTime = DateTime.Now;
            UncrawledCount = 0;
        }

        /// <summary>
        /// Sets up the crawl with all the parameters it needs to continue until finished.
        /// </summary>
        /// <param name="urlToStart">The valid url to start crawling from</param>
        /// <param name="maxAttempts">Number of times to try failed pages (not implemented)</param>
        /// <param name="secondsDelay">Number of seconds to wait between page loads</param>
        /// <param name="steps">Number of steps away from the urlToStart to transverse before stopping</param>
        /// <param name="databaseFileName">Name of the file to store data in</param>
        public void Seed(string urlToStart, int maxAttempts, int secondsDelay, int steps, string databaseFileName)
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

            _maxCrawlAttempts = maxAttempts;
            _secondsDelay = secondsDelay;
            _maxSteps = steps;

            var dbSetup = new Database();
            dbSetup.ConnectToDatabase(databaseFileName);
            _snapShot = new SnapShot(databaseFileName);
            _snapShotId = _snapShot.InsertSnapShot(urlToStart, secondsDelay, steps);
        }

        /// <summary>
        /// Start the crawling session, checks for uncrawled content before continuing to MaxSteps
        /// </summary>
        public void Start()
        {
            var step = 0;
            while (step < _maxSteps)
            {
                Continue();
                if (UncrawledCount == 0)
                {
                    break; // nothing left to crawl, just bail
                }
                step++;
            }
        }
        #endregion

        

        
        /// <summary>
        /// Iterates through saved Urls one step
        /// </summary>
        private void Continue()
        {
            List<WebNode> uncrawled = WebNodes.Where(w => w.IsCrawled == false && w.CrawlAttempts < _maxCrawlAttempts).ToList();
            foreach (var node in uncrawled)
            {
                // Make sure we're not hitting the site too fast
                var delayTime = Math.Abs(Convert.ToDouble((DateTime.Now - _startTime).TotalSeconds));
                if (delayTime < _secondsDelay)
                {
                    Task.Delay((int)(_secondsDelay - delayTime) * 1000); // nonblocking Wait
                }
                // Get the document
                var doc = GetDocument(node);
                _startTime = DateTime.Now;
                // pull out the ugly bits
                doc = CleanDocument(doc);
                // pull relevant links from it
                AddNodes(doc);
                // save the text to the database
                var wordList = GetWordList(doc);
                if (_seedPageWords == null)
                {
                    // this is the seed page grab this wordlist for later comparison
                    _seedPageWords = wordList;
                }
                else
                {
                    wordList = CleanWordList(wordList);
                }
                var cleanText = String.Join(" ", wordList);

                SaveToDatabase(node.NodeUri, cleanText);
                node.IsParsed = true;
                node.IsCrawled = true;
                node.CrawlAttempts++;
            }
            UncrawledCount = WebNodes.Count(w => w.IsCrawled == false && w.CrawlAttempts < _maxCrawlAttempts);
        }

        private bool SaveToDatabase(Uri uri, string text)
        {
            var pageContentId = _snapShot.InsertPageContent(_snapShotId, uri, text);
            return pageContentId != 0;
        }

        private static HtmlDocument GetDocument(WebNode node)
        {
            var htmlWeb = new HtmlWeb();
            var doc = htmlWeb.Load(node.NodeUri.AbsoluteUri);
            return doc;
        }

        private void AddNodes(HtmlDocument doc)
        {
            var links = doc.DocumentNode.SelectNodes(".//a[@href]");  // standard anchor links
            foreach (var link in links) //HtmlNode
            {
                var href = link.GetAttributeValue("href", string.Empty);
                Uri newUri = CleanHref(href);
                if (newUri == null)
                {
                    continue; // skip this one, we weren't able to figure it out
                }
                if (!HaveLink(newUri))
                {
                    WebNodes.Add(new WebNode()
                    {
                        NodeUri = newUri
                    });
                }
            }
        }

        /// <summary>
        /// Check to see if we already have this link in our WebNodes
        /// </summary>
        /// <param name="newUri">The new link</param>
        /// <returns>If this link is already stored returns true else false</returns>
        private bool HaveLink(Uri newUri)
        {
            foreach (var node in WebNodes)
            {
                if (node.NodeUri.AbsoluteUri == newUri.AbsoluteUri)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// removes Head, Script, Style tags and Comments to get to the content easier
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        private static HtmlDocument CleanDocument(HtmlDocument doc)
        {
            doc.DocumentNode.Descendants()
                .Where(n => n.Name == "head" || n.Name == "script" || n.Name == "style" || n.Name == "#comment")
                .ToList()
                .ForEach(n => n.Remove());
            return doc;
        }

        private List<string> GetWordList(HtmlDocument doc)
        {
            var text = new StringBuilder();
            var textNodes = doc.DocumentNode.SelectNodes("//text()");
            foreach (HtmlNode textNode in textNodes)
            {
                var textBlock = textNode.InnerText.Trim();
                if (textBlock == "")
                {
                    continue; // skip empty text blocks
                }
                text.Append(textBlock + " ");
            }
            var cleanText = CleanText(text.ToString());
            return cleanText.Split(' ').ToList();
        }

        private List<string> CleanWordList(List<string> wordList)
        {
            foreach (var currentWord in _seedPageWords)
            {
                if (wordList[0] == currentWord)
                {
                    // this page has the same first word(s) as the seed page, remove this word
                    wordList.RemoveAt(0);
                }
                else
                {
                    // As soon as there is a difference bail.
                    break;
                }
            }
            for (var c = _seedPageWords.Count-1; c > 0; c--)
            {
                var currentWord = _seedPageWords[c];
                if (wordList[wordList.Count - 1] == currentWord)
                {
                    // Same last word(s), remove this word
                    wordList.RemoveAt(wordList.Count - 1);
                }
                else
                {
                    break;
                }
            }
            return wordList;
        }

        /// <summary>
        /// Get the text from the document. It will be stored as word separated text with punctuation still in place
        /// </summary>
        /// <param name="doc">The document to pull words from</param>
        /// <param name="node">The WebNode that this document is in reference to</param>
        /// <returns>Word separated text with punctuation still in place</returns>
        private string GetText(HtmlDocument doc, WebNode node)
        {
            var text = new StringBuilder();
            var textNodes = doc.DocumentNode.SelectNodes("//text()");
            foreach (HtmlNode textNode in textNodes)
            {
                var textBlock = textNode.InnerText.Trim();
                if (textBlock == "")
                {
                    continue; // skip empty text blocks
                }
                text.Append(textBlock + " ");
            }
            node.IsParsed = true;
            var cleanText = CleanText(text.ToString());

            return cleanText;
        }



        private string CleanText(string textBlock)
        {
            var cleanText = RemoveDoubleSpaces(textBlock);
            // There could be more cleanup functions to come.
            return cleanText;
        }

        private static string RemoveDoubleSpaces(string textBlock)
        {
            StringBuilder cleanStringBuilder = new StringBuilder();
            char lastChar = ' ';
            foreach (char c in textBlock.ToCharArray())
            {
                if (c == ' ' && lastChar == ' ')
                {
                    continue; // skip double spaces;
                }
                cleanStringBuilder.Append(c);
                lastChar = c;
            }
            return cleanStringBuilder.ToString();
        }

        private Uri CleanHref(string href)
        {
            Uri cleanHref;
            if (href == string.Empty)
            {
                return null;
            }
            try
            {
                cleanHref = new Uri(href, UriKind.RelativeOrAbsolute);
            }
            catch (Exception)
            {
                return null;
            }
            if (!cleanHref.IsWellFormedOriginalString())
            {
                return null;
            }
            if (!cleanHref.IsAbsoluteUri)
            {
                var absUri = new Uri(_seedUri, cleanHref);
                if (absUri.IsAbsoluteUri)
                {
                    cleanHref = absUri; // we fixed the uri
                }
                else
                {
                    return null;
                }
            }
            else if ((cleanHref.GetLeftPart(UriPartial.Authority) != _seedUri.GetLeftPart(UriPartial.Authority)) & (!LeaveSeed))
            {
                return null; // This new Uri is not in the same domain and leaveSeed = false so skip this one.
            }
            return cleanHref;
        }

    }
}

using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using AngleSharp.Html.Parser;
using HtmlAgilityPack;
using OpenQA.Selenium;
using System.Text.RegularExpressions;

namespace ProgrammingTaskCrawler
{
    public class Crawler
    {
        public static List<CrawlerData> GetText(string html, string url)
        {
            List<CrawlerData> list = new List<CrawlerData>();
            var parser = new HtmlParser();
            var text = parser.ParseDocument(html);
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            foreach (var item in text.QuerySelectorAll("div.wrap")) //Find the Type
            {
                if (item.TextContent.Contains("Type:")) {
                    list.Add( new CrawlerData { Name="Type", Content=Regex.Replace(item.TextContent.Replace("\n", string.Empty), @"\s+", " ").Split(":")[1].Trim() }); 
                }

            }

            foreach (var item in text.QuerySelectorAll("dl"))//Find the Assignee
            {
                if (item.TextContent.Contains("Assignee:")) { 
                    list.Add(new CrawlerData { Name = "Assignee", Content = item.TextContent.Replace("\n", string.Empty).Split(":")[1].Trim() }); 
                }

            }
            IWebDriver driver = new OpenQA.Selenium.Chrome.ChromeDriver();
            driver.Navigate().GoToUrl(url);

            //Find the created time
            var time = driver.FindElement(By.Id("created-val")).FindElement(By.TagName("time")).GetAttribute("datetime");
            TimeSpan t = DateTime.Parse(time) - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            list.Add (new CrawlerData { Name = "Created", Content = time + "，" + t.TotalSeconds + $"， where {t.TotalSeconds} is the epoch" });

            foreach (var item in text.QuerySelectorAll("div.user-content-block")) //Find the Description
            {
                list.Add(new CrawlerData { Name = "Description", Content = Regex.Replace(item.TextContent.Replace("\n", string.Empty).Replace(",", "，"), @"\s+", " ").Trim() });

            }

            //Get Comments on the dynamic web page
            list.Add(new CrawlerData { Name = "Comments", Content = Regex.Replace(driver.FindElement(By.Id("issue_actions_container")).Text.Replace(",","，"), @"\s+", " ").Trim() });
            return list;


        }
        public CookieContainer CookiesContainer { get; set; }//Define cookie container
        public async Task<string> StartUp(String url, string proxy = null)
        {
            return await Task.Run(() =>
            {
                var pageSource = String.Empty;
                try
                {

                    var request = (HttpWebRequest)WebRequest.Create(url);
                    request.Accept = "*/*";
                    request.ServicePoint.Expect100Continue = false;//Speed up loading
                    request.ServicePoint.UseNagleAlgorithm = false;//Disable Nagle algorithm to speed up loading
                    request.AllowWriteStreamBuffering = false;//Disable buffering to speed up loading
                    request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");//Define gzip compression page support
                    request.ContentType = "application/x-www-form-uriencoded";//Define document and encoding type
                    request.AllowAutoRedirect = false;//Prohibit auto redirection
                    //Set up User-Agent, pretending to be Google Chrome browser
                    request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/50.0.2661.102 Safari/537.36";
                    request.Timeout = 5000;//Define the request timeout time as 5 seconds
                    request.KeepAlive = true;//Enable keep-alive
                    request.Method = "GET";//Request Type: GET        
                    if (proxy != null) request.Proxy = new WebProxy(proxy);//Set the proxy server IP, disguise the request address
                    request.CookieContainer = this.CookiesContainer;//Add cookie container
                    request.ServicePoint.ConnectionLimit = int.MaxValue;//Define max connections
                    using (var response = (HttpWebResponse)request.GetResponse())
                    {//Get request response

                        foreach (System.Net.Cookie cookie in response.Cookies) this.CookiesContainer.Add(cookie);//keep login

                        if (response.ContentEncoding.ToLower().Contains("gzip"))//Unzip
                        {
                            using (GZipStream stream = new GZipStream(response.GetResponseStream(), CompressionMode.Decompress))
                            {
                                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                                {
                                    pageSource = reader.ReadToEnd();
                                }
                            }
                        }
                        else if (response.ContentEncoding.ToLower().Contains("deflate"))//Unzip
                        {
                            using (DeflateStream stream = new DeflateStream(response.GetResponseStream(), CompressionMode.Decompress))
                            {
                                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                                {
                                    pageSource = reader.ReadToEnd();
                                }

                            }
                        }
                        else
                        {
                            using (Stream stream = response.GetResponseStream())//Origin
                            {
                                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                                {

                                    pageSource = reader.ReadToEnd();
                                }
                            }
                        }
                    }
                    request.Abort();

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    return "Error";
                }
                return pageSource;
            });
        }

        public class CrawlerData
        {
            public string Name { get; set; }
            public string Content { get; set; }
        }
    }
}

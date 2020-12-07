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
using System.Linq;
using System.Threading;
using System.Text.RegularExpressions;
using OpenQA.Selenium.Support.UI;
using AngleSharp;

namespace ProgrammingTaskCrawler
{
    public class Crawler
    {
        public async static Task<CrawlerData> GetText(string html)
        {
            var items = new CrawlerData();

            try
            {
                var context = BrowsingContext.New(Configuration.Default);
                var document = await context.OpenAsync(req => req.Content(html));       //parse the stringified html
                Console.WriteLine("Version: {0}", document.All.Where(m => m.LocalName == "a" && m.ClassList.Contains("issue-link")).FirstOrDefault().TextContent.Split(" ")[0].Trim());
                //get the issue report version
                items.Version = document.All.Where(m => m.LocalName == "a" && m.ClassList.Contains("issue-link")).FirstOrDefault().TextContent.Split(" ")[0].Trim();

                //find the Type and Assignee
                foreach (var item in document.QuerySelectorAll("span")) 
                {
                    if (item.Id == "type-val")
                    {
                        //Console.WriteLine("Type: {0}", item.TextContent.Trim());
                        items.Type = item.TextContent.Trim();

                    }
                    else if (item.Id == "assignee-val")
                    {
                        //Console.WriteLine("Assignee: {0}", item.TextContent.Trim());
                        items.Assignee = item.TextContent.Trim();
                    }

                }
                //Console.WriteLine("Type: {0}", document.All.Where(m => m.LocalName == "span" && m.Id=="type-val").FirstOrDefault().TextContent.Trim());
                //Console.WriteLine("Assignee: {0}", document.All.Where(m => m.LocalName == "span" && m.Id== "assignee-val").FirstOrDefault().TextContent.Trim());

                //Console.WriteLine("Created: {0}", document.All.Where(m => m.LocalName == "time" && m.ClassName== "livestamp").FirstOrDefault().GetAttribute("datetime").Trim());
                //find out the created time
                items.Created = document.All.Where(m => m.LocalName == "time" && m.ClassName == "livestamp").FirstOrDefault().GetAttribute("datetime").Trim();
                
                //Console.WriteLine("Created_Epoch: {0}， where {0} is the epoch", (DateTime.Parse(document.All.Where(m => m.LocalName == "time" && m.ClassName == "livestamp").FirstOrDefault().GetAttribute("datetime").Trim()) - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds);
                //find out the created time epoch
                items.CreatedEpoch = $"{(DateTime.Parse(items.Created) - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds}， where {(DateTime.Parse(items.Created) - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds} is the epoch";

                //obtain the description if empty in web => " "
                if (document.All.Where(m => m.LocalName == "div" && m.ClassName == "user-content-block").Count() > 0)
                {
                    //Console.WriteLine("Description: {0}", Regex.Replace(document.All.Where(m => m.LocalName == "div" && m.ClassName == "user-content-block").FirstOrDefault().TextContent.Replace("\n", string.Empty).Replace(",", "，"), @"\s+", " ").Trim());
                    items.Description = Regex.Replace(document.All.Where(m => m.LocalName == "div" && m.ClassName == "user-content-block").FirstOrDefault().TextContent.Replace("\n", string.Empty).Replace(",", "，"), @"\s+", " ").Trim();
                }
                else
                {
                    //Console.WriteLine("Description: '' ");
                    items.Description = "";
                }


                //Console.WriteLine("Comments: {0}", Regex.Replace(document.All.Where(m => m.LocalName == "div" && m.ClassName == "issuePanelContainer").FirstOrDefault().TextContent.Replace(",", "，"), @"\s+", " ").Trim());
                //get the comment
                items.Comments = Regex.Replace(document.All.Where(m => m.LocalName == "div" && m.ClassName == "issuePanelContainer").FirstOrDefault().TextContent.Replace(",", "，"), @"\s+", " ").Trim();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message.Trim());
            }

            return items;


        }

        public class CrawlerData
        {
            public string Version { get; set; }
            public string Type { get; set; }
            public string Assignee { get; set; }
            public string Created { get; set; }
            public string CreatedEpoch { get; set; }
            public string Description { get; set; }
            public string Comments { get; set; }
        }
    }
}

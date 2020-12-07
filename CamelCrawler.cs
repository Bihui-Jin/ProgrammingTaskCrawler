using AngleSharp.Html.Parser;
using HtmlAgilityPack;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using static ProgrammingTaskCrawler.Crawler;

namespace ProgrammingTaskCrawler
{
    class CamelCrawler
    {
        public static async Task Main(string[] args)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            var crawler = new Crawler();
            List<CrawlerData> list = new List<CrawlerData>();
            var toCSV = new ToCSV();
            string targetURL = "https://issues.apache.org/jira/projects/CAMEL/issues/CAMEL-15922?filter=allissues";
            var options = new ChromeOptions();
            options.AddArgument("headless");                                //do not open the browser
            var driverService = ChromeDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;                   //remove the log form the console
            IWebDriver driver = new ChromeDriver(driverService,options);
            //IWebDriver driver = new ChromeDriver();

            driver.Navigate().GoToUrl(targetURL);

            driver.FindElement(By.Id("subnav-trigger")).Click();                                //click the button Switch filter 
            driver.FindElement(By.XPath("//a[contains(@data-item-id,'allissues')]")).Click();   //chose the option All issues 

            var oldHtml = driver.PageSource;

            while (driver.FindElements(By.XPath("//button[@id='next-issue']")).Count != 0)  //run only when the next button is there
            {
                list.Add(await GetText(driver.PageSource));

                driver.Manage().Timeouts().AsynchronousJavaScript = TimeSpan.FromSeconds(3);
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(4);
                driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(4);
                DefaultWait<IWebDriver> fluentWait = new DefaultWait<IWebDriver>(driver);
                fluentWait.Timeout = TimeSpan.FromSeconds(4);
                fluentWait.PollingInterval = TimeSpan.FromSeconds(4);                                   //give it seconds to load
                new WebDriverWait(driver, TimeSpan.FromSeconds(10)).Until(e=> e.PageSource != oldHtml);  //go next untill the new DOM elements are there

                
                if (driver.FindElements(By.XPath("//button[@id='next-issue']")).Count != 0) driver.FindElement(By.XPath("//button[@id='next-issue']")).Click(); //go to the next issue report
                oldHtml = driver.PageSource;
            }
            
            toCSV.ExportCSV(list);
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            Console.WriteLine("RunTime: " + String.Format("{0:00}:{1:00}:{2:00}.{3:00}",ts.Hours, ts.Minutes, ts.Seconds,ts.Milliseconds / 10));
            //foreach (var i in list)
            //{
            //    Console.WriteLine("Version: {0}\nType: {1}\nAssignee: {2}\nCreated: {3}\nCreatedEpoch: {4}\nDescription: {5}\nComments: {6}", i.Version, i.Type, i.Assignee, i.Created, i.CreatedEpoch, i.Description, i.Comments);
            //}
        }
    }


}

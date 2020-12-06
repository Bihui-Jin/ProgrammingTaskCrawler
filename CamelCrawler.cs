using System;
using System.Linq;
using System.Threading.Tasks;

namespace ProgrammingTaskCrawler
{
    class CamelCrawler
    {
        public static async Task Main(string[] args)
        {
            var crawler = new Crawler();
            var toCSV = new ToCSV();
            string targetURL = "https://issues.apache.org/jira/browse/CAMEL-10597";
            //string targetURL = "https://issues.apache.org/jira/browse/AGILA-44"; //test
            var html = await crawler.StartUp(targetURL, null);
            var list = Crawler.GetText(html, targetURL);
            toCSV.ExportCSV(list);
        }
    }

  
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static ProgrammingTaskCrawler.Crawler;

namespace ProgrammingTaskCrawler
{
    public class ToCSV
    {
        public void ExportCSV(List<CrawlerData> list)
        {

            try
            {
                if (list == null)
                {
                    throw new ArgumentNullException(nameof(list));
                }

                StringBuilder csvText = new StringBuilder();
                StringBuilder csvrowText = new StringBuilder();
                foreach (var i in list)
                {
                    csvrowText.Append(",");
                    csvrowText.Append(i.Name);
                }
                csvText.AppendLine(csvrowText.ToString().Substring(1)); //headers
                csvrowText = new StringBuilder();
                foreach (var d in list)
                {
                    csvrowText.Append(",");
                    csvrowText.Append($"{d.Content}");
                }
                csvText.AppendLine(csvrowText.ToString().Substring(1)); //body

                File.WriteAllText("ApacheCrawler.csv", csvText.ToString(), Encoding.UTF8);//Export the file
            }
            catch (Exception ex)
            {
                Console.WriteLine("Export .csv file fail!" + ex.Message);
            }
        }
	}
}

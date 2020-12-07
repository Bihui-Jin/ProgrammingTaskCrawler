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
            list.RemoveAt(0);
            try
            {
                if (list == null)
                {
                    throw new ArgumentNullException(nameof(list));
                }

                StringBuilder csvText = new StringBuilder();
                StringBuilder csvrowText = new StringBuilder();
                
                csvText.AppendLine("Version,Assignee,Created,Created Epoch,Description,Comments"); //headers
                
                foreach (var d in list)
                {
                    csvrowText.Append($"{d.Version},{d.Assignee},{d.Created},{d.CreatedEpoch},{d.Description},{d.Comments}\n");
                }
                csvText.AppendLine(csvrowText.ToString()); //body

                File.WriteAllText("ApacheCrawler.csv", csvText.ToString(), Encoding.UTF8);//Export the file
            }
            catch (Exception ex)
            {
                Console.WriteLine("Export .csv file fail!" + ex.Message);
            }
        }
    }
}

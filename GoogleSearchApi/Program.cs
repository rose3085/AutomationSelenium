using Newtonsoft.Json.Linq;
using HtmlAgilityPack;
using System;
using System.Net;

namespace GoogleSearchApi
{ 
class Program
    {
       
        static async Task Main(string[] args)
        {
            System.Net.ServicePointManager.SecurityProtocol =
    System.Net.SecurityProtocolType.Tls12 | System.Net.SecurityProtocolType.Tls13;


            Console.Write("Enter search query: ");
            string query = Console.ReadLine();
            string googleSearchApiKey = "your-google-search-api-key";
            string searchEngineId = "your-search-engine-id";
           string url = $"https://www.googleapis.com/customsearch/v1?key={googleSearchApiKey}&cx={searchEngineId}&q={Uri.EscapeDataString(query)}";

            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await  client.GetAsync(url);
                string json = await response.Content.ReadAsStringAsync();

                JObject data = JObject.Parse(json);

                Console.WriteLine("\nSearch Results:\n");
              // var text = await ScrapData("https://www.greatnepaltreks.com/nepal-tourism-statistic-2024/");
                foreach (var item in data["items"])
                {
                    Console.WriteLine("Title: " + item["title"]);
                    Console.WriteLine("Link: " + item["link"]);
                    Console.WriteLine("Snippet: " + item["snippet"]);
                    Console.WriteLine();
                    var text = await ScrapData((string)item["link"],query);
                   // var text = await ScrapData("https://nepalindata.com", "");
                    var messageDto = new LLMFormatter.MessageDto
                    {
                        Text = text,
                        Data = new List<LLMFormatter.OutputJson>
                    {
                        new LLMFormatter.OutputJson { JsonField = "Place Visited" },
                        new LLMFormatter.OutputJson { JsonField = "No of Tourists" },
                        new LLMFormatter.OutputJson { JsonField = "Country" }
                    }
                    };
                    var result = await LLMFormatter.FormatMessage(messageDto);
                    Console.WriteLine(result);
                }
              
            }

        }
        public static async Task<string> ScrapData(string link,string Query)
        {
            var dataList = new List<ScrapperModel>();

            var web = new HtmlWeb();
            var handler = new HttpClientHandler
            {
                AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
            };
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
            try
            {
                string html = await httpClient.GetStringAsync(link);
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(html);
               string text = await FilterExtractedText(htmlDoc, Query);
                ExtractTables(htmlDoc);
                // Extract all inner text from the body
                return text;
            }
            catch (Exception ex)
            {
                // If fetching fails, just skip and continue
                Console.WriteLine($"⚠️ Skipping link due to error: {ex.Message}");
                return null;
            }
            //return dataList;
        }

        public static async Task<string> ExtractAllText(HtmlDocument htmlDoc)
        {
            var bodyNode = htmlDoc.DocumentNode.SelectSingleNode("//body");
            if (bodyNode != null)
            {
                string text = bodyNode.InnerText;

                // Clean up whitespace
                text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ").Trim();

                Console.WriteLine("\n--- Extracted Text ---\n");
                Console.WriteLine(text);
                return text;
            }
            else
            {
                Console.WriteLine("No <body> tag found.");
                return null;
            }
        }

        public static async Task<string> FilterExtractedText(HtmlDocument htmlDoc,string query)
        {
            var paragraphs = htmlDoc.DocumentNode.SelectNodes("//p");
            foreach (var p in paragraphs)
            {
                string text = p.InnerText.Trim();
                if (text.Contains(query))
                {
                    Console.WriteLine(text);
                    return text;
                }
                else { return null; }
                
            }
            return null;

        }
        public static void ExtractTables(HtmlDocument htmlDoc)
        {
            var tables = htmlDoc.DocumentNode.SelectNodes("//table");
            foreach (var table in tables)
            {
                foreach (var row in table.SelectNodes(".//tr"))
                {
                    var cells = row.SelectNodes(".//th|.//td");
                    if (cells != null)
                    {
                        foreach (var cell in cells)
                        {
                            Console.Write(cell.InnerText.Trim() + "\t");
                        }
                        Console.WriteLine();
                    }
                }
                Console.WriteLine("----- End of Table -----");
            }
        }

        public static async void WebScrapper(string link)
        {
            //using (HttpClient client = new HttpClient())
            //{
            //   // var url = link.ToUri;
            //    string html = await client.GetStringAsync(link);

            //    Console.WriteLine(html); // prints full page HTML
            //}
            var web = new HtmlWeb();
            var doc = await web.LoadFromWebAsync(link);

            string textContent = doc.DocumentNode.InnerText;
            Console.WriteLine(textContent);
        }


    }

}

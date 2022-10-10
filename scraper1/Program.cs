using HtmlAgilityPack;
using System.Globalization;
using Fizzler.Systems.HtmlAgilityPack;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using CsvHelper;

namespace Castascraper
{
    class Program
    {
        private static int Pages { get; set; } = 0;
        private static HtmlDocument MainHtml { get; set; } = new HtmlDocument();
        private static string RentalPage { get; set;} = "https://classifieds.castanet.net/cat/rentals/";
        private static int CurrentPage { get; set; } = 1;
        static async Task Main(string[] args)
        {
            CultureInfo culture = new CultureInfo("en-US");
            var html = await GetHtml(RentalPage);
            MainHtml.LoadHtml(html);
            Pages = CountPages(MainHtml);
            List<Listing> adInfo = new();
            
            while(CurrentPage <= Pages)
            {
                Console.Write($"\rGetting page: {CurrentPage}/{Pages}");
                string pageModifier = $"?p=";
                adInfo.AddRange(GetAds(MainHtml));
                CurrentPage++;
                html = await GetHtml(RentalPage +pageModifier + CurrentPage);
                MainHtml.LoadHtml(html);
            }
            Console.WriteLine($"{Environment.NewLine}Getting individual rental details.");
            for (int i = 0; i < adInfo.Count; i++)
            {
                Console.Write($"\rProgress: {i}/{adInfo.Count - 1}");
                html = await GetHtml(adInfo[i].Link);
                string[] details = ((string[])GetDetails(html));
                adInfo[i].SetBedsBaths(details[0], details[1]);
                adInfo[i].SetCity(details[2]);
                adInfo[i].SetType(details[3]);
                adInfo[i].Link = "https://castanet.net" + adInfo[i].Link;
            }
            MakeCSV(adInfo);
        }
        private static Task<string> GetHtml(string link)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://classifieds.castanet.net/");
            return client.GetStringAsync(link);
        }
        private static List<Listing> GetAds(HtmlDocument htmlDoc)
        {
            List<Listing> listingList = new List<Listing>();
            //selects the individual ad nodes
            var ads = htmlDoc.DocumentNode
                .SelectNodes("//a")
                .Where(x => x.GetClasses().Contains("prod_container"))
                .Select(x => x);

            foreach (var ad in ads)
            {
                var name = ad.Descendants("h2").FirstOrDefault().InnerText;
                var price = ad.Descendants("div").Where(x => x.GetClasses().Contains("price")).FirstOrDefault().InnerText;
                var link = ad.Attributes["href"].Value.ToString();
                listingList.Add(new Listing(name, link, price));
            }
            return listingList;
        }
        public static Array GetDetails(string html)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);
            var bedsBaths = htmlDoc.DocumentNode
                .SelectNodes("//div")
                .Where(x => x.HasClass("prod_right"))
                .Select(x => x.Descendants("tr")).ToList();

            string beds = bedsBaths[0].ElementAt(0).Elements("td").ElementAt(1).InnerText.ToString().Trim();
            string baths = bedsBaths[0].ElementAt(1).Elements("td").ElementAt(1).InnerText.ToString().Trim();

            var location = htmlDoc.DocumentNode
                .SelectNodes("//div")
                .Where(x => x.HasClass("prod_left"))
                .Select(x => x.Descendants("tr")).ToList();
            string city = location[0].ElementAt(2).Elements("td").ElementAt(1).InnerText.ToString().Trim().Split(",")[0];
            string type = GetHouseType(htmlDoc);
            string[] array = new[] { beds, baths, city, type };
            return array;
        }
        private static int CountPages(HtmlDocument html)
        {
            var nav = html.DocumentNode.SelectNodes("//div")
                .Where(x => x.HasClass("navigation"))
                .Select(x => x.Descendants("a").SkipLast(1));

            int pageCount = int.Parse(nav.First().ElementAt(6).InnerText.ToString());
            Console.WriteLine($"Found {pageCount} pages of ads");
            return pageCount;
        }
        public static string GetHouseType(HtmlDocument html)
        {
            var crumbs = from c in html.DocumentNode.Descendants("a")
                         where c.HasClass("ccrumbs")
                         select c.InnerText;
            string type;
            if (crumbs.Any())
            {
                type = crumbs.First().ToString();
                return type;
            }
            return "none";
        }
        public static void MakeCSV(List<Listing> list)
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            Console.WriteLine($"{Environment.NewLine}Writing csv file");
            using (var writer = new StreamWriter($"{path}\\AllListings.csv"))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteHeader<Listing>();
                csv.NextRecord();
                int count = 1;
                foreach(Listing item in list)
                {
                    csv.WriteRecord(item);
                    csv.NextRecord();
                    count++;
                }
            }
        }
    }
}


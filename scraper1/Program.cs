using HtmlAgilityPack;
using Fizzler.Systems.HtmlAgilityPack;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace scraper1
{
    class Program
    {
        private static string Pages { get; set; } = "";
        static async Task Main(string[] args)
        {
            string rentalPage = "https://classifieds.castanet.net/cat/rentals/";
            var html = await GetHtml(rentalPage);
            List<Listing> adInfo = GetAds(html);
            for(int i = 0; i < adInfo.Count; i++)
            {
                html = await GetHtml(adInfo[i].Link);
                string[] details = ((string[])GetDetails(html));
                adInfo[i].setBedsBaths(details[0], details[1]);
                Console.WriteLine($"{adInfo[i].Description} {Environment.NewLine}{adInfo[i].Bedrooms} beds {Environment.NewLine}{adInfo[i].Baths} baths{Environment.NewLine}{adInfo[i].Price.Trim()}{Environment.NewLine}");
            }
        }
        private static Task<string> GetHtml(string link)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://classifieds.castanet.net/");
            return client.GetStringAsync(link);
        }

        private static List<Listing> GetAds(string html)
        {
            List<Listing> listingList = new List<Listing>();
            var htmlDoc  = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            //finds max pages of listings
            var nav = htmlDoc.DocumentNode.SelectNodes("//div")
                .Where(x => x.HasClass("navigation"))
                .Select(x => x.Descendants("a").SkipLast(1));

            Pages = nav.First().ElementAt(6).InnerText.ToString();

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
                listingList.Add(new Listing(name, "3","0",link, price));
            }
            return listingList;
        }
        public static Array GetDetails(string html)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);
            var details = htmlDoc.DocumentNode
                .SelectNodes("//div")
                .Where(x => x.HasClass("prod_right"))
                .Select(x => x.Descendants("tr")).ToList();

            string beds = details[0].ElementAt(0).Elements("td").ElementAt(1).InnerText.ToString().Trim();
            string baths = details[0].ElementAt(1).Elements("td").ElementAt(1).InnerText.ToString().Trim();
            string[] array = new[] { beds, baths };
            return array;
        }
        private int CountPages(HtmlDocument html)
        {
            return 0;
        }

    }

}


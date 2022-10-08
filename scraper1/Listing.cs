using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace scraper1
{
    internal class Listing
    {
        public string Description { get; } = "";
        public string Bedrooms { get; set; } = "0";
        public string Link { get; } = "";
        public string Price { get; } = "0";
        public string Baths { get; set; } = "0";
        public Listing(string desc, string beds, string baths, string link, string price)
        {
            Description = desc;
            Bedrooms = beds;
            Link = link;
            Price = price;
        }

        public void setBedsBaths(string beds, string baths)
        {
            Bedrooms = beds;
            Baths = baths;
        }
    }
}

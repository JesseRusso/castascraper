﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Castascraper
{
    internal class Listing
    {
        public string Description { get; } = "";
        public string HouseType { get; set; } = "string";
        public string Bedrooms { get; set; } = "0";
        public string Baths { get; set; } = "0";
        public string City { get; set; } = "";
        public string Price { get; } = "0";
        public string Link { get; set; } = "";
        public Listing(string desc, string link, string price, string city)
        {
            Description = desc;
            Link = link;
            Price = price;
            City = city;
        }

        public void SetBedsBaths(string beds, string baths)
        {
            Bedrooms = beds;
            Baths = baths;
        }
        public void SetCity(string city)
        {
            City = city;
        }
        public void SetType(string type)
        {
            HouseType = type;
        }
        public string GetHyperlink()
        {
            return $"=HYPERLINK(\"https://classifieds.castanet.net{Link}\", \"Link to listing\")";
        }
    }
}

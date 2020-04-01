using Nest;
using System;
using System.Collections.Generic;

namespace ProductElasticSearch.Models
{
    public class User
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public string IPAddress { get; set; }
        public GeoIp GeoIp { get; set; }
    }

    public class GeoIp
    {
        public string CityName { get; set; }
        public string ContinentName { get; set; }
        public string CountryIsoCode { get; set; }
        public GeoLocation Location { get; set; }
        public string RegionName { get; set; }
    }
}
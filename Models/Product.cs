using System;
using System.Collections.Generic;

namespace ProductElasticSearch.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Ean { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Brand Brand { get; set; }
        public Category Category { get; set; }
        public Store Store { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; }
        public int Quantity { get; set; }
        public float Rating { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string Image { get; set; }
        public List<Review> Reviews { get; set; }
    }
}
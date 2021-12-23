﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Mango.Web.Models
{
    public class ProductDto
    {
        public ProductDto()
        {
            Count = 1;
        }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public double Price { get; set; }
        public string ProductDescription { get; set; }
        public string CategoryName { get; set; }
        public string ImageUrl { get; set; }
        [Range(1, 100)]
        public int Count { get; set; }
    }
}
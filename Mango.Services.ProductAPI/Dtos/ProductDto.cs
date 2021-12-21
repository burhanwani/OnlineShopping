namespace Mango.Services.ProductAPI.Dtos
{
    public class ProductDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string Price { get; set; }
        public string ProductDescription { get; set; }

        public string CategoryName { get; set; }

        public string ImageUrl { get; set; }
    }
}

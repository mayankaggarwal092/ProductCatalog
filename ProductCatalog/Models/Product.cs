namespace ProductCatalog.Models
{
    public class Product
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public required string Name { get; set; }

        public required string Description { get; set; }

        public string Category { get; set; }

        public string Brand { get; set; }

        public List<string> Tags { get; set; } = [];

        public decimal Price { get; set; }

        public double Rating { get; set; }
    }
}

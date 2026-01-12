using System;

namespace HardwareShopPOS.Models
{
    public class ItemModel
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public Guid CategoryId { get; set; }
        public decimal CostPrice { get; set; }
        public decimal RetailPrice { get; set; }
        public decimal WholesalePrice { get; set; }
        public string UnitOfMeasure { get; set; } = "piece";
        public string? Barcode { get; set; }
        public string? HSNCode { get; set; }
        public int ReorderLevel { get; set; } = 10;
        public string TaxMethod { get; set; } = "exclusive";
        public decimal TaxRate { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}

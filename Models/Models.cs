namespace HardwareShopPOS.Models
{
    public class Employee
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public string? EmployeeCode { get; set; }
        public string? Phone { get; set; }
        public string Role { get; set; } = "staff";
        public Guid? StoreId { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? LastLoginAt { get; set; }
        public string? StoreName { get; set; }
    }

    public class Store
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class Category
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class Item
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public Guid? CategoryId { get; set; }
        public decimal CostPrice { get; set; }
        public decimal RetailPrice { get; set; }
        public decimal WholesalePrice { get; set; }
        public string UnitOfMeasure { get; set; } = "piece";
        public string? Barcode { get; set; }
        public int ReorderLevel { get; set; } = 10;
        public decimal TaxRate { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public string? CategoryName { get; set; }
        public decimal StockQuantity { get; set; }
    }

    public class Customer
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public string Type { get; set; } = "retail";
        public string? Phone { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class SalesRetail
    {
        public Guid Id { get; set; }
        public string InvoiceNumber { get; set; } = "";
        public DateTime InvoiceDate { get; set; }
        public DateTime SaleDate { get; set; }
        public Guid? CustomerId { get; set; }
        public Guid StoreId { get; set; }
        public Guid? EmployeeId { get; set; }
        public string? PaymentMethod { get; set; }
        public string PaymentStatus { get; set; } = "unpaid";
        public decimal Subtotal { get; set; }
        public decimal Discount { get; set; }
        public decimal Tax { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public string? CustomerName { get; set; }
        public string? EmployeeName { get; set; }
        public List<SalesRetailItem> Items { get; set; } = new();
    }

    public class SalesRetailItem
    {
        public Guid Id { get; set; }
        public Guid SalesRetailId { get; set; }
        public Guid ItemId { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal TaxValue { get; set; }
        public decimal NetValue { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
    }

    public class CartItem
    {
        public Guid ItemId { get; set; }
        public string ItemCode { get; set; } = "";
        public string ItemName { get; set; } = "";
        public string UnitOfMeasure { get; set; } = "piece";
        public decimal UnitPrice { get; set; }
        public decimal Quantity { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal DiscountValue => (UnitPrice * Quantity * DiscountPercent) / 100;
        public decimal TaxRate { get; set; }
        public decimal TaxValue => (UnitPrice * Quantity - DiscountValue) * TaxRate / 100;
        public decimal NetValue => (UnitPrice * Quantity) - DiscountValue + TaxValue;
        public decimal AvailableStock { get; set; }
    }
}

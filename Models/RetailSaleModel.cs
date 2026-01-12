using System;
using System.Collections.Generic;

namespace HardwareShopPOS.Models
{
    public class RetailSaleModel
    {
        public Guid Id { get; set; }
        public string InvoiceNumber { get; set; } = "";
        public DateTime InvoiceDate { get; set; } = DateTime.Now;
        public DateTime SaleDate { get; set; } = DateTime.Now.Date;
        public Guid CustomerId { get; set; }
        public Guid StoreId { get; set; }
        public Guid EmployeeId { get; set; }
        public string PaymentMethod { get; set; } = "cash";
        public string PaymentStatus { get; set; } = "paid";
        public decimal Subtotal { get; set; }
        public decimal Discount { get; set; }
        public decimal Tax { get; set; }
        public decimal TotalAmount { get; set; }
        public string Description { get; set; } = "";
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public string? CustomerName { get; set; }
        public string? StoreName { get; set; }
        public string? EmployeeName { get; set; }
        public List<RetailSaleItemModel> Items { get; set; } = new();
    }

    public class RetailSaleItemModel
    {
        public Guid Id { get; set; }
        public Guid SalesRetailId { get; set; }
        public Guid ItemId { get; set; }
        public string? BatchNo { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal TaxValue { get; set; }
        public decimal NetValue { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public string? ItemName { get; set; }
        public string? ItemCode { get; set; }
    }

    public class RetailSaleLineItem
    {
        public Guid ItemId { get; set; }
        public string ItemCode { get; set; } = "";
        public string ItemName { get; set; } = "";
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountPercent { get; set; } = 0;
        public decimal DiscountValue { get; set; }
        public decimal TaxValue { get; set; }
        public decimal NetValue { get; set; }

        public void CalculateTotals(decimal taxRate = 0)
        {
            DiscountValue = (UnitPrice * Quantity * DiscountPercent) / 100;
            var subtotal = (UnitPrice * Quantity) - DiscountValue;
            TaxValue = (subtotal * taxRate) / 100;
            NetValue = subtotal + TaxValue;
        }
    }
}

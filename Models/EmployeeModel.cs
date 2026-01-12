using System;

namespace HardwareShopPOS.Models
{
    public class EmployeeModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public string? EmployeeCode { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string Role { get; set; } = "staff";
        public Guid? StoreId { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? LastLoginAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}

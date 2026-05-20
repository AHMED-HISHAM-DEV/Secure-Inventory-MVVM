using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WpfApp1.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        public string Barcode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int StockLevel { get; set; }
        public int MinStockLevel { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal CostPrice { get; set; }
        public System.DateTime? ExpiryDate { get; set; }
        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
        public bool IsLowStock => StockLevel <= MinStockLevel;
        public List<WarehouseStock> WarehouseStocks { get; set; } = new();
    }
} 
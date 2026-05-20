using System;
using System.ComponentModel.DataAnnotations;

namespace WpfApp1.Models
{
    public enum TransactionType { In, Out, Damaged }

    public class Transaction
    {
        [Key]
        public int Id { get; set; }
        public int ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;
        public TransactionType Type { get; set; }
        public int Quantity { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;

        public string LogEntry => $"[{Timestamp:HH:mm:ss}] {Type.ToString().ToUpper()} | PID: {ProductId} | Qty: {Quantity}";
    }
}
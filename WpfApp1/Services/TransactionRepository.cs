using WpfApp1.Data;
using WpfApp1.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WpfApp1.Services
{
    public class TransactionRepository
    {
        public async Task RecordTransactionAsync(Transaction transaction)
        {
            using var context = new CyberDbContext();

            var product = await context.Products.FindAsync(transaction.ProductId);
            if (product == null) throw new Exception("Product not found.");

            if (transaction.Type == TransactionType.In)
            {
                product.StockLevel += transaction.Quantity;
            }
            else if (transaction.Type == TransactionType.Out || transaction.Type == TransactionType.Damaged)
            {
                if (product.StockLevel < transaction.Quantity)
                    throw new Exception("Insufficient stock!"); 

                product.StockLevel -= transaction.Quantity;
            }

            context.Transactions.Add(transaction);
            context.Products.Update(product);

            await context.SaveChangesAsync();
        }

        public async Task<List<Transaction>> GetLatestLogsAsync()
        {
            using var context = new CyberDbContext();
            return await context.Transactions
                .Include(t => t.User) 
                .OrderByDescending(t => t.Timestamp)
                .Take(50)
                .AsNoTracking()
                .ToListAsync();
        }

        // ---- Financial Insight (تقييم المخزن) ---- //

        public async Task<decimal> CalculateTotalInventoryValueAsync()
        {
            using var context = new CyberDbContext();
            var products = await context.Products.AsNoTracking().ToListAsync();

            return (decimal)products.Sum(p => p.StockLevel * (double)p.CostPrice);
        }

        // حساب الأرباح 
        public async Task<decimal> CalculateExpectedProfitAsync()
        {
            using var context = new CyberDbContext();
            var products = await context.Products.AsNoTracking().ToListAsync();

            return (decimal)products.Sum(p => p.StockLevel * (double)(p.UnitPrice - p.CostPrice));
        }
        public async Task TransferStockAsync(int productId, int fromWarehouseId, int toWarehouseId, int quantity)
        {
            using var context = new CyberDbContext();


            using var dbTransaction = await context.Database.BeginTransactionAsync();

            try
            {

                var sourceStock = await context.WarehouseStocks
                    .FirstOrDefaultAsync(s => s.ProductId == productId && s.WarehouseId == fromWarehouseId);

                if (sourceStock == null || sourceStock.Quantity < quantity)
                    throw new Exception("INSUFFICIENT_STOCK: الرصيد في المخزن المصدر لا يكفي");

                sourceStock.Quantity -= quantity;

                var targetStock = await context.WarehouseStocks
                    .FirstOrDefaultAsync(s => s.ProductId == productId && s.WarehouseId == toWarehouseId);

                if (targetStock == null)
                {
                    context.WarehouseStocks.Add(new WarehouseStock
                    {
                        ProductId = productId,
                        WarehouseId = toWarehouseId,
                        Quantity = quantity
                    });
                }
                else
                {
                    targetStock.Quantity += quantity;
                }


                context.Transactions.Add(new Transaction { ProductId = productId, Quantity = quantity, Type = TransactionType.Out, Timestamp = DateTime.Now, UserId = 1 });
                context.Transactions.Add(new Transaction { ProductId = productId, Quantity = quantity, Type = TransactionType.In, Timestamp = DateTime.Now, UserId = 1 });

                await context.SaveChangesAsync();

                await dbTransaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await dbTransaction.RollbackAsync();
                throw new Exception($"TRANSFER_FAILED: {ex.Message}");
            }
        }
    }
}
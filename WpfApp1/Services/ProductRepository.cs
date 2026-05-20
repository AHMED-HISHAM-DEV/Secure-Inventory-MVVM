using WpfApp1.Data;
using WpfApp1.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WpfApp1.Services
{
    public class ProductRepository
    {
        public async Task<List<Product>> GetAllProductsAsync()
        {
            using var context = new CyberDbContext();
            return await context.Products.AsNoTracking().ToListAsync();
        }

        public async Task<Product?> GetProductByBarcodeAsync(string barcode)
        {
            using var context = new CyberDbContext();
            return await context.Products.FirstOrDefaultAsync(p => p.Barcode == barcode);
        }
        public async Task AddProductAsync(Product product)
        {
            using var context = new CyberDbContext();
            context.Products.Add(product);
            await context.SaveChangesAsync();
        }

        public async Task UpdateProductAsync(Product product)
        {
            using var context = new CyberDbContext();
            context.Products.Update(product);
            await context.SaveChangesAsync();
        }

        public async Task<List<Product>> GetLowStockProductsAsync()
        {
            using var context = new CyberDbContext();
            return await context.Products
                .Where(p => p.StockLevel <= p.MinStockLevel)
                .AsNoTracking()
                .ToListAsync();
        }
        public async Task DeleteProductAsync(int id)
        {
            using var context = new CyberDbContext();
            var product = await context.Products.FindAsync(id);
            if (product != null)
            {
                var transactions = context.Transactions.Where(t => t.ProductId == id);
                context.Transactions.RemoveRange(transactions);

                context.Products.Remove(product);
                await context.SaveChangesAsync();
            }
        }
    }
}
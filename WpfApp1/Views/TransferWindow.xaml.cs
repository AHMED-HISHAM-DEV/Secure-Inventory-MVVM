using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfApp1.Data;
using WpfApp1.Models;
using Microsoft.EntityFrameworkCore;
namespace WpfApp1.Views
{
    /// <summary>
    /// Interaction logic for TransferWindow.xaml
    /// </summary>
    public partial class TransferWindow : Window
    {
        private int _productId;
        private int _sourceWarehouseId;

        public TransferWindow(int productId, int sourceWarehouseId, List<Warehouse> otherWarehouses)
        {
            InitializeComponent();
            _productId = productId;
            _sourceWarehouseId = sourceWarehouseId;
            DestWarehouseCombo.ItemsSource = otherWarehouses;
        }

        private async void Transfer_Click(object sender, RoutedEventArgs e)
        {
            if (DestWarehouseCombo.SelectedItem is not Warehouse destWh || !int.TryParse(QtyTextBox.Text, out int qty)) return;

            using var context = new CyberDbContext();
            // استخدام Transaction لضمان أمان البيانات
            using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                var sourceStock = await context.WarehouseStocks.FirstOrDefaultAsync(ws => ws.ProductId == _productId && ws.WarehouseId == _sourceWarehouseId);

                if (sourceStock == null || sourceStock.Quantity < qty)
                {
                    MessageBox.Show("ERROR: رصيد غير كافٍ في المخزن المصدر!");
                    return;
                }

                // 1. خصم من المصدر
                sourceStock.Quantity -= qty;

                // 2. إضافة للوجهة
                var destStock = await context.WarehouseStocks.FirstOrDefaultAsync(ws => ws.ProductId == _productId && ws.WarehouseId == destWh.Id);
                if (destStock != null) destStock.Quantity += qty;
                else context.WarehouseStocks.Add(new WarehouseStock { ProductId = _productId, WarehouseId = destWh.Id, Quantity = qty });

                await context.SaveChangesAsync();
                await transaction.CommitAsync(); // اعتماد العملية

                MessageBox.Show("TRANSFER_SUCCESSFUL: تم نقل البضاعة بنجاح.");
                this.DialogResult = true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(); // تراجع لو حصل خطأ
                MessageBox.Show($"TRANSFER_FAILED: {ex.Message}");
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => this.Close();
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using WpfApp1.Data;
using WpfApp1.Models;
using WpfApp1.Services;
using WpfApp1.Views;

namespace WpfApp1.ViewModels
{
    public partial class InventoryViewModel : ObservableObject
    {
        private readonly ProductRepository _productRepo = new();
        private readonly TransactionRepository _transRepo = new();
        private List<Product> _allProducts = new();

        // ---------------- الخصائص (Properties) ----------------

        [ObservableProperty] private ObservableCollection<Product> _products = new();
        [ObservableProperty] private Product? _selectedProduct;

        // خصائص الإضافة والتعديل
        [ObservableProperty] private string _newBarcode = string.Empty;
        [ObservableProperty] private string _newProductName = string.Empty;
        [ObservableProperty] private decimal _newCost;
        [ObservableProperty] private decimal _newSellingPrice; // تأكد أن الـ XAML يربط هنا
        [ObservableProperty] private int _newMinStock;
        [ObservableProperty] private Warehouse? _targetWarehouseForNewProduct;
        [ObservableProperty] private Product? _selectedProductForEdit;

        // البحث والفلترة
        [ObservableProperty] private string _searchText = string.Empty;
        [ObservableProperty] private ObservableCollection<Warehouse> _warehouses = new();
        [ObservableProperty] private Warehouse? _selectedWarehouse;
        [ObservableProperty] private int _transactionQuantity = 1;

        public InventoryViewModel()
        {
            _ = LoadInitialData();
        }

        private async Task LoadInitialData()
        {
            await LoadProducts().ConfigureAwait(false);
        }

        partial void OnSelectedWarehouseChanged(Warehouse? value)
        {
            _ = LoadProducts();
        }

        partial void OnSearchTextChanged(string value)
        {
            FilterProducts();
        }

        // ---------------- دوال التحميل والفلترة ----------------

        public async Task LoadProducts()
        {
            try
            {
                using var context = new CyberDbContext();

                // تحميل المخازن
                if (Warehouses.Count == 0)
                {
                    var whs = await context.Warehouses.ToListAsync().ConfigureAwait(false);
                    whs.Insert(0, new Warehouse { Id = 0, Name = "--- GLOBAL_VIEW (ALL) ---" });

                    App.Current.Dispatcher.Invoke(() => {
                        Warehouses = new ObservableCollection<Warehouse>(whs);
                        if (SelectedWarehouse == null) SelectedWarehouse = Warehouses.First();
                    });
                }

                List<Product> resultProducts;

                if (SelectedWarehouse != null && SelectedWarehouse.Id != 0)
                {
                    resultProducts = await context.WarehouseStocks
                        .Where(ws => ws.WarehouseId == SelectedWarehouse.Id)
                        .Include(ws => ws.Product)
                        .Select(ws => new Product
                        {
                            Id = ws.Product!.Id,
                            Name = ws.Product.Name,
                            Barcode = ws.Product.Barcode,
                            CostPrice = ws.Product.CostPrice,
                            UnitPrice = ws.Product.UnitPrice,
                            MinStockLevel = ws.Product.MinStockLevel,
                            StockLevel = ws.Quantity
                        }).ToListAsync().ConfigureAwait(false);
                }
                else
                {
                    resultProducts = await context.Products
                        .Include(p => p.WarehouseStocks)
                        .Select(p => new Product
                        {
                            Id = p.Id,
                            Name = p.Name,
                            Barcode = p.Barcode,
                            CostPrice = p.CostPrice,
                            UnitPrice = p.UnitPrice,
                            MinStockLevel = p.MinStockLevel,
                            StockLevel = p.WarehouseStocks.Sum(ws => ws.Quantity)
                        }).ToListAsync().ConfigureAwait(false);
                }

                _allProducts = resultProducts;
                App.Current.Dispatcher.Invoke(FilterProducts);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"SYSTEM_FAILURE: {ex.Message}", "Cyber Error");
            }
        }

        private void FilterProducts()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                Products = new ObservableCollection<Product>(_allProducts);
            }
            else
            {
                var filtered = _allProducts.Where(p =>
                    (p.Name != null && p.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
                    (p.Barcode != null && p.Barcode.Contains(SearchText))
                ).ToList();
                Products = new ObservableCollection<Product>(filtered);
            }
        }


        [RelayCommand]
        public async Task AddProduct()
        {
            if (SelectedProductForEdit != null) return;

            if (string.IsNullOrWhiteSpace(NewProductName) || TargetWarehouseForNewProduct == null || TargetWarehouseForNewProduct.Id == 0)
            {
                MessageBox.Show("برجاء إدخال الاسم واختيار مخزن حقيقي للبدء.", "Validation Error");
                return;
            }

            if (NewSellingPrice <= NewCost)
            {
                var res = MessageBox.Show("تنبيه: سعر البيع أقل من أو يساوي التكلفة. هل تريد الاستمرار؟", "Financial Alert", MessageBoxButton.YesNo);
                if (res == MessageBoxResult.No) return;
            }

            var newProduct = new Product
            {
                Barcode = NewBarcode,
                Name = NewProductName,
                CostPrice = NewCost,
                UnitPrice = NewSellingPrice,
                MinStockLevel = NewMinStock
            };

            using var context = new CyberDbContext();
            context.Products.Add(newProduct);
            await context.SaveChangesAsync().ConfigureAwait(false);

            context.WarehouseStocks.Add(new WarehouseStock
            {
                ProductId = newProduct.Id,
                WarehouseId = TargetWarehouseForNewProduct.Id,
                Quantity = 0
            });

            await context.SaveChangesAsync().ConfigureAwait(false);
            await LoadProducts();
            App.Current.Dispatcher.Invoke(ClearForm);
        }

        [RelayCommand]
        public void PrepareUpdate(Product product)
        {
            if (product == null) return;
            SelectedProductForEdit = product;
            NewBarcode = product.Barcode;
            NewProductName = product.Name;
            NewCost = product.CostPrice;
            NewSellingPrice = product.UnitPrice;
            NewMinStock = product.MinStockLevel;
        }

        [RelayCommand]
        public async Task UpdateProduct()
        {
            if (SelectedProductForEdit == null) return;

            using var context = new CyberDbContext();
            var dbProduct = await context.Products.FindAsync(SelectedProductForEdit.Id);

            if (dbProduct != null)
            {
                dbProduct.Barcode = NewBarcode;
                dbProduct.Name = NewProductName;
                dbProduct.CostPrice = NewCost;
                dbProduct.UnitPrice = NewSellingPrice;
                dbProduct.MinStockLevel = NewMinStock;

                await context.SaveChangesAsync().ConfigureAwait(false);
                await LoadProducts();
                App.Current.Dispatcher.Invoke(ClearForm);
                MessageBox.Show("DATABASE_UPDATED: تمت مزامنة البيانات بنجاح.");
            }
        }

        [RelayCommand]
        public void ClearForm()
        {
            NewBarcode = string.Empty;
            NewProductName = string.Empty;
            NewCost = 0;
            NewSellingPrice = 0;
            NewMinStock = 0;
            TargetWarehouseForNewProduct = null;
            SelectedProductForEdit = null;
        }

        [RelayCommand]
        public async Task DeleteProduct(Product product)
        {
            if (product == null) return;
            var result = MessageBox.Show($"هل أنت متأكد من حذف [{product.Name}]؟", "تأكيد الحذف", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                await _productRepo.DeleteProductAsync(product.Id).ConfigureAwait(false);
                await LoadProducts();
            }
        }

        [RelayCommand]
        public async Task ProcessTransaction(string type)
        {
            if (SelectedProduct == null || SelectedWarehouse == null || SelectedWarehouse.Id == 0)
            {
                MessageBox.Show("برجاء اختيار منتج ومخزن محدد أولاً.");
                return;
            }

            var transaction = new Transaction
            {
                ProductId = SelectedProduct.Id,
                Quantity = TransactionQuantity,
                Type = type == "In" ? TransactionType.In : TransactionType.Out,
                UserId = 1,
                Timestamp = DateTime.Now
            };

            await _transRepo.RecordTransactionAsync(transaction).ConfigureAwait(false);

            using var context = new CyberDbContext();
            var stock = await context.WarehouseStocks
                .FirstOrDefaultAsync(ws => ws.ProductId == SelectedProduct.Id && ws.WarehouseId == SelectedWarehouse.Id);

            if (stock != null)
            {
                if (type == "In") stock.Quantity += TransactionQuantity;
                else stock.Quantity -= TransactionQuantity;
                await context.SaveChangesAsync().ConfigureAwait(false);
            }

            await LoadProducts();
        }

        [RelayCommand]
        public void ClearWarehouseFilter()
        {
            if (Warehouses.Any()) SelectedWarehouse = Warehouses.First();
        }
        [RelayCommand]
        public async Task OpenTransferDialog(Product product)
        {
            if (product == null || SelectedWarehouse == null || SelectedWarehouse.Id == 0)
            {
                MessageBox.Show("برجاء اختيار مخزن محدد (ليس Global) للبدء بعملية النقل.");
                return;
            }

            var otherWarehouses = Warehouses.Where(w => w.Id != 0 && w.Id != SelectedWarehouse.Id).ToList();

            var dialog = new TransferWindow(product.Id, SelectedWarehouse.Id, otherWarehouses);
            dialog.Owner = Application.Current.MainWindow;

            if (dialog.ShowDialog() == true)
            {
                await LoadProducts(); 
            }
        }
    }
}
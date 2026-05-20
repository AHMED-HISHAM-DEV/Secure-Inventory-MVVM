using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using WpfApp1.Data;
using WpfApp1.Models;
using WpfApp1.Services;
using Microsoft.EntityFrameworkCore;
namespace WpfApp1.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
        // for realtime update with XAML
        private readonly TransactionRepository _transRepo = new();
        [ObservableProperty] private double _totalInventoryValue;
        [ObservableProperty] private double _potentialProfit;
        [ObservableProperty] private int _lowStockCount;
        [ObservableProperty] private ObservableCollection<Warehouse> _warehouses = new();
        [ObservableProperty] private Warehouse? _selectedWarehouse;

        public ObservableCollection<string> TerminalLogs { get; set; } = new();

        public DashboardViewModel()
        {
            _ = LoadDashboardDataAsync();
        }


        //for get data and show it in screen
        public async Task LoadDashboardDataAsync()
        {
            try
            {
                using var context = new CyberDbContext();
                var whs = await context.Warehouses.ToListAsync().ConfigureAwait(false);

                App.Current.Dispatcher.Invoke(() =>
                {
                    Warehouses = new ObservableCollection<Warehouse>(whs);
                    TerminalLogs.Insert(0, $"[SYSTEM] {DateTime.Now:HH:mm:ss} : Dashboard Initialized.");
                });

                await CalculateStats();
                await LoadLogsAsync();
            }
            catch (Exception ex)
            {
                App.Current.Dispatcher.Invoke(() =>
                    TerminalLogs.Insert(0, $"[CRITICAL] {DateTime.Now:HH:mm:ss} : {ex.Message}"));
            }
        }

        private async Task LoadLogsAsync()
        {
            var logs = await _transRepo.GetLatestLogsAsync().ConfigureAwait(false);
            App.Current.Dispatcher.Invoke(() => {
                TerminalLogs.Clear();
                foreach (var log in logs) TerminalLogs.Add(log.LogEntry);
            });
        }

        // execute when user open compobox and change wharehouse
        partial void OnSelectedWarehouseChanged(Warehouse? value)
        {
            _ = CalculateStats();
            TerminalLogs.Insert(0, $"[INFO] {DateTime.Now:HH:mm:ss} : Scope -> {(value?.Name ?? "GLOBAL")}");
        }

        // coonvert method to command for XAML easy understand(button=>Logic)
        [RelayCommand]
        public async Task CalculateStats()
        {
            using var context = new CyberDbContext();

            if (SelectedWarehouse != null && SelectedWarehouse.Id != 0)
            {
                var stocks = await context.WarehouseStocks
                    .Where(ws => ws.WarehouseId == SelectedWarehouse.Id)
                    .Include(ws => ws.Product)
                    .ToListAsync().ConfigureAwait(false);

                TotalInventoryValue = (double)stocks.Sum(ws => (decimal)ws.Quantity * ws.Product!.CostPrice);
                PotentialProfit = (double)stocks.Sum(ws => (ws.Product!.UnitPrice - ws.Product.CostPrice) * (decimal)ws.Quantity);
                LowStockCount = stocks.Count(ws => ws.Quantity <= ws.Product!.MinStockLevel);
            }
            else
            {
                var products = await context.Products.Include(p => p.WarehouseStocks).ToListAsync().ConfigureAwait(false);

                TotalInventoryValue = (double)products.Sum(p => (decimal)p.WarehouseStocks.Sum(ws => ws.Quantity) * p.CostPrice);
                PotentialProfit = (double)products.Sum(p => (decimal)p.WarehouseStocks.Sum(ws => ws.Quantity) * (p.UnitPrice - p.CostPrice));
                LowStockCount = products.Count(p => p.WarehouseStocks.Sum(ws => ws.Quantity) <= p.MinStockLevel);
            }
        }

        [RelayCommand]
        public void ClearFilter() => SelectedWarehouse = Warehouses.FirstOrDefault(w => w.Id == 0);
    }
}
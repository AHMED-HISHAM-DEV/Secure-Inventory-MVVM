using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WpfApp1.Models;
using WpfApp1.Services;
namespace WpfApp1.ViewModels
{
    public partial class TransferViewModel : ObservableObject
    {
        private readonly TransactionRepository _transRepo = new();

        [ObservableProperty] private string _sourceWarehouseName = null!;
        [ObservableProperty] private ObservableCollection<Warehouse> _warehouses = new();
        [ObservableProperty] private Warehouse _selectedTargetWarehouse = null!;
        [ObservableProperty] private int _transferQuantity = 1;

        private int _productId;
        private int _sourceWhId;

        public TransferViewModel(int productId, int sourceWhId, string sourceName, List<Warehouse> allWarehouses)
        {
            _productId = productId;
            _sourceWhId = sourceWhId;
            SourceWarehouseName = sourceName;
            Warehouses = new ObservableCollection<Warehouse>(allWarehouses.Where(w => w.Id != sourceWhId));
        }

        [RelayCommand]
        public async Task Execute()
        {
            if (SelectedTargetWarehouse == null || TransferQuantity <= 0) return;

            try
            {
                await _transRepo.TransferStockAsync(_productId, _sourceWhId, SelectedTargetWarehouse.Id, TransferQuantity);
                MessageBox.Show("TRANSFER_COMPLETED: Nodes Synchronized.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Transfer Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

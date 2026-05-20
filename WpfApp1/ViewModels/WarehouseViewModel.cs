using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using WpfApp1.Data;
using WpfApp1.Models;

namespace WpfApp1.ViewModels
{
    public partial class WarehouseViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<Warehouse> _warehouses = new();

        [ObservableProperty]
        private string _newName = string.Empty;

        [ObservableProperty]
        private string _newLocation = string.Empty;

        public WarehouseViewModel()
        {
            _ = LoadWarehouses();
        }

        public async Task LoadWarehouses()
        {
            using var context = new CyberDbContext();
            var list = context.Warehouses.ToList();
            Warehouses = new ObservableCollection<Warehouse>(list);
        }

        [RelayCommand]
        public async Task AddWarehouse()
        {
            if (string.IsNullOrWhiteSpace(NewName)) return;

            using var context = new CyberDbContext();

            var warehouse = new Warehouse
            {
                Name = NewName,
                Location = NewLocation
            };

            context.Warehouses.Add(warehouse);
            await context.SaveChangesAsync();

            await LoadWarehouses(); 


            NewName = string.Empty;
            NewLocation = string.Empty;
        }

        [RelayCommand]
        public async Task DeleteWarehouse(object? parameter) 
        {
            if (parameter is not Warehouse warehouse) return;

            var result = MessageBox.Show($"حذف المخزن: {warehouse.Name}؟",
                                        "تأكيد الحذف ",
                                        MessageBoxButton.YesNo,
                                        MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                using var context = new CyberDbContext();
                context.Warehouses.Remove(warehouse);
                await context.SaveChangesAsync();
                await LoadWarehouses();
            }
        }
    }
}
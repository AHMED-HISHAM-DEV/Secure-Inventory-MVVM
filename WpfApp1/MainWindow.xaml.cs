using System;
using System.Windows;
using WpfApp1.Views;
using WpfApp1.ViewModels;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // ممكن تشغل الـ Dashboard كصفحة افتراضية عند التشغيل
            ShowDashboard(this, null!);
        }

        private async void ShowDashboard(object sender, RoutedEventArgs e)
        {
            var view = new DashboardView();

            // دلوقت الـ DataContext مش هيكون null لأنه مربوط في الـ XAML
            if (view.DataContext is DashboardViewModel vm)
            {
                await vm.LoadDashboardDataAsync();
            }

            MainContent.Content = view;
        }

        private void ShowInventory(object sender, RoutedEventArgs e)
        {
            // هنا برضه الـ InventoryViewModel هيشتغل لوحده أول ما الصفحة تفتح
            MainContent.Content = new InventoryView();
        }
        private void ShowUsers(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new Views.UserManagementView();
        }

        private async void ShowWarehouses(object sender, RoutedEventArgs e)
        {
            try
            {
                MainContent.Content = new WpfApp1.Views.WarehouseManagementView();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"UI_ERROR: {ex.Message}");
            }
        }
    }
}
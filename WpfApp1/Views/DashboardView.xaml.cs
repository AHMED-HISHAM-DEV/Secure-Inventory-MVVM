using System;
using System.Windows;
using System.Windows.Controls;
using WpfApp1.ViewModels;

namespace WpfApp1.Views
{
    /// <summary>
    /// Interaction logic for DashboardView.xaml
    /// الـ View المسؤولة عن عرض لوحة التحكم السيبرانية
    /// </summary>
    public partial class DashboardView : UserControl
    {
        public DashboardView()
        {
            InitializeComponent();

            // نربط حدث التحميل عشان نحدث البيانات أول ما الصفحة تظهر للمستخدم
            this.Loaded += DashboardView_Loaded;
        }

        private async void DashboardView_Loaded(object sender, RoutedEventArgs e)
        {
            // بنتحقق إن الـ DataContext هو الـ ViewModel المطلوب
            if (this.DataContext is DashboardViewModel vm)
            {
                try
                {
                    // استدعاء ميثود التحميل لتحديث الأرقام والـ Terminal Logs
                    await vm.LoadDashboardDataAsync();
                }
                catch (Exception ex)
                {
                    // سجل الخطأ في الـ Debug في حالة وجود مشكلة في الاتصال بالداتابيز
                    System.Diagnostics.Debug.WriteLine($"[!] DASHBOARD_SYNC_ERROR: {ex.Message}");
                }
            }
        }
    }
}
using System.Windows;
using QuestPDF.Infrastructure;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // كرر ورايا: تفعيل الرخصة المجانية للمجتمع
            QuestPDF.Settings.License = LicenseType.Community;

            base.OnStartup(e);

            // هنا ممكن تحدد شاشة البداية برمجياً لو حبيت
            // Styles.LoginWindow login = new Styles.LoginWindow();
            // login.Show();
        }
    }
}
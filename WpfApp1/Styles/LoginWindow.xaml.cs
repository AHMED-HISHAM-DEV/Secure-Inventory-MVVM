using System;
using System.Linq;
using System.Windows;
using WpfApp1.Data; // عشان لو حبيت تتأكد من اليوزر في الداتابيز مستقبلاً

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window // تأكد إنها Window مش UserControl
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        // ميثود تسجيل الدخول
        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = TxtUser.Text;
            string password = TxtPass.Password;

            using var context = new WpfApp1.Data.CyberDbContext();

            // البحث عن المستخدم في قاعدة البيانات
            var user = context.Users.FirstOrDefault(u => u.Username == username);

            if (user != null)
            {
                // استخدام BCrypt للتحقق من أن الباسورد المكتوب يطابق الـ Hash المحفوظ
                bool isPasswordCorrect = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);

                if (isPasswordCorrect)
                {
                    MainWindow main = new MainWindow();
                    main.Show();
                    this.Close();
                }
                else
                {
                    ShowAccessDenied();
                }
            }
            else
            {
                ShowAccessDenied();
            }
        }

        private void ShowAccessDenied()
        {
            MessageBox.Show("ACCESS_DENIED: Authentication Failed", "Cyber Security Alert",
                            MessageBoxButton.OK, MessageBoxImage.Error);
            TxtPass.Clear();
        }

        // ميثود الخروج من البرنامج بالكامل
        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // ميثود إضافية عشان تقدر تسحب الشاشة بالماوس لأننا لغينا الحواف (Optional)
        protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }
    }
}
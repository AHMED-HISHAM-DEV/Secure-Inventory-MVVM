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
    public partial class UserManagementViewModel : ObservableObject
    {
        [ObservableProperty] private ObservableCollection<User> _users = new();
        [ObservableProperty] private string _newUsername = string.Empty;
        [ObservableProperty] private string _newPassword = string.Empty;
        [ObservableProperty] private UserRole _selectedRole = UserRole.Operator;

        public UserManagementViewModel()
        {
            _ = LoadUsers();
        }

        public async Task LoadUsers()
        {
            using var context = new CyberDbContext();
            var list = context.Users.ToList();
            Users = new ObservableCollection<User>(list);
        }

        [RelayCommand]
        public async Task AddUser()
        {
            if (string.IsNullOrWhiteSpace(NewUsername) || string.IsNullOrWhiteSpace(NewPassword)) return;

            using var context = new CyberDbContext();


            string hashed = BCrypt.Net.BCrypt.HashPassword(NewPassword);

            var user = new User
            {
                Username = NewUsername,
                PasswordHash = hashed,
                Role = SelectedRole
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            await LoadUsers(); 
            NewUsername = NewPassword = string.Empty;
        }

        [RelayCommand]
        public async Task DeleteUser(object? parameter) 
        {
            if (parameter is not User user) return;
            if (user.Username == "root")
            {
                MessageBox.Show("ACCESS_DENIED: Cannot delete Root Admin", "Security Protection",
                                MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }

            var result = MessageBox.Show($"REVOKE ACCESS FOR: {user.Username}?",
                                        "Confirm Security Action",
                                        MessageBoxButton.YesNo,
                                        MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                using var context = new CyberDbContext();
                context.Users.Remove(user);
                await context.SaveChangesAsync();
                await LoadUsers();
            }
        }
    }
}
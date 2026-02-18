using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using POS.UI.Core.Services;

namespace POS.UI.Modules.Users.UserList
{
    public partial class UserFormView : Window, INotifyPropertyChanged
    {
        private readonly UserApiService _service;
        private readonly UserDto? _existingUser;

        public string FormTitle => IsNewUser ? "Add New User" : "Edit User";
        public bool IsNewUser => _existingUser == null;

        private string _username = string.Empty;
        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(); }
        }

        private string _fullName = string.Empty;
        public string FullName
        {
            get => _fullName;
            set { _fullName = value; OnPropertyChanged(); }
        }

        private string _email = string.Empty;
        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); }
        }

        private string _phone = string.Empty;
        public string Phone
        {
            get => _phone;
            set { _phone = value; OnPropertyChanged(); }
        }

        private int _roleId;
        public int RoleId
        {
            get => _roleId;
            set { _roleId = value; OnPropertyChanged(); }
        }

        private bool _isUserActive = true;
        public bool IsUserActive
        {
            get => _isUserActive;
            set { _isUserActive = value; OnPropertyChanged(); }
        }

        public List<RoleDto> Roles { get; }

        public UserFormView(UserDto? existingUser, List<RoleDto> roles)
        {
            InitializeComponent();
            DataContext = this;

            _service = App.ServiceProvider?.GetService(typeof(UserApiService)) as UserApiService
                ?? throw new InvalidOperationException("UserApiService not available");

            _existingUser = existingUser;
            Roles = roles;

            if (existingUser != null)
            {
                Username = existingUser.Username;
                FullName = existingUser.FullName ?? string.Empty;
                Email = existingUser.Email ?? string.Empty;
                RoleId = existingUser.RoleId;
                IsUserActive = existingUser.IsActive;
            }
            else
            {
                RoleId = roles.FirstOrDefault()?.Id ?? 0;
            }
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(Username))
            {
                Components.DialogService.Warning("Validation", "Username is required");
                return;
            }

            if (IsNewUser && string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                Components.DialogService.Warning("Validation", "Password is required");
                return;
            }

            if (RoleId <= 0)
            {
                Components.DialogService.Warning("Validation", "Please select a role");
                return;
            }

            bool success;
            string message;

            if (IsNewUser)
            {
                var dto = new CreateUserDto
                {
                    Username = Username,
                    Password = PasswordBox.Password,
                    FullName = FullName,
                    Email = Email,
                    Phone = Phone,
                    RoleId = RoleId
                };
                (success, message) = await _service.CreateUserAsync(dto);
            }
            else
            {
                var dto = new UpdateUserDto
                {
                    Id = _existingUser!.Id,
                    FullName = FullName,
                    Email = Email,
                    Phone = Phone,
                    RoleId = RoleId,
                    IsActive = IsUserActive
                };
                (success, message) = await _service.UpdateUserAsync(_existingUser.Id, dto);
            }

            if (success)
            {
                Components.DialogService.Info("Success", message);
                DialogResult = true;
                Close();
            }
            else
            {
                Components.DialogService.Error("Error", message);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

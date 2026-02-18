using POS.UI.Core.MVVM;
using POS.UI.Core.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Application = System.Windows.Application;

namespace POS.UI.Modules.Users.UserList
{
    public class UserListViewModel : ViewModelBase
    {
        private readonly UserApiService _service;
        private readonly DispatcherTimer _searchTimer;

        // Collections
        public ObservableCollection<UserDto> Users { get; set; } = new();
        public ObservableCollection<RoleDto> Roles { get; set; } = new();
        private List<UserDto> _allUsers = new();

        // Selection
        private UserDto? _selectedUser;
        public UserDto? SelectedUser
        {
            get => _selectedUser;
            set
            {
                _selectedUser = value;
                OnPropertyChanged();
                ((RelayCommand)EditCommand).RaiseCanExecuteChanged();
                ((RelayCommand)DisableCommand).RaiseCanExecuteChanged();
                ((RelayCommand)ResetPasswordCommand).RaiseCanExecuteChanged();
            }
        }

        // Search
        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                _searchTimer.Stop();
                _searchTimer.Start();
            }
        }

        private bool _showInactive;
        public bool ShowInactive
        {
            get => _showInactive;
            set
            {
                _showInactive = value;
                OnPropertyChanged();
                _ = LoadAsync();
            }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        // Commands
        public ICommand LoadCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ClearCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DisableCommand { get; }
        public ICommand ResetPasswordCommand { get; }

        public UserListViewModel(UserApiService service)
        {
            _service = service;

            _searchTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(400) };
            _searchTimer.Tick += (s, e) =>
            {
                _searchTimer.Stop();
                ApplyDisplayFilter();
            };

            LoadCommand = new RelayCommand(async () => await LoadAsync());
            RefreshCommand = new RelayCommand(async () => await LoadAsync());
            ClearCommand = new RelayCommand(() => { SearchText = string.Empty; ApplyDisplayFilter(); });

            AddCommand = new RelayCommand(async () => await AddAsync());
            EditCommand = new RelayCommand(async () => await EditAsync(), () => SelectedUser != null);
            DisableCommand = new RelayCommand(async () => await DisableAsync(), () => SelectedUser != null);
            ResetPasswordCommand = new RelayCommand(async () => await ResetPasswordAsync(), () => SelectedUser != null);

            _ = LoadAsync();
        }

        private async Task LoadAsync()
        {
            try
            {
                IsLoading = true;
                var roles = await _service.GetAllRolesAsync();
                Roles.Clear();
                foreach (var role in roles) Roles.Add(role);

                var users = await _service.GetAllUsersAsync(ShowInactive);
                _allUsers = users;
                ApplyDisplayFilter();
            }
            catch (Exception ex)
            {
                Components.DialogService.Error("Failed to load users", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ApplyDisplayFilter()
        {
            var filtered = _allUsers.AsEnumerable();

            if (!ShowInactive)
                filtered = filtered.Where(x => x.IsActive);

            if (!string.IsNullOrWhiteSpace(SearchText))
                filtered = filtered.Where(x =>
                    x.Username.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    (x.FullName?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (x.Email?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (x.RoleName?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false));

            Users.Clear();
            foreach (var user in filtered.ToList())
                Users.Add(user);
        }

        private async Task AddAsync()
        {
            var form = new UserFormView(null, Roles.ToList());
            form.Owner = Application.Current?.MainWindow;
            if (form.ShowDialog() == true)
            {
                await LoadAsync();
            }
        }

        private async Task EditAsync()
        {
            if (SelectedUser == null) return;

            var form = new UserFormView(SelectedUser, Roles.ToList());
            form.Owner = Application.Current?.MainWindow;
            if (form.ShowDialog() == true)
            {
                await LoadAsync();
            }
        }

        private async Task DisableAsync()
        {
            if (SelectedUser == null) return;

            var result = Components.DialogService.Confirm("Confirm Disable", $"Disable user '{SelectedUser.Username}'?");
            if (result != MessageBoxResult.Yes) return;

            var (success, message) = await _service.DeleteUserAsync(SelectedUser.Id);
            if (success)
            {
                Components.DialogService.Info("Success", message);
                await LoadAsync();
            }
            else
            {
                Components.DialogService.Error("Failed", message);
            }
        }

        private async Task ResetPasswordAsync()
        {
            if (SelectedUser == null) return;

            var dialog = new ResetPasswordDialog(SelectedUser.Username);
            dialog.Owner = Application.Current?.MainWindow;
            if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.NewPassword))
            {
                var (success, message) = await _service.ResetPasswordAsync(SelectedUser.Id, dialog.NewPassword);
                if (success)
                    Components.DialogService.Info("Success", message);
                else
                    Components.DialogService.Error("Failed", message);
            }
        }
    }
}

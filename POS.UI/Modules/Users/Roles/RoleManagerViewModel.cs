using POS.UI.Core.MVVM;
using POS.UI.Core.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace POS.UI.Modules.Users.Roles
{
    public class RoleManagerViewModel : ViewModelBase
    {
        private readonly UserApiService _service;

        // Collections
        public ObservableCollection<RoleDto> Roles { get; set; } = new();
        public ObservableCollection<PermissionGroupViewModel> PermissionGroups { get; set; } = new();

        // Selection
        private RoleDto? _selectedRole;
        public RoleDto? SelectedRole
        {
            get => _selectedRole;
            set
            {
                _selectedRole = value;
                OnPropertyChanged();
                _ = LoadPermissionsForRoleAsync();
                ((RelayCommand)SavePermissionsCommand).RaiseCanExecuteChanged();
            }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        private bool _hasChanges;
        public bool HasChanges
        {
            get => _hasChanges;
            set { _hasChanges = value; OnPropertyChanged(); ((RelayCommand)SavePermissionsCommand).RaiseCanExecuteChanged(); }
        }

        // Commands
        public ICommand LoadCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand SavePermissionsCommand { get; }
        public ICommand AddRoleCommand { get; }

        public RoleManagerViewModel(UserApiService service)
        {
            _service = service;

            LoadCommand = new RelayCommand(async () => await LoadAsync());
            RefreshCommand = new RelayCommand(async () => await LoadAsync());
            SavePermissionsCommand = new RelayCommand(async () => await SavePermissionsAsync(), () => SelectedRole != null && HasChanges);
            AddRoleCommand = new RelayCommand(async () => await AddRoleAsync());

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

                if (Roles.Count > 0 && SelectedRole == null)
                    SelectedRole = Roles.First();
            }
            catch (Exception ex)
            {
                Components.DialogService.Error("Failed to load roles", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadPermissionsForRoleAsync()
        {
            if (SelectedRole == null) return;

            try
            {
                IsLoading = true;
                var response = await _service.GetPermissionsByRoleIdAsync(SelectedRole.Id);
                
                if (response?.Permissions == null) return;

                // Group permissions by module
                var groups = response.Permissions
                    .GroupBy(p => p.Module)
                    .OrderBy(g => g.Key)
                    .Select(g => new PermissionGroupViewModel
                    {
                        Module = g.Key,
                        Permissions = new ObservableCollection<PermissionItemViewModel>(
                            g.Select(p => new PermissionItemViewModel
                            {
                                Id = p.Id,
                                Code = p.Code,
                                Description = p.Description,
                                IsAssigned = p.IsAssigned,
                                Parent = this
                            }))
                    });

                PermissionGroups.Clear();
                foreach (var group in groups)
                    PermissionGroups.Add(group);

                HasChanges = false;
            }
            catch (Exception ex)
            {
                Components.DialogService.Error("Failed to load permissions", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SavePermissionsAsync()
        {
            if (SelectedRole == null) return;

            try
            {
                IsLoading = true;

                var selectedPermissionIds = PermissionGroups
                    .SelectMany(g => g.Permissions)
                    .Where(p => p.IsAssigned)
                    .Select(p => p.Id)
                    .ToList();

                var (success, message) = await _service.UpdateRolePermissionsAsync(SelectedRole.Id, selectedPermissionIds);

                if (success)
                {
                    Components.DialogService.Info("Success", message);
                    HasChanges = false;
                }
                else
                {
                    Components.DialogService.Error("Failed", message);
                }
            }
            catch (Exception ex)
            {
                Components.DialogService.Error("Failed to save permissions", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task AddRoleAsync()
        {
            // Simple input dialog
            var dialog = new AddRoleDialog();
            dialog.Owner = System.Windows.Application.Current?.MainWindow;
            if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.RoleName))
            {
                var (success, message) = await _service.CreateRoleAsync(dialog.RoleName, dialog.RoleDescription);
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
        }

        public void MarkAsChanged()
        {
            HasChanges = true;
        }
    }

    public class PermissionGroupViewModel
    {
        public string Module { get; set; } = string.Empty;
        public ObservableCollection<PermissionItemViewModel> Permissions { get; set; } = new();
    }

    public class PermissionItemViewModel : ViewModelBase
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public RoleManagerViewModel? Parent { get; set; }

        private bool _isAssigned;
        public bool IsAssigned
        {
            get => _isAssigned;
            set
            {
                _isAssigned = value;
                OnPropertyChanged();
                Parent?.MarkAsChanged();
            }
        }
    }
}

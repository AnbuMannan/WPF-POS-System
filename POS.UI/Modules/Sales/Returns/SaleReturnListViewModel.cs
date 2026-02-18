using POS.Shared.Models;
using POS.UI.Components;
using POS.UI.Core.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace POS.UI.Modules.Sales.Returns;

public class SaleReturnListViewModel : INotifyPropertyChanged
{
    private readonly SaleReturnApiService _service;

    public ObservableCollection<SaleReturnDto> Returns { get; set; } = new();
    public ObservableCollection<SaleReturnDto> FilteredReturns { get; set; } = new();

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set { _searchText = value; OnPropertyChanged(); ApplyFilter(); }
    }

    private SaleReturnDto? _selectedReturn;
    public SaleReturnDto? SelectedReturn
    {
        get => _selectedReturn;
        set { _selectedReturn = value; OnPropertyChanged(); }
    }

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set { _isLoading = value; OnPropertyChanged(); }
    }

    public ICommand RefreshCommand { get; }
    public ICommand AddNewCommand { get; }
    public ICommand ViewCommand { get; }
    public ICommand ProcessCommand { get; }

    public event Action? RequestAddNew;
    public event Action<SaleReturnDto>? RequestView;

    public SaleReturnListViewModel(SaleReturnApiService service)
    {
        _service = service;
        RefreshCommand = new RelayCommand(async _ => await LoadReturnsAsync());
        AddNewCommand = new RelayCommand(_ => RequestAddNew?.Invoke());
        ViewCommand = new RelayCommand(_ => { if (SelectedReturn != null) RequestView?.Invoke(SelectedReturn); }, _ => SelectedReturn != null);
        ProcessCommand = new RelayCommand(async _ => await ProcessSelectedReturn(), _ => SelectedReturn != null && !SelectedReturn.IsProcessed);

        _ = LoadReturnsAsync();
    }

    public async Task LoadReturnsAsync()
    {
        IsLoading = true;
        try
        {
            var returns = await _service.GetAllAsync();
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                Returns.Clear();
                foreach (var r in returns) Returns.Add(r);
                ApplyFilter();
            });
        }
        catch (Exception ex)
        {
            DialogService.Error("Sales Returns", $"Failed to load returns: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ApplyFilter()
    {
        FilteredReturns.Clear();
        var filtered = string.IsNullOrWhiteSpace(SearchText)
            ? Returns
            : new ObservableCollection<SaleReturnDto>(
                Returns.Where(r =>
                    (r.ReturnNumber?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (r.OriginalBillNumber?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (r.CustomerName?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false)));

        foreach (var r in filtered)
            FilteredReturns.Add(r);
    }

    private async Task ProcessSelectedReturn()
    {
        if (SelectedReturn == null || SelectedReturn.IsProcessed) return;

        var result = DialogService.Confirm("Process Return",
            $"Process return {SelectedReturn.ReturnNumber}?\n\nThis will update inventory and cannot be undone.");
        if (result != MessageBoxResult.Yes) return;

        try
        {
            await _service.ProcessAsync(SelectedReturn.ReturnId);
            DialogService.Success("Sales Returns", "Return processed successfully.");
            await LoadReturnsAsync();
        }
        catch (Exception ex)
        {
            DialogService.Error("Sales Returns", $"Failed to process: {ex.Message}");
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public class RelayCommand : ICommand
{
    private readonly Func<object?, Task>? _executeAsync;
    private readonly Action<object?>? _execute;
    private readonly Predicate<object?>? _canExecute;

    public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public RelayCommand(Func<object?, Task> executeAsync, Predicate<object?>? canExecute = null)
    {
        _executeAsync = executeAsync;
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

    public async void Execute(object? parameter)
    {
        if (_executeAsync != null) await _executeAsync(parameter);
        else _execute?.Invoke(parameter);
    }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }
}

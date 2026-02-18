using POS.Shared.Models;
using POS.UI.Components;
using POS.UI.Core.Services;
using POS.UI.Modules.Sales.Returns;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace POS.UI.Modules.Sales.Quotations;

public class QuotationListViewModel : INotifyPropertyChanged
{
    private readonly QuotationApiService _service;

    public ObservableCollection<QuotationDto> Quotations { get; set; } = new();
    public ObservableCollection<QuotationDto> FilteredQuotations { get; set; } = new();

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set { _searchText = value; OnPropertyChanged(); ApplyFilter(); }
    }

    private QuotationDto? _selectedQuotation;
    public QuotationDto? SelectedQuotation
    {
        get => _selectedQuotation;
        set { _selectedQuotation = value; OnPropertyChanged(); }
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
    public ICommand EditCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand ConvertToSaleCommand { get; }

    public event Action? RequestAddNew;
    public event Action<QuotationDto>? RequestEdit;
    public event Action<QuotationDto>? RequestView;

    public QuotationListViewModel(QuotationApiService service)
    {
        _service = service;
        RefreshCommand = new RelayCommand(async _ => await LoadQuotationsAsync());
        AddNewCommand = new RelayCommand(_ => RequestAddNew?.Invoke());
        ViewCommand = new RelayCommand(_ => { if (SelectedQuotation != null) RequestView?.Invoke(SelectedQuotation); }, _ => SelectedQuotation != null);
        EditCommand = new RelayCommand(_ => { if (SelectedQuotation != null) RequestEdit?.Invoke(SelectedQuotation); }, _ => SelectedQuotation != null && SelectedQuotation.Status == "Open");
        DeleteCommand = new RelayCommand(async _ => await DeleteSelected(), _ => SelectedQuotation != null);
        ConvertToSaleCommand = new RelayCommand(async _ => await ConvertToSale(), _ => SelectedQuotation != null && SelectedQuotation.Status == "Open");

        _ = LoadQuotationsAsync();
    }

    public async Task LoadQuotationsAsync()
    {
        IsLoading = true;
        try
        {
            var quotations = await _service.GetAllAsync();
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                Quotations.Clear();
                foreach (var q in quotations) Quotations.Add(q);
                ApplyFilter();
            });
        }
        catch (Exception ex)
        {
            DialogService.Error("Quotations", $"Failed to load: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ApplyFilter()
    {
        FilteredQuotations.Clear();
        var filtered = string.IsNullOrWhiteSpace(SearchText)
            ? Quotations
            : new ObservableCollection<QuotationDto>(
                Quotations.Where(q =>
                    (q.QuotationNumber?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (q.CustomerName?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false)));

        foreach (var q in filtered)
            FilteredQuotations.Add(q);
    }

    private async Task DeleteSelected()
    {
        if (SelectedQuotation == null) return;
        var result = DialogService.Confirm("Delete Quotation", $"Delete quotation {SelectedQuotation.QuotationNumber}?");
        if (result != MessageBoxResult.Yes) return;

        try
        {
            await _service.DeleteAsync(SelectedQuotation.Id);
            DialogService.Success("Quotations", "Quotation deleted.");
            await LoadQuotationsAsync();
        }
        catch (Exception ex)
        {
            DialogService.Error("Quotations", $"Failed: {ex.Message}");
        }
    }

    private async Task ConvertToSale()
    {
        if (SelectedQuotation == null || SelectedQuotation.Status != "Open") return;

        var result = DialogService.Confirm("Convert to Sale",
            $"Convert quotation {SelectedQuotation.QuotationNumber} to a sale?\n\n" +
            $"Amount: {SelectedQuotation.TotalAmount:N2}");
        if (result != MessageBoxResult.Yes) return;

        try
        {
            await _service.ConvertToSaleAsync(SelectedQuotation.Id);
            DialogService.Success("Quotations", "Quotation marked as converted. Please create the sale through Billing.");
            await LoadQuotationsAsync();
        }
        catch (Exception ex)
        {
            DialogService.Error("Quotations", $"Failed: {ex.Message}");
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

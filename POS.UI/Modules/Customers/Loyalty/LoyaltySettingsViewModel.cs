using POS.Shared.Models;
using POS.UI.Components;
using POS.UI.Core.MVVM;
using POS.UI.Core.Services;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace POS.UI.Modules.Customers.Loyalty;

public class LoyaltySettingsViewModel : ViewModelBase
{
    private readonly LoyaltyApiService _service;

    private decimal _pointsPerUnitCurrency;
    public decimal PointsPerUnitCurrency
    {
        get => _pointsPerUnitCurrency;
        set { _pointsPerUnitCurrency = value; OnPropertyChanged(); }
    }

    private decimal _redemptionValuePerPoint;
    public decimal RedemptionValuePerPoint
    {
        get => _redemptionValuePerPoint;
        set { _redemptionValuePerPoint = value; OnPropertyChanged(); }
    }

    private int _minimumRedeemPoints;
    public int MinimumRedeemPoints
    {
        get => _minimumRedeemPoints;
        set { _minimumRedeemPoints = value; OnPropertyChanged(); }
    }

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set { _isLoading = value; OnPropertyChanged(); }
    }

    public ICommand LoadCommand { get; }
    public ICommand SaveCommand { get; }

    public LoyaltySettingsViewModel(LoyaltyApiService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        LoadCommand = new RelayCommand(async () => await LoadAsync());
        SaveCommand = new RelayCommand(async () => await SaveAsync(), () => !IsLoading);
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        IsLoading = true;
        try
        {
            var dto = await _service.GetSettingsAsync();
            if (dto != null)
            {
                PointsPerUnitCurrency = dto.PointsPerUnitCurrency;
                RedemptionValuePerPoint = dto.RedemptionValuePerPoint;
                MinimumRedeemPoints = dto.MinimumRedeemPoints;
            }
        }
        catch (Exception ex)
        {
            DialogService.Error("Loyalty Settings", $"Failed to load settings: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
            (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }
    }

    private async Task SaveAsync()
    {
        if (IsLoading)
            return;

        IsLoading = true;
        (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged();

        try
        {
            var dto = new UpdateLoyaltySettingsDto
            {
                PointsPerUnitCurrency = PointsPerUnitCurrency,
                RedemptionValuePerPoint = RedemptionValuePerPoint,
                MinimumRedeemPoints = MinimumRedeemPoints
            };

            await _service.SaveSettingsAsync(dto);
            DialogService.Info("Loyalty Settings", "Loyalty settings saved successfully.");
        }
        catch (Exception ex)
        {
            DialogService.Error("Loyalty Settings", $"Failed to save settings: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
            (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }
    }
}

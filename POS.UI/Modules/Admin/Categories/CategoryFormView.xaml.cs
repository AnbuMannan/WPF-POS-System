using POS.UI.Core.Exceptions;
using POS.UI.Core.Models;
using POS.UI.Core.MVVM;
using POS.UI.Core.Services;
using POS.UI.Modules.Admin.Common;
using POS.UI.Modules.Admin.Products.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;                 // 🔥 REQUIRED
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace POS.UI.Modules.Admin.Categories
{
    public partial class CategoryFormView : Window, INotifyPropertyChanged, INotifyDataErrorInfo
    {
        private readonly ProductApiService _service;
        private bool _isEdit;
        private CategoryDto _editDto;
        private bool _isSaving;

        public ObservableCollection<LookupDto> Categories { get; set; }
        public ObservableCollection<LookupDto> Brands { get; set; }
        public ObservableCollection<LookupDto> TaxProfiles { get; set; }

        public Guid ProductId { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        // ---------------- VALIDATION ENGINE ----------------

        private readonly Dictionary<string, List<string>> _errors = new();

        public bool HasErrors => _errors.Any();

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        private CancellationTokenSource _skuCts;
        private CancellationTokenSource _barcodeCts;

        private void FocusFirstInvalidControl()
        {
            var firstInvalid = FindFirstInvalid(this);

            if (firstInvalid != null)
            {
                firstInvalid.BringIntoView();   // 🔥 Auto scroll
                firstInvalid.Focus();           // 🔥 Auto focus
            }
        }

        private Control FindFirstInvalid(DependencyObject parent)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is Control control &&
                    System.Windows.Controls.Validation.GetHasError(control))
                {
                    return control;
                }

                var result = FindFirstInvalid(child);
                if (result != null)
                    return result;
            }

            return null;
        }
        public System.Collections.IEnumerable GetErrors(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
                return null;

            return _errors.ContainsKey(propertyName) ? _errors[propertyName] : null;
        }

        private void AddError(string propertyName, string error)
        {
            if (!_errors.ContainsKey(propertyName))
                _errors[propertyName] = new List<string>();

            if (!_errors[propertyName].Contains(error))
            {
                _errors[propertyName].Add(error);
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
                ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
            }
        }

        private void ClearErrors(string propertyName)
        {
            if (_errors.ContainsKey(propertyName))
            {
                _errors.Remove(propertyName);
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
                ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
            }
        }


        // ---------------- BASIC FIELDS ----------------

        private string _productName;
        public string ProductName
        {
            get => _productName;
            set
            {
                _productName = value;
                OnPropertyChanged(nameof(ProductName));

                ClearErrors(nameof(ProductName));

                if (string.IsNullOrWhiteSpace(ProductName))
                    AddError(nameof(ProductName), "Product name is required");
            }
        }


        private string _sku;
        public string SKU
        {
            get => _sku;
            set
            {
                _sku = value;
                OnPropertyChanged(nameof(SKU));
                _ = ValidateSkuAsync();   // 🔥 LIVE CHECK
            }
        }


        private string _barcode;
        public string Barcode
        {
            get => _barcode;
            set
            {
                _barcode = value;
                OnPropertyChanged(nameof(Barcode));
                _ = ValidateBarcodeAsync();   // 🔥 LIVE CHECK
            }
        }


        private string _description;
        public string Description { get => _description; set { _description = value; OnPropertyChanged(nameof(Description)); } }

        private string _unit;
        public string Unit { get => _unit; set { _unit = value; OnPropertyChanged(nameof(Unit)); } }

        private string _hsnCode;
        public string HSNCode
        {
            get => _hsnCode;
            set
            {
                _hsnCode = value;
                OnPropertyChanged(nameof(HSNCode));

                ClearErrors(nameof(HSNCode));

                if (string.IsNullOrWhiteSpace(HSNCode))
                    AddError(nameof(HSNCode), "HSN Code is required");
            }
        }


        private decimal _costPrice;
        public decimal CostPrice
        {
            get => _costPrice;
            set
            {
                _costPrice = value;
                OnPropertyChanged(nameof(CostPrice));

                ValidatePrices();
            }
        }


        private decimal _sellingPrice;
        public decimal SellingPrice
        {
            get => _sellingPrice;
            set
            {
                _sellingPrice = value;
                OnPropertyChanged(nameof(SellingPrice));

                ValidatePrices();
            }
        }


        private decimal _mrp;
        public decimal MRP
        {
            get => _mrp;
            set
            {
                _mrp = value;
                OnPropertyChanged(nameof(MRP));

                ValidatePrices();
            }
        }


        private bool _isWeighable;
        public bool IsWeighable { get => _isWeighable; set { _isWeighable = value; OnPropertyChanged(nameof(IsWeighable)); } }

        private bool _isManufactured;
        public bool IsManufactured { get => _isManufactured; set { _isManufactured = value; OnPropertyChanged(nameof(IsManufactured)); } }

        private bool _isTaxInclusive;
        public bool IsTaxInclusive { get => _isTaxInclusive; set { _isTaxInclusive = value; OnPropertyChanged(nameof(IsTaxInclusive)); } }

        private bool _isProductActive = true;
        public bool IsProductActive { get => _isProductActive; set { _isProductActive = value; OnPropertyChanged(nameof(IsProductActive)); } }

        // ---------------- FK IDS ----------------

        private Guid _categoryId;
        public Guid CategoryId
        {
            get => _categoryId;
            set
            {
                _categoryId = value;
                OnPropertyChanged(nameof(CategoryId));

                ClearErrors(nameof(CategoryId));

                if (CategoryId == Guid.Empty)
                    AddError(nameof(CategoryId), "Category is required");
            }
        }


        private Guid _brandId;
        public Guid BrandId
        {
            get => _brandId;
            set { _brandId = value; OnPropertyChanged(nameof(BrandId)); }
        }

        private Guid _taxProfileId;
        public Guid TaxProfileId
        {
            get => _taxProfileId;
            set
            {
                _taxProfileId = value;
                OnPropertyChanged(nameof(TaxProfileId));

                ClearErrors(nameof(TaxProfileId));

                if (TaxProfileId == Guid.Empty)
                    AddError(nameof(TaxProfileId), "Tax Profile is required");
            }
        }




        // ---------------- COMMANDS ----------------

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand ResetCommand { get; }

        public CategoryFormView() : this(null) { }

        public CategoryFormView(CategoryDto dto)
        {
            InitializeComponent();
            DataContext = this;

            try
            {
                // Get ProductApiService from DI container for SKU/Barcode validation
                if (App.ServiceProvider != null)
                {
                    _service = (ProductApiService)App.ServiceProvider.GetService(typeof(ProductApiService));
                }
                else
                {
                    throw new InvalidOperationException("Application service provider not initialized.");
                }
            }
            catch (Exception ex)
            {
                POS.UI.Components.DialogService.Error("Initialization Error", $"Failed to initialize: {ex.Message}");
            }

            _editDto = dto;

            Loaded += ProductFormView_Loaded;

            SaveCommand = new RelayCommand(async () => await SaveAsync(), () => !_isSaving);

            CancelCommand = new RelayCommand(CloseWindow);
            ResetCommand = new RelayCommand(ResetForm);

            // 🔥 Force initial validation
            ValidateAll();

        }

        // ---------------- LOAD ----------------

        private async void ProductFormView_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadMastersAsync();

            
        }

        private async Task LoadMastersAsync()
        {
            try
            {
                // Use App.ServiceProvider to get HttpClient if available
                HttpClient httpClient = null;
                if (App.ServiceProvider != null)
                {
                    try
                    {
                        httpClient = (HttpClient)App.ServiceProvider.GetService(typeof(HttpClient));
                    }
                    catch
                    {
                        // If not available from DI, this form might not need masters
                    }
                }

                if (httpClient != null)
                {
                    Categories = new ObservableCollection<LookupDto>(
                        await httpClient.GetFromJsonAsync<List<LookupDto>>("api/categories") ?? new List<LookupDto>());

                    Brands = new ObservableCollection<LookupDto>(
                        await httpClient.GetFromJsonAsync<List<LookupDto>>("api/brands") ?? new List<LookupDto>());

                    TaxProfiles = new ObservableCollection<LookupDto>(
                        await httpClient.GetFromJsonAsync<List<LookupDto>>("api/taxprofiles") ?? new List<LookupDto>());

                    OnPropertyChanged(nameof(Categories));
                    OnPropertyChanged(nameof(Brands));
                    OnPropertyChanged(nameof(TaxProfiles));
                }
            }
            catch (Exception ex)
            {
                POS.UI.Components.DialogService.Error("Error", $"Failed to load master data: {ex.Message}");
            }
        }

        // ---------------- SAVE ----------------

        private async Task SaveAsync()
        {
            if (HasErrors)
            {
                FocusFirstInvalidControl();
                return;
            }

            _isSaving = true;
            ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();

            

            try
            {
                

                DialogResult = true;
                CloseWindow();

            }
            catch (ApiValidationException vex)
            {
                if (vex.Error?.Errors != null)
                {
                    foreach (var kv in vex.Error.Errors)
                    {
                        ClearErrors(kv.Key);

                        foreach (var msg in kv.Value)
                            AddError(kv.Key, msg);
                    }

                    return; // 🔥 Inline errors shown, no MessageBox
                }
            }
            catch (Exception ex)
            {
                POS.UI.Components.DialogService.Error("Save Failed", ex.Message);
            }
            finally
            {
                _isSaving = false;
                ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
            }
        }
        private void CloseWindow() => Close();

        private void ResetForm()
        {
            ProductName = string.Empty;
            SKU = string.Empty;
            Barcode = string.Empty;
            Description = string.Empty;
            Unit = string.Empty;
            HSNCode = string.Empty;

            CostPrice = 0;
            SellingPrice = 0;
            MRP = 0;

            IsWeighable = false;
            IsManufactured = false;
            IsTaxInclusive = false;
            IsProductActive = true;

            CategoryId = Guid.Empty;
            BrandId = Guid.Empty;
            TaxProfileId = Guid.Empty;

        }

        private void ValidatePrices()
        {
            ClearErrors(nameof(SellingPrice));
            ClearErrors(nameof(MRP));

            if (SellingPrice < CostPrice)
                AddError(nameof(SellingPrice), "Selling price must be >= Cost price");

            if (MRP < SellingPrice)
                AddError(nameof(MRP), "MRP must be >= Selling price");
        }
        private void ValidateAll()
        {
            // Trigger validation for required fields
            ProductName = ProductName;
            HSNCode = HSNCode;
            CategoryId = CategoryId;
            TaxProfileId = TaxProfileId;

            CostPrice = CostPrice;
            SellingPrice = SellingPrice;
            MRP = MRP;
        }
        public class ApiValidationError
        {
            public Dictionary<string, string[]> Errors { get; set; }
        }

        private async Task ValidateSkuAsync()
        {
            _skuCts?.Cancel();
            _skuCts = new CancellationTokenSource();
            var token = _skuCts.Token;

            ClearErrors(nameof(SKU));

            try
            {
                // 🔥 debounce delay
                await Task.Delay(400, token);

                if (string.IsNullOrWhiteSpace(SKU))
                    return;

                bool exists = await _service.CheckSkuExistsAsync(
                    SKU,
                    _isEdit ? ProductId : null);

                if (exists)
                    AddError(nameof(SKU), "SKU already exists");
            }
            catch (TaskCanceledException)
            {
                // user typed again – ignore
            }
            catch
            {
                // ignore API failures silently
            }
        }


        private async Task ValidateBarcodeAsync()
        {
            _barcodeCts?.Cancel();
            _barcodeCts = new CancellationTokenSource();
            var token = _barcodeCts.Token;

            ClearErrors(nameof(Barcode));

            try
            {
                // 🔥 debounce delay
                await Task.Delay(400, token);

                if (string.IsNullOrWhiteSpace(Barcode))
                    return;

                bool exists = await _service.CheckBarcodeExistsAsync(
                    Barcode,
                    _isEdit ? ProductId : null);

                if (exists)
                    AddError(nameof(Barcode), "Barcode already exists");
            }
            catch (TaskCanceledException)
            {
                // ignore
            }
            catch
            {
                // ignore API failures
            }
        }



    }
}

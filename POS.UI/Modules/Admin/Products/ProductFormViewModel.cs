using POS.UI.Core.Exceptions;
using POS.Shared.Models;
using POS.UI.Core.MVVM;
using POS.UI.Core.Services;
using POS.UI.Modules.Admin.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Text.Json;
using System.Globalization;

namespace POS.UI.Modules.Admin.Products.ViewModels
{
    public class ProductFormViewModel : ViewModelBase, INotifyDataErrorInfo
    {
        private readonly ProductApiService _service;
        private readonly HttpClient _httpClient;
        private bool _isEdit;
        private ProductDto _editDto;
        private bool _isSaving;
        private CancellationTokenSource _skuCts;
        private CancellationTokenSource _barcodeCts;

        // ================= COLLECTIONS =================

        public ObservableCollection<LookupDto> Categories { get; set; }
        public ObservableCollection<LookupDto> Brands { get; set; }
        public ObservableCollection<LookupDto> TaxProfiles { get; set; }

        // ================= VALIDATION ENGINE =================

        private readonly Dictionary<string, List<string>> _errors = new(StringComparer.OrdinalIgnoreCase);

        public bool HasErrors => _errors.Any();

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

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
                ((RelayCommand)SaveCommand)?.RaiseCanExecuteChanged();
            }
        }

        private void ClearErrors(string propertyName)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                return;

            var cleared = false;

            if (_errors.ContainsKey(propertyName))
            {
                _errors.Remove(propertyName);
                cleared = true;
            }

            var camel = char.ToLowerInvariant(propertyName[0]) + propertyName.Substring(1);
            if (_errors.ContainsKey(camel))
            {
                _errors.Remove(camel);
                cleared = true;
            }

            var jsonPath = "$." + camel;
            if (_errors.ContainsKey(jsonPath))
            {
                _errors.Remove(jsonPath);
                cleared = true;
            }

            if (propertyName.Equals("HSNCode", StringComparison.OrdinalIgnoreCase) && _errors.ContainsKey("hsnCode"))
            {
                _errors.Remove("hsnCode");
                cleared = true;
            }
            if (propertyName.Equals("ProductName", StringComparison.OrdinalIgnoreCase) && _errors.ContainsKey("Name"))
            {
                _errors.Remove("Name");
                cleared = true;
            }

            if (cleared)
            {
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
                ((RelayCommand)SaveCommand)?.RaiseCanExecuteChanged();
            }
        }

        // ================= PROPERTIES =================

        private long _productId;
        public long ProductId
        {
            get => _productId;
            set
            {
                _productId = value;
                OnPropertyChanged(nameof(ProductId));
            }
        }

        private byte[] _rowVersion;
        public byte[] RowVersion
        {
            get => _rowVersion;
            set
            {
                _rowVersion = value;
                OnPropertyChanged(nameof(RowVersion));
            }
        }

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
                _ = ValidateSkuAsync();
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
                _ = ValidateBarcodeAsync();
            }
        }

        private string _description;
        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged(nameof(Description));
            }
        }

        private string _unit;
        public string Unit
        {
            get => _unit;
            set
            {
                _unit = value;
                OnPropertyChanged(nameof(Unit));
            }
        }

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
        public bool IsWeighable
        {
            get => _isWeighable;
            set
            {
                _isWeighable = value;
                OnPropertyChanged(nameof(IsWeighable));
            }
        }

        private bool _isManufactured;
        public bool IsManufactured
        {
            get => _isManufactured;
            set
            {
                _isManufactured = value;
                OnPropertyChanged(nameof(IsManufactured));
            }
        }

        private bool _isProductActive = true;
        public bool IsProductActive
        {
            get => _isProductActive;
            set
            {
                _isProductActive = value;
                OnPropertyChanged(nameof(IsProductActive));
            }
        }

        private bool _isTaxInclusive;
        /// <summary>Whether price is tax-inclusive. Named to avoid conflict with Window/Control properties.</summary>
        public bool IsTaxInclusive
        {
            get => _isTaxInclusive;
            set
            {
                _isTaxInclusive = value;
                OnPropertyChanged(nameof(IsTaxInclusive));
            }
        }

        private int _categoryId;
        public int CategoryId
        {
            get => _categoryId;
            set
            {
                _categoryId = value;
                OnPropertyChanged(nameof(CategoryId));
                ClearErrors(nameof(CategoryId));
                if (!_isEdit && CategoryId <= 0)
                    AddError(nameof(CategoryId), "Category is required");
            }
        }

        private LookupDto _selectedCategory;
        public LookupDto SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                OnPropertyChanged(nameof(SelectedCategory));
                if (value != null)
                {
                    CategoryId = (int)value.Id;
                    ClearErrors(nameof(CategoryId));
                }
                else
                    CategoryId = 0;
            }
        }

        private LookupDto _selectedBrand;
        public LookupDto SelectedBrand
        {
            get => _selectedBrand;
            set
            {
                _selectedBrand = value;
                OnPropertyChanged(nameof(SelectedBrand));
                if (SelectedBrand != null)
                    BrandId = (int)SelectedBrand.Id;
                else
                    BrandId = 0;
            }
        }

        private LookupDto _selectedTaxProfile;
        public LookupDto SelectedTaxProfile
        {
            get => _selectedTaxProfile;
            set
            {
                _selectedTaxProfile = value;
                OnPropertyChanged(nameof(SelectedTaxProfile));
                if (value != null)
                {
                    TaxProfileId = (int)value.Id;
                    ClearErrors(nameof(TaxProfileId));
                }
                else
                    TaxProfileId = 0;
            }
        }

        private int _brandId;
        public int BrandId
        {
            get => _brandId;
            set
            {
                _brandId = value;
                OnPropertyChanged(nameof(BrandId));
            }
        }

        private int _taxProfileId;
        public int TaxProfileId
        {
            get => _taxProfileId;
            set
            {
                _taxProfileId = value;
                OnPropertyChanged(nameof(TaxProfileId));
                ClearErrors(nameof(TaxProfileId));
                if (TaxProfileId <= 0)
                    AddError(nameof(TaxProfileId), "Tax Profile is required");
            }
        }

        // ================= COMMANDS =================

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand ResetCommand { get; }

        // ================= EVENTS =================

        public event EventHandler RequestClose;
        public event EventHandler<bool> RequestCloseWithResult;

        // ================= CONSTRUCTOR =================

        public ProductFormViewModel(ProductApiService service, HttpClient httpClient = null, ProductDto editDto = null)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _httpClient = httpClient;  // Can be null if only using ProductApiService
            _editDto = editDto;
            _isEdit = editDto != null;

            Categories = new ObservableCollection<LookupDto>();
            Brands = new ObservableCollection<LookupDto>();
            TaxProfiles = new ObservableCollection<LookupDto>();

            SaveCommand = new RelayCommand(async () => await SaveAsync(), () => !_isSaving && !HasErrors);
            CancelCommand = new RelayCommand(Cancel);
            ResetCommand = new RelayCommand(ResetForm);

            _ = InitializeAsync();
        }

        // ================= INITIALIZATION =================

        private async Task InitializeAsync()
        {
            await LoadMastersAsync();

            if (_editDto != null)
            {
                LoadProductData(_editDto);
                await LoadServerProductAsync(_editDto.ProductId);

                SelectedCategory = Categories.FirstOrDefault(x => x.Id == CategoryId);
                SelectedBrand = Brands.FirstOrDefault(x => x.Id == BrandId);
                SelectedTaxProfile = TaxProfiles.FirstOrDefault(x => x.Id == TaxProfileId);

                //var found = Categories.FirstOrDefault(x => x.Id == CategoryId);

                //if (found == null)
                //{
                //    MessageBox.Show(
                //        $"Category NOT FOUND in master list!\n\nProduct CategoryId:\n{CategoryId}\n\nAvailable Categories:\n" +
                //        string.Join("\n", Categories.Select(c => $"{c.Id} - {c.Name}")),
                //        "Debug",
                //        MessageBoxButton.OK,
                //        MessageBoxImage.Warning);
                //}
            }
            else
            {
                // Do NOT validate Category immediately on new form load
                ValidateAll();
            }
        }

        private async Task LoadServerProductAsync(long id)
        {
            try
            {
                var full = await _service.GetByIdAsync(id);
                if (full?.RowVersion != null)
                    RowVersion = full.RowVersion;
            }
            catch { }
        }


        private async Task LoadMastersAsync()
        {
            try
            {
                // Use injected HttpClient if available, otherwise resolve from IHttpClientFactory
                var httpClient = _httpClient;

                if (httpClient == null && App.ServiceProvider != null)
                {
                    var factory = App.ServiceProvider.GetService(typeof(IHttpClientFactory)) as IHttpClientFactory;

                    if (factory == null)
                        throw new InvalidOperationException("Unable to load master data: IHttpClientFactory not configured.");

                    // 🔥 Use the named client that has BaseAddress configured
                    httpClient = factory.CreateClient("DefaultApi");
                }

                if (httpClient == null)
                    throw new InvalidOperationException("Unable to load master data: HttpClient not available.");

                var categoriesJson = await TryGetJsonAsync(httpClient, "api/categories", "api/categories/all");
                var brandsJson = await TryGetJsonAsync(httpClient, "api/brands", "api/brands/all");
                var taxProfilesJson = await TryGetJsonAsync(httpClient, "api/taxprofiles", "api/taxprofiles/all");
                categoriesJson ??= "[]";
                brandsJson ??= "[]";
                taxProfilesJson ??= "[]";

                Categories.Clear();

                // 🔥 Add placeholder ONLY in ADD MODE
                if (!_isEdit)
                    Categories.Add(new LookupDto { Id = 0, Name = "-- Select Category --" });

                using (var doc = JsonDocument.Parse(categoriesJson))
                {
                    if (doc.RootElement.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var el in doc.RootElement.EnumerateArray())
                        {
                            var name = GetStringIgnoreCase(el, "name", "Name");
                            var id = GetLongIgnoreCase(el, "id", "Id", "categoryId", "CategoryId");
                            if (!string.IsNullOrWhiteSpace(name))
                                Categories.Add(new LookupDto { Id = id, Name = name });
                        }
                    }
                }

                Brands.Clear();
                if (!_isEdit)
                    Brands.Add(new LookupDto { Id = 0, Name = "-- Select Brand --" });

                using (var doc = JsonDocument.Parse(brandsJson))
                {
                    if (doc.RootElement.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var el in doc.RootElement.EnumerateArray())
                        {
                            var name = GetStringIgnoreCase(el, "name", "Name");
                            var id = GetLongIgnoreCase(el, "id", "Id", "brandId", "BrandId");
                            if (!string.IsNullOrWhiteSpace(name))
                                Brands.Add(new LookupDto { Id = id, Name = name });
                        }
                    }
                }

                TaxProfiles.Clear();
                if (!_isEdit)
                    TaxProfiles.Add(new LookupDto { Id = 0, Name = "-- Select Tax --" });

                using (var doc = JsonDocument.Parse(taxProfilesJson))
                {
                    if (doc.RootElement.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var el in doc.RootElement.EnumerateArray())
                        {
                            var name = GetStringIgnoreCase(el, "name", "Name");
                            var id = GetLongIgnoreCase(el, "id", "Id", "taxProfileId", "TaxProfileId");
                            if (!string.IsNullOrWhiteSpace(name))
                                TaxProfiles.Add(new LookupDto { Id = id, Name = name });
                        }
                    }
                }

                SelectedCategory = Categories.FirstOrDefault(x => x.Id == CategoryId);
                SelectedBrand = Brands.FirstOrDefault(x => x.Id == BrandId);
                SelectedTaxProfile = TaxProfiles.FirstOrDefault(x => x.Id == TaxProfileId);
            }
            catch (Exception ex)
            {
                POS.UI.Components.DialogService.Error("Error", $"Failed to load master data:\n\n{ex}");
            }
        }

        private static string GetStringIgnoreCase(JsonElement el, params string[] names)
        {
            foreach (var prop in el.EnumerateObject())
            {
                foreach (var n in names)
                {
                    if (string.Equals(prop.Name, n, StringComparison.OrdinalIgnoreCase))
                        return prop.Value.GetString();
                }
            }
            return null;
        }

        private static long GetLongIgnoreCase(JsonElement el, params string[] names)
        {
            foreach (var prop in el.EnumerateObject())
            {
                foreach (var n in names)
                {
                    if (string.Equals(prop.Name, n, StringComparison.OrdinalIgnoreCase))
                    {
                        if (prop.Value.ValueKind == JsonValueKind.Number && prop.Value.TryGetInt64(out var l))
                            return l;
                        if (prop.Value.ValueKind == JsonValueKind.String && long.TryParse(prop.Value.GetString(), out var parsed))
                            return parsed;
                    }
                }
            }
            return 0;
        }

        private static async Task<string?> TryGetJsonAsync(HttpClient httpClient, params string[] urls)
        {
            foreach (var url in urls)
            {
                try
                {
                    var resp = await httpClient.GetAsync(url);
                    if (resp.IsSuccessStatusCode)
                    {
                        return await resp.Content.ReadAsStringAsync();
                    }
                }
                catch
                {
                }
            }
            return null;
        }

        private void LoadProductData(ProductDto dto)
        {
            ProductId = dto.ProductId;
            ProductName = dto.Name;
            SKU = dto.SKU;
            Barcode = dto.Barcode;
            Description = dto.Description;
            Unit = dto.Unit;
            HSNCode = dto.HSNCode ?? string.Empty;

            CostPrice = dto.CostPrice;
            SellingPrice = dto.SellingPrice;
            MRP = dto.MRP;

            IsWeighable = dto.IsWeighable;
            IsManufactured = dto.IsManufactured;
            IsProductActive = dto.IsActive;

            CategoryId = dto.CategoryId;
            BrandId = dto.BrandId ?? 0;
            TaxProfileId = dto.TaxProfileId;
            RowVersion = dto.RowVersion;
        }

        // ================= VALIDATION =================

        private void ValidateAll()
        {
            // Trigger validation for required fields
            ProductName = ProductName ?? string.Empty;
            HSNCode = HSNCode ?? string.Empty;
            CategoryId = CategoryId;
            TaxProfileId = TaxProfileId;

            CostPrice = CostPrice;
            SellingPrice = SellingPrice;
            MRP = MRP;
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

        private async Task ValidateSkuAsync()
        {
            _skuCts?.Cancel();
            _skuCts = new CancellationTokenSource();
            var token = _skuCts.Token;

            ClearErrors(nameof(SKU));

            try
            {
                await Task.Delay(400, token);

                if (string.IsNullOrWhiteSpace(SKU))
                    return;

                bool exists = await _service.CheckSkuExistsAsync(SKU, _isEdit ? ProductId : null);

                if (exists)
                    AddError(nameof(SKU), "SKU already exists");
            }
            catch (TaskCanceledException)
            {
                // User typed again - ignore
            }
            catch
            {
                // Ignore API failures silently
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
                await Task.Delay(400, token);

                if (string.IsNullOrWhiteSpace(Barcode))
                    return;

                bool exists = await _service.CheckBarcodeExistsAsync(Barcode, _isEdit ? ProductId : null);

                if (exists)
                    AddError(nameof(Barcode), "Barcode already exists");
            }
            catch (TaskCanceledException)
            {
                // Ignore
            }
            catch
            {
                // Ignore API failures
            }
        }

        // ================= SAVE =================

        private async Task SaveAsync()
        {
            if (HasErrors)
            {
                POS.UI.Components.DialogService.Info("Validation Error", "Please fix validation errors before saving.");
                return;
            }

            _isSaving = true;
            ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();

            var dto = new ProductDto
            {
                ProductId = _isEdit ? ProductId : 0,
                Name = ProductName,
                SKU = SKU,
                Barcode = Barcode,
                Description = Description,
                Unit = Unit,
                HSNCode = HSNCode,

                CategoryId = CategoryId,
                BrandId = BrandId <= 0 ? null : BrandId,
                TaxProfileId = TaxProfileId,

                CostPrice = CostPrice,
                SellingPrice = SellingPrice,
                MRP = MRP,

                IsWeighable = IsWeighable,
                IsManufactured = IsManufactured,
                IsActive = IsProductActive,
                RowVersion = _isEdit ? RowVersion : null,
            };

            try
            {
                if (_isEdit)
                {
                    dto.UpdatedAt = DateTime.Now;
                    await _service.UpdateAsync(dto);

                    POS.UI.Components.DialogService.Info("Success", "Product updated successfully");
                }
                else
                {
                    dto.CreatedAt = DateTime.Now;
                    await _service.CreateAsync(dto);

                    POS.UI.Components.DialogService.Info("Success", "Product created successfully");
                }

                RequestCloseWithResult?.Invoke(this, true);
            }
            catch (ApiValidationException vex)
            {
                if (vex.Error?.Errors != null)
                {
                    foreach (var kv in vex.Error.Errors)
                    {

                        var key = kv.Key ?? string.Empty;
                        var messages = kv.Value ?? Array.Empty<string>();

                        if (key.Equals("product", StringComparison.OrdinalIgnoreCase) ||
                            key.Contains("updatedAt", StringComparison.OrdinalIgnoreCase))
                        {
                            var text = string.Join("\n", messages);
                            if (!string.IsNullOrWhiteSpace(text))
                                POS.UI.Components.DialogService.Error("Validation Error", text);
                            continue;
                        }

                        if (key.StartsWith("product.", StringComparison.OrdinalIgnoreCase))
                            key = key.Substring("product.".Length);

                        if (key.StartsWith("$.", StringComparison.Ordinal))
                            key = key.Substring(2);

                        if (string.Equals(key, "hsnCode", StringComparison.OrdinalIgnoreCase))
                            key = "HSNCode";

                        if (string.Equals(key, "taxProfileId", StringComparison.OrdinalIgnoreCase))
                            key = "TaxProfileId";

                        if (string.Equals(key, "categoryId", StringComparison.OrdinalIgnoreCase))
                            key = "CategoryId";
                        
                        if (string.Equals(key, "Name", StringComparison.OrdinalIgnoreCase))
                            key = "ProductName";

                        ClearErrors(key);
                        foreach (var msg in messages)
                            AddError(key, msg);
                    }
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

        // ================= CANCEL =================

        private void Cancel()
        {
            RequestClose?.Invoke(this, EventArgs.Empty);
        }

        // ================= RESET =================

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

            CategoryId = 0;
            BrandId = 0;
            TaxProfileId = 0;

            // Clear dropdown selections so ComboBoxes show no selection
            SelectedCategory = null;
            SelectedBrand = null;
            SelectedTaxProfile = null;

            _errors.Clear();
        }
    }
}

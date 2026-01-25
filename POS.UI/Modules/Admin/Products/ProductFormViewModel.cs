using POS.UI.Core.Exceptions;
using POS.UI.Core.MVVM;
using POS.UI.Core.Services;
using POS.UI.Modules.Admin.Common;
using POS.UI.Modules.Admin.Products.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace POS.UI.Modules.Admin.Products.ViewModels
{
    public class ProductFormViewModel : ViewModelBase, INotifyDataErrorInfo
    {
        private readonly ProductApiService _service;
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

        private readonly Dictionary<string, List<string>> _errors = new();

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
            if (_errors.ContainsKey(propertyName))
            {
                _errors.Remove(propertyName);
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
                ((RelayCommand)SaveCommand)?.RaiseCanExecuteChanged();
            }
        }

        // ================= PROPERTIES =================

        private Guid _productId;
        public Guid ProductId
        {
            get => _productId;
            set
            {
                _productId = value;
                OnPropertyChanged(nameof(ProductId));
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

        private bool _isTaxInclusive;
        public bool IsTaxInclusive
        {
            get => _isTaxInclusive;
            set
            {
                _isTaxInclusive = value;
                OnPropertyChanged(nameof(IsTaxInclusive));
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

        private Guid _categoryId;
        public Guid CategoryId
        {
            get => _categoryId;
            set
            {
                _categoryId = value;
                OnPropertyChanged(nameof(CategoryId));

                ClearErrors(nameof(CategoryId));

                if (!_isEdit && CategoryId == Guid.Empty)
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

                if (SelectedCategory != null)
                {
                    CategoryId = SelectedCategory.Id;   // 🔥 Sync to real Guid
                    ClearErrors(nameof(CategoryId));
                }
                else
                {
                    CategoryId = Guid.Empty;
                }
            }
        }


        private Guid _brandId;
        public Guid BrandId
        {
            get => _brandId;
            set
            {
                _brandId = value;
                OnPropertyChanged(nameof(BrandId));
            }
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

        // ================= COMMANDS =================

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand ResetCommand { get; }

        // ================= EVENTS =================

        public event EventHandler RequestClose;
        public event EventHandler<bool> RequestCloseWithResult;

        // ================= CONSTRUCTOR =================

        public ProductFormViewModel(ProductDto editDto = null)
        {
            _service = new ProductApiService(App.ApiClient);
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

                // 🔥 Force refresh selection after categories loaded
                OnPropertyChanged(nameof(CategoryId));
            }
            else
            {
                // Do NOT validate Category immediately on new form load
                ValidateAll();
            }

        }

        private async Task LoadMastersAsync()
        {
            try
            {
                var categories = await App.ApiClient.GetFromJsonAsync<List<LookupDto>>("api/categories");
                var brands = await App.ApiClient.GetFromJsonAsync<List<LookupDto>>("api/brands");
                var taxProfiles = await App.ApiClient.GetFromJsonAsync<List<LookupDto>>("api/taxprofiles");

                Categories.Clear();

                // 🔥 Add placeholder first item
                Categories.Add(new LookupDto
                {
                    Id = Guid.Empty,
                    Name = "-- Select Category --"
                });

                foreach (var cat in categories ?? new List<LookupDto>())
                    Categories.Add(cat);


                Brands.Clear();
                // 🔥 Add placeholder first item
                Brands.Add(new LookupDto
                {
                    Id = Guid.Empty,
                    Name = "-- Select Brand --"
                });
                foreach (var brand in brands ?? new List<LookupDto>())
                    Brands.Add(brand);

                TaxProfiles.Clear();
                TaxProfiles.Add(new LookupDto
                {
                    Id = Guid.Empty,
                    Name = "-- Select Tax --"
                });
                foreach (var tax in taxProfiles ?? new List<LookupDto>())
                    TaxProfiles.Add(tax);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load master data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadProductData(ProductDto dto)
        {
            ProductId = dto.ProductId;
            ProductName = dto.Name;
            SKU = dto.SKU;
            Barcode = dto.Barcode;
            Description = dto.Description;
            Unit = dto.Unit;
            HSNCode = dto.HSNCode;

            CostPrice = dto.CostPrice;
            SellingPrice = dto.SellingPrice;
            MRP = dto.MRP;

            IsWeighable = dto.IsWeighable;
            IsManufactured = dto.IsManufactured;
            IsTaxInclusive = dto.IsTaxInclusive;
            IsProductActive = dto.IsActive;

            CategoryId = dto.CategoryId;
            BrandId = dto.BrandId;
            TaxProfileId = dto.TaxProfileId;
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
                MessageBox.Show("Please fix validation errors before saving.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _isSaving = true;
            ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();

            var dto = new ProductDto
            {
                ProductId = _isEdit ? ProductId : Guid.NewGuid(),
                Name = ProductName,
                SKU = SKU,
                Barcode = Barcode,
                Description = Description,
                Unit = Unit,
                HSNCode = HSNCode,

                CategoryId = CategoryId,
                BrandId = BrandId,
                TaxProfileId = TaxProfileId,

                CostPrice = CostPrice,
                SellingPrice = SellingPrice,
                MRP = MRP,

                IsWeighable = IsWeighable,
                IsManufactured = IsManufactured,
                IsTaxInclusive = IsTaxInclusive,
                IsActive = IsProductActive,
            };

            try
            {
                if (_isEdit)
                {
                    dto.UpdatedAt = DateTime.Now;
                    await _service.UpdateAsync(dto);

                    MessageBox.Show("Product updated successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    dto.CreatedAt = DateTime.Now;
                    await _service.CreateAsync(dto);

                    MessageBox.Show("Product created successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                RequestCloseWithResult?.Invoke(this, true);
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
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Save Failed", MessageBoxButton.OK, MessageBoxImage.Error);
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

            CategoryId = Guid.Empty;
            BrandId = Guid.Empty;
            TaxProfileId = Guid.Empty;

            _errors.Clear();
        }
    }
}
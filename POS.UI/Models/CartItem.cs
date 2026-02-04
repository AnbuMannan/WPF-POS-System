using System.ComponentModel;
using System.Runtime.CompilerServices;
using POS.UI.Core.MVVM;

namespace POS.UI.Models
{
    public class CartItem : INotifyPropertyChanged
    {
        private int _lineNumber;
        private string _sku = string.Empty;
        private string _productName = string.Empty;
        private string _hsnCode = string.Empty;
        private decimal _quantity = 1;
        private string _unitName = string.Empty;
        private decimal _mrp;
        private decimal _actualPrice;
        private decimal? _discountPercent;
        private decimal _discountAmount;
        private decimal _taxRate;
        private decimal _taxAmount;
        private decimal _totalAmount;

        public Guid ProductId { get; set; }
        public Guid? UnitId { get; set; }
        public Guid? TaxProfileId { get; set; }
        /// <summary>Stored for API (SaleItem FK). Set when adding product from ProductDto.TaxProfileId.</summary>
        public int TaxProfileIdValue { get; set; }

        public int LineNumber
        {
            get => _lineNumber;
            set { _lineNumber = value; OnPropertyChanged(); }
        }

        public string SKU
        {
            get => _sku;
            set { _sku = value; OnPropertyChanged(); }
        }

        public string ProductName
        {
            get => _productName;
            set { _productName = value; OnPropertyChanged(); }
        }

        public string HSNCode
        {
            get => _hsnCode;
            set { _hsnCode = value; OnPropertyChanged(); }
        }

        public decimal Quantity
        {
            get => _quantity;
            set
            {
                if (_quantity != value && value > 0)
                {
                    _quantity = value;
                    OnPropertyChanged();
                    RecalculateTotals();
                }
            }
        }

        public string UnitName
        {
            get => _unitName;
            set { _unitName = value; OnPropertyChanged(); }
        }

        public decimal MRP
        {
            get => _mrp;
            set { _mrp = value; OnPropertyChanged(); }
        }

        public decimal ActualPrice
        {
            get => _actualPrice;
            set
            {
                if (_actualPrice != value)
                {
                    _actualPrice = value;
                    OnPropertyChanged();
                    RecalculateTotals();
                }
            }
        }

        public decimal? DiscountPercent
        {
            get => _discountPercent;
            set
            {
                if (_discountPercent != value)
                {
                    _discountPercent = value;
                    OnPropertyChanged();
                    RecalculateTotals();
                }
            }
        }

        public decimal DiscountAmount
        {
            get => _discountAmount;
            set { _discountAmount = value; OnPropertyChanged(); }
        }

        public decimal TaxRate
        {
            get => _taxRate;
            set { _taxRate = value; OnPropertyChanged(); RecalculateTotals(); }
        }

        public decimal TaxAmount
        {
            get => _taxAmount;
            set { _taxAmount = value; OnPropertyChanged(); }
        }

        public decimal TotalAmount
        {
            get => _totalAmount;
            set { _totalAmount = value; OnPropertyChanged(); }
        }

        public RelayCommand IncreaseQuantityCommand { get; }
        public RelayCommand DecreaseQuantityCommand { get; }

        public CartItem()
        {
            IncreaseQuantityCommand = new RelayCommand(() => Quantity++);
            DecreaseQuantityCommand = new RelayCommand(() =>
            {
                if (Quantity > 1) Quantity--;
            });
        }

        private void RecalculateTotals()
        {
            var subtotal = Quantity * ActualPrice;
            
            // Calculate discount
            if (DiscountPercent.HasValue && DiscountPercent.Value > 0)
            {
                DiscountAmount = subtotal * (DiscountPercent.Value / 100);
            }
            else
            {
                DiscountAmount = 0;
            }

            var taxableAmount = subtotal - DiscountAmount;
            TaxAmount = taxableAmount * (TaxRate / 100);
            TotalAmount = taxableAmount + TaxAmount;
        }

        /// <summary>Call after all properties are set (e.g. when loading from API) so tax and totals are correct.</summary>
        public void RefreshTotals()
        {
            RecalculateTotals();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class BillSummary : INotifyPropertyChanged
    {
        private decimal _subtotal;
        private decimal _discountAmount;
        private decimal _cgst;
        private decimal _sgst;
        private decimal _igst;
        private decimal _roundOff;
        private decimal _grandTotal;

        public decimal Subtotal
        {
            get => _subtotal;
            set { _subtotal = value; OnPropertyChanged(); }
        }

        public decimal DiscountAmount
        {
            get => _discountAmount;
            set { _discountAmount = value; OnPropertyChanged(); }
        }

        public decimal CGST
        {
            get => _cgst;
            set { _cgst = value; OnPropertyChanged(); }
        }

        public decimal SGST
        {
            get => _sgst;
            set { _sgst = value; OnPropertyChanged(); }
        }

        public decimal IGST
        {
            get => _igst;
            set { _igst = value; OnPropertyChanged(); }
        }

        public decimal RoundOff
        {
            get => _roundOff;
            set { _roundOff = value; OnPropertyChanged(); }
        }

        public decimal GrandTotal
        {
            get => _grandTotal;
            set { _grandTotal = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

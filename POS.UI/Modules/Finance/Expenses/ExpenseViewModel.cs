using System.Collections.ObjectModel;
using System.Windows.Input;
using POS.Shared.Models;
using POS.UI.Core.MVVM;
using POS.UI.Core.Services;

namespace POS.UI.Modules.Finance.Expenses;

public class ExpenseViewModel : ViewModelBase
{
    private readonly ExpenseApiService _service;

    public ObservableCollection<ExpenseDto> Expenses { get; } = new();
    public decimal TotalExpenses => Expenses?.Sum(e => e.Amount) ?? 0;

    public ExpenseDto NewExpense { get; set; } = new ExpenseDto
    {
        ExpenseDate = DateTime.Now,
        Category = "Other",
        Description = string.Empty,
        Amount = 0
    };

    public string[] Categories { get; } = new[]
    {
        "Office Supplies", "Maintenance", "Meals", "Travel", "Utilities", "Other"
    };

    public ICommand LoadExpensesCommand { get; }
    public ICommand SaveExpenseCommand { get; }
    public ICommand DeleteExpenseCommand { get; }

    public ExpenseViewModel(ExpenseApiService service)
    {
        _service = service;
        Expenses.CollectionChanged += (_, __) => OnPropertyChanged(nameof(TotalExpenses));
        LoadExpensesCommand = new RelayCommand(async () => await LoadAsync());
        SaveExpenseCommand = new RelayCommand(async () => await SaveAsync());
        DeleteExpenseCommand = new RelayCommand<Guid>(async id => await DeleteAsync(id), id => id != Guid.Empty);
    }

    public async Task InitializeAsync()
    {
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        var today = DateTime.Now.Date;
        var list = await _service.GetExpensesAsync(today);
        Expenses.Clear();
        foreach (var e in list)
            Expenses.Add(e);
        OnPropertyChanged(nameof(Expenses));
        OnPropertyChanged(nameof(TotalExpenses));
    }

    private async Task SaveAsync()
    {
        if (NewExpense == null || NewExpense.Amount <= 0 || string.IsNullOrWhiteSpace(NewExpense.Category))
        {
            // If you have a DialogService injected, show an error message here. Otherwise, just return.
            return;
        }
        
        if (string.IsNullOrWhiteSpace(NewExpense.Description)) return;
        if (string.IsNullOrWhiteSpace(NewExpense.CreatedBy))
            NewExpense.CreatedBy = POS.UI.Core.AppState.CurrentUserName ?? "System";

        var saved = await _service.CreateExpenseAsync(NewExpense);
        if (saved != null)
        {
            Expenses.Insert(0, saved);
            OnPropertyChanged(nameof(TotalExpenses));
        }

        NewExpense = new ExpenseDto
        {
            ExpenseDate = DateTime.Today,
            Category = "Other",
            Description = string.Empty,
            Amount = 0,
            CreatedBy = POS.UI.Core.AppState.CurrentUserName ?? "System"
        };
        OnPropertyChanged(nameof(NewExpense));
    }

    private async Task DeleteAsync(Guid id)
    {
        if (id == Guid.Empty) return;

        bool success = await _service.DeleteExpenseAsync(id);

        if (success)
        {
            var existing = Expenses.FirstOrDefault(e => e.Id == id);
            if (existing != null)
            {
                Expenses.Remove(existing);
                OnPropertyChanged(nameof(TotalExpenses));
            }
        }
        else
        {
            // Optional: Add logic to show an error popup here if needed
        }
    }
}

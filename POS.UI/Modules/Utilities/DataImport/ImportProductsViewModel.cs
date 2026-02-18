using POS.UI.Core.MVVM;
using POS.UI.Core.Services;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;

namespace POS.UI.Modules.Utilities.DataImport;

public class ImportProductsViewModel : ViewModelBase
{
    private readonly ImportApiService _service;

    public ImportProductsViewModel(ImportApiService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));

        BrowseCommand = new RelayCommand(BrowseFile);
        DownloadTemplateCommand = new RelayCommand(async () => await DownloadTemplateAsync());
        UploadCommand = new RelayCommand(async () => await UploadAsync(), () => !IsUploading && File.Exists(FilePath));
    }

    private string _filePath = string.Empty;
    public string FilePath
    {
        get => _filePath;
        set
        {
            _filePath = value;
            OnPropertyChanged();
            ((RelayCommand)UploadCommand).RaiseCanExecuteChanged();
        }
    }

    private bool _isUploading;
    public bool IsUploading
    {
        get => _isUploading;
        set
        {
            _isUploading = value;
            OnPropertyChanged();
            ((RelayCommand)UploadCommand).RaiseCanExecuteChanged();
        }
    }

    private string _importSummary = string.Empty;
    public string ImportSummary
    {
        get => _importSummary;
        set { _importSummary = value; OnPropertyChanged(); }
    }

    public ICommand BrowseCommand { get; }
    public ICommand DownloadTemplateCommand { get; }
    public ICommand UploadCommand { get; }

    private void BrowseFile()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Excel Files (*.xlsx)|*.xlsx",
            CheckFileExists = true,
            Multiselect = false
        };

        if (dialog.ShowDialog() == true)
        {
            FilePath = dialog.FileName;
            ImportSummary = string.Empty;
        }
    }

    private async Task DownloadTemplateAsync()
    {
        try
        {
            var bytes = await _service.DownloadTemplateAsync();

            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                FileName = $"ProductImportTemplate_{DateTime.Now:yyyyMMddHHmmss}.xlsx"
            };

            if (dialog.ShowDialog() == true)
            {
                File.WriteAllBytes(dialog.FileName, bytes);
                ImportSummary = "Template downloaded successfully.";
            }
        }
        catch (Exception ex)
        {
            ImportSummary = "Error downloading template: " + ex.Message;
        }
    }

    private async Task UploadAsync()
    {
        if (!File.Exists(FilePath))
        {
            ImportSummary = "Please select a valid Excel file.";
            return;
        }

        IsUploading = true;
        ImportSummary = "Uploading and processing file...";

        try
        {
            using var stream = File.OpenRead(FilePath);
            using var content = new MultipartFormDataContent();
            var fileContent = new StreamContent(stream);
            content.Add(fileContent, "file", Path.GetFileName(FilePath));

            var result = await _service.UploadProductsAsync(content);
            if (result == null)
            {
                ImportSummary = "Import completed, but no summary was returned.";
                return;
            }

            if (result.ErrorCount == 0)
            {
                ImportSummary = $"Import successful. Rows processed: {result.RowsProcessed}, Success: {result.SuccessCount}.";
            }
            else
            {
                var message = $"Import completed with errors. Rows processed: {result.RowsProcessed}, Success: {result.SuccessCount}, Errors: {result.ErrorCount}.{Environment.NewLine}";
                foreach (var error in result.Errors)
                {
                    message += error + Environment.NewLine;
                }
                ImportSummary = message;
            }
        }
        catch (Exception ex)
        {
            ImportSummary = "Error during upload: " + ex.Message;
        }
        finally
        {
            IsUploading = false;
        }
    }
}

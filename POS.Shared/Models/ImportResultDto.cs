namespace POS.Shared.Models;

public class ImportResultDto
{
    public int RowsProcessed { get; set; }
    public int SuccessCount { get; set; }
    public int ErrorCount => Errors.Count;
    public List<string> Errors { get; set; } = new();
}


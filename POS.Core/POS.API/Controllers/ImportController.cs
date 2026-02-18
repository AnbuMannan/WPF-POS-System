using System.Net.Mime;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using POS.Application.Interfaces.Services;

namespace POS.API.Controllers;

[ApiController]
[Route("api/import")]
public class ImportController : ControllerBase
{
    private readonly IImportService _importService;

    public ImportController(IImportService importService)
    {
        _importService = importService;
    }

    [HttpPost("products")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> ImportProducts(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        await using var stream = file.OpenReadStream();
        var result = await _importService.ImportProductsAsync(stream);
        return Ok(result);
    }

    [HttpGet("template")]
    public IActionResult GetTemplate()
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Products");

        ws.Cell(1, 1).Value = "Name";
        ws.Cell(1, 2).Value = "Code";
        ws.Cell(1, 3).Value = "Barcode";
        ws.Cell(1, 4).Value = "Price";
        ws.Cell(1, 5).Value = "Cost";
        ws.Cell(1, 6).Value = "Stock";
        ws.Cell(1, 7).Value = "Category";
        ws.Cell(1, 8).Value = "Brand";
        ws.Cell(1, 9).Value = "UOM";

        ws.Row(1).Style.Font.Bold = true;
        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        var fileName = $"ProductImportTemplate_{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx";
        return File(stream.ToArray(), MediaTypeNames.Application.Octet, fileName);
    }
}

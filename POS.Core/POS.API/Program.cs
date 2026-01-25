using MySqlConnector;
using POS.Application.Interfaces.Repositories;
using POS.Application.Interfaces.Services;
using POS.Application.Services;
using POS.Infrastructure.Repositories;
using System.Data;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IDbConnection>(sp =>
    new MySqlConnection(builder.Configuration.GetConnectionString("MySql")));

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();

builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService>();

builder.Services.AddScoped<ITaxProfileRepository, TaxProfileRepository>();
builder.Services.AddScoped<ITaxProfileService, TaxProfileService>();

builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();

builder.Services.AddScoped<IGstReportRepository, GstReportRepository>();
builder.Services.AddScoped<IGstReportService, GstReportService>();

builder.Services.AddScoped<IBrandRepository, BrandRepository>();
builder.Services.AddScoped<IBrandService, BrandService>();

builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
builder.Services.AddScoped<IInventoryService, InventoryService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

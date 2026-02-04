using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using MySqlConnector;
using POS.Application.Interfaces.Repositories;
using POS.Application.Interfaces.Services;
using POS.Application.Services;
using POS.Infrastructure.Repositories;
using POS.Infrastructure.Services;
using System.Data;
using Microsoft.EntityFrameworkCore;
using POS.Infrastructure.Data;
using POS.API.Mappings;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Avoid JsonException when returning EF entities with navigation cycles (e.g. Sale -> SaleItems -> Sale).
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// Register AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IDbConnection>(sp =>
    new MySqlConnection(builder.Configuration.GetConnectionString("MySql")));

var connString = builder.Configuration.GetConnectionString("MySql");
builder.Services.AddDbContext<PosDbContext>(options =>
    options.UseMySql(connString, ServerVersion.AutoDetect(connString), b => b.MigrationsAssembly("POS.API")));

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();

builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService>();

builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<ICustomerService, CustomerService>();

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

builder.Services.AddScoped<IUomRepository, UomRepository>();
builder.Services.AddScoped<IUomService, UomService>();

builder.Services.AddScoped<IBillingService, BillingService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IHeldBillService, HeldBillService>();
builder.Services.AddScoped<IDraftBillService, DraftBillService>();
builder.Services.AddScoped<IReceiptService, ReceiptService>();
builder.Services.AddScoped<IReturnService, ReturnService>();
builder.Services.AddScoped<IEODReportService, POS.Infrastructure.Services.EODReportService>();
builder.Services.AddScoped<IAuditLogService, POS.Infrastructure.Services.AuditLogService>();
builder.Services.AddHostedService<POS.API.Services.HeldBillCleanupService>();

WebApplication app;
try
{
    app = builder.Build();
}
catch (ReflectionTypeLoadException ex)
{
    var loaderErrors = string.Join(Environment.NewLine,
        ex.LoaderExceptions?.Select(e => e?.Message ?? "(null)") ?? Array.Empty<string>());
    throw new InvalidOperationException(
        "ReflectionTypeLoadException: " + ex.Message + Environment.NewLine + "Loader errors: " + loaderErrors,
        ex);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Run pending EF Core migrations on startup (auto-upgrade database schema)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PosDbContext>();
    var pending = db.Database.GetPendingMigrations();
    if (pending.Any())
    {
        db.Database.Migrate();
    }
}

app.Run();

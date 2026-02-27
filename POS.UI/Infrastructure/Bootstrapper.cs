using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using POS.UI.Core.Services;
using POS.UI.Modules.Authentication;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using System.IO;
using System.Net.Http;

namespace POS.UI.Infrastructure
{
    /// <summary>
    /// Bootstrapper for dependency injection container setup.
    /// Configures HttpClientFactory with resilience policies, all API services, logging, and other dependencies.
    /// </summary>
    public static class Bootstrapper
    {
        /// <summary>
        /// Configures all application services and returns the service provider.
        /// </summary>
        public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            // Configure Serilog
            ConfigureLogging(configuration);

            // Add Configuration
            services.AddSingleton<IConfiguration>(configuration);
            services.AddSingleton<LocalSettingsService>();

            // Add Logging
            services.AddLogging(config => config.AddSerilog());

            // Register Authentication Header Handler
            //services.AddTransient<AuthenticationHeaderHandler>();

            // Configure HttpClients
            ConfigureHttpClients(services, configuration);

            // Register API Services
            RegisterApiServices(services);
        }


        /// <summary>
        /// Configures Serilog structured logging with file and debug sinks.
        /// </summary>
        private static void ConfigureLogging(IConfiguration configuration)
        {
            var logsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            
            // Create logs directory if it doesn't exist
            if (!Directory.Exists(logsDir))
            {
                Directory.CreateDirectory(logsDir);
            }

            // Get minimum log level from configuration, default to Information
            var minLogLevel = configuration["Logging:LogLevel:Default"];
            var logEventLevel = minLogLevel switch
            {
                "Debug" => LogEventLevel.Debug,
                "Information" => LogEventLevel.Information,
                "Warning" => LogEventLevel.Warning,
                "Error" => LogEventLevel.Error,
                "Fatal" => LogEventLevel.Fatal,
                _ => LogEventLevel.Information
            };

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Is(logEventLevel)
                // Log to rolling file (daily)
                .WriteTo.File(
                    path: Path.Combine(logsDir, "app-.txt"),
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                    retainedFileCountLimit: 30,  // Keep 30 days of logs
                    fileSizeLimitBytes: 10_485_760,  // 10 MB per file
                    rollOnFileSizeLimit: true
                )
                // Log to debug output (Visual Studio)
                .WriteTo.Debug(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
                )
                // Include context information
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", "POS.UI")
                .CreateLogger();
        }

        /// <summary>
        /// Configures HttpClient factories with retry and circuit breaker policies, plus logging.
        /// </summary>
        /// 

        private static void ConfigureHttpClients(IServiceCollection services, IConfiguration configuration)
        {
            var apiSettings = configuration.GetSection("ApiSettings");
            var baseUrl = apiSettings["BaseUrl"] ?? "https://localhost:7285/";
            var timeoutSeconds = int.TryParse(apiSettings["TimeoutSeconds"], out var timeout) ? timeout : 30;
            var retryCount = int.TryParse(apiSettings["RetryCount"], out var retry) ? retry : 3;
            var failureThreshold = int.TryParse(apiSettings["CircuitBreakerFailureThreshold"], out var threshold) ? threshold : 5;
            var circuitBreakerTimeout = int.TryParse(apiSettings["CircuitBreakerTimeoutSeconds"], out var cbTimeout) ? cbTimeout : 30;

            // 🔥 Register DEFAULT HttpClient for master data loaders (Category / Brand / Tax)
            services.AddHttpClient("DefaultApi", client =>
            {
                client.BaseAddress = new Uri(baseUrl);
                client.DefaultRequestHeaders.Add("User-Agent", "POS-Client/1.0");
                client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
            });
            //.AddHttpMessageHandler<AuthenticationHeaderHandler>();

            var logger = Log.ForContext("Component", "HttpClientConfiguration");
            logger.Information("Configuring HttpClient: BaseUrl={BaseUrl}, Timeout={TimeoutSeconds}s, Retries={RetryCount}, CB Threshold={CBThreshold}", 
                baseUrl, timeoutSeconds, retryCount, failureThreshold);

            // Create retry policy with logging
            var retryPolicy = Policy
                .Handle<HttpRequestException>()
                .OrResult<HttpResponseMessage>(r => (int)r.StatusCode >= 500)
                .WaitAndRetryAsync(
                    retryCount: retryCount,
                    sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                    onRetry: (outcome, timespan, retryCount, context) =>
                    {
                        var retryLogger = Log.ForContext("Component", "RetryPolicy");
                        if (outcome.Exception != null)
                        {
                            retryLogger.Warning(outcome.Exception,
                                "Retry {RetryCount} after {DelaySeconds}s due to exception",
                                retryCount, timespan.TotalSeconds);
                        }
                        else if (outcome.Result?.StatusCode != null)
                        {
                            retryLogger.Warning(
                                "Retry {RetryCount} after {DelaySeconds}s due to HTTP {StatusCode}",
                                retryCount, timespan.TotalSeconds, (int)outcome.Result.StatusCode);
                        }
                    }
                );

            // Create circuit breaker policy with logging
            var circuitBreakerPolicy = Policy
                .Handle<HttpRequestException>()
                .OrResult<HttpResponseMessage>(r => (int)r.StatusCode >= 500)
                .CircuitBreakerAsync<HttpResponseMessage>(failureThreshold, TimeSpan.FromSeconds(circuitBreakerTimeout),
                    onBreak: (outcome, duration) =>
                    {
                        var cbLogger = Log.ForContext("Component", "CircuitBreaker");
                        cbLogger.Error("Circuit breaker opened for {DurationSeconds}s after {FailureThreshold} failures",
                            duration.TotalSeconds, failureThreshold);
                    },
                    onReset: () =>
                    {
                        var cbLogger = Log.ForContext("Component", "CircuitBreaker");
                        cbLogger.Information("Circuit breaker reset");
                    }
                );

            // Register HttpClient for ProductApiService
            services
                .AddHttpClient<ProductApiService>(client =>
                {
                    client.BaseAddress = new Uri(baseUrl);
                    client.DefaultRequestHeaders.Add("User-Agent", "POS-Client/1.0");
                    client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
                })
                //.AddHttpMessageHandler<AuthenticationHeaderHandler>()
                .AddPolicyHandler(retryPolicy)
                .AddPolicyHandler(circuitBreakerPolicy);

            // Register HttpClient for CategoryApiService
            services
                .AddHttpClient<CategoryApiService>(client =>
                {
                    client.BaseAddress = new Uri(baseUrl);
                    client.DefaultRequestHeaders.Add("User-Agent", "POS-Client/1.0");
                    client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
                })
                //.AddHttpMessageHandler<AuthenticationHeaderHandler>()
                .AddPolicyHandler(retryPolicy)
                .AddPolicyHandler(circuitBreakerPolicy);

            // Register HttpClient for CustomerApiService
            services
                .AddHttpClient<CustomerApiService>(client =>
                {
                    client.BaseAddress = new Uri(baseUrl);
                    client.DefaultRequestHeaders.Add("User-Agent", "POS-Client/1.0");
                    client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
                })
                //.AddHttpMessageHandler<AuthenticationHeaderHandler>()
                .AddPolicyHandler(retryPolicy)
                .AddPolicyHandler(circuitBreakerPolicy);

            // Register HttpClient for BrandApiService
            services
                .AddHttpClient<BrandApiService>(client =>
                {
                    client.BaseAddress = new Uri(baseUrl);
                    client.DefaultRequestHeaders.Add("User-Agent", "POS-Client/1.0");
                    client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
                })
                //.AddHttpMessageHandler<AuthenticationHeaderHandler>()
                .AddPolicyHandler(retryPolicy)
                .AddPolicyHandler(circuitBreakerPolicy);

            // Register HttpClient for TaxProfileApiService
            services
                .AddHttpClient<TaxProfileApiService>(client =>
                {
                    client.BaseAddress = new Uri(baseUrl);
                    client.DefaultRequestHeaders.Add("User-Agent", "POS-Client/1.0");
                    client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
                })
                //.AddHttpMessageHandler<AuthenticationHeaderHandler>()
                .AddPolicyHandler(retryPolicy)
                .AddPolicyHandler(circuitBreakerPolicy);

            // Register HttpClient for UomApiService
            services
                .AddHttpClient<UomApiService>(client =>
                {
                    client.BaseAddress = new Uri(baseUrl);
                    client.DefaultRequestHeaders.Add("User-Agent", "POS-Client/1.0");
                    client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
                })
                .AddPolicyHandler(retryPolicy)
                .AddPolicyHandler(circuitBreakerPolicy);

            // Register HttpClient for BillingApiService
            services
                .AddHttpClient<BillingApiService>(client =>
                {
                    client.BaseAddress = new Uri(baseUrl);
                    client.DefaultRequestHeaders.Add("User-Agent", "POS-Client/1.0");
                    client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
                })
                .AddPolicyHandler(retryPolicy)
                .AddPolicyHandler(circuitBreakerPolicy);

            // Register HttpClient for LoyaltyApiService
            services
                .AddHttpClient<LoyaltyApiService>(client =>
                {
                    client.BaseAddress = new Uri(baseUrl);
                    client.DefaultRequestHeaders.Add("User-Agent", "POS-Client/1.0");
                    client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
                })
                .AddPolicyHandler(retryPolicy)
                .AddPolicyHandler(circuitBreakerPolicy);

            // Register HttpClient for ReturnApiService
            services
                .AddHttpClient<ReturnApiService>(client =>
                {
                    client.BaseAddress = new Uri(baseUrl);
                    client.DefaultRequestHeaders.Add("User-Agent", "POS-Client/1.0");
                    client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
                })
                .AddPolicyHandler(retryPolicy)
                .AddPolicyHandler(circuitBreakerPolicy);

            // Register HttpClient for StoreApiService
            services
                .AddHttpClient<StoreApiService>(client =>
                {
                    client.BaseAddress = new Uri(baseUrl);
                    client.DefaultRequestHeaders.Add("User-Agent", "POS-Client/1.0");
                    client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
                })
                .AddPolicyHandler(retryPolicy)
                .AddPolicyHandler(circuitBreakerPolicy);

            // Register HttpClient for EODReportApiService
            services
                .AddHttpClient<EODReportApiService>(client =>
                {
                    client.BaseAddress = new Uri(baseUrl);
                    client.DefaultRequestHeaders.Add("User-Agent", "POS-Client/1.0");
                    client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
                })
                .AddPolicyHandler(retryPolicy)
                .AddPolicyHandler(circuitBreakerPolicy);

            services
                .AddHttpClient<ReportApiService>(client =>
                {
                    client.BaseAddress = new Uri(baseUrl);
                    client.DefaultRequestHeaders.Add("User-Agent", "POS-Client/1.0");
                    client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
                })
                .AddPolicyHandler(retryPolicy)
                .AddPolicyHandler(circuitBreakerPolicy);

            // Register HttpClient for AuditLogApiService
            services
                .AddHttpClient<AuditLogApiService>(client =>
                {
                    client.BaseAddress = new Uri(baseUrl);
                    client.DefaultRequestHeaders.Add("User-Agent", "POS-Client/1.0");
                    client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
                })
                .AddPolicyHandler(retryPolicy)
                .AddPolicyHandler(circuitBreakerPolicy);

            // Register HttpClient for SupplierApiService
            services
                .AddHttpClient<SupplierApiService>(client =>
                {
                    client.BaseAddress = new Uri(baseUrl);
                    client.DefaultRequestHeaders.Add("User-Agent", "POS-Client/1.0");
                    client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
                })
                .AddPolicyHandler(retryPolicy)
                .AddPolicyHandler(circuitBreakerPolicy);

            // Register HttpClient for PurchaseOrderApiService
            services
                .AddHttpClient<PurchaseOrderApiService>(client =>
                {
                    client.BaseAddress = new Uri(baseUrl);
                    client.DefaultRequestHeaders.Add("User-Agent", "POS-Client/1.0");
                    client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
                })
                .AddPolicyHandler(retryPolicy)
                .AddPolicyHandler(circuitBreakerPolicy);

            // Register HttpClient for PurchaseEntryApiService
            services
                .AddHttpClient<PurchaseEntryApiService>(client =>
                {
                    client.BaseAddress = new Uri(baseUrl);
                    client.DefaultRequestHeaders.Add("User-Agent", "POS-Client/1.0");
                    client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
                })
                .AddPolicyHandler(retryPolicy)
                .AddPolicyHandler(circuitBreakerPolicy);

            // Register HttpClient for PurchaseReturnApiService
            services
                .AddHttpClient<PurchaseReturnApiService>(client =>
                {
                    client.BaseAddress = new Uri(baseUrl);
                    client.DefaultRequestHeaders.Add("User-Agent", "POS-Client/1.0");
                    client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
                })
                .AddPolicyHandler(retryPolicy)
                .AddPolicyHandler(circuitBreakerPolicy);

            // Register HttpClient for SupplierPaymentApiService
            services
                .AddHttpClient<SupplierPaymentApiService>(client =>
                {
                    client.BaseAddress = new Uri(baseUrl);
                    client.DefaultRequestHeaders.Add("User-Agent", "POS-Client/1.0");
                    client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
                })
                .AddPolicyHandler(retryPolicy)
                .AddPolicyHandler(circuitBreakerPolicy);

            // Register HttpClient for StockAdjustmentApiService
            services
                .AddHttpClient<StockAdjustmentApiService>(client =>
                {
                    client.BaseAddress = new Uri(baseUrl);
                    client.DefaultRequestHeaders.Add("User-Agent", "POS-Client/1.0");
                    client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
                })
                .AddPolicyHandler(retryPolicy)
                .AddPolicyHandler(circuitBreakerPolicy);

            // Register HttpClient for StockApiService
            services
                .AddHttpClient<StockApiService>(client =>
                {
                    client.BaseAddress = new Uri(baseUrl);
                    client.DefaultRequestHeaders.Add("User-Agent", "POS-Client/1.0");
                    client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
                })
                .AddPolicyHandler(retryPolicy)
                .AddPolicyHandler(circuitBreakerPolicy);

            // Register HttpClient for ItemLedgerApiService
            services
                .AddHttpClient<ItemLedgerApiService>(client =>
                {
                    client.BaseAddress = new Uri(baseUrl);
                    client.DefaultRequestHeaders.Add("User-Agent", "POS-Client/1.0");
                    client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
                })
                .AddPolicyHandler(retryPolicy)
                .AddPolicyHandler(circuitBreakerPolicy);

            // Register HttpClient for CashTransactionApiService
            services
                .AddHttpClient<CashTransactionApiService>(client =>
                {
                    client.BaseAddress = new Uri(baseUrl);
                    client.DefaultRequestHeaders.Add("User-Agent", "POS-Client/1.0");
                    client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
                })
                .AddPolicyHandler(retryPolicy)
                .AddPolicyHandler(circuitBreakerPolicy);

            // Register HttpClient for CompanyProfileApiService
            services
                .AddHttpClient<CompanyProfileApiService>(client =>
                {
                    client.BaseAddress = new Uri(baseUrl);
                    client.DefaultRequestHeaders.Add("User-Agent", "POS-Client/1.0");
                    client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
                })
                .AddPolicyHandler(retryPolicy)
                .AddPolicyHandler(circuitBreakerPolicy);

            // Register HttpClient for SaleReturnApiService
            services
                .AddHttpClient<SaleReturnApiService>(client =>
                {
                    client.BaseAddress = new Uri(baseUrl);
                    client.DefaultRequestHeaders.Add("User-Agent", "POS-Client/1.0");
                    client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
                })
                .AddPolicyHandler(retryPolicy)
                .AddPolicyHandler(circuitBreakerPolicy);

            // Register HttpClient for CustomerPaymentApiService
            services
                .AddHttpClient<CustomerPaymentApiService>(client =>
                {
                    client.BaseAddress = new Uri(baseUrl);
                    client.DefaultRequestHeaders.Add("User-Agent", "POS-Client/1.0");
                    client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
                })
                .AddPolicyHandler(retryPolicy)
                .AddPolicyHandler(circuitBreakerPolicy);

            // Register HttpClient for QuotationApiService
            services
                .AddHttpClient<QuotationApiService>(client =>
                {
                    client.BaseAddress = new Uri(baseUrl);
                    client.DefaultRequestHeaders.Add("User-Agent", "POS-Client/1.0");
                    client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
                })
                .AddPolicyHandler(retryPolicy)
                .AddPolicyHandler(circuitBreakerPolicy);

            services
                .AddHttpClient<ImportApiService>(client =>
                {
                    client.BaseAddress = new Uri(baseUrl);
                    client.DefaultRequestHeaders.Add("User-Agent", "POS-Client/1.0");
                    client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
                })
                .AddPolicyHandler(retryPolicy)
                .AddPolicyHandler(circuitBreakerPolicy);

            services
                .AddHttpClient<DashboardApiService>(client =>
                {
                    client.BaseAddress = new Uri(baseUrl);
                    client.DefaultRequestHeaders.Add("User-Agent", "POS-Client/1.0");
                    client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
                })
                .AddPolicyHandler(retryPolicy)
                .AddPolicyHandler(circuitBreakerPolicy);

            logger.Information("HttpClient configuration complete");
        }

        /// <summary>
        /// Registers all API service classes as scoped services.
        /// </summary>
        private static void RegisterApiServices(IServiceCollection services)
        {
            var logger = Log.ForContext("Component", "ServiceRegistration");
            logger.Debug("Registered API services");

            // Print and receipt
            services.AddSingleton<IPrintService, PrintService>();
            services.AddSingleton<IPrintSettingsService, PrintSettingsService>();
            services.AddSingleton<IEmailReceiptService, EmailReceiptService>();

            // System Health
            services.AddSingleton<SystemHealthService>();
            services.AddTransient<POS.UI.Modules.Utilities.SystemHealth.SystemHealthViewModel>();

            // ViewModels
            services.AddTransient<POS.UI.Modules.Billing.QuickSale.QuickSaleViewModel>(sp => 
            {
                return new POS.UI.Modules.Billing.QuickSale.QuickSaleViewModel(
                    sp.GetRequiredService<ProductApiService>(),
                    sp.GetRequiredService<CategoryApiService>(),
                    sp.GetRequiredService<BillingApiService>(),
                    sp.GetRequiredService<StockApiService>(),
                    sp.GetRequiredService<IPrintSettingsService>()
                );
            });

            services.AddTransient<POS.UI.Modules.Settings.SettingsViewModel>();
            services.AddTransient<POS.UI.Modules.Diagnostics.HealthViewModel>();
        }

        /// <summary>
        /// Configures and registers authentication and license services.
        /// </summary>
        public static void RegisterAuthenticationServices(IServiceCollection services, IConfiguration configuration)
        {
            var authSettings = configuration.GetSection("AuthSettings");
            var configuredBaseUrl = configuration["AuthApiBaseUrl"];
            var authBaseUrl = !string.IsNullOrWhiteSpace(configuredBaseUrl)
                ? configuredBaseUrl
                : (authSettings["BaseUrl"] ?? "https://localhost:7143/");  // fallback for auth service
            var timeoutSeconds = int.TryParse(authSettings["TimeoutSeconds"], out var timeout) ? timeout : 30;

            var logger = Log.ForContext("Component", "AuthenticationConfiguration");
            logger.Information("Configuring Authentication Service: BaseUrl={AuthBaseUrl}", authBaseUrl);

            // Register HttpClient for AuthenticationService (separate from main API)
            services
                .AddHttpClient<AuthenticationService>(client =>
                {
                    client.BaseAddress = new Uri(authBaseUrl);
                    client.DefaultRequestHeaders.Add("User-Agent", "POS-Client/1.0");
                    client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
                });

            // Register HttpClient for UserApiService (to AuthService for user management)
            services
                .AddHttpClient<UserApiService>(client =>
                {
                    client.BaseAddress = new Uri(authBaseUrl);
                    client.DefaultRequestHeaders.Add("User-Agent", "POS-Client/1.0");
                    client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
                });

            // Register HttpClient for LicenseService (can be different endpoint)
            var licenseBaseUrl = authSettings["LicenseBaseUrl"] ?? authBaseUrl;
            services
                .AddHttpClient<LicenseService>(client =>
                {
                    client.BaseAddress = new Uri(licenseBaseUrl);
                    client.DefaultRequestHeaders.Add("User-Agent", "POS-Client/1.0");
                    client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
                });

            // Register services as singletons (one instance per app)
            services.AddSingleton<AuthenticationService>();
            services.AddSingleton<LicenseService>();
            // Register ViewModels
            services.AddTransient<LoginViewModel>();
            services.AddTransient<ActivationViewModel>();


            logger.Information("Authentication services registered successfully");
        }

        /// <summary>
        /// Flushes and closes Serilog logger.
        /// Should be called when application shuts down.
        /// </summary>
        public static void CloseLogging()
        {
            Log.CloseAndFlush();
        }
    }
}

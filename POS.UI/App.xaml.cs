using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using POS.UI.Core.Services;
using POS.UI.Infrastructure;
using POS.UI.Modules.Authentication;
using Serilog;
using System;
using System.IO;
using System.Windows;
using Application = System.Windows.Application;

namespace POS.UI
{
    public partial class App : Application
    {
        /// <summary>
        /// Global service provider for dependency injection.
        /// Database (AppDbContext/EF migrations) runs in the API project, not in the WPF client.
        /// </summary>
        public static IServiceProvider? ServiceProvider { get; private set; }

        public App()
        {
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private static void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Serilog.Log.Error(e.Exception, "Unhandled UI exception");
            POS.UI.Components.DialogService.Error("Error", $"An error occurred: {e.Exception.Message}");
            // Close any modal (e.g. Payment dialog) that may be stuck so the app stays responsive
            CloseStuckModalWindows();
            e.Handled = true;
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            Serilog.Log.Fatal(ex, "Unhandled exception (non-UI thread)");
            if (ex != null)
            {
                POS.UI.Components.DialogService.Error("Fatal Error", ex.Message);
                Current.Dispatcher?.BeginInvoke(() => CloseStuckModalWindows());
            }
        }

        /// <summary>
        /// Closes modal windows (e.g. Payment dialog) that may be stuck after an exception,
        /// so the main window does not remain blocked and the app stays responsive.
        /// </summary>
        private static void CloseStuckModalWindows()
        {
            try
            {
                var main = Current.MainWindow;
                foreach (Window w in Current.Windows)
                {
                    if (w == main || !w.IsLoaded) continue;
                    if (w.Title == "Payment" || w.Title == "Customer Display")
                    {
                        w.DialogResult = false;
                        w.Close();
                    }
                }
            }
            catch { /* ignore */ }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                // Load configuration
                var environment = GetEnvironment();
                var configuration = LoadConfiguration(environment);

                // Configure DI container (includes Serilog)
                //ServiceProvider = Bootstrapper.ConfigureServices(configuration);

                //// Register authentication services
                //var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
                //services.AddSingleton(configuration);
                //Bootstrapper.RegisterAuthenticationServices(services, configuration);
                //ServiceProvider = services.BuildServiceProvider();

                // Build ONE service collection
                var services = new ServiceCollection();

                // Configure core services (logging, http clients, ProductApiService, etc.)
                Bootstrapper.ConfigureServices(services, configuration);

                // Register authentication services INTO SAME CONTAINER
                Bootstrapper.RegisterAuthenticationServices(services, configuration);

                // 🔥 Build only once
                ServiceProvider = services.BuildServiceProvider();



                // Log application startup
                var logger = Log.ForContext<App>();
                logger.Information("=== Application Started ===");
                logger.Information("Environment: {Environment}", environment);
                logger.Information("BaseAddress: {BaseAddress}", configuration["ApiSettings:BaseUrl"]);
                logger.Information("AuthBaseAddress: {AuthBaseAddress}", configuration["AuthSettings:BaseUrl"]);

                // Apply theme if configured
                ApplyTheme();

                // ===============================
                // 🔥 START LOGIN FLOW HERE
                // ===============================

                var loginView = new LoginView();

                // Resolve LoginViewModel from DI
                var loginViewModel = ServiceProvider.GetService<LoginViewModel>();

                if (loginViewModel == null)
                {
                    POS.UI.Components.DialogService.Error("DI Error", "LoginViewModel not resolved from DI");
                    Shutdown();
                    return;
                }

                loginView.DataContext = loginViewModel;

                // Subscribe to login success
                loginViewModel.LoginSucceeded += (s, args) =>
                {
                    POS.UI.Core.AppState.SetUser(loginViewModel.Username, "Cashier");
                    var mainWindow = new MainWindow();
                    Current.MainWindow = mainWindow;
                    mainWindow.Show();
                    loginView.Close();
                };

                // Show Login first
                loginView.Show();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fatal error during application startup");
                POS.UI.Components.DialogService.Error("Startup Error", $"Failed to initialize application: {ex.Message}");
                Shutdown(1);
            }
        }
        //private void Application_Startup(object sender, StartupEventArgs e)
        //{
        //    // Create Login window
        //    var loginView = new LoginView();

        //    // 🔥 Resolve ViewModel from DI (correct way)
        //    var loginViewModel = App.ServiceProvider.GetService<LoginViewModel>();

        //    if (loginViewModel == null)
        //    {
        //        POS.UI.Components.DialogService.Error("DI Error", "LoginViewModel not resolved from DI");
        //        Shutdown();
        //        return;
        //    }

        //    loginView.DataContext = loginViewModel;

        //    // Subscribe to login success event
        //    loginViewModel.LoginSucceeded += (s, args) =>
        //    {
        //        var mainWindow = new MainWindow();
        //        Current.MainWindow = mainWindow;
        //        mainWindow.Show();
        //        loginView.Close();
        //    };

        //    // Show Login first
        //    loginView.Show();
        //}

        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                var logger = Log.ForContext<App>();
                logger.Information("=== Application Shutting Down ===");
            }
            catch
            {
                // Ignore logging errors during shutdown
            }
            finally
            {
                // Flush and close Serilog
                Bootstrapper.CloseLogging();
            }

            base.OnExit(e);
        }

        /// <summary>
        /// Loads configuration from appsettings.json and environment-specific overrides.
        /// </summary>
        private static IConfiguration LoadConfiguration(string environment)
        {
            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            // Load environment-specific overrides if they exist
            if (!string.IsNullOrEmpty(environment))
            {
                var envConfigPath = $"appsettings.{environment}.json";
                if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, envConfigPath)))
                {
                    configBuilder.AddJsonFile(envConfigPath, optional: true, reloadOnChange: true);
                }
            }

            return configBuilder.Build();
        }

        /// <summary>
        /// Determines the current environment (Development, Staging, Production).
        /// </summary>
        private static string GetEnvironment()
        {
            return Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") 
                ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                ?? "Production";
        }

        /// <summary>
        /// Applies the application theme from configuration or user preference.
        /// </summary>
        private static void ApplyTheme()
        {
            // Theme application logic can be implemented here
            // For now, this is a placeholder for future enhancement
        }
    }
}

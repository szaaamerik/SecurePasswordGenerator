using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SecurePasswordGenerator.Services;
using SecurePasswordGenerator.ViewModels.Windows;
using Wpf.Ui;

namespace SecurePasswordGenerator;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App
{
    private static readonly IHost Host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
        .ConfigureAppConfiguration(c =>
        {
            c.SetBasePath(AppContext.BaseDirectory);
        }).
        ConfigureServices((_, services) =>
        {
            services.AddHostedService<ApplicationHostService>();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<MainWindowViewModel>();
            services.AddSingleton<INavigationWindow, Views.Windows.MainWindow>();
        }).Build();
    
    
    private async void App_OnStartup(object sender, StartupEventArgs e) => await Host.StartAsync();

    private async void App_OnExit(object sender, ExitEventArgs e)
    {
        await Host.StopAsync();
        Host.Dispose();
    }
}
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using TripleTriad.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TripleTriad;
/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
    private readonly IHost _host;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private Window? _window;

    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        InitializeComponent();
        _host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration(ConfigureAppConfiguration)
            .ConfigureServices(ConfigureServices)
            .Build();
        _cancellationTokenSource = new CancellationTokenSource();
    }

    private void ConfigureAppConfiguration(HostBuilderContext hostContext, IConfigurationBuilder configuration)
    {

    }

    private void ConfigureServices(HostBuilderContext hostContext, IServiceCollection services)
    {
        services.AddCardRepository();
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<MainWindow>();
    }

    /// <summary>
    /// Invoked when the application is launched normally by the end user.  Other entry points
    /// will be used such as when the application is launched to open a specific file.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        await _host.StartAsync(_cancellationTokenSource.Token);
        _window = _host.Services.GetRequiredService<MainWindow>();
        _window.Closed += (s, e) => _cancellationTokenSource.Cancel();
        _window.Activate();
    }
}

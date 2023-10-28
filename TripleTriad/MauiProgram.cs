using Microsoft.Extensions.Logging;
using TripleTriad.Pages;
using TripleTriad.ViewModels;

namespace TripleTriad;
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // ViewModels.
        builder.Services.AddSingleton<MainViewModel>();
        // Pages.
        builder.Services.AddSingleton<MainPage>();
#if DEBUG
		//builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}

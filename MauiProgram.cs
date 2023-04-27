namespace Frontend;
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;
using Microsoft.Extensions.Configuration;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseBarcodeReader()
            .ConfigureFonts(fonts => {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            })
            .UseSentry(options => {
                // The DSN is the only required setting.
                options.Dsn = "https://bc3f5106f50c4525b5b7213364063062@o86280.ingest.sentry.io/4505029167284224";
            });

#if ANDROID || IOS
        builder.UseShiny();
        builder.Configuration.AddJsonPlatformBundle(optional: false);

        var cfg = builder.Configuration.GetSection("Firebase");
        builder.Services.AddPushFirebaseMessaging(new(
            false,
            cfg["AppId"],
            cfg["SenderId"],
            cfg["ProjectId"],
            cfg["ApiKey"]
        ));
#endif
        return builder.Build();
    }
}

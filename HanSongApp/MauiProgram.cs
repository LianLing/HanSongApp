using CommunityToolkit.Maui;
using HanSongApp.DataBase;
using HanSongApp.Services;
using HanSongApp.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Security;
using System.Reflection;

namespace HanSongApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            // 注册HttpClient
            builder.Services.AddSingleton<HttpClient>(serviceProvider =>
            {
                var handler = new HttpClientHandler();

#if ANDROID
                // Android上信任所有HTTPS证书（开发环境使用）
                handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
                {
                    if (errors == SslPolicyErrors.None) return true;
#if DEBUG
                    return true; // 调试模式信任所有证书
#else
                return errors == SslPolicyErrors.None;
#endif
                };
#endif

                return new HttpClient(handler);
            });

            // 注册API服务
            builder.Services.AddSingleton<ApiService>();

            // 注册其他所需服务
            builder.Services.AddSingleton<IConnectivity>(Connectivity.Current);

            // 注册所有页面
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<SingleRepositoryInPage>();
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<SelectConditionPage>();
            

            // 注册数据库上下文
            builder.Services.AddSingleton<DbContext>();

            // 注册服务
            builder.Services.AddSingleton<CheckStationService>();

            // 配置导航服务
            var app = builder.Build();
            return builder.Build();
        }

    }
}

using CommunityToolkit.Maui;
using HanSongApp.DataBase;
using HanSongApp.Services;
using HanSongApp.Views;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.LifecycleEvents;

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
                })
                .ConfigureLifecycleEvents(events =>
                {
#if ANDROID
                    // Android特定生命周期配置
                    events.AddAndroid(android => android
                        .OnCreate((activity, bundle) => ConfigureAndroidSecurity(activity))
                    );
#endif
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            // 注册HttpClient服务（带安全配置）
            builder.Services.AddHttpClient("SecureClient", httpClient =>
            {
#if ANDROID
                // 添加基础URL（针对Android的特殊安全需求）
                httpClient.BaseAddress = new Uri("http://10.10.38.158:8201/htsapi/db1v0/");
#else
                httpClient.BaseAddress = new Uri("https://api.industrial.com/v1");
#endif

                httpClient.Timeout = TimeSpan.FromSeconds(30);
            });

            // 注册所有服务
            builder.Services.AddSingleton<ApiService>();
            builder.Services.AddSingleton<IConnectivity>(Connectivity.Current);
            builder.Services.AddSingleton<DbContext>();
            builder.Services.AddSingleton<CheckStationService>();

            // 注册所有页面
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<SingleRepositoryInPage>();
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<SelectConditionPage>();

            return builder.Build();
        }

#if ANDROID
        /// <summary>
        /// 安卓平台安全配置 - 解决明文HTTP访问限制
        /// </summary>
        private static void ConfigureAndroidSecurity(Android.App.Activity activity)
        {
            try
            {
                // Android 9.0+的特殊处理
                if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.P)
                {
                    // 使用反射设置安全策略
                    var policyClass = Java.Lang.Class.ForName("android.net.NetworkSecurityPolicy");
                    var setMethod = policyClass.GetMethod("setCleartextTrafficPermitted", Java.Lang.Boolean.Type);
                    setMethod.Invoke(null, true);
                    
                    // 为特定IP地址添加例外
                    var builderClass = Java.Lang.Class.ForName("android.net.NetworkSecurityPolicy$Builder");
                    var newPolicy = builderClass.NewInstance();
                    var addDomainMethod = builderClass.GetMethod("addDomain", Java.Lang.Class.FromType(typeof(Java.Lang.String)), Java.Lang.Boolean.Type);
                    addDomainMethod.Invoke(newPolicy, "10.10.38.158", true);
                    addDomainMethod.Invoke(newPolicy, "10.10.38.158:8201", true);
                    
                    var buildMethod = builderClass.GetMethod("build");
                    var finalPolicy = buildMethod.Invoke(newPolicy);
                    
                    var setInstanceMethod = policyClass.GetMethod("setInstance", policyClass);
                    setInstanceMethod.Invoke(null, finalPolicy);
                }
            }
            catch (Exception ex)
            {
                // 记录错误但不中断应用启动
                System.Diagnostics.Debug.WriteLine($"安全配置失败: {ex.Message}");
            }
        }
#endif
    }
}
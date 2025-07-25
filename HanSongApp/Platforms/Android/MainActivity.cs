using Android.App;
using Android.Content.PM;
using Android.OS;
using Java.Lang;

namespace HanSongApp
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            // 关键代码：强制允许明文HTTP流量
            if (Build.VERSION.SdkInt >= BuildVersionCodes.P) // Android 9+
            {
                // 核心解决方法 - 强制允许所有明文流量
                SetSystemProperty("proxyHost", "false");
                SetSystemProperty("proxyPort", "false");
            }

            //try
            //{
            //    var policyType = Type.GetType("Android.Net.NetworkSecurityPolicy, Mono.Android");
            //    var policy = policyType?.GetProperty("Instance")?.GetValue(null);
            //    var setMethod = policyType?.GetMethod("SetCleartextTrafficPermitted", new[] { typeof(string), typeof(bool) });
            //    setMethod?.Invoke(policy, new object[] { "10.10.38.158", true });
            //    setMethod?.Invoke(policy, new object[] { "10.10.38.158:8201", true });
            //}
            //catch { /* 忽略 */ }


            base.OnCreate(savedInstanceState);
        }
        private void SetSystemProperty(string key, string value)
        {
            try
            {
                JavaSystem.SetProperty(key, value);
            }
            catch
            {
                // 忽略错误
            }
        }
    }
}

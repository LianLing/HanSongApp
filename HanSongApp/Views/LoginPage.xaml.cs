using HanSongApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanSongApp.Views
{
    public partial class LoginPage : ContentPage
    {
        private readonly ApiService _apiService;

        public LoginPage(ApiService apiService)
        {
            InitializeComponent();
            _apiService = apiService;
            Loaded += (sender, e) => usernameEntry.Focus();
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            var username = usernameEntry.Text?.Trim();
            var password = passwordEntry.Text;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                await DisplayAlert("错误", "用户名和密码不能为空", "确定");
                return;
            }

            loadingIndicator.IsVisible = true;
            loginButton.IsEnabled = false;

            try
            {
                var response = await _apiService.LoginAsync(username, password);

                if (response.IfSuccess)
                {
                    // 保存token
                    _apiService.SetAccessToken(response.Data.AccessToken);

                    // 导航到主页面
                    await Shell.Current.GoToAsync("//main");
                }
                else
                {
                    await DisplayAlert("登录失败",
                        $"{response.ErrorMessage} ({response.ErrorCode})",
                        "重试");
                }
            }
            finally
            {
                loadingIndicator.IsVisible = false;
                loginButton.IsEnabled = true;
            }
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string DeviceId { get; set; }
    }

    public class LoginResponse
    {
        public string AccessToken { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string DisplayName { get; set; }
    }
}

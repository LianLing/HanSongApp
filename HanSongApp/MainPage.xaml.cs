using HanSongApp.Views;

namespace HanSongApp
{
    public partial class MainPage : ContentPage
    {
        private readonly IServiceProvider _serviceProvider;

        public MainPage(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;
        }

        private async void SignIn_Clicked(object sender, EventArgs e)
        {
            // 通过依赖注入获取页面
            var conditionPage = _serviceProvider.GetRequiredService<SelectConditionPage>();
            await Navigation.PushAsync(conditionPage);
        }
    }

}

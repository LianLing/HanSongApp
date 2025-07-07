using HanSongApp.Views;

namespace HanSongApp
{
    public partial class MainPage : ContentPage
    {

        public MainPage()
        {
            InitializeComponent();
        }


        private void SignIn_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new ScanBarCodePage());
        }
    }

}

using HanSongApp.Services;

namespace HanSongApp.Views
{
    public partial class SingleRepositoryInPage : ContentPage
    {
        CheckStationService stationService = new(); 
        private readonly ApiService _apiService;
        public SingleRepositoryInPage(ApiService apiService)
        {
            apiService = _apiService;
            var result = stationService.GetStations();
            //var resul1 = _apiService.GetStations(input, type_code); 
        }

        private void OnBarcodeTextChanged(object sender, TextChangedEventArgs e)
        {
            // 扫码输入框文本变化时处理逻辑
        }

        private void OnBarcodeCompleted(object sender, EventArgs e)
        {
            //扫码完成后处理逻辑

        }

        private void OnSubmitClicked(object sender, EventArgs e)
        {
            // 提交按钮点击事件处理
        }

        private void OnViewHistoryClicked(object sender, EventArgs e)
        {

        }
    }
}
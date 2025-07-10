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

        }

        private void OnBarcodeCompleted(object sender, EventArgs e)
        {

        }

        private void OnSubmitClicked(object sender, EventArgs e)
        {

        }
        private void OnViewHistoryClicked(object sender, EventArgs e)
        {

        }
    }
}
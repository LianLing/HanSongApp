using HanSongApp.Services;

namespace HanSongApp.Views
{
    public partial class ScanBarCodePage : ContentPage
    {
        CheckStationService stationService = new(); 
        public ScanBarCodePage()
        {
            var result = stationService.GetStations();
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
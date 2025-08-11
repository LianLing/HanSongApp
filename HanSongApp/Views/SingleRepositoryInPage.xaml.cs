// SingleRepositoryInPage.xaml.cs
using HanSongApp.Models;
using HanSongApp.Models.HtsModels;
using HanSongApp.Services;
using Microsoft.IdentityModel.Tokens;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace HanSongApp.Views
{
    public partial class SingleRepositoryInPage : ContentPage,INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private ProductInfo _currentFilter;

        private const string BaseUrl = "http://10.10.38.158:8201";
        private const string GetSnInfoEndpoint = "/htsapi/db1v0/get_sn_info";
        private const string ChkInEndpoint = "/htsapi/db1v0/chk_in";
        private const string ChkOutEndpoint = "/htsapi/db1v0/chk_out";
        private readonly HttpClient _httpClient;

        //数据绑定属性
        public ObservableCollection<Station> Stations { get; } = new();
        public bool IsLoading { get; private set; }
        public string ErrorMessage { get; private set; }
        public int chkOutCount { get; set; } = 0;     //过站次数
        public string errorMessage { get; set; } = string.Empty;

        public SingleRepositoryInPage(ApiService apiService)
        {
            InitializeComponent();
            _apiService = apiService;

            // 绑定数据上下文
            BindingContext = this;
        }

        public void SetFilter(ProductInfo filter)
        {
            _currentFilter = filter;
            LoadData();
        }

        private async void LoadData()
        {
            try
            {
                // 1. 更新加载状态
                SetLoadingState(true, null);

                // 2. 调用API获取数据
                //var apiResponse = await _apiService.PostAsync<ProductInfo, List<Station>>(
                //    "station/getStationsByFilter",
                //    _currentFilter,
                //    retryOnFailure: true);

                //// 3. 处理API响应
                //if (apiResponse.IfSuccess)
                //{
                //    // 清空现有数据
                //    Stations.Clear();

                //    // 添加新数据
                //    foreach (var station in apiResponse.Data)
                //    {
                //        Stations.Add(station);
                //    }
                //}
                //else
                //{
                //    SetLoadingState(false, $"加载失败: {apiResponse.ErrorMessage}");
                //}
            }
            catch (Exception ex)
            {
                SetLoadingState(false, $"发生异常: {ex.Message}");
            }
            finally
            {
                SetLoadingState(false, null);
            }
        }


        // 更新加载状态和错误信息
        private void SetLoadingState(bool isLoading, string errorMessage)
        {
            // 确保在主线程更新UI
            MainThread.BeginInvokeOnMainThread(() =>
            {
                IsLoading = isLoading;
                ErrorMessage = errorMessage;
                OnPropertyChanged(nameof(IsLoading));
                OnPropertyChanged(nameof(ErrorMessage));
            });
        }

        // 扫码输入框文本变化
        private async void OnBarcodeTextChanged(object sender, TextChangedEventArgs e)
        {
            // 实现扫码逻辑...
            if (string.IsNullOrEmpty(barcodeEntry.Text))
            {
                return;
            }
        }

        // 扫码完成
        private async void OnBarcodeCompleted(object sender, EventArgs e)
        {
            //先隐藏过站结果
            chkOutFrame.IsVisible = false;
            // 实现扫码完成逻辑...
            if (string.IsNullOrEmpty(barcodeEntry.Text))
                return;
            try
            {
                SetLoadingState(true, "正在处理入站...");

                // 验证网络连接
                if (!_apiService.IsNetworkAvailable())
                {
                    await DisplayAlert("网络错误", "网络不可用，请检查连接", "确定");
                    return;
                }
                ChkInReq chkInReq = new ChkInReq()
                {
                    input = new SnInfo()
                    {
                        type = "sn",
                        key = "",
                        val = barcodeEntry.Text
                    },
                    type_code = _currentFilter.prod_type,
                    module_code = _currentFilter.prod_process_grp,
                    stage_code = _currentFilter.prod_model,
                    process_code = _currentFilter.prod_process,
                    station_code = _currentFilter.Station,
                    mo = _currentFilter.Mo,
                    userid = "1023711",
                    host_name = "111",
                    prod_line = _currentFilter.Line,
                    team = _currentFilter.ClassTeam,
                    cfg_id = "",
                    timeouts = 1
                };
                //调用API服务
                var response = await _apiService.ChkInAsync(chkInReq);

                if (!response.IfSuccess)
                {
                    await DisplayAlert("请求失败", $"网络错误: {response.ErrorMessage}", "确定");
                    return;
                }
                //处理响应结果
                if (response.Data?.result == "SUCCESS")
                {
                    //await DisplayAlert("入站成功", $"SN: {barcodeEntry.Text} 已成功入站", "确定");

                    // 执行后续操作...过站数+1
                    //调用chk_out
                    ChkOutReq chkOutReq = new ChkOutReq()
                    {
                        sn = barcodeEntry.Text,
                        type_code = _currentFilter.prod_type,
                        module_code = _currentFilter.prod_process_grp,
                        stage_code = _currentFilter.prod_model,
                        process_code = _currentFilter.prod_process,
                        station_code = _currentFilter.Station,
                        station_vercode = "00", 
                        station_next = "", 
                        test_rst = 0, 
                        note = "备注",
                        jsonLog = "",
                        mo = _currentFilter.Mo,
                        elapse = 0,
                        err_code = "",
                        userid = "1023711",
                        host_name = "111",
                        prod_line = _currentFilter.LineId,
                        team = _currentFilter.ClassTeam,
                        tool_name = "单板入库",
                        tool_ver = "1.0.0.0"
                    };
                    var responseResult = await _apiService.ChkOutAsync(chkOutReq);
                    if (!responseResult.IfSuccess)
                    {
                        await DisplayAlert("请求失败", $"网络错误: {responseResult.ErrorMessage}", "确定");
                        return;
                    }
                    if (responseResult.Data?.result == "SUCCESS")
                    {
                        chkOutCount += 1;
                        passCount.Text = chkOutCount.ToString();
                        //await DisplayAlert("出站成功", $"SN: {barcodeEntry.Text} 已成功出站", "确定");
                        {
                            chkOutFrame.IsVisible = true;
                            chkOutResult.IsVisible = true;
                            chkOutResult.BackgroundColor = Color.FromRgba("#4CAF50");
                            chkOutResult.Text = "PASS";
                            
                        }
                    }
                    else
                    {
                        errorMessage = responseResult.Data.message;
                        //await DisplayAlert("出站失败", $"原因: {errorMsg}", "确定");
                        {
                            chkOutFrame.IsVisible = true;
                            chkOutResult.IsVisible = true;
                            chkOutResult.BackgroundColor = Color.FromRgba("#FF0000");
                            chkOutResult.Text = "NG";
                            
                        }
                    }
                }
                else
                {
                    errorMessage = response.Data?.message ?? "未知错误";
                    chkOutFrame.IsVisible = true;
                    chkOutResult.IsVisible = true;
                    chkOutResult.BackgroundColor = Color.FromRgba("#FF0000");
                    chkOutResult.Text = "NG";
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                SetLoadingState(false, null);
                barcodeEntry.Text = ""; // 清空扫码框
            }
        }

        // 查看历史
        private async void OnViewHistoryClicked(object sender, EventArgs e)
        {
            // 实现查看历史逻辑...
        }

        private void ChkOutResult_Tapped(object sender, TappedEventArgs e)
        {
            chkOutResult.Text = errorMessage;
        }
    }
}
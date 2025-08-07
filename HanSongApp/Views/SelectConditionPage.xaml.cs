
using Dm.util;
using HanSongApp.Models;
using HanSongApp.Models.HtsModels;
using HanSongApp.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace HanSongApp.Views
{
    public partial class SelectConditionPage : ContentPage
    {
        private readonly IServiceProvider _serviceProvider;
        // 选中的条件
        public string? SelectedProdType { get; set; }
        public string? SelectedModel { get; set; }
        public string? SelectedModule { get; set; }
        public string? SelectedProcess { get; set; }
        public string? SelectedLine { get; set; }
        public string? LineId { get; set; }
        public string? SelectedClassTeam { get; set; }
        public string? SelectedMo { get; set; }

        public string type_code = string.Empty;
        public string Station { get; set; } 

        // 数据服务
        private readonly GetConditionService _dataService = new GetConditionService();

        public SelectConditionPage(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            // 页面加载时初始化数据
            Loaded += async (sender, e) => await InitializeDataAsync();
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        private async Task InitializeDataAsync()
        {
            try
            {
                // 加载机型数据
                List<Prod_TypeModel> prodtypes = await _dataService.OnProdTypeSelected();
                prodTypePicker.ItemsSource = prodtypes.Count > 0 ? prodtypes : new List<string>();

                // 加载线别数据
                List<LineModel> lines = await _dataService.GetLineList();
                LinePicker.ItemsSource = lines.Count > 0 ? lines : new List<string>();
                //加载班组数据
                List<TeamModel> classTeams = await _dataService.GetClassTeamList();
                ClassTeamPicker.ItemsSource = classTeams.Count > 0 ? classTeams : new List<string>();
            }
            catch (Exception ex)
            {
                await DisplayAlert("初始化错误", $"加载筛选条件失败: {ex.Message}", "确定");
            }
        }

        // 重置按钮点击
        private void OnResetClicked(object sender, EventArgs e)
        {
            // 重置所有选择
            prodTypePicker.SelectedIndex = -1;
            ModelPicker.SelectedIndex = -1;
            ModulePicker.SelectedIndex = -1;
            MoPicker.SelectedIndex = -1;
            LinePicker.SelectedIndex = -1;
            ClassTeamPicker.SelectedIndex = -1;
            ProcessPicker.SelectedIndex = -1;

            // 重置选中值
            SelectedLine = null;
            SelectedProdType = null;
            SelectedModel = null;
            SelectedModule = null;
            SelectedProcess = null;
            SelectedClassTeam = null;

        }

        // 确认按钮点击
        private async void OnConfirmClicked(object sender, EventArgs e)
        {
            
            // 构建筛选条件
            var filter = new ProductInfo
            {
                prod_type = type_code ?? "",
                prod_model = SelectedModel??"",
                prod_process_grp = SelectedModule??"",
                prod_process = SelectedProcess??"",
                Line = SelectedLine??"",
                LineId = LineId ?? "",
                ClassTeam = SelectedClassTeam??"",
                Mo = SelectedMo.Substring(0, SelectedMo.indexOf(",")) ??""
            };
            //查询站点
            Station = await _dataService.GetStation(filter);
            filter.Station = Station;

            // 通过依赖注入获取页面
            var singlepage = _serviceProvider.GetRequiredService<SingleRepositoryInPage>();
            //设置筛选条件到目标页面的公共属性
            singlepage.SetFilter(filter);
            await Navigation.PushAsync(singlepage);
        }

        private async void OnProdTypeSelected(object sender, EventArgs e)
        {
            var picker = (Picker)sender;
            //机型决定数据库
            //SelectedProdType = picker.SelectedItem.ToString();
            var selectedItem = picker.SelectedItem as Prod_TypeModel;
            SelectedProdType = selectedItem?.name;
            type_code = selectedItem?.code;
            // 根据选中的机型加载对应的工序、模组、制程、线别
            if (type_code != null)
            {
                //获取工序列表
                var models= await _dataService.GetModelList(type_code);
                ModelPicker.ItemsSource = models.Count > 0 ? models : new List<modelModel>();
                //获取模组列表
                ModulePicker.ItemsSource = models.Count > 0 ? models : new List<modelModel>();
                // 获取制程列表
                var processes = await _dataService.GetProcessList(type_code);
                ProcessPicker.ItemsSource = processes.Count > 0 ? processes.Select(p => p).ToList() : new List<string>();
                
            }
        }

        private void OnModelSelected(object sender, EventArgs e)
        {
            var picker = (Picker)sender;
            //SelectedModel = picker.SelectedItem.ToString();
            var selectedItem = picker.SelectedItem as modelModel;
            SelectedModel = selectedItem?.prod_model;
        }

        private void OnModuleTypeSelected(object sender, EventArgs e)
        {
            var picker = (Picker)sender;
            //SelectedModule = picker.SelectedItem.ToString();
            var selectedItem = picker.SelectedItem as modelModel;
            SelectedModule = selectedItem?.prod_module;
        }

        private void OnProcessSelected(object sender, EventArgs e)
        {
            //var picker = (Picker)sender;
            //SelectedProcess = picker.SelectedItem.ToString();
            SelectedProcess = ProcessPicker.SelectedItem?.ToString() ?? string.Empty;
        }

        private void OnLineSelected(object sender, EventArgs e)
        {
            var picker = (Picker)sender;
            //SelectedLine = picker.SelectedItem.ToString();
            var selectedItem = picker.SelectedItem as LineModel;
            SelectedLine = selectedItem?.name;
            LineId = selectedItem?.code;
        }

        private async void OnClassTeamSelected(object sender, EventArgs e)
        {
            var picker = (Picker)sender;
            //SelectedClassTeam = picker.SelectedItem.ToString();
            var selectedItem = picker.SelectedItem as TeamModel;
            SelectedClassTeam = selectedItem?.name;
            string classTeamCode = selectedItem?.code ?? string.Empty;
            // 获取工单列表
            if (!string.IsNullOrEmpty(classTeamCode))
            {
                var moList = await _dataService.GetMoList(classTeamCode);
                MoPicker.ItemsSource = moList.Count > 0 ? moList.Select(p => p).ToList() : new List<string>();

            }
            
        }

        private void OnMOSelected(object sender, EventArgs e)
        {
            SelectedMo = MoPicker.SelectedItem?.ToString() ?? string.Empty;
        }
    }
}
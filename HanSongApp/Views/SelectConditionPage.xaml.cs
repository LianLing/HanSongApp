
using HanSongApp.Models;
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
        public string? SelectedClassTeam { get; set; }
        public string? SelectedMo { get; set; }

        // 数据服务
        private readonly GetConditionService _dataService = new GetConditionService();

        public SelectConditionPage(IServiceProvider serviceProvider)
        {
            InitializeComponent();

            // 页面加载时初始化数据
            Loaded += async (sender, e) => await InitializeDataAsync();
            _serviceProvider = serviceProvider;
        }

        private async Task InitializeDataAsync()
        {
            try
            {
                // 加载机型数据
                List<Prod_TypeModel> prodtypes = await _dataService.OnProdTypeSelected();
                prodTypePicker.ItemsSource = prodtypes.Count > 0 ? prodtypes.Select(p => p.name).ToList() : new List<string>();

                // 加载线别数据
                List<LineModel> lines = await _dataService.GetLineList();
                LinePicker.ItemsSource = lines.Count > 0 ? lines.Select(l => l.name).ToList() : new List<string>();
                //加载班组数据
                List<TeamModel> classTeams = await _dataService.GetClassTeamList();
                ClassTeamPicker.ItemsSource = classTeams.Count > 0 ? classTeams.Select(t => t.name).ToList() : new List<string>();
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
                prod_type = SelectedProdType??"",
                prod_model = SelectedModel??"",
                prod_process_grp = SelectedModule??"",
                prod_process = SelectedProcess??"",
                Line = SelectedLine??"",
                ClassTeam = SelectedClassTeam??"",
                Mo = SelectedMo??""
            };

            // 通过依赖注入获取页面
            var singlepage = _serviceProvider.GetRequiredService<SingleRepositoryInPage>();
            await Navigation.PushAsync(singlepage);
            //await Navigation.PushAsync(new SingleRepositoryIn(filter));
        }

        private async void OnProdTypeSelected(object sender, EventArgs e)
        {
            var picker = (Picker)sender;
            //机型决定数据库
            SelectedProdType = picker.SelectedItem.ToString();

            // 根据选中的机型加载对应的工序、模组、制程、线别
            if (SelectedProdType != null)
            {
                //获取工序列表
                var models= await _dataService.GetModelList(SelectedProdType.ToString());
                ModelPicker.ItemsSource = models.Count > 0 ? models.Select(m => m.prod_model).ToList() : new List<string>();
                //获取模组列表
                ModulePicker.ItemsSource = models.Count > 0 ? models.Select(m => m.prod_module).ToList() : new List<string>();
                // 获取制程列表
                var processes = await _dataService.GetProcessList(SelectedProdType.ToString());
                ProcessPicker.ItemsSource = processes.Count > 0 ? processes.Select(p => p).ToList() : new List<string>();
                
            }
        }

        private void OnModelSelected(object sender, EventArgs e)
        {
            var picker = (Picker)sender;
            SelectedModel = picker.SelectedItem.ToString();
        }

        private void OnModuleTypeSelected(object sender, EventArgs e)
        {
            var picker = (Picker)sender;
            SelectedModule = picker.SelectedItem.ToString();
        }

        private void OnProcessSelected(object sender, EventArgs e)
        {
            var picker = (Picker)sender;
            SelectedProcess = picker.SelectedItem.ToString();
        }

        private void OnLineSelected(object sender, EventArgs e)
        {
            var picker = (Picker)sender;
            SelectedLine = picker.SelectedItem.ToString();
        }

        private async void OnClassTeamSelected(object sender, EventArgs e)
        {
            var picker = (Picker)sender;
            SelectedClassTeam = picker.SelectedItem.ToString();
            // 获取工单列表
            if (!string.IsNullOrEmpty(SelectedClassTeam))
            {
                var moList = await _dataService.GetMoList(SelectedClassTeam);
            }
            
        }

        private void OnMOSelected(object sender, EventArgs e)
        {
            var picker = (Picker)sender;
            SelectedMo = picker.SelectedItem.ToString();
        }
    }
}
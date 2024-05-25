using System;
using System.IO;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;

using GetWebResources.Core.Utils;
using GetWebResources.ViewModel;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Web.WebView2.Core;

using Serilog;

// WebView2 官方中文文档
// https://docs.microsoft.com/zh-cn/microsoft-edge/webview2/concepts/overview-features-apis?tabs=dotnetcsharp


// 与 [ObservableProperty] 特性有冲突, XAML 就先手动复制吧, 以后再看看.
// 通过静态资源加载VM,实例放在 ViewModelLocator 中(一个普通的类)
// https://www.cnblogs.com/taogeli/p/16046892.html
// APP:
//<local:ViewModelLocator x:Key="Locator"/>
// Main:
//DataContext="{Binding Source={StaticResource Locator}, Path=MainVM}"

namespace GetWebResources
{
    public partial class MainWindow : Window
    {

        private MainViewModel _vm { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = _vm = Ioc.Default.GetService<MainViewModel>();
            SaveResourcesUtils.InitHistoryList();

            ComboBoxHistory.ItemsSource = SaveResourcesUtils.HistoryList;

            ComboBoxHistory.SelectionChanged += (s, e) =>
            {
                _vm.WebView2Source = ComboBoxHistory.SelectedValue.ToString();
                SaveResourcesUtils.ClearResourcesList();
            };

            // 资源List 数量改变回调
            SaveResourcesUtils.OnResourcesListCountChanged += (num) =>
            {
                _vm.FileCount = num;
            };

            SaveResourcesUtils.OnHistoryListChanged += () =>
            {
                ComboBoxHistory.ItemsSource = SaveResourcesUtils.HistoryList;
            };

            // WebView2 core 初始化完成 (可以获取到 CoreWebView2 对象)
            Web.CoreWebView2InitializationCompleted += (s, e) => InitWebCoreEvent();

            _vm.WebReloadEvent += Web.Reload;

        }

        private void InitWebCoreEvent()
        {
            Web.CoreWebView2.SourceChanged += (s, e) =>
            {
                _vm.TextBoxWebUrl = Web.Source.ToString();
            };

            //监听资源的 请求与响应
            Web.CoreWebView2.WebResourceResponseReceived += (s, e) =>
            {
                Log.Information($"请求地址[{e.Request.Method}]: {e.Request.Uri}");

                // 只保存 get 的请求
                if (e.Request.Method == "GET")
                {
                    SaveResourcesUtils.PutUrlToResourcesList(e.Request.Uri);
                }

            };
            Web.CoreWebView2.DOMContentLoaded += (s, e) =>
            {
                Log.Information("dom 加载完毕");
            };
        }

    }
}
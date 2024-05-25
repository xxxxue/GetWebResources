using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using GetWebResources.Core.Interface;
using GetWebResources.Core.Utils;

using Serilog;

namespace GetWebResources.ViewModel;

public partial class MainViewModel : ObservableObject, ITransient
{

    //TODO:
    // 新用法有bug, 不能在xaml 里配置 local 和 DataContext,
    // 会报 重复属性之类 的错误,
    // 如果未来修复了. 就把 DataContext 设置 迁移到 XAML 里面,
    // 这样就可以有 上下文提示了. 现在是面向字符串编程.

    /// <summary>
    /// 资源数量
    /// </summary>
    [ObservableProperty]
    private int fileCount = 0;

    /// <summary>
    /// URL 文本框 内容
    /// </summary>
    [ObservableProperty]
    private string textBoxWebUrl;

    /// <summary>
    /// webview2 的 URL
    /// </summary>
    [ObservableProperty]
    private string webView2Source = " about:blank";

    /// <summary>
    /// 开始获取资源
    /// </summary>
    [RelayCommand]
    private void GetResources()
    {
        Task.Run(async () =>
        {
            try
            {
                Log.Information("正在获取资源,请稍等..");

                // 保存所有资源
                var path = await SaveResourcesUtils.SaveAllResourcesAsync();

                if (Directory.Exists(path))
                {
                    // 如果成功,则用 资源管理器 打开文件夹
                    SaveResourcesUtils.OpenFolderPath(path);
                }

                Log.Information("获取资源完成..");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "获取资源异常:");
                Log.Information("发生异常,请到Logs目录中查看详细信息");
                Log.Information("获取资源异常: " + ex);

                //打开log目录
                var basePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
                SaveResourcesUtils.OpenFolderPath(basePath);
            }
        });
    }

    /// <summary>
    /// 浏览器内核检测
    /// </summary>
    [RelayCommand]
    private void CheckCore()
    {
        WebView2Source = "https://ie.icoa.cn/";
    }

    /// <summary>
    /// 打开配置文件目录
    /// </summary>
    [RelayCommand]
    private void OpenConfig()
    {
        SaveResourcesUtils.OpenConfigPath();
    }


    [ObservableProperty]
    private bool openHostFilter = false;
    // 设置 openHostFilter 的 changed 触发事件
    partial void OnOpenHostFilterChanged(bool value)
    {
        SaveResourcesUtils.OpenHostFilterState = value;
    }

    public delegate void WebReloadDelegate();
    public event WebReloadDelegate WebReloadEvent;

    [RelayCommand]
    private void LoadUrl()
    {
        SaveResourcesUtils.ClearResourcesList();
        var url = TextBoxWebUrl;

        if (WebView2Source.ToString() == url)
        {
            WebReloadEvent?.Invoke();
        }
        else
        {
            WebView2Source = url;
            SaveResourcesUtils.PushToHistoryList(url);
        }
    }

}

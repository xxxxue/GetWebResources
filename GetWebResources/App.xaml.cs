using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;

using CommunityToolkit.Mvvm.DependencyInjection;

using GetWebResources.Core.Interface;

using Microsoft.Extensions.DependencyInjection;

using Serilog;

namespace GetWebResources;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        UseSerilog();
        UseIoc();
        UseGlobalException();

    }

    private void UseSerilog()
    {
        // 初始化 serilog
        Log.Logger = new LoggerConfiguration()
             .MinimumLevel.Debug()
             .WriteTo.File("Logs/app-log.txt", rollingInterval: RollingInterval.Day)
             .WriteTo.Console()
             .CreateLogger();
    }

    private void UseIoc()
    {
        var services = new ServiceCollection();

        var helloType = typeof(ITransient);

        List<Type> types = new List<Type>();

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            var name = assembly.FullName;
            if (name.StartsWith("System") || name.StartsWith("Microsoft"))
            {
                continue;
            }

            foreach (var type in assembly.GetTypes())
            {
                if (helloType.IsAssignableFrom(type))
                {
                    if (type.IsClass && !type.IsAbstract)
                    {
                        services.AddSingleton(type);
                    }
                }
            }
        }

        // 放入 toolkit 的 ioc 对象中
        Ioc.Default.ConfigureServices(services.BuildServiceProvider());
    }

    /// <summary>
    /// 捕获全局异常
    /// </summary>
    public void UseGlobalException()
    {

        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            var ex = e.ExceptionObject as Exception;
            Log.Error(ex, "App 发生异常:");
        };
        this.DispatcherUnhandledException += (s, e) =>
        {

            var ex = e.Exception;
            Log.Error(ex, "App_DispatcherUnhandledException 发生异常:");
            e.Handled = true;
        };
    }

}
using System.IO;
using System.Net.Mime;
using System.Reflection;
using System.Windows;
using ReTranslator.Utilities;

namespace ReTranslator;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // 注册退出时回收资源
        this.Exit += OnAppExit;
    }

    private void OnAppExit(object sender, ExitEventArgs e)
    {
        ChromeDriverSingleton.Instance.Quit();
        Textractor.Instance.Exit();
    }
}


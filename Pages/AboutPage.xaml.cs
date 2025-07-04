using System.Windows.Controls;
using System.Windows;
using System.Diagnostics;
using ReTranslator.Utilities;

namespace ReTranslator.Pages;

public partial class AboutPage : Page
{
    public AboutPage()
    {
        InitializeComponent();
        Version.Text = $"当前版本： {CheckUpdate.GetCurrentVersion()}";
    }

    private void BiliButton_Click(object sender, RoutedEventArgs e)
    {
        OpenUrl("https://space.bilibili.com/441114907");
    }
    private void QQButton_Click(object sender, RoutedEventArgs e)
    {
        // 跳转QQ群，替换成你的QQ群链接
        OpenUrl("https://jq.qq.com/?_wv=1027&k=832089871");
    }

    private void GithubButton_Click(object sender, RoutedEventArgs e)
    {
        // 跳转作者GitHub主页
        OpenUrl("https://github.com/RetCute");
    }
    private void OpenUrl(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch
        {
            MessageBox.Show("无法打开链接，请检查默认浏览器设置。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
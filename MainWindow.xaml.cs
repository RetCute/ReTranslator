
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ReTranslator.Pages;
using ReTranslator.Utilities;


namespace ReTranslator;

public partial class MainWindow : Window
{
    private MainPage mainPage;
    private SettingsPage settingsPage;
    private AboutPage aboutPage;
    
    public MainWindow()
    {
        InitializeComponent();
        mainPage = new MainPage();
        settingsPage = new SettingsPage();
        aboutPage  = new AboutPage();
        SwitchPage(mainPage);
        checkUpdate();
    }
    

    private async void checkUpdate()
    {
        try
        {
            Logger.Info("检查更新中...");
            bool updateAvailable = await CheckUpdate.IsUpdateAvailableAsync();
            if (updateAvailable)
            {
                MessageBox.Show("发现新版本!", "更新提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                Logger.Info("已经是最新版本");
            }
        }
        catch (Exception ex)
        {
            Logger.Error("检查更新失败:" + ex);
        }
    }
    
    private void Window_Drag(object sender, MouseButtonEventArgs e)
    {
        // 排除交互控件
        if (e.Source is TextBox || e.Source is Button || e.Source is ComboBox) 
            return;
        
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }
    
    private void CloseApp(object sender, RoutedEventArgs e)
    {
        MessageBoxResult result = MessageBox.Show(
            "确定要退出吗？", 
            "确认退出", 
            MessageBoxButton.YesNo, 
            MessageBoxImage.Question);
        if (result == MessageBoxResult.Yes)
        {
            Application.Current.Shutdown();
        }
    }

    private void showMainPage(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        var tag = button.Tag as string;
        Title.Text = "Retranslator" + tag;
        SwitchPage(mainPage);
    }
    
    private void showSettingsPage(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        var tag = button.Tag as string;
        Title.Text = "Retranslator" + tag;
        SwitchPage(settingsPage);
    }

    private void showAboutPage(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        var tag = button.Tag as string;
        Title.Text = "Retranslator" + tag;
        SwitchPage(aboutPage);
    }
    
    private void SwitchPage(Page content)
    {
        ContentFrame.Navigate(content);
    }

    private void Minimize(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }
}
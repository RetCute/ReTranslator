using System.Windows;
using System.Windows.Controls;
using ReTranslator.Utilities;
using Microsoft.Win32;

namespace ReTranslator.Pages;

public partial class SettingsPage : Page
{
    public SettingsPage()
    {
        InitializeComponent();
        var configManager = ConfigManager.Instance;
        setValues(configManager.Config);
    }

    private void setValues(ConfigManager.AppConfig config)
    {
        FontSize.Text = config.Size.ToString();
        TextARGB.Text = config.TextARGB;
        captrueHotkey.Text = config.CaptureHotkey;
        pauseHotkey.Text = config.PauseHotkey;
        Text_Extraction_Mode.SelectedIndex = config.TextExtractionMode;
        Textractor_Path.Text = config.TextractorPath;
        Translation_Method.SelectedIndex = config.TranslationMethod;
        Driver_Path.Text = config.Translators[0].GetParameter<string>("driver_path");
        Chrome_Path.Text =  config.Translators[0].GetParameter<string>("chrome_path");
        GPTKEY.Text = config.Translators[1].GetParameter<string>("api_key");
        GPTBase.Text = config.Translators[1].GetParameter<string>("api_base");
        GPTModel.Text = config.Translators[1].GetParameter<string>("model");
    }

    private void Selection_Changed(object sender, SelectionChangedEventArgs e)
    {
        var comboBox = sender as ComboBox;
        var index = comboBox.SelectedIndex;
        StackedViews.SelectedIndex = index;
    }
    
    private void Browse(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        var textBox = button?.Tag as TextBox;
        OpenFileDialog openFileDialog = new OpenFileDialog();
        openFileDialog.Filter = "可执行文件|*.exe|所有文件|*.*";
        bool? result = openFileDialog.ShowDialog();
        if (result == true)
        {
            textBox.Text = openFileDialog.FileName;
        }
    }

    private void SaveConfig(object sender, RoutedEventArgs e)
    {
        var configManager = ConfigManager.Instance;
        var config = configManager.Config;
        config.Size = int.Parse(FontSize.Text);
        config.TextARGB = TextARGB.Text;
        config.CaptureHotkey = captrueHotkey.Text;
        config.PauseHotkey = pauseHotkey.Text;
        config.TextExtractionMode = Text_Extraction_Mode.SelectedIndex;
        config.TextractorPath = Textractor_Path.Text;
        config.TranslationMethod = Translation_Method.SelectedIndex;
        config.Translators[0].SetParameter("driver_path", Driver_Path.Text);
        config.Translators[0].SetParameter("chrome_path", Chrome_Path.Text);
        config.Translators[1].SetParameter("api_key", GPTKEY.Text);
        config.Translators[1].SetParameter("api_base", GPTBase.Text);
        config.Translators[1].SetParameter("model", GPTModel.Text);
        configManager.SaveConfig(config);
        MessageBox.Show("保存成功", "保存成功", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
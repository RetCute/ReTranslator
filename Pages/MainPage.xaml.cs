using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Controls;
using ReTranslator.Utilities;
using System.Windows.Documents;
using System.Windows;
using ReTranslator.Translators;
using MessageBox = System.Windows.Forms.MessageBox;


namespace ReTranslator.Pages;

public partial class MainPage : Page
{
    private int pid = 0;
    private string processname = "";
    bool Running =  false;
    private Rect rect = Rect.Empty;
    private TranslatorBase translator;
    public MainPage()
    {
        InitializeComponent();
        LogBox.Document = new FlowDocument();
        System.Windows.Forms.Application.EnableVisualStyles();
        
        // 订阅日志事件
        Logger.LogAdded += (message, brush) => 
        {
            Dispatcher.Invoke(() => 
            {
                var paragraph = new Paragraph();
                var run = new Run(message)
                {
                    Foreground = brush
                };
                paragraph.Inlines.Add(run);
                LogBox.Document.Blocks.Add(paragraph);
                LogBox.ScrollToEnd();
            });
        };
        ConfigManager.Instance.ConfigChanged += OnConfigChanged;
        Initialize();
    }

    private void Run(object sender, RoutedEventArgs e)
    {
        var config = ConfigManager.Instance.Config;
        bool Tready = OCR.Instance.IsReady || Textractor.Instance.Running;
        if (Tready && pid != 0 && translator.Running && !Running)
        {
            runButton.IsEnabled = false;
            if (config.TextExtractionMode == 0)
            {
                Keys capturekey = (Keys)Enum.Parse(typeof(Keys), config.CaptureHotkey, ignoreCase: true);
                Keys pausekey = (Keys)Enum.Parse(typeof(Keys), config.PauseHotkey, ignoreCase: true);
                var keyboard = new Keyboard(capturekey, pausekey);
                var subtitle = new SubtitleWindow(pid);
                subtitle.Show();
                Window.GetWindow(this).WindowState = WindowState.Minimized;
                keyboard.OnHotkey1Pressed += () =>
                {
                    var path = Capturer.CaptureScreenshot("temp.png", rect);
                    using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        Bitmap bmp = new Bitmap(fs);
                        var text = OCR.Instance.ReadText(bmp);
                        var reply = translator.Translate(text);
                        subtitle.UpdateSubtitle(reply);
                    }
                };
                keyboard.OnHotkey2Pressed += () =>
                {
                    Logger.Info("已中止运行!");
                    runButton.IsEnabled = true;
                    subtitle.Close();
                    Running = false;
                    keyboard.Dispose();
                };
            }
            else
            {
                Keys pausekey = (Keys)Enum.Parse(typeof(Keys), config.PauseHotkey, ignoreCase: true);
                var keyboard = new Keyboard(pausekey, pausekey);
                var subtitle = new SubtitleWindow(pid);
                subtitle.Show();
                hookPanel.setTranslatorAndSubtitle(translator, subtitle);
                Textractor.Instance.setPanelAndPid(hookPanel, pid);
                Textractor.Instance.Run();
                Window.GetWindow(this).WindowState = WindowState.Minimized;
                keyboard.OnHotkey1Pressed += () =>
                {
                    runButton.IsEnabled = true;
                    subtitle.Close();
                    Textractor.Instance.Exit();
                    Running = false;
                    keyboard.Dispose();
                };
            }
        }
        else
        {
            Logger.Error("启动失败");
            Logger.Error("请检查OCR/Textractor和翻译器是否初始化成功");
            Logger.Error("请检查是否选定了截图区域与进程");
        }
    }

    private void Initialize()
    {
        var config = ConfigManager.Instance.Config;
        if (config.TextExtractionMode == 0)
        {
            captureAreaButton.IsEnabled = true;
            OCR.Instance.Init();
        }
        else
        {
            captureAreaButton.IsEnabled = false;
            Textractor.Instance.Init(config.TextractorPath);
        }
        initTranslator();
    }
    private void OnConfigChanged(object sender, EventArgs e)
    {
        Initialize();
    }

    private void initTranslator()
    {
        if (translator != null)
        {
            translator.Quit();
        }
        var config = ConfigManager.Instance.Config;
        if (config.TranslationMethod == 0)
        {
            translator = new GPTWeb();
            translator.Init(config.Translators[config.TranslationMethod]);
        }
        else
        {
            translator = new GPTAPI();
            translator.Init(config.Translators[config.TranslationMethod]);
        }
    }

    private void getCaptureArea(object sender, RoutedEventArgs e)
    {
        Window.GetWindow(this).WindowState = WindowState.Minimized;
        string path = Capturer.CaptureScreenshot();
        path = Capturer.DimImage(path, 0.5f);
        rect = AreaSelector.SelectArea(path);
        Console.WriteLine(rect.IsEmpty);
        if (!rect.IsEmpty)
        {
            Logger.Info($"已选中: {rect}");
        }
        else
        {
            Logger.Warning($"你没有选取截图区域!");
        }

        try
        {
            File.Delete(path);
            Logger.Success("删除缓存图片成功");
        }
        catch (Exception ex)
        {
            Logger.Error("删除缓存图片失败:" + ex);
        }
    }
    
    private void selectProcess(object sender, RoutedEventArgs e)
    {
        // 获取进程列表
        var processes = ProcessUtils.GetAllProcesses();
    
        // 显示对话框
        var dialog = new AttachProcessDialog(processes);
        if (dialog.ShowDialog() == true)
        {
            // 获取选择的进程信息
            string processName = dialog.SelectedProcessName;
            int processId = dialog.SelectedProcessId;
        
            // 使用进程信息...
            Logger.Info($"已选择进程: {processName} PID: {processId}");
            pid = processId;
            processname =  processName;
        }
        else
        {
            Logger.Warning("你没有选择任何进程!");
        }
    }
}
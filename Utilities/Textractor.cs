using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ReTranslator.Translators;
using System.Windows;
using System.Windows.Threading;
using ReTranslator.Utilities;

namespace ReTranslator.Utilities;

public class Textractor
{
    private static readonly Lazy<Textractor> _lazyInstance = new(() => new Textractor());

    public static Textractor Instance => _lazyInstance.Value;

    private Process Process;
    private int pid;
    public bool Running = false;
    private string TextractorPath;
    private HookCodePanel HookPanel;

    private Textractor() {}

    public void Init(string textractorPath)
    {
        Logger.Info("初始化Textractor...");
        if (File.Exists(textractorPath))
        {
            TextractorPath = textractorPath;
            Running = true;
            Logger.Success("Textractor初始化成功!");
        }
        else
        {
            Logger.Error("初始化Textractor失败,文件不存在");
        }
    }

    public void setPanelAndPid(HookCodePanel hookPanel, int Pid)
    {
        HookPanel = hookPanel;
        pid = Pid;
    }
    public void createProcess()
    {
        Process = new Process();
        Process.StartInfo.FileName = TextractorPath;
        Process.StartInfo.UseShellExecute = false;
        Process.StartInfo.CreateNoWindow = true;
        Process.StartInfo.RedirectStandardInput = true;
        Process.StartInfo.RedirectStandardOutput = true;
        Process.StartInfo.RedirectStandardError = true;
        Process.StartInfo.StandardInputEncoding = new UnicodeEncoding(false, false);
        Process.StartInfo.StandardOutputEncoding = new UnicodeEncoding(false, false);
        Process.OutputDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    HookPanel.ProcessText(e.Data.Trim());
                });
            }
        };
        Process.ErrorDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
                Logger.Error(e.Data);
        };
        Process.Start();
        Process.BeginOutputReadLine();
        Process.BeginErrorReadLine();
    }

    private void Monitor()
    {
        while (Running)
        {
            if (!ProcessExists(pid))
            {
                Exit();
            }
        }
    }

    public void Exit()
    {
        try
        {
            Detach();
            Process.Kill();
            Running = false;
            Logger.Info("已退出!");
        }
        catch (Exception e)
        {
            Logger.Error(e.ToString());
        }
    }
    
    private void Attach()
    {
        Process.StandardInput.WriteLine($"attach -P {pid}");
        Process.StandardInput.Flush();
    }

    private void Detach()
    {
        Process.StandardInput.WriteLine($"detach -P {pid}");
        Process.StandardInput.Flush();
    }
    
    public void Run()
    {
        if (Process == null || Process.HasExited)
        {
            createProcess();
            Console.Write("创建成功");
        }
        Attach();
        Task.Run(() => Monitor());
    }
    
    private bool ProcessExists(int pid)
    {
        try
        {
            Process.GetProcessById(pid);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
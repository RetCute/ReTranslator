using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interop;

namespace ReTranslator.Utilities
{
    public static class ProcessUtils
    {
        #region Win32 API 声明
        private const int max_path = 260;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWindowVisible(IntPtr hWnd);
        
        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);
        
        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
        
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);
        
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleFileName(IntPtr hModule, [Out] System.Text.StringBuilder lpFilename, int nSize);
        
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);
        
        [DllImport("psapi.dll", SetLastError = true)]
        private static extern uint GetModuleFileNameEx(IntPtr hProcess, IntPtr hModule, [Out] System.Text.StringBuilder lpFilename, int nSize);
        
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);

        #endregion

        #region 公共方法

        /// <summary>
        /// 获取所有非系统进程
        /// </summary>
        public static Dictionary<string, int> GetAllProcesses()
        {
            var processesMap = new Dictionary<string, int>();
            var seenNames = new HashSet<string>();
            const int maxPath = 260;

            // 遍历所有进程
            foreach (var process in Process.GetProcesses())
            {
                try
                {
                    if (process.Id == 0) continue; // 排除系统进程

                    string fileName = GetProcessFileName(process);

                    // 排除 Windows 目录下的进程
                    if (!string.IsNullOrEmpty(fileName) && !fileName.Contains("\\Windows\\"))
                    {
                        // 获取文件名
                        string processName = System.IO.Path.GetFileName(fileName);
                        
                        if (!seenNames.Contains(processName))
                        {
                            seenNames.Add(processName);
                            processesMap[processName] = process.Id;
                        }
                    }
                }
                catch
                {
                    // 忽略所有错误
                }
            }
            return processesMap;
        }

        /// <summary>
        /// 获取进程窗口句柄
        /// </summary>
        public static IntPtr GetWindowHandle(int pid)
        {
            IntPtr result = IntPtr.Zero;
            
            EnumWindows(delegate(IntPtr hwnd, IntPtr lParam)
            {
                GetWindowThreadProcessId(hwnd, out uint windowPid);
                if (windowPid == pid && IsWindowVisible(hwnd))
                {
                    result = hwnd;
                    return false; // 停止枚举
                }
                return true; // 继续枚举
            }, IntPtr.Zero);
            
            return result;
        }

        #endregion

        #region 私有方法

        private static string GetProcessFileName(Process process)
        {
            // 直接尝试从 MainModule 获取
            try
            {
                return process.MainModule?.FileName;
            }
            catch
            {
                // 使用 API 方式获取
                IntPtr hProcess = OpenProcess(0x0010 | 0x0400, false, (uint)process.Id);
                if (hProcess == IntPtr.Zero)
                    return null;

                try
                {
                    var sb = new System.Text.StringBuilder(max_path);
                    if (GetModuleFileNameEx(hProcess, IntPtr.Zero, sb, max_path) > 0)
                    {
                        return sb.ToString();
                    }
                }
                finally
                {
                    CloseHandle(hProcess);
                }
            }
            return null;
        }

        #endregion
    }

    /// <summary>
    /// 进程选择对话框
    /// </summary>
    public partial class AttachProcessDialog : Window
    {
        #region 私有字段

        private Dictionary<string, int> _processesMap;
        private Label _label;
        private ListBox _processList;
        private TextBox _processEdit;
        private Button _okButton;
        private Button _cancelButton;
        private ICollectionView _collectionView; // 新增：集合视图用于过滤
        private ObservableCollection<string> _processNames; // 新增：可观察集合

        #endregion

        #region 公共属性

        /// <summary>
        /// 选择的进程名称
        /// </summary>
        public string SelectedProcessName { get; private set; }
        
        /// <summary>
        /// 选择的进程ID
        /// </summary>
        public int SelectedProcessId { get; private set; }

        #endregion

        #region 构造函数

        /// <summary>
        /// 创建进程选择对话框
        /// </summary>
        /// <param name="processesMap">进程名称到ID的映射</param>
        public AttachProcessDialog(Dictionary<string, int> processesMap)
        {
            _processesMap = processesMap;
            InitializeComponents();
            PopulateProcessList();
        }

        #endregion

        #region 初始化方法

        private void InitializeComponents()
        {
            // 窗口设置
            WindowStyle = WindowStyle.SingleBorderWindow;
            SizeToContent = SizeToContent.WidthAndHeight;
            Topmost = true;
            Title = "SELECT_PROCESS";
            ResizeMode = ResizeMode.NoResize;
            
            // 创建主容器
            var grid = new Grid();
            grid.Margin = new Thickness(15);
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

            // 标签
            _label = new Label
            {
                Content = "选择进程",
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 10)
            };
            Grid.SetRow(_label, 0);
            grid.Children.Add(_label);

            // 进程列表
            _processList = new ListBox
            {
                MinWidth = 350,
                MinHeight = 200,
                MaxHeight = 300,
                Margin = new Thickness(0, 0, 0, 10)
            };
            Grid.SetRow(_processList, 1);
            grid.Children.Add(_processList);

            // 进程搜索框
            _processEdit = new TextBox
            {
                MinWidth = 350,
                Margin = new Thickness(0, 0, 0, 10),
                ToolTip = "输入进程名称进行筛选"
            };
            Grid.SetRow(_processEdit, 2);
            grid.Children.Add(_processEdit);

            // 按钮容器
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 10, 0, 0)
            };
            Grid.SetRow(buttonPanel, 4);
            grid.Children.Add(buttonPanel);

            // 确定按钮
            _okButton = new Button
            {
                Content = "OK",
                MinWidth = 80,
                Margin = new Thickness(0, 0, 10, 0),
                IsDefault = true
            };
            buttonPanel.Children.Add(_okButton);

            // 取消按钮
            _cancelButton = new Button
            {
                Content = "Cancel",
                MinWidth = 80,
                IsCancel = true
            };
            buttonPanel.Children.Add(_cancelButton);

            // 设置窗口内容
            Content = grid;

            // 初始化集合
            _processNames = new ObservableCollection<string>();
            _collectionView = CollectionViewSource.GetDefaultView(_processNames);
            _processList.ItemsSource = _collectionView;

            // 注册事件
            _okButton.Click += (s, e) => OnOkClicked();
            _cancelButton.Click += (s, e) => Close();
            _processList.SelectionChanged += (s, e) => OnProcessSelected();
            _processList.MouseDoubleClick += (s, e) => OnOkClicked();
            _processEdit.TextChanged += (s, e) => OnProcessEdited();
        }

        private void PopulateProcessList()
        {
            _processNames.Clear();
            foreach (var processName in _processesMap.Keys.OrderBy(k => k))
            {
                _processNames.Add(processName);
            }
        }

        #endregion

        #region 事件处理方法

        private void OnOkClicked()
        {
            var processName = _processEdit.Text.Trim();
            if (!string.IsNullOrEmpty(processName) 
                && _processesMap.ContainsKey(processName))
            {
                SelectedProcessName = processName;
                SelectedProcessId = _processesMap[processName];
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show(this, 
                    "请选择一个进程或者输入有效的进程名称！", 
                    "Warning", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Warning);
            }
        }

        private void OnProcessSelected()
        {
            if (_processList.SelectedItem != null)
            {
                _processEdit.Text = _processList.SelectedItem.ToString();
            }
        }

        // 修改后的过滤方法
        private void OnProcessEdited()
        {
            var filter = _processEdit.Text.Trim().ToLower();
            _collectionView.Filter = item =>
            {
                if (string.IsNullOrWhiteSpace(filter))
                    return true; // 空搜索显示全部
                
                return (item as string)?.ToLower().Contains(filter) == true;
            };
        }

        #endregion
    }
}
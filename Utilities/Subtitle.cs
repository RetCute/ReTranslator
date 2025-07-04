using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace ReTranslator.Utilities
{
    public class SubtitleWindow : Window
    {
        #region Win32 API 声明

        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

        #endregion

        #region 私有字段

        private DispatcherTimer _autoHideTimer;
        private DispatcherTimer _monitorTimer;
        private TextBlock _subtitleTextBlock;
        private CheckBox _keepBorderCheckbox;
        private bool _isDragging;
        private Point _dragStartPosition;
        private bool _controlsVisible;
        private IntPtr _targetHwnd;

        #endregion

        #region 构造函数

        public SubtitleWindow(int? pid = null)
        {
            // 窗口设置
            InitializeWindowSettings();

            // 创建UI元素
            InitializeUI();

            // 启动定时器
            SetupTimers();

            // 如果需要监控进程
            if (pid.HasValue)
            {
                InitializeProcessMonitoring(pid.Value);
            }
        }

        #endregion

        #region 初始化方法

        private void InitializeWindowSettings()
        {
            // 窗口样式设置
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Background = Brushes.Transparent;
            Topmost = true;
            ShowInTaskbar = false;

            // 初始大小和位置
            Width = 700;
            Height = 150;
            Left = (SystemParameters.PrimaryScreenWidth - Width) / 2;
            Top = (SystemParameters.PrimaryScreenHeight - Height) / 2;

            // 创建主容器
            var grid = new Grid();
            Content = grid;
        }

        private void InitializeUI()
        {
            var grid = (Grid)Content;
            var config = ConfigManager.Instance.Config;
            // 字幕标签 - 严格对应 PyQt 中的 QLabel

            
            _subtitleTextBlock = new TextBlock
            {
                Text = "文本字幕", // 默认文本
                FontSize = config.Size,
                FontFamily = new FontFamily("Arial"),
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap, // 启用文本换行
                Foreground = new SolidColorBrush(ParseArgb(config.TextARGB)),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
            };


            _subtitleTextBlock.Width = 700;
            _subtitleTextBlock.Height = 150;

            grid.Children.Add(_subtitleTextBlock);

            // 保持边框复选框 - 严格对应 PyQt 中的 QCheckBox
            _keepBorderCheckbox = new CheckBox
            {
                Content = "保持边框状态",
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(580, 125, 0, 0), // 对应 PyQt 中的位置
                Foreground = Brushes.White,
                FontFamily = new FontFamily("Arial"),
                FontSize = 15,
                FontWeight = FontWeights.Bold,
                Visibility = Visibility.Hidden
            };

            // 事件绑定
            _keepBorderCheckbox.Checked += (s, e) => KeepBorderStateChanged();
            _keepBorderCheckbox.Unchecked += (s, e) => KeepBorderStateChanged();

            grid.Children.Add(_keepBorderCheckbox);
        }

        private void SetupTimers()
        {
            // 自动隐藏定时器 - 对应 PyQt 中的 autoHideTimer
            _autoHideTimer = new DispatcherTimer();
            _autoHideTimer.Interval = TimeSpan.FromSeconds(2); // 2000ms
            _autoHideTimer.Tick += (s, e) => AutoHideControls();
        }

        private void InitializeProcessMonitoring(int pid)
        {
            // 进程监控定时器 - 对应 PyQt 中的 timer
            _monitorTimer = new DispatcherTimer();
            _monitorTimer.Interval = TimeSpan.FromSeconds(1); // 1000ms
            _monitorTimer.Tick += (s, e) => CheckProcessWindow(pid);
            _monitorTimer.Start();
        }

        #endregion

        #region 功能方法 - 严格对应 PyQt 方法

        public void UpdateSubtitle(string text)
        {
            // 对应 PyQt 中的 updateSubtitle 方法
            _subtitleTextBlock.Text = text;
        }

        public void Exit()
        {
            // 对应 PyQt 中的 exit 方法
            _monitorTimer?.Stop();
            _autoHideTimer?.Stop();
            Close();
        }

        private void KeepBorderStateChanged()
        {
            // 对应 PyQt 中的 keepBorderStateChanged 方法
            if (!_keepBorderCheckbox.IsChecked ?? false)
            {
                ToggleControls(true);
            }
        }

        private void ToggleControls(bool forceHide = false)
        {
            // 对应 PyQt 中的 toggleControls 方法
            if (_keepBorderCheckbox.IsChecked ?? false && !forceHide)
            {
                _controlsVisible = true;
                _subtitleTextBlock.Background = new SolidColorBrush(Color.FromArgb(128, 0, 0, 0));
            }
            else
            {
                _controlsVisible = !_controlsVisible;

                if (_controlsVisible)
                {
                    _subtitleTextBlock.Background = new SolidColorBrush(Color.FromArgb(128, 0, 0, 0));
                    _keepBorderCheckbox.Visibility = Visibility.Visible;
                    _autoHideTimer.Start();
                }
                else
                {
                    _subtitleTextBlock.Background = Brushes.Transparent;
                    _keepBorderCheckbox.Visibility = Visibility.Hidden;
                    _autoHideTimer.Stop();
                }
            }
        }

        private void AutoHideControls()
        {
            // 对应 PyQt 中的 autoHideControls 方法
            if (_controlsVisible)
            {
                ToggleControls();
            }
        }

        #endregion

        #region 窗口事件处理 - 严格对应 PyQt 事件

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            // 对应 PyQt 中的 mousePressEvent
            base.OnMouseLeftButtonDown(e);

            if (e.ChangedButton == MouseButton.Left)
            {
                _isDragging = true;
                _dragStartPosition = e.GetPosition(this);
                CaptureMouse();
                Cursor = Cursors.Hand;

                if (!_controlsVisible)
                {
                    ToggleControls();
                }
                else
                {
                    _autoHideTimer.Start();
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            // 对应 PyQt 中的 mouseMoveEvent
            base.OnMouseMove(e);

            if (_isDragging && e.LeftButton == MouseButtonState.Pressed)
            {
                var currentPosition = PointToScreen(e.GetPosition(this));
                var newPosition = new Point(
                    currentPosition.X - _dragStartPosition.X,
                    currentPosition.Y - _dragStartPosition.Y);

                Left = newPosition.X;
                Top = newPosition.Y;
            }
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            // 对应 PyQt 中的 mouseReleaseEvent
            base.OnMouseLeftButtonUp(e);

            if (_isDragging)
            {
                _isDragging = false;
                ReleaseMouseCapture();
                Cursor = Cursors.Arrow;
            }
        }

        #endregion

        #region 进程监控方法

        private void CheckProcessWindow(int pid)
        {
            try
            {
                var process = Process.GetProcessById(pid);
                if (process == null) return;

                var mainHwnd = process.MainWindowHandle;
                if (mainHwnd == IntPtr.Zero)
                {
                    // 没有主窗口，隐藏字幕
                    Visibility = Visibility.Hidden;
                    return;
                }

                bool isMinimized = IsIconic(mainHwnd);
                if (isMinimized)
                {
                    Visibility = Visibility.Hidden;
                }
                else
                {
                    Visibility = Visibility.Visible;
                }
            }
            catch
            {
                Logger.Error("目标进程已结束");
                _monitorTimer?.Stop();
                Close();
            }
        }
        
        public static Color ParseArgb(string input)
        {
            // 移除空格和括号
            string cleanInput = input.Replace("(", "").Replace(")", "");
            
            // 分割字符串
            var parts = cleanInput.Split(',');
            
            if (parts.Length != 4)
                throw new FormatException("无效的颜色格式");
            
            // 分别解析A,R,G,B分量
            byte a = ParseByte(parts[0]);
            byte r = ParseByte(parts[1]);
            byte g = ParseByte(parts[2]);
            byte b = ParseByte(parts[3]);
            
            // 创建颜色
            return Color.FromArgb(a, r, g, b);
        }
        
        private static byte ParseByte(string input)
        {
            if (byte.TryParse(input.Trim(), out byte result))
            {
                return result;
            }
            throw new FormatException($"无效的数字值: {input}");
        }

        #endregion
    }
}
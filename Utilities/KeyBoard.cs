using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Interop;

namespace ReTranslator.Utilities
{
    public class Keyboard : IDisposable
    {
        private readonly IntPtr _windowHandle;
        private readonly int _hotkeyId1 = 1;
        private readonly int _hotkeyId2 = 2;
        private readonly Keys _key1;
        private readonly Keys _key2;
        private HwndSource _source;

        public event Action OnHotkey1Pressed;
        public event Action OnHotkey2Pressed;

        public Keyboard(Keys hotkey1, Keys hotkey2)
        {
            _key1 = hotkey1;
            _key2 = hotkey2;

            var mainWindow = System.Windows.Application.Current.MainWindow;
            _windowHandle = new WindowInteropHelper(mainWindow).Handle;
            _source = HwndSource.FromHwnd(_windowHandle);
            _source.AddHook(HwndHook);

            RegisterHotKey(_windowHandle, _hotkeyId1, 0, (int)_key1);
            RegisterHotKey(_windowHandle, _hotkeyId2, 0, (int)_key2);
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;

            if (msg == WM_HOTKEY)
            {
                int id = wParam.ToInt32();
                if (id == _hotkeyId1)
                {
                    OnHotkey1Pressed?.Invoke();
                    handled = true;
                }
                else if (id == _hotkeyId2)
                {
                    OnHotkey2Pressed?.Invoke();
                    handled = true;
                }
            }

            return IntPtr.Zero;
        }

        public void Dispose()
        {
            UnregisterHotKey(_windowHandle, _hotkeyId1);
            UnregisterHotKey(_windowHandle, _hotkeyId2);
            _source?.RemoveHook(HwndHook);
        }

        // P/Invoke
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, int vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
    }
}

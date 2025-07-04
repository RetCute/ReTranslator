using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;

namespace ReTranslator.Utilities
{
    public static class Capturer
    {
        // 截图（默认全屏，也可传入 Rect）
        public static string CaptureScreenshot(string savePath = "temp.png", Rect? region = null)
        {
            int x, y, width, height;

            if (region.HasValue)
            {
                x = (int)region.Value.X;
                y = (int)region.Value.Y;
                width = (int)region.Value.Width;
                height = (int)region.Value.Height;
            }
            else
            {
                x = 0;
                y = 0;
                width = Screen.PrimaryScreen.Bounds.Width;
                height = Screen.PrimaryScreen.Bounds.Height;
            }

            using (Bitmap bmp = new Bitmap(width, height))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.CopyFromScreen(x, y, 0, 0, new System.Drawing.Size(width, height));
                }

                bmp.Save(savePath, ImageFormat.Png);
            }

            return Path.GetFullPath(savePath);
        }

        // 给图片变暗处理
        public static string DimImage(string path, float dimFactor = 0.5f)
        {
            using (Bitmap bmp = new Bitmap(path))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    using (Brush dimBrush = new SolidBrush(Color.FromArgb((int)((1 - dimFactor) * 255), Color.Black)))
                    {
                        g.FillRectangle(dimBrush, 0, 0, bmp.Width, bmp.Height);
                    }
                }

                path = path.Replace(".png", "_dimmed.png");
                bmp.Save(path, ImageFormat.Png);
                return Path.GetFullPath(path);
            }
        }
    }
}
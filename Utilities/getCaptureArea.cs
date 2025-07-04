using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.IO;

namespace ReTranslator.Utilities
{
    public class AreaSelector : Window
    {
        private Point? startPoint;
        private Rectangle selectionRectangle;
        private Canvas canvas;
        private Image backgroundImage;
        private Rect selectedArea = Rect.Empty;

        private AreaSelector(string imagePath)
        {
            // Window properties
            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;
            Topmost = true;
            AllowsTransparency = true;
            Background = Brushes.Transparent;
            WindowState = WindowState.Maximized;

            // Create Canvas
            canvas = new Canvas();
            Content = canvas;

            // Load background image
            BitmapImage bitmap = new BitmapImage();
            using (FileStream stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad; // 一次性加载进内存
                bitmap.StreamSource = stream;
                bitmap.EndInit();
                bitmap.Freeze();
            }

            backgroundImage = new Image { Source = bitmap };
            canvas.Children.Add(backgroundImage);

            // Rectangle for selection
            selectionRectangle = new Rectangle
            {
                Stroke = Brushes.Red,
                StrokeThickness = 2,
                Visibility = Visibility.Collapsed
            };
            canvas.Children.Add(selectionRectangle);

            // Mouse events
            MouseLeftButtonDown += OnMouseDown;
            MouseMove += OnMouseMove;
            MouseLeftButtonUp += OnMouseUp;
        }

        public static Rect SelectArea(string imagePath)
        {
            AreaSelector selector = new AreaSelector(imagePath);
            selector.ShowDialog();
            return selector.selectedArea;
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            startPoint = e.GetPosition(canvas);
            Canvas.SetLeft(selectionRectangle, startPoint.Value.X);
            Canvas.SetTop(selectionRectangle, startPoint.Value.Y);
            selectionRectangle.Width = 0;
            selectionRectangle.Height = 0;
            selectionRectangle.Visibility = Visibility.Visible;
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (startPoint.HasValue && e.LeftButton == MouseButtonState.Pressed)
            {
                Point currentPoint = e.GetPosition(canvas);

                double x = Math.Min(currentPoint.X, startPoint.Value.X);
                double y = Math.Min(currentPoint.Y, startPoint.Value.Y);
                double width = Math.Abs(currentPoint.X - startPoint.Value.X);
                double height = Math.Abs(currentPoint.Y - startPoint.Value.Y);

                Canvas.SetLeft(selectionRectangle, x);
                Canvas.SetTop(selectionRectangle, y);
                selectionRectangle.Width = width;
                selectionRectangle.Height = height;
            }
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (startPoint.HasValue)
            {
                Point endPoint = e.GetPosition(canvas);

                double x = Math.Min(startPoint.Value.X, endPoint.X);
                double y = Math.Min(startPoint.Value.Y, endPoint.Y);
                double width = Math.Abs(startPoint.Value.X - endPoint.X);
                double height = Math.Abs(startPoint.Value.Y - endPoint.Y);

                selectedArea = new Rect(x, y, width, height);

                Close();
            }
        }
    }
}

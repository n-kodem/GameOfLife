using GameOfLife.ViewModels;
using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GameOfLife
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                if (DataContext is MainViewModel vm)
                {
                    vm.RequestExportImage += OnRequestExportImage;
                }
            };
        }

        private void OnRequestExportImage()
        {
            // Find the GridCanvas in the visual tree or just use its name if we add x:Name
            // For simplicity, let's assume we gave it x:Name="MainCanvas" in XAML
            var canvas = FindName("MainCanvas") as FrameworkElement;
            if (canvas == null) return;

            var sfd = new SaveFileDialog { Filter = "PNG Image (*.png)|*.png" };
            if (sfd.ShowDialog() == true)
            {
                var rect = new Rect(canvas.RenderSize);
                var rtb = new RenderTargetBitmap((int)rect.Width, (int)rect.Height, 96, 96, PixelFormats.Pbgra32);
                rtb.Render(canvas);

                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(rtb));
                using (var stream = File.Create(sfd.FileName))
                {
                    encoder.Save(stream);
                }
            }
        }
    }
}

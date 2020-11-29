using ColorPickerWPF;
using Microsoft.Win32;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace AnimatedTokenMaker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private string _border;
        private bool _dragging;
        private string _file;
        private int _offSetX;
        private int _offSetY;
        private float _scale = 1;

        private TokenMaker _tokenMaker;

        public MainWindow()
        {
            _tokenMaker = new TokenMaker(new VideoExporter());

            SetBorder(GetBorders()[0]);

            InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public int OffsetX
        {
            get
            {
                return _offSetX;
            }
            set
            {
                _offSetX = value;
                Changed(nameof(OffsetX));
                RefreshImage();
            }
        }

        public int OffsetY
        {
            get
            {
                return _offSetY;
            }
            set
            {
                _offSetY = value;
                Changed(nameof(OffsetY));
                RefreshImage();
            }
        }

        public System.Windows.Media.ImageSource Preview1 => GetPreviewImage(Preview1Frame);

        public int Preview1Frame { get; set; } = -1;

        public System.Windows.Media.ImageSource Preview2 => GetPreviewImage(Preview2Frame);

        public int Preview2Frame { get; set; } = -1;

        public System.Windows.Media.ImageSource Preview3 => GetPreviewImage(Preview3Frame);

        public int Preview3Frame { get; set; } = 50;

        public System.Windows.Media.ImageSource Preview4 => GetPreviewImage(Preview4Frame);

        public int Preview4Frame { get; set; } = -1;

        public System.Windows.Media.ImageSource Preview5 => GetPreviewImage(Preview5Frame);

        public int Preview5Frame { get; set; } = -1;

        public float Scale
        {
            get
            {
                return _scale;
            }
            set
            {
                _scale = value;
                Changed(nameof(Scale));
                RefreshImage();
            }
        }

        public void Changed(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public string[] GetBorders()
        {
            return Directory.EnumerateFiles("Borders").ToArray();
        }

        public System.Drawing.Image GetPreview(int frame)
        {
            if (string.IsNullOrEmpty(_file))
            {
                return System.Drawing.Image.FromFile(_border);
            }
            return _tokenMaker.GetPreview(frame);
        }

        public void LoadFile(string file)
        {
            _file = file;
            ControlPanel.IsEnabled = true;

            _tokenMaker.LoadSource(new FfmpegImageSource(_file, GetSettings()));
        }

        public void SetBorder(string border)
        {
            _border = border;
            RefreshImage();
            _tokenMaker.LoadBorder(new BorderImage(_border));
        }

        private void BorderSelector_Loaded(object sender, RoutedEventArgs e)
        {
            var box = sender as ComboBox;
            box.ItemsSource = GetBorders();
            box.SelectedIndex = 0;
        }

        private void BorderSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var box = sender as ComboBox;
            SetBorder(box.SelectedValue.ToString());
        }

        private void ColorPicker_Picked(object sender, System.EventArgs e)
        {
            var colorPicker = sender as ColorPickRow;
            _tokenMaker.SetColor(colorPicker.Color);
            RefreshImage();
        }

        private void DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            _dragging = false;
            RefreshImage();
        }

        private void DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            _dragging = true;
        }

        private void EnablePreview1Button_Click(object sender, RoutedEventArgs e)
        {
            Preview1Frame = Preview1Frame < 0 ? 10 : -1;
            RefreshImage();
        }

        private void EnablePreview2Button_Click(object sender, RoutedEventArgs e)
        {
            Preview2Frame = Preview2Frame < 0 ? 30 : -1;
            RefreshImage();
        }

        private void EnablePreview3Button_Click(object sender, RoutedEventArgs e)
        {
            Preview3Frame = Preview3Frame < 0 ? 50 : -1;
            RefreshImage();
        }

        private void EnablePreview4Button_Click(object sender, RoutedEventArgs e)
        {
            Preview4Frame = Preview4Frame < 0 ? 70 : -1;
            RefreshImage();
        }

        private void EnablePreview5Button_Click(object sender, RoutedEventArgs e)
        {
            Preview5Frame = Preview5Frame < 0 ? 90 : -1;
            RefreshImage();
        }

        private BitmapImage GetPreviewImage(int frame)
        {
            if (frame < 0)
            {
                return null;
            }

            var preview = (Bitmap)GetPreview(frame);
            var bitmapImage = new BitmapImage();
            using (var memory = new MemoryStream())
            {
                preview.Save(memory, ImageFormat.Png);
                memory.Position = 0;
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }

        private SourceSetting GetSettings()
        {
            return new SourceSetting(Properties.Settings.Default.Framerate,
                                       Properties.Settings.Default.MaxTime);
        }

        private void LoadImage_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.ShowDialog();

            var file = ofd.FileName;
            if (!string.IsNullOrEmpty(file) && File.Exists(file))
            {
                ImageNameLabel.Content = file;
                LoadFile(file);
                RefreshImage();
            }
        }

        private void RefreshImage()
        {
            if (_dragging)
            {
                return;
            }

            _tokenMaker.SetScale(Scale);
            _tokenMaker.SetOffset(OffsetX, OffsetY);

            Changed(nameof(Preview1));
            Changed(nameof(Preview2));
            Changed(nameof(Preview3));
            Changed(nameof(Preview4));
            Changed(nameof(Preview5));
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            _tokenMaker.Create();
        }
    }
}
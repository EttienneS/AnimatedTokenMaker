using Microsoft.Win32;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AnimatedTokenMaker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private readonly VideoExporter _videoExporter;
        private string _border;
        private bool _dragging;
        private string _file;
        private int _offSetX;
        private int _offSetY;
        private float _scale;

        public MainWindow()
        {
            SetBorder(GetBorders()[0]);

            InitializeComponent();

            _videoExporter = new VideoExporter();
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

        public ImageSource Preview1 => GetPreviewImage((Bitmap)GetPreview(10));
        public ImageSource Preview2 => GetPreviewImage((Bitmap)GetPreview(30));
        public ImageSource Preview3 => GetPreviewImage((Bitmap)GetPreview(50));
        public ImageSource Preview4 => GetPreviewImage((Bitmap)GetPreview(70));
        public ImageSource Preview5 => GetPreviewImage((Bitmap)GetPreview(90));


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
            return GetTokenMaker().GetPreview(frame);
        }

        public void LoadFile(string file)
        {
            _file = file;
        }

        public void MakeToken()
        {
            GetTokenMaker().Create();
        }


        public void SetBorder(string border)
        {
            _border = border;
        }

        private static BitmapImage GetPreviewImage(Bitmap preview)
        {
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

        private void DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            _dragging = false;
            RefreshImage();
        }

        private void DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            _dragging = true;
        }

        private TokenMaker GetTokenMaker()
        {
            return new TokenMaker(_file, new BorderImage(_border), _videoExporter, _scale, _offSetX, _offSetY);
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
            Changed(nameof(Preview1));
            Changed(nameof(Preview2));
            Changed(nameof(Preview3));
            Changed(nameof(Preview4));
            Changed(nameof(Preview5));
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            GetTokenMaker().Create();
        }
    }
}
using AnimatedTokenMaker.Border;
using AnimatedTokenMaker.Exporter;
using AnimatedTokenMaker.Source;
using ColorPickerWPF;
using Microsoft.Win32;
using System.Collections.Generic;
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
        private readonly IFFmpegService _ffmpegService;
        private readonly ISourceSetting _sourceSetting;
        private readonly ITokenMaker _tokenMaker;
        private string _border;

        public MainWindow()
        {
            if (!File.Exists("ffmpeg.exe"))
            {
                MessageBox.Show("This application needs FFmpeg.exe to work.\n\nPlease download the exe from https://ffmpeg.org and put it in the same folder as this application.", "FFmpeg not found!", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }

            _sourceSetting = new SourceSetting(Properties.Settings.Default.Framerate,
                                               Properties.Settings.Default.MaxTime);
            _ffmpegService = new FFmpegService(_sourceSetting);

            _tokenMaker = new TokenMaker(new VideoExporter(_ffmpegService));

            SetBorder(GetBorders()[0]);

            InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public System.Windows.Media.ImageSource Preview => GetPreviewImage(PreviewFrame);

        public int PreviewFrame { get; set; } = 10;

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
            return _tokenMaker.GetPreview(frame);
        }

        public void SetBorder(string border)
        {
            _border = border;
            RefreshPreview();
            _tokenMaker.LoadBorder(new BorderImage(_border));
        }

        private static string ShowFileDialog()
        {
            var ofd = new OpenFileDialog();
            ofd.ShowDialog();

            var file = ofd.FileName;
            return file;
        }

        private void AddStaticLayer_Click(object sender, RoutedEventArgs e)
        {
            var file = ShowFileDialog();

            LayerList.Items.Add(CreateLayerView(file));
        }

        private SourceView CreateLayerView(string file)
        {
            ISourceFile layer;

            if (IsStaticImage(file))
            {
                layer = new StaticImageSource(file);
            }
            else
            {
                layer = new VideoSource(file, _ffmpegService);
            }

            _tokenMaker.AddLayer(layer);

            var view = new SourceView(layer, Path.GetFileName(file), _tokenMaker.GetBorderSize());
            view.OnLayerChanged += OnLayerChanged;
            view.OnMoveLayerDown += OnMoveLayerDown;
            view.OnMoveLayerUp += OnMoveLayerUp;
            view.OnRemoveLayer += OnRemoveLayer;

            _layerLookup.Add(layer, view);

            RefreshPreview();
            return view;
        }

        private Dictionary<ISourceFile, SourceView> _layerLookup = new Dictionary<ISourceFile, SourceView>();

        private void OnRemoveLayer(ISourceFile layer)
        {
            LayerList.Items.Remove(_layerLookup[layer]);
            _tokenMaker.RemoveLayer(layer);
        }

        private void OnMoveLayerUp(ISourceFile layer)
        {
            var view = _layerLookup[layer];
            var index = LayerList.Items.IndexOf(view);

            if (index == 0)
            {
                return;
            }

            LayerList.Items.RemoveAt(index);
            LayerList.Items.Insert(index - 1, view);
            _tokenMaker.MoveLayerUp(layer);

            RefreshPreview();
        }

        private void OnMoveLayerDown(ISourceFile layer)
        {
            var view = _layerLookup[layer];
            var index = LayerList.Items.IndexOf(view);

            if (index == LayerList.Items.Count - 1)
            {
                return;
            }

            LayerList.Items.RemoveAt(index);
            LayerList.Items.Insert(index + 1, view);
            _tokenMaker.MoveLayerDown(layer);

            RefreshPreview();
        }

        private void OnLayerChanged(ISourceFile layer)
        {
            RefreshPreview();
        }

        private bool IsStaticImage(string file)
        {
            var staticExtensions = new[] { "png", "bmp", "jpg", "tiff", "wmf", "exif", "emf", "ico" };
            return staticExtensions.Contains(Path.GetExtension(file).Trim('.'));
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
            _tokenMaker.SetBorderColor(Color.FromArgb(colorPicker.Color.A, colorPicker.Color.R, colorPicker.Color.G, colorPicker.Color.B));
            RefreshPreview();
        }

        private void DeleteStaticLayer_Click(object sender, RoutedEventArgs e)
        {
            LayerList.Items.Remove(LayerList.SelectedItem);
        }

        private BitmapImage GetPreviewImage(int frame)
        {
            if (frame < 0)
            {
                return null;
            }

            using (var preview = (Bitmap)GetPreview(frame))
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
        }

        private void RefreshPreview()
        {
            Changed(nameof(Preview));
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            _tokenMaker.ExportToken();
        }
    }
}
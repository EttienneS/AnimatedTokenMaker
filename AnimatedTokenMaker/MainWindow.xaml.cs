using AnimatedTokenMaker.Border;
using AnimatedTokenMaker.Controls;
using AnimatedTokenMaker.Exporter;
using AnimatedTokenMaker.Source;
using ColorPickerWPF;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace AnimatedTokenMaker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private readonly IFFmpegService _ffmpegService;
        private readonly SourceFactory _sourceFactory;
        private readonly ISourceSetting _sourceSetting;
        private readonly ITokenMaker _tokenMaker;
        private string _border;

        private Dictionary<ISourceFile, SourceView> _layerLookup = new Dictionary<ISourceFile, SourceView>();

        private Task _previewTask;

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

            _sourceFactory = new SourceFactory(_ffmpegService);

            _tokenMaker = new TokenMaker(new VideoExporter(_ffmpegService));

            SetBorder(GetBorders()[0]);

            InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public System.Windows.Media.ImageSource Preview { get; set; }

        public int PreviewFrame { get; set; } = 10;

        public void Changed(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public string[] GetBorders()
        {
            return Directory.EnumerateFiles("Borders").ToArray();
        }

        public void SetBorder(string border)
        {
            _border = border;
            _tokenMaker.LoadBorder(new BorderImage(_border));

            StartPreviewUpdate();
        }

        public void StartPreviewUpdate(int frame)
        {
            if (_previewTask?.IsCompleted == false)
            {
                return;
            }

            _previewTask = Task.Run(() =>
            {
                var preview = _tokenMaker.GetPreview(frame);

                var op = Dispatcher.BeginInvoke((Action)(() =>
                {
                    Preview = preview.ToBitmapImage();
                    Changed(nameof(Preview));
                }));
            });
        }

        private static string ShowFileDialog()
        {
            var ofd = new OpenFileDialog();
            ofd.ShowDialog();

            return ofd.FileName;
        }

        private void AddStaticLayer_Click(object sender, RoutedEventArgs e)
        {
            var file = ShowFileDialog();
            if (string.IsNullOrEmpty(file))
            {
                return;
            }
            IsEnabled = false;

            StartAddLayerView(file);
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
            StartPreviewUpdate();
        }

        private void DeleteStaticLayer_Click(object sender, RoutedEventArgs e)
        {
            LayerList.Items.Remove(LayerList.SelectedItem);
        }

        private void OnLayerChanged(ISourceFile layer)
        {
            StartPreviewUpdate();
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

            StartPreviewUpdate();
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

            StartPreviewUpdate();
        }

        private void OnRemoveLayer(ISourceFile layer)
        {
            LayerList.Items.Remove(_layerLookup[layer]);
            _tokenMaker.RemoveLayer(layer);
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            IsEnabled = false;
            Task.Run(() =>
            {
                _tokenMaker.ExportToken();
                Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => IsEnabled = true));
            });
        }

        private void StartAddLayerView(string file)
        {
            var loadingControl = new LoadingControl();
            LayerList.Items.Add(loadingControl);

            Task.Run(() =>
            {
                var layer = _sourceFactory.GetSource(file);
                _tokenMaker.AddLayer(layer);

                Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                {
                    var view = new SourceView(layer, Path.GetFileName(file), _tokenMaker.GetBorderSize());
                    view.OnLayerChanged += OnLayerChanged;
                    view.OnMoveLayerDown += OnMoveLayerDown;
                    view.OnMoveLayerUp += OnMoveLayerUp;
                    view.OnRemoveLayer += OnRemoveLayer;

                    _layerLookup.Add(layer, view);
                    LayerList.Items.Remove(loadingControl);
                    LayerList.Items.Add(view);

                    IsEnabled = true;
                }));
            });
        }

        private void StartPreviewUpdate()
        {
            StartPreviewUpdate(0);
        }
    }
}
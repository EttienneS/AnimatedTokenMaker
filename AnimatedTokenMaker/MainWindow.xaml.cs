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

            _ffmpegService = new FFmpegService();
            _sourceFactory = new SourceFactory(_ffmpegService, GetDefaultSetting());

            _tokenMaker = new TokenMaker(new VideoExporter(_ffmpegService, GetDefaultSetting()));
            _tokenMaker.OnExportLayerCompleted += OnExportLayerCompleted;

            SetBorder(GetBorders()[0]);

            InitializeComponent();
        }

        private static SourceSetting GetDefaultSetting()
        {
            return new SourceSetting(0, Properties.Settings.Default.Framerate, Properties.Settings.Default.MaxTime);
        }

        private void OnExportLayerCompleted(int layer, int total)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                ExportProgress.Maximum = total;
                ExportProgress.Value = layer;
            }));
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

            UpdatePreview();
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

                Dispatcher.BeginInvoke((Action)(() =>
               {
                   Preview = preview.ToBitmapImage();
                   Changed(nameof(Preview));
               }));
            });
        }

        private static string[] ShowFileDialog()
        {
            var ofd = new OpenFileDialog();
            ofd.Multiselect = true;
            ofd.ShowDialog();

            return ofd.FileNames;
        }

        private void AddStaticLayer_Click(object sender, RoutedEventArgs e)
        {
            var files = ShowFileDialog();
            if (files.Length == 0)
            {
                return;
            }
            IsEnabled = false;

            foreach (var file in files)
            {
                StartAddLayerView(file);
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

        private void ColorPicker_Picked(object sender, System.EventArgs e)
        {
            var colorPicker = sender as ColorPickRow;
            _tokenMaker.SetBorderColor(Color.FromArgb(colorPicker.Color.A, colorPicker.Color.R, colorPicker.Color.G, colorPicker.Color.B));
            UpdatePreview();
        }

        private void DeleteStaticLayer_Click(object sender, RoutedEventArgs e)
        {
            LayerList.Items.Remove(LayerList.SelectedItem);
        }

        private void OnLayerChanged(ISourceFile layer)
        {
            UpdatePreview();
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

            UpdatePreview();
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

            UpdatePreview();
        }

        private void OnRemoveLayer(ISourceFile layer)
        {
            LayerList.Items.Remove(_layerLookup[layer]);
            _tokenMaker.RemoveLayer(layer);

            UpdatePreview();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var sfd = new SaveFileDialog
            {
                AddExtension = true,
                DefaultExt = ".webm",
                Filter = "webm files (*.webm)|*.webm"
            };

            sfd.ShowDialog();

            if (!string.IsNullOrEmpty(sfd.FileName))
            {
                if (File.Exists(sfd.FileName))
                {
                    File.Delete(sfd.FileName);
                }

                IsEnabled = false;
                ExportProgress.Visibility = Visibility.Visible;

                Directory.CreateDirectory(Path.GetDirectoryName(sfd.FileName));

                Task.Run(() =>
                {
                    _tokenMaker.ExportToken(sfd.FileName);
                    Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                    {
                        IsEnabled = true;
                        ExportProgress.Visibility = Visibility.Hidden;
                        ExportProgress.Value = 0;
                    }));
                });
            }
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

        private void UpdatePreview()
        {
            StartPreviewUpdate(PreviewFrame);
        }

        private void FrameSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            UpdatePreview();
        }
    }
}
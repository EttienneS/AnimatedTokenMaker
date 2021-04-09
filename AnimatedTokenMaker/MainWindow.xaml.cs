using AnimatedTokenMaker.Border;
using AnimatedTokenMaker.Controls;
using AnimatedTokenMaker.Exporter;
using AnimatedTokenMaker.Services;
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
        private readonly SourceFactory _sourceFactory;
        private readonly ITokenMaker _tokenMaker;
        private string _border;

        private Dictionary<ISourceFile, SourceView> _layerLookup = new Dictionary<ISourceFile, SourceView>();

        private Task _previewTask;

        public MainWindow()
        {
            if (!ServiceManager.Instance.Ready())
            {
                Close();
            }

            _sourceFactory = new SourceFactory(GetDefaultSetting());

            _tokenMaker = new TokenMaker(new VideoExporter(ServiceManager.Instance.FFmpegService, GetDefaultSetting()));
            _tokenMaker.OnExportLayerCompleted += OnExportLayerCompleted;

            InitializeComponent();

            InitMaskAndBorder();
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
            return Directory.EnumerateFiles("Assets\\Borders", "*.png", SearchOption.TopDirectoryOnly)
                            .Where(f => !f.Contains("_mask"))
                            .ToArray();
        }

        public void StartPreviewUpdate(int frame)
        {
            if (_previewTask?.IsCompleted == false)
            {
                return;
            }
            try
            {
                _tokenMaker.LoadBorder(new BorderImage(_border));
            }
            catch (FileNotFoundException ex)
            {
                MessageBox.Show(ex.Message, "Border mask not found", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private static SourceSetting GetDefaultSetting()
        {
            return new SourceSetting(0, Properties.Settings.Default.Framerate, Properties.Settings.Default.MaxTime);
        }

        private static string[] ShowFileDialog(string path = "")
        {
            var ofd = new OpenFileDialog
            {
                Multiselect = true
            };

            if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
            {
                ofd.InitialDirectory = path;
            }
            ofd.ShowDialog();

            return ofd.FileNames;
        }

        private void AddGradient_Click(object sender, RoutedEventArgs e)
        {
            GetAndAddLayer(Path.Combine(Environment.CurrentDirectory, "Assets\\Gradients"));
        }

        private void AddStaticLayer_Click(object sender, RoutedEventArgs e)
        {
            GetAndAddLayer();
        }

        private void GetAndAddLayer(string defaultPath = "")
        {
            var files = ShowFileDialog(defaultPath);
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

        private void BorderSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var box = sender as ComboBox;
            _border = box.SelectedValue.ToString();

            var name = Path.GetFileNameWithoutExtension(_border);
            UpdatePreview();
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

        private void FrameSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            UpdatePreview();
        }

        private void InitMaskAndBorder()
        {
            BorderSelectorBox.ItemsSource = GetBorders();
            BorderSelectorBox.SelectedIndex = 0;

            _border = BorderSelectorBox.SelectedValue.ToString();
        }

        private void OnExportLayerCompleted(int layer, int total)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                ExportProgress.Maximum = total;
                ExportProgress.Value = layer;
            }));
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

        private void ResetUi(LoadingControl loadingControl)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                LayerList.Items.Remove(loadingControl);
                IsEnabled = true;
            }));
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
                try
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
                        ResetUi(loadingControl);

                        LayerList.Items.Add(view);
                    }));
                }
                catch (SourceNotReadyException ex)
                {
                    MessageBox.Show(ex.Message, "Source not available", MessageBoxButton.OK);
                    ResetUi(loadingControl);
                }
            });
        }

        private void UpdatePreview()
        {
            StartPreviewUpdate(PreviewFrame);
        }
    }
}
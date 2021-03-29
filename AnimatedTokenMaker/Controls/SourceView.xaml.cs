using AnimatedTokenMaker.Source;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Point = System.Windows.Point;
using Size = System.Drawing.Size;

namespace AnimatedTokenMaker
{
    /// <summary>
    /// Interaction logic for SourceView.xaml
    /// </summary>
    public partial class SourceView : UserControl, INotifyPropertyChanged
    {
        private readonly Size _borderSize;
        private readonly ISourceFile _source;

        private bool _dragging;

        public SourceView(ISourceFile source, string name, Size borderSize) : this()
        {
            _source = source;
            _borderSize = borderSize;

            DisplayName = name;

            if (source is IVideoSource video)
            {
                VideoControlPanel.IsEnabled = true;
                TotalVideoDuration = video.GetDurationInSeconds();
                ClipLenght = video.GetClipLenght();

                Changed(nameof(TotalVideoDuration));
                Changed(nameof(ClipLenght));
            }

            StartPreviewUpdate();
            Changed(nameof(DisplayName));
        }

        public SourceView()
        {
            InitializeComponent();
        }

        public event LayerDelegates.MoveLayerDownDelegate OnLayerChanged;

        public event LayerDelegates.MoveLayerDownDelegate OnMoveLayerDown;

        public event LayerDelegates.MoveLayerDownDelegate OnMoveLayerUp;

        public event LayerDelegates.MoveLayerDownDelegate OnRemoveLayer;

        public event PropertyChangedEventHandler PropertyChanged;

        public string DisplayName { get; }

        public int OffsetX { get; set; } = 0;
        public int OffsetY { get; set; } = 0;
        public System.Windows.Media.ImageSource Preview { get; set; }
        public float Scale { get; set; } = 1f;
        public float Alpha { get; set; } = 1f;
        public int TotalVideoDuration { get; set; } = 0;
        public int ClipStart { get; set; } = 0;
        public int ClipLenght { get; set; } = 5;

        public void Changed(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
            LayerChanged();
        }

        public void StartPreviewUpdate()
        {
            if (_source == null)
            {
                return;
            }

            Task.Run(() =>
            {
                var preview = new Bitmap(_source.GetFrame(0, _borderSize));

                var op = Dispatcher.BeginInvoke((Action)(() =>
                {
                    Preview = preview.ToBitmapImage();
                    Changed(nameof(Preview));
                }));
            });
        }

        private void DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            UpdateSourceWithThis();
            _dragging = false;

            StartPreviewUpdate();
        }

        private void UpdateSourceWithThis()
        {
            _source.SetOffset(OffsetX, OffsetY);
            _source.SetScale(Scale);
            _source.SetAlpha(Alpha);
        }

        private void DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            _dragging = true;
        }

        private void LayerChanged()
        {
            OnLayerChanged?.Invoke(_source);
        }

        private void MoveDown_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            OnMoveLayerDown?.Invoke(_source);
        }

        private void MoveUp_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            OnMoveLayerUp?.Invoke(_source);
        }

        private void Remove_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            OnRemoveLayer?.Invoke(_source);
        }

        private void Slider_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            if (!_dragging)
            {
                StartPreviewUpdate();
            }
        }

        private void UpdateClip_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ClipStart < 0)
            {
                MessageBox.Show("Invalid clip start!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (ClipStart + ClipLenght > TotalVideoDuration)
            {
                MessageBox.Show("Start time + lenght is greater than total duration!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                if (_source is VideoSource video)
                {
                    var currentSetting = video.GetSetting();

                    video.UpdateSetting(new SourceSetting(ClipStart, currentSetting.GetFrameRate(), ClipLenght));
                    video.UpdateFrames();

                    StartPreviewUpdate();
                }
            }
        }


        private Point? _startPoint;

        private void PreviewImage_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(sender as System.Windows.Controls.Image);
            _dragging = true;
        }

        private void PreviewImage_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _startPoint = null;
            _dragging = false;

            UpdateSourceWithThis();
            StartPreviewUpdate();
        }

        private void PreviewImage_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (_startPoint.HasValue)
            {
                var newPoint = e.GetPosition(sender as System.Windows.Controls.Image);
                OffsetX = (int)(_startPoint.Value.X - newPoint.X);
                OffsetY = (int)(_startPoint.Value.Y - newPoint.Y);

                Changed(nameof(OffsetX));
                Changed(nameof(OffsetY));

            }
        }
    }
}
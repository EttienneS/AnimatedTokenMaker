using AnimatedTokenMaker.Source;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
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

        private bool _allowEdit = true;
        private int _clipLenght = 5;
        private int _clipStart = 0;
        private bool _dragging;
        private Point? _startPoint;

        private string _statusText = "";

        private int _updateCounter = 0;

        private bool _updating;

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

        public bool AllowEditing
        {
            get
            {
                return _allowEdit;
            }
            set
            {
                _allowEdit = value;
                StatusText = _allowEdit ? "" : "Updating layer, please wait...";
                OnPropertyChanged(nameof(AllowEditing));
            }
        }

        public float Alpha { get; set; } = 1f;

        public int ClipLenght
        {
            get
            {
                return _clipLenght;
            }
            set
            {
                if (_clipLenght != value)
                {
                    _clipLenght = value;
                    ReloadView();
                    OnPropertyChanged(nameof(ClipLenght));
                }
            }
        }

        public int ClipStart
        {
            get
            {
                return _clipStart;
            }
            set
            {
                if (_clipStart != value)
                {
                    _clipStart = value;
                    ReloadView();
                    OnPropertyChanged(nameof(ClipStart));
                }
            }
        }

        public string DisplayName { get; }

        public int OffsetX { get; set; } = 0;

        public int OffsetY { get; set; } = 0;

        public System.Windows.Media.ImageSource Preview { get; set; }

        public float Scale { get; set; } = 1f;

        public string StatusText
        {
            get
            {
                return _statusText;
            }
            set
            {
                _statusText = value;
                OnPropertyChanged(nameof(StatusText));
            }
        }

        public int TotalVideoDuration { get; set; } = 0;

        public void Changed(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
            LayerChanged();
        }

        public void StartPreviewUpdate(bool incrementCount = true)
        {
            if (_source == null)
            {
                return;
            }
            if (incrementCount)
            {
                _updateCounter++;
            }

            if (!_updating)
            {
                _updating = true;
                Task.Run(() =>
                {
                    var preview = new Bitmap(_source.GetFrame(0, _borderSize));

                    var op = Dispatcher.BeginInvoke((Action)(() =>
                    {
                        Preview = preview.ToBitmapImage();
                        Changed(nameof(Preview));
                    }));

                    _updateCounter--;
                    _updating = false;

                    if (_updateCounter > 0)
                    {
                        StartPreviewUpdate(false);
                    }
                });
            }
        }

        public void StartUpdateFrames()
        {
            if (_source is VideoSource video)
            {
                Task.Run(() =>
                {
                    var currentSetting = video.GetSetting();

                    video.UpdateSetting(new SourceSetting(ClipStart, currentSetting.GetFrameRate(), ClipLenght));
                    video.UpdateFrames();

                    StartPreviewUpdate();

                    AllowEditing = true;
                });
            }
        }

        public bool TryInvariantFloatParse(string input, out float result)
        {
            var ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            ci.NumberFormat.CurrencyDecimalSeparator = ".";
            if (float.TryParse(input, NumberStyles.Any, ci, out result))
            {
                return true;
            }

            result = 0f;
            return false;
        }

        protected void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        private bool AlmostEquals(float value1, float value2)
        {
            return Math.Abs(value1 - value2) < 0.001f;
        }

        private float Clamp(float value, float min, float max)
        {
            if (value < min)
            {
                value = min;
            }
            else if (value > max)
            {
                value = max;
            }
            return value;
        }

        private void DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            OnPropertyChanged(nameof(Scale));
            OnPropertyChanged(nameof(Alpha));
            OnPropertyChanged(nameof(OffsetX));
            OnPropertyChanged(nameof(OffsetY));

            UpdateSourceWithThis();
            _dragging = false;

            StartPreviewUpdate();
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

        private void PreviewImage_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(sender as System.Windows.Controls.Image);
            _dragging = true;
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

        private void PreviewImage_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _startPoint = null;
            _dragging = false;

            UpdateSourceWithThis();
            StartPreviewUpdate();
        }

        private void ReloadView()
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
                AllowEditing = false;
                StartUpdateFrames();
            }
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

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool changed = false;
            if (sender == ScaleTextbox)
            {
                if (TryInvariantFloatParse(ScaleTextbox.Text, out float result))
                {
                    if (!AlmostEquals(Scale, result))
                    {
                        Scale = Clamp(result, 0.1f, 3f);
                        OnPropertyChanged(nameof(Scale));
                        changed = true;
                    }
                }
            }

            if (sender == AlphaTextbox)
            {
                if (TryInvariantFloatParse(AlphaTextbox.Text, out float result))
                {
                    if (!AlmostEquals(Alpha, result))
                    {
                        Alpha = Clamp(result, 0f, 1f);
                        OnPropertyChanged(nameof(Alpha));
                        changed = true;
                    }
                }
            }

            if (sender == LeftTextbox)
            {
                if (int.TryParse(LeftTextbox.Text, out int result))
                {
                    if (!AlmostEquals(OffsetX, result))
                    {
                        OffsetX = (int)Clamp(result, -50, 50);
                        OnPropertyChanged(nameof(OffsetX));
                        changed = true;
                    }
                }
            }

            if (sender == TopTextbox)
            {
                if (int.TryParse(TopTextbox.Text, out int result))
                {
                    if (!AlmostEquals(OffsetY, result))
                    {
                        OffsetY = (int)Clamp(result, -50, 50);
                        OnPropertyChanged(nameof(OffsetY));
                        changed = true;
                    }
                }
            }

            if (changed)
            {
                UpdateSourceWithThis();
                StartPreviewUpdate();
            }
        }

        private void UpdateSourceWithThis()
        {
            _source.SetOffset(OffsetX, OffsetY);
            _source.SetScale(Scale);
            _source.SetAlpha(Alpha);
        }
    }
}
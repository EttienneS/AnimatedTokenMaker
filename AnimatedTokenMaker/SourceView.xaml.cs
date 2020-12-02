using AnimatedTokenMaker.Source;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

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

            Changed(nameof(Preview));
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
        public System.Windows.Media.ImageSource Preview => GetPreviewImage(0);
        public float Scale { get; set; } = 1f;

        public void Changed(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
            LayerChanged();
        }

        private void DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            _source.SetOffset(OffsetX, OffsetY);
            _source.SetScale(Scale);
            _dragging = false;

            Changed(nameof(Preview));
        }

        private void DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            _dragging = true;
        }

        private BitmapImage GetPreviewImage(int frame)
        {
            if (frame < 0 || _source == null)
            {
                return null;
            }

            using (var preview = _source.GetFrame(frame, _borderSize))
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
                Changed(nameof(Preview));
            }
        }
    }
}
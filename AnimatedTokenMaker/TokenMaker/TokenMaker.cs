using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace AnimatedTokenMaker
{
    public class TokenMaker
    {
        private readonly IVideoExporter _videoExporter;
        private readonly string _workingFolder = Path.Combine("temp", Guid.NewGuid().ToString());
        private IBorderImage _border;
        private ISourceFile _imageSource;
        private int _offsetX = 0;
        private int _offsetY = 0;
        private float _scale = 1f;

        public TokenMaker(IVideoExporter videoExporter)
        {
            _videoExporter = videoExporter;

            Directory.CreateDirectory(_workingFolder);
        }

        public Bitmap CombineImageWithBorder(Bitmap srcImage, int offsetX, int offsetY)
        {
            var borderImage = _border.GetColoredBorderImage();
            var newImage = _border.GetEmptyBorderSizedBitmap();

            for (int y = 0; y < newImage.Height; y++)
            {
                for (int x = 0; x < newImage.Width; x++)
                {
                    var px = borderImage.GetPixel(x, y);
                    if (px.A == 0)
                    {
                        newImage.SetPixel(x, y, px);
                    }
                    else if (px.R == 0 && px.G == 0 && px.B == 0)
                    {
                        var oX = x + offsetX;
                        var oY = y + offsetY;
                        var sample = Color.FromArgb(0, 0, 0, 0);

                        if (oX < srcImage.Width && oY < srcImage.Height && oX > 0 && oY > 0)
                        {
                            sample = srcImage.GetPixel(oX, oY);
                        }

                        newImage.SetPixel(x, y, sample);
                    }
                    else
                    {
                        newImage.SetPixel(x, y, px);
                    }
                }
            }

            return newImage;
        }

        public void Create()
        {
            for (int i = 0; i < _imageSource.GetFrameCount(); i++)
            {
                var newImage = GetCombinedImageForFrame(i);
                newImage.Save(Path.Combine(_workingFolder, "t" + i.ToString("").PadLeft(4, '0') + ".png"), ImageFormat.Png);
            }

            _videoExporter.GenerateVideoFromFolder(_workingFolder);
        }

        public Bitmap GetPreview(int frame = 0)
        {
            var count = _imageSource.GetFrameCount();
            if (count == 1)
            {
                frame = 0;
            }
            count--;
            return GetCombinedImageForFrame((int)(((decimal)frame / 100) * count));
        }

        public void LoadBorder(IBorderImage border)
        {
            _border = border;
        }

        public void LoadSource(ISourceFile source)
        {
            _imageSource = source;
        }

        public void SetOffset(int x, int y)
        {
            _offsetX = x;
            _offsetY = y;
        }

        public void SetScale(float scale)
        {
            // stop scale from going too small
            _scale = Math.Max(0.1f, scale);
        }

        private Bitmap GetCombinedImageForFrame(int frame)
        {
            var scaledImageOfCurrentFrame = _imageSource.GetScaledFrame(frame, _scale);

            var calcX = (Math.Abs(_offsetX) / 100f) * scaledImageOfCurrentFrame.Width * (_offsetX > 0 ? 1f : -1f);
            var calcY = (Math.Abs(_offsetY) / 100f) * scaledImageOfCurrentFrame.Height * (_offsetY > 0 ? 1f : -1f);

            return CombineImageWithBorder(scaledImageOfCurrentFrame, (int)calcX, (int)calcY);
        }

        internal void SetColor(System.Windows.Media.Color color)
        {
            _border.SetBorderColor(Color.FromArgb(color.A, color.R, color.G, color.B));
        }
    }
}
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace AnimatedTokenMaker
{
    public class TokenMaker
    {
        private readonly IBorderImage _border;
        private readonly IVideoExporter _videoExporter;
        private readonly string _workingFolder = Path.Combine("temp", Guid.NewGuid().ToString());
        private readonly Image _inputImage;
        private readonly FrameDimension _dimension;

        private readonly float _scale;
        private readonly int _offsetX;
        private readonly int _offsetY;

        public TokenMaker(string inputFile, IBorderImage border, IVideoExporter videoExporter, float scale, int offSetX, int offSetY)
        {
            _border = border;
            _videoExporter = videoExporter;

            _inputImage = Image.FromFile(inputFile);

            Directory.CreateDirectory(_workingFolder);
            _dimension = new FrameDimension(_inputImage.FrameDimensionsList[0]);

            _scale = scale;
            _offsetX = offSetX;
            _offsetY = offSetY;
        }

        public void Create()
        {
            for (int i = 0; i < GetFrameCount(); i++)
            {
                var newImage = GetCombinedImageForFrame(i);
                newImage.Save(Path.Combine(_workingFolder, "t" + i.ToString("").PadLeft(4, '0') + ".png"), ImageFormat.Png);
            }

            _videoExporter.GenerateVideoFromFolder(_workingFolder);
        }

        private Bitmap GetCombinedImageForFrame(int i)
        {
            SelectFrame(i);

            var scaledImageOfCurrentFrame = ScaleImage((Bitmap)_inputImage);

            var calcX = (Math.Abs(_offsetX) / 100f) * scaledImageOfCurrentFrame.Width * (_offsetX > 0 ? 1f : -1f);
            var calcY = (Math.Abs(_offsetY) / 100f) * scaledImageOfCurrentFrame.Height * (_offsetY > 0 ? 1f : -1f);

            return _border.CombineWithImage(scaledImageOfCurrentFrame, (int)calcX, (int)calcY);
        }

        public Bitmap ScaleImage(Bitmap sourceImage)
        {
            var borderSize = _border.GetEmptyBorderSizedBitmap();

            var adjustedScale = _scale + 1f;
            var scaledImage = new Bitmap(sourceImage, Math.Max(24, (int)(sourceImage.Width * adjustedScale)), Math.Max(24, (int)(sourceImage.Height * adjustedScale)));

            if (scaledImage.Width < borderSize.Width || scaledImage.Height < borderSize.Height)
            {
                // grow image to be at least as big as the border
                var ratio = Math.Max((float)scaledImage.Width / borderSize.Width, (float)scaledImage.Height / borderSize.Height);
                scaledImage = new Bitmap(scaledImage, new Size((int)(scaledImage.Width * ratio), (int)(scaledImage.Height * ratio)));
            }

            return scaledImage;
        }

        private void SelectFrame(int i)
        {
            _inputImage.SelectActiveFrame(_dimension, i);
        }

        private int GetFrameCount()
        {
            return _inputImage.GetFrameCount(_dimension);
        }

        public Bitmap GetPreview(int frame = 0)
        {
            var count = GetFrameCount();
            if (count == 1)
            {
                frame = 0;
            }
            count--;
            return GetCombinedImageForFrame((int)(((decimal)frame / 100) * count));
        }
    }
}
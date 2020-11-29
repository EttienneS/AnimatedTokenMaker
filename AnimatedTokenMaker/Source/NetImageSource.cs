using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace AnimatedTokenMaker.Source
{
    public class NetImageSource : ISourceFile
    {
        private readonly Image _inputImage;
        private readonly FrameDimension _dimension;

        public NetImageSource(string inputFile)
        {
            _inputImage = Image.FromFile(inputFile);
            _dimension = new FrameDimension(_inputImage.FrameDimensionsList[0]);
        }

        public Bitmap GetScaledFrame(int frame, float scale)
        {
            var sourceImage = GetImageAtFrame(frame);
            return new Bitmap(sourceImage, Math.Max(24, (int)(sourceImage.Width * scale)), Math.Max(24, (int)(sourceImage.Height * scale)));
        }

        public int GetFrameCount()
        {
            return _inputImage.GetFrameCount(_dimension);
        }

        private Bitmap GetImageAtFrame(int frame)
        {
            _inputImage.SelectActiveFrame(_dimension, frame);
            return (Bitmap)_inputImage;
        }

        public void Dispose()
        {
        }
    }
}